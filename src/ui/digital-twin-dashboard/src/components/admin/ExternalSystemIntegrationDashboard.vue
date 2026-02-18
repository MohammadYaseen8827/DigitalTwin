<script setup lang="ts">
import { ref, computed, onMounted, type Ref } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  useExternalSystems, 
  type ExternalSystem, 
  type ExternalSystemCreateDto, 
  type ExternalSystemUpdateDto,
  type DataSynchronization,
  ExternalSystemStatus,
  EntityType,
  IntegrationType,
  SyncDirection,
  SyncStatus
} from '@/services/external-systems.service'
import { 
  Plug, 
  Database, 
  Settings, 
  Plus, 
  Edit, 
  Trash2, 
  CheckCircle, 
  XCircle, 
  Wifi, 
  WifiOff, 
  RefreshCw,
  AlertTriangle,
  Clock,
  Play,
  Pause,
  RotateCcw
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
  PieChart as EChartsPie,
  LineChart as EChartsLine
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
  EChartsBar,
  EChartsPie,
  EChartsLine,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  CanvasRenderer
])

const toast = useToast()

// Use external systems composable
const {
  externalSystems,
  systemIntegrations,
  dataSynchronizations,
  currentSystem,
  loading,
  error,
  fetchExternalSystems,
  fetchSystemIntegrations,
  fetchDataSynchronizations,
  createExternalSystem,
  updateExternalSystem,
  deleteExternalSystem,
  connectExternalSystem,
  disconnectExternalSystem,
  testExternalSystemConnection
} = useExternalSystems()

// State
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDetailsModal = ref(false)
const editingSystem = ref<ExternalSystem | null>(null)
const viewingSystem = ref<ExternalSystem | null>(null)
const testingConnection = ref<string | null>(null)

// Form data
const formData = ref({
  name: '',
  systemType: '',
  connectionUrl: '',
  apiKey: '',
  username: '',
  password: ''
})

// System type options
const systemTypeOptions = [
  { value: 'ERP', label: 'ERP System' },
  { value: 'CMMS', label: 'CMMS System' },
  { value: 'SCADA', label: 'SCADA System' },
  { value: 'MES', label: 'MES System' },
  { value: 'CRM', label: 'CRM System' },
  { value: 'Custom', label: 'Custom Integration' }
]

// Computed
const connectedSystems = computed(() => externalSystems.filter((s: ExternalSystem) => s.status === ExternalSystemStatus.Connected))
const disconnectedSystems = computed(() => externalSystems.filter((s: ExternalSystem) => s.status === ExternalSystemStatus.Disconnected))
const errorSystems = computed(() => externalSystems.filter((s: ExternalSystem) => s.status === ExternalSystemStatus.Error))

const systemStats = computed(() => ({
  total: externalSystems.length,
  connected: connectedSystems.value.length,
  disconnected: disconnectedSystems.value.length,
  error: errorSystems.value.length,
  connectionRate: externalSystems.length > 0 ? Math.round((connectedSystems.value.length / externalSystems.length) * 100) : 0
}))

const pendingSynchronizations = computed(() => dataSynchronizations.filter((s: DataSynchronization) => s.status === SyncStatus.Pending))
const failedSynchronizations = computed(() => dataSynchronizations.filter((s: DataSynchronization) => s.status === SyncStatus.Failed))
const recentSynchronizations = computed(() => dataSynchronizations.slice(0, 10))

// Methods
const openCreateModal = () => {
  formData.value = {
    name: '',
    systemType: '',
    connectionUrl: '',
    apiKey: '',
    username: '',
    password: ''
  }
  showCreateModal.value = true
}

const openEditModal = (system: ExternalSystem) => {
  editingSystem.value = system
  formData.value = {
    name: system.name,
    systemType: system.systemType,
    connectionUrl: system.connectionUrl,
    apiKey: system.apiKey || '',
    username: system.username || '',
    password: ''
  }
  showEditModal.value = true
}

const openDetailsModal = (system: ExternalSystem) => {
  viewingSystem.value = system
  showDetailsModal.value = true
}

const closeModal = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showDetailsModal.value = false
  editingSystem.value = null
  viewingSystem.value = null
}

const handleSubmit = async () => {
  try {
    if (editingSystem.value) {
      // Update existing system
      const updateData: ExternalSystemUpdateDto = {
        name: formData.value.name || undefined,
        systemType: formData.value.systemType || undefined,
        connectionUrl: formData.value.connectionUrl || undefined,
        apiKey: formData.value.apiKey || undefined,
        username: formData.value.username || undefined,
        password: formData.value.password || undefined
      }
      
      await updateExternalSystem(editingSystem.value.id, updateData)
      toast.success('External system updated successfully')
    } else {
      // Create new system
      const createData: ExternalSystemCreateDto = {
        name: formData.value.name,
        systemType: formData.value.systemType,
        connectionUrl: formData.value.connectionUrl,
        apiKey: formData.value.apiKey,
        username: formData.value.username,
        password: formData.value.password
      }
      
      await createExternalSystem(createData)
      toast.success('External system created successfully')
    }
    
    closeModal()
  } catch (error) {
    console.error('Failed to save external system:', error)
    toast.error('Failed to save external system')
  }
}

const handleDelete = async (system: ExternalSystem) => {
  if (!confirm(`Are you sure you want to delete external system "${system.name}"? This action cannot be undone.`)) {
    return
  }
  
  try {
    await deleteExternalSystem(system.id)
    toast.success('External system deleted successfully')
  } catch (error) {
    console.error('Failed to delete external system:', error)
    toast.error('Failed to delete external system')
  }
}

const handleConnect = async (system: ExternalSystem) => {
  try {
    await connectExternalSystem(system.id)
    toast.success('External system connected')
  } catch (error) {
    console.error('Failed to connect external system:', error)
    toast.error('Failed to connect external system')
  }
}

const handleDisconnect = async (system: ExternalSystem) => {
  try {
    await disconnectExternalSystem(system.id)
    toast.success('External system disconnected')
  } catch (error) {
    console.error('Failed to disconnect external system:', error)
    toast.error('Failed to disconnect external system')
  }
}

const handleTestConnection = async (system: ExternalSystem) => {
  testingConnection.value = system.id
  try {
    const isConnected = await testExternalSystemConnection(system.id)
    if (isConnected) {
      toast.success(`Connection test successful for ${system.name}`)
    } else {
      toast.error(`Connection test failed for ${system.name}`)
    }
  } catch (error) {
    console.error('Failed to test connection:', error)
    toast.error('Failed to test connection')
  } finally {
    testingConnection.value = null
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getStatusBadgeClass = (status: ExternalSystemStatus) => {
  switch (status) {
    case ExternalSystemStatus.Connected:
      return 'bg-green-100 text-green-800'
    case ExternalSystemStatus.Disconnected:
      return 'bg-gray-100 text-gray-800'
    case ExternalSystemStatus.Error:
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const getStatusIcon = (status: ExternalSystemStatus) => {
  switch (status) {
    case ExternalSystemStatus.Connected:
      return CheckCircle
    case ExternalSystemStatus.Disconnected:
      return XCircle
    case ExternalSystemStatus.Error:
      return AlertTriangle
    default:
      return XCircle
  }
}

const getSystemTypeIcon = (systemType: string) => {
  switch (systemType.toUpperCase()) {
    case 'ERP':
      return Database
    case 'CMMS':
      return Settings
    case 'SCADA':
      return Wifi
    case 'MES':
      return Play
    case 'CRM':
      return Database
    default:
      return Plug
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    fetchExternalSystems(),
    fetchSystemIntegrations(),
    fetchDataSynchronizations()
  ])
})
</script>

<template>
  <BaseCard>
    <div class="external-systems space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">External System Integration</h1>
          <p class="text-gray-600 mt-2">Manage connections to ERP, CMMS, SCADA, and other external systems</p>
        </div>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="w-4 h-4 mr-2" />
          Add External System
        </BaseButton>
      </div>

      <!-- Stats Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard class="p-4">
          <div class="flex items-center">
            <Plug class="w-8 h-8 text-blue-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Total Systems</p>
              <p class="text-2xl font-bold">{{ systemStats.total }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <Wifi class="w-8 h-8 text-green-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Connected</p>
              <p class="text-2xl font-bold">{{ systemStats.connected }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <WifiOff class="w-8 h-8 text-gray-500 mr-3" />
            <div>
              <p class="text-sm text-gray-600">Disconnected</p>
              <p class="text-2xl font-bold">{{ systemStats.disconnected }}</p>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="p-4">
          <div class="flex items-center">
            <div class="w-8 h-8 bg-purple-100 rounded-full flex items-center justify-center mr-3">
              <span class="text-purple-600 font-bold text-sm">{{ systemStats.connectionRate }}%</span>
            </div>
            <div>
              <p class="text-sm text-gray-600">Connection Rate</p>
              <p class="text-2xl font-bold">{{ systemStats.connectionRate }}%</p>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Loading/Error States -->
      <div v-if="loading" class="text-center py-8">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        <p class="mt-2 text-gray-600">Loading external systems...</p>
      </div>

      <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4">
        <p class="text-red-700">{{ error }}</p>
        <BaseButton variant="outline" @click="fetchExternalSystems" class="mt-2">
          Retry
        </BaseButton>
      </div>

      <!-- External Systems Table -->
      <div v-else>
        <h2 class="text-xl font-semibold mb-4">External Systems</h2>
        
        <div v-if="externalSystems.length === 0" class="text-center py-12 bg-gray-50 rounded-lg">
          <Plug class="w-12 h-12 mx-auto text-gray-400 mb-4" />
          <p class="text-gray-500 mb-4">No external systems configured</p>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            Add Your First External System
          </BaseButton>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="min-w-full divide-y divide-gray-200">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">System</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Last Connected</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="system in externalSystems" :key="system.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <component :is="getSystemTypeIcon(system.systemType)" class="w-5 h-5 text-gray-400 mr-3" />
                    <div>
                      <div class="text-sm font-medium text-gray-900">{{ system.name }}</div>
                      <div class="text-sm text-gray-500 truncate max-w-xs">{{ system.connectionUrl }}</div>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {{ system.systemType }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full" :class="getStatusBadgeClass(system.status)">
                    <component :is="getStatusIcon(system.status)" class="w-3 h-3 mr-1 inline" />
                    {{ system.status }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {{ system.lastConnected !== '0001-01-01T00:00:00' ? formatDate(system.lastConnected) : 'Never' }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <div class="flex space-x-2">
                    <BaseButton variant="outline" size="sm" @click="openDetailsModal(system)">
                      <Settings class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton variant="outline" size="sm" @click="openEditModal(system)">
                      <Edit class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton 
                      variant="outline" 
                      size="sm" 
                      @click="handleTestConnection(system)"
                      :disabled="testingConnection === system.id"
                    >
                      <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': testingConnection === system.id }" />
                    </BaseButton>
                    <BaseButton 
                      :variant="system.status === ExternalSystemStatus.Connected ? 'outline' : 'primary'" 
                      size="sm" 
                      @click="system.status === ExternalSystemStatus.Connected ? handleDisconnect(system) : handleConnect(system)"
                    >
                      <component :is="system.status === ExternalSystemStatus.Connected ? WifiOff : Wifi" class="w-4 h-4" />
                    </BaseButton>
                    <BaseButton variant="danger" size="sm" @click="handleDelete(system)">
                      <Trash2 class="w-4 h-4" />
                    </BaseButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Recent Synchronizations -->
      <div v-if="dataSynchronizations.length > 0">
        <h2 class="text-xl font-semibold mb-4">Recent Synchronizations</h2>
        <BaseCard class="p-4">
          <div class="space-y-3">
            <div 
              v-for="sync in recentSynchronizations" 
              :key="sync.id"
              class="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
            >
              <div class="flex items-center">
                <div class="w-3 h-3 rounded-full mr-3" :class="{
                  'bg-yellow-500': sync.status === SyncStatus.Pending,
                  'bg-blue-500': sync.status === SyncStatus.Processing,
                  'bg-green-500': sync.status === SyncStatus.Completed,
                  'bg-red-500': sync.status === SyncStatus.Failed
                }"></div>
                <div>
                  <p class="text-sm font-medium">{{ EntityType[sync.entityType as keyof typeof EntityType] }} synchronization</p>
                  <p class="text-xs text-gray-500">{{ formatDate(sync.startedAt) }}</p>
                </div>
              </div>
              <div class="flex items-center space-x-2">
                <span class="px-2 py-1 text-xs rounded-full" :class="{
                  'bg-yellow-100 text-yellow-800': sync.status === SyncStatus.Pending,
                  'bg-blue-100 text-blue-800': sync.status === SyncStatus.Processing,
                  'bg-green-100 text-green-800': sync.status === SyncStatus.Completed,
                  'bg-red-100 text-red-800': sync.status === SyncStatus.Failed
                }">
                  {{ sync.status }}
                </span>
                <span class="text-xs text-gray-500">
                  {{ SyncDirection[sync.direction as keyof typeof SyncDirection] }}
                </span>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>

  <!-- Create/Edit Modal -->
  <div v-if="showCreateModal || showEditModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
    <BaseCard class="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
      <div class="p-6">
        <h2 class="text-xl font-semibold mb-4">
          {{ editingSystem ? 'Edit External System' : 'Add External System' }}
        </h2>
        
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <BaseInput
            v-model="formData.name"
            label="System Name"
            placeholder="Enter system name"
            required
          />
          
          <BaseSelect
            v-model="formData.systemType"
            label="System Type"
            :options="systemTypeOptions"
            required
          />
          
          <BaseInput
            v-model="formData.connectionUrl"
            label="Connection URL"
            placeholder="https://api.example.com"
            required
          />
          
          <BaseInput
            v-model="formData.apiKey"
            label="API Key"
            placeholder="Enter API key (optional)"
          />
          
          <BaseInput
            v-model="formData.username"
            label="Username"
            placeholder="Enter username (optional)"
          />
          
          <BaseInput
            v-model="formData.password"
            label="Password"
            type="password"
            placeholder="Enter password (optional)"
          />
          
          <div class="flex justify-end space-x-3 pt-4">
            <BaseButton variant="outline" @click="closeModal">Cancel</BaseButton>
            <BaseButton variant="primary" type="submit" :disabled="loading">
              {{ editingSystem ? 'Update System' : 'Add System' }}
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Details Modal -->
  <div v-if="showDetailsModal && viewingSystem" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
    <BaseCard class="w-full max-w-2xl">
      <div class="p-6">
        <h2 class="text-xl font-semibold mb-4">System Details</h2>
        
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700">Name</label>
            <p class="mt-1 text-sm text-gray-900">{{ viewingSystem.name }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Type</label>
            <p class="mt-1 text-sm text-gray-900">{{ viewingSystem.systemType }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Connection URL</label>
            <p class="mt-1 text-sm text-gray-900 font-mono bg-gray-50 p-2 rounded">{{ viewingSystem.connectionUrl }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Status</label>
            <span class="mt-1 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium" :class="getStatusBadgeClass(viewingSystem.status)">
              <component :is="getStatusIcon(viewingSystem.status)" class="w-3 h-3 mr-1" />
              {{ viewingSystem.status }}
            </span>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Last Connected</label>
            <p class="mt-1 text-sm text-gray-900">
              {{ viewingSystem.lastConnected !== '0001-01-01T00:00:00' ? formatDate(viewingSystem.lastConnected) : 'Never' }}
            </p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Created</label>
            <p class="mt-1 text-sm text-gray-900">{{ formatDate(viewingSystem.createdAt) }}</p>
          </div>
          
          <div>
            <label class="block text-sm font-medium text-gray-700">Last Updated</label>
            <p class="mt-1 text-sm text-gray-900">{{ formatDate(viewingSystem.updatedAt) }}</p>
          </div>
        </div>
        
        <div class="flex justify-end space-x-3 pt-4">
          <BaseButton variant="outline" @click="closeModal">Close</BaseButton>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.external-systems {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .external-systems {
    padding: 0.5rem;
  }
}
</style>