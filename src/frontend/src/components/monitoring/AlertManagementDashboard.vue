<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import { useAlertsStore } from '@/stores/alerts'
import type { MachineDto, AlertDto } from '@/api/types'
import { 
  Bell, 
  AlertTriangle, 
  Info, 
  CheckCircle,
  XCircle,
  Filter,
  Search,
  Eye,
  EyeOff
} from 'lucide-vue-next'

const toast = useToast()

// Store
const alertsStore = useAlertsStore()
const { alerts: storeAlerts, loading, error } = storeToRefs(alertsStore)
const { fetchAlerts, acknowledgeAlert: storeAcknowledgeAlert, dismissAlert } = alertsStore

// Local state for UI
const machines = ref<MachineDto[]>([])
const searchQuery = ref('')
const severityFilter = ref('all')
const statusFilter = ref('all')

// Modal state
const showAcknowledgeModal = ref(false)
const selectedAlert = ref<any>(null)
const acknowledgeComment = ref('')

// Computed
const filteredAlerts = computed(() => {
  let filtered = [...storeAlerts.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(alert => 
      (alert.message?.toLowerCase().includes(query)) ||
      (alert.machineName?.toLowerCase().includes(query)) ||
      (alert.machineId?.toLowerCase().includes(query))
    )
  }
  
  // Apply severity filter
  if (severityFilter.value !== 'all') {
    filtered = filtered.filter(alert => alert.severity?.toLowerCase() === severityFilter.value)
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    const isAcknowledged = statusFilter.value === 'acknowledged'
    filtered = filtered.filter(alert => alert.acknowledged === isAcknowledged)
  }
  
  // Sort by severity and creation time
  return filtered.sort((a, b) => {
    const severityOrder: Record<string, number> = { 'critical': 3, 'high': 3, 'warning': 2, 'medium': 2, 'info': 1, 'low': 1 }
    const aSeverity = a.severity?.toLowerCase() || 'low'
    const bSeverity = b.severity?.toLowerCase() || 'low'
    
    if ((severityOrder[aSeverity] || 0) !== (severityOrder[bSeverity] || 0)) {
      return (severityOrder[bSeverity] || 0) - (severityOrder[aSeverity] || 0)
    }
    
    return new Date(b.createdAt || 0).getTime() - new Date(a.createdAt || 0).getTime()
  })
})

const severityOptions = [
  { label: 'All Severities', value: 'all' },
  { label: 'Info', value: 'info' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' }
]

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Unacknowledged', value: 'unacknowledged' },
  { label: 'Acknowledged', value: 'acknowledged' }
]

// Methods
const loadAlerts = async () => {
  try {
    await fetchAlerts()
    toast.success('Alerts loaded successfully')
  } catch (err) {
    console.error('Error loading alerts:', err)
    toast.error('Failed to load alerts')
  }
}

const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Error loading machines:', error)
  }
}

const getSeverityIcon = (severity: string) => {
  switch (severity.toLowerCase()) {
    case 'critical': return AlertTriangle
    case 'warning': return AlertTriangle
    case 'info': return Info
    default: return Bell
  }
}

const getSeverityColor = (severity: string) => {
  switch (severity.toLowerCase()) {
    case 'critical': return 'text-red-600 bg-red-100'
    case 'warning': return 'text-orange-600 bg-orange-100'
    case 'info': return 'text-blue-600 bg-blue-100'
    default: return 'text-gray-600 bg-gray-100'
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const getTimeAgo = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffMinutes = Math.floor(diffMs / (1000 * 60))
  
  if (diffHours > 24) {
    return `${Math.floor(diffHours / 24)} days ago`
  } else if (diffHours > 0) {
    return `${diffHours} hours ago`
  } else if (diffMinutes > 0) {
    return `${diffMinutes} minutes ago`
  } else {
    return 'Just now'
  }
}

const acknowledgeAlert = (alert: any) => {
  selectedAlert.value = alert
  acknowledgeComment.value = ''
  showAcknowledgeModal.value = true
}

const confirmAcknowledge = () => {
  if (!selectedAlert.value) return
  
  // Update the alert in our local array
  const alertIndex = alerts.value.findIndex(a => a.id === selectedAlert.value.id)
  if (alertIndex !== -1) {
    alerts.value[alertIndex] = {
      ...alerts.value[alertIndex],
      isAcknowledged: true,
      acknowledgedBy: 'current-user', // Would come from auth context
      acknowledgedAt: new Date().toISOString(),
      comment: acknowledgeComment.value || undefined
    }
  }
  
  toast.success('Alert acknowledged successfully')
  closeAcknowledgeModal()
}

const closeAcknowledgeModal = () => {
  showAcknowledgeModal.value = false
  selectedAlert.value = null
  acknowledgeComment.value = ''
}

const handleDismissAlert = async (alertId: string) => {
  if (!confirm('Are you sure you want to dismiss this alert?')) {
    return
  }
  
  await dismissAlert(alertId)
  toast.success('Alert dismissed successfully')
}

onMounted(() => {
  loadAlerts()
  loadMachines()
})
</script>

<template>
  <BaseCard class="alert-management">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Bell class="header-icon" />
          Alert Management
        </h2>
        <p class="header-subtitle">Monitor and manage system alerts and notifications</p>
      </div>
      <div class="header-stats">
        <div class="stat-badge critical">
          <AlertTriangle class="stat-icon" />
          <span>{{ filteredAlerts.filter(a => a.severity.toLowerCase() === 'critical' && !a.isAcknowledged).length }} Critical</span>
        </div>
        <div class="stat-badge warning">
          <AlertTriangle class="stat-icon" />
          <span>{{ filteredAlerts.filter(a => a.severity.toLowerCase() === 'warning' && !a.isAcknowledged).length }} Warning</span>
        </div>
        <div class="stat-badge info">
          <Info class="stat-icon" />
          <span>{{ filteredAlerts.filter(a => a.severity.toLowerCase() === 'info' && !a.isAcknowledged).length }} Info</span>
        </div>
      </div>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search alerts..."
          class="search-input"
        >
          <template #prefix>
            <Search class="input-icon" />
          </template>
        </BaseInput>
        
        <BaseSelect
          v-model="severityFilter"
          :options="severityOptions"
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
      <BaseSkeleton v-for="i in 6" :key="i" height="100px" class="mb-4" />
    </div>

    <!-- Alerts List -->
    <div v-else class="alerts-container">
      <div v-if="filteredAlerts.length === 0" class="empty-state">
        <Bell class="empty-icon" />
        <h3>No alerts found</h3>
        <p>No alerts match your current filters</p>
      </div>
      
      <div v-else class="alerts-grid">
        <BaseCard
          v-for="alert in filteredAlerts"
          :key="alert.id"
          class="alert-card"
          :class="{ 'alert-acknowledged': alert.isAcknowledged }"
        >
          <div class="alert-header">
            <div class="alert-title">
              <component 
                :is="getSeverityIcon(alert.severity)" 
                class="severity-icon"
                :class="getSeverityColor(alert.severity)"
              />
              <div>
                <h3>{{ alert.machineName }}</h3>
                <p class="alert-machine-id">ID: {{ alert.machineId }}</p>
              </div>
            </div>
            <div class="alert-status">
              <span 
                class="status-badge"
                :class="alert.isAcknowledged ? 'acknowledged' : 'unacknowledged'"
              >
                {{ alert.isAcknowledged ? 'Acknowledged' : 'Unacknowledged' }}
              </span>
              <span class="severity-badge" :class="getSeverityColor(alert.severity)">
                {{ alert.severity }}
              </span>
            </div>
          </div>
          
          <div class="alert-content">
            <p class="alert-message">{{ alert.message }}</p>
            
            <div class="alert-meta">
              <div class="meta-item">
                <span class="time-ago">{{ getTimeAgo(alert.createdAt) }}</span>
              </div>
              <div class="meta-item" v-if="alert.isAcknowledged && alert.acknowledgedBy">
                <span>Acknowledged by {{ alert.acknowledgedBy }}</span>
              </div>
              <div class="meta-item" v-if="alert.comment">
                <span>Comment: {{ alert.comment }}</span>
              </div>
            </div>
          </div>
          
          <div class="alert-actions" v-if="!alert.isAcknowledged">
            <BaseButton
              variant="primary"
              size="sm"
              @click="acknowledgeAlert(alert)"
            >
              <CheckCircle class="action-icon" />
              Acknowledge
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="dismissAlert(alert.id)"
              class="dismiss-button"
            >
              <XCircle class="action-icon" />
              Dismiss
            </BaseButton>
          </div>
        </BaseCard>
      </div>
    </div>

    <!-- Acknowledge Modal -->
    <div 
      v-if="showAcknowledgeModal" 
      class="modal-overlay"
      @click="closeAcknowledgeModal"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Acknowledge Alert</h3>
        </template>
        
        <div class="modal-body">
          <div class="alert-preview">
            <div class="preview-header">
              <component 
                :is="getSeverityIcon(selectedAlert?.severity || '')" 
                class="preview-icon"
                :class="getSeverityColor(selectedAlert?.severity || '')"
              />
              <div>
                <h4>{{ selectedAlert?.machineName }}</h4>
                <p>{{ selectedAlert?.message }}</p>
              </div>
            </div>
          </div>
          
          <BaseInput
            v-model="acknowledgeComment"
            label="Comment (optional)"
            type="textarea"
            placeholder="Add any additional notes about this acknowledgment..."
          />
        </div>
        
        <div class="modal-actions">
          <BaseButton variant="ghost" @click="closeAcknowledgeModal">
            Cancel
          </BaseButton>
          <BaseButton variant="primary" @click="confirmAcknowledge">
            Acknowledge Alert
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.alert-management {
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

.header-stats {
  display: flex;
  gap: 1rem;
}

.stat-badge {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.stat-badge.critical {
  background: #fee2e2;
  color: #dc2626;
}

.stat-badge.warning {
  background: #ffedd5;
  color: #ea580c;
}

.stat-badge.info {
  background: #dbeafe;
  color: #2563eb;
}

.stat-icon {
  width: 1rem;
  height: 1rem;
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

.alerts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 1rem;
}

.alert-card {
  padding: 1.5rem;
  transition: all 0.2s ease;
}

.alert-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.alert-card.alert-acknowledged {
  opacity: 0.7;
  background: #f8fafc;
}

.alert-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.alert-title {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  flex: 1;
}

.severity-icon {
  width: 1.5rem;
  height: 1.5rem;
  padding: 0.25rem;
  border-radius: 50%;
  flex-shrink: 0;
}

.alert-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.alert-machine-id {
  font-size: 0.75rem;
  color: #64748b;
}

.alert-status {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  align-items: flex-end;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.status-badge.unacknowledged {
  background: #fef2f2;
  color: #dc2626;
}

.status-badge.acknowledged {
  background: #f0fdf4;
  color: #16a34a;
}

.severity-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.alert-content {
  margin-bottom: 1rem;
}

.alert-message {
  color: #334155;
  line-height: 1.5;
  margin-bottom: 1rem;
}

.alert-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  font-size: 0.875rem;
  color: #64748b;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.time-ago {
  font-weight: 500;
}

.alert-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
}

.dismiss-button {
  color: #64748b !important;
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

.alert-preview {
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 1rem;
  margin-bottom: 1.5rem;
}

.preview-header {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.preview-icon {
  width: 1.5rem;
  height: 1.5rem;
  padding: 0.25rem;
  border-radius: 50%;
  flex-shrink: 0;
}

.preview-header h4 {
  font-size: 1rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.preview-header p {
  color: #64748b;
  font-size: 0.875rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .alert-management {
    padding: 0.5rem;
  }
  
  .header-stats {
    flex-wrap: wrap;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .alerts-grid {
    grid-template-columns: 1fr;
  }
  
  .alert-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .alert-status {
    align-items: flex-start;
  }
}
</style>