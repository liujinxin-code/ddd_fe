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
    const result = await authApi.login(payload)
    localStorage.setItem('flowengine_token', result.data.token)
    persistUser({ userId: result.data.userId, username: result.data.username })
    return result.data
  },
  async loadUser() {
    const result = await authApi.me()
    persistUser(result.data)
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
