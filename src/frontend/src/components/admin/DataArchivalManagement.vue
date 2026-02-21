<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { 
  archiveTelemetry,
  getArchivalHistory,
  getArchivalStatus,
  type ArchivalStatus,
  type DataArchivalRequest
} from '@/services/dataArchival.service'
import { 
  Database, 
  Archive, 
  Clock, 
  CheckCircle,
  AlertCircle,
  Play,
  History,
  RefreshCw,
  Trash2
} from 'lucide-vue-next'

const toast = useToast()

// State
const archivalJobs: any = ref<ArchivalStatus[]>([])
const loading = ref(false)
const archiving = ref(false)
const retentionDays = ref(90)
const selectedJob: any = ref(null)

// Computed
const activeJobs = computed(() => archivalJobs.value.filter((job: any) => 
  job.status === 'Pending' || job.status === 'Running'
))

const completedJobs = computed(() => archivalJobs.value.filter((job: any) => 
  job.status === 'Completed'
))

const failedJobs = computed(() => archivalJobs.value.filter((job: any) => 
  job.status === 'Failed'
))

const getStatusColor = (status: string): string => {
  const colors: Record<string, string> = {
    'Pending': 'bg-yellow-100 text-yellow-800',
    'Running': 'bg-blue-100 text-blue-800',
    'Completed': 'bg-green-100 text-green-800',
    'Failed': 'bg-red-100 text-red-800'
  }
  return colors[status] || 'bg-gray-100 text-gray-800'
}

const getStatusIcon = (status: string) => {
  const icons: Record<string, any> = {
    'Pending': Clock,
    'Running': RefreshCw,
    'Completed': CheckCircle,
    'Failed': AlertCircle
  }
  return icons[status] || Database
}

// Methods
const loadArchivalHistory = async () => {
  try {
    loading.value = true
    archivalJobs.value = await getArchivalHistory(20)
  } catch (error) {
    console.error('Failed to load archival history:', error)
    toast.error('Failed to load archival history')
  } finally {
    loading.value = false
  }
}

const startArchival = async () => {
  if (retentionDays.value < 1) {
    toast.error('Retention days must be at least 1')
    return
  }
  
  try {
    archiving.value = true
    const request: DataArchivalRequest = {
      retentionDays: retentionDays.value
    }
    
    const job = await archiveTelemetry(request)
    archivalJobs.value.unshift(job)
    toast.success('Data archival job started successfully')
    
    // Start polling for job status
    pollJobStatus(job.jobId)
  } catch (error) {
    console.error('Failed to start archival:', error)
    toast.error('Failed to start data archival')
  } finally {
    archiving.value = false
  }
}

const pollJobStatus = (jobId: string) => {
  const interval = setInterval(async () => {
    try {
      const status = await getArchivalStatus(jobId)
      const jobIndex = archivalJobs.value.findIndex((j: any) => j.jobId === jobId)
      
      if (jobIndex !== -1) {
        archivalJobs.value[jobIndex] = status
        
        // Stop polling when job is completed or failed
        if (status.status === 'Completed' || status.status === 'Failed') {
          clearInterval(interval)
          toast.info(`Archival job ${status.status.toLowerCase()}`)
        }
      } else {
        clearInterval(interval)
      }
    } catch (error) {
      console.error('Failed to poll job status:', error)
      clearInterval(interval)
    }
  }, 5000) // Poll every 5 seconds
}

const refreshJob = async (jobId: string) => {
  try {
    const status = await getArchivalStatus(jobId)
    const jobIndex = archivalJobs.value.findIndex((j: any) => j.jobId === jobId)
    if (jobIndex !== -1) {
      archivalJobs.value[jobIndex] = status
    }
    toast.success('Job status refreshed')
  } catch (error) {
    console.error('Failed to refresh job:', error)
    toast.error('Failed to refresh job status')
  }
}

const deleteJob = async (jobId: string) => {
  if (!confirm('Are you sure you want to delete this archival job record?')) {
    return
  }
  
  try {
    // In a real implementation, this would call a DELETE endpoint
    archivalJobs.value = archivalJobs.value.filter((j: any) => j.jobId !== jobId)
    toast.success('Job record deleted')
  } catch (error) {
    console.error('Failed to delete job:', error)
    toast.error('Failed to delete job record')
  }
}

const formatFileSize = (mb: number): string => {
  if (mb < 1024) return `${mb.toFixed(1)} MB`
  return `${(mb / 1024).toFixed(1)} GB`
}

const formatTime = (dateString: string): string => {
  return new Date(dateString).toLocaleString()
}

const formatDuration = (startedAt: string, completedAt?: string): string => {
  const start = new Date(startedAt)
  const end = completedAt ? new Date(completedAt) : new Date()
  const diffMs = end.getTime() - start.getTime()
  const diffMinutes = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMinutes / 60)
  
  if (diffHours > 0) return `${diffHours}h ${diffMinutes % 60}m`
  if (diffMinutes > 0) return `${diffMinutes}m`
  return `${Math.floor(diffMs / 1000)}s`
}

// Lifecycle
onMounted(async () => {
  await loadArchivalHistory()
})
</script>

<template>
  <BaseCard>
    <div class="data-archival space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Data Archival Management</h1>
          <p class="text-gray-600 mt-2">Manage telemetry data archival and retention policies</p>
        </div>
        <BaseButton variant="outline" @click="loadArchivalHistory" :disabled="loading">
          <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loading }" />
          Refresh
        </BaseButton>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Jobs</p>
                <p class="text-2xl font-bold">{{ activeJobs.length }}</p>
              </div>
              <Archive class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Completed</p>
                <p class="text-2xl font-bold">{{ completedJobs.length }}</p>
              </div>
              <CheckCircle class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Failed</p>
                <p class="text-2xl font-bold">{{ failedJobs.length }}</p>
              </div>
              <AlertCircle class="w-8 h-8 text-red-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Archived</p>
                <p class="text-2xl font-bold">
                  {{ archivalJobs.reduce((sum: number, job: any) => sum + (job.archivedRecords || 0), 0).toLocaleString() }}
                </p>
              </div>
              <Database class="w-8 h-8 text-purple-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Archival Controls -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Start New Archival Job</h2>
          
          <div class="flex flex-col sm:flex-row gap-4 items-end">
            <div class="flex-1">
              <BaseInput
                v-model.number="retentionDays"
                label="Retention Period (Days)"
                type="number"
                min="1"
                max="3650"
                placeholder="Enter number of days to retain data"
              />
              <p class="text-sm text-gray-600 mt-1">
                Data older than {{ retentionDays }} days will be archived
              </p>
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="startArchival"
              :disabled="archiving || retentionDays < 1"
              class="whitespace-nowrap"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': archiving }" />
              {{ archiving ? 'Starting...' : 'Start Archival' }}
            </BaseButton>
          </div>
        </div>
      </BaseCard>

      <!-- Archival History -->
      <BaseCard>
        <div class="p-6">
          <div class="flex justify-between items-center mb-4">
            <h2 class="text-xl font-semibold">Archival History</h2>
            <div class="text-sm text-gray-600">
              Showing {{ archivalJobs.length }} jobs
            </div>
          </div>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading archival history...</p>
          </div>
          
          <div v-else-if="archivalJobs.length === 0" class="text-center py-8">
            <Archive class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No archival jobs found</p>
            <p class="text-sm text-gray-500 mt-1">Start your first archival job above</p>
          </div>
          
          <div v-else class="space-y-4">
            <BaseCard
              v-for="job in archivalJobs"
              :key="job.jobId"
              class="hover:shadow-md transition-shadow"
            >
              <div class="p-4">
                <div class="flex flex-col sm:flex-row justify-between gap-4">
                  <div class="flex-1">
                    <div class="flex items-start justify-between mb-3">
                      <div class="flex items-center gap-3">
                        <component 
                          :is="getStatusIcon(job.status)"
                          class="w-6 h-6"
                          :class="getStatusColor(job.status).replace('bg-', 'text-').replace(' text-', ' text-')"
                        />
                        <div>
                          <h3 class="text-lg font-semibold text-gray-900">
                            Job #{{ job.jobId.substring(0, 8) }}
                          </h3>
                          <p class="text-gray-600">
                            Retention: {{ job.retentionDays || retentionDays }} days
                          </p>
                        </div>
                      </div>
                      <span 
                        class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                        :class="getStatusColor(job.status)"
                      >
                        {{ job.status }}
                      </span>
                    </div>
                    
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4 text-sm text-gray-600 mb-3">
                      <div>
                        <span class="font-medium">Started:</span>
                        <p>{{ formatTime(job.startedAt) }}</p>
                      </div>
                      <div v-if="job.completedAt">
                        <span class="font-medium">Completed:</span>
                        <p>{{ formatTime(job.completedAt) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Duration:</span>
                        <p>{{ formatDuration(job.startedAt, job.completedAt) }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Records Archived:</span>
                        <p>{{ job.archivedRecords?.toLocaleString() || '0' }}</p>
                      </div>
                      <div>
                        <span class="font-medium">Space Freed:</span>
                        <p>{{ job.freedSpaceMB ? formatFileSize(job.freedSpaceMB) : 'N/A' }}</p>
                      </div>
                      <div v-if="job.progress !== undefined">
                        <span class="font-medium">Progress:</span>
                        <p>{{ job.progress }}%</p>
                      </div>
                    </div>
                    
                    <p v-if="job.message" class="text-sm text-gray-700 bg-gray-50 p-2 rounded">
                      {{ job.message }}
                    </p>
                  </div>
                  
                  <div class="flex flex-col gap-2">
                    <BaseButton
                      v-if="job.status === 'Running' || job.status === 'Pending'"
                      size="sm"
                      variant="outline"
                      @click="refreshJob(job.jobId)"
                    >
                      <RefreshCw class="w-4 h-4 mr-1" />
                      Refresh
                    </BaseButton>
                    
                    <BaseButton
                      size="sm"
                      variant="outline"
                      @click="deleteJob(job.jobId)"
                      class="text-red-600 hover:text-red-700"
                    >
                      <Trash2 class="w-4 h-4 mr-1" />
                      Delete
                    </BaseButton>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
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

@media (max-width: 640px) {
  .data-archival {
    padding: 0.5rem;
  }
}
</style>