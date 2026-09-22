# KeePassNatMsg

[![CI Build & Test](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg?color=blue)](https://github.com/oycol/keepassnatmsg/releases/latest)

[中文说明 (Chinese)](README.md) | [Compatibility Matrix](COMPATIBILITY.md)

**KeePassNatMsg** is a modern Native Messaging bridge plugin for KeePass 2.x, designed to integrate seamlessly with the official **[KeePassXC-Browser](https://github.com/keepassxreboot/keepassxc-browser)** extension for credential auto-fill, secure password generation, and TOTP retrieval.

---

## 📌 Support Baseline

| Dimension | Supported Baseline | Details |
|---|---|---|
| **Operating System** | Windows 10 / Windows 11 (64-bit) | Registered under user HKCU registry |
| **KeePass Version** | 2.35 ~ 2.60+ | Uses `PwDatabase.CustomData` & `PwEntry.CustomData` |
| **Runtime Target** | .NET Framework 4.8 | Runs inside standard KeePass 2.x process |
| **Target Browsers** | Google Chrome, Microsoft Edge | Official store extension IDs supported out-of-the-box |
| **Protocol Version** | KeePassXC-Browser Protocol **2.7.0** | Passkeys / WebAuthn are strictly NOT supported |

---

## 🚀 Key Features

- **Single PLGX File Delivery (`KeePassNatMsg.plgx`)**: Self-contained with bundled native proxy and dependencies. Drop into `Plugins\` to run.
- **Zero-Script One-Click Browser Integration (Chrome & Edge)**: Configure both Google Chrome and Microsoft Edge `NativeMessagingHosts` directly from KeePass Options without touching PowerShell scripts.
- **Strict & Secure URL Matching**:
  - Completely removed Levenshtein fuzzy distance matching to prevent credential leaks to typo-squatted domains.
  - Enforces exact host matching and one-way parent-to-subdomain matching (root domain entries can match subdomains, but child subdomain entries never match parent requests).
  - Optional scheme matching (HTTP vs HTTPS isolation).
- **Native Multi-URL Support**:
  - Recognizes comma-separated URLs in the primary `URL` field (e.g., `https://login.live.com/, https://login.microsoftonline.com/`).
  - Supports custom string fields like `URL1`, `URL2`, `KP2A_URL_1` in the Advanced tab.
- **Lightweight Login Counting (`get-logins-count`)**: Returns candidate count without prompting user access, decrypting passwords, or resolving TOTP.
- **Two-Factor Authentication (TOTP)**: Supports KeePass native `{TIMEOTP}` placeholder as well as legacy `otp` and `TOTP Seed` fields.
- **Standard Database Groups Payload**: Conforms to KeePassXC-Browser 1.10.4 top-level group tree format.

---

## 📥 Quick Installation (Windows)

1. Download `KeePassNatMsg.plgx` from [Releases](https://github.com/oycol/keepassnatmsg/releases/latest).
2. Copy it to your KeePass plugins folder:
   - Installed version: `C:\Program Files\KeePass Password Safe 2\Plugins\`
   - Portable version: `<KeePass-Folder>\Plugins\`
3. Launch KeePass and open your database.
4. Open `Tools -> KeePassNatMsg Options`.
5. Under **Browser Integration**, click **Install / Repair Integration**.
   - Ensure the status turns green: `Status: OK (Browser integration is active and verified)`.
6. Install **KeePassXC-Browser** from the official store:
   - [Chrome Web Store](https://chromewebstore.google.com/detail/keepassxc-browser/pdffhmdngciaglkoonimfcmckehcpafo)
   - [Edge Add-ons](https://microsoftedge.microsoft.com/addons/detail/keepassxcbrowser/oboonakemofpalcgghocfoadofidjkkk)
7. Open the browser extension popup, click **Connect**, and approve the connection in KeePass.

---

## ⚠️ Known Limitations & Security Notes

1. **No Passkeys (WebAuthn) Support**:
   - KeePass 2.x currently lacks native storage models for WebAuthn private keys.
   - The protocol version is **strictly locked to `2.7.0`**. Never claim `2.7.7+`, as doing so causes browser extensions to crash normal password logins when requesting Passkeys.
2. **Do Not Run KeePass as Administrator**:
   - Windows UIPI blocks normal browser processes from connecting to elevated named pipes. Both KeePass and browsers must run under the same standard user account.
3. **Strict Extension ID Allowlist**:
   - Manifest `allowed_origins` is restricted to official stable extension IDs (`pdffhmdngciaglkoonimfcmckehcpafo` and `oboonakemofpalcgghocfoadofidjkkk`). Sideloaded / unpacked extensions with random IDs will be rejected.
4. **Platform Scope**:
   - The one-click installer currently targets Windows Chrome & Edge. Linux and macOS require manual manifest configuration.
5. **No Wildcard URLs**:
   - Wildcards like `https://*.example.com` are not parsed as valid absolute URIs and are unsupported.

---

## 🛠️ Troubleshooting

- **Key exchange failed / extension disconnected**:
  1. Ensure KeePass is running and the database is unlocked.
  2. Verify KeePass was not started with "Run as administrator".
  3. Re-run **Install / Repair Integration** in Options.
  4. Fully restart your Chrome or Edge browser.
- **Log Locations**:
  - `%LOCALAPPDATA%\KeePassNatMsg\plugin.log`
  - `%LOCALAPPDATA%\KeePassNatMsg\proxy.log`
