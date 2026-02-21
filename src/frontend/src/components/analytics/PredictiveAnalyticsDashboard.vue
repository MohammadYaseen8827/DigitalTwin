<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { requestPrediction, fetchPredictionHistory } from '@/services/predictions.service'
import { TrendingUp, Clock, AlertTriangle, CheckCircle, BarChart3, Zap, Target, Calendar } from 'lucide-vue-next'

// Temporary interface for prediction data
interface PredictionData {
  id: string
  machineId: string
  remainingUsefulLife: number
  confidence: number
  timestamp: string
}

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  ScatterChart as EChartsScatter,
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
  EChartsScatter,
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
const predicting = ref(false)
const loadingHistory = ref(false)
const machineId = ref('00000000-0000-0000-0000-000000000001')
const predictionHistory = ref<PredictionData[]>([])
const currentPrediction = ref<PredictionData | null>(null)

// Chart refs
const predictionChartRef = ref<HTMLDivElement | null>(null)
const historyChartRef = ref<HTMLDivElement | null>(null)
const confidenceChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let predictionChartInstance: echarts.ECharts | null = null
let historyChartInstance: echarts.ECharts | null = null
let confidenceChartInstance: echarts.ECharts | null = null

// Computed
const avgRul = computed(() => {
  if (predictionHistory.value.length === 0) return 0
  const total = predictionHistory.value.reduce((sum: number, pred: PredictionData) => sum + pred.remainingUsefulLife, 0)
  return Math.round(total / predictionHistory.value.length)
})

const avgConfidence = computed(() => {
  if (predictionHistory.value.length === 0) return 0
  const total = predictionHistory.value.reduce((sum: number, pred: PredictionData) => sum + pred.confidence, 0)
  return (total / predictionHistory.value.length * 100).toFixed(1)
})

const criticalPredictions = computed(() => {
  return predictionHistory.value.filter((pred: PredictionData) => pred.remainingUsefulLife < 30).length
})

const predictionStatus = computed(() => {
  if (!currentPrediction.value) return 'No prediction'
  
  const rul = currentPrediction.value.remainingUsefulLife
  if (rul < 30) return 'Critical'
  if (rul < 90) return 'Warning'
  return 'Healthy'
})

const statusColor = computed(() => {
  const status = predictionStatus.value
  if (status === 'Critical') return 'text-red-600'
  if (status === 'Warning') return 'text-yellow-600'
  if (status === 'Healthy') return 'text-green-600'
  return 'text-gray-600'
})

const statusBgColor = computed(() => {
  const status = predictionStatus.value
  if (status === 'Critical') return 'bg-red-100'
  if (status === 'Warning') return 'bg-yellow-100'
  if (status === 'Healthy') return 'bg-green-100'
  return 'bg-gray-100'
})

// Methods
const makePrediction = async () => {
  try {
    predicting.value = true
    
    const prediction = await requestPrediction(machineId.value)
    currentPrediction.value = prediction
    
    // Add to history
    predictionHistory.value.unshift(prediction)
    
    renderPredictionChart()
    renderConfidenceChart()
    
    const status = predictionStatus.value
    if (status === 'Critical') {
      toast.error(`Critical condition detected: ${prediction.remainingUsefulLife} hours remaining`)
    } else if (status === 'Warning') {
      toast.warning(`Maintenance recommended: ${prediction.remainingUsefulLife} hours remaining`)
    } else {
      toast.success(`Equipment is healthy: ${prediction.remainingUsefulLife} hours remaining`)
    }
  } catch (error) {
    console.error('Failed to make prediction:', error)
    toast.error('Failed to generate prediction')
  } finally {
    predicting.value = false
  }
}

const loadHistory = async () => {
  try {
    loadingHistory.value = true
    
    const history = await fetchPredictionHistory(machineId.value, 50)
    predictionHistory.value = history
    
    renderHistoryChart()
    renderConfidenceChart()
    
    toast.success(`Loaded ${history.length} historical predictions`)
  } catch (error) {
    console.error('Failed to load history:', error)
    toast.error('Failed to load prediction history')
  } finally {
    loadingHistory.value = false
  }
}

const renderPredictionChart = () => {
  if (!predictionChartRef.value || !currentPrediction.value) return
  
  if (!predictionChartInstance) {
    predictionChartInstance = echarts.init(predictionChartRef.value)
  }
  
  const rul = currentPrediction.value.remainingUsefulLife
  const confidence = currentPrediction.value.confidence * 100
  
  const option = {
    title: {
      text: 'Current Prediction Analysis',
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
      data: ['RUL (Hours)', 'Confidence (%)']
    },
    yAxis: [
      {
        type: 'value',
        name: 'Hours',
        position: 'left'
      },
      {
        type: 'value',
        name: 'Percentage',
        position: 'right',
        min: 0,
        max: 100
      }
    ],
    series: [
      {
        name: 'Value',
        type: 'bar',
        data: [
          { value: rul, itemStyle: { color: rul < 30 ? '#ef4444' : rul < 90 ? '#f59e0b' : '#10b981' } },
          { value: confidence, itemStyle: { color: confidence > 80 ? '#10b981' : confidence > 60 ? '#f59e0b' : '#ef4444' } }
        ],
        label: {
          show: true,
          position: 'top',
          formatter: (params: any) => {
            if (params.name === 'RUL (Hours)') return `${params.value}h`
            return `${params.value.toFixed(1)}%`
          }
        }
      }
    ]
  }
  
  predictionChartInstance.setOption(option, true)
}

const renderHistoryChart = () => {
  if (!historyChartRef.value || predictionHistory.value.length === 0) return
  
  if (!historyChartInstance) {
    historyChartInstance = echarts.init(historyChartRef.value)
  }
  
  const timestamps = predictionHistory.value.map((pred: PredictionData) => {
    const date = new Date(pred.timestamp)
    return `${date.getMonth() + 1}/${date.getDate()} ${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`
  })
  
  const rulValues = predictionHistory.value.map((pred: PredictionData) => pred.remainingUsefulLife)
  const confidenceValues = predictionHistory.value.map((pred: PredictionData) => pred.confidence * 100)
  
  const option = {
    title: {
      text: 'Prediction History Trend',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30,
      data: ['RUL (Hours)', 'Confidence (%)']
    },
    xAxis: {
      type: 'category',
      data: timestamps,
      name: 'Timestamp'
    },
    yAxis: [
      {
        type: 'value',
        name: 'RUL (Hours)',
        position: 'left'
      },
      {
        type: 'value',
        name: 'Confidence (%)',
        position: 'right',
        min: 0,
        max: 100
      }
    ],
    dataZoom: [
      {
        type: 'inside',
        start: 0,
        end: 100
      }
    ],
    series: [
      {
        name: 'RUL (Hours)',
        type: 'line',
        data: rulValues,
        smooth: true,
        itemStyle: { color: '#3b82f6' }
      },
      {
        name: 'Confidence (%)',
        type: 'line',
        yAxisIndex: 1,
        data: confidenceValues,
        smooth: true,
        itemStyle: { color: '#10b981' }
      }
    ]
  }
  
  historyChartInstance.setOption(option, true)
}

const renderConfidenceChart = () => {
  if (!confidenceChartRef.value) return
  
  if (!confidenceChartInstance) {
    confidenceChartInstance = echarts.init(confidenceChartRef.value)
  }
  
  // Distribution of confidence levels
  const highConf = predictionHistory.value.filter((p: PredictionData) => p.confidence >= 0.8).length
  const medConf = predictionHistory.value.filter((p: PredictionData) => p.confidence >= 0.6 && p.confidence < 0.8).length
  const lowConf = predictionHistory.value.filter((p: PredictionData) => p.confidence < 0.6).length
  
  const option = {
    title: {
      text: 'Prediction Confidence Distribution',
      left: 'center'
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 10,
      top: 50,
      data: ['High (>80%)', 'Medium (60-80%)', 'Low (<60%)']
    },
    series: [
      {
        name: 'Confidence',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        data: [
          { value: highConf, name: 'High (>80%)', itemStyle: { color: '#10b981' } },
          { value: medConf, name: 'Medium (60-80%)', itemStyle: { color: '#f59e0b' } },
          { value: lowConf, name: 'Low (<60%)', itemStyle: { color: '#ef4444' } }
        ],
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }
  
  confidenceChartInstance.setOption(option, true)
}

const formatDate = (timestamp: string) => {
  return new Date(timestamp).toLocaleString()
}

const getRulColor = (rul: number) => {
  if (rul < 30) return 'text-red-600'
  if (rul < 90) return 'text-yellow-600'
  return 'text-green-600'
}

const getConfidenceColor = (confidence: number) => {
  if (confidence >= 0.8) return 'text-green-600'
  if (confidence >= 0.6) return 'text-yellow-600'
  return 'text-red-600'
}

const resizeCharts = () => {
  predictionChartInstance?.resize()
  historyChartInstance?.resize()
  confidenceChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  loadHistory()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  predictionChartInstance?.dispose()
  historyChartInstance?.dispose()
  confidenceChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="predictive-analytics space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Predictive Analytics</h1>
          <p class="text-gray-600 mt-2">AI-powered equipment failure prediction and RUL estimation</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="primary" 
            @click="makePrediction"
            :disabled="predicting"
          >
            <Zap class="w-4 h-4 mr-2" :class="{ 'animate-spin': predicting }" />
            {{ predicting ? 'Predicting...' : 'New Prediction' }}
          </BaseButton>
          
          <BaseButton 
            variant="outline" 
            @click="loadHistory"
            :disabled="loadingHistory"
          >
            <BarChart3 class="w-4 h-4 mr-2" />
            Load History
          </BaseButton>
        </div>
      </div>

      <!-- Machine Selection -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Target Equipment</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <BaseInput
              v-model="machineId"
              label="Machine ID"
              placeholder="Enter machine UUID"
            />
            
            <div class="flex items-end">
              <BaseButton 
                variant="outline" 
                @click="loadHistory"
                :disabled="loadingHistory"
                class="w-full"
              >
                <Target class="w-4 h-4 mr-2" />
                Load Machine History
              </BaseButton>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Current Prediction -->
      <div v-if="currentPrediction">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Latest Prediction</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <!-- Prediction Details -->
              <div>
                <div class="bg-gradient-to-r from-blue-50 to-indigo-50 p-6 rounded-lg">
                  <div class="flex items-start gap-4">
                    <TrendingUp class="w-8 h-8 text-blue-600 mt-1" />
                    <div class="flex-1">
                      <h3 class="text-lg font-semibold mb-2">Remaining Useful Life</h3>
                      <p class="text-3xl font-bold mb-2" :class="getRulColor(currentPrediction.remainingUsefulLife)">
                        {{ currentPrediction.remainingUsefulLife }} hours
                      </p>
                      
                      <div class="space-y-2 mt-4">
                        <div class="flex justify-between">
                          <span class="text-sm text-gray-600">Confidence</span>
                          <span class="font-medium" :class="getConfidenceColor(currentPrediction.confidence)">
                            {{ (currentPrediction.confidence * 100).toFixed(1) }}%
                          </span>
                        </div>
                        
                        <div class="flex justify-between">
                          <span class="text-sm text-gray-600">Status</span>
                          <span 
                            class="px-2 py-1 rounded-full text-sm font-medium"
                            :class="statusBgColor"
                          >
                            {{ predictionStatus }}
                          </span>
                        </div>
                        
                        <div class="flex justify-between">
                          <span class="text-sm text-gray-600">Timestamp</span>
                          <span class="text-sm">{{ formatDate(currentPrediction.timestamp) }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              
              <!-- Prediction Visualization -->
              <div ref="predictionChartRef" class="w-full h-64"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <Clock class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ avgRul }}</p>
            <p class="text-sm text-gray-600">Avg. RUL (hours)</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <CheckCircle class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ avgConfidence }}%</p>
            <p class="text-sm text-gray-600">Avg. Confidence</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <AlertTriangle class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
            <p class="text-2xl font-bold text-yellow-600">{{ criticalPredictions }}</p>
            <p class="text-sm text-gray-600">Critical Warnings</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <BarChart3 class="w-8 h-8 text-purple-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ predictionHistory.length }}</p>
            <p class="text-sm text-gray-600">Total Predictions</p>
          </div>
        </BaseCard>
      </div>

      <!-- Historical Analysis -->
      <div v-if="predictionHistory.length > 0">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Historical Analysis</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
              <div ref="historyChartRef" class="w-full h-80"></div>
              <div ref="confidenceChartRef" class="w-full h-80"></div>
            </div>
            
            <!-- Recent Predictions Table -->
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Timestamp
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      RUL (Hours)
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Confidence
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Status
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr 
                    v-for="(prediction, index) in predictionHistory.slice(0, 10)" 
                    :key="index"
                    class="hover:bg-gray-50"
                  >
                    <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                      {{ formatDate(prediction.timestamp) }}
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span 
                        class="font-medium"
                        :class="getRulColor(prediction.remainingUsefulLife)"
                      >
                        {{ prediction.remainingUsefulLife }}h
                      </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span 
                        class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                        :class="getConfidenceColor(prediction.confidence)"
                      >
                        {{ (prediction.confidence * 100).toFixed(1) }}%
                      </span>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap text-sm">
                      <span 
                        class="px-2 py-1 rounded-full text-xs font-medium"
                        :class="[
                          prediction.remainingUsefulLife < 30 ? 'bg-red-100 text-red-800' :
                          prediction.remainingUsefulLife < 90 ? 'bg-yellow-100 text-yellow-800' :
                          'bg-green-100 text-green-800'
                        ]"
                      >
                        {{
                          prediction.remainingUsefulLife < 30 ? 'Critical' :
                          prediction.remainingUsefulLife < 90 ? 'Warning' :
                          'Healthy'
                        }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Empty State -->
      <BaseCard v-if="predictionHistory.length === 0 && !loadingHistory">
        <div class="p-12 text-center">
          <TrendingUp class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 class="text-lg font-medium text-gray-900 mb-2">No Prediction Data</h3>
          <p class="text-gray-500 mb-6">Load historical predictions or generate a new prediction to see analytics.</p>
          <div class="flex gap-3 justify-center">
            <BaseButton @click="loadHistory" variant="outline">
              Load History
            </BaseButton>
            <BaseButton @click="makePrediction" variant="primary">
              Generate Prediction
            </BaseButton>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.predictive-analytics {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .predictive-analytics {
    padding: 0.5rem;
  }
}
</style>