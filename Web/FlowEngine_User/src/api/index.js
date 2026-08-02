import { request } from './http'

// 后端尚未提供该接口时，调用直接抛出明确提示且不发送任何请求（用于临时禁用前端对接）。
const notImplemented = (feature) => {
  throw new Error(`「${feature}」功能暂未开放：后端尚未提供该接口`)
}

// 所有路径基于 VITE_API_BASE_URL（默认 /api），指向 1.Open 的 api/[controller]/[action]。
// 后端返回 camelCase 信封 { code, message, data, dataTotal }，已在 http.js 归一化为 { code, msg, data, count }。

export const authApi = {
  // 登录：body { name, password }；返回 { token, user: { userid, username, agentAmount } }
  login: (body) => request('/User/login', { method: 'POST', body }),
  // 注册：body { username, email, password, agentDomain }
  register: (body) => request('/User/register', { method: 'POST', body }),
  // 获取当前用户信息（GET）
  info: () => request('/User/info'),
  // 退出登录（GET）
  logout: () => request('/User/logout'),
  // 修改密码：后端尚未提供此端点，临时禁用调用（不发送请求）
  changePassword: () => notImplemented('修改密码'),
  // 消费流水：后端尚未提供此端点，临时禁用调用（不发送请求）
  consumptions: () => notImplemented('消费流水'),
}

export const homeApi = {
  // 平台列表：POST {} → [{ platformId, platformName }]
  platforms: () => request('/Platform/list', { method: 'POST', body: {} }),
  // 子平台列表（二级联动）：POST { platformId } → [{ subPlatformId, subPlatformName, subPlatformNotice }]
  subs: (platformId) => request('/Platform/subs', { method: 'POST', body: { platformId } }),
  // 业务配置列表：POST { platformId, subPlatformId, pageIndex, pageSize, sorting }
  // 需 platform + subPlatform 两级；返回字段映射为前端使用的命名。
  configs: async (params) => {
    const result = await request('/Config/list', {
      method: 'POST',
      body: {
        platformId: params.platformId,
        subPlatformId: params.subPlatformId,
        pageIndex: params.page,
        pageSize: params.pageSize,
        sorting: params.sorting,
        keyword: params.keyword || undefined,
      },
    })
    const data = (result.data || []).map((c) => ({
      configId: c.configId,
      configName: c.configName,
      configTips: c.configNotice,
      price: c.displayPrice, // 前台展示价（= showPriceUnit × unitPrice）
      priceUnit: c.showPriceUnit, // 展示单位（如 1000）
      minQuantity: c.minQuantity,
      maxQuantity: c.maxQuantity,
      orderUnit: c.orderUnit,
      jsonTemplate: c.jsonTemplate,
    }))
    return { ...result, data }
  },
}

export const agentApi = {
  // 下级用户分页：POST { pageIndex, pageSize, keyword }
  children: async (params) => {
    const result = await request('/Agent/children', {
      method: 'POST',
      body: {
        pageIndex: params.page,
        pageSize: params.pageSize,
        keyword: params.keyword || undefined,
      },
    })
    const data = (result.data || []).map((c) => ({
      userId: c.userid,
      username: c.username,
      email: c.email,
      userAmount: c.userAmount,
      enabled: c.userStatus === 1, // TkUserStatus：Enable=1 / Disable=0
      createTime: null, // 后端下级列表未返回创建时间
    }))
    return { ...result, data }
  },
  // 创建下级用户：POST { username, email, password }
  createChild: (body) => request('/Agent/create-children', { method: 'POST', body }),
  // 业务加价列表：后端尚未提供 GET 列表端点，临时禁用调用（不发送请求）
  markups: () => notImplemented('业务加价列表'),
  // 设置单业务加价：POST { configId, markupAddPrice }
  setMarkup: (body) => request('/Agent/markup', { method: 'POST', body }),
  // 修改下级状态：POST { childrenUserid, userStatus }
  changeStatus: (userId, payload) =>
    request('/Agent/update-status', {
      method: 'POST',
      body: { childrenUserid: userId, userStatus: payload.enabled ? 1 : 0 },
    }),
  // 重置下级密码：POST { childrenUserid } → { password }
  resetPassword: (userId) =>
    request('/Agent/reset-password', { method: 'POST', body: { childrenUserid: userId } }),
  // 转赠余额：POST { childrenUserid, transferAmount }（agentUserid 由后端从当前登录注入）
  transfer: (body) =>
    request('/Agent/transfer', {
      method: 'POST',
      body: { childrenUserid: body.childUserId, transferAmount: body.amount },
    }),
  // 提取代理余额：后端尚未提供此端点，临时禁用调用（不发送请求）
  withdraw: () => notImplemented('提取代理余额'),
}

export const orderApi = {
  // 订单列表 / 批量下单：后端尚未提供订单端点，临时禁用调用（不发送请求）
  list: () => notImplemented('订单列表'),
  createBatch: () => notImplemented('下单'),
}

export const noticeApi = {
  // 首页公告分页：POST { pageIndex, pageSize } → [{ noticeId, noticeContent, noticeType, createTime }]
  homepage: (params) =>
    request('/Notice/homepage', {
      method: 'POST',
      body: { pageIndex: params.page, pageSize: params.pageSize },
    }),
  // 弹窗公告：POST {} → 单条或 null
  popup: () => request('/Notice/popup', { method: 'POST', body: {} }),
}
