/**
 * Error Logging Service
 * 
 * Provides centralized error logging with support for external services like Sentry.
 * Can be configured to log to console in development and external service in production.
 */

interface ErrorContext {
  userId?: string
  machineId?: string
  simulationId?: string
  component?: string
  action?: string
  [key: string]: any
}

interface ErrorLoggerConfig {
  enabled: boolean
  environment: 'development' | 'staging' | 'production'
  dsn?: string // Sentry DSN or other service endpoint
  sampleRate?: number
  debug?: boolean
}

class ErrorLogger {
  private config: ErrorLoggerConfig
  private initialized = false

  constructor() {
    this.config = {
      enabled: import.meta.env.VITE_ERROR_LOGGING_ENABLED === 'true',
      environment: (import.meta.env.VITE_ENVIRONMENT || 'development') as any,
      dsn: import.meta.env.VITE_SENTRY_DSN,
      sampleRate: parseFloat(import.meta.env.VITE_ERROR_SAMPLE_RATE || '1.0'),
      debug: import.meta.env.DEV
    }
  }

  /**
   * Initialize error logging service
   * In a real implementation, this would initialize Sentry or another service
   */
  init() {
    if (this.initialized || !this.config.enabled) {
      return
    }

    // In production, you would initialize Sentry here:
    // import * as Sentry from '@sentry/vue'
    // Sentry.init({
    //   app,
    //   dsn: this.config.dsn,
    //   environment: this.config.environment,
    //   sampleRate: this.config.sampleRate,
    //   integrations: [
    //     new Sentry.BrowserTracing(),
    //     new Sentry.Replay()
    //   ],
    //   tracesSampleRate: 1.0,
    //   replaysSessionSampleRate: 0.1,
    //   replaysOnErrorSampleRate: 1.0
    // })

    this.initialized = true
    
    if (this.config.debug) {
      console.log('[ErrorLogger] Initialized', this.config)
    }
  }

  /**
   * Log an error
   */
  logError(error: Error, context?: ErrorContext) {
    if (!this.config.enabled) {
      return
    }

    // Console logging for development
    if (this.config.debug) {
      console.error('[ErrorLogger]', error, context)
    }

    // In production, send to Sentry:
    // Sentry.captureException(error, {
    //   contexts: { custom: context },
    //   tags: {
    //     component: context?.component,
    //     action: context?.action
    //   }
    // })

    // Store error locally for debugging
    this.storeErrorLocally(error, context)
  }

  /**
   * Log a message (non-error)
   */
  logMessage(message: string, level: 'info' | 'warning' | 'debug' = 'info', context?: ErrorContext) {
    if (!this.config.enabled) {
      return
    }

    if (this.config.debug) {
      const consoleMethod = level === 'warning' ? 'warn' : level === 'debug' ? 'log' : 'info'
      console[consoleMethod]('[ErrorLogger]', message, context)
    }

    // In production, send to Sentry:
    // Sentry.captureMessage(message, {
    //   level,
    //   contexts: { custom: context }
    // })
  }

  /**
   * Set user context for error tracking
   */
  setUser(userId: string, email?: string, username?: string) {
    if (!this.config.enabled) {
      return
    }

    // In production, set Sentry user:
    // Sentry.setUser({ id: userId, email, username })

    if (this.config.debug) {
      console.log('[ErrorLogger] User set:', { userId, email, username })
    }
  }

  /**
   * Clear user context
   */
  clearUser() {
    if (!this.config.enabled) {
      return
    }

    // In production:
    // Sentry.setUser(null)

    if (this.config.debug) {
      console.log('[ErrorLogger] User cleared')
    }
  }

  /**
   * Add breadcrumb for debugging
   */
  addBreadcrumb(message: string, category: string, data?: Record<string, any>) {
    if (!this.config.enabled) {
      return
    }

    // In production:
    // Sentry.addBreadcrumb({
    //   message,
    //   category,
    //   data,
    //   level: 'info'
    // })

    if (this.config.debug) {
      console.log('[ErrorLogger] Breadcrumb:', { message, category, data })
    }
  }

  /**
   * Store error locally in sessionStorage for debugging
   */
  private storeErrorLocally(error: Error, context?: ErrorContext) {
    try {
      const errors = this.getStoredErrors()
      errors.push({
        timestamp: new Date().toISOString(),
        message: error.message,
        stack: error.stack,
        context,
        name: error.name
      })

      // Keep only last 50 errors
      const recentErrors = errors.slice(-50)
      sessionStorage.setItem('error_log', JSON.stringify(recentErrors))
    } catch (e) {
      // Ignore storage errors
      console.warn('Failed to store error locally:', e)
    }
  }

  /**
   * Get stored errors from sessionStorage
   */
  getStoredErrors(): any[] {
    try {
      const stored = sessionStorage.getItem('error_log')
      return stored ? JSON.parse(stored) : []
    } catch {
      return []
    }
  }

  /**
   * Clear stored errors
   */
  clearStoredErrors() {
    try {
      sessionStorage.removeItem('error_log')
    } catch (e) {
      console.warn('Failed to clear stored errors:', e)
    }
  }

  /**
   * Export errors for debugging
   */
  exportErrors(): string {
    const errors = this.getStoredErrors()
    return JSON.stringify(errors, null, 2)
  }
}

// Singleton instance
export const errorLogger = new ErrorLogger()

// Convenience functions
export function logError(error: Error, context?: ErrorContext) {
  errorLogger.logError(error, context)
}

export function logMessage(message: string, level: 'info' | 'warning' | 'debug' = 'info', context?: ErrorContext) {
  errorLogger.logMessage(message, level, context)
}

export function setUser(userId: string, email?: string, username?: string) {
  errorLogger.setUser(userId, email, username)
}

export function clearUser() {
  errorLogger.clearUser()
}

export function addBreadcrumb(message: string, category: string, data?: Record<string, any>) {
  errorLogger.addBreadcrumb(message, category, data)
}

// Initialize on import
errorLogger.init()
