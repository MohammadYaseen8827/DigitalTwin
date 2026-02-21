<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { 
  Database, 
  Zap, 
  Play,
  Pause,
  Settings,
  Download,
  Filter,
  Search,
  Plus,
  Trash2,
  Activity
} from 'lucide-vue-next'

const toast = useToast()

// State
const dataGenerators = ref<any[]>([
  {
    id: 'GEN001',
    name: 'Telemetry Data Generator',
    type: 'telemetry',
    status: 'active',
    recordsPerSecond: 10,
    totalGenerated: 125000,
    uptime: '2h 15m',
    lastGenerated: new Date().toISOString(),
    config: {
      sensors: ['temperature', 'pressure', 'vibration'],
      noiseLevel: 0.1,
      trend: 'stable'
    }
  },
  {
    id: 'GEN002',
    name: 'Maintenance Event Simulator',
    type: 'maintenance',
    status: 'paused',
    recordsPerSecond: 2,
    totalGenerated: 8500,
    uptime: '0h 0m',
    lastGenerated: new Date(Date.now() - 3600000).toISOString(),
    config: {
      eventTypes: ['preventive', 'corrective', 'emergency'],
      frequency: 'weekly'
    }
  },
  {
    id: 'GEN003',
    name: 'Production Line Simulator',
    type: 'production',
    status: 'active',
    recordsPerSecond: 5,
    totalGenerated: 45000,
    uptime: '1h 30m',
    lastGenerated: new Date().toISOString(),
    config: {
      lines: 3,
      efficiencyRange: [85, 95],
      downtimeProbability: 0.05
    }
  }
])

const datasets = ref<any[]>([
  {
    id: 'DS001',
    name: 'Historical Telemetry Dataset',
    type: 'telemetry',
    size: '2.3 GB',
    recordCount: 1250000,
    generatedAt: new Date(Date.now() - 86400000).toISOString(),
    format: 'parquet',
    description: '6 months of sensor data from manufacturing equipment'
  },
  {
    id: 'DS002',
    name: 'Maintenance Log Dataset',
    type: 'maintenance',
    size: '156 MB',
    recordCount: 85000,
    generatedAt: new Date(Date.now() - 172800000).toISOString(),
    format: 'csv',
    description: '2 years of maintenance records and work orders'
  }
])

const loading = ref(false)
const searchQuery = ref('')
const typeFilter = ref('all')
const statusFilter = ref('all')
const showCreateModal = ref(false)
const showConfigureModal = ref(false)
const selectedGenerator = ref<any>(null)

// Form state
const generatorForm = ref({
  name: '',
  type: 'telemetry',
  recordsPerSecond: 10,
  duration: 3600, // seconds
  config: {
    sensors: ['temperature'],
    noiseLevel: 0.1,
    trend: 'stable'
  }
})

// Computed
const filteredGenerators = computed(() => {
  let filtered = [...dataGenerators.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(gen => 
      gen.name.toLowerCase().includes(query) ||
      gen.type.toLowerCase().includes(query)
    )
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter(gen => gen.type === typeFilter.value)
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(gen => gen.status === statusFilter.value)
  }
  
  return filtered
})

// Computed property for sensor string conversion
const sensorString = computed({
  get: () => generatorForm.value.config.sensors.join(','),
  set: (value: string) => {
    generatorForm.value.config.sensors = value.split(',').map(s => s.trim()).filter(s => s)
  }
})

const typeOptions = [
  { label: 'All Types', value: 'all' },
  { label: 'Telemetry', value: 'telemetry' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Production', value: 'production' },
  { label: 'Alerts', value: 'alerts' },
  { label: 'Quality', value: 'quality' }
]

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Active', value: 'active' },
  { label: 'Paused', value: 'paused' },
  { label: 'Stopped', value: 'stopped' }
]

// Methods
const loadData = async () => {
  try {
    loading.value = true
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Data generators loaded successfully')
  } catch (error) {
    console.error('Error loading data:', error)
    toast.error('Failed to load data generators')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openConfigureModal = (generator: any) => {
  selectedGenerator.value = generator
  generatorForm.value = {
    name: generator.name,
    type: generator.type,
    recordsPerSecond: generator.recordsPerSecond,
    duration: 3600,
    config: { ...generator.config }
  }
  showConfigureModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showConfigureModal.value = false
  selectedGenerator.value = null
  resetForm()
}

const resetForm = () => {
  generatorForm.value = {
    name: '',
    type: 'telemetry',
    recordsPerSecond: 10,
    duration: 3600,
    config: {
      sensors: ['temperature'],
      noiseLevel: 0.1,
      trend: 'stable'
    }
  }
}

const handleSubmit = async () => {
  try {
    if (selectedGenerator.value) {
      // Update existing generator
      const index = dataGenerators.value.findIndex(g => g.id === selectedGenerator.value.id)
      if (index !== -1) {
        dataGenerators.value[index] = {
          ...dataGenerators.value[index],
          ...generatorForm.value
        }
      }
      toast.success('Generator updated successfully')
    } else {
      // Create new generator
      const newGenerator = {
        id: `GEN${String(dataGenerators.value.length + 1).padStart(3, '0')}`,
        ...generatorForm.value,
        status: 'paused',
        totalGenerated: 0,
        uptime: '0h 0m',
        lastGenerated: null
      }
      dataGenerators.value.push(newGenerator)
      toast.success('Generator created successfully')
    }
    
    closeModals()
  } catch (error) {
    console.error('Error saving generator:', error)
    toast.error('Failed to save generator')
  }
}

const startGenerator = (generatorId: string) => {
  const genIndex = dataGenerators.value.findIndex(g => g.id === generatorId)
  if (genIndex !== -1) {
    dataGenerators.value[genIndex].status = 'active'
    dataGenerators.value[genIndex].lastGenerated = new Date().toISOString()
    toast.success('Generator started')
  }
}

const pauseGenerator = (generatorId: string) => {
  const genIndex = dataGenerators.value.findIndex(g => g.id === generatorId)
  if (genIndex !== -1) {
    dataGenerators.value[genIndex].status = 'paused'
    toast.success('Generator paused')
  }
}

const stopGenerator = (generatorId: string) => {
  const genIndex = dataGenerators.value.findIndex(g => g.id === generatorId)
  if (genIndex !== -1) {
    dataGenerators.value[genIndex].status = 'stopped'
    toast.success('Generator stopped')
  }
}

const deleteGenerator = (generatorId: string) => {
  if (!confirm('Are you sure you want to delete this generator?')) {
    return
  }
  
  dataGenerators.value = dataGenerators.value.filter(g => g.id !== generatorId)
  toast.success('Generator deleted successfully')
}

const downloadDataset = (dataset: any) => {
  toast.success(`Downloading ${dataset.name}`)
  // Simulate download
  setTimeout(() => {
    toast.success('Download completed')
  }, 2000)
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'active': return 'text-green-600 bg-green-100'
    case 'paused': return 'text-yellow-600 bg-yellow-100'
    case 'stopped': return 'text-red-600 bg-red-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const getTypeColor = (type: string) => {
  switch (type) {
    case 'telemetry': return 'text-blue-500'
    case 'maintenance': return 'text-orange-500'
    case 'production': return 'text-green-500'
    case 'alerts': return 'text-red-500'
    case 'quality': return 'text-purple-500'
    default: return 'text-gray-500'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

onMounted(() => {
  loadData()
})
</script>

<template>
  <BaseCard class="synthetic-data-generator">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Database class="header-icon" />
          Synthetic Data Generator
        </h2>
        <p class="header-subtitle">Create realistic test data for development and testing purposes</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Plus class="button-icon" />
        New Generator
      </BaseButton>
    </template>

    <!-- Active Generators -->
    <div class="section">
      <div class="section-header">
        <h3>Data Generators</h3>
      </div>

      <!-- Filters -->
      <div class="filters-section">
        <div class="filter-row">
          <BaseInput
            v-model="searchQuery"
            placeholder="Search generators..."
            class="search-input"
          >
            <template #prefix>
              <Search class="input-icon" />
            </template>
          </BaseInput>
          
          <BaseSelect
            v-model="typeFilter"
            :options="typeOptions"
            class="filter-select"
          />
          
          <BaseSelect
            v-model="statusFilter"
            :options="statusOptions"
            class="filter-select"
          />
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-container">
        <BaseSkeleton v-for="i in 3" :key="i" height="140px" class="mb-4" />
      </div>

      <!-- Generators List -->
      <div v-else class="generators-container">
        <div v-if="filteredGenerators.length === 0" class="empty-state">
          <Database class="empty-icon" />
          <h3>No generators found</h3>
          <p>Create your first data generator to start producing synthetic data</p>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="button-icon" />
            Create Generator
          </BaseButton>
        </div>
        
        <div v-else class="generators-grid">
          <BaseCard
            v-for="generator in filteredGenerators"
            :key="generator.id"
            class="generator-card"
          >
            <div class="generator-header">
              <div class="generator-title">
                <Database class="generator-icon" :class="getTypeColor(generator.type)" />
                <div>
                  <h3>{{ generator.name }}</h3>
                  <p class="generator-type">{{ generator.type }}</p>
                </div>
              </div>
              <span 
                class="status-badge"
                :class="getStatusColor(generator.status)"
              >
                {{ generator.status }}
              </span>
            </div>
            
            <div class="generator-metrics">
              <div class="metric-item">
                <Zap class="metric-icon text-blue-500" />
                <div>
                  <div class="metric-value">{{ generator.recordsPerSecond }}/sec</div>
                  <div class="metric-label">Rate</div>
                </div>
              </div>
              
              <div class="metric-item">
                <Activity class="metric-icon text-green-500" />
                <div>
                  <div class="metric-value">{{ generator.totalGenerated.toLocaleString() }}</div>
                  <div class="metric-label">Generated</div>
                </div>
              </div>
              
              <div class="metric-item">
                <Clock class="metric-icon text-purple-500" />
                <div>
                  <div class="metric-value">{{ generator.uptime }}</div>
                  <div class="metric-label">Uptime</div>
                </div>
              </div>
            </div>
            
            <div class="generator-config">
              <div class="config-item" v-for="(value, key) in generator.config" :key="key">
                <span class="config-key">{{ key }}:</span>
                <span class="config-value">{{ JSON.stringify(value) }}</span>
              </div>
            </div>
            
            <div class="generator-actions">
              <template v-if="generator.status === 'paused'">
                <BaseButton
                  variant="primary"
                  size="sm"
                  @click="startGenerator(generator.id)"
                >
                  <Play class="action-icon" />
                  Start
                </BaseButton>
              </template>
              
              <template v-else-if="generator.status === 'active'">
                <BaseButton
                  variant="outline"
                  size="sm"
                  @click="pauseGenerator(generator.id)"
                >
                  <Pause class="action-icon" />
                  Pause
                </BaseButton>
              </template>
              
              <BaseButton
                variant="outline"
                size="sm"
                @click="openConfigureModal(generator)"
              >
                <Settings class="action-icon" />
                Configure
              </BaseButton>
              
              <BaseButton
                variant="outline"
                size="sm"
                @click="deleteGenerator(generator.id)"
                class="delete-button"
              >
                <Trash2 class="action-icon" />
                Delete
              </BaseButton>
            </div>
          </BaseCard>
        </div>
      </div>
    </div>

    <!-- Generated Datasets -->
    <div class="section">
      <div class="section-header">
        <h3>Generated Datasets</h3>
      </div>
      
      <div class="datasets-grid">
        <BaseCard
          v-for="dataset in datasets"
          :key="dataset.id"
          class="dataset-card"
        >
          <div class="dataset-header">
            <div class="dataset-title">
              <Database class="dataset-icon" :class="getTypeColor(dataset.type)" />
              <div>
                <h3>{{ dataset.name }}</h3>
                <p class="dataset-meta">
                  {{ dataset.recordCount.toLocaleString() }} records • {{ dataset.size }}
                </p>
              </div>
            </div>
            <span class="format-badge">{{ dataset.format.toUpperCase() }}</span>
          </div>
          
          <p class="dataset-description">{{ dataset.description }}</p>
          
          <div class="dataset-meta-grid">
            <div class="meta-item">
              <span class="meta-label">Generated:</span>
              <span class="meta-value">{{ formatDate(dataset.generatedAt) }}</span>
            </div>
            <div class="meta-item">
              <span class="meta-label">Type:</span>
              <span class="meta-value">{{ dataset.type }}</span>
            </div>
          </div>
          
          <BaseButton
            variant="primary"
            size="sm"
            @click="downloadDataset(dataset)"
            class="download-button"
          >
            <Download class="button-icon" />
            Download Dataset
          </BaseButton>
        </BaseCard>
      </div>
    </div>

    <!-- Create/Configure Modal -->
    <div 
      v-if="showCreateModal || showConfigureModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>{{ selectedGenerator ? 'Configure Generator' : 'Create Generator' }}</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="generatorForm.name"
              label="Generator Name"
              required
            />
            
            <BaseSelect
              v-model="generatorForm.type"
              label="Data Type"
              :options="typeOptions.filter(o => o.value !== 'all')"
              required
            />
            
            <BaseInput
              v-model.number="generatorForm.recordsPerSecond"
              label="Records Per Second"
              type="number"
              min="1"
              max="1000"
              required
            />
            
            <BaseInput
              v-model.number="generatorForm.duration"
              label="Duration (seconds)"
              type="number"
              min="1"
              max="86400"
              required
            />
            
            <BaseInput
              v-model="sensorString"
              label="Sensors (comma separated)"
              placeholder="temperature,pressure,vibration"
              class="full-width"
            />
            
            <BaseInput
              v-model.number="generatorForm.config.noiseLevel"
              label="Noise Level"
              type="number"
              min="0"
              max="1"
              step="0.01"
            />
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ selectedGenerator ? 'Update' : 'Create' }} Generator
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.synthetic-data-generator {
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

.section {
  margin-bottom: 2rem;
}

.section-header h3 {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 1rem;
}

.filters-section {
  margin-bottom: 1.5rem;
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

.generators-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.generator-card {
  padding: 1.5rem;
}

.generator-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.generator-title {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  flex: 1;
}

.generator-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.generator-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.generator-type {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.generator-metrics {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1rem;
  margin-bottom: 1rem;
}

.metric-item {
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
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
}

.metric-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
}

.generator-config {
  margin-bottom: 1rem;
  padding: 0.75rem;
  background: #f1f5f9;
  border-radius: 0.5rem;
  font-size: 0.875rem;
}

.config-item {
  display: flex;
  justify-content: space-between;
  margin-bottom: 0.25rem;
}

.config-item:last-child {
  margin-bottom: 0;
}

.config-key {
  color: #64748b;
  font-weight: 500;
}

.config-value {
  color: #1e293b;
  font-family: monospace;
}

.generator-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  flex-wrap: wrap;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
}

.delete-button {
  color: #dc2626 !important;
}

.datasets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1rem;
}

.dataset-card {
  padding: 1.5rem;
}

.dataset-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.dataset-title {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  flex: 1;
}

.dataset-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.dataset-title h3 {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.dataset-meta {
  font-size: 0.75rem;
  color: #64748b;
}

.format-badge {
  padding: 0.25rem 0.5rem;
  background: #e2e8f0;
  color: #334155;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
}

.dataset-description {
  color: #64748b;
  font-size: 0.875rem;
  line-height: 1.5;
  margin-bottom: 1rem;
}

.dataset-meta-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-bottom: 1rem;
}

.meta-item {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.meta-label {
  color: #64748b;
}

.meta-value {
  color: #1e293b;
  font-weight: 500;
}

.download-button {
  width: 100%;
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

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .synthetic-data-generator {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .generators-grid {
    grid-template-columns: 1fr;
  }
  
  .generator-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .generator-metrics {
    grid-template-columns: 1fr;
  }
  
  .datasets-grid {
    grid-template-columns: 1fr;
  }
  
  .dataset-meta-grid {
    grid-template-columns: 1fr;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>