<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import SectionContainer from '@/components/base/SectionContainer.vue'
import { 
  generateReport, 
  getReportTemplates, 
  scheduleReport,
  exportData
} from '@/services/reporting.service'
import type { 
  ReportTemplate, 
  ReportGenerationRequest, 
  ExportFormat,
  ReportSchedule
} from '@/types/reporting.types'

const toast = useToast()

// Reactive state
const templates = ref<ReportTemplate[]>([])
const selectedTemplateId = ref<string>('')
const loading = ref(false)
const generating = ref(false)
const scheduling = ref(false)
const activeTab = ref('generate')

// Report parameters
const reportParameters = ref<Record<string, any>>({})
const selectedFormat = ref<ExportFormat>('pdf')
const dateRange = ref({
  startDate: '',
  endDate: ''
})

// Schedule parameters
const scheduleCron = ref('0 0 9 * * *') // Daily at 9 AM
const scheduleRecipients = ref('')
const scheduleActive = ref(true)

// Computed properties
const filteredTemplates = computed(() => {
  return templates.value.filter(template => 
    template.category === 'Maintenance' || template.category === 'Production'
  )
})

const canGenerate = computed(() => {
  return selectedTemplateId.value && 
         selectedFormat.value &&
         !generating.value
})

const canSchedule = computed(() => {
  return selectedTemplateId.value && 
         scheduleCron.value &&
         scheduleRecipients.value.trim() &&
         !scheduling.value
})

// Methods
const loadTemplates = async () => {
  try {
    loading.value = true
    templates.value = await getReportTemplates()
    if (templates.value.length > 0) {
      selectedTemplateId.value = templates.value[0].id
    }
  } catch (error) {
    console.error('Failed to load templates:', error)
    toast.error('Unable to load report templates')
  } finally {
    loading.value = false
  }
}

const handleTemplateChange = (templateId: string) => {
  selectedTemplateId.value = templateId
  // Reset parameters when template changes
  reportParameters.value = {}
}

const generateReportHandler = async () => {
  if (!selectedTemplateId.value) return

  const selectedTemplate = templates.value.find(t => t.id === selectedTemplateId.value)
  if (!selectedTemplate) return

  try {
    generating.value = true
    
    const request: ReportGenerationRequest = {
      templateId: selectedTemplate.id,
      parameters: {
        ...reportParameters.value,
        dateRange: dateRange.value.startDate && dateRange.value.endDate ? dateRange.value : undefined
      },
      format: selectedFormat.value,
      includeCharts: true,
      includeRawData: false
    }

    const result = await generateReport(request)
    
    toast.success(`Report "${result.fileName}" generated successfully!`)
    
    // In a real app, you'd provide a download link or auto-download
    if (import.meta.env.DEV) console.debug('Download URL:', result.downloadUrl)
    
  } catch (error) {
    console.error('Failed to generate report:', error)
    toast.error('Failed to generate report')
  } finally {
    generating.value = false
  }
}

const scheduleReportHandler = async () => {
  if (!selectedTemplateId.value) return

  const selectedTemplate = templates.value.find(t => t.id === selectedTemplateId.value)
  if (!selectedTemplate) return

  try {
    scheduling.value = true
    
    const schedule: ReportSchedule = {
      id: '',
      templateId: selectedTemplate.id,
      parameters: {
        ...reportParameters.value,
        dateRange: dateRange.value.startDate && dateRange.value.endDate ? dateRange.value : undefined
      },
      format: selectedFormat.value,
      cronExpression: scheduleCron.value,
      recipients: scheduleRecipients.value.split(',').map(email => email.trim()),
      isActive: scheduleActive.value,
      createdAt: new Date().toISOString(),
      lastRun: undefined,
      nextRun: undefined
    }

    const result = await scheduleReport(schedule)
    
    toast.success(`Report scheduled successfully! Next run: ${result.nextRun}`)
    
  } catch (error) {
    console.error('Failed to schedule report:', error)
    toast.error('Failed to schedule report')
  } finally {
    scheduling.value = false
  }
}

const exportSampleData = async () => {
  try {
    const sampleData = [
      { machine: 'CNC-001', status: 'Operational', efficiency: 95.2, lastMaintenance: '2026-01-15' },
      { machine: 'IM-002', status: 'Warning', efficiency: 78.5, lastMaintenance: '2026-01-10' },
      { machine: 'PRESS-003', status: 'Critical', efficiency: 45.1, lastMaintenance: '2026-01-05' }
    ]

    const blob = await exportData(sampleData, selectedFormat.value, 'sample_export')
    
    // Create download link
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `sample_export.${selectedFormat.value}`
    document.body.appendChild(a)
    a.click()
    window.URL.revokeObjectURL(url)
    document.body.removeChild(a)
    
    toast.success('Sample data exported successfully!')
    
  } catch (error) {
    console.error('Failed to export data:', error)
    toast.error('Failed to export data')
  }
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Lifecycle
onMounted(() => {
  loadTemplates()
})
</script>

<template>
  <SectionContainer title="Reporting System" eyebrow="Analytics & Insights" maxWidth="xl" bordered>
    <div class="reporting-page">
      <!-- Tab Navigation -->
      <div class="tabs">
        <button 
          :class="{ active: activeTab === 'generate' }"
          @click="activeTab = 'generate'"
          class="tab-button"
        >
          Generate Report
        </button>
        <button 
          :class="{ active: activeTab === 'schedule' }"
          @click="activeTab = 'schedule'"
          class="tab-button"
        >
          Schedule Reports
        </button>
        <button 
          :class="{ active: activeTab === 'export' }"
          @click="activeTab = 'export'"
          class="tab-button"
        >
          Data Export
        </button>
      </div>

      <!-- Loading State -->
      <BaseCard v-if="loading" variant="soft" class="loading-card">
        <div class="loading-content">
          <BaseSkeleton width="100%" height="40px" />
          <BaseSkeleton width="100%" height="200px" />
        </div>
      </BaseCard>

      <!-- Generate Report Tab -->
      <div v-else-if="activeTab === 'generate'" class="tab-content">
        <BaseCard variant="soft">
          <template #header>
            <h3>Create New Report</h3>
            <p>Select a template and configure parameters to generate your report</p>
          </template>

          <div class="form-grid">
            <!-- Template Selection -->
            <div class="form-group">
              <label>Report Template</label>
              <BaseSelect 
                v-model="selectedTemplateId" 
                :options="filteredTemplates"
                option-label="name"
                option-value="id"
                placeholder="Choose report template"
              />
              <p v-if="selectedTemplateId" class="template-description">
                {{ templates.find(t => t.id === selectedTemplateId)?.description }}
              </p>
            </div>

            <!-- Format Selection -->
            <div class="form-group">
              <label>Output Format</label>
              <BaseSelect
                v-model="selectedFormat"
                :options="[
                  { label: 'PDF Document', value: 'pdf' },
                  { label: 'Excel Spreadsheet', value: 'excel' },
                  { label: 'CSV Data', value: 'csv' },
                  { label: 'JSON Data', value: 'json' }
                ]"
                option-label="label"
                option-value="value"
                placeholder="Choose output format"
              />
            </div>

            <!-- Date Range (if applicable) -->
            <div v-if="selectedTemplateId" class="form-group">
              <label>Date Range</label>
              <div class="date-range-inputs">
                <BaseInput
                  v-model="dateRange.startDate"
                  type="date"
                  label="Start Date"
                  required
                />
                <BaseInput
                  v-model="dateRange.endDate"
                  type="date"
                  label="End Date"
                  required
                />
              </div>
            </div>

            <!-- Dynamic Parameters -->
            <div v-if="templates.find(t => t.id === selectedTemplateId)?.parameters?.length" class="form-group">
              <label>Additional Parameters</label>
              <div class="parameters-grid">
                <div 
                  v-for="param in templates.find(t => t.id === selectedTemplateId)?.parameters || []" 
                  :key="param.name"
                  class="parameter-field"
                >
                  <BaseInput
                    v-if="param.type === 'string' || param.type === 'number'"
                    v-model="reportParameters[param.name]"
                    :type="param.type"
                    :label="param.displayName"
                    :required="param.required"
                    :placeholder="param.defaultValue?.toString()"
                  />
                  
                  <BaseSelect
                    v-else-if="param.type === 'boolean'"
                    v-model="reportParameters[param.name]"
                    :options="[
                      { label: 'Yes', value: true },
                      { label: 'No', value: false }
                    ]"
                    option-label="label"
                    option-value="value"
                    :label="param.displayName"
                  />
                </div>
              </div>
            </div>
          </div>

          <div class="actions">
            <BaseButton
              variant="primary"
              :disabled="!canGenerate"
              :loading="generating"
              @click="generateReportHandler"
            >
              Generate Report
            </BaseButton>
            
            <BaseButton
              variant="outline"
              @click="loadTemplates"
            >
              Refresh Templates
            </BaseButton>
          </div>
        </BaseCard>
      </div>

      <!-- Schedule Reports Tab -->
      <div v-else-if="activeTab === 'schedule'" class="tab-content">
        <BaseCard variant="soft">
          <template #header>
            <h3>Schedule Automated Reports</h3>
            <p>Set up recurring reports to be generated automatically</p>
          </template>

          <div class="form-grid">
            <div class="form-group">
              <label>Template</label>
              <BaseSelect 
                v-model="selectedTemplateId" 
                :options="filteredTemplates"
                option-label="name"
                option-value="id"
                placeholder="Choose template to schedule"
              />
            </div>

            <div class="form-group">
              <label>Cron Expression</label>
              <BaseInput
                v-model="scheduleCron"
                placeholder="0 0 9 * * * (daily at 9 AM)"
                hint="Unix-style cron expression for scheduling"
              />
            </div>

            <div class="form-group">
              <label>Recipient Emails</label>
              <BaseInput
                v-model="scheduleRecipients"
                placeholder="admin@company.com, ops@company.com"
                hint="Comma-separated email addresses"
              />
            </div>

            <div class="form-group">
              <label class="checkbox-label">
                <input 
                  v-model="scheduleActive" 
                  type="checkbox" 
                  class="checkbox"
                />
                Active Schedule
              </label>
            </div>
          </div>

          <div class="actions">
            <BaseButton
              variant="primary"
              :disabled="!canSchedule"
              :loading="scheduling"
              @click="scheduleReportHandler"
            >
              Create Schedule
            </BaseButton>
          </div>
        </BaseCard>
      </div>

      <!-- Data Export Tab -->
      <div v-else-if="activeTab === 'export'" class="tab-content">
        <BaseCard variant="soft">
          <template #header>
            <h3>Data Export</h3>
            <p>Export raw data in various formats for external analysis</p>
          </template>

          <div class="export-options">
            <div class="format-selection">
              <label>Select Export Format:</label>
              <div class="format-buttons">
                <BaseButton
                  v-for="format in ['csv', 'excel', 'json']"
                  :key="format"
                  :variant="selectedFormat === format ? 'primary' : 'outline'"
                  @click="selectedFormat = format as ExportFormat"
                >
                  {{ format.toUpperCase() }}
                </BaseButton>
              </div>
            </div>

            <div class="export-actions">
              <BaseButton
                variant="primary"
                @click="exportSampleData"
              >
                Export Sample Data
              </BaseButton>
              
              <p class="export-hint">
                This will export sample equipment data in {{ selectedFormat.toUpperCase() }} format
              </p>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </SectionContainer>
</template>

<style scoped>
.reporting-page {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.tabs {
  display: flex;
  gap: var(--space-4);
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: var(--space-4);
}

.tab-button {
  padding: var(--space-12) var(--space-20);
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  font-weight: 500;
  cursor: pointer;
  border-radius: var(--radius-lg) var(--radius-lg) 0 0;
  transition: all 0.2s ease;
}

.tab-button:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-hover);
}

.tab-button.active {
  color: var(--color-primary);
  background: var(--color-surface);
  border-bottom: 2px solid var(--color-primary);
}

.tab-content {
  padding: var(--space-16) 0;
}

.loading-card {
  min-height: 300px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: var(--space-16);
  margin-bottom: var(--space-20);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.form-group label {
  font-weight: 500;
  color: var(--color-text-primary);
}

.template-description {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  margin-top: var(--space-4);
}

.date-range-inputs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-8);
}

.parameters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--space-12);
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  cursor: pointer;
  font-weight: 500;
}

.checkbox {
  width: 18px;
  height: 18px;
  accent-color: var(--color-primary);
}

.actions {
  display: flex;
  gap: var(--space-12);
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
}

.export-options {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.format-selection {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.format-selection label {
  font-weight: 500;
  color: var(--color-text-primary);
}

.format-buttons {
  display: flex;
  gap: var(--space-8);
}

.export-actions {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.export-hint {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  margin: 0;
}

@media (max-width: 768px) {
  .tabs {
    flex-direction: column;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .date-range-inputs {
    grid-template-columns: 1fr;
  }
  
  .format-buttons {
    flex-direction: column;
  }
}
</style>