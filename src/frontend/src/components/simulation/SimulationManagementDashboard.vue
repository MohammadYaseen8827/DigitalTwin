<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import { 
  createSimulation, 
  runSimulationStep, 
  cancelSimulation 
} from '@/services/simulation.service'
import type { MachineDto, SimulationRequestDto } from '@/api/types'
import { 
  Play, 
  Pause, 
  Square, 
  Settings,
  BarChart3,
  Clock,
  Zap,
  Filter,
  Search,
  CheckCircle
} from 'lucide-vue-next'

const toast = useToast()

// State
const simulations = ref<any[]>([
  {
    id: 'SIM001',
    machineId: 'M001',
    machineName: 'CNC Machine #1',
    status: 'running',
    degradationModel: 'Linear',
    currentStep: 45,
    totalSteps: 100,
    startTime: new Date(Date.now() - 3600000).toISOString(),
    lastUpdateTime: new Date().toISOString(),
    progress: 45
  },
  {
    id: 'SIM002',
    machineId: 'M002',
    machineName: 'Injection Molder #2',
    status: 'paused',
    degradationModel: 'Exponential',
    currentStep: 23,
    totalSteps: 150,
    startTime: new Date(Date.now() - 7200000).toISOString(),
    lastUpdateTime: new Date(Date.now() - 3600000).toISOString(),
    progress: 15
  },
  {
    id: 'SIM003',
    machineId: 'M003',
    machineName: 'Conveyor System #1',
    status: 'completed',
    degradationModel: 'Weibull',
    currentStep: 200,
    totalSteps: 200,
    startTime: new Date(Date.now() - 86400000).toISOString(),
    endTime: new Date(Date.now() - 72000000).toISOString(),
    progress: 100
  }
])

const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showConfigureModal = ref(false)
const selectedSimulation = ref<any>(null)

// Form state
const simulationForm = ref<Omit<SimulationRequestDto, 'stepInterval'> & { stepInterval?: string }>({
  machineId: '',
  degradationModel: 'wiener',
  steps: 100,
  stepInterval: '1h',
  persistTelemetry: true
})

// Computed
const filteredSimulations = computed(() => {
  let filtered = [...simulations.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(sim => 
      sim.machineName.toLowerCase().includes(query) ||
      sim.degradationModel.toLowerCase().includes(query) ||
      sim.id.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(sim => sim.status === statusFilter.value)
  }
  
  return filtered
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Running', value: 'running' },
  { label: 'Paused', value: 'paused' },
  { label: 'Completed', value: 'completed' },
  { label: 'Cancelled', value: 'cancelled' }
]

const degradationModels = [
  { label: 'Linear', value: 'linear' },
  { label: 'Exponential', value: 'exponential' },
  { label: 'Weibull', value: 'weibull' },
  { label: 'Gamma', value: 'gamma' }
]

const stepIntervals = [
  { label: '1 minute', value: '1m' },
  { label: '5 minutes', value: '5m' },
  { label: '15 minutes', value: '15m' },
  { label: '1 hour', value: '1h' },
  { label: '6 hours', value: '6h' },
  { label: '1 day', value: '1d' }
]

// Methods
const loadSimulations = async () => {
  try {
    loading.value = true
    // In a real implementation, this would fetch from the simulation service
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Simulations loaded successfully')
  } catch (error) {
    console.error('Error loading simulations:', error)
    toast.error('Failed to load simulations')
  } finally {
    loading.value = false
  }
}

const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Error loading machines:', error)
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openConfigureModal = (simulation: any) => {
  selectedSimulation.value = simulation
  showConfigureModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showConfigureModal.value = false
  selectedSimulation.value = null
  resetForm()
}

const resetForm = () => {
  simulationForm.value = {
    machineId: '',
    degradationModel: 'wiener',
    steps: 100,
    stepInterval: '1h',
    persistTelemetry: true
  }
}

const handleSubmit = async () => {
  try {
    // Create new simulation
    await createSimulation(simulationForm.value.machineId, {
      degradationModel: { value: simulationForm.value.degradationModel },
      steps: { value: simulationForm.value.steps },
      persistTelemetry: { value: simulationForm.value.persistTelemetry }
    })
    toast.success('Simulation created successfully')
    closeModals()
    await loadSimulations()
  } catch (error) {
    console.error('Error creating simulation:', error)
    toast.error('Failed to create simulation')
  }
}

const startSimulation = async (simulationId: string, machineId: string) => {
  try {
    await runSimulationStep(simulationId, machineId)
    // Update local state
    const simIndex = simulations.value.findIndex(s => s.id === simulationId)
    if (simIndex !== -1) {
      simulations.value[simIndex].status = 'running'
      simulations.value[simIndex].currentStep += 1
      simulations.value[simIndex].progress = Math.round(
        (simulations.value[simIndex].currentStep / simulations.value[simIndex].totalSteps) * 100
      )
    }
    toast.success('Simulation started')
  } catch (error) {
    console.error('Error starting simulation:', error)
    toast.error('Failed to start simulation')
  }
}

const pauseSimulation = (simulationId: string) => {
  const simIndex = simulations.value.findIndex(s => s.id === simulationId)
  if (simIndex !== -1) {
    simulations.value[simIndex].status = 'paused'
    toast.success('Simulation paused')
  }
}

const stopSimulation = async (simulationId: string, machineId: string) => {
  try {
    await cancelSimulation(simulationId, machineId)
    const simIndex = simulations.value.findIndex(s => s.id === simulationId)
    if (simIndex !== -1) {
      simulations.value[simIndex].status = 'cancelled'
      simulations.value[simIndex].endTime = new Date().toISOString()
    }
    toast.success('Simulation stopped')
  } catch (error) {
    console.error('Error stopping simulation:', error)
    toast.error('Failed to stop simulation')
  }
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'running': return 'text-green-600 bg-green-100'
    case 'paused': return 'text-yellow-600 bg-yellow-100'
    case 'completed': return 'text-blue-600 bg-blue-100'
    case 'cancelled': return 'text-red-600 bg-red-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'running': return Play
    case 'paused': return Pause
    case 'completed': return CheckCircle
    case 'cancelled': return Square
    default: return Clock
  }
}

const formatDuration = (startTime: string, endTime?: string) => {
  const start = new Date(startTime)
  const end = endTime ? new Date(endTime) : new Date()
  const diffMs = end.getTime() - start.getTime()
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffMinutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))
  
  if (diffHours > 0) {
    return `${diffHours}h ${diffMinutes}m`
  }
  return `${diffMinutes}m`
}

const getProgressColor = (progress: number) => {
  if (progress < 30) return 'bg-red-500'
  if (progress < 70) return 'bg-yellow-500'
  return 'bg-green-500'
}

onMounted(() => {
  loadSimulations()
  loadMachines()
})
</script>

<template>
  <BaseCard class="simulation-management">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Settings class="header-icon" />
          Simulation Management
        </h2>
        <p class="header-subtitle">Create, configure, and monitor equipment degradation simulations</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Play class="button-icon" />
        New Simulation
      </BaseButton>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search simulations..."
          class="search-input"
        >
          <template #prefix>
            <Search class="input-icon" />
          </template>
        </BaseInput>
        
        <BaseSelect
          v-model="statusFilter"
          :options="statusOptions"
          class="filter-select"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <BaseSkeleton v-for="i in 6" :key="i" height="140px" class="mb-4" />
    </div>

    <!-- Simulations List -->
    <div v-else class="simulations-container">
      <div v-if="filteredSimulations.length === 0" class="empty-state">
        <Settings class="empty-icon" />
        <h3>No simulations found</h3>
        <p>Create your first simulation to get started</p>
        <BaseButton variant="primary" @click="openCreateModal">
          <Play class="button-icon" />
          Create Simulation
        </BaseButton>
      </div>
      
      <div v-else class="simulations-grid">
        <BaseCard
          v-for="simulation in filteredSimulations"
          :key="simulation.id"
          class="simulation-card"
        >
          <div class="simulation-header">
            <div class="simulation-title">
              <h3>{{ simulation.machineName }}</h3>
              <p class="simulation-id">ID: {{ simulation.id }}</p>
            </div>
            <span 
              class="status-badge"
              :class="getStatusColor(simulation.status)"
            >
              <component :is="getStatusIcon(simulation.status)" class="status-icon" />
              {{ simulation.status }}
            </span>
          </div>
          
          <div class="simulation-details">
            <div class="detail-row">
              <BarChart3 class="detail-icon" />
              <div>
                <span class="detail-label">Model:</span>
                <span class="detail-value">{{ simulation.degradationModel }}</span>
              </div>
            </div>
            
            <div class="detail-row">
              <Clock class="detail-icon" />
              <div>
                <span class="detail-label">Progress:</span>
                <span class="detail-value">{{ simulation.currentStep }}/{{ simulation.totalSteps }} steps</span>
              </div>
            </div>
            
            <div class="detail-row">
              <Zap class="detail-icon" />
              <div>
                <span class="detail-label">Duration:</span>
                <span class="detail-value">{{ formatDuration(simulation.startTime, simulation.endTime) }}</span>
              </div>
            </div>
          </div>
          
          <!-- Progress Bar -->
          <div class="progress-section">
            <div class="progress-label">
              <span>Progress</span>
              <span>{{ simulation.progress }}%</span>
            </div>
            <div class="progress-bar">
              <div 
                class="progress-fill"
                :class="getProgressColor(simulation.progress)"
                :style="{ width: simulation.progress + '%' }"
              ></div>
            </div>
          </div>
          
          <div class="simulation-actions">
            <template v-if="simulation.status === 'running'">
              <BaseButton
                variant="outline"
                size="sm"
                @click="pauseSimulation(simulation.id)"
              >
                <Pause class="action-icon" />
                Pause
              </BaseButton>
              <BaseButton
                variant="outline"
                size="sm"
                @click="stopSimulation(simulation.id, simulation.machineId)"
                class="stop-button"
              >
                <Square class="action-icon" />
                Stop
              </BaseButton>
            </template>
            
            <template v-else-if="simulation.status === 'paused'">
              <BaseButton
                variant="primary"
                size="sm"
                @click="startSimulation(simulation.id, simulation.machineId)"
              >
                <Play class="action-icon" />
                Resume
              </BaseButton>
              <BaseButton
                variant="outline"
                size="sm"
                @click="stopSimulation(simulation.id, simulation.machineId)"
                class="stop-button"
              >
                <Square class="action-icon" />
                Stop
              </BaseButton>
            </template>
            
            <template v-else>
              <BaseButton
                variant="outline"
                size="sm"
                @click="openConfigureModal(simulation)"
              >
                <Settings class="action-icon" />
                Configure
              </BaseButton>
            </template>
          </div>
        </BaseCard>
      </div>
    </div>

    <!-- Create Modal -->
    <div 
      v-if="showCreateModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Create New Simulation</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseSelect
              v-model="simulationForm.machineId"
              label="Machine"
              :options="machines.map(m => ({ label: `${m.name} (${m.id})`, value: m.id }))"
              required
            />
            
            <BaseSelect
              v-model="simulationForm.degradationModel"
              label="Degradation Model"
              :options="degradationModels"
              required
            />
            
            <BaseInput
              v-model.number="simulationForm.steps"
              label="Number of Steps"
              type="number"
              min="1"
              max="1000"
              required
            />
            
            <div class="checkbox-wrapper">
              <input 
                id="persistTelemetry" 
                v-model="simulationForm.persistTelemetry" 
                type="checkbox" 
                class="checkbox-input"
              />
              <label for="persistTelemetry" class="checkbox-label">Persist Telemetry Data</label>
            </div>
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              Create Simulation
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>

    <!-- Configure Modal -->
    <div 
      v-if="showConfigureModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Configure Simulation</h3>
        </template>
        
        <div class="configure-content">
          <div class="simulation-info">
            <h4>{{ selectedSimulation?.machineName }}</h4>
            <p>ID: {{ selectedSimulation?.id }}</p>
            <p>Status: {{ selectedSimulation?.status }}</p>
          </div>
          
          <div class="configuration-options">
            <BaseButton
              variant="outline"
              @click="startSimulation(selectedSimulation?.id, selectedSimulation?.machineId)"
              :disabled="selectedSimulation?.status === 'running'"
            >
              <Play class="button-icon" />
              Start Simulation
            </BaseButton>
            
            <BaseButton
              variant="outline"
              @click="pauseSimulation(selectedSimulation?.id)"
              :disabled="selectedSimulation?.status !== 'running'"
            >
              <Pause class="button-icon" />
              Pause Simulation
            </BaseButton>
            
            <BaseButton
              variant="outline"
              class="stop-button"
              @click="stopSimulation(selectedSimulation?.id, selectedSimulation?.machineId)"
            >
              <Square class="button-icon" />
              Stop Simulation
            </BaseButton>
          </div>
        </div>
        
        <div class="modal-actions">
          <BaseButton variant="ghost" @click="closeModals">
            Close
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.simulation-management {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

.header-content {
  flex: 1;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.header-icon {
  width: 1.5rem;
  height: 1.5rem;
  color: #3b82f6;
}

.header-subtitle {
  color: #64748b;
  font-size: 1rem;
}

.button-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
}

.filters-section {
  margin: 1.5rem 0;
}

.filter-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  align-items: center;
}

.search-input {
  flex: 1;
  min-width: 250px;
}

.input-icon {
  width: 1rem;
  height: 1rem;
  color: #94a3b8;
}

.filter-select {
  min-width: 150px;
}

.loading-container {
  padding: 2rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #64748b;
}

.empty-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.simulations-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.simulation-card {
  padding: 1.5rem;
}

.simulation-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.simulation-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.simulation-id {
  font-size: 0.75rem;
  color: #64748b;
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.status-icon {
  width: 0.75rem;
  height: 0.75rem;
}

.simulation-details {
  margin-bottom: 1rem;
}

.detail-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
  font-size: 0.875rem;
}

.detail-icon {
  width: 1rem;
  height: 1rem;
  color: #64748b;
  flex-shrink: 0;
}

.detail-label {
  color: #64748b;
  margin-right: 0.5rem;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
}

.progress-section {
  margin-bottom: 1rem;
}

.progress-label {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
  color: #64748b;
}

.progress-bar {
  height: 0.5rem;
  background: #e2e8f0;
  border-radius: 0.25rem;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.simulation-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
}

.stop-button {
  color: #dc2626 !important;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 50;
}

.modal-content {
  width: 100%;
  max-width: 500px;
}

.modal-form {
  padding: 1rem 0;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.checkbox-input {
  grid-column: 1 / -1;
}

.configure-content {
  padding: 1rem 0;
}

.simulation-info {
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 1rem;
  margin-bottom: 1.5rem;
}

.simulation-info h4 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.simulation-info p {
  color: #64748b;
  font-size: 0.875rem;
  margin-bottom: 0.25rem;
}

.configuration-options {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .simulation-management {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .simulations-grid {
    grid-template-columns: 1fr;
  }
  
  .simulation-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>