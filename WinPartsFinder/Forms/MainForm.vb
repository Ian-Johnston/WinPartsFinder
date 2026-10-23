' WinPartsFinder by Ian Johnston
' VS 2022 VB app for Win10/11


Imports System.IO
Imports System.Reflection
Imports System.Text

' The .exe's "Product version" (Explorer > Properties > Details) comes from AppVersion below too.
<Assembly: AssemblyInformationalVersion(MainForm.AppVersion)>


Public Class MainForm

    ' ==== THE version number - change it here for each release ====
    Public Const AppVersion As String = "V1.004"
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
    Private colThumb As DataGridViewImageColumn
    Private colSupplierPartLink As DataGridViewLinkColumn
    Private colDatasheetLink As DataGridViewLinkColumn
    Private _photoPopup As ImagePopupForm

    Private Const ThumbRowHeight As Integer = ImageCache.ThumbSize + 4

    ' Designer layout, for Settings > Reset window layout.
    Private _defaultSize As Size
    Private _defaultSplitter As Integer
    Private _defaultColumns As Dictionary(Of String, ColumnLayout)

    ' Supplier list sub-item indexes
    Private Const SubMode As Integer = 1
    Private Const SubStatus As Integer = 2
    Private Const SubHits As Integer = 3
    Private Const SubTime As Integer = 4

#Region "Startup / shutdown"

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _settings = AppSettings.Load()
        Text = $"WinPartsFinder - {AppVersion}      {TitleSuffix}"
        Icon = Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath) ' WinPartsFinder.ico, via <ApplicationIcon>
        Theme.IsDark = _settings.DarkMode
        lvSuppliers.OwnerDraw = True ' only so the column headers can be drawn dark
        _defaultSize = Size
        _defaultSplitter = splitMain.SplitterDistance
        ' Can't be made smaller than the default (reset) size - unless the screen itself is smaller.
        Dim area = Screen.FromControl(Me).WorkingArea
        MinimumSize = New Size(Math.Min(_defaultSize.Width, area.Width), Math.Min(_defaultSize.Height, area.Height))
        RestoreWindowPlacement()

        Theme.SetCueBanner(txtFilter, "narrow results…")
        cboMode.Items.AddRange({"Keyword", "Manufacturer part no."})
        cboMode.SelectedIndex = 0
        nudMax.Value = Math.Max(nudMax.Minimum, Math.Min(nudMax.Maximum, _settings.MaxResults))
        chkInStock.Checked = _settings.InStockOnly

        ConfigureGridColumns()
        _defaultColumns = CurrentColumnLayout()
        ApplyColumnLayout(_settings.ResultColumns)
        ApplyThumbnailSetting()
        _table.DefaultView.Sort = "Order" ' arrival order until the user sorts by a column
        dgvResults.DataSource = _table.DefaultView
        _linkFont = New Font(dgvResults.Font, FontStyle.Italic)

        RebuildSuppliers()
        RefreshAutoComplete()
        ApplyFilter()
        Theme.Apply(Me)

        If Not _suppliers.Any(Function(s) s.IsConfigured) Then
            SetStatus("No API keys set yet - open Settings to add Mouser / DigiKey / element14 keys. Until then you get website links.")
        Else
            SetStatus("Ready.")
        End If
        txtSearch.Select()
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        _cts?.Cancel()
        SaveUiSettings()
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
    End Sub

    Private Function CurrentWindowPlacement() As WindowPlacement
        ' RestoreBounds holds the normal size while maximised/minimised.
        Dim b = If(WindowState = FormWindowState.Normal, Bounds, RestoreBounds)
        Return New WindowPlacement With {
            .X = b.X, .Y = b.Y, .Width = b.Width, .Height = b.Height,
            .Maximized = WindowState = FormWindowState.Maximized,
            .SplitterDistance = splitMain.SplitterDistance
        }
    End Function

    Private Sub ResetWindowLayout()
        WindowState = FormWindowState.Normal
        Size = _defaultSize
        Dim area = Screen.FromControl(Me).WorkingArea
        Location = New Point(area.Left + (area.Width - Width) \ 2, area.Top + (area.Height - Height) \ 2)
        splitMain.SplitterDistance = _defaultSplitter
        ApplyColumnLayout(_defaultColumns)
    End Sub

    Private Sub SaveUiSettings()
        _settings.MainWindow = CurrentWindowPlacement()
        _settings.ResultColumns = CurrentColumnLayout()
        _settings.MaxResults = CInt(nudMax.Value)
        _settings.InStockOnly = chkInStock.Checked
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

    Private Sub RefreshAutoComplete()
        Dim src As New AutoCompleteStringCollection()
        src.AddRange(_settings.RecentSearches.ToArray())
        txtSearch.AutoCompleteCustomSource = src
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
            dgvResults.Columns(kv.Key).ToolTipText = kv.Value & vbCrLf & "Click to sort."
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
        For Each r As DataGridViewRow In dgvResults.Rows
            r.Height = h
        Next
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
            DBNull.Value)                  ' No - given when the search / Load more finishes

        Dim key = MpnKey(p.ManufacturerPartNumber)
        If p.ImageUrl.Length > 0 AndAlso key.Length > 0 AndAlso Not _imagesByMpn.ContainsKey(key) Then
            _imagesByMpn(key) = (p.ImageUrl, If(p.LargeImageUrl.Length > 0, p.LargeImageUrl, p.ImageUrl))
        End If
        Return row
    End Function

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
        ' Types unticked in the Types… window (blank type = "(no type)"). Website-link rows always stay.
        If _hiddenTypes.Count > 0 Then
            parts.Add("(LinkOnly = true OR ISNULL(Type, '') NOT IN (" &
                      String.Join(", ", _hiddenTypes.Select(Function(t) "'" & t.Replace("'", "''") & "'")) & "))")
        End If
        Dim f = txtFilter.Text.Trim()
        If f.Length > 0 Then
            Dim pat = EscapeLike(f)
            parts.Add($"(LinkOnly = true OR MfrPartNo LIKE '%{pat}%' OR Description LIKE '%{pat}%' OR Type LIKE '%{pat}%' " &
                      $"OR Manufacturer LIKE '%{pat}%' OR Supplier LIKE '%{pat}%' OR SupplierPartNo LIKE '%{pat}%')")
        End If
        _table.DefaultView.RowFilter = String.Join(" AND ", parts)
        UpdateCountStatus()
    End Sub

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
        UpdateTypesButton()
        UpdateTotalParts()
        Dim shown = _table.DefaultView.Count
        Dim total = _table.Rows.Count
        If total = 0 Then Return
        Dim removed = RemovedCount()
        SetStatus($"{shown} of {total} results shown." &
                  If(removed > 0, $" ({removed} removed - right-click > Restore removed rows to bring them back)", ""))
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
            Select(Function(r) TryCast(r.DataBoundItem, DataRowView)).
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

    Private Function CurrentRow() As DataRowView
        If dgvResults.CurrentRow Is Nothing Then Return Nothing
        Return TryCast(dgvResults.CurrentRow.DataBoundItem, DataRowView)
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
            .Query = txtSearch.Text.Trim(),
            .Mode = If(cboMode.SelectedIndex = 1, SearchMode.PartNumber, SearchMode.Keyword),
            .MaxResults = CInt(nudMax.Value)})
    End Sub

    Private Async Sub btnAdvanced_Click(sender As Object, e As EventArgs) Handles btnAdvanced.Click
        If _busy Then Return
        ' Open with last time's choices, but today's search text, batch size and ticked suppliers.
        Dim start = _settings.LastAdvanced
        ' (Unless Search for was left blank last time and the search box still shows those words.)
        Dim typed = txtSearch.Text.Trim()
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
        txtSearch.Text = o.Query
        nudMax.Value = Math.Max(nudMax.Minimum, Math.Min(nudMax.Maximum, o.MaxResults))

        ' Make the supplier list match the window (without the "ticked after a search" auto-search).
        _loading = True
        For Each item As ListViewItem In lvSuppliers.Items
            item.Checked = o.Suppliers.Contains(item.Text)
            _settings.EnabledSuppliers(item.Text) = item.Checked
        Next
        _loading = False

        Await RunSearchAsync(o)
    End Sub

    ' Ctrl+F opens Advanced search.
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If keyData = (Keys.Control Or Keys.F) Then
            btnAdvanced.PerformClick()
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ''' <summary>Runs a search (normal or Advanced) across the ticked suppliers.</summary>
    Private Async Function RunSearchAsync(opts As AdvancedSearchOptions) As Task
        Dim query = opts.Query
        If query.Length = 0 Then
            txtSearch.Select()
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
        Dim advanced = opts.Describe()

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
                  If(advanced.Length > 0, $"Advanced: {advanced}" & If(_advancedHidden > 0, $" ({_advancedHidden} left out)", "") & ". ", "") &
                  If(more.Count > 0, $"More available from {String.Join(", ", more)} - click Load more. ", "") &
                  "Double-click a row to open it in your browser.")
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

    ''' <summary>The Price column shows the price at the Advanced search quantity, when one is set.</summary>
    Private Sub UpdatePriceHeader()
        Dim col = dgvResults.Columns("UnitPrice")
        Dim qty = If(_active Is Nothing, 0, _active.Quantity)
        col.HeaderText = If(qty > 0, $"Price ({qty:#,##0})", "Price (1)")
        col.ToolTipText = If(qty > 0, $"Price each when buying {qty:#,##0} (from the price breaks).",
                             "Price each at the lowest price break (usually qty 1).") & vbCrLf & "Click to sort."
    End Sub

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
                    If Not _active.Matches(p) Then
                        st.Filtered += 1
                        _advancedHidden += 1
                        Continue For
                    End If
                End If
                st.RowCount += 1
                added.Add(AddResult(p, SeqFor(s, st.RowCount)))
            Next
            _table.EndLoadData()
            ' Earlier rows (e.g. Mouser) may now be able to borrow a photo from these.
            If colThumb.Visible Then dgvResults.InvalidateColumn(colThumb.Index)
            item.SubItems(SubStatus).Text = If(st.RowCount = 0, "No matches", "OK")
            item.ToolTipText = $"{s.Name}: {st.RowCount} result(s)" &
                               If(st.Total.HasValue, $" of {st.Total} found.", ".") &
                               If(st.Duplicates > 0, $" {st.Duplicates} repeated by {s.Name} and skipped.", "") &
                               If(st.Filtered > 0, $" {st.Filtered} left out by the Advanced search filters.", "") &
                               If(HasMore(s), " Right-click > Load more for the next batch.", "")
            If Not firstPage AndAlso st.RowCount = before Then item.SubItems(SubStatus).Text = "No more"
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
            Dim v = TryCast(r.DataBoundItem, DataRowView)
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
        btnSearch.Enabled = Not busy
        btnAdvanced.Enabled = Not busy
        btnCancel.Enabled = busy
        btnSettings.Enabled = Not busy
        UseWaitCursor = busy
        UpdateLoadMoreButton()
        UpdateTypesButton()
    End Sub

    Private Sub SetStatus(text As String)
        lblStatus.Text = text
    End Sub

#End Region

#Region "Grid events"

    Private Sub chkInStock_CheckedChanged(sender As Object, e As EventArgs) Handles chkInStock.CheckedChanged
        ApplyFilter()
    End Sub

    Private Sub txtFilter_TextChanged(sender As Object, e As EventArgs) Handles txtFilter.TextChanged
        ApplyFilter()
    End Sub

    Private Sub dgvResults_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles dgvResults.CellFormatting
        If e.RowIndex < 0 OrElse e.CellStyle Is Nothing Then Return
        Dim row = TryCast(dgvResults.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If row Is Nothing Then Return

        If e.ColumnIndex = colThumb.Index Then
            e.Value = _images.GetThumbnail(ImageUrlsFor(row).Thumb)
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
    End Sub

    Private Sub dgvResults_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvResults.ColumnHeaderMouseClick
        Dim col = dgvResults.Columns(e.ColumnIndex)
        If col.Name <> "Supplier" OrElse e.Button <> MouseButtons.Left Then Return
        Dim descending = col.HeaderCell.SortGlyphDirection = SortOrder.Ascending
        _table.DefaultView.Sort = If(descending, "Seq DESC", "Seq")
        col.HeaderCell.SortGlyphDirection = If(descending, SortOrder.Descending, SortOrder.Ascending)
    End Sub

    Private Sub dgvResults_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellDoubleClick
        ' The photo and link cells already act on a single click.
        If e.RowIndex < 0 OrElse e.ColumnIndex = colThumb.Index OrElse e.ColumnIndex = colSupplierPartLink.Index OrElse
           e.ColumnIndex = colDatasheetLink.Index Then Return
        OpenUrl(Field(CurrentRow(), "ProductUrl"))
    End Sub

    Private Sub dgvResults_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellContentClick
        If e.RowIndex < 0 Then Return
        Dim row = TryCast(dgvResults.Rows(e.RowIndex).DataBoundItem, DataRowView)
        If e.ColumnIndex = colSupplierPartLink.Index Then
            OpenUrl(Field(row, "ProductUrl"))
        ElseIf e.ColumnIndex = colDatasheetLink.Index Then
            Dim url = Field(row, "DatasheetUrl")
            If url.Length > 0 Then OpenUrl(url)
        End If
    End Sub

    Private Sub dgvResults_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvResults.CellClick
        If e.RowIndex < 0 OrElse e.ColumnIndex <> colThumb.Index Then Return
        Dim row = TryCast(dgvResults.Rows(e.RowIndex).DataBoundItem, DataRowView)
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
        Dim row = CurrentRow()
        Dim removed = RemovedCount()
        ' With every row removed there's nothing to click on, but Restore must still be reachable.
        If row Is Nothing AndAlso removed = 0 Then
            e.Cancel = True
            Return
        End If
        Dim selectedCount = dgvResults.SelectedRows.Count
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
            Select(Function(r) TryCast(r.DataBoundItem, DataRowView)).Where(Function(v) v IsNot Nothing).
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
        Dim hasQuery = txtSearch.Text.Trim().Length > 0
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
        OpenUrl(DirectCast(lvSuppliers.SelectedItems(0).Tag, Supplier).GetSearchUrl(txtSearch.Text.Trim()))
    End Sub

    Private Sub mnuOpenAllSearches_Click(sender As Object, e As EventArgs) Handles mnuOpenAllSearches.Click
        For Each item As ListViewItem In lvSuppliers.Items
            If item.Checked Then OpenUrl(DirectCast(item.Tag, Supplier).GetSearchUrl(txtSearch.Text.Trim()))
        Next
    End Sub

    Private Sub lvSuppliers_DoubleClick(sender As Object, e As EventArgs) Handles lvSuppliers.DoubleClick
        mnuOpenSupplierSearch.PerformClick()
    End Sub

    ' Owner drawing is only used for the header (so it can go dark); rows draw normally.
    Private Sub lvSuppliers_DrawColumnHeader(sender As Object, e As DrawListViewColumnHeaderEventArgs) Handles lvSuppliers.DrawColumnHeader
        If Not Theme.IsDark Then
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

    ' Stretch the last column to the edge, so no light-coloured header strip is left over.
    Private Sub lvSuppliers_Resize(sender As Object, e As EventArgs) Handles lvSuppliers.Resize
        If lvSuppliers.Columns.Count = 0 Then Return
        Dim others = lvSuppliers.Columns.Cast(Of ColumnHeader)().Take(lvSuppliers.Columns.Count - 1).Sum(Function(c) c.Width)
        colTime.Width = Math.Max(50, lvSuppliers.ClientSize.Width - others)
    End Sub

#End Region

#Region "Settings / export"

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        Using f As New SettingsForm(_settings)
            If f.ShowDialog(Me) = DialogResult.OK Then
                If f.ResetWindowRequested Then ResetWindowLayout()
                SaveUiSettings()
                Theme.IsDark = _settings.DarkMode
                RebuildSuppliers()
                ApplyThumbnailSetting()
                Theme.Apply(Me)
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
        Dim safeName = String.Concat(txtSearch.Text.Trim().Split(Path.GetInvalidFileNameChars()))
        Using dlg As New SaveFileDialog With {
            .Filter = "CSV file (*.csv)|*.csv",
            .FileName = $"WinPartsFinder {safeName} {DateTime.Now:yyyy-MM-dd HHmm}.csv"
        }
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
            ' The Datasheet column is the URL itself; it goes at the end as "Datasheet URL" instead.
            Dim cols = VisibleColumns().Where(Function(c) c.Name <> "Datasheet").ToList()
            Dim names = cols.Select(Function(c) c.DataPropertyName).Concat({"ProductUrl", "DatasheetUrl"}).ToList()
            Dim headers = cols.Select(Function(c) c.HeaderText).Concat({"Product URL", "Datasheet URL"})
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
