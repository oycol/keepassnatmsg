# KeePassNatMsg

[![CI](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg)](https://github.com/oycol/keepassnatmsg/releases/latest)

[English](README.en.md) · [兼容性说明](COMPATIBILITY.md)

KeePassNatMsg 是 KeePass 2.x 的 Native Messaging 插件，使 Chrome 和 Edge 中的 KeePassXC-Browser 可以访问 KeePass 数据库。

## 支持范围

- Windows 10/11 x64
- KeePass 2.35 及以上版本
- KeePassXC-Browser 1.10.4（已验证版本）
- Google Chrome、Microsoft Edge
- 协议版本 2.7.0
- 不支持 Passkeys / WebAuthn

## 安装

1. 从 [Releases](https://github.com/oycol/keepassnatmsg/releases/latest) 下载 `KeePassNatMsg.plgx`。
2. 复制到 KeePass 的 `Plugins` 目录。
3. 重启 KeePass 并打开数据库。
4. 打开 `Tools → KeePassNatMsg Options → Browser Integration`。
5. 点击 `Install / Repair`。
6. 重启 Chrome 和 Edge，在 KeePassXC-Browser 中点击 `Connect`，然后在 KeePass 中批准关联。

插件会把内置代理部署到当前用户目录，并注册 Chrome、Edge 所需的 HKCU Native Messaging 项；浏览器集成配置无需运行 PowerShell 安装脚本或使用管理员权限。若 KeePass 安装在 `Program Files`，首次复制 PLGX 文件仍可能需要管理员权限。

## Options

- **Browser Integration**：分别显示代理、Manifest、Chrome、Edge 的状态，并提供安装、修复、卸载。
- **Preferences**：匹配规则、数据库范围、新登录默认组和高风险免确认设置。
- **Associations**：管理当前数据库的浏览器关联；界面只显示密钥指纹，不显示密钥原文。

## URL 匹配

- 同一条目的 URL 可写多个 IPv4 网段，例如 `CIDR:10.125.1.0/24, CIDR:10.125.2.0/24`；只匹配数字 IPv4 主机，不查询 DNS，也不匹配网段外地址。
- 为避免全网凭据泄漏，不接受 `/0`；网络地址须与掩码对齐，其他无效规则不匹配。
- `Regex:` 已移除，不自动迁移；升级前请手动改为 CIDR 或普通 URL。
- CIDR 不区分 HTTP/HTTPS；浏览器扩展是否自动填入仍取决于扩展设置。

## 注意事项

- KeePass 与浏览器必须使用同一普通 Windows 用户运行；不要以管理员身份启动 KeePass。
- `Always allow credential access` 和 `Always allow credential updates` 会绕过确认弹窗，默认关闭。
- 手动加载的浏览器扩展如果使用不同扩展 ID，不在默认白名单内。

## 故障排查

如果扩展连接失败：

1. 确认 KeePass 正在运行且数据库已解锁。
2. 确认 KeePass 没有以管理员身份运行。
3. 在 Browser Integration 页点击 `Install / Repair`。
4. 完全退出并重启浏览器。

日志目录：`%LOCALAPPDATA%\KeePassNatMsg\`
