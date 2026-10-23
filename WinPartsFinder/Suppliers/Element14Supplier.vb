Imports System.Net.Http

''' <summary>
''' element14 Product Search API - covers Farnell (uk.farnell.com) and Newark (www.newark.com)
''' with the same key. Register at https://partner.element14.com/
''' </summary>
Public Class Element14Supplier
    Inherits ApiSupplier

    Private Const BaseUrl As String = "https://api.element14.com/catalog/products"

    ' Farnell and Newark share one key, and the Basic tier allows 2 calls/second.
    ' These are Shared so both instances queue through the same gate.
    Private Shared ReadOnly _rateGate As New SemaphoreSlim(1, 1)
    Private Shared _lastCallUtc As DateTime = DateTime.MinValue
    Private Shared ReadOnly MinCallSpacing As TimeSpan = TimeSpan.FromMilliseconds(600)

    Private ReadOnly _storeId As String
    Private ReadOnly _currency As String
    Private ReadOnly _apiKey As String

    Public Sub New(name As String, storeId As String, currency As String, apiKey As String, searchUrlTemplate As String)
        MyBase.New(name, searchUrlTemplate)
        _storeId = storeId
        _currency = currency
        _apiKey = If(apiKey, "").Trim()
    End Sub

    Public Overrides ReadOnly Property IsConfigured As Boolean
        Get
            Return _apiKey.Length > 0
        End Get
    End Property

    Public Overrides Async Function SearchAsync(query As String, mode As SearchMode, maxResults As Integer, offset As Integer,
                                                filters As SupplierFilters, ct As CancellationToken) As Task(Of SearchPage)
        filters = If(filters, SupplierFilters.None)
        ' any: = keyword, manuPartNum: = manufacturer part no., id: = Farnell/CPC/Newark order code
        Dim prefix = If(mode = SearchMode.Keyword, "any:", If(mode = SearchMode.SupplierCode, "id:", "manuPartNum:"))
        Dim term = prefix & query
        ' element14 filters on its side: resultsSettings.refinements.filters=inStock,rohsCompliant
        Dim refine As New List(Of String)
        If filters.InStockOnly Then refine.Add("inStock")
        If filters.RohsOnly Then refine.Add("rohsCompliant")
        ' (The API accepts at least 100 per request - tested - but 50 keeps it in step with the others.)
        Dim url = BaseUrl &
            "?term=" & Uri.EscapeDataString(term) &
            "&storeInfo.id=" & _storeId &
            "&resultsSettings.offset=" & offset.ToString() &
            "&resultsSettings.numberOfResults=" & Math.Min(maxResults, 50).ToString() &
            If(refine.Count > 0, "&resultsSettings.refinements.filters=" & String.Join(",", refine), "") &
            "&resultsSettings.responseGroup=large" &
            "&callInfo.responseDataFormat=json" &
            "&callInfo.apiKey=" & Uri.EscapeDataString(_apiKey)

        Await _rateGate.WaitAsync(ct).ConfigureAwait(False)
        Try
            Dim wait = _lastCallUtc + MinCallSpacing - DateTime.UtcNow
            If wait > TimeSpan.Zero Then Await Task.Delay(wait, ct).ConfigureAwait(False)
            Try
                Using req As New HttpRequestMessage(HttpMethod.Get, url)
                    Dim page = Parse(Await SendForJsonAsync(req, ct).ConfigureAwait(False))
                    If mode = SearchMode.ExactPartNumber Then
                        page = New SearchPage(page.Parts.Where(Function(p) SamePartNumber(p.ManufacturerPartNumber, query)).ToList(), Nothing)
                        page.TotalAvailable = page.Parts.Count
                    End If
                    If filters.InStockOnly AndAlso mode <> SearchMode.Keyword Then
                        page.Parts = page.Parts.Where(Function(p) p.Stock.GetValueOrDefault() > 0).ToList()
                    End If
                    Return page
                End Using
            Finally
                _lastCallUtc = DateTime.UtcNow
            End Try
        Finally
            _rateGate.Release()
        End Try
    End Function

    Private Function Parse(root As JToken) As SearchPage
        Dim fault = TryCast(root.GetPath("Fault"), JObject)
        If fault IsNot Nothing Then
            Throw New SupplierException(Truncate(fault.ToString(Formatting.None), 300))
        End If

        ' The wrapper name depends on the search type, e.g. "keywordSearchReturn",
        ' "manufacturerPartNumberSearchReturn" - so take whichever "...Return" object is there.
        Dim wrapper As JToken = Nothing
        Dim rootObj = TryCast(root, JObject)
        If rootObj IsNot Nothing Then
            For Each prop In rootObj.Properties()
                If prop.Name.EndsWith("Return", StringComparison.OrdinalIgnoreCase) Then
                    wrapper = prop.Value
                    Exit For
                End If
            Next
        End If

        Dim results As New List(Of PartResult)
        For Each p In wrapper.GetArr("products")
            Dim sku = p.GetStr("sku")
            Dim stock = ToNullableLong(p.GetNum("stock", "level"))
            If Not stock.HasValue Then stock = ToNullableLong(p.GetNum("inv"))

            Dim moq = p.GetNum("translatedMinimumOrderQuality")   ' (sic - that's the API's spelling)
            If Not moq.HasValue Then moq = p.GetNum("translatedMinimumOrderQuantity")

            Dim lead = p.GetStr("stock", "leastLeadTime")
            Dim r As New PartResult With {
                .Supplier = Name,
                .SupplierPartNumber = sku,
                .ManufacturerPartNumber = p.GetStr("translatedManufacturerPartNumber"),
                .Manufacturer = If(p.GetStr("brandName").Length > 0, p.GetStr("brandName"), p.GetStr("vendorName")),
                .Description = p.GetStr("displayName"),
                .Category = TypeFromName(p.GetStr("displayName")),
                .Stock = stock,
                .StockInfo = If(lead.Length > 0 AndAlso lead <> "0", $"Lead {lead} days", ""),
                .Currency = _currency,
                .MinOrderQty = ToNullableInt(moq),
                .Status = p.GetStr("productStatus"),
                .ProductUrl = GetSearchUrl(sku)
            }
            For Each pb In p.GetArr("prices")
                Dim qty = ToNullableInt(pb.GetNum("from"))
                Dim cost = pb.GetNum("cost")
                If qty.HasValue AndAlso cost.HasValue Then r.PriceBreaks.Add(New PriceBreak(qty.Value, cost.Value))
            Next
            If r.PriceBreaks.Count > 0 Then r.UnitPrice = r.PriceBreaks.OrderBy(Function(b) b.Quantity).First().Price

            Dim ds = p.GetArr("datasheets").FirstOrDefault()
            r.DatasheetUrl = ds.GetStr("url")

            ' Only the file name comes back, e.g. "/GE8SOIC-40.jpg". Both stores serve
            ' it from the en_GB path (en_US 404s, even on Newark).
            Dim imageFile = p.GetStr("image", "baseName")
            If imageFile.Length > 0 Then
                r.ImageUrl = $"https://{_storeId}/productimages/standard/en_GB{imageFile}"
                r.LargeImageUrl = $"https://{_storeId}/productimages/large/en_GB{imageFile}"
            End If
            results.Add(r)
        Next
        Return New SearchPage(results, ToNullableInt(wrapper.GetNum("numberOfResults")))
    End Function

    ''' <summary>
    ''' element14 sends no category, but every product name reads "MAKER - PART NO - Type, details…",
    ''' e.g. "ANALOG DEVICES - AD7781BRUZ - Analogue to Digital Converter, Delta-Sigma, 20 bit". Returns the type.
    ''' </summary>
    Private Shared Function TypeFromName(displayName As String) As String
        Dim parts = displayName.Split({" - "}, 3, StringSplitOptions.None)
        If parts.Length < 3 Then Return ""
        Return parts(2).Split(","c)(0).Trim()
    End Function

End Class
