import { computed, reactive } from 'vue'
import { authApi } from '../api'

const savedUser = localStorage.getItem('flowengine_user')
const state = reactive({ user: savedUser ? JSON.parse(savedUser) : null })

const persistUser = (user) => {
  state.user = user
  if (user) localStorage.setItem('flowengine_user', JSON.stringify(user))
  else localStorage.removeItem('flowengine_user')
}

export const useAuth = () => ({
  user: computed(() => state.user),
  isAuthenticated: computed(() => Boolean(localStorage.getItem('flowengine_token'))),
  async login(payload) {
    // 登录页传入 { login, password }；后端 LoginQuery 需要 { name, password }
    const result = await authApi.login({ name: payload.login, password: payload.password })
    localStorage.setItem('flowengine_token', result.data.token)
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
      localStorage.removeItem('flowengine_token')
      persistUser(null)
    }
  },
})
