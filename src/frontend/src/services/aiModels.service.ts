import axiosClient from '@/api/axiosClient'
import type { 
  AIModelDto, 
  ModelPredictionDto, 
  ModelMetricsDto, 
  ModelInputDto,
  ModelValidationResultDto,
  DeploymentStatusDto 
} from '@/api/types'

/**
 * AI Model Management Service
 * Handles all API interactions for AI/ML model management
 */

// Model CRUD Operations
export async function fetchAllAIModels(): Promise<AIModelDto[]> {
  const response = await axiosClient.get<AIModelDto[]>('AIModel')
  return response.data || []
}

export async function fetchAIModelById(id: string): Promise<AIModelDto> {
  const response = await axiosClient.get<AIModelDto>(`/AIModel/${encodeURIComponent(id)}`)
  return response.data
}

export async function deployAIModel(modelData: {
  name: string
  description: string
  version: string
  modelType: string
  algorithm: string
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  trainingDataSize: number
  features: string[]
  tags: string[]
  modelFile: ArrayBuffer
  targetMachines: string[]
}): Promise<AIModelDto> {
  const formData = new FormData()
  
  // Add model metadata
  formData.append('Name', modelData.name)
  formData.append('Description', modelData.description)
  formData.append('Version', modelData.version)
  formData.append('ModelType', modelData.modelType)
  formData.append('Algorithm', modelData.algorithm)
  formData.append('Accuracy', modelData.accuracy.toString())
  formData.append('Precision', modelData.precision.toString())
  formData.append('Recall', modelData.recall.toString())
  formData.append('F1Score', modelData.f1Score.toString())
  formData.append('TrainingDataSize', modelData.trainingDataSize.toString())
  
  // Add features as JSON array
  formData.append('Features', JSON.stringify(modelData.features))
  
  // Add tags as JSON array
  if (modelData.tags.length > 0) {
    formData.append('Tags', JSON.stringify(modelData.tags))
  }
  
  // Add target machines as JSON array
  if (modelData.targetMachines.length > 0) {
    formData.append('TargetMachines', JSON.stringify(modelData.targetMachines))
  }
  
  // Add model file
  const blob = new Blob([modelData.modelFile], { type: 'application/octet-stream' })
  formData.append('ModelFile', blob, `${modelData.name}_${modelData.version}.model`)
  
  const response = await axiosClient.post<AIModelDto>('AIModel', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
  
  return response.data
}

export async function updateAIModel(
  id: string, 
  updates: Partial<{
    name: string
    description: string
    version: string
    status: string
    accuracy: number
    precision: number
    recall: number
    f1Score: number
    trainingDataSize: number
    features: string[]
    tags: string[]
    modelFile: ArrayBuffer
  }>
): Promise<AIModelDto> {
  const formData = new FormData()
  
  // Add non-null updates
  Object.entries(updates).forEach(([key, value]) => {
    if (value !== undefined && value !== null) {
      if (key === 'features' || key === 'tags') {
        formData.append(key.charAt(0).toUpperCase() + key.slice(1), JSON.stringify(value))
      } else if (key === 'modelFile' && value instanceof ArrayBuffer) {
        const blob = new Blob([value], { type: 'application/octet-stream' })
        formData.append('ModelFile', blob, 'updated_model.model')
      } else {
        formData.append(key.charAt(0).toUpperCase() + key.slice(1), value.toString())
      }
    }
  })
  
  const response = await axiosClient.put<AIModelDto>(
    `/AIModel/${encodeURIComponent(id)}`, 
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }
  )
  
  return response.data
}

export async function deleteAIModel(id: string): Promise<void> {
  await axiosClient.delete(`/AIModel/${encodeURIComponent(id)}`)
}

// Model Operations
export async function predictWithModel(
  modelId: string, 
  input: ModelInputDto
): Promise<ModelPredictionDto> {
  const response = await axiosClient.post<ModelPredictionDto>(
    `/AIModel/${encodeURIComponent(modelId)}/predict`,
    input
  )
  return response.data
}

export async function getModelMetrics(modelId: string): Promise<ModelMetricsDto> {
  const response = await axiosClient.get<ModelMetricsDto>(
    `/AIModel/${encodeURIComponent(modelId)}/metrics`
  )
  return response.data
}

export async function retrainModel(
  modelId: string,
  trainingData: string[],
  hyperparameters?: Record<string, any>
): Promise<AIModelDto> {
  const requestData = {
    ModelId: modelId,
    NewTrainingData: trainingData,
    Hyperparameters: hyperparameters
  }
  
  const response = await axiosClient.post<AIModelDto>(
    `/AIModel/${encodeURIComponent(modelId)}/retrain`,
    requestData
  )
  return response.data
}

// Deployment Operations
export async function getCompatibleMachines(modelId: string): Promise<string[]> {
  const response = await axiosClient.get<string[]>(
    `/AIModel/${encodeURIComponent(modelId)}/compatible-machines`
  )
  return response.data
}

export async function deployModelToMachines(
  modelId: string,
  machineIds: string[]
): Promise<void> {
  await axiosClient.post(
    `/AIModel/${encodeURIComponent(modelId)}/deploy-to-machines`,
    { MachineIds: machineIds }
  )
}

export async function getModelDeploymentStatus(modelId: string): Promise<DeploymentStatusDto> {
  const response = await axiosClient.get<DeploymentStatusDto>(
    `/AIModel/${encodeURIComponent(modelId)}/deployment-status`
  )
  return response.data
}

// Model Validation
export async function validateModel(
  modelFile: ArrayBuffer,
  modelType: string
): Promise<ModelValidationResultDto> {
  const formData = new FormData()
  const blob = new Blob([modelFile], { type: 'application/octet-stream' })
  formData.append('ModelFile', blob)
  formData.append('ModelType', modelType)
  
  const response = await axiosClient.post<ModelValidationResultDto>(
    '/AIModel/validate-model',
    formData,
    {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }
  )
  
  return response.data
}

// Utility functions
export function getModelStatusColor(status: string): string {
  const statusColors: Record<string, string> = {
    'draft': 'var(--color-text-secondary)',
    'training': 'var(--color-warning)',
    'deployed': 'var(--color-success)',
    'archived': 'var(--color-text-secondary)'
  }
  return statusColors[status.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getModelTypeDisplayName(type: string): string {
  const typeNames: Record<string, string> = {
    'degradation': 'Degradation Prediction',
    'failure': 'Failure Detection',
    'performance': 'Performance Optimization',
    'quality': 'Quality Control'
  }
  return typeNames[type.toLowerCase()] || type
}

export function getAlgorithmDisplayName(algorithm: string): string {
  const algorithmNames: Record<string, string> = {
    'random_forest': 'Random Forest',
    'neural_network': 'Neural Network',
    'svm': 'Support Vector Machine',
    'xgboost': 'XGBoost',
    'lstm': 'LSTM'
  }
  return algorithmNames[algorithm.toLowerCase()] || algorithm
}

export function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}