Imports System.Globalization
Imports System.Net.Http

''' <summary>
''' Exchange rates for showing every price in one currency: the European Central Bank's daily reference rates,
''' via the free Frankfurter service (no key needed). Rates are "per euro" and saved in settings.json, so they're
''' only fetched again after 12 hours - and the last good rates still work if the PC is offline.
''' The ECB publishes once each working day (about 16:00 CET); it has no Taiwan dollar (TWD) rate.
''' </summary>
Public Class ExchangeRates

    Public Const SourceName As String = "ECB"
    Private Shared ReadOnly Urls As String() = {"https://api.frankfurter.dev/v1/latest", "https://api.frankfurter.app/latest"}

    ''' <summary>How long fetched rates are used before asking for new ones.</summary>
    Public Shared ReadOnly RefreshEvery As TimeSpan = TimeSpan.FromHours(12)

    ''' <summary>Currencies offered in Settings - all have ECB rates.</summary>
    Public Shared ReadOnly Currencies As String() = {
        "GBP", "EUR", "USD", "CAD", "AUD", "NZD", "CHF", "SEK", "NOK", "DKK", "PLN", "CZK", "HUF", "RON",
        "ILS", "ZAR", "JPY", "KRW", "CNY", "HKD", "SGD", "MYR", "THB", "PHP", "INR"}

    ''' <summary>The ECB date the rates are for, e.g. "2026-10-23".</summary>
    Public Property RatesDate As String = ""
    ''' <summary>When the app last fetched them (UTC).</summary>
    Public Property FetchedUtc As DateTime?
    ''' <summary>Units of each currency per 1 euro (EUR itself = 1).</summary>
    Public Property PerEuro As New Dictionary(Of String, Decimal)

    <JsonIgnore> Public ReadOnly Property HasRates As Boolean
        Get
            Return PerEuro IsNot Nothing AndAlso PerEuro.Count > 0
        End Get
    End Property

    ''' <summary>True when there are no rates yet, or they were fetched 12 hours ago or more.</summary>
    <JsonIgnore> Public ReadOnly Property NeedsRefresh As Boolean
        Get
            Return Not HasRates OrElse Not FetchedUtc.HasValue OrElse DateTime.UtcNow - FetchedUtc.Value >= RefreshEvery
        End Get
    End Property

    ''' <summary>How many days old the ECB rates are (a weekend makes them 2-3 days old, which is normal).</summary>
    <JsonIgnore> Public ReadOnly Property AgeDays As Integer
        Get
            Dim d As DateTime
            If Not DateTime.TryParseExact(RatesDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then Return 0
            Return Math.Max(0, (DateTime.Today - d).Days)
        End Get
    End Property

    ''' <summary>e.g. "ECB rates of 23 Oct 2026".</summary>
    Public Function Describe() As String
        Dim d As DateTime
        If DateTime.TryParseExact(RatesDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, d) Then
            Return $"{SourceName} rates of {d:d MMM yyyy}"
        End If
        Return $"{SourceName} rates"
    End Function

    ''' <summary>How many <paramref name="toCur"/> one <paramref name="fromCur"/> buys, or Nothing if either rate is missing.</summary>
    Public Function Rate(fromCur As String, toCur As String) As Decimal?
        If String.IsNullOrEmpty(fromCur) OrElse String.IsNullOrEmpty(toCur) Then Return Nothing
        If String.Equals(fromCur, toCur, StringComparison.OrdinalIgnoreCase) Then Return 1D
        Dim f, t As Decimal
        If Not PerEuro.TryGetValue(fromCur.ToUpperInvariant(), f) OrElse f <= 0 Then Return Nothing
        If Not PerEuro.TryGetValue(toCur.ToUpperInvariant(), t) Then Return Nothing
        Return t / f
    End Function

    ''' <summary>Fetches today's rates. Tries a second address if the first fails; throws if both do.</summary>
    Public Shared Async Function DownloadAsync(ct As CancellationToken) As Task(Of ExchangeRates)
        Dim lastError As Exception = Nothing
        For Each url In Urls
            Try
                Using req As New HttpRequestMessage(HttpMethod.Get, url)
                    Dim j = Await SendForJsonAsync(req, ct).ConfigureAwait(False)
                    Dim baseCur = j.GetStr("base")
                    Dim r As New ExchangeRates With {.RatesDate = j.GetStr("date"), .FetchedUtc = DateTime.UtcNow}
                    Dim rates = TryCast(j.GetPath("rates"), JObject)
                    If rates Is Nothing OrElse Not String.Equals(baseCur, "EUR", StringComparison.OrdinalIgnoreCase) Then
                        Throw New SupplierException("Unexpected exchange-rate reply.")
                    End If
                    For Each prop In rates.Properties()
                        Dim v = rates.GetNum(prop.Name)
                        If v.HasValue AndAlso v.Value > 0 Then r.PerEuro(prop.Name.ToUpperInvariant()) = v.Value
                    Next
                    r.PerEuro("EUR") = 1D
                    Return r
                End Using
            Catch ex As OperationCanceledException When ct.IsCancellationRequested
                Throw
            Catch ex As Exception
                lastError = ex
            End Try
        Next
        Throw New SupplierException("Couldn't get exchange rates: " & lastError?.Message)
    End Function

    ''' <summary>The currency Windows uses for this PC's region, if it's one we have rates for; else GBP.</summary>
    Public Shared Function DefaultCurrency() As String
        Try
            Dim cur = New RegionInfo(CultureInfo.CurrentCulture.Name).ISOCurrencySymbol
            If Currencies.Contains(cur) Then Return cur
        Catch
            ' e.g. a neutral culture with no region
        End Try
        Return "GBP"
    End Function

End Class
