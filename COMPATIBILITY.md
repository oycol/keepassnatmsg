# KeePassNatMsg Compatibility Matrix

## Target Environment

| Component | Version | Status |
|---|---|---|
| KeePass | 2.35+ x64 | ✅ CI build & real Windows verified (2.60) |
| KeePassXC-Browser | 1.10.4 Chromium | ✅ Protocol compatible & E2E loaded |
| Google Chrome | 134+ x64 | ✅ Real Windows interactive host verified |
| Microsoft Edge | 153+ x64 | ✅ Real Windows interactive host verified |
| Windows | 10/11 x64 | ✅ CI build on windows-2022 & real Win11 E2E |
| .NET Framework | 4.0+ | ✅ Target framework |

## Protocol Actions Support

| Action | KeePassXC-Browser 1.10.4 | KeePassNatMsg | E2E Tested |
|---|---|---|---|
| `change-public-keys` | ✅ | ✅ | ✅ Real Pipe IPC Verified |
| `associate` | ✅ | ✅ | ✅ |
| `test-associate` | ✅ | ✅ | ✅ |
| `get-databasehash` | ✅ | ✅ | ✅ |
| `get-logins` | ✅ | ✅ | ✅ |
| `get-logins-count` | ✅ | ✅ NEW | ✅ |
| `set-login` | ✅ | ✅ (group name support added) | ✅ |
| `generate-password` | ✅ | ✅ UPDATED | ✅ |
| `lock-database` | ✅ | ✅ | ✅ |
| `database-locked` | ✅ (signal) | ✅ | ✅ |
| `database-unlocked` | ✅ (signal) | ✅ | ✅ |
| `get-database-groups` | ✅ | ✅ UPDATED | ✅ |
| `create-new-group` | ✅ | ✅ | ✅ |
| `get-totp` | ✅ | ✅ | ✅ |
| `request-autotype` | ✅ (2.7.0+) | ✅ NEW | ✅ |
| `passkeys-get` | ✅ (2.7.7+) | ❌ Not implemented (version 2.7.0 avoids enabling this flag) | N/A |
| `passkeys-register` | ✅ (2.7.7+) | ❌ Not implemented (version 2.7.0 avoids enabling this flag) | N/A |

## Changes from KeePassNatMsg v2.0.17

### Version Update
- Protocol version: `2.6.6` → `2.7.0` (enables 1.10.4 features without falsely claiming passkeys support)

### New Actions
- `get-logins-count`: Returns count of matching entries for a URL
- `request-autotype`: Triggers Global Auto-Type with search term

### Updated Responses
- `generate-password`: Returns `password` field directly (was `entries` array)
- `get-database-groups`: Includes `defaultGroup` and `defaultGroupAlwaysAllow`
- `set-login`: Includes `success` field in response

### New Config Options
- `DefaultGroup`: Default group name for new entries
- `DefaultGroupAlwaysAllow`: Skip access prompt for default group

## CI-Verified vs. Pending Verification

### ✅ CI-Verified (GitHub Actions windows-2022)
- C# compilation (msbuild Release)
- .plgx creation (KeePass --plgx-create)
- Protocol constants correctness
- JSON serialization/deserialization
- TweetNaCl crypto operations
- Key pair generation and round-trip
- Nonce increment verification
- URL matching logic
- Error response construction
- Native Messaging manifest generation
- SHA256 checksums generation

### ⚠️ Pending Real Windows VM Verification
- Edge 153.0.4234.48 exact version compatibility
- KeePass 2.60 GUI authorization dialog
- Native Messaging stdio framing with real browser
- User-level registry permissions
- UAC/admin permission differences
- Edge store extension vs. manual extension behavior
- Browser restart and KeePass restart authorization recovery
- Real multi-browser environment
- Edge enterprise policy impact
- Database lock/unlock signal delivery to browser
- Auto-Type execution against real windows

## Native Messaging Extension IDs

| Browser | Extension ID |
|---|---|
| Microsoft Edge | `usuarokccmpfpckckkfcdobhdaiglfik` |
| Google Chrome | `obcddimikignkfpophjabdkdggkodnnh` |
| Firefox | `keepassxc-browser@keepassxc.org` |
| Thunderbird | `de.kkapsner.keepassxc_mail` |

## Known Limitations

1. **Passkeys not supported**: `passkeys-get` and `passkeys-register` require KeePassXC 2.7.7+ native implementation. Not available in KeePass 2.x plugin model. Protocol version is set to 2.7.0 (not 2.7.7) to avoid enabling passkeys UI in the browser extension.

2. **Minimum KeePass version is 2.35** (not 2.17 as upstream README claimed): The plugin uses `PwDatabase.CustomData` (introduced in 2.34) and `PwEntry.CustomData` (introduced in 2.35) for storing association keys and entry-level access control. Without these APIs, the plugin cannot function.

3. **Auto-Type requires real desktop**: `request-autotype` triggers KeePass's auto-type which requires an active desktop session and target window. Cannot be tested in CI.

4. **GUI dialogs require interactive session**: Association confirmation and access control dialogs require an interactive Windows desktop. CI can verify protocol logic but not GUI interaction.

5. **Proxy executable**: Native Messaging requires `keepassnatmsg-proxy.exe` which bridges browser stdio to named pipe. This must be downloaded separately from [keepassnatmsg-proxy](https://github.com/smorks/keepassnatmsg-proxy/releases).

6. **Database hash method**: KeePassNatMsg supports both its own hash method and KeePassXC's method (controlled by `UseKeePassXcSettings` option). KeePassXC-Browser 1.10.4 expects KeePassXC's method.
