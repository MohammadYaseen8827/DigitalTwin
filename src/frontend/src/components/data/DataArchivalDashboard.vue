<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { 
  Archive, 
  Database, 
  Clock, 
  Download,
  Trash2,
  Settings,
  Calendar,
  HardDrive,
  Filter,
  Search,
  Play,
  Pause,
  CheckCircle
} from 'lucide-vue-next'

const toast = useToast()

// State
const archivalJobs = ref<any[]>([
  {
    id: 'ARCH001',
    name: 'Monthly Telemetry Archive',
    status: 'completed',
    entityType: 'telemetry',
    dataSize: '2.3 GB',
    recordCount: 1250000,
    retentionPeriod: 365,
    schedule: 'monthly',
    lastRun: new Date(Date.now() - 86400000).toISOString(),
    nextRun: new Date(Date.now() + 2592000000).toISOString()
  },
  {
    id: 'ARCH002',
    name: 'Weekly Maintenance Logs',
    status: 'running',
    entityType: 'maintenance',
    dataSize: '156 MB',
    recordCount: 85000,
    retentionPeriod: 180,
    schedule: 'weekly',
    lastRun: new Date().toISOString(),
    nextRun: new Date(Date.now() + 604800000).toISOString()
  },
  {
    id: 'ARCH003',
    name: 'Daily Alert History',
    status: 'scheduled',
    entityType: 'alerts',
    dataSize: '45 MB',
    recordCount: 25000,
    retentionPeriod: 90,
    schedule: 'daily',
    lastRun: new Date(Date.now() - 86400000).toISOString(),
    nextRun: new Date(Date.now() + 86400000).toISOString()
  }
])

const storageStats = ref({
  totalStorage: '1.2 TB',
  usedStorage: '856 GB',
  freeStorage: '344 GB',
  utilization: 71
})

const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const showCreateModal = ref(false)
const showConfigureModal = ref(false)
const selectedJob = ref<any>(null)

// Form state
const archivalForm = ref({
  name: '',
  entityType: 'telemetry',
  retentionPeriod: 365,
  schedule: 'monthly',
  compressionEnabled: true,
  encryptionEnabled: true
})

// Computed
const filteredJobs = computed(() => {
  let filtered = [...archivalJobs.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(job => 
      job.name.toLowerCase().includes(query) ||
      job.entityType.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(job => job.status === statusFilter.value)
  }
  
  return filtered
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Scheduled', value: 'scheduled' },
  { label: 'Running', value: 'running' },
  { label: 'Completed', value: 'completed' },
  { label: 'Failed', value: 'failed' }
]

const entityTypeOptions = [
  { label: 'Telemetry Data', value: 'telemetry' },
  { label: 'Maintenance Records', value: 'maintenance' },
  { label: 'Alert History', value: 'alerts' },
  { label: 'Production Data', value: 'production' },
  { label: 'System Logs', value: 'logs' }
]

const scheduleOptions = [
  { label: 'Daily', value: 'daily' },
  { label: 'Weekly', value: 'weekly' },
  { label: 'Monthly', value: 'monthly' },
  { label: 'Quarterly', value: 'quarterly' },
  { label: 'Yearly', value: 'yearly' }
]

// Methods
const loadArchivalData = async () => {
  try {
    loading.value = true
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500))
    toast.success('Archival data loaded successfully')
  } catch (error) {
    console.error('Error loading archival data:', error)
    toast.error('Failed to load archival data')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openConfigureModal = (job: any) => {
  selectedJob.value = job
  archivalForm.value = {
    name: job.name,
    entityType: job.entityType,
    retentionPeriod: job.retentionPeriod,
    schedule: job.schedule,
    compressionEnabled: true,
    encryptionEnabled: true
  }
  showConfigureModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showConfigureModal.value = false
  selectedJob.value = null
  resetForm()
}

const resetForm = () => {
  archivalForm.value = {
    name: '',
    entityType: 'telemetry',
    retentionPeriod: 365,
    schedule: 'monthly',
    compressionEnabled: true,
    encryptionEnabled: true
  }
}

const handleSubmit = async () => {
  try {
    if (selectedJob.value) {
      // Update existing job
      const index = archivalJobs.value.findIndex(j => j.id === selectedJob.value.id)
      if (index !== -1) {
        archivalJobs.value[index] = {
          ...archivalJobs.value[index],
          ...archivalForm.value,
          nextRun: new Date(Date.now() + 86400000).toISOString() // Tomorrow
        }
      }
      toast.success('Archival job updated successfully')
    } else {
      // Create new job
      const newJob = {
        id: `ARCH${String(archivalJobs.value.length + 1).padStart(3, '0')}`,
        ...archivalForm.value,
        status: 'scheduled',
        dataSize: '0 MB',
        recordCount: 0,
        lastRun: null,
        nextRun: new Date(Date.now() + 86400000).toISOString()
      }
      archivalJobs.value.push(newJob)
      toast.success('Archival job created successfully')
    }
    
    closeModals()
  } catch (error) {
    console.error('Error saving archival job:', error)
    toast.error('Failed to save archival job')
  }
}

const runArchivalJob = async (jobId: string) => {
  try {
    const jobIndex = archivalJobs.value.findIndex(j => j.id === jobId)
    if (jobIndex !== -1) {
      // Simulate job execution
      archivalJobs.value[jobIndex].status = 'running'
      archivalJobs.value[jobIndex].lastRun = new Date().toISOString()
      
      // Simulate completion after delay
      setTimeout(() => {
        archivalJobs.value[jobIndex].status = 'completed'
        archivalJobs.value[jobIndex].dataSize = `${Math.floor(Math.random() * 1000) + 50} MB`
        archivalJobs.value[jobIndex].recordCount = Math.floor(Math.random() * 100000) + 10000
        toast.success('Archival job completed successfully')
      }, 3000)
    }
    
    toast.success('Archival job started')
  } catch (error) {
    console.error('Error running archival job:', error)
    toast.error('Failed to start archival job')
  }
}

const pauseArchivalJob = (jobId: string) => {
  const jobIndex = archivalJobs.value.findIndex(j => j.id === jobId)
  if (jobIndex !== -1) {
    archivalJobs.value[jobIndex].status = 'scheduled'
    toast.success('Archival job paused')
  }
}

const deleteArchivalJob = (jobId: string) => {
  if (!confirm('Are you sure you want to delete this archival job?')) {
    return
  }
  
  archivalJobs.value = archivalJobs.value.filter(j => j.id !== jobId)
  toast.success('Archival job deleted successfully')
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'scheduled': return 'text-blue-600 bg-blue-100'
    case 'running': return 'text-yellow-600 bg-yellow-100'
    case 'completed': return 'text-green-600 bg-green-100'
    case 'failed': return 'text-red-600 bg-red-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const getStatusIcon = (status: string) => {
  switch (status) {
    case 'scheduled': return Clock
    case 'running': return Play
    case 'completed': return CheckCircle
    case 'failed': return Trash2
    default: return Database
  }
}

const formatDate = (dateString: string | null) => {
  if (!dateString) return 'Never'
  return new Date(dateString).toLocaleDateString()
}

const formatFileSize = (bytes: number) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

const getUtilizationColor = (utilization: number) => {
  if (utilization < 70) return 'bg-green-500'
  if (utilization < 85) return 'bg-yellow-500'
  return 'bg-red-500'
}

onMounted(() => {
  loadArchivalData()
})
</script>

<template>
  <BaseCard class="data-archival">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Archive class="header-icon" />
          Data Archival Management
        </h2>
        <p class="header-subtitle">Automated data archiving, retention policies, and storage management</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Database class="button-icon" />
        New Archival Job
      </BaseButton>
    </template>

    <!-- Storage Overview -->
    <div class="storage-overview">
      <BaseCard class="storage-stats">
        <div class="stats-grid">
          <div class="stat-item">
            <HardDrive class="stat-icon text-blue-500" />
            <div>
              <div class="stat-value">{{ storageStats.totalStorage }}</div>
              <div class="stat-label">Total Storage</div>
            </div>
          </div>
          
          <div class="stat-item">
            <Database class="stat-icon text-green-500" />
            <div>
              <div class="stat-value">{{ storageStats.usedStorage }}</div>
              <div class="stat-label">Used Storage</div>
            </div>
          </div>
          
          <div class="stat-item">
            <div class="free-storage-icon">
              <HardDrive class="inner-icon" />
              <div class="free-indicator"></div>
            </div>
            <div>
              <div class="stat-value">{{ storageStats.freeStorage }}</div>
              <div class="stat-label">Free Storage</div>
            </div>
          </div>
          
          <div class="stat-item">
            <div class="utilization-bar">
              <div 
                class="utilization-fill"
                :class="getUtilizationColor(storageStats.utilization)"
                :style="{ width: storageStats.utilization + '%' }"
              ></div>
            </div>
            <div>
              <div class="stat-value">{{ storageStats.utilization }}%</div>
              <div class="stat-label">Utilization</div>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search archival jobs..."
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
      <BaseSkeleton v-for="i in 6" :key="i" height="120px" class="mb-4" />
    </div>

    <!-- Archival Jobs -->
    <div v-else class="jobs-container">
      <div v-if="filteredJobs.length === 0" class="empty-state">
        <Archive class="empty-icon" />
        <h3>No archival jobs found</h3>
        <p>Create your first archival job to start managing data retention</p>
        <BaseButton variant="primary" @click="openCreateModal">
          <Database class="button-icon" />
          Create Archival Job
        </BaseButton>
      </div>
      
      <div v-else class="jobs-grid">
        <BaseCard
          v-for="job in filteredJobs"
          :key="job.id"
          class="job-card"
        >
          <div class="job-header">
            <div class="job-title">
              <h3>{{ job.name }}</h3>
              <p class="job-id">ID: {{ job.id }}</p>
            </div>
            <span 
              class="status-badge"
              :class="getStatusColor(job.status)"
            >
              <component :is="getStatusIcon(job.status)" class="status-icon" />
              {{ job.status }}
            </span>
          </div>
          
          <div class="job-details">
            <div class="detail-grid">
              <div class="detail-item">
                <Database class="detail-icon text-blue-500" />
                <div>
                  <div class="detail-value">{{ job.entityType }}</div>
                  <div class="detail-label">Entity Type</div>
                </div>
              </div>
              
              <div class="detail-item">
                <Calendar class="detail-icon text-green-500" />
                <div>
                  <div class="detail-value">{{ job.retentionPeriod }} days</div>
                  <div class="detail-label">Retention</div>
                </div>
              </div>
              
              <div class="detail-item">
                <Clock class="detail-icon text-purple-500" />
                <div>
                  <div class="detail-value">{{ job.schedule }}</div>
                  <div class="detail-label">Schedule</div>
                </div>
              </div>
              
              <div class="detail-item">
                <HardDrive class="detail-icon text-orange-500" />
                <div>
                  <div class="detail-value">{{ job.dataSize }}</div>
                  <div class="detail-label">Data Size</div>
                </div>
              </div>
            </div>
            
            <div class="job-meta">
              <div class="meta-item">
                <span>Last run: {{ formatDate(job.lastRun) }}</span>
              </div>
              <div class="meta-item">
                <span>Next run: {{ formatDate(job.nextRun) }}</span>
              </div>
              <div class="meta-item">
                <span>{{ job.recordCount.toLocaleString() }} records</span>
              </div>
            </div>
          </div>
          
          <div class="job-actions">
            <template v-if="job.status === 'scheduled'">
              <BaseButton
                variant="primary"
                size="sm"
                @click="runArchivalJob(job.id)"
              >
                <Play class="action-icon" />
                Run Now
              </BaseButton>
            </template>
            
            <template v-else-if="job.status === 'running'">
              <BaseButton
                variant="outline"
                size="sm"
                @click="pauseArchivalJob(job.id)"
              >
                <Pause class="action-icon" />
                Pause
              </BaseButton>
            </template>
            
            <BaseButton
              variant="outline"
              size="sm"
              @click="openConfigureModal(job)"
            >
              <Settings class="action-icon" />
              Configure
            </BaseButton>
            
            <BaseButton
              variant="outline"
              size="sm"
              @click="deleteArchivalJob(job.id)"
              class="delete-button"
            >
              <Trash2 class="action-icon" />
              Delete
            </BaseButton>
          </div>
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
          <h3>{{ selectedJob ? 'Configure Archival Job' : 'Create Archival Job' }}</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="archivalForm.name"
              label="Job Name"
              required
            />
            
            <BaseSelect
              v-model="archivalForm.entityType"
              label="Entity Type"
              :options="entityTypeOptions"
              required
            />
            
            <BaseInput
              v-model.number="archivalForm.retentionPeriod"
              label="Retention Period (days)"
              type="number"
              min="1"
              max="3650"
              required
            />
            
            <BaseSelect
              v-model="archivalForm.schedule"
              label="Schedule"
              :options="scheduleOptions"
              required
            />
            
            <div class="checkbox-wrapper">
              <input 
                id="compressionEnabled" 
                v-model="archivalForm.compressionEnabled" 
                type="checkbox" 
                class="checkbox-input"
              />
              <label for="compressionEnabled" class="checkbox-label">Enable Compression</label>
            </div>
            
            <div class="checkbox-wrapper">
              <input 
                id="encryptionEnabled" 
                v-model="archivalForm.encryptionEnabled" 
                type="checkbox" 
                class="checkbox-input"
              />
              <label for="encryptionEnabled" class="checkbox-label">Enable Encryption</label>
            </div>
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ selectedJob ? 'Update' : 'Create' }} Job
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.data-archival {
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

.storage-overview {
  margin-bottom: 2rem;
}

.storage-stats {
  padding: 1.5rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  width: 2rem;
  height: 2rem;
}

.free-storage-icon {
  position: relative;
  width: 2rem;
  height: 2rem;
}

.inner-icon {
  width: 2rem;
  height: 2rem;
  color: #10b981;
}

.free-indicator {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 12px;
  height: 12px;
  background: #10b981;
  border: 2px solid white;
  border-radius: 50%;
}

.utilization-bar {
  width: 40px;
  height: 2rem;
  background: #e2e8f0;
  border-radius: 0.25rem;
  overflow: hidden;
  flex-shrink: 0;
}

.utilization-fill {
  height: 100%;
  transition: width 0.3s ease;
}

.stat-value {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
}

.stat-label {
  font-size: 0.875rem;
  color: #64748b;
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

.jobs-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1rem;
}

.job-card {
  padding: 1.5rem;
}

.job-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.job-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.job-id {
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

.job-details {
  margin-bottom: 1rem;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  margin-bottom: 1rem;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.detail-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.detail-value {
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
}

.detail-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
}

.job-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.75rem;
  color: #64748b;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.job-actions {
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

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .data-archival {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .jobs-grid {
    grid-template-columns: 1fr;
  }
  
  .job-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .detail-grid {
    grid-template-columns: 1fr;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>