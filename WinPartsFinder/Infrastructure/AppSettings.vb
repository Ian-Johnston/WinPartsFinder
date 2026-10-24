Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

''' <summary>
''' User settings, stored as JSON in %AppData%\WinPartsFinder\settings.json.
''' API keys/secrets are encrypted with Windows DPAPI (current user only).
''' </summary>
Public Class AppSettings

    ' Supplier names - also used as dictionary keys in this file.
    Public Const RS As String = "RS Components"
    Public Const Farnell As String = "Farnell"
    Public Const CPC As String = "CPC"
    Public Const DigiKey As String = "DigiKey"
    Public Const Mouser As String = "Mouser"
    Public Const Newark As String = "Newark"
    Public Const TME As String = "TME"
    Public Const Rapid As String = "Rapid"
    Public Const LCSC As String = "LCSC"
    Public Const AliExpress As String = "AliExpress"

    Public Shared ReadOnly SupplierOrder As String() = {Farnell, CPC, DigiKey, Mouser, Newark, TME, RS, Rapid, LCSC, AliExpress}

    ''' <summary>Website search URLs. {0} is replaced by the URL-encoded search text.</summary>
    Public Shared ReadOnly DefaultSearchUrls As New Dictionary(Of String, String) From {
        {RS, "https://uk.rs-online.com/web/c/?searchTerm={0}"},
        {Farnell, "https://uk.farnell.com/search?st={0}"},
        {CPC, "https://cpc.farnell.com/search?st={0}"},
        {DigiKey, "https://www.digikey.co.uk/en/products/result?keywords={0}"},
        {Mouser, "https://www.mouser.co.uk/c/?q={0}"},
        {Newark, "https://www.newark.com/search?st={0}"},
        {TME, "https://www.tme.eu/gb/en/katalog/?search={0}"},
        {Rapid, "https://www.rapidonline.com/search?query={0}"},
        {LCSC, "https://www.lcsc.com/search?q={0}"},
        {AliExpress, "https://www.aliexpress.com/wholesale?SearchText={0}"}
    }

    ' ---- Secrets (plain text in memory only) ----
    <JsonIgnore> Public Property MouserApiKey As String = ""
    <JsonIgnore> Public Property DigiKeyClientId As String = ""
    <JsonIgnore> Public Property DigiKeyClientSecret As String = ""
    <JsonIgnore> Public Property Element14ApiKey As String = ""
    <JsonIgnore> Public Property TmeToken As String = ""
    <JsonIgnore> Public Property TmeSecret As String = ""

    ' ---- Encrypted copies that are written to disk ----
    Public Property MouserApiKeyEnc As String = ""
    Public Property DigiKeyClientIdEnc As String = ""
    Public Property DigiKeyClientSecretEnc As String = ""
    Public Property Element14ApiKeyEnc As String = ""
    Public Property TmeTokenEnc As String = ""
    Public Property TmeSecretEnc As String = ""

    ' ---- DigiKey locale ----
    Public Property DigiKeySite As String = "UK"
    Public Property DigiKeyLanguage As String = "en"
    Public Property DigiKeyCurrency As String = "GBP"

    ' ---- Farnell country store (element14), e.g. "nz.element14.com" - the same key works for all of them ----
    Public Property FarnellStore As String = "uk.farnell.com"

    ' ---- TME locale (country decides stock/prices offered; currency of the prices) ----
    Public Property TmeCountry As String = "GB"
    Public Property TmeCurrency As String = "GBP"

    ' ---- General ----
    Public Property MaxResults As Integer = 25
    Public Property InStockOnly As Boolean = False
    Public Property ShowThumbnails As Boolean = True
    Public Property DarkMode As Boolean = False
    ''' <summary>User-added website-link suppliers, searched after the built-in ones.</summary>
    Public Property CustomSuppliers As New List(Of CustomSupplier)
    ''' <summary>Main window size/position from last time; Nothing = designer default.</summary>
    Public Property MainWindow As WindowPlacement
    ''' <summary>Results grid column widths and order, keyed by column name.</summary>
    Public Property ResultColumns As New Dictionary(Of String, ColumnLayout)
    Public Property EnabledSuppliers As New Dictionary(Of String, Boolean)
    Public Property SearchUrls As New Dictionary(Of String, String)
    Public Property RecentSearches As New List(Of String)
    ''' <summary>The Advanced search window reopens with these - while the app is open only (not saved),
    ''' so every run starts with a plain Advanced search.</summary>
    <JsonIgnore> Public Property LastAdvanced As New AdvancedSearchOptions

    Public Shared ReadOnly Property FilePath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WinPartsFinder", "settings.json")

    ''' <summary>Where settings lived when the app was called GetParts.</summary>
    Private Shared ReadOnly Property OldGetPartsFilePath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GetParts", "settings.json")

    ''' <summary>
    ''' First run after the rename: copy the old GetParts settings (API keys, layout, suppliers) across.
    ''' The keys still decrypt, because DPAPI ties them to the Windows user, not the folder.
    ''' The old file is left where it was, so the old GetParts build still works.
    ''' </summary>
    Private Shared Sub MoveOldSettings()
        Try
            If Not File.Exists(FilePath) AndAlso File.Exists(OldGetPartsFilePath) Then
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath))
                File.Copy(OldGetPartsFilePath, FilePath)
            End If
        Catch
            ' Not worth stopping for - the user can re-enter their keys in Settings.
        End Try
    End Sub

    Public Function GetSearchUrl(supplierName As String) As String
        Dim url As String = Nothing
        If SearchUrls.TryGetValue(supplierName, url) AndAlso Not String.IsNullOrWhiteSpace(url) Then Return url
        Return DefaultSearchUrl(supplierName, FarnellStore)
    End Function

    ''' <summary>The built-in website search URL. Farnell's follows the country store chosen in Settings.</summary>
    Public Shared Function DefaultSearchUrl(supplierName As String, farnellStore As String) As String
        If supplierName = Farnell Then Return $"https://{Element14Supplier.FindFarnellStore(farnellStore).Id}/search?st={{0}}"
        Dim url As String = Nothing
        Return If(DefaultSearchUrls.TryGetValue(supplierName, url), url, "")
    End Function

    Public Function IsEnabled(supplierName As String) As Boolean
        Dim v As Boolean
        Return Not EnabledSuppliers.TryGetValue(supplierName, v) OrElse v
    End Function

    Public Sub AddRecentSearch(text As String)
        RecentSearches.RemoveAll(Function(s) String.Equals(s, text, StringComparison.OrdinalIgnoreCase))
        RecentSearches.Insert(0, text)
        If RecentSearches.Count > 100 Then RecentSearches.RemoveRange(100, RecentSearches.Count - 100)
    End Sub

    Public Shared Function Load() As AppSettings
        MoveOldSettings()
        Dim s As AppSettings = Nothing
        Try
            If File.Exists(FilePath) Then
                ' Replace (not add to) the default empty lists/dictionaries when reading.
                s = JsonConvert.DeserializeObject(Of AppSettings)(File.ReadAllText(FilePath),
                        New JsonSerializerSettings With {.ObjectCreationHandling = ObjectCreationHandling.Replace})
            End If
        Catch ex As Exception
            MessageBox.Show("Could not read settings, defaults will be used." & vbCrLf & ex.Message,
                            "WinPartsFinder", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
        If s Is Nothing Then s = New AppSettings()
        s.MouserApiKey = Unprotect(s.MouserApiKeyEnc)
        s.DigiKeyClientId = Unprotect(s.DigiKeyClientIdEnc)
        s.DigiKeyClientSecret = Unprotect(s.DigiKeyClientSecretEnc)
        s.Element14ApiKey = Unprotect(s.Element14ApiKeyEnc)
        s.TmeToken = Unprotect(s.TmeTokenEnc)
        s.TmeSecret = Unprotect(s.TmeSecretEnc)
        Return s
    End Function

    Public Sub Save()
        MouserApiKeyEnc = Protect(MouserApiKey)
        DigiKeyClientIdEnc = Protect(DigiKeyClientId)
        DigiKeyClientSecretEnc = Protect(DigiKeyClientSecret)
        Element14ApiKeyEnc = Protect(Element14ApiKey)
        TmeTokenEnc = Protect(TmeToken)
        TmeSecretEnc = Protect(TmeSecret)
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath))
        File.WriteAllText(FilePath, JsonConvert.SerializeObject(Me, Formatting.Indented))
    End Sub

    Private Shared Function Protect(plain As String) As String
        If String.IsNullOrEmpty(plain) Then Return ""
        Dim bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), Nothing, DataProtectionScope.CurrentUser)
        Return Convert.ToBase64String(bytes)
    End Function

    Private Shared Function Unprotect(enc As String) As String
        If String.IsNullOrEmpty(enc) Then Return ""
        Try
            Dim bytes = ProtectedData.Unprotect(Convert.FromBase64String(enc), Nothing, DataProtectionScope.CurrentUser)
            Return Encoding.UTF8.GetString(bytes)
        Catch
            Return "" ' e.g. settings copied from another Windows user
        End Try
    End Function

End Class

''' <summary>Where the main window was, in its normal (not maximised) state.</summary>
Public Class WindowPlacement
    Public Property X As Integer
    Public Property Y As Integer
    Public Property Width As Integer
    Public Property Height As Integer
    Public Property Maximized As Boolean
    Public Property SplitterDistance As Integer
End Class

Public Class ColumnLayout
    Public Property Width As Integer
    Public Property DisplayIndex As Integer
End Class

''' <summary>A supplier the user added in Settings: just a name and a website search URL.</summary>
Public Class CustomSupplier
    Public Property Name As String = ""
    ''' <summary>{0} is replaced by the URL-encoded search text.</summary>
    Public Property SearchUrl As String = ""
End Class
