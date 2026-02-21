<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchActiveMaintenance,
  planMaintenance,
  startMaintenance,
  completeMaintenance,
  cancelMaintenance
} from '@/services/maintenance.service'
import type { 
  MaintenanceRecordDto, 
  PlanMaintenanceRequest, 
  CompleteMaintenanceRequest 
} from '@/api/types'
import { 
  Plus, 
  Edit, 
  Trash2, 
  Calendar,
  Wrench,
  Clock,
  CheckCircle,
  AlertTriangle,
  Filter,
  Search
} from 'lucide-vue-next'

const toast = useToast()

// State
const maintenanceRecords = ref<MaintenanceRecordDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const typeFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedRecord = ref<MaintenanceRecordDto | null>(null)

// Form state
const maintenanceForm = ref<PlanMaintenanceRequest>({
  machineId: '',
  type: 'preventive',
  plannedDate: '',
  notes: ''
})

// Computed
const filteredRecords = computed(() => {
  let filtered = [...maintenanceRecords.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(record => 
      record.notes?.toLowerCase().includes(query) ||
      record.performedBy?.toLowerCase().includes(query) ||
      record.machineId.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(record => record.status.toLowerCase() === statusFilter.value)
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter(record => record.type.toLowerCase() === typeFilter.value)
  }
  
  return filtered
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Planned', value: 'planned' },
  { label: 'In Progress', value: 'inprogress' },
  { label: 'Completed', value: 'completed' },
  { label: 'Cancelled', value: 'cancelled' }
]

const typeOptions = [
  { label: 'All Types', value: 'all' },
  { label: 'Preventive', value: 'preventive' },
  { label: 'Corrective', value: 'corrective' },
  { label: 'Predictive', value: 'predictive' }
]

// Methods
const loadMaintenanceRecords = async () => {
  try {
    loading.value = true
    const records = await fetchActiveMaintenance()
    maintenanceRecords.value = records as any
    toast.success('Maintenance records loaded successfully')
  } catch (error) {
    console.error('Error loading maintenance records:', error)
    toast.error('Failed to load maintenance records')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openEditModal = (record: MaintenanceRecordDto) => {
  selectedRecord.value = record
  maintenanceForm.value = {
    machineId: record.machineId,
    type: record.type,
    plannedDate: record.plannedDate || new Date().toISOString().split('T')[0],
    notes: record.notes || '',
    alertId: record.alertId
  }
  showEditModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  selectedRecord.value = null
  resetForm()
}

const resetForm = () => {
  maintenanceForm.value = {
    machineId: '',
    type: 'preventive',
    plannedDate: '',
    notes: ''
  }
}

const handleSubmit = async () => {
  try {
    const formData = {
      ...maintenanceForm.value,
      notes: maintenanceForm.value.notes || ''
    }
    
    if (selectedRecord.value) {
      // For editing, we'll treat it as planning a new maintenance
      await planMaintenance(formData)
      toast.success('Maintenance record updated successfully')
    } else {
      // Create new record
      await planMaintenance(formData)
      toast.success('Maintenance record created successfully')
    }
    
    closeModals()
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Error saving maintenance record:', error)
    toast.error('Failed to save maintenance record')
  }
}

const handleDelete = async (record: MaintenanceRecordDto) => {
  if (!confirm(`Are you sure you want to cancel maintenance record for machine "${record.machineId}"?`)) {
    return
  }
  
  try {
    await cancelMaintenance(record.id, 'User cancelled')
    toast.success('Maintenance record cancelled successfully')
    await loadMaintenanceRecords()
  } catch (error) {
    console.error('Error cancelling maintenance record:', error)
    toast.error('Failed to cancel maintenance record')
  }
}

const getStatusColor = (status: string) => {
  const lowerStatus = status.toLowerCase();
  switch (lowerStatus) {
    case 'planned': return 'text-blue-600 bg-blue-100'
    case 'inprogress': return 'text-yellow-600 bg-yellow-100'
    case 'completed': return 'text-green-600 bg-green-100'
    case 'cancelled': return 'text-red-600 bg-red-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const formatDate = (dateString?: string) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString()
}

onMounted(() => {
  loadMaintenanceRecords()
})
</script>

<template>
  <BaseCard class="maintenance-management">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Wrench class="header-icon" />
          Maintenance Management
        </h2>
        <p class="header-subtitle">Schedule, track, and manage maintenance activities</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Plus class="button-icon" />
        New Maintenance Record
      </BaseButton>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search maintenance records..."
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
        
        
        
        <BaseSelect
          v-model="typeFilter"
          :options="typeOptions"
          class="filter-select"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <BaseSkeleton v-for="i in 6" :key="i" height="120px" class="mb-4" />
    </div>

    <!-- Records List -->
    <div v-else class="records-container">
      <div class="stats-summary">
        <BaseCard class="stat-card">
          <div class="stat-content">
            <Calendar class="stat-icon text-blue-500" />
            <div>
              <div class="stat-value">{{ filteredRecords.filter(r => r.status.toLowerCase() === 'planned').length }}</div>
              <div class="stat-label">Scheduled</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <Clock class="stat-icon text-yellow-500" />
            <div>
              <div class="stat-value">{{ filteredRecords.filter(r => r.status.toLowerCase() === 'inprogress').length }}</div>
              <div class="stat-label">In Progress</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <CheckCircle class="stat-icon text-green-500" />
            <div>
              <div class="stat-value">{{ filteredRecords.filter(r => r.status === 'completed').length }}</div>
              <div class="stat-label">Completed</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <AlertTriangle class="stat-icon text-red-500" />
            <div>
              <div class="stat-value">{{ filteredRecords.filter(r => r.type.toLowerCase() === 'emergency').length }}</div>
              <div class="stat-label">Emergency</div>
            </div>
          </div>
        </BaseCard>
      </div>

      <div class="records-grid">
        <BaseCard
          v-for="record in filteredRecords"
          :key="record.id"
          class="record-card"
        >
          <div class="record-header">
            <div class="record-title">
              <h3>Machine: {{ record.machineId }}</h3>
              <span class="record-type">{{ record.type }}</span>
            </div>
            <span 
              class="status-badge"
              :class="getStatusColor(record.status)"
            >
              {{ record.status }}
            </span>
          </div>
          
          <div class="record-details">
            <p class="record-description">{{ record.notes || 'No notes provided' }}</p>
            
            <div class="record-meta">
              <div class="meta-item">
                <Calendar class="meta-icon" />
                <span>{{ formatDate(record.plannedDate) }}</span>
              </div>
              <div class="meta-item" v-if="record.performedBy">
                <span>Performed by: {{ record.performedBy }}</span>
              </div>
              <div class="meta-item" v-if="record.createdAt">
                <span>Created: {{ formatDate(record.createdAt) }}</span>
              </div>
            </div>
          </div>
          
          <div class="record-actions">
            <BaseButton
              variant="outline"
              size="sm"
              @click="openEditModal(record)"
            >
              <Edit class="action-icon" />
              Edit
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="handleDelete(record)"
              class="delete-button"
            >
              <Trash2 class="action-icon" />
              Delete
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
          <h3>{{ selectedRecord ? 'Edit Maintenance Record' : 'Create Maintenance Record' }}</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="maintenanceForm.machineId"
              label="Machine ID"
              required
            />
            
            <BaseSelect
              v-model="maintenanceForm.type"
              label="Type"
              :options="[
                { label: 'Preventive', value: 'preventive' },
                { label: 'Corrective', value: 'corrective' },
                { label: 'Emergency', value: 'emergency' }
              ]"
              required
            />
            
            <BaseInput
              v-model="maintenanceForm.plannedDate"
              label="Planned Date"
              type="date"
              required
            />
            
            <BaseInput
              v-model="maintenanceForm.notes"
              label="Notes"
              type="textarea"
              class="full-width"
            />
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ selectedRecord ? 'Update' : 'Create' }} Record
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.maintenance-management {
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

.stats-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  padding: 1rem;
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  width: 2rem;
  height: 2rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
}

.stat-label {
  font-size: 0.875rem;
  color: #64748b;
}

.records-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.record-card {
  padding: 1.5rem;
}

.record-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.record-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.record-type {
  font-size: 0.75rem;
  text-transform: uppercase;
  color: #64748b;
  background: #f1f5f9;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.record-description {
  color: #64748b;
  margin-bottom: 1rem;
  line-height: 1.5;
}

.record-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #64748b;
}

.meta-icon {
  width: 1rem;
  height: 1rem;
}

.record-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
}

.delete-button {
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
  max-width: 600px;
  max-height: 90vh;
  overflow-y: auto;
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

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .maintenance-management {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .records-grid {
    grid-template-columns: 1fr;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>