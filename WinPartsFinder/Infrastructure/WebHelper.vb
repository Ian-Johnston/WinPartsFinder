Imports System.Net
Imports System.Net.Http

Public Class SupplierException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class

''' <summary>One shared HttpClient for the whole app (the recommended pattern).</summary>
Public Module WebHelper

    Public ReadOnly Client As HttpClient = CreateClient()

    Private Function CreateClient() As HttpClient
        ' (On .NET Framework 4.8 the secure-connection (TLS) version comes from Windows, which is what we want.)
        Dim handler As New HttpClientHandler With {
            .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
        }
        Dim c As New HttpClient(handler) With {.Timeout = TimeSpan.FromSeconds(30)}
        c.DefaultRequestHeaders.UserAgent.ParseAdd("WinPartsFinder/1.0")
        c.DefaultRequestHeaders.Accept.ParseAdd("application/json")
        Return c
    End Function

    ''' <summary>Sends the request and parses the JSON body; throws SupplierException on HTTP errors.</summary>
    Public Async Function SendForJsonAsync(request As HttpRequestMessage, ct As CancellationToken) As Task(Of JToken)
        Using response = Await Client.SendAsync(request, ct).ConfigureAwait(False)
            Dim body = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
            If Not response.IsSuccessStatusCode Then
                Throw New SupplierException($"HTTP {CInt(response.StatusCode)} {response.ReasonPhrase}: {Truncate(body, 300)}")
            End If
            Try
                Return ParseJson(body)
            Catch ex As JsonException
                Throw New SupplierException("Unexpected (non-JSON) response: " & Truncate(body, 200))
            End Try
        End Using
    End Function

    Public Sub OpenInBrowser(url As String)
        If String.IsNullOrWhiteSpace(url) Then Return
        Process.Start(New ProcessStartInfo(url) With {.UseShellExecute = True})
    End Sub

End Module
