# KeePassNatMsg 兼容性与协议支持矩阵

## 1. 运行环境基线

| 组件 | 版本要求 | 验证状态 | 依据与说明 |
|---|---|---|---|
| **KeePass** | 2.35 ~ 2.60+ | ✅ 已实机验证 | 2.60 实机运行通过；最低需要 2.35+ 的 `CustomData` 支持 |
| **KeePassXC-Browser** | 1.10.4+ | ✅ 协议兼容 | 锁定 2.7.0 通信协议，避免 2.7.7+ 触发未实现的 Passkeys |
| **Google Chrome** | 134+ x64 | ✅ 实机验证 | 官方扩展 ID 注册与命名管道通信测试通过 |
| **Microsoft Edge** | 120+ / 153+ x64 | ✅ 注册表验证 | 支持写入 HKCU Edge NativeMessagingHosts 并在实机断言 |
| **操作系统** | Windows 10/11 x64 | ✅ CI 与实机验证 | CI windows-2022 构建，本地 Win11 实机管道验证通过 |
| **目标运行时** | .NET Framework 4.8 | ✅ 构建目标 | 项目 TargetFrameworkVersion 为 v4.8 |

---

## 2. 协议动作 (Actions) 支持与验证深度

| Action 动作 | 动作说明 | 插件支持 | 单元测试 | 管道验证 | 真实扩展端到端 |
|---|---|---|---|---|---|
| `change-public-keys` | 协商密钥交换 | ✅ 完整支持 | ✅ | ✅ | ✅ 实机命名管道闭环已验证 |
| `associate` | 关联客户端并命名 | ✅ 完整支持 | ✅ | ⚠️ 依赖 GUI 弹窗 | 需交互式桌面点击允许 |
| `test-associate` | 校验已保存关联密钥 | ✅ 完整支持 | ✅ | ⚠️ 待补充无头脚本 | 依赖已关联 CustomData |
| `get-databasehash` | 获取数据库标识哈希 | ✅ 完整支持 | ✅ | ✅ | 握手阶段返回 |
| `get-logins` | 按 URL 查询凭据 | ✅ 严格主机与多 URL | ✅ | ⚠️ 依赖解密与条目 | 单元测试覆盖主机与 Scheme |
| `get-logins-count` | 轻量统计凭据数量 | ✅ 独立只读统计 | ✅ | ⚠️ 待补管道断言 | 不弹授权、不读密码 |
| `set-login` | 保存或更新凭据 | ✅ 支持分组路径 | ✅ | ⚠️ 依赖用户确认 | 默认弹窗确认更新 |
| `generate-password` | 调用配置生成强密码 | ✅ 返回 password 字段 | ✅ | ⚠️ 依赖 KeePass 配置 | 符合 1.10.4 协议格式 |
| `lock-database` | 锁定所有已打开数据库 | ✅ 调用 LockAllDocuments | ✅ | ⚠️ 待补会话断言 | 触发 KeePass 主界面锁库 |
| `database-locked` | 数据库锁定事件信号 | ✅ 主动向通道广播 | ✅ | ⚠️ 待捕获信号 | 事件监听器触发 |
| `database-unlocked` | 数据库解锁事件信号 | ✅ 主动向通道广播 | ✅ | ⚠️ 待捕获信号 | 解锁事件触发 |
| `get-database-groups` | 获取分组树与默认组 | ✅ 顶层规范 JSON 结构 | ✅ | ⚠️ 依赖打开数据库 | 已适配 1.10.4 字段要求 |
| `create-new-group` | 创建指定路径分组 | ✅ 支持斜杠多级分组 | ✅ | ⚠️ 待测试持久化 | 自动查找或创建子树 |
| `get-totp` | 按条目 UUID 读取两步验证 | ✅ 兼容原生与旧插件 | ✅ | ⚠️ 依赖 TOTP 配置 | 支持 {TIMEOTP}、otp、TOTP Seed |
| `request-autotype` | 触发全局自动输入 | ⚠️ 尽力而为模式 | ⚠️ | ❌ 不支持无头会话 | 依赖目标活动窗口 |
| `passkeys-get` | 获取 Passkeys 凭据 | ❌ 明确不支持 | N/A | N/A | 协议锁定 2.7.0 避免开启该功能 |
| `passkeys-register` | 注册 Passkeys 凭据 | ❌ 明确不支持 | N/A | N/A | 协议锁定 2.7.0 避免开启该功能 |

---

## 3. 内置代理与 Native Messaging 说明

- **代理打包形态**：`keepassnatmsg-proxy.exe` 已经作为嵌入资源直接编译入 `KeePassNatMsg.plgx` 中，**用户无需单独下载外部代理**。
- **部署位置**：点击“Install / Repair Integration”后，插件自动解压代理并生成 Manifest 写入当前用户目录：
  - 文件目录：`%LOCALAPPDATA%\KeePassNatMsg\`
  - Manifest：`org.keepassxc.keepassxc_browser.json`
- **注册表路径 (HKCU)**：
  - Chrome：`HKCU\Software\Google\Chrome\NativeMessagingHosts\org.keepassxc.keepassxc_browser`
  - Edge：`HKCU\Software\Microsoft\Edge\NativeMessagingHosts\org.keepassxc.keepassxc_browser`
- **官方扩展白名单**：
  - Chrome Web Store：`pdffhmdngciaglkoonimfcmckehcpafo`
  - Edge Add-ons：`oboonakemofpalcgghocfoadofidjkkk`
