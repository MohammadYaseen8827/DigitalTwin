/**
 * CSRF Token Service
 * Manages CSRF token fetching and storage for state-changing operations
 */

import axiosClient from '@/api/axiosClient'

export interface AntiForgeryTokenResponse {
  headerName: string
  requestToken: string
  formFieldName: string
}

class CsrfTokenManager {
  private token: string | null = null
  private headerName: string = 'X-CSRF-TOKEN'
  private formFieldName: string = '__RequestVerificationToken'
  private tokenPromise: Promise<string> | null = null
  private tokenExpiry: number = 0
  private readonly TOKEN_LIFETIME_MS = 60 * 60 * 1000 // 1 hour

  /**
   * Get CSRF token, fetching from server if needed
   */
  async getToken(): Promise<string> {
    // Return existing token if still valid
    if (this.token && Date.now() < this.tokenExpiry) {
      return this.token
    }

    // If a fetch is already in progress, wait for it
    if (this.tokenPromise) {
      return this.tokenPromise
    }

    // Fetch new token
    this.tokenPromise = this.fetchToken()
    
    try {
      const token = await this.tokenPromise
      this.tokenExpiry = Date.now() + this.TOKEN_LIFETIME_MS
      return token
    } finally {
      this.tokenPromise = null
    }
  }

  /**
   * Fetch CSRF token from server
   */
  private async fetchToken(): Promise<string> {
    try {
      const response = await axiosClient.get<AntiForgeryTokenResponse>('/AntiForgery/tokens')
      this.token = response.requestToken
      this.headerName = response.headerName || 'X-CSRF-TOKEN'
      this.formFieldName = response.formFieldName || '__RequestVerificationToken'
      return this.token
    } catch (error) {
      console.error('Failed to fetch CSRF token:', error)
      throw new Error('Failed to fetch CSRF token. Please refresh the page.')
    }
  }

  /**
   * Get the header name for CSRF token
   */
  getHeaderName(): string {
    return this.headerName
  }

  /**
   * Clear cached token (force refresh on next request)
   */
  clearToken(): void {
    this.token = null
    this.tokenExpiry = 0
    this.tokenPromise = null
  }

  /**
   * Check if token is valid and not expired
   */
  hasValidToken(): boolean {
    return this.token !== null && Date.now() < this.tokenExpiry
  }
}

// Singleton instance
export const csrfTokenManager = new CsrfTokenManager()

/**
 * Get CSRF token (convenience function)
 */
export async function getCsrfToken(): Promise<string> {
  return csrfTokenManager.getToken()
}

/**
 * Get CSRF header name
 */
export function getCsrfHeaderName(): string {
  return csrfTokenManager.getHeaderName()
}
