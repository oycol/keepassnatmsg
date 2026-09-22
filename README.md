# KeePassNatMsg

[![CI Build & Test](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml/badge.svg)](https://github.com/oycol/keepassnatmsg/actions/workflows/ci.yml)
[![Latest Release](https://img.shields.io/github/v/release/oycol/keepassnatmsg?color=blue)](https://github.com/oycol/keepassnatmsg/releases/latest)

*[English Version Below](#english-version)*

**KeePassNatMsg** 是一款现代化的 KeePass 2.x 插件，它通过“原生消息传递（Native Messaging）”技术，将 KeePass 数据库与浏览器扩展（专为 **[KeePassXC-Browser](https://github.com/keepassxreboot/keepassxc-browser)** 设计）安全连接。

---

## 🚀 v2.2.0 全新特性 (断代级重构)

- **纯净单文件交付 (`KeePassNatMsg.plgx`)**:
  - 无需手工解压杂乱的 DLL，无需执行外部安装脚本。
  - 下载单文件丢进 `Plugins\` 目录即刻可用。
- **免脚本的一键双浏览器集成 (Chrome & Edge)**:
  - 彻底告别 PowerShell 脚本执行策略报错（`PSSecurityException`）。
  - 直接在 KeePass 界面点击一键安装，自动为 **Microsoft Edge** 和 **Google Chrome** 注册原生消息宿主。
- **内嵌 64 位纯净原生 Proxy**:
  - 弃用了上游动辄崩溃的旧版代理。现在插件自带了防假死、防截断的 64 位强健通信引擎，后台自动提取并验证 SHA256。
  - 完全适配当前用户的 `HKCU` 注册表，**无需管理员权限**。
- **零模糊匹配（防钓鱼级安全严格）**:
  - 剔除历史代码中为了“容错”而使用的模糊距离匹配算法。
  - 现在的匹配规则基于**顶级域名严格比对（eTLD+1）**，100% 杜绝形似钓鱼网站（如 `rnicrosoft.com`）骗取密码。
- **KeePassXC-Browser 1.10.4 全功能就绪**:
  - 精确锁定 `2.7.0` 通信协议，完美支持新扩展的全部能力，且不会因错误宣称支持 Passkeys 导致浏览器崩溃。
- **重新设计、上手即用的选项界面**:
  - 为所有配置项添加了友好的鼠标悬浮气泡提示（Tooltip）。
  - 默认配置已调优（默认支持多 URL 高级字段搜索，默认宽容显示所有同域账号），即装即用。

---

## 📥 快速安装指南

1. **下载**:
   从 [Latest Release](https://github.com/oycol/keepassnatmsg/releases/latest) 下载唯一的 `KeePassNatMsg.plgx` 文件。
2. **安装**:
   将该文件复制到 KeePass 的插件目录中。
   - 默认安装版路径: `C:\Program Files\KeePass Password Safe 2\Plugins\`
   - 便携版路径: `<你的KeePass目录>\Plugins\`
3. **在 KeePass 中开启集成**:
   - 重启 KeePass。
   - 顶部菜单选择：`Tools -> KeePassNatMsg Options`。
   - 在 **Browser Integration** 区域点击 **Install / Repair Integration**。
   - 当状态变绿提示 `Ready` 后即可点击 OK。
4. **连接浏览器**:
   - 在应用商店安装 [KeePassXC-Browser](https://chromewebstore.google.com/detail/keepassxc-browser/oboonakemofpalcgghocfoadofidjkkk)。
   - 点击浏览器右上角的扩展图标，点击 **Connect（连接）**，并在 KeePass 弹窗中允许即可。

---

## 💡 多网址 / 微软多级页面匹配技巧

遇到像微软那样复杂的登录（如先在 `login.microsoftonline.com` 输账号，又跳去 `login.live.com` 输密码）时，**不要创建多个重复的条目**！

- 在 KeePass 编辑条目。
- 在“常规 (General)”的 **URL** 框填入第一个网址（如 `https://login.live.com/`）。
- 切换到“高级 (Advanced)”标签页，在“字符串字段 (String fields)”里点击“添加”。
- 名字写 **`URL1`**，值写另一个网址（如 `https://login.microsoftonline.com/`）。
插件现在会自动提取所有 URL 字段同时进行安全匹配！

---
<br/><br/>

---
# English Version

**KeePassNatMsg** is a modern KeePass 2.x plugin that securely exposes your credentials to browser extensions (specifically **[KeePassXC-Browser](https://github.com/keepassxreboot/keepassxc-browser)**) via Native Messaging.

## 🚀 Key Features in v2.2.0 (Architectural Overhaul)

- **Single-File Delivery (`KeePassNatMsg.plgx`)**: Drop `KeePassNatMsg.plgx` into your KeePass `Plugins\` folder. No external scripts or manual proxy downloads.
- **Zero-Script One-Click Browser Integration (Chrome & Edge)**: Set up Native Messaging hosts for both **Microsoft Edge** and **Google Chrome** directly from KeePass UI without fighting PowerShell execution policies.
- **Embedded 64-bit Proxy Engine**: Replaced upstream brittle proxy with a loop-reading, synchronous 64-bit executable that eliminates Stdin partial packet truncation and crashes on Windows 11.
- **Zero Fuzzy Matching (Anti-Phishing Strictness)**: Removed historical Levenshtein fuzzy matching. Now uses strict eTLD+1 matching to 100% prevent typo-squatting phishing attacks (e.g. `rnicrosoft.com`).
- **KeePassXC-Browser 1.10.4 Ready**: Locks protocol version to `2.7.0` ensuring full compatibility with modern features without causing browser crash loops over unimplemented Passkeys.
- **Redesigned & User-Friendly Options**: Modern layout with descriptive ToolTips. Default configurations are optimized out-of-the-box.

## 📥 Quick Installation (Windows)

1. **Download**: Grab `KeePassNatMsg.plgx` from the [Latest Release](https://github.com/oycol/keepassnatmsg/releases/latest).
2. **Install**: Copy it to your KeePass Plugins directory (e.g., `C:\Program Files\KeePass Password Safe 2\Plugins\`).
3. **Configure in KeePass**:
   - Restart KeePass.
   - Open menu: `Tools -> KeePassNatMsg Options`.
   - Click **Install / Repair Integration**. Wait for the status to turn green (`Ready`).
4. **Connect from your browser**:
   - Install [KeePassXC-Browser](https://chromewebstore.google.com/detail/keepassxc-browser/oboonakemofpalcgghocfoadofidjkkk) from Chrome Web Store or Edge Add-ons.
   - Click the extension icon and hit **Connect**. Confirm the popup in KeePass.

## 💡 Multiple URLs matching

To match an entry against multiple URLs (e.g., `login.live.com` and `login.microsoftonline.com`), do NOT create duplicate entries.
Simply add the primary URL in the General tab, go to the **Advanced** tab, and add a new String Field named **`URL1`** with the second URL. The plugin natively evaluates all of them for a strict match.
