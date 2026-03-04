/**
 * Dashboard Service
 * Handles dashboard statistics and overview data
 */

import axiosClient from '@/api/axiosClient'

export interface DashboardStatsDto {
  totalMachines: number
  activeMachines: number
  totalProductionLines: number
  activeAlerts: number
  criticalAlerts: number
  warningAlerts: number
  averageHealthScore: number
  machinesNeedingAttention: number
  upcomingMaintenance: number
  recentPredictions: number
  lastUpdated: string
}

export interface DashboardOverview {
  stats: DashboardStatsDto
  recentAlerts: Array<{
    id: string
    severity: string
    message: string
    timestamp: string
  }>
  machineHealthDistribution: Array<{
    status: string
    count: number
    percentage: number
  }>
  maintenanceSchedule: Array<{
    machineId: string
    machineName: string
    scheduledDate: string
    maintenanceType: string
  }>
}

/**
 * Get dashboard statistics
 */
export async function getDashboardStats(): Promise<DashboardStatsDto> {
  const response = await axiosClient.get<DashboardStatsDto>('Dashboard/stats')
  return response as unknown as DashboardStatsDto
}

/**
 * Get comprehensive dashboard overview (composed from stats)
 */
export async function getDashboardOverview(): Promise<DashboardOverview> {
  // Dashboard/overview endpoint doesn't exist in backend
  // Compose from available data
  const stats = await getDashboardStats()
  return {
    stats,
    recentAlerts: [],
    machineHealthDistribution: [],
    maintenanceSchedule: []
  }
}

// Export service object for convenience
export const dashboardService = {
  getDashboardStats,
  getDashboardOverview
}

export default dashboardService
