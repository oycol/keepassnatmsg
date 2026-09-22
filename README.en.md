# KeePassNatMsg

[![CI](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg)](https://github.com/oycol/keepassnatmsg/releases/latest)

[中文](README.md) · [Compatibility](COMPATIBILITY.md)

KeePassNatMsg is a Native Messaging plugin for KeePass 2.x. It connects KeePassXC-Browser in Google Chrome and Microsoft Edge to a KeePass database.

## Supported

- Windows 10/11 x64
- KeePass 2.35 or newer
- KeePassXC-Browser 1.10.4 (verified version)
- Google Chrome and Microsoft Edge
- Protocol version 2.7.0
- Passkeys / WebAuthn are not supported

## Install

1. Download `KeePassNatMsg.plgx` from [Releases](https://github.com/oycol/keepassnatmsg/releases/latest).
2. Copy it to the KeePass `Plugins` directory.
3. Restart KeePass and open the database.
4. Open `Tools → KeePassNatMsg Options → Browser Integration`.
5. Click `Install / Repair`.
6. Restart Chrome and Edge, click `Connect` in KeePassXC-Browser, then approve the association in KeePass.

The plugin deploys its bundled proxy and registers the required HKCU Native Messaging entries. Browser integration does not require a PowerShell installer or administrator rights. Copying the PLGX into a KeePass installation under `Program Files` may still require elevation.

## Options

- **Browser Integration** shows separate proxy, manifest, Chrome, and Edge status.
- **Preferences** contains matching, database scope, default group, and high-risk prompt bypass settings.
- **Associations** manages browser associations and displays fingerprints instead of secret key material.

## Notes

- Run KeePass and the browsers as the same standard Windows user. Do not run KeePass as administrator.
- The two “Always allow” settings bypass confirmation dialogs and are disabled by default.
- Unpacked browser extensions with a different extension ID are not allowed by the default manifest.

## Troubleshooting

If the extension cannot connect:

1. Confirm KeePass is running and the database is unlocked.
2. Confirm KeePass is not elevated.
3. Click `Install / Repair` on the Browser Integration page.
4. Fully restart the browser.

Logs are stored under `%LOCALAPPDATA%\KeePassNatMsg\`.
