import axiosClient from '@/api/axiosClient'
import { errorReporter } from './errorReporter.service'

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
  const response = await axiosClient.post<ArchivalStatus>('DataArchival', request)
  return response.data
}

/**
 * Get archival job status
 * NOTE: This endpoint is not implemented in the backend
 */
export async function getArchivalStatus(jobId: string): Promise<ArchivalStatus> {
  errorReporter.warn(`getArchivalStatus: Backend endpoint /DataArchival/status/${jobId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

/**
 * Get recent archival jobs
 * NOTE: This endpoint is not implemented in the backend
 */
export async function getArchivalHistory(limit: number = 10): Promise<ArchivalStatus[]> {
  errorReporter.warn(`getArchivalHistory: Backend endpoint /DataArchival/history not implemented`)
  throw new Error('This endpoint is not yet implemented')
}