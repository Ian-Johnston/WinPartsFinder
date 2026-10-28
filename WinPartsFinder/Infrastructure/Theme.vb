Imports System.Runtime.InteropServices

''' <summary>The colour themes the user can pick in Settings.</summary>
Public Enum ThemeKind
    Light     ' the normal Windows look
    SoftGrey  ' light, with the glare taken off
    DarkGrey  ' a softer, slate-grey dark
    Dark
End Enum

''' <summary>
''' Colours for the whole app. WinForms on .NET Framework has no built-in dark mode, so Apply() walks a
''' form's controls and recolours them (and the title bar). Light uses the Windows defaults; the other
''' themes each have their own set of colours (a <see cref="Palette"/>).
''' </summary>
Public Module Theme

    ''' <summary>One theme's colours.</summary>
    Private Class Palette
        Public Back, Surface, Input, Header, Border, Text, Selection, RowSelection, RowSelectionText As Color
        Public Muted, Link, Good, Converted, Borrowed As Color
        ''' <summary>Dark title bar and scroll bars.</summary>
        Public DarkChrome As Boolean
    End Class

    Private ReadOnly DarkPalette As New Palette With {
        .Back = Color.FromArgb(32, 32, 32), .Surface = Color.FromArgb(43, 43, 43), .Input = Color.FromArgb(51, 51, 51),
        .Header = Color.FromArgb(56, 56, 56), .Border = Color.FromArgb(78, 78, 78), .Text = Color.FromArgb(232, 232, 232),
        .Selection = Color.FromArgb(0, 92, 160), .RowSelection = Color.FromArgb(72, 72, 72), .RowSelectionText = Color.White,
        .Muted = Color.FromArgb(150, 150, 150), .Link = Color.FromArgb(110, 175, 255), .Good = Color.FromArgb(110, 205, 120),
        .Converted = Color.FromArgb(205, 150, 255), .Borrowed = Color.FromArgb(255, 170, 70), .DarkChrome = True}

    ' Slate rather than black: less contrast, easier on the eyes at night.
    Private ReadOnly DarkGreyPalette As New Palette With {
        .Back = Color.FromArgb(44, 48, 56), .Surface = Color.FromArgb(53, 58, 67), .Input = Color.FromArgb(61, 67, 77),
        .Header = Color.FromArgb(66, 72, 83), .Border = Color.FromArgb(92, 100, 114), .Text = Color.FromArgb(222, 226, 232),
        .Selection = Color.FromArgb(52, 104, 170), .RowSelection = Color.FromArgb(80, 88, 102), .RowSelectionText = Color.White,
        .Muted = Color.FromArgb(150, 157, 168), .Link = Color.FromArgb(125, 182, 255), .Good = Color.FromArgb(125, 212, 135),
        .Converted = Color.FromArgb(212, 165, 255), .Borrowed = Color.FromArgb(255, 178, 85), .DarkChrome = True}

    ' Light greys instead of bright white; text and colours a touch darker so they still stand out.
    Private ReadOnly SoftGreyPalette As New Palette With {
        .Back = Color.FromArgb(214, 214, 214), .Surface = Color.FromArgb(230, 230, 230), .Input = Color.FromArgb(238, 238, 238),
        .Header = Color.FromArgb(200, 200, 200), .Border = Color.FromArgb(158, 158, 158), .Text = Color.FromArgb(24, 24, 24),
        .Selection = Color.FromArgb(168, 190, 218), .RowSelection = Color.FromArgb(194, 194, 194), .RowSelectionText = Color.Black,
        .Muted = Color.FromArgb(105, 105, 105), .Link = Color.FromArgb(28, 78, 190), .Good = Color.FromArgb(0, 105, 0),
        .Converted = Color.FromArgb(118, 0, 160), .Borrowed = Color.FromArgb(170, 85, 0), .DarkChrome = False}

    ' Selected grid rows are grey, not the usual blue, so blue link text (supplier part no.) stays readable.
    Private ReadOnly LightRowSelection As Color = Color.FromArgb(218, 218, 218)

    ''' <summary>The theme in use. Set it, then call Apply() on each open window.</summary>
    Public Property Current As ThemeKind = ThemeKind.Light

    ''' <summary>The current theme's colours, or Nothing for Light (Windows defaults).</summary>
    Private ReadOnly Property P As Palette
        Get
            Select Case Current
                Case ThemeKind.Dark : Return DarkPalette
                Case ThemeKind.DarkGrey : Return DarkGreyPalette
                Case ThemeKind.SoftGrey : Return SoftGreyPalette
                Case Else : Return Nothing
            End Select
        End Get
    End Property

    ''' <summary>True for Dark grey and Dark (dark title bar, light text).</summary>
    Public ReadOnly Property IsDark As Boolean
        Get
            Return P IsNot Nothing AndAlso P.DarkChrome
        End Get
    End Property

    ''' <summary>True for every theme except Light - those draw their own colours instead of the Windows ones.</summary>
    Public ReadOnly Property IsCustom As Boolean
        Get
            Return P IsNot Nothing
        End Get
    End Property

    ''' <summary>The names shown in Settings, in ThemeKind order.</summary>
    Public ReadOnly Names As String() = {"Light", "Soft grey", "Dark grey", "Dark"}

    ' ---- Colours the forms use for meaning, in every theme ----
    Public ReadOnly Property TextColor As Color
        Get
            Return If(IsCustom, P.Text, SystemColors.ControlText)
        End Get
    End Property

    Public ReadOnly Property MutedText As Color
        Get
            Return If(IsCustom, P.Muted, SystemColors.GrayText)
        End Get
    End Property

    Public ReadOnly Property LinkText As Color
        Get
            Return If(IsCustom, P.Link, Color.RoyalBlue)
        End Get
    End Property

    Public ReadOnly Property GoodText As Color
        Get
            Return If(IsCustom, P.Good, Color.DarkGreen)
        End Get
    End Property

    ''' <summary>Prices converted to the "Prices in" currency, so they stand out as estimates.</summary>
    Public ReadOnly Property ConvertedText As Color
        Get
            Return If(IsCustom, P.Converted, Color.FromArgb(128, 0, 170))
        End Get
    End Property

    ''' <summary>A package borrowed from another supplier's listing of the same part (Mouser doesn't give one).</summary>
    Public ReadOnly Property BorrowedText As Color
        Get
            Return If(IsCustom, P.Borrowed, Color.FromArgb(190, 95, 0))
        End Get
    End Property

    Public ReadOnly Property HeaderBack As Color
        Get
            Return If(IsCustom, P.Header, SystemColors.Control)
        End Get
    End Property

    Public ReadOnly Property BorderColor As Color
        Get
            Return If(IsCustom, P.Border, SystemColors.ControlDark)
        End Get
    End Property

    Public Sub Apply(form As Form)
        SetDarkTitleBar(form, IsDark)
        If IsCustom Then
            form.BackColor = P.Back
            form.ForeColor = P.Text
        Else
            form.ResetBackColor()
            form.ResetForeColor()
        End If
        ' Labels, check boxes and panels pick up the form's colours automatically,
        ' so only controls with their own default colours need handling.
        ApplyTo(form)
        form.Invalidate(True)
    End Sub

    Private Sub ApplyTo(parent As Control)
        For Each c As Control In parent.Controls
            Select Case True
                Case TypeOf c Is DataGridView
                    StyleGrid(DirectCast(c, DataGridView))
                Case TypeOf c Is Button
                    StyleButton(DirectCast(c, Button))
                Case TypeOf c Is ComboBox
                    StyleCombo(DirectCast(c, ComboBox))
                    EdgePainter.Attach(c)
                Case TypeOf c Is TextBox
                    DirectCast(c, TextBox).BorderStyle = If(IsCustom, BorderStyle.FixedSingle, BorderStyle.Fixed3D)
                    StyleInput(c)
                    EdgePainter.Attach(c)
                Case TypeOf c Is UpDownBase
                    DirectCast(c, UpDownBase).BorderStyle = If(IsCustom, BorderStyle.FixedSingle, BorderStyle.Fixed3D)
                    StyleInput(c)
                    EdgePainter.Attach(c)
                Case TypeOf c Is TextBoxBase, TypeOf c Is ListView
                    StyleInput(c)
                Case TypeOf c Is LinkLabel
                    Dim l = DirectCast(c, LinkLabel)
                    l.LinkColor = LinkText
                    l.ActiveLinkColor = LinkText
                    l.VisitedLinkColor = LinkText
                Case TypeOf c Is ToolStrip
                    StyleToolStrip(DirectCast(c, ToolStrip))
            End Select

            If c.ContextMenuStrip IsNot Nothing Then StyleToolStrip(c.ContextMenuStrip)
            SetDarkScrollbars(c)
            If Not TypeOf c Is DataGridView Then ApplyTo(c) ' (a grid's children are just its scroll bars)
        Next
    End Sub

    Private Sub StyleInput(c As Control)
        If IsCustom Then
            c.BackColor = P.Input
            c.ForeColor = P.Text
        Else
            c.ResetBackColor()
            c.ResetForeColor()
        End If
    End Sub

    Private Sub StyleCombo(cb As ComboBox)
        cb.FlatStyle = If(IsCustom, FlatStyle.Flat, FlatStyle.Standard)
        StyleInput(cb)
        ' A DropDownList combo ignores BackColor, so in the custom themes it's drawn by hand.
        If cb.DropDownStyle = ComboBoxStyle.DropDownList Then
            RemoveHandler cb.DrawItem, AddressOf DrawComboItem
            If IsCustom Then
                cb.DrawMode = DrawMode.OwnerDrawFixed
                AddHandler cb.DrawItem, AddressOf DrawComboItem
            Else
                cb.DrawMode = DrawMode.Normal
            End If
        End If
    End Sub

    Private Sub DrawComboItem(sender As Object, e As DrawItemEventArgs)
        Dim pal = P
        If pal Is Nothing Then Return
        Dim cb = DirectCast(sender, ComboBox)
        Dim selected = (e.State And DrawItemState.Selected) = DrawItemState.Selected AndAlso
                       (e.State And DrawItemState.ComboBoxEdit) <> DrawItemState.ComboBoxEdit
        Using b As New SolidBrush(If(selected, pal.Selection, pal.Input))
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using
        If e.Index >= 0 Then
            TextRenderer.DrawText(e.Graphics, cb.GetItemText(cb.Items(e.Index)), cb.Font, e.Bounds,
                                  If(selected AndAlso pal.DarkChrome, Color.White, pal.Text),
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End If
    End Sub

    Private Sub StyleButton(b As Button)
        If IsCustom Then
            b.FlatStyle = FlatStyle.Flat
            b.FlatAppearance.BorderColor = P.Border
            b.BackColor = P.Surface
            b.ForeColor = P.Text
        Else
            b.FlatStyle = FlatStyle.Standard
            b.ResetBackColor()
            b.ResetForeColor()
            b.UseVisualStyleBackColor = True
        End If
    End Sub

    Private Sub StyleGrid(g As DataGridView)
        Dim cells = g.DefaultCellStyle
        Dim headers = g.ColumnHeadersDefaultCellStyle
        If IsCustom Then
            g.BackgroundColor = P.Back
            g.GridColor = P.Border
            cells.BackColor = P.Surface
            cells.ForeColor = P.Text
            cells.SelectionBackColor = P.RowSelection
            cells.SelectionForeColor = P.RowSelectionText
            g.EnableHeadersVisualStyles = False
            ' Flat header edges - the normal raised ones draw bright white lines on a dark header.
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            g.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single
            headers.BackColor = P.Header
            headers.ForeColor = P.Text
            headers.SelectionBackColor = P.Header
            headers.SelectionForeColor = P.Text
        Else
            g.BackgroundColor = SystemColors.Window
            g.GridColor = SystemColors.ControlDark
            cells.BackColor = SystemColors.Window
            cells.ForeColor = SystemColors.ControlText
            cells.SelectionBackColor = LightRowSelection
            cells.SelectionForeColor = SystemColors.ControlText
            g.EnableHeadersVisualStyles = True
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised
            g.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Raised
            headers.BackColor = SystemColors.Control
            headers.ForeColor = SystemColors.WindowText
            headers.SelectionBackColor = SystemColors.Highlight
            headers.SelectionForeColor = SystemColors.HighlightText
        End If
        For Each col In g.Columns.OfType(Of DataGridViewLinkColumn)()
            col.LinkColor = LinkText
            col.ActiveLinkColor = LinkText
        Next
    End Sub

    ''' <summary>For a menu that isn't attached to a control (so Apply doesn't reach it), e.g. the grid's header menu.</summary>
    Public Sub StyleMenu(menu As ToolStrip)
        StyleToolStrip(menu)
    End Sub

    Private Sub StyleToolStrip(ts As ToolStrip)
        If IsCustom Then
            ts.Renderer = New ToolStripProfessionalRenderer(New PaletteColorTable(P))
            ts.BackColor = P.Surface
            ts.ForeColor = P.Text
        Else
            ts.RenderMode = ToolStripRenderMode.ManagerRenderMode
            ts.ResetBackColor()
            ts.ResetForeColor()
        End If
        For Each item As ToolStripItem In ts.Items
            If IsCustom Then item.ForeColor = P.Text Else item.ResetForeColor()
        Next
    End Sub

    ''' <summary>Menu and status-bar colours for the custom themes.</summary>
    Private Class PaletteColorTable
        Inherits ProfessionalColorTable

        Private ReadOnly _p As Palette

        Public Sub New(p As Palette)
            _p = p
        End Sub

        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return _p.Border
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return _p.Selection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return _p.Selection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return _p.Selection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return _p.Selection
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return _p.Border
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientBegin As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientEnd As Color
            Get
                Return _p.Surface
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripBorder As Color
            Get
                Return _p.Surface
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Windows draws text-box edges, and flat drop-downs' edges and arrow button, in its own (light) colours,
    ''' whatever the BackColor. In the custom themes this paints over them in the theme's colours, straight after
    ''' Windows has drawn the control. In Light it does nothing, so the normal Windows look is untouched.
    ''' </summary>
    Private Class EdgePainter
        Inherits NativeWindow

        Private Const WM_PAINT As Integer = &HF
        Private Const WM_NCPAINT As Integer = &H85
        Private Shared ReadOnly Attached As New HashSet(Of Control)

        Private ReadOnly _c As Control

        Public Shared Sub Attach(c As Control)
            If Attached.Contains(c) Then Return
            Attached.Add(c)
            Dim hook = Sub()
                           Dim painter As New EdgePainter(c)
                       End Sub
            If c.IsHandleCreated Then hook() Else AddHandler c.HandleCreated, Sub() hook()
            AddHandler c.Disposed, Sub() Attached.Remove(c)
        End Sub

        Private Sub New(c As Control)
            _c = c
            AssignHandle(c.Handle)
            AddHandler c.HandleDestroyed, Sub() ReleaseHandle()
        End Sub

        Protected Overrides Sub WndProc(ByRef m As Message)
            MyBase.WndProc(m)
            If (m.Msg = WM_PAINT OrElse m.Msg = WM_NCPAINT) AndAlso IsCustom Then PaintEdges()
        End Sub

        Private Sub PaintEdges()
            Dim pal = P
            If pal Is Nothing OrElse Not _c.IsHandleCreated Then Return
            Dim hdc = GetWindowDC(_c.Handle)
            If hdc = IntPtr.Zero Then Return
            Try
                Using g = Graphics.FromHdc(hdc), edge As New Pen(pal.Border)
                    Dim w = _c.Width, h = _c.Height
                    If TypeOf _c Is ComboBox Then
                        ' Flat drop-downs also have a light inner edge and a light arrow button.
                        Using inner As New Pen(pal.Input)
                            g.DrawRectangle(inner, 1, 1, w - 3, h - 3)
                        End Using
                        Dim bw = SystemInformation.HorizontalScrollBarArrowWidth
                        Dim btn As New Rectangle(w - bw - 1, 1, bw, h - 2)
                        Using back As New SolidBrush(pal.Surface), arrow As New SolidBrush(pal.Text)
                            g.FillRectangle(back, btn)
                            Dim cx = btn.Left + btn.Width \ 2, cy = btn.Top + btn.Height \ 2
                            g.FillPolygon(arrow, {New Point(cx - 4, cy - 2), New Point(cx + 4, cy - 2), New Point(cx, cy + 2)})
                        End Using
                        g.DrawLine(edge, btn.Left - 1, 1, btn.Left - 1, h - 2)
                    End If
                    g.DrawRectangle(edge, 0, 0, w - 1, h - 1)
                End Using
            Finally
                ReleaseDC(_c.Handle, hdc)
            End Try
        End Sub
    End Class

    <DllImport("user32.dll")>
    Private Function GetWindowDC(hwnd As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Function ReleaseDC(hwnd As IntPtr, hdc As IntPtr) As Integer
    End Function

#Region "Win32: dark title bar and scroll bars (Windows 10 20H1+ / Windows 11)"

    <DllImport("dwmapi.dll")>
    Private Function DwmSetWindowAttribute(hwnd As IntPtr, attr As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hwnd As IntPtr, appName As String, idList As String) As Integer
    End Function

    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20
    Private Const DWMWA_CAPTION_COLOR As Integer = 35
    Private Const DWMWA_COLOR_DEFAULT As Integer = &HFFFFFFFF

    <DllImport("user32.dll", CharSet:=CharSet.Unicode)>
    Private Function SendMessage(hwnd As IntPtr, msg As Integer, wParam As IntPtr, lParam As String) As IntPtr
    End Function

    Private Const EM_SETCUEBANNER As Integer = &H1501

    ''' <summary>Grey hint text shown in an empty text box (TextBox.PlaceholderText doesn't exist on .NET Framework).</summary>
    Public Sub SetCueBanner(tb As TextBox, hint As String)
        If tb.IsHandleCreated Then
            SendMessage(tb.Handle, EM_SETCUEBANNER, IntPtr.Zero, hint)
        Else
            AddHandler tb.HandleCreated, Sub() SendMessage(tb.Handle, EM_SETCUEBANNER, IntPtr.Zero, hint)
        End If
    End Sub

    Private Sub SetDarkTitleBar(form As Form, dark As Boolean)
        If Not form.IsHandleCreated Then Return
        Dim value = If(dark, 1, 0)
        Try
            DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, value, 4)
        Catch
            ' older Windows: title bar just stays light
        End Try
        ' Dark mode's title bar is near-black; Dark Grey gets its own slate instead (Windows 11 only -
        ' Windows 10 ignores it and keeps the black one). Other themes go back to the Windows colour.
        Dim caption = DWMWA_COLOR_DEFAULT
        If Current = ThemeKind.DarkGrey Then caption = ColorTranslator.ToWin32(P.Back)
        Try
            DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, caption, 4)
        Catch
        End Try
    End Sub

    Private Sub SetDarkScrollbars(c As Control)
        If Not c.IsHandleCreated Then Return
        ' (A scrolling panel too, e.g. the filters under the supplier list.)
        If Not (TypeOf c Is DataGridView OrElse TypeOf c Is ListView OrElse TypeOf c Is ScrollBar OrElse
                TypeOf c Is TextBoxBase OrElse (TypeOf c Is Panel AndAlso DirectCast(c, Panel).AutoScroll)) Then Return
        Try
            SetWindowTheme(c.Handle, If(IsDark, "DarkMode_Explorer", "Explorer"), Nothing)
        Catch
        End Try
        For Each child As Control In c.Controls ' the grid's own scroll bars
            SetDarkScrollbars(child)
        Next
    End Sub

#End Region

End Module
