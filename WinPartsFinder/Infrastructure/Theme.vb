Imports System.Runtime.InteropServices

''' <summary>
''' Light / dark colours for the whole app. .NET 8 WinForms has no built-in dark mode,
''' so Apply() walks a form's controls and recolours them (and the title bar).
''' Calling Apply again with IsDark = False puts everything back to the Windows defaults.
''' </summary>
Public Module Theme

    Public Property IsDark As Boolean

    ' ---- Dark palette ----
    Private ReadOnly DarkBack As Color = Color.FromArgb(32, 32, 32)
    Private ReadOnly DarkSurface As Color = Color.FromArgb(43, 43, 43)
    Private ReadOnly DarkInput As Color = Color.FromArgb(51, 51, 51)
    Private ReadOnly DarkHeader As Color = Color.FromArgb(56, 56, 56)
    Private ReadOnly DarkBorder As Color = Color.FromArgb(78, 78, 78)
    Private ReadOnly DarkText As Color = Color.FromArgb(232, 232, 232)
    Private ReadOnly DarkSelection As Color = Color.FromArgb(0, 92, 160)

    ' Selected grid rows are grey, not the usual blue, so blue link text (supplier part no.) stays readable.
    Private ReadOnly LightRowSelection As Color = Color.FromArgb(218, 218, 218)
    Private ReadOnly DarkRowSelection As Color = Color.FromArgb(72, 72, 72)

    ' ---- Colours the forms use for meaning, in either theme ----
    Public ReadOnly Property TextColor As Color
        Get
            Return If(IsDark, DarkText, SystemColors.ControlText)
        End Get
    End Property

    Public ReadOnly Property MutedText As Color
        Get
            Return If(IsDark, Color.FromArgb(150, 150, 150), SystemColors.GrayText)
        End Get
    End Property

    Public ReadOnly Property LinkText As Color
        Get
            Return If(IsDark, Color.FromArgb(110, 175, 255), Color.RoyalBlue)
        End Get
    End Property

    Public ReadOnly Property GoodText As Color
        Get
            Return If(IsDark, Color.FromArgb(110, 205, 120), Color.DarkGreen)
        End Get
    End Property

    Public ReadOnly Property HeaderBack As Color
        Get
            Return If(IsDark, DarkHeader, SystemColors.Control)
        End Get
    End Property

    Public ReadOnly Property BorderColor As Color
        Get
            Return If(IsDark, DarkBorder, SystemColors.ControlDark)
        End Get
    End Property

    Public Sub Apply(form As Form)
        SetDarkTitleBar(form, IsDark)
        If IsDark Then
            form.BackColor = DarkBack
            form.ForeColor = DarkText
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
                Case TypeOf c Is TextBox
                    DirectCast(c, TextBox).BorderStyle = If(IsDark, BorderStyle.FixedSingle, BorderStyle.Fixed3D)
                    StyleInput(c)
                Case TypeOf c Is UpDownBase
                    DirectCast(c, UpDownBase).BorderStyle = If(IsDark, BorderStyle.FixedSingle, BorderStyle.Fixed3D)
                    StyleInput(c)
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
        If IsDark Then
            c.BackColor = DarkInput
            c.ForeColor = DarkText
        Else
            c.ResetBackColor()
            c.ResetForeColor()
        End If
    End Sub

    Private Sub StyleCombo(cb As ComboBox)
        cb.FlatStyle = If(IsDark, FlatStyle.Flat, FlatStyle.Standard)
        StyleInput(cb)
        ' A DropDownList combo ignores BackColor, so in dark mode it's drawn by hand.
        If cb.DropDownStyle = ComboBoxStyle.DropDownList Then
            RemoveHandler cb.DrawItem, AddressOf DrawComboItem
            If IsDark Then
                cb.DrawMode = DrawMode.OwnerDrawFixed
                AddHandler cb.DrawItem, AddressOf DrawComboItem
            Else
                cb.DrawMode = DrawMode.Normal
            End If
        End If
    End Sub

    Private Sub DrawComboItem(sender As Object, e As DrawItemEventArgs)
        Dim cb = DirectCast(sender, ComboBox)
        Dim selected = (e.State And DrawItemState.Selected) = DrawItemState.Selected AndAlso
                       (e.State And DrawItemState.ComboBoxEdit) <> DrawItemState.ComboBoxEdit
        Using b As New SolidBrush(If(selected, DarkSelection, DarkInput))
            e.Graphics.FillRectangle(b, e.Bounds)
        End Using
        If e.Index >= 0 Then
            TextRenderer.DrawText(e.Graphics, cb.GetItemText(cb.Items(e.Index)), cb.Font, e.Bounds, DarkText,
                                  TextFormatFlags.VerticalCenter Or TextFormatFlags.Left Or TextFormatFlags.EndEllipsis)
        End If
    End Sub

    Private Sub StyleButton(b As Button)
        If IsDark Then
            b.FlatStyle = FlatStyle.Flat
            b.FlatAppearance.BorderColor = DarkBorder
            b.BackColor = DarkSurface
            b.ForeColor = DarkText
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
        If IsDark Then
            g.BackgroundColor = DarkBack
            g.GridColor = DarkBorder
            cells.BackColor = DarkSurface
            cells.ForeColor = DarkText
            cells.SelectionBackColor = DarkRowSelection
            cells.SelectionForeColor = Color.White
            g.EnableHeadersVisualStyles = False
            headers.BackColor = DarkHeader
            headers.ForeColor = DarkText
            headers.SelectionBackColor = DarkHeader
            headers.SelectionForeColor = DarkText
        Else
            g.BackgroundColor = SystemColors.Window
            g.GridColor = SystemColors.ControlDark
            cells.BackColor = SystemColors.Window
            cells.ForeColor = SystemColors.ControlText
            cells.SelectionBackColor = LightRowSelection
            cells.SelectionForeColor = SystemColors.ControlText
            g.EnableHeadersVisualStyles = True
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

    Private Sub StyleToolStrip(ts As ToolStrip)
        If IsDark Then
            ts.Renderer = New ToolStripProfessionalRenderer(New DarkColorTable())
            ts.BackColor = DarkSurface
            ts.ForeColor = DarkText
        Else
            ts.RenderMode = ToolStripRenderMode.ManagerRenderMode
            ts.ResetBackColor()
            ts.ResetForeColor()
        End If
        For Each item As ToolStripItem In ts.Items
            If IsDark Then item.ForeColor = DarkText Else item.ResetForeColor()
        Next
    End Sub

    Private Class DarkColorTable
        Inherits ProfessionalColorTable

        Public Overrides ReadOnly Property ToolStripDropDownBackground As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientBegin As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientMiddle As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property ImageMarginGradientEnd As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property MenuBorder As Color
            Get
                Return DarkBorder
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemBorder As Color
            Get
                Return DarkSelection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelected As Color
            Get
                Return DarkSelection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientBegin As Color
            Get
                Return DarkSelection
            End Get
        End Property
        Public Overrides ReadOnly Property MenuItemSelectedGradientEnd As Color
            Get
                Return DarkSelection
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorDark As Color
            Get
                Return DarkBorder
            End Get
        End Property
        Public Overrides ReadOnly Property SeparatorLight As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientBegin As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property StatusStripGradientEnd As Color
            Get
                Return DarkSurface
            End Get
        End Property
        Public Overrides ReadOnly Property ToolStripBorder As Color
            Get
                Return DarkSurface
            End Get
        End Property
    End Class

#Region "Win32: dark title bar and scroll bars (Windows 10 20H1+ / Windows 11)"

    <DllImport("dwmapi.dll")>
    Private Function DwmSetWindowAttribute(hwnd As IntPtr, attr As Integer, ByRef value As Integer, size As Integer) As Integer
    End Function

    <DllImport("uxtheme.dll", CharSet:=CharSet.Unicode)>
    Private Function SetWindowTheme(hwnd As IntPtr, appName As String, idList As String) As Integer
    End Function

    Private Const DWMWA_USE_IMMERSIVE_DARK_MODE As Integer = 20

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
    End Sub

    Private Sub SetDarkScrollbars(c As Control)
        If Not c.IsHandleCreated Then Return
        If Not (TypeOf c Is DataGridView OrElse TypeOf c Is ListView OrElse TypeOf c Is ScrollBar OrElse
                TypeOf c Is TextBoxBase) Then Return
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
