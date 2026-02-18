// Centralized API response normalization with Magic MCP toasts for consistent UX messaging.
import type { AxiosError, AxiosResponse } from 'axios'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
  message?: string
}

type ResponseData<T> = T & { message?: string; success?: boolean }

export function handleApiResponse<T = unknown>(response: AxiosResponse<ResponseData<T>>): T {
  const { status, data } = response
  const normalizedData = (data ?? {}) as ResponseData<T>

  if (status >= 200 && status < 300) {
    const successMessage = normalizedData.message
    if (successMessage) {
      toast.success(successMessage)
    }

    return normalizedData
  }

  // Non-success status codes still funnel through here to bubble up errors
  return normalizedData
}

export function handleApiError(error: AxiosError<ProblemDetails>): Promise<never> {
  const status = error.response?.status
  const data = error.response?.data

  if (status === 400 && data?.errors) {
    const firstError = Object.values(data.errors)[0]
    const message = Array.isArray(firstError) ? firstError[0] : firstError
    toast.warning(message ?? 'Validation failed. Please review your input.')
  } else if (status === 401) {
    toast.warning('Your session has expired. Please sign in again.')
  } else if (status === 403) {
    toast.error('You do not have permission to perform this action.')
  } else if (status === 404) {
    toast.info(data?.message ?? 'The requested resource could not be found.')
  } else if (status && status >= 500) {
    toast.error('Server error. Our team has been notified.')
  } else if (data?.message ?? data?.detail ?? data?.title) {
    toast.error(data.message ?? data.detail ?? data.title ?? 'Request failed')
  } else {
    toast.error('Unexpected error occurred. Please try again later.')
  }

  return Promise.reject(error)
}
