# KeePassNatMsg

[![CI Build & Test](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg?color=blue)](https://github.com/oycol/keepassnatmsg/releases/latest)

[English Documentation](README.en.md) | [兼容性矩阵与协议说明](COMPATIBILITY.md)

**KeePassNatMsg** 是一款面向 KeePass 2.x 的现代 Native Messaging 桥接插件，用于配合官方 **[KeePassXC-Browser](https://github.com/keepassxreboot/keepassxc-browser)** 扩展实现安全的凭据自动填充、密码生成与 TOTP 读取。

---

## 📌 支持范围与基线

| 维度 | 支持基线 | 说明 |
|---|---|---|
| **操作系统** | Windows 10 / Windows 11 (64-bit) | 一键注册通过 HKCU 注册表生效 |
| **KeePass 版本** | 2.35 ~ 2.60+ | 依赖 `PwDatabase.CustomData` 与 `PwEntry.CustomData` |
| **运行环境** | .NET Framework 4.8 编译 | 随 KeePass 2.x 在 Windows 运行 |
| **目标浏览器** | Google Chrome、Microsoft Edge | 支持官方商店扩展 ID，免脚本注册 |
| **通信协议** | KeePassXC-Browser 协议版本 **2.7.0** | 明确不支持 Passkeys / WebAuthn |

---

## 🚀 核心功能

- **单文件分发 (`KeePassNatMsg.plgx`)**：插件自包含所需依赖与内置代理，放入 `Plugins\` 目录即可使用。
- **免脚本双浏览器一键集成 (Chrome & Edge)**：直接在 KeePass Options 界面点击一键安装，自动部署代理并向当前用户的 `HKCU` 注册 Chrome 和 Edge 的 `NativeMessagingHosts`。
- **严格安全的主机匹配规则**：
  - 彻底移除了拼写模糊匹配（Levenshtein 距离），避免形似域名命中错误账号。
  - 支持主机完全匹配，以及安全单向子域名匹配（保存为根域名的条目可匹配子域名请求；子域名条目绝不反向匹配父域名请求）。
  - 支持协议方案（Scheme）隔离过滤（可选开启）。
- **原生多 URL 支持**：
  - 支持在条目的主 `URL` 字段中使用逗号分隔多个网址（如 `https://login.live.com/, https://login.microsoftonline.com/`）。
  - 开启 `SearchUrls` 时，自动解析高级属性中的 `URL1`、`URL2`、`KP2A_URL_1` 等附加字段。
- **轻量登录计数 (`get-logins-count`)**：计数接口独立统计匹配条目数量，不弹出访问确认，不解密密码，不展开 TOTP。
- **动态两步验证 (TOTP)**：支持 KeePass 原生 `{TIMEOTP}` 占位符，并兼容 `otp`、`TOTP Seed` 自定义字段。
- **标准数据库分组响应**：适配 KeePassXC-Browser 1.10.4 的规范顶层分组树结构与默认分组配置。

---

## 📥 快速安装指南 (Windows)

1. 从 [Releases](https://github.com/oycol/keepassnatmsg/releases/latest) 下载 `KeePassNatMsg.plgx`。
2. 将文件放入 KeePass 的 `Plugins\` 目录：
   - 默认安装版：`C:\Program Files\KeePass Password Safe 2\Plugins\`
   - 便携版：`<KeePass目录>\Plugins\`
3. 启动（或重启）KeePass，打开数据库。
4. 打开顶部菜单：`Tools -> KeePassNatMsg Options`。
5. 在 **Browser Integration** 区域点击 **Install / Repair Integration**。
   - 当状态显示为绿色 `Status: OK (Browser integration is active and verified)` 时，表示 Chrome 与 Edge 注册均已就绪。
6. 打开浏览器并安装 **KeePassXC-Browser**：
   - [Chrome Web Store 官方扩展](https://chromewebstore.google.com/detail/keepassxc-browser/pdffhmdngciaglkoonimfcmckehcpafo)
   - [Edge Add-ons 官方扩展](https://microsoftedge.microsoft.com/addons/detail/keepassxcbrowser/oboonakemofpalcgghocfoadofidjkkk)
7. 在浏览器中打开扩展弹窗，点击 **Connect**，在 KeePass 弹出的确认窗口中允许连接并输入名称。

---

## ⚠️ 明确限制与安全约定

1. **不支持 Passkeys (WebAuthn)**：
   - KeePass 2.x 原生数据库结构目前未内置 Passkeys 私钥存储模型。
   - 本插件**协议版本严格锁定为 `2.7.0`**，禁止虚标为 `2.7.7`，避免触发浏览器扩展的 Passkeys 逻辑而导致常规登录中断。
2. **不允许管理员权限运行 KeePass**：
   - Windows UIPI 权限隔离规则禁止普通权限的浏览器连接高权限（以管理员身份启动）进程创建的命名管道。
   - KeePass 与 Chrome/Edge 必须使用同一普通用户身份运行。
3. **扩展 ID 白名单限制**：
   - 为防止仿冒扩展窃取凭据，Manifest 中的 `allowed_origins` 仅内置了官方正式版 ID：
     - Chrome 官方版：`pdffhmdngciaglkoonimfcmckehcpafo`
     - Edge / Chromium 官方版：`oboonakemofpalcgghocfoadofidjkkk`
   - 手动加载的开发版或自解压版扩展因 ID 随机，默认不会被连接。
4. **一键安装边界**：
   - 一键安装/修复仅面向 Windows 环境下的 Chrome 与 Edge。Linux/macOS 需依赖 Unix Socket 与手工 Manifest 配置。
5. **不支持通配符 URL**：
   - 匹配引擎遵循严格规范化 Host 匹配，暂不支持包含 `*` 通配符的 URL（如 `https://*.example.com`）。

---

## 🛠️ 故障排查

- **提示“密钥交换未成功”或扩展红灯**：
  1. 确认 KeePass 正在运行且数据库处于已解锁状态。
  2. 确认 KeePass 没有以“管理员身份运行”。
  3. 进入 `Tools -> KeePassNatMsg Options`，重新点击一次 **Install / Repair Integration**。
  4. 完全重启 Chrome 或 Edge 浏览器（关闭后台驻留进程）。
- **特定网站匹配不到多账号**：
  - 检查条目的 URL 字段是否写了完整协议（`https://...`）。
  - 若为单账号多个登录入口，在条目高级设置中添加 `URL1` 属性写入备用入口。
- **诊断日志位置**：
  - `%LOCALAPPDATA%\KeePassNatMsg\plugin.log`
  - `%LOCALAPPDATA%\KeePassNatMsg\proxy.log`
  - 日志中可能包含请求的主机与时间，排查完毕后建议清理。
