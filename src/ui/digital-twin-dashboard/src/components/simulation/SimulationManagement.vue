<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  createSimulation,
  getSimulation,
  runSimulationStep,
  pauseSimulation,
  resumeSimulation,
  cancelSimulation,
  getAllSimulationStatuses
} from '@/services/simulation.service'
import { 
  Play, 
  Pause, 
  Square, 
  RotateCcw,
  Activity,
  Settings,
  Filter,
  Search,
  Plus,
  RefreshCw,
  Clock,
  CheckCircle,
  XCircle
} from 'lucide-vue-next'

const toast = useToast()

// State
const simulations: any = ref([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const machineFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showDetailsModal = ref(false)
const selectedSimulation: any = ref(null)

// Form state
const createForm: any = ref({
  machineId: '',
  degradationModel: 'linear',
  steps: 100,
  stepInterval: '00:01:00',
  persistTelemetry: true
})

// Computed
const filteredSimulations = computed(() => {
  let filtered = [...simulations.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((sim: any) => 
      sim.machineId?.toLowerCase().includes(query) ||
      sim.status?.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter((sim: any) => sim.status === statusFilter.value)
  }
  
  // Apply machine filter
  if (machineFilter.value !== 'all') {
    filtered = filtered.filter((sim: any) => sim.machineId === machineFilter.value)
  }
  
  return filtered
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Idle', value: 'idle' },
  { label: 'Running', value: 'running' },
  { label: 'Paused', value: 'paused' },
  { label: 'Completed', value: 'completed' },
  { label: 'Failed', value: 'failed' },
  { label: 'Cancelled', value: 'cancelled' }
]

const machineOptions = computed(() => {
  const machines = [...new Set(simulations.value.map((s: any) => s.machineId))]
  return [
    { label: 'All Machines', value: 'all' },
    ...machines.map((id: unknown) => ({ 
      label: String(id),
      value: String(id)
    }))
  ]
})

const degradationModels = [
  { label: 'Linear', value: 'linear' },
  { label: 'Exponential', value: 'exponential' },
  { label: 'Wiener Process', value: 'wiener' },
  { label: 'Markov Chain', value: 'markov' },
  { label: 'Physics-Based', value: 'physics_based' }
]

// Methods
const loadSimulations = async () => {
  try {
    loading.value = true
    
    // Get all simulation statuses
    const statuses = await getAllSimulationStatuses()
    
    // Convert object to array format
    simulations.value = Object.entries(statuses).map(([machineId, status]: [string, any]) => ({
      id: `sim_${machineId}`,
      machineId,
      ...status,
      currentStep: status.currentStep || 0,
      totalSteps: status.totalSteps || 100
    }))
  } catch (error) {
    console.error('Failed to load simulations:', error)
    toast.error('Unable to load simulations')
  } finally {
    loading.value = false
  }
}

const refreshSimulations = async () => {
  await loadSimulations()
  toast.success('Simulations refreshed')
}

const openCreateModal = () => {
  resetCreateForm()
  showCreateModal.value = true
}

const openDetailsModal = async (simulation: any) => {
  try {
    // Get detailed simulation info
    const details = await getSimulation(simulation.id.replace('sim_', ''), simulation.machineId)
    selectedSimulation.value = { ...simulation, ...details }
    showDetailsModal.value = true
  } catch (error) {
    console.error('Failed to load simulation details:', error)
    toast.error('Failed to load simulation details')
    selectedSimulation.value = simulation
    showDetailsModal.value = true
  }
}

const closeModals = () => {
  showCreateModal.value = false
  showDetailsModal.value = false
  selectedSimulation.value = null
  resetCreateForm()
}

const resetCreateForm = () => {
  createForm.value = {
    machineId: '',
    degradationModel: 'linear',
    steps: 100,
    stepInterval: '00:01:00',
    persistTelemetry: true
  }
}

const handleCreateSimulation = async () => {
  try {
    const params = {
      degradationModel: createForm.value.degradationModel,
      steps: createForm.value.steps,
      stepInterval: createForm.value.stepInterval,
      persistTelemetry: createForm.value.persistTelemetry
    }
    
    await createSimulation(createForm.value.machineId, params)
    toast.success('Simulation created successfully')
    closeModals()
    await loadSimulations()
  } catch (error) {
    console.error('Failed to create simulation:', error)
    toast.error('Failed to create simulation')
  }
}

const handleRunSimulation = async (simulation: any) => {
  try {
    await runSimulationStep(simulation.id.replace('sim_', ''), simulation.machineId)
    toast.success('Simulation step executed')
    await loadSimulations()
  } catch (error) {
    console.error('Failed to run simulation:', error)
    toast.error('Failed to run simulation')
  }
}

const handlePauseSimulation = async (simulation: any) => {
  try {
    await pauseSimulation(simulation.id.replace('sim_', ''), simulation.machineId)
    toast.success('Simulation paused')
    await loadSimulations()
  } catch (error) {
    console.error('Failed to pause simulation:', error)
    toast.error('Failed to pause simulation')
  }
}

const handleResumeSimulation = async (simulation: any) => {
  try {
    await resumeSimulation(simulation.id.replace('sim_', ''), simulation.machineId)
    toast.success('Simulation resumed')
    await loadSimulations()
  } catch (error) {
    console.error('Failed to resume simulation:', error)
    toast.error('Failed to resume simulation')
  }
}

const handleCancelSimulation = async (simulation: any) => {
  if (!confirm('Are you sure you want to cancel this simulation?')) {
    return
  }
  
  try {
    await cancelSimulation(simulation.id.replace('sim_', ''), simulation.machineId)
    toast.success('Simulation cancelled')
    await loadSimulations()
  } catch (error) {
    console.error('Failed to cancel simulation:', error)
    toast.error('Failed to cancel simulation')
  }
}

const getStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    'idle': 'bg-gray-500',
    'running': 'bg-green-500',
    'paused': 'bg-yellow-500',
    'completed': 'bg-blue-500',
    'failed': 'bg-red-500',
    'cancelled': 'bg-gray-400'
  }
  return colors[status?.toLowerCase()] || 'bg-gray-500'
}

const getStatusDisplay = (status: string): string => {
  const displays: Record<string, string> = {
    'idle': 'Idle',
    'running': 'Running',
    'paused': 'Paused',
    'completed': 'Completed',
    'failed': 'Failed',
    'cancelled': 'Cancelled'
  }
  return displays[status?.toLowerCase()] || status
}

const getProgressPercentage = (current: number, total: number): number => {
  if (!total) return 0
  return Math.min(100, Math.max(0, (current / total) * 100))
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleString()
}

const formatDuration = (seconds?: number): string => {
  if (!seconds) return '0s'
  
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const secs = seconds % 60
  
  if (hours > 0) return `${hours}h ${minutes}m`
  if (minutes > 0) return `${minutes}m ${secs}s`
  return `${secs}s`
}

// Lifecycle
onMounted(async () => {
  await loadSimulations()
})
</script>

<template>
  <BaseCard>
    <div class="simulation-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Simulation Management</h1>
          <p class="text-gray-600 mt-2">Orchestrate and monitor equipment simulations</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="refreshSimulations">
            <RefreshCw class="w-4 h-4 mr-2" />
            Refresh
          </BaseButton>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            New Simulation
          </BaseButton>
        </div>
      </div>

      <!-- Simulation Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Simulations</p>
                <p class="text-2xl font-bold">
                  {{ simulations.filter((s: any) => s.status === 'running').length }}
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
                <p class="text-sm text-gray-600">Idle</p>
                <p class="text-2xl font-bold">
                  {{ simulations.filter((s: any) => s.status === 'idle').length }}
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
                <p class="text-sm text-gray-600">Completed</p>
                <p class="text-2xl font-bold">
                  {{ simulations.filter((s: any) => s.status === 'completed').length }}
                </p>
              </div>
              <CheckCircle class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Machines</p>
                <p class="text-2xl font-bold">{{ simulations.length }}</p>
              </div>
              <Settings class="w-8 h-8 text-purple-500" />
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
                  placeholder="Search simulations..."
                  class="pl-10"
                />
              </div>
            </div>
            
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

      <!-- Simulations List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Simulations ({{ filteredSimulations.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading simulations...</p>
          </div>
          
          <div v-else-if="filteredSimulations.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No simulations found</p>
            <p class="text-sm text-gray-500 mt-1">Try adjusting your search or filters</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="simulation in filteredSimulations"
              :key="simulation.id"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex flex-col sm:flex-row justify-between gap-4">
                  <div class="flex-1">
                    <div class="flex items-start justify-between mb-3">
                      <div>
                        <h3 class="text-lg font-semibold text-gray-900">
                          Machine: {{ simulation.machineId }}
                        </h3>
                        <p class="text-gray-600">
                          Status: {{ getStatusDisplay(simulation.status) }}
                        </p>
                      </div>
                      <span 
                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium text-white"
                        :class="getStatusColor(simulation.status)"
                      >
                        {{ getStatusDisplay(simulation.status) }}
                      </span>
                    </div>
                    
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600 mb-3">
                      <div>
                        <span class="font-medium">Progress:</span>
                        <p>{{ simulation.currentStep }} / {{ simulation.totalSteps }} steps</p>
                      </div>
                      <div>
                        <span class="font-medium">Last Updated:</span>
                        <p>{{ formatTime(simulation.updatedAt) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Created:</span>
                        <p>{{ formatTime(simulation.createdAt) }}</p>
                      </div>
                    </div>
                    
                    <!-- Progress Bar -->
                    <div class="w-full bg-gray-200 rounded-full h-2 mb-2">
                      <div 
                        class="bg-blue-600 h-2 rounded-full transition-all duration-300"
                        :style="{ width: getProgressPercentage(simulation.currentStep, simulation.totalSteps) + '%' }"
                      ></div>
                    </div>
                    <p class="text-xs text-gray-500">
                      {{ getProgressPercentage(simulation.currentStep, simulation.totalSteps).toFixed(1) }}% complete
                    </p>
                  </div>
                  
                  <div class="flex flex-col gap-2">
                    <template v-if="simulation.status === 'idle'">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="handleRunSimulation(simulation)"
                      >
                        <Play class="w-4 h-4 mr-1" />
                        Start
                      </BaseButton>
                    </template>
                    
                    <template v-else-if="simulation.status === 'running'">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="handlePauseSimulation(simulation)"
                      >
                        <Pause class="w-4 h-4 mr-1" />
                        Pause
                      </BaseButton>
                      <BaseButton
                        size="sm"
                        variant="outline"
                        @click="handleCancelSimulation(simulation)"
                        class="text-red-600 hover:text-red-700"
                      >
                        <Square class="w-4 h-4 mr-1" />
                        Cancel
                      </BaseButton>
                    </template>
                    
                    <template v-else-if="simulation.status === 'paused'">
                      <BaseButton
                        size="sm"
                        variant="primary"
                        @click="handleResumeSimulation(simulation)"
                      >
                        <Play class="w-4 h-4 mr-1" />
                        Resume
                      </BaseButton>
                      <BaseButton
                        size="sm"
                        variant="outline"
                        @click="handleCancelSimulation(simulation)"
                        class="text-red-600 hover:text-red-700"
                      >
                        <Square class="w-4 h-4 mr-1" />
                        Cancel
                      </BaseButton>
                    </template>
                    
                    <BaseButton
                      size="sm"
                      variant="outline"
                      @click="openDetailsModal(simulation)"
                    >
                      <Settings class="w-4 h-4 mr-1" />
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

  <!-- Create Simulation Modal -->
  <div 
    v-if="showCreateModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-md" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Create New Simulation</h2>
        
        <form @submit.prevent="handleCreateSimulation" class="space-y-4">
          <BaseInput
            v-model="createForm.machineId"
            label="Machine ID"
            required
          />
          
          <BaseSelect
            v-model="createForm.degradationModel"
            :options="degradationModels"
            label="Degradation Model"
            required
          />
          
          <BaseInput
            v-model.number="createForm.steps"
            label="Number of Steps"
            type="number"
            min="1"
            max="1000"
            required
          />
          
          <BaseInput
            v-model="createForm.stepInterval"
            label="Step Interval (HH:MM:SS)"
            placeholder="00:01:00"
            required
          />
          
          <div class="flex items-center">
            <input
              id="persist-telemetry"
              v-model="createForm.persistTelemetry"
              type="checkbox"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label for="persist-telemetry" class="ml-2 block text-sm text-gray-900">
              Persist telemetry data
            </label>
          </div>
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              Create Simulation
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>

  <!-- Simulation Details Modal -->
  <div 
    v-if="showDetailsModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-lg" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Simulation Details</h2>
        
        <div v-if="selectedSimulation" class="space-y-4">
          <div class="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span class="font-medium">Machine ID:</span>
              <p>{{ selectedSimulation.machineId }}</p>
            </div>
            <div>
              <span class="font-medium">Status:</span>
              <p>{{ getStatusDisplay(selectedSimulation.status) }}</p>
            </div>
            <div>
              <span class="font-medium">Current Step:</span>
              <p>{{ selectedSimulation.currentStep }}</p>
            </div>
            <div>
              <span class="font-medium">Total Steps:</span>
              <p>{{ selectedSimulation.totalSteps }}</p>
            </div>
            <div>
              <span class="font-medium">Created:</span>
              <p>{{ formatTime(selectedSimulation.createdAt) }}</p>
            </div>
            <div>
              <span class="font-medium">Last Updated:</span>
              <p>{{ formatTime(selectedSimulation.updatedAt) }}</p>
            </div>
            <div v-if="selectedSimulation.completedAt">
              <span class="font-medium">Completed:</span>
              <p>{{ formatTime(selectedSimulation.completedAt) }}</p>
            </div>
          </div>
          
          <div class="pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Close
            </BaseButton>
          </div>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.simulation-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .simulation-management {
    padding: 0.5rem;
  }
}
</style>