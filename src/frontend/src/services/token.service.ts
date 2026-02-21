import axiosClient from '@/api/axiosClient'

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export interface RevokeTokenRequest {
  refreshToken?: string
}

export interface MessageResponse {
  message: string
}

/**
 * Refresh access token using refresh token.
 */
export async function refreshToken(request: RefreshTokenRequest): Promise<TokenResponse> {
  const response = await axiosClient.post<TokenResponse>('/Token/refresh', request)
  return response as unknown as TokenResponse
}

/**
 * Revoke refresh token (logout from server).
 */
export async function revokeToken(request: RevokeTokenRequest = {}): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('/Token/revoke', request)
  return response as unknown as MessageResponse
}

// Export service object for convenience
export const tokenService = {
  refreshToken,
  revokeToken
}

export default tokenService
