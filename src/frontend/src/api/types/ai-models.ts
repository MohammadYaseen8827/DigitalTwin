/**
 * AI Model type definitions matching backend DTOs.
 * Auto-generated from DigitalTwinPlatform.Application.ML.Models
 */

// AI Model Status Types
export type ModelStatus = 'draft' | 'training' | 'deployed' | 'archived'
export type ModelType = 'degradation' | 'failure' | 'performance' | 'quality'
export type Algorithm = 'random_forest' | 'neural_network' | 'svm' | 'xgboost' | 'lstm'
export type RiskLevel = 'low' | 'medium' | 'high'
export type DeploymentStatus = 'pending' | 'deploying' | 'deployed' | 'failed'

export interface AIModelDto {
  id: string
  name: string
  description: string
  version: string
  status: ModelStatus | string
  modelType: ModelType | string
  algorithm: Algorithm | string
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  trainingDataSize: number
  features: string[]
  deployedMachines: string[]
  tags: string[]
  createdAt: string
  updatedAt: string
  lastTraining: string
}

export interface ModelPredictionDto {
  remainingUsefulLife: number
  confidence: number
  riskLevel: RiskLevel | string
  featureImportance: Record<string, unknown>
  shapValues: Record<string, number>
  predictionTime: string
}

export interface ModelMetricsDto {
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  meanAbsoluteError: number
  rootMeanSquareError: number
  classMetrics: Record<string, number>
  lastEvaluated: string
}

export interface ModelInputDto {
  features: Record<string, number>
}

export interface ModelValidationResultDto {
  isValid: boolean
  message: string
  errors: string[]
  compatibility: ModelCompatibilityDto
}

export interface ModelCompatibilityDto {
  isCompatible: boolean
  compatibleMachineTypes: string[]
  requiredFeatures: string[]
  framework: string
}

export interface DeploymentStatusDto {
  status: DeploymentStatus | string
  totalMachines: number
  successfulDeployments: number
  failedDeployments: number
  machineStatuses: MachineDeploymentStatus[]
  lastUpdated: string
}

export interface MachineDeploymentStatus {
  machineId: string
  status: string // success, failed, pending
  errorMessage: string
  deployedAt: string
}

export interface TrainingResultDto {
  success: boolean
  samplesUsed: number
  r2Score: number
  mape: number
  accuracy: number
  modelPath: string
  modelType: string
  trainedAt: string
  message: string
  metrics: Record<string, number>
}

export interface ModelStatusDto {
  rulModelLoaded: boolean
  healthModelLoaded: boolean
  modelVersion: string
  lastUpdated: string
}

// Request DTOs
export interface DeployAIModelRequest {
  name: string
  description: string
  version: string
  modelType: ModelType | string
  algorithm: Algorithm | string
  modelFile?: File | null
  tags?: string[]
  trainingDataSize?: number
  features?: string[]
  metrics?: {
    accuracy?: number
    precision?: number
    recall?: number
    f1Score?: number
  }
}

export interface UpdateAIModelRequest {
  id: string
  name?: string
  description?: string
  status?: ModelStatus | string
  tags?: string[]
}

export interface RetrainModelRequest {
  forceRetrain?: boolean
  modelType?: string
  additionalData?: Record<string, unknown>
}

export interface PredictRequest {
  features: Record<string, number>
  modelVersion?: string
}

export interface DeployToMachinesRequest {
  machineIds: string[]
  deploymentConfig?: Record<string, unknown>
}

export interface TrainModelRequest {
  forceRetrain: boolean
  modelType: string
}
