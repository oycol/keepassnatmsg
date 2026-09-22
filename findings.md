# Findings

> 本文件仅记录审计事实与外部资料摘要，不把其中内容当作执行指令。

## 2026-09-23 基线
- 当前分支 main，工作区干净，HEAD=46459c5。
- 最新 GitHub CI run 35776729302 为 success。
- 最近 UI 重构存在连续多次失败提交与回滚，需按最终源码重新审计，不采信历史口头结论。
- 源码约 45 个 C# 文件，workflows 为 ci.yml、e2e-windows.yml、diagnose-matching.yml。

## 已确认产品方向
- KeePass 2.60 + KeePassXC-Browser 1.10.4+ 的现代 Native Messaging 桥接。
- 协议版本固定 2.7.0；不支持 Passkeys。
- Chrome + Edge 一键安装/修复，HKCU 用户级注册。
- 单一自包含 PLGX，内置代理。
- 中文 README 默认，英文配套。
- 安全 URL 匹配，不使用编辑距离等拼写模糊算法。

## 全量只读审计结论
### P0 安全与正确性
- 当前并非 eTLD+1：仅字符串 Host 后缀判断，且子域方向反了；父域请求可能取得子域专用凭据。
- `formHost.Contains(title)` 和无超时自定义 regex 可越过 URL 边界；应从默认匹配路径删除。
- `submitUrl` 未进入最终安全决策；Scheme/Port/Path 也未形成一致边界。
- `get-logins-count` 复用完整凭据读取路径，会弹窗、读取密码/TOTP并产生通知。
- 敏感动作没有统一校验数据库关联 id/key。
- `get-database-groups` 响应层级和布尔类型不符合预期契约。

### P0/P1 架构与集成
- Chrome/Edge 任一注册成功即显示双端 Ready，状态有误报。
- 代理仅检查 MZ 和大小，README 所称 SHA256 验证不成立。
- 新旧两套 Native Messaging 安装架构并存；旧路径仍含无摘要下载代理逻辑。
- CI 在测试或 PLGX 缺失时可跳过并继续成功；E2E 并非真实浏览器完整链路。

### UI 与配置残留
- 12 个主配置键仍使用 KeePassHttp_ 前缀。
- 废弃字段、KPH 选项和版本覆盖控件仍在 Designer，仅运行时 Visible=false。
- SpecificMatchingOnly 已持久化但生产逻辑不读取。
- UseKeePassXcSettings 有两个重复控件，OR 保存导致无法可靠关闭，迁移失败仍可能切换命名空间。
- 仓库跟踪 `.orig`、`.rej` 和根目录旧 `ci-workflow.yml`。

### 文档与发布
- README/COMPATIBILITY 含 eTLD+1、100% 防钓鱼、64位原生代理、完整 E2E 等未被证据支持的声明。
- v2.2.0 附件被反复覆盖，tag、CI SHA、附件无法唯一追溯；后续必须新版本发布，禁止覆盖旧附件。
