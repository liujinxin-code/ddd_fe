import { createRouter, createWebHistory } from 'vue-router'

const HomePage = () => import('../views/HomePage.vue')
const homeIndex = () => import('../views/home/homeIndex.vue')
const agentIndex = () => import('../views/agent/agentIndex.vue')
const orderIndex = () => import('../views/order/orderIndex.vue')
const consumptionIndex = () => import('../views/consumption/consumptionIndex.vue')
const ticketIndex = () => import('../views/ticket/ticketIndex.vue')
const LoginPage = () => import('../views/auth/LoginPage.vue')

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'LoginPage',
      component: LoginPage,
    },
    {
      path: '/',
      redirect: '/login',
    },
    {
      path: '/app',
      name: 'HomePage',
      component: HomePage,
      children: [
        {
          path: 'homeIndex',
          name: 'homeIndex',
          component: homeIndex,
        },
        {
          path: 'agent',
          name: 'agentIndex',
          component: agentIndex,
        },
        {
          path: 'order',
          name: 'orderIndex',
          component: orderIndex,
        },
        {
          path: 'consumption',
          name: 'consumptionIndex',
          component: consumptionIndex,
        },
        {
          path: 'ticket',
          name: 'ticketIndex',
          component: ticketIndex,
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const authenticated = Boolean(localStorage.getItem('flowengine_token'))
  if (to.path !== '/login' && !authenticated) return '/login'
  if (to.path === '/login' && authenticated) return '/app/homeIndex'
  return true
})

export default router
