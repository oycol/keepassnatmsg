# KeePass Options 专用 Windows 会话高 DPI 验收计划

## 已验证现状
- 当前自托管 Runner 在线、空闲；提升的 admin SSH 位于 Session 0，交互式 Runner 和 admin 控制台位于 Session 3。SSH 主机 ED25519 指纹已通过 Runner 自报与当前 SSH 服务对照。
- 机器没有符合 KeePass 专用测试命名的本地用户，未启用 Windows 自动登录。当前探针显示 2560×1440、有效 96 DPI；4K 是枚举支持，不是实测通过。
- 上次 PLGX 备份恢复已在工作流 36616212197 回读成功并清除保留状态；E2E 36609814526 失败，不能当作高 DPI 证据。

## 安全门槛
1. 不保存或在聊天、日志、进程参数中传递测试用户密码；不设置无密码账号或明文 AutoAdminLogon。专用用户的首次交互登录必须通过经批准的凭据通道完成；仅 SSH 管理员令牌不构成可用的交互桌面。
2. 在任何停用旧 Runner 前，记录当前 Runner 注册 ID、进程/会话、启动位置、原桌面分辨率和实际 DPI、以及现有文件/注册表备份状态；确认 SSH 在 Runner 离线时依旧可达。重启旧 Runner 的途径要经过实测，不能只写恢复命令。
3. 专用用户在已登录且解锁的桌面启动 Runner，验证 job 进程与 KeePass 在同一 Session ID；失败时先恢复旧会话 Runner，禁止再切显示。
4. DPI 档位通过 Windows 受支持的 UI 设置配置；若必须注销，仅注销专用测试用户并重新登录。禁止写未公开的缩放接口或仅靠注册表/请求值判定成功。每档必须读回 KeePass 进程所在会话的实际 DPI ≥144。
5. 2K 和 4K 分别记录显示模式原值、CDS_TEST、切换后的实际分辨率/DPI；在真实 KeePass 主窗打开 Tools → KeePassNatMsg Options，四页逐一 Select 并检验静态标签、控件矩形、工作区边界、底部版本及 Save/Cancel；只按 Cancel。每档 finally 恢复原显示模式并读回；失败立即停止后续档位。
6. 每档 E2E 严格区分 passed、blocked 和 failed；blocked 不计入高 DPI 验收。E2E 必须使用唯一合成库，备份/恢复插件和 HKCU Native Messaging 注册项，最终回读哈希及 Runner 在线状态。
7. 完成后恢复旧 Runner 的原会话和启动方式，验证 GitHub API 在线、只读 job 成功、专用测试会话不再接单；绝不打 tag 或发布未经 CI/E2E 通过的版本。

## 执行顺序
- [ ] A. 验证可用且不泄漏凭据的专用用户交互登录方式及旧 Runner 的可靠重启方式；预演恢复。若未通过，停止在只读状态。
- [ ] B. 创建最小权限专用用户并配置隔离目录/测试库；通过已验证交互方式登录；确认会话与 Runner 归属。不得在 Session 0 直接跑 GUI 检查。
- [ ] C. 在专用会话配置 150% 或以上缩放、必要时注销重登；读回实际 DPI 和显示模式，不足即 blocked。
- [ ] D. 对 2K、4K 依次执行四页宿主窗口验收；上一档未恢复则不得继续下一档。
- [ ] E. 恢复旧用户会话、Runner、原显示设置和插件/注册，回读证据；同一 SHA CI 与真实 E2E 全绿后报告结果。

## 当前阻塞
SSH 管理权限已验证，但尚无已验证的专用用户交互登录方式或旧 Session 3 Runner 的无人值守恢复方式。不可为了完成测试而自动启用不安全自动登录、设置空密码或注销唯一可用的交互 Runner。
