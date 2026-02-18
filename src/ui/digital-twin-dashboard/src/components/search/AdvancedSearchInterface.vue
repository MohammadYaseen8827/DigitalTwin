<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import { fetchActiveAlerts } from '@/services/alerts.service'
import { fetchActiveMaintenance, fetchMaintenanceHistory } from '@/services/maintenance.service'
import { fetchProductionLines } from '@/services/productionLines.service'
import { fetchRecentTelemetry } from '@/services/telemetry.service'
import { 
  Search, 
  Filter,
  RefreshCw,
  Database,
  Server,
  AlertTriangle,
  Wrench,
  Factory,
  Activity,
  Calendar,
  MapPin,
  Hash,
  Clock,
  User
} from 'lucide-vue-next'

const toast = useToast()

// State
const searchResults: any = ref([])
const loading = ref(false)
const searchTerm = ref('')
const searchEntity = ref('all')
const dateRange = ref('all')
const sortBy = ref('relevance')

// Search state
const searchPerformed = ref(false)
const totalResults = ref(0)

// Entity data caches
const machinesCache: any = ref([])
const alertsCache: any = ref([])
const maintenanceCache: any = ref([])
const productionLinesCache: any = ref([])
const telemetryCache: any = ref([])

// Computed
const entityOptions = [
  { label: 'All Entities', value: 'all' },
  { label: 'Machines', value: 'machines' },
  { label: 'Alerts', value: 'alerts' },
  { label: 'Maintenance Records', value: 'maintenance' },
  { label: 'Production Lines', value: 'productionLines' },
  { label: 'Telemetry Data', value: 'telemetry' }
]

const dateRangeOptions = [
  { label: 'Any Time', value: 'all' },
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' },
  { label: 'Last 30 Days', value: '30d' },
  { label: 'Last 90 Days', value: '90d' }
]

const sortOptions = [
  { label: 'Relevance', value: 'relevance' },
  { label: 'Date (Newest)', value: 'date_desc' },
  { label: 'Date (Oldest)', value: 'date_asc' },
  { label: 'Name (A-Z)', value: 'name_asc' },
  { label: 'Name (Z-A)', value: 'name_desc' }
]

const filteredResults = computed(() => {
  let results = [...searchResults.value]
  
  // Apply date range filter
  if (dateRange.value !== 'all') {
    const cutoffDate = getDateCutoff(dateRange.value)
    results = results.filter((item: any) => {
      const itemDate = new Date(item.timestamp || item.createdAt || item.date || new Date())
      return itemDate >= cutoffDate
    })
  }
  
  // Apply sorting
  results = sortResults(results, sortBy.value)
  
  return results
})

const entityCounts = computed(() => {
  const counts: Record<string, number> = {
    machines: 0,
    alerts: 0,
    maintenance: 0,
    productionLines: 0,
    telemetry: 0
  }
  
  searchResults.value.forEach((item: any) => {
    counts[item.entityType]++
  })
  
  return counts
})

// Methods
const loadData = async () => {
  try {
    loading.value = true
    
    // Load all entity data in parallel
    const [machines, alerts, maintenance, productionLines, telemetry] = await Promise.all([
      fetchMachines().catch(() => []),
      fetchActiveAlerts('').catch(() => []),
      Promise.all([
        fetchActiveMaintenance().catch(() => []),
        fetchMaintenanceHistory('').catch(() => [])
      ]).then(([active, history]) => [...active, ...history]),
      fetchProductionLines().catch(() => []),
      fetchRecentTelemetry({ limit: 1000 }).catch(() => [])
    ])
    
    machinesCache.value = machines
    alertsCache.value = alerts
    maintenanceCache.value = maintenance
    productionLinesCache.value = productionLines
    telemetryCache.value = telemetry
    
    toast.success('Search data loaded successfully')
  } catch (error) {
    console.error('Failed to load search data:', error)
    toast.error('Failed to load search data')
  } finally {
    loading.value = false
  }
}

const performSearch = async () => {
  if (!searchTerm.value.trim()) {
    toast.warning('Please enter a search term')
    return
  }
  
  try {
    loading.value = true
    searchPerformed.value = true
    
    const query = searchTerm.value.toLowerCase().trim()
    const results: any[] = []
    
    // Search across selected entities
    if (searchEntity.value === 'all' || searchEntity.value === 'machines') {
      const machineResults = searchMachines(query)
      results.push(...machineResults)
    }
    
    if (searchEntity.value === 'all' || searchEntity.value === 'alerts') {
      const alertResults = searchAlerts(query)
      results.push(...alertResults)
    }
    
    if (searchEntity.value === 'all' || searchEntity.value === 'maintenance') {
      const maintenanceResults = searchMaintenance(query)
      results.push(...maintenanceResults)
    }
    
    if (searchEntity.value === 'all' || searchEntity.value === 'productionLines') {
      const productionLineResults = searchProductionLines(query)
      results.push(...productionLineResults)
    }
    
    if (searchEntity.value === 'all' || searchEntity.value === 'telemetry') {
      const telemetryResults = searchTelemetry(query)
      results.push(...telemetryResults)
    }
    
    searchResults.value = results
    totalResults.value = results.length
    
    toast.success(`Found ${results.length} results`)
  } catch (error) {
    console.error('Search failed:', error)
    toast.error('Search failed')
  } finally {
    loading.value = false
  }
}

const searchMachines = (query: string) => {
  return machinesCache.value
    .filter((machine: any) => 
      machine.name?.toLowerCase().includes(query) ||
      machine.type?.toLowerCase().includes(query) ||
      machine.location?.toLowerCase().includes(query) ||
      machine.id?.toLowerCase().includes(query) ||
      machine.serialNumber?.toLowerCase().includes(query)
    )
    .map((machine: any) => ({
      ...machine,
      entityType: 'machines',
      displayName: machine.name,
      displayType: 'Machine',
      icon: Server,
      timestamp: machine.lastUpdated || new Date().toISOString(),
      snippet: `Type: ${machine.type}, Location: ${machine.location}, Status: ${machine.status}`
    }))
}

const searchAlerts = (query: string) => {
  return alertsCache.value
    .filter((alert: any) => 
      alert.message?.toLowerCase().includes(query) ||
      alert.machineId?.toLowerCase().includes(query) ||
      alert.severity?.toLowerCase().includes(query)
    )
    .map((alert: any) => ({
      ...alert,
      entityType: 'alerts',
      displayName: `Alert: ${alert.message?.substring(0, 50)}${alert.message?.length > 50 ? '...' : ''}`,
      displayType: 'Alert',
      icon: AlertTriangle,
      timestamp: alert.createdAt,
      snippet: `Machine: ${alert.machineId}, Severity: ${alert.severity}, Status: ${alert.isAcknowledged ? 'Acknowledged' : 'Active'}`
    }))
}

const searchMaintenance = (query: string) => {
  return maintenanceCache.value
    .filter((record: any) => 
      record.type?.toLowerCase().includes(query) ||
      record.machineId?.toLowerCase().includes(query) ||
      record.notes?.toLowerCase().includes(query) ||
      record.performedBy?.toLowerCase().includes(query)
    )
    .map((record: any) => ({
      ...record,
      entityType: 'maintenance',
      displayName: `Maintenance: ${record.type}`,
      displayType: 'Maintenance',
      icon: Wrench,
      timestamp: record.date || record.plannedDate || record.completionDate,
      snippet: `Machine: ${record.machineId}, Status: ${record.status}, Performed by: ${record.performedBy}`
    }))
}

const searchProductionLines = (query: string) => {
  return productionLinesCache.value
    .filter((line: any) => 
      line.name?.toLowerCase().includes(query) ||
      line.id?.toLowerCase().includes(query) ||
      JSON.stringify(line.configuration || {}).toLowerCase().includes(query)
    )
    .map((line: any) => ({
      ...line,
      entityType: 'productionLines',
      displayName: line.name,
      displayType: 'Production Line',
      icon: Factory,
      timestamp: new Date().toISOString(),
      snippet: `Machines: ${line.machineIds?.length || 0}, Configuration entries: ${Object.keys(line.configuration || {}).length}`
    }))
}

const searchTelemetry = (query: string) => {
  return telemetryCache.value
    .filter((data: any) => 
      data.machineId?.toLowerCase().includes(query) ||
      data.dataType?.toLowerCase().includes(query) ||
      JSON.stringify(data.data || {}).toLowerCase().includes(query)
    )
    .map((data: any) => ({
      ...data,
      entityType: 'telemetry',
      displayName: `Telemetry: ${data.dataType}`,
      displayType: 'Telemetry Data',
      icon: Activity,
      timestamp: data.timestamp,
      snippet: `Machine: ${data.machineId}, Data points: ${Object.keys(data.data || {}).length}`
    }))
}

const getDateCutoff = (range: string): Date => {
  const now = new Date()
  switch (range) {
    case '24h':
      return new Date(now.getTime() - 24 * 60 * 60 * 1000)
    case '7d':
      return new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000)
    case '30d':
      return new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000)
    case '90d':
      return new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000)
    default:
      return new Date(0)
  }
}

const sortResults = (results: any[], sortType: string): any[] => {
  return [...results].sort((a, b) => {
    switch (sortType) {
      case 'date_desc':
        return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
      case 'date_asc':
        return new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()
      case 'name_asc':
        return (a.displayName || '').localeCompare(b.displayName || '')
      case 'name_desc':
        return (b.displayName || '').localeCompare(a.displayName || '')
      case 'relevance':
      default:
        // For relevance, we could implement more sophisticated scoring
        // For now, sort by date descending
        return new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
    }
  })
}

const clearSearch = () => {
  searchTerm.value = ''
  searchResults.value = []
  searchPerformed.value = false
  totalResults.value = 0
}

const refreshData = async () => {
  await loadData()
  if (searchTerm.value.trim()) {
    await performSearch()
  }
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleString()
}

const getEntityColor = (entityType: string): string => {
  const colors: Record<string, string> = {
    machines: 'bg-blue-100 text-blue-800',
    alerts: 'bg-red-100 text-red-800',
    maintenance: 'bg-yellow-100 text-yellow-800',
    productionLines: 'bg-purple-100 text-purple-800',
    telemetry: 'bg-green-100 text-green-800'
  }
  return colors[entityType] || 'bg-gray-100 text-gray-800'
}

const getEntityIcon = (entityType: string) => {
  const icons: Record<string, any> = {
    machines: Server,
    alerts: AlertTriangle,
    maintenance: Wrench,
    productionLines: Factory,
    telemetry: Activity
  }
  return icons[entityType] || Database
}

// Lifecycle
onMounted(async () => {
  await loadData()
})
</script>

<template>
  <BaseCard>
    <div class="advanced-search space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Advanced Search</h1>
          <p class="text-gray-600 mt-2">Search across all system entities and data</p>
        </div>
        <BaseButton variant="outline" @click="refreshData" :disabled="loading">
          <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
          Refresh Data
        </BaseButton>
      </div>

      <!-- Search Controls -->
      <BaseCard>
        <div class="p-6 space-y-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <div class="relative">
                <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                <BaseInput
                  v-model="searchTerm"
                  placeholder="Enter search terms..."
                  class="pl-10 text-lg"
                  @keyup.enter="performSearch"
                />
              </div>
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="performSearch"
              :disabled="loading || !searchTerm.trim()"
              class="self-end sm:self-auto"
            >
              <Search class="w-4 h-4 mr-2" />
              Search
            </BaseButton>
            
            <BaseButton 
              variant="outline" 
              @click="clearSearch"
              :disabled="!searchPerformed"
              class="self-end sm:self-auto"
            >
              Clear
            </BaseButton>
          </div>
          
          <div class="flex flex-col sm:flex-row gap-4">
            <BaseSelect
              v-model="searchEntity"
              :options="entityOptions"
              label="Search In"
              class="flex-1"
            />
            
            <BaseSelect
              v-model="dateRange"
              :options="dateRangeOptions"
              label="Date Range"
              class="flex-1"
            />
            
            <BaseSelect
              v-model="sortBy"
              :options="sortOptions"
              label="Sort By"
              class="flex-1"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Results Summary -->
      <div v-if="searchPerformed" class="grid grid-cols-2 md:grid-cols-5 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <Database class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-sm text-gray-600">Total Results</p>
            <p class="text-2xl font-bold">{{ totalResults }}</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Server class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-sm text-gray-600">Machines</p>
            <p class="text-2xl font-bold">{{ entityCounts.machines }}</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <AlertTriangle class="w-8 h-8 text-red-500 mx-auto mb-2" />
            <p class="text-sm text-gray-600">Alerts</p>
            <p class="text-2xl font-bold">{{ entityCounts.alerts }}</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Wrench class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
            <p class="text-sm text-gray-600">Maintenance</p>
            <p class="text-2xl font-bold">{{ entityCounts.maintenance }}</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Activity class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-sm text-gray-600">Telemetry</p>
            <p class="text-2xl font-bold">{{ entityCounts.telemetry }}</p>
          </div>
        </BaseCard>
      </div>

      <!-- Search Results -->
      <BaseCard v-if="searchPerformed">
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">
              Search Results ({{ filteredResults.length }})
            </h2>
            <div class="text-sm text-gray-600">
              Sorted by: {{ sortOptions.find(o => o.value === sortBy)?.label }}
            </div>
          </div>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Searching...</p>
          </div>
          
          <div v-else-if="filteredResults.length === 0" class="text-center py-8">
            <Search class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No results found</p>
            <p class="text-sm text-gray-500 mt-1">Try different search terms or broaden your search</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="(result, index) in filteredResults"
              :key="`${result.entityType}-${result.id || index}`"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex items-start gap-4">
                  <component 
                    :is="getEntityIcon(result.entityType)"
                    class="w-6 h-6 mt-1 flex-shrink-0"
                    :class="getEntityColor(result.entityType).replace('bg-', 'text-').replace(' text-', ' text-')"
                  />
                  
                  <div class="flex-1 min-w-0">
                    <div class="flex items-center gap-2 mb-2">
                      <h3 class="text-lg font-semibold text-gray-900 truncate">
                        {{ result.displayName }}
                      </h3>
                      <span 
                        class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium"
                        :class="getEntityColor(result.entityType)"
                      >
                        {{ result.displayType }}
                      </span>
                    </div>
                    
                    <p class="text-gray-600 mb-3">
                      {{ result.snippet }}
                    </p>
                    
                    <div class="flex flex-wrap gap-4 text-sm text-gray-500">
                      <div class="flex items-center gap-1">
                        <Calendar class="w-4 h-4" />
                        <span>{{ formatTime(result.timestamp) }}</span>
                      </div>
                      
                      <div v-if="result.machineId" class="flex items-center gap-1">
                        <Hash class="w-4 h-4" />
                        <span>Machine: {{ result.machineId }}</span>
                      </div>
                      
                      <div v-if="result.location" class="flex items-center gap-1">
                        <MapPin class="w-4 h-4" />
                        <span>{{ result.location }}</span>
                      </div>
                      
                      <div v-if="result.performedBy" class="flex items-center gap-1">
                        <User class="w-4 h-4" />
                        <span>By: {{ result.performedBy }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>

      <!-- Welcome Message -->
      <div v-else class="text-center py-12">
        <Search class="w-16 h-16 text-gray-400 mx-auto mb-4" />
        <h3 class="text-xl font-semibold text-gray-900 mb-2">Search Across All System Data</h3>
        <p class="text-gray-600 max-w-2xl mx-auto">
          Find machines, alerts, maintenance records, production lines, and telemetry data all in one place. 
          Enter search terms above to get started.
        </p>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.advanced-search {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .advanced-search {
    padding: 0.5rem;
  }
}
</style>