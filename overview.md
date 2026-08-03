# 提取代理余额功能实现概览

## 问题
代理管理页顶部的「提取代理余额（暂未开放）」按钮无法点击：后端没有对应接口，前端 `agentApi.withdraw` 也是 `notImplemented` 占位。

## 后端改动
1. **领域层** `4.Domain/Domain/Entities/TkUser.cs`
   - 新增 `WithdrawAgentAmountToUserAmountFunc(decimal amount)`：校验代理身份、金额 > 0、代理余额充足，将 `AgentAmount` 转到 `UserAmount` 并递增 `UserVersion`。

2. **应用层** `2.Application/Application/Events/Agent/`
   - 新增 `WithdrawAgentAmountCommand`（`AgentUserId`, `Amount`）。
   - 新增 `WithdrawAgentAmountCommandValidator`（金额 > 0）。
   - 新增 `WithdrawAgentAmountCommandHandler`：乐观并发重试加载代理、调用领域方法、写入 `ConsumeStatus.AgentWithdraw` 流水并落库。

3. **接口层** `1.Open/Open/Open/Controllers/AgentController.cs`
   - 新增 `POST /Agent/withdraw`，`AgentUserId` 由 `CurrentUser.Userid` 注入，防止越权。

## 前端改动
1. `src/api/index.js`
   - `agentApi.withdraw` 从 `notImplemented` 占位改为真实请求 `POST /Agent/withdraw`，body `{ amount }`。

2. `src/views/agent/agentIndex.vue`
   - 顶部「提取代理余额」按钮由 `disabled` 改为可点击，打开提取模态框。
   - 提取成功后同时刷新仪表盘与当前用户信息，确保余额显示及时更新。

## 验证
- 后端 `Domain` / `Application` / `Infrastructure` / `Open` 均 `dotnet build` 成功。
- 前端 `npm run build` 通过（exit 0）。

## 后续
重启 `1.Open` 服务后刷新代理管理页，即可正常使用「提取代理余额」功能。
