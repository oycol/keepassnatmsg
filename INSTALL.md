# KeePassNatMsg Installation Guide

## Prerequisites

- KeePass 2.60 (or 2.x) installed at `C:\Program Files\KeePass Password Safe 2\`
- KeePassXC-Browser 1.10.4 extension installed in your browser
- Windows 10/11 x64
- `keepassnatmsg-proxy.exe` (downloaded from [releases](https://github.com/smorks/keepassnatmsg-proxy/releases))

## Quick Install

1. Download the latest release package (zip) from GitHub Releases
2. Extract the package
3. Open PowerShell (not as admin — user mode is sufficient)
4. Run the install script:

```powershell
.\install.ps1 -Browser edge
```

For all supported browsers:

```powershell
.\install.ps1 -Browser all
```

### Script Parameters

| Parameter | Default | Description |
|---|---|---|
| `-KeePassPath` | `C:\Program Files\KeePass Password Safe 2` | KeePass installation directory |
| `-Browser` | `edge` | Target browser: `edge`, `chrome`, `firefox`, `all` |
| `-Force` | (switch) | Reinstall even if already installed |

## Manual Install

### 1. Install the Plugin

Copy `KeePassNatMsg.plgx` to:
```
C:\Program Files\KeePass Password Safe 2\Plugins\
```

### 2. Install the Proxy

Copy `keepassnatmsg-proxy.exe` to:
```
%LOCALAPPDATA%\KeePassNatMsg\
```

### 3. Create Native Messaging Manifest

Create a JSON file at `%LOCALAPPDATA%\KeePassNatMsg\kpnm_edge.json`:

```json
{
    "name": "org.keepassxc.keepassxc_browser",
    "description": "KeePassXC-Browser native messaging host (KeePassNatMsg)",
    "type": "stdio",
    "path": "C:\\Users\\YOUR_USERNAME\\AppData\\Local\\KeePassNatMsg\\keepassnatmsg-proxy.exe",
    "allowed_origins": [
        "chrome-extension://usuarokccmpfpckckkfcdobhdaiglfik/"
    ]
}
```

Replace `YOUR_USERNAME` with your actual Windows username.

### 4. Register in Registry

For Edge:
```powershell
reg add "HKCU\Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser" /ve /t REG_SZ /d "%LOCALAPPDATA%\KeePassNatMsg\kpnm_edge.json" /f
```

For Chrome:
```powershell
reg add "HKCU\Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser" /ve /t REG_SZ /d "%LOCALAPPDATA%\KeePassNatMsg\kpnm_chrome.json" /f
```

For Firefox:
```powershell
reg add "HKCU\Software\Mozilla\NativeMessagingHosts\org.keepassxc.keepassxc_browser" /ve /t REG_SZ /d "%LOCALAPPDATA%\KeePassNatMsg\kpnm_firefox.json" /f
```

### 5. Configure KeePass

1. Restart KeePass
2. Go to **Tools → KeePassNatMsg Options**
3. Check **"Use KeePassXC-Browser Settings"** (important for 1.10.4 compatibility)
4. Open your database
5. Click the KeePassXC-Browser extension icon in your browser
6. Click **Connect**
7. Approve the association in the KeePass dialog

## Uninstall

```powershell
.\uninstall.ps1 -Browser all
```

Or manually:
1. Remove `KeePassNatMsg.plgx` from KeePass Plugins directory
2. Remove `%LOCALAPPDATA%\KeePassNatMsg\` directory
3. Remove registry entries for each browser

## Troubleshooting

### Extension cannot connect
- Verify `keepassnatmsg-proxy.exe` exists in `%LOCALAPPDATA%\KeePassNatMsg\`
- Verify the JSON manifest path in registry matches the actual file location
- Verify `allowed_origins` contains the correct extension ID
- Check that KeePass is running and a database is open
- Check **Use KeePassXC-Browser Settings** in KeePassNatMsg Options

### Association fails
- Ensure KeePass 2.60 is running
- Try deleting existing associations in KeePassNatMsg Options → Keys
- Check that the database is unlocked

### Plugin not loaded
- Verify `.plgx` file is in the correct Plugins directory
- Check KeePass version (requires 2.60 or later)
- Look for error messages in KeePass startup

### Proxy not found
- Download from https://github.com/smorks/keepassnatmsg-proxy/releases
- Place `keepassnatmsg-proxy.exe` in `%LOCALAPPDATA%\KeePassNatMsg\`
