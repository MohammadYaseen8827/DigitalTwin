// API Configuration Utility
// Handles different environments and provides fallbacks

export interface ApiConfig {
  baseUrl: string;
  timeout: number;
  version: string;
  withCredentials: boolean;
}

class ApiConfiguration {
  private config: ApiConfig;

  constructor() {
    this.config = this.loadConfiguration();
  }

  private loadConfiguration(): ApiConfig {
    // Determine environment
    const isDevelopment = import.meta.env.DEV;
    const isProduction = import.meta.env.PROD;
    
    // Base URL configuration
    let baseUrl = import.meta.env.VITE_API_BASE_URL;
    
    // Fallback logic based on environment
    if (!baseUrl) {
      if (isDevelopment) {
        // Default development URL
        baseUrl = 'http://localhost:5000/api';
      } else {
        // Production relative path
        baseUrl = '/api';
      }
    }

    // Ensure trailing slash consistency
    if (!baseUrl.endsWith('/')) {
      baseUrl = baseUrl + '/';
    }

    return {
      baseUrl,
      timeout: parseInt(import.meta.env.VITE_API_TIMEOUT || '30000'),
      version: import.meta.env.VITE_API_VERSION || '1.0',
      withCredentials: import.meta.env.VITE_API_WITH_CREDENTIALS === 'true'
    };
  }

  public get(): ApiConfig {
    return { ...this.config };
  }

  public getBaseUrl(): string {
    return this.config.baseUrl;
  }

  public getFullUrl(endpoint: string): string {
    // Remove leading slash from endpoint if present
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    return `${this.config.baseUrl}${cleanEndpoint}`;
  }

  public isDevelopment(): boolean {
    return import.meta.env.DEV === true;
  }

  public isProduction(): boolean {
    return import.meta.env.PROD === true;
  }
}

// Export singleton instance
export const apiConfig = new ApiConfiguration();

// Export for backward compatibility
export default apiConfig;