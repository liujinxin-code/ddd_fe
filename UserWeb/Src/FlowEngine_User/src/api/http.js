const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')

export class ApiError extends Error {
  constructor(message, code) {
    super(message)
    this.name = 'ApiError'
    this.code = code
  }
}

/**
 * 上传多个文件（multipart/form-data），与 request 共享相同的令牌注入、401 处理与信封归一化逻辑。
 * @param {string} path 接口路径，如 '/Ticket/upload'
 * @param {File[]} files 文件列表，表单字段名为 files
 * @returns {Promise<object>} 归一化后的后端信封 { code, msg, data, count }
 */
export async function uploadFiles(path, files) {
  const token = localStorage.getItem('flowengine_token')
  const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || '/api').replace(/\/$/, '')
  const form = new FormData()
  files.forEach((f) => form.append('files', f))

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: form,
  })

  if (response.status === 401) {
    localStorage.removeItem('flowengine_token')
    localStorage.removeItem('flowengine_user')
    if (window.location.pathname !== '/login') {
      window.location.assign('/login?reason=expired')
    }
    throw new ApiError('登录已失效，请重新登录。', 401)
  }

  if (response.status === 403) {
    localStorage.removeItem('flowengine_token')
    localStorage.removeItem('flowengine_user')
    if (window.location.pathname !== '/login') {
      window.location.assign('/login?reason=forbidden')
    }
    throw new ApiError('权限不足，请重新登录。', 403)
  }

  let result
  try {
    result = await response.json()
  } catch {
    throw new ApiError('服务器响应格式不正确。', response.status)
  }

  if (result && typeof result === 'object') {
    if (result.dataTotal !== undefined) result.count = result.dataTotal
    if (result.message !== undefined) result.msg = result.message
  }

  if (!response.ok || result.code !== 200) {
    if (result.code === 401 || result.code === 403) {
      localStorage.removeItem('flowengine_token')
      localStorage.removeItem('flowengine_user')
      if (window.location.pathname !== '/login') {
        window.location.assign(result.code === 403 ? '/login?reason=forbidden' : '/login?reason=expired')
      }
    }
    throw new ApiError(
      result.msg || (result.code === 403 ? '权限不足，请重新登录。' : '上传失败，请稍后重试。'),
      result.code || response.status,
    )
  }
  return result
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

  // ── 403 处理：已登录但权限不足 ──
  if (response.status === 403) {
    localStorage.removeItem('flowengine_token')
    localStorage.removeItem('flowengine_user')
    if (window.location.pathname !== '/login') {
      window.location.assign('/login?reason=forbidden')
    }
    throw new ApiError('权限不足，请重新登录。', 403)
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
    if (result.code === 401 || result.code === 403) {
      localStorage.removeItem('flowengine_token')
      localStorage.removeItem('flowengine_user')
      if (window.location.pathname !== '/login') {
        window.location.assign(result.code === 403 ? '/login?reason=forbidden' : '/login?reason=expired')
      }
    }
    throw new ApiError(
      result.msg || (result.code === 403 ? '权限不足，请重新登录。' : '请求失败，请稍后重试。'),
      result.code || response.status,
    )
  }
  return result
}
