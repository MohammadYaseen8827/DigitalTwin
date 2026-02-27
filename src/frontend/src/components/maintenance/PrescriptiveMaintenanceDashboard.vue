<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { prescriptiveService, type MaintenanceWindow } from '@/services/prescriptive.service'
import { Calendar, DollarSign, AlertTriangle, CheckCircle, TrendingUp, Clock } from 'lucide-vue-next'

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
const loading = ref(false)
const analyzing = ref(false)
const machineId = ref('')
const isValidMachineId = computed(() => {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
  return uuidRegex.test(machineId.value)
})
const analysisDays = ref(30)
const analysisResults = ref<MaintenanceWindow[]>([])
const optimalWindow = ref<MaintenanceWindow | null>(null)

// Chart refs
const costChartRef = ref<HTMLDivElement | null>(null)
const riskChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let costChartInstance: echarts.ECharts | null = null
let riskChartInstance: echarts.ECharts | null = null

// Computed
const averageCost = computed(() => {
  if (analysisResults.value.length === 0) return 0
  const total = analysisResults.value.reduce((sum, window) => sum + window.estimatedCost, 0)
  return total / analysisResults.value.length
})

const highestRisk = computed(() => {
  if (analysisResults.value.length === 0) return 0
  return Math.max(...analysisResults.value.map(w => w.riskScore))
})

const lowestRisk = computed(() => {
  if (analysisResults.value.length === 0) return 0
  return Math.min(...analysisResults.value.map(w => w.riskScore))
})

const recommendedAction = computed(() => {
  if (!optimalWindow.value) return 'No recommendation available'
  
  const risk = optimalWindow.value.riskScore
  if (risk < 0.3) return 'Routine maintenance suggested'
  if (risk < 0.7) return 'Preventive maintenance recommended'
  return 'Immediate maintenance required'
})

const actionColor = computed(() => {
  if (!optimalWindow.value) return 'text-gray-600'
  
  const risk = optimalWindow.value.riskScore
  if (risk < 0.3) return 'text-green-600'
  if (risk < 0.7) return 'text-yellow-600'
  return 'text-red-600'
})

// Methods
const runAnalysis = async () => {
  try {
    analyzing.value = true
    
    const results = await prescriptiveService.getAnalysis(machineId.value, analysisDays.value)
    analysisResults.value = results
    
    renderCostChart()
    renderRiskChart()
    
    toast.success(`Analysis completed with ${results.length} maintenance windows`)
  } catch (error) {
    console.error('Failed to run analysis:', error)
    toast.error('Failed to run prescriptive analysis')
  } finally {
    analyzing.value = false
  }
}

const getOptimalMaintenance = async () => {
  try {
    loading.value = true
    
    optimalWindow.value = await prescriptiveService.getOptimal(machineId.value)
    
    toast.success('Optimal maintenance window calculated')
  } catch (error) {
    console.error('Failed to get optimal maintenance:', error)
    toast.error('Failed to calculate optimal maintenance window')
  } finally {
    loading.value = false
  }
}

const renderCostChart = () => {
  if (!costChartRef.value || analysisResults.value.length === 0) return
  
  if (!costChartInstance) {
    costChartInstance = echarts.init(costChartRef.value)
  }
  
  const dates = analysisResults.value.map(window => {
    const date = new Date(window.scheduledDate)
    return `${date.getMonth() + 1}/${date.getDate()}`
  })
  
  const costs = analysisResults.value.map(window => window.estimatedCost)
  
  const option = {
    title: {
      text: 'Estimated Maintenance Costs Over Time',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const param = params[0]
        const window = analysisResults.value[param.dataIndex]
        return `
          <strong>Date:</strong> ${new Date(window.scheduledDate).toLocaleDateString()}<br/>
          <strong>Cost:</strong> $${window.estimatedCost.toLocaleString()}<br/>
          <strong>Risk:</strong> ${(window.riskScore * 100).toFixed(1)}%
        `
      }
    },
    xAxis: {
      type: 'category',
      data: dates,
      name: 'Scheduled Date'
    },
    yAxis: {
      type: 'value',
      name: 'Estimated Cost ($)'
    },
    series: [{
      name: 'Cost',
      type: 'line',
      data: costs,
      smooth: true,
      itemStyle: { color: '#3b82f6' },
      areaStyle: { opacity: 0.3 }
    }]
  }
  
  costChartInstance.setOption(option, true)
}

const renderRiskChart = () => {
  if (!riskChartRef.value || analysisResults.value.length === 0) return
  
  if (!riskChartInstance) {
    riskChartInstance = echarts.init(riskChartRef.value)
  }
  
  const dates = analysisResults.value.map(window => {
    const date = new Date(window.scheduledDate)
    return `${date.getMonth() + 1}/${date.getDate()}`
  })
  
  const risks = analysisResults.value.map(window => window.riskScore * 100)
  
  const option = {
    title: {
      text: 'Risk Scores Over Time',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        const param = params[0]
        const window = analysisResults.value[param.dataIndex]
        return `
          <strong>Date:</strong> ${new Date(window.scheduledDate).toLocaleDateString()}<br/>
          <strong>Risk:</strong> ${(window.riskScore * 100).toFixed(1)}%<br/>
          <strong>Cost:</strong> $${window.estimatedCost.toLocaleString()}
        `
      }
    },
    xAxis: {
      type: 'category',
      data: dates,
      name: 'Scheduled Date'
    },
    yAxis: {
      type: 'value',
      name: 'Risk Score (%)',
      min: 0,
      max: 100
    },
    series: [{
      name: 'Risk',
      type: 'bar',
      data: risks,
      itemStyle: {
        color: (params: any) => {
          const risk = params.value
          if (risk < 30) return '#10b981' // green
          if (risk < 70) return '#f59e0b' // yellow
          return '#ef4444' // red
        }
      }
    }]
  }
  
  riskChartInstance.setOption(option, true)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

const getRiskColor = (riskScore: number) => {
  if (riskScore < 0.3) return 'text-green-600'
  if (riskScore < 0.7) return 'text-yellow-600'
  return 'text-red-600'
}

const getRiskBgColor = (riskScore: number) => {
  if (riskScore < 0.3) return 'bg-green-100'
  if (riskScore < 0.7) return 'bg-yellow-100'
  return 'bg-red-100'
}

const resizeCharts = () => {
  costChartInstance?.resize()
  riskChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  costChartInstance?.dispose()
  riskChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="prescriptive-maintenance space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Prescriptive Maintenance</h1>
          <p class="text-gray-600 mt-2">Optimize maintenance schedules using predictive analytics</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="primary" 
            @click="runAnalysis"
            :disabled="analyzing"
          >
            <TrendingUp class="w-4 h-4 mr-2" :class="{ 'animate-spin': analyzing }" />
            {{ analyzing ? 'Analyzing...' : 'Run Analysis' }}
          </BaseButton>
          
          <BaseButton 
            variant="outline" 
            @click="getOptimalMaintenance"
            :disabled="loading"
          >
            <Calendar class="w-4 h-4 mr-2" />
            Get Optimal Schedule
          </BaseButton>
        </div>
      </div>

      <!-- Configuration -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Analysis Configuration</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <BaseInput
              v-model="machineId"
              label="Machine ID"
              placeholder="Enter machine UUID"
            />
            
            <BaseInput
              v-model.number="analysisDays"
              label="Analysis Period (Days)"
              type="number"
              min="1"
              max="365"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <DollarSign class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">${{ averageCost.toLocaleString(undefined, { maximumFractionDigits: 0 }) }}</p>
            <p class="text-sm text-gray-600">Avg. Maintenance Cost</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <AlertTriangle class="w-8 h-8 text-red-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ (highestRisk * 100).toFixed(0) }}%</p>
            <p class="text-sm text-gray-600">Highest Risk</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <CheckCircle class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ (lowestRisk * 100).toFixed(0) }}%</p>
            <p class="text-sm text-gray-600">Lowest Risk</p>
          </div>
        </BaseCard>
        
        <BaseCard v-if="optimalWindow">
          <div class="p-4 text-center">
            <Clock class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ formatDate(optimalWindow.scheduledDate) }}</p>
            <p class="text-sm text-gray-600">Optimal Date</p>
          </div>
        </BaseCard>
      </div>

      <!-- Optimal Maintenance Window -->
      <BaseCard v-if="optimalWindow">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Optimal Maintenance Recommendation</h2>
          
          <div class="bg-gradient-to-r from-blue-50 to-indigo-50 p-6 rounded-lg">
            <div class="flex items-start gap-4">
              <Calendar class="w-8 h-8 text-blue-600 mt-1" />
              <div class="flex-1">
                <h3 class="text-lg font-semibold mb-2">Recommended Maintenance Date</h3>
                <p class="text-2xl font-bold mb-2">{{ formatDate(optimalWindow.scheduledDate) }}</p>
                
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-4">
                  <div>
                    <p class="text-sm text-gray-600">Estimated Cost</p>
                    <p class="text-xl font-semibold text-green-600">${{ optimalWindow.estimatedCost.toLocaleString() }}</p>
                  </div>
                  
                  <div>
                    <p class="text-sm text-gray-600">Risk Score</p>
                    <p class="text-xl font-semibold" :class="getRiskColor(optimalWindow.riskScore)">
                      {{ (optimalWindow.riskScore * 100).toFixed(1) }}%
                    </p>
                  </div>
                  
                  <div>
                    <p class="text-sm text-gray-600">Action</p>
                    <p class="text-xl font-semibold" :class="actionColor">{{ recommendedAction }}</p>
                  </div>
                </div>
                
                <div class="mt-4 p-3 bg-white rounded border">
                  <p class="font-medium mb-1">Recommendation:</p>
                  <p class="text-gray-700">{{ optimalWindow.recommendation }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Analysis Results -->
      <div v-if="analysisResults.length > 0">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Maintenance Windows Analysis</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-6">
              <div ref="costChartRef" class="w-full h-80"></div>
              <div ref="riskChartRef" class="w-full h-80"></div>
            </div>
            
            <!-- Detailed Results Table -->
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Scheduled Date
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Estimated Cost
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Risk Score
                    </th>
                    <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Recommendation
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white divide-y divide-gray-200">
                  <tr 
                    v-for="(window, index) in analysisResults" 
                    :key="index"
                    class="hover:bg-gray-50"
                  >
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <Calendar class="w-4 h-4 text-gray-400 mr-2" />
                        {{ formatDate(window.scheduledDate) }}
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <div class="flex items-center">
                        <DollarSign class="w-4 h-4 text-green-400 mr-2" />
                        ${{ window.estimatedCost.toLocaleString() }}
                      </div>
                    </td>
                    <td class="px-6 py-4 whitespace-nowrap">
                      <span 
                        class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                        :class="getRiskBgColor(window.riskScore)"
                      >
                        {{ (window.riskScore * 100).toFixed(1) }}%
                      </span>
                    </td>
                    <td class="px-6 py-4 text-sm text-gray-900">
                      {{ window.recommendation }}
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Empty State -->
      <BaseCard v-if="analysisResults.length === 0 && !analyzing">
        <div class="p-12 text-center">
          <TrendingUp class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 class="text-lg font-medium text-gray-900 mb-2">No Analysis Results Yet</h3>
          <p class="text-gray-500 mb-6">Run a prescriptive analysis to see maintenance recommendations and cost projections.</p>
          <BaseButton @click="runAnalysis" variant="primary">
            Run First Analysis
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.prescriptive-maintenance {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .prescriptive-maintenance {
    padding: 0.5rem;
  }
}
</style>