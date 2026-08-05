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
  // 修改密码：body { oldPassword, newPassword }
  changePassword: (body) => request('/User/change-password', { method: 'POST', body }),
  // 消费流水：后端尚未提供此端点，临时禁用调用（不发送请求）
  consumptions: () => notImplemented('消费流水'),
}

export const homeApi = {
  // 平台列表：POST {} → [{ platformId, platformName }]
  platforms: () => request('/Platform/list', { method: 'POST', body: {} }),
  // 业务类型列表（二级联动）：POST { platformId } → [{ subPlatformId, subPlatformName, subPlatformNotice }]
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
      // 基础字段
      configId: c.configId,
      configName: c.configName,
      configNotice: c.configNotice,
      configTips: c.configNotice,
      // 价格字段（保留后端原始命名，供卡片模板使用）
      unitPrice: c.unitPrice,
      showPriceUnit: c.showPriceUnit,
      displayPrice: c.displayPrice,
      // 兼容弹窗使用的别名
      price: c.displayPrice,
      priceUnit: c.showPriceUnit,
      // 数量约束
      minQuantity: c.minQuantity,
      maxQuantity: c.maxQuantity,
      orderUnit: c.orderUnit,
      jsonTemplate: c.jsonTemplate,
    }))
    return { ...result, data }
  },
}

export const agentApi = {
  // 代理仪表盘：POST {} → { userAmount, agentAmount, enabledChildrenCount, totalChildrenCount }
  dashboard: () => request('/Agent/dashboard', { method: 'POST', body: {} }),
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
      createTime: c.createTime,
    }))
    return { ...result, data }
  },
  // 获取当前总体加价百分比 → { overallPercent }
  getOverallPrice: () => request('/Agent/overall-price-info', { method: 'POST' }),
  // 设置总体加价百分比：POST { overallPercent }（0-200，每代理仅一条，首次新增之后修改）
  setOverallPrice: (overallPercent) => request('/Agent/overall-price', { method: 'POST', body: { overallPercent } }),
  // 创建下级用户：POST { username, email, password }
  createChild: (body) => request('/Agent/create-children', { method: 'POST', body }),
  // 业务加价列表：POST { pageIndex, pageSize, keyword } → [{ markupId, configId, configName, configNotice, configPrice, basePrice, markupAddPrice, showPriceUnit, childDisplayPrice, createTime }]
  markups: (params) => request('/Agent/markups', {
    method: 'POST',
    body: {
      pageIndex: params.page,
      pageSize: params.pageSize,
      keyword: params.keyword || undefined,
    },
  }),
  // 新增加价可选配置列表：POST { platformId, subPlatformId, pageIndex, pageSize } → [{ configId, configName, configNotice, basePrice, showPriceUnit, minQuantity, maxQuantity, orderUnit }]
  markupConfigs: (params) => request('/Agent/markup-configs', {
    method: 'POST',
    body: {
      platformId: params.platformId,
      subPlatformId: params.subPlatformId,
      pageIndex: params.page,
      pageSize: params.pageSize,
    },
  }),
  // 设置单业务加价：POST { configId, markupAddPrice }（存在则修改，不存在则新增）
  setMarkup: (body) => request('/Agent/markup', { method: 'POST', body }),
  // 删除单业务加价：POST { configId }
  deleteMarkup: (configId) => request('/Agent/markup-delete', { method: 'POST', body: { configId } }),
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
  // 提取代理余额到用户余额：POST { amount }
  withdraw: (amount) => request('/Agent/withdraw', { method: 'POST', body: { amount } }),
}

export const orderApi = {
  // 批量下单：POST /Order/create
  // body { items: [{ configId, orderLink, quantity, comments }] }
  //  - 粉丝模板(1)：orderLink 必填 + quantity 必填
  //  - 评论模板(2)：orderLink 必填 + comments 必填（一条评论 = 一个数量，订单数量恒等于评论条数，无需传 quantity）
  //  - 账户模板(3)：orderLink 可空 + quantity = 购买账户个数（同次多个账户算一个订单）
  // 金额由服务端按用户定价计算并即时扣余额；任一明细失败则整批回滚。
  // → data { orderNos: string[], totalAmount: number }
  createBatch: (items) => request('/Order/create', { method: 'POST', body: { items } }),
  // 我的订单分页列表：POST /Order/list
  // body { orderState, keyword, pageIndex, pageSize, sorting }
  //  - orderState：0 不筛选 / 1 正在执行 / 2 已完单 / 3 部分完成 / 4 已取消（未收费）
  //  - keyword 匹配订单号或下单链接；sorting 白名单 createtime/orderamount/quantity/orderstate
  // 只返回当前登录用户自己的订单（用户id 由后端从 JWT 注入，前端不传，杜绝越权）。
  // → data [{ orderNo, orderState, orderLink, platformName, subPlatformName, configName,
  //           orderAmount, quantity, successQuantity, beginQuantity, refundAmount, createTime, jsonTemplate }]
  list: (params = {}) =>
    request('/Order/list', {
      method: 'POST',
      body: {
        orderState: params.state || 0,
        keyword: params.keyword || '',
        pageIndex: params.page || 1,
        pageSize: params.pageSize || 10,
        sorting: params.sorting || 'createtime desc',
      },
    }),
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
