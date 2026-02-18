<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useToast } from '@/composables/useToast'
import { 
  getHealthStatus,
  getDatabaseHealth,
  isSystemHealthy,
  type HealthCheckResult
} from '@/services/health.service'
import { RefreshCw, Heart, Database, Wifi, Cloud, CheckCircle, XCircle, AlertTriangle } from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  GaugeChart as EChartsGauge,
  BarChart as EChartsBar
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
  EChartsGauge,
  EChartsBar,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const healthData = ref<HealthCheckResult | null>(null)
const databaseHealth = ref<any>(null)
const loading = ref(false)
const autoRefreshEnabled = ref(true)
const refreshInterval = ref<NodeJS.Timeout | null>(null)
const lastChecked = ref<string>('')

// Chart refs
const overallHealthChartRef = ref<HTMLDivElement | null>(null)
const componentStatusChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let overallHealthChartInstance: echarts.ECharts | null = null
let componentStatusChartInstance: echarts.ECharts | null = null

// Computed
const overallStatus = computed(() => {
  if (!healthData.value) return 'Unknown'
  return healthData.value.status
})

const overallStatusColor = computed(() => {
  const status = overallStatus.value
  if (status === 'Healthy') return 'text-green-600'
  if (status === 'Degraded') return 'text-yellow-600'
  return 'text-red-600'
})

const overallStatusIcon = computed(() => {
  const status = overallStatus.value
  if (status === 'Healthy') return CheckCircle
  if (status === 'Degraded') return AlertTriangle
  return XCircle
})

const componentEntries = computed(() => {
  if (!healthData.value?.entries) return []
  return Object.entries(healthData.value.entries).map(([name, entry]) => ({
    name,
    ...entry
  }))
})

const healthyComponents = computed(() => {
  return componentEntries.value.filter(entry => entry.status === 'Healthy').length
})

const totalComponents = computed(() => {
  return componentEntries.value.length
})

const healthPercentage = computed(() => {
  if (totalComponents.value === 0) return 0
  return Math.round((healthyComponents.value / totalComponents.value) * 100)
})

// Methods
const checkHealth = async () => {
  try {
    loading.value = true
    
    // Get overall health status
    healthData.value = await getHealthStatus()
    
    // Get database health specifically
    databaseHealth.value = await getDatabaseHealth()
    
    lastChecked.value = new Date().toLocaleTimeString()
    
    renderOverallHealthChart()
    renderComponentStatusChart()
    
    if (overallStatus.value === 'Healthy') {
      toast.success('System is healthy')
    } else if (overallStatus.value === 'Degraded') {
      toast.warning('System is degraded')
    } else {
      toast.error('System is unhealthy')
    }
  } catch (error) {
    console.error('Failed to check health:', error)
    toast.error('Failed to check system health')
  } finally {
    loading.value = false
  }
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
    checkHealth()
  }, 30000) // 30 seconds
}

const stopAutoRefresh = () => {
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
    refreshInterval.value = null
  }
}

const renderOverallHealthChart = () => {
  if (!overallHealthChartRef.value) return
  
  if (!overallHealthChartInstance) {
    overallHealthChartInstance = echarts.init(overallHealthChartRef.value)
  }
  
  const status = overallStatus.value
  let value = 0
  let color = '#ef4444' // red
  
  if (status === 'Healthy') {
    value = 100
    color = '#10b981' // green
  } else if (status === 'Degraded') {
    value = 50
    color = '#f59e0b' // yellow
  }
  
  const option = {
    series: [{
      type: 'gauge',
      startAngle: 180,
      endAngle: 0,
      center: ['50%', '75%'],
      radius: '90%',
      min: 0,
      max: 100,
      splitNumber: 4,
      axisLine: {
        lineStyle: {
          width: 6,
          color: [
            [0.5, '#ef4444'],
            [0.75, '#f59e0b'],
            [1, '#10b981']
          ]
        }
      },
      pointer: {
        icon: 'path://M12.8,0.7l12,40.1H0.7L12.8,0.7z',
        length: '12%',
        width: 20,
        offsetCenter: [0, '-60%'],
        itemStyle: {
          color: 'auto'
        }
      },
      axisTick: {
        length: 12,
        lineStyle: {
          color: 'auto',
          width: 2
        }
      },
      splitLine: {
        length: 20,
        lineStyle: {
          color: 'auto',
          width: 5
        }
      },
      axisLabel: {
        color: '#464646',
        fontSize: 12,
        distance: -60,
        formatter: function (value: number) {
          if (value === 0) return 'Critical'
          if (value === 25) return 'Poor'
          if (value === 50) return 'Fair'
          if (value === 75) return 'Good'
          if (value === 100) return 'Excellent'
          return ''
        }
      },
      title: {
        offsetCenter: [0, '20%'],
        fontSize: 16
      },
      detail: {
        fontSize: 30,
        offsetCenter: [0, '0%'],
        valueAnimation: true,
        formatter: '{value}%',
        color: 'auto'
      },
      data: [{
        value: value,
        name: status
      }]
    }]
  }
  
  overallHealthChartInstance.setOption(option, true)
}

const renderComponentStatusChart = () => {
  if (!componentStatusChartRef.value || !healthData.value?.entries) return
  
  if (!componentStatusChartInstance) {
    componentStatusChartInstance = echarts.init(componentStatusChartRef.value)
  }
  
  const entries = componentEntries.value
  const statusColors: Record<string, string> = {
    'Healthy': '#10b981',
    'Degraded': '#f59e0b',
    'Unhealthy': '#ef4444'
  }
  
  const data = entries.map(entry => ({
    name: entry.name,
    value: entry.status === 'Healthy' ? 100 : entry.status === 'Degraded' ? 50 : 0,
    itemStyle: {
      color: statusColors[entry.status] || '#9ca3af'
    }
  }))
  
  const option = {
    title: {
      text: 'Component Status',
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
      data: entries.map(e => e.name),
      axisLabel: {
        rotate: 45
      }
    },
    yAxis: {
      type: 'value',
      max: 100
    },
    series: [{
      name: 'Status',
      type: 'bar',
      data: data,
      label: {
        show: true,
        position: 'top',
        formatter: (params: any) => {
          const entry = entries.find(e => e.name === params.name)
          return entry?.status || ''
        }
      }
    }]
  }
  
  componentStatusChartInstance.setOption(option, true)
}

const getStatusIcon = (status: string) => {
  if (status === 'Healthy') return CheckCircle
  if (status === 'Degraded') return AlertTriangle
  return XCircle
}

const getStatusColor = (status: string) => {
  if (status === 'Healthy') return 'text-green-600'
  if (status === 'Degraded') return 'text-yellow-600'
  return 'text-red-600'
}

const getStatusBgColor = (status: string) => {
  if (status === 'Healthy') return 'bg-green-100'
  if (status === 'Degraded') return 'bg-yellow-100'
  return 'bg-red-100'
}

const resizeCharts = () => {
  overallHealthChartInstance?.resize()
  componentStatusChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  checkHealth()
  if (autoRefreshEnabled.value) {
    startAutoRefresh()
  }
  window.addEventListener('resize', resizeCharts)
})

onUnmounted(() => {
  stopAutoRefresh()
  window.removeEventListener('resize', resizeCharts)
  overallHealthChartInstance?.dispose()
  componentStatusChartInstance?.dispose()
})
</script>

<template>
  <BaseCard>
    <div class="system-health space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">System Health Dashboard</h1>
          <p class="text-gray-600 mt-2">Monitor the health status of all system components</p>
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
            @click="checkHealth"
            :disabled="loading"
          >
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
            {{ loading ? 'Checking...' : 'Check Now' }}
          </BaseButton>
        </div>
      </div>

      <!-- Last Checked -->
      <div v-if="lastChecked" class="text-sm text-gray-500 text-right">
        Last checked: {{ lastChecked }}
      </div>

      <!-- Overall Health Status -->
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <BaseCard class="lg:col-span-1">
          <div class="p-6 text-center">
            <component 
              :is="overallStatusIcon" 
              class="w-12 h-12 mx-auto mb-4" 
              :class="overallStatusColor"
            />
            <h2 class="text-2xl font-bold mb-2" :class="overallStatusColor">
              {{ overallStatus }}
            </h2>
            <p class="text-gray-600">Overall System Status</p>
          </div>
        </BaseCard>

        <BaseCard class="lg:col-span-2">
          <div class="p-6">
            <h3 class="text-lg font-semibold mb-4">Health Overview</h3>
            <div ref="overallHealthChartRef" class="w-full h-64"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Component Status Summary -->
      <BaseCard>
        <div class="p-6">
          <h3 class="text-lg font-semibold mb-4">Component Status Summary</h3>
          
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
            <BaseCard>
              <div class="p-4 text-center">
                <Heart class="w-8 h-8 text-blue-500 mx-auto mb-2" />
                <p class="text-2xl font-bold">{{ totalComponents }}</p>
                <p class="text-sm text-gray-600">Total Components</p>
              </div>
            </BaseCard>
            
            <BaseCard>
              <div class="p-4 text-center">
                <CheckCircle class="w-8 h-8 text-green-500 mx-auto mb-2" />
                <p class="text-2xl font-bold text-green-600">{{ healthyComponents }}</p>
                <p class="text-sm text-gray-600">Healthy</p>
              </div>
            </BaseCard>
            
            <BaseCard>
              <div class="p-4 text-center">
                <AlertTriangle class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
                <p class="text-2xl font-bold text-yellow-600">{{ totalComponents - healthyComponents }}</p>
                <p class="text-sm text-gray-600">Issues</p>
              </div>
            </BaseCard>
            
            <BaseCard>
              <div class="p-4 text-center">
                <div class="w-8 h-8 bg-gray-200 rounded-full flex items-center justify-center mx-auto mb-2">
                  <span class="text-sm font-bold">{{ healthPercentage }}%</span>
                </div>
                <p class="text-2xl font-bold">{{ healthPercentage }}%</p>
                <p class="text-sm text-gray-600">Health Score</p>
              </div>
            </BaseCard>
          </div>
          
          <div ref="componentStatusChartRef" class="w-full h-80"></div>
        </div>
      </BaseCard>

      <!-- Detailed Component Status -->
      <BaseCard>
        <div class="p-6">
          <h3 class="text-lg font-semibold mb-4">Detailed Component Status</h3>
          
          <div class="space-y-3">
            <div
              v-for="entry in componentEntries"
              :key="entry.name"
              class="flex items-center justify-between p-4 rounded-lg border"
              :class="getStatusBgColor(entry.status)"
            >
              <div class="flex items-center gap-3">
                <component 
                  :is="getStatusIcon(entry.status)" 
                  class="w-5 h-5" 
                  :class="getStatusColor(entry.status)"
                />
                <div>
                  <h4 class="font-medium capitalize">{{ entry.name.replace(/([A-Z])/g, ' $1').trim() }}</h4>
                  <p class="text-sm text-gray-600">{{ entry.description || 'No description' }}</p>
                </div>
              </div>
              
              <div class="text-right">
                <span 
                  class="px-3 py-1 rounded-full text-sm font-medium"
                  :class="[
                    entry.status === 'Healthy' ? 'bg-green-100 text-green-800' :
                    entry.status === 'Degraded' ? 'bg-yellow-100 text-yellow-800' :
                    'bg-red-100 text-red-800'
                  ]"
                >
                  {{ entry.status }}
                </span>
                <p class="text-xs text-gray-500 mt-1">{{ entry.duration }}</p>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Database Health (if available separately) -->
      <BaseCard v-if="databaseHealth">
        <div class="p-6">
          <h3 class="text-lg font-semibold mb-4">Database Health</h3>
          
          <div class="flex items-center gap-3 p-4 rounded-lg border" :class="getStatusBgColor(databaseHealth.status)">
            <Database class="w-6 h-6" :class="getStatusColor(databaseHealth.status)" />
            <div class="flex-1">
              <h4 class="font-medium">Database Connection</h4>
              <p class="text-sm text-gray-600">{{ databaseHealth.description || 'Database health check' }}</p>
            </div>
            <span 
              class="px-3 py-1 rounded-full text-sm font-medium"
              :class="[
                databaseHealth.status === 'Healthy' ? 'bg-green-100 text-green-800' :
                databaseHealth.status === 'Degraded' ? 'bg-yellow-100 text-yellow-800' :
                'bg-red-100 text-red-800'
              ]"
            >
              {{ databaseHealth.status }}
            </span>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.system-health {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .system-health {
    padding: 0.5rem;
  }
}
</style>