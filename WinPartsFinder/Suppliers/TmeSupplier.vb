Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text

''' <summary>
''' TME (Transfer Multisort Elektronik) Product API v2 - https://developers.tme.eu
''' Login: OAuth2 client credentials (Token + Application secret) gives a 5-minute access token.
''' A search is /products/search (parts, photos, total) then /products/data (prices + stock) for that page.
''' Getting the Token needs the temporary token from the tme.eu shop account - see Help.
''' </summary>
Public Class TmeSupplier
    Inherits ApiSupplier

    Private Const BaseUrl As String = "https://api.tme.eu"
    Private Const MaxPerPage As Integer = 50 ' /products/data takes up to 50 symbols

    Private ReadOnly _token As String
    Private ReadOnly _secret As String
    Private ReadOnly _country As String
    Private ReadOnly _currency As String

    Private _accessToken As String
    Private _accessExpiresUtc As DateTime = DateTime.MinValue
    Private ReadOnly _authLock As New SemaphoreSlim(1, 1)

    Public Sub New(token As String, secret As String, country As String, currency As String, searchUrlTemplate As String)
        MyBase.New(AppSettings.TME, searchUrlTemplate)
        _token = If(token, "").Trim()
        _secret = If(secret, "").Trim()
        _country = If(String.IsNullOrWhiteSpace(country), "GB", country.Trim().ToUpperInvariant())
        _currency = If(String.IsNullOrWhiteSpace(currency), "GBP", currency.Trim().ToUpperInvariant())
    End Sub

    Public Overrides ReadOnly Property IsConfigured As Boolean
        Get
            Return _token.Length > 0 AndAlso _secret.Length > 0
        End Get
    End Property

    Private Async Function GetAccessTokenAsync(ct As CancellationToken) As Task(Of String)
        Await _authLock.WaitAsync(ct).ConfigureAwait(False)
        Try
            If _accessToken IsNot Nothing AndAlso DateTime.UtcNow < _accessExpiresUtc Then Return _accessToken

            Using req As New HttpRequestMessage(HttpMethod.Post, BaseUrl & "/auth/token") With {
                .Content = New FormUrlEncodedContent(New Dictionary(Of String, String) From {{"grant_type", "client_credentials"}})
            }
                Dim basic = Convert.ToBase64String(Encoding.UTF8.GetBytes(_token & ":" & _secret))
                req.Headers.Authorization = New AuthenticationHeaderValue("Basic", basic)
                Dim root = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
                _accessToken = root.GetStr("access_token")
                If _accessToken.Length = 0 Then Throw New SupplierException("TME did not return an access token.")
                Dim expires = root.GetNum("expires_in").GetValueOrDefault(300D)
                _accessExpiresUtc = DateTime.UtcNow.AddSeconds(CDbl(expires) - 30)
                Return _accessToken
            End Using
        Finally
            _authLock.Release()
        End Try
    End Function

    ''' <summary>GET with the Bearer token; retries once with a fresh token if TME says it expired.</summary>
    Private Async Function GetJsonAsync(pathAndQuery As String, ct As CancellationToken) As Task(Of JToken)
        For attempt = 1 To 2
            Dim access = Await GetAccessTokenAsync(ct).ConfigureAwait(False)
            Using req As New HttpRequestMessage(HttpMethod.Get, BaseUrl & pathAndQuery)
                req.Headers.Authorization = New AuthenticationHeaderValue("Bearer", access)
                req.Headers.AcceptLanguage.ParseAdd("en")
                Try
                    Dim root = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
                    Dim status = root.GetStr("status")
                    If status.Length > 0 AndAlso Not status.Equals("OK", StringComparison.OrdinalIgnoreCase) Then
                        Throw New SupplierException($"TME: {status} {Truncate(root.ToString(Formatting.None), 250)}")
                    End If
                    Return root
                Catch ex As SupplierException When attempt = 1 AndAlso ex.Message.StartsWith("HTTP 401")
                    _accessToken = Nothing ' expired early - log in again and retry once
                End Try
            End Using
        Next
        Throw New SupplierException("TME login failed.")
    End Function

    Private Shared Function Q(name As String, value As String) As String
        Return Uri.EscapeDataString(name) & "=" & Uri.EscapeDataString(value)
    End Function

    Public Overrides Async Function SearchAsync(query As String, mode As SearchMode, maxResults As Integer, offset As Integer,
                                                filters As SupplierFilters, ct As CancellationToken) As Task(Of SearchPage)
        Dim limit = Math.Max(1, Math.Min(maxResults, MaxPerPage))
        Dim page = offset \ limit + 1 ' TME pages are numbered from 1

        ' 1) Search: products (symbol, manufacturer, description, photos...) and the total count.
        Dim phrase = If(query.Length > 40, query.Substring(0, 40), query) ' TME allows 2-40 characters
        Dim searchParams As New List(Of String) From {
            Q("phrase", phrase), Q("scope[]", "products"), Q("scope[]", "counters"),
            Q("country", _country), Q("limit", limit.ToString()), Q("page", page.ToString())}
        ' TME filters stock on its side. (Its search has no RoHS option, so that one is ignored for TME.)
        If filters IsNot Nothing AndAlso filters.InStockOnly Then searchParams.Add(Q("filter[in_stock]", "true"))
        Dim searchQuery = String.Join("&", searchParams)
        Dim found = Await GetJsonAsync("/products/search?" & searchQuery, ct).ConfigureAwait(False)

        Dim data = found.GetPath("data")
        Dim products = data.GetArr("products", "elements")
        Dim total = ToNullableInt(data.GetNum("counters", "count"))

        Dim results As New List(Of PartResult)
        Dim bySymbol As New Dictionary(Of String, PartResult)(StringComparer.OrdinalIgnoreCase)
        For Each p In products
            Dim symbol = p.GetStr("symbol")
            If symbol.Length = 0 Then Continue For
            Dim mpn = FirstText(p.GetPath("manufacturer_symbols"))
            If mode = SearchMode.PartNumber AndAlso mpn.Length > 0 AndAlso
               Not mpn.StartsWith(query, StringComparison.OrdinalIgnoreCase) Then Continue For
            If mode = SearchMode.ExactPartNumber AndAlso Not SamePartNumber(mpn, query) Then Continue For
            If mode = SearchMode.SupplierCode AndAlso Not SamePartNumber(symbol, query) Then Continue For

            Dim r As New PartResult With {
                .Supplier = Name,
                .SupplierPartNumber = symbol,
                .ManufacturerPartNumber = If(mpn.Length > 0, mpn, symbol),
                .Manufacturer = p.GetStr("manufacturer", "name"),
                .Description = p.GetStr("description"),
                .Currency = _currency,
                .MinOrderQty = ToNullableInt(p.GetNum("minimal_amount")),
                .Status = FirstText(p.GetPath("product_status")),
                .ProductUrl = GetSearchUrl(symbol),
                .ImageUrl = FixUrl(p.GetStr("assets", "primary_photo", "prime")),
                .LargeImageUrl = FixUrl(p.GetStr("assets", "primary_photo", "high_resolution"))
            }
            If r.ImageUrl.Length = 0 Then r.ImageUrl = FixUrl(p.GetStr("assets", "primary_photo", "thumbnail"))
            results.Add(r)
            bySymbol(symbol) = r
        Next

        ' 2) Prices and stock for this page's parts, in one call.
        If bySymbol.Count > 0 Then
            Dim dataQuery = String.Join("&",
                {Q("scope[]", "prices"), Q("scope[]", "stock"), Q("country", _country), Q("currency", _currency)}.
                Concat(bySymbol.Keys.Select(Function(sym) Q("symbols[]", sym))))
            Dim pricing = Await GetJsonAsync("/products/data?" & dataQuery, ct).ConfigureAwait(False)
            ApplyPricesAndStock(pricing, bySymbol)
        End If

        If mode = SearchMode.ExactPartNumber OrElse mode = SearchMode.SupplierCode Then total = results.Count
        If filters IsNot Nothing AndAlso filters.InStockOnly Then
            results = results.Where(Function(r) r.Stock.GetValueOrDefault() > 0).ToList() ' belt and braces
        End If
        Return New SearchPage(results, total)
    End Function

    ''' <summary>Fills in stock and price breaks from /products/data.</summary>
    Private Sub ApplyPricesAndStock(pricing As JToken, bySymbol As Dictionary(Of String, PartResult))
        Dim elements = pricing.GetArr("data", "elements")
        If elements.Count = 0 Then elements = pricing.GetArr("data", "products", "elements")
        For Each el In elements
            Dim r As PartResult = Nothing
            If Not bySymbol.TryGetValue(el.GetStr("symbol"), r) Then Continue For

            Dim stock = el.GetNum("stock_quantity")
            If Not stock.HasValue Then stock = el.GetNum("stock", "quantity")
            If Not stock.HasValue Then stock = el.GetNum("stock", "stock_quantity")
            r.Stock = ToNullableLong(stock)

            Dim prices = el.GetPath("prices")
            Dim cur = prices.GetStr("currency")
            If cur.Length > 0 Then r.Currency = cur
            Dim breaks = prices.GetArr("elements")
            If breaks.Count = 0 Then breaks = prices.GetArr("price_list")
            For Each pb In breaks
                Dim qty = ToNullableInt(pb.GetNum("amount"))
                Dim price = pb.GetNum("price")
                If Not price.HasValue Then price = pb.GetNum("price_value")
                If qty.HasValue AndAlso price.HasValue Then r.PriceBreaks.Add(New PriceBreak(qty.Value, price.Value))
            Next
            If r.PriceBreaks.Count > 0 Then r.UnitPrice = r.PriceBreaks.OrderBy(Function(b) b.Quantity).First().Price
            Dim priceType = prices.GetStr("type")
            If priceType.Length > 0 Then r.StockInfo = $"Prices {priceType.ToLowerInvariant()}" ' e.g. "net" (ex VAT)
        Next
    End Sub

    ''' <summary>First text in a value that may be a string, an array of strings, or an array of objects.</summary>
    Private Shared Function FirstText(t As JToken) As String
        If t Is Nothing Then Return ""
        Select Case t.Type
            Case JTokenType.String
                Return CStr(DirectCast(t, JValue).Value).Trim()
            Case JTokenType.Array
                For Each item In DirectCast(t, JArray)
                    Dim s = If(item.Type = JTokenType.String, CStr(DirectCast(item, JValue).Value),
                               If(item.GetStr("symbol").Length > 0, item.GetStr("symbol"),
                                  If(item.GetStr("name").Length > 0, item.GetStr("name"), item.GetStr("value"))))
                    If Not String.IsNullOrWhiteSpace(s) Then Return s.Trim()
                Next
        End Select
        Return ""
    End Function

    Private Shared Function FixUrl(url As String) As String
        If url.StartsWith("//") Then Return "https:" & url
        Return url
    End Function

End Class
