/**
 * Anti-Forgery Token Service
 * Handles CSRF/XSRF token validation
 */

import axiosClient from '@/api/axiosClient'

export interface ValidateTokenRequest {
  token: string
}

export interface ValidateTokenResponse {
  valid: boolean
  message?: string
}

/**
 * Validate an anti-forgery token
 */
export async function validateToken(request: ValidateTokenRequest): Promise<ValidateTokenResponse> {
  try {
    await axiosClient.post('/AntiForgery/validate', request)
    return { valid: true }
  } catch (error: any) {
    return {
      valid: false,
      message: error?.response?.data?.message || 'Token validation failed'
    }
  }
}

// Export service object for convenience
export const antiForgeryService = {
  validateToken
}

export default antiForgeryService
