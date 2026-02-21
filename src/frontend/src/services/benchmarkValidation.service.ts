import axiosClient from '@/api/axiosClient'

// Define TypeScript interfaces
export interface BenchmarkDataset {
  id: string
  name: string
  description: string
  source: string
  size: number
  metrics: string[]
  createdAt: string
  updatedAt: string
}

export interface ValidationRequest {
  modelId: string
  datasetId: string
  validationType: 'accuracy' | 'robustness' | 'fairness' | 'completeness'
  parameters?: Record<string, any>
}

export interface ValidationResult {
  id: string
  modelId: string
  datasetId: string
  validationType: string
  status: 'pending' | 'running' | 'completed' | 'failed'
  results: Record<string, any>
  score?: number
  passed: boolean
  errors: string[]
  durationMs: number
  createdAt: string
  completedAt?: string
}

export interface BenchmarkComparison {
  modelId: string
  modelName: string
  datasetId: string
  scores: Record<string, number>
  rank: number
  comparedAt: string
}

export interface ValidationReport {
  id: string
  title: string
  modelIds: string[]
  datasetIds: string[]
  results: ValidationResult[]
  summary: {
    totalValidations: number
    passedValidations: number
    failedValidations: number
    averageScore: number
  }
  createdAt: string
  expiresAt: string
}

// Benchmark Validation Service Class
class BenchmarkValidationService {
  private baseUrl = '/BenchmarkValidation'

  async getAvailableDatasets(): Promise<BenchmarkDataset[]> {
    try {
      const response = await axiosClient.get<BenchmarkDataset[]>(`${this.baseUrl}/benchmarks`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch benchmark datasets:', error)
      throw error
    }
  }

  async getDatasetByName(datasetName: string): Promise<BenchmarkDataset> {
    try {
      const response = await axiosClient.get<BenchmarkDataset>(`${this.baseUrl}/benchmarks/${encodeURIComponent(datasetName)}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch dataset ${datasetName}:`, error)
      throw error
    }
  }

  async validateModel(request: ValidationRequest): Promise<ValidationResult> {
    try {
      const response = await axiosClient.post<ValidationResult>(`${this.baseUrl}/validate`, request)
      return response.data
    } catch (error) {
      console.error('Failed to validate model:', error)
      throw error
    }
  }

  // NOTE: The following 12 methods are removed as they have no corresponding backend endpoints
  // - getValidationResults
  // - getValidationResultById
  // - compareModels
  // - generateValidationReport
  // - getValidationReports
  // - getValidationReportById
  // - deleteValidationReport
  // - uploadBenchmarkDataset
  // - deleteBenchmarkDataset
  // - getValidationHistory
  // - cancelValidation
  // - retryValidation
  // These can be implemented in future phases when benchmark validation endpoints are available
}

// Create singleton instance
export const benchmarkValidationService = new BenchmarkValidationService()