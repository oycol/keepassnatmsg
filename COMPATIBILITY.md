# 兼容性说明

## 运行环境

| 组件 | 支持范围 |
|---|---|
| Windows | Windows 10/11 x64 |
| KeePass | 2.35+；构建与验证基线为 2.60 |
| KeePassXC-Browser | 1.10.4（已验证版本） |
| 浏览器 | Google Chrome、Microsoft Edge |
| .NET Framework | 4.8 构建目标 |
| 协议 | 2.7.0 |

## 支持的主要功能

- 浏览器关联与关联校验
- 查询、保存和更新登录凭据
- 登录数量查询
- 密码生成
- 数据库锁定通知
- 分组查询和创建
- TOTP
- Auto-Type 请求（依赖 Windows 交互桌面）
- Chrome 和 Edge Native Messaging 一键安装/修复

## 不支持

- Passkeys / WebAuthn
- Firefox 一键安装
- 管理员权限 KeePass 与普通权限浏览器之间的连接
- 任意浏览器扩展 ID；默认仅允许官方 Chrome 和 Edge 扩展

## 安全边界

- 协议版本固定为 2.7.0，避免浏览器启用未实现的 Passkeys 功能。
- URL 匹配仅允许普通 URL 和显式 IPv4 CIDR 网段规则；旧 `Regex:` 规则不再匹配，需人工修改旧条目。
- 浏览器关联密钥存储在 KeePass 数据库 CustomData 中；Options 只显示短指纹。
- Native Messaging 配置写入当前用户 HKCU，不默认写入 HKLM。

## 验证边界

CI 验证构建、单元测试和 PLGX 打包。涉及浏览器扩展弹窗、KeePass 授权窗口和 Auto-Type 的路径仍需要交互式 Windows 桌面测试。
