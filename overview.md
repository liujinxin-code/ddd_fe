# 修改密码接口补齐与前端对接

## 现状
- 后端 `UserController` 原先没有修改密码端点。
- 前端 `authApi.changePassword` 是 `notImplemented` 占位，`HomePage.vue` 的「修改密码」菜单项是 `disabled` 状态。
- `ChangePasswordModal` 组件已完整实现（原始密码 / 新密码 / 确认新密码，含两次一致校验）。

## 后端新增
- `IPasswordHelper.VerifyPassword(password, storedHash)` + 实现（Argon2id，按 `base64(salt+hash)` 剥离 salt 后做常时比较）。
- `TkUser.ChangePasswordFunc(newPasswordHash)` 领域方法（更新 `Password` 并递增版本号）。
- `ChangePasswordCommand`(record) + `ChangePasswordCommandHandler`：
  - 校验登录态、原密码非空、新密码 ≥ 6 位、新旧密码不同（防无意义修改）。
  - `GetByIdAsync` 取被追踪实体，验证原密码后更新并 `SaveChangesAsync`。
- `UserController` 新增 `POST /User/change-password`，`UserId` 由 `CurrentUser.Userid` 注入（防越权）。

## 前端改动
- `src/api/index.js`：`changePassword` 改为真实请求 `POST /User/change-password`，body `{ oldPassword, newPassword }`。
- `src/views/HomePage.vue`：把「修改密码」菜单项从 `disabled` 改为点击打开 `ChangePasswordModal`。

## 验证
- 后端 `Application` + `Infrastructure` `dotnet build` 成功；`Open` 因本地 VS/调试进程占用 DLL 导致复制失败（无编译错误），重启 `1.Open` 后生效。
- 前端 `npm run build` 通过（exit 0）。
