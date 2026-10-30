Imports System.Globalization
Imports System.IO
Imports System.Text

''' <summary>
''' The BOM pop-up: parts collected across searches (right-click > Add to BOM, or the toolbar button), with
''' quantities, totals and a link to each product page for buying. Stays open beside the main window; closing
''' it only hides it, so the list is kept. The main window saves it between runs.
''' </summary>
Public Class BomForm

    Private _bom As New Bom()
    Private _filePath As String = ""
    Private _dirty As Boolean
    Private _settingText As Boolean          ' True while the name / notes boxes are filled from code
    ' Each Remove (one or several lines) can be put back with Ctrl+Z, most recent first.
    Private ReadOnly _undo As New Stack(Of List(Of (Index As Integer, Line As BomLine)))
    ' "Prices in" currency from the main window; Nothing = show each supplier's own currency.
    Private _convertTo As String
    Private _rates As ExchangeRates
    Private _hoverDeleteRow As Integer = -1
    Private _boldFont As Font

    ''' <summary>Raised whenever the lines, name or notes change, or another BOM is loaded.</summary>
    Public Event BomChanged As EventHandler

    Public ReadOnly Property Bom As Bom
        Get
            Return _bom
        End Get
    End Property

    ''' <summary>The file this BOM was last saved to or loaded from; "" = never saved.</summary>
    Public ReadOnly Property FilePath As String
        Get
            Return _filePath
        End Get
    End Property

    ''' <summary>Folder the last BOM was saved to or loaded from - where Load, Save As and Open Folder start.</summary>
    Public Property LastFolder As String = ""

    ''' <summary>True when there's something in the BOM that hasn't been saved (an empty, unnamed BOM doesn't count).</summary>
    Public ReadOnly Property HasUnsavedChanges As Boolean
        Get
            CommitEdit()
            Return _dirty AndAlso Not (_bom.Lines.Count = 0 AndAlso _bom.Name.Trim().Length = 0 AndAlso _bom.Notes.Trim().Length = 0)
        End Get
    End Property

    Private Sub BomForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Wide enough for its text on one line, never narrower than the other buttons.
        For Each b In {btnOpenFolder, btnExportTxt}
            b.Width = Math.Max(btnSaveAs.Width, TextRenderer.MeasureText(b.Text, b.Font).Width + 20)
        Next
        ApplyTheme()
    End Sub

    ''' <summary>Puts a BOM in the window, e.g. the one restored at startup.</summary>
    Public Sub SetBom(bom As Bom, filePath As String, dirty As Boolean)
        _bom = bom
        _filePath = If(filePath, "")
        _dirty = dirty
        _undo.Clear()
        _settingText = True
        txtName.Text = bom.Name
        txtNotes.Text = bom.Notes
        _settingText = False
        RefreshGrid()
        RaiseEvent BomChanged(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Adds parts (or adds to the quantity of ones already in the BOM). Returns how many lines are new.</summary>
    Public Function AddLines(lines As IEnumerable(Of BomLine)) As Integer
        Dim before = _bom.Lines.Count
        Dim touched As New List(Of BomLine)
        For Each l In lines
            touched.Add(_bom.Add(l))
        Next
        If touched.Count = 0 Then Return 0
        RefreshGrid(touched)
        MarkDirty()
        Return _bom.Lines.Count - before
    End Function

    ''' <summary>Shows prices in <paramref name="convertTo"/> (the main window's "Prices in"), or Nothing for the suppliers' own.</summary>
    Public Sub SetCurrency(convertTo As String, rates As ExchangeRates)
        _convertTo = convertTo
        _rates = rates
        RefreshGrid()
    End Sub

    ''' <summary>Colours for the current theme - called when the window opens and after the theme changes.</summary>
    Public Sub ApplyTheme()
        Theme.Apply(Me)
        _boldFont?.Dispose()
        _boldFont = New Font(Font, FontStyle.Bold)
        lblGrandTotal.Font = _boldFont
        RefreshGrid()
    End Sub

    ''' <summary>Finishes typing in a Qty cell, so the value isn't lost when the app closes.</summary>
    Public Sub CommitEdit()
        If dgvBom.IsCurrentCellInEditMode Then dgvBom.EndEdit()
    End Sub

    Private Sub BomForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' The X only hides the window - the BOM stays (and is saved when the app closes).
        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            CommitEdit()
            Hide()
        End If
    End Sub

#Region "Grid"

    Private Function LineAt(rowIndex As Integer) As BomLine
        If rowIndex < 0 OrElse rowIndex >= dgvBom.Rows.Count Then Return Nothing
        Return TryCast(dgvBom.Rows(rowIndex).Tag, BomLine)
    End Function

    Private Function SelectedLines() As List(Of BomLine)
        Return dgvBom.SelectedRows.Cast(Of DataGridViewRow)().OrderBy(Function(r) r.Index).
            Select(Function(r) TryCast(r.Tag, BomLine)).Where(Function(l) l IsNot Nothing).ToList()
    End Function

    ''' <summary>Rebuilds the rows from the BOM, keeping the selection (or selecting <paramref name="select"/>).</summary>
    Private Sub RefreshGrid(Optional [select] As IEnumerable(Of BomLine) = Nothing)
        If dgvBom.IsCurrentCellInEditMode Then Return ' CellEndEdit refreshes afterwards
        Dim keep = If([select]?.ToList(), SelectedLines())
        Dim firstShown = dgvBom.FirstDisplayedScrollingRowIndex
        dgvBom.Rows.Clear()
        For Each l In _bom.Lines
            Dim i = dgvBom.Rows.Add()
            Dim row = dgvBom.Rows(i)
            row.Tag = l
            FillRow(row, l)
        Next
        ' (Setting the current cell selects just that row, so the others are selected after it.)
        Dim keepRows = dgvBom.Rows.Cast(Of DataGridViewRow)().Where(Function(r) keep.Contains(DirectCast(r.Tag, BomLine))).ToList()
        If keepRows.Count > 0 Then
            dgvBom.CurrentCell = keepRows(0).Cells(colSupplier.Index)
            keepRows.ForEach(Sub(r) r.Selected = True)
            If [select] IsNot Nothing Then firstShown = keepRows(0).Index
        Else
            dgvBom.ClearSelection()
        End If
        If firstShown >= 0 AndAlso firstShown < dgvBom.Rows.Count Then dgvBom.FirstDisplayedScrollingRowIndex = firstShown
        UpdateTotals()
        UpdateButtons()
    End Sub

    Private Sub FillRow(row As DataGridViewRow, l As BomLine)
        Dim price = ShownPrice(l)
        row.Cells(colInclude.Index).Value = l.Include
        row.Cells(colSupplier.Index).Value = l.Supplier
        row.Cells(colPartNo.Index).Value = l.SupplierPartNumber
        row.Cells(colMpn.Index).Value = l.ManufacturerPartNumber
        row.Cells(colDescription.Index).Value = DescriptionText(l)
        row.Cells(colQty.Index).Value = l.Qty.ToString(CultureInfo.CurrentCulture)
        row.Cells(colUnitPrice.Index).Value = If(price.Unit.HasValue, Money(price.Unit.Value, price.Currency, "0.00##"), "")
        row.Cells(colTotal.Index).Value = If(price.Unit.HasValue, Money(price.Unit.Value * l.Qty, price.Currency, "0.00"), "")
        row.Cells(colLink.Index).Value = If(l.ProductUrl.Length > 0, "Open", "")
        row.Cells(colDelete.Index).Value = "✕"
        SetToolTips(row, l, price.Converted)

        ' Unticked lines are greyed out; converted prices are purple, as in the results.
        Dim fore = If(l.Include, Theme.TextColor, Theme.MutedText)
        row.DefaultCellStyle.ForeColor = fore
        row.DefaultCellStyle.SelectionForeColor = If(l.Include, Color.Empty, Theme.MutedText)
        Dim priceFore = If(l.Include AndAlso price.Converted, Theme.ConvertedText, Color.Empty)
        row.Cells(colUnitPrice.Index).Style.ForeColor = priceFore
        row.Cells(colUnitPrice.Index).Style.SelectionForeColor = priceFore
        row.Cells(colTotal.Index).Style.ForeColor = priceFore
        row.Cells(colTotal.Index).Style.SelectionForeColor = priceFore
    End Sub

    ''' <summary>The description, or for a line loaded without one (no supplier yet) its refs, value and footprint.</summary>
    Private Shared Function DescriptionText(l As BomLine) As String
        If l.Description.Length > 0 Then Return l.Description
        Return String.Join("  ", {l.Refs, l.ManufacturerPartNumber, l.Footprint}.Where(Function(s) s.Length > 0))
    End Function

    ''' <summary>The price each at the line's quantity, converted to the "Prices in" currency when that's on.</summary>
    Private Function ShownPrice(l As BomLine) As (Unit As Decimal?, Currency As String, Converted As Boolean)
        Dim unit = l.PriceEach()
        If Not unit.HasValue Then Return (Nothing, l.Currency, False)
        If _convertTo IsNot Nothing AndAlso _rates IsNot Nothing AndAlso
           Not String.Equals(l.Currency, _convertTo, StringComparison.OrdinalIgnoreCase) Then
            Dim rate = _rates.Rate(l.Currency, _convertTo)
            If rate.HasValue Then Return (Math.Round(unit.Value * rate.Value, 4), _convertTo, True)
        End If
        Return (unit, l.Currency, False)
    End Function

    ''' <summary>e.g. "£1.25", "€0.139", "CHF 3.10".</summary>
    Private Shared Function Money(value As Decimal, currency As String, format As String) As String
        Dim number = value.ToString(format, CultureInfo.CurrentCulture)
        Select Case If(currency, "").ToUpperInvariant()
            Case "GBP" : Return "£" & number
            Case "EUR" : Return "€" & number
            Case "USD" : Return "$" & number
            Case "" : Return number
            Case Else : Return currency.ToUpperInvariant() & " " & number
        End Select
    End Function

    ''' <summary>Several amounts in different currencies, e.g. "£61.20 + €11.40", in the order the currencies first appear in the list.</summary>
    Private Shared Function MoneyList(amounts As Dictionary(Of String, Decimal), order As List(Of String)) As String
        Return String.Join(" + ", amounts.OrderBy(Function(a) order.IndexOf(a.Key)).Select(Function(a) Money(a.Value, a.Key, "0.00")))
    End Function

    Private Sub UpdateTotals()
        Dim bySupplier As New List(Of (Supplier As String, Amounts As Dictionary(Of String, Decimal)))
        Dim grand As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
        Dim currencies As New List(Of String)
        Dim noPrice = 0
        Dim anyConverted = False
        For Each l In _bom.Lines.Where(Function(x) x.Include)
            Dim price = ShownPrice(l)
            If Not price.Unit.HasValue Then
                noPrice += 1
                Continue For
            End If
            anyConverted = anyConverted OrElse price.Converted
            Dim amount = price.Unit.Value * l.Qty
            Dim name = If(l.Supplier.Length > 0, l.Supplier, "(no supplier)")
            Dim entry = bySupplier.FirstOrDefault(Function(s) String.Equals(s.Supplier, name, StringComparison.OrdinalIgnoreCase))
            If entry.Amounts Is Nothing Then
                entry = (name, New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase))
                bySupplier.Add(entry)
            End If
            Dim cur = price.Currency.ToUpperInvariant()
            If Not currencies.Contains(cur) Then currencies.Add(cur)
            entry.Amounts(cur) = If(entry.Amounts.ContainsKey(cur), entry.Amounts(cur), 0D) + amount
            grand(cur) = If(grand.ContainsKey(cur), grand(cur), 0D) + amount
        Next

        If _bom.Lines.Count = 0 Then
            lblSubtotals.Text = "No parts yet - right-click a result and choose Add to BOM."
        Else
            Dim bits = bySupplier.Select(Function(s) $"{s.Supplier} {MoneyList(s.Amounts, currencies)}").ToList()
            If noPrice > 0 Then bits.Add($"{noPrice} without a price")
            lblSubtotals.Text = String.Join("  ·  ", bits)
        End If
        lblGrandTotal.Text = "Grand total: " & If(grand.Count = 0, Money(0D, If(_convertTo, ""), "0.00"), MoneyList(grand, currencies))
        tipBom.SetToolTip(lblSubtotals, lblSubtotals.Text.Replace("  ·  ", vbCrLf))
        tipBom.SetToolTip(lblGrandTotal, If(anyConverted,
            $"Converted prices use the {_rates.Describe()} - approximate: the supplier's own rate, VAT and import charges will differ.",
            "Ticked lines only."))
        Text = $"BOM – {_bom.DisplayName}{If(_dirty, " *", "")}"
    End Sub

    Private Sub UpdateButtons()
        Dim any = dgvBom.SelectedRows.Count > 0
        btnRemove.Enabled = any
        btnQtyDown.Enabled = any
        btnQtyUp.Enabled = any
    End Sub

    Private Sub dgvBom_SelectionChanged(sender As Object, e As EventArgs) Handles dgvBom.SelectionChanged
        UpdateButtons()
    End Sub

    Private Sub MarkDirty()
        _dirty = True
        UpdateTotals()
        RaiseEvent BomChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub dgvBom_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellContentClick
        Dim l = LineAt(e.RowIndex)
        If l Is Nothing Then Return
        If e.ColumnIndex = colInclude.Index Then
            l.Include = Not l.Include
            FillRow(dgvBom.Rows(e.RowIndex), l)
            MarkDirty()
        ElseIf e.ColumnIndex = colLink.Index Then
            OpenUrl(l.ProductUrl)
        End If
    End Sub

    Private Sub dgvBom_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellClick
        If e.ColumnIndex = colDelete.Index Then
            Dim l = LineAt(e.RowIndex)
            If l IsNot Nothing Then RemoveLines({l})
        End If
    End Sub

    ' Double-click a line to open its product page (not on the tick box, Qty, link or ✕, which do their own thing).
    Private Sub dgvBom_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellDoubleClick
        If e.ColumnIndex = colInclude.Index OrElse e.ColumnIndex = colQty.Index OrElse
           e.ColumnIndex = colLink.Index OrElse e.ColumnIndex = colDelete.Index Then Return
        Dim l = LineAt(e.RowIndex)
        If l IsNot Nothing Then OpenUrl(l.ProductUrl)
    End Sub

    ' Qty: whole numbers only. 0 asks whether to remove the line; anything else that isn't a number puts the old value back.
    Private Sub dgvBom_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellEndEdit
        If e.ColumnIndex <> colQty.Index Then Return
        Dim l = LineAt(e.RowIndex)
        If l Is Nothing Then Return
        Dim typed = Convert.ToString(dgvBom.Rows(e.RowIndex).Cells(e.ColumnIndex).Value).Trim()
        Dim q As Integer
        Dim valid = Integer.TryParse(typed, NumberStyles.Integer, CultureInfo.CurrentCulture, q) AndAlso q >= 0
        ' (The grid is still finishing the edit - change its rows afterwards.)
        BeginInvoke(New Action(
            Sub()
                If valid AndAlso q = 0 Then
                    AskRemove({l})
                ElseIf valid AndAlso q <> l.Qty Then
                    l.Qty = q
                    RefreshGrid()
                    MarkDirty()
                Else
                    RefreshGrid()
                End If
            End Sub))
    End Sub

    ' Each cell's tooltip is set as its row is filled in: the grid's CellToolTipTextNeeded event only fires for a
    ' grid with a DataSource (or in virtual mode), and this one has neither.
    Private Sub SetToolTips(row As DataGridViewRow, l As BomLine, converted As Boolean)
        row.Cells(colInclude.Index).ToolTipText = If(l.Include,
            "Ticked: counted in the totals. Untick to leave this line out.",
            "Unticked: not counted in the totals. Tick to count this line.")
        row.Cells(colDelete.Index).ToolTipText = "Remove this line (Ctrl+Z puts it back)"
        row.Cells(colLink.Index).ToolTipText = If(l.ProductUrl.Length = 0, "",
            If(l.Supplier.Length > 0, $"Open this part's page on {l.Supplier}'s website", "Open this part's page"))
        row.Cells(colQty.Index).ToolTipText = "Click and type a new quantity, or use Qty − / Qty +"

        Dim prices As New List(Of String)
        Dim unit = l.PriceEach()
        If unit.HasValue Then prices.Add($"Supplier's price: {Money(unit.Value, l.Currency, "0.00##")} each at qty {l.Qty}")
        If l.PriceBreaks.Count > 1 Then
            prices.Add("Price breaks: " & String.Join("   ", l.PriceBreaks.OrderBy(Function(b) b.Quantity).
                                                       Select(Function(b) $"{b.Quantity}+ {b.Price:0.00##}")))
        End If
        If converted Then prices.Add($"Converted to {_convertTo} at the {_rates.Describe()} (approximate)")
        row.Cells(colUnitPrice.Index).ToolTipText = String.Join(vbCrLf, prices)
        row.Cells(colTotal.Index).ToolTipText = String.Join(vbCrLf, prices)

        Dim info As New List(Of String)
        If l.Description.Length > 0 Then info.Add(l.Description)
        If l.ManufacturerPartNumber.Length > 0 Then info.Add("Manufacturer part no.: " & l.ManufacturerPartNumber)
        If l.Manufacturer.Length > 0 Then info.Add("Manufacturer: " & l.Manufacturer)
        If l.Footprint.Length > 0 Then info.Add("Package: " & l.Footprint)
        If l.Refs.Length > 0 Then info.Add("Refs: " & l.Refs)
        If l.Stock.HasValue Then info.Add($"Stock when added: {l.Stock.Value:N0}")
        If l.DateAdded.HasValue Then info.Add($"Added: {l.DateAdded.Value:d MMM yyyy}")
        For Each c In {colSupplier, colPartNo, colMpn, colDescription}
            row.Cells(c.Index).ToolTipText = String.Join(vbCrLf, info)
        Next
    End Sub

    ' The ✕ is dim red until the mouse is over it, then bright red with a hand cursor.
    Private Sub dgvBom_CellMouseEnter(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellMouseEnter
        If e.ColumnIndex = colDelete.Index AndAlso e.RowIndex >= 0 Then
            _hoverDeleteRow = e.RowIndex
            dgvBom.Cursor = Cursors.Hand
            dgvBom.InvalidateCell(e.ColumnIndex, e.RowIndex)
        End If
    End Sub

    Private Sub dgvBom_CellMouseLeave(sender As Object, e As DataGridViewCellEventArgs) Handles dgvBom.CellMouseLeave
        If e.ColumnIndex = colDelete.Index AndAlso _hoverDeleteRow >= 0 Then
            Dim was = _hoverDeleteRow
            _hoverDeleteRow = -1
            dgvBom.Cursor = Cursors.Default
            If was < dgvBom.Rows.Count Then dgvBom.InvalidateCell(colDelete.Index, was)
        End If
    End Sub

    Private Sub dgvBom_CellPainting(sender As Object, e As DataGridViewCellPaintingEventArgs) Handles dgvBom.CellPainting
        If e.ColumnIndex <> colDelete.Index OrElse e.RowIndex < 0 Then Return
        e.PaintBackground(e.ClipBounds, (e.State And DataGridViewElementStates.Selected) <> 0)
        Dim hot = e.RowIndex = _hoverDeleteRow
        Dim red = If(hot, Color.FromArgb(235, 50, 50),
                     If(Theme.IsDark, Color.FromArgb(150, 80, 80), Color.FromArgb(210, 120, 120)))
        TextRenderer.DrawText(e.Graphics, "✕", If(hot, _boldFont, e.CellStyle.Font), e.CellBounds, red,
                              TextFormatFlags.HorizontalCenter Or TextFormatFlags.VerticalCenter)
        e.Handled = True
    End Sub

    ' Right-click selects the line under the mouse first (unless it's already part of the selection).
    Private Sub dgvBom_CellMouseDown(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvBom.CellMouseDown
        If e.Button = MouseButtons.Right AndAlso e.RowIndex >= 0 AndAlso Not dgvBom.Rows(e.RowIndex).Selected Then
            dgvBom.CurrentCell = dgvBom.Rows(e.RowIndex).Cells(Math.Max(0, e.ColumnIndex))
        End If
    End Sub

    Private Sub BomForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If dgvBom.IsCurrentCellInEditMode OrElse ActiveControl Is txtName OrElse ActiveControl Is txtNotes Then Return
        If e.KeyCode = Keys.Z AndAlso e.Modifiers = Keys.Control Then
            UndoRemove()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Delete AndAlso e.Modifiers = Keys.None AndAlso ActiveControl Is dgvBom Then
            RemoveLines(SelectedLines())
            e.Handled = True
        ElseIf e.KeyCode = Keys.Space AndAlso e.Modifiers = Keys.None AndAlso ActiveControl Is dgvBom Then
            ' Space ticks / unticks the selected lines.
            Dim lines = SelectedLines()
            If lines.Count > 0 Then
                Dim include = Not lines.All(Function(l) l.Include)
                lines.ForEach(Sub(l) l.Include = include)
                RefreshGrid()
                MarkDirty()
            End If
            e.Handled = True
        End If
    End Sub

#End Region

#Region "Removing lines and quantities"

    Private Sub RemoveLines(lines As IEnumerable(Of BomLine))
        Dim removed = lines.Select(Function(l) (Index:=_bom.Lines.IndexOf(l), Line:=l)).Where(Function(x) x.Index >= 0).
                            OrderBy(Function(x) x.Index).ToList()
        If removed.Count = 0 Then Return
        For Each x In removed
            _bom.Lines.Remove(x.Line)
        Next
        _undo.Push(removed)
        _hoverDeleteRow = -1
        dgvBom.Cursor = Cursors.Default
        RefreshGrid()
        MarkDirty()
    End Sub

    Private Sub AskRemove(lines As IList(Of BomLine))
        Dim what = If(lines.Count = 1, If(lines(0).SupplierPartNumber.Length > 0, lines(0).SupplierPartNumber, "this line"), $"these {lines.Count} lines")
        If MessageBox.Show(Me, $"Remove {what} from the BOM?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            RemoveLines(lines)
        Else
            RefreshGrid()
        End If
    End Sub

    Private Sub UndoRemove()
        If _undo.Count = 0 Then Return
        Dim removed = _undo.Pop()
        For Each x In removed ' lowest index first, so each goes back where it was
            _bom.Lines.Insert(Math.Min(x.Index, _bom.Lines.Count), x.Line)
        Next
        RefreshGrid(removed.Select(Function(x) x.Line))
        MarkDirty()
    End Sub

    Private Sub btnRemove_Click(sender As Object, e As EventArgs) Handles btnRemove.Click, mnuRemove.Click
        RemoveLines(SelectedLines())
    End Sub

    Private Sub btnQtyUp_Click(sender As Object, e As EventArgs) Handles btnQtyUp.Click
        Dim lines = SelectedLines()
        If lines.Count = 0 Then Return
        lines.ForEach(Sub(l) l.Qty += 1)
        RefreshGrid()
        MarkDirty()
    End Sub

    ' Lines already at 1 would go to 0: those are offered for removal instead.
    Private Sub btnQtyDown_Click(sender As Object, e As EventArgs) Handles btnQtyDown.Click
        Dim lines = SelectedLines()
        If lines.Count = 0 Then Return
        Dim atOne = lines.Where(Function(l) l.Qty <= 1).ToList()
        Dim others = lines.Except(atOne).ToList()
        If others.Count > 0 Then
            others.ForEach(Sub(l) l.Qty -= 1)
            RefreshGrid(lines)
            MarkDirty()
        End If
        If atOne.Count > 0 Then AskRemove(atOne)
    End Sub

    Private Sub cmsBom_Opening(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles cmsBom.Opening
        Dim lines = SelectedLines()
        If lines.Count = 0 Then
            e.Cancel = True
            Return
        End If
        Dim l = If(MenuLine(), lines(0))
        mnuOpenProduct.Enabled = l.ProductUrl.Length > 0
        mnuCopyPartNo.Enabled = l.SupplierPartNumber.Length > 0
        mnuCopyMpn.Enabled = l.ManufacturerPartNumber.Length > 0
        mnuRemove.Text = If(lines.Count > 1, $"Remove {lines.Count} lines", "Remove")
    End Sub

    Private Function MenuLine() As BomLine
        Dim l = LineAt(If(dgvBom.CurrentRow?.Index, -1))
        Return If(l, SelectedLines().FirstOrDefault())
    End Function

    Private Sub mnuOpenProduct_Click(sender As Object, e As EventArgs) Handles mnuOpenProduct.Click
        Dim l = MenuLine()
        If l IsNot Nothing Then OpenUrl(l.ProductUrl)
    End Sub

    Private Sub mnuCopyPartNo_Click(sender As Object, e As EventArgs) Handles mnuCopyPartNo.Click
        Dim l = MenuLine()
        If l IsNot Nothing AndAlso l.SupplierPartNumber.Length > 0 Then Clipboard.SetText(l.SupplierPartNumber)
    End Sub

    Private Sub mnuCopyMpn_Click(sender As Object, e As EventArgs) Handles mnuCopyMpn.Click
        Dim l = MenuLine()
        If l IsNot Nothing AndAlso l.ManufacturerPartNumber.Length > 0 Then Clipboard.SetText(l.ManufacturerPartNumber)
    End Sub

    Private Sub OpenUrl(url As String)
        If String.IsNullOrWhiteSpace(url) Then Return
        Try
            WebHelper.OpenInBrowser(url)
        Catch ex As Exception
            MessageBox.Show(Me, "Could not open " & url & vbCrLf & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

#End Region

#Region "Name, notes and files"

    Private Sub txtName_TextChanged(sender As Object, e As EventArgs) Handles txtName.TextChanged
        If _settingText Then Return
        _bom.Name = txtName.Text
        MarkDirty()
    End Sub

    Private Sub txtNotes_TextChanged(sender As Object, e As EventArgs) Handles txtNotes.TextChanged
        If _settingText Then Return
        _bom.Notes = txtNotes.Text
        MarkDirty()
    End Sub

    Private Sub btnNew_Click(sender As Object, e As EventArgs) Handles btnNew.Click
        CommitEdit()
        If Not ConfirmDiscard() Then Return
        SetBom(New Bom(), "", dirty:=False)
        txtName.Select()
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        CommitEdit()
        If Not ConfirmDiscard() Then Return
        Using dlg As New OpenFileDialog With {.Filter = "BOM (*.csv)|*.csv|All files (*.*)|*.*"}
            Dim folder = StartFolder()
            If folder.Length > 0 Then dlg.InitialDirectory = folder
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                SetBom(Bom.Load(dlg.FileName), dlg.FileName, dirty:=False)
                LastFolder = Path.GetDirectoryName(dlg.FileName)
            Catch ex As Exception
                MessageBox.Show(Me, "Could not load the BOM: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        CommitEdit()
        SaveBom()
    End Sub

    Private Sub btnSaveAs_Click(sender As Object, e As EventArgs) Handles btnSaveAs.Click
        CommitEdit()
        SaveBomAs()
    End Sub

    ''' <summary>Unsaved changes: Save / Don't Save / Cancel. Returns False for Cancel (or a save that failed).</summary>
    Public Function ConfirmDiscard() As Boolean
        If Not HasUnsavedChanges Then Return True
        Select Case MessageBox.Show(Me, $"Save changes to ""{_bom.DisplayName}""?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            Case DialogResult.Yes : Return SaveBom()
            Case DialogResult.No : Return True
            Case Else : Return False
        End Select
    End Function

    Private Function SaveBom() As Boolean
        If _filePath.Length = 0 Then Return SaveBomAs()
        Return WriteFile(_filePath)
    End Function

    ''' <summary>Asks where to save. An unnamed BOM takes the file's name.</summary>
    Private Function SaveBomAs() As Boolean
        Dim suggested = String.Concat(_bom.DisplayName.Split(Path.GetInvalidFileNameChars())).Trim()
        Using dlg As New SaveFileDialog With {
            .Filter = "BOM (*.csv)|*.csv",
            .FileName = If(suggested.Length > 0, suggested, Bom.UntitledName) & ".csv"
        }
            Dim folder = StartFolder()
            If folder.Length > 0 Then dlg.InitialDirectory = folder
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return False
            If _bom.Name.Trim().Length = 0 Then
                _settingText = True
                txtName.Text = Path.GetFileNameWithoutExtension(dlg.FileName)
                _settingText = False
                _bom.Name = txtName.Text
            End If
            Return WriteFile(dlg.FileName)
        End Using
    End Function

    Private Function WriteFile(path As String) As Boolean
        Try
            _bom.Save(path)
        Catch ex As Exception
            MessageBox.Show(Me, "Could not save the BOM: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return False
        End Try
        _filePath = path
        LastFolder = IO.Path.GetDirectoryName(path)
        _dirty = False
        UpdateTotals()
        RaiseEvent BomChanged(Me, EventArgs.Empty)
        Return True
    End Function

    ''' <summary>This BOM's folder, else the last one used, else "" (the dialogs then start in Documents).</summary>
    Private Function StartFolder() As String
        For Each folder In {If(_filePath.Length > 0, Path.GetDirectoryName(_filePath), ""), LastFolder}
            If Not String.IsNullOrEmpty(folder) AndAlso Directory.Exists(folder) Then Return folder
        Next
        Return ""
    End Function

    Private Sub btnExportTxt_Click(sender As Object, e As EventArgs) Handles btnExportTxt.Click
        CommitEdit()
        If _bom.Lines.Count = 0 Then
            MessageBox.Show(Me, "Nothing to export yet.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        Dim suggested = String.Concat(_bom.DisplayName.Split(Path.GetInvalidFileNameChars())).Trim()
        Using dlg As New SaveFileDialog With {
            .Filter = "Text file (*.txt)|*.txt",
            .FileName = If(suggested.Length > 0, suggested, Bom.UntitledName) & ".txt"
        }
            Dim folder = StartFolder()
            If folder.Length > 0 Then dlg.InitialDirectory = folder
            If dlg.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                File.WriteAllText(dlg.FileName, BomText(), New UTF8Encoding(True)) ' BOM so £ and € show properly everywhere
            Catch ex As Exception
                MessageBox.Show(Me, "Could not export the BOM: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    ''' <summary>
    ''' The BOM as plain text for reading: its name, then each supplier's lines in columns lined up with spaces
    ''' (with a subtotal), then the grand total. Prices are as shown in the window. Unticked lines are listed but not counted.
    ''' </summary>
    Private Function BomText() As String
        Const MaxDescription As Integer = 45
        Const Gap As String = "  "
        Dim headers = {"Supplier Part No.", "Mfr Part No.", "Description", "Qty", "Unit Price", "Total", "Link"}
        Dim rightAligned = {False, False, False, True, True, True, False}
        Dim cells = _bom.Lines.Select(
            Function(l)
                Dim price = ShownPrice(l)
                Dim desc = DescriptionText(l)
                If desc.Length > MaxDescription Then desc = desc.Substring(0, MaxDescription - 1) & "…"
                Return {l.SupplierPartNumber, l.ManufacturerPartNumber, desc, l.Qty.ToString(CultureInfo.CurrentCulture),
                        If(price.Unit.HasValue, Money(price.Unit.Value, price.Currency, "0.00##"), ""),
                        If(price.Unit.HasValue, Money(price.Unit.Value * l.Qty, price.Currency, "0.00"), ""),
                        l.ProductUrl & If(l.Include, "", "   (not counted)")}
            End Function).ToList()
        ' Each column as wide as its widest entry (the link is last, so it isn't padded).
        Dim widths = Enumerable.Range(0, headers.Length).
            Select(Function(c) Math.Max(headers(c).Length, cells.Max(Function(r) r(c).Length))).ToArray()
        Dim lineOf = Function(values As String()) As String
                         Dim parts = values.Select(Function(v, c) If(c = values.Length - 1, v,
                                                   If(rightAligned(c), v.PadLeft(widths(c)), v.PadRight(widths(c)))))
                         Return ("  " & String.Join(Gap, parts)).TrimEnd()
                     End Function
        ' Right edge of the Total column, so subtotals line up under it.
        Dim totalEnd = 2 + widths.Take(6).Sum() + Gap.Length * 5

        Dim currencies As New List(Of String)
        Dim grand As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
        Dim noPrice = 0
        Dim sb As New StringBuilder()
        sb.AppendLine("BOM: " & _bom.DisplayName)
        For Each group In _bom.Lines.Select(Function(l, i) (Line:=l, Cells:=cells(i))).
                                     GroupBy(Function(x) If(x.Line.Supplier.Length > 0, x.Line.Supplier, "(no supplier)"))
            sb.AppendLine()
            sb.AppendLine(group.Key)
            sb.AppendLine(lineOf(headers))
            Dim subtotal As New Dictionary(Of String, Decimal)(StringComparer.OrdinalIgnoreCase)
            For Each x In group
                sb.AppendLine(lineOf(x.Cells))
                If Not x.Line.Include Then Continue For
                Dim price = ShownPrice(x.Line)
                If Not price.Unit.HasValue Then
                    noPrice += 1
                    Continue For
                End If
                Dim cur = price.Currency.ToUpperInvariant()
                If Not currencies.Contains(cur) Then currencies.Add(cur)
                Dim amount = price.Unit.Value * x.Line.Qty
                subtotal(cur) = If(subtotal.ContainsKey(cur), subtotal(cur), 0D) + amount
                grand(cur) = If(grand.ContainsKey(cur), grand(cur), 0D) + amount
            Next
            If subtotal.Count > 0 Then sb.AppendLine(("Subtotal:  " & MoneyList(subtotal, currencies)).PadLeft(totalEnd))
        Next
        sb.AppendLine()
        sb.AppendLine("Grand total: " & If(grand.Count = 0, Money(0D, If(_convertTo, ""), "0.00"), MoneyList(grand, currencies)) &
                      If(noPrice > 0, $"   ({noPrice} line{If(noPrice = 1, "", "s")} without a price)", ""))
        Return sb.ToString()
    End Function

    ' Opens the BOM's folder in Explorer with its file highlighted - or, before it's saved, the last BOM folder (else Documents).
    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Try
            If _filePath.Length > 0 AndAlso File.Exists(_filePath) Then
                Process.Start("explorer.exe", $"/select,""{_filePath}""")
                Return
            End If
            Dim folder = StartFolder()
            If folder.Length = 0 Then folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            Process.Start("explorer.exe", $"""{folder}""")
        Catch ex As Exception
            MessageBox.Show(Me, "Could not open the folder: " & ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

#End Region

End Class
