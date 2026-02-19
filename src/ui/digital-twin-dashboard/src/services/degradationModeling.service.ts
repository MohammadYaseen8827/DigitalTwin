// File: src/services/degradationModeling.service.ts
import axiosClient from '@/api/axiosClient'
import type { 
  DegradationModelDto,
  CreateDegradationModelDto,
  UpdateDegradationModelDto,
  DegradationModelType,
  DegradationPredictionDto,
  AIModelTrainingRequestDto,
  AIModelTrainingResultDto,
  DegradationAnalysisDto
} from '@/api/types'
import { useToast } from '@/lib/magic-mcp-ui'

const toast = useToast()

/**
 * Get all degradation models
 */
export async function fetchDegradationModels(): Promise<DegradationModelDto[]> {
  try {
    const response = await axiosClient.get('/DegradationModeling')
    return response.data
  } catch (error) {
    console.error('Failed to fetch degradation models:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch degradation models'
    toast.error(message)
    throw error
  }
}

/**
 * Get degradation model by ID
 */
export async function fetchDegradationModel(modelId: string): Promise<DegradationModelDto> {
  try {
    const response = await axiosClient.get(`/DegradationModeling/${encodeURIComponent(modelId)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch degradation model ${modelId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to fetch degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Create new degradation model
 */
export async function createDegradationModel(model: CreateDegradationModelDto): Promise<DegradationModelDto> {
  try {
    const response = await axiosClient.post('/DegradationModeling', model)
    toast.success('Degradation model created successfully')
    return response.data
  } catch (error) {
    console.error('Failed to create degradation model:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to create degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Update degradation model
 */
export async function updateDegradationModel(modelId: string, model: UpdateDegradationModelDto): Promise<DegradationModelDto> {
  try {
    const response = await axiosClient.put(`/DegradationModeling/${encodeURIComponent(modelId)}`, model)
    toast.success('Degradation model updated successfully')
    return response.data
  } catch (error) {
    console.error(`Failed to update degradation model ${modelId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to update degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Delete degradation model
 */
export async function deleteDegradationModel(modelId: string): Promise<void> {
  try {
    await axiosClient.delete(`/DegradationModeling/${encodeURIComponent(modelId)}`)
    toast.success('Degradation model deleted successfully')
  } catch (error) {
    console.error(`Failed to delete degradation model ${modelId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to delete degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Train degradation model
 */
export async function trainDegradationModel(request: AIModelTrainingRequestDto): Promise<AIModelTrainingResultDto> {
  try {
    const response = await axiosClient.post('/DegradationModeling/train', request)
    toast.success('Degradation model training started')
    return response.data
  } catch (error) {
    console.error('Failed to train degradation model:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to train degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Get training status for a model
 */
export async function fetchTrainingStatus(modelId: string): Promise<AIModelTrainingResultDto> {
  try {
    const response = await axiosClient.get(`/DegradationModeling/${encodeURIComponent(modelId)}/training-status`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch training status for model ${modelId}:`, error)
    throw error
  }
}

/**
 * Predict degradation using a model
 */
export async function predictDegradation(
  modelId: string,
  inputData: any[],
  horizon?: number
): Promise<DegradationPredictionDto[]> {
  try {
    const requestData = {
      modelId,
      inputData,
      horizon: horizon || 30 // Default 30 days prediction
    }

    const response = await axiosClient.post('/DegradationModeling/predict', requestData)
    return response.data
  } catch (error) {
    console.error('Failed to predict degradation:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to predict degradation'
    toast.error(message)
    throw error
  }
}

/**
 * Estimate degradation for a machine
 */
export async function estimateDegradation(
  machineId: string,
  parameters: {
    modelType?: DegradationModelType
    timeHorizon?: number
    confidenceLevel?: number
    includeUncertainty?: boolean
  }
): Promise<DegradationAnalysisDto> {
  try {
    const response = await axiosClient.post(`/api/degradation/estimate/${encodeURIComponent(machineId)}`, parameters)
    return response.data
  } catch (error) {
    console.error(`Failed to estimate degradation for machine ${machineId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to estimate degradation'
    toast.error(message)
    throw error
  }
}

/**
 * Analyze degradation patterns
 */
export async function analyzeDegradation(
  machineId: string,
  timeRange: { startDate: string; endDate: string },
  modelType?: DegradationModelType
): Promise<DegradationAnalysisDto> {
  try {
    const params = new URLSearchParams()
    params.append('machineId', machineId)
    params.append('startDate', timeRange.startDate)
    params.append('endDate', timeRange.endDate)
    if (modelType) {
      params.append('modelType', modelType)
    }

    const response = await axiosClient.get(`/DegradationModeling/analyze?${params.toString()}`)
    return response.data
  } catch (error) {
    console.error('Failed to analyze degradation:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to analyze degradation'
    toast.error(message)
    throw error
  }
}

/**
 * Get available degradation model types
 */
export async function fetchDegradationModelTypes(): Promise<DegradationModelType[]> {
  try {
    const response = await axiosClient.get('/DegradationModeling/model-types')
    return response.data
  } catch (error) {
    console.error('Failed to fetch degradation model types:', error)
    throw error
  }
}

/**
 * Get model performance metrics
 */
export async function fetchDegradationModelMetrics(modelId: string): Promise<any> {
  try {
    const response = await axiosClient.get(`/DegradationModeling/${encodeURIComponent(modelId)}/metrics`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch degradation model metrics ${modelId}:`, error)
    throw error
  }
}

/**
 * Validate degradation model
 */
export async function validateDegradationModel(
  modelId: string,
  validationData: any[]
): Promise<any> {
  try {
    const requestData = {
      modelId,
      validationData
    }

    const response = await axiosClient.post('/DegradationModeling/validate', requestData)
    toast.success('Degradation model validation completed')
    return response.data
  } catch (error) {
    console.error('Failed to validate degradation model:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to validate degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Get degradation model recommendations
 */
export async function fetchDegradationRecommendations(machineId: string): Promise<any[]> {
  try {
    const response = await axiosClient.get(`/DegradationModeling/recommendations/${encodeURIComponent(machineId)}`)
    return response.data
  } catch (error) {
    console.error(`Failed to fetch degradation recommendations for machine ${machineId}:`, error)
    throw error
  }
}

/**
 * Export degradation model
 */
export async function exportDegradationModel(modelId: string, format: 'JSON' | 'XML' | 'CSV' = 'JSON'): Promise<Blob> {
  try {
    const response = await axiosClient.get(
      `/DegradationModeling/${encodeURIComponent(modelId)}/export`,
      {
        params: { format },
        responseType: 'blob'
      }
    )
    
    // Create download link
    const url = window.URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = `degradation-model-${modelId}.${format.toLowerCase()}`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(url)
    
    toast.success('Degradation model exported successfully')
    return response.data
  } catch (error) {
    console.error(`Failed to export degradation model ${modelId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to export degradation model'
    toast.error(message)
    throw error
  }
}

/**
 * Import degradation model
 */
export async function importDegradationModel(file: File, metadata?: any): Promise<DegradationModelDto> {
  try {
    const formData = new FormData()
    formData.append('file', file)
    if (metadata) {
      formData.append('metadata', JSON.stringify(metadata))
    }

    const response = await axiosClient.post('/DegradationModeling/import', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    toast.success('Degradation model imported successfully')
    return response.data
  } catch (error) {
    console.error('Failed to import degradation model:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to import degradation model'
    toast.error(message)
    throw error
  }
}

// Utility functions
export function getDegradationModelTypeDisplayName(type: DegradationModelType): string {
  const typeNames: Record<DegradationModelType, string> = {
    'MarkovChain': 'Markov Chain',
    'Weibull': 'Weibull Distribution',
    'Exponential': 'Exponential Decay',
    'Linear': 'Linear Degradation',
    'Polynomial': 'Polynomial Regression',
    'NeuralNetwork': 'Neural Network',
    'RandomForest': 'Random Forest',
    'LSTM': 'Long Short-Term Memory'
  }
  return typeNames[type] || type
}

export function getDegradationSeverityColor(severity: number): string {
  if (severity >= 0.8) return 'var(--color-error)'
  if (severity >= 0.6) return 'var(--color-warning)'
  if (severity >= 0.4) return 'var(--color-text-warning)'
  return 'var(--color-success)'
}

export function formatDegradationRate(rate: number): string {
  return `${(rate * 100).toFixed(2)}% per day`
}

export function calculateRemainingUsefulLife(currentHealth: number, degradationRate: number): number {
  if (degradationRate <= 0) return Infinity
  return Math.max(0, (currentHealth - 0.1) / degradationRate) // Assume failure at 10% health
}
