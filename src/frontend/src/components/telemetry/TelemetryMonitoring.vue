<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchTelemetryByMachine,
  fetchRecentTelemetry
} from '@/services/telemetry.service'
import { fetchMachines } from '@/services/machines.service'
import type { TelemetryDto } from '@/api/types/index'
import { 
  Activity, 
  BarChart, 
  LineChart, 
  Gauge, 
  Filter,
  Search,
  RefreshCw,
  Clock,
  Database,
  TrendingUp
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  BarChart as EChartsBar,
  GaugeChart as EChartsGauge
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsLine,
  EChartsBar,
  EChartsGauge,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const telemetryData: any = ref([])
const machines: any = ref([])
const loading = ref(false)
const searchQuery = ref('')
const machineFilter = ref('all')
const timeRange = ref('1h')
const dataTypeFilter = ref('all')
const autoRefresh = ref(true)
const refreshInterval = ref<NodeJS.Timeout | null>(null)

// Chart refs
const lineChartRef = ref<HTMLDivElement | null>(null)
const barChartRef = ref<HTMLDivElement | null>(null)
const gaugeChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let lineChartInstance: echarts.ECharts | null = null
let barChartInstance: echarts.ECharts | null = null
let gaugeChartInstance: echarts.ECharts | null = null

// Computed
const filteredTelemetry = computed(() => {
  let filtered = [...telemetryData.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((item: any) => 
      item.machineId?.toLowerCase().includes(query) ||
      item.dataType?.toLowerCase().includes(query)
    )
  }
  
  // Apply machine filter
  if (machineFilter.value !== 'all') {
    filtered = filtered.filter((item: any) => item.machineId === machineFilter.value)
  }
  
  // Apply data type filter
  if (dataTypeFilter.value !== 'all') {
    filtered = filtered.filter((item: any) => item.dataType === dataTypeFilter.value)
  }
  
  return filtered
})

const machineOptions = computed(() => {
  const machineIds = [...new Set(telemetryData.value.map((t: any) => t.machineId))]
  return [
    { label: 'All Machines', value: 'all' },
    ...machineIds.map((id: unknown) => ({
      label: machines.value.find((m: any) => m.id === id)?.name || String(id),
      value: String(id)
    }))
  ]
})

const dataTypeOptions = computed(() => {
  const dataTypes = [...new Set(telemetryData.value.map((t: any) => t.dataType))]
  return [
    { label: 'All Data Types', value: 'all' },
    ...dataTypes.map((type: unknown) => ({
      label: String(type),
      value: String(type)
    }))
  ]
})

const timeRangeOptions = [
  { label: 'Last 1 Hour', value: '1h' },
  { label: 'Last 6 Hours', value: '6h' },
  { label: 'Last 12 Hours', value: '12h' },
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' }
]

// Metrics
const metrics = computed(() => {
  const recentData = filteredTelemetry.value.slice(0, 100) // Last 100 readings
  const numericValues = recentData
    .flatMap((item: any) => 
      Object.values(item.data || {})
        .filter((val: any) => typeof val === 'number')
    ) as number[]
  
  if (numericValues.length === 0) return null
  
  const sum = numericValues.reduce((a, b) => a + b, 0)
  const avg = sum / numericValues.length
  const min = Math.min(...numericValues)
  const max = Math.max(...numericValues)
  
  return {
    average: avg,
    minimum: min,
    maximum: max,
    count: numericValues.length,
    lastValue: numericValues.length > 0 ? numericValues[numericValues.length - 1] : null
  }
})

// Methods
const loadTelemetry = async () => {
  try {
    loading.value = true
    
    const [recentData, machineData] = await Promise.all([
      fetchRecentTelemetry({ 
        range: timeRange.value,
        limit: 1000 
      }),
      fetchMachines()
    ])
    
    telemetryData.value = recentData
    machines.value = machineData
    
    renderCharts()
  } catch (error) {
    console.error('Failed to load telemetry:', error)
    toast.error('Unable to load telemetry data')
  } finally {
    loading.value = false
  }
}

const refreshTelemetry = async () => {
  await loadTelemetry()
  toast.success('Telemetry data refreshed')
}

const startAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
  }
  
  refreshInterval.value = setInterval(() => {
    if (autoRefresh.value) {
      loadTelemetry()
    }
  }, 30000) // Refresh every 30 seconds
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
  renderLineChart()
  renderBarChart()
  renderGaugeChart()
}

const renderLineChart = () => {
  if (!lineChartRef.value) return
  
  if (!lineChartInstance) {
    lineChartInstance = echarts.init(lineChartRef.value)
  }
  
  // Prepare data for line chart
  const groupedData: Record<string, any[]> = {}
  filteredTelemetry.value.forEach((item: any) => {
    const key = `${item.machineId}-${item.dataType}`
    if (!groupedData[key]) {
      groupedData[key] = []
    }
    
    // Extract numeric values from data
    Object.entries(item.data || {}).forEach(([field, value]) => {
      if (typeof value === 'number') {
        groupedData[key].push({
          timestamp: new Date(item.timestamp).getTime(),
          value,
          field
        })
      }
    })
  })
  
  // Create series for each data group
  const series = Object.entries(groupedData).map(([key, data]) => {
    const [machineId, dataType] = key.split('-')
    return {
      name: `${machines.value.find((m: any) => m.id === machineId)?.name || machineId} - ${dataType}`,
      type: 'line',
      data: data
        .sort((a, b) => a.timestamp - b.timestamp)
        .map(item => [item.timestamp, item.value]),
      smooth: true,
      showSymbol: false
    }
  })
  
  const option = {
    title: {
      text: 'Telemetry Trends Over Time',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      formatter: (params: any) => {
        if (!params || params.length === 0) return ''
        const time = new Date(params[0].value[0]).toLocaleString()
        let html = `<strong>${time}</strong><br/>`
        params.forEach((param: any) => {
          html += `${param.seriesName}: ${param.value[1].toFixed(2)}<br/>`
        })
        return html
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
      name: 'Value'
    },
    series: series.slice(0, 10) // Limit to 10 series for readability
  }
  
  lineChartInstance.setOption(option, true)
}

const renderBarChart = () => {
  if (!barChartRef.value) return
  
  if (!barChartInstance) {
    barChartInstance = echarts.init(barChartRef.value)
  }
  
  // Aggregate data by machine
  const machineStats: Record<string, { count: number; avgValue: number }> = {}
  filteredTelemetry.value.forEach((item: any) => {
    if (!machineStats[item.machineId]) {
      machineStats[item.machineId] = { count: 0, avgValue: 0 }
    }
    
    const numericValues = Object.values(item.data || {})
      .filter((val: any) => typeof val === 'number') as number[]
    
    if (numericValues.length > 0) {
      const avg = numericValues.reduce((a, b) => a + b, 0) / numericValues.length
      machineStats[item.machineId].count += 1
      machineStats[item.machineId].avgValue += avg
    }
  })
  
  // Calculate averages
  Object.values(machineStats).forEach(stat => {
    if (stat.count > 0) {
      stat.avgValue /= stat.count
    }
  })
  
  const option = {
    title: {
      text: 'Telemetry Volume by Machine',
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
      data: Object.keys(machineStats).map(id => 
        machines.value.find((m: any) => m.id === id)?.name || id
      ),
      name: 'Machine'
    },
    yAxis: [
      {
        type: 'value',
        name: 'Reading Count'
      },
      {
        type: 'value',
        name: 'Average Value',
        position: 'right'
      }
    ],
    series: [
      {
        name: 'Reading Count',
        type: 'bar',
        data: Object.values(machineStats).map(stat => stat.count),
        itemStyle: {
          color: '#5470c6'
        }
      },
      {
        name: 'Average Value',
        type: 'line',
        yAxisIndex: 1,
        data: Object.values(machineStats).map(stat => stat.avgValue.toFixed(2)),
        itemStyle: {
          color: '#91cc75'
        },
        smooth: true
      }
    ]
  }
  
  barChartInstance.setOption(option, true)
}

const renderGaugeChart = () => {
  if (!gaugeChartRef.value || !metrics.value) return
  
  if (!gaugeChartInstance) {
    gaugeChartInstance = echarts.init(gaugeChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Current Average Value',
      left: 'center'
    },
    series: [
      {
        type: 'gauge',
        center: ['50%', '60%'],
        startAngle: 200,
        endAngle: -20,
        min: Math.floor(metrics.value.minimum),
        max: Math.ceil(metrics.value.maximum),
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
          formatter: '{value}',
          color: 'inherit'
        },
        data: [
          {
            value: metrics.value.average.toFixed(2)
          }
        ]
      }
    ]
  }
  
  gaugeChartInstance.setOption(option, true)
}

const formatTime = (dateString: string): string => {
  return new Date(dateString).toLocaleString()
}

const resizeCharts = () => {
  lineChartInstance?.resize()
  barChartInstance?.resize()
  gaugeChartInstance?.resize()
}

// Lifecycle
onMounted(async () => {
  window.addEventListener('resize', resizeCharts)
  await loadTelemetry()
  startAutoRefresh()
})

onUnmounted(() => {
  window.removeEventListener('resize', resizeCharts)
  stopAutoRefresh()
  lineChartInstance?.dispose()
  barChartInstance?.dispose()
  gaugeChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="telemetry-monitoring space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Telemetry Monitoring</h1>
          <p class="text-gray-600 mt-2">Real-time monitoring of machine telemetry data</p>
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
          <BaseButton variant="outline" @click="refreshTelemetry">
            <RefreshCw class="w-4 h-4 mr-2" />
            Manual Refresh
          </BaseButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Readings</p>
                <p class="text-2xl font-bold">{{ telemetryData.length }}</p>
              </div>
              <Database class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Machines</p>
                <p class="text-2xl font-bold">
                  {{ [...new Set(telemetryData.map((t: any) => t.machineId))].length }}
                </p>
              </div>
              <Activity class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Data Types</p>
                <p class="text-2xl font-bold">
                  {{ [...new Set(telemetryData.map((t: any) => t.dataType))].length }}
                </p>
              </div>
              <BarChart class="w-8 h-8 text-purple-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard v-if="metrics">
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Avg Value</p>
                <p class="text-2xl font-bold">{{ metrics.average.toFixed(2) }}</p>
              </div>
              <TrendingUp class="w-8 h-8 text-orange-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Filters -->
      <BaseCard>
        <div class="p-4 space-y-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <div class="relative">
                <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                <BaseInput
                  v-model="searchQuery"
                  placeholder="Search telemetry data..."
                  class="pl-10"
                />
              </div>
            </div>
            
            <BaseSelect
              v-model="timeRange"
              :options="timeRangeOptions"
              @change="loadTelemetry"
            />
            
            <BaseSelect
              v-model="machineFilter"
              :options="machineOptions"
            />
            
            <BaseSelect
              v-model="dataTypeFilter"
              :options="dataTypeOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Charts Section -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Line Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="lineChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <!-- Bar Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="barChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Gauge Chart -->
      <BaseCard v-if="metrics">
        <div class="p-6">
          <div class="flex justify-center">
            <div ref="gaugeChartRef" class="w-64 h-64"></div>
          </div>
        </div>
      </BaseCard>

      <!-- Telemetry Data Table -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Recent Telemetry Data ({{ filteredTelemetry.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading telemetry data...</p>
          </div>
          
          <div v-else-if="filteredTelemetry.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No telemetry data found</p>
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
                    Machine
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Data Type
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Data
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr 
                  v-for="(item, index) in filteredTelemetry.slice(0, 50)" 
                  :key="`${item.id}-${index}`"
                  class="hover:bg-gray-50"
                >
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatTime(item.timestamp) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ machines.find((m: any) => m.id === item.machineId)?.name || item.machineId }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ item.dataType }}
                  </td>
                  <td class="px-6 py-4 text-sm text-gray-900">
                    <div class="max-w-md truncate">
                      {{ JSON.stringify(item.data) }}
                    </div>
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
.telemetry-monitoring {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .telemetry-monitoring {
    padding: 0.5rem;
  }
}
</style>