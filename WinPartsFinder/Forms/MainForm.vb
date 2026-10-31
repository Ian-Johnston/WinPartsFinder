' WinPartsFinder by Ian Johnston
' VS 2022 VB app for Win10/11


Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Text

' The .exe's "Product version" (Explorer > Properties > Details) comes from AppVersion below too.
<Assembly: AssemblyInformationalVersion(MainForm.AppVersion)>


Public Class MainForm

    ' ==== THE version number - change it here for each release ====
    Public Const AppVersion As String = "V1.021"
    Private Const TitleSuffix As String = "(Free for Non-Commercial Use • Support WinPartsFinder - see About)"

    Private _settings As AppSettings
    Private _suppliers As New List(Of Supplier)
    Private ReadOnly _table As DataTable = CreateResultsTable()
    Private _cts As CancellationTokenSource
    Private _linkFont As Font
    Private _loading As Boolean

    Private WithEvents _images As New ImageCache()
    ' First photo seen for each manufacturer part no., so suppliers without photos (Mouser) can borrow one.
    Private ReadOnly _imagesByMpn As New Dictionary(Of String, (Thumb As String, Large As String))
    ' Package by manufacturer part no., so rows without one (Mouser's search never gives it) can borrow it.
    Private ReadOnly _packagesByMpn As New Dictionary(Of String, (Package As String, Supplier As String))
    Private colThumb As DataGridViewImageColumn
    Private colSupplierPartLink As DataGridViewLinkColumn
    Private colDatasheetLink As DataGridViewLinkColumn
    Private _photoPopup As ImagePopupForm

    Private Const ThumbRowHeight As Integer = ImageCache.ThumbSize + 4

    ' Designer layout, for Settings > Reset window layout.
    Private _defaultSize As Size
    Private _defaultSplitter As Integer
    Private _defaultLeftSplitter As Integer
    Private _defaultColumns As Dictionary(Of String, ColumnLayout)

    ' Supplier list sub-item indexes
    Private Const SubMode As Integer = 1
    Private Const SubStatus As Integer = 2
    Private Const SubHits As Integer = 3
    Private Const SubTime As Integer = 4
    Private _supplierHover As DarkListHover
    Private WithEvents _bomForm As BomForm

#Region "Startup / shutdown"

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _settings = AppSettings.Load()
        Text = $"WinPartsFinder - {AppVersion}      {TitleSuffix}"
        Icon = Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ' WinPartsFinder.ico, via <ApplicationIcon>
        Theme.Current = _settings.AppTheme
        lvSuppliers.OwnerDraw = True ' only so the column headers can be drawn dark
        _supplierHover = New DarkListHover(lvSuppliers)
        _defaultSize = Size
        _defaultSplitter = splitMain.SplitterDistance
        _defaultLeftSplitter = splitLeft.SplitterDistance
        ' Can't be made smaller than the default (reset) size - unless the screen itself is smaller.
        Dim area = Screen.FromControl(Me).WorkingArea
        MinimumSize = New Size(Math.Min(_defaultSize.Width, area.Width), Math.Min(_defaultSize.Height, area.Height))
        RestoreWindowPlacement()

        Theme.SetCueBanner(txtFilter, "narrow results…")
        cboMode.Items.AddRange({"Keyword", "Manufacturer part no."})
        cboMode.SelectedIndex = 0
        nudMax.Value = Math.Max(nudMax.Minimum, Math.Min(nudMax.Maximum, _settings.MaxResults))
        chkInStock.Checked = _settings.InStockOnly
        chkActive.Checked = _settings.ActiveOnly
        BuildStatusFilterRows()
        _settingConvert = True ' (no fetching while the window is still being set up)
        chkConvert.Checked = _settings.ConvertPrices
        _settingConvert = False
        UpdateConvertText()

        ConfigureGridColumns()
        _defaultColumns = CurrentColumnLayout()
        ApplyColumnLayout(_settings.ResultColumns)
        ApplyThumbnailSetting()
        _table.DefaultView.Sort = "LinkOnly DESC, Order" ' website links first, then arrival order until the user sorts
        dgvResults.DataSource = _table.DefaultView
        _linkFont = New Font(dgvResults.Font, FontStyle.Italic)

        RebuildSuppliers()
        RefreshAutoComplete()
        ApplyFilter()
        Theme.Apply(Me)
        MatchFiltersToSupplierList()
        CreateBomForm()
        AlignToolbar()

        If Not _suppliers.Any(Function(s) s.IsConfigured) Then
            SetStatus("No API keys set yet - open Settings to add Mouser / DigiKey / element14 keys. Until then you get website links.")
        Else
            SetStatus("Ready.")
        End If
        cboSearch.Select()
        ' Prices in one currency: fetch new exchange rates in the background if the saved ones are 12+ hours old.
        If chkConvert.Checked Then RefreshRatesInBackground()
    End Sub

    Private Async Sub RefreshRatesInBackground()
        Await RefreshRatesAsync(showDone:=False)
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Unsaved BOM changes: Save / Don't Save / Cancel (Cancel keeps the app open).
        If e.CloseReason = CloseReason.UserClosing AndAlso _bomForm IsNot Nothing AndAlso _bomForm.HasUnsavedChanges Then
            ShowBomWindow()
            If Not _bomForm.ConfirmDiscard() Then
                e.Cancel = True
                Return
            End If
        End If
        _cts?.Cancel()
        SaveBomSettings()
        SaveUiSettings()
    End Sub

    ''' <summary>
    ''' Gives the toolbar one even height and centres everything on one line. Worked out from the real control
    ''' sizes when the window opens (and after a theme change), so it stays lined up at any Windows scaling.
    ''' </summary>
    Private Sub AlignToolbar()
        ' The drop-downs set the height: Windows sizes them from the font and won't stretch them.
        Dim rowHeight = cboMode.Height
        txtFilter.AutoSize = False
        txtFilter.Height = rowHeight
        ' Types… grows with its text ("Types (3 hidden)…") - its width is set in UpdateTypesButton instead.
        btnTypes.AutoSize = False
        btnTypes.MinimumSize = Size.Empty
        UpdateTypesButton()
        ' Buttons are a little taller than the boxes: at the boxes' height the flat (custom theme) buttons
        ' cut off the bottoms of letters like y, p and g.
        Dim buttonHeight = rowHeight + CInt(4 * DeviceDpi / 96.0)
        ' Export CSV… is the longest fixed label: wide enough for its text, never narrower than the others.
        btnExport.Width = Math.Max(btnSearch.Width, TextRenderer.MeasureText(btnExport.Text, btnExport.Font).Width + 20)
        UpdateBomButtons()
        For Each b In {btnSearch, btnAdvanced, btnCancel, btnLoadMore, btnTypes, btnExport, btnAddToBom, btnBom, btnSettings, btnHelp, btnAbout}
            b.Height = buttonHeight
        Next

        ' Everything else (labels, tick box, number box) is centred on the same middle line.
        Dim lineHeight = flpTop.Controls.Cast(Of Control)().Where(Function(c) c IsNot flpFilter).
                             Concat(flpFilter.Controls.Cast(Of Control)()).Max(Function(c) c.Height)
        Const Gap As Integer = 3 ' normal space above and below each control
        Dim centre = Sub(c As Control, extraTop As Integer)
                         Dim spare = lineHeight - c.Height
                         c.Margin = New Padding(c.Margin.Left, Gap + extraTop + spare \ 2, c.Margin.Right, Gap + spare - spare \ 2)
                     End Sub
        For Each c As Control In flpTop.Controls
            If c IsNot flpFilter Then centre(c, 0) ' the Filter panel's own two controls are centred below
        Next
        For Each c As Control In flpFilter.Controls
            centre(c, 0)
        Next
        ' Help and About sit outside the toolbar's panel, so they also need its inside padding.
        centre(btnHelp, flpTop.Padding.Top)
        centre(btnAbout, flpTop.Padding.Top)
    End Sub

    Private Sub RestoreWindowPlacement()
        Dim p = _settings.MainWindow
        If p Is Nothing OrElse p.Width < MinimumSize.Width OrElse p.Height < MinimumSize.Height Then Return
        Dim bounds As New Rectangle(p.X, p.Y, p.Width, p.Height)
        ' Ignore it if that spot is no longer on any screen (e.g. a monitor was unplugged).
        Dim visible = Screen.AllScreens.Any(Function(s)
                                                Dim overlap = Rectangle.Intersect(s.WorkingArea, bounds)
                                                Return overlap.Width >= 200 AndAlso overlap.Height >= 100
                                            End Function)
        If Not visible Then Return
        StartPosition = FormStartPosition.Manual
        Me.Bounds = bounds
        If p.Maximized Then WindowState = FormWindowState.Maximized
        If p.SplitterDistance > 0 Then
            splitMain.SplitterDistance = Math.Min(p.SplitterDistance, Math.Max(splitMain.Panel1MinSize, splitMain.Width - 200))
        End If
        If p.LeftSplitterDistance > 0 Then
            splitLeft.SplitterDistance = Math.Min(p.LeftSplitterDistance, Math.Max(splitLeft.Panel1MinSize, splitLeft.Height - 60))
        End If
    End Sub

    Private Function CurrentWindowPlacement() As WindowPlacement
        ' RestoreBounds holds the normal size while maximised/minimised.
        Dim b = If(WindowState = FormWindowState.Normal, Bounds, RestoreBounds)
        Return New WindowPlacement With {
            .X = b.X, .Y = b.Y, .Width = b.Width, .Height = b.Height,
            .Maximized = WindowState = FormWindowState.Maximized,
            .SplitterDistance = splitMain.SplitterDistance,
            .LeftSplitterDistance = splitLeft.SplitterDistance
        }
    End Function

    Private Sub ResetWindowLayout()
        WindowState = FormWindowState.Normal
        Size = _defaultSize
        Dim area = Screen.FromControl(Me).WorkingArea
        Location = New Point(area.Left + (area.Width - Width) \ 2, area.Top + (area.Height - Height) \ 2)
        splitMain.SplitterDistance = _defaultSplitter
        splitLeft.SplitterDistance = _defaultLeftSplitter
        ApplyColumnLayout(_defaultColumns)
    End Sub

    Private Sub SaveUiSettings()
        _settings.MainWindow = CurrentWindowPlacement()
        _settings.ResultColumns = CurrentColumnLayout()
        _settings.MaxResults = CInt(nudMax.Value)
        _settings.InStockOnly = chkInStock.Checked
        _settings.ActiveOnly = chkActive.Checked
        _settings.StatusFilters = _statusRows.Select(Function(r) New StatusFilter With {.Term = r.Box.Text, .Hide = r.Check.Checked}).ToList()
        _settings.ConvertPrices = chkConvert.Checked
        For Each item As ListViewItem In lvSuppliers.Items
            _settings.EnabledSuppliers(DirectCast(item.Tag, Supplier).Name) = item.Checked
        Next
        Try
            _settings.Save()
        Catch ex As Exception
            MessageBox.Show(Me, "Could not save settings: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ''' <summary>
    ''' (Re)creates the suppliers from the settings. After Settings > OK the current search's
    ''' Status / Hits / Time and Load more position are carried over by supplier name, so saving
    ''' a setting (e.g. Dark mode) doesn't wipe them.
    ''' </summary>
    Private Sub RebuildSuppliers()
        Dim previous As New Dictionary(Of String, (Status As String, Time As String, Tip As String, Paging As PagingState))
        For Each old As ListViewItem In lvSuppliers.Items
            Dim oldSupplier = DirectCast(old.Tag, Supplier)
            Dim st As PagingState = Nothing
            _paging.TryGetValue(oldSupplier, st)
            previous(oldSupplier.Name) = (old.SubItems(SubStatus).Text, old.SubItems(SubTime).Text, old.ToolTipText, st)
        Next
        _paging.Clear()

        _loading = True
        _suppliers = SupplierFactory.Create(_settings)
        lvSuppliers.BeginUpdate()
        lvSuppliers.Items.Clear()
        For Each s In _suppliers
            Dim item As New ListViewItem(s.Name) With {.Tag = s, .Checked = _settings.IsEnabled(s.Name)}
            item.SubItems.Add(s.ModeText)
            item.SubItems.Add("")
            item.SubItems.Add("")
            item.SubItems.Add("")
            item.ToolTipText = If(s.IsConfigured,
                                  $"{s.Name}: searched via API.",
                                  $"{s.Name}: no API search - a website search link will be added to the results.")
            If Not s.IsConfigured Then item.ForeColor = Theme.MutedText

            Dim prev As (Status As String, Time As String, Tip As String, Paging As PagingState) = Nothing
            If previous.TryGetValue(s.Name, prev) Then
                item.SubItems(SubStatus).Text = prev.Status
                item.SubItems(SubTime).Text = prev.Time
                If prev.Paging IsNot Nothing Then
                    ' Still searchable (key not removed): keep Load more's place and the tooltip.
                    If s.IsConfigured Then
                        _paging(s) = prev.Paging
                        item.ToolTipText = prev.Tip
                    End If
                    item.SubItems(SubHits).Text = HitsText(s, prev.Paging)
                ElseIf prev.Status.Length > 0 Then
                    item.ToolTipText = prev.Tip ' e.g. an error message
                End If
            End If
            lvSuppliers.Items.Add(item)
        Next
        lvSuppliers.EndUpdate()
        _loading = False
        UpdateLoadMoreButton()
    End Sub

    ' The Search box is an editable drop-down: type as normal (matching saved searches are suggested),
    ' or click its arrow to pick one of the last 30. Picking one only fills the box - Enter or Search runs it.
    Private Const RecentShown As Integer = 30

    Private Sub RefreshAutoComplete()
        Dim src As New AutoCompleteStringCollection()
        src.AddRange(_settings.RecentSearches.ToArray())
        cboSearch.AutoCompleteCustomSource = src
        Dim typed = cboSearch.Text ' refilling the list can clear the text
        cboSearch.BeginUpdate()
        cboSearch.Items.Clear()
        cboSearch.Items.AddRange(_settings.RecentSearches.Take(RecentShown).Cast(Of Object)().ToArray())
        cboSearch.EndUpdate()
        cboSearch.Text = typed
    End Sub

#End Region

#Region "Results table"

    Private Shared Function CreateResultsTable() As DataTable
        Dim t As New DataTable("Results")
        t.Columns.Add("Supplier", GetType(String))
        t.Columns.Add("MfrPartNo", GetType(String))
        t.Columns.Add("Manufacturer", GetType(String))
        t.Columns.Add("Description", GetType(String))
        t.Columns.Add("Type", GetType(String))
        t.Columns.Add("Stock", GetType(Long))
        t.Columns.Add("UnitPrice", GetType(Decimal))
        t.Columns.Add("Currency", GetType(String))
        t.Columns.Add("PriceBreaks", GetType(String))
        t.Columns.Add("MOQ", GetType(Integer))
        t.Columns.Add("Availability", GetType(String))
        t.Columns.Add("SupplierPartNo", GetType(String))
        t.Columns.Add("Status", GetType(String))
        t.Columns.Add("ProductUrl", GetType(String))
        t.Columns.Add("DatasheetUrl", GetType(String))
        t.Columns.Add("ImageUrl", GetType(String))
        t.Columns.Add("LargeImageUrl", GetType(String))
        t.Columns.Add("LinkOnly", GetType(Boolean))
        ' Rows the user removed (right-click > Remove). They stay in the table so Restore can bring them back.
        t.Columns.Add("Removed", GetType(Boolean))
        ' Hidden order key: supplier's place in the supplier list, then the row's number within that supplier.
        ' Clicking the Supplier header sorts by it: every supplier's rows together, loaded-more ones included.
        t.Columns.Add("Seq", GetType(Integer))
        ' Hidden default order: the search first, then each Load more batch at the end - each grouped by supplier.
        t.Columns.Add("Order", GetType(Long))
        ' Row number shown in the # column. Given out when a search / Load more finishes and never changes,
        ' so a number written down still finds the same part after removing, filtering or sorting.
        t.Columns.Add("No", GetType(Integer))
        ' "Prices in GBP": UnitPrice / Currency / PriceBreaks show converted prices; the supplier's own are kept here.
        t.Columns.Add("OrigUnitPrice", GetType(Decimal))
        t.Columns.Add("OrigCurrency", GetType(String))
        t.Columns.Add("OrigBreaks", GetType(String))
        t.Columns.Add("BreaksRaw", GetType(String))   ' "1:0.2;10:0.139" - the breaks as numbers, for converting
        t.Columns.Add("Converted", GetType(Boolean))
        t.Columns.Add("RateUsed", GetType(Decimal))
        t.Columns.Add("Package", GetType(String))
        t.Columns.Add("PackageFrom", GetType(String)) ' supplier the package was borrowed from; "" = the row's own
        ' The supplier's place in the supplier list (Seq without the row number), so "Supplier, then Price" sorts
        ' the prices within each supplier.
        t.Columns.Add("SupplierRank", GetType(Integer))
        Return t
    End Function

    ''' <summary>Grid columns are defined here rather than auto-generated, so we can have photo and link columns.</summary>
    Private Sub ConfigureGridColumns()
        dgvResults.AutoGenerateColumns = False
        dgvResults.Columns.Clear()

        ' Row number, always first and frozen so it stays in view when scrolling sideways.
        Dim colNo = AddTextColumn("No", "#", 42)
        colNo.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        colNo.Frozen = True

        colThumb = New DataGridViewImageColumn With {
            .Name = "Thumb",
            .HeaderText = "",
            .Width = ImageCache.ThumbSize + 8,
            .ImageLayout = DataGridViewImageCellLayout.Normal,
            .SortMode = DataGridViewColumnSortMode.NotSortable,
            .Resizable = DataGridViewTriState.False,
            .ToolTipText = "Click for a bigger photo"
        }
        colThumb.DefaultCellStyle.NullValue = Nothing ' otherwise the grid draws its own red-X icon
        colThumb.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        dgvResults.Columns.Add(colThumb)

        ' Sorted by hand (see ColumnHeaderMouseClick): by supplier-list order, keeping each supplier's rows in order.
        AddTextColumn("Supplier", "Supplier", 90).SortMode = DataGridViewColumnSortMode.Programmatic

        colSupplierPartLink = New DataGridViewLinkColumn With {
            .Name = "SupplierPartNo",
            .DataPropertyName = "SupplierPartNo",
            .HeaderText = "Supplier part no.",
            .Width = 104,
            .SortMode = DataGridViewColumnSortMode.Automatic,
            .TrackVisitedState = False,
            .LinkBehavior = LinkBehavior.HoverUnderline,
            .ToolTipText = "Click to open the product page"
        }
        dgvResults.Columns.Add(colSupplierPartLink)

        AddTextColumn("MfrPartNo", "Mfr part no.", 140)
        AddTextColumn("Manufacturer", "Manufacturer", 110)
        AddTextColumn("Type", "Type", 160)
        AddTextColumn("Description", "Description", 280)
        AddTextColumn("Package", "Package", 80)
        Dim stock = AddTextColumn("Stock", "Stock", 42)
        stock.DefaultCellStyle.Format = "#,##0"
        stock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        Dim price = AddTextColumn("UnitPrice", "Price (1)", 56)
        price.DefaultCellStyle.Format = "#,##0.00##"
        price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        AddTextColumn("Currency", "Cur", 40)
        AddTextColumn("PriceBreaks", "Price breaks", 220)
        AddTextColumn("MOQ", "MOQ", 45).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        AddTextColumn("Availability", "Availability / lead", 90)
        ' Shows "Datasheet" (click to open) when the supplier gave a datasheet link, otherwise blank.
        colDatasheetLink = New DataGridViewLinkColumn With {
            .Name = "Datasheet",
            .DataPropertyName = "DatasheetUrl",
            .HeaderText = "Datasheet",
            .Width = 75,
            .SortMode = DataGridViewColumnSortMode.Automatic,
            .TrackVisitedState = False,
            .LinkBehavior = LinkBehavior.HoverUnderline
        }
        dgvResults.Columns.Add(colDatasheetLink)
        AddTextColumn("Status", "Status", 54)

        ' Header tooltips (click a header to sort; drag to reorder; drag the edge to resize).
        Dim tips As New Dictionary(Of String, String) From {
            {"No", "Part number in this list - stays with the part until the next search, even if rows are removed or sorted." & vbCrLf & "Website-link rows have no number, so the last # matches Total parts."},
            {"Supplier", "Who sells it. Blue italic rows are website links for suppliers without an API."},
            {"MfrPartNo", "Manufacturer part number."},
            {"Manufacturer", "Who makes the part."},
            {"Type", "Kind of part, in the supplier's own words (its category). Use Types… to hide whole types." & vbCrLf & "Blank for website links, and for the odd part a supplier hasn't categorised."},
            {"Description", "Supplier's description of the part."},
            {"Package", "Package / case, e.g. 8-SOIC, TSSOP, SO8 - in the supplier's own wording." & vbCrLf & "From DigiKey, Farnell, CPC, Newark and TME (from its description). Mouser's search doesn't give it," & vbCrLf &
             "so Mouser rows borrow it (in orange) from another supplier's listing of exactly the same manufacturer part number."},
            {"Stock", "Quantity in stock now. Green = in stock, grey row = none."},
            {"UnitPrice", "Price each at the lowest price break (usually qty 1)."},
            {"Currency", "Currency of the prices in this row."},
            {"PriceBreaks", "Quantity+ unit price, for each price break."},
            {"MOQ", "Minimum order quantity."},
            {"Availability", "Stock wording, packaging and lead time from the supplier."},
            {"Status", "Lifecycle status, e.g. Active, Obsolete, Not recommended for new designs."},
            {"Datasheet", "Click to open the datasheet. Blank when the supplier didn't give one (TME and website links never do)."}
        }
        For Each kv In tips
            dgvResults.Columns(kv.Key).ToolTipText = kv.Value & vbCrLf & SortHint
        Next
        ' Header clicks are handled here (ColumnHeaderMouseClick), so several columns can be sorted at once.
        For Each col As DataGridViewColumn In dgvResults.Columns
            If col.SortMode <> DataGridViewColumnSortMode.NotSortable Then col.SortMode = DataGridViewColumnSortMode.Programmatic
        Next
    End Sub

    Private Function CurrentColumnLayout() As Dictionary(Of String, ColumnLayout)
        Return dgvResults.Columns.Cast(Of DataGridViewColumn)().ToDictionary(
            Function(c) c.Name,
            Function(c) New ColumnLayout With {.Width = c.Width, .DisplayIndex = c.DisplayIndex})
    End Function

    ''' <summary>Restores widths and (drag-and-drop) column order saved by the user.</summary>
    Private Sub ApplyColumnLayout(layout As Dictionary(Of String, ColumnLayout))
        If layout Is Nothing OrElse layout.Count = 0 Then Return
        For Each kv In layout
            If Not dgvResults.Columns.Contains(kv.Key) Then Continue For ' column renamed/removed in a later version
            Dim col = dgvResults.Columns(kv.Key)
            If col.Resizable <> DataGridViewTriState.False Then col.Width = Math.Max(col.MinimumWidth, kv.Value.Width)
        Next
        ' The # column always stays first. The rest go after it in their saved order, set lowest first
        ' so each move doesn't disturb the ones already placed. (Layouts saved before # existed still work.)
        Dim position = 1
        For Each kv In layout.Where(Function(x) x.Key <> "No").OrderBy(Function(x) x.Value.DisplayIndex)
            If dgvResults.Columns.Contains(kv.Key) Then
                dgvResults.Columns(kv.Key).DisplayIndex = Math.Min(position, dgvResults.Columns.Count - 1)
                position += 1
            End If
        Next
        ' Columns added in a later version than the saved layout go just after the column they follow by default.
        For Each col As DataGridViewColumn In dgvResults.Columns
            If Not layout.ContainsKey(col.Name) AndAlso col.Index > 0 Then
                col.DisplayIndex = Math.Min(dgvResults.Columns(col.Index - 1).DisplayIndex + 1, dgvResults.Columns.Count - 1)
            End If
        Next
        dgvResults.Columns("No").DisplayIndex = 0
    End Sub

    Private Function AddTextColumn(dataName As String, header As String, width As Integer) As DataGridViewTextBoxColumn
        Dim col As New DataGridViewTextBoxColumn With {
            .Name = dataName,
            .DataPropertyName = dataName,
            .HeaderText = header,
            .Width = width,
            .SortMode = DataGridViewColumnSortMode.Automatic
        }
        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
        dgvResults.Columns.Add(col)
        Return col
    End Function

    Private Sub ApplyThumbnailSetting()
        Dim show = _settings.ShowThumbnails
        colThumb.Visible = show
        Dim h = If(show, ThumbRowHeight, dgvResults.Font.Height + 9)
        dgvResults.RowTemplate.Height = h
        SizeRows()
    End Sub

    ''' <summary>Website-link rows are half height (no photo, one line of text); never smaller than a plain text row.</summary>
    Private Sub SizeRows(Optional first As Integer = 0, Optional count As Integer = -1)
        Dim normal = dgvResults.RowTemplate.Height
        Dim linkHeight = Math.Max(dgvResults.Font.Height + 9, normal \ 2)
        Dim last = If(count < 0, dgvResults.Rows.Count - 1, Math.Min(dgvResults.Rows.Count - 1, first + count - 1))
        For i = first To last
            Dim r = dgvResults.Rows(i)
            Dim v = RowView(r.Index)
            Dim want = If(v IsNot Nothing AndAlso CBool(v("LinkOnly")), linkHeight, normal)
            If r.Height <> want Then r.Height = want
        Next
    End Sub

    ' Rows are re-created when the grid is sorted or filtered, and added as results arrive - size them each time.
    ' The grid also clears every header's sort arrow when it reloads (it doesn't know about our own sorting),
    ' so put the arrows and level numbers back.
    Private Sub dgvResults_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles dgvResults.DataBindingComplete
        SizeRows()
        RefreshSortHeaders()
    End Sub

    Private Sub dgvResults_RowsAdded(sender As Object, e As DataGridViewRowsAddedEventArgs) Handles dgvResults.RowsAdded
        SizeRows(e.RowIndex, e.RowCount)
    End Sub

    Private Shared Function MpnKey(mpn As String) As String
        Return If(mpn, "").Trim().ToUpperInvariant()
    End Function

    ''' <summary>The row's own photo, or one borrowed from another supplier with the same MPN.</summary>
    Private Function ImageUrlsFor(row As DataRowView) As (Thumb As String, Large As String)
        Dim thumb = Field(row, "ImageUrl")
        If thumb.Length = 0 Then
            If CBool(row("LinkOnly")) Then Return ("", "")
            Dim borrowed As (Thumb As String, Large As String) = Nothing
            If _imagesByMpn.TryGetValue(MpnKey(Field(row, "MfrPartNo")), borrowed) Then Return borrowed
            Return ("", "")
        End If
        Dim large = Field(row, "LargeImageUrl")
        Return (thumb, If(large.Length > 0, large, thumb))
    End Function

    Private Sub _images_ThumbnailReady(sender As Object, e As EventArgs) Handles _images.ThumbnailReady
        If colThumb.Visible Then dgvResults.InvalidateColumn(colThumb.Index)
    End Sub

    Private Function AddResult(p As PartResult, seq As Integer) As DataRow
        Dim row = _table.Rows.Add(
            p.Supplier,
            p.ManufacturerPartNumber,
            p.Manufacturer,
            p.Description,
            p.Category,
            If(p.Stock.HasValue, CObj(p.Stock.Value), DBNull.Value),
            If(p.UnitPrice.HasValue, CObj(p.UnitPrice.Value), DBNull.Value),
            p.Currency,
            p.PriceBreaksText(),
            If(p.MinOrderQty.HasValue, CObj(p.MinOrderQty.Value), DBNull.Value),
            p.StockInfo,
            p.SupplierPartNumber,
            p.Status,
            p.ProductUrl,
            p.DatasheetUrl,
            p.ImageUrl,
            p.LargeImageUrl,
            p.IsLinkOnly,
            False,
            seq,
            _batch * OrderPerBatch + seq,  ' Order
            DBNull.Value,                  ' No - given when the search / Load more finishes
            If(p.UnitPrice.HasValue, CObj(p.UnitPrice.Value), DBNull.Value),
            p.Currency,
            p.PriceBreaksText(),
            String.Join(";", p.PriceBreaks.OrderBy(Function(b) b.Quantity).
                        Select(Function(b) b.Quantity.ToString(CultureInfo.InvariantCulture) & ":" & b.Price.ToString(CultureInfo.InvariantCulture))),
            False,
            DBNull.Value,
            p.Package,
            "",
            seq \ SeqPerSupplier)
        ConvertRow(row)

        Dim key = MpnKey(p.ManufacturerPartNumber)
        If p.ImageUrl.Length > 0 AndAlso key.Length > 0 AndAlso Not _imagesByMpn.ContainsKey(key) Then
            _imagesByMpn(key) = (p.ImageUrl, If(p.LargeImageUrl.Length > 0, p.LargeImageUrl, p.ImageUrl))
        End If
        If p.Package.Length > 0 AndAlso key.Length > 0 AndAlso Not _packagesByMpn.ContainsKey(key) Then
            _packagesByMpn(key) = (p.Package, p.Supplier)
        End If
        Return row
    End Function

    ''' <summary>
    ''' Rows with no package (Mouser's search never gives one) take it from another supplier's row with exactly the
    ''' same manufacturer part number. Kept in the table, so sorting and Filter include it; shown in its own colour.
    ''' </summary>
    Private Sub BorrowPackages()
        For Each r As DataRow In _table.Rows
            If CBool(r("LinkOnly")) OrElse Convert.ToString(r("Package")).Length > 0 Then Continue For
            Dim found As (Package As String, Supplier As String) = Nothing
            If _packagesByMpn.TryGetValue(MpnKey(Convert.ToString(r("MfrPartNo"))), found) AndAlso
               found.Supplier <> Convert.ToString(r("Supplier")) Then
                r("Package") = found.Package
                r("PackageFrom") = found.Supplier
            End If
        Next
    End Sub

    Private _nextRowNo As Integer = 1
    ' 0 = the search itself, 1 = first Load more, ... Each batch's rows sort after the previous batch's.
    Private _batch As Integer
    Private Const OrderPerBatch As Long = 10_000_000L

    ''' <summary>Numbers any parts that don't have a # yet, in the default (arrival) order.
    ''' Website-link rows aren't parts, so they get no number - the last # then always equals "Total parts".</summary>
    Private Sub NumberNewRows()
        Dim unnumbered = _table.Rows.Cast(Of DataRow)().
            Where(Function(r) r("No") Is DBNull.Value AndAlso Not CBool(r("LinkOnly"))).
            OrderBy(Function(r) CLng(r("Order"))).ToList()
        For Each r In unnumbered
            r("No") = _nextRowNo
            _nextRowNo += 1
        Next
    End Sub

    Private Const SeqPerSupplier As Integer = 100000

    Private Function SeqFor(s As Supplier, rowNumber As Integer) As Integer
        Return _suppliers.IndexOf(s) * SeqPerSupplier + rowNumber
    End Function

    Private Sub ApplyFilter()
        Dim parts As New List(Of String) From {"Removed = false"}
        ' Unticked suppliers' rows are hidden, not deleted - tick them again to bring them back.
        Dim hidden = lvSuppliers.Items.Cast(Of ListViewItem)().Where(Function(i) Not i.Checked).
            Select(Function(i) "'" & DirectCast(i.Tag, Supplier).Name.Replace("'", "''") & "'").ToList()
        If hidden.Count > 0 Then parts.Add($"Supplier NOT IN ({String.Join(", ", hidden)})")
        If chkInStock.Checked Then parts.Add("(LinkOnly = true OR Stock > 0)")
        ' Active parts only / the ticked status rows. LIKE ignores case (the table isn't CaseSensitive);
        ' parts with no status always stay.
        If chkActive.Checked Then parts.Add(StatusNotLike(NotActiveWords))
        Dim terms = HiddenStatusTerms()
        If terms.Count > 0 Then parts.Add(StatusNotLike(terms))
        ' Types unticked in the Types… window (blank type = "(no type)"). Website-link rows always stay.
        If _hiddenTypes.Count > 0 Then
            parts.Add("(LinkOnly = true OR ISNULL(Type, '') NOT IN (" &
                      String.Join(", ", _hiddenTypes.Select(Function(t) "'" & t.Replace("'", "''") & "'")) & "))")
        End If
        Dim f = txtFilter.Text.Trim()
        If f.Length > 0 Then
            Dim pat = EscapeLike(f)
            parts.Add($"(LinkOnly = true OR MfrPartNo LIKE '%{pat}%' OR Description LIKE '%{pat}%' OR Type LIKE '%{pat}%' OR Package LIKE '%{pat}%' " &
                      $"OR Manufacturer LIKE '%{pat}%' OR Supplier LIKE '%{pat}%' OR SupplierPartNo LIKE '%{pat}%')")
        End If
        _table.DefaultView.RowFilter = String.Join(" AND ", parts)
        ResyncGrid()
        UpdateCountStatus()
    End Sub

    ''' <summary>Status words that mean a part isn't active, for Active parts only.</summary>
    Private Shared ReadOnly NotActiveWords As String() = {"obsolete", "discontinued", "not recommended", "nrnd", "end of life", "eol",
                                                          "last time", "no longer", "not for new"}

    ''' <summary>Row filter keeping website links and parts whose Status contains none of <paramref name="words"/>.</summary>
    Private Shared Function StatusNotLike(words As IEnumerable(Of String)) As String
        Return "(LinkOnly = true OR NOT (" &
               String.Join(" OR ", words.Select(Function(w) $"ISNULL(Status, '') LIKE '%{EscapeLike(w)}%'")) & "))"
    End Function

    ''' <summary>The text of every ticked status row that has some.</summary>
    Private Function HiddenStatusTerms() As List(Of String)
        Return _statusRows.Where(Function(r) r.Check.Checked).Select(Function(r) r.Box.Text.Trim()).
            Where(Function(t) t.Length > 0).ToList()
    End Function

    Private Shared Function EscapeLike(s As String) As String
        Dim sb As New StringBuilder()
        For Each c In s
            Select Case c
                Case "'"c : sb.Append("''")
                Case "["c, "]"c, "*"c, "%"c : sb.Append("[").Append(c).Append("]")
                Case Else : sb.Append(c)
            End Select
        Next
        Return sb.ToString()
    End Function

    ''' <summary>Types… needs some parts to list, and shows how many types are hidden.</summary>
    Private Sub UpdateTypesButton()
        btnTypes.Enabled = Not _busy AndAlso _table.Rows.Cast(Of DataRow)().Any(Function(r) Not CBool(r("LinkOnly")))
        btnTypes.Text = If(_hiddenTypes.Count > 0, $"Types ({_hiddenTypes.Count} hidden)…", "Types…")
        ' Wide enough for its text, never narrower than the other buttons.
        btnTypes.Width = Math.Max(btnSearch.Width, TextRenderer.MeasureText(btnTypes.Text, btnTypes.Font).Width + 20)
    End Sub

    ''' <summary>
    ''' Lists every part type in the current results with tick boxes; unticked types are hidden from the grid
    ''' (not deleted) until the next search. Modal, so nothing else can happen until OK or Cancel.
    ''' </summary>
    Private Sub btnTypes_Click(sender As Object, e As EventArgs) Handles btnTypes.Click
        If _busy Then Return
        Dim types = _table.Rows.Cast(Of DataRow)().
            Where(Function(r) Not CBool(r("LinkOnly")) AndAlso Not CBool(r("Removed"))).
            GroupBy(Function(r) Convert.ToString(r("Type")).Trim(), StringComparer.OrdinalIgnoreCase).
            Select(Function(g) New TypesForm.TypeEntry With {
                .Name = g.Key,
                .Count = g.Count(),
                .Suppliers = g.Select(Function(r) Convert.ToString(r("Supplier"))).Distinct().ToList()}).
            OrderBy(Function(t) t.Name.Length = 0).ThenBy(Function(t) t.Name, StringComparer.OrdinalIgnoreCase).
            ToList()
        If types.Count = 0 Then Return

        Using f As New TypesForm(types, _hiddenTypes)
            If f.ShowDialog(Me) <> DialogResult.OK Then Return
            _hiddenTypes.Clear()
            _hiddenTypes.UnionWith(f.HiddenTypes)
        End Using
        ApplyFilter()
    End Sub

    Private Sub UpdateCountStatus()
        UpdateStatusTicks()
        UpdateTypesButton()
        UpdateTotalParts()
        Dim shown = _table.DefaultView.Count
        Dim total = _table.Rows.Count
        If total = 0 Then Return
        Dim removed = RemovedCount()
        SetStatus($"{shown} of {total} results shown." &
                  If(removed > 0, $" ({removed} removed - right-click > Restore removed rows to bring them back)", "") &
                  If(HiddenNote().Length > 0, " " & HiddenNote(), ""))
    End Sub

    ''' <summary>"Total parts = 140", plus "(showing 98)" when Filter / In stock only / Remove hide some. Website-link rows don't count.</summary>
    Private Sub UpdateTotalParts()
        Dim parts = _table.Rows.Cast(Of DataRow)().Count(Function(r) Not CBool(r("LinkOnly")))
        Dim showing = _table.DefaultView.Cast(Of DataRowView)().Count(Function(v) Not CBool(v("LinkOnly")))
        lblTotal.Text = $"Total parts = {parts:#,##0}" & If(showing < parts, $"  (showing {showing:#,##0})", "")
    End Sub

    Private Function RemovedCount() As Integer
        Return _table.Rows.Cast(Of DataRow)().Count(Function(r) CBool(r("Removed")))
    End Function

    ''' <summary>Hides the selected rows until the next search (or Restore).</summary>
    Private Sub RemoveSelectedRows()
        ' Collect first: each change re-filters the view, which shifts the grid's rows.
        Dim rows = dgvResults.SelectedRows.Cast(Of DataGridViewRow)().
            Select(Function(r) RowView(r.Index)).
            Where(Function(v) v IsNot Nothing).Select(Function(v) v.Row).ToList()
        If rows.Count = 0 Then Return
        For Each r In rows
            r("Removed") = True
        Next
        UpdateCountStatus()
    End Sub

    Private Sub RestoreRemovedRows()
        For Each r As DataRow In _table.Rows
            If CBool(r("Removed")) Then r("Removed") = False
        Next
        UpdateCountStatus()
    End Sub

    ''' <summary>The part behind grid row <paramref name="index"/>, or Nothing. The grid can briefly have more rows
    ''' than the filtered view (it missed a re-filter), and asking it for one of those throws - see ResyncGrid.</summary>
    Private Function RowView(index As Integer) As DataRowView
        If index < 0 OrElse index >= _table.DefaultView.Count OrElse index >= dgvResults.Rows.Count Then Return Nothing
        Return TryCast(dgvResults.Rows(index).DataBoundItem, DataRowView)
    End Function

    ''' <summary>
    ''' Makes the grid rebuild its rows if it's out of step with the filtered view. Seen once after ticking
    ''' In stock only: the grid kept its old rows, painted ones that were gone, and crashed ("Index 16 does
    ''' not have a value"). The cause wasn't found, so this puts it right whenever it happens.
    ''' </summary>
    Private Sub ResyncGrid()
        If dgvResults.Rows.Count = _table.DefaultView.Count Then Return
        DirectCast(dgvResults.BindingContext(_table.DefaultView), CurrencyManager).Refresh()
    End Sub

    ' A row the grid can't find in the view: rebuild the grid (once it has finished painting) instead of
    ' showing the grid's error box.
    Private Sub dgvResults_DataError(sender As Object, e As DataGridViewDataErrorEventArgs) Handles dgvResults.DataError
        e.ThrowException = False
        BeginInvoke(New Action(AddressOf ResyncGrid))
    End Sub

    Private Function CurrentRow() As DataRowView
        If dgvResults.CurrentRow Is Nothing Then Return Nothing
        Return RowView(dgvResults.CurrentRow.Index)
    End Function

    Private Shared Function Field(row As DataRowView, name As String) As String
        If row Is Nothing OrElse row(name) Is DBNull.Value Then Return ""
        Return Convert.ToString(row(name))
    End Function

#End Region

#Region "Searching"

    Private Async Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        ' A normal search: just the toolbar settings, no Advanced filters.
        Await RunSearchAsync(New AdvancedSearchOptions With {
            .Query = cboSearch.Text.Trim(),
            .Mode = If(cboMode.SelectedIndex = 1, SearchMode.PartNumber, SearchMode.Keyword),
            .MaxResults = CInt(nudMax.Value)})
    End Sub

    Private Async Sub btnAdvanced_Click(sender As Object, e As EventArgs) Handles btnAdvanced.Click
        If _busy Then Return
        ' Open with last time's choices, but today's search text, batch size and ticked suppliers.
        Dim start = _settings.LastAdvanced
        ' (Unless Search for was left blank last time and the search box still shows those words.)
        Dim typed = cboSearch.Text.Trim()
        If typed.Length > 0 AndAlso Not (start.ContainsIsQuery AndAlso String.Equals(typed, start.Query, StringComparison.OrdinalIgnoreCase)) Then
            start.Query = typed
            start.ContainsIsQuery = False
        End If
        start.MaxResults = CInt(nudMax.Value)
        start.Suppliers = lvSuppliers.Items.Cast(Of ListViewItem)().Where(Function(i) i.Checked).Select(Function(i) i.Text).ToList()

        Dim o As AdvancedSearchOptions
        Using f As New AdvancedSearchForm(start, _suppliers.Select(Function(s) s.Name).ToList())
            If f.ShowDialog(Me) <> DialogResult.OK Then Return
            o = f.Options
        End Using
        _settings.LastAdvanced = o
        cboSearch.Text = o.Query
        nudMax.Value = Math.Max(nudMax.Minimum, Math.Min(nudMax.Maximum, o.MaxResults))

        ' Make the supplier list match the window (without the "ticked after a search" auto-search).
        _loading = True
        For Each item As ListViewItem In lvSuppliers.Items
            item.Checked = o.Suppliers.Contains(item.Text)
            _settings.EnabledSuppliers(item.Text) = item.Checked
        Next
        _loading = False

        Await RunSearchAsync(o, fromAdvanced:=True)
    End Sub

    ' Ctrl+F opens Advanced search.
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        ' Enter in the open recent-searches list just picks that search (it would otherwise also press Search).
        If keyData = Keys.Enter AndAlso cboSearch.DroppedDown Then
            cboSearch.DroppedDown = False
            cboSearch.SelectionStart = cboSearch.Text.Length
            cboSearch.SelectionLength = 0
            Return True
        End If
        If keyData = (Keys.Control Or Keys.F) Then
            btnAdvanced.PerformClick()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ''' <summary>Runs a search (normal or Advanced) across the ticked suppliers.</summary>
    ''' <param name="fromAdvanced">True when it came from the Advanced search window, so the status line lists its options.</param>
    Private Async Function RunSearchAsync(opts As AdvancedSearchOptions, Optional fromAdvanced As Boolean = False) As Task
        Dim query = opts.Query
        If query.Length = 0 Then
            cboSearch.Select()
            Return
        End If
        If _busy Then Return
        Dim selected = lvSuppliers.Items.Cast(Of ListViewItem)().Where(Function(i) i.Checked).ToList()
        If selected.Count = 0 Then
            MessageBox.Show(Me, "Tick at least one supplier on the left.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        _settings.AddRecentSearch(query)
        RefreshAutoComplete()
        _table.Rows.Clear()
        _paging.Clear()
        _searched.Clear()
        _nextRowNo = 1
        _batch = 0
        UpdateTotalParts()
        For Each item As ListViewItem In lvSuppliers.Items
            item.SubItems(SubStatus).Text = If(item.Checked, "Waiting…", "")
            item.SubItems(SubHits).Text = ""
            item.SubItems(SubTime).Text = ""
        Next

        _active = opts
        _advancedHidden = 0
        _codeMpn = ""
        _hiddenTypes.Clear()
        ApplyFilter()       ' supplier ticks may have just changed
        UpdatePriceHeader() ' "Price (25)" when a quantity is set

        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim ct = _cts.Token
        Dim max = opts.MaxResults
        Dim filters = opts.SupplierFilters()
        Dim sw = Stopwatch.StartNew()
        ' A normal search's Manufacturer part no. mode isn't an "Advanced" option, so only describe Advanced searches.
        Dim advanced = If(fromAdvanced, opts.Describe(), "")

        SetBusy(True)
        SetStatus($"Searching {selected.Count} supplier(s) for ""{query}""" & If(advanced.Length > 0, $" ({advanced})", "") & "…")
        Dim codeNote = ""
        Try
            If opts.Mode = SearchMode.SupplierCode Then
                codeNote = Await SearchByOrderCodeAsync(selected, query, max, filters, ct)
            Else
                Await Task.WhenAll(selected.Select(Function(i) SearchOneAsync(i, query, opts.Mode, max, filters, ct)))
            End If
        Finally
            SetBusy(False)
        End Try
        NumberNewRows()

        Dim apiHits = _table.Rows.Cast(Of DataRow)().Count(Function(r) Not CBool(r("LinkOnly")))
        Dim more = LoadMoreCandidates().Select(Function(i) i.Text).ToList()
        SetStatus(If(codeNote.Length > 0, codeNote & " ", "") &
                  $"""{query}"": {apiHits} part(s) found in {sw.Elapsed.TotalSeconds:0.0}s. " &
                  If(advanced.Length > 0, $"Advanced: {advanced.TrimEnd("."c)}" & If(_advancedHidden > 0, $" ({_advancedHidden} left out)", "") & ". ", "") &
                  If(more.Count > 0, $"More available from {String.Join(", ", more)} - click Load more. ", "") &
                  If(HiddenNote().Length > 0, HiddenNote(), "Double-click a row to open it in your browser."))
    End Function

    ''' <summary>
    ''' When every part found is hidden (e.g. In stock only with nothing in stock), says what's hiding them,
    ''' so an empty grid doesn't look like "no results". Empty when some parts are showing.
    ''' </summary>
    Private Function HiddenNote() As String
        Dim found = _table.Rows.Cast(Of DataRow)().Count(Function(r) Not CBool(r("LinkOnly")))
        Dim showing = _table.DefaultView.Cast(Of DataRowView)().Count(Function(v) Not CBool(v("LinkOnly")))
        If found = 0 OrElse showing > 0 Then Return ""
        Dim why As New List(Of String)
        If chkInStock.Checked Then why.Add("In stock only (none are in stock)")
        If chkActive.Checked Then why.Add("Active parts only")
        If HiddenStatusTerms().Count > 0 Then why.Add("the ticked Status filters")
        If txtFilter.Text.Trim().Length > 0 Then why.Add("the Filter box")
        If _hiddenTypes.Count > 0 Then why.Add("hidden Types")
        If lvSuppliers.Items.Cast(Of ListViewItem)().Any(Function(i) Not i.Checked) Then why.Add("unticked suppliers")
        If RemovedCount() > 0 Then why.Add("removed rows")
        Return $"All {found} are hidden by {String.Join(", ", why)} - change that to see them."
    End Function

    ''' <summary>
    ''' Order-code search: look the code up at every API supplier (it belongs to one of them), take the
    ''' manufacturer part number from the match, then search the other suppliers for that exact part.
    ''' Returns a note for the status bar.
    ''' </summary>
    Private Async Function SearchByOrderCodeAsync(selected As List(Of ListViewItem), code As String, max As Integer,
                                                  filters As SupplierFilters, ct As CancellationToken) As Task(Of String)
        Dim apiItems = selected.Where(Function(i) DirectCast(i.Tag, Supplier).IsConfigured).ToList()
        Await Task.WhenAll(apiItems.Select(Function(i) SearchOneAsync(i, code, SearchMode.SupplierCode, max, filters, ct)))

        Dim hit = _table.Rows.Cast(Of DataRow)().FirstOrDefault(
            Function(r) Not CBool(r("LinkOnly")) AndAlso Convert.ToString(r("MfrPartNo")).Length > 0)
        If hit Is Nothing Then
            ' Nobody recognised it: give the website-link suppliers the code as it is.
            For Each i In selected.Except(apiItems)
                Await SearchOneAsync(i, code, SearchMode.SupplierCode, max, filters, ct)
            Next
            Return $"No supplier recognised order code ""{code}""."
        End If

        Dim mpn = Convert.ToString(hit("MfrPartNo"))
        Dim foundAt = Convert.ToString(hit("Supplier"))
        _codeMpn = mpn
        ' Everyone who didn't recognise the code (including website-link suppliers) gets the part number instead.
        Dim others = selected.Where(Function(i)
                                        Dim st As PagingState = Nothing
                                        Return Not _paging.TryGetValue(DirectCast(i.Tag, Supplier), st) OrElse st.RowCount = 0
                                    End Function).ToList()
        SetStatus($"Order code {code} is {mpn} at {foundAt} - now searching the other suppliers for {mpn}…")
        Await Task.WhenAll(others.Select(Function(i) SearchOneAsync(i, mpn, SearchMode.ExactPartNumber, max, filters, ct)))
        Return $"Order code {code} is {mpn} ({foundAt}); the other suppliers were searched for {mpn}."
    End Function

    ''' <summary>The Price column shows the price at the Advanced search quantity, when one is set,
    ''' and the "Prices in" currency when that's ticked, e.g. "Price (100) GBP".</summary>
    Private Sub UpdatePriceHeader()
        Dim col = dgvResults.Columns("UnitPrice")
        Dim qty = If(_active Is Nothing, 0, _active.Quantity)
        col.HeaderText = If(qty > 0, $"Price ({qty:#,##0})", "Price (1)") & If(Converting, " " & _settings.ConvertCurrency, "")
        col.ToolTipText = If(qty > 0, $"Price each when buying {qty:#,##0} (from the price breaks).",
                             "Price each at the lowest price break (usually qty 1).") &
                          If(Converting, vbCrLf & $"Shown in {_settings.ConvertCurrency} (purple = converted, approximate).", "") &
                          vbCrLf & SortHint
        RefreshSortHeaders()
    End Sub

#Region "Prices in one currency"

    ' "Prices in GBP" (toolbar): every price is shown in the currency chosen in Settings, using ECB exchange rates.
    ' Rates are saved in settings.json and only fetched again after 12 hours (see ExchangeRates).
    Private _settingConvert As Boolean      ' True while the tick box is set from code (don't fetch)
    Private _fetchingRates As Boolean

    ''' <summary>True when prices are being shown converted (ticked, and rates are available).</summary>
    Private ReadOnly Property Converting As Boolean
        Get
            Return chkConvert.Checked AndAlso _settings.Rates.HasRates
        End Get
    End Property

    Private Sub UpdateConvertText()
        chkConvert.Text = $"Prices in {_settings.ConvertCurrency}"
    End Sub

    Private Async Sub chkConvert_CheckedChanged(sender As Object, e As EventArgs) Handles chkConvert.CheckedChanged
        If _settingConvert Then Return
        _settings.ConvertPrices = chkConvert.Checked
        If chkConvert.Checked AndAlso _settings.Rates.NeedsRefresh Then
            Await RefreshRatesAsync(showDone:=True)
        Else
            ApplyConversion()
        End If
    End Sub

    ''' <summary>
    ''' Fetches new exchange rates if the saved ones are 12+ hours old (or there are none), then shows the prices.
    ''' If the rates can't be fetched, the last saved ones are used; with none at all, the tick is taken off.
    ''' </summary>
    Private Async Function RefreshRatesAsync(showDone As Boolean) As Task
        If _fetchingRates Then Return
        If _settings.Rates.NeedsRefresh Then
            _fetchingRates = True
            Dim oldStatus = lblStatus.Text
            SetStatus("Getting today's exchange rates…")
            Try
                _settings.Rates = Await ExchangeRates.DownloadAsync(CancellationToken.None)
                Try
                    _settings.Save() ' so a restart within 12 hours doesn't fetch again
                Catch
                    ' saved again on exit anyway
                End Try
            Catch ex As Exception
                If Not _settings.Rates.HasRates Then
                    _settingConvert = True
                    chkConvert.Checked = False
                    _settingConvert = False
                    _settings.ConvertPrices = False
                    SetStatus($"Prices in {_settings.ConvertCurrency} is off: {ex.Message} Check your internet connection and tick it again.")
                    ApplyConversion(showStatus:=False)
                    Return
                End If
                SetStatus($"Couldn't get new exchange rates ({ex.Message}) - using the saved {_settings.Rates.Describe()}.")
                showDone = False
            Finally
                _fetchingRates = False
            End Try
            If Not showDone AndAlso lblStatus.Text.StartsWith("Getting today", StringComparison.Ordinal) Then SetStatus(oldStatus)
        End If
        ApplyConversion(showStatus:=showDone)
    End Function

    ''' <summary>Shows every row's prices converted, or back as the supplier gave them, to match the tick box.</summary>
    Private Sub ApplyConversion(Optional showStatus As Boolean = True)
        For Each r As DataRow In _table.Rows
            ConvertRow(r)
        Next
        UpdatePriceHeader()
        dgvResults.Invalidate()
        SetBomCurrency()
        If showStatus Then
            If Converting Then
                Dim missing = _table.Rows.Cast(Of DataRow)().
                    Where(Function(r) Not CBool(r("LinkOnly")) AndAlso Not CBool(r("Converted")) AndAlso
                                      Convert.ToString(r("OrigCurrency")).Length > 0 AndAlso
                                      Not String.Equals(Convert.ToString(r("OrigCurrency")), _settings.ConvertCurrency, StringComparison.OrdinalIgnoreCase)).
                    Select(Function(r) Convert.ToString(r("OrigCurrency")).ToUpperInvariant()).Distinct().ToList()
                SetStatus($"Prices shown in {_settings.ConvertCurrency} (purple = converted) at {_settings.Rates.Describe()}." &
                          If(_settings.Rates.AgeDays > 4, $" These rates are {_settings.Rates.AgeDays} days old.", "") &
                          If(missing.Count > 0, $" No rate for {String.Join(", ", missing)}, so those are shown as the supplier gave them.", "") &
                          " Converted prices are approximate.")
            Else
                SetStatus("Prices shown as each supplier gave them.")
            End If
        End If
    End Sub

    ''' <summary>Puts one row's converted prices (or the supplier's own) into the Price, Cur and Price breaks columns.</summary>
    Private Sub ConvertRow(r As DataRow)
        If CBool(r("LinkOnly")) Then Return
        Dim origCur = Convert.ToString(r("OrigCurrency"))
        Dim target = _settings.ConvertCurrency
        Dim rate As Decimal? = Nothing
        If Converting AndAlso Not String.Equals(origCur, target, StringComparison.OrdinalIgnoreCase) Then
            rate = _settings.Rates.Rate(origCur, target)
        End If
        If rate.HasValue Then
            r("UnitPrice") = If(r("OrigUnitPrice") Is DBNull.Value, CObj(DBNull.Value), CObj(Math.Round(CDec(r("OrigUnitPrice")) * rate.Value, 4)))
            r("Currency") = target
            r("PriceBreaks") = BreaksText(Convert.ToString(r("BreaksRaw")), rate.Value)
            r("Converted") = True
            r("RateUsed") = rate.Value
        Else
            r("UnitPrice") = r("OrigUnitPrice")
            r("Currency") = origCur
            r("PriceBreaks") = r("OrigBreaks")
            r("Converted") = False
            r("RateUsed") = DBNull.Value
        End If
    End Sub

    ''' <summary>"1+ 0.20   10+ 0.139" from "1:0.2;10:0.139", with every price multiplied by the rate.</summary>
    Private Shared Function BreaksText(raw As String, rate As Decimal) As String
        Return String.Join("   ", raw.Split({";"c}, StringSplitOptions.RemoveEmptyEntries).
            Select(Function(pair)
                       Dim bits = pair.Split(":"c)
                       Dim price = Math.Round(Decimal.Parse(bits(1), CultureInfo.InvariantCulture) * rate, 4)
                       Return $"{bits(0)}+ {price:0.00##}"
                   End Function))
    End Function

    ''' <summary>A price in the "Prices in" currency when converting (for Advanced search's Max price each).</summary>
    Private Function PriceInShownCurrency(price As Decimal, currency As String) As Decimal
        If Not Converting Then Return price
        Dim rate = _settings.Rates.Rate(currency, _settings.ConvertCurrency)
        Return If(rate.HasValue, price * rate.Value, price)
    End Function

    ' Hover over a converted price: what the supplier actually charges, and the rate used.
    Private Sub dgvResults_CellToolTipTextNeeded(sender As Object, e As DataGridViewCellToolTipTextNeededEventArgs) Handles dgvResults.CellToolTipTextNeeded
        If e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim prop = dgvResults.Columns(e.ColumnIndex).DataPropertyName
        If prop = "Package" Then
            Dim r = RowView(e.RowIndex)
            If r IsNot Nothing AndAlso Field(r, "PackageFrom").Length > 0 Then
                e.ToolTipText = $"{Field(r, "Supplier")} doesn't give the package - this is {Field(r, "PackageFrom")}'s, " &
                                $"for the same manufacturer part number ({Field(r, "MfrPartNo")})."
            End If
            Return
        End If
        If prop <> "UnitPrice" AndAlso prop <> "Currency" AndAlso prop <> "PriceBreaks" Then Return
        Dim row = RowView(e.RowIndex)
        If row Is Nothing OrElse CBool(row("LinkOnly")) OrElse Not chkConvert.Checked Then Return
        Dim origCur = Field(row, "OrigCurrency")
        If CBool(row("Converted")) Then
            Dim orig = If(row("OrigUnitPrice") Is DBNull.Value, "", $"{CDec(row("OrigUnitPrice")):0.00##} {origCur}")
            e.ToolTipText = $"{Field(row, "Supplier")} price: {orig}" & vbCrLf &
                            $"Breaks: {Field(row, "OrigBreaks")}" & vbCrLf &
                            $"Converted at 1 {origCur} = {CDec(row("RateUsed")):0.#####} {_settings.ConvertCurrency} ({_settings.Rates.Describe()})." & vbCrLf &
                            "Approximate - the supplier's own rate, VAT and import charges will differ."
        ElseIf Converting AndAlso origCur.Length > 0 AndAlso
               Not String.Equals(origCur, _settings.ConvertCurrency, StringComparison.OrdinalIgnoreCase) Then
            e.ToolTipText = $"No exchange rate for {origCur}, so this price is shown as {Field(row, "Supplier")} gave it."
        End If
    End Sub

#End Region

    ''' <summary>Where a supplier's results got to, so Load more can fetch the next page.</summary>
    Private Class PagingState
        Public Query As String
        Public Mode As SearchMode
        Public Filters As SupplierFilters ' in stock / RoHS, applied by the supplier
        Public Filtered As Integer        ' parts left out by Advanced search filters
        Public NextOffset As Integer      ' where the next page starts (in the supplier's own numbering)
        Public Total As Integer?          ' total matches the supplier reported
        Public LastPageFull As Boolean    ' fallback when no total is given
        Public RowCount As Integer        ' rows added to the grid (after removing duplicates)
        Public Duplicates As Integer      ' repeats the supplier sent that were skipped
        Public Fetched As Integer         ' parts the supplier has sent so far (before duplicates and filters)
        Public Batches As Integer         ' pages fetched: the search, then each Load more
        Public PageSize As Integer        ' Max/supp. for this search
        Public LastAddedNothing As Boolean ' the last Load more added no new rows
        Public ReadOnly Seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        Public ReadOnly Property HasMore As Boolean
            Get
                Return If(Total.HasValue, NextOffset < Total.Value, LastPageFull)
            End Get
        End Property
    End Class

    Private ReadOnly _paging As New Dictionary(Of Supplier, PagingState)
    Private _busy As Boolean

    ' The current search (normal or Advanced), so a supplier ticked afterwards can be searched on its own,
    ' and so Load more applies the same filters.
    Private _active As AdvancedSearchOptions
    Private _advancedHidden As Integer   ' parts left out by Advanced filters in this search
    Private _codeMpn As String = ""      ' order-code search: the part number the code turned out to be
    ' Part types unticked in the Types… window ("" = parts with no type). Cleared by every new search.
    Private ReadOnly _hiddenTypes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    ' Names of suppliers already searched (or given a link row) in the current search.
    Private ReadOnly _searched As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Runs one supplier's first page. Called on the UI thread, so it's safe to touch controls after each Await.</summary>
    Private Async Function SearchOneAsync(item As ListViewItem, query As String, mode As SearchMode,
                                          max As Integer, filters As SupplierFilters, ct As CancellationToken,
                                          Optional added As List(Of DataRow) = Nothing) As Task
        Dim s = DirectCast(item.Tag, Supplier)
        If added Is Nothing Then added = New List(Of DataRow)
        _searched.Add(s.Name)
        If Not s.IsConfigured Then
            added.Add(AddResult(s.MakeLinkResult(query), SeqFor(s, 0)))
            item.SubItems(SubStatus).Text = "Link added"
            Return
        End If
        Dim st As New PagingState With {.Query = query, .Mode = mode, .Filters = filters}
        _paging(s) = st
        Await FetchPageAsync(item, st, max, ct, added)
    End Function

    ''' <summary>A supplier ticked after the search: search just that one now, adding its rows at the end.</summary>
    Private Async Function SearchJustThisSupplierAsync(item As ListViewItem) As Task
        Dim s = DirectCast(item.Tag, Supplier)
        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim added As New List(Of DataRow)
        _batch += 1 ' goes at the end, like Load more
        SetBusy(True)
        ' After an order-code search, search the newcomer for the part number the code turned out to be.
        Dim q = If(_active.Mode = SearchMode.SupplierCode AndAlso _codeMpn.Length > 0, _codeMpn, _active.Query)
        Dim m = If(_active.Mode = SearchMode.SupplierCode AndAlso _codeMpn.Length > 0, SearchMode.ExactPartNumber, _active.Mode)
        SetStatus($"Searching {s.Name} for ""{q}""…")
        Try
            Await SearchOneAsync(item, q, m, _active.MaxResults, _active.SupplierFilters(), _cts.Token, added)
        Finally
            SetBusy(False)
        End Try
        NumberNewRows()
        SelectRows(added)
        Dim parts = added.Where(Function(r) Not CBool(r("LinkOnly"))).Count()
        SetStatus(If(s.IsConfigured,
                     $"Added {parts} part(s) from {s.Name} at the end. The new rows are selected.",
                     $"Added a {s.Name} website link at the end."))
    End Function

    ''' <summary>Fetches the next page for one supplier and adds its rows (to <paramref name="added"/> too).</summary>
    Private Async Function FetchPageAsync(item As ListViewItem, st As PagingState, pageSize As Integer,
                                          ct As CancellationToken, added As List(Of DataRow)) As Task
        Dim s = DirectCast(item.Tag, Supplier)
        Dim firstPage = st.NextOffset = 0
        item.SubItems(SubStatus).Text = If(firstPage, "Searching…", "Loading more…")
        item.ToolTipText = ""
        Dim sw = Stopwatch.StartNew()
        Try
            Dim page = Await s.SearchAsync(st.Query, st.Mode, pageSize, st.NextOffset, st.Filters, ct)
            st.NextOffset += pageSize
            st.PageSize = pageSize
            st.Batches += 1
            st.Fetched += page.Parts.Count
            st.Total = page.TotalAvailable
            st.LastPageFull = page.Parts.Count >= pageSize
            Dim before = st.RowCount
            _table.BeginLoadData()
            For Each p In page.Parts
                ' Skip parts already shown: pages can overlap (Mouser repeats one now and then; DigiKey's
                ' exact matches). Manufacturer is in the key because Mouser lists e.g. both onsemi's and
                ' Diodes' LM358 with no Mouser part number - different parts, not duplicates.
                Dim key = p.SupplierPartNumber & "|" & p.ManufacturerPartNumber & "|" & p.Manufacturer
                If key.Length > 2 AndAlso Not st.Seen.Add(key) Then
                    st.Duplicates += 1
                    Continue For
                End If
                ' Advanced search: price at the chosen quantity, then the "only show" filters.
                If _active IsNot Nothing Then
                    If _active.Quantity > 0 Then p.UnitPrice = AdvancedSearchOptions.PriceAt(p, _active.Quantity)
                    If Not _active.Matches(p, AddressOf PriceInShownCurrency) Then
                        st.Filtered += 1
                        _advancedHidden += 1
                        Continue For
                    End If
                End If
                st.RowCount += 1
                added.Add(AddResult(p, SeqFor(s, st.RowCount)))
            Next
            _table.EndLoadData()
            ' Earlier rows (e.g. Mouser) may now be able to borrow a photo, and a package, from these.
            If colThumb.Visible Then dgvResults.InvalidateColumn(colThumb.Index)
            BorrowPackages()
            item.SubItems(SubStatus).Text = If(st.RowCount = 0, "No matches", "OK")
            st.LastAddedNothing = Not firstPage AndAlso st.RowCount = before
            If st.LastAddedNothing Then item.SubItems(SubStatus).Text = "No more"
            item.ToolTipText = HitsToolTip(s, st)
        Catch ex As OperationCanceledException When ct.IsCancellationRequested
            item.SubItems(SubStatus).Text = "Cancelled"
        Catch ex As Exception
            ' Timeout, bad key, rate limit... show it, and on a first page fall back to a website link.
            item.SubItems(SubStatus).Text = "Error"
            item.ToolTipText = $"{s.Name}: {ex.Message}"
            If firstPage Then AddResult(s.MakeLinkResult(st.Query), SeqFor(s, 0))
        End Try
        item.SubItems(SubHits).Text = HitsText(s, st)
        item.SubItems(SubTime).Text = $"{sw.Elapsed.TotalSeconds:0.0}s"
        UpdateCountStatus()
        UpdateLoadMoreButton()
    End Function

    ''' <summary>"50 / 312" while there's more to load; just the count once everything has been fetched
    ''' (which can be a little under the supplier's total, if it sent the same part twice).</summary>
    Private Function HitsText(s As Supplier, st As PagingState) As String
        If st.Total.HasValue AndAlso HasMore(s) Then Return $"{st.RowCount} / {st.Total}"
        Return st.RowCount.ToString()
    End Function

    ''' <summary>
    ''' The supplier row's tooltip: what its Hits numbers mean - how many match, how many have been fetched,
    ''' how many are shown and why the rest aren't, and whether Load more has more to get.
    ''' </summary>
    Private Function HitsToolTip(s As Supplier, st As PagingState) As String
        Dim lines As New List(Of String) From {$"{s.Name} - Hits {HitsText(s, st)}"}
        If st.Total.HasValue Then lines.Add($"{st.Total.Value:N0} part{If(st.Total.Value = 1, "", "s")} match ""{st.Query}"" at {s.Name}.")
        If HasMore(s) Then
            lines.Add($"Fetched so far: {st.Fetched:N0}" &
                      If(st.Batches > 1, $" ({st.Batches} batches of {st.PageSize}).", "."))
        Else
            lines.Add(If(st.Fetched = 0, "Nothing found.", $"All {st.Fetched:N0} have been fetched."))
        End If
        If st.Fetched > 0 Then
            Dim shown = $"Shown: {st.RowCount:N0}"
            If st.Filtered > 0 Then
                Dim filters = If(_active?.Describe(), "")
                shown &= $" - {st.Filtered:N0} left out by the Advanced search filters" & If(filters.Length > 0, $" ({filters}).", ".")
            Else
                shown &= "."
            End If
            lines.Add(shown)
        End If
        If st.Duplicates > 0 Then lines.Add($"{st.Duplicates:N0} sent twice by {s.Name} and skipped.")
        If st.LastAddedNothing Then lines.Add("The last Load more found nothing new that passed the filters.")
        If HasMore(s) Then
            Dim left = If(st.Total.HasValue, $"{Math.Max(0, st.Total.Value - st.Fetched):N0} more to fetch: ", "There may be more: ")
            lines.Add(left & $"click Load more for the next {st.PageSize}.")
        End If
        Return String.Join(vbCrLf, lines)
    End Function

    Private Function HasMore(s As Supplier) As Boolean
        Dim st As PagingState = Nothing
        Return _paging.TryGetValue(s, st) AndAlso s.CanLoadMore(st.Mode) AndAlso st.HasMore
    End Function

    ''' <summary>Ticked suppliers that have another page to fetch.</summary>
    Private Function LoadMoreCandidates() As List(Of ListViewItem)
        Return lvSuppliers.Items.Cast(Of ListViewItem)().
            Where(Function(i) i.Checked AndAlso HasMore(DirectCast(i.Tag, Supplier))).ToList()
    End Function

    ' (Which suppliers have more is shown in the Hits column and the status bar; the button's tooltip is in the designer.)
    Private Sub UpdateLoadMoreButton()
        btnLoadMore.Enabled = Not _busy AndAlso LoadMoreCandidates().Count > 0
    End Sub

    Private Async Sub btnLoadMore_Click(sender As Object, e As EventArgs) Handles btnLoadMore.Click
        Await LoadMoreAsync(LoadMoreCandidates())
    End Sub

    Private Async Function LoadMoreAsync(items As List(Of ListViewItem)) As Task
        items = items.Where(Function(i) HasMore(DirectCast(i.Tag, Supplier))).ToList()
        If items.Count = 0 OrElse _busy Then Return

        _cts?.Dispose()
        _cts = New CancellationTokenSource()
        Dim ct = _cts.Token
        Dim pageSize = CInt(nudMax.Value)
        Dim added As New List(Of DataRow)
        _batch += 1 ' this batch goes at the end
        SetBusy(True)
        SetStatus($"Loading more from {String.Join(", ", items.Select(Function(i) i.Text))}…")
        Try
            Await Task.WhenAll(items.Select(Function(i) FetchPageAsync(i, _paging(DirectCast(i.Tag, Supplier)), pageSize, ct, added)))
        Finally
            SetBusy(False)
        End Try

        NumberNewRows()
        SelectRows(added)
        Dim summary = String.Join(", ", items.Select(Function(i)
                                                         Dim st = _paging(DirectCast(i.Tag, Supplier))
                                                         Return $"{i.Text} {st.RowCount}" & If(st.Total.HasValue, $" of {st.Total}", "")
                                                     End Function))
        SetStatus($"Loaded {added.Count} more ({summary}). The new rows are selected.")
    End Function

    ''' <summary>Selects the given rows and scrolls to the first one, so new results are easy to spot.</summary>
    Private Sub SelectRows(rows As List(Of DataRow))
        If rows.Count = 0 Then Return
        Dim wanted As New HashSet(Of DataRow)(rows)
        dgvResults.ClearSelection()
        Dim first = -1
        For Each r As DataGridViewRow In dgvResults.Rows
            Dim v = RowView(r.Index)
            If v IsNot Nothing AndAlso wanted.Contains(v.Row) Then
                r.Selected = True
                If first < 0 Then first = r.Index
            End If
        Next
        If first >= 0 Then dgvResults.FirstDisplayedScrollingRowIndex = first
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        _cts?.Cancel()
    End Sub

    Private Sub SetBusy(busy As Boolean)
        _busy = busy
        If busy Then cboSearch.DroppedDown = False
        btnSearch.Enabled = Not busy
        btnAdvanced.Enabled = Not busy
        btnCancel.Enabled = busy
        btnSettings.Enabled = Not busy
        UseWaitCursor = busy
        If Not busy Then
            ' Windows only redraws the cursor when the mouse moves, so the hourglass would stay until then.
            ' Put the arrow back and "move" the mouse to where it already is, so it changes straight away.
            Cursor.Current = Cursors.Default
            Cursor.Position = Cursor.Position
        End If
        UpdateLoadMoreButton()
        UpdateTypesButton()
    End Sub

    Private Sub SetStatus(text As String)
        lblStatus.Text = text
    End Sub

#End Region

#Region "Grid events"

    Private Sub chkInStock_CheckedChanged(sender As Object, e As EventArgs) Handles chkInStock.CheckedChanged, chkActive.CheckedChanged
        ApplyFilter()
    End Sub

    ' The left panel's status rows: tick box + the Status text to hide. Made in code - there are StatusFilterCount of them.
    Private ReadOnly _statusRows As New List(Of (Check As CheckBox, Box As TextBox, Tick As Label))
    Private Const StatusBoxWidth As Integer = 290 ' at 100% scaling

    ''' <summary>The filters under the supplier list share its background, in every theme - the text boxes' outlines
    ''' mark the rows. (The tick boxes and labels are see-through, so they pick it up too.)</summary>
    Private Sub MatchFiltersToSupplierList()
        pnlFilters.BackColor = lvSuppliers.BackColor
        pnlFilters.ForeColor = lvSuppliers.ForeColor
        UpdateStatusTicks() ' the ✓ colour follows the theme
    End Sub

    Private Sub BuildStatusFilterRows()
        tlpFilters.SuspendLayout()
        For Each f In _settings.StatusFilters
            Dim term = If(f.Term, "")
            Dim hasText = term.Trim().Length > 0
            Dim chk As New CheckBox With {.AutoSize = True, .Checked = f.Hide AndAlso hasText, .Enabled = hasText,
                                          .Margin = New Padding(3, 5, 0, 2)}
            ' Fixed width (not stretched with the panel), scaled for the Windows display scaling.
            Dim box As New TextBox With {.Text = term, .Anchor = AnchorStyles.Left, .Margin = New Padding(3, 2, 3, 2),
                                         .Width = CInt(StatusBoxWidth * DeviceDpi / 96.0)}
            Theme.SetCueBanner(box, "Status text to hide…")
            tipMain.SetToolTip(chk, "Tick to hide parts whose Status contains the text beside it.")
            tipMain.SetToolTip(box, "Type it as it appears in the Status column (capitals don't matter)." & vbCrLf &
                                    "Parts whose Status contains this are hidden while the box beside it is ticked.")
            Dim rowIndex = tlpFilters.RowCount
            tlpFilters.RowCount += 1
            tlpFilters.RowStyles.Add(New RowStyle())
            tlpFilters.Controls.Add(chk, 0, rowIndex)
            tlpFilters.Controls.Add(box, 1, rowIndex)
            Dim tick As New Label With {.AutoSize = True, .Font = New Font(Font, FontStyle.Bold), .Margin = New Padding(0, 5, 3, 2)}
            tlpFilters.Controls.Add(tick, 2, rowIndex)
            AddHandler chk.CheckedChanged, Sub() ApplyFilter()
            AddHandler box.TextChanged, Sub() StatusBoxChanged(chk, box)
            AddHandler box.Leave, Sub() StatusBoxLeft(chk, box)
            _statusRows.Add((chk, box, tick))
        Next
        tlpFilters.ResumeLayout()
    End Sub

    ''' <summary>Puts a green ✓ beside each ticked status row that is hiding at least one part of the current
    ''' results; hovering over it says how many.</summary>
    Private Sub UpdateStatusTicks()
        If _statusRows.Count = 0 Then Return
        Dim statuses = _table.Rows.Cast(Of DataRow)().
            Where(Function(r) Not CBool(r("LinkOnly")) AndAlso Not CBool(r("Removed"))).
            Select(Function(r) Convert.ToString(r("Status"))).ToList()
        For Each r In _statusRows
            Dim term = r.Box.Text.Trim()
            Dim hiding = If(r.Check.Checked AndAlso term.Length > 0,
                            statuses.Where(Function(s) s.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).Count(), 0)
            r.Tick.Text = If(hiding > 0, "✓", "")
            r.Tick.ForeColor = Theme.GoodText
            tipMain.SetToolTip(r.Tick, If(hiding > 0, $"Hiding {hiding} part(s) whose Status contains ""{term}"".", Nothing))
        Next
    End Sub

    ''' <summary>Typing in a status box re-filters straight away when its row is ticked. Clearing it only
    ''' unticks and greys the tick box once the user leaves the box.</summary>
    Private Sub StatusBoxChanged(chk As CheckBox, box As TextBox)
        If box.Text.Trim().Length > 0 Then chk.Enabled = True
        If chk.Checked Then ApplyFilter()
    End Sub

    Private Sub StatusBoxLeft(chk As CheckBox, box As TextBox)
        If box.Text.Trim().Length > 0 Then Return
        chk.Checked = False
        chk.Enabled = False
    End Sub

    Private Sub txtFilter_TextChanged(sender As Object, e As EventArgs) Handles txtFilter.TextChanged
        ApplyFilter()
    End Sub

    Private Sub dgvResults_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvResults.CellFormatting
        If e.RowIndex < 0 OrElse e.CellStyle Is Nothing Then Return
        Dim row = RowView(e.RowIndex)
        If row Is Nothing Then Return

        If e.ColumnIndex = colThumb.Index Then
            ' Website-link rows have no part, so no photo (not even the "no photo" box).
            e.Value = If(CBool(row("LinkOnly")), Nothing, _images.GetThumbnail(ImageUrlsFor(row).Thumb))
            e.FormattingApplied = True
            Return
        End If

        If dgvResults.Columns(e.ColumnIndex).Name = "No" Then
            e.CellStyle.ForeColor = Theme.MutedText ' quieter than the part data, in either theme
            Return
        End If

        ' Rows with a page to open but no supplier part no. still get a clickable link.
        If e.ColumnIndex = colSupplierPartLink.Index AndAlso Field(row, "SupplierPartNo").Length = 0 AndAlso
           Field(row, "ProductUrl").Length > 0 Then
            e.Value = If(CBool(row("LinkOnly")), "Search website", "View")
            e.FormattingApplied = True
        End If

        ' The datasheet cell holds the URL; show a short link word instead (blank when there's none).
        If e.ColumnIndex = colDatasheetLink.Index Then
            Dim url = Field(row, "DatasheetUrl")
            e.Value = If(url.Length > 0, "Datasheet", "")
            e.FormattingApplied = True
            dgvResults.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText = url
            Return
        End If

        If CBool(row("LinkOnly")) Then
            e.CellStyle.ForeColor = Theme.LinkText
            e.CellStyle.Font = _linkFont
        ElseIf row("Stock") IsNot DBNull.Value AndAlso CLng(row("Stock")) = 0 Then
            e.CellStyle.ForeColor = Theme.MutedText
        ElseIf dgvResults.Columns(e.ColumnIndex).DataPropertyName = "Stock" Then
            e.CellStyle.ForeColor = Theme.GoodText
        End If

        ' A package borrowed from another supplier's listing of the same part, in its own colour.
        If dgvResults.Columns(e.ColumnIndex).DataPropertyName = "Package" AndAlso Field(row, "PackageFrom").Length > 0 Then
            e.CellStyle.ForeColor = Theme.BorrowedText
            e.CellStyle.SelectionForeColor = Theme.BorrowedText
        End If

        ' Converted prices (Prices in GBP) in their own colour, so they read as estimates.
        If Not CBool(row("LinkOnly")) AndAlso CBool(row("Converted")) Then
            Select Case dgvResults.Columns(e.ColumnIndex).DataPropertyName
                Case "UnitPrice", "Currency", "PriceBreaks"
                    e.CellStyle.ForeColor = Theme.ConvertedText
                    e.CellStyle.SelectionForeColor = Theme.ConvertedText
            End Select
        End If
    End Sub

#Region "Sorting by one or more columns"

    Private Const SortHint As String = "Click to sort; Shift+click to sort by this as well; right-click for more."

    ' The columns sorted by, in order (first = main sort). Empty = arrival order.
    Private ReadOnly _sortKeys As New List(Of (Column As String, Descending As Boolean))
    Private _headerMenu As ContextMenuStrip
    Private _headerMenuColumn As String

    ''' <summary>What a column sorts on: Supplier by its place in the supplier list, Datasheet by its link, the rest by their own values.</summary>
    Private Function SortField(columnName As String) As String
        Select Case columnName
            Case "Supplier" : Return "SupplierRank"
            Case Else : Return dgvResults.Columns(columnName).DataPropertyName
        End Select
    End Function

    Private Sub dgvResults_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvResults.ColumnHeaderMouseClick
        Dim col = dgvResults.Columns(e.ColumnIndex)
        If col.SortMode = DataGridViewColumnSortMode.NotSortable Then Return
        If e.Button = MouseButtons.Right Then
            ShowHeaderMenu(col.Name)
        ElseIf e.Button = MouseButtons.Left Then
            If (ModifierKeys And Keys.Shift) = Keys.Shift Then ThenSortBy(col.Name) Else SortBy(col.Name)
        End If
    End Sub

    ''' <summary>Sort by just this column; if it's already the only sort, reverse it.</summary>
    Private Sub SortBy(columnName As String)
        Dim reverse = _sortKeys.Count = 1 AndAlso _sortKeys(0).Column = columnName AndAlso Not _sortKeys(0).Descending
        _sortKeys.Clear()
        _sortKeys.Add((columnName, reverse))
        ApplySort()
    End Sub

    ''' <summary>Add this column as the next sort level; if it's already one, reverse that level.</summary>
    Private Sub ThenSortBy(columnName As String)
        Dim i = _sortKeys.FindIndex(Function(k) k.Column = columnName)
        If i >= 0 Then
            _sortKeys(i) = (columnName, Not _sortKeys(i).Descending)
        Else
            _sortKeys.Add((columnName, False))
        End If
        ApplySort()
    End Sub

    Private Sub ClearSorting()
        _sortKeys.Clear()
        ApplySort()
    End Sub

    ''' <summary>Sorts the rows by every level in turn, then arrival order for anything still equal.</summary>
    Private Sub ApplySort()
        ' Website-link rows always stay at the top, whatever is sorted.
        Dim parts As New List(Of String) From {"LinkOnly DESC"}
        parts.AddRange(_sortKeys.Select(Function(k) SortField(k.Column) & If(k.Descending, " DESC", "")))
        parts.Add("Order")
        _table.DefaultView.Sort = String.Join(", ", parts)
        RefreshSortHeaders()
    End Sub

    ' Small level numbers added to header text when sorting by more than one column.
    Private Const LevelDigits As String = "¹²³⁴⁵⁶⁷⁸⁹"

    ''' <summary>Arrows on the sorted columns, and a level number on each when there's more than one.</summary>
    Private Sub RefreshSortHeaders()
        For Each col As DataGridViewColumn In dgvResults.Columns
            Dim text = col.HeaderText.TrimEnd(LevelDigits.ToCharArray()).TrimEnd()
            Dim i = _sortKeys.FindIndex(Function(k) k.Column = col.Name)
            If i >= 0 AndAlso _sortKeys.Count > 1 AndAlso i < LevelDigits.Length Then text &= " " & LevelDigits(i)
            If col.HeaderText <> text Then col.HeaderText = text
            col.HeaderCell.SortGlyphDirection =
                If(i < 0, SortOrder.None, If(_sortKeys(i).Descending, SortOrder.Descending, SortOrder.Ascending))
        Next
    End Sub

    Private Sub ShowHeaderMenu(columnName As String)
        If _headerMenu Is Nothing Then
            _headerMenu = New ContextMenuStrip()
            _headerMenu.Items.Add("Sort by this", Nothing, Sub() SortBy(_headerMenuColumn))
            _headerMenu.Items.Add("Next sort by this", Nothing, Sub() ThenSortBy(_headerMenuColumn))
            _headerMenu.Items.Add(New ToolStripSeparator())
            _headerMenu.Items.Add("Clear sorting", Nothing, Sub() ClearSorting())
        End If
        _headerMenuColumn = columnName
        Dim header = dgvResults.Columns(columnName).HeaderText.TrimEnd(LevelDigits.ToCharArray()).TrimEnd()
        Dim level = _sortKeys.FindIndex(Function(k) k.Column = columnName)
        _headerMenu.Items(0).Text = $"Sort by {header}"
        _headerMenu.Items(1).Text = If(level >= 0, $"Reverse {header} (sort level {level + 1})", $"Next sort by {header}")
        _headerMenu.Items(1).Enabled = _sortKeys.Count > 0
        _headerMenu.Items(3).Enabled = _sortKeys.Count > 0
        Theme.StyleMenu(_headerMenu)
        _headerMenu.Show(Cursor.Position)
    End Sub

#End Region

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        ' The photo and link cells already act on a single click.
        If e.RowIndex < 0 OrElse e.ColumnIndex = colThumb.Index OrElse e.ColumnIndex = colSupplierPartLink.Index OrElse
           e.ColumnIndex = colDatasheetLink.Index Then Return
        OpenUrl(Field(CurrentRow(), "ProductUrl"))
    End Sub

    Private Sub dgvResults_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim row = RowView(e.RowIndex)
        If e.ColumnIndex = colSupplierPartLink.Index Then
            OpenUrl(Field(row, "ProductUrl"))
        ElseIf e.ColumnIndex = colDatasheetLink.Index Then
            Dim url = Field(row, "DatasheetUrl")
            If url.Length > 0 Then OpenUrl(url)
        End If
    End Sub

    Private Sub dgvResults_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellClick
        If e.RowIndex < 0 OrElse e.ColumnIndex <> colThumb.Index Then Return
        Dim row = RowView(e.RowIndex)
        If row Is Nothing Then Return
        Dim urls = ImageUrlsFor(row)
        If urls.Thumb.Length = 0 OrElse _images.HasFailed(urls.Thumb) Then Return ' nothing to enlarge

        Dim title = String.Join(" - ", {Field(row, "MfrPartNo"), Field(row, "Manufacturer")}.Where(Function(s) s.Length > 0))
        ' One pop-up at a time: clicking another thumbnail replaces it.
        If _photoPopup IsNot Nothing AndAlso Not _photoPopup.IsDisposed Then _photoPopup.Close()
        _photoPopup = New ImagePopupForm(urls.Large, urls.Thumb, If(title.Length > 0, title, "Photo"), Field(row, "Description"))
        _photoPopup.Location = Cursor.Position + New Size(16, 16)
        _photoPopup.Show(Me)
    End Sub

    Private Sub dgvResults_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellMouseEnter
        dgvResults.Cursor = If(e.RowIndex >= 0 AndAlso e.ColumnIndex = colThumb.Index, Cursors.Hand, Cursors.Default)
    End Sub

    Private Sub dgvResults_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvResults.CellMouseDown
        ' Right-click selects the row under the mouse before the context menu shows -
        ' unless it's already part of a multi-row selection, which is kept (for Remove).
        If e.Button = MouseButtons.Right AndAlso e.RowIndex >= 0 AndAlso e.ColumnIndex >= 0 AndAlso
           Not dgvResults.Rows(e.RowIndex).Selected Then
            dgvResults.CurrentCell = dgvResults.Rows(e.RowIndex).Cells(e.ColumnIndex)
        End If
    End Sub

    Private Sub cmsResults_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles cmsResults.Opening
        ' The part menu is for rows - not for a right-click on the column headers.
        Dim hit = dgvResults.HitTest(dgvResults.PointToClient(Cursor.Position).X, dgvResults.PointToClient(Cursor.Position).Y)
        If hit.Type = DataGridViewHitTestType.ColumnHeader OrElse hit.Type = DataGridViewHitTestType.TopLeftHeader Then
            e.Cancel = True
            Return
        End If
        Dim row = CurrentRow()
        Dim removed = RemovedCount()
        ' With every row removed there's nothing to click on, but Restore must still be reachable.
        If row Is Nothing AndAlso removed = 0 Then
            e.Cancel = True
            Return
        End If
        Dim selectedCount = dgvResults.SelectedRows.Count
        Dim bomCount = SelectedPartRows().Count
        mnuAddToBom.Enabled = bomCount > 0
        mnuAddToBom.Text = If(bomCount > 1, $"Add {bomCount} rows to BOM", "Add to BOM")
        mnuOpenProduct.Enabled = Field(row, "ProductUrl").Length > 0
        mnuOpenDatasheet.Enabled = Field(row, "DatasheetUrl").Length > 0
        mnuCopyMpn.Enabled = Field(row, "MfrPartNo").Length > 0
        mnuCopySupplierPart.Enabled = row IsNot Nothing AndAlso Field(row, "SupplierPartNo").Length > 0 AndAlso Not CBool(row("LinkOnly"))
        mnuCopyRow.Enabled = selectedCount > 0
        mnuCopyRow.Text = If(selectedCount > 1, $"Copy {selectedCount} rows", "Copy row")
        mnuRemoveRows.Enabled = selectedCount > 0
        mnuRemoveRows.Text = If(selectedCount > 1, $"Remove {selectedCount} rows from results", "Remove from results")
        mnuRestoreRows.Enabled = removed > 0
        mnuRestoreRows.Text = If(removed > 0, $"Restore removed rows ({removed})", "Restore removed rows")

        ' Right-click on a Status cell that has some text: offer to hide every part with that status.
        _menuStatus = ""
        If hit.Type = DataGridViewHitTestType.Cell AndAlso dgvResults.Columns(hit.ColumnIndex).DataPropertyName = "Status" Then
            _menuStatus = Field(RowView(hit.RowIndex), "Status").Trim()
        End If
        mnuSep3.Visible = _menuStatus.Length > 0
        mnuHideStatus.Visible = _menuStatus.Length > 0
        Dim shown = If(_menuStatus.Length > 40, _menuStatus.Substring(0, 40) & "…", _menuStatus)
        mnuHideStatus.Text = $"Hide parts with Status ""{shown.Replace("&", "&&")}"""
    End Sub

    ' The Status cell right-clicked, for Hide parts with this Status ("" = not a Status cell).
    Private _menuStatus As String = ""

    ''' <summary>Ticks the left-panel row holding this status, or puts it in the first empty row and ticks that.</summary>
    Private Sub mnuHideStatus_Click(sender As Object, e As EventArgs) Handles mnuHideStatus.Click
        If _menuStatus.Length = 0 Then Return
        Dim target = _statusRows.FirstOrDefault(Function(r) String.Equals(r.Box.Text.Trim(), _menuStatus, StringComparison.OrdinalIgnoreCase))
        If target.Box Is Nothing Then target = _statusRows.FirstOrDefault(Function(r) r.Box.Text.Trim().Length = 0)
        If target.Box Is Nothing Then
            MessageBox.Show(Me, $"All {_statusRows.Count} Status rows under the supplier list are in use." & vbCrLf &
                            "Clear one (or change its text), then try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        If target.Box.Text.Trim().Length = 0 Then target.Box.Text = _menuStatus
        target.Check.Enabled = True
        target.Check.Checked = True
        pnlFilters.ScrollControlIntoView(target.Box)
    End Sub

    Private Sub mnuOpenProduct_Click(sender As Object, e As EventArgs) Handles mnuOpenProduct.Click
        OpenUrl(Field(CurrentRow(), "ProductUrl"))
    End Sub

    Private Sub mnuOpenDatasheet_Click(sender As Object, e As EventArgs) Handles mnuOpenDatasheet.Click
        Dim url = Field(CurrentRow(), "DatasheetUrl")
        If url.StartsWith("//") Then url = "https:" & url
        OpenUrl(url)
    End Sub

    Private Sub mnuCopyMpn_Click(sender As Object, e As EventArgs) Handles mnuCopyMpn.Click
        CopyText(Field(CurrentRow(), "MfrPartNo"))
    End Sub

    Private Sub mnuCopySupplierPart_Click(sender As Object, e As EventArgs) Handles mnuCopySupplierPart.Click
        CopyText(Field(CurrentRow(), "SupplierPartNo"))
    End Sub

    Private Sub mnuCopyRow_Click(sender As Object, e As EventArgs) Handles mnuCopyRow.Click
        ' One tab-separated line per selected row, in grid order - pastes straight into Excel.
        Dim cols = VisibleColumns()
        Dim lines = dgvResults.SelectedRows.Cast(Of DataGridViewRow)().OrderBy(Function(r) r.Index).
            Select(Function(r) RowView(r.Index)).Where(Function(v) v IsNot Nothing).
            Select(Function(v) String.Join(vbTab, cols.Select(Function(c) Field(v, c.DataPropertyName))))
        CopyText(String.Join(vbCrLf, lines))
    End Sub

    Private Sub mnuRemoveRows_Click(sender As Object, e As EventArgs) Handles mnuRemoveRows.Click
        RemoveSelectedRows()
    End Sub

    Private Sub mnuRestoreRows_Click(sender As Object, e As EventArgs) Handles mnuRestoreRows.Click
        RestoreRemovedRows()
    End Sub

    Private Sub dgvResults_KeyDown(sender As Object, e As KeyEventArgs) Handles dgvResults.KeyDown
        If e.KeyCode = Keys.Delete AndAlso e.Modifiers = Keys.None Then
            RemoveSelectedRows()
            e.Handled = True
        End If
    End Sub

    Private Sub CopyText(text As String)
        If text.Length > 0 Then Clipboard.SetText(text)
    End Sub

    Private Sub OpenUrl(url As String)
        Try
            WebHelper.OpenInBrowser(url)
        Catch ex As Exception
            MessageBox.Show(Me, "Could not open " & url & vbCrLf & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

#End Region

#Region "Supplier list"

    ''' <summary>
    ''' Untick: hide that supplier's rows (kept in memory). Tick: show them again - or, if it wasn't part
    ''' of the current search, search just that supplier now.
    ''' </summary>
    Private Async Sub lvSuppliers_ItemChecked(sender As Object, e As ItemCheckedEventArgs) Handles lvSuppliers.ItemChecked
        If _loading Then Return
        Dim s = DirectCast(e.Item.Tag, Supplier)
        _settings.EnabledSuppliers(s.Name) = e.Item.Checked
        ApplyFilter()
        UpdateLoadMoreButton()

        Dim searchDone = _active IsNot Nothing AndAlso Not String.IsNullOrEmpty(_active.Query)
        If Not e.Item.Checked OrElse Not searchDone OrElse _searched.Contains(s.Name) Then Return
        If _busy Then
            ' Don't start a second search on top of one that's running.
            e.Item.SubItems(SubStatus).Text = "Not searched yet"
            e.Item.ToolTipText = $"{s.Name} was ticked during a search - press Search again to include it."
            Return
        End If
        Await SearchJustThisSupplierAsync(e.Item)
    End Sub

    Private Sub cmsSuppliers_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles cmsSuppliers.Opening
        Dim hasQuery = cboSearch.Text.Trim().Length > 0
        mnuOpenSupplierSearch.Enabled = hasQuery AndAlso lvSuppliers.SelectedItems.Count > 0
        mnuOpenAllSearches.Enabled = hasQuery

        Dim item = If(lvSuppliers.SelectedItems.Count > 0, lvSuppliers.SelectedItems(0), Nothing)
        Dim s = If(item Is Nothing, Nothing, DirectCast(item.Tag, Supplier))
        mnuLoadMoreSupplier.Enabled = s IsNot Nothing AndAlso Not _busy AndAlso HasMore(s)
        mnuLoadMoreSupplier.Text = If(s Is Nothing, "Load more", $"Load {nudMax.Value} more from {s.Name}")
    End Sub

    Private Async Sub mnuLoadMoreSupplier_Click(sender As Object, e As EventArgs) Handles mnuLoadMoreSupplier.Click
        If lvSuppliers.SelectedItems.Count = 0 Then Return
        Await LoadMoreAsync(New List(Of ListViewItem) From {lvSuppliers.SelectedItems(0)})
    End Sub

    Private Sub mnuOpenSupplierSearch_Click(sender As Object, e As EventArgs) Handles mnuOpenSupplierSearch.Click
        If lvSuppliers.SelectedItems.Count = 0 Then Return
        OpenUrl(DirectCast(lvSuppliers.SelectedItems(0).Tag, Supplier).GetSearchUrl(cboSearch.Text.Trim()))
    End Sub

    Private Sub mnuOpenAllSearches_Click(sender As Object, e As EventArgs) Handles mnuOpenAllSearches.Click
        For Each item As ListViewItem In lvSuppliers.Items
            If item.Checked Then OpenUrl(DirectCast(item.Tag, Supplier).GetSearchUrl(cboSearch.Text.Trim()))
        Next
    End Sub

    Private Sub lvSuppliers_DoubleClick(sender As Object, e As EventArgs) Handles lvSuppliers.DoubleClick
        mnuOpenSupplierSearch.PerformClick()
    End Sub

    ' Windows only shows a row's tooltip over its first column (the supplier name). Over Mode, Status, Hits and
    ' Time the same text is shown with this one instead, after the usual pause.
    Private ReadOnly _rowTip As New ToolTip With {.ShowAlways = True} ' also while e.g. the BOM window has the focus
    Private WithEvents _rowTipTimer As New Windows.Forms.Timer With {.Interval = 500}
    Private _rowTipItem As ListViewItem

    Private Sub lvSuppliers_MouseMove(sender As Object, e As MouseEventArgs) Handles lvSuppliers.MouseMove
        Dim hit = lvSuppliers.HitTest(e.Location)
        Dim target = If(hit.Item IsNot Nothing AndAlso hit.SubItem IsNot Nothing AndAlso
                        hit.Item.SubItems.IndexOf(hit.SubItem) > 0 AndAlso hit.Item.ToolTipText.Length > 0, hit.Item, Nothing)
        If target Is _rowTipItem Then Return
        HideRowTip()
        _rowTipItem = target
        If target IsNot Nothing Then _rowTipTimer.Start()
    End Sub

    ' The dark themes' hover code (DarkListHover) sends the list a "mouse left" after every move, so only hide when
    ' the pointer really is outside the list.
    Private Sub lvSuppliers_MouseLeave(sender As Object, e As EventArgs) Handles lvSuppliers.MouseLeave
        If lvSuppliers.ClientRectangle.Contains(lvSuppliers.PointToClient(Cursor.Position)) Then Return
        HideRowTip()
        _rowTipItem = Nothing
    End Sub

    Private Sub _rowTipTimer_Tick(sender As Object, e As EventArgs) Handles _rowTipTimer.Tick
        _rowTipTimer.Stop()
        If _rowTipItem Is Nothing OrElse _rowTipItem.ListView Is Nothing Then Return
        Dim p = lvSuppliers.PointToClient(Cursor.Position)
        Dim y = p.Y + SystemInformation.CursorSize.Height \ 2 + 4
        ' A ToolTip's first Show - and the first after each Hide - displays nothing (a WinForms quirk), so it's always shown twice.
        _rowTip.Show(_rowTipItem.ToolTipText, lvSuppliers, p.X, y, 15000)
        _rowTip.Show(_rowTipItem.ToolTipText, lvSuppliers, p.X, y, 15000)
    End Sub

    Private Sub HideRowTip()
        _rowTipTimer.Stop()
        _rowTip.Hide(lvSuppliers)
    End Sub

    ' Owner drawing is only used for the header (so it can go dark); rows draw normally.
    Private Sub lvSuppliers_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs) Handles lvSuppliers.DrawColumnHeader
        If Not Theme.IsCustom Then
            e.DrawDefault = True
            Return
        End If
        Using b As New SolidBrush(Theme.HeaderBack)
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using
        Using p As New Pen(Theme.BorderColor)
            e.Graphics.DrawLine(p, e.Bounds.Right - 1, e.Bounds.Top, e.Bounds.Right - 1, e.Bounds.Bottom - 1)
            e.Graphics.DrawLine(p, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1)
        End Using
        Dim flags = TextFormatFlags.VerticalCenter Or TextFormatFlags.EndEllipsis Or
                    If(e.Header.TextAlign = HorizontalAlignment.Right, TextFormatFlags.Right, TextFormatFlags.Left)
        TextRenderer.DrawText(e.Graphics, e.Header.Text, lvSuppliers.Font, Rectangle.Inflate(e.Bounds, -5, 0), Theme.TextColor, flags)
    End Sub

    Private Sub lvSuppliers_DrawItem(sender As Object, e As DrawListViewItemEventArgs) Handles lvSuppliers.DrawItem
        e.DrawDefault = True
    End Sub

    Private Sub lvSuppliers_DrawSubItem(sender As Object, e As DrawListViewSubItemEventArgs) Handles lvSuppliers.DrawSubItem
        e.DrawDefault = True
    End Sub

    ''' <summary>
    ''' In the dark themes Windows draws the row under the mouse in its own hover style - black text, ignoring the
    ''' list's colours - and the selected row with a dark border while the mouse is over it. This makes the list
    ''' forget the row under the mouse each time it moves, and gives that row its own lighter blue band instead.
    ''' </summary>
    Private NotInheritable Class DarkListHover
        Inherits NativeWindow ' on the list itself, for its mouse moves

        Private Const WM_MOUSEMOVE As Integer = &H200
        Private Const WM_MOUSELEAVE As Integer = &H2A3
        Private Const WM_NOTIFY As Integer = &H4E
        Private Const NM_CUSTOMDRAW As Integer = -12
        Private Const CDDS_ITEMPREPAINT As Integer = &H10001
        Private Const CDDS_SUBITEM As Integer = &H20000
        Private Const CDRF_NEWFONT As Integer = &H2
        Private Const WM_LBUTTONDOWN As Integer = &H201
        Private Const WM_LBUTTONUP As Integer = &H202

        <Runtime.InteropServices.DllImport("user32.dll")>
        Private Shared Function SendMessage(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As IntPtr) As IntPtr
        End Function

        <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
        Private Structure NMHDR
            Public hwndFrom As IntPtr
            Public idFrom As IntPtr
            Public code As Integer
        End Structure

        ' (NMHDR is kept as its own structure: on 64-bit Windows it's padded, so dwDrawStage starts 8 bytes on.)
        <Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Sequential)>
        Private Structure NMLVCUSTOMDRAW
            Public hdr As NMHDR
            Public dwDrawStage As Integer
            Public hdc As IntPtr
            Public left, top, right, bottom As Integer
            Public dwItemSpec As IntPtr
            Public uItemState As Integer
            Public lItemlParam As IntPtr
            Public clrText As Integer
            Public clrTextBk As Integer
        End Structure

        Private Shared ReadOnly TextOffset As Integer =
            Runtime.InteropServices.Marshal.OffsetOf(GetType(NMLVCUSTOMDRAW), "clrText").ToInt32()
        Private Shared ReadOnly BackOffset As Integer =
            Runtime.InteropServices.Marshal.OffsetOf(GetType(NMLVCUSTOMDRAW), "clrTextBk").ToInt32()

        Private ReadOnly _list As ListView
        Private ReadOnly _parentHook As ParentHook
        Private _hovered As Integer = -1 ' row with our own hover band
        Private _sendingLeave As Boolean

        Public Sub New(lv As ListView)
            _list = lv
            AddHandler lv.HandleCreated, Sub() AssignHandle(lv.Handle)
            AddHandler lv.HandleDestroyed, Sub() ReleaseHandle()
            If lv.IsHandleCreated Then AssignHandle(lv.Handle)
            ' The list tells its parent when it draws each row, so the band's colour is set there.
            _parentHook = New ParentHook(Me)
            Dim parent = lv.Parent
            AddHandler parent.HandleCreated, Sub() _parentHook.AssignHandle(parent.Handle)
            AddHandler parent.HandleDestroyed, Sub() _parentHook.ReleaseHandle()
            If parent.IsHandleCreated Then _parentHook.AssignHandle(parent.Handle)
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            If m.Msg = WM_MOUSELEAVE AndAlso Not _sendingLeave Then SetHovered(-1) ' the mouse really has left
            MyBase.WndProc(m)
            If Not Theme.IsDark OrElse Handle = IntPtr.Zero Then Return
            If m.Msg = WM_MOUSEMOVE Then
                Dim item = _list.GetItemAt(CShort(m.LParam.ToInt64() And &HFFFF), CShort((m.LParam.ToInt64() >> 16) And &HFFFF))
                SetHovered(If(item Is Nothing, -1, item.Index))
            End If
            ' After a move or a click (which can leave the clicked row marked as hovered, with no move since),
            ' telling the list the mouse has left makes it forget the row under the mouse (LVM_SETHOTITEM -1 doesn't).
            If m.Msg = WM_MOUSEMOVE OrElse m.Msg = WM_LBUTTONDOWN OrElse m.Msg = WM_LBUTTONUP Then
                _sendingLeave = True
                SendMessage(Handle, WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero)
                _sendingLeave = False
            End If
        End Sub

        ''' <summary>Moves the hover band to another row (-1 for none), redrawing both rows.</summary>
        Private Sub SetHovered(index As Integer)
            If index = _hovered Then Return
            Dim old = _hovered
            _hovered = index
            For Each i In {old, index}
                If i >= 0 AndAlso i < _list.Items.Count Then _list.Invalidate(_list.Items(i).Bounds)
            Next
        End Sub

        ''' <summary>
        ''' After the list has set a row's colours: the selected row gets its own text colour back (the list leaves
        ''' black for Windows' selection, which shows it on the dark blue), and the hovered row gets the band.
        ''' </summary>
        Private Sub AfterParentNotify(ByRef m As Message)
            If Not Theme.IsDark Then Return
            Dim cd = DirectCast(Runtime.InteropServices.Marshal.PtrToStructure(m.LParam, GetType(NMLVCUSTOMDRAW)), NMLVCUSTOMDRAW)
            If cd.hdr.hwndFrom <> Handle OrElse cd.hdr.code <> NM_CUSTOMDRAW OrElse
               (cd.dwDrawStage And Not CDDS_SUBITEM) <> CDDS_ITEMPREPAINT Then Return
            Dim index = cd.dwItemSpec.ToInt32()
            If index < 0 OrElse index >= _list.Items.Count Then Return
            Dim item = _list.Items(index)
            If item.Selected Then
                Runtime.InteropServices.Marshal.WriteInt32(m.LParam, TextOffset, ColorTranslator.ToWin32(item.ForeColor))
            ElseIf index = _hovered Then
                Runtime.InteropServices.Marshal.WriteInt32(m.LParam, BackOffset, ColorTranslator.ToWin32(Theme.RowHoverBack))
            Else
                Return
            End If
            m.Result = New IntPtr(m.Result.ToInt64() Or CDRF_NEWFONT) ' (use the colours we just set)
        End Sub

        Private NotInheritable Class ParentHook
            Inherits NativeWindow

            Private ReadOnly _owner As DarkListHover

            Public Sub New(owner As DarkListHover)
                _owner = owner
            End Sub

            Protected Overrides Sub WndProc(ByRef m As Message)
                MyBase.WndProc(m)
                If m.Msg = WM_NOTIFY Then _owner.AfterParentNotify(m)
            End Sub
        End Class
    End Class

    ' Stretch the last column to the edge, so no light-coloured header strip is left over.
    Private Sub lvSuppliers_Resize(sender As Object, e As EventArgs) Handles lvSuppliers.Resize
        If lvSuppliers.Columns.Count = 0 Then Return
        Dim others = lvSuppliers.Columns.Cast(Of ColumnHeader)().Take(lvSuppliers.Columns.Count - 1).Sum(Function(c) c.Width)
        colTime.Width = Math.Max(50, lvSuppliers.ClientSize.Width - others)
    End Sub

#End Region

#Region "BOM"

    ''' <summary>Every run starts with an empty BOM; saved ones are opened with Load in the BOM window.</summary>
    Private Sub CreateBomForm()
        _bomForm = New BomForm With {.LastFolder = _settings.BomFolder}
        SetBomCurrency()
    End Sub

    ''' <summary>Remembers the BOM window's place and the last BOM folder (not the BOM itself).</summary>
    Private Sub SaveBomSettings()
        If _bomForm Is Nothing Then Return
        If _bomForm.IsHandleCreated Then _settings.BomWindow = PlacementOf(_bomForm)
        _settings.BomFolder = _bomForm.LastFolder
    End Sub

    Private Shared Function PlacementOf(f As Form) As WindowPlacement
        Dim r = If(f.WindowState = FormWindowState.Normal, f.Bounds, f.RestoreBounds)
        Return New WindowPlacement With {.X = r.X, .Y = r.Y, .Width = r.Width, .Height = r.Height}
    End Function

    Private Sub SetBomCurrency()
        _bomForm?.SetCurrency(If(Converting, _settings.ConvertCurrency, Nothing), _settings.Rates)
    End Sub

    ''' <summary>Selected result rows that are real parts (website-link rows can't go in a BOM), top to bottom.</summary>
    Private Function SelectedPartRows() As List(Of DataRowView)
        Return dgvResults.SelectedRows.Cast(Of DataGridViewRow)().OrderBy(Function(r) r.Index).
            Select(Function(r) RowView(r.Index)).
            Where(Function(v) v IsNot Nothing AndAlso Not CBool(v("LinkOnly"))).ToList()
    End Function

    Private Sub UpdateBomButtons()
        btnAddToBom.Enabled = SelectedPartRows().Count > 0
        btnBom.Text = $"B&OM ({If(_bomForm Is Nothing, 0, _bomForm.Bom.Lines.Count)})…"
        ' Wide enough for their text, never narrower than the other buttons.
        btnAddToBom.Width = Math.Max(btnSearch.Width, TextRenderer.MeasureText(btnAddToBom.Text, btnAddToBom.Font).Width + 20)
        btnBom.Width = Math.Max(btnSearch.Width, TextRenderer.MeasureText(btnBom.Text, btnBom.Font).Width + 20)
    End Sub

    Private Sub dgvResults_SelectionChanged(sender As Object, e As EventArgs) Handles dgvResults.SelectionChanged
        btnAddToBom.Enabled = SelectedPartRows().Count > 0
    End Sub

    Private Sub _bomForm_BomChanged(sender As Object, e As EventArgs) Handles _bomForm.BomChanged
        UpdateBomButtons()
    End Sub

    Private Sub AddToBom_Click(sender As Object, e As EventArgs) Handles btnAddToBom.Click, mnuAddToBom.Click
        Dim rows = SelectedPartRows()
        If rows.Count = 0 Then Return
        Dim lines = rows.Select(AddressOf ToBomLine).ToList()
        Dim added = _bomForm.AddLines(lines)
        Dim more = lines.Count - added
        Dim total = _bomForm.Bom.Lines.Count
        If lines.Count = 1 AndAlso more = 1 Then
            Dim l = _bomForm.Bom.Lines.First(Function(x) x.SamePart(lines(0)))
            SetStatus($"{lines(0).SupplierPartNumber} was already in the BOM - quantity now {l.Qty}.")
        Else
            SetStatus($"Added to the BOM: {added} new line{If(added = 1, "", "s")}" &
                      If(more > 0, $", {more} already there (quantity increased)", "") &
                      $". The BOM has {total} line{If(total = 1, "", "s")}.")
        End If
    End Sub

    ''' <summary>A BOM line from a result row, with the supplier's own price (not the converted one).
    ''' Quantity: the Advanced search quantity if one is set, but never less than the minimum order.</summary>
    Private Function ToBomLine(row As DataRowView) As BomLine
        Dim qty = If(_active IsNot Nothing AndAlso _active.Quantity > 0, _active.Quantity, 1)
        If row("MOQ") IsNot DBNull.Value Then qty = Math.Max(qty, CInt(row("MOQ")))
        Return New BomLine With {
            .Qty = qty,
            .Supplier = Field(row, "Supplier"),
            .SupplierPartNumber = Field(row, "SupplierPartNo"),
            .ManufacturerPartNumber = Field(row, "MfrPartNo"),
            .Manufacturer = Field(row, "Manufacturer"),
            .Description = Field(row, "Description"),
            .Footprint = Field(row, "Package"),
            .UnitPrice = If(row("OrigUnitPrice") Is DBNull.Value, Nothing, CType(CDec(row("OrigUnitPrice")), Decimal?)),
            .Currency = Field(row, "OrigCurrency"),
            .PriceBreaks = BomLine.ParseBreaks(Field(row, "BreaksRaw")),
            .ProductUrl = Field(row, "ProductUrl"),
            .Stock = If(row("Stock") Is DBNull.Value, Nothing, CType(CLng(row("Stock")), Long?)),
            .DateAdded = DateTime.Today}
    End Function

    ' The BOM window opens where it was last time (if that's still on a screen), else over the middle of this one.
    Private Sub btnBom_Click(sender As Object, e As EventArgs) Handles btnBom.Click
        ShowBomWindow()
    End Sub

    Private Sub ShowBomWindow()
        If _bomForm.Visible Then
            If _bomForm.WindowState = FormWindowState.Minimized Then _bomForm.WindowState = FormWindowState.Normal
            _bomForm.Activate()
            Return
        End If
        If Not _bomForm.IsHandleCreated Then
            _bomForm.StartPosition = FormStartPosition.Manual
            Dim p = _settings.BomWindow
            Dim bounds = If(p Is Nothing, Rectangle.Empty, New Rectangle(p.X, p.Y, p.Width, p.Height))
            Dim onScreen = p IsNot Nothing AndAlso p.Width >= _bomForm.MinimumSize.Width AndAlso p.Height >= _bomForm.MinimumSize.Height AndAlso
                           Screen.AllScreens.Any(Function(s)
                                                     Dim overlap = Rectangle.Intersect(s.WorkingArea, bounds)
                                                     Return overlap.Width >= 200 AndAlso overlap.Height >= 100
                                                 End Function)
            If onScreen Then
                _bomForm.Bounds = bounds
            Else
                _bomForm.Location = New Point(Left + (Width - _bomForm.Width) \ 2, Top + (Height - _bomForm.Height) \ 2)
            End If
        End If
        _bomForm.Show(Me)
    End Sub

#End Region

#Region "Settings / export"

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        Dim ratesBefore = _settings.Rates
        Using f As New SettingsForm(_settings)
            Dim result = f.ShowDialog(Me)
            ' "Update now" fetched new exchange rates: show them straight away, even if Settings was cancelled.
            If result <> DialogResult.OK AndAlso _settings.Rates IsNot ratesBefore Then ApplyConversion(showStatus:=False)
            If result = DialogResult.OK Then
                If f.ResetWindowRequested Then ResetWindowLayout()
                SaveUiSettings()
                Theme.Current = _settings.AppTheme
                RebuildSuppliers()
                ApplyThumbnailSetting()
                Theme.Apply(Me)
                If _bomForm.IsHandleCreated Then _bomForm.ApplyTheme()
                MatchFiltersToSupplierList()
                AlignToolbar() ' dark mode changes some control heights
                UpdateConvertText()
                ApplyConversion(showStatus:=False) ' the "Prices in" currency may have changed
                SetStatus("Settings saved.")
            End If
        End Using
    End Sub

    ' Help stays open (not modal) so it can be read while using the app; one window at a time.
    Private _help As HelpForm

    Private Sub btnHelp_Click(sender As Object, e As EventArgs) Handles btnHelp.Click
        If _help Is Nothing OrElse _help.IsDisposed Then
            _help = New HelpForm()
            _help.Show(Me)
        Else
            _help.Activate()
        End If
    End Sub

    Private Sub btnAbout_Click(sender As Object, e As EventArgs) Handles btnAbout.Click
        Using f As New AboutForm()
            f.ShowDialog(Me)
        End Using
    End Sub

    Private Function VisibleColumns() As List(Of DataGridViewColumn)
        Return dgvResults.Columns.Cast(Of DataGridViewColumn)().
            Where(Function(c) c.Visible AndAlso c.DataPropertyName.Length > 0).
            OrderBy(Function(c) c.DisplayIndex).ToList()
    End Function

    Private Sub btnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        If _table.DefaultView.Count = 0 Then
            MessageBox.Show(Me, "Nothing to export yet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim safeName = String.Concat(cboSearch.Text.Trim().Split(Path.GetInvalidFileNameChars()))
        Using dlg As New SaveFileDialog With {
            .Filter = "CSV file (*.csv)|*.csv",
            .FileName = $"WinPartsFinder {safeName} {DateTime.Now:yyyy-MM-dd HHmm}.csv"
        }
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
            ' The Datasheet column is the URL itself; it goes at the end as "Datasheet URL" instead.
            Dim cols = VisibleColumns().Where(Function(c) c.Name <> "Datasheet").ToList()
            Dim names = cols.Select(Function(c) c.DataPropertyName).Concat({"ProductUrl", "DatasheetUrl"}).ToList()
            Dim headers = cols.Select(Function(c) c.HeaderText).Concat({"Product URL", "Datasheet URL"}).ToList()
            If Converting Then ' keep the supplier's own prices too
                names.AddRange({"OrigUnitPrice", "OrigCurrency", "OrigBreaks"})
                headers.AddRange({"Supplier's price", "Supplier's currency", "Supplier's price breaks"})
            End If
            Dim sb As New StringBuilder()
            sb.AppendLine(String.Join(",", headers.Select(AddressOf CsvQuote)))
            For Each row As DataRowView In _table.DefaultView
                sb.AppendLine(String.Join(",", names.Select(Function(n) CsvQuote(Field(row, n)))))
            Next
            Try
                File.WriteAllText(dlg.FileName, sb.ToString(), New UTF8Encoding(True)) ' BOM so Excel shows £ properly
                SetStatus("Exported to " & dlg.FileName)
            Catch ex As Exception
                MessageBox.Show(Me, "Export failed: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Shared Function CsvQuote(s As String) As String
        If s.IndexOfAny({","c, """"c, ControlChars.Cr, ControlChars.Lf}) >= 0 Then
            Return """" & s.Replace("""", """""") & """"
        End If
        Return s
    End Function

#End Region

End Class
