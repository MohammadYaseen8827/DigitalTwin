import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
    {
        path: '/',
        redirect: '/dashboard'
    },
    {
        path: '/login',
        name: 'Login',
        component: () => import('@/views/Login.vue'),
        meta: { requiresAuth: false, layout: 'auth' }
    },
    {
        path: '/register',
        name: 'Register',
        component: () => import('@/views/Login.vue'),
        meta: { requiresAuth: false, layout: 'auth' }
    },
    {
        path: '/forgot-password',
        name: 'ForgotPassword',
        component: () => import('@/views/Login.vue'),
        meta: { requiresAuth: false, layout: 'auth' }
    },
    {
        path: '/reset-password',
        name: 'PasswordReset',
        component: () => import('@/views/Login.vue'),
        meta: { requiresAuth: false, layout: 'auth' }
    },
    {
        path: '/dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { requiresAuth: true, title: 'Dashboard' }
    },
    {
        path: '/machines',
        name: 'Machines',
        component: () => import('@/views/Machines.vue'),
        meta: { requiresAuth: true, title: 'Machines' }
    },
    {
        path: '/machines/:id',
        name: 'MachineDetail',
        component: () => import('@/views/MachineDetail.vue'),
        meta: { requiresAuth: true, title: 'Machine Details' }
    },
    {
        path: '/predictions',
        name: 'Predictions',
        component: () => import('@/views/Predictions.vue'),
        meta: { requiresAuth: true, title: 'Predictions' }
    },
    {
        path: '/alerts',
        name: 'Alerts',
        component: () => import('@/views/Alerts.vue'),
        meta: { requiresAuth: true, title: 'Alerts' }
    },
    {
        path: '/settings',
        name: 'Settings',
        component: () => import('@/views/Settings.vue'),
        meta: { requiresAuth: true, title: 'Settings' }
    },
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/NotFound.vue'),
        meta: { title: 'Page Not Found' }
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) return savedPosition
        return { top: 0 }
    }
})

router.beforeEach(async (to, from, next) => {
    // Dynamic import to avoid circular dependency
    const { useAuthStore } = await import('@/stores/auth')
    const authStore = useAuthStore()

    const requiresAuth = to.meta.requiresAuth !== false

    if (requiresAuth && !authStore.isAuthenticated) {
        await authStore.checkAuth()
        if (!authStore.isAuthenticated) {
            next({ name: 'Login', query: { redirect: to.fullPath } })
            return
        }
    }

    if (to.name === 'Login' && authStore.isAuthenticated) {
        next({ name: 'Dashboard' })
        return
    }

    const title = to.meta.title as string
    document.title = title ? `${title} | Digital Twin Platform` : 'Digital Twin Platform'

    next()
})

export default router
