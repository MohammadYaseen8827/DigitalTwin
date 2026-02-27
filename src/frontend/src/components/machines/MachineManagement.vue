<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseModal from '@/components/base/BaseModal.vue'
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
const isModalOpen = ref(false)
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
      machine.status.toString().toLowerCase() === statusFilter.value.toLowerCase()
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
    toast.error('Unable to load machines')
  } finally {
    loading.value = false
  }
}

const refreshMachines = async () => {
  await loadMachines()
  toast.success('Machines refreshed')
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

const openCreateModal = () => {
  selectedMachine.value = null
  resetForm()
  isModalOpen.value = true
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
  isModalOpen.value = true
}

const closeModals = () => {
  isModalOpen.value = false
  selectedMachine.value = null
  resetForm()
}

const handleSubmit = async () => {
  try {
    if (selectedMachine.value) {
      await updateMachine(selectedMachine.value.id, machineForm.value)
      toast.success('Machine updated successfully')
    } else {
      await createMachine(machineForm.value)
      toast.success('Machine created successfully')
    }
    
    closeModals()
    await loadMachines()
  } catch (error) {
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
  <div class="machine-management space-y-6 p-4">
    <!-- Header -->
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900">Machine Management</h1>
        <p class="text-gray-600 mt-2">Monitor and manage all equipment</p>
      </div>
      <div class="flex gap-2">
        <BaseButton variant="outline" @click="refreshMachines">
          <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
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
      <div class="p-4 flex flex-col sm:flex-row gap-4">
        <div class="flex-1 relative">
          <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
          <BaseInput
            v-model="searchQuery"
            placeholder="Search machines..."
            class="pl-10"
          />
        </div>
        
        <BaseSelect
          v-slot:prefix
          v-model="statusFilter"
          :options="statusOptions"
          class="w-full sm:w-48"
        />
        
        <BaseSelect
          v-model="typeFilter"
          :options="typeOptions"
          class="w-full sm:w-48"
        />
      </div>
    </BaseCard>

    <!-- Machines List -->
    <BaseCard>
      <div class="p-6">
        <div class="flex items-center justify-between mb-4">
          <h2 class="text-xl font-semibold">
            Machines ({{ filteredMachines.length }})
          </h2>
        </div>
        
        <div v-if="loading" class="text-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-500 mx-auto"></div>
          <p class="mt-4 text-gray-600 font-medium">Loading equipment data...</p>
        </div>
        
        <div v-else-if="filteredMachines.length === 0" class="text-center py-12">
          <Activity class="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <p class="text-gray-600 text-lg font-medium">No machines found</p>
          <p class="text-gray-500 mt-1">Try adjusting your search or filters to see results</p>
        </div>
        
        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <BaseCard
            v-for="machine in filteredMachines"
            :key="machine.id"
            class="hover:shadow-lg transition-all duration-200 border-l-4"
            :class="[
              machine.status === 'critical' ? 'border-red-500' : 
              machine.status === 'warning' ? 'border-yellow-500' : 'border-transparent'
            ]"
          >
            <div class="p-5">
              <div class="flex justify-between items-start mb-4">
                <div>
                  <h3 class="text-lg font-bold text-gray-900 line-clamp-1">{{ machine.name }}</h3>
                  <p class="text-xs text-gray-500 font-mono">{{ machine.serialNumber || 'No Serial' }}</p>
                </div>
                <span 
                  class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold text-white shadow-sm"
                  :class="getStatusColor(machine.status)"
                >
                  {{ getStatusDisplay(machine.status) }}
                </span>
              </div>
              
              <div class="space-y-3 text-sm">
                <div class="flex justify-between items-center text-gray-600">
                  <span class="flex items-center gap-2"><Filter class="w-3.5 h-3.5" /> Type</span>
                  <span class="font-semibold text-gray-900">{{ machine.type }}</span>
                </div>
                <div class="flex justify-between items-center text-gray-600 border-t border-gray-50 pt-2">
                  <span>Location</span>
                  <span class="font-medium text-gray-800">{{ machine.location }}</span>
                </div>
                <div class="flex justify-between items-center text-gray-600 mt-1">
                  <span>Next Maintenace</span>
                  <span 
                    class="font-medium"
                    :class="new Date(machine.nextMaintenance) < new Date() ? 'text-red-600 font-bold' : 'text-gray-800'"
                  >
                    {{ formatTime(machine.nextMaintenance) }}
                  </span>
                </div>
              </div>
              
              <div class="flex gap-3 mt-6 pt-4 border-t border-gray-100">
                <BaseButton
                  size="sm"
                  variant="outline"
                  class="flex-1"
                  @click="openEditModal(machine)"
                >
                  <Edit class="w-4 h-4 mr-2" />
                  Edit
                </BaseButton>
                <BaseButton
                  size="sm"
                  variant="outline"
                  class="text-red-600 border-red-100 hover:bg-red-50 flex-1"
                  @click="handleDelete(machine)"
                >
                  <Trash2 class="w-4 h-4 mr-2" />
                  Delete
                </BaseButton>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>
    </BaseCard>

    <!-- Create/Edit Modal -->
    <BaseModal
      v-model="isModalOpen"
      :title="selectedMachine ? 'Edit Machine' : 'Add New Machine'"
      size="lg"
    >
      <form id="machine-form" @submit.prevent="handleSubmit" class="space-y-6">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <BaseInput
            v-model="machineForm.name"
            label="Machine Name"
            placeholder="e.g. CNC Milling Machine A1"
            required
          />
          
          <BaseInput
            v-model="machineForm.serialNumber"
            label="Serial Number"
            placeholder="e.g. SN-9842-XJ"
          />
          
          <BaseInput
            v-model="machineForm.type"
            label="Equipment Type"
            placeholder="e.g. Lathe, Press, Motor"
            required
          />
          
          <BaseInput
            v-model="machineForm.manufacturer"
            label="Manufacturer"
            placeholder="e.g. Siemens, ABB"
          />
          
          <BaseInput
            v-model="machineForm.model"
            label="Model Number"
          />
          
          <BaseSelect
            v-model="machineForm.status"
            :options="statusOptions.filter(o => o.value !== 'all')"
            label="Current Status"
            required
          />
          
          <BaseInput
            v-model.number="machineForm.criticality"
            label="Criticality Level (1-5)"
            type="number"
            min="1"
            max="5"
            helper-text="1 = Lowest, 5 = Mission Critical"
          />
          
          <BaseInput
            v-model="machineForm.location"
            label="Installation Location"
            required
          />
        </div>
      </form>

      <template #footer>
        <BaseButton variant="ghost" @click="closeModals">
          Cancel
        </BaseButton>
        <BaseButton variant="primary" type="submit" form="machine-form">
          {{ selectedMachine ? 'Save Changes' : 'Register Machine' }}
        </BaseButton>
      </template>
    </BaseModal>
  </div>
</template>

<style scoped>
.machine-management {
  max-width: 1400px;
  margin: 0 auto;
}

.line-clamp-1 {
  display: -webkit-box;
  -webkit-line-clamp: 1;
  -webkit-box-orient: vertical;  
  overflow: hidden;
}
</style>