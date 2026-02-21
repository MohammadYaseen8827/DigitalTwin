import axiosClient from '@/api/axiosClient'
import type { PredictionDto, HealthClassification } from '@/api/types'

// Response DTOs
export interface RulPredictionDto {
  machineId: string
  remainingUsefulLifeDays: number
  failureProbability: number
  confidenceInterval: {
    lower: number
    upper: number
  }
  featureContributions: Record<string, number>
  predictionDate: string
  modelVersion: string
}

export interface HealthPredictionDto {
  machineId: string
  healthStatus: HealthClassification
  healthScore: number
  factors: {
    name: string
    contribution: number
    status: string
  }[]
  recommendations: string[]
  lastUpdated: string
}

export interface RulSummaryDto {
  machineId: string
  currentRul: number
  averageRul: number
  trend: 'improving' | 'stable' | 'degrading'
  trendPercentage: number
  predictionsCount: number
  confidenceLevel: number
}

export interface ModelStatusDto {
  rulModelLoaded: boolean
  healthModelLoaded: boolean
  modelVersion: string
  lastUpdated: string
}

export interface AnomalyDetectionDto {
  machineId: string
  isAnomaly: boolean
  anomalyScore: number
  threshold: number
  anomalousFeatures: {
    feature: string
    value: number
    expectedRange: { min: number; max: number }
    severity: string
  }[]
  detectionTime: string
}

export interface PredictionSearchParams {
  query?: string
  machineId?: string
  startDate?: string
  endDate?: string
  minRul?: number
  maxRul?: number
  page?: number
  pageSize?: number
}

export interface PredictionSearchResult {
  items: PredictionDto[]
  totalCount: number
  page: number
  pageSize: number
}

// Request DTOs
export interface TrainModelRequest {
  forceRetrain: boolean
  modelType: string
}

/**
 * Get all predictions (paginated).
 */
export async function getAllPredictions(params?: { page?: number; pageSize?: number }): Promise<PredictionSearchResult> {
  const response = await axiosClient.get<PredictionSearchResult>('/Predictions', { params })
  return response as unknown as PredictionSearchResult
}

/**
 * Request a new RUL prediction for a machine.
 */
export async function requestRulPrediction(machineId: string): Promise<RulPredictionDto> {
  const response = await axiosClient.post<RulPredictionDto>(`/Predictions/rul/${encodeURIComponent(machineId)}`)
  return response as unknown as RulPredictionDto
}

/**
 * Get health status prediction for a machine.
 */
export async function getHealthPrediction(machineId: string): Promise<HealthPredictionDto> {
  const response = await axiosClient.get<HealthPredictionDto>(`/Predictions/health/${encodeURIComponent(machineId)}`)
  return response as unknown as HealthPredictionDto
}

/**
 * Get RUL summary statistics for a machine.
 */
export async function getRulSummary(machineId: string): Promise<RulSummaryDto> {
  const response = await axiosClient.get<RulSummaryDto>(`/Predictions/rul/${encodeURIComponent(machineId)}/summary`)
  return response as unknown as RulSummaryDto
}

/**
 * Get prediction model status.
 */
export async function getModelStatus(): Promise<ModelStatusDto> {
  const response = await axiosClient.get<ModelStatusDto>('/Predictions/status')
  return response as unknown as ModelStatusDto
}

/**
 * Get RUL prediction history for a machine.
 */
export async function getRulHistory(machineId: string, take = 100): Promise<PredictionDto[]> {
  const response = await axiosClient.get<PredictionDto[]>(`/Predictions/rul/${encodeURIComponent(machineId)}`, {
    params: { take }
  })
  return response as unknown as PredictionDto[]
}

/**
 * Get anomaly detection results for a machine.
 */
export async function getAnomalyDetection(machineId: string): Promise<AnomalyDetectionDto> {
  const response = await axiosClient.get<AnomalyDetectionDto>(`/Predictions/anomaly/${encodeURIComponent(machineId)}`)
  return response as unknown as AnomalyDetectionDto
}

/**
 * Search predictions with filters.
 */
export async function searchPredictions(params: PredictionSearchParams): Promise<PredictionSearchResult> {
  const response = await axiosClient.get<PredictionSearchResult>('/Predictions/search', { params })
  return response as unknown as PredictionSearchResult
}

/**
 * Request a new generic prediction for a machine.
 */
export async function requestPrediction(machineId: string): Promise<PredictionDto> {
  const response = await axiosClient.post<PredictionDto>('/Predictions', { machineId })
  return response as unknown as PredictionDto
}

/**
 * Fetch prediction history for a machine.
 * NOTE: This endpoint is not implemented in the backend
 */
export async function fetchPredictionHistory(machineId: string, take = 100): Promise<PredictionDto[]> {
  console.warn(`fetchPredictionHistory: Backend endpoint /Predictions/${machineId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

/**
 * Request ensemble prediction using advanced analytics.
 */
export async function requestEnsemblePrediction(machineId: string): Promise<PredictionDto> {
  const response = await axiosClient.post<PredictionDto>(`/AdvancedAnalytics/predictions/${encodeURIComponent(machineId)}/ensemble`)
  return response as unknown as PredictionDto
}

/**
 * Trigger AI model training.
 */
export async function trainAIModel(forceRetrain = false, modelType = 'all'): Promise<{ success: boolean; message: string }> {
  const response = await axiosClient.post('/Predictions/ai/train', {
    forceRetrain,
    modelType
  })
  return response as unknown as { success: boolean; message: string }
}

/**
 * Trigger model retraining (legacy endpoint).
 */
export async function retrainModels(forceRetrain = false, modelType = 'all'): Promise<{ success: boolean; message: string }> {
  const response = await axiosClient.post('/Predictions/train', {
    forceRetrain,
    modelType
  })
  return response as unknown as { success: boolean; message: string }
}

// Export service object for convenience
export const predictionsService = {
  getAllPredictions,
  requestRulPrediction,
  getHealthPrediction,
  getRulSummary,
  getModelStatus,
  getRulHistory,
  getAnomalyDetection,
  searchPredictions,
  requestPrediction,
  fetchPredictionHistory,
  requestEnsemblePrediction,
  trainAIModel,
  retrainModels
}

export default predictionsService
