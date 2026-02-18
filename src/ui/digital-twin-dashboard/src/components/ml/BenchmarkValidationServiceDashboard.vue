<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseTextarea from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { useBenchmarkValidation } from '@/services/benchmarkValidation.service'
import { 
  Target, 
  Play, 
  FileText, 
  BarChart3, 
  Calendar, 
  Upload, 
  Download,
  CheckCircle,
  XCircle,
  AlertTriangle,
  RotateCw
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
  LineChart as EChartsLine,
  PieChart as EChartsPie
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsBar,
  EChartsLine,
  EChartsPie,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()
const {
  datasets,
  results,
  reports,
  loading,
  error,
  fetchDatasets,
  fetchResults,
  fetchReports,
  validateModel,
  compareModels,
  generateReport
} = useBenchmarkValidation()

// State
const validating = ref(false)
const comparing = ref(false)
const generatingReport = ref(false)
const selectedDataset = ref('')
const modelId = ref('')
const validationType = ref('accuracy')
const selectedModels = ref<string[]>([])
const selectedReportModels = ref<string[]>([])
const selectedReportDatasets = ref<string[]>([])

// Validation types
const validationTypes = [
  { value: 'accuracy', label: 'Accuracy Validation' },
  { value: 'robustness', label: 'Robustness Testing' },
  { value: 'fairness', label: 'Fairness Assessment' },
  { value: 'completeness', label: 'Completeness Check' }
]

// Computed
const isValidForm = computed(() => {
  return selectedDataset.value && modelId.value
})

const overallPassRate = computed(() => {
  if (results.value.length === 0) return 0
  const passed = results.value.filter(r => r.passed).length
  return Math.round((passed / results.value.length) * 100)
})

const recentResults = computed(() => {
  return results.value.slice(0, 10)
})

// Methods
const handleValidate = async () => {
  if (!isValidForm.value) {
    toast.error('Please select a dataset and enter a model ID')
    return
  }

  try {
    validating.value = true
    const request = {
      modelId: modelId.value,
      datasetId: selectedDataset.value,
      validationType: validationType.value as any
    }
    
    const result = await validateModel(request)
    toast.success(`Validation completed: ${result.passed ? 'PASSED' : 'FAILED'} (${result.score?.toFixed(2)})`)
    
  } catch (error) {
    console.error('Validation failed:', error)
    toast.error('Model validation failed')
  } finally {
    validating.value = false
  }
}

const handleCompare = async () => {
  if (selectedModels.value.length < 2) {
    toast.error('Please select at least 2 models to compare')
    return
  }

  try {
    comparing.value = true
    const comparison = await compareModels(selectedModels.value, selectedDataset.value)
    
    toast.success(`Compared ${comparison.length} models successfully`)
    // In a real implementation, you'd display the comparison results
    
  } catch (error) {
    console.error('Comparison failed:', error)
    toast.error('Model comparison failed')
  } finally {
    comparing.value = false
  }
}

const handleGenerateReport = async () => {
  if (selectedReportModels.value.length === 0 || selectedReportDatasets.value.length === 0) {
    toast.error('Please select models and datasets for the report')
    return
  }

  try {
    generatingReport.value = true
    const report = await generateReport(selectedReportModels.value, selectedReportDatasets.value)
    
    toast.success(`Report generated: ${report.title}`)
    // In a real implementation, you'd provide download functionality
    
  } catch (error) {
    console.error('Report generation failed:', error)
    toast.error('Failed to generate validation report')
  } finally {
    generatingReport.value = false
  }
}

const toggleModelSelection = (modelId: string) => {
  const index = selectedModels.value.indexOf(modelId)
  if (index > -1) {
    selectedModels.value.splice(index, 1)
  } else {
    selectedModels.value.push(modelId)
  }
}

const toggleReportModelSelection = (modelId: string) => {
  const index = selectedReportModels.value.indexOf(modelId)
  if (index > -1) {
    selectedReportModels.value.splice(index, 1)
  } else {
    selectedReportModels.value.push(modelId)
  }
}

const toggleReportDatasetSelection = (datasetId: string) => {
  const index = selectedReportDatasets.value.indexOf(datasetId)
  if (index > -1) {
    selectedReportDatasets.value.splice(index, 1)
  } else {
    selectedReportDatasets.value.push(datasetId)
  }
}

const refreshAll = async () => {
  await Promise.all([
    fetchDatasets(),
    fetchResults(),
    fetchReports()
  ])
  toast.success('All data refreshed')
}

// Lifecycle
onMounted(async () => {
  await refreshAll()
})
</script>

<template>
  <BaseCard>
    <div class="benchmark-validation-dashboard space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Benchmark Validation</h1>
          <p class="text-gray-600 mt-2">Validate and compare ML models against industry benchmarks</p>
        </div>
        <BaseButton variant="outline" @click="refreshAll" :disabled="loading">
          <RotateCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
          Refresh Data
        </BaseButton>
      </div>

      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard class="p-4">
          <div class="flex items-center">
            <Target class="w-8 h-8 text-blue-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Available Datasets</p>
              <p class="text-2xl font-bold">{{ datasets.length }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <BarChart3 class="w-8 h-8 text-green-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Validation Results</p>
              <p class="text-2xl font-bold">{{ results.length }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <FileText class="w-8 h-8 text-purple-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Generated Reports</p>
              <p class="text-2xl font-bold">{{ reports.length }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <div class="w-8 h-8 bg-green-100 rounded-full flex items-center justify-center mr-3">
              <span class="text-green-600 font-bold text-sm">{{ overallPassRate }}%</span>
            </div>
            <div>
              <p class="text-sm text-gray-600">Pass Rate</p>
              <p class="text-2xl font-bold">{{ overallPassRate }}%</p>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Main Content Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Model Validation -->
        <BaseCard class="p-6">
          <h3 class="text-xl font-semibold mb-4 flex items-center gap-2">
            <Target class="w-5 h-5 text-blue-500" />
            Model Validation
          </h3>
          
          <div class="space-y-4">
            <BaseSelect
              v-model="selectedDataset"
              label="Benchmark Dataset"
              :options="datasets.map(d => ({ value: d.id, label: d.name }))"
              placeholder="Select benchmark dataset"
            />
            
            <BaseInput
              v-model="modelId"
              label="Model ID"
              placeholder="Enter model identifier"
            />
            
            <BaseSelect
              v-model="validationType"
              label="Validation Type"
              :options="validationTypes"
            />
            
            <BaseButton 
              variant="primary" 
              class="w-full"
              @click="handleValidate"
              :disabled="validating || !isValidForm"
            >
              <Play v-if="!validating" class="w-4 h-4 mr-2" />
              <RotateCw v-else class="w-4 h-4 mr-2 animate-spin" />
              {{ validating ? 'Validating...' : 'Validate Model' }}
            </BaseButton>
          </div>
        </BaseCard>

        <!-- Model Comparison -->
        <BaseCard class="p-6">
          <h3 class="text-xl font-semibold mb-4 flex items-center gap-2">
            <BarChart3 class="w-5 h-5 text-green-500" />
            Model Comparison
          </h3>
          
          <div class="space-y-4">
            <BaseSelect
              v-model="selectedDataset"
              label="Reference Dataset"
              :options="datasets.map(d => ({ value: d.id, label: d.name }))"
              placeholder="Select reference dataset"
            />
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">Select Models to Compare</label>
              <div class="max-h-32 overflow-y-auto border rounded-md p-2 space-y-1">
                <div 
                  v-for="result in results" 
                  :key="result.modelId"
                  class="flex items-center p-2 rounded hover:bg-gray-50 cursor-pointer"
                  :class="{ 'bg-blue-50': selectedModels.includes(result.modelId) }"
                  @click="toggleModelSelection(result.modelId)"
                >
                  <input 
                    type="checkbox" 
                    :checked="selectedModels.includes(result.modelId)"
                    class="mr-2 rounded"
                    @click.stop
                  />
                  <span class="text-sm">{{ result.modelId.substring(0, 8) }}... ({{ result.validationType }})</span>
                </div>
              </div>
            </div>
            
            <BaseButton 
              variant="primary" 
              class="w-full"
              @click="handleCompare"
              :disabled="comparing || selectedModels.length < 2"
            >
              <BarChart3 v-if="!comparing" class="w-4 h-4 mr-2" />
              <RotateCw v-else class="w-4 h-4 mr-2 animate-spin" />
              {{ comparing ? 'Comparing...' : `Compare ${selectedModels.length} Models` }}
            </BaseButton>
          </div>
        </BaseCard>
      </div>

      <!-- Report Generation -->
      <BaseCard class="p-6">
        <h3 class="text-xl font-semibold mb-4 flex items-center gap-2">
          <FileText class="w-5 h-5 text-purple-500" />
          Validation Report Generator
        </h3>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Select Models</label>
            <div class="max-h-40 overflow-y-auto border rounded-md p-2 space-y-1">
              <div 
                v-for="result in results" 
                :key="`report-${result.modelId}`"
                class="flex items-center p-2 rounded hover:bg-gray-50 cursor-pointer"
                :class="{ 'bg-purple-50': selectedReportModels.includes(result.modelId) }"
                @click="toggleReportModelSelection(result.modelId)"
              >
                <input 
                  type="checkbox" 
                  :checked="selectedReportModels.includes(result.modelId)"
                  class="mr-2 rounded"
                  @click.stop
                />
                <span class="text-sm">{{ result.modelId.substring(0, 12) }}...</span>
              </div>
            </div>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">Select Datasets</label>
            <div class="max-h-40 overflow-y-auto border rounded-md p-2 space-y-1">
              <div 
                v-for="dataset in datasets" 
                :key="`report-${dataset.id}`"
                class="flex items-center p-2 rounded hover:bg-gray-50 cursor-pointer"
                :class="{ 'bg-purple-50': selectedReportDatasets.includes(dataset.id) }"
                @click="toggleReportDatasetSelection(dataset.id)"
              >
                <input 
                  type="checkbox" 
                  :checked="selectedReportDatasets.includes(dataset.id)"
                  class="mr-2 rounded"
                  @click.stop
                />
                <span class="text-sm">{{ dataset.name }}</span>
              </div>
            </div>
          </div>
        </div>
        
        <BaseButton 
          variant="primary" 
          class="w-full mt-4"
          @click="handleGenerateReport"
          :disabled="generatingReport || selectedReportModels.length === 0 || selectedReportDatasets.length === 0"
        >
          <FileText v-if="!generatingReport" class="w-4 h-4 mr-2" />
          <RotateCw v-else class="w-4 h-4 mr-2 animate-spin" />
          {{ generatingReport ? 'Generating Report...' : `Generate Report (${selectedReportModels.length} models, ${selectedReportDatasets.length} datasets)` }}
        </BaseButton>
      </BaseCard>

      <!-- Recent Results -->
      <BaseCard class="p-6">
        <h3 class="text-xl font-semibold mb-4">Recent Validation Results</h3>
        <div v-if="recentResults.length === 0" class="text-center py-8 text-gray-500">
          No validation results available
        </div>
        <div v-else class="space-y-3">
          <div 
            v-for="result in recentResults" 
            :key="result.id"
            class="flex items-center justify-between p-3 rounded-lg border"
            :class="result.passed ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'"
          >
            <div class="flex items-center gap-3">
              <component :is="result.passed ? CheckCircle : XCircle" class="w-5 h-5" :class="result.passed ? 'text-green-500' : 'text-red-500'" />
              <div>
                <p class="font-medium">{{ result.modelId.substring(0, 12) }}...</p>
                <p class="text-sm text-gray-600">{{ result.validationType }} validation</p>
              </div>
            </div>
            <div class="text-right">
              <p class="font-medium" :class="result.passed ? 'text-green-600' : 'text-red-600'">
                {{ result.passed ? 'PASSED' : 'FAILED' }}
              </p>
              <p class="text-sm text-gray-500">
                Score: {{ result.score?.toFixed(3) || 'N/A' }}
              </p>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.benchmark-validation-dashboard {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}
</style>