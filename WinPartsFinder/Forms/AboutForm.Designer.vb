<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AboutForm
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
        tipAbout = New ToolTip(components)
        flpLeft = New FlowLayoutPanel()
        flpTitle = New FlowLayoutPanel()
        lblTitle = New Label()
        lblVersion = New Label()
        lblCreditsHead = New Label()
        lblCredits = New Label()
        lblDevHead = New Label()
        lblDev = New Label()
        lblSupportHead = New Label()
        lblSupport = New Label()
        lblLicenceHead = New Label()
        lblLicence = New Label()
        lblDisclaimerHead = New Label()
        lblDisclaimer = New Label()
        pnlRight = New Panel()
        lblAuthor = New Label()
        lnkWebsite = New LinkLabel()
        lnkYouTube = New LinkLabel()
        lnkTwitter = New LinkLabel()
        picCartoon = New PictureBox()
        lblSupportBig = New Label()
        btnDonate = New Button()
        btnClose = New Button()
        flpLeft.SuspendLayout()
        flpTitle.SuspendLayout()
        pnlRight.SuspendLayout()
        CType(picCartoon, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        '
        'flpLeft
        '
        flpLeft.AutoScroll = True
        flpLeft.Controls.Add(flpTitle)
        flpLeft.Controls.Add(lblCreditsHead)
        flpLeft.Controls.Add(lblCredits)
        flpLeft.Controls.Add(lblDevHead)
        flpLeft.Controls.Add(lblDev)
        flpLeft.Controls.Add(lblSupportHead)
        flpLeft.Controls.Add(lblSupport)
        flpLeft.Controls.Add(lblLicenceHead)
        flpLeft.Controls.Add(lblLicence)
        flpLeft.Controls.Add(lblDisclaimerHead)
        flpLeft.Controls.Add(lblDisclaimer)
        flpLeft.Dock = DockStyle.Fill
        flpLeft.FlowDirection = FlowDirection.TopDown
        flpLeft.Location = New Point(0, 0)
        flpLeft.Name = "flpLeft"
        flpLeft.Padding = New Padding(10, 8, 10, 8)
        flpLeft.Size = New Size(640, 620)
        flpLeft.TabIndex = 0
        flpLeft.WrapContents = False
        '
        'flpTitle
        '
        flpTitle.AutoSize = True
        flpTitle.Controls.Add(lblTitle)
        flpTitle.Controls.Add(lblVersion)
        flpTitle.Margin = New Padding(0, 0, 0, 6)
        flpTitle.Name = "flpTitle"
        flpTitle.TabIndex = 0
        '
        'lblTitle
        '
        lblTitle.AutoSize = True
        lblTitle.Font = New Font("Segoe UI", 24.0F, FontStyle.Bold)
        lblTitle.Margin = New Padding(0)
        lblTitle.Name = "lblTitle"
        lblTitle.Text = "WinPartsFinder"
        '
        'lblVersion
        '
        lblVersion.AutoSize = True
        lblVersion.Font = New Font("Segoe UI", 8.25F)
        lblVersion.Margin = New Padding(10, 22, 0, 0)
        lblVersion.Name = "lblVersion"
        lblVersion.Text = "(UK 2026)"
        '
        'lblCreditsHead
        '
        lblCreditsHead.AutoSize = True
        lblCreditsHead.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        lblCreditsHead.Margin = New Padding(0, 6, 0, 0)
        lblCreditsHead.Name = "lblCreditsHead"
        lblCreditsHead.Text = "Credits && Acknowledgements:"
        '
        'lblCredits
        '
        lblCredits.AutoSize = True
        lblCredits.MaximumSize = New Size(600, 0)
        lblCredits.Margin = New Padding(0)
        lblCredits.Name = "lblCredits"
        lblCredits.Text = "WinPartsFinder app by Ian Johnston." & vbCrLf &
            "Part data from Mouser, DigiKey and element14 (Farnell / Newark) web APIs." & vbCrLf &
            "Supplier names and trademarks belong to their respective owners."
        '
        'lblDevHead
        '
        lblDevHead.AutoSize = True
        lblDevHead.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        lblDevHead.Margin = New Padding(0, 12, 0, 0)
        lblDevHead.Name = "lblDevHead"
        lblDevHead.Text = "Development:"
        '
        'lblDev
        '
        lblDev.AutoSize = True
        lblDev.MaximumSize = New Size(600, 0)
        lblDev.Margin = New Padding(0)
        lblDev.Name = "lblDev"
        lblDev.Text = "Developed in VB.NET using Microsoft Visual Studio."
        '
        'lblSupportHead
        '
        lblSupportHead.AutoSize = True
        lblSupportHead.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        lblSupportHead.Margin = New Padding(0, 12, 0, 0)
        lblSupportHead.Name = "lblSupportHead"
        lblSupportHead.Text = "Supporting WinPartsFinder:"
        '
        'lblSupport
        '
        lblSupport.AutoSize = True
        lblSupport.MaximumSize = New Size(600, 0)
        lblSupport.Margin = New Padding(0)
        lblSupport.Name = "lblSupport"
        lblSupport.Text = "If you find the program useful, please consider donating using the button provided." & vbCrLf &
            "Your support helps me spend more time developing and extending WinPartsFinder, and also helps fund " &
            "the purchase of faulty test equipment for repair on my YouTube channel."
        '
        'lblLicenceHead
        '
        lblLicenceHead.AutoSize = True
        lblLicenceHead.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        lblLicenceHead.Margin = New Padding(0, 12, 0, 0)
        lblLicenceHead.Name = "lblLicenceHead"
        lblLicenceHead.Text = "Licence && Usage:"
        '
        'lblLicence
        '
        lblLicence.AutoSize = True
        lblLicence.MaximumSize = New Size(600, 0)
        lblLicence.Margin = New Padding(0)
        lblLicence.Name = "lblLicence"
        lblLicence.Text = "WinPartsFinder is provided free of charge for personal, non-commercial use only." & vbCrLf &
            "The WinPartsFinder source code is made publicly available and may be used, modified and redistributed " &
            "for non-commercial purposes only." & vbCrLf &
            "Neither WinPartsFinder nor its source code, in whole or in part, may be used for commercial purposes " &
            "without a separate commercial licence from the author. This includes use within businesses and organisations." & vbCrLf &
            "Please contact the author (www.ianjohnston.com) to discuss commercial licensing arrangements." & vbCrLf &
            "Each user must obtain their own supplier API keys and follow each supplier's API terms of use."
        '
        'lblDisclaimerHead
        '
        lblDisclaimerHead.AutoSize = True
        lblDisclaimerHead.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold)
        lblDisclaimerHead.Margin = New Padding(0, 12, 0, 0)
        lblDisclaimerHead.Name = "lblDisclaimerHead"
        lblDisclaimerHead.Text = "Disclaimer && Limitation of Liability:"
        '
        'lblDisclaimer
        '
        lblDisclaimer.AutoSize = True
        lblDisclaimer.MaximumSize = New Size(600, 0)
        lblDisclaimer.Margin = New Padding(0)
        lblDisclaimer.Name = "lblDisclaimer"
        lblDisclaimer.Text = "WinPartsFinder and its source code are provided ""as is"", without warranty of any kind, express or implied. " &
            "Use of this software and source code is entirely at the user's own risk." & vbCrLf &
            "Prices, stock levels and other part data come from third parties and may be out of date or wrong - " &
            "always check with the supplier before ordering." & vbCrLf &
            "To the fullest extent permitted by law, the author shall not be liable for any loss, damage, data loss, " &
            "loss of use, loss of profits, or other direct, indirect, incidental or consequential loss arising from " &
            "the installation, modification or use of, or inability to use, this software or source code."
        '
        'pnlRight
        '
        pnlRight.Controls.Add(lblAuthor)
        pnlRight.Controls.Add(lnkWebsite)
        pnlRight.Controls.Add(lnkYouTube)
        pnlRight.Controls.Add(lnkTwitter)
        pnlRight.Controls.Add(picCartoon)
        pnlRight.Controls.Add(lblSupportBig)
        pnlRight.Controls.Add(btnDonate)
        pnlRight.Controls.Add(btnClose)
        pnlRight.Dock = DockStyle.Right
        pnlRight.Location = New Point(640, 0)
        pnlRight.Name = "pnlRight"
        pnlRight.Size = New Size(440, 620)
        pnlRight.TabIndex = 1
        '
        'lblAuthor
        '
        lblAuthor.AutoSize = True
        lblAuthor.Font = New Font("Segoe UI", 9.75F, FontStyle.Bold Or FontStyle.Italic)
        lblAuthor.Location = New Point(4, 62)
        lblAuthor.Name = "lblAuthor"
        lblAuthor.Text = "Ian Johnston."
        '
        'lnkWebsite
        '
        lnkWebsite.AutoSize = True
        lnkWebsite.Location = New Point(4, 86)
        lnkWebsite.Name = "lnkWebsite"
        lnkWebsite.TabIndex = 0
        lnkWebsite.TabStop = True
        lnkWebsite.Tag = "https://www.ianjohnston.com"
        lnkWebsite.Text = "www.ianjohnston.com"
        '
        'lnkYouTube
        '
        lnkYouTube.AutoSize = True
        lnkYouTube.Location = New Point(4, 108)
        lnkYouTube.Name = "lnkYouTube"
        lnkYouTube.TabIndex = 1
        lnkYouTube.TabStop = True
        lnkYouTube.Tag = "https://www.youtube.com/user/IanScottJohnston"
        lnkYouTube.Text = "www.youtube.com/user/IanScottJohnston"
        '
        'lnkTwitter
        '
        lnkTwitter.AutoSize = True
        lnkTwitter.Location = New Point(4, 130)
        lnkTwitter.Name = "lnkTwitter"
        lnkTwitter.TabIndex = 2
        lnkTwitter.TabStop = True
        lnkTwitter.Tag = "https://www.twitter.com/IanSJohnston"
        lnkTwitter.Text = "www.twitter.com/IanSJohnston"
        '
        'picCartoon
        '
        picCartoon.BackColor = Color.Transparent
        picCartoon.Location = New Point(240, 12)
        picCartoon.Name = "picCartoon"
        picCartoon.Size = New Size(150, 184)
        picCartoon.SizeMode = PictureBoxSizeMode.Zoom
        picCartoon.TabIndex = 3
        picCartoon.TabStop = False
        '
        'lblSupportBig
        '
        lblSupportBig.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        lblSupportBig.ForeColor = Color.FromArgb(220, 30, 30)
        lblSupportBig.Location = New Point(210, 202)
        lblSupportBig.Name = "lblSupportBig"
        lblSupportBig.Size = New Size(210, 26)
        lblSupportBig.Text = "SUPPORT WinPartsFinder"
        lblSupportBig.TextAlign = ContentAlignment.MiddleCenter
        '
        'btnDonate
        '
        btnDonate.Location = New Point(250, 236)
        btnDonate.Name = "btnDonate"
        btnDonate.Size = New Size(130, 56)
        btnDonate.TabIndex = 4
        btnDonate.Text = "Donate via PayPal"
        btnDonate.UseVisualStyleBackColor = True
        '
        'btnClose
        '
        btnClose.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        btnClose.DialogResult = DialogResult.Cancel
        btnClose.Location = New Point(335, 578)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(90, 28)
        btnClose.TabIndex = 5
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = True
        '
        'tipAbout
        '
        tipAbout.AutoPopDelay = 15000
        tipAbout.InitialDelay = 500
        tipAbout.SetToolTip(lnkWebsite, "Ian Johnston's website.")
        tipAbout.SetToolTip(lnkYouTube, "Ian's YouTube channel - test equipment repairs and more.")
        tipAbout.SetToolTip(lnkTwitter, "Ian on Twitter / X.")
        tipAbout.SetToolTip(btnDonate, "Opens PayPal in your browser to make a donation towards WinPartsFinder.")
        tipAbout.SetToolTip(btnClose, "Close this window (Esc).")
        '
        'AboutForm
        '
        AcceptButton = btnClose
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnClose
        ClientSize = New Size(1080, 620)
        Controls.Add(flpLeft)
        Controls.Add(pnlRight)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "AboutForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "About WinPartsFinder"
        flpLeft.ResumeLayout(False)
        flpLeft.PerformLayout()
        flpTitle.ResumeLayout(False)
        flpTitle.PerformLayout()
        pnlRight.ResumeLayout(False)
        pnlRight.PerformLayout()
        CType(picCartoon, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents tipAbout As ToolTip
    Friend WithEvents flpLeft As FlowLayoutPanel
    Friend WithEvents flpTitle As FlowLayoutPanel
    Friend WithEvents lblTitle As Label
    Friend WithEvents lblVersion As Label
    Friend WithEvents lblCreditsHead As Label
    Friend WithEvents lblCredits As Label
    Friend WithEvents lblDevHead As Label
    Friend WithEvents lblDev As Label
    Friend WithEvents lblSupportHead As Label
    Friend WithEvents lblSupport As Label
    Friend WithEvents lblLicenceHead As Label
    Friend WithEvents lblLicence As Label
    Friend WithEvents lblDisclaimerHead As Label
    Friend WithEvents lblDisclaimer As Label
    Friend WithEvents pnlRight As Panel
    Friend WithEvents lblAuthor As Label
    Friend WithEvents lnkWebsite As LinkLabel
    Friend WithEvents lnkYouTube As LinkLabel
    Friend WithEvents lnkTwitter As LinkLabel
    Friend WithEvents picCartoon As PictureBox
    Friend WithEvents lblSupportBig As Label
    Friend WithEvents btnDonate As Button
    Friend WithEvents btnClose As Button

End Class
