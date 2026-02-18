// Enhanced error handling utilities for standardized API error responses
import type { AxiosError } from 'axios'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

export interface ApiError {
  errorCode: string
  message: string
  details?: string
  timestamp: string
  requestId: string
  metadata?: Record<string, unknown>
  validationErrors?: ValidationError[]
}

export interface ValidationError {
  propertyName: string
  errorMessage: string
  attemptedValue?: unknown
}

export interface ErrorResponse {
  error: ApiError
  statusCode: number
}

/**
 * Extract API error from Axios error response
 */
export function extractApiError(error: AxiosError<ErrorResponse>): ApiError | null {
  const errorResponse = error.response?.data
  if (errorResponse?.error) {
    return errorResponse.error
  }
  return null
}

/**
 * Handle API error with user-friendly messages
 */
export function handleApiError(error: AxiosError<ErrorResponse>): Promise<never> {
  const apiError = extractApiError(error)
  const status = error.response?.status ?? 500

  if (apiError) {
    // Handle validation errors
    if (apiError.validationErrors && apiError.validationErrors.length > 0) {
      const firstError = apiError.validationErrors[0]
      toast.warning(
        `${firstError.propertyName}: ${firstError.errorMessage}`,
        'Validation Error'
      )
    }
    // Handle specific error codes
    else if (apiError.errorCode === 'RESOURCE_NOT_FOUND') {
      toast.info(apiError.message || 'Resource not found')
    }
    else if (apiError.errorCode === 'BUSINESS_RULE_VIOLATION') {
      toast.warning(apiError.message || 'Business rule violation')
    }
    else if (apiError.errorCode === 'UNAUTHORIZED') {
      toast.warning('Your session has expired. Please sign in again.')
    }
    else if (apiError.errorCode === 'FORBIDDEN') {
      toast.error('You do not have permission to perform this action.')
    }
    else if (status >= 500) {
      toast.error('Server error. Our team has been notified.')
    }
    else {
      toast.error(apiError.message || 'An error occurred')
    }
  }
  else {
    // Fallback for non-standard errors
    if (status === 401) {
      toast.warning('Your session has expired. Please sign in again.')
    }
    else if (status === 403) {
      toast.error('You do not have permission to perform this action.')
    }
    else if (status === 404) {
      toast.info('The requested resource could not be found.')
    }
    else if (status >= 500) {
      toast.error('Server error. Our team has been notified.')
    }
    else {
      toast.error('An unexpected error occurred. Please try again later.')
    }
  }

  return Promise.reject(error)
}

/**
 * Get user-friendly error message from API error
 */
export function getErrorMessage(error: ApiError | null): string {
  if (!error) {
    return 'An unexpected error occurred'
  }

  if (error.validationErrors && error.validationErrors.length > 0) {
    return error.validationErrors
      .map(e => `${e.propertyName}: ${e.errorMessage}`)
      .join(', ')
  }

  return error.message || error.details || 'An error occurred'
}

/**
 * Check if error is a validation error
 */
export function isValidationError(error: ApiError | null): boolean {
  return error?.errorCode === 'VALIDATION_ERROR' || 
         (error?.validationErrors?.length ?? 0) > 0
}

/**
 * Check if error is a not found error
 */
export function isNotFoundError(error: ApiError | null): boolean {
  return error?.errorCode === 'RESOURCE_NOT_FOUND' || 
         error?.errorCode === 'NOT_FOUND'
}
