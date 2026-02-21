<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { Cloud, RefreshCw, Server, Factory, GitBranch, CheckCircle, XCircle, AlertTriangle, Clock, Activity } from 'lucide-vue-next'

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

// State
const syncingMachine = ref(false)
const syncingLine = ref(false)
const upserting = ref(false)
const loadingStatus = ref(false)
const machineId = ref('00000000-0000-0000-0000-000000000001')
const lineId = ref('00000000-0000-0000-0000-000000000002')
const twinId = ref('')
const modelId = ref('dtmi:digitaltwins:equipment:Machinery;1')
const twinProperties = ref('{}')
const syncHistory = ref<any[]>([])
const connectionStatus = ref<'connected' | 'disconnected' | 'connecting'>('connecting')

// Chart refs
const syncStatsChartRef = ref<HTMLDivElement | null>(null)
const statusChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let syncStatsChartInstance: echarts.ECharts | null = null
let statusChartInstance: echarts.ECharts | null = null

// Computed
const isValidJson = computed(() => {
  try {
    JSON.parse(twinProperties.value)
    return true
  } catch {
    return false
  }
})

const syncSuccessRate = computed(() => {
  if (syncHistory.value.length === 0) return 0
  const successes = syncHistory.value.filter(s => s.status === 'success').length
  return Math.round((successes / syncHistory.value.length) * 100)
})

const recentSyncs = computed(() => {
  return syncHistory.value.slice(0, 5)
})

const getConnectionStatusColor = computed(() => {
  if (connectionStatus.value === 'connected') return 'text-green-600'
  if (connectionStatus.value === 'connecting') return 'text-yellow-600'
  return 'text-red-600'
})

const getConnectionStatusIcon = computed(() => {
  if (connectionStatus.value === 'connected') return CheckCircle
  if (connectionStatus.value === 'connecting') return RefreshCw
  return XCircle
})

// Methods
const syncMachineToADT = async () => {
  try {
    syncingMachine.value = true
    
    // Mock API call
    await new Promise(resolve => setTimeout(resolve, 2000))
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'machine',
      targetId: machineId.value,
      timestamp: new Date().toISOString(),
      status: 'success',
      duration: Math.floor(Math.random() * 1000) + 500 // 500-1500ms
    }
    
    syncHistory.value.unshift(syncRecord)
    
    renderSyncStatsChart()
    
    toast.success(`Machine ${machineId.value.substring(0, 8)} synced to Azure Digital Twins`)
  } catch (error) {
    console.error('Failed to sync machine:', error)
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'machine',
      targetId: machineId.value,
      timestamp: new Date().toISOString(),
      status: 'failed',
      duration: 0,
      error: 'Connection timeout'
    }
    
    syncHistory.value.unshift(syncRecord)
    
    toast.error('Failed to sync machine to Azure Digital Twins')
  } finally {
    syncingMachine.value = false
  }
}

const syncProductionLineToADT = async () => {
  try {
    syncingLine.value = true
    
    // Mock API call
    await new Promise(resolve => setTimeout(resolve, 2500))
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'production-line',
      targetId: lineId.value,
      timestamp: new Date().toISOString(),
      status: 'success',
      duration: Math.floor(Math.random() * 1500) + 800 // 800-2300ms
    }
    
    syncHistory.value.unshift(syncRecord)
    
    renderSyncStatsChart()
    
    toast.success(`Production line ${lineId.value.substring(0, 8)} synced to Azure Digital Twins`)
  } catch (error) {
    console.error('Failed to sync production line:', error)
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'production-line',
      targetId: lineId.value,
      timestamp: new Date().toISOString(),
      status: 'failed',
      duration: 0,
      error: 'Authentication failed'
    }
    
    syncHistory.value.unshift(syncRecord)
    
    toast.error('Failed to sync production line to Azure Digital Twins')
  } finally {
    syncingLine.value = false
  }
}

const upsertTwin = async () => {
  try {
    upserting.value = true
    
    // Validate JSON
    let propertiesObj
    try {
      propertiesObj = JSON.parse(twinProperties.value)
    } catch (e) {
      toast.error('Invalid JSON format for twin properties')
      return
    }
    
    // Mock API call
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    const syncRecord = {
      id: `upsert_${Date.now()}`,
      type: 'twin-upsert',
      targetId: twinId.value,
      timestamp: new Date().toISOString(),
      status: 'success',
      duration: Math.floor(Math.random() * 800) + 300 // 300-1100ms
    }
    
    syncHistory.value.unshift(syncRecord)
    
    renderSyncStatsChart()
    
    toast.success(`Twin ${twinId.value || 'new'} upserted successfully`)
    
    // Reset form
    twinId.value = ''
    twinProperties.value = '{}'
  } catch (error) {
    console.error('Failed to upsert twin:', error)
    
    const syncRecord = {
      id: `upsert_${Date.now()}`,
      type: 'twin-upsert',
      targetId: twinId.value,
      timestamp: new Date().toISOString(),
      status: 'failed',
      duration: 0,
      error: 'Invalid twin model'
    }
    
    syncHistory.value.unshift(syncRecord)
    
    toast.error('Failed to upsert digital twin')
  } finally {
    upserting.value = false
  }
}

const checkConnectionStatus = async () => {
  try {
    loadingStatus.value = true
    connectionStatus.value = 'connecting'
    
    // Mock connection check
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    // Randomize connection status for demo
    const statuses: Array<'connected' | 'disconnected' | 'connecting'> = ['connected', 'disconnected', 'connecting']
    connectionStatus.value = statuses[Math.floor(Math.random() * 3)]
    
    toast.success('Connection status updated')
  } catch (error) {
    console.error('Failed to check connection status:', error)
    connectionStatus.value = 'disconnected'
    toast.error('Failed to check Azure Digital Twins connection')
  } finally {
    loadingStatus.value = false
  }
}

const renderSyncStatsChart = () => {
  if (!syncStatsChartRef.value) return
  
  if (!syncStatsChartInstance) {
    syncStatsChartInstance = echarts.init(syncStatsChartRef.value)
  }
  
  const successCount = syncHistory.value.filter(s => s.status === 'success').length
  const failedCount = syncHistory.value.filter(s => s.status === 'failed').length
  
  const option = {
    title: {
      text: 'Sync Operations Statistics',
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
      data: ['Successful', 'Failed']
    },
    series: [
      {
        name: 'Sync Operations',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        data: [
          { value: successCount, name: 'Successful', itemStyle: { color: '#10b981' } },
          { value: failedCount, name: 'Failed', itemStyle: { color: '#ef4444' } }
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
  
  syncStatsChartInstance.setOption(option, true)
}

const renderStatusChart = () => {
  if (!statusChartRef.value) return
  
  if (!statusChartInstance) {
    statusChartInstance = echarts.init(statusChartRef.value)
  }
  
  // Mock timeline data
  const times = []
  const statuses = []
  
  for (let i = 23; i >= 0; i--) {
    const time = new Date(Date.now() - i * 60 * 60 * 1000)
    times.push(`${time.getHours()}:00`)
    statuses.push(Math.random() > 0.2 ? 1 : 0) // 80% uptime
  }
  
  const option = {
    title: {
      text: '24-Hour Connection Status',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: times
    },
    yAxis: {
      type: 'value',
      name: 'Status',
      min: 0,
      max: 1
    },
    series: [{
      name: 'Connection',
      type: 'line',
      data: statuses,
      step: 'start',
      itemStyle: { color: '#3b82f6' },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0, y: 0, x2: 0, y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(59, 130, 246, 0.3)' },
            { offset: 1, color: 'rgba(59, 130, 246, 0.05)' }
          ]
        }
      }
    }]
  }
  
  statusChartInstance.setOption(option, true)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const getTypeIcon = (type: string) => {
  if (type === 'machine') return Server
  if (type === 'production-line') return Factory
  return GitBranch
}

const getStatusColor = (status: string) => {
  return status === 'success' ? 'text-green-600' : 'text-red-600'
}

const resizeCharts = () => {
  syncStatsChartInstance?.resize()
  statusChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  checkConnectionStatus()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  syncStatsChartInstance?.dispose()
  statusChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="azure-digital-twin space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Azure Digital Twins Integration</h1>
          <p class="text-gray-600 mt-2">Manage synchronization between platform and Azure Digital Twins</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="checkConnectionStatus"
            :disabled="loadingStatus"
          >
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loadingStatus }" />
            Check Connection
          </BaseButton>
        </div>
      </div>

      <!-- Connection Status -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Connection Status</h2>
          
          <div class="flex items-center gap-4 p-4 bg-gray-50 rounded-lg">
            <component 
              :is="getConnectionStatusIcon" 
              class="w-8 h-8" 
              :class="getConnectionStatusColor"
            />
            <div>
              <h3 class="font-medium">Azure Digital Twins Service</h3>
              <p class="text-sm text-gray-600">
                Status: 
                <span class="font-medium" :class="getConnectionStatusColor">
                  {{ connectionStatus.charAt(0).toUpperCase() + connectionStatus.slice(1) }}
                </span>
              </p>
            </div>
            <div class="ml-auto text-right">
              <p class="text-sm text-gray-600">Success Rate</p>
              <p class="text-2xl font-bold text-green-600">{{ syncSuccessRate }}%</p>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Sync Operations -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Machine Sync -->
        <BaseCard>
          <div class="p-6">
            <h3 class="text-lg font-semibold mb-4 flex items-center gap-2">
              <Server class="w-5 h-5" />
              Sync Machine to ADT
            </h3>
            
            <BaseInput
              v-model="machineId"
              label="Machine ID"
              placeholder="Enter machine UUID"
              class="mb-4"
            />
            
            <BaseButton 
              variant="primary" 
              @click="syncMachineToADT"
              :disabled="syncingMachine"
              class="w-full"
            >
              <Cloud class="w-4 h-4 mr-2" :class="{ 'animate-spin': syncingMachine }" />
              {{ syncingMachine ? 'Syncing...' : 'Sync Machine' }}
            </BaseButton>
          </div>
        </BaseCard>

        <!-- Production Line Sync -->
        <BaseCard>
          <div class="p-6">
            <h3 class="text-lg font-semibold mb-4 flex items-center gap-2">
              <Factory class="w-5 h-5" />
              Sync Production Line to ADT
            </h3>
            
            <BaseInput
              v-model="lineId"
              label="Production Line ID"
              placeholder="Enter production line UUID"
              class="mb-4"
            />
            
            <BaseButton 
              variant="primary" 
              @click="syncProductionLineToADT"
              :disabled="syncingLine"
              class="w-full"
            >
              <Cloud class="w-4 h-4 mr-2" :class="{ 'animate-spin': syncingLine }" />
              {{ syncingLine ? 'Syncing...' : 'Sync Production Line' }}
            </BaseButton>
          </div>
        </BaseCard>
      </div>

      <!-- Twin Management -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Digital Twin Management</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
            <BaseInput
              v-model="twinId"
              label="Twin ID (Optional - blank for new)"
              placeholder="Enter existing twin ID or leave blank"
            />
            
            <BaseInput
              v-model="modelId"
              label="Model ID"
              placeholder="Enter DTMI model identifier"
            />
          </div>
          
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">
              Twin Properties (JSON)
            </label>
            <textarea
              v-model="twinProperties"
              rows="6"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              :class="{ 'border-red-300': !isValidJson && twinProperties }"
              placeholder='{"temperature": 72.5, "status": "operational", "location": "floor1"}'
            ></textarea>
            <p v-if="!isValidJson && twinProperties" class="mt-1 text-sm text-red-600">
              Invalid JSON format
            </p>
          </div>
          
          <BaseButton 
            variant="primary" 
            @click="upsertTwin"
            :disabled="upserting || !isValidJson"
            class="w-full md:w-auto"
          >
            <GitBranch class="w-4 h-4 mr-2" :class="{ 'animate-spin': upserting }" />
            {{ upserting ? 'Creating/Updating...' : 'Create/Update Twin' }}
          </BaseButton>
        </div>
      </BaseCard>

      <!-- Analytics -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Sync Statistics</h3>
            <div ref="syncStatsChartRef" class="w-full h-64"></div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-6">
            <h3 class="font-medium mb-4">Connection Timeline</h3>
            <div ref="statusChartRef" class="w-full h-64"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Recent Sync History -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Recent Sync Operations</h2>
          
          <div v-if="syncHistory.length === 0" class="text-center py-8 text-gray-500">
            <Activity class="w-12 h-12 mx-auto mb-3 text-gray-300" />
            <p>No sync operations recorded yet</p>
          </div>
          
          <div v-else class="space-y-3">
            <BaseCard
              v-for="sync in recentSyncs"
              :key="sync.id"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex justify-between items-start">
                  <div class="flex items-center gap-3">
                    <component 
                      :is="getTypeIcon(sync.type)" 
                      class="w-5 h-5 text-gray-500"
                    />
                    <div>
                      <h4 class="font-medium capitalize">
                        {{ sync.type.replace('-', ' ') }}
                      </h4>
                      <p class="text-sm text-gray-600">
                        Target: {{ sync.targetId.substring(0, 8) }}...
                      </p>
                    </div>
                  </div>
                  
                  <div class="text-right">
                    <div class="flex items-center gap-2">
                      <component 
                        :is="sync.status === 'success' ? CheckCircle : XCircle" 
                        class="w-4 h-4" 
                        :class="getStatusColor(sync.status)"
                      />
                      <span 
                        class="px-2 py-1 rounded-full text-xs font-medium"
                        :class="sync.status === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                      >
                        {{ sync.status }}
                      </span>
                    </div>
                    <p class="text-xs text-gray-500 mt-1">
                      {{ formatDate(sync.timestamp) }}
                      <span v-if="sync.duration">• {{ sync.duration }}ms</span>
                    </p>
                    <p v-if="sync.error" class="text-xs text-red-600 mt-1">
                      Error: {{ sync.error }}
                    </p>
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
.azure-digital-twin {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .azure-digital-twin {
    padding: 0.5rem;
  }
}
</style>