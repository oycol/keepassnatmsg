# KeePassNatMsg Rollback Guide

## Rollback to KeePassNatMsg v2.0.17

If the KeePassXC-Browser 1.10.4 compatible version causes issues, you can roll back to the original KeePassNatMsg v2.0.17.

### Step 1: Uninstall Current Version

```powershell
.\install\uninstall.ps1 -Browser all
```

This removes:
- Plugin file (.plgx or .dll)
- Native Messaging Host manifest
- Registry entries

**Preserved:**
- Your KDBX database
- Association keys in Custom Data
- Entry-level Allow/Deny configurations

### Step 2: Install Original v2.0.17

1. Download `KeePassNatMsg.plgx` from the [v2.0.17 release](https://github.com/smorks/keepassnatmsg/releases/tag/v2.0.17)
2. Copy to `C:\Program Files\KeePass Password Safe 2\Plugins\`
3. Restart KeePass

### Step 3: Verify

1. Open KeePass
2. Go to Tools → KeePassNatMsg Options
3. Verify your existing keys are still listed
4. Reconnect your browser extension

## Rollback to KeePassRPC (Kee)

If you want to switch entirely to the Kee browser extension:

1. Uninstall KeePassNatMsg (see Step 1 above)
2. Install [KeePassRPC](https://github.com/kee-org/keepassrpc) plugin
3. Install the [Kee](https://www.kee.pm) browser extension
4. Kee and KeePassXC-Browser use different protocols and association keys

## Rollback to KeePassHTTP (deprecated)

1. Uninstall KeePassNatMsg
2. Install [KeePassHTTP](https://github.com/pfn/keepasshttp) plugin
3. Use a browser extension that supports the KeePassHTTP protocol

**Warning**: KeePassHTTP is deprecated and has known security limitations.

## Notes

- Association keys are stored in database Custom Data with prefix `KPXC_BROWSER_` (when using KeePassXC-Browser settings) or `KeePassNatMsgDbKey_` (legacy mode)
- Switching between KeePassNatMsg settings mode and KeePassXC-Browser settings mode can be done in Options without losing keys
- The `Move Config` feature in Options can migrate keys between settings modes
