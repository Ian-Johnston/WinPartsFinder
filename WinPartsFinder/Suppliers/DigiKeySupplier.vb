Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text

''' <summary>
''' DigiKey Product Information API v4 (OAuth2 client-credentials / "2-legged").
''' Create an app at https://developer.digikey.com/ and subscribe it to "ProductInformation V4".
''' </summary>
Public Class DigiKeySupplier
    Inherits ApiSupplier

    Private Const TokenUrl As String = "https://api.digikey.com/v1/oauth2/token"
    Private Const SearchUrl As String = "https://api.digikey.com/products/v4/search/keyword"

    Private ReadOnly _clientId As String
    Private ReadOnly _clientSecret As String
    Private ReadOnly _site As String
    Private ReadOnly _language As String
    Private ReadOnly _currency As String

    Private _token As String
    Private _tokenExpiresUtc As DateTime = DateTime.MinValue
    Private ReadOnly _tokenLock As New SemaphoreSlim(1, 1)

    Public Sub New(clientId As String, clientSecret As String, site As String, language As String,
                   currency As String, searchUrlTemplate As String)
        MyBase.New(AppSettings.DigiKey, searchUrlTemplate)
        _clientId = If(clientId, "").Trim()
        _clientSecret = If(clientSecret, "").Trim()
        _site = If(String.IsNullOrWhiteSpace(site), "UK", site.Trim())
        _language = If(String.IsNullOrWhiteSpace(language), "en", language.Trim())
        _currency = If(String.IsNullOrWhiteSpace(currency), "GBP", currency.Trim())
    End Sub

    Public Overrides ReadOnly Property IsConfigured As Boolean
        Get
            Return _clientId.Length > 0 AndAlso _clientSecret.Length > 0
        End Get
    End Property

    Private Async Function GetTokenAsync(ct As CancellationToken) As Task(Of String)
        Await _tokenLock.WaitAsync(ct).ConfigureAwait(False)
        Try
            If _token IsNot Nothing AndAlso DateTime.UtcNow < _tokenExpiresUtc Then Return _token

            Dim form As New Dictionary(Of String, String) From {
                {"client_id", _clientId},
                {"client_secret", _clientSecret},
                {"grant_type", "client_credentials"}
            }
            Using req As New HttpRequestMessage(HttpMethod.Post, TokenUrl) With {
                .Content = New FormUrlEncodedContent(form)
            }
                Using doc = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
                    _token = doc.RootElement.GetStr("access_token")
                    If _token.Length = 0 Then Throw New SupplierException("DigiKey did not return an access token.")
                    Dim expires = doc.RootElement.GetNum("expires_in").GetValueOrDefault(600D)
                    _tokenExpiresUtc = DateTime.UtcNow.AddSeconds(CDbl(expires) - 60)
                    Return _token
                End Using
            End Using
        Finally
            _tokenLock.Release()
        End Try
    End Function

    Public Overrides Async Function SearchAsync(query As String, mode As SearchMode, maxResults As Integer, offset As Integer,
                                                ct As CancellationToken) As Task(Of SearchPage)
        Dim token = Await GetTokenAsync(ct).ConfigureAwait(False)
        Dim payload = New With {.Keywords = query, .Limit = Math.Min(maxResults, 50), .Offset = offset}

        Using req As New HttpRequestMessage(HttpMethod.Post, SearchUrl) With {
            .Content = New StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        }
            req.Headers.Authorization = New AuthenticationHeaderValue("Bearer", token)
            req.Headers.Add("X-DIGIKEY-Client-Id", _clientId)
            req.Headers.Add("X-DIGIKEY-Locale-Site", _site)
            req.Headers.Add("X-DIGIKEY-Locale-Language", _language)
            req.Headers.Add("X-DIGIKEY-Locale-Currency", _currency)

            Try
                Using doc = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
                    Dim parts = Parse(doc.RootElement, mode, query, includeExact:=offset = 0)
                    Return New SearchPage(parts, ToNullableInt(doc.RootElement.GetNum("ProductsCount")))
                End Using
            Catch ex As SupplierException When ex.Message.StartsWith("HTTP 401")
                _token = Nothing ' force a fresh token next time
                Throw
            End Try
        End Using
    End Function

    Private Function Parse(root As JsonElement, mode As SearchMode, query As String, includeExact As Boolean) As List(Of PartResult)
        Dim results As New List(Of PartResult)
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        ' Exact matches first (they come back on every page, so only use them on the first), then the page of results.
        Dim exact = If(includeExact, root.GetArr("ExactMatches"), New List(Of JsonElement))
        For Each p In exact.Concat(root.GetArr("Products"))
            Dim mpn = p.GetStr("ManufacturerProductNumber")
            If mode = SearchMode.PartNumber AndAlso
               Not mpn.StartsWith(query, StringComparison.OrdinalIgnoreCase) Then Continue For

            ' A product has several packagings (reel, cut tape, Digi-Reel...).
            ' Pick the one with the smallest minimum order - the useful one for repairs.
            Dim variation = p.GetArr("ProductVariations").
                OrderBy(Function(v) v.GetNum("MinimumOrderQuantity").GetValueOrDefault(Decimal.MaxValue)).
                FirstOrDefault()

            Dim dkPn = variation.GetStr("DigiKeyProductNumber")
            If Not seen.Add(mpn & "|" & dkPn) Then Continue For

            Dim r As New PartResult With {
                .Supplier = Name,
                .SupplierPartNumber = dkPn,
                .ManufacturerPartNumber = mpn,
                .Manufacturer = p.GetStr("Manufacturer", "Name"),
                .Description = p.GetStr("Description", "ProductDescription"),
                .Stock = ToNullableLong(p.GetNum("QuantityAvailable")),
                .Currency = _currency,
                .MinOrderQty = ToNullableInt(variation.GetNum("MinimumOrderQuantity")),
                .Status = p.GetStr("ProductStatus", "Status"),
                .ProductUrl = p.GetStr("ProductUrl"),
                .DatasheetUrl = p.GetStr("DatasheetUrl"),
                .ImageUrl = p.GetStr("PhotoUrl")
            }
            Dim pkg = variation.GetStr("PackageType", "Name")
            Dim lead = p.GetStr("ManufacturerLeadWeeks")
            r.StockInfo = String.Join(" | ", {pkg, If(lead.Length > 0, $"Lead {lead} wks", "")}.Where(Function(x) x.Length > 0))

            For Each pb In variation.GetArr("StandardPricing")
                Dim qty = ToNullableInt(pb.GetNum("BreakQuantity"))
                Dim price = pb.GetNum("UnitPrice")
                If qty.HasValue AndAlso price.HasValue Then r.PriceBreaks.Add(New PriceBreak(qty.Value, price.Value))
            Next
            r.UnitPrice = If(r.PriceBreaks.Count > 0,
                             r.PriceBreaks.OrderBy(Function(b) b.Quantity).First().Price,
                             p.GetNum("UnitPrice"))
            ' (No cap here: dropping any of this page's products would lose them for good,
            ' because Load more carries on from the next offset.)
            results.Add(r)
        Next
        Return results
    End Function

End Class
