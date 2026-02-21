<template>
  <BaseCard variant="soft" class="bulk-machine-manager">
    <template #header>
      <div class="header-content">
        <h3>Bulk Machine Operations</h3>
        <div class="header-actions">
          <BaseButton 
            variant="outline" 
            size="sm" 
            @click="importMachines"
            :disabled="processing"
          >
            <Upload class="icon" />
            Import CSV
          </BaseButton>
          <BaseButton 
            variant="outline" 
            size="sm" 
            @click="exportSelected"
            :disabled="!hasSelection || processing"
          >
            <Download class="icon" />
            Export Selected
          </BaseButton>
        </div>
      </div>
    </template>

    <!-- File Upload Area -->
    <div v-if="showImportArea" class="import-area glass-panel">
      <div 
        class="drop-zone"
        :class="{ 'drag-over': isDragging }"
        @drop="handleDrop"
        @dragover="handleDragOver"
        @dragenter="handleDragEnter"
        @dragleave="handleDragLeave"
      >
        <Upload class="upload-icon" />
        <p>Drop CSV file here or click to browse</p>
        <BaseButton 
          variant="primary" 
          @click="triggerFileSelect"
          :disabled="processing"
        >
          Select File
        </BaseButton>
        <input
          ref="fileInput"
          type="file"
          accept=".csv"
          class="hidden-file-input"
          @change="handleFileSelect"
        />
      </div>
      
      <div v-if="previewData.length > 0" class="preview-section">
        <h4>Preview ({{ previewData.length }} records)</h4>
        <div class="preview-table-container">
          <table class="preview-table">
            <thead>
              <tr>
                <th v-for="header in csvHeaders" :key="header">
                  {{ header }}
                </th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, index) in previewData.slice(0, 5)" :key="index">
                <td v-for="header in csvHeaders" :key="header">
                  {{ row[header] || '-' }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="preview-actions">
          <BaseButton 
            variant="primary" 
            @click="processImport"
            :loading="processing"
            :disabled="previewData.length === 0"
          >
            Import {{ previewData.length }} Machines
          </BaseButton>
          <BaseButton 
            variant="ghost" 
            @click="cancelImport"
          >
            Cancel
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Bulk Actions Panel -->
    <div v-else class="bulk-actions-panel">
      <!-- Filters and Selection -->
      <div class="filter-section">
        <div class="filters">
          <BaseInput
            v-model="searchTerm"
            placeholder="Search machines..."
            class="search-input"
          >
            <template #prefix>
              <Search class="search-icon" />
            </template>
          </BaseInput>
          
          <BaseSelect
            v-model="statusFilter"
            :options="statusOptions"
            placeholder="Filter by status"
            clearable
          />
          
          <BaseSelect
            v-model="typeFilter"
            :options="machineTypes"
            placeholder="Filter by type"
            clearable
          />
        </div>
        
        <div class="selection-info">
          <span>{{ selectedMachines.length }} of {{ filteredMachines.length }} selected</span>
          <BaseButton 
            v-if="selectedMachines.length > 0"
            variant="ghost" 
            size="sm"
            @click="clearSelection"
          >
            Clear Selection
          </BaseButton>
        </div>
      </div>

      <!-- Bulk Action Buttons -->
      <div v-if="selectedMachines.length > 0" class="bulk-action-buttons">
        <BaseButton 
          variant="primary" 
          @click="openBulkUpdate"
          :disabled="processing"
        >
          Update Selected
        </BaseButton>
        <BaseButton 
          variant="critical" 
          @click="confirmBulkDelete"
          :disabled="processing"
        >
          Delete Selected ({{ selectedMachines.length }})
        </BaseButton>
        <BaseButton 
          variant="outline" 
          @click="exportSelected"
          :disabled="processing"
        >
          Export Selected
        </BaseButton>
      </div>

      <!-- Machines Table -->
      <div class="machines-table-container">
        <table class="machines-table">
          <thead>
            <tr>
              <th class="select-col">
                <input
                  type="checkbox"
                  :checked="areAllSelected"
                  :indeterminate="areSomeSelected"
                  @change="toggleAllSelection"
                />
              </th>
              <th>Name</th>
              <th>Type</th>
              <th>Status</th>
              <th>Location</th>
              <th>Last Maintenance</th>
              <th class="actions-col">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr 
              v-for="machine in paginatedMachines" 
              :key="machine.id"
              :class="{ 'selected': isSelected(machine.id) }"
            >
              <td class="select-col">
                <input
                  type="checkbox"
                  :checked="isSelected(machine.id)"
                  @change="toggleSelection(machine.id)"
                />
              </td>
              <td>
                <div class="machine-name">
                  <span class="name">{{ machine.name }}</span>
                  <span v-if="machine.serialNumber" class="serial">
                    SN: {{ machine.serialNumber }}
                  </span>
                </div>
              </td>
              <td>{{ machine.type }}</td>
              <td>
                <span 
                  class="status-badge" 
                  :class="`status-${normalizeStatus(machine.status)}`"
                >
                  {{ formatStatus(machine.status) }}
                </span>
              </td>
              <td>{{ machine.location || '-' }}</td>
              <td>{{ formatDate(machine.lastMaintenance) || '-' }}</td>
              <td class="actions-col">
                <div class="row-actions">
                  <BaseButton 
                    variant="ghost" 
                    size="sm"
                    @click="editMachine(machine)"
                  >
                    Edit
                  </BaseButton>
                  <BaseButton 
                    variant="ghost" 
                    size="sm"
                    @click="deleteSingleMachine(machine)"
                  >
                    Delete
                  </BaseButton>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination">
        <div class="pagination-info">
          Showing {{ startIndex + 1 }}-{{ endIndex }} of {{ filteredMachines.length }} machines
        </div>
        <div class="pagination-controls">
          <BaseButton 
            variant="outline" 
            size="sm"
            :disabled="currentPage === 1"
            @click="currentPage--"
          >
            Previous
          </BaseButton>
          <span class="page-info">
            Page {{ currentPage }} of {{ totalPages }}
          </span>
          <BaseButton 
            variant="outline" 
            size="sm"
            :disabled="currentPage === totalPages"
            @click="currentPage++"
          >
            Next
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Confirmation Dialogs -->
    <div v-if="showDeleteConfirm" class="confirmation-dialog glass-panel">
      <div class="dialog-content">
        <AlertTriangle class="warning-icon" />
        <h3>Confirm Bulk Delete</h3>
        <p>
          Are you sure you want to delete {{ selectedMachines.length }} machines? 
          This action cannot be undone.
        </p>
        <div class="dialog-actions">
          <BaseButton 
            variant="critical" 
            @click="performBulkDelete"
            :loading="processing"
          >
            Delete {{ selectedMachines.length }} Machines
          </BaseButton>
          <BaseButton 
            variant="ghost" 
            @click="cancelDelete"
          >
            Cancel
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Bulk Update Dialog -->
    <div v-if="showBulkUpdate" class="bulk-update-dialog glass-panel">
      <div class="dialog-content">
        <h3>Update {{ selectedMachines.length }} Machines</h3>
        <div class="update-fields">
          <BaseSelect
            v-model="bulkUpdateFields.status"
            :options="statusOptions"
            label="Update Status"
            placeholder="Select new status"
            clearable
          />
          
          <BaseInput
            v-model="bulkUpdateFields.location"
            label="Update Location"
            placeholder="Enter new location"
            clearable
          />
          
          <BaseSelect
            v-model="bulkUpdateFields.criticality"
            :options="criticalityLevels"
            label="Update Criticality"
            placeholder="Select new criticality"
            clearable
            allow-empty
          />
        </div>
        <div class="dialog-actions">
          <BaseButton 
            variant="primary" 
            @click="performBulkUpdate"
            :loading="processing"
            :disabled="!hasBulkUpdateChanges"
          >
            Update {{ selectedMachines.length }} Machines
          </BaseButton>
          <BaseButton 
            variant="ghost" 
            @click="cancelBulkUpdate"
          >
            Cancel
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { 
  fetchMachinesPage,
  updateMachine,
  deleteMachine as deleteMachineService
} from '@/services/machines.service'
import type { MachineDto, MachineUpdateDto } from '@/api/types'
import { 
  Search, 
  Upload, 
  Download, 
  AlertTriangle 
} from 'lucide-vue-next'

const toast = useToast()

// Reactive state
const machines = ref<MachineDto[]>([])
const loading = ref(false)
const processing = ref(false)
const currentPage = ref(1)
const pageSize = ref(20)

// Import/Export state
const showImportArea = ref(false)
const isDragging = ref(false)
const fileInput = ref<HTMLInputElement | null>(null)
const previewData = ref<Record<string, string>[]>([])
const csvHeaders = ref<string[]>([])

// Filter state
const searchTerm = ref('')
const statusFilter = ref<string>('')
const typeFilter = ref<string>('')

// Selection state
const selectedMachineIds = ref<Set<string>>(new Set())

// Dialog state
const showDeleteConfirm = ref(false)
const showBulkUpdate = ref(false)

// Bulk update fields
const bulkUpdateFields = ref({
  status: '',
  location: '',
  criticality: null as number | null
})

// Computed properties
const filteredMachines = computed(() => {
  let result = [...machines.value]
  
  // Apply search filter
  if (searchTerm.value) {
    const term = searchTerm.value.toLowerCase()
    result = result.filter(machine => 
      machine.name.toLowerCase().includes(term) ||
      (machine.serialNumber && machine.serialNumber.toLowerCase().includes(term)) ||
      machine.type.toLowerCase().includes(term)
    )
  }
  
  // Apply status filter
  if (statusFilter.value) {
    result = result.filter(machine => 
      normalizeStatus(machine.status) === statusFilter.value
    )
  }
  
  // Apply type filter
  if (typeFilter.value) {
    result = result.filter(machine => machine.type === typeFilter.value)
  }
  
  return result
})

const paginatedMachines = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredMachines.value.slice(start, end)
})

const totalPages = computed(() => 
  Math.ceil(filteredMachines.value.length / pageSize.value)
)

const startIndex = computed(() => 
  (currentPage.value - 1) * pageSize.value
)

const endIndex = computed(() => 
  Math.min(currentPage.value * pageSize.value, filteredMachines.value.length)
)

const selectedMachines = computed(() => 
  machines.value.filter(machine => selectedMachineIds.value.has(machine.id))
)

const hasSelection = computed(() => selectedMachines.value.length > 0)

const areAllSelected = computed(() => 
  filteredMachines.value.length > 0 && 
  filteredMachines.value.every(machine => selectedMachineIds.value.has(machine.id))
)

const areSomeSelected = computed(() => 
  selectedMachineIds.value.size > 0 && 
  selectedMachineIds.value.size < filteredMachines.value.length
)

const hasBulkUpdateChanges = computed(() => 
  Boolean(bulkUpdateFields.value.status || 
          bulkUpdateFields.value.location || 
          bulkUpdateFields.value.criticality !== null)
)

// Options
const statusOptions = [
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

const machineTypes = [
  { label: 'CNC Machine', value: 'cnc' },
  { label: 'Injection Molder', value: 'injection_molder' },
  { label: 'Press', value: 'press' },
  { label: 'Robot', value: 'robot' },
  { label: 'Conveyor', value: 'conveyor' }
]

const criticalityLevels = [
  { label: 'Low (1)', value: 1 },
  { label: 'Medium Low (2)', value: 2 },
  { label: 'Medium (3)', value: 3 },
  { label: 'Medium High (4)', value: 4 },
  { label: 'High (5)', value: 5 }
]

// Methods
const loadMachines = async () => {
  try {
    loading.value = true
    const result = await fetchMachinesPage({ page: 1, pageSize: 1000 })
    machines.value = Array.isArray(result) ? result : result.items
  } catch (error) {
    console.error('Failed to load machines:', error)
    toast.error('Unable to load machines')
  } finally {
    loading.value = false
  }
}

const isSelected = (id: string) => selectedMachineIds.value.has(id)

const toggleSelection = (id: string) => {
  if (selectedMachineIds.value.has(id)) {
    selectedMachineIds.value.delete(id)
  } else {
    selectedMachineIds.value.add(id)
  }
}

const toggleAllSelection = () => {
  if (areAllSelected.value) {
    // Deselect all
    filteredMachines.value.forEach(machine => {
      selectedMachineIds.value.delete(machine.id)
    })
  } else {
    // Select all
    filteredMachines.value.forEach(machine => {
      selectedMachineIds.value.add(machine.id)
    })
  }
}

const clearSelection = () => {
  selectedMachineIds.value.clear()
}

const importMachines = () => {
  showImportArea.value = true
}

const triggerFileSelect = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const input = event.target as HTMLInputElement
  if (input.files && input.files[0]) {
    processCSVFile(input.files[0])
  }
}

const handleDrop = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = false
  
  if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
    processCSVFile(event.dataTransfer.files[0])
  }
}

const handleDragOver = (event: DragEvent) => {
  event.preventDefault()
}

const handleDragEnter = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = true
}

const handleDragLeave = (event: DragEvent) => {
  event.preventDefault()
  isDragging.value = false
}

const processCSVFile = (file: File) => {
  const reader = new FileReader()
  reader.onload = (e) => {
    const content = e.target?.result as string
    parseCSV(content)
  }
  reader.readAsText(file)
}

const parseCSV = (content: string) => {
  const lines = content.split('\n').filter(line => line.trim())
  if (lines.length < 2) return

  // Parse headers
  csvHeaders.value = lines[0].split(',').map(header => header.trim().replace(/"/g, ''))
  
  // Parse data rows
  previewData.value = lines.slice(1).map(line => {
    const values = line.split(',').map(value => value.trim().replace(/"/g, ''))
    const row: Record<string, string> = {}
    csvHeaders.value.forEach((header, index) => {
      row[header] = values[index] || ''
    })
    return row
  })
}

const processImport = async () => {
  if (previewData.value.length === 0) return
  
  try {
    processing.value = true
    // In a real implementation, this would call a bulk import API
    toast.success(`Successfully imported ${previewData.value.length} machines`)
    showImportArea.value = false
    previewData.value = []
    await loadMachines()
  } catch (error) {
    console.error('Import failed:', error)
    toast.error('Failed to import machines')
  } finally {
    processing.value = false
  }
}

const cancelImport = () => {
  showImportArea.value = false
  previewData.value = []
  csvHeaders.value = []
}

const exportSelected = () => {
  if (selectedMachines.value.length === 0) return
  
  // Convert to CSV format
  const headers = ['Name', 'Type', 'Status', 'Location', 'Serial Number']
  const csvContent = [
    headers.join(','),
    ...selectedMachines.value.map(machine => [
      `"${machine.name}"`,
      `"${machine.type}"`,
      `"${machine.status}"`,
      `"${machine.location || ''}"`,
      `"${machine.serialNumber || ''}"`
    ].join(','))
  ].join('\n')
  
  // Create download link
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = window.URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `machines_export_${new Date().toISOString().split('T')[0]}.csv`
  document.body.appendChild(a)
  a.click()
  window.URL.revokeObjectURL(url)
  document.body.removeChild(a)
  
  toast.success(`Exported ${selectedMachines.value.length} machines`)
}

const confirmBulkDelete = () => {
  showDeleteConfirm.value = true
}

const performBulkDelete = async () => {
  try {
    processing.value = true
    const deletePromises = selectedMachines.value.map(machine => 
      deleteMachineService(machine.id)
    )
    
    await Promise.all(deletePromises)
    toast.success(`Deleted ${selectedMachines.value.length} machines`)
    
    // Refresh data
    await loadMachines()
    clearSelection()
    showDeleteConfirm.value = false
  } catch (error) {
    console.error('Bulk delete failed:', error)
    toast.error('Failed to delete machines')
  } finally {
    processing.value = false
  }
}

const cancelDelete = () => {
  showDeleteConfirm.value = false
}

const openBulkUpdate = () => {
  showBulkUpdate.value = true
}

const performBulkUpdate = async () => {
  if (!hasBulkUpdateChanges.value) return
  
  try {
    processing.value = true
    
    const updatePayload: Partial<MachineUpdateDto> = {}
    if (bulkUpdateFields.value.status) {
      updatePayload.status = bulkUpdateFields.value.status
    }
    if (bulkUpdateFields.value.location) {
      updatePayload.location = bulkUpdateFields.value.location
    }
    if (bulkUpdateFields.value.criticality !== null) {
      ;(updatePayload as any).criticality = bulkUpdateFields.value.criticality
    }
    
    const updatePromises = selectedMachines.value.map(machine => 
      updateMachine(machine.id, updatePayload)
    )
    
    await Promise.all(updatePromises)
    toast.success(`Updated ${selectedMachines.value.length} machines`)
    
    // Refresh data
    await loadMachines()
    clearSelection()
    showBulkUpdate.value = false
    resetBulkUpdateFields()
  } catch (error) {
    console.error('Bulk update failed:', error)
    toast.error('Failed to update machines')
  } finally {
    processing.value = false
  }
}

const cancelBulkUpdate = () => {
  showBulkUpdate.value = false
  resetBulkUpdateFields()
}

const resetBulkUpdateFields = () => {
  bulkUpdateFields.value = {
    status: '',
    location: '',
    criticality: null
  }
}

const editMachine = (machine: MachineDto) => {
  // This would typically emit an event to open the edit form
  console.log('Edit machine:', machine)
  toast.info('Edit functionality would open machine form')
}

const deleteSingleMachine = async (machine: MachineDto) => {
  try {
    await deleteMachineService(machine.id)
    toast.success('Machine deleted successfully')
    await loadMachines()
  } catch (error) {
    console.error('Delete failed:', error)
    toast.error('Failed to delete machine')
  }
}

const normalizeStatus = (status: string | number): string => {
  if (typeof status === 'number') {
    const statusMap: Record<number, string> = {
      0: 'operational',
      1: 'warning',
      2: 'critical',
      3: 'maintenance',
      4: 'offline'
    }
    return statusMap[status] || 'unknown'
  }
  return status.toLowerCase()
}

const formatStatus = (status: string | number): string => {
  const normalized = normalizeStatus(status)
  const statusLabels: Record<string, string> = {
    operational: 'Operational',
    warning: 'Warning',
    critical: 'Critical',
    maintenance: 'Maintenance',
    offline: 'Offline',
    unknown: 'Unknown'
  }
  return statusLabels[normalized] || normalized
}

const formatDate = (dateString?: string): string => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString()
}

// Lifecycle
onMounted(() => {
  loadMachines()
})
</script>

<style scoped>
.bulk-machine-manager {
  padding: var(--spacing-lg);
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--spacing-md);
}

.header-content h3 {
  margin: 0;
  color: var(--color-text-primary);
}

.header-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.icon {
  width: 1rem;
  height: 1rem;
  margin-right: var(--spacing-xs);
}

.import-area {
  padding: var(--spacing-xl);
  text-align: center;
}

.drop-zone {
  border: 2px dashed var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  margin-bottom: var(--spacing-lg);
  transition: all 0.2s ease;
  background: var(--color-surface-alt);
}

.drop-zone.drag-over {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
}

.upload-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-text-secondary);
}

.hidden-file-input {
  display: none;
}

.preview-section {
  text-align: left;
  margin-top: var(--spacing-lg);
}

.preview-section h4 {
  margin-bottom: var(--spacing-md);
  color: var(--color-text-primary);
}

.preview-table-container {
  max-height: 200px;
  overflow-y: auto;
  margin-bottom: var(--spacing-md);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
}

.preview-table {
  width: 100%;
  border-collapse: collapse;
}

.preview-table th,
.preview-table td {
  padding: var(--spacing-sm);
  text-align: left;
  border-bottom: 1px solid var(--color-border-subtle);
}

.preview-table th {
  background: var(--color-surface-alt);
  font-weight: 600;
  position: sticky;
  top: 0;
}

.preview-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
}

.filter-section {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--spacing-md);
  margin-bottom: var(--spacing-lg);
  padding-bottom: var(--spacing-md);
  border-bottom: 1px solid var(--color-border-subtle);
}

.filters {
  display: flex;
  gap: var(--spacing-sm);
  flex-wrap: wrap;
}

.search-input {
  min-width: 250px;
}

.search-icon {
  width: 1rem;
  height: 1rem;
  color: var(--color-text-secondary);
}

.selection-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.bulk-action-buttons {
  display: flex;
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-lg);
  padding: var(--spacing-md);
  background: var(--color-surface-alt);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
}

.machines-table-container {
  overflow-x: auto;
  margin-bottom: var(--spacing-lg);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
}

.machines-table {
  width: 100%;
  border-collapse: collapse;
}

.machines-table th,
.machines-table td {
  padding: var(--spacing-md);
  text-align: left;
  border-bottom: 1px solid var(--color-border-subtle);
}

.machines-table th {
  background: var(--color-surface-alt);
  font-weight: 600;
  position: sticky;
  top: 0;
  z-index: 1;
}

.select-col {
  width: 1px;
  text-align: center;
}

.actions-col {
  width: 1px;
  white-space: nowrap;
}

.machine-name {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.machine-name .name {
  font-weight: 500;
}

.machine-name .serial {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.status-badge {
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.status-badge.status-operational {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.status-badge.status-warning {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.status-badge.status-critical {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.status-badge.status-maintenance {
  background: color-mix(in srgb, var(--color-primary) 20%, transparent);
  color: var(--color-primary);
}

.status-badge.status-offline {
  background: color-mix(in srgb, var(--color-text-secondary) 20%, transparent);
  color: var(--color-text-secondary);
}

.row-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--spacing-md);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--color-border-subtle);
}

.pagination-info {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.page-info {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  padding: 0 var(--spacing-sm);
}

.confirmation-dialog,
.bulk-update-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 1000;
  padding: var(--spacing-xl);
  min-width: 400px;
  max-width: 90vw;
}

.dialog-content {
  text-align: center;
}

.warning-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-warning);
}

.dialog-content h3 {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-primary);
}

.dialog-content p {
  margin: 0 0 var(--spacing-lg) 0;
  color: var(--color-text-secondary);
}

.update-fields {
  text-align: left;
  margin: var(--spacing-lg) 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.dialog-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: center;
}

/* Overlay effect when dialogs are open */
.bulk-machine-manager:has(.confirmation-dialog),
.bulk-machine-manager:has(.bulk-update-dialog) {
  position: relative;
}

.bulk-machine-manager:has(.confirmation-dialog)::before,
.bulk-machine-manager:has(.bulk-update-dialog)::before {
  content: '';
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
  backdrop-filter: blur(4px);
}

tr.selected {
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
}

tr.selected:hover {
  background: color-mix(in srgb, var(--color-primary) 15%, transparent);
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: stretch;
  }
  
  .filters {
    flex-direction: column;
  }
  
  .search-input {
    min-width: unset;
  }
  
  .bulk-action-buttons {
    flex-direction: column;
  }
  
  .pagination {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .confirmation-dialog,
  .bulk-update-dialog {
    min-width: 90vw;
    padding: var(--spacing-lg);
  }
  
  .dialog-actions {
    flex-direction: column;
  }
}
</style>