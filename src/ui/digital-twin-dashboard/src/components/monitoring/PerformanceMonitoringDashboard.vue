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
  getRecentMetrics,
  type PerformanceMetrics,
  type PerformanceStatistics
} from '@/services/performanceMetrics.service'
import { Activity, AlertTriangle, Clock, BarChart3, TrendingUp, Zap, Filter, RefreshCw } from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  BarChart as EChartsBar,
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
const loading = ref(false)
const loadingStats = ref(false)
const autoRefreshEnabled = ref(true)
const refreshInterval = ref<NodeJS.Timeout | null>(null)
const timeRange = ref('1h')
const operationFilter = ref('')
const metrics = ref<PerformanceMetrics[]>([])
const statistics = ref<PerformanceStatistics | null>(null)
const violations = ref<PerformanceMetrics[]>([])

// Chart refs
const durationChartRef = ref<HTMLDivElement | null>(null)
const operationsChartRef = ref<HTMLDivElement | null>(null)
const violationsChartRef = ref<HTMLDivElement | null>(null)
const throughputChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let durationChartInstance: echarts.ECharts | null = null
let operationsChartInstance: echarts.ECharts | null = null
let violationsChartInstance: echarts.ECharts | null = null
let throughputChartInstance: echarts.ECharts | null = null

// Options
const timeRangeOptions = [
  { label: 'Last 15 minutes', value: '15m' },
  { label: 'Last 1 hour', value: '1h' },
  { label: 'Last 6 hours', value: '6h' },
  { label: 'Last 24 hours', value: '24h' },
  { label: 'Last 7 days', value: '7d' }
]

// Computed
const avgDurationFormatted = computed(() => {
  if (!statistics.value) return '0ms'
  return `${statistics.value.averageDurationMs.toFixed(2)}ms`
})

const errorRateFormatted = computed(() => {
  if (!statistics.value) return '0%'
  return `${(statistics.value.errorRate * 100).toFixed(2)}%`
})

const throughputFormatted = computed(() => {
  if (!statistics.value) return '0'
  return statistics.value.throughputPerMinute.toFixed(2)
})

const violationCount = computed(() => {
  return violations.value.length
})

const slowOperationsCount = computed(() => {
  if (!statistics.value) return 0
  return statistics.value.slowestOperations.length
})

const getOperationNames = computed(() => {
  const names = [...new Set(metrics.value.map(m => m.operationName))]
  return names.map(name => ({ label: name, value: name }))
})

// Methods
const getTimeRangeDates = () => {
  const now = new Date()
  let past: Date
  
  switch (timeRange.value) {
    case '15m':
      past = new Date(now.getTime() - 15 * 60 * 1000)
      break
    case '1h':
      past = new Date(now.getTime() - 60 * 60 * 1000)
      break
    case '6h':
      past = new Date(now.getTime() - 6 * 60 * 60 * 1000)
      break
    case '24h':
      past = new Date(now.getTime() - 24 * 60 * 60 * 1000)
      break
    case '7d':
      past = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
      break
    default:
      past = new Date(now.getTime() - 60 * 60 * 1000)
  }
  
  return {
    startTime: past.toISOString(),
    endTime: now.toISOString()
  }
}

const loadMetrics = async () => {
  try {
    loading.value = true
    
    const { startTime, endTime } = getTimeRangeDates()
    const filter = {
      startTime,
      endTime,
      operationName: operationFilter.value || undefined
    }
    
    const [metricsData, violationsData] = await Promise.all([
      getPerformanceMetrics(filter),
      getThresholdViolations(filter)
    ])
    
    metrics.value = metricsData
    violations.value = violationsData
    
    renderDurationChart()
    renderOperationsChart()
    renderViolationsChart()
    renderThroughputChart()
    
    toast.success(`Loaded ${metricsData.length} performance metrics`)
  } catch (error) {
    console.error('Failed to load metrics:', error)
    toast.error('Failed to load performance metrics')
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    loadingStats.value = true
    
    const { startTime, endTime } = getTimeRangeDates()
    const filter = {
      startTime,
      endTime,
      operationName: operationFilter.value || undefined
    }
    
    statistics.value = await getPerformanceStatistics(filter)
    
    toast.success('Statistics updated')
  } catch (error) {
    console.error('Failed to load statistics:', error)
    toast.error('Failed to load performance statistics')
  } finally {
    loadingStats.value = false
  }
}

const refreshAll = async () => {
  await Promise.all([
    loadMetrics(),
    loadStatistics()
  ])
}

const toggleAutoRefresh = () => {
  autoRefreshEnabled.value = !autoRefreshEnabled.value
  
  if (autoRefreshEnabled.value) {
    startAutoRefresh()
    toast.success('Auto-refresh enabled (30s intervals)')
  } else {
    stopAutoRefresh()
    toast.info('Auto-refresh disabled')
  }
}

const startAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
  }
  
  refreshInterval.value = setInterval(() => {
    refreshAll()
  }, 30000) // 30 seconds
}

const stopAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
    refreshInterval.value = null
  }
}

const renderDurationChart = () => {
  if (!durationChartRef.value || metrics.value.length === 0) return
  
  if (!durationChartInstance) {
    durationChartInstance = echarts.init(durationChartRef.value)
  }
  
  const timestamps = metrics.value.map(m => {
    const date = new Date(m.timestamp)
    return `${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`
  })
  
  const durations = metrics.value.map(m => m.durationMs)
  const thresholds = metrics.value.map(m => m.thresholdMs)
  
  const option = {
    title: {
      text: 'Operation Duration Trends',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30,
      data: ['Duration (ms)', 'Threshold (ms)']
    },
    xAxis: {
      type: 'category',
      data: timestamps,
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'Milliseconds'
    },
    dataZoom: [
      {
        type: 'inside',
        start: Math.max(0, (timestamps.length - 50) / timestamps.length * 100),
        end: 100
      }
    ],
    series: [
      {
        name: 'Duration (ms)',
        type: 'line',
        data: durations,
        smooth: true,
        itemStyle: { color: '#3b82f6' }
      },
      {
        name: 'Threshold (ms)',
        type: 'line',
        data: thresholds,
        smooth: true,
        itemStyle: { color: '#f59e0b' },
        lineStyle: { type: 'dashed' }
      }
    ]
  }
  
  durationChartInstance.setOption(option, true)
}

const renderOperationsChart = () => {
  if (!operationsChartRef.value || !statistics.value) return
  
  if (!operationsChartInstance) {
    operationsChartInstance = echarts.init(operationsChartRef.value)
  }
  
  const operations = statistics.value.mostCommonOperations.slice(0, 10)
  const names = operations.map(op => op.operationName)
  const counts = operations.map(op => op.count)
  
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
      data: names,
      axisLabel: {
        rotate: 45
      }
    },
    yAxis: {
      type: 'value',
      name: 'Count'
    },
    series: [{
      name: 'Operations',
      type: 'bar',
      data: counts,
      itemStyle: {
        color: '#10b981'
      }
    }]
  }
  
  operationsChartInstance.setOption(option, true)
}

const renderViolationsChart = () => {
  if (!violationsChartRef.value || violations.value.length === 0) return
  
  if (!violationsChartInstance) {
    violationsChartInstance = echarts.init(violationsChartRef.value)
  }
  
  const violationCounts: Record<string, number> = {}
  violations.value.forEach(v => {
    violationCounts[v.operationName] = (violationCounts[v.operationName] || 0) + 1
  })
  
  const names = Object.keys(violationCounts)
  const counts = Object.values(violationCounts)
  
  const option = {
    title: {
      text: 'Threshold Violations by Operation',
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
      data: names
    },
    series: [
      {
        name: 'Violations',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        data: names.map((name, index) => ({
          name,
          value: counts[index],
          itemStyle: { color: index % 2 === 0 ? '#ef4444' : '#f97316' }
        })),
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
  
  violationsChartInstance.setOption(option, true)
}

const renderThroughputChart = () => {
  if (!throughputChartRef.value || metrics.value.length === 0) return
  
  if (!throughputChartInstance) {
    throughputChartInstance = echarts.init(throughputChartRef.value)
  }
  
  // Group by minute for throughput calculation
  const minuteGroups: Record<string, number> = {}
  metrics.value.forEach(m => {
    const minute = new Date(m.timestamp).toISOString().substring(0, 16) // YYYY-MM-DDTHH:mm
    minuteGroups[minute] = (minuteGroups[minute] || 0) + 1
  })
  
  const minutes = Object.keys(minuteGroups).sort()
  const throughput = minutes.map(minute => minuteGroups[minute])
  
  const option = {
    title: {
      text: 'Operations Per Minute',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: minutes.map(m => m.substring(11)), // Just HH:mm
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'Operations'
    },
    series: [{
      name: 'Throughput',
      type: 'line',
      data: throughput,
      smooth: true,
      itemStyle: { color: '#8b5cf6' },
      areaStyle: { opacity: 0.3 }
    }]
  }
  
  throughputChartInstance.setOption(option, true)
}

const formatDate = (timestamp: string) => {
  return new Date(timestamp).toLocaleString()
}

const getDurationColor = (duration: number, threshold: number) => {
  if (duration > threshold) return 'text-red-600'
  if (duration > threshold * 0.8) return 'text-yellow-600'
  return 'text-green-600'
}

const resizeCharts = () => {
  durationChartInstance?.resize()
  operationsChartInstance?.resize()
  violationsChartInstance?.resize()
  throughputChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  refreshAll()
  if (autoRefreshEnabled.value) {
    startAutoRefresh()
  }
  window.addEventListener('resize', resizeCharts)
})

onUnmounted(() => {
  stopAutoRefresh()
  window.removeEventListener('resize', resizeCharts)
  durationChartInstance?.dispose()
  operationsChartInstance?.dispose()
  violationsChartInstance?.dispose()
  throughputChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="performance-monitoring space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Performance Monitoring</h1>
          <p class="text-gray-600 mt-2">Real-time system performance metrics and threshold monitoring</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="toggleAutoRefresh"
            :class="{ 'bg-blue-50 border-blue-200': autoRefreshEnabled }"
          >
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': autoRefreshEnabled }" />
            Auto Refresh: {{ autoRefreshEnabled ? 'ON' : 'OFF' }}
          </BaseButton>
          
          <BaseButton 
            variant="primary" 
            @click="refreshAll"
            :disabled="loading || loadingStats"
          >
            <Activity class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading || loadingStats }" />
            {{ (loading || loadingStats) ? 'Refreshing...' : 'Refresh All' }}
          </BaseButton>
        </div>
      </div>

      <!-- Filters -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Filter Options</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <BaseSelect
              v-model="timeRange"
              :options="timeRangeOptions"
              label="Time Range"
            />
            
            <BaseSelect
              v-model="operationFilter"
              :options="[{ label: 'All Operations', value: '' }, ...getOperationNames]"
              label="Operation Filter"
            />
            
            <div class="flex items-end">
              <BaseButton 
                variant="outline" 
                @click="refreshAll"
                :disabled="loading || loadingStats"
                class="w-full"
              >
                <Filter class="w-4 h-4 mr-2" />
                Apply Filters
              </BaseButton>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <Clock class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ avgDurationFormatted }}</p>
            <p class="text-sm text-gray-600">Avg. Duration</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <AlertTriangle class="w-8 h-8 text-red-500 mx-auto mb-2" />
            <p class="text-2xl font-bold text-red-600">{{ violationCount }}</p>
            <p class="text-sm text-gray-600">Threshold Violations</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Zap class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ errorRateFormatted }}</p>
            <p class="text-sm text-gray-600">Error Rate</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <TrendingUp class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ throughputFormatted }}</p>
            <p class="text-sm text-gray-600">Ops/Minute</p>
          </div>
        </BaseCard>
      </div>

      <!-- Statistics -->
      <BaseCard v-if="statistics">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Performance Statistics</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <h3 class="font-medium mb-3">Duration Metrics</h3>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Min Duration</span>
                  <span>{{ statistics.minDurationMs.toFixed(2) }}ms</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Max Duration</span>
                  <span>{{ statistics.maxDurationMs.toFixed(2) }}ms</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">95th Percentile</span>
                  <span>{{ statistics.percentile95DurationMs.toFixed(2) }}ms</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">99th Percentile</span>
                  <span>{{ statistics.percentile99DurationMs.toFixed(2) }}ms</span>
                </div>
              </div>
            </div>
            
            <div>
              <h3 class="font-medium mb-3">Operation Counts</h3>
              <div class="space-y-2">
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Total Operations</span>
                  <span class="font-medium">{{ statistics.totalOperations }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Slow Operations</span>
                  <span class="font-medium text-orange-600">{{ slowOperationsCount }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Error Rate</span>
                  <span class="font-medium" :class="statistics.errorRate > 0.05 ? 'text-red-600' : 'text-green-600'">
                    {{ (statistics.errorRate * 100).toFixed(2) }}%
                  </span>
                </div>
                <div class="flex justify-between">
                  <span class="text-sm text-gray-600">Throughput</span>
                  <span class="font-medium">{{ statistics.throughputPerMinute.toFixed(2) }}/min</span>
                </div>
              </div>
            </div>
            
            <div>
              <h3 class="font-medium mb-3">Top Operations</h3>
              <div class="space-y-2 max-h-32 overflow-y-auto">
                <div
                  v-for="(op, index) in statistics.mostCommonOperations.slice(0, 5)"
                  :key="index"
                  class="flex justify-between text-sm"
                >
                  <span class="truncate">{{ op.operationName }}</span>
                  <span class="font-medium">{{ op.count }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Duration Trends</h3>
            <div ref="durationChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Common Operations</h3>
            <div ref="operationsChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Threshold Violations</h3>
            <div ref="violationsChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Throughput Analysis</h3>
            <div ref="throughputChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Threshold Violations Table -->
      <BaseCard v-if="violations.length > 0">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Recent Threshold Violations</h2>
          
          <div class="overflow-x-auto">
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
                    Threshold
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Excess
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="(violation, index) in violations.slice(0, 10)" 
                  :key="index"
                  class="hover:bg-gray-50"
                >
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatDate(violation.timestamp) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {{ violation.operationName }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <span 
                      class="font-medium"
                      :class="getDurationColor(violation.durationMs, violation.thresholdMs)"
                    >
                      {{ violation.durationMs.toFixed(2) }}ms
                    </span>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {{ violation.thresholdMs }}ms
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm">
                    <span class="px-2 py-1 rounded-full text-xs font-medium bg-red-100 text-red-800">
                      +{{ (violation.durationMs - violation.thresholdMs).toFixed(2) }}ms
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </BaseCard>

      <!-- Empty State -->
      <BaseCard v-if="metrics.length === 0 && !loading">
        <div class="p-12 text-center">
          <Activity class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 class="text-lg font-medium text-gray-900 mb-2">No Performance Data</h3>
          <p class="text-gray-500 mb-6">Select a time range and refresh to load performance metrics.</p>
          <BaseButton @click="refreshAll" variant="primary">
            Load Performance Data
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.performance-monitoring {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .performance-monitoring {
    padding: 0.5rem;
  }
}
</style>