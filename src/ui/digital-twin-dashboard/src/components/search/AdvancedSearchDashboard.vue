<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import { searchService } from '@/services/search.service'
import type { MachineDto, SearchRequestDto, SearchResultDto, SearchSuggestionDto, SavedSearchDto } from '@/api/types'
import { 
  Search, 
  Filter, 
  Save, 
  Download,
  Calendar,
  Hash,
  MapPin,
  Activity,
  Settings,
  X,
  ChevronDown
} from 'lucide-vue-next'

const toast = useToast()

// State
const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchResults = ref<any[]>([])
const savedSearches = ref<SavedSearchDto[]>([])
const suggestions = ref<SearchSuggestionDto[]>([])

const searchTerm = ref('')
const entityType = ref('all')
const dateRange = ref('all')
const statusFilter = ref('all')
const sortBy = ref('relevance')
const showAdvancedFilters = ref(false)
const showSaveModal = ref(false)
const searchName = ref('')

// Form state for advanced filters
const advancedFilters = ref({
  minTemperature: '',
  maxTemperature: '',
  minPressure: '',
  maxPressure: '',
  location: '',
  manufacturer: '',
  model: ''
})

// Computed
const entityOptions = [
  { label: 'All Entities', value: 'all' },
  { label: 'Machines', value: 'machines' },
  { label: 'Telemetry Data', value: 'telemetry' },
  { label: 'Maintenance Records', value: 'maintenance' },
  { label: 'Alerts', value: 'alerts' },
  { label: 'Production Lines', value: 'production_lines' }
]

const dateRangeOptions = [
  { label: 'Any Time', value: 'all' },
  { label: 'Last Hour', value: '1h' },
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' },
  { label: 'Last 30 Days', value: '30d' },
  { label: 'Last Year', value: '1y' }
]

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

const sortOptions = [
  { label: 'Relevance', value: 'relevance' },
  { label: 'Date (Newest)', value: 'date_desc' },
  { label: 'Date (Oldest)', value: 'date_asc' },
  { label: 'Name (A-Z)', value: 'name_asc' },
  { label: 'Name (Z-A)', value: 'name_desc' }
]

const filteredSavedSearches = computed(() => {
  return savedSearches.value.filter(search => search.name.toLowerCase().includes(searchTerm.value.toLowerCase()))
})

// Methods
const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    toast.error('Error loading machines')
  }
}

const loadSavedSearches = async () => {
  try {
    savedSearches.value = await searchService.getSavedSearches()
  } catch (error) {
    toast.error('Error loading saved searches')
  }
}

const performSearch = async () => {
  if (!searchTerm.value.trim()) {
    toast.error('Please enter a search term')
    return
  }

  try {
    loading.value = true
    
    const searchRequest: SearchRequestDto = {
      query: searchTerm.value,
      entityTypes: entityType.value === 'all' ? undefined : [entityType.value],
      dateRange: dateRange.value === 'all' ? undefined : dateRange.value,
      statusFilter: statusFilter.value === 'all' ? undefined : statusFilter.value,
      sortBy: sortBy.value,
      page: 0,
      pageSize: 20
    }

    // Add advanced filters if any are set
    if (Object.values(advancedFilters.value).some(v => v)) {
      searchRequest.advancedFilters = {
        minTemperature: advancedFilters.value.minTemperature ? parseFloat(advancedFilters.value.minTemperature) : undefined,
        maxTemperature: advancedFilters.value.maxTemperature ? parseFloat(advancedFilters.value.maxTemperature) : undefined,
        minPressure: advancedFilters.value.minPressure ? parseFloat(advancedFilters.value.minPressure) : undefined,
        maxPressure: advancedFilters.value.maxPressure ? parseFloat(advancedFilters.value.maxPressure) : undefined,
        location: advancedFilters.value.location || undefined,
        manufacturer: advancedFilters.value.manufacturer || undefined,
        model: advancedFilters.value.model || undefined
      }
    }

    const result = await searchService.search(searchRequest)
    searchResults.value = result.items
    
    toast.success(`Found ${result.totalCount} results`)
    
  } catch (error) {
    toast.error('Search failed. Please try again.')
  } finally {
    loading.value = false
  }
}

const calculateMatchScore = (text: string, query: string): number => {
  const textLower = text.toLowerCase()
  const queryLower = query.toLowerCase()
  
  if (textLower === queryLower) return 1.0
  if (textLower.startsWith(queryLower)) return 0.9
  if (textLower.includes(queryLower)) return 0.7
  return 0.3
}

const clearSearch = () => {
  searchTerm.value = ''
  searchResults.value = []
  entityType.value = 'all'
  dateRange.value = 'all'
  statusFilter.value = 'all'
  sortBy.value = 'relevance'
  advancedFilters.value = {
    minTemperature: '',
    maxTemperature: '',
    minPressure: '',
    maxPressure: '',
    location: '',
    manufacturer: '',
    model: ''
  }
}

const toggleAdvancedFilters = () => {
  showAdvancedFilters.value = !showAdvancedFilters.value
}

const saveSearch = () => {
  if (!searchTerm.value.trim() || !searchName.value.trim()) {
    toast.error('Please enter search term and name')
    return
  }
  
  const newSearch = {
    id: `SEARCH${String(savedSearches.value.length + 1).padStart(3, '0')}`,
    name: searchName.value,
    query: searchTerm.value,
    entityType: entityType.value,
    createdAt: new Date().toISOString(),
    lastUsed: new Date().toISOString()
  }
  
  savedSearches.value.push(newSearch)
  showSaveModal.value = false
  searchName.value = ''
  toast.success('Search saved successfully')
}

const loadSavedSearch = (savedSearch: any) => {
  searchTerm.value = savedSearch.query
  entityType.value = savedSearch.entityType
  performSearch()
  toast.success(`Loaded search: ${savedSearch.name}`)
}

const deleteSavedSearch = (searchId: string) => {
  if (!confirm('Are you sure you want to delete this saved search?')) {
    return
  }
  
  savedSearches.value = savedSearches.value.filter(s => s.id !== searchId)
  toast.success('Saved search deleted')
}

const exportResults = () => {
  if (searchResults.value.length === 0) {
    toast.error('No results to export')
    return
  }
  
  const csvContent = [
    ['ID', 'Type', 'Title', 'Subtitle', 'Status', 'Created'].join(','),
    ...searchResults.value.map(result => [
      result.id,
      result.type,
      `"${result.title}"`,
      `"${result.subtitle}"`,
      result.status,
      result.createdAt
    ].join(','))
  ].join('\n')
  
  const blob = new Blob([csvContent], { type: 'text/csv' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = `search_results_${new Date().toISOString().slice(0, 10)}.csv`
  link.click()
  URL.revokeObjectURL(url)
  
  toast.success('Results exported successfully')
}

const getStatusColor = (status: string) => {
  switch (status?.toString().toLowerCase()) {
    case 'operational':
    case 'normal':
      return 'text-green-600 bg-green-100'
    case 'warning':
      return 'text-yellow-600 bg-yellow-100'
    case 'critical':
      return 'text-red-600 bg-red-100'
    case 'maintenance':
      return 'text-blue-600 bg-blue-100'
    case 'offline':
      return 'text-gray-600 bg-gray-100'
    default:
      return 'text-gray-600 bg-gray-100'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

onMounted(() => {
  loadMachines()
})
</script>

<template>
  <BaseCard class="advanced-search">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Search class="header-icon" />
          Advanced Search
        </h2>
        <p class="header-subtitle">Powerful search across all system entities and data</p>
      </div>
      <div class="header-controls" v-if="searchResults.length > 0">
        <BaseButton variant="outline" @click="exportResults">
          <Download class="button-icon" />
          Export Results
        </BaseButton>
      </div>
    </template>

    <!-- Search Interface -->
    <div class="search-interface">
      <div class="search-bar">
        <div class="search-input-group">
          <BaseInput
            v-model="searchTerm"
            placeholder="Enter search terms..."
            @keyup.enter="performSearch"
            class="main-search-input"
          >
            <template #prefix>
              <Search class="search-icon" />
            </template>
          </BaseInput>
          <BaseButton variant="primary" @click="performSearch" :disabled="loading || !searchTerm.trim()">
            <Search v-if="!loading" class="button-icon" />
            <div v-else class="spinner"></div>
            Search
          </BaseButton>
        </div>
        
        <BaseButton 
          variant="outline" 
          @click="toggleAdvancedFilters"
          class="filters-toggle"
        >
          <Filter class="button-icon" />
          Advanced Filters
          <ChevronDown 
            class="chevron" 
            :class="{ 'rotated': showAdvancedFilters }" 
          />
        </BaseButton>
      </div>

      <!-- Advanced Filters Panel -->
      <div v-show="showAdvancedFilters" class="advanced-filters-panel">
        <div class="filters-grid">
          <BaseSelect
            v-model="entityType"
            label="Entity Type"
            :options="entityOptions"
          />
          
          <BaseSelect
            v-model="dateRange"
            label="Date Range"
            :options="dateRangeOptions"
          />
          
          <BaseSelect
            v-model="statusFilter"
            label="Status"
            :options="statusOptions"
          />
          
          <BaseSelect
            v-model="sortBy"
            label="Sort By"
            :options="sortOptions"
          />
          
          <BaseInput
            v-model="advancedFilters.minTemperature"
            label="Min Temperature (°C)"
            type="number"
          />
          
          <BaseInput
            v-model="advancedFilters.maxTemperature"
            label="Max Temperature (°C)"
            type="number"
          />
          
          <BaseInput
            v-model="advancedFilters.minPressure"
            label="Min Pressure (PSI)"
            type="number"
          />
          
          <BaseInput
            v-model="advancedFilters.maxPressure"
            label="Max Pressure (PSI)"
            type="number"
          />
          
          <BaseInput
            v-model="advancedFilters.location"
            label="Location"
          />
          
          <BaseInput
            v-model="advancedFilters.manufacturer"
            label="Manufacturer"
          />
          
          <BaseInput
            v-model="advancedFilters.model"
            label="Model"
            class="full-width"
          />
        </div>
        
        <div class="filter-actions">
          <BaseButton variant="ghost" @click="clearSearch">
            <X class="button-icon" />
            Clear All
          </BaseButton>
          <BaseButton variant="primary" @click="performSearch" :disabled="loading">
            Apply Filters
          </BaseButton>
        </div>
      </div>
    </div>

    <!-- Main Content Area -->
    <div class="content-area">
      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <BaseSkeleton v-for="i in 8" :key="i" height="80px" class="result-skeleton" />
      </div>

      <!-- Search Results -->
      <div v-else-if="searchResults.length > 0" class="results-section">
        <div class="results-header">
          <h3>Search Results ({{ searchResults.length }})</h3>
          <BaseButton variant="outline" @click="showSaveModal = true">
            <Save class="button-icon" />
            Save Search
          </BaseButton>
        </div>
        
        <div class="results-list">
          <BaseCard
            v-for="result in searchResults"
            :key="result.id"
            class="result-item"
          >
            <div class="result-content">
              <div class="result-main">
                <div class="result-type-badge" :class="`type-${result.type}`">
                  {{ result.type }}
                </div>
                <div class="result-text">
                  <h4>{{ result.title }}</h4>
                  <p class="result-subtitle">{{ result.subtitle }}</p>
                </div>
              </div>
              
              <div class="result-meta">
                <span 
                  class="status-badge"
                  :class="getStatusColor(result.status)"
                >
                  {{ result.status }}
                </span>
                <span class="result-date">{{ formatDate(result.createdAt) }}</span>
                <span class="match-score">{{ Math.round(result.matchScore * 100) }}% match</span>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>

      <!-- Saved Searches -->
      <div v-else class="saved-searches-section">
        <div class="section-header">
          <h3>Saved Searches</h3>
          <p>Quickly access your frequently used searches</p>
        </div>
        
        <div class="saved-searches-grid">
          <BaseCard
            v-for="savedSearch in filteredSavedSearches"
            :key="savedSearch.id"
            class="saved-search-card"
          >
            <div class="saved-search-content">
              <div class="saved-search-info">
                <h4>{{ savedSearch.name }}</h4>
                <p class="search-query">"{{ savedSearch.query }}"</p>
                <div class="search-meta">
                  <span class="entity-type">{{ savedSearch.entityType }}</span>
                  <span class="last-used">Last used: {{ formatDate(savedSearch.lastUsed) }}</span>
                </div>
              </div>
              
              <div class="saved-search-actions">
                <BaseButton
                  variant="primary"
                  size="sm"
                  @click="loadSavedSearch(savedSearch)"
                >
                  <Search class="button-icon" />
                  Load
                </BaseButton>
                <BaseButton
                  variant="outline"
                  size="sm"
                  @click="deleteSavedSearch(savedSearch.id)"
                  class="delete-button"
                >
                  <X class="button-icon" />
                </BaseButton>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>
    </div>

    <!-- Save Search Modal -->
    <div 
      v-if="showSaveModal" 
      class="modal-overlay"
      @click="showSaveModal = false"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Save Current Search</h3>
        </template>
        
        <div class="modal-body">
          <BaseInput
            v-model="searchName"
            label="Search Name"
            placeholder="Enter a name for this search..."
            required
          />
          <p class="save-info">
            This will save your current search term and filters for quick access later.
          </p>
        </div>
        
        <div class="modal-actions">
          <BaseButton variant="ghost" @click="showSaveModal = false">
            Cancel
          </BaseButton>
          <BaseButton 
            variant="primary" 
            @click="saveSearch"
            :disabled="!searchName.trim()"
          >
            <Save class="button-icon" />
            Save Search
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.advanced-search {
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

.header-controls {
  display: flex;
  gap: 0.75rem;
}

.button-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
}

.spinner {
  width: 1rem;
  height: 1rem;
  border: 2px solid #e2e8f0;
  border-top: 2px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.search-interface {
  margin-bottom: 2rem;
}

.search-bar {
  display: flex;
  gap: 1rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.search-input-group {
  display: flex;
  flex: 1;
  min-width: 300px;
  gap: 0.5rem;
}

.main-search-input {
  flex: 1;
}

.search-icon {
  width: 1rem;
  height: 1rem;
  color: #94a3b8;
}

.filters-toggle {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.chevron {
  width: 1rem;
  height: 1rem;
  transition: transform 0.2s ease;
}

.chevron.rotated {
  transform: rotate(180deg);
}

.advanced-filters-panel {
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 1.5rem;
  margin-top: 1rem;
}

.filters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.full-width {
  grid-column: 1 / -1;
}

.filter-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.content-area {
  min-height: 400px;
}

.loading-container {
  padding: 2rem;
}

.result-skeleton {
  margin-bottom: 1rem;
}

.results-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.results-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.results-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
}

.results-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.result-item {
  padding: 1rem;
  transition: all 0.2s ease;
}

.result-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.result-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.result-main {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  flex: 1;
}

.result-type-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
}

.result-type-badge.type-machine {
  background: #dbeafe;
  color: #2563eb;
}

.result-type-badge.type-telemetry {
  background: #dcfce7;
  color: #16a34a;
}

.result-type-badge.type-maintenance {
  background: #ffedd5;
  color: #ea580c;
}

.result-type-badge.type-alert {
  background: #fee2e2;
  color: #dc2626;
}

.result-text h4 {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.result-subtitle {
  font-size: 0.875rem;
  color: #64748b;
  margin: 0;
}

.result-meta {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 0.75rem;
}

.status-badge {
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-weight: 500;
  text-transform: capitalize;
}

.result-date {
  color: #64748b;
}

.match-score {
  color: #8b5cf6;
  font-weight: 500;
}

.saved-searches-section {
  padding: 1rem 0;
}

.section-header {
  margin-bottom: 1.5rem;
}

.section-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.section-header p {
  color: #64748b;
}

.saved-searches-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
}

.saved-search-card {
  padding: 1rem;
}

.saved-search-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.saved-search-info h4 {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.search-query {
  font-size: 0.875rem;
  color: #64748b;
  font-style: italic;
  margin-bottom: 0.5rem;
}

.search-meta {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  font-size: 0.75rem;
  color: #64748b;
}

.entity-type {
  text-transform: capitalize;
  font-weight: 500;
}

.saved-search-actions {
  display: flex;
  gap: 0.5rem;
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
  max-width: 500px;
}

.modal-body {
  padding: 1rem 0;
}

.save-info {
  font-size: 0.875rem;
  color: #64748b;
  margin-top: 1rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .advanced-search {
    padding: 0.5rem;
  }
  
  .search-bar {
    flex-direction: column;
  }
  
  .search-input-group {
    min-width: auto;
  }
  
  .filters-grid {
    grid-template-columns: 1fr;
  }
  
  .result-content {
    flex-direction: column;
    gap: 1rem;
  }
  
  .result-meta {
    justify-content: space-between;
  }
  
  .saved-searches-grid {
    grid-template-columns: 1fr;
  }
  
  .saved-search-content {
    flex-direction: column;
    gap: 1rem;
  }
}
</style>