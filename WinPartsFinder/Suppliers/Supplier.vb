''' <summary>
''' Base class for all suppliers. Suppliers with an API override SearchAsync;
''' the rest just supply a website search link.
''' </summary>
Public MustInherit Class Supplier

    Protected Sub New(name As String, searchUrlTemplate As String)
        Me.Name = name
        Me.SearchUrlTemplate = searchUrlTemplate
    End Sub

    Public ReadOnly Property Name As String
    Public Property SearchUrlTemplate As String

    ''' <summary>True when the API can be called (key present etc).</summary>
    Public MustOverride ReadOnly Property IsConfigured As Boolean

    ''' <summary>Short text for the supplier list, e.g. "API" or "Web link".</summary>
    Public MustOverride ReadOnly Property ModeText As String

    ''' <summary>Fetches one page of results, starting at <paramref name="offset"/> (0 = first page).</summary>
    Public Overridable Function SearchAsync(query As String, mode As SearchMode, maxResults As Integer, offset As Integer,
                                            ct As CancellationToken) As Task(Of SearchPage)
        Throw New NotSupportedException($"{Name} has no API search.")
    End Function

    ''' <summary>Whether "Load more" (next page) works for this kind of search.</summary>
    Public Overridable Function CanLoadMore(mode As SearchMode) As Boolean
        Return False
    End Function

    Public Function GetSearchUrl(query As String) As String
        Return SearchUrlTemplate.Replace("{0}", Uri.EscapeDataString(query))
    End Function

    Public Function MakeLinkResult(query As String) As PartResult
        Return New PartResult With {
            .Supplier = Name,
            .Description = $"Search {Name} website for ""{query}"" (double-click to open)",
            .StockInfo = "see website",
            .ProductUrl = GetSearchUrl(query),
            .IsLinkOnly = True
        }
    End Function

End Class

''' <summary>Supplier with no usable public API: opens the website search.</summary>
Public Class WebLinkSupplier
    Inherits Supplier

    Public Sub New(name As String, searchUrlTemplate As String)
        MyBase.New(name, searchUrlTemplate)
    End Sub

    Public Overrides ReadOnly Property IsConfigured As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property ModeText As String
        Get
            Return "Web link"
        End Get
    End Property

End Class

''' <summary>Base for suppliers that have an API but need a key.</summary>
Public MustInherit Class ApiSupplier
    Inherits Supplier

    Protected Sub New(name As String, searchUrlTemplate As String)
        MyBase.New(name, searchUrlTemplate)
    End Sub

    Public Overrides ReadOnly Property ModeText As String
        Get
            Return If(IsConfigured, "API", "API (no key)")
        End Get
    End Property

    Public Overrides Function CanLoadMore(mode As SearchMode) As Boolean
        Return True
    End Function

End Class

''' <summary>One page of results from a supplier.</summary>
Public Class SearchPage
    Public Property Parts As New List(Of PartResult)
    ''' <summary>How many matches the supplier says it has in total, if it said.</summary>
    Public Property TotalAvailable As Integer?

    Public Sub New(parts As List(Of PartResult), totalAvailable As Integer?)
        Me.Parts = parts
        Me.TotalAvailable = totalAvailable
    End Sub
End Class

Public Module SupplierFactory

    Public Function Create(s As AppSettings) As List(Of Supplier)
        Dim all As New Dictionary(Of String, Supplier) From {
            {AppSettings.RS, New WebLinkSupplier(AppSettings.RS, s.GetSearchUrl(AppSettings.RS))},
            {AppSettings.Farnell, New Element14Supplier(AppSettings.Farnell, "uk.farnell.com", "GBP",
                                                        s.Element14ApiKey, s.GetSearchUrl(AppSettings.Farnell))},
            {AppSettings.DigiKey, New DigiKeySupplier(s.DigiKeyClientId, s.DigiKeyClientSecret, s.DigiKeySite,
                                                      s.DigiKeyLanguage, s.DigiKeyCurrency, s.GetSearchUrl(AppSettings.DigiKey))},
            {AppSettings.Mouser, New MouserSupplier(s.MouserApiKey, s.GetSearchUrl(AppSettings.Mouser))},
            {AppSettings.Newark, New Element14Supplier(AppSettings.Newark, "www.newark.com", "USD",
                                                       s.Element14ApiKey, s.GetSearchUrl(AppSettings.Newark))},
            {AppSettings.Rapid, New WebLinkSupplier(AppSettings.Rapid, s.GetSearchUrl(AppSettings.Rapid))},
            {AppSettings.LCSC, New WebLinkSupplier(AppSettings.LCSC, s.GetSearchUrl(AppSettings.LCSC))},
            {AppSettings.AliExpress, New WebLinkSupplier(AppSettings.AliExpress, s.GetSearchUrl(AppSettings.AliExpress))}
        }
        Dim result = AppSettings.SupplierOrder.Select(Function(n) all(n)).ToList()
        For Each c In s.CustomSuppliers
            If String.IsNullOrWhiteSpace(c.Name) OrElse all.ContainsKey(c.Name) Then Continue For
            result.Add(New WebLinkSupplier(c.Name.Trim(), c.SearchUrl.Trim()))
        Next
        Return result
    End Function

End Module
