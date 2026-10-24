<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class TypesForm
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
        tipTypes = New ToolTip(components)
        lblHint = New Label()
        lvTypes = New ListView()
        colType = New ColumnHeader()
        colParts = New ColumnHeader()
        colSuppliers = New ColumnHeader()
        flpButtons = New FlowLayoutPanel()
        btnCancel = New Button()
        btnOK = New Button()
        btnNone = New Button()
        btnAll = New Button()
        flpButtons.SuspendLayout()
        SuspendLayout()
        '
        'lblHint
        '
        lblHint.AutoSize = False
        lblHint.Dock = DockStyle.Top
        lblHint.Name = "lblHint"
        lblHint.Padding = New Padding(8, 8, 8, 4)
        lblHint.Size = New Size(620, 46)
        lblHint.TabIndex = 0
        lblHint.Text = "Untick the types you don't want - their parts are hidden from the results (not deleted). Each supplier words its types its own way, so similar ones may appear more than once. A new search shows every type again."
        '
        'lvTypes
        '
        lvTypes.CheckBoxes = True
        lvTypes.Columns.AddRange(New ColumnHeader() {colType, colParts, colSuppliers})
        lvTypes.Dock = DockStyle.Fill
        lvTypes.FullRowSelect = True
        lvTypes.HeaderStyle = ColumnHeaderStyle.Clickable
        lvTypes.Name = "lvTypes"
        lvTypes.TabIndex = 1
        lvTypes.UseCompatibleStateImageBehavior = False
        lvTypes.View = View.Details
        '
        'colType
        '
        colType.Text = "Type"
        colType.Width = 300
        '
        'colParts
        '
        colParts.Text = "Parts"
        colParts.TextAlign = HorizontalAlignment.Right
        colParts.Width = 55
        '
        'colSuppliers
        '
        colSuppliers.Text = "Suppliers"
        colSuppliers.Width = 230
        '
        'flpButtons
        '
        flpButtons.AutoSize = True
        flpButtons.Controls.Add(btnCancel)
        flpButtons.Controls.Add(btnOK)
        flpButtons.Controls.Add(btnNone)
        flpButtons.Controls.Add(btnAll)
        flpButtons.Dock = DockStyle.Bottom
        flpButtons.FlowDirection = FlowDirection.RightToLeft
        flpButtons.Name = "flpButtons"
        flpButtons.Padding = New Padding(8)
        flpButtons.TabIndex = 2
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
        'btnNone
        '
        btnNone.Margin = New Padding(3, 3, 24, 3)
        btnNone.Name = "btnNone"
        btnNone.Size = New Size(85, 28)
        btnNone.TabIndex = 1
        btnNone.Text = "Untick &all"
        btnNone.UseVisualStyleBackColor = True
        '
        'btnAll
        '
        btnAll.Name = "btnAll"
        btnAll.Size = New Size(85, 28)
        btnAll.TabIndex = 0
        btnAll.Text = "&Tick all"
        btnAll.UseVisualStyleBackColor = True
        '
        'tipTypes
        '
        tipTypes.AutoPopDelay = 15000
        tipTypes.InitialDelay = 500
        tipTypes.SetToolTip(btnAll, "Show every type.")
        tipTypes.SetToolTip(btnNone, "Hide every type - then tick just the ones you want.")
        tipTypes.SetToolTip(btnOK, "Show the ticked types and hide the rest.")
        tipTypes.SetToolTip(btnCancel, "Close without changing anything.")
        '
        'TypesForm
        '
        AcceptButton = btnOK
        AutoScaleDimensions = New SizeF(7.0F, 15.0F)
        AutoScaleMode = AutoScaleMode.Font
        CancelButton = btnCancel
        ClientSize = New Size(620, 480)
        Controls.Add(lvTypes)
        Controls.Add(lblHint)
        Controls.Add(flpButtons)
        MaximizeBox = False
        MinimizeBox = False
        MinimumSize = New Size(460, 300)
        Name = "TypesForm"
        ShowIcon = False
        ShowInTaskbar = False
        StartPosition = FormStartPosition.CenterParent
        Text = "Part types"
        flpButtons.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents tipTypes As ToolTip
    Friend WithEvents lblHint As Label
    Friend WithEvents lvTypes As ListView
    Friend WithEvents colType As ColumnHeader
    Friend WithEvents colParts As ColumnHeader
    Friend WithEvents colSuppliers As ColumnHeader
    Friend WithEvents flpButtons As FlowLayoutPanel
    Friend WithEvents btnCancel As Button
    Friend WithEvents btnOK As Button
    Friend WithEvents btnNone As Button
    Friend WithEvents btnAll As Button

End Class
