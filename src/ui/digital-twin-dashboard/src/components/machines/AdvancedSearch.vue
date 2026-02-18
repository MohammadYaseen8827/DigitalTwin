<template>
  <BaseCard variant="soft" class="advanced-search">
    <template #header>
      <div class="header-content">
        <div>
          <h3>Advanced Machine Search</h3>
          <p>Find machines using multiple criteria and filters</p>
        </div>
        <div class="header-actions">
          <BaseButton 
            variant="outline" 
            size="sm"
            @click="saveSearch"
            :disabled="!hasActiveFilters || savingSearch"
          >
            <Bookmark class="icon" />
            Save Search
          </BaseButton>
          <BaseButton 
            variant="outline" 
            size="sm"
            @click="resetAllFilters"
          >
            <RotateCcw class="icon" />
            Reset
          </BaseButton>
        </div>
      </div>
    </template>

    <div class="search-container">
      <!-- Search Controls -->
      <div class="search-controls">
        <!-- Quick Search -->
        <div class="quick-search">
          <BaseInput
            v-model="searchQuery"
            placeholder="Search by name, serial number, or location..."
            class="search-input"
            @input="debouncedSearch"
          >
            <template #prefix>
              <Search class="search-icon" />
            </template>
            <template #suffix>
              <button 
                v-if="searchQuery" 
                class="clear-button"
                @click="clearSearch"
              >
                <X class="icon" />
              </button>
            </template>
          </BaseInput>
        </div>

        <!-- Advanced Filters Toggle -->
        <BaseButton
          variant="ghost"
          @click="showAdvancedFilters = !showAdvancedFilters"
          class="filters-toggle"
        >
          <Filter class="icon" />
          Advanced Filters
          <ChevronDown 
            class="chevron" 
            :class="{ 'rotated': showAdvancedFilters }" 
          />
        </BaseButton>
      </div>

      <!-- Advanced Filters Panel -->
      <div v-show="showAdvancedFilters" class="filters-panel">
        <div class="filters-grid">
          <!-- Status Filter -->
          <div class="filter-group">
            <label class="filter-label">Status</label>
            <div class="checkbox-group">
              <label 
                v-for="status in statusOptions" 
                :key="status.value"
                class="checkbox-label"
              >
                <input
                  type="checkbox"
                  :value="status.value"
                  v-model="filters.status"
                  class="checkbox"
                />
                <span class="status-indicator" :class="`status-${status.value}`"></span>
                {{ status.label }}
              </label>
            </div>
          </div>

          <!-- Machine Type Filter -->
          <div class="filter-group">
            <label class="filter-label">Machine Type</label>
            <div class="checkbox-group">
              <label 
                v-for="type in machineTypes" 
                :key="type.value"
                class="checkbox-label"
              >
                <input
                  type="checkbox"
                  :value="type.value"
                  v-model="filters.machineType"
                  class="checkbox"
                />
                {{ type.label }}
              </label>
            </div>
          </div>

          <!-- Criticality Filter -->
          <div class="filter-group">
            <label class="filter-label">Criticality Level</label>
            <BaseSelect
              v-model="filters.criticality"
              :options="criticalityOptions"
              placeholder="Any criticality"
              clearable
              multiple
            />
          </div>

          <!-- Date Range Filters -->
          <div class="filter-group">
            <label class="filter-label">Installation Date</label>
            <div class="date-range">
              <BaseInput
                v-model="filters.installationDate.from"
                type="date"
                placeholder="From"
              />
              <span class="date-separator">to</span>
              <BaseInput
                v-model="filters.installationDate.to"
                type="date"
                placeholder="To"
              />
            </div>
          </div>

          <div class="filter-group">
            <label class="filter-label">Last Maintenance</label>
            <div class="date-range">
              <BaseInput
                v-model="filters.lastMaintenance.from"
                type="date"
                placeholder="From"
              />
              <span class="date-separator">to</span>
              <BaseInput
                v-model="filters.lastMaintenance.to"
                type="date"
                placeholder="To"
              />
            </div>
          </div>

          <!-- Location Filter -->
          <div class="filter-group">
            <label class="filter-label">Location</label>
            <BaseInput
              v-model="filters.location"
              placeholder="Enter location..."
            />
          </div>

          <!-- Manufacturer Filter -->
          <div class="filter-group">
            <label class="filter-label">Manufacturer</label>
            <BaseSelect
              v-model="filters.manufacturer"
              :options="manufacturers"
              placeholder="Any manufacturer"
              clearable
            />
          </div>

          <!-- Metadata Search -->
          <div class="filter-group metadata-filter">
            <label class="filter-label">Metadata Search</label>
            <BaseInput
              v-model="filters.metadataKey"
              placeholder="Metadata key (e.g., department)"
              class="metadata-key"
            />
            <BaseInput
              v-model="filters.metadataValue"
              placeholder="Metadata value"
              class="metadata-value"
            />
          </div>
        </div>

        <!-- Filter Actions -->
        <div class="filter-actions">
          <BaseButton 
            variant="primary" 
            @click="applyFilters"
            :disabled="processing"
          >
            Apply Filters
          </BaseButton>
          <BaseButton 
            variant="outline" 
            @click="resetFilters"
          >
            Reset Filters
          </BaseButton>
        </div>
      </div>

      <!-- Active Filters Display -->
      <div v-if="hasActiveFilters" class="active-filters">
        <div class="filters-summary">
          <span class="results-count">
            {{ filteredMachines.length }} machines found
          </span>
          <div class="applied-filters">
            <span 
              v-for="filter in activeFilterChips" 
              :key="filter.key"
              class="filter-chip"
            >
              <span class="filter-key">{{ filter.label }}:</span>
              <span class="filter-value">{{ filter.value }}</span>
              <button 
                class="remove-filter"
                @click="removeFilter(filter.key)"
              >
                <X class="icon" />
              </button>
            </span>
          </div>
        </div>
      </div>

      <!-- Results -->
      <div class="results-container">
        <!-- Loading State -->
        <div v-if="loading" class="loading-state">
          <BaseSkeleton width="100%" height="400px" />
        </div>

        <!-- Results Grid -->
        <div v-else-if="filteredMachines.length > 0" class="results-grid">
          <BaseCard
            v-for="machine in paginatedResults"
            :key="machine.id"
            variant="bordered"
            class="machine-result"
            @click="selectMachine(machine)"
          >
            <div class="machine-header">
              <div class="machine-basic-info">
                <h4 class="machine-name">{{ machine.name }}</h4>
                <span 
                  v-if="machine.serialNumber"
                  class="machine-serial"
                >
                  SN: {{ machine.serialNumber }}
                </span>
              </div>
              <span 
                class="status-badge"
                :class="`status-${normalizeStatus(machine.status)}`"
              >
                {{ formatStatus(machine.status) }}
              </span>
            </div>
            
            <div class="machine-details">
              <div class="detail-row">
                <span class="detail-label">Type:</span>
                <span class="detail-value">{{ machine.type }}</span>
              </div>
              
              <div class="detail-row">
                <span class="detail-label">Location:</span>
                <span class="detail-value">{{ machine.location || 'Not specified' }}</span>
              </div>
              
              <div class="detail-row">
                <span class="detail-label">Criticality:</span>
                <span class="detail-value">
                  <span class="criticality-indicator" :class="`level-${machine.criticality || 3}`">
                    Level {{ machine.criticality || 3 }}
                  </span>
                </span>
              </div>
              
              <div 
                v-if="machine.installationDate" 
                class="detail-row"
              >
                <span class="detail-label">Installed:</span>
                <span class="detail-value">{{ formatDate(machine.installationDate) }}</span>
              </div>
              
              <div 
                v-if="machine.lastMaintenance" 
                class="detail-row"
              >
                <span class="detail-label">Last Maintenance:</span>
                <span class="detail-value">{{ formatDate(machine.lastMaintenance) }}</span>
              </div>
            </div>
            
            <div class="machine-actions">
              <BaseButton 
                variant="primary" 
                size="sm"
                @click.stop="viewDetails(machine)"
              >
                View Details
              </BaseButton>
              <BaseButton 
                variant="outline" 
                size="sm"
                @click.stop="editMachine(machine)"
              >
                Edit
              </BaseButton>
            </div>
          </BaseCard>
        </div>

        <!-- Empty State -->
        <div v-else class="empty-state">
          <div class="empty-content">
            <Search class="empty-icon" />
            <h3>No machines found</h3>
            <p>
              Try adjusting your search criteria or filters to find what you're looking for.
            </p>
            <BaseButton 
              variant="outline" 
              @click="resetAllFilters"
            >
              Reset All Filters
            </BaseButton>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="filteredMachines.length > 0" class="pagination">
        <div class="pagination-info">
          Showing {{ startIndex + 1 }}-{{ endIndex }} of {{ filteredMachines.length }} results
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

    <!-- Saved Searches Modal -->
    <div v-if="showSavedSearches" class="modal-overlay" @click="closeSavedSearches">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Saved Searches</h3>
          <BaseButton variant="ghost" @click="closeSavedSearches">
            <X class="icon" />
          </BaseButton>
        </div>
        
        <div class="modal-body">
          <div 
            v-if="savedSearches.length === 0" 
            class="no-saved-searches"
          >
            <Bookmark class="bookmark-icon" />
            <p>You haven't saved any searches yet.</p>
          </div>
          
          <div 
            v-else 
            class="saved-searches-list"
          >
            <div 
              v-for="search in savedSearches" 
              :key="search.id"
              class="saved-search-item"
            >
              <div class="search-info">
                <h4>{{ search.name }}</h4>
                <p>{{ search.criteria }}</p>
                <span class="search-date">
                  Saved {{ formatDate(search.savedAt) }}
                </span>
              </div>
              <div class="search-actions">
                <BaseButton 
                  variant="primary" 
                  size="sm"
                  @click="loadSavedSearch(search)"
                >
                  Load
                </BaseButton>
                <BaseButton 
                  variant="ghost" 
                  size="sm"
                  @click="deleteSavedSearch(search.id)"
                >
                  <Trash2 class="icon" />
                </BaseButton>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  Search, 
  Filter, 
  Bookmark, 
  RotateCcw, 
  ChevronDown, 
  X, 
  Trash2 
} from 'lucide-vue-next'

const emit = defineEmits<{
  (e: 'machine-selected', machine: MachineDto): void
  (e: 'view-details', machine: MachineDto): void
  (e: 'edit-machine', machine: MachineDto): void
}>()

const toast = useToast()

// Reactive state
const allMachines = ref<MachineDto[]>([])
const loading = ref(false)
const processing = ref(false)
const savingSearch = ref(false)

// Search state
const searchQuery = ref('')
const showAdvancedFilters = ref(false)
const currentPage = ref(1)
const pageSize = ref(12)

// Filters
const filters = ref({
  status: [] as string[],
  machineType: [] as string[],
  criticality: [] as number[],
  installationDate: { from: '', to: '' },
  lastMaintenance: { from: '', to: '' },
  location: '',
  manufacturer: '',
  metadataKey: '',
  metadataValue: ''
})

// Saved searches
const showSavedSearches = ref(false)
const savedSearches = ref<Array<{
  id: string
  name: string
  filters: any
  criteria: string
  savedAt: string
}>>([])

// Computed properties
const filteredMachines = computed(() => {
  let results = [...allMachines.value]
  
  // Apply text search
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    results = results.filter(machine => 
      machine.name.toLowerCase().includes(query) ||
      (machine.serialNumber && machine.serialNumber.toLowerCase().includes(query)) ||
      (machine.location && machine.location.toLowerCase().includes(query))
    )
  }
  
  // Apply status filter
  if (filters.value.status.length > 0) {
    results = results.filter(machine => 
      filters.value.status.includes(normalizeStatus(machine.status))
    )
  }
  
  // Apply machine type filter
  if (filters.value.machineType.length > 0) {
    results = results.filter(machine => 
      filters.value.machineType.includes(machine.type)
    )
  }
  
  // Apply criticality filter
  if (filters.value.criticality.length > 0) {
    results = results.filter(machine => 
      machine.criticality && filters.value.criticality.includes(machine.criticality)
    )
  }
  
  // Apply location filter
  if (filters.value.location) {
    results = results.filter(machine => 
      machine.location && machine.location.toLowerCase().includes(filters.value.location.toLowerCase())
    )
  }
  
  // Apply manufacturer filter
  if (filters.value.manufacturer) {
    results = results.filter(machine => 
      machine.manufacturer && machine.manufacturer.toLowerCase().includes(filters.value.manufacturer.toLowerCase())
    )
  }
  
  // Apply date range filters
  if (filters.value.installationDate.from || filters.value.installationDate.to) {
    results = results.filter(machine => {
      if (!machine.installationDate) return false
      const installDate = new Date(machine.installationDate)
      const fromDate = filters.value.installationDate.from ? new Date(filters.value.installationDate.from) : null
      const toDate = filters.value.installationDate.to ? new Date(filters.value.installationDate.to) : null
      
      if (fromDate && installDate < fromDate) return false
      if (toDate && installDate > toDate) return false
      return true
    })
  }
  
  if (filters.value.lastMaintenance.from || filters.value.lastMaintenance.to) {
    results = results.filter(machine => {
      if (!machine.lastMaintenance) return false
      const maintDate = new Date(machine.lastMaintenance)
      const fromDate = filters.value.lastMaintenance.from ? new Date(filters.value.lastMaintenance.from) : null
      const toDate = filters.value.lastMaintenance.to ? new Date(filters.value.lastMaintenance.to) : null
      
      if (fromDate && maintDate < fromDate) return false
      if (toDate && maintDate > toDate) return false
      return true
    })
  }
  
  // Apply metadata filter
  if (filters.value.metadataKey && filters.value.metadataValue) {
    results = results.filter(machine => {
      if (!machine.metadata) return false
      const value = machine.metadata[filters.value.metadataKey]
      return value && String(value).toLowerCase().includes(filters.value.metadataValue.toLowerCase())
    })
  }
  
  return results
})

const paginatedResults = computed(() => {
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

const hasActiveFilters = computed(() => {
  return searchQuery.value ||
         filters.value.status.length > 0 ||
         filters.value.machineType.length > 0 ||
         filters.value.criticality.length > 0 ||
         filters.value.installationDate.from ||
         filters.value.installationDate.to ||
         filters.value.lastMaintenance.from ||
         filters.value.lastMaintenance.to ||
         filters.value.location ||
         filters.value.manufacturer ||
         (filters.value.metadataKey && filters.value.metadataValue)
})

const activeFilterChips = computed(() => {
  const chips = []
  
  if (searchQuery.value) {
    chips.push({ key: 'search', label: 'Search', value: searchQuery.value })
  }
  
  if (filters.value.status.length > 0) {
    chips.push({ 
      key: 'status', 
      label: 'Status', 
      value: filters.value.status.map(s => 
        statusOptions.find(opt => opt.value === s)?.label || s
      ).join(', ')
    })
  }
  
  if (filters.value.machineType.length > 0) {
    chips.push({ 
      key: 'type', 
      label: 'Type', 
      value: filters.value.machineType.map(t => 
        machineTypes.find(opt => opt.value === t)?.label || t
      ).join(', ')
    })
  }
  
  if (filters.value.criticality.length > 0) {
    chips.push({ 
      key: 'criticality', 
      label: 'Criticality', 
      value: filters.value.criticality.join(', ')
    })
  }
  
  if (filters.value.location) {
    chips.push({ key: 'location', label: 'Location', value: filters.value.location })
  }
  
  if (filters.value.manufacturer) {
    chips.push({ key: 'manufacturer', label: 'Manufacturer', value: filters.value.manufacturer })
  }
  
  return chips
})

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
  { label: 'Conveyor', value: 'conveyor' },
  { label: 'Pump', value: 'pump' },
  { label: 'Motor', value: 'motor' }
]

const criticalityOptions = [
  { label: 'Level 1 - Low', value: 1 },
  { label: 'Level 2 - Medium Low', value: 2 },
  { label: 'Level 3 - Medium', value: 3 },
  { label: 'Level 4 - Medium High', value: 4 },
  { label: 'Level 5 - High', value: 5 }
]

const manufacturers = [
  { label: 'Siemens', value: 'siemens' },
  { label: 'ABB', value: 'abb' },
  { label: 'Fanuc', value: 'fanuc' },
  { label: 'KUKA', value: 'kuka' },
  { label: 'Universal Robots', value: 'ur' }
]

// Methods
const loadMachines = async () => {
  try {
    loading.value = true
    allMachines.value = await fetchMachines()
  } catch (error) {
    console.error('Failed to load machines:', error)
    toast.error('Unable to load machines')
  } finally {
    loading.value = false
  }
}

const debouncedSearch = debounce(() => {
  currentPage.value = 1
}, 300)

const clearSearch = () => {
  searchQuery.value = ''
  currentPage.value = 1
}

const applyFilters = () => {
  currentPage.value = 1
  toast.success('Filters applied')
}

const resetFilters = () => {
  filters.value = {
    status: [],
    machineType: [],
    criticality: [],
    installationDate: { from: '', to: '' },
    lastMaintenance: { from: '', to: '' },
    location: '',
    manufacturer: '',
    metadataKey: '',
    metadataValue: ''
  }
  currentPage.value = 1
}

const resetAllFilters = () => {
  searchQuery.value = ''
  resetFilters()
  toast.info('All filters reset')
}

const removeFilter = (key: string) => {
  switch (key) {
    case 'search':
      searchQuery.value = ''
      break
    case 'status':
      filters.value.status = []
      break
    case 'type':
      filters.value.machineType = []
      break
    case 'criticality':
      filters.value.criticality = []
      break
    case 'location':
      filters.value.location = ''
      break
    case 'manufacturer':
      filters.value.manufacturer = ''
      break
  }
  currentPage.value = 1
}

const saveSearch = async () => {
  if (!hasActiveFilters.value) return
  
  try {
    savingSearch.value = true
    
    const searchName = prompt('Enter a name for this search:')
    if (!searchName) return
    
    const newSearch = {
      id: `search_${Date.now()}`,
      name: searchName,
      filters: JSON.parse(JSON.stringify(filters.value)),
      criteria: generateSearchCriteria(),
      savedAt: new Date().toISOString()
    }
    
    savedSearches.value.push(newSearch)
    localStorage.setItem('savedSearches', JSON.stringify(savedSearches.value))
    
    toast.success('Search saved successfully')
  } catch (error) {
    console.error('Failed to save search:', error)
    toast.error('Failed to save search')
  } finally {
    savingSearch.value = false
  }
}

const loadSavedSearch = (search: any) => {
  filters.value = JSON.parse(JSON.stringify(search.filters))
  showSavedSearches.value = false
  currentPage.value = 1
  toast.success(`Loaded search: ${search.name}`)
}

const deleteSavedSearch = (id: string) => {
  if (!confirm('Delete this saved search?')) return
  
  savedSearches.value = savedSearches.value.filter(s => s.id !== id)
  localStorage.setItem('savedSearches', JSON.stringify(savedSearches.value))
  toast.success('Search deleted')
}

const closeSavedSearches = () => {
  showSavedSearches.value = false
}

const selectMachine = (machine: MachineDto) => {
  emit('machine-selected', machine)
}

const viewDetails = (machine: MachineDto) => {
  emit('view-details', machine)
}

const editMachine = (machine: MachineDto) => {
  emit('edit-machine', machine)
}

const generateSearchCriteria = (): string => {
  const criteria = []
  
  if (searchQuery.value) {
    criteria.push(`"${searchQuery.value}"`)
  }
  
  if (filters.value.status.length > 0) {
    criteria.push(`${filters.value.status.length} status(es)`)
  }
  
  if (filters.value.machineType.length > 0) {
    criteria.push(`${filters.value.machineType.length} type(s)`)
  }
  
  if (filters.value.criticality.length > 0) {
    criteria.push(`criticality ${filters.value.criticality.join(', ')}`)
  }
  
  if (filters.value.location) {
    criteria.push(`location: ${filters.value.location}`)
  }
  
  return criteria.join(', ') || 'All machines'
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

function debounce<F extends (...args: any[]) => any>(func: F, wait: number): (...args: Parameters<F>) => void {
  let timeout: NodeJS.Timeout
  return (...args: Parameters<F>) => {
    clearTimeout(timeout)
    timeout = setTimeout(() => func(...args), wait)
  }
}

// Watch for filter changes to reset pagination
watch(filters, () => {
  currentPage.value = 1
}, { deep: true })

// Lifecycle
onMounted(() => {
  loadMachines()
  
  // Load saved searches from localStorage
  const saved = localStorage.getItem('savedSearches')
  if (saved) {
    try {
      savedSearches.value = JSON.parse(saved)
    } catch (error) {
      console.error('Failed to load saved searches:', error)
    }
  }
})
</script>

<style scoped>
.advanced-search {
  padding: var(--spacing-lg);
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: var(--spacing-md);
}

.header-content h3 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.header-content p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.header-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.search-container {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-lg);
}

.search-controls {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.quick-search {
  position: relative;
}

.search-input {
  width: 100%;
}

.search-icon {
  width: 1rem;
  height: 1rem;
  color: var(--color-text-secondary);
}

.clear-button {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--color-text-secondary);
  padding: var(--spacing-xs);
  border-radius: var(--radius-full);
}

.clear-button:hover {
  background: var(--color-surface-hover);
}

.filters-toggle {
  display: flex;
  align-items: center;
  gap: var(--spacing-xs);
  width: fit-content;
}

.chevron {
  width: 1rem;
  height: 1rem;
  transition: transform 0.2s ease;
}

.chevron.rotated {
  transform: rotate(180deg);
}

.filters-panel {
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border-subtle);
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-sm);
}

.filter-label {
  font-weight: 500;
  color: var(--color-text-primary);
  font-size: 0.875rem;
}

.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  cursor: pointer;
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.checkbox {
  width: 16px;
  height: 16px;
  accent-color: var(--color-primary);
}

.status-indicator {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  display: inline-block;
}

.status-indicator.status-operational {
  background: var(--color-success);
}

.status-indicator.status-warning {
  background: var(--color-warning);
}

.status-indicator.status-critical {
  background: var(--color-error);
}

.status-indicator.status-maintenance {
  background: var(--color-primary);
}

.status-indicator.status-offline {
  background: var(--color-text-secondary);
}

.date-range {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
}

.date-separator {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.metadata-filter {
  grid-column: 1 / -1;
}

.metadata-key,
.metadata-value {
  flex: 1;
}

.filter-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: flex-end;
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--color-border-subtle);
}

.active-filters {
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--spacing-md);
  border: 1px solid var(--color-border-subtle);
}

.filters-summary {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-md);
  align-items: center;
  justify-content: space-between;
}

.results-count {
  font-weight: 500;
  color: var(--color-text-primary);
}

.applied-filters {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
}

.filter-chip {
  display: inline-flex;
  align-items: center;
  gap: var(--spacing-xs);
  background: var(--color-primary);
  color: white;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-size: 0.75rem;
}

.filter-key {
  font-weight: 500;
}

.filter-value {
  font-weight: 300;
}

.remove-filter {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: var(--spacing-xs);
  border-radius: var(--radius-full);
  margin-left: var(--spacing-xs);
}

.remove-filter:hover {
  background: rgba(255, 255, 255, 0.2);
}

.results-container {
  min-height: 400px;
}

.loading-state {
  padding: var(--spacing-xl);
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--spacing-md);
}

.machine-result {
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid var(--color-border-subtle);
}

.machine-result:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.machine-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--spacing-md);
}

.machine-basic-info {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.machine-name {
  margin: 0;
  color: var(--color-text-primary);
  font-size: 1.125rem;
}

.machine-serial {
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

.machine-details {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
  margin-bottom: var(--spacing-md);
}

.detail-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.detail-label {
  color: var(--color-text-secondary);
}

.detail-value {
  color: var(--color-text-primary);
  font-weight: 500;
}

.criticality-indicator {
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-size: 0.75rem;
}

.criticality-indicator.level-1 {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.criticality-indicator.level-2,
.criticality-indicator.level-3 {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.criticality-indicator.level-4,
.criticality-indicator.level-5 {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.machine-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.empty-state {
  text-align: center;
  padding: var(--spacing-2xl) var(--spacing-lg);
}

.empty-content {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  width: 4rem;
  height: 4rem;
  margin-bottom: var(--spacing-lg);
  color: var(--color-text-secondary);
  opacity: 0.5;
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

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: var(--spacing-md);
  backdrop-filter: blur(4px);
}

.modal-content {
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--color-border-subtle);
}

.modal-header h3 {
  margin: 0;
  color: var(--color-text-primary);
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: var(--spacing-lg);
}

.no-saved-searches {
  text-align: center;
  padding: var(--spacing-xl);
}

.bookmark-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-text-secondary);
  opacity: 0.5;
}

.saved-searches-list {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.saved-search-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: var(--spacing-md);
  background: var(--color-surface-alt);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
}

.search-info h4 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.search-info p {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.search-date {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.search-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.icon {
  width: 1rem;
  height: 1rem;
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: stretch;
  }
  
  .header-actions {
    width: 100%;
    justify-content: flex-end;
  }
  
  .filters-grid {
    grid-template-columns: 1fr;
  }
  
  .filters-summary {
    flex-direction: column;
    align-items: stretch;
  }
  
  .results-grid {
    grid-template-columns: 1fr;
  }
  
  .pagination {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .modal-content {
    margin: var(--spacing-sm);
    max-height: 95vh;
  }
  
  .saved-search-item {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .search-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>