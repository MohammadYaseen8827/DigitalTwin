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
  private baseUrl = '/api/benchmarkvalidation'

  async getAvailableDatasets(): Promise<BenchmarkDataset[]> {
    try {
      const response = await axiosClient.get<BenchmarkDataset[]>(`/api/benchmark/benchmarks`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch benchmark datasets:', error)
      throw error
    }
  }

  async getDatasetsFromValidationService(): Promise<BenchmarkDataset[]> {
    try {
      const response = await axiosClient.get<BenchmarkDataset[]>(`${this.baseUrl}/datasets`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch benchmark datasets:', error)
      throw error
    }
  }

  async getDatasetById(id: string): Promise<BenchmarkDataset> {
    try {
      const response = await axiosClient.get<BenchmarkDataset>(`${this.baseUrl}/datasets/${id}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch dataset ${id}:`, error)
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

  async getValidationResults(modelId?: string, datasetId?: string): Promise<ValidationResult[]> {
    try {
      const params = new URLSearchParams()
      if (modelId) params.append('modelId', modelId)
      if (datasetId) params.append('datasetId', datasetId)
      
      const response = await axiosClient.get<ValidationResult[]>(`${this.baseUrl}/results?${params.toString()}`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch validation results:', error)
      throw error
    }
  }

  async getValidationResultById(id: string): Promise<ValidationResult> {
    try {
      const response = await axiosClient.get<ValidationResult>(`${this.baseUrl}/results/${id}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch validation result ${id}:`, error)
      throw error
    }
  }

  async compareModels(modelIds: string[], datasetId: string): Promise<BenchmarkComparison[]> {
    try {
      const response = await axiosClient.post<BenchmarkComparison[]>(`${this.baseUrl}/compare`, {
        modelIds,
        datasetId
      })
      return response.data
    } catch (error) {
      console.error('Failed to compare models:', error)
      throw error
    }
  }

  async generateValidationReport(modelIds: string[], datasetIds: string[]): Promise<ValidationReport> {
    try {
      const response = await axiosClient.post<ValidationReport>(`${this.baseUrl}/reports`, {
        modelIds,
        datasetIds
      })
      return response.data
    } catch (error) {
      console.error('Failed to generate validation report:', error)
      throw error
    }
  }

  async getValidationReports(): Promise<ValidationReport[]> {
    try {
      const response = await axiosClient.get<ValidationReport[]>(`${this.baseUrl}/reports`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch validation reports:', error)
      throw error
    }
  }

  async getValidationReportById(id: string): Promise<ValidationReport> {
    try {
      const response = await axiosClient.get<ValidationReport>(`${this.baseUrl}/reports/${id}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch validation report ${id}:`, error)
      throw error
    }
  }

  async deleteValidationReport(id: string): Promise<void> {
    try {
      await axiosClient.delete(`${this.baseUrl}/reports/${id}`)
    } catch (error) {
      console.error(`Failed to delete validation report ${id}:`, error)
      throw error
    }
  }

  async uploadBenchmarkDataset(formData: FormData): Promise<BenchmarkDataset> {
    try {
      const response = await axiosClient.post<BenchmarkDataset>(`${this.baseUrl}/datasets/upload`, formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      })
      return response.data
    } catch (error) {
      console.error('Failed to upload benchmark dataset:', error)
      throw error
    }
  }

  async deleteBenchmarkDataset(id: string): Promise<void> {
    try {
      await axiosClient.delete(`${this.baseUrl}/datasets/${id}`)
    } catch (error) {
      console.error(`Failed to delete benchmark dataset ${id}:`, error)
      throw error
    }
  }

  async getValidationHistory(modelId: string, limit: number = 50): Promise<ValidationResult[]> {
    try {
      const response = await axiosClient.get<ValidationResult[]>(`${this.baseUrl}/history/${modelId}?limit=${limit}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch validation history for model ${modelId}:`, error)
      throw error
    }
  }

  async cancelValidation(validationId: string): Promise<void> {
    try {
      await axiosClient.post(`${this.baseUrl}/results/${validationId}/cancel`)
    } catch (error) {
      console.error(`Failed to cancel validation ${validationId}:`, error)
      throw error
    }
  }

  async retryValidation(validationId: string): Promise<ValidationResult> {
    try {
      const response = await axiosClient.post<ValidationResult>(`${this.baseUrl}/results/${validationId}/retry`)
      return response.data
    } catch (error) {
      console.error(`Failed to retry validation ${validationId}:`, error)
      throw error
    }
  }
}

// Create singleton instance
export const benchmarkValidationService = new BenchmarkValidationService()