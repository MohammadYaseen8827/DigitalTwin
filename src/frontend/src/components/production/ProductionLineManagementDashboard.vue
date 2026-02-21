<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  Factory, 
  Play, 
  Pause, 
  Settings,
  Users,
  Activity,
  Filter,
  Search,
  Plus,
  Edit,
  Trash2
} from 'lucide-vue-next'

const toast = useToast()

// State
const productionLines = ref<any[]>([
  {
    id: 'PL001',
    name: 'Main Assembly Line',
    status: 'operational',
    machineCount: 8,
    currentOutput: 1250,
    targetOutput: 1500,
    efficiency: 83.3,
    machines: ['M001', 'M002', 'M003', 'M004', 'M005', 'M006', 'M007', 'M008'],
    configuration: {
      shiftHours: '24/7',
      operators: 12,
      supervisor: 'John Smith'
    },
    lastUpdated: new Date().toISOString()
  },
  {
    id: 'PL002',
    name: 'Quality Control Line',
    status: 'warning',
    machineCount: 5,
    currentOutput: 850,
    targetOutput: 1000,
    efficiency: 85,
    machines: ['M009', 'M010', 'M011', 'M012', 'M013'],
    configuration: {
      shiftHours: '8AM-4PM',
      operators: 6,
      supervisor: 'Sarah Johnson'
    },
    lastUpdated: new Date(Date.now() - 3600000).toISOString()
  },
  {
    id: 'PL003',
    name: 'Packaging Line',
    status: 'maintenance',
    machineCount: 3,
    currentOutput: 0,
    targetOutput: 800,
    efficiency: 0,
    machines: ['M014', 'M015', 'M016'],
    configuration: {
      shiftHours: 'Maintenance',
      operators: 0,
      supervisor: 'Mike Wilson'
    },
    lastUpdated: new Date(Date.now() - 7200000).toISOString()
  }
])

const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showAssignModal = ref(false)
const selectedLine = ref<any>(null)

// Form state
const lineForm = ref<{
  name: string;
  configuration: Record<string, any>;
  machineIds: string[];
}>({
  name: '',
  configuration: {},
  machineIds: []
})

const assignForm = ref({
  machineId: '',
  lineId: ''
})

// Computed
const filteredLines = computed(() => {
  let filtered = [...productionLines.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(line => 
      line.name.toLowerCase().includes(query) ||
      line.id.toLowerCase().includes(query) ||
      line.configuration.supervisor?.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(line => line.status === statusFilter.value)
  }
  
  return filtered
})

const availableMachines = computed(() => {
  // Get machines not assigned to any production line
  const assignedMachineIds = productionLines.value.flatMap(line => line.machines || [])
  return machines.value.filter(machine => !assignedMachineIds.includes(machine.id))
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

// Methods
const loadProductionLines = async () => {
  try {
    loading.value = true
    // In a real implementation, this would fetch from the production lines service
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Production lines loaded successfully')
  } catch (error) {
    console.error('Error loading production lines:', error)
    toast.error('Failed to load production lines')
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

const openEditModal = (line: any) => {
  selectedLine.value = line
  lineForm.value = {
    name: line.name,
    configuration: line.configuration,
    machineIds: line.machines || []
  }
  showEditModal.value = true
}

const openAssignModal = (line: any) => {
  selectedLine.value = line
  assignForm.value.lineId = line.id
  assignForm.value.machineId = ''
  showAssignModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showAssignModal.value = false
  selectedLine.value = null
  resetForm()
}

const resetForm = () => {
  lineForm.value = {
    name: '',
    configuration: {},
    machineIds: []
  }
  assignForm.value = {
    machineId: '',
    lineId: ''
  }
}

const handleSubmit = async () => {
  try {
    // Create/update production line
    toast.success(selectedLine.value ? 'Production line updated successfully' : 'Production line created successfully')
    closeModals()
    await loadProductionLines()
  } catch (error) {
    console.error('Error saving production line:', error)
    toast.error('Failed to save production line')
  }
}

const handleAssignMachine = async () => {
  try {
    if (!selectedLine.value || !assignForm.value.machineId) return
    
    // Add machine to production line
    const lineIndex = productionLines.value.findIndex(l => l.id === selectedLine.value.id)
    if (lineIndex !== -1) {
      if (!productionLines.value[lineIndex].machines) {
        productionLines.value[lineIndex].machines = []
      }
      productionLines.value[lineIndex].machines.push(assignForm.value.machineId)
      productionLines.value[lineIndex].machineCount += 1
    }
    
    toast.success('Machine assigned successfully')
    closeModals()
  } catch (error) {
    console.error('Error assigning machine:', error)
    toast.error('Failed to assign machine')
  }
}

const removeMachine = async (lineId: string, machineId: string) => {
  if (!confirm('Are you sure you want to remove this machine from the production line?')) {
    return
  }
  
  try {
    const lineIndex = productionLines.value.findIndex(l => l.id === lineId)
    if (lineIndex !== -1) {
      productionLines.value[lineIndex].machines = 
        productionLines.value[lineIndex].machines.filter((id: string) => id !== machineId)
      productionLines.value[lineIndex].machineCount -= 1
    }
    
    toast.success('Machine removed successfully')
  } catch (error) {
    console.error('Error removing machine:', error)
    toast.error('Failed to remove machine')
  }
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'operational': return 'text-green-600 bg-green-100'
    case 'warning': return 'text-yellow-600 bg-yellow-100'
    case 'maintenance': return 'text-blue-600 bg-blue-100'
    case 'offline': return 'text-red-600 bg-red-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const getEfficiencyColor = (efficiency: number) => {
  if (efficiency >= 90) return 'text-green-600'
  if (efficiency >= 75) return 'text-yellow-600'
  return 'text-red-600'
}

const formatEfficiency = (efficiency: number) => {
  return efficiency.toFixed(1) + '%'
}

onMounted(() => {
  loadProductionLines()
  loadMachines()
})
</script>

<template>
  <BaseCard class="production-line-management">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Factory class="header-icon" />
          Production Line Management
        </h2>
        <p class="header-subtitle">Manage factory production lines and equipment assignments</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Plus class="button-icon" />
        New Production Line
      </BaseButton>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search production lines..."
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
      <BaseSkeleton v-for="i in 6" :key="i" height="160px" class="mb-4" />
    </div>

    <!-- Production Lines List -->
    <div v-else class="lines-container">
      <div v-if="filteredLines.length === 0" class="empty-state">
        <Factory class="empty-icon" />
        <h3>No production lines found</h3>
        <p>Create your first production line to get started</p>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="button-icon" />
          Create Production Line
        </BaseButton>
      </div>
      
      <div v-else class="lines-grid">
        <BaseCard
          v-for="line in filteredLines"
          :key="line.id"
          class="line-card"
        >
          <div class="line-header">
            <div class="line-title">
              <h3>{{ line.name }}</h3>
              <p class="line-id">ID: {{ line.id }}</p>
            </div>
            <span 
              class="status-badge"
              :class="getStatusColor(line.status)"
            >
              {{ line.status }}
            </span>
          </div>
          
          <div class="line-metrics">
            <div class="metric-card">
              <Users class="metric-icon text-blue-500" />
              <div>
                <div class="metric-value">{{ line.machineCount }}</div>
                <div class="metric-label">Machines</div>
              </div>
            </div>
            
            <div class="metric-card">
              <Activity class="metric-icon text-green-500" />
              <div>
                <div class="metric-value" :class="getEfficiencyColor(line.efficiency)">
                  {{ formatEfficiency(line.efficiency) }}
                </div>
                <div class="metric-label">Efficiency</div>
              </div>
            </div>
            
            <div class="metric-card">
              <span class="metric-icon">📊</span>
              <div>
                <div class="metric-value">{{ line.currentOutput }}/{{ line.targetOutput }}</div>
                <div class="metric-label">Output</div>
              </div>
            </div>
          </div>
          
          <div class="line-details">
            <div class="detail-row">
              <span class="detail-label">Supervisor:</span>
              <span class="detail-value">{{ line.configuration.supervisor }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Shift Hours:</span>
              <span class="detail-value">{{ line.configuration.shiftHours }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Operators:</span>
              <span class="detail-value">{{ line.configuration.operators }}</span>
            </div>
          </div>
          
          <!-- Assigned Machines -->
          <div class="machines-section">
            <div class="section-header">
              <h4>Assigned Machines</h4>
              <BaseButton
                variant="outline"
                size="sm"
                @click="openAssignModal(line)"
              >
                <Plus class="button-icon" />
                Assign Machine
              </BaseButton>
            </div>
            
            <div class="machines-list">
              <div 
                v-for="machineId in line.machines" 
                :key="machineId"
                class="machine-tag"
              >
                <span>{{ machineId }}</span>
                <button 
                  @click="removeMachine(line.id, machineId)"
                  class="remove-machine"
                >
                  ×
                </button>
              </div>
              <div v-if="line.machines.length === 0" class="no-machines">
                No machines assigned
              </div>
            </div>
          </div>
          
          <div class="line-actions">
            <BaseButton
              variant="outline"
              size="sm"
              @click="openEditModal(line)"
            >
              <Edit class="action-icon" />
              Edit
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="openAssignModal(line)"
            >
              <Settings class="action-icon" />
              Manage
            </BaseButton>
          </div>
        </BaseCard>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div 
      v-if="showCreateModal || showEditModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>{{ selectedLine ? 'Edit Production Line' : 'Create Production Line' }}</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="lineForm.name"
              label="Line Name"
              required
            />
            
            <BaseInput
              v-model="lineForm.configuration.supervisor"
              label="Supervisor"
            />
            
            <BaseInput
              v-model="lineForm.configuration.shiftHours"
              label="Shift Hours"
            />
            
            <BaseInput
              v-model.number="lineForm.configuration.operators"
              label="Number of Operators"
              type="number"
            />
            
            <BaseSelect
              v-model="lineForm.machineIds"
              label="Assigned Machines"
              :options="availableMachines.map(m => ({ label: `${m.name} (${m.id})`, value: m.id }))"
              multiple
              class="full-width"
            />
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ selectedLine ? 'Update' : 'Create' }} Line
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>

    <!-- Assign Machine Modal -->
    <div 
      v-if="showAssignModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Assign Machine to {{ selectedLine?.name }}</h3>
        </template>
        
        <div class="assign-content">
          <BaseSelect
            v-model="assignForm.machineId"
            label="Select Machine"
            :options="availableMachines.map(m => ({ label: `${m.name} (${m.id})`, value: m.id }))"
          />
        </div>
        
        <div class="modal-actions">
          <BaseButton variant="ghost" @click="closeModals">
            Cancel
          </BaseButton>
          <BaseButton 
            variant="primary" 
            @click="handleAssignMachine"
            :disabled="!assignForm.machineId"
          >
            Assign Machine
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.production-line-management {
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

.lines-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1rem;
}

.line-card {
  padding: 1.5rem;
}

.line-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.line-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.line-id {
  font-size: 0.75rem;
  color: #64748b;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.line-metrics {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 1rem;
}

.metric-card {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: #f8fafc;
  border-radius: 0.5rem;
}

.metric-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.metric-value {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
}

.metric-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
}

.line-details {
  margin-bottom: 1rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 0.25rem 0;
  font-size: 0.875rem;
}

.detail-label {
  color: #64748b;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
}

.machines-section {
  margin-bottom: 1rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.75rem;
}

.section-header h4 {
  font-size: 0.875rem;
  font-weight: 600;
  color: #334155;
  text-transform: uppercase;
}

.machines-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.machine-tag {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  background: #e2e8f0;
  border-radius: 0.25rem;
  font-size: 0.75rem;
}

.remove-machine {
  background: none;
  border: none;
  color: #64748b;
  cursor: pointer;
  font-size: 1rem;
  font-weight: bold;
  padding: 0;
  width: 1rem;
  height: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

.remove-machine:hover {
  color: #dc2626;
}

.no-machines {
  color: #94a3b8;
  font-size: 0.875rem;
  font-style: italic;
}

.line-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
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
  max-width: 600px;
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

.full-width {
  grid-column: 1 / -1;
}

.assign-content {
  padding: 1rem 0;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .production-line-management {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .lines-grid {
    grid-template-columns: 1fr;
  }
  
  .line-metrics {
    grid-template-columns: 1fr;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>