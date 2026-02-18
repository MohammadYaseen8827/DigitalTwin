import axiosClient from '@/api/axiosClient'

export interface DataArchivalRequest {
  retentionDays: number
}

export interface ArchivalStatus {
  jobId: string
  status: 'Pending' | 'Running' | 'Completed' | 'Failed'
  progress: number
  message: string
  startedAt: string
  completedAt?: string
  archivedRecords: number
  freedSpaceMB: number
}

/**
 * Archive old telemetry data
 */
export async function archiveTelemetry(request: DataArchivalRequest): Promise<ArchivalStatus> {
  const response = await axiosClient.post<ArchivalStatus>('/DataArchival', request)
  return response.data
}

/**
 * Get archival job status
 */
export async function getArchivalStatus(jobId: string): Promise<ArchivalStatus> {
  const response = await axiosClient.get<ArchivalStatus>(`/DataArchival/status/${jobId}`)
  return response.data
}

/**
 * Get recent archival jobs
 */
export async function getArchivalHistory(limit: number = 10): Promise<ArchivalStatus[]> {
  const response = await axiosClient.get<ArchivalStatus[]>(`/DataArchival/history?limit=${limit}`)
  return response.data
}