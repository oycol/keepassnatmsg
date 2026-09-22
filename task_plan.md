# KeePassNatMsg 全面重构计划

## 目标
以当前仓库与真实 CI/E2E 为事实源，整理产品需求和支持边界，清除 KeePassHttp/旧实现残留，形成可维护的现代 Native Messaging 架构；更新中文默认、英文配套的 README，并完成真实构建、单测与 Windows E2E 验证。

## 不可变约束
- 协议版本固定为 2.7.0，不声明 Passkeys/WebAuthn 支持。
- 单一自包含 KeePassNatMsg.plgx，内置原生代理；Chrome 与 Edge HKCU 双轨注册。
- URL 匹配不得使用拼写模糊匹配；必须有防钓鱼边界测试。
- 不以管理员权限运行 KeePass；远端/外部写入后回读验证。
- 不保留仅靠 Visible=false 隐藏的废弃 UI 或失效配置。

## 阶段
1. [complete] 基线与历史需求审计
2. [complete] 产品需求、协议能力与限制矩阵
3. [complete] 残留代码/配置/UI/文件清理设计
4. [complete] 测试先行的架构重构
5. [complete] README 中文默认 + 英文版及限制文档
6. [in_progress] 独立代码审查、安全扫描、完整 CI/E2E
7. [pending] 发布物回读验证与最终交付

## 验收标准
- 仓库不存在已废弃配置、事件、控件、迁移入口或无引用文件。
- 配置项均有明确产品意义、默认值、持久化测试和 UI 说明。
- 匹配行为有精确主机/子域/方案/路径/多 URL/钓鱼域回归测试。
- README 列出真实支持功能、协议动作、安装流程、兼容范围、明确限制。
- 本地可执行测试与 GitHub CI 成功；Windows E2E 成功。
- Release 附件来自最终成功 CI，回读附件名称与校验和。

## 已知风险
- 历史自动生成 Designer 文件曾被 sed/patch 反复修改，可能有死控件或布局残留。
- Skill 中仍有 Levenshtein 与“零模糊匹配”相互矛盾的旧描述，需按源码与测试纠正。
- 当前所谓 eTLD+1 可能只是字符串后缀判断，不能据名称宣称使用 Public Suffix List。
- Release v2.2.0 被多次覆盖，同一版本内容不可追溯，最终应考虑新版本发布。

## 错误记录
- 暂无本阶段错误。
