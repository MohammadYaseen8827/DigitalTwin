import axiosClient from '@/api/axiosClient'

export interface MaintenanceWindow {
  scheduledDate: string
  estimatedCost: number
  riskScore: number
  recommendation: string
}

export const prescriptiveService = {
  async getAnalysis(machineId: string, days: number = 30): Promise<MaintenanceWindow[]> {
    const response = await axiosClient.get(`/prescriptive/${machineId}/analysis?days=${days}`)
    return response.data
  },

  async getOptimal(machineId: string): Promise<MaintenanceWindow> {
    const response = await axiosClient.get(`/prescriptive/${machineId}/optimal`)
    return response.data
  }
}
