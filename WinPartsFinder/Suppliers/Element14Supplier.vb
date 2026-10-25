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

    ''' <summary>One element14 country store: same API key, its own stock and prices.</summary>
    Public Class Store
        Public ReadOnly Property Id As String           ' e.g. "nz.element14.com"
        Public ReadOnly Property Country As String
        Public ReadOnly Property Currency As String     ' the API doesn't say, so it's listed here (checked against live prices)
        Public ReadOnly Property ImageLocale As String  ' photo folder: en_GB on most stores, en_US on AU/NZ

        Public Sub New(id As String, country As String, currency As String, Optional imageLocale As String = "en_GB")
            Me.Id = id : Me.Country = country : Me.Currency = currency : Me.ImageLocale = imageLocale
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Country} - {Id} ({Currency})"
        End Function
    End Class

    ''' <summary>The stores the Farnell supplier can use - each one tested live with a search and a photo (Oct 2026).
    ''' Taiwan is left out (no ECB exchange rate for TWD), and Indonesia (the API rejects it).</summary>
    Public Shared ReadOnly FarnellStores As New List(Of Store) From {
        New Store("uk.farnell.com", "United Kingdom", "GBP"),
        New Store("ie.farnell.com", "Ireland", "EUR"),
        New Store("de.farnell.com", "Germany", "EUR"),
        New Store("fr.farnell.com", "France", "EUR"),
        New Store("it.farnell.com", "Italy", "EUR"),
        New Store("es.farnell.com", "Spain", "EUR"),
        New Store("nl.farnell.com", "Netherlands", "EUR"),
        New Store("be.farnell.com", "Belgium", "EUR"),
        New Store("at.farnell.com", "Austria", "EUR"),
        New Store("pt.farnell.com", "Portugal", "EUR"),
        New Store("fi.farnell.com", "Finland", "EUR"),
        New Store("sk.farnell.com", "Slovakia", "EUR"),
        New Store("si.farnell.com", "Slovenia", "EUR"),
        New Store("ee.farnell.com", "Estonia", "EUR"),
        New Store("lv.farnell.com", "Latvia", "EUR"),
        New Store("lt.farnell.com", "Lithuania", "EUR"),
        New Store("bg.farnell.com", "Bulgaria", "EUR"),
        New Store("ch.farnell.com", "Switzerland", "CHF"),
        New Store("se.farnell.com", "Sweden", "SEK"),
        New Store("dk.farnell.com", "Denmark", "DKK"),
        New Store("no.farnell.com", "Norway", "NOK"),
        New Store("pl.farnell.com", "Poland", "PLN"),
        New Store("cz.farnell.com", "Czech Republic", "CZK"),
        New Store("hu.farnell.com", "Hungary", "HUF"),
        New Store("ro.farnell.com", "Romania", "RON"),
        New Store("il.farnell.com", "Israel", "USD"),
        New Store("tr.farnell.com", "Turkey", "EUR"),
        New Store("export.farnell.com", "Export (other countries)", "GBP"),
        New Store("au.element14.com", "Australia", "AUD", "en_US"),
        New Store("nz.element14.com", "New Zealand", "NZD", "en_US"),
        New Store("sg.element14.com", "Singapore", "SGD"),
        New Store("my.element14.com", "Malaysia", "MYR"),
        New Store("in.element14.com", "India", "INR"),
        New Store("cn.element14.com", "China", "CNY"),
        New Store("hk.element14.com", "Hong Kong", "HKD"),
        New Store("kr.element14.com", "South Korea", "KRW"),
        New Store("th.element14.com", "Thailand", "THB"),
        New Store("ph.element14.com", "Philippines", "PHP"),
        New Store("vn.element14.com", "Vietnam", "USD")
    }

    ''' <summary>The store with this id, or the UK one if it isn't known.</summary>
    Public Shared Function FindFarnellStore(id As String) As Store
        Return If(FarnellStores.FirstOrDefault(Function(s) String.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase)), FarnellStores(0))
    End Function

    Private ReadOnly _storeId As String
    Private ReadOnly _currency As String
    Private ReadOnly _imageLocale As String
    Private ReadOnly _apiKey As String

    Public Sub New(name As String, storeId As String, currency As String, apiKey As String, searchUrlTemplate As String,
                   Optional imageLocale As String = "en_GB")
        MyBase.New(name, searchUrlTemplate)
        _storeId = storeId
        _currency = currency
        _imageLocale = imageLocale
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

            ' Only the file name comes back, e.g. "/GE8SOIC-40.jpg". Most stores (Newark too) serve it from
            ' the en_GB folder; Australia and New Zealand only from en_US.
            Dim imageFile = p.GetStr("image", "baseName")
            If imageFile.Length > 0 Then
                r.ImageUrl = $"https://{_storeId}/productimages/standard/{_imageLocale}{imageFile}"
                r.LargeImageUrl = $"https://{_storeId}/productimages/large/{_imageLocale}{imageFile}"
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
