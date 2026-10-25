Public Class SettingsForm

    Private ReadOnly _settings As AppSettings

    ''' <summary>True if the user asked to put the main window back to its original size and position.</summary>
    Public ReadOnly Property ResetWindowRequested As Boolean
        Get
            Return Not btnResetWindow.Enabled
        End Get
    End Property

    ' dgvUrls row tags: built-in suppliers can be edited but not renamed or deleted.
    Private Const BuiltInTag As String = "builtin"
    Private Const CustomTag As String = "custom"

    ' Parameterless constructor for the WinForms designer.
    Public Sub New()
        Me.New(New AppSettings())
    End Sub

    Public Sub New(settings As AppSettings)
        InitializeComponent()
        _settings = settings
    End Sub

    Private Sub SettingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        txtMouserKey.Text = _settings.MouserApiKey
        txtDkClientId.Text = _settings.DigiKeyClientId
        txtDkSecret.Text = _settings.DigiKeyClientSecret
        cboDkSite.Items.AddRange(DigiKeySupplier.Sites)
        cboDkCurrency.Items.AddRange(DigiKeySupplier.Currencies)
        cboDkSite.Text = _settings.DigiKeySite
        cboDkCurrency.Text = _settings.DigiKeyCurrency
        txtE14Key.Text = _settings.Element14ApiKey
        cboFarnellStore.Items.AddRange(Element14Supplier.FarnellStores.ToArray())
        cboFarnellStore.SelectedItem = Element14Supplier.FindFarnellStore(_settings.FarnellStore)
        txtTmeToken.Text = _settings.TmeToken
        txtTmeSecret.Text = _settings.TmeSecret
        cboTmeCountry.Items.AddRange(TmeSupplier.Countries)
        cboTmeCurrency.Items.AddRange(TmeSupplier.Currencies)
        cboTmeCountry.Text = _settings.TmeCountry
        cboTmeCurrency.Text = _settings.TmeCurrency
        cboConvertCurrency.Items.AddRange(ExchangeRates.Currencies)
        cboConvertCurrency.SelectedItem = If(ExchangeRates.Currencies.Contains(_settings.ConvertCurrency), _settings.ConvertCurrency, "GBP")
        ShowRatesInfo()
        chkShowThumbnails.Checked = _settings.ShowThumbnails
        cboTheme.Items.AddRange(Theme.Names)
        cboTheme.SelectedIndex = CInt(_settings.AppTheme)
        LoadUrls(Function(name) _settings.GetSearchUrl(name), _settings.CustomSuppliers)
        Theme.Apply(Me)
    End Sub

    Private Sub LoadUrls(getUrl As Func(Of String, String), customs As IEnumerable(Of CustomSupplier))
        dgvUrls.Rows.Clear()
        For Each supplierName In AppSettings.SupplierOrder
            Dim row = dgvUrls.Rows(dgvUrls.Rows.Add(supplierName, getUrl(supplierName)))
            row.Tag = BuiltInTag
            row.Cells(0).ReadOnly = True
            row.Cells(0).ToolTipText = "Built-in supplier (can't be renamed or deleted)"
        Next
        For Each c In customs
            AddCustomRow(c.Name, c.SearchUrl)
        Next
        UpdateDeleteButton()
    End Sub

    Private Function AddCustomRow(name As String, url As String) As DataGridViewRow
        Dim row = dgvUrls.Rows(dgvUrls.Rows.Add(name, url))
        row.Tag = CustomTag
        row.Cells(0).ToolTipText = "Your supplier - click to rename"
        Return row
    End Function

    Private Function IsCustom(row As DataGridViewRow) As Boolean
        Return row IsNot Nothing AndAlso CStr(row.Tag) = CustomTag
    End Function

    Private Function CurrentCustoms() As List(Of CustomSupplier)
        Return dgvUrls.Rows.Cast(Of DataGridViewRow)().Where(AddressOf IsCustom).
            Select(Function(r) New CustomSupplier With {
                .Name = Convert.ToString(r.Cells(0).Value).Trim(),
                .SearchUrl = Convert.ToString(r.Cells(1).Value).Trim()}).ToList()
    End Function

    Private Sub btnAddSupplier_Click(sender As Object, e As EventArgs) Handles btnAddSupplier.Click
        dgvUrls.EndEdit()
        Dim existing = dgvUrls.Rows.Cast(Of DataGridViewRow)().
            Select(Function(r) Convert.ToString(r.Cells(0).Value)).ToHashSet(StringComparer.OrdinalIgnoreCase)
        Dim name = "New supplier"
        Dim n = 2
        While existing.Contains(name)
            name = $"New supplier {n}"
            n += 1
        End While
        Dim row = AddCustomRow(name, "https://www.example.com/search?q={0}")
        dgvUrls.CurrentCell = row.Cells(0)
        dgvUrls.BeginEdit(True)
    End Sub

    Private Sub btnDeleteSupplier_Click(sender As Object, e As EventArgs) Handles btnDeleteSupplier.Click
        Dim row = dgvUrls.CurrentRow
        If Not IsCustom(row) Then Return
        dgvUrls.CancelEdit()
        dgvUrls.Rows.Remove(row)
        UpdateDeleteButton()
    End Sub

    Private Sub dgvUrls_CurrentCellChanged(sender As Object, e As EventArgs) Handles dgvUrls.CurrentCellChanged
        UpdateDeleteButton()
    End Sub

    Private Sub UpdateDeleteButton()
        btnDeleteSupplier.Enabled = IsCustom(dgvUrls.CurrentRow)
    End Sub

    Private Function ValidateUrls() As Boolean
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataGridViewRow In dgvUrls.Rows
            Dim name = Convert.ToString(row.Cells(0).Value).Trim()
            Dim url = Convert.ToString(row.Cells(1).Value).Trim()
            Dim problem As String = Nothing
            Dim badCell = 1

            If name.Length = 0 Then
                problem = "Every supplier needs a name."
                badCell = 0
            ElseIf Not seen.Add(name) Then
                problem = $"There are two suppliers called ""{name}""."
                badCell = 0
            ElseIf IsCustom(row) AndAlso url.Length = 0 Then
                problem = $"Enter a search URL for {name}."
            ElseIf url.Length > 0 AndAlso Not url.Contains("{0}") Then
                problem = $"The search URL for {name} must contain {{0}} where the search text goes."
            ElseIf url.Length > 0 AndAlso Not (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) OrElse
                                               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) Then
                problem = $"The search URL for {name} must start with https:// (or http://)."
            End If

            If problem IsNot Nothing Then
                MessageBox.Show(Me, problem, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
                dgvUrls.CurrentCell = row.Cells(badCell)
                Return False
            End If
        Next
        Return True
    End Function

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        dgvUrls.EndEdit()
        If Not ValidateUrls() Then Return

        _settings.MouserApiKey = txtMouserKey.Text.Trim()
        _settings.DigiKeyClientId = txtDkClientId.Text.Trim()
        _settings.DigiKeyClientSecret = txtDkSecret.Text.Trim()
        _settings.DigiKeySite = cboDkSite.Text.Trim().ToUpperInvariant()
        _settings.DigiKeyCurrency = cboDkCurrency.Text.Trim().ToUpperInvariant()
        _settings.Element14ApiKey = txtE14Key.Text.Trim()
        _settings.FarnellStore = SelectedFarnellStore()
        _settings.TmeToken = txtTmeToken.Text.Trim()
        _settings.TmeSecret = txtTmeSecret.Text.Trim()
        If cboTmeCountry.Text.Trim().Length > 0 Then _settings.TmeCountry = cboTmeCountry.Text.Trim().ToUpperInvariant()
        If cboTmeCurrency.Text.Trim().Length > 0 Then _settings.TmeCurrency = cboTmeCurrency.Text.Trim().ToUpperInvariant()
        If cboConvertCurrency.SelectedItem IsNot Nothing Then _settings.ConvertCurrency = CStr(cboConvertCurrency.SelectedItem)
        _settings.ShowThumbnails = chkShowThumbnails.Checked
        _settings.AppTheme = CType(Math.Max(0, cboTheme.SelectedIndex), ThemeKind)

        _settings.SearchUrls.Clear()
        For Each row As DataGridViewRow In dgvUrls.Rows
            If IsCustom(row) Then Continue For
            Dim name = Convert.ToString(row.Cells(0).Value)
            Dim url = Convert.ToString(row.Cells(1).Value).Trim()
            ' Only store overrides, so future default changes still apply.
            If url.Length > 0 AndAlso url <> AppSettings.DefaultSearchUrl(name, _settings.FarnellStore) Then _settings.SearchUrls(name) = url
        Next
        _settings.CustomSuppliers = CurrentCustoms()

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub btnResetUrls_Click(sender As Object, e As EventArgs) Handles btnResetUrls.Click
        ' Resets the built-in URLs only; your own suppliers are kept.
        dgvUrls.EndEdit()
        LoadUrls(Function(name) AppSettings.DefaultSearchUrl(name, SelectedFarnellStore()), CurrentCustoms())
    End Sub

    ''' <summary>"Rates: ECB rates of 23 Oct 2026 (fetched 25 Oct 10:02)" - or that there are none yet.</summary>
    Private Sub ShowRatesInfo()
        Dim r = _settings.Rates
        lblRates.Text = If(r.HasRates AndAlso r.FetchedUtc.HasValue,
                           $"{r.Describe()} (fetched {r.FetchedUtc.Value.ToLocalTime():d MMM HH:mm})",
                           "No rates yet.")
    End Sub

    ' Rates are data, not a preference, so a successful update is kept even if Settings is then cancelled.
    Private Async Sub btnUpdateRates_Click(sender As Object, e As EventArgs) Handles btnUpdateRates.Click
        btnUpdateRates.Enabled = False
        lblRates.Text = "Getting today's rates…"
        Try
            _settings.Rates = Await ExchangeRates.DownloadAsync(CancellationToken.None)
            ShowRatesInfo()
        Catch ex As Exception
            ShowRatesInfo()
            MessageBox.Show(Me, ex.Message & vbCrLf & "Check your internet connection and try again.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Finally
            btnUpdateRates.Enabled = True
        End Try
    End Sub

    Private Function SelectedFarnellStore() As String
        Dim st = TryCast(cboFarnellStore.SelectedItem, Element14Supplier.Store)
        Return If(st Is Nothing, _settings.FarnellStore, st.Id)
    End Function

    ' Changing the Farnell store also moves Farnell's website link to that store - unless you'd set your own link.
    Private _shownFarnellStore As String

    Private Sub cboFarnellStore_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboFarnellStore.SelectedIndexChanged
        Dim newStore = SelectedFarnellStore()
        If _shownFarnellStore IsNot Nothing AndAlso newStore <> _shownFarnellStore Then
            For Each row As DataGridViewRow In dgvUrls.Rows
                If Convert.ToString(row.Cells(0).Value) = AppSettings.Farnell AndAlso
                   Convert.ToString(row.Cells(1).Value).Trim() = AppSettings.DefaultSearchUrl(AppSettings.Farnell, _shownFarnellStore) Then
                    row.Cells(1).Value = AppSettings.DefaultSearchUrl(AppSettings.Farnell, newStore)
                End If
            Next
        End If
        _shownFarnellStore = newStore
    End Sub

    Private Sub btnResetWindow_Click(sender As Object, e As EventArgs) Handles btnResetWindow.Click
        ' Takes effect on OK, like everything else here.
        btnResetWindow.Enabled = False
        btnResetWindow.Text = "Layout resets on OK"
    End Sub

    Private Sub chkShowKeys_CheckedChanged(sender As Object, e As EventArgs) Handles chkShowKeys.CheckedChanged
        For Each tb In {txtMouserKey, txtDkClientId, txtDkSecret, txtE14Key, txtTmeToken, txtTmeSecret}
            tb.UseSystemPasswordChar = Not chkShowKeys.Checked
        Next
    End Sub

    Private Sub cboTheme_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cboTheme.SelectedIndexChanged
        ' Live preview on this window; the main window follows when OK is clicked.
        If Not IsHandleCreated OrElse cboTheme.SelectedIndex < 0 Then Return
        Theme.Current = CType(cboTheme.SelectedIndex, ThemeKind)
        Theme.Apply(Me)
    End Sub

    Private Sub SettingsForm_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        If DialogResult <> DialogResult.OK Then Theme.Current = _settings.AppTheme ' undo the preview
    End Sub

    ' API Help isn't modal: it's owned by this window, so the key boxes stay usable while it's open,
    ' and it closes along with Settings. One at a time.
    Private _apiHelp As HelpForm

    Private Sub btnApiHelp_Click(sender As Object, e As EventArgs) Handles btnApiHelp.Click
        If _apiHelp IsNot Nothing AndAlso Not _apiHelp.IsDisposed Then
            _apiHelp.Activate()
            Return
        End If
        _apiHelp = New HelpForm(apiHelp:=True) With {.StartPosition = FormStartPosition.Manual}
        ' Beside Settings (right if there's room, else left) so it doesn't cover the key boxes.
        Dim area = Screen.FromControl(Me).WorkingArea
        Dim w = Math.Min(_apiHelp.Width, area.Width)
        Dim h = Math.Min(Math.Max(_apiHelp.MinimumSize.Height, Height), area.Height)
        Dim x = If(Right + w <= area.Right, Right, Math.Max(area.Left, Left - w))
        _apiHelp.Bounds = New Rectangle(x, Math.Max(area.Top, Math.Min(Top, area.Bottom - h)), w, h)
        _apiHelp.Show(Me)
    End Sub

    Private Sub KeyLink_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
        Handles lnkMouser.LinkClicked, lnkDigiKey.LinkClicked, lnkE14.LinkClicked, lnkTme.LinkClicked, lnkRates.LinkClicked
        WebHelper.OpenInBrowser(CStr(DirectCast(sender, LinkLabel).Tag))
    End Sub

End Class
