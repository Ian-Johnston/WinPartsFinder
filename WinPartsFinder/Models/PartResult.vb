''' <summary>One line of search results from a supplier.</summary>
Public Class PartResult
    Public Property Supplier As String = ""
    Public Property SupplierPartNumber As String = ""
    Public Property ManufacturerPartNumber As String = ""
    Public Property Manufacturer As String = ""
    Public Property Description As String = ""

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
    PartNumber
End Enum
