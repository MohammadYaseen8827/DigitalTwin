// File: src/services/modelLifecycle.service.ts
import axiosClient from '@/api/axiosClient'
import { useToast } from '@/lib/magic-mcp-ui'
import { errorReporter } from './errorReporter.service'

const toast = useToast()

// === Types ===
export interface ModelVersionDto {
  id: string
  modelType: string
  version: string
  modelPath: string
  trainedAt: string
  metrics: Record<string, number>
  trainingDatasetHash?: string
  status: string
  promotedAt?: string
  notes?: string
  createdAt: string
  updatedAt?: string
}

export interface RegisterModelVersionRequest {
  modelType: string
  modelPath: string
  metrics: Record<string, number>
  trainingDatasetHash?: string
  notes?: string
}

export interface PromoteModelRequest {
  targetStatus: string
  notes?: string
}

export interface CompareModelsRequest {
  modelVersionId1: string
  modelVersionId2: string
}

export interface ModelComparisonResult {
  bestModel?: any
  comparisons: any[]
}

export interface ModelPerformanceSummary {
  modelId: string
  accuracy: number
  latency: number
  throughput: number
  errorRate: number
}

// === Backend-aligned Endpoints ===

/**
 * Register a new model version
 * POST /api/ModelLifecycle/register
 */
export async function registerModelVersion(request: RegisterModelVersionRequest): Promise<ModelVersionDto> {
  try {
    const response = await axiosClient.post('/ModelLifecycle/register', request)
    toast.success('Model version registered successfully')
    return response.data
  } catch (error) {
    errorReporter.error('Failed to register model version:', error)
    throw error
  }
}

/**
 * Promote a model to a new status
 * POST /api/ModelLifecycle/{id}/promote
 */
export async function promoteModelVersion(modelId: string, request: PromoteModelRequest): Promise<ModelVersionDto> {
  try {
    const response = await axiosClient.post(`/ModelLifecycle/${encodeURIComponent(modelId)}/promote`, request)
    toast.success(`Model promoted to ${request.targetStatus}`)
    return response.data
  } catch (error) {
    errorReporter.error(`Failed to promote model ${modelId}:`, error)
    throw error
  }
}

/**
 * Get all model versions (optionally filtered by type)
 * GET /api/ModelLifecycle
 */
export async function fetchModelLifecycles(modelType?: string): Promise<ModelVersionDto[]> {
  try {
    const params = modelType ? { modelType } : {}
    const response = await axiosClient.get('/ModelLifecycle', { params })
    return response.data
  } catch (error) {
    errorReporter.error('Failed to fetch model lifecycles:', error)
    throw error
  }
}

/**
 * Get current production version for a model type
 * GET /api/ModelLifecycle/production/{modelType}
 */
export async function getProductionVersion(modelType: string): Promise<ModelVersionDto | null> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/production/${encodeURIComponent(modelType)}`)
    return response.data
  } catch (error: any) {
    if (error.response?.status === 404) {
      return null
    }
    errorReporter.error(`Failed to get production version for ${modelType}:`, error)
    throw error
  }
}

/**
 * Compare two model versions
 * POST /api/ModelLifecycle/compare
 */
export async function compareModelVersions(request: CompareModelsRequest): Promise<ModelComparisonResult> {
  try {
    const response = await axiosClient.post('/ModelLifecycle/compare', request)
    return response.data
  } catch (error) {
    errorReporter.error('Failed to compare models:', error)
    throw error
  }
}

/**
 * Get performance summary for a model version
 * GET /api/ModelLifecycle/{id}/performance
 */
export async function getModelPerformance(modelId: string): Promise<ModelPerformanceSummary> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/${encodeURIComponent(modelId)}/performance`)
    return response.data
  } catch (error) {
    errorReporter.error(`Failed to get performance for model ${modelId}:`, error)
    throw error
  }
}

// === Utility functions ===

export function getStageColor(stage: string): string {
  const stageColors: Record<string, string> = {
    'development': 'var(--color-text-secondary)',
    'staging': 'var(--color-warning)',
    'production': 'var(--color-success)',
    'deprecated': 'var(--color-error)',
    'retired': 'var(--color-text-secondary)'
  }
  return stageColors[stage.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getStatusDisplayName(status: string): string {
  const statusNames: Record<string, string> = {
    'development': 'Development',
    'staging': 'Staging',
    'production': 'Production',
    'deprecated': 'Deprecated',
    'retired': 'Retired'
  }
  return statusNames[status.toLowerCase()] || status
}

// Export service object
export const modelLifecycleService = {
  registerModelVersion,
  promoteModelVersion,
  fetchModelLifecycles,
  getProductionVersion,
  compareModelVersions,
  getModelPerformance,
  getStageColor,
  getStatusDisplayName
}

export default modelLifecycleService
