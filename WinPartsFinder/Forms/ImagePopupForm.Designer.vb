<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ImagePopupForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        tipPopup = New ToolTip(components)
        picPhoto = New PictureBox()
        lblLoading = New Label()
        lblCaption = New Label()
        CType(picPhoto, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        'picPhoto
        '
        picPhoto.BackColor = Color.White
        picPhoto.Cursor = Cursors.Hand
        picPhoto.Dock = DockStyle.Fill
        picPhoto.Location = New Point(0, 0)
        picPhoto.Name = "picPhoto"
        picPhoto.Size = New Size(320, 280)
        picPhoto.SizeMode = PictureBoxSizeMode.Zoom
        picPhoto.TabIndex = 0
        picPhoto.TabStop = False
        picPhoto.Visible = False
        '
        'lblLoading
        '
        lblLoading.Dock = DockStyle.Fill
        lblLoading.ForeColor = SystemColors.GrayText
        lblLoading.Location = New Point(0, 0)
        lblLoading.Name = "lblLoading"
        lblLoading.Size = New Size(320, 280)
        lblLoading.TabIndex = 1
        lblLoading.Text = "Loading photo…"
        lblLoading.TextAlign = ContentAlignment.MiddleCenter
        '
        'lblCaption
        '
        lblCaption.AutoEllipsis = True
        lblCaption.Dock = DockStyle.Bottom
        lblCaption.Location = New Point(0, 280)
        lblCaption.Name = "lblCaption"
        lblCaption.Padding = New Padding(6, 4, 6, 4)
        lblCaption.Size = New Size(320, 40)
        lblCaption.TabIndex = 2
        '
        'tipPopup
        '
        tipPopup.SetToolTip(picPhoto, "Click, or press Esc, to close. Click another thumbnail to swap photos.")
        '
        'ImagePopupForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(320, 320)
        Controls.Add(picPhoto)
        Controls.Add(lblLoading)
        Controls.Add(lblCaption)
        FormBorderStyle = FormBorderStyle.FixedToolWindow
        KeyPreview = True
        Name = "ImagePopupForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        Text = "Photo"
        TopMost = True
        CType(picPhoto, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tipPopup As ToolTip
    Friend WithEvents picPhoto As PictureBox
    Friend WithEvents lblLoading As Label
    Friend WithEvents lblCaption As Label

End Class
