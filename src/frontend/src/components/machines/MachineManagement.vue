<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchMachines, 
  createMachine, 
  updateMachine, 
  deleteMachine 
} from '@/services/machines.service'
import type { MachineDto, MachineCreateDto, MachineUpdateDto } from '@/api/types'
import { 
  Plus, 
  Edit, 
  Trash2, 
  Activity,
  Filter,
  Search,
  RefreshCw
} from 'lucide-vue-next'

const toast = useToast()

// State
const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const typeFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedMachine = ref<MachineDto | null>(null)

// Form state
const machineForm = ref<MachineCreateDto>({
  name: '',
  serialNumber: '',
  type: '',
  manufacturer: '',
  model: '',
  status: 'operational',
  criticality: 3,
  location: '',
  installationDate: '',
  warrantyExpiry: '',
  lastMaintenance: '',
  nextMaintenance: '',
  maintenanceInterval: 30,
  degradationModel: 'linear',
  threshold: 80,
  isActive: true,
  metadata: {}
})

// Computed
const filteredMachines = computed(() => {
  let filtered = [...machines.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(machine => 
      machine.name.toLowerCase().includes(query) ||
      machine.serialNumber?.toLowerCase().includes(query) ||
      machine.manufacturer?.toLowerCase().includes(query) ||
      machine.location.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(machine => 
      machine.status.toString() === statusFilter.value
    )
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter(machine => machine.type === typeFilter.value)
  }
  
  return filtered
})

const machineTypes = computed(() => {
  const types = [...new Set(machines.value.map(m => m.type))]
  return types.sort()
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

const typeOptions = computed(() => [
  { label: 'All Types', value: 'all' },
  ...machineTypes.value.map(type => ({ label: type, value: type }))
])

// Methods
const loadMachines = async () => {
  try {
    loading.value = true
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Failed to load machines:', error)
    toast.error('Unable to load machines')
  } finally {
    loading.value = false
  }
}

const refreshMachines = async () => {
  await loadMachines()
  toast.success('Machines refreshed')
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openEditModal = (machine: MachineDto) => {
  selectedMachine.value = machine
  machineForm.value = {
    name: machine.name,
    serialNumber: machine.serialNumber || '',
    type: machine.type,
    manufacturer: machine.manufacturer || '',
    model: machine.model || '',
    status: typeof machine.status === 'number' ? 'operational' : machine.status,
    criticality: machine.criticality || 3,
    location: machine.location,
    installationDate: machine.installationDate || '',
    warrantyExpiry: machine.warrantyExpiry || '',
    lastMaintenance: machine.lastMaintenance || '',
    nextMaintenance: machine.nextMaintenance || '',
    maintenanceInterval: machine.maintenanceInterval || 30,
    degradationModel: machine.degradationModel || 'linear',
    threshold: machine.threshold || 80,
    isActive: machine.isActive !== undefined ? machine.isActive : true,
    metadata: machine.metadata || {}
  }
  showEditModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  selectedMachine.value = null
  resetForm()
}

const resetForm = () => {
  machineForm.value = {
    name: '',
    serialNumber: '',
    type: '',
    manufacturer: '',
    model: '',
    status: 'operational',
    criticality: 3,
    location: '',
    installationDate: '',
    warrantyExpiry: '',
    lastMaintenance: '',
    nextMaintenance: '',
    maintenanceInterval: 30,
    degradationModel: 'linear',
    threshold: 80,
    isActive: true,
    metadata: {}
  }
}

const handleSubmit = async () => {
  try {
    if (selectedMachine.value) {
      // Update existing machine
      const updateData: MachineUpdateDto = {
        ...machineForm.value,
        status: machineForm.value.status
      }
      await updateMachine(selectedMachine.value.id, updateData)
      toast.success('Machine updated successfully')
    } else {
      // Create new machine
      await createMachine(machineForm.value)
      toast.success('Machine created successfully')
    }
    
    closeModals()
    await loadMachines()
  } catch (error) {
    console.error('Failed to save machine:', error)
    toast.error('Failed to save machine')
  }
}

const handleDelete = async (machine: MachineDto) => {
  if (!confirm(`Are you sure you want to delete machine "${machine.name}"?`)) {
    return
  }
  
  try {
    await deleteMachine(machine.id)
    machines.value = machines.value.filter(m => m.id !== machine.id)
    toast.success('Machine deleted successfully')
  } catch (error) {
    console.error('Failed to delete machine:', error)
    toast.error('Failed to delete machine')
  }
}

const getStatusColor = (status: string | number): string => {
  const statusStr = typeof status === 'number' ? status.toString() : status
  const colors: Record<string, string> = {
    'operational': 'bg-green-500',
    'warning': 'bg-yellow-500',
    'critical': 'bg-red-500',
    'maintenance': 'bg-blue-500',
    'offline': 'bg-gray-500'
  }
  return colors[statusStr.toLowerCase()] || 'bg-gray-500'
}

const getStatusDisplay = (status: string | number): string => {
  if (typeof status === 'number') {
    const statusMap: Record<number, string> = {
      0: 'Operational',
      1: 'Warning',
      2: 'Critical',
      3: 'Maintenance',
      4: 'Offline'
    }
    return statusMap[status] || status.toString()
  }
  return status.charAt(0).toUpperCase() + status.slice(1)
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString()
}

// Lifecycle
onMounted(async () => {
  await loadMachines()
})
</script>

<template>
  <BaseCard>
    <div class="machine-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Machine Management</h1>
          <p class="text-gray-600 mt-2">Monitor and manage all equipment</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="refreshMachines">
            <RefreshCw class="w-4 h-4 mr-2" />
            Refresh
          </BaseButton>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            Add Machine
          </BaseButton>
        </div>
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
                  placeholder="Search machines..."
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

      <!-- Machines List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Machines ({{ filteredMachines.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading machines...</p>
          </div>
          
          <div v-else-if="filteredMachines.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No machines found</p>
            <p class="text-sm text-gray-500 mt-1">Try adjusting your search or filters</p>
          </div>
          
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <BaseCard
              v-for="machine in filteredMachines"
              :key="machine.id"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex justify-between items-start mb-3">
                  <h3 class="text-lg font-semibold text-gray-900">{{ machine.name }}</h3>
                  <span 
                    class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium text-white"
                    :class="getStatusColor(machine.status)"
                  >
                    {{ getStatusDisplay(machine.status) }}
                  </span>
                </div>
                
                <div class="space-y-2 text-sm text-gray-600">
                  <div class="flex justify-between">
                    <span>Type:</span>
                    <span class="font-medium">{{ machine.type }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span>Serial:</span>
                    <span class="font-medium">{{ machine.serialNumber || 'N/A' }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span>Location:</span>
                    <span class="font-medium">{{ machine.location }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span>Last Maintenance:</span>
                    <span class="font-medium">{{ formatTime(machine.lastMaintenance) }}</span>
                  </div>
                  <div class="flex justify-between">
                    <span>Next Maintenance:</span>
                    <span class="font-medium">{{ formatTime(machine.nextMaintenance) }}</span>
                  </div>
                </div>
                
                <div class="flex gap-2 mt-4">
                  <BaseButton
                    size="sm"
                    variant="outline"
                    @click="openEditModal(machine)"
                  >
                    <Edit class="w-4 h-4 mr-1" />
                    Edit
                  </BaseButton>
                  <BaseButton
                    size="sm"
                    variant="outline"
                    @click="handleDelete(machine)"
                    class="text-red-600 hover:text-red-700 hover:bg-red-50"
                  >
                    <Trash2 class="w-4 h-4 mr-1" />
                    Delete
                  </BaseButton>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>

  <!-- Create/Edit Modal -->
  <div 
    v-if="showCreateModal || showEditModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard 
      class="w-full max-w-2xl max-h-[90vh] overflow-y-auto"
      @click.stop
    >
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">
          {{ selectedMachine ? 'Edit Machine' : 'Add New Machine' }}
        </h2>
        
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <BaseInput
              v-model="machineForm.name"
              label="Machine Name"
              required
            />
            
            <BaseInput
              v-model="machineForm.serialNumber"
              label="Serial Number"
            />
            
            <BaseInput
              v-model="machineForm.type"
              label="Type"
              required
            />
            
            <BaseInput
              v-model="machineForm.manufacturer"
              label="Manufacturer"
            />
            
            <BaseInput
              v-model="machineForm.model"
              label="Model"
            />
            
            <BaseSelect
              v-model="machineForm.status"
              :options="[
                { label: 'Operational', value: 'operational' },
                { label: 'Warning', value: 'warning' },
                { label: 'Critical', value: 'critical' },
                { label: 'Maintenance', value: 'maintenance' },
                { label: 'Offline', value: 'offline' }
              ]"
              label="Status"
              required
            />
            
            <BaseInput
              v-model.number="machineForm.criticality"
              label="Criticality (1-5)"
              type="number"
              min="1"
              max="5"
            />
            
            <BaseInput
              v-model="machineForm.location"
              label="Location"
              required
            />
          </div>
          
          <div class="flex justify-end gap-3 pt-6">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ selectedMachine ? 'Update' : 'Create' }} Machine
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.machine-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .machine-management {
    padding: 0.5rem;
  }
}
</style>