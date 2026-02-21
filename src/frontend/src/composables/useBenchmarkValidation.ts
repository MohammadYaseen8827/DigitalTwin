import { ref, computed } from 'vue'
import {
  benchmarkValidationService,
  type BenchmarkDataset,
  type ValidationRequest,
  type ValidationResult,
  type ValidationReport
} from '@/services/benchmarkValidation.service'

// Reactive state
const datasets = ref<BenchmarkDataset[]>([])
const results = ref<ValidationResult[]>([])
const reports = ref<ValidationReport[]>([])
const loading = ref(false)
const error = ref<string | null>(null)

export function useBenchmarkValidation() {
  const fetchDatasets = async () => {
    loading.value = true
    error.value = null
    try {
      datasets.value = await benchmarkValidationService.getAvailableDatasets()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch datasets'
      console.error(error.value)
    } finally {
      loading.value = false
    }
  }

  const fetchResults = async () => {
    // NOTE: This is a placeholder - the backend endpoint doesn't exist yet
    // When implemented, replace with actual service call
    results.value = []
    console.warn('fetchResults is not implemented - backend endpoint unavailable')
  }

  const fetchReports = async () => {
    // NOTE: This is a placeholder - the backend endpoint doesn't exist yet
    reports.value = []
    console.warn('fetchReports is not implemented - backend endpoint unavailable')
  }

  const validateModel = async (request: ValidationRequest): Promise<ValidationResult | null> => {
    loading.value = true
    error.value = null
    try {
      const result = await benchmarkValidationService.validateModel(request)
      results.value.unshift(result)
      return result
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Validation failed'
      console.error(error.value)
      return null
    } finally {
      loading.value = false
    }
  }

  const compareModels = async (_modelIds: string[], _datasetId: string) => {
    // NOTE: This is a placeholder - the backend endpoint doesn't exist yet
    console.warn('compareModels is not implemented - backend endpoint unavailable')
    return null
  }

  const generateReport = async (_modelIds: string[], _datasetIds: string[]): Promise<ValidationReport | null> => {
    // NOTE: This is a placeholder - the backend endpoint doesn't exist yet
    console.warn('generateReport is not implemented - backend endpoint unavailable')
    return null
  }

  return {
    // State
    datasets: computed(() => datasets.value),
    results: computed(() => results.value),
    reports: computed(() => reports.value),
    loading: computed(() => loading.value),
    error: computed(() => error.value),
    
    // Actions
    fetchDatasets,
    fetchResults,
    fetchReports,
    validateModel,
    compareModels,
    generateReport
  }
}
