# FlowEngine_User 前端编码规范（团队基线）

> 适用栈：Vue 3 + Vite + `<script setup>` + ant-design-vue 4 + pinia 3 + vue-router 5 + 原生 fetch（`src/api/http.js` 封装）。
> 目的：把"合格的前端代码"从个人习惯变成团队可检查的基线，减少联调事故与返工。

---

## 1. API 层契约纪律（最高优先级）

- 所有请求必须走 `src/api/http.js` 的 `request()`，**禁止裸 `fetch` / `XMLHttpRequest`**。
- **每个 api 方法必须显式列出它发送的全部字段。** 调用方传进来的参数，函数体漏写一个就静默失效。
  - ✅ `body: { platformId, subPlatformId, pageIndex, pageSize, sorting, keyword }`
  - ❌ 只写部分字段，靠"前端传了就有"的错觉。
  - 真实事故：`configs()` 曾漏写 `keyword`，前端搜索传了词但后端收不到，失效数轮才被发现。
- 响应信封在 `http.js` 统一归一化为 `{ code, msg, data, count }`，业务层只认这套字段，不碰 `dataTotal` / `message`。
- 错误必须向上抛 `ApiError`，**禁止 `try/catch` 吞掉后返回 undefined**（空数据会让页面误判为"无结果"）。
- `401` 已在 `http.js` 统一处理（清 token + 跳 `/login?reason=expired`），业务层不要再写一遍 401 分支。

## 2. 组件结构

- 一律 `<script setup>` + Composition API。脚本分区顺序：`状态 ref/reactive` → `computed` → `methods` → `watch` → `onMounted`。
- 一个组件只做一件事；单文件 >250 行考虑拆子组件。
- `defineProps` / `defineEmits` 显式声明类型与默认值，禁止用隐式 `$attrs` 透传业务字段。

## 3. 样式

- **禁止魔法色值 / 像素**，统一用 SCSS 设计令牌（见 `homeIndex.vue` 顶部）：
  `$bg-card #fff` / `$border-base #e5e9ef` / `$text-primary #1e293b` / `$text-secondary #64748b` / `$text-muted #94a3b8` / `$color-primary #586ee1` / `$color-accent #f59e0b` / `$r-sm $r-md`。
- 覆盖 antd 组件用 `:deep()`；**portal 弹出层**（Select 下拉、Modal 内容）必须用**非 scoped** `<style>` + 专属 class（如 `.filter-select-dropdown`），否则样式够不到（真实踩坑：Select 下拉变形）。
- 设计语言与 `HomePage` 统一浅色；禁止在浅色页面里写局部深色"孤岛"。

## 4. 状态管理

- 跨路由共享状态用 **pinia store**；单页内临时 UI 状态用 `ref` / `reactive`，不要把临时状态塞进全局 store。
- `localStorage` 只放会话信息（`flowengine_token` / `flowengine_user`），**禁止存业务数据**。

## 5. 三态与健壮性

- 任何列表 / 请求必须处理 **loading / empty / error** 三态（参考 `homeIndex` 的 `Spin` + `Empty`）。
- 表单 / 下单提交：校验前置 + 布尔锁（如 `ordering`）防重复提交 + 失败 `message.error`。

## 6. 分页与搜索

- 分页参数遵循后端 `PagedQuery` 约定（`PageIndex` / `PageSize`），总数由响应的 `count` 回填驱动分页器。
- 搜索输入用 **300ms 防抖**；`keyword` 为空时不发送该字段（`params.keyword || undefined`）。

## 7. 安全红线

- 日志 / 控制台**禁止打印 token、密钥、用户敏感信息**。
- 请求只带 `Authorization: Bearer`；`userid` 一律后端从 `CurrentUser` 注入，前端不传（防 IDOR）。

## 8. 进阶（建议逐步推进）

- 新代码逐步 TypeScript 化；至少 API 层补 JSDoc 类型。
- 引入 **ESLint** 并把上述规则的子集（未使用变量、魔法数字、缺失 try 处理等）作为 PR 门槛。

---

### 附：合格代码自查清单（合并前过一遍）

- [ ] 所有请求走 `request()`，且 api 方法显式列出全部发送字段
- [ ] 响应只用 `{ code, msg, data, count }`，无 `dataTotal/message` 硬依赖
- [ ] 错误向上抛，无静默吞掉
- [ ] 列表有三态（loading/empty/error）
- [ ] 样式用设计令牌，antd 覆盖走 `:deep()`，弹出层走非 scoped
- [ ] 无魔法色值 / 像素
- [ ] 提交操作有校验 + 防重复提交锁
- [ ] 无 token / 密钥打印
- [ ] 单组件 ≤250 行（超则拆分）
