import type { AxiosError } from 'axios'

/**
 * Error severity levels
 */
export enum ErrorSeverity {
  INFO = 'info',
  WARNING = 'warning',
  ERROR = 'error',
  CRITICAL = 'critical'
}

/**
 * Error report structure
 */
export interface ErrorReport {
  message: string
  error?: Error | unknown
  severity: ErrorSeverity
  context?: Record<string, unknown>
  timestamp: Date
  source?: string
}

/**
 * Global error reporter configuration
 */
class ErrorReporter {
  private isDevelopment: boolean
  private apiEndpoint?: string

  constructor() {
    this.isDevelopment = import.meta.env.DEV === true
    this.apiEndpoint = import.meta.env.VITE_ERROR_REPORTING_ENDPOINT
  }

  /**
   * Report an error to the appropriate destination
   */
  report(report: ErrorReport): void {
    // Always log to console in development
    if (this.isDevelopment) {
      this.logToConsole(report)
      return
    }

    // In production, send to monitoring service
    this.sendToMonitoringService(report)
  }

  /**
   * Log error to console. In production, only log a generic message to avoid leaking context.
   */
  private logToConsole(report: ErrorReport): void {
    if (this.isDevelopment) {
      const prefix = `[${report.severity.toUpperCase()}]${report.source ? ` [${report.source}]` : ''}`
      switch (report.severity) {
        case ErrorSeverity.INFO:
          console.log(prefix, report.message, report.context || '')
          break
        case ErrorSeverity.WARNING:
          console.warn(prefix, report.message, report.error || '', report.context || '')
          break
        case ErrorSeverity.ERROR:
        case ErrorSeverity.CRITICAL:
          console.error(prefix, report.message, report.error || '', report.context || '')
          break
      }
    } else if (report.severity === ErrorSeverity.ERROR || report.severity === ErrorSeverity.CRITICAL) {
      console.error('[Error] An error occurred.')
    }
  }

  /**
   * Send error to monitoring service (production)
   */
  private sendToMonitoringService(report: ErrorReport): void {
    // Send to API if configured
    if (this.apiEndpoint) {
      fetch(this.apiEndpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          message: report.message,
          severity: report.severity,
          context: report.context,
          timestamp: report.timestamp.toISOString(),
          source: report.source,
          userAgent: navigator.userAgent,
          url: window.location.href
        })
      }).catch(() => {
        // Silently fail to avoid infinite loops
      })
    }

    // You could also integrate with services like Sentry, LogRocket, etc.
    // Example: Sentry.captureException(report.error)
  }

  /**
   * Report an error from an Axios HTTP request
   */
  reportAxiosError(error: AxiosError, context?: string): void {
    const message = context 
      ? `${context}: ${error.message}` 
      : `API Error: ${error.message}`
    
    this.report({
      message,
      error,
      severity: error.response?.status === 500 ? ErrorSeverity.CRITICAL : ErrorSeverity.ERROR,
      context: {
        status: error.response?.status,
        statusText: error.response?.statusText,
        url: error.config?.url,
        method: error.config?.method
      },
      timestamp: new Date(),
      source: 'api'
    })
  }

  /**
   * Report validation errors
   */
  reportValidationError(message: string, errors?: Record<string, string[]>): void {
    this.report({
      message,
      severity: ErrorSeverity.WARNING,
      context: { validationErrors: errors },
      timestamp: new Date(),
      source: 'validation'
    })
  }

  /**
   * Report info message (development only)
   */
  info(message: string, context?: Record<string, unknown>): void {
    if (!this.isDevelopment) return
    
    this.report({
      message,
      severity: ErrorSeverity.INFO,
      context,
      timestamp: new Date()
    })
  }

  /**
   * Report warning
   */
  warn(message: string, context?: Record<string, unknown>): void {
    this.report({
      message,
      severity: ErrorSeverity.WARNING,
      context,
      timestamp: new Date()
    })
  }

  /**
   * Report error
   */
  error(message: string, error?: Error | unknown, context?: Record<string, unknown>): void {
    this.report({
      message,
      error,
      severity: ErrorSeverity.ERROR,
      context,
      timestamp: new Date()
    })
  }
}

// Export singleton instance
export const errorReporter = new ErrorReporter()

// Convenience exports
export const reportError = (message: string, error?: Error | unknown, context?: Record<string, unknown>) => 
  errorReporter.error(message, error, context)

export const reportWarning = (message: string, context?: Record<string, unknown>) => 
  errorReporter.warn(message, context)

export const reportInfo = (message: string, context?: Record<string, unknown>) => 
  errorReporter.info(message, context)

export const reportAxiosError = (error: AxiosError, context?: string) => 
  errorReporter.reportAxiosError(error, context)

export default errorReporter
