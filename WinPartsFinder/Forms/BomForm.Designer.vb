<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class BomForm
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
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle3 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle4 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle5 As DataGridViewCellStyle = New DataGridViewCellStyle()
        tlpTop = New TableLayoutPanel()
        lblName = New Label()
        txtName = New TextBox()
        dgvBom = New DataGridView()
        colInclude = New DataGridViewCheckBoxColumn()
        colSupplier = New DataGridViewTextBoxColumn()
        colPartNo = New DataGridViewTextBoxColumn()
        colMpn = New DataGridViewTextBoxColumn()
        colDescription = New DataGridViewTextBoxColumn()
        colQty = New DataGridViewTextBoxColumn()
        colUnitPrice = New DataGridViewTextBoxColumn()
        colTotal = New DataGridViewTextBoxColumn()
        colLink = New DataGridViewLinkColumn()
        colDelete = New DataGridViewTextBoxColumn()
        cmsBom = New ContextMenuStrip(components)
        mnuOpenProduct = New ToolStripMenuItem()
        mnuCopyPartNo = New ToolStripMenuItem()
        mnuCopyMpn = New ToolStripMenuItem()
        mnuSep1 = New ToolStripSeparator()
        mnuRemove = New ToolStripMenuItem()
        tlpBottom = New TableLayoutPanel()
        lblSubtotals = New Label()
        lblGrandTotal = New Label()
        lblNotes = New Label()
        txtNotes = New TextBox()
        flpButtons = New FlowLayoutPanel()
        btnNew = New Button()
        btnLoad = New Button()
        btnSave = New Button()
        btnSaveAs = New Button()
        btnOpenFolder = New Button()
        btnExportTxt = New Button()
        btnRemove = New Button()
        btnQtyDown = New Button()
        btnQtyUp = New Button()
        tipBom = New ToolTip(components)
        tlpTop.SuspendLayout()
        CType(dgvBom, System.ComponentModel.ISupportInitialize).BeginInit()
        cmsBom.SuspendLayout()
        tlpBottom.SuspendLayout()
        flpButtons.SuspendLayout()
        SuspendLayout()
        '
        'tlpTop
        '
        tlpTop.AutoSize = True
        tlpTop.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpTop.ColumnCount = 2
        tlpTop.ColumnStyles.Add(New ColumnStyle())
        tlpTop.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpTop.Controls.Add(lblName, 0, 0)
        tlpTop.Controls.Add(txtName, 1, 0)
        tlpTop.Dock = DockStyle.Top
        tlpTop.Location = New Point(0, 0)
        tlpTop.Name = "tlpTop"
        tlpTop.Padding = New Padding(6, 6, 6, 3)
        tlpTop.RowCount = 1
        tlpTop.RowStyles.Add(New RowStyle())
        tlpTop.Size = New Size(784, 38)
        tlpTop.TabIndex = 0
        '
        'lblName
        '
        lblName.Anchor = AnchorStyles.Left
        lblName.AutoSize = True
        lblName.Location = New Point(9, 13)
        lblName.Name = "lblName"
        lblName.Size = New Size(66, 15)
        lblName.TabIndex = 0
        lblName.Text = "BOM name:"
        '
        'txtName
        '
        txtName.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        txtName.Location = New Point(81, 9)
        txtName.Name = "txtName"
        txtName.Size = New Size(694, 23)
        txtName.TabIndex = 1
        '
        'dgvBom
        '
        dgvBom.AllowUserToAddRows = False
        dgvBom.AllowUserToDeleteRows = False
        dgvBom.AllowUserToResizeRows = False
        dgvBom.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvBom.Columns.AddRange(New DataGridViewColumn() {colInclude, colSupplier, colPartNo, colMpn, colDescription, colQty, colUnitPrice, colTotal, colLink, colDelete})
        dgvBom.ContextMenuStrip = cmsBom
        dgvBom.Dock = DockStyle.Fill
        dgvBom.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2
        dgvBom.Location = New Point(0, 38)
        dgvBom.Name = "dgvBom"
        dgvBom.RowHeadersVisible = False
        dgvBom.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        dgvBom.Size = New Size(784, 283)
        dgvBom.TabIndex = 1
        '
        'colInclude
        '
        colInclude.HeaderText = ""
        colInclude.Name = "colInclude"
        colInclude.ReadOnly = True
        colInclude.Resizable = DataGridViewTriState.False
        colInclude.SortMode = DataGridViewColumnSortMode.NotSortable
        colInclude.ToolTipText = "Ticked lines count in the totals"
        colInclude.Width = 28
        '
        'colSupplier
        '
        colSupplier.HeaderText = "Supplier"
        colSupplier.Name = "colSupplier"
        colSupplier.ReadOnly = True
        colSupplier.SortMode = DataGridViewColumnSortMode.NotSortable
        colSupplier.Width = 80
        '
        'colPartNo
        '
        colPartNo.HeaderText = "Supplier Part No."
        colPartNo.Name = "colPartNo"
        colPartNo.ReadOnly = True
        colPartNo.SortMode = DataGridViewColumnSortMode.NotSortable
        colPartNo.ToolTipText = "The supplier's own order code"
        colPartNo.Width = 120
        '
        'colMpn
        '
        colMpn.HeaderText = "Mfr Part No."
        colMpn.Name = "colMpn"
        colMpn.ReadOnly = True
        colMpn.SortMode = DataGridViewColumnSortMode.NotSortable
        colMpn.ToolTipText = "The manufacturer's part number"
        colMpn.Width = 110
        '
        'colDescription
        '
        colDescription.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        colDescription.HeaderText = "Description"
        colDescription.MinimumWidth = 80
        colDescription.Name = "colDescription"
        colDescription.ReadOnly = True
        colDescription.SortMode = DataGridViewColumnSortMode.NotSortable
        '
        'colQty
        '
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleRight
        colQty.DefaultCellStyle = DataGridViewCellStyle1
        colQty.HeaderText = "Qty"
        colQty.Name = "colQty"
        colQty.SortMode = DataGridViewColumnSortMode.NotSortable
        colQty.ToolTipText = "Click and type a new quantity, or use Qty − / Qty +"
        colQty.Width = 55
        '
        'colUnitPrice
        '
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight
        colUnitPrice.DefaultCellStyle = DataGridViewCellStyle2
        colUnitPrice.HeaderText = "Unit Price"
        colUnitPrice.Name = "colUnitPrice"
        colUnitPrice.ReadOnly = True
        colUnitPrice.SortMode = DataGridViewColumnSortMode.NotSortable
        colUnitPrice.Width = 85
        '
        'colTotal
        '
        DataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleRight
        colTotal.DefaultCellStyle = DataGridViewCellStyle3
        colTotal.HeaderText = "Total"
        colTotal.Name = "colTotal"
        colTotal.ReadOnly = True
        colTotal.SortMode = DataGridViewColumnSortMode.NotSortable
        colTotal.Width = 85
        '
        'colLink
        '
        DataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleCenter
        colLink.DefaultCellStyle = DataGridViewCellStyle4
        colLink.HeaderText = "Link"
        colLink.Name = "colLink"
        colLink.ReadOnly = True
        colLink.SortMode = DataGridViewColumnSortMode.NotSortable
        colLink.TrackVisitedState = False
        colLink.Width = 50
        '
        'colDelete
        '
        DataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleCenter
        colDelete.DefaultCellStyle = DataGridViewCellStyle5
        colDelete.HeaderText = ""
        colDelete.Name = "colDelete"
        colDelete.ReadOnly = True
        colDelete.Resizable = DataGridViewTriState.False
        colDelete.SortMode = DataGridViewColumnSortMode.NotSortable
        colDelete.Width = 28
        '
        'cmsBom
        '
        cmsBom.Items.AddRange(New ToolStripItem() {mnuOpenProduct, mnuCopyPartNo, mnuCopyMpn, mnuSep1, mnuRemove})
        cmsBom.Name = "cmsBom"
        cmsBom.Size = New Size(230, 98)
        '
        'mnuOpenProduct
        '
        mnuOpenProduct.Name = "mnuOpenProduct"
        mnuOpenProduct.Size = New Size(229, 22)
        mnuOpenProduct.Text = "Open product page"
        '
        'mnuCopyPartNo
        '
        mnuCopyPartNo.Name = "mnuCopyPartNo"
        mnuCopyPartNo.Size = New Size(229, 22)
        mnuCopyPartNo.Text = "Copy supplier part no."
        '
        'mnuCopyMpn
        '
        mnuCopyMpn.Name = "mnuCopyMpn"
        mnuCopyMpn.Size = New Size(229, 22)
        mnuCopyMpn.Text = "Copy manufacturer part no."
        '
        'mnuSep1
        '
        mnuSep1.Name = "mnuSep1"
        mnuSep1.Size = New Size(226, 6)
        '
        'mnuRemove
        '
        mnuRemove.Name = "mnuRemove"
        mnuRemove.Size = New Size(229, 22)
        mnuRemove.Text = "Remove"
        '
        'tlpBottom
        '
        tlpBottom.AutoSize = True
        tlpBottom.AutoSizeMode = AutoSizeMode.GrowAndShrink
        tlpBottom.ColumnCount = 2
        tlpBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpBottom.ColumnStyles.Add(New ColumnStyle())
        tlpBottom.Controls.Add(lblSubtotals, 0, 0)
        tlpBottom.Controls.Add(lblGrandTotal, 1, 0)
        tlpBottom.Controls.Add(lblNotes, 0, 1)
        tlpBottom.Controls.Add(txtNotes, 0, 2)
        tlpBottom.Controls.Add(flpButtons, 0, 3)
        tlpBottom.Controls.Add(btnOpenFolder, 1, 3)
        tlpBottom.Dock = DockStyle.Bottom
        tlpBottom.Location = New Point(0, 321)
        tlpBottom.Name = "tlpBottom"
        tlpBottom.Padding = New Padding(6, 3, 6, 6)
        tlpBottom.RowCount = 4
        tlpBottom.RowStyles.Add(New RowStyle())
        tlpBottom.RowStyles.Add(New RowStyle())
        tlpBottom.RowStyles.Add(New RowStyle())
        tlpBottom.RowStyles.Add(New RowStyle())
        tlpBottom.Size = New Size(784, 160)
        tlpBottom.TabIndex = 2
        '
        'lblSubtotals
        '
        lblSubtotals.AutoEllipsis = True
        lblSubtotals.Dock = DockStyle.Fill
        lblSubtotals.Location = New Point(9, 3)
        lblSubtotals.Name = "lblSubtotals"
        lblSubtotals.Size = New Size(600, 21)
        lblSubtotals.TabIndex = 0
        lblSubtotals.TextAlign = ContentAlignment.MiddleLeft
        '
        'lblGrandTotal
        '
        lblGrandTotal.Anchor = AnchorStyles.Right
        lblGrandTotal.AutoSize = True
        lblGrandTotal.Location = New Point(680, 6)
        lblGrandTotal.Name = "lblGrandTotal"
        lblGrandTotal.Size = New Size(95, 15)
        lblGrandTotal.TabIndex = 1
        lblGrandTotal.Text = "Grand total: 0.00"
        '
        'lblNotes
        '
        lblNotes.AutoSize = True
        tlpBottom.SetColumnSpan(lblNotes, 2)
        lblNotes.Location = New Point(9, 27)
        lblNotes.Margin = New Padding(3, 3, 3, 0)
        lblNotes.Name = "lblNotes"
        lblNotes.Size = New Size(41, 15)
        lblNotes.TabIndex = 2
        lblNotes.Text = "Notes:"
        '
        'txtNotes
        '
        tlpBottom.SetColumnSpan(txtNotes, 2)
        txtNotes.Dock = DockStyle.Fill
        txtNotes.Location = New Point(9, 45)
        txtNotes.Multiline = True
        txtNotes.Name = "txtNotes"
        txtNotes.ScrollBars = ScrollBars.Vertical
        txtNotes.Size = New Size(766, 64)
        txtNotes.TabIndex = 3
        '
        'flpButtons
        '
        flpButtons.AutoSize = True
        flpButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink
        flpButtons.Controls.Add(btnNew)
        flpButtons.Controls.Add(btnLoad)
        flpButtons.Controls.Add(btnSave)
        flpButtons.Controls.Add(btnSaveAs)
        flpButtons.Controls.Add(btnExportTxt)
        flpButtons.Controls.Add(btnRemove)
        flpButtons.Controls.Add(btnQtyDown)
        flpButtons.Controls.Add(btnQtyUp)
        flpButtons.Location = New Point(6, 112)
        flpButtons.Margin = New Padding(0, 3, 0, 0)
        flpButtons.Name = "flpButtons"
        flpButtons.Size = New Size(560, 34)
        flpButtons.TabIndex = 4
        flpButtons.WrapContents = False
        '
        'btnNew
        '
        btnNew.Name = "btnNew"
        btnNew.Size = New Size(75, 28)
        btnNew.TabIndex = 0
        btnNew.Text = "New"
        '
        'btnLoad
        '
        btnLoad.Name = "btnLoad"
        btnLoad.Size = New Size(75, 28)
        btnLoad.TabIndex = 1
        btnLoad.Text = "Load…"
        '
        'btnSave
        '
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(75, 28)
        btnSave.TabIndex = 2
        btnSave.Text = "Save"
        '
        'btnSaveAs
        '
        btnSaveAs.Name = "btnSaveAs"
        btnSaveAs.Size = New Size(75, 28)
        btnSaveAs.TabIndex = 3
        btnSaveAs.Text = "Save As…"
        '
        'btnOpenFolder
        '
        btnOpenFolder.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        btnOpenFolder.Margin = New Padding(3, 6, 3, 3) ' level with the buttons in flpButtons
        btnOpenFolder.Name = "btnOpenFolder"
        btnOpenFolder.Size = New Size(85, 28)
        btnOpenFolder.TabIndex = 7
        btnOpenFolder.Text = "Open Folder"
        '
        'btnExportTxt
        '
        btnExportTxt.Name = "btnExportTxt"
        btnExportTxt.Size = New Size(85, 28)
        btnExportTxt.TabIndex = 8
        btnExportTxt.Text = "Export TXT…"
        '
        'btnRemove
        '
        btnRemove.Margin = New Padding(18, 3, 3, 3)
        btnRemove.Name = "btnRemove"
        btnRemove.Size = New Size(75, 28)
        btnRemove.TabIndex = 4
        btnRemove.Text = "Remove"
        '
        'btnQtyDown
        '
        btnQtyDown.Name = "btnQtyDown"
        btnQtyDown.Size = New Size(60, 28)
        btnQtyDown.TabIndex = 5
        btnQtyDown.Text = "Qty −"
        '
        'btnQtyUp
        '
        btnQtyUp.Name = "btnQtyUp"
        btnQtyUp.Size = New Size(60, 28)
        btnQtyUp.TabIndex = 6
        btnQtyUp.Text = "Qty +"
        '
        'tipBom
        '
        tipBom.SetToolTip(btnNew, "Start an empty BOM")
        tipBom.SetToolTip(btnLoad, "Open a saved BOM in place of this one")
        tipBom.SetToolTip(btnSave, "Save this BOM to its file")
        tipBom.SetToolTip(btnSaveAs, "Save this BOM to a new file")
        tipBom.SetToolTip(btnOpenFolder, "Open the folder this BOM is saved in")
        tipBom.SetToolTip(btnExportTxt, "Export to .txt")
        tipBom.SetToolTip(btnRemove, "Remove the selected lines (Ctrl+Z puts them back)")
        tipBom.SetToolTip(btnQtyDown, "One fewer of the selected lines")
        tipBom.SetToolTip(btnQtyUp, "One more of the selected lines")
        '
        'BomForm
        '
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(894, 481)
        Controls.Add(dgvBom)
        Controls.Add(tlpBottom)
        Controls.Add(tlpTop)
        KeyPreview = True
        MinimumSize = New Size(560, 360)
        Name = "BomForm"
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "BOM"
        tlpTop.ResumeLayout(False)
        tlpTop.PerformLayout()
        CType(dgvBom, System.ComponentModel.ISupportInitialize).EndInit()
        cmsBom.ResumeLayout(False)
        tlpBottom.ResumeLayout(False)
        tlpBottom.PerformLayout()
        flpButtons.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tlpTop As TableLayoutPanel
    Friend WithEvents lblName As Label
    Friend WithEvents txtName As TextBox
    Friend WithEvents dgvBom As DataGridView
    Friend WithEvents colInclude As DataGridViewCheckBoxColumn
    Friend WithEvents colSupplier As DataGridViewTextBoxColumn
    Friend WithEvents colPartNo As DataGridViewTextBoxColumn
    Friend WithEvents colMpn As DataGridViewTextBoxColumn
    Friend WithEvents colDescription As DataGridViewTextBoxColumn
    Friend WithEvents colQty As DataGridViewTextBoxColumn
    Friend WithEvents colUnitPrice As DataGridViewTextBoxColumn
    Friend WithEvents colTotal As DataGridViewTextBoxColumn
    Friend WithEvents colLink As DataGridViewLinkColumn
    Friend WithEvents colDelete As DataGridViewTextBoxColumn
    Friend WithEvents cmsBom As ContextMenuStrip
    Friend WithEvents mnuOpenProduct As ToolStripMenuItem
    Friend WithEvents mnuCopyPartNo As ToolStripMenuItem
    Friend WithEvents mnuCopyMpn As ToolStripMenuItem
    Friend WithEvents mnuSep1 As ToolStripSeparator
    Friend WithEvents mnuRemove As ToolStripMenuItem
    Friend WithEvents tlpBottom As TableLayoutPanel
    Friend WithEvents lblSubtotals As Label
    Friend WithEvents lblGrandTotal As Label
    Friend WithEvents lblNotes As Label
    Friend WithEvents txtNotes As TextBox
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnNew As Button
    Friend WithEvents btnLoad As Button
    Friend WithEvents btnSave As Button
    Friend WithEvents btnSaveAs As Button
    Friend WithEvents btnOpenFolder As Button
    Friend WithEvents btnExportTxt As Button
    Friend WithEvents btnRemove As Button
    Friend WithEvents btnQtyDown As Button
    Friend WithEvents btnQtyUp As Button
    Friend WithEvents tipBom As ToolTip

End Class
