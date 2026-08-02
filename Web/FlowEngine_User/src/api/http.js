const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')

export class ApiError extends Error {
  constructor(message, code) {
    super(message)
    this.name = 'ApiError'
    this.code = code
  }
}

const buildUrl = (path, params) => {
  const url = new URL(`${API_BASE_URL}${path}`, window.location.origin)
  Object.entries(params || {}).forEach(([key, value]) => {
    if (value === undefined || value === null || value === '') return
    const values = Array.isArray(value) ? value : [value]
    values.forEach((item) => url.searchParams.append(key, item))
  })
  return API_BASE_URL.startsWith('http') ? url.toString() : `${url.pathname}${url.search}`
}

export async function request(path, options = {}) {
  const token = localStorage.getItem('flowengine_token')
  const response = await fetch(buildUrl(path, options.params), {
    method: options.method || 'GET',
    headers: {
      ...(options.body ? { 'Content-Type': 'application/json' } : {}),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
    body: options.body ? JSON.stringify(options.body) : undefined,
  })

  // ── 401 优先处理：JWT 中间件返回的可能是纯文本而非 JSON ──
  if (response.status === 401) {
    localStorage.removeItem('flowengine_token')
    localStorage.removeItem('flowengine_user')
    if (window.location.pathname !== '/login') {
      window.location.assign('/login?reason=expired')
    }
    throw new ApiError('登录已失效，请重新登录。', 401)
  }

  let result
  try {
    result = await response.json()
  } catch {
    throw new ApiError('服务器响应格式不正确。', response.status)
  }

  // 归一化后端 ApiResult 信封：后端返回 camelCase { code, message, data, dataTotal }，
  // 前端历史约定读 { code, msg, data, count }。在此桥接，避免改动各视图。
  if (result && typeof result === 'object') {
    if (result.dataTotal !== undefined) result.count = result.dataTotal
    if (result.message !== undefined) result.msg = result.message
  }

  if (!response.ok || result.code !== 200) {
    if (result.code === 401) {
      localStorage.removeItem('flowengine_token')
      localStorage.removeItem('flowengine_user')
      if (window.location.pathname !== '/login') window.location.assign('/login')
    }
    throw new ApiError(result.msg || '请求失败，请稍后重试。', result.code || response.status)
  }
  return result
}
