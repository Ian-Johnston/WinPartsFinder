<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
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
        tlpTop = New TableLayoutPanel()
        flpTop = New FlowLayoutPanel()
        lblSearch = New Label()
        txtSearch = New TextBox()
        cboMode = New ComboBox()
        lblMax = New Label()
        nudMax = New NumericUpDown()
        btnSearch = New Button()
        btnCancel = New Button()
        btnLoadMore = New Button()
        chkInStock = New CheckBox()
        flpFilter = New FlowLayoutPanel()
        lblFilter = New Label()
        txtFilter = New TextBox()
        btnExport = New Button()
        btnSettings = New Button()
        btnAbout = New Button()
        btnHelp = New Button()
        tipMain = New ToolTip(components)
        splitMain = New SplitContainer()
        lvSuppliers = New ListView()
        colSupplier = New ColumnHeader()
        colMode = New ColumnHeader()
        colStatus = New ColumnHeader()
        colHits = New ColumnHeader()
        colTime = New ColumnHeader()
        cmsSuppliers = New ContextMenuStrip(components)
        mnuOpenSupplierSearch = New ToolStripMenuItem()
        mnuOpenAllSearches = New ToolStripMenuItem()
        mnuSupplierSep = New ToolStripSeparator()
        mnuLoadMoreSupplier = New ToolStripMenuItem()
        dgvResults = New DataGridView()
        cmsResults = New ContextMenuStrip(components)
        mnuOpenProduct = New ToolStripMenuItem()
        mnuOpenDatasheet = New ToolStripMenuItem()
        mnuSep1 = New ToolStripSeparator()
        mnuCopyMpn = New ToolStripMenuItem()
        mnuCopySupplierPart = New ToolStripMenuItem()
        mnuCopyRow = New ToolStripMenuItem()
        mnuSep2 = New ToolStripSeparator()
        mnuRemoveRows = New ToolStripMenuItem()
        mnuRestoreRows = New ToolStripMenuItem()
        ssMain = New StatusStrip()
        lblStatus = New ToolStripStatusLabel()
        lblTotal = New ToolStripStatusLabel()
        tlpTop.SuspendLayout()
        flpTop.SuspendLayout()
        flpFilter.SuspendLayout()
        CType(nudMax, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(splitMain, System.ComponentModel.ISupportInitialize).BeginInit()
        splitMain.Panel1.SuspendLayout()
        splitMain.Panel2.SuspendLayout()
        splitMain.SuspendLayout()
        cmsSuppliers.SuspendLayout()
        CType(dgvResults, System.ComponentModel.ISupportInitialize).BeginInit()
        cmsResults.SuspendLayout()
        ssMain.SuspendLayout()
        SuspendLayout()
        '
        'tlpTop - toolbar row: the flow of controls on the left (wraps when narrow), Help + About pinned far right
        '
        tlpTop.AutoSize = True
        tlpTop.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpTop.ColumnCount = 3
        tlpTop.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpTop.ColumnStyles.Add(New ColumnStyle())
        tlpTop.ColumnStyles.Add(New ColumnStyle())
        tlpTop.Controls.Add(flpTop, 0, 0)
        tlpTop.Controls.Add(btnHelp, 1, 0)
        tlpTop.Controls.Add(btnAbout, 2, 0)
        tlpTop.Dock = DockStyle.Top
        tlpTop.Location = New Point(0, 0)
        tlpTop.Name = "tlpTop"
        tlpTop.RowCount = 1
        tlpTop.RowStyles.Add(New RowStyle())
        tlpTop.Size = New Size(1264, 43)
        tlpTop.TabIndex = 0
        '
        'flpTop
        '
        flpTop.AutoSize = True
        flpTop.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flpTop.Controls.Add(lblSearch)
        flpTop.Controls.Add(txtSearch)
        flpTop.Controls.Add(cboMode)
        flpTop.Controls.Add(lblMax)
        flpTop.Controls.Add(nudMax)
        flpTop.Controls.Add(btnSearch)
        flpTop.Controls.Add(btnCancel)
        flpTop.Controls.Add(btnLoadMore)
        flpTop.Controls.Add(chkInStock)
        flpTop.Controls.Add(flpFilter)
        flpTop.Controls.Add(btnExport)
        flpTop.Controls.Add(btnSettings)
        flpTop.Dock = DockStyle.Fill
        flpTop.Location = New Point(0, 0)
        flpTop.Margin = New Padding(0)
        flpTop.Name = "flpTop"
        flpTop.Padding = New Padding(6)
        flpTop.Size = New Size(1264, 43)
        flpTop.TabIndex = 0
        '
        'lblSearch
        '
        lblSearch.AutoSize = True
        lblSearch.Margin = New Padding(3, 8, 0, 0)
        lblSearch.Name = "lblSearch"
        lblSearch.Size = New Size(45, 15)
        lblSearch.TabIndex = 0
        lblSearch.Text = "Search:"
        '
        'txtSearch
        '
        txtSearch.AutoCompleteMode = AutoCompleteMode.SuggestAppend
        txtSearch.AutoCompleteSource = AutoCompleteSource.CustomSource
        txtSearch.Font = New Font("Segoe UI", 10.0F)
        txtSearch.Margin = New Padding(3, 4, 3, 3)
        txtSearch.Name = "txtSearch"
        txtSearch.Size = New Size(300, 25)
        txtSearch.TabIndex = 1
        '
        'cboMode
        '
        cboMode.DropDownStyle = ComboBoxStyle.DropDownList
        cboMode.Margin = New Padding(3, 5, 3, 3)
        cboMode.Name = "cboMode"
        cboMode.Size = New Size(160, 23)
        cboMode.TabIndex = 2
        '
        'lblMax
        '
        lblMax.AutoSize = True
        lblMax.Margin = New Padding(9, 8, 0, 0)
        lblMax.Name = "lblMax"
        lblMax.Size = New Size(64, 15)
        lblMax.TabIndex = 3
        lblMax.Text = "Max/supp.:"
        '
        'nudMax
        '
        nudMax.Margin = New Padding(3, 5, 3, 3)
        nudMax.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        nudMax.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        nudMax.Name = "nudMax"
        nudMax.Size = New Size(50, 23)
        nudMax.TabIndex = 4
        nudMax.Value = New Decimal(New Integer() {25, 0, 0, 0})
        '
        'btnSearch
        '
        btnSearch.Margin = New Padding(9, 3, 3, 3)
        btnSearch.Name = "btnSearch"
        btnSearch.Size = New Size(85, 28)
        btnSearch.TabIndex = 5
        btnSearch.Text = "&Search"
        btnSearch.UseVisualStyleBackColor = True
        '
        'btnCancel
        '
        btnCancel.Enabled = False
        btnCancel.Name = "btnCancel"
        btnCancel.Size = New Size(70, 28)
        btnCancel.TabIndex = 6
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        'btnLoadMore
        '
        btnLoadMore.Enabled = False
        btnLoadMore.Name = "btnLoadMore"
        btnLoadMore.Size = New Size(85, 28)
        btnLoadMore.TabIndex = 6
        btnLoadMore.Text = "Load &more"
        btnLoadMore.UseVisualStyleBackColor = True
        '
        'chkInStock
        '
        chkInStock.AutoSize = True
        chkInStock.Margin = New Padding(12, 8, 3, 3)
        chkInStock.Name = "chkInStock"
        chkInStock.Size = New Size(95, 19)
        chkInStock.TabIndex = 7
        chkInStock.Text = "In stock only"
        chkInStock.UseVisualStyleBackColor = True
        '
        'flpFilter - keeps "Filter:" and its box together when the toolbar wraps
        '
        flpFilter.AutoSize = True
        flpFilter.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flpFilter.Controls.Add(lblFilter)
        flpFilter.Controls.Add(txtFilter)
        flpFilter.Margin = New Padding(0)
        flpFilter.Name = "flpFilter"
        flpFilter.TabIndex = 8
        flpFilter.WrapContents = False
        '
        'lblFilter
        '
        lblFilter.AutoSize = True
        lblFilter.Margin = New Padding(9, 8, 0, 0)
        lblFilter.Name = "lblFilter"
        lblFilter.Size = New Size(36, 15)
        lblFilter.TabIndex = 8
        lblFilter.Text = "Filter:"
        '
        'txtFilter
        '
        txtFilter.Margin = New Padding(3, 5, 3, 3)
        txtFilter.Name = "txtFilter"
        txtFilter.Size = New Size(150, 23)
        txtFilter.TabIndex = 9
        '
        'btnExport
        '
        btnExport.Margin = New Padding(12, 3, 3, 3)
        btnExport.Name = "btnExport"
        btnExport.Size = New Size(85, 28)
        btnExport.TabIndex = 10
        btnExport.Text = "&Export CSV…"
        btnExport.UseVisualStyleBackColor = True
        '
        'btnSettings
        '
        btnSettings.Name = "btnSettings"
        btnSettings.Size = New Size(85, 28)
        btnSettings.TabIndex = 11
        btnSettings.Text = "Se&ttings…"
        btnSettings.UseVisualStyleBackColor = True
        '
        'btnAbout
        '
        btnHelp.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnHelp.Margin = New Padding(3, 9, 3, 3)
        btnHelp.Name = "btnHelp"
        btnHelp.Size = New Size(75, 28)
        btnHelp.TabIndex = 12
        btnHelp.Text = "&Help…"
        btnHelp.UseVisualStyleBackColor = True
        '
        'btnAbout
        '
        btnAbout.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnAbout.Margin = New Padding(3, 9, 9, 3)
        btnAbout.Name = "btnAbout"
        btnAbout.Size = New Size(75, 28)
        btnAbout.TabIndex = 13
        btnAbout.Text = "&About…"
        btnAbout.UseVisualStyleBackColor = True
        '
        'splitMain
        '
        splitMain.Dock = DockStyle.Fill
        splitMain.FixedPanel = FixedPanel.Panel1
        splitMain.Location = New Point(0, 43)
        splitMain.Name = "splitMain"
        '
        'splitMain.Panel1
        '
        splitMain.Panel1.Controls.Add(lvSuppliers)
        '
        'splitMain.Panel2
        '
        splitMain.Panel2.Controls.Add(dgvResults)
        splitMain.Size = New Size(1264, 616)
        splitMain.SplitterDistance = 440
        splitMain.TabIndex = 1
        '
        'lvSuppliers
        '
        lvSuppliers.CheckBoxes = True
        lvSuppliers.Columns.AddRange(New ColumnHeader() {colSupplier, colMode, colStatus, colHits, colTime})
        lvSuppliers.ContextMenuStrip = cmsSuppliers
        lvSuppliers.Dock = DockStyle.Fill
        lvSuppliers.FullRowSelect = True
        lvSuppliers.Location = New Point(0, 0)
        lvSuppliers.MultiSelect = False
        lvSuppliers.Name = "lvSuppliers"
        lvSuppliers.ShowItemToolTips = True
        lvSuppliers.Size = New Size(440, 616)
        lvSuppliers.TabIndex = 0
        lvSuppliers.UseCompatibleStateImageBehavior = False
        lvSuppliers.View = View.Details
        '
        'colSupplier
        '
        colSupplier.Text = "Supplier"
        colSupplier.Width = 110
        '
        'colMode
        '
        colMode.Text = "Mode"
        colMode.Width = 80
        '
        'colStatus
        '
        colStatus.Text = "Status"
        colStatus.Width = 90
        '
        'colHits
        '
        colHits.Text = "Hits"
        colHits.TextAlign = HorizontalAlignment.Right
        colHits.Width = 70
        '
        'colTime
        '
        colTime.Text = "Time"
        colTime.TextAlign = HorizontalAlignment.Right
        colTime.Width = 50
        '
        'cmsSuppliers
        '
        cmsSuppliers.Items.AddRange(New ToolStripItem() {mnuOpenSupplierSearch, mnuOpenAllSearches, mnuSupplierSep, mnuLoadMoreSupplier})
        cmsSuppliers.Name = "cmsSuppliers"
        cmsSuppliers.ShowItemToolTips = True
        cmsSuppliers.Size = New Size(260, 76)
        '
        'mnuSupplierSep
        '
        mnuSupplierSep.Name = "mnuSupplierSep"
        mnuSupplierSep.Size = New Size(256, 6)
        '
        'mnuLoadMoreSupplier
        '
        mnuLoadMoreSupplier.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        mnuLoadMoreSupplier.Name = "mnuLoadMoreSupplier"
        mnuLoadMoreSupplier.Size = New Size(259, 22)
        mnuLoadMoreSupplier.Text = "Load more"
        mnuLoadMoreSupplier.ToolTipText = "Fetch the next batch of results from just this supplier (one API call)."
        '
        'mnuOpenSupplierSearch
        '
        mnuOpenSupplierSearch.Name = "mnuOpenSupplierSearch"
        mnuOpenSupplierSearch.Size = New Size(259, 22)
        mnuOpenSupplierSearch.Text = "Open search on this supplier's website"
        '
        'mnuOpenAllSearches
        '
        mnuOpenAllSearches.Name = "mnuOpenAllSearches"
        mnuOpenAllSearches.Size = New Size(259, 22)
        mnuOpenAllSearches.Text = "Open search on all ticked websites"
        '
        'dgvResults
        '
        dgvResults.AllowUserToAddRows = False
        dgvResults.AllowUserToDeleteRows = False
        dgvResults.AllowUserToOrderColumns = True
        dgvResults.AllowUserToResizeRows = False
        dgvResults.BackgroundColor = SystemColors.Window
        dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvResults.ContextMenuStrip = cmsResults
        dgvResults.Dock = DockStyle.Fill
        dgvResults.Location = New Point(0, 0)
        dgvResults.MultiSelect = True
        dgvResults.Name = "dgvResults"
        dgvResults.ReadOnly = True
        dgvResults.RowHeadersVisible = False
        dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvResults.Size = New Size(880, 616)
        dgvResults.TabIndex = 0
        '
        'cmsResults
        '
        cmsResults.Items.AddRange(New ToolStripItem() {mnuOpenProduct, mnuOpenDatasheet, mnuSep1, mnuCopyMpn, mnuCopySupplierPart, mnuCopyRow, mnuSep2, mnuRemoveRows, mnuRestoreRows})
        cmsResults.Name = "cmsResults"
        cmsResults.ShowItemToolTips = True
        cmsResults.Size = New Size(250, 170)
        '
        'mnuOpenProduct
        '
        mnuOpenProduct.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        mnuOpenProduct.Name = "mnuOpenProduct"
        mnuOpenProduct.Size = New Size(229, 22)
        mnuOpenProduct.Text = "Open product page"
        '
        'mnuOpenDatasheet
        '
        mnuOpenDatasheet.Name = "mnuOpenDatasheet"
        mnuOpenDatasheet.Size = New Size(229, 22)
        mnuOpenDatasheet.Text = "Open datasheet"
        '
        'mnuSep1
        '
        mnuSep1.Name = "mnuSep1"
        mnuSep1.Size = New Size(226, 6)
        '
        'mnuCopyMpn
        '
        mnuCopyMpn.Name = "mnuCopyMpn"
        mnuCopyMpn.Size = New Size(229, 22)
        mnuCopyMpn.Text = "Copy manufacturer part no."
        '
        'mnuCopySupplierPart
        '
        mnuCopySupplierPart.Name = "mnuCopySupplierPart"
        mnuCopySupplierPart.Size = New Size(229, 22)
        mnuCopySupplierPart.Text = "Copy supplier part no."
        '
        'mnuCopyRow
        '
        mnuCopyRow.Name = "mnuCopyRow"
        mnuCopyRow.Size = New Size(229, 22)
        mnuCopyRow.Text = "Copy row"
        mnuCopyRow.ToolTipText = "Copy the selected rows as tab-separated text - pastes straight into Excel."
        '
        'mnuSep2
        '
        mnuSep2.Name = "mnuSep2"
        mnuSep2.Size = New Size(246, 6)
        '
        'mnuRemoveRows
        '
        mnuRemoveRows.Name = "mnuRemoveRows"
        mnuRemoveRows.ShortcutKeyDisplayString = "Del"
        mnuRemoveRows.Size = New Size(249, 22)
        mnuRemoveRows.Text = "Remove from results"
        mnuRemoveRows.ToolTipText = "Hide the selected rows until the next search. Ctrl/Shift-click to select several."
        '
        'mnuRestoreRows
        '
        mnuRestoreRows.Name = "mnuRestoreRows"
        mnuRestoreRows.Size = New Size(249, 22)
        mnuRestoreRows.Text = "Restore removed rows"
        mnuRestoreRows.ToolTipText = "Bring back every row you removed from this search."
        '
        'ssMain
        '
        ssMain.Items.AddRange(New ToolStripItem() {lblStatus, lblTotal})
        ssMain.Location = New Point(0, 659)
        ssMain.Name = "statusStrip"
        ssMain.ShowItemToolTips = True
        ssMain.Size = New Size(1264, 22)
        ssMain.TabIndex = 2
        '
        'lblStatus
        '
        lblStatus.Name = "lblStatus"
        lblStatus.Size = New Size(1249, 17)
        lblStatus.Spring = True
        lblStatus.Text = "Ready"
        lblStatus.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblTotal - permanent count in the bottom-right corner (the message on the left keeps changing)
        '
        lblTotal.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        lblTotal.Margin = New Padding(12, 3, 4, 2)
        lblTotal.Name = "lblTotal"
        lblTotal.Text = "Total parts = 0"
        lblTotal.ToolTipText = "Parts found so far in this search (not counting website-link rows)." & vbCrLf & "'showing' appears when Filter, In stock only or removed rows hide some of them."
        '
        'tipMain
        '
        tipMain.AutoPopDelay = 15000
        tipMain.InitialDelay = 500
        tipMain.ReshowDelay = 100
        tipMain.SetToolTip(txtSearch, "Part number or keywords to search for, e.g. LM358 or 10uF 25V 0805." & vbCrLf & "Press Enter to search. Previous searches drop down as you type.")
        tipMain.SetToolTip(cboMode, "Keyword: searches descriptions and part numbers." & vbCrLf & "Manufacturer part no.: looks for parts whose manufacturer part number starts with what you typed.")
        tipMain.SetToolTip(nudMax, "How many results to fetch from each supplier per search, and per Load more (up to 50)." & vbCrLf & "Fewer is faster. Each search or Load more uses one call of each supplier's daily API allowance.")
        tipMain.SetToolTip(btnSearch, "Search every ticked supplier at once (Enter).")
        tipMain.SetToolTip(btnCancel, "Stop any searches still running.")
        tipMain.SetToolTip(btnLoadMore, "Fetch the next batch (Max/supp.) from every ticked supplier that has more - see Hits, e.g. 50 / 312." & vbCrLf & "Uses one API call per supplier. New rows are added at the end and selected - click the Supplier header to group them." & vbCrLf & "Right-click a supplier to load more from just that one.")
        tipMain.SetToolTip(chkInStock, "Hide parts with no stock. Website-link rows always stay.")
        tipMain.SetToolTip(txtFilter, "Narrow the current results as you type." & vbCrLf & "Matches supplier, supplier part no., mfr part no., manufacturer and description.")
        tipMain.SetToolTip(btnExport, "Save the rows currently shown to a CSV file (opens in Excel).")
        tipMain.SetToolTip(btnSettings, "API keys, supplier search URLs, your own suppliers, photos, dark mode and window layout.")
        tipMain.SetToolTip(btnHelp, "How to use WinPartsFinder.")
        tipMain.SetToolTip(btnAbout, "Version, credits, licence and donate.")        '
        'MainForm
        '
        AcceptButton = btnSearch
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(1264, 681)
        Controls.Add(splitMain)
        Controls.Add(ssMain)
        Controls.Add(tlpTop)
        MinimumSize = New Size(800, 400)
        Name = "MainForm"
        StartPosition = FormStartPosition.CenterScreen
        Text = "WinPartsFinder - component supplier search"
        flpFilter.ResumeLayout(False)
        flpFilter.PerformLayout()
        tlpTop.ResumeLayout(False)
        tlpTop.PerformLayout()
        flpTop.ResumeLayout(False)
        flpTop.PerformLayout()
        CType(nudMax, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.Panel1.ResumeLayout(False)
        splitMain.Panel2.ResumeLayout(False)
        CType(splitMain, System.ComponentModel.ISupportInitialize).EndInit()
        splitMain.ResumeLayout(False)
        cmsSuppliers.ResumeLayout(False)
        CType(dgvResults, System.ComponentModel.ISupportInitialize).EndInit()
        cmsResults.ResumeLayout(False)
        ssMain.ResumeLayout(False)
        ssMain.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tlpTop As TableLayoutPanel
    Friend WithEvents flpTop As FlowLayoutPanel
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents cboMode As ComboBox
    Friend WithEvents lblMax As Label
    Friend WithEvents nudMax As NumericUpDown
    Friend WithEvents btnSearch As Button
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnLoadMore As Button
    Friend WithEvents chkInStock As CheckBox
    Friend WithEvents flpFilter As FlowLayoutPanel
    Friend WithEvents lblFilter As Label
    Friend WithEvents txtFilter As TextBox
    Friend WithEvents btnExport As Button
    Friend WithEvents btnSettings As Button
    Friend WithEvents btnAbout As Button
    Friend WithEvents btnHelp As Button
    Friend WithEvents tipMain As ToolTip
    Friend WithEvents splitMain As SplitContainer
    Friend WithEvents lvSuppliers As ListView
    Friend WithEvents colSupplier As ColumnHeader
    Friend WithEvents colMode As ColumnHeader
    Friend WithEvents colStatus As ColumnHeader
    Friend WithEvents colHits As ColumnHeader
    Friend WithEvents colTime As ColumnHeader
    Friend WithEvents cmsSuppliers As ContextMenuStrip
    Friend WithEvents mnuOpenSupplierSearch As ToolStripMenuItem
    Friend WithEvents mnuOpenAllSearches As ToolStripMenuItem
    Friend WithEvents mnuSupplierSep As ToolStripSeparator
    Friend WithEvents mnuLoadMoreSupplier As ToolStripMenuItem
    Friend WithEvents dgvResults As DataGridView
    Friend WithEvents cmsResults As ContextMenuStrip
    Friend WithEvents mnuOpenProduct As ToolStripMenuItem
    Friend WithEvents mnuOpenDatasheet As ToolStripMenuItem
    Friend WithEvents mnuSep1 As ToolStripSeparator
    Friend WithEvents mnuCopyMpn As ToolStripMenuItem
    Friend WithEvents mnuCopySupplierPart As ToolStripMenuItem
    Friend WithEvents mnuCopyRow As ToolStripMenuItem
    Friend WithEvents mnuSep2 As ToolStripSeparator
    Friend WithEvents mnuRemoveRows As ToolStripMenuItem
    Friend WithEvents mnuRestoreRows As ToolStripMenuItem
    Friend WithEvents ssMain As StatusStrip
    Friend WithEvents lblStatus As ToolStripStatusLabel
    Friend WithEvents lblTotal As ToolStripStatusLabel

End Class
