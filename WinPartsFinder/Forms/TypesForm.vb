''' <summary>
''' The Types… window: every part type in the current results, with tick boxes.
''' On OK, <see cref="HiddenTypes"/> holds the unticked ones ("" = parts with no type).
''' </summary>
Public Class TypesForm

    ''' <summary>One line in the list.</summary>
    Public Class TypeEntry
        Public Property Name As String = ""   ' "" = no type
        Public Property Count As Integer
        Public Property Suppliers As New List(Of String)
    End Class

    Private Const NoType As String = "(no type)"

    Private ReadOnly _types As List(Of TypeEntry)
    Private ReadOnly _hidden As HashSet(Of String)
    Private _sortColumn As Integer = 0
    Private _sortDescending As Boolean

    ''' <summary>The types left unticked - valid after the window closes with OK.</summary>
    Public ReadOnly Property HiddenTypes As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ' Parameterless constructor for the WinForms designer.
    Public Sub New()
        Me.New(New List(Of TypeEntry), New HashSet(Of String))
    End Sub

    Public Sub New(types As List(Of TypeEntry), hidden As HashSet(Of String))
        InitializeComponent()
        _types = types
        _hidden = hidden
    End Sub

    Private Sub TypesForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lvTypes.BeginUpdate()
        For Each t In _types
            Dim item As New ListViewItem(If(t.Name.Length = 0, NoType, t.Name)) With {
                .Tag = t,
                .Checked = Not _hidden.Contains(t.Name)
            }
            item.SubItems.Add(t.Count.ToString("#,##0"))
            item.SubItems.Add(String.Join(", ", t.Suppliers))
            lvTypes.Items.Add(item)
        Next
        lvTypes.EndUpdate()
        Theme.Apply(Me)
        FitColumns()
        UpdateTitle()
    End Sub

    ' Suppliers takes whatever width is left, so there's no sideways scroll bar.
    Private Sub FitColumns() Handles lvTypes.Resize
        If colSuppliers Is Nothing Then Return
        colSuppliers.Width = Math.Max(80, lvTypes.ClientSize.Width - colType.Width - colParts.Width)
    End Sub

    Private Sub lvTypes_ItemChecked(sender As Object, e As ItemCheckedEventArgs) Handles lvTypes.ItemChecked
        UpdateTitle()
    End Sub

    ''' <summary>"Part types - 12 of 15 shown (140 of 163 parts)".</summary>
    Private Sub UpdateTitle()
        Dim items = lvTypes.Items.Cast(Of ListViewItem)().ToList()
        Dim ticked = items.Where(Function(i) i.Checked).ToList()
        Dim parts = items.Sum(Function(i) DirectCast(i.Tag, TypeEntry).Count)
        Dim shown = ticked.Sum(Function(i) DirectCast(i.Tag, TypeEntry).Count)
        Text = $"Part types - {ticked.Count} of {items.Count} shown ({shown:#,##0} of {parts:#,##0} parts)"
    End Sub

    Private Sub btnAll_Click(sender As Object, e As EventArgs) Handles btnAll.Click
        SetAll(True)
    End Sub

    Private Sub btnNone_Click(sender As Object, e As EventArgs) Handles btnNone.Click
        SetAll(False)
    End Sub

    Private Sub SetAll(ticked As Boolean)
        lvTypes.BeginUpdate()
        For Each item As ListViewItem In lvTypes.Items
            item.Checked = ticked
        Next
        lvTypes.EndUpdate()
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        HiddenTypes.Clear()
        For Each item As ListViewItem In lvTypes.Items
            If Not item.Checked Then HiddenTypes.Add(DirectCast(item.Tag, TypeEntry).Name)
        Next
        DialogResult = DialogResult.OK
        Close()
    End Sub

    ' Click a header to sort by it; click again to reverse.
    Private Sub lvTypes_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles lvTypes.ColumnClick
        _sortDescending = e.Column = _sortColumn AndAlso Not _sortDescending
        _sortColumn = e.Column
        lvTypes.ListViewItemSorter = New TypeSorter(_sortColumn, _sortDescending)
    End Sub

    Private Class TypeSorter
        Implements IComparer

        Private ReadOnly _column As Integer
        Private ReadOnly _descending As Boolean

        Public Sub New(column As Integer, descending As Boolean)
            _column = column
            _descending = descending
        End Sub

        Public Function Compare(x As Object, y As Object) As Integer Implements IComparer.Compare
            Dim a = DirectCast(DirectCast(x, ListViewItem).Tag, TypeEntry)
            Dim b = DirectCast(DirectCast(y, ListViewItem).Tag, TypeEntry)
            Dim result As Integer
            Select Case _column
                Case 1 : result = a.Count.CompareTo(b.Count)
                Case 2 : result = String.Compare(String.Join(", ", a.Suppliers), String.Join(", ", b.Suppliers), StringComparison.OrdinalIgnoreCase)
                Case Else : result = String.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
            End Select
            Return If(_descending, -result, result)
        End Function
    End Class

End Class
