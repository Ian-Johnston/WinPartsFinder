# WinPartsFinder

**Search the big electronic component suppliers at once — stock, prices, price breaks, MOQ and lifecycle status side by side in one grid.**

Built for repairing test equipment and other electronics: type a part number or a few keywords, press Enter, and see who has it, how many, and at what price — without opening six browser tabs.

Windows 10 / 11 desktop app, written in VB.NET (Visual Studio 2022, .NET Framework 4.8).

<!-- Add a screenshot: save it as docs/WinPartsFinder_Main.png in the repo and this line will show it -->
![WinPartsFinder](docs/WinPartsFinder_Main.png)

---

## Suppliers

| Supplier | How it's searched | What you need |
|---|---|---|
| **Mouser** | Live API — stock, prices, lifecycle | Free Mouser **Search API** key |
| **DigiKey** | Live API — stock, prices, lifecycle | Free DigiKey **Client ID + Client Secret** |
| **Farnell** (UK) | Live API — stock, prices | Free **element14** API key |
| **CPC** (UK) | Live API — stock, prices | Same element14 key as Farnell |
| **Newark** (US) | Live API — stock, prices | Same element14 key as Farnell |
| **TME** (EU, ships to UK) | Live API — stock, prices | Free TME **Token + Application secret** |
| **RS Components** | Website link | Nothing — no public search API |
| **Rapid** | Website link | Nothing — no public search API |
| **LCSC** | Website link | Nothing — no official public search API |
| **AliExpress** | Website link | Nothing — only an affiliate API |
| **Your own** | Website link | Add any supplier in Settings with its search URL |

A *website link* row opens that supplier's own search results in your browser with one click. Any API supplier without a key works as a website link too, so the app is useful straight away.

## Features

- **One search, every supplier** — results arrive as each supplier answers.
- **Advanced search** (Ctrl+F) — exact part number, look up a supplier order code across every supplier, in stock / RoHS / active only, maker, quantity pricing and max price.
- **Load more** — fetch the next batch from suppliers that have more (e.g. *50 / 312*).
- **Part photos** — thumbnails, click for a bigger view.
- **Click the supplier part number** to open the product page; right-click for the datasheet or to copy part numbers.
- **Sort, filter and tidy** — sort by price or stock, *In stock only*, a live text filter, remove rows you don't want, untick a supplier to hide its results.
- **Row numbers** (#) that stay put, so you can note a part and find it again.
- **Export to CSV** for Excel.
- **Dark mode**, remembered window and column layout, tooltips on everything, built-in Help.

## Download and run

1. Download `WinPartsFinder.msi` from the latest release on the **Releases** page.
2. Run it. It installs to `C:\Program Files (x86)\IanJ\WinPartsFinder\` with a Start menu and desktop shortcut.
   - Nothing else to install: it uses .NET Framework 4.8, which is built into Windows 10 and 11.
   - The installer isn't code-signed yet, so Windows SmartScreen may say *"Windows protected your PC"* — click **More info → Run anyway**.
3. Start **WinPartsFinder**, click **Settings…**, paste in your API keys (see below) and search.

To upgrade, just run the newer `.msi` — it replaces the old version and keeps your settings and keys. Uninstall from **Settings → Apps**.

## Getting your free API keys

Each user needs their own keys — they're free. The **Get a key** links in Settings open each sign-up page.

**Mouser**
1. Go to the [Mouser API Hub](https://www.mouser.co.uk/api-hub/) and sign up for the **Search API**.
2. Paste the key into *Mouser Search API key*.

**DigiKey**
1. Sign in at [developer.digikey.com](https://developer.digikey.com/) and create an **Organization**.
2. Add a **Production App**. Select **ProductInformation V4** (leave the others as *None*). The *OAuth Callback* can be `https://localhost` — it isn't used.
3. Open the app and copy its **Client ID** and **Client Secret** into Settings.
4. Choose your DigiKey **site** and **currency** (e.g. UK / GBP).

**element14 (Farnell, CPC + Newark)**
1. Register an application at [partner.element14.com](https://partner.element14.com/).
2. Tick *Issue a new key for Product Search API*. The **Basic** tier is plenty for one person.
3. Paste the key into *element14 API key* — it works for Farnell, CPC and Newark.

**TME** (two websites are involved)
1. At [developers.tme.eu](https://developers.tme.eu/en) sign up, enter an application name (e.g. *WinPartsFinder*) and click **Create application**. This gives you the **Application secret**.
2. In another tab, log in to your **shop** account at [www.tme.eu](https://www.tme.eu) (not the developers site) — create a free customer account if you don't have one. Click **User panel → Applications → Register new app** and copy the **temporary token** it shows.
3. Back on developers.tme.eu, paste it into **Temporary token**, choose Key purpose **To compare distributors offer**, click **Generate new private key**, then **Show private keys**.
4. Paste that **Token** and the **Application secret** into Settings.

Free tiers allow roughly 1,000 calls a day per supplier; each search, and each *Load more*, is one call.

## Your keys and privacy

- Keys are stored **encrypted for your Windows user only** (Windows DPAPI) in `%AppData%\WinPartsFinder\settings.json`. They are never stored in the program or its source code.
- The app talks only to the suppliers' own APIs and websites. No accounts, no tracking, no telemetry.

## Building from source

1. Clone the repo and open `WinPartsFinder.sln` in **Visual Studio 2022** (17.8 or later) with the *.NET desktop development* workload.
2. Press **F5**.

The version number lives in one place: `AppVersion` at the top of `WinPartsFinder/Forms/MainForm.vb`.

```
WinPartsFinder/
  Forms/           Main window, Settings, Help, About, photo pop-up
  Suppliers/       Mouser, DigiKey, element14 (Farnell/Newark), website-link suppliers
  Models/          PartResult, PriceBreak
  Infrastructure/  Settings, theming (dark mode), web/JSON helpers, photo cache
```

**Adding an API supplier:** create a class in `Suppliers/` that inherits `ApiSupplier`, implement `IsConfigured` and `SearchAsync`, then add it in `AppSettings` and `SupplierFactory.Create`.

## Licence and usage

WinPartsFinder is provided **free of charge for personal, non-commercial use only**.
The source code is publicly available and may be used, modified and redistributed **for non-commercial purposes only**.
Neither WinPartsFinder nor its source code, in whole or in part, may be used for commercial purposes — including use within businesses and organisations — without a separate commercial licence from the author. Please contact the author via [www.ianjohnston.com](https://www.ianjohnston.com) to discuss commercial licensing.

Each user must obtain their own supplier API keys and follow each supplier's API terms of use.

## Disclaimer

WinPartsFinder and its source code are provided "as is", without warranty of any kind, express or implied. Use is entirely at your own risk.
Prices, stock levels and other part data come from third parties and may be out of date or wrong — **always check with the supplier before ordering.**
Supplier names and trademarks belong to their respective owners. WinPartsFinder is not affiliated with or endorsed by any supplier.

## Support WinPartsFinder

If you find it useful, please consider a donation — it helps fund more development and the faulty test equipment repaired on my YouTube channel.

[**Donate via PayPal**](https://www.paypal.com/donate/?business=ian%40ianj.net&no_recurring=0&item_name=Support+WinPartsFinder&currency_code=GBP)

## Author

**Ian Johnston**
- Website: [www.ianjohnston.com](https://www.ianjohnston.com)
- YouTube: [youtube.com/user/IanScottJohnston](https://www.youtube.com/user/IanScottJohnston)
- Twitter / X: [@IanSJohnston](https://www.twitter.com/IanSJohnston)
