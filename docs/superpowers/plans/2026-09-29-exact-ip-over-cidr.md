# 精确 IPv4 优先于 CIDR：实施计划

> 按 superpowers 子代理开发、先红后绿、逐任务审查执行；设计已在聊天获用户批准。

**目标：** 请求 IPv4 同时命中精确 IP URL 条目与仅 CIDR 条目时，仅回传有效精确 IP 条目；否则沿用 CIDR；`get-logins` 与 `get-logins-count` 同步。

**边界：** 同一条目有精确 IP 与 CIDR 时保留；先应用原有数据库范围、搜索权限、拒绝规则、URL 匹配与过期过滤，只有有效精确条目才压制仅 CIDR 条目；非 IPv4 请求和密码库数据不变。不得修改 release/tag/main，不要恢复 Favicon/Regex。C#5/.NET Framework 4.8。

**文件职责：** `KeePassNatMsg/Entry/EntrySearch.cs` 汇合候选、过滤与优先级；`KeePassNatMsg/Entry/UrlMatchingHelper.cs` 在必要时只增加供优先级识别的精确 IPv4 URL 判定；`KeePassNatMsg.Tests/` 增加真实 KeePass 条目/数据库匹配测试（不可用复制实现的纯函数测试代替）；`scripts/browser-cidr-e2e.js` 和 `.github/workflows/e2e-windows.yml` 在独立测试库加精确条目与 CIDR 条目并核对实际扩展填入，仅在端到端覆盖需要时改。

## Task 1：失败测试与最小实现
- [ ] 阅读 `EntrySearch.FindMatchingEntries` / `GetLoginsHandler` / `CountMatchingEntries` / URL 辅助与已有 NUnit 装配方式，确定可从 KeePass 测试宿主调用的真实路径；必要时只读研究测试运行器依赖。
- [ ] 建立精确 IP + CIDR 同时命中的真实条目测试；在旧代码 Windows CI 中观察预期失败（不是编译错误/伪造数据）。
- [ ] 实现精确优先：将每个已过滤条目分为“至少一个精确 IPv4 URL 与当前请求匹配”或“仅 CIDR 等其他规则匹配”；若前一类非空且请求为规范 IPv4，只排除仅 CIDR 命中的条目；避免误删普通 URL 条目。沿用现有 scheme、搜索权限和拒绝策略。
- [ ] 扩展回归：多精确账号、同条目混合规则、仅 CIDR、被拒绝或过期的精确条目不能压制 CIDR、多数据库，以及计数/响应一致；逐项红绿，完整 NUnit。
- [ ] 提交可单独审查的测试与实现；输出 CI 运行 ID/SHA/真实结果。

## Task 2：浏览器 E2E 与文档
- [ ] 在现有唯一合成数据库与官方 1.10.4 扩展的 E2E 上验证：仅 CIDR 时成功填入；同 IP 新增精确条目时，扩展只收到/只使用精确条目（不能只靠文本日志宣称）；不访问真实密码库；清理并回读 Runner 状态。
- [ ] 仅更新说明中与优先级相关的段落；保留现有 URL 语义与 release 文案。
- [ ] 提交并运行同一 SHA 的完整 Windows CI、真实浏览器 E2E，下载并核验 PLGX/SHA256SUMS。失败即定位并修复后重跑。

## 审查与交付
- [ ] 每任务由独立审查子代理看需求覆盖与代码质量；最终聚焦审查完整分支。对异常结果不得报告为通过。
- [ ] 汇报精确验收范围、运行链接、制品哈希与局限。未经明确授权不合并 main、不打 tag、不发布。
