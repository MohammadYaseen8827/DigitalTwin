import axiosClient from '@/api/axiosClient'

export interface DriftDetectionResult {
  modelName: string
  featureDriftScore: number
  predictionDriftScore: number
  timestamp: string
  isDriftDetected: boolean
  recommendation: string
  detectedAt: string
}

export interface DriftMetrics {
  modelName: string
  featureDriftScore: number
  predictionDriftScore: number
  timestamp: string
  sampleSize: number
}

export interface DriftThresholds {
  featureDriftThreshold: number
  predictionDriftThreshold: number
  alertThreshold: number
}

export interface DriftReport {
  modelName?: string
  periodDays: number
  totalDetections: number
  driftEvents: number
  averageFeatureDrift: number
  averagePredictionDrift: number
  recommendations: string[]
  detailedMetrics: DriftMetrics[]
}

export interface DriftDetectionRequest {
  referenceData: any[]
  currentData: any[]
  thresholds?: DriftThresholds
  method?: string
}

/**
 * Get current drift status for all models
 */
export async function getCurrentDriftStatus(): Promise<Record<string, DriftDetectionResult>> {
  const response = await axiosClient.get<Record<string, DriftDetectionResult>>('Drift/status')
  return response.data
}

/**
 * Get drift history for a specific model
 */
export async function getDriftHistory(modelName: string, hours: number = 24): Promise<DriftMetrics[]> {
  const response = await axiosClient.get<DriftMetrics[]>(`/Drift/history/${modelName}?hours=${hours}`)
  return response.data
}

/**
 * Get drift thresholds for a model
 */
export async function getThresholds(modelName: string): Promise<DriftThresholds> {
  const response = await axiosClient.get<DriftThresholds>(`/Drift/thresholds/${modelName}`)
  return response.data
}

/**
 * Configure drift thresholds for a model
 */
export async function configureThresholds(modelName: string, thresholds: DriftThresholds): Promise<void> {
  await axiosClient.put(`/Drift/thresholds/${modelName}`, thresholds)
}

/**
 * Generate drift report
 */
export async function generateDriftReport(modelName?: string, days: number = 7): Promise<DriftReport> {
  const params = new URLSearchParams()
  if (modelName) params.append('modelName', modelName)
  params.append('days', days.toString())
  
  const response = await axiosClient.get<DriftReport>(`/Drift/report?${params.toString()}`)
  return response.data
}

/**
 * Manually detect drift
 */
export async function detectDrift(request: DriftDetectionRequest): Promise<DriftDetectionResult> {
  const response = await axiosClient.post<DriftDetectionResult>('Drift/detect', request)
  return response.data
}

/**
 * Get recent drift detections (last 24 hours)
 */
export async function getRecentDriftDetections(): Promise<Record<string, DriftDetectionResult>> {
  return getCurrentDriftStatus()
}

/**
 * Get drift monitoring configuration
 */
export async function getMonitoringConfig(): Promise<Record<string, any>> {
  const response = await axiosClient.get<Record<string, any>>('Drift/monitoring/config')
  return response.data
}

// Export service object for convenience
export const driftDetectionService = {
  getCurrentDriftStatus,
  getDriftHistory,
  getThresholds,
  configureThresholds,
  generateDriftReport,
  detectDrift,
  getRecentDriftDetections,
  getMonitoringConfig
}

export default driftDetectionService