<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class SettingsForm
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
        tipSettings = New ToolTip(components)
        tlpMain = New TableLayoutPanel()
        lblMouser = New Label()
        txtMouserKey = New TextBox()
        lnkMouser = New LinkLabel()
        lblDkId = New Label()
        txtDkClientId = New TextBox()
        lnkDigiKey = New LinkLabel()
        lblDkSecret = New Label()
        txtDkSecret = New TextBox()
        lblDkLocale = New Label()
        flpDkLocale = New FlowLayoutPanel()
        cboDkSite = New ComboBox()
        lblDkCurrency = New Label()
        cboDkCurrency = New ComboBox()
        lblE14 = New Label()
        txtE14Key = New TextBox()
        lnkE14 = New LinkLabel()
        lblTmeToken = New Label()
        txtTmeToken = New TextBox()
        lnkTme = New LinkLabel()
        lblTmeSecret = New Label()
        txtTmeSecret = New TextBox()
        lblUrls = New Label()
        dgvUrls = New DataGridView()
        colUrlSupplier = New DataGridViewTextBoxColumn()
        colUrlTemplate = New DataGridViewTextBoxColumn()
        flpUrlButtons = New FlowLayoutPanel()
        btnAddSupplier = New Button()
        btnDeleteSupplier = New Button()
        btnResetWindow = New Button()
        chkDarkMode = New CheckBox()
        flpButtons = New FlowLayoutPanel()
        btnCancel = New Button()
        btnOK = New Button()
        btnResetUrls = New Button()
        chkShowKeys = New CheckBox()
        chkShowThumbnails = New CheckBox()
        tlpMain.SuspendLayout()
        flpDkLocale.SuspendLayout()
        CType(dgvUrls, System.ComponentModel.ISupportInitialize).BeginInit()
        flpUrlButtons.SuspendLayout()
        flpButtons.SuspendLayout()
        SuspendLayout()
        '
        'tlpMain
        '
        tlpMain.ColumnCount = 3
        tlpMain.ColumnStyles.Add(New ColumnStyle())
        tlpMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpMain.ColumnStyles.Add(New ColumnStyle())
        tlpMain.Controls.Add(lblMouser, 0, 0)
        tlpMain.Controls.Add(txtMouserKey, 1, 0)
        tlpMain.Controls.Add(lnkMouser, 2, 0)
        tlpMain.Controls.Add(lblDkId, 0, 1)
        tlpMain.Controls.Add(txtDkClientId, 1, 1)
        tlpMain.Controls.Add(lnkDigiKey, 2, 1)
        tlpMain.Controls.Add(lblDkSecret, 0, 2)
        tlpMain.Controls.Add(txtDkSecret, 1, 2)
        tlpMain.Controls.Add(lblDkLocale, 0, 3)
        tlpMain.Controls.Add(flpDkLocale, 1, 3)
        tlpMain.Controls.Add(lblE14, 0, 4)
        tlpMain.Controls.Add(txtE14Key, 1, 4)
        tlpMain.Controls.Add(lnkE14, 2, 4)
        tlpMain.Controls.Add(lblTmeToken, 0, 5)
        tlpMain.Controls.Add(txtTmeToken, 1, 5)
        tlpMain.Controls.Add(lnkTme, 2, 5)
        tlpMain.Controls.Add(lblTmeSecret, 0, 6)
        tlpMain.Controls.Add(txtTmeSecret, 1, 6)
        tlpMain.Controls.Add(lblUrls, 0, 7)
        tlpMain.Controls.Add(dgvUrls, 0, 8)
        tlpMain.Controls.Add(flpUrlButtons, 0, 9)
        tlpMain.Dock = DockStyle.Fill
        tlpMain.Location = New Point(0, 0)
        tlpMain.Name = "tlpMain"
        tlpMain.Padding = New Padding(8)
        tlpMain.RowCount = 10
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle(SizeType.Percent, 100.0F))
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.Size = New Size(684, 421)
        tlpMain.TabIndex = 0
        '
        'lblMouser
        '
        lblMouser.Anchor = AnchorStyles.Left
        lblMouser.AutoSize = True
        lblMouser.Name = "lblMouser"
        lblMouser.Text = "Mouser Search API key:"
        '
        'txtMouserKey
        '
        txtMouserKey.Dock = DockStyle.Fill
        txtMouserKey.Name = "txtMouserKey"
        txtMouserKey.TabIndex = 0
        txtMouserKey.UseSystemPasswordChar = True
        '
        'lnkMouser
        '
        lnkMouser.Anchor = AnchorStyles.Left
        lnkMouser.AutoSize = True
        lnkMouser.Name = "lnkMouser"
        lnkMouser.TabStop = True
        lnkMouser.Tag = "https://www.mouser.co.uk/api-hub/"
        lnkMouser.Text = "Get a key"
        '
        'lblDkId
        '
        lblDkId.Anchor = AnchorStyles.Left
        lblDkId.AutoSize = True
        lblDkId.Name = "lblDkId"
        lblDkId.Text = "DigiKey Client ID:"
        '
        'txtDkClientId
        '
        txtDkClientId.Dock = DockStyle.Fill
        txtDkClientId.Name = "txtDkClientId"
        txtDkClientId.TabIndex = 1
        txtDkClientId.UseSystemPasswordChar = True
        '
        'lnkDigiKey
        '
        lnkDigiKey.Anchor = AnchorStyles.Left
        lnkDigiKey.AutoSize = True
        lnkDigiKey.Name = "lnkDigiKey"
        lnkDigiKey.TabStop = True
        lnkDigiKey.Tag = "https://developer.digikey.com/"
        lnkDigiKey.Text = "Get a key"
        '
        'lblDkSecret
        '
        lblDkSecret.Anchor = AnchorStyles.Left
        lblDkSecret.AutoSize = True
        lblDkSecret.Name = "lblDkSecret"
        lblDkSecret.Text = "DigiKey Client Secret:"
        '
        'txtDkSecret
        '
        txtDkSecret.Dock = DockStyle.Fill
        txtDkSecret.Name = "txtDkSecret"
        txtDkSecret.TabIndex = 2
        txtDkSecret.UseSystemPasswordChar = True
        '
        'lblDkLocale
        '
        lblDkLocale.Anchor = AnchorStyles.Left
        lblDkLocale.AutoSize = True
        lblDkLocale.Name = "lblDkLocale"
        lblDkLocale.Text = "DigiKey site:"
        '
        'flpDkLocale
        '
        flpDkLocale.AutoSize = True
        flpDkLocale.Controls.Add(cboDkSite)
        flpDkLocale.Controls.Add(lblDkCurrency)
        flpDkLocale.Controls.Add(cboDkCurrency)
        flpDkLocale.Margin = New Padding(0)
        flpDkLocale.Name = "flpDkLocale"
        flpDkLocale.TabIndex = 3
        '
        'cboDkSite
        '
        cboDkSite.Items.AddRange(New Object() {"UK", "US", "IE", "DE", "FR", "NL", "CA", "AU"})
        cboDkSite.Name = "cboDkSite"
        cboDkSite.Size = New Size(70, 23)
        cboDkSite.TabIndex = 0
        '
        'lblDkCurrency
        '
        lblDkCurrency.AutoSize = True
        lblDkCurrency.Margin = New Padding(12, 7, 3, 0)
        lblDkCurrency.Name = "lblDkCurrency"
        lblDkCurrency.Text = "Currency:"
        '
        'cboDkCurrency
        '
        cboDkCurrency.Items.AddRange(New Object() {"GBP", "EUR", "USD", "CAD", "AUD"})
        cboDkCurrency.Name = "cboDkCurrency"
        cboDkCurrency.Size = New Size(70, 23)
        cboDkCurrency.TabIndex = 1
        '
        'lblE14
        '
        lblE14.Anchor = AnchorStyles.Left
        lblE14.AutoSize = True
        lblE14.Name = "lblE14"
        lblE14.Text = "element14 API key" & vbCrLf & "(Farnell, CPC, Newark):"
        '
        'txtE14Key
        '
        txtE14Key.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtE14Key.Name = "txtE14Key"
        txtE14Key.TabIndex = 4
        txtE14Key.UseSystemPasswordChar = True
        '
        'lnkE14
        '
        lnkE14.Anchor = AnchorStyles.Left
        lnkE14.AutoSize = True
        lnkE14.Name = "lnkE14"
        lnkE14.TabStop = True
        lnkE14.Tag = "https://partner.element14.com/"
        lnkE14.Text = "Get a key"
        '
        'lblTmeToken
        '
        lblTmeToken.Anchor = AnchorStyles.Left
        lblTmeToken.AutoSize = True
        lblTmeToken.Name = "lblTmeToken"
        lblTmeToken.Text = "TME Token:"
        '
        'txtTmeToken
        '
        txtTmeToken.Dock = DockStyle.Fill
        txtTmeToken.Name = "txtTmeToken"
        txtTmeToken.TabIndex = 4
        txtTmeToken.UseSystemPasswordChar = True
        '
        'lnkTme
        '
        lnkTme.Anchor = AnchorStyles.Left
        lnkTme.AutoSize = True
        lnkTme.Name = "lnkTme"
        lnkTme.TabStop = True
        lnkTme.Tag = "https://developers.tme.eu/en"
        lnkTme.Text = "Get a key"
        '
        'lblTmeSecret
        '
        lblTmeSecret.Anchor = AnchorStyles.Left
        lblTmeSecret.AutoSize = True
        lblTmeSecret.Name = "lblTmeSecret"
        lblTmeSecret.Text = "TME Application secret:"
        '
        'txtTmeSecret
        '
        txtTmeSecret.Dock = DockStyle.Fill
        txtTmeSecret.Name = "txtTmeSecret"
        txtTmeSecret.TabIndex = 4
        txtTmeSecret.UseSystemPasswordChar = True
        '
        'lblUrls
        '
        lblUrls.AutoSize = True
        tlpMain.SetColumnSpan(lblUrls, 3)
        lblUrls.Margin = New Padding(3, 12, 3, 3)
        lblUrls.Name = "lblUrls"
        lblUrls.Text = "Suppliers and website search URLs - {0} is replaced by the search text. Add your own suppliers here:"
        '
        'dgvUrls
        '
        dgvUrls.AllowUserToAddRows = False
        dgvUrls.AllowUserToDeleteRows = False
        dgvUrls.AllowUserToResizeRows = False
        dgvUrls.BackgroundColor = SystemColors.Window
        dgvUrls.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvUrls.Columns.AddRange(New DataGridViewColumn() {colUrlSupplier, colUrlTemplate})
        tlpMain.SetColumnSpan(dgvUrls, 3)
        dgvUrls.Dock = DockStyle.Fill
        dgvUrls.Name = "dgvUrls"
        dgvUrls.RowHeadersVisible = False
        dgvUrls.TabIndex = 5
        '
        'colUrlSupplier
        '
        colUrlSupplier.HeaderText = "Supplier"
        colUrlSupplier.Name = "colUrlSupplier"
        colUrlSupplier.Width = 140
        '
        'colUrlTemplate
        '
        colUrlTemplate.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colUrlTemplate.HeaderText = "Search URL"
        colUrlTemplate.Name = "colUrlTemplate"
        '
        'flpUrlButtons
        '
        flpUrlButtons.AutoSize = True
        tlpMain.SetColumnSpan(flpUrlButtons, 3)
        flpUrlButtons.Controls.Add(btnAddSupplier)
        flpUrlButtons.Controls.Add(btnDeleteSupplier)
        flpUrlButtons.Margin = New Padding(0)
        flpUrlButtons.Name = "flpUrlButtons"
        flpUrlButtons.TabIndex = 6
        '
        'btnAddSupplier
        '
        btnAddSupplier.Name = "btnAddSupplier"
        btnAddSupplier.Size = New Size(110, 28)
        btnAddSupplier.TabIndex = 0
        btnAddSupplier.Text = "&Add supplier"
        btnAddSupplier.UseVisualStyleBackColor = True
        '
        'btnDeleteSupplier
        '
        btnDeleteSupplier.Enabled = False
        btnDeleteSupplier.Name = "btnDeleteSupplier"
        btnDeleteSupplier.Size = New Size(110, 28)
        btnDeleteSupplier.TabIndex = 1
        btnDeleteSupplier.Text = "&Delete supplier"
        btnDeleteSupplier.UseVisualStyleBackColor = True
        '
        'btnResetWindow
        '
        btnResetWindow.Margin = New Padding(3, 3, 24, 3)
        btnResetWindow.Name = "btnResetWindow"
        btnResetWindow.Size = New Size(170, 28)
        btnResetWindow.TabIndex = 2
        btnResetWindow.Text = "Reset main &window layout"
        btnResetWindow.UseVisualStyleBackColor = True
        '
        'chkDarkMode
        '
        chkDarkMode.AutoSize = True
        chkDarkMode.Margin = New Padding(3, 8, 12, 3)
        chkDarkMode.Name = "chkDarkMode"
        chkDarkMode.TabIndex = 5
        chkDarkMode.Text = "Dark mode"
        chkDarkMode.UseVisualStyleBackColor = True
        '
        'flpButtons
        '
        flpButtons.AutoSize = True
        flpButtons.Controls.Add(btnCancel)
        flpButtons.Controls.Add(btnOK)
        flpButtons.Controls.Add(btnResetWindow)
        flpButtons.Controls.Add(btnResetUrls)
        flpButtons.Controls.Add(chkShowKeys)
        flpButtons.Controls.Add(chkShowThumbnails)
        flpButtons.Controls.Add(chkDarkMode)
        flpButtons.Dock = DockStyle.Bottom
        flpButtons.FlowDirection = FlowDirection.RightToLeft
        flpButtons.Name = "flpButtons"
        flpButtons.Padding = New Padding(8)
        flpButtons.TabIndex = 1
        '
        'btnCancel
        '
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(85, 28)
        btnCancel.TabIndex = 3
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        'btnOK
        '
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(85, 28)
        btnOK.TabIndex = 2
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = True
        '
        'btnResetUrls
        '
        btnResetUrls.Margin = New Padding(3, 3, 3, 3)
        btnResetUrls.Name = "btnResetUrls"
        btnResetUrls.Size = New Size(120, 28)
        btnResetUrls.TabIndex = 1
        btnResetUrls.Text = "Reset URLs"
        btnResetUrls.UseVisualStyleBackColor = True
        '
        'chkShowKeys
        '
        chkShowKeys.AutoSize = True
        chkShowKeys.Margin = New Padding(3, 8, 3, 3)
        chkShowKeys.Name = "chkShowKeys"
        chkShowKeys.TabIndex = 0
        chkShowKeys.Text = "Show keys"
        chkShowKeys.UseVisualStyleBackColor = True
        '
        'chkShowThumbnails
        '
        chkShowThumbnails.AutoSize = True
        chkShowThumbnails.Margin = New Padding(3, 8, 12, 3)
        chkShowThumbnails.Name = "chkShowThumbnails"
        chkShowThumbnails.TabIndex = 4
        chkShowThumbnails.Text = "Show part photos"
        chkShowThumbnails.UseVisualStyleBackColor = True
        '
        'tipSettings
        '
        tipSettings.AutoPopDelay = 15000
        tipSettings.InitialDelay = 500
        tipSettings.ReshowDelay = 100
        tipSettings.SetToolTip(txtMouserKey, "Your Mouser Search API key (a long code with dashes)." & vbCrLf & "Leave blank to use Mouser as a website link only.")
        tipSettings.SetToolTip(lnkMouser, "Opens Mouser's API Hub - sign up there for a free Search API key.")
        tipSettings.SetToolTip(txtDkClientId, "Client ID of your DigiKey Production App (with ProductInformation V4 ticked).")
        tipSettings.SetToolTip(txtDkSecret, "Client Secret of the same DigiKey app. Keep it private, like a password.")
        tipSettings.SetToolTip(lnkDigiKey, "Opens the DigiKey developer portal - create an Organization, then a Production App.")
        tipSettings.SetToolTip(cboDkSite, "Which DigiKey country site to price and stock from (UK = digikey.co.uk).")
        tipSettings.SetToolTip(cboDkCurrency, "Currency DigiKey prices are shown in.")
        tipSettings.SetToolTip(txtE14Key, "Your element14 Product Search API key. One key covers both Farnell and Newark.")
        tipSettings.SetToolTip(lnkE14, "Opens the element14 partner portal - register an application for a free key.")
        tipSettings.SetToolTip(txtTmeToken, "Your TME private key Token (50 characters), from developers.tme.eu > API Token keys > Show private keys." & vbCrLf & "Leave blank to use TME as a website link only.")
        tipSettings.SetToolTip(txtTmeSecret, "The Application secret (20 characters) of your app at developers.tme.eu. Keep it private, like a password.")
        tipSettings.SetToolTip(lnkTme, "Opens the TME developer site - create an application there, then generate a private key (see Help).")
        tipSettings.SetToolTip(dgvUrls, "Website search URL for each supplier. {0} is replaced by your search text." & vbCrLf & "Used for website-link rows and for suppliers without an API. Click a URL to edit it.")
        tipSettings.SetToolTip(btnAddSupplier, "Add your own supplier: give it a name and a search URL containing {0}." & vbCrLf & "It shows as a website link in the results.")
        tipSettings.SetToolTip(btnDeleteSupplier, "Delete the selected supplier you added. Built-in suppliers can't be deleted - untick them on the main window instead.")
        tipSettings.SetToolTip(chkDarkMode, "Switch the whole app to a dark theme. Previews here straight away.")
        tipSettings.SetToolTip(chkShowThumbnails, "Show a small part photo at the start of each result row. Untick for a compact grid.")
        tipSettings.SetToolTip(chkShowKeys, "Show the API keys as plain text instead of dots.")
        tipSettings.SetToolTip(btnResetUrls, "Put the built-in suppliers' search URLs back to the defaults. Your own suppliers are kept.")
        tipSettings.SetToolTip(btnResetWindow, "Put the main window back to its original size and position, with the default column widths and order." & vbCrLf & "Happens when you click OK.")
        tipSettings.SetToolTip(btnOK, "Save these settings. Keys are stored encrypted for your Windows user only.")
        tipSettings.SetToolTip(btnCancel, "Close without saving any changes.")
        '
        'SettingsForm
        '
        AcceptButton = btnOK
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New Size(900, 590)
        Controls.Add(tlpMain)
        Controls.Add(flpButtons)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(880, 560)
        Name = "SettingsForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "WinPartsFinder settings"
        tlpMain.ResumeLayout(False)
        tlpMain.PerformLayout()
        flpDkLocale.ResumeLayout(False)
        flpDkLocale.PerformLayout()
        CType(dgvUrls, System.ComponentModel.ISupportInitialize).EndInit()
        flpUrlButtons.ResumeLayout(False)
        flpButtons.ResumeLayout(False)
        flpButtons.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tipSettings As ToolTip
    Friend WithEvents tlpMain As TableLayoutPanel
    Friend WithEvents lblMouser As Label
    Friend WithEvents txtMouserKey As TextBox
    Friend WithEvents lnkMouser As LinkLabel
    Friend WithEvents lblDkId As Label
    Friend WithEvents txtDkClientId As TextBox
    Friend WithEvents lnkDigiKey As LinkLabel
    Friend WithEvents lblDkSecret As Label
    Friend WithEvents txtDkSecret As TextBox
    Friend WithEvents lblDkLocale As Label
    Friend WithEvents flpDkLocale As FlowLayoutPanel
    Friend WithEvents cboDkSite As ComboBox
    Friend WithEvents lblDkCurrency As Label
    Friend WithEvents cboDkCurrency As ComboBox
    Friend WithEvents lblE14 As Label
    Friend WithEvents txtE14Key As TextBox
    Friend WithEvents lnkE14 As LinkLabel
    Friend WithEvents lblTmeToken As Label
    Friend WithEvents txtTmeToken As TextBox
    Friend WithEvents lnkTme As LinkLabel
    Friend WithEvents lblTmeSecret As Label
    Friend WithEvents txtTmeSecret As TextBox
    Friend WithEvents lblUrls As Label
    Friend WithEvents dgvUrls As DataGridView
    Friend WithEvents colUrlSupplier As DataGridViewTextBoxColumn
    Friend WithEvents colUrlTemplate As DataGridViewTextBoxColumn
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnOK As Button
    Friend WithEvents btnResetUrls As Button
    Friend WithEvents chkShowKeys As CheckBox
    Friend WithEvents chkShowThumbnails As CheckBox
    Friend WithEvents flpUrlButtons As FlowLayoutPanel
    Friend WithEvents btnAddSupplier As Button
    Friend WithEvents btnDeleteSupplier As Button
    Friend WithEvents btnResetWindow As Button
    Friend WithEvents chkDarkMode As CheckBox

End Class
