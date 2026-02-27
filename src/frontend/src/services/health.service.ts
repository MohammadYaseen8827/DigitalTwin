import axiosClient from '@/api/axiosClient'
import { errorReporter } from './errorReporter.service'

export interface HealthCheckResult {
  status: 'Healthy' | 'Degraded' | 'Unhealthy'
  totalDuration: string
  entries: Record<string, HealthCheckEntry>
}

export interface HealthCheckEntry {
  status: 'Healthy' | 'Degraded' | 'Unhealthy'
  description?: string
  duration: string
  exception?: string
  data?: Record<string, unknown>
  tags?: string[]
}

/**
 * Get application health status
 */
export async function getHealthStatus(): Promise<HealthCheckResult> {
  try {
    const response = await axiosClient.get<HealthCheckResult>('/health')
    return response
  } catch (error) {
    errorReporter.error('Failed to get health status:', error)
    // Return unhealthy status if health check fails
    return {
      status: 'Unhealthy',
      totalDuration: '0ms',
      entries: {
        api: {
          status: 'Unhealthy',
          description: 'Failed to reach health endpoint',
          duration: '0ms'
        }
      }
    }
  }
}

/**
 * Get database health status
 */
export async function getDatabaseHealth(): Promise<HealthCheckEntry> {
  try {
    const health = await getHealthStatus()
    return health.entries.database || health.entries.Database || {
      status: 'Unhealthy',
      description: 'Database health not available',
      duration: '0ms'
    }
  } catch (error) {
    errorReporter.error('Failed to get database health:', error)
    return {
      status: 'Unhealthy',
      description: 'Failed to check database health',
      duration: '0ms'
    }
  }
}

/**
 * Check if system is healthy
 */
export async function isSystemHealthy(): Promise<boolean> {
  try {
    const health = await getHealthStatus()
    return health.status === 'Healthy'
  } catch (error) {
    return false
  }
}

/**
 * Get readiness status
 */
export async function getReadinessStatus(): Promise<{ ready: boolean; checks: Record<string, boolean> }> {
  try {
    const response = await axiosClient.get<{ ready: boolean; checks: Record<string, boolean> }>('/Health/ready')
    return response
  } catch (error) {
    errorReporter.error('Failed to get readiness status:', error)
    return {
      ready: false,
      checks: {}
    }
  }
}

// Export service object for convenience
export const healthService = {
  getHealthStatus,
  getDatabaseHealth,
  isSystemHealthy,
  getReadinessStatus
}

export default healthService
