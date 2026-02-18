import axiosClient from '@/api/axiosClient'

export interface SyntheticDataGenerationResult {
  trajectories: Trajectory[]
  statistics: GenerationStatistics
  validationReport?: DataValidationReport
}

export interface Trajectory {
  id: string
  machineType: string
  sensorData: SensorReading[]
  timestamps: string[]
  healthIndicators: HealthIndicator[]
}

export interface SensorReading {
  sensorId: string
  values: number[]
  timestamps: string[]
  unit: string
}

export interface HealthIndicator {
  name: string
  values: number[]
  timestamps: string[]
  description: string
}

export interface GenerationStatistics {
  totalTrajectories: number
  averageTrajectoryLength: number
  dataPointsGenerated: number
  sensorsCovered: string[]
  timeRange: string
  generationTimeMs: number
  dataQualityScore: number
}

export interface DataValidationReport {
  isValid: boolean
  qualityMetrics: QualityMetric[]
  deviations: Deviation[]
  recommendations: string[]
}

export interface QualityMetric {
  name: string
  value: number
  threshold: number
  passed: boolean
}

export interface Deviation {
  field: string
  actualValue: any
  expectedValue: any
  severity: 'low' | 'medium' | 'high'
}

export interface GenerateDataRequest {
  machineType: string
  numberOfTrajectories: number
  timeRange?: string
  randomSeed?: number
}

export interface ValidateDataRequest {
  telemetryData: any[]
  benchmarkDataset: string
}

export interface GetStatisticsRequest {
  machineType: string
  numberOfTrajectories: number
  timeRange?: string
  randomSeed?: number
}

/**
 * Generate synthetic telemetry data
 */
export async function generateSyntheticData(request: GenerateDataRequest): Promise<SyntheticDataGenerationResult> {
  const response = await axiosClient.post<SyntheticDataGenerationResult>('/SyntheticData/generate', request)
  return response.data
}

/**
 * Validate synthetic data against benchmarks
 */
export async function validateSyntheticData(request: ValidateDataRequest): Promise<DataValidationReport> {
  const response = await axiosClient.post<DataValidationReport>('/SyntheticData/validate', request)
  return response.data
}

/**
 * Get generation statistics
 */
export async function getGenerationStatistics(request: GetStatisticsRequest): Promise<GenerationStatistics> {
  const response = await axiosClient.post<GenerationStatistics>('/SyntheticData/statistics', request)
  return response.data
}

/**
 * Quick generate with default parameters
 */
export async function quickGenerate(machineType: string): Promise<SyntheticDataGenerationResult> {
  const request: GenerateDataRequest = {
    machineType,
    numberOfTrajectories: 5,
    timeRange: 'P1D' // 1 day
  }
  return generateSyntheticData(request)
}