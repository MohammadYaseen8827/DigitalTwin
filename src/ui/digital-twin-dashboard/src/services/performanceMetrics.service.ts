import axiosClient from '@/api/axiosClient'

export interface PerformanceMetrics {
  id: string
  operationName: string
  durationMs: number
  timestamp: string
  statusCode?: number
  exceededThreshold: boolean
  thresholdMs: number
  machineId?: string
  userId?: string
}

export interface PerformanceStatistics {
  totalOperations: number
  averageDurationMs: number
  minDurationMs: number
  maxDurationMs: number
  percentile95DurationMs: number
  percentile99DurationMs: number
  errorRate: number
  throughputPerMinute: number
  mostCommonOperations: Array<{
    operationName: string
    count: number
    averageDurationMs: number
  }>
  slowestOperations: Array<{
    operationName: string
    averageDurationMs: number
    count: number
  }>
}

export interface MetricsFilter {
  startTime?: string
  endTime?: string
  operationName?: string
}

/**
 * Get performance metrics
 */
export async function getPerformanceMetrics(filter: MetricsFilter = {}): Promise<PerformanceMetrics[]> {
  const params = new URLSearchParams()
  if (filter.startTime) params.append('startTime', filter.startTime)
  if (filter.endTime) params.append('endTime', filter.endTime)
  if (filter.operationName) params.append('operationName', filter.operationName)
  
  const response = await axiosClient.get<PerformanceMetrics[]>(`/PerformanceMetrics?${params.toString()}`)
  return response.data
}

/**
 * Get performance statistics
 */
export async function getPerformanceStatistics(filter: MetricsFilter = {}): Promise<PerformanceStatistics> {
  const params = new URLSearchParams()
  if (filter.startTime) params.append('startTime', filter.startTime)
  if (filter.endTime) params.append('endTime', filter.endTime)
  if (filter.operationName) params.append('operationName', filter.operationName)
  
  const response = await axiosClient.get<PerformanceStatistics>(`/PerformanceMetrics/statistics?${params.toString()}`)
  return response.data
}

/**
 * Get threshold violations
 */
export async function getThresholdViolations(filter: MetricsFilter = {}): Promise<PerformanceMetrics[]> {
  const params = new URLSearchParams()
  if (filter.startTime) params.append('startTime', filter.startTime)
  if (filter.endTime) params.append('endTime', filter.endTime)
  
  const response = await axiosClient.get<PerformanceMetrics[]>(`/PerformanceMetrics/threshold-violations?${params.toString()}`)
  return response.data
}

/**
 * Get recent performance metrics (last hour)
 */
export async function getRecentMetrics(): Promise<PerformanceMetrics[]> {
  const oneHourAgo = new Date(Date.now() - 60 * 60 * 1000).toISOString()
  return getPerformanceMetrics({ startTime: oneHourAgo })
}