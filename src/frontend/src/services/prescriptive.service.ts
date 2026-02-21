import axiosClient from '@/api/axiosClient'

export interface MaintenanceWindow {
  scheduledDate: string
  estimatedCost: number
  riskScore: number
  recommendation: string
  machineId?: string
  priority?: 'low' | 'medium' | 'high' | 'critical'
  maintenanceType?: string
  estimatedDuration?: number
}

export interface WhatIfScenario {
  scenarioId: string
  description: string
  parameters: Record<string, number>
  predictedOutcome: MaintenanceWindow
  confidenceScore: number
}

/**
 * Get what-if analysis for maintenance scenarios.
 */
export async function getAnalysis(machineId: string, days: number = 30): Promise<MaintenanceWindow[]> {
  const response = await axiosClient.get<MaintenanceWindow[]>(
    `/Prescriptive/${encodeURIComponent(machineId)}/analysis`,
    { params: { days } }
  )
  return response as unknown as MaintenanceWindow[]
}

/**
 * Get optimal maintenance date recommendation.
 */
export async function getOptimal(machineId: string): Promise<MaintenanceWindow> {
  const response = await axiosClient.get<MaintenanceWindow>(
    `/Prescriptive/${encodeURIComponent(machineId)}/optimal`
  )
  return response as unknown as MaintenanceWindow
}

// Export service object for convenience
export const prescriptiveService = {
  getAnalysis,
  getOptimal
}

export default prescriptiveService
