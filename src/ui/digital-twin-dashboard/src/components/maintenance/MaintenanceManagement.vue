<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  planMaintenance,
  startMaintenance,
  completeMaintenance,
  cancelMaintenance,
  fetchMaintenanceHistory,
  fetchActiveMaintenance
} from '@/services/maintenance.service'
import { 
  Wrench, 
  Play, 
  CheckCircle, 
  XCircle,
  Clock,
  Calendar,
  Filter,
  Search,
  Plus,
  History
} from 'lucide-vue-next'

const toast = useToast()

// State
const maintenanceRecords: any = ref([])
const activeMaintenance: any = ref([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const typeFilter = ref('all')

// Modal state
const showPlanModal = ref(false)
const showCompleteModal = ref(false)
const showCancelModal = ref(false)
const selectedRecord: any = ref(null)

// Form state
const planForm: any = ref({
  machineId: '',
  type: 'preventive',
  plannedDate: new Date().toISOString().split('T')[0],
  notes: '',
  alertId: ''
})

const completeForm: any = ref({
  performedBy: '',
  finalNotes: ''
})

const cancelReason = ref('')

// Computed
const filteredRecords = computed(() => {
  let filtered = [...maintenanceRecords.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((record: any) => 
      (record.machineName?.toLowerCase().includes(query) ||
       record.type?.toLowerCase().includes(query) ||
       record.performedBy?.toLowerCase().includes(query) ||
       record.notes?.toLowerCase().includes(query))
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter((record: any) => record.status?.toLowerCase() === statusFilter.value)
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter((record: any) => record.type?.toLowerCase() === typeFilter.value)
  }
  
  return filtered
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Planned', value: 'planned' },
  { label: 'InProgress', value: 'inprogress' },
  { label: 'Completed', value: 'completed' },
  { label: 'Cancelled', value: 'cancelled' }
]

const typeOptions = [
  { label: 'All Types', value: 'all' },
  { label: 'Preventive', value: 'preventive' },
  { label: 'Corrective', value: 'corrective' },
  { label: 'Emergency', value: 'emergency' }
]

// Methods
const loadMaintenanceRecords = async () => {
  try {
    loading.value = true
    
    // Load both active and historical records
    const [active, history] = await Promise.all([
      fetchActiveMaintenance(),
      fetchMaintenanceHistory('')
    ])
    
    activeMaintenance.value = active
    maintenanceRecords.value = [...active, ...history].sort((a: any, b: any) => {
      const dateA = a.plannedDate ? new Date(a.plannedDate).getTime() : 0
      const dateB = b.plannedDate ? new Date(b.plannedDate).getTime() : 0
      return dateB - dateA
    })
  } catch (error) {
    console.error('Failed to load maintenance records:', error)
    toast.error('Unable to load maintenance records')
  } finally {
    loading.value = false
  }
}

const refreshMaintenance = async () => {
  await loadMaintenanceRecords()
  toast.success('Maintenance records refreshed')
}

const openPlanModal = () => {
  resetPlanForm()
  showPlanModal.value = true
}

const openCompleteModal = (record: any) => {
  selectedRecord.value = record
  completeForm.value = {
    performedBy: '',
    finalNotes: ''
  }
  showCompleteModal.value = true
}

const openCancelModal = (record: any) => {
  selectedRecord.value = record
  cancelReason.value = ''
  showCancelModal.value = true
}

const closeModals = () => {
  showPlanModal.value = false
  showCompleteModal.value = false
  showCancelModal.value = false
  selectedRecord.value = null
  resetPlanForm()
}

const resetPlanForm = () => {
  planForm.value = {
    machineId: '',
    type: 'preventive',
    plannedDate: new Date().toISOString().split('T')[0],
    notes: '',
    alertId: ''
  }
}

const handlePlanMaintenance = async () => {
  try {
    await planMaintenance(planForm.value)
    toast.success('Maintenance planned successfully')
    closeModals()
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Failed to plan maintenance:', error)
    toast.error('Failed to plan maintenance')
  }
}

const handleStartMaintenance = async (record: any) => {
  try {
    await startMaintenance(record.id)
    toast.success('Maintenance started successfully')
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Failed to start maintenance:', error)
    toast.error('Failed to start maintenance')
  }
}

const handleCompleteMaintenance = async () => {
  if (!selectedRecord.value) return
  
  try {
    await completeMaintenance(selectedRecord.value.id, completeForm.value)
    toast.success('Maintenance completed successfully')
    closeModals()
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Failed to complete maintenance:', error)
    toast.error('Failed to complete maintenance')
  }
}

const handleCancelMaintenance = async () => {
  if (!selectedRecord.value) return
  
  try {
    await cancelMaintenance(selectedRecord.value.id, cancelReason.value)
    toast.success('Maintenance cancelled successfully')
    closeModals()
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Failed to cancel maintenance:', error)
    toast.error('Failed to cancel maintenance')
  }
}

const getStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    'planned': 'bg-blue-500',
    'inprogress': 'bg-yellow-500',
    'completed': 'bg-green-500',
    'cancelled': 'bg-gray-500'
  }
  return colors[status?.toLowerCase()] || 'bg-gray-500'
}

const getStatusDisplay = (status: string): string => {
  const displays: Record<string, string> = {
    'planned': 'Planned',
    'inprogress': 'In Progress',
    'completed': 'Completed',
    'cancelled': 'Cancelled'
  }
  return displays[status?.toLowerCase()] || status
}

const getTypeDisplay = (type: string): string => {
  const displays: Record<string, string> = {
    'preventive': 'Preventive',
    'corrective': 'Corrective',
    'emergency': 'Emergency'
  }
  return displays[type?.toLowerCase()] || type
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString()
}

const formatDateTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleString()
}

// Lifecycle
onMounted(async () => {
  await loadMaintenanceRecords()
})
</script>

<template>
  <BaseCard>
    <div class="maintenance-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Maintenance Management</h1>
          <p class="text-gray-600 mt-2">Plan, track, and manage equipment maintenance</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="refreshMaintenance">
            <History class="w-4 h-4 mr-2" />
            Refresh
          </BaseButton>
          <BaseButton variant="primary" @click="openPlanModal">
            <Plus class="w-4 h-4 mr-2" />
            Plan Maintenance
          </BaseButton>
        </div>
      </div>

      <!-- Active Maintenance Summary -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Maintenance</p>
                <p class="text-2xl font-bold">{{ activeMaintenance.length }}</p>
              </div>
              <Wrench class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Planned</p>
                <p class="text-2xl font-bold">
                  {{ maintenanceRecords.filter((r: any) => r.status === 'planned').length }}
                </p>
              </div>
              <Clock class="w-8 h-8 text-gray-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">In Progress</p>
                <p class="text-2xl font-bold">
                  {{ maintenanceRecords.filter((r: any) => r.status === 'inprogress').length }}
                </p>
              </div>
              <Play class="w-8 h-8 text-yellow-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Completed Today</p>
                <p class="text-2xl font-bold">
                  {{ maintenanceRecords.filter((r: any) => 
                    r.status === 'completed' && 
                    new Date(r.completedAt || '').toDateString() === new Date().toDateString()
                  ).length }}
                </p>
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
                  placeholder="Search maintenance records..."
                  class="pl-10"
                />
              </div>
            </div>
            
            <BaseSelect
              v-model="statusFilter"
              :options="statusOptions"
            />
            
            <BaseSelect
              v-model="typeFilter"
              :options="typeOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Maintenance Records List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Maintenance Records ({{ filteredRecords.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading maintenance records...</p>
          </div>
          
          <div v-else-if="filteredRecords.length === 0" class="text-center py-8">
            <Wrench class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No maintenance records found</p>
            <p class="text-sm text-gray-500 mt-1">Try adjusting your search or filters</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="record in filteredRecords"
              :key="record.id"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex flex-col sm:flex-row justify-between gap-4">
                  <div class="flex-1">
                    <div class="flex items-start justify-between mb-3">
                      <div>
                        <h3 class="text-lg font-semibold text-gray-900">
                          {{ record.machineName || 'Unnamed Machine' }}
                        </h3>
                        <p class="text-gray-600">{{ getTypeDisplay(record.type) }} Maintenance</p>
                      </div>
                      <span 
                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium text-white"
                        :class="getStatusColor(record.status)"
                      >
                        {{ getStatusDisplay(record.status) }}
                      </span>
                    </div>
                    
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600">
                      <div>
                        <span class="font-medium">Planned Date:</span>
                        <p>{{ formatTime(record.plannedDate) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Performed By:</span>
                        <p>{{ record.performedBy || 'Not assigned' }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Notes:</span>
                        <p class="truncate">{{ record.notes || 'No notes' }}</p>
                      </div>
                      <div v-if="record.startedAt">
                        <span class="font-medium">Started:</span>
                        <p>{{ formatDateTime(record.startedAt) }}</p>
                      </div>
                      <div v-if="record.completedAt">
                        <span class="font-medium">Completed:</span>
                        <p>{{ formatDateTime(record.completedAt) }}</p>
                      </div>
                      <div v-if="record.cancelledAt">
                        <span class="font-medium">Cancelled:</span>
                        <p>{{ formatDateTime(record.cancelledAt) }}</p>
                      </div>
                    </div>
                  </div>
                  
                  <div class="flex flex-col gap-2">
                    <template v-if="record.status === 'planned'">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="handleStartMaintenance(record)"
                      >
                        <Play class="w-4 h-4 mr-1" />
                        Start
                      </BaseButton>
                      <BaseButton
                        size="sm"
                        variant="outline"
                        @click="openCancelModal(record)"
                        class="text-red-600 hover:text-red-700"
                      >
                        <XCircle class="w-4 h-4 mr-1" />
                        Cancel
                      </BaseButton>
                    </template>
                    
                    <template v-else-if="record.status === 'inprogress'">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="openCompleteModal(record)"
                      >
                        <CheckCircle class="w-4 h-4 mr-1" />
                        Complete
                      </BaseButton>
                      <BaseButton
                        size="sm"
                        variant="outline"
                        @click="openCancelModal(record)"
                        class="text-red-600 hover:text-red-700"
                      >
                        <XCircle class="w-4 h-4 mr-1" />
                        Cancel
                      </BaseButton>
                    </template>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>

  <!-- Plan Maintenance Modal -->
  <div 
    v-if="showPlanModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Plan Maintenance</h2>
        
        <form @submit.prevent="handlePlanMaintenance" class="space-y-4">
          <BaseInput
            v-model="planForm.machineId"
            label="Machine ID"
            required
          />
          
          <BaseSelect
            v-model="planForm.type"
            :options="[
              { label: 'Preventive', value: 'preventive' },
              { label: 'Corrective', value: 'corrective' },
              { label: 'Emergency', value: 'emergency' }
            ]"
            label="Maintenance Type"
            required
          />
          
          <BaseInput
            v-model="planForm.plannedDate"
            label="Planned Date"
            type="date"
            required
          />
          
          <BaseInput
            v-model="planForm.notes"
            label="Notes"
            type="textarea"
            rows="3"
          />
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              Plan Maintenance
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Complete Maintenance Modal -->
  <div 
    v-if="showCompleteModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Complete Maintenance</h2>
        
        <div class="mb-4 p-3 bg-blue-50 rounded-lg">
          <p class="text-sm text-blue-800">
            Completing maintenance for: 
            <strong>{{ selectedRecord?.machineName }}</strong>
          </p>
        </div>
        
        <form @submit.prevent="handleCompleteMaintenance" class="space-y-4">
          <BaseInput
            v-model="completeForm.performedBy"
            label="Performed By"
            required
          />
          
          <BaseInput
            v-model="completeForm.finalNotes"
            label="Final Notes"
            type="textarea"
            rows="3"
          />
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              Complete Maintenance
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Cancel Maintenance Modal -->
  <div 
    v-if="showCancelModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Cancel Maintenance</h2>
        
        <div class="mb-4 p-3 bg-yellow-50 rounded-lg">
          <p class="text-sm text-yellow-800">
            Cancelling maintenance for: 
            <strong>{{ selectedRecord?.machineName }}</strong>
          </p>
        </div>
        
        <form @submit.prevent="handleCancelMaintenance" class="space-y-4">
          <BaseInput
            v-model="cancelReason"
            label="Cancellation Reason"
            type="textarea"
            rows="3"
            required
          />
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton 
              variant="primary" 
              type="submit"
              class="bg-red-600 hover:bg-red-700"
            >
              Cancel Maintenance
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.maintenance-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .maintenance-management {
    padding: 0.5rem;
  }
}
</style>