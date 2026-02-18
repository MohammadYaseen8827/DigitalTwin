<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { 
  FileText, 
  BarChart3, 
  Download,
  Calendar,
  Filter,
  Search,
  Plus,
  Edit,
  Trash2,
  Eye,
  Clock,
  CheckCircle,
  Wrench,
  Factory,
  Shield
} from 'lucide-vue-next'

const toast = useToast()

// State
const reports = ref<any[]>([
  {
    id: 'REP001',
    name: 'Monthly Maintenance Report',
    type: 'maintenance',
    format: 'pdf',
    status: 'completed',
    generatedBy: 'Admin',
    generatedAt: new Date(Date.now() - 86400000).toISOString(),
    fileSize: '2.4 MB',
    downloadCount: 23
  },
  {
    id: 'REP002',
    name: 'Equipment Performance Dashboard',
    type: 'analytics',
    format: 'excel',
    status: 'scheduled',
    generatedBy: 'System',
    generatedAt: new Date(Date.now() + 86400000).toISOString(),
    fileSize: '0 KB',
    downloadCount: 0
  },
  {
    id: 'REP003',
    name: 'Daily Production Summary',
    type: 'production',
    format: 'csv',
    status: 'running',
    generatedBy: 'Scheduler',
    generatedAt: new Date().toISOString(),
    fileSize: '0 KB',
    downloadCount: 0
  },
  {
    id: 'REP004',
    name: 'Quarterly Reliability Analysis',
    type: 'reliability',
    format: 'pdf',
    status: 'completed',
    generatedBy: 'Engineer',
    generatedAt: new Date(Date.now() - 2592000000).toISOString(),
    fileSize: '5.7 MB',
    downloadCount: 45
  }
])

const reportTemplates = ref<any[]>([
  {
    id: 'TMPL001',
    name: 'Standard Maintenance Report',
    category: 'maintenance',
    description: 'Comprehensive maintenance activity summary',
    fields: ['machine_id', 'maintenance_type', 'duration', 'cost', 'technician'],
    createdAt: new Date(Date.now() - 86400000).toISOString()
  },
  {
    id: 'TMPL002',
    name: 'Production Efficiency Report',
    category: 'production',
    description: 'Line and equipment performance metrics',
    fields: ['line_id', 'output', 'efficiency', 'downtime', 'oee'],
    createdAt: new Date(Date.now() - 172800000).toISOString()
  }
])

const loading = ref(false)
const searchQuery = ref('')
const typeFilter = ref('all')
const statusFilter = ref('all')
const showCreateModal = ref(false)
const showScheduleModal = ref(false)
const showPreviewModal = ref(false)
const selectedReport = ref<any>(null)

// Form state
const reportForm = ref({
  name: '',
  type: 'maintenance',
  templateId: '',
  schedule: 'manual',
  frequency: 'daily',
  recipients: '',
  format: 'pdf'
})

// Computed
const filteredReports = computed(() => {
  let filtered = [...reports.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(report => 
      report.name.toLowerCase().includes(query) ||
      report.generatedBy.toLowerCase().includes(query)
    )
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter(report => report.type === typeFilter.value)
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(report => report.status === statusFilter.value)
  }
  
  return filtered
})

const typeOptions = [
  { label: 'All Types', value: 'all' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Production', value: 'production' },
  { label: 'Analytics', value: 'analytics' },
  { label: 'Reliability', value: 'reliability' },
  { label: 'Custom', value: 'custom' }
]

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Scheduled', value: 'scheduled' },
  { label: 'Running', value: 'running' },
  { label: 'Completed', value: 'completed' },
  { label: 'Failed', value: 'failed' }
]

const formatOptions = [
  { label: 'PDF', value: 'pdf' },
  { label: 'Excel', value: 'excel' },
  { label: 'CSV', value: 'csv' },
  { label: 'JSON', value: 'json' }
]

const scheduleOptions = [
  { label: 'Manual', value: 'manual' },
  { label: 'Scheduled', value: 'scheduled' }
]

// Methods
const loadReports = async () => {
  try {
    loading.value = true
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Reports loaded successfully')
  } catch (error) {
    console.error('Error loading reports:', error)
    toast.error('Failed to load reports')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openScheduleModal = (report: any) => {
  selectedReport.value = report
  showScheduleModal.value = true
}

const openPreviewModal = (report: any) => {
  selectedReport.value = report
  showPreviewModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showScheduleModal.value = false
  showPreviewModal.value = false
  selectedReport.value = null
  resetForm()
}

const resetForm = () => {
  reportForm.value = {
    name: '',
    type: 'maintenance',
    templateId: '',
    schedule: 'manual',
    frequency: 'daily',
    recipients: '',
    format: 'pdf'
  }
}

const handleSubmit = async () => {
  try {
    if (reportForm.value.schedule === 'manual') {
      // Generate report immediately
      const newReport = {
        id: `REP${String(reports.value.length + 1).padStart(3, '0')}`,
        ...reportForm.value,
        status: 'running',
        generatedBy: 'Current User',
        generatedAt: new Date().toISOString(),
        fileSize: '0 KB',
        downloadCount: 0
      }
      
      reports.value.push(newReport)
      
      // Simulate report generation
      setTimeout(() => {
        const index = reports.value.findIndex(r => r.id === newReport.id)
        if (index !== -1) {
          reports.value[index].status = 'completed'
          reports.value[index].fileSize = `${(Math.random() * 10).toFixed(1)} MB`
          toast.success('Report generated successfully')
        }
      }, 3000)
      
    } else {
      // Schedule report
      const newReport = {
        id: `REP${String(reports.value.length + 1).padStart(3, '0')}`,
        ...reportForm.value,
        status: 'scheduled',
        generatedBy: 'Scheduler',
        generatedAt: calculateNextRun(reportForm.value.frequency),
        fileSize: '0 KB',
        downloadCount: 0
      }
      
      reports.value.push(newReport)
      toast.success('Report scheduled successfully')
    }
    
    closeModals()
  } catch (error) {
    console.error('Error creating report:', error)
    toast.error('Failed to create report')
  }
}

const calculateNextRun = (frequency: string): string => {
  const now = new Date()
  switch (frequency) {
    case 'daily':
      return new Date(now.getTime() + 86400000).toISOString()
    case 'weekly':
      return new Date(now.getTime() + 604800000).toISOString()
    case 'monthly':
      return new Date(now.setMonth(now.getMonth() + 1)).toISOString()
    default:
      return new Date(now.getTime() + 86400000).toISOString()
  }
}

const generateReport = async (reportId: string) => {
  try {
    const reportIndex = reports.value.findIndex(r => r.id === reportId)
    if (reportIndex !== -1) {
      reports.value[reportIndex].status = 'running'
      reports.value[reportIndex].generatedAt = new Date().toISOString()
      
      // Simulate generation
      setTimeout(() => {
        reports.value[reportIndex].status = 'completed'
        reports.value[reportIndex].fileSize = `${(Math.random() * 5 + 1).toFixed(1)} MB`
        toast.success('Report generated successfully')
      }, 2000)
    }
    
    toast.success('Report generation started')
  } catch (error) {
    console.error('Error generating report:', error)
    toast.error('Failed to generate report')
  }
}

const downloadReport = (report: any) => {
  // Simulate download
  report.downloadCount += 1
  toast.success(`Downloading ${report.name}`)
  
  // In real implementation, this would trigger actual file download
  setTimeout(() => {
    toast.success('Download completed')
  }, 1000)
}

const deleteReport = (reportId: string) => {
  if (!confirm('Are you sure you want to delete this report?')) {
    return
  }
  
  reports.value = reports.value.filter(r => r.id !== reportId)
  toast.success('Report deleted successfully')
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

const getTypeIcon = (type: string) => {
  switch (type) {
    case 'maintenance': return Wrench
    case 'production': return Factory
    case 'analytics': return BarChart3
    case 'reliability': return Shield
    default: return FileText
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

const formatTime = (dateString: string) => {
  return new Date(dateString).toLocaleTimeString()
}

onMounted(() => {
  loadReports()
})
</script>

<template>
  <BaseCard class="reporting-dashboard">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <FileText class="header-icon" />
          Reporting Dashboard
        </h2>
        <p class="header-subtitle">Generate, schedule, and manage business reports and analytics</p>
      </div>
      <BaseButton variant="primary" @click="openCreateModal">
        <Plus class="button-icon" />
        New Report
      </BaseButton>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search reports..."
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
      <BaseSkeleton v-for="i in 6" :key="i" height="120px" class="mb-4" />
    </div>

    <!-- Reports List -->
    <div v-else class="reports-container">
      <div v-if="filteredReports.length === 0" class="empty-state">
        <FileText class="empty-icon" />
        <h3>No reports found</h3>
        <p>Create your first report to get started</p>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="button-icon" />
          Create Report
        </BaseButton>
      </div>
      
      <div v-else class="reports-grid">
        <BaseCard
          v-for="report in filteredReports"
          :key="report.id"
          class="report-card"
        >
          <div class="report-header">
            <div class="report-title">
              <FileText class="report-icon" :class="`type-${report.type}`" />
              <div>
                <h3>{{ report.name }}</h3>
                <p class="report-meta">
                  Generated by {{ report.generatedBy }} • {{ formatDate(report.generatedAt) }}
                </p>
              </div>
            </div>
            <span 
              class="status-badge"
              :class="getStatusColor(report.status)"
            >
              {{ report.status }}
            </span>
          </div>
          
          <div class="report-details">
            <div class="detail-grid">
              <div class="detail-item">
                <span class="detail-label">Type</span>
                <span class="detail-value">{{ report.type }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">Format</span>
                <span class="detail-value">{{ report.format.toUpperCase() }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">Size</span>
                <span class="detail-value">{{ report.fileSize }}</span>
              </div>
              <div class="detail-item">
                <span class="detail-label">Downloads</span>
                <span class="detail-value">{{ report.downloadCount }}</span>
              </div>
            </div>
          </div>
          
          <div class="report-actions">
            <template v-if="report.status === 'completed'">
              <BaseButton
                variant="primary"
                size="sm"
                @click="downloadReport(report)"
              >
                <Download class="action-icon" />
                Download
              </BaseButton>
              <BaseButton
                variant="outline"
                size="sm"
                @click="openPreviewModal(report)"
              >
                <Eye class="action-icon" />
                Preview
              </BaseButton>
            </template>
            
            <template v-else-if="report.status === 'scheduled'">
              <BaseButton
                variant="primary"
                size="sm"
                @click="generateReport(report.id)"
              >
                <Play class="action-icon" />
                Generate Now
              </BaseButton>
            </template>
            
            <BaseButton
              variant="outline"
              size="sm"
              @click="openScheduleModal(report)"
            >
              <Clock class="action-icon" />
              Schedule
            </BaseButton>
            
            <BaseButton
              variant="outline"
              size="sm"
              @click="deleteReport(report.id)"
              class="delete-button"
            >
              <Trash2 class="action-icon" />
              Delete
            </BaseButton>
          </div>
        </BaseCard>
      </div>
    </div>

    <!-- Create Report Modal -->
    <div 
      v-if="showCreateModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>Create New Report</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="reportForm.name"
              label="Report Name"
              required
            />
            
            <BaseSelect
              v-model="reportForm.type"
              label="Report Type"
              :options="typeOptions.filter(o => o.value !== 'all')"
              required
            />
            
            <BaseSelect
              v-model="reportForm.templateId"
              label="Template"
              :options="reportTemplates.map(t => ({ label: t.name, value: t.id }))"
            />
            
            <BaseSelect
              v-model="reportForm.schedule"
              label="Generation Method"
              :options="scheduleOptions"
              required
            />
            
            <BaseSelect
              v-if="reportForm.schedule === 'scheduled'"
              v-model="reportForm.frequency"
              label="Frequency"
              :options="[
                { label: 'Daily', value: 'daily' },
                { label: 'Weekly', value: 'weekly' },
                { label: 'Monthly', value: 'monthly' }
              ]"
              required
            />
            
            <BaseInput
              v-model="reportForm.recipients"
              label="Email Recipients (comma separated)"
              placeholder="email1@company.com, email2@company.com"
            />
            
            <BaseSelect
              v-model="reportForm.format"
              label="Output Format"
              :options="formatOptions"
              required
            />
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ reportForm.schedule === 'manual' ? 'Generate Report' : 'Schedule Report' }}
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>

    <!-- Preview Modal -->
    <div 
      v-if="showPreviewModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="preview-modal" @click.stop>
        <template #header>
          <h3>Report Preview: {{ selectedReport?.name }}</h3>
        </template>
        
        <div class="preview-content">
          <div class="preview-info">
            <p><strong>Type:</strong> {{ selectedReport?.type }}</p>
            <p><strong>Format:</strong> {{ selectedReport?.format.toUpperCase() }}</p>
            <p><strong>Generated:</strong> {{ formatDate(selectedReport?.generatedAt) }} at {{ formatTime(selectedReport?.generatedAt) }}</p>
            <p><strong>Size:</strong> {{ selectedReport?.fileSize }}</p>
          </div>
          
          <div class="preview-placeholder">
            <FileText class="preview-icon" />
            <p>Report preview would appear here</p>
            <p class="preview-note">In a real implementation, this would show the actual report content</p>
          </div>
        </div>
        
        <div class="modal-actions">
          <BaseButton variant="ghost" @click="closeModals">
            Close
          </BaseButton>
          <BaseButton 
            variant="primary" 
            @click="downloadReport(selectedReport)"
          >
            <Download class="button-icon" />
            Download Report
          </BaseButton>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.reporting-dashboard {
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

.reports-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.report-card {
  padding: 1.5rem;
}

.report-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.report-title {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
  flex: 1;
}

.report-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.report-icon.type-maintenance { color: #f59e0b; }
.report-icon.type-production { color: #3b82f6; }
.report-icon.type-analytics { color: #10b981; }
.report-icon.type-reliability { color: #8b5cf6; }

.report-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.report-meta {
  font-size: 0.75rem;
  color: #64748b;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: capitalize;
}

.report-details {
  margin-bottom: 1rem;
}

.detail-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.detail-label {
  color: #64748b;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
}

.report-actions {
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

.preview-modal {
  width: 100%;
  max-width: 800px;
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

.preview-content {
  padding: 1rem 0;
}

.preview-info {
  background: #f8fafc;
  border-radius: 0.5rem;
  padding: 1rem;
  margin-bottom: 1.5rem;
  font-size: 0.875rem;
}

.preview-info p {
  margin-bottom: 0.5rem;
}

.preview-info p:last-child {
  margin-bottom: 0;
}

.preview-placeholder {
  text-align: center;
  padding: 2rem;
  border: 2px dashed #cbd5e1;
  border-radius: 0.5rem;
  color: #64748b;
}

.preview-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.preview-note {
  font-size: 0.75rem;
  margin-top: 0.5rem;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .reporting-dashboard {
    padding: 0.5rem;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .reports-grid {
    grid-template-columns: 1fr;
  }
  
  .report-header {
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