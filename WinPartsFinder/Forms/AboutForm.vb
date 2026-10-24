Public Class AboutForm

    Private Const DonateUrl As String =
        "https://www.paypal.com/donate/?business=ian%40ianj.net&no_recurring=0&item_name=Support+WinPartsFinder&currency_code=GBP"

    ' Windows pads big text more than small text, which pushed "WinPartsFinder" right of the text below it.
    ' So the title is drawn here without that padding, offset to match the small labels' padding.
    Private _titleText As String
    Private Const TitleInkOffset As Integer = 1

    ' A light ™ tucked up against the top of the title (a bold one at title size is far too heavy).
    Private Const TradeMark As String = "™"
    Private _tmFont As Font
    Private _titleInk As Rectangle   ' where the title's letters actually are, drawn at (0,0)
    Private _tmPos As Point

    ' Positions come from where the letters are really drawn (not font maths), so they line up at any DPI.
    Private Sub SetUpTitle()
        _titleText = lblTitle.Text
        _tmFont = New Font(lblTitle.Font.FontFamily, lblTitle.Font.Size * 0.55F, FontStyle.Regular)
        Dim sz = TextRenderer.MeasureText(_titleText, lblTitle.Font, Size.Empty, TextFormatFlags.NoPadding)
        Dim tmSize = TextRenderer.MeasureText(TradeMark, _tmFont, Size.Empty, TextFormatFlags.NoPadding)
        _titleInk = TextInk(_titleText, lblTitle.Font)
        Dim tmInk = TextInk(TradeMark, _tmFont)
        ' Just after the last letter, with the top of the ™ level with the top of the capital W.
        _tmPos = New Point(TitleInkOffset + _titleInk.Right + 2, Math.Max(0, _titleInk.Top - tmInk.Top))
        lblTitle.AutoSize = False
        lblTitle.Size = New Size(_tmPos.X + tmSize.Width + 2, sz.Height)
        lblTitle.Text = ""
    End Sub

    ''' <summary>Where text's letters land when drawn at (0,0) - measured sizes include spare space around them.</summary>
    Private Shared Function TextInk(text As String, font As Font) As Rectangle
        Dim sz = TextRenderer.MeasureText(text, font, Size.Empty, TextFormatFlags.NoPadding)
        Using bmp As New Bitmap(Math.Max(1, sz.Width + 8), Math.Max(1, sz.Height + 4))
            Using g = Graphics.FromImage(bmp)
                g.Clear(Color.White)
                TextRenderer.DrawText(g, text, font, Point.Empty, Color.Black, Color.White, TextFormatFlags.NoPadding)
            End Using
            Return InkBounds(bmp, Color.White)
        End Using
    End Function

    ''' <summary>The smallest rectangle holding every pixel that differs clearly from the background.</summary>
    Private Shared Function InkBounds(bmp As Bitmap, back As Color) As Rectangle
        Dim left = Integer.MaxValue, top = Integer.MaxValue, right = -1, bottom = -1
        For y = 0 To bmp.Height - 1
            For x = 0 To bmp.Width - 1
                Dim c = bmp.GetPixel(x, y)
                If Math.Abs(CInt(c.R) - back.R) + Math.Abs(CInt(c.G) - back.G) + Math.Abs(CInt(c.B) - back.B) > 240 Then
                    left = Math.Min(left, x) : top = Math.Min(top, y)
                    right = Math.Max(right, x) : bottom = Math.Max(bottom, y)
                End If
            Next
        Next
        If right < 0 Then Return Rectangle.Empty
        Return Rectangle.FromLTRB(left, top, right + 1, bottom + 1)
    End Function

    ''' <summary>Lines up the bottom of "© 2026 Ian Johnston" with the bottom of the big title.</summary>
    Private Sub AlignVersionBaseline()
        ' Where the small label really draws its letters, padding and all.
        lblVersion.Margin = New Padding(lblVersion.Margin.Left, 0, 0, 0)
        Dim versionInk As Rectangle
        Using bmp As New Bitmap(Math.Max(1, lblVersion.PreferredWidth), Math.Max(1, lblVersion.PreferredHeight))
            lblVersion.DrawToBitmap(bmp, New Rectangle(Point.Empty, bmp.Size))
            versionInk = InkBounds(bmp, lblVersion.BackColor)
        End Using
        If versionInk.IsEmpty OrElse _titleInk.IsEmpty Then Return
        lblVersion.Margin = New Padding(lblVersion.Margin.Left, lblTitle.Margin.Top + _titleInk.Bottom - versionInk.Bottom, 0, 0)
    End Sub

    Private Sub lblTitle_Paint(sender As Object, e As PaintEventArgs) Handles lblTitle.Paint
        TextRenderer.DrawText(e.Graphics, _titleText, lblTitle.Font, New Point(TitleInkOffset, 0), lblTitle.ForeColor,
                              TextFormatFlags.NoPadding)
        If _tmFont IsNot Nothing Then
            TextRenderer.DrawText(e.Graphics, TradeMark, _tmFont, _tmPos, lblTitle.ForeColor, TextFormatFlags.NoPadding)
        End If
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
        _tmFont?.Dispose()
    End Sub

End Class
