<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  getCurrentDriftStatus,
  getDriftHistory,
  getThresholds,
  configureThresholds,
  generateDriftReport,
  detectDrift,
  type DriftDetectionResult,
  type DriftMetrics,
  type DriftThresholds,
  type DriftReport,
  type DriftDetectionRequest
} from '@/services/driftDetection.service'
import { 
  Activity, 
  AlertTriangle, 
  BarChart3, 
  Settings,
  Play,
  Download,
  Clock,
  TrendingUp,
  Shield
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  BarChart as EChartsBar
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
  EChartsLine,
  EChartsBar,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const driftStatus = ref<Record<string, DriftDetectionResult>>({})
const driftHistory = ref<DriftMetrics[]>([])
const thresholds = ref<DriftThresholds | null>(null)
const selectedModel = ref('')
const reportPeriod = ref(7)
const loading = ref(false)
const detecting = ref(false)
const configuring = ref(false)

// Chart refs
const driftChartRef = ref<HTMLDivElement | null>(null)
const thresholdChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let driftChartInstance: echarts.ECharts | null = null
let thresholdChartInstance: echarts.ECharts | null = null

// Computed
const modelNames = computed(() => {
  return Object.keys(driftStatus.value)
})

const modelOptions = computed(() => {
  return modelNames.value.map(name => ({
    label: name,
    value: name
  }))
})

const driftedModels = computed(() => {
  return Object.entries(driftStatus.value)
    .filter(([_, result]) => result.isDriftDetected)
    .map(([name, result]) => ({ name, ...result }))
})

const avgFeatureDrift = computed(() => {
  const results = Object.values(driftStatus.value)
  if (results.length === 0) return 0
  const sum = results.reduce((acc, result) => acc + result.featureDriftScore, 0)
  return (sum / results.length) * 100
})

const avgPredictionDrift = computed(() => {
  const results = Object.values(driftStatus.value)
  if (results.length === 0) return 0
  const sum = results.reduce((acc, result) => acc + result.predictionDriftScore, 0)
  return (sum / results.length) * 100
})

// Methods
const loadDriftStatus = async () => {
  try {
    loading.value = true
    driftStatus.value = await getCurrentDriftStatus()
    
    if (modelNames.value.length > 0 && !selectedModel.value) {
      selectedModel.value = modelNames.value[0]
      await loadModelData()
    }
    
    renderCharts()
  } catch (error) {
    console.error('Failed to load drift status:', error)
    toast.error('Failed to load drift status')
  } finally {
    loading.value = false
  }
}

const loadModelData = async () => {
  if (!selectedModel.value) return
  
  try {
    const [history, modelThresholds] = await Promise.all([
      getDriftHistory(selectedModel.value, 168), // Last 7 days
      getThresholds(selectedModel.value)
    ])
    
    driftHistory.value = history
    thresholds.value = modelThresholds
    
    renderCharts()
  } catch (error) {
    console.error('Failed to load model data:', error)
    toast.error('Failed to load model data')
  }
}

const detectModelDrift = async () => {
  if (!selectedModel.value) {
    toast.warning('Please select a model first')
    return
  }
  
  try {
    detecting.value = true
    
    // In a real implementation, you would provide actual reference and current data
    const request: DriftDetectionRequest = {
      referenceData: [], // Would come from historical data
      currentData: [], // Would come from recent data
      method: 'ks-test' // Kolmogorov-Smirnov test
    }
    
    const result = await detectDrift(request)
    
    // Update local state
    driftStatus.value[selectedModel.value] = result
    
    toast.success(`Drift detection completed for ${selectedModel.value}`)
    renderCharts()
  } catch (error) {
    console.error('Failed to detect drift:', error)
    toast.error('Failed to detect drift')
  } finally {
    detecting.value = false
  }
}

const updateThresholds = async () => {
  if (!selectedModel.value || !thresholds.value) {
    toast.warning('Please select a model and load thresholds first')
    return
  }
  
  try {
    configuring.value = true
    await configureThresholds(selectedModel.value, thresholds.value)
    toast.success('Thresholds updated successfully')
  } catch (error) {
    console.error('Failed to update thresholds:', error)
    toast.error('Failed to update thresholds')
  } finally {
    configuring.value = false
  }
}

const generateReport = async () => {
  try {
    loading.value = true
    
    const report = await generateDriftReport(
      selectedModel.value || undefined,
      reportPeriod.value
    )
    
    // Download report as JSON
    const dataStr = JSON.stringify(report, null, 2)
    const blob = new Blob([dataStr], { type: 'application/json' })
    const url = URL.createObjectURL(blob)
    
    const a = document.createElement('a')
    a.href = url
    a.download = `drift-report-${selectedModel.value || 'all'}-${new Date().toISOString().split('T')[0]}.json`
    document.body.appendChild(a)
    a.click()
    document.body.removeChild(a)
    URL.revokeObjectURL(url)
    
    toast.success('Drift report downloaded successfully')
  } catch (error) {
    console.error('Failed to generate report:', error)
    toast.error('Failed to generate drift report')
  } finally {
    loading.value = false
  }
}

const renderCharts = () => {
  renderDriftChart()
  renderThresholdChart()
}

const renderDriftChart = () => {
  if (!driftChartRef.value || driftHistory.value.length === 0) return
  
  if (!driftChartInstance) {
    driftChartInstance = echarts.init(driftChartRef.value)
  }
  
  const data = driftHistory.value
    .slice(-100) // Last 100 points
    .map((item) => ({
      timestamp: new Date(item.timestamp).getTime(),
      featureDrift: item.featureDriftScore * 100,
      predictionDrift: item.predictionDriftScore * 100
    }))
    .sort((a, b) => a.timestamp - b.timestamp)
  
  const option = {
    title: {
      text: `Drift Scores Over Time - ${selectedModel.value}`,
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30
    },
    xAxis: {
      type: 'time',
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'Drift Score (%)',
      min: 0,
      max: 100
    },
    series: [
      {
        name: 'Feature Drift',
        type: 'line',
        data: data.map((d) => [d.timestamp, d.featureDrift]),
        smooth: true,
        itemStyle: { color: '#3b82f6' }
      },
      {
        name: 'Prediction Drift',
        type: 'line',
        data: data.map((d) => [d.timestamp, d.predictionDrift]),
        smooth: true,
        itemStyle: { color: '#ef4444' }
      }
    ]
  }
  
  driftChartInstance.setOption(option, true)
}

const renderThresholdChart = () => {
  if (!thresholdChartRef.value || !thresholds.value) return
  
  if (!thresholdChartInstance) {
    thresholdChartInstance = echarts.init(thresholdChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Current Drift Thresholds',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: ['Feature Drift', 'Prediction Drift', 'Alert Threshold']
    },
    yAxis: {
      type: 'value',
      name: 'Threshold (%)',
      min: 0,
      max: 100
    },
    series: [{
      name: 'Thresholds',
      type: 'bar',
      data: [
        thresholds.value.featureDriftThreshold * 100,
        thresholds.value.predictionDriftThreshold * 100,
        thresholds.value.alertThreshold * 100
      ],
      itemStyle: {
        color: (params: any) => {
          const value = params.value
          if (value < 20) return '#10b981' // Green
          if (value < 50) return '#f59e0b' // Yellow
          return '#ef4444' // Red
        }
      }
    }]
  }
  
  thresholdChartInstance.setOption(option, true)
}

const formatPercentage = (score: number): string => {
  return `${(score * 100).toFixed(2)}%`
}

const getDriftStatusColor = (score: number): string => {
  if (score < 0.2) return 'text-green-600'
  if (score < 0.5) return 'text-yellow-600'
  return 'text-red-600'
}

const resizeCharts = () => {
  driftChartInstance?.resize()
  thresholdChartInstance?.resize()
}

// Lifecycle
onMounted(async () => {
  window.addEventListener('resize', resizeCharts)
  await loadDriftStatus()
})

onUnmounted(() => {
  window.removeEventListener('resize', resizeCharts)
  driftChartInstance?.dispose()
  thresholdChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="drift-detection space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Drift Detection</h1>
          <p class="text-gray-600 mt-2">Monitor model performance degradation and concept drift</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="loadDriftStatus" :disabled="loading">
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
            Refresh
          </BaseButton>
          <BaseButton variant="outline" @click="generateReport" :disabled="loading">
            <Download class="w-4 h-4 mr-2" />
            Generate Report
          </BaseButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <Activity class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ modelNames.length }}</p>
            <p class="text-sm text-gray-600">Monitored Models</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <AlertTriangle class="w-8 h-8 text-red-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ driftedModels.length }}</p>
            <p class="text-sm text-gray-600">Drifted Models</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <TrendingUp class="w-8 h-8 text-purple-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ avgFeatureDrift.toFixed(1) }}%</p>
            <p class="text-sm text-gray-600">Avg Feature Drift</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Shield class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ avgPredictionDrift.toFixed(1) }}%</p>
            <p class="text-sm text-gray-600">Avg Prediction Drift</p>
          </div>
        </BaseCard>
      </div>

      <!-- Model Selection and Controls -->
      <BaseCard>
        <div class="p-6">
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
            <BaseSelect
              v-model="selectedModel"
              :options="modelOptions"
              label="Select Model"
              @change="loadModelData"
            />
            
            <BaseInput
              v-model.number="reportPeriod"
              label="Report Period (Days)"
              type="number"
              min="1"
              max="365"
            />
            
            <div class="flex items-end gap-2">
              <BaseButton 
                variant="primary" 
                @click="detectModelDrift"
                :disabled="detecting || !selectedModel"
                class="flex-1"
              >
                <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': detecting }" />
                {{ detecting ? 'Detecting...' : 'Detect Drift' }}
              </BaseButton>
              
              <BaseButton 
                variant="outline" 
                @click="updateThresholds"
                :disabled="configuring || !thresholds"
              >
                <Settings class="w-4 h-4 mr-2" :class="{ 'animate-spin': configuring }" />
                {{ configuring ? 'Updating...' : 'Update Thresholds' }}
              </BaseButton>
            </div>
          </div>
          
          <!-- Threshold Configuration -->
          <div v-if="thresholds" class="border-t pt-4 mt-4">
            <h3 class="font-medium mb-3">Drift Thresholds</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <BaseInput
                v-model.number="thresholds.featureDriftThreshold"
                label="Feature Drift Threshold"
                type="number"
                step="0.01"
                min="0"
                max="1"
              />
              
              <BaseInput
                v-model.number="thresholds.predictionDriftThreshold"
                label="Prediction Drift Threshold"
                type="number"
                step="0.01"
                min="0"
                max="1"
              />
              
              <BaseInput
                v-model.number="thresholds.alertThreshold"
                label="Alert Threshold"
                type="number"
                step="0.01"
                min="0"
                max="1"
              />
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Drift History Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="driftChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <!-- Thresholds Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="thresholdChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Drifted Models Table -->
      <BaseCard v-if="driftedModels.length > 0">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Models with Detected Drift ({{ driftedModels.length }})
          </h2>
          
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Model Name
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Feature Drift
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Prediction Drift
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Detected At
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Recommendation
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="model in driftedModels" 
                  :key="model.name"
                  class="hover:bg-gray-50"
                >
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {{ model.name }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm">
                    <span :class="getDriftStatusColor(model.featureDriftScore)">
                      {{ formatPercentage(model.featureDriftScore) }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm">
                    <span :class="getDriftStatusColor(model.predictionDriftScore)">
                      {{ formatPercentage(model.predictionDriftScore) }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ new Date(model.detectedAt).toLocaleString() }}
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-900">
                    {{ model.recommendation }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </BaseCard>

      <!-- All Models Status -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">All Model Status</h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading drift status...</p>
          </div>
          
          <div v-else-if="modelNames.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No models are currently being monitored</p>
          </div>
          
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <BaseCard
              v-for="(result, modelName) in driftStatus"
              :key="modelName"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex justify-between items-start mb-3">
                  <h3 class="font-semibold text-gray-900">{{ modelName }}</h3>
                  <span 
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                    :class="result.isDriftDetected ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'"
                  >
                    {{ result.isDriftDetected ? 'Drift Detected' : 'Stable' }}
                  </span>
                </div>
                
                <div class="space-y-2 text-sm">
                  <div class="flex justify-between">
                    <span class="text-gray-600">Feature Drift:</span>
                    <span :class="getDriftStatusColor(result.featureDriftScore)">
                      {{ formatPercentage(result.featureDriftScore) }}
                    </span>
                  </div>
                  <div class="flex justify-between">
                    <span class="text-gray-600">Prediction Drift:</span>
                    <span :class="getDriftStatusColor(result.predictionDriftScore)">
                      {{ formatPercentage(result.predictionDriftScore) }}
                    </span>
                  </div>
                  <div class="pt-2 text-xs text-gray-500">
                    Last checked: {{ new Date(result.timestamp).toLocaleString() }}
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.drift-detection {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .drift-detection {
    padding: 0.5rem;
  }
}
</style>