''' <summary>One line of search results from a supplier.</summary>
Public Class PartResult
    Public Property Supplier As String = ""
    Public Property SupplierPartNumber As String = ""
    Public Property ManufacturerPartNumber As String = ""
    Public Property Manufacturer As String = ""
    Public Property Description As String = ""
    ''' <summary>Kind of part, e.g. "Analog to Digital Converters - ADC" - the supplier's own category wording.</summary>
    Public Property Category As String = ""

    ''' <summary>Quantity in stock, or Nothing when the supplier didn't say.</summary>
    Public Property Stock As Long?
    ''' <summary>Free-text availability / lead time info.</summary>
    Public Property StockInfo As String = ""

    ''' <summary>Unit price at the lowest price break.</summary>
    Public Property UnitPrice As Decimal?
    Public Property Currency As String = ""
    Public Property PriceBreaks As New List(Of PriceBreak)
    Public Property MinOrderQty As Integer?

    Public Property Status As String = ""
    Public Property ProductUrl As String = ""
    Public Property DatasheetUrl As String = ""

    ''' <summary>Product photo for the grid thumbnail.</summary>
    Public Property ImageUrl As String = ""
    ''' <summary>Bigger version for the pop-up, if the supplier has one (else ImageUrl is used).</summary>
    Public Property LargeImageUrl As String = ""

    ''' <summary>True when this row is just a "search on the website" link (no API data).</summary>
    Public Property IsLinkOnly As Boolean

    Public Function PriceBreaksText() As String
        Return String.Join("   ", PriceBreaks.OrderBy(Function(b) b.Quantity).
                                  Select(Function(b) $"{b.Quantity}+ {b.Price:0.00##}"))
    End Function
End Class

Public Class PriceBreak
    Public Property Quantity As Integer
    Public Property Price As Decimal

    Public Sub New(quantity As Integer, price As Decimal)
        Me.Quantity = quantity
        Me.Price = price
    End Sub
End Class

Public Enum SearchMode
    Keyword
    ''' <summary>Manufacturer part number starts with the search text.</summary>
    PartNumber
    ''' <summary>Manufacturer part number is exactly the search text.</summary>
    ExactPartNumber
    ''' <summary>A supplier's own order code (Farnell/CPC/Newark, Mouser, DigiKey or TME number).</summary>
    SupplierCode
End Enum

''' <summary>Filters the suppliers apply on their side (so a page of 50 is 50 matching parts).</summary>
Public Class SupplierFilters
    Public Property InStockOnly As Boolean
    Public Property RohsOnly As Boolean

    Public Shared ReadOnly None As New SupplierFilters()
End Class

''' <summary>Everything set in the Advanced search window. Also saved, so it reopens with the last choices.</summary>
Public Class AdvancedSearchOptions
    Public Property Query As String = ""
    Public Property Mode As SearchMode = SearchMode.Keyword
    Public Property MaxResults As Integer = 25

    ' Applied by the suppliers
    Public Property InStockOnly As Boolean
    Public Property RohsOnly As Boolean

    ' Applied by the app to what comes back
    Public Property ActiveOnly As Boolean
    Public Property ManufacturerContains As String = ""
    ''' <summary>Only parts whose manufacturer part number contains this (case and spaces ignored), e.g. 931G.</summary>
    Public Property PartNoContains As String = ""
    Public Property MustContain As String = ""
    ''' <summary>
    ''' True when Search for was left blank, so the part-number fragment and MustContain words were searched for
    ''' instead. The words are then not checked again letter-for-letter (the suppliers matched them their own way),
    ''' and the window reopens with Search for blank.
    ''' </summary>
    Public Property ContainsIsQuery As Boolean
    ''' <summary>0 = not set. When set, prices are shown at this quantity and parts with a higher MOQ are left out.</summary>
    Public Property Quantity As Integer
    ''' <summary>Nothing = no limit. Compared with the price at Quantity (or at 1).</summary>
    Public Property MaxUnitPrice As Decimal?

    ''' <summary>Suppliers to search (names). Empty = whatever is ticked in the main list.</summary>
    Public Property Suppliers As New List(Of String)

    Public Function SupplierFilters() As SupplierFilters
        Return New SupplierFilters With {.InStockOnly = InStockOnly, .RohsOnly = RohsOnly}
    End Function

    ''' <summary>Short summary for the status bar, e.g. "in stock, RoHS, qty 25". Empty if nothing is set.</summary>
    Public Function Describe() As String
        Dim bits As New List(Of String)
        Select Case Mode
            Case SearchMode.PartNumber : bits.Add("mfr part no.")
            Case SearchMode.ExactPartNumber : bits.Add("exact part no.")
            Case SearchMode.SupplierCode : bits.Add("supplier code")
        End Select
        If InStockOnly Then bits.Add("in stock")
        If RohsOnly Then bits.Add("RoHS")
        If ActiveOnly Then bits.Add("active only")
        If ManufacturerContains.Trim().Length > 0 Then bits.Add($"maker ""{ManufacturerContains.Trim()}""")
        If PartNoContains.Trim().Length > 0 Then bits.Add($"part no. contains ""{PartNoContains.Trim()}""")
        If WordsToCheck.Length > 0 Then bits.Add($"contains ""{WordsToCheck}""")
        If Quantity > 0 Then bits.Add($"qty {Quantity}")
        If MaxUnitPrice.HasValue Then bits.Add($"max {MaxUnitPrice.Value:0.00##} each")
        Return String.Join(", ", bits)
    End Function

    ''' <summary>The Description also contains words to check in each part - none when they were the search itself.</summary>
    Private ReadOnly Property WordsToCheck As String
        Get
            Return If(ContainsIsQuery, "", MustContain.Trim())
        End Get
    End Property

    ''' <summary>True if any of the app-side filters are set.</summary>
    Public ReadOnly Property HasAppFilters As Boolean
        Get
            Return ActiveOnly OrElse ManufacturerContains.Trim().Length > 0 OrElse PartNoContains.Trim().Length > 0 OrElse WordsToCheck.Length > 0 OrElse
                   Quantity > 0 OrElse MaxUnitPrice.HasValue
        End Get
    End Property

    ''' <summary>App-side filters. Website-link rows always pass.</summary>
    Public Function Matches(p As PartResult) As Boolean
        If p.IsLinkOnly Then Return True
        If ActiveOnly AndAlso IsNotActive(p.Status) Then Return False
        If ManufacturerContains.Trim().Length > 0 AndAlso
           p.Manufacturer.IndexOf(ManufacturerContains.Trim(), StringComparison.OrdinalIgnoreCase) < 0 Then Return False
        If PartNoContains.Trim().Length > 0 AndAlso
           NoSpaces(p.ManufacturerPartNumber).IndexOf(NoSpaces(PartNoContains), StringComparison.OrdinalIgnoreCase) < 0 Then Return False
        For Each word In WordsToCheck.Split({" "c, ","c}, StringSplitOptions.RemoveEmptyEntries)
            If p.Description.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0 AndAlso
               p.ManufacturerPartNumber.IndexOf(word, StringComparison.OrdinalIgnoreCase) < 0 Then Return False
        Next
        If Quantity > 0 AndAlso p.MinOrderQty.HasValue AndAlso p.MinOrderQty.Value > Quantity Then Return False
        If MaxUnitPrice.HasValue Then
            ' No price listed counts as "not under the limit" - those usually can't be bought anyway.
            Dim price = PriceAt(p, Math.Max(1, Quantity))
            If Not price.HasValue OrElse price.Value > MaxUnitPrice.Value Then Return False
        End If
        Return True
    End Function

    ''' <summary>Unit price when buying <paramref name="qty"/>: the highest price break at or below that quantity.</summary>
    Public Shared Function PriceAt(p As PartResult, qty As Integer) As Decimal?
        If p.PriceBreaks.Count = 0 Then Return p.UnitPrice
        Dim ordered = p.PriceBreaks.OrderBy(Function(b) b.Quantity).ToList()
        Dim best = ordered(0)
        For Each b In ordered
            If b.Quantity <= qty Then best = b
        Next
        Return best.Price
    End Function

    Private Shared Function NoSpaces(s As String) As String
        Return If(s, "").Replace(" ", "")
    End Function

    Private Shared Function IsNotActive(status As String) As Boolean
        If String.IsNullOrWhiteSpace(status) Then Return False ' unknown - keep it
        For Each word In {"obsolete", "discontinued", "not recommended", "nrnd", "end of life", "eol",
                          "last time", "no longer", "not for new"}
            If status.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0 Then Return True
        Next
        Return False
    End Function
End Class
