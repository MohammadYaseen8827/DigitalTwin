import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { toast } from 'vue-sonner'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.provide('toast', toast)

app.mount('#app')
