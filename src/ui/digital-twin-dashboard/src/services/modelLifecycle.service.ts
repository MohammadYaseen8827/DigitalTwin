// File: src/services/modelLifecycle.service.ts
import axiosClient from '@/api/axiosClient'
import type { 
  ModelLifecycleDto,
  CreateModelLifecycleDto,
  UpdateModelLifecycleDto,
  ModelLifecycleTransitionDto,
  ModelLifecycleMetricsDto
} from '@/api/types'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

/**
 * Get all model lifecycle entries
 */
export async function fetchModelLifecycles(): Promise<ModelLifecycleDto[]> {
  try {
    const response = await axiosClient.get('/ModelLifecycle')
    return response.data
  } catch (error) {
    console.error('Failed to fetch model lifecycles:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch model lifecycles'
    toast.error(message)
    throw error
  }
}

/**
 * Get model lifecycle by ID
 */
export async function fetchModelLifecycle(lifecycleId: string): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch model lifecycle ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch model lifecycle'
    toast.error(message)
    throw error
  }
}

/**
 * Get model lifecycle by model ID and version
 */
export async function fetchModelLifecycleByVersion(modelId: string, version: string): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/model/${encodeURIComponent(modelId)}/version/${encodeURIComponent(version)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch model lifecycle for model ${modelId} version ${version}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch model lifecycle version'
    toast.error(message)
    throw error
  }
}

/**
 * Create new model lifecycle entry
 */
export async function createModelLifecycle(lifecycle: CreateModelLifecycleDto): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.post('/ModelLifecycle', lifecycle)
    toast.success('Model lifecycle entry created successfully')
    return response.data
  } catch (error) {
    console.error('Failed to create model lifecycle:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to create model lifecycle'
    toast.error(message)
    throw error
  }
}

/**
 * Update model lifecycle entry
 */
export async function updateModelLifecycle(lifecycleId: string, lifecycle: UpdateModelLifecycleDto): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.put(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}`, lifecycle)
    toast.success('Model lifecycle updated successfully')
    return response.data
  } catch (error) {
    console.error(`Failed to update model lifecycle ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to update model lifecycle'
    toast.error(message)
    throw error
  }
}

/**
 * Delete model lifecycle entry
 */
export async function deleteModelLifecycle(lifecycleId: string): Promise<void> {
  try {
    await axiosClient.delete(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}`)
    toast.success('Model lifecycle deleted successfully')
  } catch (error) {
    console.error(`Failed to delete model lifecycle ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to delete model lifecycle'
    toast.error(message)
    throw error
  }
}

/**
 * Get lifecycle history for a model
 */
export async function fetchModelLifecycleHistory(modelId: string): Promise<ModelLifecycleDto[]> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/model/${encodeURIComponent(modelId)}/history`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch model lifecycle history for model ${modelId}:`, error)
    throw error
  }
}

/**
 * Transition model to next stage
 */
export async function transitionModelStage(
  lifecycleId: string,
  transition: {
    toStage: string
    reason: string
    approvedBy: string
    notes?: string
  }
): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.post(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}/transition`, transition)
    toast.success(`Model transitioned to ${transition.toStage} stage`)
    return response.data
  } catch (error) {
    console.error(`Failed to transition model lifecycle ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to transition model stage'
    toast.error(message)
    throw error
  }
}

/**
 * Get available transitions for current stage
 */
export async function fetchAvailableTransitions(currentStage: string): Promise<string[]> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/transitions/${encodeURIComponent(currentStage)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch available transitions for stage ${currentStage}:`, error)
    throw error
  }
}

/**
 * Get model lifecycle metrics
 */
export async function fetchModelLifecycleMetrics(modelId: string, version?: string): Promise<ModelLifecycleMetricsDto> {
  try {
    const params = new URLSearchParams()
    if (version) {
      params.append('version', version)
    }

    const response = await axiosClient.get(`/ModelLifecycle/model/${encodeURIComponent(modelId)}/metrics?${params.toString()}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch model lifecycle metrics for model ${modelId}:`, error)
    throw error
  }
}

/**
 * Promote model to next stage
 */
export async function promoteModel(lifecycleId: string, promotionData: {
  targetStage: string
  justification: string
  approvedBy: string
  conditions?: string[]
}): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.post(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}/promote`, promotionData)
    toast.success(`Model promoted to ${promotionData.targetStage}`)
    return response.data
  } catch (error) {
    console.error(`Failed to promote model ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to promote model'
    toast.error(message)
    throw error
  }
}

/**
 * Demote model to previous stage
 */
export async function demoteModel(lifecycleId: string, demotionData: {
  targetStage: string
  reason: string
  approvedBy: string
  issues?: string[]
}): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.post(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}/demote`, demotionData)
    toast.success(`Model demoted to ${demotionData.targetStage}`)
    return response.data
  } catch (error) {
    console.error(`Failed to demote model ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to demote model'
    toast.error(message)
    throw error
  }
}

/**
 * Retire model
 */
export async function retireModel(lifecycleId: string, retirementData: {
  reason: string
  replacementModelId?: string
  approvedBy: string
  deprecationDate?: string
}): Promise<ModelLifecycleDto> {
  try {
    const response = await axiosClient.post(`/ModelLifecycle/${encodeURIComponent(lifecycleId)}/retire`, retirementData)
    toast.success('Model retired successfully')
    return response.data
  } catch (error) {
    console.error(`Failed to retire model ${lifecycleId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to retire model'
    toast.error(message)
    throw error
  }
}

/**
 * Get models in specific stage
 */
export async function fetchModelsByStage(stage: string): Promise<ModelLifecycleDto[]> {
  try {
    const response = await axiosClient.get(`/ModelLifecycle/stage/${encodeURIComponent(stage)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch models in stage ${stage}:`, error)
    throw error
  }
}

/**
 * Get models ready for promotion
 */
export async function fetchModelsReadyForPromotion(): Promise<ModelLifecycleDto[]> {
  try {
    const response = await axiosClient.get('/ModelLifecycle/ready-for-promotion')
    return response.data
  } catch (error) {
    console.error('Failed to fetch models ready for promotion:', error)
    throw error
  }
}

/**
 * Get models requiring attention
 */
export async function fetchModelsRequiringAttention(): Promise<ModelLifecycleDto[]> {
  try {
    const response = await axiosClient.get('/ModelLifecycle/requiring-attention')
    return response.data
  } catch (error) {
    console.error('Failed to fetch models requiring attention:', error)
    throw error
  }
}

/**
 * Get lifecycle statistics
 */
export async function fetchLifecycleStatistics(): Promise<{
  totalModels: number
  modelsByStage: Record<string, number>
  modelsByStatus: Record<string, number>
  averageTimeInStages: Record<string, number>
  promotionRate: number
  retirementRate: number
}> {
  try {
    const response = await axiosClient.get('/ModelLifecycle/statistics')
    return response.data
  } catch (error) {
    console.error('Failed to fetch lifecycle statistics:', error)
    throw error
  }
}

/**
 * Export lifecycle report
 */
export async function exportLifecycleReport(
  modelId?: string,
  dateRange?: { startDate: string; endDate: string },
  format: 'PDF' | 'CSV' | 'JSON' = 'PDF'
): Promise<Blob> {
  try {
    const params = new URLSearchParams()
    params.append('format', format)
    if (modelId) {
      params.append('modelId', modelId)
    }
    if (dateRange) {
      params.append('startDate', dateRange.startDate)
      params.append('endDate', dateRange.endDate)
    }

    const response = await axiosClient.get('/ModelLifecycle/export', {
      params,
      responseType: 'blob'
    })
    
    // Create download link
    const url = window.URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = `model-lifecycle-report.${format.toLowerCase()}`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
    toast.success('Lifecycle report exported successfully')
    return response.data
  } catch (error) {
    console.error('Failed to export lifecycle report:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to export lifecycle report'
    toast.error(message)
    throw error
  }
}

// Utility functions
export function getStageColor(stage: string): string {
  const stageColors: Record<string, string> = {
    'development': 'var(--color-text-secondary)',
    'training': 'var(--color-warning)',
    'validation': 'var(--color-info)',
    'deployment': 'var(--color-success)',
    'retired': 'var(--color-text-secondary)'
  }
  return stageColors[stage.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getStatusColor(status: string): string {
  const statusColors: Record<string, string> = {
    'development': 'var(--color-text-secondary)',
    'training': 'var(--color-warning)',
    'validation': 'var(--color-info)',
    'deployment': 'var(--color-success)',
    'retired': 'var(--color-text-secondary)'
  }
  return statusColors[status.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getStageDisplayName(stage: string): string {
  const stageNames: Record<string, string> = {
    'development': 'Development',
    'training': 'Training',
    'validation': 'Validation',
    'deployment': 'Deployment',
    'retired': 'Retired'
  }
  return stageNames[stage.toLowerCase()] || stage
}

export function canPromoteFromStage(stage: string): boolean {
  const promotableStages = ['development', 'training', 'validation']
  return promotableStages.includes(stage.toLowerCase())
}

export function canDemoteFromStage(stage: string): boolean {
  const demotableStages = ['training', 'validation', 'deployment']
  return demotableStages.includes(stage.toLowerCase())
}

export function isFinalStage(stage: string): boolean {
  return stage.toLowerCase() === 'retired'
}

export function calculateStageProgress(lifecycle: ModelLifecycleDto): number {
  const stageOrder = ['development', 'training', 'validation', 'deployment', 'retired']
  const currentIndex = stageOrder.indexOf(lifecycle.stage.toLowerCase())
  return currentIndex >= 0 ? ((currentIndex + 1) / stageOrder.length) * 100 : 0
}
