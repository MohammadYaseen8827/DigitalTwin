<template>
  <main class="prediction-history-page container">
    <div class="page-header glass-panel">
      <div>
        <p class="eyebrow">Historical Analysis</p>
        <h1>Prediction History</h1>
        <p class="subtitle">
          Review historical predictions, track model performance, and analyze prediction accuracy over time
        </p>
      </div>
      <div class="header-actions">
        <div class="machine-selector">
          <label for="machine-select">Select Machine:</label>
          <select 
            id="machine-select"
            v-model="selectedMachineId" 
            @change="loadHistory"
            class="machine-dropdown"
          >
            <option value="">All Machines</option>
            <option v-for="machine in machines" :key="machine.id" :value="machine.id">
              {{ machine.name }} ({{ machine.type }})
            </option>
          </select>
        </div>
        <BaseButton 
          size="md" 
          variant="primary" 
          :disabled="loading" 
          @click="loadHistory"
          :loading="loading"
        >
          <template #icon>
            <RefreshCw class="h-4 w-4" />
          </template>
          Refresh History
        </BaseButton>
      </div>
    </div>

    <div v-if="loading" class="loading-state glass-panel">
      <div class="spinner"></div>
      <p>Loading prediction history...</p>
    </div>

    <div v-else-if="error" class="error-state glass-panel">
      <AlertCircle class="error-icon" />
      <div>
        <h3>Failed to Load History</h3>
        <p>{{ error }}</p>
      </div>
      <BaseButton size="sm" variant="outline" @click="loadHistory">
        Try Again
      </BaseButton>
    </div>

    <template v-else>
      <!-- Summary Cards -->
      <div class="summary-grid">
        <div class="summary-card glass-panel">
          <div class="card-icon">
            <BarChart3 class="icon" />
          </div>
          <div class="card-content">
            <div class="card-value">{{ totalPredictions }}</div>
            <div class="card-label">Total Predictions</div>
          </div>
        </div>
        
        <div class="summary-card glass-panel">
          <div class="card-icon success">
            <CheckCircle class="icon" />
          </div>
          <div class="card-content">
            <div class="card-value">{{ accuratePredictions }}</div>
            <div class="card-label">Accurate Predictions</div>
          </div>
        </div>
        
        <div class="summary-card glass-panel">
          <div class="card-icon warning">
            <Clock class="icon" />
          </div>
          <div class="card-content">
            <div class="card-value">{{ averageAge }}d</div>
            <div class="card-label">Average Age</div>
          </div>
        </div>
        
        <div class="summary-card glass-panel">
          <div class="card-icon info">
            <TrendingUp class="icon" />
          </div>
          <div class="card-content">
            <div class="card-value">{{ accuracyRate }}%</div>
            <div class="card-label">Accuracy Rate</div>
          </div>
        </div>
      </div>

      <!-- History Table -->
      <div class="history-section glass-panel">
        <div class="section-header">
          <h2>Prediction Records</h2>
          <div class="table-controls">
            <div class="filter-controls">
              <label>Show:</label>
              <select v-model="itemsPerPage" class="items-per-page">
                <option :value="10">10 items</option>
                <option :value="25">25 items</option>
                <option :value="50">50 items</option>
                <option :value="100">100 items</option>
              </select>
            </div>
            <div class="search-box">
              <Search class="search-icon" />
              <input 
                v-model="searchQuery" 
                type="text" 
                placeholder="Search predictions..."
                class="search-input"
              >
            </div>
          </div>
        </div>

        <div class="table-container">
          <table class="history-table">
            <thead>
              <tr>
                <th @click="sortBy('createdAt')" class="sortable">
                  Date
                  <ChevronUp v-if="sortField === 'createdAt' && sortDirection === 'asc'" class="sort-icon" />
                  <ChevronDown v-else class="sort-icon" />
                </th>
                <th>Machine</th>
                <th>Predicted RUL</th>
                <th>Actual RUL</th>
                <th>Confidence</th>
                <th>Status</th>
                <th>Model</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="prediction in paginatedPredictions" :key="prediction.id" class="prediction-row">
                <td>{{ formatDate(prediction.createdAt) }}</td>
                <td>
                  <div class="machine-cell">
                    <div class="machine-name">{{ getMachineName(prediction.machineId) }}</div>
                    <div class="machine-id">{{ prediction.machineId }}</div>
                  </div>
                </td>
                <td>
                  <div class="rul-cell">
                    <span class="predicted-value">{{ Math.round(prediction.remainingUsefulLifeDays) }} days</span>
                    <div v-if="prediction.rulLowerBound !== undefined && prediction.rulUpperBound !== undefined" class="confidence-range">
                      ({{ Math.round(prediction.rulLowerBound) }} - {{ Math.round(prediction.rulUpperBound) }})
                    </div>
                  </div>
                </td>
                <td>
                  <span v-if="prediction.actualRul !== undefined" class="actual-value">
                    {{ Math.round(prediction.actualRul) }} days
                  </span>
                  <span v-else class="no-data">N/A</span>
                </td>
                <td>
                  <div class="confidence-cell">
                    <div class="confidence-bar">
                      <div 
                        class="confidence-fill" 
                        :style="{ width: `${getConfidence(prediction)}%` }"
                        :class="getConfidenceClass(getConfidence(prediction))"
                      ></div>
                    </div>
                    <span class="confidence-percent">{{ getConfidence(prediction) }}%</span>
                  </div>
                </td>
                <td>
                  <span class="status-badge" :class="getStatusClass(prediction.healthStatus)">
                    {{ prediction.healthStatus }}
                  </span>
                </td>
                <td>
                  <span class="model-version">v{{ prediction.modelVersion }}</span>
                </td>
                <td>
                  <div class="action-buttons">
                    <BaseButton 
                      size="sm" 
                      variant="ghost" 
                      @click="viewDetails(prediction)"
                      title="View Details"
                    >
                      <Eye class="h-4 w-4" />
                    </BaseButton>
                    <BaseButton 
                      size="sm" 
                      variant="ghost" 
                      @click="compareWithOthers(prediction)"
                      title="Compare"
                    >
                      <GitCompare class="h-4 w-4" />
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
            Showing {{ startIndex + 1 }} to {{ Math.min(startIndex + itemsPerPage, filteredPredictions.length) }} 
            of {{ filteredPredictions.length }} predictions
          </div>
          <div class="pagination-controls">
            <BaseButton 
              size="sm" 
              variant="outline" 
              :disabled="currentPage === 1"
              @click="currentPage--"
            >
              Previous
            </BaseButton>
            <span class="page-info">Page {{ currentPage }} of {{ totalPages }}</span>
            <BaseButton 
              size="sm" 
              variant="outline" 
              :disabled="currentPage === totalPages"
              @click="currentPage++"
            >
              Next
            </BaseButton>
          </div>
        </div>
      </div>
    </template>
    
    <!-- Modal -->
    <div v-if="selectedPrediction" class="modal-overlay" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Prediction Details</h3>
          <button class="close-button" @click="closeModal">
            <X class="h-5 w-5" />
          </button>
        </div>
        <div class="modal-body">
          <PredictionDetail :prediction="selectedPrediction" />
        </div>
      </div>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useMachinesStore } from '@/stores/machines.store'
import { fetchPredictionHistory } from '@/services/predictions.service'
import type { PredictionDto } from '@/api/types'
import { 
  RefreshCw, 
  AlertCircle, 
  BarChart3, 
  CheckCircle, 
  Clock, 
  TrendingUp,
  Search,
  ChevronUp,
  ChevronDown,
  Eye,
  GitCompare,
  Activity,
  PieChart,
  X
} from 'lucide-vue-next'

const machinesStore = useMachinesStore()
const loading = ref(false)
const error = ref<string | null>(null)
const predictions = ref<PredictionDto[]>([])
const selectedMachineId = ref('')
const searchQuery = ref('')
const currentPage = ref(1)
const itemsPerPage = ref(25)
const sortField = ref('createdAt')
const sortDirection = ref<'asc' | 'desc'>('desc')
const selectedPrediction = ref<PredictionDto | null>(null)

const machines = computed(() => machinesStore.machines)

// Initialize machines if needed
if (machines.value.length === 0) {
  machinesStore.fetchMachines()
}

const filteredPredictions = computed(() => {
  let result = predictions.value
  
  // Filter by machine
  if (selectedMachineId.value) {
    result = result.filter(p => p.machineId === selectedMachineId.value)
  }
  
  // Filter by search query
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(p => 
      p.machineId.toLowerCase().includes(query) ||
      p.healthStatus?.toLowerCase().includes(query) ||
      p.modelVersion.includes(query)
    )
  }
  
  // Sort
  result.sort((a, b) => {
    const aVal = a[sortField.value as keyof PredictionDto]
    const bVal = b[sortField.value as keyof PredictionDto]
    
    if (aVal < bVal) return sortDirection.value === 'asc' ? -1 : 1
    if (aVal > bVal) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
  
  return result
})

const paginatedPredictions = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  return filteredPredictions.value.slice(start, start + itemsPerPage.value)
})

const totalPages = computed(() => 
  Math.ceil(filteredPredictions.value.length / itemsPerPage.value)
)

const startIndex = computed(() => 
  (currentPage.value - 1) * itemsPerPage.value
)

// Summary calculations
const totalPredictions = computed(() => predictions.value.length)
const accuratePredictions = computed(() => 
  predictions.value.filter(p => p.accuracyScore && p.accuracyScore > 0.8).length
)
const averageAge = computed(() => {
  if (predictions.value.length === 0) return 0
  const totalAge = predictions.value.reduce((sum, p) => {
    const age = Math.floor((Date.now() - new Date(p.createdAt).getTime()) / (1000 * 60 * 60 * 24))
    return sum + age
  }, 0)
  return Math.round(totalAge / predictions.value.length)
})
const accuracyRate = computed(() => 
  totalPredictions.value > 0 
    ? Math.round((accuratePredictions.value / totalPredictions.value) * 100)
    : 0
)

const machineTypeDistribution = computed(() => {
  const distribution: Record<string, number> = {}
  predictions.value.forEach(p => {
    const machine = machines.value.find(m => m.id === p.machineId)
    if (machine) {
      distribution[machine.type] = (distribution[machine.type] || 0) + 1
    }
  })
  return distribution
})

const getMachineName = (machineId: string) => {
  const machine = machines.value.find(m => m.id === machineId)
  return machine ? machine.name : machineId
}

const getConfidence = (prediction: PredictionDto) => {
  if (prediction.rulLowerBound !== undefined && prediction.rulUpperBound !== undefined) {
    const range = prediction.rulUpperBound - prediction.rulLowerBound
    return Math.max(0, 100 - (range / prediction.remainingUsefulLifeDays) * 50)
  }
  return 75 // Default confidence
}

const getConfidenceClass = (confidence: number) => {
  if (confidence >= 80) return 'high'
  if (confidence >= 60) return 'medium'
  return 'low'
}

const getStatusClass = (status: string | undefined) => {
  if (!status) return 'unknown'
  const s = status.toLowerCase()
  if (s.includes('healthy')) return 'healthy'
  if (s.includes('degraded')) return 'degraded'
  if (s.includes('critical')) return 'critical'
  return 'unknown'
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const sortBy = (field: string) => {
  if (sortField.value === field) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortField.value = field
    sortDirection.value = 'desc'
  }
}

const loadHistory = async () => {
  loading.value = true
  error.value = null
  
  try {
    const machineId = selectedMachineId.value || ''
    const history = await fetchPredictionHistory(machineId, 1000) // Get more for filtering
    predictions.value = history
    currentPage.value = 1
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Failed to load prediction history'
    console.error('History load error:', err)
  } finally {
    loading.value = false
  }
}

const viewDetails = (prediction: PredictionDto) => {
  selectedPrediction.value = prediction
}

const compareWithOthers = (prediction: PredictionDto) => {
  // Would open comparison view
  console.log('Comparing prediction:', prediction)
}

const closeModal = () => {
  selectedPrediction.value = null
}

// Load initial data
loadHistory()
</script>

<style scoped>
.prediction-history-page {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  padding: 2rem;
}

.eyebrow {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 0.5rem;
}

.page-header h1 {
  margin: 0 0 0.5rem 0;
  color: var(--color-text-primary);
}

.subtitle {
  color: var(--color-text-secondary);
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.machine-selector {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.machine-selector label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
}

.machine-dropdown {
  padding: 0.5rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  background: var(--color-background);
  color: var(--color-text-primary);
  min-width: 200px;
}

.loading-state,
.error-state {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 3rem;
  text-align: center;
}

.error-state {
  flex-direction: column;
}

.error-icon {
  width: 3rem;
  height: 3rem;
  color: var(--color-error);
}

.spinner {
  width: 2rem;
  height: 2rem;
  border: 2px solid var(--color-border);
  border-top: 2px solid var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.summary-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1.5rem;
}

.card-icon {
  width: 3rem;
  height: 3rem;
  border-radius: 0.75rem;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-primary);
  color: white;
}

.card-icon.success {
  background: var(--color-success);
}

.card-icon.warning {
  background: var(--color-warning);
}

.card-icon.info {
  background: var(--color-info);
}

.card-content {
  flex: 1;
}

.card-value {
  font-size: 1.875rem;
  font-weight: 700;
  line-height: 1;
  margin-bottom: 0.25rem;
  color: var(--color-text-primary);
}

.card-label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.history-section {
  margin-bottom: 2rem;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid var(--color-border);
}

.table-controls {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.filter-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.items-per-page {
  padding: 0.375rem 0.75rem;
  border: 1px solid var(--color-border);
  border-radius: 0.375rem;
  background: var(--color-background);
  color: var(--color-text-primary);
}

.search-box {
  display: flex;
  align-items: center;
  position: relative;
}

.search-icon {
  position: absolute;
  left: 0.75rem;
  width: 1rem;
  height: 1rem;
  color: var(--color-text-secondary);
}

.search-input {
  padding: 0.375rem 0.75rem 0.375rem 2.5rem;
  border: 1px solid var(--color-border);
  border-radius: 0.375rem;
  background: var(--color-background);
  color: var(--color-text-primary);
  width: 200px;
}

.table-container {
  overflow-x: auto;
}

.history-table {
  width: 100%;
  border-collapse: collapse;
}

.history-table th {
  text-align: left;
  padding: 0.75rem;
  border-bottom: 2px solid var(--color-border);
  font-weight: 600;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.history-table th.sortable {
  cursor: pointer;
  user-select: none;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.sort-icon {
  width: 1rem;
  height: 1rem;
}

.history-table td {
  padding: 0.75rem;
  border-bottom: 1px solid var(--color-border);
}

.prediction-row:hover {
  background: var(--color-background-secondary);
}

.machine-cell {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.machine-name {
  font-weight: 500;
  color: var(--color-text-primary);
}

.machine-id {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.rul-cell {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.predicted-value {
  font-weight: 600;
  color: var(--color-text-primary);
}

.confidence-range {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.actual-value {
  color: var(--color-success);
  font-weight: 500;
}

.no-data {
  color: var(--color-text-secondary);
  font-style: italic;
}

.confidence-cell {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.confidence-bar {
  width: 60px;
  height: 8px;
  background: var(--color-border);
  border-radius: 4px;
  overflow: hidden;
}

.confidence-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.confidence-fill.high {
  background: var(--color-success);
}

.confidence-fill.medium {
  background: var(--color-warning);
}

.confidence-fill.low {
  background: var(--color-error);
}

.confidence-percent {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-primary);
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.status-badge.healthy {
  background: var(--color-success);
  color: white;
}

.status-badge.degraded {
  background: var(--color-warning);
  color: white;
}

.status-badge.critical {
  background: var(--color-error);
  color: white;
}

.status-badge.unknown {
  background: var(--color-text-secondary);
  color: white;
}

.model-version {
  font-family: monospace;
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.action-buttons {
  display: flex;
  gap: 0.5rem;
}

.pagination {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid var(--color-border);
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.page-info {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.pagination-info {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: var(--color-background);
  border-radius: 0.75rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.modal-header h3 {
  margin: 0;
  color: var(--color-text-primary);
}

.close-button {
  background: none;
  border: none;
  padding: 0.5rem;
  border-radius: 0.375rem;
  cursor: pointer;
  color: var(--color-text-secondary);
  transition: background-color 0.2s ease;
}

.close-button:hover {
  background: var(--color-background-secondary);
}

.modal-body {
  padding: 1.5rem;
}

/* Responsive Design */
@media (max-width: 1024px) {
  .page-header {
    flex-direction: column;
    gap: 1.5rem;
    align-items: stretch;
  }
  
  .header-actions {
    flex-direction: column;
    align-items: stretch;
  }
  
  .table-controls {
    flex-direction: column;
    align-items: stretch;
    gap: 1rem;
  }
  
  .search-input {
    width: 100%;
  }
}

@media (max-width: 768px) {
  .prediction-history-page {
    padding: 1rem;
  }
  
  .summary-grid {
    grid-template-columns: 1fr;
  }
  
  .pagination {
    flex-direction: column;
    gap: 1rem;
    align-items: center;
  }
  
  .table-container {
    font-size: 0.875rem;
  }
  
  .history-table th,
  .history-table td {
    padding: 0.5rem;
  }
  
  .action-buttons {
    flex-direction: column;
  }
}
</style>
