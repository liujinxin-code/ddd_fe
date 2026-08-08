import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

import '@/assets/css/base.scss'

const app = createApp(App)

app.use(createPinia())
app.use(router)

console.log('MODE=', import.meta.env.MODE)
console.log('VITE_API_BASE_URL=', import.meta.env.VITE_API_BASE_URL)

app.mount('#app')
