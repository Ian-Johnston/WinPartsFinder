<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class HelpForm
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
        tipHelp = New ToolTip(components)
        pnlText = New Panel()
        rtbHelp = New RichTextBox()
        flpButtons = New FlowLayoutPanel()
        btnOK = New Button()
        pnlText.SuspendLayout()
        flpButtons.SuspendLayout()
        SuspendLayout()
        '
        'pnlText - white margin around the text, like a page
        '
        pnlText.Controls.Add(rtbHelp)
        pnlText.Dock = DockStyle.Fill
        pnlText.Location = New Point(0, 0)
        pnlText.Name = "pnlText"
        pnlText.Padding = New Padding(14, 10, 6, 6)
        pnlText.Size = New Size(820, 600)
        pnlText.TabIndex = 0
        '
        'rtbHelp
        '
        rtbHelp.BorderStyle = BorderStyle.None
        rtbHelp.DetectUrls = False ' the example URLs aren't meant to be clicked
        rtbHelp.Dock = DockStyle.Fill
        rtbHelp.Font = New Font("Segoe UI", 10.0F)
        rtbHelp.Location = New Point(14, 10)
        rtbHelp.Name = "rtbHelp"
        rtbHelp.ReadOnly = True
        rtbHelp.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbHelp.Size = New Size(800, 584)
        rtbHelp.TabIndex = 0
        rtbHelp.Text = ""
        '
        'flpButtons
        '
        flpButtons.AutoSize = True
        flpButtons.Controls.Add(btnOK)
        flpButtons.Dock = DockStyle.Bottom
        flpButtons.FlowDirection = FlowDirection.RightToLeft
        flpButtons.Name = "flpButtons"
        flpButtons.Padding = New Padding(8)
        flpButtons.TabIndex = 1
        '
        'btnOK
        '
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(85, 28)
        btnOK.TabIndex = 0
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        '
        'tipHelp
        '
        tipHelp.SetToolTip(btnOK, "Close Help (Esc). You can keep Help open while you use WinPartsFinder.")
        '
        'HelpForm
        '
        AcceptButton = btnOK
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnOK
        ClientSize = New Size(820, 650)
        Controls.Add(pnlText)
        Controls.Add(flpButtons)
        MinimizeBox = False
        MinimumSize = New Size(500, 400)
        Name = "HelpForm"
        ShowIcon = False
        StartPosition = FormStartPosition.CenterParent
        Text = "WinPartsFinder    Help"
        pnlText.ResumeLayout(False)
        flpButtons.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tipHelp As ToolTip
    Friend WithEvents pnlText As Panel
    Friend WithEvents rtbHelp As RichTextBox
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnOK As Button

End Class
