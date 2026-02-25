import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface User {
    id: string
    email: string
    fullName: string
    userName: string
    roles: string[]
}

// Secure token manager using sessionStorage
class SecureTokenManager {
    private static readonly ACCESS_TOKEN_KEY = 'secure_access_token'
    private static readonly REFRESH_TOKEN_KEY = 'refresh_token'
    private static readonly USER_KEY = 'secure_user_data'

    static storeToken(token: string, user: User, refreshToken?: string): void {
        // Use sessionStorage instead of localStorage for better security
        sessionStorage.setItem(this.ACCESS_TOKEN_KEY, token)
        sessionStorage.setItem(this.USER_KEY, JSON.stringify(user))
        if (refreshToken) {
            sessionStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken)
        }
    }

    static getToken(): string | null {
        return sessionStorage.getItem(this.ACCESS_TOKEN_KEY)
    }

    static getRefreshToken(): string | null {
        return sessionStorage.getItem(this.REFRESH_TOKEN_KEY)
    }

    static getUser(): User | null {
        const userData = sessionStorage.getItem(this.USER_KEY)
        return userData ? JSON.parse(userData) : null
    }

    static clearAll(): void {
        sessionStorage.removeItem(this.ACCESS_TOKEN_KEY)
        sessionStorage.removeItem(this.REFRESH_TOKEN_KEY)
        sessionStorage.removeItem(this.USER_KEY)
    }

    static updateAccessToken(token: string): void {
        sessionStorage.setItem(this.ACCESS_TOKEN_KEY, token)
    }

    static updateRefreshToken(refreshToken: string): void {
        sessionStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken)
    }
}

// Export SecureTokenManager for use in other files
export { SecureTokenManager }

export const useAuthStore = defineStore('auth', () => {
    // Initialize from secure storage
    const token = ref<string | null>(null)
    const refreshToken = ref<string | null>(null)
    const user = ref<User | null>(null)

    const isAuthenticated = computed(() => !!token.value && !!user.value)

    function setToken(newToken: string, userData: User, newRefreshToken?: string) {
        token.value = newToken
        user.value = userData
        if (newRefreshToken) {
            refreshToken.value = newRefreshToken
        }
        SecureTokenManager.storeToken(newToken, userData, newRefreshToken)
    }

    function updateAccessToken(newToken: string) {
        token.value = newToken
        SecureTokenManager.updateAccessToken(newToken)
    }

    function updateRefreshToken(newRefreshToken: string) {
        refreshToken.value = newRefreshToken
        SecureTokenManager.updateRefreshToken(newRefreshToken)
    }

    function clearAuth() {
        token.value = null
        refreshToken.value = null
        user.value = null
        SecureTokenManager.clearAll()
    }

    // Initialize from storage on store creation
    function initialize() {
        token.value = SecureTokenManager.getToken()
        refreshToken.value = SecureTokenManager.getRefreshToken()
        user.value = SecureTokenManager.getUser()
    }

    // Auto-initialize
    initialize()

    return {
        token,
        refreshToken,
        user,
        isAuthenticated,
        setToken,
        updateAccessToken,
        updateRefreshToken,
        clearAuth,
        initialize,
        hasRole: (role: string) => computed(() => 
            user.value?.roles?.includes(role) ?? false
        )
    }
})