<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchActiveAlerts,
  acknowledgeAlert
} from '@/services/alerts.service'
import { 
  Bell, 
  CheckCircle, 
  AlertTriangle,
  Info,
  XCircle,
  Clock,
  Filter,
  Search,
  RefreshCw,
  Eye
} from 'lucide-vue-next'

const toast = useToast()

// State
const alerts: any = ref([])
const loading = ref(false)
const searchQuery = ref('')
const severityFilter = ref('all')
const statusFilter = ref('all')
const machineFilter = ref('all')

// Modal state
const showAcknowledgeModal = ref(false)
const selectedAlert: any = ref(null)
const acknowledgeComment = ref('')

// Computed
const filteredAlerts = computed(() => {
  let filtered = [...alerts.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((alert: any) => 
      alert.message?.toLowerCase().includes(query) ||
      alert.machineName?.toLowerCase().includes(query) ||
      alert.severity?.toLowerCase().includes(query)
    )
  }
  
  // Apply severity filter
  if (severityFilter.value !== 'all') {
    filtered = filtered.filter((alert: any) => 
      alert.severity?.toLowerCase() === severityFilter.value
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    const isAcknowledged = statusFilter.value === 'acknowledged'
    filtered = filtered.filter((alert: any) => 
      alert.isAcknowledged === isAcknowledged
    )
  }
  
  // Apply machine filter
  if (machineFilter.value !== 'all') {
    filtered = filtered.filter((alert: any) => 
      alert.machineId === machineFilter.value
    )
  }
  
  return filtered
})

const activeAlerts = computed(() => alerts.value.filter((a: any) => !a.isAcknowledged))
const acknowledgedAlerts = computed(() => alerts.value.filter((a: any) => a.isAcknowledged))

const severityOptions = [
  { label: 'All Severities', value: 'all' },
  { label: 'Critical', value: 'critical' },
  { label: 'Warning', value: 'warning' },
  { label: 'Info', value: 'info' }
]

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Active', value: 'active' },
  { label: 'Acknowledged', value: 'acknowledged' }
]

const machineOptions = computed(() => {
  const machines = [...new Set(alerts.value.map((a: any) => a.machineId))]
  return [
    { label: 'All Machines', value: 'all' },
    ...machines.map((id: unknown) => ({ 
      label: alerts.value.find((a: any) => a.machineId === id)?.machineName || String(id),
      value: String(id)
    }))
  ]
})

// Methods
const loadAlerts = async () => {
  try {
    loading.value = true
    alerts.value = await fetchActiveAlerts('')
  } catch (error) {
    console.error('Failed to load alerts:', error)
    toast.error('Unable to load alerts')
  } finally {
    loading.value = false
  }
}

const refreshAlerts = async () => {
  await loadAlerts()
  toast.success('Alerts refreshed')
}

const openAcknowledgeModal = (alert: any) => {
  selectedAlert.value = alert
  acknowledgeComment.value = ''
  showAcknowledgeModal.value = true
}

const closeAcknowledgeModal = () => {
  showAcknowledgeModal.value = false
  selectedAlert.value = null
  acknowledgeComment.value = ''
}

const handleAcknowledgeAlert = async () => {
  if (!selectedAlert.value) return
  
  try {
    await acknowledgeAlert(selectedAlert.value.id)
    toast.success('Alert acknowledged successfully')
    
    // Update local state
    const alertIndex = alerts.value.findIndex((a: any) => a.id === selectedAlert.value.id)
    if (alertIndex !== -1) {
      alerts.value[alertIndex] = {
        ...alerts.value[alertIndex],
        isAcknowledged: true,
        acknowledgedBy: 'current_user',
        acknowledgedAt: new Date().toISOString()
      }
    }
    
    closeAcknowledgeModal()
  } catch (error) {
    console.error('Failed to acknowledge alert:', error)
    toast.error('Failed to acknowledge alert')
  }
}

const getSeverityColor = (severity: string): string => {
  const colors: Record<string, string> = {
    'critical': 'bg-red-500',
    'warning': 'bg-yellow-500',
    'info': 'bg-blue-500'
  }
  return colors[severity?.toLowerCase()] || 'bg-gray-500'
}

const getSeverityIcon = (severity: string) => {
  const icons: Record<string, any> = {
    'critical': AlertTriangle,
    'warning': AlertTriangle,
    'info': Info
  }
  return icons[severity?.toLowerCase()] || Bell
}

const getSeverityDisplay = (severity: string): string => {
  const displays: Record<string, string> = {
    'critical': 'Critical',
    'warning': 'Warning',
    'info': 'Info'
  }
  return displays[severity?.toLowerCase()] || severity
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleString()
}

const getTimeAgo = (dateString?: string): string => {
  if (!dateString) return 'Unknown'
  
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMinutes = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMinutes / 60)
  const diffDays = Math.floor(diffHours / 24)
  
  if (diffMinutes < 1) return 'Just now'
  if (diffMinutes < 60) return `${diffMinutes}m ago`
  if (diffHours < 24) return `${diffHours}h ago`
  return `${diffDays}d ago`
}

// Lifecycle
onMounted(async () => {
  await loadAlerts()
})
</script>

<template>
  <BaseCard>
    <div class="alert-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Alert Management</h1>
          <p class="text-gray-600 mt-2">Monitor and manage system alerts in real-time</p>
        </div>
        <BaseButton variant="outline" @click="refreshAlerts">
          <RefreshCw class="w-4 h-4 mr-2" />
          Refresh Alerts
        </BaseButton>
      </div>

      <!-- Alert Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Alerts</p>
                <p class="text-2xl font-bold">{{ activeAlerts.length }}</p>
              </div>
              <Bell class="w-8 h-8 text-red-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Critical</p>
                <p class="text-2xl font-bold">
                  {{ alerts.filter((a: any) => a.severity === 'critical' && !a.isAcknowledged).length }}
                </p>
              </div>
              <AlertTriangle class="w-8 h-8 text-red-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Warnings</p>
                <p class="text-2xl font-bold">
                  {{ alerts.filter((a: any) => a.severity === 'warning' && !a.isAcknowledged).length }}
                </p>
              </div>
              <AlertTriangle class="w-8 h-8 text-yellow-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Acknowledged</p>
                <p class="text-2xl font-bold">{{ acknowledgedAlerts.length }}</p>
              </div>
              <CheckCircle class="w-8 h-8 text-green-500" />
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
                  placeholder="Search alerts..."
                  class="pl-10"
                />
              </div>
            </div>
            
            <BaseSelect
              v-model="severityFilter"
              :options="severityOptions"
            />
            
            <BaseSelect
              v-model="statusFilter"
              :options="statusOptions"
            />
            
            <BaseSelect
              v-model="machineFilter"
              :options="machineOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Alerts List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Alerts ({{ filteredAlerts.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading alerts...</p>
          </div>
          
          <div v-else-if="filteredAlerts.length === 0" class="text-center py-8">
            <Bell class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No alerts found</p>
            <p class="text-sm text-gray-500 mt-1">Try adjusting your search or filters</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="alert in filteredAlerts"
              :key="alert.id"
              class="hover:shadow-md transition-shadow"
              :class="{ 'border-l-4 border-red-500': alert.severity === 'critical' }"
            >
              <div class="p-4">
                <div class="flex flex-col sm:flex-row justify-between gap-4">
                  <div class="flex-1">
                    <div class="flex items-start justify-between mb-3">
                      <div class="flex items-start gap-3">
                        <component 
                          :is="getSeverityIcon(alert.severity)"
                          class="w-6 h-6 mt-0.5"
                          :class="getSeverityColor(alert.severity)"
                        />
                        <div>
                          <h3 class="text-lg font-semibold text-gray-900">
                            {{ alert.machineName || 'Unknown Machine' }}
                          </h3>
                          <p class="text-gray-700 mt-1">{{ alert.message }}</p>
                        </div>
                      </div>
                      <div class="flex items-center gap-2">
                        <span 
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium text-white"
                          :class="getSeverityColor(alert.severity)"
                        >
                          {{ getSeverityDisplay(alert.severity) }}
                        </span>
                        <span 
                          v-if="alert.isAcknowledged"
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800"
                        >
                          <CheckCircle class="w-3 h-3 mr-1" />
                          Acknowledged
                        </span>
                      </div>
                    </div>
                    
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600">
                      <div>
                        <span class="font-medium">Generated:</span>
                        <p>{{ formatTime(alert.createdAt) }}</p>
                        <p class="text-xs text-gray-500">{{ getTimeAgo(alert.createdAt) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Machine ID:</span>
                        <p class="font-mono text-xs">{{ alert.machineId }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Status:</span>
                        <p>{{ alert.isAcknowledged ? 'Acknowledged' : 'Active' }}</p>
                      </div>
                      <div v-if="alert.acknowledgedAt">
                        <span class="font-medium">Acknowledged:</span>
                        <p>{{ formatTime(alert.acknowledgedAt) }}</p>
                        <p class="text-xs text-gray-500">by {{ alert.acknowledgedBy || 'Unknown' }}</p>
                      </div>
                    </div>
                  </div>
                  
                  <div class="flex flex-col gap-2">
                    <template v-if="!alert.isAcknowledged">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="openAcknowledgeModal(alert)"
                      >
                        <CheckCircle class="w-4 h-4 mr-1" />
                        Acknowledge
                      </BaseButton>
                    </template>
                    <template v-else>
                      <BaseButton
                        size="sm"
                        variant="outline"
                        disabled
                      >
                        <CheckCircle class="w-4 h-4 mr-1" />
                        Acknowledged
                      </BaseButton>
                    </template>
                    
                    <BaseButton
                      size="sm"
                      variant="outline"
                    >
                      <Eye class="w-4 h-4 mr-1" />
                      Details
                    </BaseButton>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>

  <!-- Acknowledge Alert Modal -->
  <div 
    v-if="showAcknowledgeModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeAcknowledgeModal"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Acknowledge Alert</h2>
        
        <div class="mb-4 p-3 bg-blue-50 rounded-lg">
          <p class="text-sm text-blue-800">
            Acknowledging alert for: 
            <strong>{{ selectedAlert?.machineName }}</strong>
          </p>
          <p class="text-sm text-blue-700 mt-1">{{ selectedAlert?.message }}</p>
        </div>
        
        <form @submit.prevent="handleAcknowledgeAlert" class="space-y-4">
          <BaseInput
            v-model="acknowledgeComment"
            label="Comment (Optional)"
            type="textarea"
            rows="3"
            placeholder="Add any notes about this acknowledgment..."
          />
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeAcknowledgeModal">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              Acknowledge Alert
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.alert-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .alert-management {
    padding: 0.5rem;
  }
}
</style>