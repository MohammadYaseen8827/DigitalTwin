import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '@/services/api'

// Types
export interface User {
    id: string
    name: string
    email: string
    avatar?: string
    role: 'admin' | 'user' | 'viewer'
    twoFactorEnabled: boolean
    lastLogin?: string
    createdAt: string
}

export interface AuthState {
    user: User | null
    token: string | null
    refreshToken: string | null
    isAuthenticated: boolean
    isLoading: boolean
    sessionTimeout: number | null
    lastActivity: number | null
}

export interface LoginCredentials {
    email: string
    password: string
    rememberMe?: boolean
}

export interface RegisterData {
    name: string
    email: string
    password: string
    confirmPassword: string
    acceptTerms: boolean
}

export interface PasswordResetRequest {
    email: string
}

export interface PasswordResetConfirm {
    email: string
    token: string
    password: string
    confirmPassword: string
}

export interface SessionWarning {
    show: boolean
    remainingSeconds: number
}

const API_URL = import.meta.env.VITE_API_URL || '/api'

// API service already has interceptors configured
const authApi = api

export const useAuthStore = defineStore('auth', () => {
    // State
    const user = ref<User | null>(null)
    const token = ref<string | null>(localStorage.getItem('auth_token'))
    const refreshToken = ref<string | null>(localStorage.getItem('refresh_token'))
    const isLoading = ref(false)
    const error = ref<string | null>(null)
    const sessionTimeoutId = ref<number | null>(null)
    const warningTimeoutId = ref<number | null>(null)
    const lastActivity = ref<number | null>(parseInt(localStorage.getItem('last_activity') || '0'))

    // Session warning state
    const sessionWarning = ref<SessionWarning>({
        show: false,
        remainingSeconds: 0
    })

    // Session timeout configuration (in milliseconds)
    const SESSION_TIMEOUT = 30 * 60 * 1000 // 30 minutes
    const WARNING_BEFORE_TIMEOUT = 2 * 60 * 1000 // 2 minutes before
    const WARNING_INTERVAL = 1000 // Check every second

    // Computed
    const isAuthenticated = computed(() => !!token.value && !!user.value)
    const userFullName = computed(() => user.value?.name || 'User')
    const userInitials = computed(() => {
        if (!user.value?.name) return 'U'
        return user.value.name
            .split(' ')
            .map(n => n[0])
            .join('')
            .toUpperCase()
            .slice(0, 2)
    })
    const isAdmin = computed(() => user.value?.role === 'admin')
    const sessionTimeRemaining = computed(() => {
        if (!lastActivity.value) return 0
        const elapsed = Date.now() - lastActivity.value
        return Math.max(0, SESSION_TIMEOUT - elapsed)
    })

    // Actions
    async function login(credentials: LoginCredentials): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            const response = await authApi.post<{
                accessToken: string
                refreshToken: string
                user: User
            }>(`${API_URL}/auth/login`, {
                email: credentials.email,
                password: credentials.password,
                rememberMe: credentials.rememberMe
            })

            const { accessToken, refreshToken: refresh, user: userData } = response.data

            token.value = accessToken
            refreshToken.value = refresh
            user.value = userData

            // Store tokens securely
            if (credentials.rememberMe) {
                localStorage.setItem('auth_token', accessToken)
                localStorage.setItem('refresh_token', refresh)
            } else {
                sessionStorage.setItem('auth_token', accessToken)
                sessionStorage.setItem('refresh_token', refresh)
            }

            localStorage.setItem('last_activity', Date.now().toString())
            lastActivity.value = Date.now()

            // Set up axios default auth header
            // Note: The api service already has interceptors, so we need to set the token there
            localStorage.setItem('token', accessToken)
            localStorage.setItem('refreshToken', refresh)

            // Start session monitoring
            startSessionMonitoring()

            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Login failed. Please check your credentials.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function register(data: RegisterData): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/register`, {
                name: data.name,
                email: data.email,
                password: data.password,
                confirmPassword: data.confirmPassword,
                acceptTerms: data.acceptTerms
            })

            // Auto-login after registration
            return await login({ email: data.email, password: data.password })
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Registration failed. Please try again.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function requestPasswordReset(email: string): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/forgot-password`, { email })
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to send reset email.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function resetPassword(data: PasswordResetConfirm): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/reset-password`, {
                email: data.email,
                token: data.token,
                password: data.password
            })
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Password reset failed.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function checkAuth(): Promise<boolean> {
        if (!token.value) {
            // Try to get from session storage
            const sessionToken = sessionStorage.getItem('auth_token')
            if (sessionToken) {
                token.value = sessionToken
            } else {
                return false
            }
        }

        try {
            const response = await authApi.get<User>(`${API_URL}/auth/me`)
            user.value = response.data

            // Update last activity
            lastActivity.value = Date.now()
            localStorage.setItem('last_activity', Date.now().toString())

            startSessionMonitoring()
            return true
        } catch (err) {
            logout()
            return false
        }
    }

    async function refreshSession(): Promise<boolean> {
        if (!refreshToken.value) return false

        try {
            const response = await authApi.post<{
                accessToken: string
                refreshToken: string
            }>(`${API_URL}/auth/refresh`, {
                refreshToken: refreshToken.value
            })

            const { accessToken, refreshToken: newRefresh } = response.data

            token.value = accessToken
            refreshToken.value = newRefresh

            localStorage.setItem('auth_token', accessToken)
            localStorage.setItem('refresh_token', newRefresh)
            localStorage.setItem('last_activity', Date.now().toString())

            lastActivity.value = Date.now()
            localStorage.setItem('token', accessToken)
            localStorage.setItem('refreshToken', newRefresh)

            sessionWarning.value = { show: false, remainingSeconds: 0 }
            startSessionMonitoring()

            return true
        } catch (err) {
            logout()
            return false
        }
    }

    function updateLastActivity(): void {
        lastActivity.value = Date.now()
        localStorage.setItem('last_activity', Date.now().toString())
    }

    function startSessionMonitoring(): void {
        // Clear existing timeouts
        clearSessionTimeouts()

        // Check for session warning
        warningTimeoutId.value = window.setTimeout(() => {
            showSessionWarning()
        }, SESSION_TIMEOUT - WARNING_BEFORE_TIMEOUT)
    }

    function showSessionWarning(): void {
        const checkInterval = setInterval(() => {
            const remaining = sessionTimeRemaining.value

            if (remaining <= 0) {
                clearInterval(checkInterval)
                logout()
                return
            }

            sessionWarning.value = {
                show: true,
                remainingSeconds: Math.ceil(remaining / 1000)
            }
        }, WARNING_INTERVAL)

        // Store interval ID for cleanup
        sessionWarning.value['intervalId'] = checkInterval
    }

    function extendSession(): void {
        if (sessionWarning.value['intervalId']) {
            clearInterval(sessionWarning.value['intervalId'])
        }

        refreshSession()
        sessionWarning.value = { show: false, remainingSeconds: 0 }
    }

    function clearSessionTimeouts(): void {
        if (sessionTimeoutId.value) clearTimeout(sessionTimeoutId.value)
        if (warningTimeoutId.value) clearTimeout(warningTimeoutId.value)

        if (sessionWarning.value['intervalId']) {
            clearInterval(sessionWarning.value['intervalId'])
        }
    }

    async function logout(): Promise<void> {
        try {
            if (token.value) {
                await authApi.post(`${API_URL}/auth/logout`)
            }
        } catch (err) {
            // Ignore logout errors
        } finally {
            // Clear state
            user.value = null
            token.value = null
            refreshToken.value = null
            error.value = null
            lastActivity.value = null

            // Clear storage
            localStorage.removeItem('auth_token')
            localStorage.removeItem('refresh_token')
            localStorage.removeItem('last_activity')
            sessionStorage.removeItem('auth_token')
            sessionStorage.removeItem('refresh_token')

            // Clear headers
            // Note: The api service handles its own headers
            localStorage.removeItem('token')
            localStorage.removeItem('refreshToken')

            // Clear timeouts
            clearSessionTimeouts()
            sessionWarning.value = { show: false, remainingSeconds: 0 }

            // Redirect to login
            const router = useRouter()
            await router.push({ name: 'Login' })
        }
    }

    async function updateProfile(data: Partial<User>): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            const response = await authApi.put<User>(`${API_URL}/auth/profile`, data)
            user.value = response.data
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to update profile.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function changePassword(currentPassword: string, newPassword: string): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/change-password`, {
                currentPassword,
                newPassword
            })
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to change password.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function enableTwoFactor(): Promise<string | null> {
        isLoading.value = true
        error.value = null

        try {
            const response = await authApi.post<{ qrCode: string }>(`${API_URL}/auth/2fa/enable`)
            return response.data.qrCode
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to enable 2FA.'
            return null
        } finally {
            isLoading.value = false
        }
    }

    async function verifyTwoFactor(code: string): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/2fa/verify`, { code })
            if (user.value) {
                user.value.twoFactorEnabled = true
            }
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Invalid verification code.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    async function disableTwoFactor(code: string): Promise<boolean> {
        isLoading.value = true
        error.value = null

        try {
            await authApi.post(`${API_URL}/auth/2fa/disable`, { code })
            if (user.value) {
                user.value.twoFactorEnabled = false
            }
            return true
        } catch (err: any) {
            error.value = err.response?.data?.message || 'Failed to disable 2FA.'
            return false
        } finally {
            isLoading.value = false
        }
    }

    // Initialize
    function init(): void {
        // Set up activity listeners
        document.addEventListener('mousemove', updateLastActivity)
        document.addEventListener('keypress', updateLastActivity)
        document.addEventListener('click', updateLastActivity)
        document.addEventListener('scroll', updateLastActivity)

        // Check auth on init
        if (token.value) {
            checkAuth()
        }
    }

    // Cleanup
    function cleanup(): void {
        document.removeEventListener('mousemove', updateLastActivity)
        document.removeEventListener('keypress', updateLastActivity)
        document.removeEventListener('click', updateLastActivity)
        document.removeEventListener('scroll', updateLastActivity)
        clearSessionTimeouts()
    }

    return {
        // State
        user,
        token,
        refreshToken,
        isLoading,
        error,
        sessionWarning,

        // Computed
        isAuthenticated,
        userFullName,
        userInitials,
        isAdmin,
        sessionTimeRemaining,

        // Actions
        login,
        register,
        requestPasswordReset,
        resetPassword,
        checkAuth,
        refreshSession,
        extendSession,
        logout,
        updateProfile,
        changePassword,
        enableTwoFactor,
        verifyTwoFactor,
        disableTwoFactor,
        updateLastActivity,

        // Lifecycle
        init,
        cleanup
    }
})
