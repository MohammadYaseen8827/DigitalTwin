<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { useAzureDigitalTwin } from '@/services/azureDigitalTwin.service'
import { 
  Cloud, 
  RefreshCw, 
  Server, 
  Factory, 
  GitBranch, 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Clock, 
  Activity,
  Play,
  Pause,
  RotateCcw
} from 'lucide-vue-next'

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
const {
  status,
  entities,
  loading,
  error,
  fetchStatus,
  fetchEntities,
  syncMachine,
  syncProductionLine,
  syncAll
} = useAzureDigitalTwin()

// State
const syncingMachine = ref(false)
const syncingLine = ref(false)
const syncingAll = ref(false)
const machineId = ref('')
const lineId = ref('')
const syncHistory = ref<any[]>([])

// Computed
const connectionStatusColor = computed(() => {
  if (!status.value) return 'text-gray-600'
  if (status.value.isConnected) return 'text-green-600'
  return 'text-red-600'
})

const connectionStatusIcon = computed(() => {
  if (!status.value) return Clock
  if (status.value.isConnected) return CheckCircle
  return XCircle
})

const syncSuccessRate = computed(() => {
  if (syncHistory.value.length === 0) return 0
  const successes = syncHistory.value.filter(s => s.success).length
  return Math.round((successes / syncHistory.value.length) * 100)
})

// Methods
const handleSyncMachine = async () => {
  if (!machineId.value) {
    toast.error('Please enter a machine ID')
    return
  }
  
  try {
    syncingMachine.value = true
    const result = await syncMachine(machineId.value)
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'machine',
      targetId: machineId.value,
      timestamp: new Date().toISOString(),
      success: result.success,
      message: result.message,
      syncedEntities: result.syncedEntities,
      errors: result.errors
    }
    
    syncHistory.value.unshift(syncRecord)
    toast.success(result.message)
    
  } catch (error) {
    console.error('Failed to sync machine:', error)
    toast.error('Failed to sync machine to Azure Digital Twins')
  } finally {
    syncingMachine.value = false
  }
}

const handleSyncLine = async () => {
  if (!lineId.value) {
    toast.error('Please enter a production line ID')
    return
  }
  
  try {
    syncingLine.value = true
    const result = await syncProductionLine(lineId.value)
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'production-line',
      targetId: lineId.value,
      timestamp: new Date().toISOString(),
      success: result.success,
      message: result.message,
      syncedEntities: result.syncedEntities,
      errors: result.errors
    }
    
    syncHistory.value.unshift(syncRecord)
    toast.success(result.message)
    
  } catch (error) {
    console.error('Failed to sync production line:', error)
    toast.error('Failed to sync production line to Azure Digital Twins')
  } finally {
    syncingLine.value = false
  }
}

const handleSyncAll = async () => {
  try {
    syncingAll.value = true
    const result = await syncAll()
    
    const syncRecord = {
      id: `sync_${Date.now()}`,
      type: 'full-sync',
      targetId: 'all-entities',
      timestamp: new Date().toISOString(),
      success: result.success,
      message: result.message,
      syncedEntities: result.syncedEntities,
      errors: result.errors
    }
    
    syncHistory.value.unshift(syncRecord)
    toast.success(result.message)
    
  } catch (error) {
    console.error('Failed to sync all entities:', error)
    toast.error('Failed to sync all entities to Azure Digital Twins')
  } finally {
    syncingAll.value = false
  }
}

const refreshStatus = async () => {
  await fetchStatus()
  toast.success('Azure Digital Twin status refreshed')
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    fetchStatus(),
    fetchEntities()
  ])
})
</script>

<template>
  <BaseCard>
    <div class="azure-digital-twin-dashboard space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Azure Digital Twin Integration</h1>
          <p class="text-gray-600 mt-2">Manage synchronization between Digital Twin Platform and Azure Digital Twins</p>
        </div>
        <BaseButton variant="outline" @click="refreshStatus" :disabled="loading">
          <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
          Refresh Status
        </BaseButton>
      </div>

      <!-- Connection Status -->
      <BaseCard class="p-6">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-4">
            <component :is="connectionStatusIcon" class="w-8 h-8" :class="connectionStatusColor" />
            <div>
              <h3 class="text-lg font-semibold">Connection Status</h3>
              <p class="text-gray-600">
                {{ status?.isConnected ? 'Connected to Azure Digital Twins' : 'Disconnected' }}
              </p>
            </div>
          </div>
          <div class="text-right">
            <p class="text-sm text-gray-500">Last Sync</p>
            <p class="font-medium">
              {{ status?.lastSync ? new Date(status.lastSync).toLocaleString() : 'Never' }}
            </p>
          </div>
        </div>
      </BaseCard>

      <!-- Sync Controls -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <!-- Machine Sync -->
        <BaseCard class="p-6">
          <h3 class="text-lg font-semibold mb-4 flex items-center gap-2">
            <Server class="w-5 h-5 text-blue-500" />
            Machine Sync
          </h3>
          <BaseInput
            v-model="machineId"
            label="Machine ID"
            placeholder="Enter machine GUID"
            class="mb-4"
          />
          <BaseButton 
            variant="primary" 
            class="w-full"
            @click="handleSyncMachine"
            :disabled="syncingMachine || !machineId"
          >
            <Play v-if="!syncingMachine" class="w-4 h-4 mr-2" />
            <RefreshCw v-else class="w-4 h-4 mr-2 animate-spin" />
            {{ syncingMachine ? 'Syncing...' : 'Sync Machine' }}
          </BaseButton>
        </BaseCard>

        <!-- Production Line Sync -->
        <BaseCard class="p-6">
          <h3 class="text-lg font-semibold mb-4 flex items-center gap-2">
            <Factory class="w-5 h-5 text-green-500" />
            Production Line Sync
          </h3>
          <BaseInput
            v-model="lineId"
            label="Line ID"
            placeholder="Enter line GUID"
            class="mb-4"
          />
          <BaseButton 
            variant="primary" 
            class="w-full"
            @click="handleSyncLine"
            :disabled="syncingLine || !lineId"
          >
            <Play v-if="!syncingLine" class="w-4 h-4 mr-2" />
            <RefreshCw v-else class="w-4 h-4 mr-2 animate-spin" />
            {{ syncingLine ? 'Syncing...' : 'Sync Line' }}
          </BaseButton>
        </BaseCard>

        <!-- Full Sync -->
        <BaseCard class="p-6">
          <h3 class="text-lg font-semibold mb-4 flex items-center gap-2">
            <Cloud class="w-5 h-5 text-purple-500" />
            Full Sync
          </h3>
          <p class="text-sm text-gray-600 mb-4">
            Sync all machines and production lines
          </p>
          <BaseButton 
            variant="primary" 
            class="w-full"
            @click="handleSyncAll"
            :disabled="syncingAll"
          >
            <Play v-if="!syncingAll" class="w-4 h-4 mr-2" />
            <RefreshCw v-else class="w-4 h-4 mr-2 animate-spin" />
            {{ syncingAll ? 'Syncing All...' : 'Sync All Entities' }}
          </BaseButton>
        </BaseCard>
      </div>

      <!-- Sync History -->
      <BaseCard class="p-6">
        <h3 class="text-xl font-semibold mb-4">Sync History</h3>
        <div v-if="syncHistory.length === 0" class="text-center py-8 text-gray-500">
          No sync history available
        </div>
        <div v-else class="space-y-3 max-h-96 overflow-y-auto">
          <div 
            v-for="sync in syncHistory" 
            :key="sync.id"
            class="flex items-center justify-between p-3 rounded-lg border"
            :class="sync.success ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'"
          >
            <div class="flex items-center gap-3">
              <component :is="sync.success ? CheckCircle : XCircle" class="w-5 h-5" :class="sync.success ? 'text-green-500' : 'text-red-500'" />
              <div>
                <p class="font-medium">{{ sync.type }} sync</p>
                <p class="text-sm text-gray-600">Target: {{ sync.targetId }}</p>
              </div>
            </div>
            <div class="text-right">
              <p class="text-sm">{{ new Date(sync.timestamp).toLocaleTimeString() }}</p>
              <p class="text-xs text-gray-500">{{ sync.message }}</p>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.azure-digital-twin-dashboard {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}
</style>