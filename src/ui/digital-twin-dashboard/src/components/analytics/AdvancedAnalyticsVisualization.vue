<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  getAdvancedAnalyticsDashboard,
  ensemblePrediction,
  detectAnomalies,
  forecastTimeSeries,
  generateMaintenanceRecommendation
} from '@/services/advancedAnalytics.service'
import { fetchMachines } from '@/services/machines.service'
import type { 
  AdvancedAnalyticsDashboard,
  AnomalyDetectionResult,
  ForecastResult,
  PrescriptiveRecommendation
} from '@/services/advancedAnalytics.service'
import { 
  Brain, 
  TrendingUp, 
  AlertTriangle,
  Calendar,
  DollarSign,
  Zap,
  BarChart3,
  LineChart,
  PieChart,
  Filter,
  RefreshCw,
  Settings,
  Play,
  Download
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  BarChart as EChartsBar,
  PieChart as EChartsPie,
  ScatterChart as EChartsScatter
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
  EChartsPie,
  EChartsScatter,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const dashboardData: any = ref<AdvancedAnalyticsDashboard | null>(null)
const machines: any = ref([])
const loading = ref(false)
const selectedMachineId = ref<string>('')

// Chart refs
const healthChartRef = ref<HTMLDivElement | null>(null)
const anomalyChartRef = ref<HTMLDivElement | null>(null)
const forecastChartRef = ref<HTMLDivElement | null>(null)
const recommendationChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let healthChartInstance: echarts.ECharts | null = null
let anomalyChartInstance: echarts.ECharts | null = null
let forecastChartInstance: echarts.ECharts | null = null
let recommendationChartInstance: echarts.ECharts | null = null

// Form state for additional analytics
const forecastForm = ref({
  metric: 'temperature',
  horizon: 30
})

const recommendationForm = ref({
  preventiveMaintenanceCost: 5000,
  reactiveFailureCost: 25000,
  downtimeCostPerHour: 2000
})

// Computed
const healthScorePercentage = computed(() => {
  if (!dashboardData.value) return 0
  return Math.round(dashboardData.value.healthScore * 100)
})

const healthScoreColor = computed(() => {
  const score = healthScorePercentage.value
  if (score >= 80) return 'text-green-600'
  if (score >= 60) return 'text-yellow-600'
  if (score >= 40) return 'text-orange-600'
  return 'text-red-600'
})

const healthScoreBg = computed(() => {
  const score = healthScorePercentage.value
  if (score >= 80) return 'bg-green-100'
  if (score >= 60) return 'bg-yellow-100'
  if (score >= 40) return 'bg-orange-100'
  return 'bg-red-100'
})

const anomalyCount = computed(() => {
  if (!dashboardData.value?.anomalyDetection?.anomalies) return 0
  return dashboardData.value.anomalyDetection.anomalies.length
})

const anomalyRiskLevel = computed(() => {
  if (!dashboardData.value?.anomalyDetection) return 'Low'
  const score = dashboardData.value.anomalyDetection.overallRiskScore
  if (score >= 0.8) return 'Critical'
  if (score >= 0.6) return 'High'
  if (score >= 0.4) return 'Medium'
  return 'Low'
})

const anomalyRiskColor = computed(() => {
  const level = anomalyRiskLevel.value
  const colors: Record<string, string> = {
    'Critical': 'text-red-600',
    'High': 'text-orange-600',
    'Medium': 'text-yellow-600',
    'Low': 'text-green-600'
  }
  return colors[level] || 'text-gray-600'
})

// Methods
const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Failed to load machines:', error)
  }
}

const loadDashboard = async () => {
  if (!selectedMachineId.value) return
  
  try {
    loading.value = true
    dashboardData.value = await getAdvancedAnalyticsDashboard(selectedMachineId.value)
    renderCharts()
    toast.success('Analytics dashboard loaded successfully')
  } catch (error) {
    console.error('Failed to load dashboard:', error)
    toast.error('Failed to load analytics dashboard')
  } finally {
    loading.value = false
  }
}

const runEnsemblePrediction = async () => {
  if (!selectedMachineId.value) return
  
  try {
    loading.value = true
    const prediction = await ensemblePrediction(selectedMachineId.value)
    
    // Update dashboard with new prediction
    if (dashboardData.value) {
      dashboardData.value.prediction = prediction
    }
    
    toast.success('Ensemble prediction completed')
  } catch (error) {
    console.error('Failed to run ensemble prediction:', error)
    toast.error('Failed to run ensemble prediction')
  } finally {
    loading.value = false
  }
}

const runAnomalyDetection = async () => {
  if (!selectedMachineId.value) return
  
  try {
    loading.value = true
    const result = await detectAnomalies(selectedMachineId.value, {
      startTime: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
      endTime: new Date().toISOString()
    })
    
    // Update dashboard with new anomaly results
    if (dashboardData.value) {
      dashboardData.value.anomalyDetection = result
    }
    
    toast.success('Anomaly detection completed')
  } catch (error) {
    console.error('Failed to run anomaly detection:', error)
    toast.error('Failed to run anomaly detection')
  } finally {
    loading.value = false
  }
}

const runForecasting = async () => {
  if (!selectedMachineId.value) return
  
  try {
    loading.value = true
    const result = await forecastTimeSeries(
      selectedMachineId.value,
      forecastForm.value.metric,
      { forecastHorizon: forecastForm.value.horizon }
    )
    
    renderForecastChart(result)
    toast.success('Forecasting completed')
  } catch (error) {
    console.error('Failed to run forecasting:', error)
    toast.error('Failed to run forecasting')
  } finally {
    loading.value = false
  }
}

const generateRecommendations = async () => {
  if (!selectedMachineId.value) return
  
  try {
    loading.value = true
    const recommendation = await generateMaintenanceRecommendation(selectedMachineId.value, {
      costFactors: {
        preventiveMaintenanceCost: recommendationForm.value.preventiveMaintenanceCost,
        reactiveFailureCost: recommendationForm.value.reactiveFailureCost,
        downtimeCostPerHour: recommendationForm.value.downtimeCostPerHour
      },
      businessImpact: {
        productionLossPerHour: 10000,
        customerImpact: 5
      },
      timeConstraints: {}
    })
    
    // Update dashboard with new recommendation
    if (dashboardData.value) {
      dashboardData.value.maintenanceRecommendation = recommendation
    }
    
    renderRecommendationChart(recommendation)
    toast.success('Maintenance recommendations generated')
  } catch (error) {
    console.error('Failed to generate recommendations:', error)
    toast.error('Failed to generate recommendations')
  } finally {
    loading.value = false
  }
}

const renderCharts = () => {
  renderHealthChart()
  renderAnomalyChart()
  renderRecommendationChart(dashboardData.value?.maintenanceRecommendation)
}

const renderHealthChart = () => {
  if (!healthChartRef.value || !dashboardData.value) return
  
  if (!healthChartInstance) {
    healthChartInstance = echarts.init(healthChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Machine Health Score',
      left: 'center'
    },
    series: [
      {
        type: 'gauge',
        center: ['50%', '60%'],
        startAngle: 200,
        endAngle: -20,
        min: 0,
        max: 100,
        splitNumber: 5,
        itemStyle: {
          color: '#5470c6'
        },
        progress: {
          show: true,
          width: 12
        },
        pointer: {
          show: false
        },
        axisLine: {
          lineStyle: {
            width: 12
          }
        },
        axisTick: {
          distance: -20,
          splitNumber: 5,
          lineStyle: {
            width: 1,
            color: '#999'
          }
        },
        splitLine: {
          distance: -25,
          length: 10,
          lineStyle: {
            width: 2,
            color: '#999'
          }
        },
        axisLabel: {
          distance: -10,
          color: '#999',
          fontSize: 10
        },
        anchor: {
          show: false
        },
        title: {
          show: false
        },
        detail: {
          valueAnimation: true,
          width: '60%',
          lineHeight: 30,
          borderRadius: 8,
          offsetCenter: [0, '5%'],
          fontSize: 20,
          fontWeight: 'bolder',
          formatter: '{value}%',
          color: 'inherit'
        },
        data: [
          {
            value: healthScorePercentage.value
          }
        ]
      }
    ]
  }
  
  healthChartInstance.setOption(option, true)
}

const renderAnomalyChart = () => {
  if (!anomalyChartRef.value || !dashboardData.value?.anomalyDetection?.anomalies) return
  
  if (!anomalyChartInstance) {
    anomalyChartInstance = echarts.init(anomalyChartRef.value)
  }
  
  const anomalies = dashboardData.value.anomalyDetection.anomalies
  const severityCounts = {
    critical: anomalies.filter((a: any) => a.severity === 'critical').length,
    high: anomalies.filter((a: any) => a.severity === 'high').length,
    medium: anomalies.filter((a: any) => a.severity === 'medium').length,
    low: anomalies.filter((a: any) => a.severity === 'low').length
  }
  
  const option = {
    title: {
      text: 'Anomaly Distribution by Severity',
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    legend: {
      bottom: 10
    },
    series: [
      {
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 20,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: [
          { value: severityCounts.critical, name: 'Critical', itemStyle: { color: '#ef4444' } },
          { value: severityCounts.high, name: 'High', itemStyle: { color: '#f97316' } },
          { value: severityCounts.medium, name: 'Medium', itemStyle: { color: '#eab308' } },
          { value: severityCounts.low, name: 'Low', itemStyle: { color: '#22c55e' } }
        ]
      }
    ]
  }
  
  anomalyChartInstance.setOption(option, true)
}

const renderForecastChart = (result: ForecastResult) => {
  if (!forecastChartRef.value) return
  
  if (!forecastChartInstance) {
    forecastChartInstance = echarts.init(forecastChartRef.value)
  }
  
  const option = {
    title: {
      text: `Forecast: ${result.metric}`,
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      data: ['Actual', 'Forecast', 'Confidence Interval'],
      bottom: 10
    },
    xAxis: {
      type: 'category',
      data: result.timestamps.map(t => new Date(t).toLocaleDateString())
    },
    yAxis: {
      type: 'value'
    },
    series: [
      {
        name: 'Forecast',
        type: 'line',
        data: result.forecastedValues,
        smooth: true,
        itemStyle: { color: '#3b82f6' }
      },
      {
        name: 'Confidence Interval',
        type: 'line',
        data: result.confidenceIntervals.map(ci => ci.upper),
        smooth: true,
        lineStyle: { opacity: 0 },
        areaStyle: {
          opacity: 0.1,
          color: '#3b82f6'
        },
        stack: 'confidence'
      },
      {
        name: 'Confidence Interval',
        type: 'line',
        data: result.confidenceIntervals.map(ci => ci.lower),
        smooth: true,
        lineStyle: { opacity: 0 },
        areaStyle: {
          opacity: 0.1,
          color: '#3b82f6'
        },
        stack: 'confidence'
      }
    ]
  }
  
  forecastChartInstance.setOption(option, true)
}

const renderRecommendationChart = (recommendation: PrescriptiveRecommendation | undefined) => {
  if (!recommendationChartRef.value || !recommendation) return
  
  if (!recommendationChartInstance) {
    recommendationChartInstance = echarts.init(recommendationChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Maintenance Recommendation Impact',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: ['Cost Savings', 'Risk Reduction', 'Priority Score']
    },
    yAxis: {
      type: 'value'
    },
    series: [
      {
        type: 'bar',
        data: [
          { value: recommendation.expectedCost, itemStyle: { color: '#10b981' } },
          { value: recommendation.riskReduction, itemStyle: { color: '#3b82f6' } },
          { value: recommendation.priorityScore, itemStyle: { color: '#f59e0b' } }
        ],
        label: {
          show: true,
          position: 'top'
        }
      }
    ]
  }
  
  recommendationChartInstance.setOption(option, true)
}

const exportReport = () => {
  toast.info('Export functionality coming soon')
}

const resizeCharts = () => {
  healthChartInstance?.resize()
  anomalyChartInstance?.resize()
  forecastChartInstance?.resize()
  recommendationChartInstance?.resize()
}

// Watch for machine selection changes
watch(selectedMachineId, () => {
  if (selectedMachineId.value) {
    loadDashboard()
  }
})

// Lifecycle
onMounted(async () => {
  window.addEventListener('resize', resizeCharts)
  await loadMachines()
})

// Cleanup
onUnmounted(() => {
  window.removeEventListener('resize', resizeCharts)
  healthChartInstance?.dispose()
  anomalyChartInstance?.dispose()
  forecastChartInstance?.dispose()
  recommendationChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="advanced-analytics space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Advanced Analytics Visualization</h1>
          <p class="text-gray-600 mt-2">Comprehensive predictive and prescriptive analytics dashboard</p>
        </div>
        <div class="flex gap-2">
          <BaseSelect
            v-model="selectedMachineId"
            :options="[{ label: 'Select Machine', value: '' }, ...machines.map((m: any) => ({ label: m.name, value: m.id }))] as any"
            class="w-64"
          />
          <BaseButton 
            variant="outline" 
            @click="loadDashboard"
            :disabled="!selectedMachineId || loading"
          >
            <RefreshCw 
              class="w-4 h-4 mr-2" 
              :class="{ 'animate-spin': loading }"
            />
            Refresh
          </BaseButton>
        </div>
      </div>

      <!-- Machine Selection Prompt -->
      <div v-if="!selectedMachineId" class="text-center py-12">
        <Brain class="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <h3 class="text-xl font-semibold text-gray-900 mb-2">Select a Machine to Begin</h3>
        <p class="text-gray-600">Choose a machine to access advanced analytics capabilities including ensemble predictions, anomaly detection, and prescriptive recommendations.</p>
      </div>

      <!-- Dashboard Content -->
      <template v-else-if="dashboardData">
        <!-- Health Overview -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <BaseCard>
            <div class="p-4">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-600">Health Score</p>
                  <p class="text-2xl font-bold" :class="healthScoreColor">
                    {{ healthScorePercentage }}%
                  </p>
                </div>
                <Zap class="w-8 h-8 text-blue-500" />
              </div>
            </div>
          </BaseCard>
          
          <BaseCard>
            <div class="p-4">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-600">Anomalies Detected</p>
                  <p class="text-2xl font-bold">{{ anomalyCount }}</p>
                </div>
                <AlertTriangle class="w-8 h-8 text-orange-500" />
              </div>
            </div>
          </BaseCard>
          
          <BaseCard>
            <div class="p-4">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-600">Risk Level</p>
                  <p class="text-2xl font-bold" :class="anomalyRiskColor">
                    {{ anomalyRiskLevel }}
                  </p>
                </div>
                <TrendingUp class="w-8 h-8 text-purple-500" />
              </div>
            </div>
          </BaseCard>
          
          <BaseCard>
            <div class="p-4">
              <div class="flex items-center justify-between">
                <div>
                  <p class="text-sm text-gray-600">Last Updated</p>
                  <p class="text-sm font-medium">
                    {{ new Date(dashboardData.generatedAt).toLocaleTimeString() }}
                  </p>
                </div>
                <Calendar class="w-8 h-8 text-green-500" />
              </div>
            </div>
          </BaseCard>
        </div>

        <!-- Main Charts Grid -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Health Score Gauge -->
          <BaseCard>
            <div class="p-6">
              <div ref="healthChartRef" class="w-full h-80"></div>
            </div>
          </BaseCard>
          
          <!-- Anomaly Distribution -->
          <BaseCard>
            <div class="p-6">
              <div ref="anomalyChartRef" class="w-full h-80"></div>
            </div>
          </BaseCard>
        </div>

        <!-- Analytics Actions Section -->
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-6">Advanced Analytics Actions</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Ensemble Prediction -->
              <div class="space-y-4">
                <h3 class="font-medium flex items-center gap-2">
                  <Brain class="w-5 h-5 text-blue-500" />
                  Ensemble Prediction
                </h3>
                <p class="text-sm text-gray-600">
                  Combine multiple ML models for improved prediction accuracy
                </p>
                <BaseButton 
                  variant="primary" 
                  @click="runEnsemblePrediction"
                  :disabled="loading"
                  size="sm"
                >
                  <Play class="w-4 h-4 mr-2" />
                  Run Prediction
                </BaseButton>
              </div>
              
              <!-- Anomaly Detection -->
              <div class="space-y-4">
                <h3 class="font-medium flex items-center gap-2">
                  <AlertTriangle class="w-5 h-5 text-orange-500" />
                  Anomaly Detection
                </h3>
                <p class="text-sm text-gray-600">
                  Detect unusual patterns and potential equipment issues
                </p>
                <BaseButton 
                  variant="primary" 
                  @click="runAnomalyDetection"
                  :disabled="loading"
                  size="sm"
                >
                  <Play class="w-4 h-4 mr-2" />
                  Detect Anomalies
                </BaseButton>
              </div>
              
              <!-- Time Series Forecasting -->
              <div class="space-y-4">
                <h3 class="font-medium flex items-center gap-2">
                  <LineChart class="w-5 h-5 text-purple-500" />
                  Time Series Forecasting
                </h3>
                <div class="space-y-3">
                  <BaseSelect
                    v-model="forecastForm.metric"
                    :options="[
                      { label: 'Temperature', value: 'temperature' },
                      { label: 'Pressure', value: 'pressure' },
                      { label: 'Vibration', value: 'vibration' },
                      { label: 'Flow Rate', value: 'flow_rate' }
                    ]"
                    size="sm"
                  />
                  <BaseInput
                    v-model.number="forecastForm.horizon"
                    label="Forecast Horizon (days)"
                    type="number"
                    min="1"
                    max="365"
                    size="sm"
                  />
                  <BaseButton 
                    variant="primary" 
                    @click="runForecasting"
                    :disabled="loading"
                    size="sm"
                  >
                    <Play class="w-4 h-4 mr-2" />
                    Generate Forecast
                  </BaseButton>
                </div>
              </div>
              
              <!-- Maintenance Recommendations -->
              <div class="space-y-4">
                <h3 class="font-medium flex items-center gap-2">
                  <Calendar class="w-5 h-5 text-green-500" />
                  Maintenance Recommendations
                </h3>
                <div class="space-y-3">
                  <BaseInput
                    v-model.number="recommendationForm.preventiveMaintenanceCost"
                    label="Preventive Cost ($)"
                    type="number"
                    size="sm"
                  />
                  <BaseInput
                    v-model.number="recommendationForm.reactiveFailureCost"
                    label="Reactive Cost ($)"
                    type="number"
                    size="sm"
                  />
                  <BaseInput
                    v-model.number="recommendationForm.downtimeCostPerHour"
                    label="Downtime Cost/Hour ($)"
                    type="number"
                    size="sm"
                  />
                  <BaseButton 
                    variant="primary" 
                    @click="generateRecommendations"
                    :disabled="loading"
                    size="sm"
                  >
                    <Play class="w-4 h-4 mr-2" />
                    Generate Recommendations
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>
        </BaseCard>

        <!-- Forecast Chart (shows when available) -->
        <BaseCard v-if="forecastChartInstance">
          <div class="p-6">
            <div ref="forecastChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>

        <!-- Recommendation Impact Chart -->
        <BaseCard v-if="dashboardData.maintenanceRecommendation">
          <div class="p-6">
            <div ref="recommendationChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>

        <!-- Action Buttons -->
        <div class="flex justify-end gap-3">
          <BaseButton variant="outline" @click="exportReport">
            <Download class="w-4 h-4 mr-2" />
            Export Report
          </BaseButton>
        </div>
      </template>

      <!-- Loading State -->
      <div v-else-if="loading" class="text-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500 mx-auto mb-4"></div>
        <p class="text-gray-600">Loading advanced analytics dashboard...</p>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.advanced-analytics {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .advanced-analytics {
    padding: 0.5rem;
  }
}
</style>