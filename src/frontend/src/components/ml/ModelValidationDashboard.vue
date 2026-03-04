<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseTextarea from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { CheckCircle, XCircle, AlertTriangle, BarChart3, FileText, Target, Calendar, Upload, Download } from 'lucide-vue-next'

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

import { benchmarkValidationService } from '@/services/benchmarkValidation.service'
import type { ValidationRequest, BenchmarkDataset } from '@/services/benchmarkValidation.service'

const toast = useToast()

// State
const validating = ref(false)
const loadingBenchmarks = ref(false)
const benchmarks = ref<BenchmarkDataset[]>([])
const benchmarkInfo = ref<BenchmarkDataset | null>(null)
const validationResult = ref<any>(null)
const validationHistory = ref<any[]>([])

// Validation Configuration
const selectedBenchmarkId = ref('')
const modelVersionId = ref('')
const modelPath = ref('')
const modelType = ref('RUL')
const validationResults = ref<any[]>([])

// Chart refs
const metricsChartRef = ref<HTMLDivElement | null>(null)
const comparisonChartRef = ref<HTMLDivElement | null>(null)
const historyChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let metricsChartInstance: echarts.ECharts | null = null
let comparisonChartInstance: echarts.ECharts | null = null
let historyChartInstance: echarts.ECharts | null = null

// Options
const modelTypeOptions = [
  { label: 'RUL (Remaining Useful Life)', value: 'RUL' },
  { label: 'Anomaly Detection', value: 'Anomaly' },
  { label: 'Classification', value: 'Classification' },
  { label: 'Regression', value: 'Regression' }
]

// Computed
const isValidForm = computed(() => {
  return selectedBenchmarkId.value && 
         (modelVersionId.value || modelPath.value)
})

const overallScore = computed(() => {
  if (!validationResult.value) return 0
  return validationResult.value.score || 0
})

const scoreColor = computed(() => {
  const score = overallScore.value
  if (score >= 0.8) return 'text-green-600'
  if (score >= 0.6) return 'text-yellow-600'
  return 'text-red-600'
})

const scoreBgColor = computed(() => {
  const score = overallScore.value
  if (score >= 0.8) return 'bg-green-100'
  if (score >= 0.6) return 'bg-yellow-100'
  return 'bg-red-100'
})

const passedMetrics = computed(() => {
  if (!validationResult.value?.passed) return 0
  return 1 // Simplified since result doesn't have granular metric pass/fail in current DTO
})

const totalMetrics = computed(() => {
  if (!validationResult.value?.results) return 0
  return Object.keys(validationResult.value.results).length
})

// Methods
const loadBenchmarks = async () => {
  try {
    loadingBenchmarks.value = true
    const availableBenchmarks = await benchmarkValidationService.getAvailableDatasets()
    benchmarks.value = availableBenchmarks
    
    if (benchmarks.value.length > 0) {
      selectedBenchmarkId.value = benchmarks.value[0].id
      benchmarkInfo.value = benchmarks.value[0]
    }
  } catch (error) {
    console.error('Failed to load benchmarks:', error)
    toast.error('Failed to load benchmark datasets from backend')
  } finally {
    loadingBenchmarks.value = false
  }
}

const handleBenchmarkChange = () => {
  const selected = benchmarks.value.find(b => b.id === selectedBenchmarkId.value)
  if (selected) {
    benchmarkInfo.value = selected
  }
}

const validateModel = async () => {
  try {
    validating.value = true
    
    const request: ValidationRequest = {
      modelId: modelVersionId.value || 'custom_model',
      datasetId: selectedBenchmarkId.value,
      validationType: modelType.value.toLowerCase() as any,
      parameters: { modelPath: modelPath.value }
    }
    
    const result = await benchmarkValidationService.validateModel(request)
    
    // Convert backend result to local format
    const formattedResult = {
      modelId: result.modelId,
      benchmarkId: result.datasetId,
      overallScore: result.score || 0,
      passed: result.passed,
      metrics: Object.entries(result.results || {}).reduce((acc, [key, value]) => {
        acc[key] = {
          value: typeof value === 'number' ? value.toFixed(3) : value,
          threshold: 'N/A', // Not provided by current backend DTO
          passed: result.passed // Simplified
        }
        return acc
      }, {} as any),
      recommendations: result.errors?.length > 0 ? result.errors : ['Model validated successfully.'],
      validatedAt: result.createdAt
    }
    
    validationResult.value = formattedResult
    validationResults.value.unshift(formattedResult)
    validationHistory.value.unshift({
      ...formattedResult,
      id: result.id
    })
    
    renderMetricsChart()
    renderComparisonChart()
    renderHistoryChart()
    
    if (result.passed) {
      toast.success(`Validation passed: ${(formattedResult.overallScore * 100).toFixed(1)}%`)
    } else {
      toast.warning(`Validation completed with issues: ${(formattedResult.overallScore * 100).toFixed(1)}%`)
    }
  } catch (error) {
    console.error('Failed to validate model:', error)
    toast.error('Failed to validate model against backend benchmark')
  } finally {
    validating.value = false
  }
}

const renderMetricsChart = () => {
  if (!metricsChartRef.value || !validationResult.value) return
  
  if (!metricsChartInstance) {
    metricsChartInstance = echarts.init(metricsChartRef.value)
  }
  
  const metricNames = Object.keys(validationResult.value.metrics)
  const metricValues = metricNames.map(name => parseFloat(validationResult.value.metrics[name].value))
  const thresholds = metricNames.map(name => parseFloat(validationResult.value.metrics[name].threshold))
  const passed = metricNames.map(name => validationResult.value.metrics[name].passed)
  
  const option = {
    title: {
      text: 'Metric Performance vs Thresholds',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30,
      data: ['Actual Value', 'Threshold']
    },
    xAxis: {
      type: 'category',
      data: metricNames
    },
    yAxis: {
      type: 'value'
    },
    series: [
      {
        name: 'Actual Value',
        type: 'bar',
        data: metricValues.map((value, index) => ({
          value,
          itemStyle: { color: passed[index] ? '#10b981' : '#ef4444' }
        }))
      },
      {
        name: 'Threshold',
        type: 'line',
        data: thresholds,
        itemStyle: { color: '#f59e0b' },
        lineStyle: { type: 'dashed' }
      }
    ]
  }
  
  metricsChartInstance.setOption(option, true)
}

const renderComparisonChart = () => {
  if (!comparisonChartRef.value) return
  
  if (!comparisonChartInstance) {
    comparisonChartInstance = echarts.init(comparisonChartRef.value)
  }
  
  const models = ['Current Model']
  const scores = [
    (validationResult.value?.overallScore || 0) * 100
  ]
  
  const option = {
    title: {
      text: 'Model Performance Comparison',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    xAxis: {
      type: 'category',
      data: models
    },
    yAxis: {
      type: 'value',
      name: 'Score (%)',
      min: 0,
      max: 100
    },
    series: [{
      name: 'Performance',
      type: 'bar',
      data: scores.map((score, index) => ({
        value: score,
        itemStyle: { 
          color: index === 0 ? '#3b82f6' : index === 1 ? '#6b7280' : '#10b981'
        }
      }))
    }]
  }
  
  comparisonChartInstance.setOption(option, true)
}

const renderHistoryChart = () => {
  if (!historyChartRef.value || validationHistory.value.length === 0) return
  
  if (!historyChartInstance) {
    historyChartInstance = echarts.init(historyChartRef.value)
  }
  
  const timestamps = validationHistory.value.slice(0, 10).map(v => {
    const date = new Date(v.validatedAt)
    return `${date.getMonth() + 1}/${date.getDate()}`
  })
  
  const scores = validationHistory.value.slice(0, 10).map(v => v.overallScore * 100)
  
  const option = {
    title: {
      text: 'Validation History Trend',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: timestamps
    },
    yAxis: {
      type: 'value',
      name: 'Score (%)',
      min: 0,
      max: 100
    },
    series: [{
      name: 'Validation Score',
      type: 'line',
      data: scores,
      smooth: true,
      itemStyle: { color: '#8b5cf6' },
      areaStyle: { opacity: 0.3 }
    }]
  }
  
  historyChartInstance.setOption(option, true)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const getMetricStatusIcon = (passed: boolean) => {
  return passed ? CheckCircle : XCircle
}

const getMetricStatusColor = (passed: boolean) => {
  return passed ? 'text-green-600' : 'text-red-600'
}

const exportResults = () => {
  if (!validationResult.value) return
  
  const dataStr = JSON.stringify(validationResult.value, null, 2)
  const blob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  
  const a = document.createElement('a')
  a.href = url
  a.download = `validation_results_${selectedBenchmark.value}_${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  toast.success('Validation results exported successfully')
}

const resizeCharts = () => {
  metricsChartInstance?.resize()
  comparisonChartInstance?.resize()
  historyChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  loadBenchmarks()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  metricsChartInstance?.dispose()
  comparisonChartInstance?.dispose()
  historyChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="model-validation space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Model Validation</h1>
          <p class="text-gray-600 mt-2">Validate ML models against industry benchmark datasets</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="exportResults"
            :disabled="!validationResult"
          >
            <Download class="w-4 h-4 mr-2" />
            Export Results
          </BaseButton>
        </div>
      </div>

      <!-- Benchmark Selection -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Select Benchmark Dataset</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <BaseSelect
                v-model="selectedBenchmarkId"
                :options="benchmarks.map(b => ({ label: b.name, value: b.id }))"
                label="Benchmark Dataset"
                :loading="loadingBenchmarks"
                @change="handleBenchmarkChange"
              />
            </div>
            
            <div v-if="benchmarkInfo" class="bg-gray-50 p-4 rounded-lg">
              <h3 class="font-medium mb-2">{{ benchmarkInfo.name }}</h3>
              <p class="text-sm text-gray-600 mb-2">{{ benchmarkInfo.description }}</p>
              <div class="grid grid-cols-3 gap-2 text-xs">
                <div>
                  <span class="text-gray-500">Size:</span>
                  <span class="ml-1 font-medium">{{ benchmarkInfo.size }}</span>
                </div>
                <div>
                  <span class="text-gray-500">Samples:</span>
                  <span class="ml-1 font-medium">{{ benchmarkInfo.samples.toLocaleString() }}</span>
                </div>
                <div>
                  <span class="text-gray-500">Features:</span>
                  <span class="ml-1 font-medium">{{ benchmarkInfo.features }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Model Configuration -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Model Configuration</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <BaseSelect
              v-model="modelType"
              :options="modelTypeOptions"
              label="Model Type"
            />
            
            <BaseInput
              v-model="modelVersionId"
              label="Model Version ID (Optional)"
              placeholder="Enter existing model version ID"
            />
          </div>
          
          <BaseInput
            v-model="modelPath"
            label="Model Path (Alternative)"
            placeholder="Enter path to custom model file"
            type="text"
          />
          
          <BaseButton 
            variant="primary" 
            @click="validateModel"
            :disabled="!isValidForm || validating"
            class="w-full md:w-auto mt-4"
          >
            <Target class="w-4 h-4 mr-2" :class="{ 'animate-spin': validating }" />
            {{ validating ? 'Validating...' : 'Validate Model' }}
          </BaseButton>
        </div>
      </BaseCard>

      <!-- Validation Results -->
      <div v-if="validationResult">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Validation Results</h2>
            
            <!-- Overall Score -->
            <div class="flex justify-center mb-6">
              <div class="text-center">
                <div 
                  class="w-32 h-32 rounded-full flex items-center justify-center text-3xl font-bold"
                  :class="scoreBgColor"
                >
                  <span :class="scoreColor">{{ (overallScore * 100).toFixed(0) }}%</span>
                </div>
                <p class="mt-2 font-medium">Overall Validation Score</p>
                <p class="text-sm text-gray-600">Higher is better</p>
              </div>
            </div>
            
            <!-- Metrics Grid -->
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
              <BaseCard
                v-for="(metric, name) in validationResult.metrics"
                :key="name"
                class="hover:shadow-md transition-shadow"
              >
                <div class="p-4 text-center">
                  <component 
                    :is="getMetricStatusIcon(metric.passed)" 
                    class="w-6 h-6 mx-auto mb-2" 
                    :class="getMetricStatusColor(metric.passed)"
                  />
                  <h3 class="font-medium text-sm mb-1">{{ name }}</h3>
                  <p class="text-2xl font-bold mb-1">{{ metric.value }}</p>
                  <p class="text-xs text-gray-600">Threshold: {{ metric.threshold }}</p>
                  <span 
                    class="inline-block mt-2 px-2 py-1 text-xs rounded-full"
                    :class="metric.passed ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                  >
                    {{ metric.passed ? 'Passed' : 'Failed' }}
                  </span>
                </div>
              </BaseCard>
            </div>
            
            <!-- Recommendations -->
            <div v-if="validationResult.recommendations?.length > 0" class="bg-blue-50 p-4 rounded-lg mb-6">
              <h3 class="font-medium mb-2 flex items-center">
                <AlertTriangle class="w-4 h-4 text-blue-600 mr-2" />
                Recommendations
              </h3>
              <ul class="text-sm text-gray-700 space-y-1">
                <li 
                  v-for="(rec, index) in validationResult.recommendations" 
                  :key="index"
                  class="flex items-start"
                >
                  <span class="text-blue-600 mr-2">•</span>
                  {{ rec }}
                </li>
              </ul>
            </div>
            
            <!-- Charts -->
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div ref="metricsChartRef" class="w-full h-80"></div>
              <div ref="comparisonChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Validation History -->
      <BaseCard v-if="validationHistory.length > 0">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Validation History</h2>
          
          <div ref="historyChartRef" class="w-full h-64 mb-6"></div>
          
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Timestamp
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Benchmark
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Model
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Score
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="(validation, index) in validationHistory.slice(0, 5)" 
                  :key="index"
                  class="hover:bg-gray-50"
                >
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatDate(validation.validatedAt) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {{ validation.benchmark }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ validation.modelId }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span 
                      class="font-medium"
                      :class="validation.overallScore >= 0.8 ? 'text-green-600' : validation.overallScore >= 0.6 ? 'text-yellow-600' : 'text-red-600'"
                    >
                      {{ (validation.overallScore * 100).toFixed(1) }}%
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span 
                      class="px-2 py-1 rounded-full text-xs font-medium"
                      :class="[
                        validation.overallScore >= 0.8 ? 'bg-green-100 text-green-800' :
                        validation.overallScore >= 0.6 ? 'bg-yellow-100 text-yellow-800' :
                        'bg-red-100 text-red-800'
                      ]"
                    >
                      {{
                        validation.overallScore >= 0.8 ? 'Excellent' :
                        validation.overallScore >= 0.6 ? 'Good' :
                        'Needs Improvement'
                      }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </BaseCard>

      <!-- Empty State -->
      <BaseCard v-if="!validationResult && !validating">
        <div class="p-12 text-center">
          <Target class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 class="text-lg font-medium text-gray-900 mb-2">No Validation Results</h3>
          <p class="text-gray-500 mb-6">Configure a model and select a benchmark to run validation.</p>
          <BaseButton @click="validateModel" variant="primary" :disabled="!isValidForm">
            Run First Validation
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.model-validation {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .model-validation {
    padding: 0.5rem;
  }
}
</style>