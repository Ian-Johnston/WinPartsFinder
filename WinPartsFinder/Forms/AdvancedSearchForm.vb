Imports System.Globalization

''' <summary>The Advanced search window. On Search, <see cref="Options"/> holds everything chosen.</summary>
Public Class AdvancedSearchForm

    Private ReadOnly _start As AdvancedSearchOptions
    Private ReadOnly _supplierNames As List(Of String)

    ''' <summary>The choices made - valid after the window closes with OK.</summary>
    Public Property Options As AdvancedSearchOptions

    ' Parameterless constructor for the WinForms designer.
    Public Sub New()
        Me.New(New AdvancedSearchOptions(), New List(Of String))
    End Sub

    ''' <param name="start">What to show when the window opens (last time's choices).</param>
    ''' <param name="supplierNames">Every supplier, in supplier-list order.</param>
    Public Sub New(start As AdvancedSearchOptions, supplierNames As List(Of String))
        InitializeComponent()
        _start = If(start, New AdvancedSearchOptions())
        _supplierNames = supplierNames
    End Sub

    Private Sub AdvancedSearchForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each n In _supplierNames
            clbSuppliers.Items.Add(n)
        Next
        Show(_start)
        Theme.Apply(Me)
        txtQuery.Select()
        txtQuery.SelectAll()
    End Sub

    Private Overloads Sub Show(o As AdvancedSearchOptions)
        txtQuery.Text = o.Query
        cboMode.SelectedIndex = CInt(o.Mode)
        chkInStock.Checked = o.InStockOnly
        chkRohs.Checked = o.RohsOnly
        chkActive.Checked = o.ActiveOnly
        txtMaker.Text = o.ManufacturerContains
        txtContains.Text = o.MustContain
        nudQty.Value = Math.Max(nudQty.Minimum, Math.Min(nudQty.Maximum, o.Quantity))
        txtMaxPrice.Text = If(o.MaxUnitPrice.HasValue, o.MaxUnitPrice.Value.ToString("0.00##", CultureInfo.CurrentCulture), "")
        nudMax.Value = Math.Max(nudMax.Minimum, Math.Min(nudMax.Maximum, o.MaxResults))
        For i = 0 To clbSuppliers.Items.Count - 1
            clbSuppliers.SetItemChecked(i, o.Suppliers.Count = 0 OrElse o.Suppliers.Contains(CStr(clbSuppliers.Items(i))))
        Next
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        Dim query = txtQuery.Text.Trim()
        If query.Length < 2 Then
            MessageBox.Show(Me, "Type at least 2 characters to search for.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            txtQuery.Select()
            Return
        End If

        Dim maxPrice As Decimal? = Nothing
        Dim priceText = txtMaxPrice.Text.Trim().TrimStart("£"c, "$"c, "€"c)
        If priceText.Length > 0 Then
            Dim d As Decimal
            If Not Decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.CurrentCulture, d) OrElse d <= 0 Then
                MessageBox.Show(Me, "Max price each should be a number, e.g. 0.50 - or leave it blank.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
                txtMaxPrice.Select()
                Return
            End If
            maxPrice = d
        End If

        Dim chosen = clbSuppliers.CheckedItems.Cast(Of Object)().Select(Function(x) CStr(x)).ToList()
        If chosen.Count = 0 Then
            MessageBox.Show(Me, "Tick at least one supplier to search.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Options = New AdvancedSearchOptions With {
            .Query = query,
            .Mode = CType(Math.Max(0, cboMode.SelectedIndex), SearchMode),
            .MaxResults = CInt(nudMax.Value),
            .InStockOnly = chkInStock.Checked,
            .RohsOnly = chkRohs.Checked,
            .ActiveOnly = chkActive.Checked,
            .ManufacturerContains = txtMaker.Text.Trim(),
            .MustContain = txtContains.Text.Trim(),
            .Quantity = CInt(nudQty.Value),
            .MaxUnitPrice = maxPrice,
            .Suppliers = chosen
        }
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click
        Show(New AdvancedSearchOptions With {.Query = txtQuery.Text, .MaxResults = CInt(nudMax.Value)})
    End Sub

End Class
