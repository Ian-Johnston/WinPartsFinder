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
        flpHdrMouser = New FlowLayoutPanel()
        flpHdrDigiKey = New FlowLayoutPanel()
        flpHdrE14 = New FlowLayoutPanel()
        flpHdrTme = New FlowLayoutPanel()
        lblHdrMouser = New Label()
        lblHdrDigiKey = New Label()
        lblHdrE14 = New Label()
        lblHdrTme = New Label()
        lblPricesRule = New Label()
        lblPricesRuleEnd = New Label()
        lblHdrPrices = New Label()
        lblConvert = New Label()
        flpConvert = New FlowLayoutPanel()
        cboConvertCurrency = New ComboBox()
        lblRates = New Label()
        btnUpdateRates = New Button()
        lnkRates = New LinkLabel()
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
        lblE14Store = New Label()
        cboFarnellStore = New ComboBox()
        lblTmeToken = New Label()
        txtTmeToken = New TextBox()
        lnkTme = New LinkLabel()
        lblTmeSecret = New Label()
        txtTmeSecret = New TextBox()
        lblTmeLocale = New Label()
        flpTmeLocale = New FlowLayoutPanel()
        cboTmeCountry = New ComboBox()
        lblTmeCurrency = New Label()
        cboTmeCurrency = New ComboBox()
        lblUrls = New Label()
        btnApiHelp = New Button()
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
        flpTmeLocale.SuspendLayout()
        flpHdrMouser.SuspendLayout()
        flpHdrDigiKey.SuspendLayout()
        flpHdrE14.SuspendLayout()
        flpHdrTme.SuspendLayout()
        flpConvert.SuspendLayout()
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
        tlpMain.Controls.Add(flpHdrMouser, 0, 0)
        tlpMain.Controls.Add(lblMouser, 0, 1)
        tlpMain.Controls.Add(txtMouserKey, 1, 1)
        tlpMain.Controls.Add(flpHdrDigiKey, 0, 2)
        tlpMain.Controls.Add(lblDkId, 0, 3)
        tlpMain.Controls.Add(txtDkClientId, 1, 3)
        tlpMain.Controls.Add(lblDkSecret, 0, 4)
        tlpMain.Controls.Add(txtDkSecret, 1, 4)
        tlpMain.Controls.Add(lblDkLocale, 0, 5)
        tlpMain.Controls.Add(flpDkLocale, 1, 5)
        tlpMain.Controls.Add(flpHdrE14, 0, 6)
        tlpMain.Controls.Add(lblE14, 0, 7)
        tlpMain.Controls.Add(txtE14Key, 1, 7)
        tlpMain.Controls.Add(lblE14Store, 0, 8)
        tlpMain.Controls.Add(cboFarnellStore, 1, 8)
        tlpMain.Controls.Add(flpHdrTme, 0, 9)
        tlpMain.Controls.Add(lblTmeToken, 0, 10)
        tlpMain.Controls.Add(txtTmeToken, 1, 10)
        tlpMain.Controls.Add(lblTmeSecret, 0, 11)
        tlpMain.Controls.Add(txtTmeSecret, 1, 11)
        tlpMain.Controls.Add(lblTmeLocale, 0, 12)
        tlpMain.Controls.Add(flpTmeLocale, 1, 12)
        tlpMain.Controls.Add(lblPricesRule, 0, 13)
        tlpMain.Controls.Add(lblHdrPrices, 0, 14)
        tlpMain.Controls.Add(lblConvert, 0, 15)
        tlpMain.Controls.Add(flpConvert, 1, 15)
        tlpMain.Controls.Add(lblPricesRuleEnd, 0, 16)
        tlpMain.Controls.Add(lblUrls, 0, 17)
        tlpMain.Controls.Add(btnApiHelp, 2, 12)
        tlpMain.Controls.Add(dgvUrls, 0, 18)
        tlpMain.Controls.Add(flpUrlButtons, 0, 19)
        tlpMain.Dock = DockStyle.Fill
        tlpMain.Location = New Point(0, 0)
        tlpMain.Name = "tlpMain"
        tlpMain.Padding = New Padding(8)
        tlpMain.RowCount = 20
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
        tlpMain.RowStyles.Add(New RowStyle())
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
        'flpHdrMouser - supplier heading with its Get a key link right after it
        '
        flpHdrMouser.AutoSize = True
        flpHdrMouser.Controls.Add(lblHdrMouser)
        flpHdrMouser.Controls.Add(lnkMouser)
        tlpMain.SetColumnSpan(flpHdrMouser, 3)
        flpHdrMouser.Margin = New Padding(0, 0, 0, 0)
        flpHdrMouser.Name = "flpHdrMouser"
        flpHdrMouser.WrapContents = False
        '
        'lblHdrMouser - supplier heading
        '
        lblHdrMouser.AutoSize = True
        lblHdrMouser.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblHdrMouser.Margin = New Padding(3, 6, 3, 0) ' lower, so the Get a key underline lines up with the bottom of the heading
        lblHdrMouser.Name = "lblHdrMouser"
        lblHdrMouser.Text = "Mouser"
        '
        'flpHdrDigiKey - supplier heading with its Get a key link right after it
        '
        flpHdrDigiKey.AutoSize = True
        flpHdrDigiKey.Controls.Add(lblHdrDigiKey)
        flpHdrDigiKey.Controls.Add(lnkDigiKey)
        tlpMain.SetColumnSpan(flpHdrDigiKey, 3)
        flpHdrDigiKey.Margin = New Padding(0, 8, 0, 0)
        flpHdrDigiKey.Name = "flpHdrDigiKey"
        flpHdrDigiKey.WrapContents = False
        '
        'lblHdrDigiKey - supplier heading
        '
        lblHdrDigiKey.AutoSize = True
        lblHdrDigiKey.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblHdrDigiKey.Margin = New Padding(3, 6, 3, 0) ' lower, so the Get a key underline lines up with the bottom of the heading
        lblHdrDigiKey.Name = "lblHdrDigiKey"
        lblHdrDigiKey.Text = "DigiKey"
        '
        'flpHdrE14 - supplier heading with its Get a key link right after it
        '
        flpHdrE14.AutoSize = True
        flpHdrE14.Controls.Add(lblHdrE14)
        flpHdrE14.Controls.Add(lnkE14)
        tlpMain.SetColumnSpan(flpHdrE14, 3)
        flpHdrE14.Margin = New Padding(0, 8, 0, 0)
        flpHdrE14.Name = "flpHdrE14"
        flpHdrE14.WrapContents = False
        '
        'lblHdrE14 - supplier heading
        '
        lblHdrE14.AutoSize = True
        lblHdrE14.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblHdrE14.Margin = New Padding(3, 6, 3, 0) ' lower, so the Get a key underline lines up with the bottom of the heading
        lblHdrE14.Name = "lblHdrE14"
        lblHdrE14.Text = "element14 - Farnell, CPC and Newark (one key for all three)"
        '
        'flpHdrTme - supplier heading with its Get a key link right after it
        '
        flpHdrTme.AutoSize = True
        flpHdrTme.Controls.Add(lblHdrTme)
        flpHdrTme.Controls.Add(lnkTme)
        tlpMain.SetColumnSpan(flpHdrTme, 3)
        flpHdrTme.Margin = New Padding(0, 8, 0, 0)
        flpHdrTme.Name = "flpHdrTme"
        flpHdrTme.WrapContents = False
        '
        'lblHdrTme - supplier heading
        '
        lblHdrTme.AutoSize = True
        lblHdrTme.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblHdrTme.Margin = New Padding(3, 6, 3, 0) ' lower, so the Get a key underline lines up with the bottom of the heading
        lblHdrTme.Name = "lblHdrTme"
        lblHdrTme.Text = "TME"
        '
        'lblPricesRule - grey line between the API keys and the Prices section
        '
        lblPricesRule.AutoSize = False
        lblPricesRule.BorderStyle = BorderStyle.Fixed3D
        tlpMain.SetColumnSpan(lblPricesRule, 3)
        lblPricesRule.Dock = DockStyle.Fill
        lblPricesRule.Margin = New Padding(3, 7, 3, 0)
        lblPricesRule.Name = "lblPricesRule"
        lblPricesRule.Size = New Size(100, 2)
        '
        'lblPricesRuleEnd - grey line under the Prices section
        '
        lblPricesRuleEnd.AutoSize = False
        lblPricesRuleEnd.BorderStyle = BorderStyle.Fixed3D
        tlpMain.SetColumnSpan(lblPricesRuleEnd, 3)
        lblPricesRuleEnd.Dock = DockStyle.Fill
        lblPricesRuleEnd.Margin = New Padding(3, 8, 3, 0)
        lblPricesRuleEnd.Name = "lblPricesRuleEnd"
        lblPricesRuleEnd.Size = New Size(100, 2)
        '
        'lblHdrPrices - heading for "Prices in GBP"
        '
        lblHdrPrices.AutoSize = True
        tlpMain.SetColumnSpan(lblHdrPrices, 2)
        lblHdrPrices.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblHdrPrices.Margin = New Padding(3, 10, 3, 3)
        lblHdrPrices.Name = "lblHdrPrices"
        lblHdrPrices.Text = "Prices - Single currency (if enabled)"
        '
        'lblConvert
        '
        lblConvert.Anchor = AnchorStyles.Left
        lblConvert.AutoSize = True
        lblConvert.Margin = New Padding(20, 3, 3, 3)
        lblConvert.Name = "lblConvert"
        lblConvert.Text = "Show prices in:"
        '
        'flpConvert
        '
        flpConvert.AutoSize = True
        flpConvert.Controls.Add(cboConvertCurrency)
        flpConvert.Controls.Add(lblRates)
        flpConvert.Controls.Add(btnUpdateRates)
        flpConvert.Controls.Add(lnkRates)
        flpConvert.Margin = New Padding(0)
        flpConvert.WrapContents = False
        flpConvert.Name = "flpConvert"
        flpConvert.TabIndex = 6
        '
        'cboConvertCurrency - filled from ExchangeRates.Currencies
        '
        cboConvertCurrency.DropDownStyle = ComboBoxStyle.DropDownList
        cboConvertCurrency.MaxDropDownItems = 25
        cboConvertCurrency.Name = "cboConvertCurrency"
        cboConvertCurrency.Size = New Size(70, 23)
        cboConvertCurrency.TabIndex = 0
        '
        'lblRates - "Rates: ECB rates of 23 Oct 2026 (fetched 25 Oct 10:02)"
        '
        lblRates.AutoSize = True
        lblRates.Margin = New Padding(12, 7, 3, 0)
        lblRates.Name = "lblRates"
        lblRates.Text = "Rates:"
        '
        'btnUpdateRates
        '
        btnUpdateRates.Margin = New Padding(12, 1, 3, 3)
        btnUpdateRates.Name = "btnUpdateRates"
        btnUpdateRates.Size = New Size(95, 25)
        btnUpdateRates.TabIndex = 1
        btnUpdateRates.Text = "Update now"
        btnUpdateRates.UseVisualStyleBackColor = True
        '
        'lnkRates - the exchange-rate source's own page
        '
        lnkRates.AutoSize = True
        lnkRates.Margin = New Padding(12, 7, 3, 0)
        lnkRates.Name = "lnkRates"
        lnkRates.TabIndex = 2
        lnkRates.TabStop = True
        lnkRates.Tag = "https://www.ecb.europa.eu/stats/policy_and_exchange_rates/euro_reference_exchange_rates/html/index.en.html"
        lnkRates.Text = "ECB rates webpage"
        '
        'lblMouser
        '
        lblMouser.Anchor = AnchorStyles.Left
        lblMouser.AutoSize = True
        lblMouser.Name = "lblMouser"
        lblMouser.Margin = New Padding(20, 3, 3, 3)
        lblMouser.Text = "Search API key:"
        '
        'txtMouserKey
        '
        txtMouserKey.Dock = DockStyle.Fill
        tlpMain.SetColumnSpan(txtMouserKey, 2)
        txtMouserKey.Name = "txtMouserKey"
        txtMouserKey.TabIndex = 0
        txtMouserKey.UseSystemPasswordChar = True
        '
        'lnkMouser
        '
        lnkMouser.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        lnkMouser.AutoSize = True
        lnkMouser.Margin = New Padding(10, 3, 3, 1)
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
        lblDkId.Margin = New Padding(20, 3, 3, 3)
        lblDkId.Text = "Client ID:"
        '
        'txtDkClientId
        '
        txtDkClientId.Dock = DockStyle.Fill
        tlpMain.SetColumnSpan(txtDkClientId, 2)
        txtDkClientId.Name = "txtDkClientId"
        txtDkClientId.TabIndex = 1
        txtDkClientId.UseSystemPasswordChar = True
        '
        'lnkDigiKey
        '
        lnkDigiKey.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        lnkDigiKey.AutoSize = True
        lnkDigiKey.Margin = New Padding(10, 3, 3, 1)
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
        lblDkSecret.Margin = New Padding(20, 3, 3, 3)
        lblDkSecret.Text = "Client Secret:"
        '
        'txtDkSecret
        '
        txtDkSecret.Dock = DockStyle.Fill
        tlpMain.SetColumnSpan(txtDkSecret, 2)
        txtDkSecret.Name = "txtDkSecret"
        txtDkSecret.TabIndex = 2
        txtDkSecret.UseSystemPasswordChar = True
        '
        'lblDkLocale
        '
        lblDkLocale.Anchor = AnchorStyles.Left
        lblDkLocale.AutoSize = True
        lblDkLocale.Name = "lblDkLocale"
        lblDkLocale.Margin = New Padding(20, 3, 3, 3)
        lblDkLocale.Text = "Site:"
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
        cboDkSite.Name = "cboDkSite"
        cboDkSite.MaxDropDownItems = 20
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
        cboDkCurrency.Name = "cboDkCurrency"
        cboDkCurrency.MaxDropDownItems = 20
        cboDkCurrency.Size = New Size(70, 23)
        cboDkCurrency.TabIndex = 1
        '
        'lblE14
        '
        lblE14.Anchor = AnchorStyles.Left
        lblE14.AutoSize = True
        lblE14.Name = "lblE14"
        lblE14.Margin = New Padding(20, 3, 3, 3)
        lblE14.Text = "API key:"
        '
        'txtE14Key
        '
        txtE14Key.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        tlpMain.SetColumnSpan(txtE14Key, 2)
        txtE14Key.Name = "txtE14Key"
        txtE14Key.TabIndex = 4
        txtE14Key.UseSystemPasswordChar = True
        '
        'lnkE14
        '
        lnkE14.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        lnkE14.AutoSize = True
        lnkE14.Margin = New Padding(10, 3, 3, 1)
        lnkE14.Name = "lnkE14"
        lnkE14.TabStop = True
        lnkE14.Tag = "https://partner.element14.com/"
        lnkE14.Text = "Get a key"
        '
        'lblE14Store
        '
        lblE14Store.Anchor = AnchorStyles.Left
        lblE14Store.AutoSize = True
        lblE14Store.Name = "lblE14Store"
        lblE14Store.Margin = New Padding(20, 3, 3, 3)
        lblE14Store.Text = "Farnell store (country):"
        '
        'cboFarnellStore - filled from Element14Supplier.FarnellStores
        '
        cboFarnellStore.DropDownStyle = ComboBoxStyle.DropDownList
        cboFarnellStore.MaxDropDownItems = 20
        cboFarnellStore.Name = "cboFarnellStore"
        cboFarnellStore.Size = New Size(320, 23)
        cboFarnellStore.TabIndex = 4
        '
        'lblTmeToken
        '
        lblTmeToken.Anchor = AnchorStyles.Left
        lblTmeToken.AutoSize = True
        lblTmeToken.Name = "lblTmeToken"
        lblTmeToken.Margin = New Padding(20, 3, 3, 3)
        lblTmeToken.Text = "Token:"
        '
        'txtTmeToken
        '
        txtTmeToken.Dock = DockStyle.Fill
        tlpMain.SetColumnSpan(txtTmeToken, 2)
        txtTmeToken.Name = "txtTmeToken"
        txtTmeToken.TabIndex = 4
        txtTmeToken.UseSystemPasswordChar = True
        '
        'lnkTme
        '
        lnkTme.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom
        lnkTme.AutoSize = True
        lnkTme.Margin = New Padding(10, 3, 3, 1)
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
        lblTmeSecret.Margin = New Padding(20, 3, 3, 3)
        lblTmeSecret.Text = "Application secret:"
        '
        'txtTmeSecret
        '
        txtTmeSecret.Dock = DockStyle.Fill
        tlpMain.SetColumnSpan(txtTmeSecret, 2)
        txtTmeSecret.Name = "txtTmeSecret"
        txtTmeSecret.TabIndex = 4
        txtTmeSecret.UseSystemPasswordChar = True
        '
        'lblTmeLocale
        '
        lblTmeLocale.Anchor = AnchorStyles.Left
        lblTmeLocale.AutoSize = True
        lblTmeLocale.Name = "lblTmeLocale"
        lblTmeLocale.Margin = New Padding(20, 3, 3, 3)
        lblTmeLocale.Text = "Country:"
        '
        'flpTmeLocale
        '
        flpTmeLocale.AutoSize = True
        flpTmeLocale.Controls.Add(cboTmeCountry)
        flpTmeLocale.Controls.Add(lblTmeCurrency)
        flpTmeLocale.Controls.Add(cboTmeCurrency)
        flpTmeLocale.Margin = New Padding(0)
        flpTmeLocale.Name = "flpTmeLocale"
        flpTmeLocale.TabIndex = 5
        '
        'cboTmeCountry - filled from TmeSupplier.Countries; any code can be typed
        '
        cboTmeCountry.MaxDropDownItems = 20
        cboTmeCountry.Name = "cboTmeCountry"
        cboTmeCountry.Size = New Size(70, 23)
        cboTmeCountry.TabIndex = 0
        '
        'lblTmeCurrency
        '
        lblTmeCurrency.AutoSize = True
        lblTmeCurrency.Margin = New Padding(12, 7, 3, 0)
        lblTmeCurrency.Name = "lblTmeCurrency"
        lblTmeCurrency.Text = "Currency:"
        '
        'cboTmeCurrency - filled from TmeSupplier.Currencies
        '
        cboTmeCurrency.Name = "cboTmeCurrency"
        cboTmeCurrency.Size = New Size(70, 23)
        cboTmeCurrency.TabIndex = 1
        '
        'lblUrls
        '
        lblUrls.AutoSize = True
        tlpMain.SetColumnSpan(lblUrls, 2)
        lblUrls.Anchor = AnchorStyles.Left Or AnchorStyles.Bottom ' just above the table, not level with the API Help button
        lblUrls.Margin = New Padding(3, 16, 3, 1)
        lblUrls.Name = "lblUrls"
        lblUrls.Text = "Suppliers and website search URLs - {0} is replaced by the search text. Add your own suppliers here:"
        '
        'btnApiHelp - beside the key boxes: how to get each key
        '
        btnApiHelp.Anchor = AnchorStyles.Right Or AnchorStyles.Bottom
        btnApiHelp.Margin = New Padding(3, 3, 3, 0)
        btnApiHelp.Name = "btnApiHelp"
        btnApiHelp.Size = New Size(85, 28)
        btnApiHelp.TabIndex = 5
        btnApiHelp.Text = "API &Help"
        btnApiHelp.UseVisualStyleBackColor = True
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
        tipSettings.SetToolTip(cboDkSite, "Which DigiKey country site to price and stock from (UK = digikey.co.uk, NZ = digikey.co.nz ...)." & vbCrLf & "Pick one, or type any other DigiKey country code.")
        tipSettings.SetToolTip(cboDkCurrency, "Currency DigiKey prices are shown in. Pick one, or type another currency code.")
        tipSettings.SetToolTip(cboFarnellStore, "Which Farnell / element14 country store to use for the Farnell supplier: its stock, prices and currency." & vbCrLf & "The same element14 key works for every store. CPC (UK) and Newark (USA) aren't affected.")
        tipSettings.SetToolTip(cboConvertCurrency, "The currency for the main window's ""Prices in"" tick box. Every price is converted into it using the" & vbCrLf & "European Central Bank's daily rates. Converted prices are shown in purple and are approximate.")
        tipSettings.SetToolTip(lnkRates, "Opens the European Central Bank's euro exchange reference rates page - where the rates come from.")
        tipSettings.SetToolTip(btnUpdateRates, "Fetch today's exchange rates now. (They're also fetched automatically when they're 12 hours old.)")
        tipSettings.SetToolTip(cboTmeCountry, "Country TME prices and stock are for (two-letter code, e.g. GB, IE, NZ). Pick one or type another.")
        tipSettings.SetToolTip(cboTmeCurrency, "Currency TME prices are shown in. TME's own currencies are EUR, USD and PLN; GBP also works.")
        tipSettings.SetToolTip(txtE14Key, "Your element14 Product Search API key. One key covers Farnell, CPC and Newark.")
        tipSettings.SetToolTip(lnkE14, "Opens the element14 partner portal - register an application for a free key.")
        tipSettings.SetToolTip(txtTmeToken, "Your TME private key Token (50 characters), from developers.tme.eu > API Token keys > Show private keys." & vbCrLf & "Leave blank to use TME as a website link only.")
        tipSettings.SetToolTip(txtTmeSecret, "The Application secret (20 characters) of your app at developers.tme.eu. Keep it private, like a password.")
        tipSettings.SetToolTip(lnkTme, "Opens the TME developer site - create an application there, then generate a private key (see API Help).")
        tipSettings.SetToolTip(btnApiHelp, "Step-by-step instructions for getting each supplier's free API keys." & vbCrLf & "Stays open beside Settings while you paste the keys in.")
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
        ClientSize = New Size(900, 818)
        Controls.Add(tlpMain)
        Controls.Add(flpButtons)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(924, 818) ' wide enough that the bottom row of buttons stays on one line
        Name = "SettingsForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "WinPartsFinder settings"
        tlpMain.ResumeLayout(False)
        tlpMain.PerformLayout()
        flpDkLocale.ResumeLayout(False)
        flpDkLocale.PerformLayout()
        flpTmeLocale.ResumeLayout(False)
        flpTmeLocale.PerformLayout()
        flpHdrMouser.ResumeLayout(False)
        flpHdrMouser.PerformLayout()
        flpHdrDigiKey.ResumeLayout(False)
        flpHdrDigiKey.PerformLayout()
        flpHdrE14.ResumeLayout(False)
        flpHdrE14.PerformLayout()
        flpHdrTme.ResumeLayout(False)
        flpHdrTme.PerformLayout()
        flpConvert.ResumeLayout(False)
        flpConvert.PerformLayout()
        CType(dgvUrls, System.ComponentModel.ISupportInitialize).EndInit()
        flpUrlButtons.ResumeLayout(False)
        flpButtons.ResumeLayout(False)
        flpButtons.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tipSettings As ToolTip
    Friend WithEvents tlpMain As TableLayoutPanel
    Friend WithEvents flpHdrMouser As FlowLayoutPanel
    Friend WithEvents lblHdrMouser As Label
    Friend WithEvents flpHdrDigiKey As FlowLayoutPanel
    Friend WithEvents lblHdrDigiKey As Label
    Friend WithEvents flpHdrE14 As FlowLayoutPanel
    Friend WithEvents lblHdrE14 As Label
    Friend WithEvents flpHdrTme As FlowLayoutPanel
    Friend WithEvents lblHdrTme As Label
    Friend WithEvents lblPricesRule As Label
    Friend WithEvents lblPricesRuleEnd As Label
    Friend WithEvents lblHdrPrices As Label
    Friend WithEvents lblConvert As Label
    Friend WithEvents flpConvert As FlowLayoutPanel
    Friend WithEvents cboConvertCurrency As ComboBox
    Friend WithEvents lblRates As Label
    Friend WithEvents btnUpdateRates As Button
    Friend WithEvents lnkRates As LinkLabel
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
    Friend WithEvents lblE14Store As Label
    Friend WithEvents cboFarnellStore As ComboBox
    Friend WithEvents lblTmeToken As Label
    Friend WithEvents txtTmeToken As TextBox
    Friend WithEvents lnkTme As LinkLabel
    Friend WithEvents lblTmeSecret As Label
    Friend WithEvents txtTmeSecret As TextBox
    Friend WithEvents lblTmeLocale As Label
    Friend WithEvents flpTmeLocale As FlowLayoutPanel
    Friend WithEvents cboTmeCountry As ComboBox
    Friend WithEvents lblTmeCurrency As Label
    Friend WithEvents cboTmeCurrency As ComboBox
    Friend WithEvents lblUrls As Label
    Friend WithEvents btnApiHelp As Button
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
