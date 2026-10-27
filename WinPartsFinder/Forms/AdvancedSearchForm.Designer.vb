<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class AdvancedSearchForm
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
        tipAdv = New ToolTip(components)
        tlpMain = New TableLayoutPanel()
        lblQuery = New Label()
        txtQuery = New TextBox()
        lblMode = New Label()
        cboMode = New ComboBox()
        lblSupplierSide = New Label()
        flpSupplierSide = New FlowLayoutPanel()
        chkInStock = New CheckBox()
        chkRohs = New CheckBox()
        lblMaker = New Label()
        txtMaker = New TextBox()
        lblPartNo = New Label()
        txtPartNo = New TextBox()
        lblContains = New Label()
        txtContains = New TextBox()
        lblQty = New Label()
        flpQty = New FlowLayoutPanel()
        nudQty = New NumericUpDown()
        lblMaxPrice = New Label()
        txtMaxPrice = New TextBox()
        lblMax = New Label()
        nudMax = New NumericUpDown()
        lblSuppliers = New Label()
        clbSuppliers = New CheckedListBox()
        lblHint = New Label()
        flpButtons = New FlowLayoutPanel()
        btnCancel = New Button()
        btnSearch = New Button()
        btnClear = New Button()
        tlpMain.SuspendLayout()
        flpSupplierSide.SuspendLayout()
        flpQty.SuspendLayout()
        CType(nudQty, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(nudMax, System.ComponentModel.ISupportInitialize).BeginInit()
        flpButtons.SuspendLayout()
        SuspendLayout()
        '
        'tlpMain
        '
        tlpMain.ColumnCount = 2
        tlpMain.ColumnStyles.Add(New ColumnStyle())
        tlpMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        tlpMain.Controls.Add(lblQuery, 0, 0)
        tlpMain.Controls.Add(txtQuery, 1, 0)
        tlpMain.Controls.Add(lblMode, 0, 1)
        tlpMain.Controls.Add(cboMode, 1, 1)
        tlpMain.Controls.Add(lblSupplierSide, 0, 2)
        tlpMain.Controls.Add(flpSupplierSide, 1, 2)
        tlpMain.Controls.Add(lblMaker, 0, 3)
        tlpMain.Controls.Add(txtMaker, 1, 3)
        tlpMain.Controls.Add(lblPartNo, 0, 4)
        tlpMain.Controls.Add(txtPartNo, 1, 4)
        tlpMain.Controls.Add(lblContains, 0, 5)
        tlpMain.Controls.Add(txtContains, 1, 5)
        tlpMain.Controls.Add(lblQty, 0, 6)
        tlpMain.Controls.Add(flpQty, 1, 6)
        tlpMain.Controls.Add(lblMax, 0, 7)
        tlpMain.Controls.Add(nudMax, 1, 7)
        tlpMain.Controls.Add(lblSuppliers, 0, 8)
        tlpMain.Controls.Add(clbSuppliers, 1, 8)
        tlpMain.Controls.Add(lblHint, 0, 9)
        tlpMain.Dock = DockStyle.Fill
        tlpMain.Location = New Point(0, 0)
        tlpMain.Name = "tlpMain"
        tlpMain.Padding = New Padding(10)
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
        tlpMain.Size = New Size(560, 520)
        tlpMain.TabIndex = 0
        '
        'lblQuery
        '
        lblQuery.Anchor = AnchorStyles.Left
        lblQuery.AutoSize = True
        lblQuery.Name = "lblQuery"
        lblQuery.Text = "Search for:"
        '
        'txtQuery
        '
        txtQuery.Dock = DockStyle.Fill
        txtQuery.Font = New Font("Segoe UI", 10.0F)
        txtQuery.Name = "txtQuery"
        txtQuery.TabIndex = 0
        '
        'lblMode
        '
        lblMode.Anchor = AnchorStyles.Left
        lblMode.AutoSize = True
        lblMode.Name = "lblMode"
        lblMode.Text = "Search type:"
        '
        'cboMode
        '
        cboMode.Dock = DockStyle.Fill
        cboMode.DropDownStyle = ComboBoxStyle.DropDownList
        cboMode.Items.AddRange(New Object() {"Keyword", "Manufacturer part no. (starts with)", "Exact manufacturer part no.", "Supplier order code - find the part at every supplier"})
        cboMode.Name = "cboMode"
        cboMode.TabIndex = 1
        '
        'lblSupplierSide
        '
        lblSupplierSide.Anchor = AnchorStyles.Left
        lblSupplierSide.AutoSize = True
        lblSupplierSide.Margin = New Padding(3, 12, 3, 3)
        lblSupplierSide.Name = "lblSupplierSide"
        lblSupplierSide.Text = "Suppliers filter:"
        '
        'flpSupplierSide
        '
        flpSupplierSide.AutoSize = True
        flpSupplierSide.Controls.Add(chkInStock)
        flpSupplierSide.Controls.Add(chkRohs)
        flpSupplierSide.Margin = New Padding(0, 9, 0, 0)
        flpSupplierSide.Name = "flpSupplierSide"
        flpSupplierSide.TabIndex = 2
        '
        'chkInStock
        '
        chkInStock.AutoSize = True
        chkInStock.Name = "chkInStock"
        chkInStock.TabIndex = 0
        chkInStock.Text = "In stock only"
        chkInStock.UseVisualStyleBackColor = True
        '
        'chkRohs
        '
        chkRohs.AutoSize = True
        chkRohs.Margin = New Padding(18, 3, 3, 3)
        chkRohs.Name = "chkRohs"
        chkRohs.TabIndex = 1
        chkRohs.Text = "RoHS compliant only"
        chkRohs.UseVisualStyleBackColor = True
        '
        'lblMaker
        '
        lblMaker.Anchor = AnchorStyles.Left
        lblMaker.AutoSize = True
        lblMaker.Name = "lblMaker"
        lblMaker.Text = "Manufacturer name contains:"
        '
        'txtMaker
        '
        txtMaker.Dock = DockStyle.Fill
        txtMaker.Name = "txtMaker"
        txtMaker.TabIndex = 4
        '
        'lblPartNo
        '
        lblPartNo.Anchor = AnchorStyles.Left
        lblPartNo.AutoSize = True
        lblPartNo.Name = "lblPartNo"
        lblPartNo.Text = "Manufacturer part no. contains:"
        '
        'txtPartNo
        '
        txtPartNo.Dock = DockStyle.Fill
        txtPartNo.Name = "txtPartNo"
        txtPartNo.TabIndex = 5
        '
        'lblContains
        '
        lblContains.Anchor = AnchorStyles.Left
        lblContains.AutoSize = True
        lblContains.Name = "lblContains"
        lblContains.Text = "Description contains:"
        '
        'txtContains
        '
        txtContains.Dock = DockStyle.Fill
        txtContains.Name = "txtContains"
        txtContains.TabIndex = 5
        '
        'lblQty
        '
        lblQty.Anchor = AnchorStyles.Left
        lblQty.AutoSize = True
        lblQty.Name = "lblQty"
        lblQty.Text = "Quantity needed:"
        '
        'flpQty
        '
        flpQty.AutoSize = True
        flpQty.Controls.Add(nudQty)
        flpQty.Controls.Add(lblMaxPrice)
        flpQty.Controls.Add(txtMaxPrice)
        flpQty.Margin = New Padding(0)
        flpQty.Name = "flpQty"
        flpQty.TabIndex = 6
        '
        'nudQty
        '
        nudQty.Maximum = New Decimal(New Integer() {1000000, 0, 0, 0})
        nudQty.Name = "nudQty"
        nudQty.Size = New Size(80, 23)
        nudQty.TabIndex = 0
        nudQty.ThousandsSeparator = True
        '
        'lblMaxPrice
        '
        lblMaxPrice.AutoSize = True
        lblMaxPrice.Margin = New Padding(18, 7, 3, 0)
        lblMaxPrice.Name = "lblMaxPrice"
        lblMaxPrice.Text = "Max price each:"
        '
        'txtMaxPrice
        '
        txtMaxPrice.Name = "txtMaxPrice"
        txtMaxPrice.Size = New Size(80, 23)
        txtMaxPrice.TabIndex = 1
        '
        'lblMax
        '
        lblMax.Anchor = AnchorStyles.Left
        lblMax.AutoSize = True
        lblMax.Name = "lblMax"
        lblMax.Text = "Results per supplier:"
        '
        'nudMax
        '
        nudMax.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        nudMax.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        nudMax.Name = "nudMax"
        nudMax.Size = New Size(60, 23)
        nudMax.TabIndex = 7
        nudMax.Value = New Decimal(New Integer() {25, 0, 0, 0})
        '
        'lblSuppliers
        '
        lblSuppliers.AutoSize = True
        lblSuppliers.Margin = New Padding(3, 9, 3, 3)
        lblSuppliers.Name = "lblSuppliers"
        lblSuppliers.Text = "Suppliers to search:"
        '
        'clbSuppliers
        '
        clbSuppliers.CheckOnClick = True
        clbSuppliers.Dock = DockStyle.Fill
        clbSuppliers.IntegralHeight = False
        clbSuppliers.MultiColumn = True
        clbSuppliers.ColumnWidth = 150
        clbSuppliers.Margin = New Padding(3, 9, 3, 3)
        clbSuppliers.Name = "clbSuppliers"
        clbSuppliers.TabIndex = 8
        '
        'lblHint
        '
        lblHint.AutoSize = True
        tlpMain.SetColumnSpan(lblHint, 2)
        lblHint.ForeColor = SystemColors.GrayText
        lblHint.MaximumSize = New Size(530, 0)
        lblHint.Margin = New Padding(3, 9, 3, 3)
        lblHint.Name = "lblHint"
        lblHint.Text = "In stock / RoHS are applied by the suppliers themselves (TME has no RoHS option). Manufacturer, part no., description, quantity and price are applied to what comes back. Active parts only is under the supplier list on the main window. Leave anything blank or 0 to ignore it."
        '
        'flpButtons
        '
        flpButtons.AutoSize = True
        flpButtons.Controls.Add(btnCancel)
        flpButtons.Controls.Add(btnSearch)
        flpButtons.Controls.Add(btnClear)
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
        btnCancel.TabIndex = 2
        btnCancel.Text = "Cancel"
        btnCancel.UseVisualStyleBackColor = True
        '
        'btnSearch
        '
        btnSearch.Name = "btnSearch"
        btnSearch.Size = New Size(85, 28)
        btnSearch.TabIndex = 1
        btnSearch.Text = "&Search"
        btnSearch.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        btnClear.Margin = New Padding(3, 3, 24, 3)
        btnClear.Name = "btnClear"
        btnClear.Size = New Size(85, 28)
        btnClear.TabIndex = 0
        btnClear.Text = "C&lear"
        btnClear.UseVisualStyleBackColor = True
        '
        'tipAdv
        '
        tipAdv.AutoPopDelay = 15000
        tipAdv.InitialDelay = 500
        tipAdv.SetToolTip(txtQuery, "Part number, keywords or a supplier order code (2-40 characters for TME)." & vbCrLf & "Leave blank to search for the Manufacturer part no. contains and Description contains text instead.")
        tipAdv.SetToolTip(cboMode, "Keyword: descriptions and part numbers." & vbCrLf & "Manufacturer part no.: starts with what you typed." & vbCrLf & "Exact: only that exact manufacturer part number." & vbCrLf & "Supplier order code: e.g. a Farnell, CPC, Mouser, DigiKey or TME number - finds that part, then searches every other supplier for the same part.")
        tipAdv.SetToolTip(chkInStock, "The suppliers send only parts they have in stock, so you get a full page of in-stock parts." & vbCrLf & "(In stock only on the main window just hides out-of-stock rows already found.)")
        tipAdv.SetToolTip(chkRohs, "The suppliers send only RoHS compliant parts (Mouser, DigiKey, Farnell, CPC, Newark - TME has no RoHS search option).")
        tipAdv.SetToolTip(txtMaker, "Only parts whose manufacturer (the maker's name) contains this, e.g. Texas or Analog. Not the part number.")
        tipAdv.SetToolTip(txtPartNo, "Only parts whose manufacturer part number contains this, anywhere in it, e.g. 931G finds KSC931G LFS." & vbCrLf & "Tip: leave Search for blank and put what the part is in Description contains, e.g. switch -" & vbCrLf & "the two are then searched for together. DigiKey and Mouser match part of a number; Farnell, CPC and Newark don't.")
        tipAdv.SetToolTip(txtContains, "Only parts whose description (or part number) contains all of these words, e.g. SOIC or 0603 X7R." & vbCrLf & "With Search for blank, these words become the search instead (matched the suppliers' own way).")
        tipAdv.SetToolTip(nudQty, "How many you need. Prices are then shown at that quantity, and parts with a higher minimum order are left out. 0 = ignore.")
        tipAdv.SetToolTip(txtMaxPrice, "Leave out parts costing more than this each (at the quantity above). Blank = no limit.")
        tipAdv.SetToolTip(nudMax, "How many results to fetch from each supplier (up to 50). Load more works as normal afterwards.")
        tipAdv.SetToolTip(clbSuppliers, "Which suppliers to search. The supplier list on the main window is updated to match.")
        tipAdv.SetToolTip(btnClear, "Reset every option to its default.")
        '
        'AdvancedSearchForm
        '
        AcceptButton = btnSearch
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New Size(560, 600)
        Controls.Add(tlpMain)
        Controls.Add(flpButtons)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(540, 590)
        Name = "AdvancedSearchForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Advanced search"
        tlpMain.ResumeLayout(False)
        tlpMain.PerformLayout()
        flpSupplierSide.ResumeLayout(False)
        flpSupplierSide.PerformLayout()
        flpQty.ResumeLayout(False)
        flpQty.PerformLayout()
        CType(nudQty, System.ComponentModel.ISupportInitialize).EndInit()
        CType(nudMax, System.ComponentModel.ISupportInitialize).EndInit()
        flpButtons.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tipAdv As ToolTip
    Friend WithEvents tlpMain As TableLayoutPanel
    Friend WithEvents lblQuery As Label
    Friend WithEvents txtQuery As TextBox
    Friend WithEvents lblMode As Label
    Friend WithEvents cboMode As ComboBox
    Friend WithEvents lblSupplierSide As Label
    Friend WithEvents flpSupplierSide As FlowLayoutPanel
    Friend WithEvents chkInStock As CheckBox
    Friend WithEvents chkRohs As CheckBox
    Friend WithEvents lblMaker As Label
    Friend WithEvents txtMaker As TextBox
    Friend WithEvents lblPartNo As Label
    Friend WithEvents txtPartNo As TextBox
    Friend WithEvents lblContains As Label
    Friend WithEvents txtContains As TextBox
    Friend WithEvents lblQty As Label
    Friend WithEvents flpQty As FlowLayoutPanel
    Friend WithEvents nudQty As NumericUpDown
    Friend WithEvents lblMaxPrice As Label
    Friend WithEvents txtMaxPrice As TextBox
    Friend WithEvents lblMax As Label
    Friend WithEvents nudMax As NumericUpDown
    Friend WithEvents lblSuppliers As Label
    Friend WithEvents clbSuppliers As CheckedListBox
    Friend WithEvents lblHint As Label
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnSearch As Button
    Friend WithEvents btnClear As Button

End Class
