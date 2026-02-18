import axios, { type AxiosInstance, type AxiosRequestConfig, type AxiosResponse, type InternalAxiosRequestConfig } from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'
const TOKEN_KEY = 'token'
const REFRESH_TOKEN_KEY = 'refreshToken'

const apiClient: AxiosInstance = axios.create({
    baseURL: API_BASE_URL,
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json'
    }
})

apiClient.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
        const token = localStorage.getItem(TOKEN_KEY)
        if (token && config.headers) {
            config.headers.Authorization = `Bearer ${token}`
        }
        return config
    },
    (error) => {
        return Promise.reject(error)
    }
)

apiClient.interceptors.response.use(
    (response: AxiosResponse) => {
        return response
    },
    async (error) => {
        const originalRequest = error.config

        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true

            try {
                const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
                if (refreshToken) {
                    const response = await axios.post<{ accessToken: string; refreshToken: string }>(
                        `${API_BASE_URL}/auth/refresh`,
                        { refreshToken }
                    )

                    const accessToken = response.data.accessToken
                    const newRefreshToken = response.data.refreshToken

                    localStorage.setItem(TOKEN_KEY, accessToken)
                    localStorage.setItem(REFRESH_TOKEN_KEY, newRefreshToken)

                    originalRequest.headers.Authorization = `Bearer ${accessToken}`
                    return apiClient(originalRequest)
                }
            } catch (refreshError) {
                localStorage.removeItem(TOKEN_KEY)
                localStorage.removeItem(REFRESH_TOKEN_KEY)
                window.location.href = '/login'
                return Promise.reject(refreshError)
            }
        }

        const message = error.response?.data?.message ||
            error.response?.data?.error ||
            error.message ||
            'An unexpected error occurred'

        error.userMessage = message
        return Promise.reject(error)
    }
)

export const api = {
    get<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        return apiClient.get<T>(url, config)
    },

    post<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        return apiClient.post<T>(url, data, config)
    },

    put<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        return apiClient.put<T>(url, data, config)
    },

    patch<T>(url: string, data?: unknown, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        return apiClient.patch<T>(url, data, config)
    },

    delete<T>(url: string, config?: AxiosRequestConfig): Promise<AxiosResponse<T>> {
        return apiClient.delete<T>(url, config)
    }
}

export default apiClient
