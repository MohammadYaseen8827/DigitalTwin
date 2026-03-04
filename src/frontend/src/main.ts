import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { toast } from 'vue-sonner'
import * as echarts from 'echarts'
import { echartsTheme } from './assets/echarts-theme'

import App from './App.vue'
import router from './router'
import clickOutside from './directives/clickOutside'

// Register ECharts Theme
echarts.registerTheme('hub-dark', echartsTheme)

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.provide('toast', toast)
app.directive('click-outside', clickOutside)

app.mount('#app')
