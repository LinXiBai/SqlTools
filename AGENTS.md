# AGENTS.md — SqlDemo / CoreToolkit

> 本文件规范 Kimi 在本项目中的开发行为，确保代码变更可追溯、可审查。

---

## 提交规范

- **完成每个功能模块或一批相关修改后**，Kimi 必须汇总变更文件清单，主动询问用户是否提交。
- **禁止自动执行** `git commit` / `git push`，必须获得用户明确确认（如用户回复"提交"、"commit"、"push" 等）。
- **提交信息格式**：
  - `feat: 简述功能` — 新功能
  - `fix: 简述修复` — Bug 修复
  - `refactor: 简述重构` — 代码重构
  - `docs: 简述文档` — 文档/注释更新
  - `test: 简述测试` — 测试相关
  - `chore: 简述杂项` — 构建、配置、清理等
- **提交前检查项**：
  - [ ] 编译是否通过（如有项目文件变更）
  - [ ] 是否有敏感信息泄露（密码、密钥、绝对路径等）
  - [ ] `.vs/` / `bin/` / `obj/` 等忽略目录是否被误跟踪

---

## 编码风格

- 使用 **C# 命名规范**：PascalCase（类/方法/属性）、camelCase（局部变量/参数）、_camelCase（私有字段）
- 公共 API 必须添加 XML 文档注释
- 异步方法以 `Async` 结尾
- 优先使用 `var` 进行局部变量声明

---

## 项目结构约定

- `CoreToolkit/` — 核心库，保持与 UI 无关
- `CoreToolkit.Desktop/` — WPF / MVVM 辅助类
- `MotionTest.WPF/` — 运动控制测试程序
- `LicenseManager.WPF/` — 许可证管理程序
- `CoreToolkit.UnitTests/` — xUnit 单元测试

---

*最后更新：2026-04-24*
