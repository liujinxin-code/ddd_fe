import { request } from './http'

export const authApi = {
  login: (body) => request('/tkuser/login', { method: 'POST', body }),
  register: (body) => request('/tkuser/register', { method: 'POST', body }),
  me: () => request('/tkuser/me'),
  changePassword: (body) => request('/tkuser/change-password', { method: 'POST', body }),
  logout: () => request('/tkuser/logout', { method: 'POST' }),
  consumptions: (params) => request('/tkuser/consumptions', { params }),
}

export const homeApi = {
  platforms: () => request('/home/platforms'),
  configs: (params) => request('/home/configs', { params }),
}

export const agentApi = {
  children: (params) => request('/agent/children', { params }),
  createChild: (body) => request('/agent/children', { method: 'POST', body }),
  markups: (params) => request('/agent/markups', { params }),
  setMarkup: (body) => request('/agent/markups', { method: 'POST', body }),
  changeStatus: (userId, body) =>
    request(`/agent/children/${userId}/status`, { method: 'POST', body }),
  resetPassword: (userId) =>
    request(`/agent/children/${userId}/reset-password`, { method: 'POST' }),
  transfer: (body) => request('/agent/transfers', { method: 'POST', body }),
  withdraw: (body) => request('/agent/withdrawals', { method: 'POST', body }),
}

export const orderApi = {
  list: (params) => request('/orders', { params }),
  createBatch: (body) => request('/orders/batch', { method: 'POST', body }),
}
