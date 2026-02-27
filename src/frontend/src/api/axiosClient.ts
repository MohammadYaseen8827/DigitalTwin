// Shared axios instance with interceptors to centralize auth headers, loading state, and response handling.
import axios, {
  AxiosHeaders,
  type AxiosError,
  type AxiosResponse,
  type InternalAxiosRequestConfig
} from 'axios'
import { handleApiError, handleApiResponse } from '@/utils/responseHandler'
import type { ProblemDetails } from '@/utils/responseHandler'
import { logError, addBreadcrumb } from '@/services/errorLogger.service'
import { apiConfig } from '@/utils/apiConfig'
import { csrfTokenManager, getCsrfHeaderName } from '@/services/csrf.service'
import { useAuthStore } from '@/stores/auth.store'

// Need to access SecureTokenManager methods - import the class if needed
// For now, using sessionStorage directly for refresh token
const getRefreshToken = () => sessionStorage.getItem('refresh_token')

/**
 * Lightweight event hub so UIs can listen for global API loading states.
 */
export const apiEvents = new EventTarget()

const pendingRequestIds = new Set<string>()

function emitLoadingEvent(): void {
  apiEvents.dispatchEvent(
    new CustomEvent('loading', {
      detail: {
        active: pendingRequestIds.size > 0,
        pending: pendingRequestIds.size
      }
    })
  )
}

declare module 'axios' {
  interface InternalAxiosRequestConfig<D = any> {
    /**
     * Internal metadata injected by the axios client for bookkeeping.
     */
    metadata?: {
      requestId?: string
    }
  }
}

const axiosClient = axios.create({
  baseURL: apiConfig.getBaseUrl(),
  timeout: apiConfig.get().timeout,
  withCredentials: apiConfig.get().withCredentials,
  params: {
    'api-version': apiConfig.get().version
  }
})

axiosClient.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  // Add authentication token
  const authStore = useAuthStore()
  const token = authStore.token || sessionStorage.getItem('secure_access_token')
  if (token) {
    const headers = AxiosHeaders.from(config.headers)
    headers.set('Authorization', `Bearer ${token}`)
    config.headers = headers
  }

  // Add CSRF token for state-changing operations (POST, PUT, DELETE, PATCH)
  const method = config.method?.toUpperCase()
  if (method && ['POST', 'PUT', 'DELETE', 'PATCH'].includes(method)) {
    try {
      const csrfToken = await csrfTokenManager.getToken()
      const headers = AxiosHeaders.from(config.headers)
      headers.set(getCsrfHeaderName(), csrfToken)
      config.headers = headers
    } catch (error) {
      logError(error as Error, {
        component: 'axiosClient',
        action: 'get_csrf_token',
        method
      })
      // Continue without CSRF token - backend will reject if required
    }
  }

  const requestId = `${method ?? 'get'}::${config.url ?? 'unknown'}::${Date.now()}`
  config.metadata = {
    ...(config.metadata ?? {}),
    requestId
  }

  pendingRequestIds.add(requestId)
  emitLoadingEvent()

  // Log API request as breadcrumb
  addBreadcrumb(
    `API Request: ${method} ${config.url}`,
    'http',
    { method: config.method, url: config.url }
  )

  return config
})

// Track if we're currently refreshing to avoid infinite loops
let isRefreshing = false
let failedQueue: Array<{
  resolve: (value: any) => void
  reject: (error: any) => void
}> = []

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve(token)
    }
  })

  failedQueue = []
}

axiosClient.interceptors.response.use(
  (response: AxiosResponse) => {
    const requestId = response.config.metadata?.requestId
    if (requestId) {
      pendingRequestIds.delete(requestId)
      emitLoadingEvent()
    }

    return handleApiResponse(response)
  },
  async (error: AxiosError<ProblemDetails>) => {
    const requestId = error.config?.metadata?.requestId
    if (requestId) {
      pendingRequestIds.delete(requestId)
      emitLoadingEvent()
    }

    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // Handle 401 Unauthorized - attempt token refresh
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // If already refreshing, queue this request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then(token => {
            if (originalRequest.headers) {
              originalRequest.headers['Authorization'] = `Bearer ${token}`
            }
            return axiosClient(originalRequest)
          })
          .catch(err => {
            return Promise.reject(err)
          })
      }

      originalRequest._retry = true
      isRefreshing = true

      const authStore = useAuthStore()
      const refreshTokenValue = authStore.refreshToken || getRefreshToken()

      if (!refreshTokenValue) {
        // No refresh token available, logout user
        authStore.clearAuth()
        processQueue(error, null)
        isRefreshing = false
        window.location.href = '/login'
        return Promise.reject(error)
      }

      try {
        // Attempt to refresh token - using centralized URL config
        const response = await axios.post<{ AccessToken: string; RefreshToken?: string }>(
          apiConfig.getFullUrl('Token/refresh'),
          { refreshToken: refreshTokenValue }
        )

        const { AccessToken, RefreshToken: newRefreshToken } = response.data

        // Update tokens in store
        authStore.updateAccessToken(AccessToken)

        if (newRefreshToken) {
          authStore.updateRefreshToken(newRefreshToken)
        }

        // Update authorization header
        if (originalRequest.headers) {
          originalRequest.headers['Authorization'] = `Bearer ${AccessToken}`
        }

        // Process queued requests
        processQueue(null, AccessToken)
        isRefreshing = false

        // Retry original request
        return axiosClient(originalRequest)
      } catch (refreshError) {
        // Refresh failed, logout user
        processQueue(refreshError, null)
        isRefreshing = false
        authStore.clearAuth()
        sessionStorage.removeItem('refresh_token')
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }

    // Log API error
    logError(error as Error, {
      component: 'axiosClient',
      action: 'api_request',
      url: error.config?.url,
      method: error.config?.method,
      status: error.response?.status,
      statusText: error.response?.statusText
    })

    return handleApiError(error)
  }
)

export default axiosClient
