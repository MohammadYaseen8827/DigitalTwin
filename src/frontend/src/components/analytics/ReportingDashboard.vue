<template>
  <div :class="styles['reporting-container']">
    <!-- Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <FileText :width="14" :height="14" />
          <span>Intelligence Synthesis</span>
        </div>
        <h1 :class="styles['title']">Reporting & Data Archival</h1>
        <p :class="styles['description']">Automated executive summaries and deep-dive data exports</p>
      </div>

      <div :class="styles['header-tabs']">
        <button 
          v-for="tab in ['generate', 'history', 'schedules']" 
          :key="tab"
          :class="[styles['tab-btn'], activeTab === tab && styles['tab-btn--active']]"
          @click="activeTab = tab"
        >
          {{ tab.charAt(0).toUpperCase() + tab.slice(1) }}
        </button>
      </div>
    </header>

    <!-- KPI Bento -->
    <div :class="styles['kpi-grid']">
      <UiCard variant="glass" hover padding="md" :class="styles['kpi-card']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--blue']]"><Database :width="20" :height="20" /></div>
          <div :class="styles['kpi-data']">
            <span :class="styles['kpi-label']">Data Lake Volume</span>
            <div :class="styles['kpi-value']">4.2 TB</div>
          </div>
        </div>
      </UiCard>
      <UiCard variant="glass" hover padding="md" :class="styles['kpi-card']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--emerald']]"><Zap :width="20" :height="20" /></div>
          <div :class="styles['kpi-data']">
            <span :class="styles['kpi-label']">Sync Efficiency</span>
            <div :class="styles['kpi-value']">99.8%</div>
          </div>
        </div>
      </UiCard>
      <UiCard variant="glass" hover padding="md" :class="styles['kpi-card']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--amber']]"><Clock :width="20" :height="20" /></div>
          <div :class="styles['kpi-data']">
            <span :class="styles['kpi-label']">Avg Synthesis Time</span>
            <div :class="styles['kpi-value']">1.4s</div>
          </div>
        </div>
      </UiCard>
    </div>

    <!-- Main Content -->
    <div :class="styles['main-layout']">
      <!-- Synthesis Panel -->
      <div v-if="activeTab === 'generate'" :class="styles['synthesis-grid']">
        <UiCard variant="default" padding="lg" :class="styles['form-card']">
          <template #header>
            <div :class="styles['card-header-inner']">
              <div :class="styles['card-title-wrap']">
                <h3 :class="styles['card-title']">Report Synthesis</h3>
                <p :class="styles['card-sub']">Configure intelligence parameters for artifact generation</p>
              </div>
            </div>
          </template>
          
          <div :class="styles['form-body']">
            <div :class="styles['form-group']">
              <label :class="styles['field-label']">Intelligence Template</label>
              <UiSelect v-model="selectedTemplate" @change="initializeParameters">
                <option v-for="t in templates" :key="t.id" :value="t.id">{{ t.name }}</option>
              </UiSelect>
            </div>

            <div :class="styles['form-grid-row']">
              <div :class="styles['form-group']">
                <label :class="styles['field-label']">Output Format</label>
                <UiSelect v-model="reportFormat">
                  <option v-for="opt in formatOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                </UiSelect>
              </div>
              <div :class="styles['form-group']">
                <label :class="styles['field-label']">Temporal Range</label>
                <UiSelect v-model="exportFilters.dateRange">
                  <option v-for="opt in dateRangeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
                </UiSelect>
              </div>
            </div>

            <div :class="styles['form-footer']">
               <UiButton variant="ghost" @click="scheduleReport">Schedule Protocol</UiButton>
               <UiButton variant="primary" :loading="generating" @click="generateReport">
                 <Zap :width="14" :height="14" />
                 Synthesize Report
               </UiButton>
            </div>
          </div>
        </UiCard>

        <UiCard variant="default" padding="lg" :class="styles['analytics-card']">
          <template #header>
            <h3 :class="styles['card-title']">Format Distribution</h3>
          </template>
          <div ref="reportStatsChartRef" :class="styles['chart-canvas']" />
        </UiCard>
      </div>

      <!-- History View -->
      <div v-if="activeTab === 'history'" :class="styles['history-view']">
        <UiCard variant="default" padding="none" :class="styles['table-card']">
          <table :class="styles['data-table']">
            <thead>
              <tr>
                <th>Artifact Name</th>
                <th>Type</th>
                <th>Generated</th>
                <th>Size</th>
                <th>Status</th>
                <th :class="styles['text-right']">Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="report in reportHistory" :key="report.id">
                <td>
                  <div :class="styles['report-name-cell']">
                    <FileText :width="14" :height="14" :class="styles['file-icon']" />
                    <span>{{ report.fileName }}</span>
                  </div>
                </td>
                <td><UiBadge size="sm">{{ report.format }}</UiBadge></td>
                <td>{{ new Date(report.createdAt).toLocaleDateString() }}</td>
                <td :class="styles['mono']">{{ (report.size / 1024).toFixed(1) }} KB</td>
                <td><UiBadge variant="success" dot>Synchronized</UiBadge></td>
                <td :class="styles['text-right']">
                  <UiButton variant="ghost" size="sm" @click="downloadReport(report)">
                    <Download :width="14" :height="14" />
                  </UiButton>
                </td>
              </tr>
            </tbody>
          </table>
          <div v-if="reportHistory.length === 0" :class="styles['empty-table']">
            No artifacts found in the digital archive.
          </div>
        </UiCard>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, useCssModule, shallowRef } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { FileText, Download, Zap, Database, Clock, History, FileSpreadsheet } from 'lucide-vue-next'
import { reportingService } from '@/services/reporting.service'

const styles = useCssModule()
const toast = useToast()

import * as echarts from 'echarts/core'
import { BarChart, PieChart, LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

echarts.use([BarChart, PieChart, LineChart, GridComponent, TooltipComponent, LegendComponent, TitleComponent, CanvasRenderer])

// State
const activeTab = ref('generate')
const generating = ref(false)
const templates = ref<any[]>([])
const reportHistory = ref<any[]>([])
const selectedTemplate = ref('')
const reportFormat = ref('pdf')
const exportFilters = ref({ dateRange: 'last30days' })

const reportStatsChartRef = ref<HTMLDivElement | null>(null)
const chartInstances = shallowRef<Record<string, echarts.ECharts>>({})

const formatOptions = [
  { label: 'Portable Document (PDF)', value: 'pdf' },
  { label: 'Excel Spreadsheet', value: 'excel' },
  { label: 'Tabular CSV', value: 'csv' }
]

const dateRangeOptions = [
  { label: 'Last 7 Days', value: 'last7days' },
  { label: 'Last 30 Days', value: 'last30days' },
  { label: 'Last 90 Days', value: 'last90days' }
]

// Methods
const loadTemplates = async () => {
  templates.value = await reportingService.getReportTemplates()
  if (templates.value.length > 0) selectedTemplate.value = templates.value[0].id
}

const loadHistory = async () => {
  const response = await reportingService.getReportHistory()
  reportHistory.value = response.items
  setTimeout(() => renderStats(), 50)
}

const generateReport = async () => {
  generating.value = true
  try {
    await reportingService.generateReport({ templateId: selectedTemplate.value, format: reportFormat.value, parameters: {} })
    toast.success('Intelligence artifact synthesized')
    await loadHistory()
  } finally {
    generating.value = false
  }
}

const renderStats = () => {
  if (!reportStatsChartRef.value) return
  if (!chartInstances.value.stats) chartInstances.value.stats = echarts.init(reportStatsChartRef.value, 'hub-dark')
  
  chartInstances.value.stats.setOption({
    backgroundColor: 'transparent',
    tooltip: { trigger: 'item' },
    series: [{
      type: 'pie',
      radius: ['65%', '90%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 8, borderColor: 'var(--color-depth-0)', borderWidth: 2 },
      label: { show: false },
      data: [
        { value: 45, name: 'PDF', itemStyle: { color: '#EF4444' } },
        { value: 25, name: 'Excel', itemStyle: { color: '#10B981' } },
        { value: 30, name: 'CSV', itemStyle: { color: '#3B82F6' } }
      ]
    }]
  })
}

const downloadReport = (report: any) => { toast.info(`Downloading ${report.fileName}`) }
const scheduleReport = () => { toast.success('Generation protocol scheduled') }
const initializeParameters = () => {}

onMounted(() => {
  loadTemplates()
  loadHistory()
})
</script>

<style module>
.reporting-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  max-width: 1400px;
  margin: 0 auto;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.eyebrow {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--color-primary);
  margin-bottom: var(--space-6);
}

.title {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  letter-spacing: -0.03em;
  color: var(--color-text-primary);
  margin: 0;
}

.description {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.header-tabs {
  display: flex;
  background: var(--color-depth-1);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-subtle);
}

.tab-btn {
  padding: var(--space-6) var(--space-16);
  border-radius: var(--radius-md);
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-muted);
  background: transparent;
  border: none;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.tab-btn--active {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  box-shadow: var(--shadow-sm);
}

/* KPI Grid */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-16);
}

.kpi-inner {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.kpi-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
}

.kpi-icon--blue { color: var(--color-primary); }
.kpi-icon--emerald { color: var(--color-success); }
.kpi-icon--amber { color: var(--color-warning); }

.kpi-data {
  display: flex;
  flex-direction: column;
}

.kpi-label {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.kpi-value {
  font-size: var(--font-size-xl);
  font-weight: 800;
  color: var(--color-text-primary);
}

/* Main Layout */
.main-layout {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.synthesis-grid {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: var(--space-24);
}

.form-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.form-grid-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-16);
}

.field-label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-dim);
  margin-bottom: var(--space-6);
  display: block;
}

.form-footer {
  display: flex;
  justify-content: space-between;
  padding-top: var(--space-12);
  border-top: 1px solid var(--color-border-subtle);
}

.chart-canvas {
  height: 240px;
  width: 100%;
}

.card-title {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.card-sub {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

/* Table */
.table-card {
  overflow: hidden;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

.data-table th {
  background: var(--color-depth-1);
  padding: var(--space-12) var(--space-20);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-dim);
  letter-spacing: 0.05em;
  border-bottom: 1px solid var(--color-border-subtle);
}

.data-table td {
  padding: var(--space-16) var(--space-20);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  border-bottom: 1px solid var(--color-border-subtle);
}

.data-table tr:hover td {
  background: rgba(255, 255, 255, 0.01);
  color: var(--color-text-primary);
}

.report-name-cell {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  font-weight: 600;
}

.file-icon {
  color: var(--color-primary);
}

.mono {
  font-family: var(--font-mono);
  font-size: 11px;
}

.text-right { text-align: right; }

.empty-table {
  padding: var(--space-48);
  text-align: center;
  color: var(--color-text-dim);
}

@media (max-width: 1024px) {
  .synthesis-grid { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
  .kpi-grid { grid-template-columns: 1fr; }
}
</style>
