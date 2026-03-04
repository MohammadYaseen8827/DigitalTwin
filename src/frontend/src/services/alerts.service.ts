import axiosClient from '@/api/axiosClient'
import type { AlertDto, AlertStatsDto } from '@/api/types'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

/**
 * Fetch active alerts for all machines
 */
export async function fetchActiveAlerts(machineId?: string): Promise<AlertDto[]> {
  try {
    const params = machineId ? { machineId } : {}
    const response = await axiosClient.get('Alerts', { params })
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error('Failed to fetch active alerts:', error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch active alerts'
    toast.error(message)
    throw error
  }
}

/**
 * Fetch specific alert by ID
 */
export async function fetchAlert(alertId: string): Promise<AlertDto> {
  try {
    const response = await axiosClient.get(`/Alerts/${encodeURIComponent(alertId)}`)
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error(`Failed to fetch alert ${alertId}:`, error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch alert'
    toast.error(message)
    throw error
  }
}

/**
 * Acknowledge an alert
 */
export async function acknowledgeAlert(alertId: string): Promise<void> {
  try {
    await axiosClient.put(`/Alerts/${encodeURIComponent(alertId)}/acknowledge`)
    toast.success('Alert acknowledged successfully')
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error(`Failed to acknowledge alert ${alertId}:`, error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to acknowledge alert'
    toast.error(message)
    throw error
  }
}

/**
 * Delete an alert
 */
export async function deleteAlert(alertId: string): Promise<void> {
  try {
    await axiosClient.delete(`/Alerts/${encodeURIComponent(alertId)}`)
    toast.success('Alert deleted successfully')
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error(`Failed to delete alert ${alertId}:`, error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to delete alert'
    toast.error(message)
    throw error
  }
}

/**
 * Fetch all alerts (including resolved)
 */
export async function fetchAllAlerts(machineId?: string): Promise<AlertDto[]> {
  try {
    const params = machineId ? { machineId } : {}
    const response = await axiosClient.get('/Alerts/all', { params })
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error('Failed to fetch all alerts:', error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch all alerts'
    toast.error(message)
    throw error
  }
}

/**
 * Fetch alert statistics
 */
export async function fetchAlertStats(machineId?: string): Promise<AlertStatsDto> {
  try {
    const params = machineId ? { machineId } : {}
    const response = await axiosClient.get('/Alerts/stats', { params })
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error('Failed to fetch alert stats:', error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch alert stats'
    toast.error(message)
    throw error
  }
}

/**
 * Search alerts with filters
 */
export async function searchAlerts(params: {
  query?: string
  status?: string
  severity?: string
  machineId?: string
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}): Promise<{ alerts: AlertDto[]; totalCount: number }> {
  try {
    const response = await axiosClient.get('/Alerts/search', { params })
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error('Failed to search alerts:', error)
    }
    const message = (error as any)?.response?.data?.message ?? 'Failed to search alerts'
    toast.error(message)
    throw error
  }
}

/**
 * Resolve an alert - DEPRECATED (no backend endpoint)
 * Use deleteAlert instead
 */
export async function resolveAlert(alertId: string): Promise<void> {
  if (import.meta.env.DEV) {
    console.warn('resolveAlert is deprecated, use deleteAlert instead')
  }
  return deleteAlert(alertId)
}

/**
 * Escalate an alert - DEPRECATED (no backend endpoint)
 * Alert escalation is not supported in current backend
 */
export async function escalateAlert(alertId: string): Promise<void> {
  throw new Error('Alert escalation is not supported in the current backend')
}

/**
 * Get alert by ID (fetch single alert)
 */
export async function getAlertById(alertId: string): Promise<AlertDto> {
  try {
    const response = await axiosClient.get(`/Alerts/${encodeURIComponent(alertId)}`)
    return response.data
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error(`Failed to fetch alert ${alertId}:`, error)
    }
    throw error
  }
}

/**
 * Acknowledge alert by ID (PUT method) - DEPRECATED
 * Use acknowledgeAlert instead
 */
export async function acknowledgeAlertPut(alertId: string): Promise<void> {
  return acknowledgeAlert(alertId)
}

// Utility functions
export function getSeverityColor(severity: string): string {
  const severityColors: Record<string, string> = {
    'info': 'var(--color-info)',
    'warning': 'var(--color-warning)',
    'critical': 'var(--color-error)',
    'error': 'var(--color-error)'
  }
  return severityColors[severity.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getStatusColor(status: string): string {
  const statusColors: Record<string, string> = {
    'active': 'var(--color-error)',
    'acknowledged': 'var(--color-warning)',
    'resolved': 'var(--color-success)',
    'closed': 'var(--color-text-secondary)'
  }
  return statusColors[status.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getSeverityIcon(severity: string): string {
  const severityIcons: Record<string, string> = {
    'info': 'ℹ️',
    'warning': '⚠️',
    'critical': '🚨',
    'error': '❌'
  }
  return severityIcons[severity.toLowerCase()] || 'ℹ️'
}

export function formatAlertDuration(createdAt: string, acknowledgedAt?: string, resolvedAt?: string): string {
  const created = new Date(createdAt)
  const end = resolvedAt ? new Date(resolvedAt) : acknowledgedAt ? new Date(acknowledgedAt) : new Date()
  const durationMs = end.getTime() - created.getTime()
  
  const hours = Math.floor(durationMs / (1000 * 60 * 60))
  const minutes = Math.floor((durationMs % (1000 * 60 * 60)) / (1000 * 60))
  
  if (hours > 24) {
    const days = Math.floor(hours / 24)
    return `${days}d ${hours % 24}h ${minutes}m`
  }
  
  return `${hours}h ${minutes}m`
}

// Service object export for convenience
export const alertsService = {
  fetchActiveAlerts,
  fetchAlert,
  acknowledgeAlert,
  deleteAlert,
  fetchAllAlerts,
  fetchAlertStats,
  searchAlerts,
  resolveAlert,
  escalateAlert,
  getAlertById,
  acknowledgeAlertPut,
  getSeverityColor,
  getStatusColor,
  getSeverityIcon,
  formatAlertDuration
}

export default alertsService
