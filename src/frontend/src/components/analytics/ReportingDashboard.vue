<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseTextarea from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { FileText, Download, Calendar, Filter, Send, Clock, BarChart3, PieChart, FileSpreadsheet } from 'lucide-vue-next'
import { reportingService } from '@/services/reporting.service'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
  PieChart as EChartsPie,
  LineChart as EChartsLine
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsBar,
  EChartsPie,
  EChartsLine,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const activeTab = ref('generate')
const generating = ref(false)
const exporting = ref(false)
const loadingTemplates = ref(false)
const templates = ref<any[]>([])
const reportHistory = ref<any[]>([])

// Report Generation State
const selectedTemplate = ref('')
const reportParameters = ref<Record<string, any>>({})
const reportFormat = ref('pdf')
const scheduledReports = ref<any[]>([])

// Data Export State
const exportEntity = ref('machines')
const exportFormat = ref('csv')
const exportFilters = ref({
  dateRange: 'last30days',
  status: '',
  machineType: ''
})

// Chart refs
const reportStatsChartRef = ref<HTMLDivElement | null>(null)
const exportStatsChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let reportStatsChartInstance: echarts.ECharts | null = null
let exportStatsChartInstance: echarts.ECharts | null = null

// Options
const formatOptions = [
  { label: 'PDF', value: 'pdf' },
  { label: 'Excel', value: 'excel' },
  { label: 'CSV', value: 'csv' },
  { label: 'JSON', value: 'json' }
]

const entityOptions = [
  { label: 'Machines', value: 'machines' },
  { label: 'Maintenance Records', value: 'maintenance' },
  { label: 'Alerts', value: 'alerts' },
  { label: 'Telemetry Data', value: 'telemetry' },
  { label: 'Production Lines', value: 'productionLines' }
]

const dateRangeOptions = [
  { label: 'Last 7 Days', value: 'last7days' },
  { label: 'Last 30 Days', value: 'last30days' },
  { label: 'Last 90 Days', value: 'last90days' },
  { label: 'Custom Range', value: 'custom' }
]

// Computed
const selectedTemplateDetails = computed(() => {
  return templates.value.find(t => t.id === selectedTemplate.value)
})

const requiredParameters = computed(() => {
  if (!selectedTemplateDetails.value) return []
  return selectedTemplateDetails.value.parameters?.filter((p: any) => p.required) || []
})

const isFormValid = computed(() => {
  return selectedTemplate.value && 
         requiredParameters.value.every((param: any) => 
           reportParameters.value[param.name] !== undefined && 
           reportParameters.value[param.name] !== ''
         )
})

// Methods
const loadTemplates = async () => {
  try {
    loadingTemplates.value = true
    templates.value = await reportingService.getReportTemplates()
    
    // Set default template
    if (templates.value.length > 0) {
      selectedTemplate.value = templates.value[0].id
      initializeParameters()
    }
  } catch (error) {
    console.error('Failed to load templates:', error)
    toast.error('Failed to load report templates')
  } finally {
    loadingTemplates.value = false
  }
}

const loadHistory = async () => {
  try {
    const response = await reportingService.getReportHistory()
    reportHistory.value = response.items
    renderReportStatsChart()
  } catch (error) {
    console.error('Failed to load report history:', error)
  }
}

const initializeParameters = () => {
  if (!selectedTemplateDetails.value) return
  
  const params: Record<string, any> = {}
  selectedTemplateDetails.value.parameters?.forEach((param: any) => {
    if (param.type === 'date-range') {
      params[param.name] = 'last30days'
    } else if (param.options && param.options.length > 0) {
      params[param.name] = param.options[0]
    } else {
      params[param.name] = ''
    }
  })
  
  reportParameters.value = params
}

const generateReport = async () => {
  try {
    generating.value = true
    const response = await reportingService.generateReport({
      templateId: selectedTemplate.value,
      parameters: reportParameters.value,
      format: reportFormat.value
    })
    
    toast.success(`Report "${response.fileName}" generated successfully`)
    await loadHistory()
  } catch (error) {
    console.error('Failed to generate report:', error)
    toast.error('Failed to generate report')
  } finally {
    generating.value = false
  }
}

const exportData = async () => {
  try {
    exporting.value = true
    const blob = await reportingService.exportData([], exportFormat.value as any)
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', `${exportEntity.value}_export.${exportFormat.value}`)
    document.body.appendChild(link)
    link.click()
    link.remove()
    toast.success('Data exported successfully')
  } catch (error) {
    console.error('Failed to export data:', error)
    toast.error('Failed to export data')
  } finally {
    exporting.value = false
  }
}

const scheduleReport = () => {
  const schedule = {
    id: `schedule_${Date.now()}`,
    templateId: selectedTemplate.value,
    templateName: selectedTemplateDetails.value?.name || selectedTemplate.value,
    cronExpression: '0 0 * * 1', // Weekly on Monday
    format: reportFormat.value,
    parameters: { ...reportParameters.value },
    createdAt: new Date().toISOString(),
    isActive: true
  }
  
  scheduledReports.value.push(schedule)
  toast.success('Report scheduled successfully')
}

const downloadReport = async (report: any) => {
  try {
    toast.info(`Downloading ${report.fileName}...`)
    const blob = await reportingService.downloadReport(report.id)
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.setAttribute('download', report.fileName)
    document.body.appendChild(link)
    link.click()
    link.remove()
  } catch (error) {
    console.error('Download failed:', error)
    toast.error('Failed to download report')
  }
}

const renderReportStatsChart = () => {
  if (!reportStatsChartRef.value) return
  
  if (!reportStatsChartInstance) {
    reportStatsChartInstance = echarts.init(reportStatsChartRef.value)
  }
  
  // Mock data for demonstration
  const data = [
    { name: 'Equipment Health', value: 24 },
    { name: 'Maintenance Effectiveness', value: 18 },
    { name: 'Production Impact', value: 15 },
    { name: 'Custom Reports', value: 9 }
  ]
  
  const option = {
    title: {
      text: 'Report Generation Statistics',
      left: 'center'
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 10,
      top: 50,
      data: data.map(item => item.name)
    },
    series: [
      {
        name: 'Reports',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        data: data,
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }
  
  reportStatsChartInstance.setOption(option, true)
}

const renderExportStatsChart = () => {
  if (!exportStatsChartRef.value) return
  
  if (!exportStatsChartInstance) {
    exportStatsChartInstance = echarts.init(exportStatsChartRef.value)
  }
  
  // Mock data for demonstration
  const entities = ['Machines', 'Maintenance', 'Alerts', 'Telemetry']
  const counts = [1200, 850, 2300, 4500]
  
  const option = {
    title: {
      text: 'Data Export Statistics',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    xAxis: {
      type: 'category',
      data: entities
    },
    yAxis: {
      type: 'value',
      name: 'Records Exported'
    },
    series: [{
      name: 'Exports',
      type: 'bar',
      data: counts,
      itemStyle: {
        color: '#3b82f6'
      }
    }]
  }
  
  exportStatsChartInstance.setOption(option, true)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

const formatFileSize = (bytes: number) => {
  if (bytes < 1024) return bytes + ' B'
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB'
  return (bytes / (1024 * 1024)).toFixed(1) + ' MB'
}

const resizeCharts = () => {
  reportStatsChartInstance?.resize()
  exportStatsChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  loadTemplates()
  loadHistory()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  reportStatsChartInstance?.dispose()
  exportStatsChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="reporting-dashboard space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Reporting & Analytics</h1>
          <p class="text-gray-600 mt-2">Generate reports and export data for business intelligence</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline">
            <BarChart3 class="w-4 h-4 mr-2" />
            View Analytics
          </BaseButton>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in [
              { id: 'generate', name: 'Generate Reports', icon: FileText },
              { id: 'export', name: 'Export Data', icon: FileSpreadsheet },
              { id: 'scheduled', name: 'Scheduled Reports', icon: Clock },
              { id: 'history', name: 'Report History', icon: FileText }
            ]"
            :key="tab.id"
            @click="activeTab = tab.id"
            class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"
            :class="[
              activeTab === tab.id
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            <component :is="tab.icon" class="w-4 h-4 inline mr-2" />
            {{ tab.name }}
          </button>
        </nav>
      </div>

      <!-- Generate Reports Tab -->
      <div v-show="activeTab === 'generate'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Generate Custom Reports</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
              <!-- Template Selection -->
              <div class="lg:col-span-1">
                <BaseCard>
                  <div class="p-4">
                    <h3 class="font-medium mb-3">Select Report Template</h3>
                    
                    <div v-if="loadingTemplates" class="text-center py-8">
                      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
                      <p class="mt-2 text-sm text-gray-500">Loading templates...</p>
                    </div>
                    
                    <div v-else class="space-y-3">
                      <div
                        v-for="template in templates"
                        :key="template.id"
                        @click="selectedTemplate = template.id"
                        class="p-3 rounded-lg border cursor-pointer transition-colors"
                        :class="[
                          selectedTemplate === template.id
                            ? 'border-blue-500 bg-blue-50'
                            : 'border-gray-200 hover:border-gray-300'
                        ]"
                      >
                        <div class="flex items-start gap-2">
                          <div class="flex-1">
                            <h4 class="font-medium text-sm">{{ template.name }}</h4>
                            <p class="text-xs text-gray-600 mt-1">{{ template.description }}</p>
                            <span class="inline-block mt-2 px-2 py-1 text-xs rounded-full bg-gray-100 text-gray-800">
                              {{ template.category }}
                            </span>
                          </div>
                          <div v-if="template.isDefault" class="text-blue-500">
                            <span class="text-xs">★</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </BaseCard>
              </div>
              
              <!-- Parameter Configuration -->
              <div class="lg:col-span-2">
                <BaseCard>
                  <div class="p-4">
                    <h3 class="font-medium mb-4">Configure Report Parameters</h3>
                    
                    <div v-if="selectedTemplateDetails" class="space-y-4">
                      <!-- Format Selection -->
                      <div>
                        <label class="block text-sm font-medium text-gray-700 mb-2">Output Format</label>
                        <div class="flex gap-2">
                          <button
                            v-for="format in selectedTemplateDetails.supportedFormats"
                            :key="format"
                            @click="reportFormat = format"
                            class="px-3 py-2 text-sm rounded-md border transition-colors"
                            :class="[
                              reportFormat === format
                                ? 'border-blue-500 bg-blue-50 text-blue-700'
                                : 'border-gray-300 hover:border-gray-400'
                            ]"
                          >
                            {{ format.toUpperCase() }}
                          </button>
                        </div>
                      </div>
                      
                      <!-- Dynamic Parameters -->
                      <div v-for="param in selectedTemplateDetails.parameters" :key="param.name" class="space-y-2">
                        <label class="block text-sm font-medium text-gray-700">
                          {{ param.displayName }}
                          <span v-if="param.required" class="text-red-500">*</span>
                        </label>
                        
                        <div v-if="param.type === 'date-range'">
                          <BaseSelect
                            v-model="reportParameters[param.name]"
                            :options="dateRangeOptions"
                            :placeholder="`Select ${param.displayName}`"
                          />
                        </div>
                        
                        <div v-else-if="param.options && param.options.length > 0">
                          <BaseSelect
                            v-model="reportParameters[param.name]"
                            :options="param.options.map((o: any) => ({ label: o, value: o }))"
                            :placeholder="`Select ${param.displayName}`"
                          />
                        </div>
                        
                        <div v-else>
                          <BaseInput
                            v-model="reportParameters[param.name]"
                            :type="param.type === 'date' ? 'date' : 'text'"
                            :placeholder="`Enter ${param.displayName}`"
                          />
                        </div>
                      </div>
                      
                      <!-- Actions -->
                      <div class="flex gap-3 pt-4">
                        <BaseButton 
                          variant="primary" 
                          @click="generateReport"
                          :disabled="!isFormValid || generating"
                          class="flex-1"
                        >
                          <Send class="w-4 h-4 mr-2" :class="{ 'animate-spin': generating }" />
                          {{ generating ? 'Generating...' : 'Generate Report' }}
                        </BaseButton>
                        
                        <BaseButton 
                          variant="outline" 
                          @click="scheduleReport"
                          :disabled="!isFormValid"
                        >
                          <Clock class="w-4 h-4 mr-2" />
                          Schedule
                        </BaseButton>
                      </div>
                    </div>
                    
                    <div v-else class="text-center py-8 text-gray-500">
                      Select a report template to configure parameters
                    </div>
                  </div>
                </BaseCard>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Export Data Tab -->
      <div v-show="activeTab === 'export'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Export Business Data</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <BaseCard>
                <div class="p-4">
                  <h3 class="font-medium mb-4">Export Configuration</h3>
                  
                  <div class="space-y-4">
                    <BaseSelect
                      v-model="exportEntity"
                      :options="entityOptions"
                      label="Data Entity"
                    />
                    
                    <BaseSelect
                      v-model="exportFormat"
                      :options="formatOptions"
                      label="Export Format"
                    />
                    
                    <BaseSelect
                      v-model="exportFilters.dateRange"
                      :options="dateRangeOptions"
                      label="Date Range"
                    />
                    
                    <BaseButton 
                      variant="primary" 
                      @click="exportData"
                      :disabled="exporting"
                      class="w-full"
                    >
                      <Download class="w-4 h-4 mr-2" :class="{ 'animate-spin': exporting }" />
                      {{ exporting ? 'Exporting...' : 'Export Data' }}
                    </BaseButton>
                  </div>
                </div>
              </BaseCard>
              
              <BaseCard>
                <div class="p-4">
                  <h3 class="font-medium mb-4">Export Statistics</h3>
                  <div ref="exportStatsChartRef" class="w-full h-64"></div>
                </div>
              </BaseCard>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Scheduled Reports Tab -->
      <div v-show="activeTab === 'scheduled'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Scheduled Reports</h2>
            
            <div v-if="scheduledReports.length === 0" class="text-center py-12">
              <Clock class="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <h3 class="text-lg font-medium text-gray-900 mb-2">No Scheduled Reports</h3>
              <p class="text-gray-500">Schedule reports to be automatically generated on a recurring basis.</p>
            </div>
            
            <div v-else class="space-y-3">
              <BaseCard
                v-for="schedule in scheduledReports"
                :key="schedule.id"
                class="hover:shadow-md transition-shadow"
              >
                <div class="p-4 flex justify-between items-center">
                  <div>
                    <h4 class="font-medium">{{ schedule.templateName }}</h4>
                    <p class="text-sm text-gray-600">
                      Cron: {{ schedule.cronExpression }} • Format: {{ schedule.format.toUpperCase() }}
                    </p>
                  </div>
                  <div class="flex items-center gap-2">
                    <span 
                      class="px-2 py-1 text-xs rounded-full"
                      :class="schedule.isActive ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'"
                    >
                      {{ schedule.isActive ? 'Active' : 'Inactive' }}
                    </span>
                    <BaseButton variant="outline" size="sm">Edit</BaseButton>
                  </div>
                </div>
              </BaseCard>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Report History Tab -->
      <div v-show="activeTab === 'history'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Report History</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
              <BaseCard>
                <div class="p-4 text-center">
                  <FileText class="w-8 h-8 text-blue-500 mx-auto mb-2" />
                  <p class="text-2xl font-bold">{{ reportHistory.length }}</p>
                  <p class="text-sm text-gray-600">Total Reports</p>
                </div>
              </BaseCard>
              
              <BaseCard>
                <div class="p-4 text-center">
                  <div ref="reportStatsChartRef" class="w-full h-24"></div>
                </div>
              </BaseCard>
              
              <BaseCard>
                <div class="p-4 text-center">
                  <Download class="w-8 h-8 text-green-500 mx-auto mb-2" />
                  <p class="text-2xl font-bold">
                    {{ reportHistory.filter(r => r.status === 'completed').length }}
                  </p>
                  <p class="text-sm text-gray-600">Completed</p>
                </div>
              </BaseCard>
            </div>
            
            <div v-if="reportHistory.length === 0" class="text-center py-12">
              <FileText class="w-12 h-12 text-gray-300 mx-auto mb-4" />
              <h3 class="text-lg font-medium text-gray-900 mb-2">No Reports Generated</h3>
              <p class="text-gray-500">Generate your first report to see it appear here.</p>
            </div>
            
            <div v-else class="space-y-3">
              <BaseCard
                v-for="report in reportHistory"
                :key="report.id"
                class="hover:shadow-md transition-shadow"
              >
                <div class="p-4 flex justify-between items-center">
                  <div>
                    <h4 class="font-medium">{{ report.templateName }}</h4>
                    <p class="text-sm text-gray-600">
                      {{ formatDate(report.generatedAt) }} • {{ formatFileSize(report.fileSize) }} • {{ report.format.toUpperCase() }}
                    </p>
                  </div>
                  <div class="flex items-center gap-2">
                    <span 
                      class="px-2 py-1 text-xs rounded-full bg-green-100 text-green-800"
                    >
                      {{ report.status }}
                    </span>
                    <BaseButton 
                      variant="outline" 
                      size="sm"
                      @click="downloadReport(report)"
                    >
                      <Download class="w-4 h-4 mr-1" />
                      Download
                    </BaseButton>
                  </div>
                </div>
              </BaseCard>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.reporting-dashboard {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .reporting-dashboard {
    padding: 0.5rem;
  }
}
</style>