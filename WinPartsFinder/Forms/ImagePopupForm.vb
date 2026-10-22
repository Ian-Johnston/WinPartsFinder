''' <summary>Small always-on-top pop-up showing a bigger product photo. Closes on click, Esc or its X button.</summary>
Public Class ImagePopupForm

    Private Const MaxPhotoSide As Integer = 600
    Private Const MinWidth As Integer = 260

    Private ReadOnly _urls As List(Of String)

    ' Parameterless constructor for the WinForms designer.
    Public Sub New()
        Me.New("", "", "Photo", "")
    End Sub

    ''' <param name="largeUrl">Preferred (bigger) image.</param>
    ''' <param name="fallbackUrl">Used if the large one can't be loaded.</param>
    Public Sub New(largeUrl As String, fallbackUrl As String, title As String, caption As String)
        InitializeComponent()
        _urls = {largeUrl, fallbackUrl}.Where(Function(u) Not String.IsNullOrEmpty(u)).Distinct().ToList()
        Text = title
        lblCaption.Text = caption
    End Sub

    Private Async Sub ImagePopupForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Theme.Apply(Me) ' the photo area itself stays white - product shots have white backgrounds
        KeepOnScreen()
        Dim img As Image = Nothing
        For Each url In _urls
            Try
                img = Await ImageCache.DownloadImageAsync(url, CancellationToken.None)
                Exit For
            Catch
                ' try the next one
            End Try
        Next

        If IsDisposed Then
            img?.Dispose()
            Return
        End If
        If img Is Nothing Then
            lblLoading.Text = "Photo not available"
            Return
        End If

        picPhoto.Image = img
        picPhoto.Visible = True
        lblLoading.Visible = False

        ' Size the window to the photo (shrinking big ones), then make sure it still fits on screen.
        Dim scale = Math.Min(1.0, MaxPhotoSide / Math.Max(img.Width, img.Height))
        ClientSize = New Size(Math.Max(MinWidth, CInt(img.Width * scale)),
                              CInt(img.Height * scale) + lblCaption.Height)
        KeepOnScreen()
    End Sub

    Private Sub KeepOnScreen()
        Dim area = Screen.FromPoint(Location).WorkingArea
        Left = Math.Max(area.Left, Math.Min(Left, area.Right - Width))
        Top = Math.Max(area.Top, Math.Min(Top, area.Bottom - Height))
    End Sub

    Private Sub CloseOnClick(sender As Object, e As EventArgs) Handles picPhoto.Click, lblLoading.Click, lblCaption.Click
        Close()
    End Sub

    Private Sub ImagePopupForm_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then Close()
    End Sub

    Private Sub ImagePopupForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        picPhoto.Image?.Dispose()
    End Sub

End Class
