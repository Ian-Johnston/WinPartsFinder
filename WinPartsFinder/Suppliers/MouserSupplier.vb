Imports System.Net.Http
Imports System.Text

''' <summary>
''' Mouser Search API v1. Free key from https://www.mouser.co.uk/api-hub/ (Search API).
''' Prices come back in the currency of the account the key was registered with.
''' </summary>
Public Class MouserSupplier
    Inherits ApiSupplier

    Private Const BaseUrl As String = "https://api.mouser.com/api/v1/search/"
    Private ReadOnly _apiKey As String

    Public Sub New(apiKey As String, searchUrlTemplate As String)
        MyBase.New(AppSettings.Mouser, searchUrlTemplate)
        _apiKey = If(apiKey, "").Trim()
    End Sub

    Public Overrides ReadOnly Property IsConfigured As Boolean
        Get
            Return _apiKey.Length > 0
        End Get
    End Property

    ' The part-number endpoint has no paging; keyword search does (startingRecord).
    Public Overrides Function CanLoadMore(mode As SearchMode) As Boolean
        Return mode = SearchMode.Keyword
    End Function

    Public Overrides Async Function SearchAsync(query As String, mode As SearchMode, maxResults As Integer, offset As Integer,
                                                ct As CancellationToken) As Task(Of SearchPage)
        Dim endpoint As String
        Dim payload As Object
        If mode = SearchMode.PartNumber Then
            endpoint = "partnumber"
            payload = New With {
                .SearchByPartRequest = New With {.mouserPartNumber = query, .partSearchOptions = "None"}
            }
        Else
            endpoint = "keyword"
            payload = New With {
                .SearchByKeywordRequest = New With {
                    .keyword = query,
                    .records = Math.Min(maxResults, 50),
                    .startingRecord = offset,
                    .searchOptions = "None",
                    .searchWithYourSignUpLanguage = "false"
                }
            }
        End If

        Dim url = $"{BaseUrl}{endpoint}?apiKey={Uri.EscapeDataString(_apiKey)}"
        Dim json = JsonConvert.SerializeObject(payload)
        Using req As New HttpRequestMessage(HttpMethod.Post, url) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
            Dim root = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
            Dim parts = Parse(root, maxResults)
            Dim total = ToNullableInt(root.GetNum("SearchResults", "NumberOfResult"))
            Return New SearchPage(parts, total)
        End Using
    End Function

    Private Function Parse(root As JToken, maxResults As Integer) As List(Of PartResult)
        Dim errors = root.GetArr("Errors")
        If errors.Count > 0 Then
            Throw New SupplierException(String.Join("; ", errors.Select(Function(er) er.GetStr("Message"))))
        End If

        Dim results As New List(Of PartResult)
        For Each p In root.GetArr("SearchResults", "Parts").Take(maxResults)
            Dim availability = p.GetStr("Availability")
            Dim stock = ParseLongLoose(p.GetStr("AvailabilityInStock"))
            If Not stock.HasValue Then
                ' "1,234 In Stock" or "None" / "On Order"
                stock = If(availability.IndexOf("In Stock", StringComparison.OrdinalIgnoreCase) >= 0,
                           ParseLongLoose(availability), 0L)
            End If

            Dim leadTime = p.GetStr("LeadTime")
            If ParseLongLoose(leadTime).GetValueOrDefault() = 0 Then leadTime = "" ' "0 Days" means unknown

            Dim mouserPn = p.GetStr("MouserPartNumber")
            Dim notStocked = mouserPn.Equals("N/A", StringComparison.OrdinalIgnoreCase)

            Dim r As New PartResult With {
                .Supplier = Name,
                .SupplierPartNumber = If(notStocked, "", mouserPn),
                .ManufacturerPartNumber = p.GetStr("ManufacturerPartNumber"),
                .Manufacturer = p.GetStr("Manufacturer"),
                .Description = p.GetStr("Description"),
                .Stock = stock,
                .StockInfo = JoinNonEmpty(If(notStocked, "Not stocked by Mouser", availability),
                                          If(leadTime.Length > 0, "Lead " & leadTime, "")),
                .MinOrderQty = ToNullableInt(ParseDecimalLoose(p.GetStr("Min"))),
                .Status = p.GetStr("LifecycleStatus"),
                .ProductUrl = p.GetStr("ProductDetailUrl"),
                .DatasheetUrl = p.GetStr("DataSheetUrl")
            }
            ' ImageUrl is deliberately left blank: Mouser's image server rejects non-browser
            ' requests ("Access denied"). The grid borrows a photo of the same MPN from
            ' another supplier instead.
            For Each pb In p.GetArr("PriceBreaks")
                Dim price = ParseDecimalLoose(pb.GetStr("Price"))
                Dim qty = ToNullableInt(pb.GetNum("Quantity"))
                If price.HasValue AndAlso qty.HasValue Then r.PriceBreaks.Add(New PriceBreak(qty.Value, price.Value))
                If r.Currency.Length = 0 Then r.Currency = pb.GetStr("Currency")
            Next
            If r.PriceBreaks.Count > 0 Then r.UnitPrice = r.PriceBreaks.OrderBy(Function(b) b.Quantity).First().Price
            If r.Status.Length = 0 AndAlso p.GetStr("IsDiscontinued") = "true" Then r.Status = "Discontinued"
            results.Add(r)
        Next
        Return results
    End Function

    Private Shared Function JoinNonEmpty(ParamArray parts() As String) As String
        Return String.Join(" | ", parts.Where(Function(x) Not String.IsNullOrWhiteSpace(x)))
    End Function

End Class
