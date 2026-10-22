Public Class AboutForm

    Private Const DonateUrl As String =
        "https://www.paypal.com/donate/?business=ian%40ianj.net&no_recurring=0&item_name=Support+WinPartsFinder&currency_code=GBP"

    ' Windows pads big text more than small text, which pushed "WinPartsFinder" right of the text below it.
    ' So the title is drawn here without that padding, offset to match the small labels' padding.
    Private _titleText As String
    Private Const TitleInkOffset As Integer = 1

    Private Sub SetUpTitle()
        _titleText = lblTitle.Text
        Dim sz = TextRenderer.MeasureText(_titleText, lblTitle.Font, Size.Empty, TextFormatFlags.NoPadding)
        lblTitle.AutoSize = False
        lblTitle.Size = New Size(sz.Width + TitleInkOffset + 2, sz.Height)
        lblTitle.Text = ""
    End Sub

    ''' <summary>Puts "(UK 2026)" on the same baseline as the big title, at any font size or DPI.</summary>
    Private Sub AlignVersionBaseline()
        Using g = lblTitle.CreateGraphics()
            ' Both texts are drawn from the top of their labels, so the gap between baselines
            ' is just the difference in the fonts' ascents.
            ' (+1: Windows snaps the big font's baseline a pixel lower than the maths says - checked on a render.)
            Dim drop = AscentPixels(lblTitle.Font, g) - AscentPixels(lblVersion.Font, g)
            lblVersion.Margin = New Padding(lblVersion.Margin.Left, lblTitle.Margin.Top + CInt(Math.Round(drop)) + 1, 0, 0)
        End Using
    End Sub

    Private Shared Function AscentPixels(f As Font, g As Graphics) As Single
        Dim ff = f.FontFamily
        Return f.SizeInPoints * g.DpiY / 72.0F * ff.GetCellAscent(f.Style) / ff.GetEmHeight(f.Style)
    End Function

    Private Sub lblTitle_Paint(sender As Object, e As PaintEventArgs) Handles lblTitle.Paint
        TextRenderer.DrawText(e.Graphics, _titleText, lblTitle.Font, New Point(TitleInkOffset, 0), lblTitle.ForeColor,
                              TextFormatFlags.NoPadding)
    End Sub

    Private Sub AboutForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetUpTitle()
        AlignVersionBaseline()

        Using s = GetType(AboutForm).Assembly.GetManifestResourceStream("WinPartsFinder.AboutCartoon.png")
            If s IsNot Nothing Then
                Using decoded = Image.FromStream(s)
                    picCartoon.Image = New Bitmap(decoded) ' copy, so the stream can be closed
                End Using
            End If
        End Using

        Theme.Apply(Me)
    End Sub

    Private Sub Link_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
        Handles lnkWebsite.LinkClicked, lnkYouTube.LinkClicked, lnkTwitter.LinkClicked
        WebHelper.OpenInBrowser(CStr(DirectCast(sender, LinkLabel).Tag))
    End Sub

    Private Sub btnDonate_Click(sender As Object, e As EventArgs) Handles btnDonate.Click
        WebHelper.OpenInBrowser(DonateUrl)
    End Sub

    Private Sub AboutForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        picCartoon.Image?.Dispose()
    End Sub

End Class
