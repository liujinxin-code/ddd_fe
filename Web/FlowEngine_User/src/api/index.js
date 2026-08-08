import { request, uploadFiles } from './http'

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
  // 消费流水（余额变动）分页列表：POST /ConsumeLog/list
  // body { consumeStatus, keyword, pageIndex, pageSize, sorting }
  //  - consumeStatus：-1 不筛选 / 0 订单消费 / 1 充值 / 2 代理提现(个人余额增加) /
  //    3 转赠支出 / 4 转赠收入 / 5 订单退款 / 6 代理提现(代理收益余额减少)
  //  - keyword 匹配流水号；sorting 白名单 createtime/beforeamount/afteramount/consumestatus
  // 只返回当前登录用户自己的流水（用户id 由后端从 JWT 注入，前端不传，杜绝越权）。
  // → data [{ consumeId, consumeNo, consumeStatus, beforeAmount, afterAmount, changeAmount, createTime }]
  consumptions: (params = {}) =>
    request('/ConsumeLog/list', {
      method: 'POST',
      body: {
        consumeStatus: params.status == null ? -1 : Number(params.status),
        keyword: params.keyword || '',
        pageIndex: params.page || 1,
        pageSize: params.pageSize || 10,
        sorting: params.sorting || 'createtime desc',
      },
    }),
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
  // 下级用户分页：POST { pageIndex, pageSize, keyword, userStatus }
  //  - userStatus：undefined=全部 / 1=启用 / 0=停用（对应 TkUserStatus 枚举；前端用 childQuery.enabled 映射）
  children: async (params) => {
    const result = await request('/Agent/children', {
      method: 'POST',
      body: {
        pageIndex: params.page,
        pageSize: params.pageSize,
        keyword: params.keyword || undefined,
        userStatus: params.enabled === undefined ? undefined : (params.enabled ? 1 : 0),
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

export const serviceImageApi = {
  // 获取当前用户应展示的客服微信图片 → { imageUrl, agentUserid }
  myWechat: () => request('/ServiceImage/my-wechat', { method: 'POST' }),
  // 获取当前用户自己上传的客服微信图片（代理后台预览）
  myOwn: () => request('/ServiceImage/my-own', { method: 'POST' }),
  // 代理上传/更换自己的客服微信图片：POST { imageUrl }
  upload: (imageUrl) => request('/ServiceImage/upload', { method: 'POST', body: { imageUrl } }),
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

export const enumApi = {
  // 枚举同步：消费流水类型 → [{ value, name, label }]
  consumeStatus: () => request('/Enum/consume-status', { method: 'POST', body: {} }),
  // 枚举同步：订单状态 → [{ value, name, label }]
  orderState: () => request('/Enum/order-state', { method: 'POST', body: {} }),
  // 枚举同步：工单状态 → [{ value, name, label }]
  ticketStatus: () => request('/Enum/ticket-status', { method: 'POST', body: {} }),
  // 枚举同步：工单问题类型 → [{ value, name, label }]
  ticketType: () => request('/Enum/ticket-type', { method: 'POST', body: {} }),
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

// 静态资源根地址（如 http://localhost:9080），用于拼接后端返回的图片相对路径 /images/...
const ASSET_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '/api')
  .replace(/\/$/, '')
  .replace(/\/api$/, '')

export const fileApi = {
  // 通用文件上传（multipart，单文件≤5MB、数量上限可配置，不限制类型）：POST /File/upload
  // → data 为可直接访问的完整 http(s) URL 数组（前缀来自后端 appsettings FileSettings:BaseUrl）
  upload: (files) => uploadFiles('/File/upload', files),
}

export const ticketApi = {
  // 工单列表（仅当前登录用户本人）：POST /Ticket/list
  // body { ticketStatus, ticketType, keyword, pageIndex, pageSize, sorting }
  //  - ticketStatus：-1 不筛选 / 0 待处理 / 1 已处理
  //  - ticketType：-1 不筛选 / 0 订单问题 / 1 下单问题 / 2 网站问题 / 3 网站建议
  //  - keyword 匹配工单内容；sorting 白名单 createtime/ticketstatus/tickettype
  // → data [{ ticketId, ticketContent, ticketImages:[url...], ticketResult, ticketStatus, ticketType, userid, createTime }]
  list: (params = {}) =>
    request('/Ticket/list', {
      method: 'POST',
      body: {
        ticketStatus: params.status == null ? -1 : Number(params.status),
        ticketType: params.type == null ? -1 : Number(params.type),
        keyword: params.keyword || '',
        pageIndex: params.page || 1,
        pageSize: params.pageSize || 10,
        sorting: params.sorting || 'createtime desc',
      },
    }),
  // 提交工单：POST /Ticket/create
  // body { ticketContent, ticketType, ticketImages:[url...] }
  // → data 为新工单 id
  create: (body) => request('/Ticket/create', { method: 'POST', body }),
  // 上传工单图片：复用通用文件上传 /File/upload（见 fileApi.upload）
  // → data 为图片完整 http(s) URL 数组（单文件≤5MB、数量上限可配置，不限制类型）
  upload: (files) => uploadFiles('/File/upload', files),
}

// 将后端返回的图片相对路径拼接为可访问的完整地址
export const resolveAssetUrl = (path) => {
  if (!path) return ''
  if (/^https?:\/\//.test(path)) return path
  return `${ASSET_BASE_URL}${path}`
}
