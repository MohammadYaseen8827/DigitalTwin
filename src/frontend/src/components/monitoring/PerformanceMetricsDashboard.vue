<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  getPerformanceMetrics,
  getPerformanceStatistics,
  getThresholdViolations,
  type PerformanceMetrics,
  type PerformanceStatistics,
  type MetricsFilter
} from '@/services/performanceMetrics.service'
import { 
  Activity, 
  BarChart3, 
  TrendingUp, 
  AlertTriangle,
  Clock,
  Filter,
  RefreshCw,
  Zap,
  Database,
  Server
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
const metrics: any = ref<PerformanceMetrics[]>([])
const statistics: any = ref<PerformanceStatistics | null>(null)
const loading = ref(false)
const autoRefresh = ref(true)
const refreshInterval = ref<NodeJS.Timeout | null>(null)
const timeRange = ref('1h')
const operationFilter = ref('all')

// Chart refs
const durationChartRef = ref<HTMLDivElement | null>(null)
const operationsChartRef = ref<HTMLDivElement | null>(null)
const violationsChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let durationChartInstance: echarts.ECharts | null = null
let operationsChartInstance: echarts.ECharts | null = null
let violationsChartInstance: echarts.ECharts | null = null

// Computed
const filteredMetrics = computed(() => {
  let filtered = [...metrics.value]
  
  if (operationFilter.value !== 'all') {
    filtered = filtered.filter((metric: any) => metric.operationName === operationFilter.value)
  }
  
  return filtered
})

const operationOptions = computed(() => {
  const operations = [...new Set(metrics.value.map((m: any) => m.operationName))]
  return [
    { label: 'All Operations', value: 'all' },
    ...operations.map((op: unknown) => ({ label: String(op), value: String(op) }))
  ]
})

const timeRangeOptions = [
  { label: 'Last 15 Minutes', value: '15m' },
  { label: 'Last 1 Hour', value: '1h' },
  { label: 'Last 6 Hours', value: '6h' },
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' }
]

const thresholdViolations = computed(() => {
  return metrics.value.filter((m: any) => m.exceededThreshold)
})

const slowOperations = computed(() => {
  return metrics.value.filter((m: any) => m.durationMs > m.thresholdMs)
})

const errorOperations = computed(() => {
  return metrics.value.filter((m: any) => m.statusCode && m.statusCode >= 400)
})

// Methods
const getTimeRangeDates = (): { startTime: string; endTime: string } => {
  const now = new Date()
  let startTime: Date
  
  switch (timeRange.value) {
    case '15m':
      startTime = new Date(now.getTime() - 15 * 60 * 1000)
      break
    case '1h':
      startTime = new Date(now.getTime() - 60 * 60 * 1000)
      break
    case '6h':
      startTime = new Date(now.getTime() - 6 * 60 * 60 * 1000)
      break
    case '24h':
      startTime = new Date(now.getTime() - 24 * 60 * 60 * 1000)
      break
    case '7d':
      startTime = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
      break
    default:
      startTime = new Date(now.getTime() - 60 * 60 * 1000)
  }
  
  return {
    startTime: startTime.toISOString(),
    endTime: now.toISOString()
  }
}

const loadMetrics = async () => {
  try {
    loading.value = true
    
    const { startTime, endTime } = getTimeRangeDates()
    const filter: MetricsFilter = { startTime, endTime }
    
    const [metricsData, statsData, violationsData] = await Promise.all([
      getPerformanceMetrics(filter),
      getPerformanceStatistics(filter),
      getThresholdViolations(filter)
    ])
    
    metrics.value = metricsData
    statistics.value = statsData
    
    renderCharts()
  } catch (error) {
    console.error('Failed to load performance metrics:', error)
    toast.error('Failed to load performance metrics')
  } finally {
    loading.value = false
  }
}

const refreshMetrics = async () => {
  await loadMetrics()
  toast.success('Performance metrics refreshed')
}

const startAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
  }
  
  refreshInterval.value = setInterval(() => {
    if (autoRefresh.value) {
      loadMetrics()
    }
  }, 60000) // Refresh every minute
}

const stopAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
    refreshInterval.value = null
  }
}

const toggleAutoRefresh = () => {
  autoRefresh.value = !autoRefresh.value
  if (autoRefresh.value) {
    startAutoRefresh()
    toast.success('Auto-refresh enabled')
  } else {
    stopAutoRefresh()
    toast.info('Auto-refresh disabled')
  }
}

const renderCharts = () => {
  renderDurationChart()
  renderOperationsChart()
  renderViolationsChart()
}

const renderDurationChart = () => {
  if (!durationChartRef.value) return
  
  if (!durationChartInstance) {
    durationChartInstance = echarts.init(durationChartRef.value)
  }
  
  const data = filteredMetrics.value
    .slice(-100) // Last 100 points for better performance
    .map((m: any) => ({
      timestamp: new Date(m.timestamp).getTime(),
      duration: m.durationMs,
      operation: m.operationName,
      exceeded: m.exceededThreshold
    }))
    .sort((a, b) => a.timestamp - b.timestamp)
  
  const option = {
    title: {
      text: 'Operation Duration Over Time',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        if (!params || params.length === 0) return ''
        const param = params[0]
        const time = new Date(param.value[0]).toLocaleString()
        const duration = param.value[1]
        const operation = param.seriesName
        const exceeded = param.data.exceeded ? ' (Threshold Exceeded!)' : ''
        return `${time}<br/>${operation}: ${duration}ms${exceeded}`
      }
    },
    legend: {
      type: 'scroll',
      bottom: 10
    },
    xAxis: {
      type: 'time',
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'Duration (ms)'
    },
    series: [{
      name: 'Duration',
      type: 'line',
      data: data.map(d => [d.timestamp, d.duration]),
      itemStyle: {
        color: data.some(d => d.exceeded) ? '#ef4444' : '#3b82f6'
      },
      markLine: {
        silent: true,
        lineStyle: {
          color: '#f59e0b',
          type: 'dashed'
        },
        data: [{
          yAxis: Math.max(...data.map(d => d.duration)) * 0.8,
          name: '80% Threshold'
        }]
      }
    }]
  }
  
  durationChartInstance.setOption(option, true)
}

const renderOperationsChart = () => {
  if (!operationsChartRef.value || !statistics.value) return
  
  if (!operationsChartInstance) {
    operationsChartInstance = echarts.init(operationsChartRef.value)
  }
  
  const commonOps = statistics.value.mostCommonOperations.slice(0, 10)
  
  const option = {
    title: {
      text: 'Most Common Operations',
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
      data: commonOps.map((op: any) => op.operationName),
      name: 'Operation'
    },
    yAxis: {
      type: 'value',
      name: 'Count'
    },
    series: [{
      name: 'Operations',
      type: 'bar',
      data: commonOps.map((op: any) => ({
        value: op.count,
        itemStyle: {
          color: op.averageDurationMs > 1000 ? '#ef4444' : '#10b981'
        }
      })),
      label: {
        show: true,
        position: 'top'
      }
    }]
  }
  
  operationsChartInstance.setOption(option, true)
}

const renderViolationsChart = () => {
  if (!violationsChartRef.value) return
  
  if (!violationsChartInstance) {
    violationsChartInstance = echarts.init(violationsChartRef.value)
  }
  
  const violationsByOperation = thresholdViolations.value.reduce((acc: any, violation: any) => {
    if (!acc[violation.operationName]) {
      acc[violation.operationName] = 0
    }
    acc[violation.operationName]++
    return acc
  }, {})
  
  const data = Object.entries(violationsByOperation)
    .map(([operation, count]) => ({ operation, count }))
    .sort((a, b) => (b.count as number) - (a.count as number))
    .slice(0, 10)
  
  const option = {
    title: {
      text: 'Threshold Violations by Operation',
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      data: data.map(d => ({
        name: d.operation,
        value: d.count
      })),
      label: {
        show: true,
        formatter: '{b}: {c} ({d}%)'
      }
    }]
  }
  
  violationsChartInstance.setOption(option, true)
}

const formatTime = (dateString: string): string => {
  return new Date(dateString).toLocaleString()
}

const formatDuration = (ms: number): string => {
  if (ms < 1000) return `${ms}ms`
  if (ms < 60000) return `${(ms / 1000).toFixed(2)}s`
  return `${(ms / 60000).toFixed(2)}m`
}

const resizeCharts = () => {
  durationChartInstance?.resize()
  operationsChartInstance?.resize()
  violationsChartInstance?.resize()
}

// Lifecycle
onMounted(async () => {
  window.addEventListener('resize', resizeCharts)
  await loadMetrics()
  startAutoRefresh()
})

onUnmounted(() => {
  window.removeEventListener('resize', resizeCharts)
  stopAutoRefresh()
  durationChartInstance?.dispose()
  operationsChartInstance?.dispose()
  violationsChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="performance-metrics space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Performance Metrics</h1>
          <p class="text-gray-600 mt-2">Monitor system performance and identify bottlenecks</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="toggleAutoRefresh"
            :class="{ 'bg-blue-50 text-blue-700': autoRefresh }"
          >
            <RefreshCw 
              class="w-4 h-4 mr-2" 
              :class="{ 'animate-spin': autoRefresh }"
            />
            {{ autoRefresh ? 'Auto Refresh ON' : 'Auto Refresh OFF' }}
          </BaseButton>
          <BaseButton variant="outline" @click="refreshMetrics" :disabled="loading">
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
            Refresh
          </BaseButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Operations</p>
                <p class="text-2xl font-bold">{{ statistics?.totalOperations?.toLocaleString() || 0 }}</p>
              </div>
              <Activity class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Avg Duration</p>
                <p class="text-2xl font-bold">{{ formatDuration(statistics?.averageDurationMs || 0) }}</p>
              </div>
              <Clock class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Violations</p>
                <p class="text-2xl font-bold">{{ thresholdViolations.length }}</p>
              </div>
              <AlertTriangle class="w-8 h-8 text-red-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Error Rate</p>
                <p class="text-2xl font-bold">{{ (statistics?.errorRate || 0).toFixed(2) }}%</p>
              </div>
              <Zap class="w-8 h-8 text-orange-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Filters -->
      <BaseCard>
        <div class="p-4 space-y-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <BaseSelect
              v-model="timeRange"
              :options="timeRangeOptions"
              @change="loadMetrics"
            />
            
            <BaseSelect
              v-model="operationFilter"
              :options="operationOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Duration Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="durationChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <!-- Operations Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="operationsChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Violations Chart -->
      <BaseCard>
        <div class="p-6">
          <div ref="violationsChartRef" class="w-full h-80"></div>
        </div>
      </BaseCard>

      <!-- Detailed Metrics Table -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Recent Operations ({{ filteredMetrics.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading performance metrics...</p>
          </div>
          
          <div v-else-if="filteredMetrics.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No performance metrics found</p>
            <p class="text-sm text-gray-500 mt-1">Try adjusting your filters or time range</p>
          </div>
          
          <div v-else class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Timestamp
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Operation
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Duration
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Threshold
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="(metric, index) in filteredMetrics.slice(0, 50)" 
                  :key="`${metric.id}-${index}`"
                  class="hover:bg-gray-50"
                  :class="{ 'bg-red-50': metric.exceededThreshold }"
                >
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatTime(metric.timestamp) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ metric.operationName }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <span :class="metric.exceededThreshold ? 'text-red-600' : 'text-gray-900'">
                      {{ formatDuration(metric.durationMs) }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm">
                    <span 
                      class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      :class="metric.statusCode && metric.statusCode >= 400 ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'"
                    >
                      {{ metric.statusCode || '200' }}
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ formatDuration(metric.thresholdMs) }}
                    <span v-if="metric.exceededThreshold" class="text-red-600 ml-1">⚠️</span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.performance-metrics {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .performance-metrics {
    padding: 0.5rem;
  }
}
</style>