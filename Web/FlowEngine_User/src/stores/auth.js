import { computed, reactive } from 'vue'
import { authApi } from '../api'

const savedToken = localStorage.getItem('flowengine_token') || ''
const savedUser = localStorage.getItem('flowengine_user')
const state = reactive({
  token: savedToken,
  user: savedUser ? JSON.parse(savedUser) : null,
})

const persistUser = (user) => {
  state.user = user
  if (user) localStorage.setItem('flowengine_user', JSON.stringify(user))
  else localStorage.removeItem('flowengine_user')
}

// token 走响应式 state，登录/登出时实时联动 UI。
// 注意：localStorage.getItem 本身不是 Vue 响应式源，若 isAuthenticated 直读 localStorage，
// 登录后 computed 不会重新求值，导致全局「联系微信」按钮首次登录不显示（需刷新才出现）。
const persistToken = (token) => {
  state.token = token || ''
  if (token) localStorage.setItem('flowengine_token', token)
  else localStorage.removeItem('flowengine_token')
}

export const useAuth = () => ({
  user: computed(() => state.user),
  isAuthenticated: computed(() => Boolean(state.token)),
  async login(payload) {
    // 登录页传入 { login, password }；后端 LoginQuery 需要 { name, password }
    const result = await authApi.login({ name: payload.login, password: payload.password })
    persistToken(result.data.token)
    const u = result.data.user
    persistUser({ userId: u.userid, username: u.username })
    return result.data
  },
  async loadUser() {
    const result = await authApi.info()
    const u = result.data
    persistUser({
      userId: u.userid,
      username: u.username,
      isAgent: u.isAgent === 1,
      agentUserId: u.agentUserid || 0,
      userAmount: u.userAmount,
      agentAmount: u.agentAmount,
    })
    return result.data
  },
  async logout() {
    try {
      await authApi.logout()
    } finally {
      persistToken('')
      persistUser(null)
    }
  },
})
