# KeePass 宿主 Options 2K/4K 验收实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax.

**Goal:** 在专用 Windows 交互 Runner 的真实 KeePass 宿主中，分别在 2560×1440 与 3840×2160、有效 DPI ≥144 时验证四页 Options 可见、控件不裁切，并证实显示模式恢复；不符合前置条件则明确阻塞。

**Architecture:** 复用现有一次性合成库和 KeePass 进程，修改 `scripts/verify-options-hosted-display.ps1` 的物理设备枚举与可见控件验证；工作流仅增加模式前置检查与独立证据读取，不把 `CreateControl()` 当成宿主实测。整个测试不写 KeePass 配置，仅通过 Tools 菜单打开并以 Cancel 关闭；显示更改在 `finally` 恢复并回读。

**Tech Stack:** Windows PowerShell 5.1、Win32 EnumDisplaySettings/ChangeDisplaySettingsEx、UI Automation、GitHub Actions 自托管交互式 Runner。

**Spec:** 用户在对话中确认的 2K/4K 宿主窗口设计；本计划固化分辨率/DPI/恢复硬门槛。

## Global Constraints

- 工作从 `feat/exact-ip-over-cidr` 的已验证提交 `5ca42e0` 开始，独立测试分支；不得修改 main/tag/release，也不得覆盖正在使用的 Windows 会话或真实密码库。
- 用户授权临时切换显示模式并恢复；变更前记录实际主显示设备、模式和有效 DPI；如预检失败不切换。
- 目标分别为 2560×1440、3840×2160，且有效 DPI ≥144；96 DPI/100% 只属于普通分辨率测试，不能计为高 DPI 通过。
- 只在 UIAutomation 打开的 KeePass 托管 Options 窗口上断言；四个页签 Browser Integration、Matching Rules、Database & Security、Associations 与 Save/Cancel 可见且在工作区内；不点击 Save、Install/Repair 或关联删除。
- 前一只读探针 `36601787312` 枚举两种模式，但实际 96 DPI；当前 Runner `offline`，上线且交互会话就绪前不得切换显示。
- Runner 所有插件、HKCU 注册和合成库状态仍由现有 E2E 备份/恢复脚本管理；严禁输出数据库、协议和凭据。

---

### Task 1: 显示模式预检和恢复的可信证据

**Files:**
- Modify: `scripts/verify-options-hosted-display.ps1`（显示设备选择、模式预检/切换/恢复、JSON 分类）
- Test: `scripts/probe-display-capabilities.ps1` 的只读模式枚举输出，或新建不修改真实显示的校验脚本。

**Interfaces:** 输入 `-ProcessId`、`-TestDatabasePath`、`-TargetWidth`、`-TargetHeight`、`-MinimumDpi`、`-OutputPath`；输出 JSON 的 `original`、`actual`、`passed`、`restored`、`error`、`restoreError`。退出码非零表示实测或恢复失败，前置条件不足单列 `blocked` 且不称通过。

- [ ] **RED:** 使用只读探针已确认目标模式可枚举，写安全断言验证旧脚本 `EnumDisplaySettings($null, ...)` 在这台 Runner 无法得到正确主设备模式；模拟/只读执行，确保此断言在旧实现失败，而不是语法错误。
- [ ] **GREEN:** 使用 `[System.Windows.Forms.Screen]::PrimaryScreen.DeviceName` 显式调用 `EnumDisplaySettings` 和 `ChangeDisplaySettingsEx`；在切换前记录显示模式和 DPI，目标模式用 `CDS_TEST` 探测且检查颜色深度/方向/刷新率。切换后读取实际模式与 DPI；`finally` 无条件恢复并对原始模式和 DPI 回读。恢复失败立即非零并留下脱敏证据；缺目标模式或缩放不足输出 `blocked`，不虚标 PASS。
- [ ] **VERIFY:** PowerShell 解析/静态检查；在 Runner 在线且无人使用时先只读验证模式与 DPI，再小范围尝试单目标切换；每次查看 JSON 中的 original/actual/restored，失败即停止后续显示变更。
- [ ] **REVIEW/COMMIT:** 实现代理提交，独立代理审查 Win32 结构布局、P/Invoke 错误处理、恢复路径和证据真实性。

### Task 2: KeePass 托管 Options 真正可见性验收

**Files:**
- Modify: `scripts/verify-options-hosted-display.ps1`（菜单定位、页签逐页选择、按钮/标签与控件几何）
- Modify: `.github/workflows/e2e-windows.yml`（在 KeePass 停止前执行、上传脱敏证据、严格结果分类）

**Interfaces:** 同 Task 1 JSON；`tabs` 为每页选中并验证记录，不是仅数目。`passed` 只能在分辨率、DPI、四页内容与恢复均成功后为真。

- [x] **RED:** 写真实 UIA 检查，证明旧脚本只枚举页签但不逐页选中、只查 Save/Cancel 而未查四页控件标签及裁切；在 96 DPI 场景不得把此测试宣称通过。
- [x] **GREEN:** 由测试 KeePass PID 及唯一合成库路径绑定窗口；UIA 打开 Tools → KeePassNatMsg Options；逐页 Select/读取可见控件矩形、常驻帮助文字、底部 Save/Cancel/版本区且检查工作区和父容器裁切；只点 Cancel，失败时安全退出。
- [ ] **VERIFY:** 先完成 2K，再完成 4K；检查每档实际几何/DPI/恢复证据和 Runner 备份恢复；若 DPI 仍为 96 或 Runner 离线，记录“未验收”，不能回退成独立 GUI 检查冒充宿主实测。最终同一 SHA 的 CI/E2E 全绿后才给功能通过结论。
- [ ] **REVIEW/COMMIT:** 独立代理审查无密码泄漏和恢复可靠性；仅功能分支提交，不打 tag 或发布。

## Preflight dependency table

| 任务/接口 | 依赖/产出 | 裁决 |
|---|---|---|
| Task 1 单任务 | 只读枚举应先于显示切换 | 缩放不足或 Runner 离线则停止，不把 blocked 当 passed |
| Task 1 → Task 2：`original/actual/restored` | Task 2 只能消费真实读回的模式和 DPI | 任一未验证直接阻断 Task 2 |
| Task 2 单任务 | 四页选择/内容和 Cancel 后窗口关闭 | 页签“存在”不等于已逐页验收；恢复失败非零 |
| Task 2 ↔ 工作流 `e2e-windows.yml` | 在独立 KeePass 测试进程停止前运行 | 不更动生产 Options/匹配逻辑；Runner 状态恢复单独回读 |
