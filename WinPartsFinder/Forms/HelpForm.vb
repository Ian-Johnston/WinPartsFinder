' pop-up help window

Public Class HelpForm

    ' Help text. Simple markup, turned into rich text by ShowContent():
    '   # Heading       - bold heading
    '   - text          - bullet point
    '   **word**        - bold, inside any line
    '   (blank line)    - paragraph gap
    Private Const HelpContent As String = "
# GETTING STARTED
WinPartsFinder searches several component suppliers at once and puts stock, prices, minimum order and lifecycle status side by side in one grid.

- Click **Settings…** and enter your API keys (see API KEYS below). Without keys, suppliers still appear as website links.
- Type a part number or some keywords in the **Search** box, e.g. LM358 or 100nF 50V 0603, and press **Enter**.
- Results arrive from each supplier as soon as it answers. Click any column header to sort, e.g. by Price or Stock.

# API KEYS
Mouser, DigiKey, element14 (Farnell, CPC + Newark) and TME have free APIs. Each user needs their own keys. Links to each sign-up page are in Settings.
- **Mouser:** sign up at the Mouser API Hub for a **Search API** key.
- **DigiKey:** at developer.digikey.com create an Organization, then a **Production App** with **ProductInformation V4** selected. The OAuth Callback can be https://localhost. Copy the Client ID and Client Secret.
- **element14:** register an application at partner.element14.com. The **Basic** tier is plenty. One key covers Farnell, CPC and Newark.
- **TME:** this takes two websites. (1) At developers.tme.eu sign up, enter an application name and click **Create application** - that gives the **Application secret**. (2) In another tab log in to your shop account at **www.tme.eu** (not the developers site), click **User panel > Applications > Register new app** and copy the **temporary token** it shows. (3) Back on developers.tme.eu paste it into **Temporary token**, choose Key purpose **To compare distributors offer**, click **Generate new private key**, then **Show private keys**. Enter that **Token** and the **Application secret** in Settings.
Keys are saved encrypted for your Windows user only, in %AppData%\WinPartsFinder\settings.json. They are never stored in the program or its source code.

# SEARCHING
- **Keyword** searches descriptions and part numbers. **Manufacturer part no.** looks for parts whose manufacturer part number starts with what you typed.
- **Max/supp.** is how many results are fetched from each supplier per search, up to 50. Suppliers don't send more than 50 at a time - use **Load more** for the next batch.
- **Cancel** stops any searches still running.
- Previous searches drop down as you type in the Search box.

# ADVANCED SEARCH
Click **Advanced…** next to Search, or press **Ctrl+F**. The window remembers your choices from last time. Results go into the same grid, and the bottom status line lists the Advanced options used. A plain **Search** afterwards goes back to normal.
- **Search type:** **Keyword**; **Manufacturer part no. (starts with)**; **Exact manufacturer part no.** (only parts whose number matches exactly, ignoring spaces); or **Supplier order code**, e.g. a Farnell 3117070 or Mouser 595-LM358DR. A code belongs to one supplier: WinPartsFinder finds it there, reads the manufacturer part number, then searches every other ticked supplier for that exact part.
- **In stock only** and **RoHS only** are passed to each supplier's own search, so you get a full batch of matching parts. TME has no RoHS option, so it ignores that box.
- **Active parts only** hides parts marked obsolete, discontinued, end of life or not recommended for new designs. Parts with no status shown are kept.
- **Manufacturer contains**, e.g. Texas, keeps only that maker's parts.
- **Description also contains** keeps parts whose description or part number has every word you type. Suppliers word things differently - DigiKey says 2 CIRCUIT rather than dual, for example.
- **Quantity needed** shows the price each at that quantity (the column header changes to e.g. **Price (100)**) and hides parts whose minimum order is more than that.
- **Max price each** hides parts dearer than that at your quantity (or at 1). Parts with no price listed are hidden too. Prices are compared in each supplier's own currency.
- **Suppliers** sets which suppliers are searched; the supplier list on the left changes to match.
- The options from **Active parts only** down are applied by WinPartsFinder after each batch arrives, so a supplier's **Hits** can be lower than the batch size. Hover over the supplier to see how many were left out. Load more and newly ticked suppliers use the same options.
- **Clear** resets everything except the search text. **Cancel** closes the window without searching.

# THE SUPPLIER LIST
- Tick the suppliers to search. Your choice is remembered.
- After a search, **untick** a supplier to hide its rows straight away, and **tick** it again to bring them back. Nothing is lost until the next search.
- Ticking a supplier that **wasn't part of the search** searches just that supplier straight away and adds its results at the end.
- **Mode** shows how each supplier is searched: **API** (live data), **API (no key)** (add a key in Settings), or **Web link**.
- **Status**, **Hits** and **Time** show how each search went. If it says **Error**, hover over the supplier to see why.
- **Hits** like **50 / 312** means 50 shown out of 312 the supplier has, so Load more is available. Once everything has been fetched it shows just the count, which can be a little under the supplier's total: suppliers sometimes send the same part twice, and repeats are skipped (hover over the supplier to see how many).
- **Not searched yet** means the supplier was ticked while a search was running. Press **Search** again to include it.
- Double-click a supplier, or right-click it, to open the same search on that supplier's website.

# THE RESULTS
- **#** numbers each row so you can note a part and find it again. Numbers are given when a search finishes and never change - removing, filtering or sorting rows leaves them as they are. Click **#** to sort by it. A new search starts again from 1; Load more carries on from the last number. Website-link rows aren't parts, so they have no number - the last # always matches **Total parts** in the bottom-right corner.
- **Stock** is green when in stock. Rows with no stock are greyed out.
- **Price (1)** is the price each at the lowest price break, usually quantity 1 (after an Advanced search with a quantity, the price at that quantity). **Price breaks** lists every break as quantity+ price.
- **Datasheet** opens the part's datasheet in your browser (hover to see the link). It's blank when the supplier didn't send one - TME and the website-link suppliers never do.
- **MOQ** is the minimum order quantity. **Availability / lead** shows packaging and lead time. **Status** shows lifecycle, e.g. Obsolete.
- Blue italic rows are **website links** for suppliers without an API (RS, Rapid, LCSC, AliExpress and any you add). Click them to see results on that website.
- Click a **Supplier part no.** to open the product page. Double-clicking anywhere else on a row does the same.
- Photos load in the background after the results appear. Click a **photo** for a bigger view; it stays on top until you click it or press Esc, and clicking another photo swaps it. A box with a cross means no photo is available.
- Mouser does not allow its photos to be downloaded, so Mouser rows borrow a photo of the same part from DigiKey or Farnell when one was found.
- Search results are grouped by supplier, in supplier-list order; rows from **Load more** are added at the end. Click a header such as **Price (1)** to sort by it, **Supplier** to put each supplier's rows together, or **#** to go back to the original order.
- Drag a column edge to resize it, or drag a header to move the column (**#** always stays first). Column layout, window size and position, and the divider beside the supplier list are all remembered. The window can't be made smaller than its starting size.

# LOAD MORE
When a supplier has more matches than it sent, its **Hits** shows e.g. **50 / 312** and **Load more** becomes available.
- **Load more** (toolbar) fetches the next batch from every ticked supplier that has more.
- **Right-click a supplier > Load 50 more from …** fetches the next batch from just that supplier.
- New rows are added at the end (or into place if you've sorted by a column), and are selected so you can spot them. Click the **Supplier** header to put each supplier's rows together.
- Each Load more uses one API call per supplier. The batch size is the **Max/supp.** number.
- Mouser can't load more when searching by **Manufacturer part no.** A new search starts again from the first batch.
- Advanced searches by **Exact manufacturer part no.** or **Supplier order code** never need Load more: each supplier sends just the matching part(s).
- If what you want isn't in the first batch, a more exact search usually works better than loading more, e.g. add the package: LM358 SOIC.

# RIGHT-CLICK MENU
- **Open product page** and **Open datasheet** open the supplier's pages in your browser.
- **Copy manufacturer / supplier part no.** copies just that number. **Copy row** copies the selected rows, including their **#**, ready to paste into Excel.
- **Remove from results** (or the **Delete** key) hides parts you don't want. Select several with **Ctrl**-click or **Shift**-click, or all with **Ctrl+A**.
- **Restore removed rows** brings them all back. Removed rows stay removed if you untick and re-tick their supplier. A new search always starts with nothing removed.

# FILTERING AND EXPORT
- **In stock only** hides parts with no stock.
- **Filter** narrows the current results as you type. It matches the supplier, both part numbers, manufacturer and description. It is not case sensitive.
- **Total parts = …** in the bottom-right corner counts every part found in this search (website-link rows aren't counted). When Filter, In stock only, removed rows or unticked suppliers hide some, it adds **(showing …)** for how many you can see.
- **Export CSV…** saves the rows currently shown, including their **#**, product and datasheet links, to a file that opens in Excel.

# SETTINGS
- Paste your API keys in the boxes at the top. **Get a key** opens each supplier's sign-up page, and **Show keys** shows them as text instead of dots.
- **DigiKey site** and **Currency** choose which DigiKey country prices and stock are shown for.
- **Search URLs** are used for website links. {0} is replaced by your search text. **Reset URLs** puts the built-in ones back.
- **Add supplier** adds your own supplier as a website link: give it a name and its search URL with {0} where the search text goes, e.g. https://cpc.farnell.com/search?st={0}. **Delete supplier** removes it.
- **Show part photos** turns the photo column on or off. **Dark mode** switches the whole app to a dark theme.
- **Reset main window layout** puts the main window's size and position, the divider, and the results column widths and order back to how they started. Like everything in Settings, it happens when you click **OK**; **Cancel** changes nothing.
- Saving settings keeps your current results, supplier statuses and Load more position.

# SUPPLIER NOTES
- **Mouser** prices are in the currency of the account your key was registered with.
- **DigiKey** shows the packaging with the smallest minimum order, usually Cut Tape, which suits repairs best.
- **Farnell** and **CPC** prices are in GBP and **Newark** in USD. They are searched one after the other to stay within element14's limit of 2 calls per second.
- **TME** shows UK stock and prices in GBP. Prices are those of your own tme.eu account (net, ex VAT). Each search uses a few TME calls (log in, search, prices).
- **RS Components**, **Rapid** and **LCSC** have no public search API, so they appear as website links.
- **AliExpress** also appears as a website link. Its only API is for registered affiliates, so it isn't built in.
- Free API tiers allow roughly 1000 calls a day per supplier. Each search, and each Load more, is one call. Mouser also allows 30 calls a minute.

# TROUBLESHOOTING
- **HTTP 401** - the key or secret is wrong. Paste it again in Settings, checking for spaces at either end.
- **HTTP 403** - the key isn't active yet, or (DigiKey) ProductInformation V4 isn't selected on your app.
- **Timeout** - the supplier was slow to answer. Search again.
- If the main window ends up off-screen or a mess, use **Settings > Reset main window layout**.
- Prices and stock come from the suppliers and can change quickly. Always check on the supplier's website before ordering.

# GETTING HELP
- Hover over any button, box or column header for a tip about what it does.
- This Help window can stay open while you use WinPartsFinder. **Esc** closes it, and the About window too.
- **About…** shows the version, licence and credits, and has the **Donate via PayPal** button. The version number is also in the title bar.
"

    Private Sub HelpForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Theme.Apply(Me)
        ' A read-only RichTextBox goes grey; keep it looking like a page.
        If Not Theme.IsDark Then
            rtbHelp.BackColor = SystemColors.Window
            pnlText.BackColor = SystemColors.Window
        Else
            pnlText.BackColor = rtbHelp.BackColor
        End If
        ShowContent()
    End Sub

    Private Sub ShowContent()
        Dim body = rtbHelp.Font
        Using bold As New Font(body, FontStyle.Bold),
              heading As New Font(body.FontFamily, body.Size + 1.5F, FontStyle.Bold),
              gap As New Font(body.FontFamily, 4.0F)
            rtbHelp.Clear()
            Dim lines = HelpContent.Replace(vbCrLf, vbLf).Trim(ChrW(10)).Split(ChrW(10))
            For Each raw In lines
                Dim line = raw.TrimEnd()
                If line.StartsWith("# ") Then
                    If rtbHelp.TextLength > 0 Then Append(vbLf, gap, False)
                    Append(line.Substring(2) & vbLf, heading, False)
                    Append(vbLf, gap, False)
                ElseIf line.StartsWith("- ") Then
                    AppendRich(line.Substring(2), body, bold, bullet:=True)
                ElseIf line.Length = 0 Then
                    Append(vbLf, gap, False)
                Else
                    AppendRich(line, body, bold, bullet:=False)
                End If
            Next
        End Using
        ' The newline after the last bullet starts an empty paragraph that would still show a bullet - delete it.
        ' (A read-only RichTextBox ignores deletes, so unlock it briefly.)
        If rtbHelp.TextLength > 0 AndAlso rtbHelp.Text.EndsWith(ChrW(10)) Then
            rtbHelp.ReadOnly = False
            rtbHelp.Select(rtbHelp.TextLength - 1, 1)
            rtbHelp.SelectedText = ""
            rtbHelp.ReadOnly = True
        End If
        rtbHelp.SelectionStart = 0
        rtbHelp.ScrollToCaret()
    End Sub

    ''' <summary>Appends one paragraph, with **bold** parts.</summary>
    Private Sub AppendRich(text As String, body As Font, bold As Font, bullet As Boolean)
        Dim parts = text.Split({"**"}, StringSplitOptions.None)
        For i = 0 To parts.Length - 1
            If parts(i).Length > 0 Then Append(parts(i), If(i Mod 2 = 1, bold, body), bullet)
        Next
        Append(vbLf, body, bullet)
    End Sub

    Private Sub Append(text As String, font As Font, bullet As Boolean)
        rtbHelp.SelectionStart = rtbHelp.TextLength
        rtbHelp.SelectionLength = 0
        rtbHelp.SelectionFont = font
        rtbHelp.SelectionColor = Theme.TextColor
        rtbHelp.SelectionBullet = bullet
        rtbHelp.SelectionIndent = If(bullet, 8, 0)
        rtbHelp.BulletIndent = If(bullet, 14, 0)
        rtbHelp.SelectedText = text
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Close()
    End Sub

End Class
