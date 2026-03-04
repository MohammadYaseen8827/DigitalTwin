import axiosClient from '@/api/axiosClient'
import type { User } from '@/stores/auth.store'

// Request DTOs
export interface LoginRequest {
  email: string
  password: string
  rememberMe?: boolean
}

export interface RegisterRequest {
  email: string
  password: string
  name?: string
  fullName?: string
  confirmPassword?: string
  acceptTerms?: boolean
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface UpdateProfileRequest {
  name?: string
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  email: string
  token: string
  password: string
}

export interface TwoFactorVerifyRequest {
  code: string
}

// Response DTOs
export interface AuthResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
  user?: UserInfo
}

export interface UserInfo {
  id: string
  email: string
  name: string
}

export interface CurrentUserResponse {
  id: string
  name: string
  email: string
  role: string
  twoFactorEnabled: boolean
  lastLogin?: string | null
  createdAt: string
}

export interface RegisterResponse {
  message: string
  userId: string
  email: string
}

export interface TwoFactorSetupResponse {
  qrCode: string
  secret: string
}

export interface MessageResponse {
  message: string
}

/**
 * Register a new user account.
 */
export async function register(request: RegisterRequest): Promise<RegisterResponse> {
  const response = await axiosClient.post<RegisterResponse>('Auth/register', request)
  return response as unknown as RegisterResponse
}

/**
 * Authenticate user and get access tokens.
 */
export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await axiosClient.post<AuthResponse>('Auth/login', request)
  return response as unknown as AuthResponse
}

/**
 * Refresh access token using refresh token.
 */
export async function refreshToken(refreshTokenValue: string): Promise<AuthResponse> {
  const response = await axiosClient.post<AuthResponse>('Auth/refresh', {
    refreshToken: refreshTokenValue
  })
  return response as unknown as AuthResponse
}

/**
 * Revoke a refresh token.
 */
export async function revokeToken(refreshTokenValue: string): Promise<void> {
  await axiosClient.post('Auth/revoke', {
    refreshToken: refreshTokenValue
  })
}

/**
 * Get current authenticated user's profile.
 */
export async function getCurrentUser(): Promise<CurrentUserResponse> {
  const response = await axiosClient.get<CurrentUserResponse>('Auth/me')
  return response as unknown as CurrentUserResponse
}

/**
 * Update the current user's profile.
 */
export async function updateProfile(request: UpdateProfileRequest): Promise<CurrentUserResponse> {
  const response = await axiosClient.put<CurrentUserResponse>('Auth/profile', request)
  return response as unknown as CurrentUserResponse
}

/**
 * Change the current user's password.
 */
export async function changePassword(request: ChangePasswordRequest): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('Auth/change-password', request)
  return response as unknown as MessageResponse
}

/**
 * Initiate forgot password flow.
 */
export async function forgotPassword(request: ForgotPasswordRequest): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('Auth/forgot-password', request)
  return response as unknown as MessageResponse
}

/**
 * Reset password using reset token.
 */
export async function resetPassword(request: ResetPasswordRequest): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('Auth/reset-password', request)
  return response as unknown as MessageResponse
}

/**
 * Log out current user and revoke all tokens.
 */
export async function logout(): Promise<void> {
  await axiosClient.post('Auth/logout')
}

/**
 * Enable two-factor authentication - returns QR code for setup.
 */
export async function enableTwoFactor(): Promise<TwoFactorSetupResponse> {
  const response = await axiosClient.post<TwoFactorSetupResponse>('Auth/2fa/enable')
  return response as unknown as TwoFactorSetupResponse
}

/**
 * Verify 2FA code and complete 2FA setup.
 */
export async function verifyTwoFactor(request: TwoFactorVerifyRequest): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('Auth/2fa/verify', request)
  return response as unknown as MessageResponse
}

/**
 * Disable two-factor authentication.
 */
export async function disableTwoFactor(request: TwoFactorVerifyRequest): Promise<MessageResponse> {
  const response = await axiosClient.post<MessageResponse>('Auth/2fa/disable', request)
  return response as unknown as MessageResponse
}

// Legacy export for backward compatibility
export async function getProfile(): Promise<User> {
  const response = await getCurrentUser()
  return {
    id: response.id,
    email: response.email,
    fullName: response.name,
    userName: response.email,
    roles: [response.role]
  }
}
