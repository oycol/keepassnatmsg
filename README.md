# KeePassNatMsg

[![CI Build & Test](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg?color=blue)](https://github.com/oycol/keepassnatmsg/releases/latest)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)

**KeePassNatMsg** is a modern KeePass 2.x plugin that securely exposes your KeePass credentials to browser extensions (specifically **[KeePassXC-Browser](https://github.com/keepassxreboot/keepassxc-browser)**) via Chrome Native Messaging.

---

## 🚀 Key Features in v2.2.0

- **Single-File Delivery (`KeePassNatMsg.plgx`)**:
  - No complex packages, manual DLL extractions, or external dependencies.
  - Simply drop `KeePassNatMsg.plgx` into your KeePass `Plugins\` folder.
- **Zero-Script One-Click Browser Integration (Chrome & Edge)**:
  - No PowerShell execution policy issues (`PSSecurityException`).
  - No manual `install.ps1` or `uninstall.ps1` scripts needed.
  - Set up or remove Native Messaging hosts for both **Microsoft Edge** and **Google Chrome** directly from KeePass UI (`Tools -> KeePassNatMsg Options -> Browser Integration (Chrome & Edge)`).
- **Embedded Proxy Engine**:
  - `keepassnatmsg-proxy.exe` is embedded directly inside the plugin assembly.
  - Automatically extracted to `%LOCALAPPDATA%\KeePassNatMsg` and verified via SHA256 integrity check.
  - Fully compatible with current-user `HKCU` registry entries (no administrator privileges required).
- **KeePassXC-Browser 1.10.4 Ready**:
  - Declares protocol version `2.7.0` ensuring full compatibility with the latest KeePassXC-Browser without false Passkeys capability claims.
- **Redesigned Options Interface**:
  - Modern, spacious UI layout with structured GroupBoxes:
    - **Chrome Browser Integration**: Real-time status display with one-click Install/Repair and Uninstall buttons.
    - **Credential Matching & Access Rules**: Intuitive URL matching, scheme checks, and unlock behavior controls.
    - **Result Sorting**: Sort credentials by username or title.
    - **Danger Zone**: Clearly isolated bypass settings (Always Allow Access/Updates) preventing accidental misconfiguration.
- **Battle-Tested & Automated E2E Regression**:
  - 85 automated unit tests in CI.
  - Verified on real Windows 11 interactive sessions across named pipe IPC, handshake protocols, and browser launch.

---

## 📥 Quick Installation (Windows)

1. **Download**:
   Grab the single `KeePassNatMsg.plgx` file from the [Latest Release](https://github.com/oycol/keepassnatmsg/releases/latest).
2. **Install**:
   Copy `KeePassNatMsg.plgx` to your KeePass Plugins directory:
   - Typical path: `C:\Program Files\KeePass Password Safe 2\Plugins\`
   - Or portable path: `<KeePass_Directory>\Plugins\`
3. **Configure in KeePass**:
   - Restart KeePass.
   - Open menu: `Tools -> KeePassNatMsg Options`.
   - In the **Browser Integration (Chrome & Edge)** section, click **Install / Repair Integration**.
   - The status indicator will turn green: `Ready (Host registered & verified)`.
4. **Connect from your browser**:
   - Install [KeePassXC-Browser](https://chromewebstore.google.com/detail/keepassxc-browser/oboonakemofpalcgghocfoadofidjkkk) from Chrome Web Store or Microsoft Edge Add-ons.
   - Click the extension icon and click **Connect**.
   - Confirm association in the KeePass popup prompt. That's it!

---

## 📦 Release Artifacts Explained

| Artifact | Purpose | Recommended For |
| :--- | :--- | :--- |
| **`KeePassNatMsg.plgx`** | **Single self-contained plugin package** containing full plugin logic, embedded proxy executable, and integration services. | **All standard KeePass 2.x users (Recommended)** |
| **`KeePassNatMsg-binary.zip`** | Clean pre-compiled plugin DLL and dependencies for environments where PLGX compilation is disabled by policy. | Advanced / enterprise environments |
| **`SHA256SUMS`** | Cryptographic hash list to verify download integrity. | Integrity verification |

> *Note: External helper scripts (`install.ps1`, `uninstall.ps1`, etc.) are completely deprecated and removed because all operations are handled natively inside the plugin.*

---

## ⚙️ Options Overview

Open via `Tools -> KeePassNatMsg Options`:

### General Tab
- **Browser Integration (Chrome & Edge)**:
  - **Status**: Displays real-time registry and manifest configuration state for both Chrome and Edge.
  - **Install / Repair**: Extracts the embedded proxy and writes current-user HKCU manifest keys for both Chrome and Edge.
  - **Uninstall**: Cleanly removes registered Chrome and Edge Native Messaging host keys and files.
- **Credential Matching**:
  - *Only return best matching entries for URL*: Uses Levenshtein distance to prioritize exact URL path matches.
  - *Match URL scheme*: Restricts credentials to `http` or `https` matching schemes.
  - *Include expired entries*: Allows querying expired entries if needed.
  - *Request unlock if database is locked*: Prompts for master password on browser connection attempt.
- **Result Sorting**: Sort entries by username or title.
- **Danger Zone**:
  - *Always allow access / updates without asking*: Bypasses KeePass confirmation dialogs (use with caution).

### Keys Tab
- Manage all paired browser associations. Revoke obsolete or untrusted browser public keys with a single click.

---

## 🛠️ Building from Source

### Prerequisites
- Windows 10/11 or Windows Server
- Visual Studio 2019/2022 or .NET SDK with MSBuild
- KeePass 2.35+

### Build Steps
```powershell
git clone https://github.com/oycol/keepassnatmsg.git
cd keepassnatmsg

# Build solution
nuget restore KeePassNatMsg.sln
msbuild KeePassNatMsg.sln /p:Configuration=Release

# Run unit tests
vstest.console.exe KeePassNatMsg.Tests\bin\Release\KeePassNatMsg.Tests.dll
```

---

## 📄 License & Attribution

- Released under the [GNU General Public License v3.0](LICENSE).
- Originally based on [KeePassHttp](https://github.com/pfn/keepasshttp) and [keepassnatmsg](https://github.com/smorks/keepassnatmsg).
- Embedded Native Messaging proxy based on `keepassnatmsg-proxy` (GPL-3.0).
