Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Net.Http

''' <summary>
''' Downloads product photos and keeps 48x48 thumbnails in memory for the session.
''' GetThumbnail must be called on the UI thread; ThumbnailReady is raised there too.
''' </summary>
Public Class ImageCache

    Public Const ThumbSize As Integer = 48

    Public Shared ReadOnly NoImage As Image = MakePlaceholder(drawCross:=True)
    Public Shared ReadOnly LoadingImage As Image = MakePlaceholder(drawCross:=False)

    ' Be polite to the image servers: a few downloads at a time.
    Private Shared ReadOnly _downloadGate As New SemaphoreSlim(4, 4)

    Private ReadOnly _thumbs As New Dictionary(Of String, Image)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _failed As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _pending As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    ''' <summary>Raised (on the UI thread) whenever a thumbnail finishes loading or fails.</summary>
    Public Event ThumbnailReady As EventHandler

    ''' <summary>Returns the thumbnail if cached, otherwise starts a download and returns a placeholder.</summary>
    Public Function GetThumbnail(url As String) As Image
        If String.IsNullOrEmpty(url) OrElse _failed.Contains(url) Then Return NoImage
        Dim img As Image = Nothing
        If _thumbs.TryGetValue(url, img) Then Return img
        If _pending.Add(url) Then LoadThumbnail(url)
        Return LoadingImage
    End Function

    Public Function HasFailed(url As String) As Boolean
        Return _failed.Contains(url)
    End Function

    Private Async Sub LoadThumbnail(url As String)
        Dim thumb As Image = Nothing
        Try
            Using full = Await DownloadImageAsync(url, CancellationToken.None)
                thumb = MakeThumbnail(full)
            End Using
        Catch
            ' Bad URL, 404, not an image... just show the "no image" box.
        End Try
        _pending.Remove(url)
        If thumb Is Nothing Then _failed.Add(url) Else _thumbs(url) = thumb
        RaiseEvent ThumbnailReady(Me, EventArgs.Empty)
    End Sub

    ''' <summary>Downloads an image. Throws if the server sends something that isn't an image.</summary>
    Public Shared Async Function DownloadImageAsync(url As String, ct As CancellationToken) As Task(Of Image)
        Await _downloadGate.WaitAsync(ct).ConfigureAwait(False)
        Try
            Using req As New HttpRequestMessage(HttpMethod.Get, url)
                ' Only formats GDI+ can decode. "image/*" makes some servers (Farnell) send WebP.
                req.Headers.Accept.ParseAdd("image/jpeg, image/png, image/gif, image/bmp")
                Using resp = Await WebHelper.Client.SendAsync(req, ct).ConfigureAwait(False)
                    resp.EnsureSuccessStatusCode()
                    Dim mediaType = If(resp.Content.Headers.ContentType?.MediaType, "")
                    If Not mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidDataException($"Not an image ({mediaType}).")
                    End If
                    Dim bytes = Await resp.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(False)
                    Using ms As New MemoryStream(bytes), decoded = Image.FromStream(ms)
                        Return New Bitmap(decoded) ' copy, so the stream can be closed
                    End Using
                End Using
            End Using
        Finally
            _downloadGate.Release()
        End Try
    End Function

    Private Shared Function MakeThumbnail(source As Image) As Image
        Dim bmp As New Bitmap(ThumbSize, ThumbSize)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.SmoothingMode = SmoothingMode.HighQuality
            Dim inner = ThumbSize - 4
            Dim scale = Math.Min(inner / source.Width, inner / source.Height)
            Dim w = CInt(source.Width * scale)
            Dim h = CInt(source.Height * scale)
            g.DrawImage(source, (ThumbSize - w) \ 2, (ThumbSize - h) \ 2, w, h)
            g.DrawRectangle(Pens.Gainsboro, 0, 0, ThumbSize - 1, ThumbSize - 1)
        End Using
        Return bmp
    End Function

    Private Shared Function MakePlaceholder(drawCross As Boolean) As Image
        Dim bmp As New Bitmap(ThumbSize, ThumbSize)
        Using g = Graphics.FromImage(bmp)
            g.Clear(Color.White)
            g.DrawRectangle(Pens.Silver, 0, 0, ThumbSize - 1, ThumbSize - 1)
            If drawCross Then
                g.SmoothingMode = SmoothingMode.AntiAlias
                Using p As New Pen(Color.LightGray, 1.5F)
                    g.DrawLine(p, 12, 12, ThumbSize - 13, ThumbSize - 13)
                    g.DrawLine(p, ThumbSize - 13, 12, 12, ThumbSize - 13)
                End Using
            End If
        End Using
        Return bmp
    End Function

End Class
