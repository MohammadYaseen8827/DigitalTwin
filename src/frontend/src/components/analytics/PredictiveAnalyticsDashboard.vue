<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { requestPrediction, fetchPredictionHistory } from '@/services/predictions.service'
import { TrendingUp, Clock, AlertTriangle, CheckCircle, BarChart3, Zap, Target, Calendar, BrainCircuit, Activity, ShieldCheck, Gauge } from 'lucide-vue-next'

const styles = useCssModule()

// Temporary interface for prediction data
interface PredictionData {
  id: string
  machineId: string
  remainingUsefulLife: number
  confidence: number
  timestamp: string
}

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  ScatterChart as EChartsScatter,
  BarChart as EChartsBar
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
  EChartsLine,
  EChartsScatter,
  EChartsBar,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const predicting = ref(false)
const loadingHistory = ref(false)
const machineId = ref('')
const isValidMachineId = computed(() => {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
  return uuidRegex.test(machineId.value)
})
const predictionHistory = ref<PredictionData[]>([])
const currentPrediction = ref<PredictionData | null>(null)

// Chart refs
const predictionChartRef = ref<HTMLDivElement | null>(null)
const historyChartRef = ref<HTMLDivElement | null>(null)
const confidenceChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let predictionChartInstance: echarts.ECharts | null = null
let historyChartInstance: echarts.ECharts | null = null
let confidenceChartInstance: echarts.ECharts | null = null

// Computed
const avgRul = computed(() => {
  if (predictionHistory.value.length === 0) return 0
  const total = predictionHistory.value.reduce((sum: number, pred: PredictionData) => sum + pred.remainingUsefulLife, 0)
  return Math.round(total / predictionHistory.value.length)
})

const avgConfidence = computed(() => {
  if (predictionHistory.value.length === 0) return 0
  const total = predictionHistory.value.reduce((sum: number, pred: PredictionData) => sum + pred.confidence, 0)
  return (total / predictionHistory.value.length * 100).toFixed(1)
})

const criticalPredictions = computed(() => {
  return predictionHistory.value.filter((pred: PredictionData) => pred.remainingUsefulLife < 30).length
})

const predictionStatus = computed(() => {
  if (!currentPrediction.value) return 'No prediction'
  
  const rul = currentPrediction.value.remainingUsefulLife
  if (rul < 30) return 'Critical'
  if (rul < 90) return 'Warning'
  return 'Healthy'
})

const statusColor = computed(() => {
  const status = predictionStatus.value
  if (status === 'Critical') return 'text-red-600'
  if (status === 'Warning') return 'text-yellow-600'
  if (status === 'Healthy') return 'text-green-600'
  return 'text-gray-600'
})

const statusBgColor = computed(() => {
  const status = predictionStatus.value
  if (status === 'Critical') return 'bg-red-100'
  if (status === 'Warning') return 'bg-yellow-100'
  if (status === 'Healthy') return 'bg-green-100'
  return 'bg-gray-100'
})

// Methods
const makePrediction = async () => {
  try {
    predicting.value = true
    
    const prediction = await requestPrediction(machineId.value)
    currentPrediction.value = prediction
    
    // Add to history
    predictionHistory.value.unshift(prediction)
    
    renderPredictionChart()
    renderConfidenceChart()
    
    const status = predictionStatus.value
    if (status === 'Critical') {
      toast.error(`Critical condition detected: ${prediction.remainingUsefulLife} hours remaining`)
    } else if (status === 'Warning') {
      toast.warning(`Maintenance recommended: ${prediction.remainingUsefulLife} hours remaining`)
    } else {
      toast.success(`Equipment is healthy: ${prediction.remainingUsefulLife} hours remaining`)
    }
  } catch (error) {
    console.error('Failed to make prediction:', error)
    toast.error('Failed to generate prediction')
  } finally {
    predicting.value = false
  }
}

const loadHistory = async () => {
  try {
    loadingHistory.value = true
    
    const history = await fetchPredictionHistory(machineId.value, 50)
    predictionHistory.value = history
    
    renderHistoryChart()
    renderConfidenceChart()
    
    toast.success(`Loaded ${history.length} historical predictions`)
  } catch (error) {
    console.error('Failed to load history:', error)
    toast.error('Failed to load prediction history')
  } finally {
    loadingHistory.value = false
  }
}

const historyRange = ref('50')

const kpiCards = computed(() => [
  { label: 'Avg. RUL Estimation', value: `${avgRul.value}h`, icon: Clock, variant: 'primary' },
  { label: 'Forecast Confidence', value: `${avgConfidence.value}%`, icon: ShieldCheck, variant: 'success' },
  { label: 'Anomaly Criticality', value: criticalPredictions.value, icon: AlertTriangle, variant: 'danger' },
  { label: 'Processed Insights', value: predictionHistory.value.length, icon: BrainCircuit, variant: 'info' },
])

const predictionStatusVariant = computed(() => {
  const status = predictionStatus.value
  if (status === 'Critical') return 'danger'
  if (status === 'Warning') return 'warning'
  return 'success'
})

function getConfidenceLevel(conf: number) {
  if (conf >= 0.8) return 'high'
  if (conf >= 0.6) return 'med'
  return 'low'
}

function normalizeStatus(rul: number) {
  if (rul < 30) return 'Critical'
  if (rul < 90) return 'Warning'
  return 'Healthy'
}

function getStatusVariant(rul: number) {
  if (rul < 30) return 'danger'
  if (rul < 90) return 'warning'
  return 'success'
}

const renderPredictionChart = () => {
  if (!predictionChartRef.value || !currentPrediction.value) return
  
  if (!predictionChartInstance) {
    predictionChartInstance = echarts.init(predictionChartRef.value, 'hub-dark')
  }

  const rul = currentPrediction.value.remainingUsefulLife
  const confidence = currentPrediction.value.confidence * 100
  
  const option = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(5, 7, 10, 0.9)',
      borderColor: 'rgba(255, 255, 255, 0.1)',
      textStyle: { color: '#F8FAFC' }
    },
    xAxis: {
      type: 'category',
      data: ['RUL (Hours)', 'Confidence (%)'],
      axisLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.1)' } }
    },
    yAxis: [
      { type: 'value', splitLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.05)' } } },
      { type: 'value', min: 0, max: 100, splitLine: { show: false } }
    ],
    series: [
      {
        type: 'bar',
        barWidth: '40%',
        data: [
          { 
            value: rul, 
            itemStyle: { 
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                { offset: 0, color: rul < 30 ? '#ef4444' : '#3b82f6' },
                { offset: 1, color: 'rgba(59, 130, 246, 0.1)' }
              ]),
              borderRadius: [4, 4, 0, 0]
            } 
          },
          { 
            value: confidence, 
            yAxisIndex: 1,
            itemStyle: { 
              color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
                { offset: 0, color: '#10b981' },
                { offset: 1, color: 'rgba(16, 185, 129, 0.1)' }
              ]),
              borderRadius: [4, 4, 0, 0]
            } 
          }
        ]
      }
    ]
  }
  
  predictionChartInstance.setOption(option, true)
}

const renderHistoryChart = () => {
  if (!historyChartRef.value || predictionHistory.value.length === 0) return
  
  if (!historyChartInstance) {
    historyChartInstance = echarts.init(historyChartRef.value, 'dark')
  }
  
  const timestamps = predictionHistory.value.slice(0, 20).reverse().map((pred: PredictionData) => {
    const date = new Date(pred.timestamp)
    return `${date.getHours()}:${date.getMinutes().toString().padStart(2, '0')}`
  })
  
  const rulValues = predictionHistory.value.slice(0, 20).reverse().map((pred: PredictionData) => pred.remainingUsefulLife)
  
  const option = {
    backgroundColor: 'transparent',
    grid: { left: '3%', right: '4%', bottom: '3%', containLabel: true },
    tooltip: { trigger: 'axis' },
    xAxis: { 
      type: 'category', 
      data: timestamps,
      axisLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.1)' } }
    },
    yAxis: { 
      type: 'value',
      splitLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.05)' } }
    },
    series: [
      {
        name: 'RUL Trend',
        type: 'line',
        data: rulValues,
        smooth: true,
        showSymbol: false,
        lineStyle: { width: 3, color: '#3b82f6' },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(59, 130, 246, 0.2)' },
            { offset: 1, color: 'transparent' }
          ])
        }
      }
    ]
  }
  
  historyChartInstance.setOption(option, true)
}

const renderConfidenceChart = () => {
  if (!confidenceChartRef.value) return
  
  if (!confidenceChartInstance) {
    confidenceChartInstance = echarts.init(confidenceChartRef.value, 'dark')
  }
  
  const highConf = predictionHistory.value.filter((p: PredictionData) => p.confidence >= 0.8).length
  const medConf = predictionHistory.value.filter((p: PredictionData) => p.confidence >= 0.6 && p.confidence < 0.8).length
  const lowConf = predictionHistory.value.filter((p: PredictionData) => p.confidence < 0.6).length
  
  const option = {
    backgroundColor: 'transparent',
    tooltip: { trigger: 'item' },
    series: [
      {
        type: 'pie',
        radius: ['60%', '85%'],
        avoidLabelOverlap: false,
        itemStyle: { borderRadius: 10, borderColor: 'var(--color-depth-0)', borderWidth: 2 },
        label: { show: false },
        data: [
          { value: highConf, name: 'High Confidence', itemStyle: { color: '#10b981' } },
          { value: medConf, name: 'Standard', itemStyle: { color: '#f59e0b' } },
          { value: lowConf, name: 'Low/Review', itemStyle: { color: '#ef4444' } }
        ]
      }
    ]
  }
  
  confidenceChartInstance.setOption(option, true)
}

const formatDate = (timestamp: string) => {
  return new Date(timestamp).toLocaleString()
}

const getRulColor = (rul: number) => {
  if (rul < 30) return 'text-red-600'
  if (rul < 90) return 'text-yellow-600'
  return 'text-green-600'
}

const getConfidenceColor = (confidence: number) => {
  if (confidence >= 0.8) return 'text-green-600'
  if (confidence >= 0.6) return 'text-yellow-600'
  return 'text-red-600'
}

const resizeCharts = () => {
  predictionChartInstance?.resize()
  historyChartInstance?.resize()
  confidenceChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  loadHistory()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  predictionChartInstance?.dispose()
  historyChartInstance?.dispose()
  confidenceChartInstance?.dispose()
}
</script>

<template>
  <div :class="styles['dashboard-container']">
    <!-- Header Area -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <BrainCircuit :width="14" :height="14" />
          <span>Intelligence Engine</span>
        </div>
        <h1 :class="styles['title']">Predictive Analytics</h1>
        <p :class="styles['description']">High-fidelity RUL estimation and failure probability modeling</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton 
          variant="secondary" 
          @click="loadHistory"
          :disabled="loadingHistory"
        >
          <History :width="16" :height="16" />
          Sync History
        </UiButton>
        <UiButton 
          variant="primary" 
          @click="makePrediction"
          :loading="predicting"
        >
          <Zap :width="16" :height="16" />
          Generate Forecast
        </UiButton>
      </div>
    </header>

    <div :class="styles['content-layout']">
      <!-- Target Selection & Global Stats -->
      <section :class="styles['top-strip']">
        <UiCard variant="default" padding="md" :class="styles['selection-card']">
          <div :class="styles['selection-content']">
            <div :class="styles['selection-info']">
              <h3 :class="styles['section-title']">Asset Targeting</h3>
              <p :class="styles['section-sub']">Target machine for AI inference</p>
            </div>
            <div :class="styles['selection-form']">
              <UiInput
                v-model="machineId"
                placeholder="Machine UUID (e.g. CNC-01...)"
                :class="styles['target-input']"
              >
                <template #prefix><Target :width="14" :height="14" /></template>
              </UiInput>
              <UiButton variant="outline" @click="loadHistory" :disabled="!isValidMachineId">
                Load History
              </UiButton>
            </div>
          </div>
        </UiCard>

        <!-- KPI Bento Grid -->
        <div :class="styles['kpi-grid']">
          <UiCard v-for="kpi in kpiCards" :key="kpi.label" variant="glass" padding="sm" :class="styles['kpi-card']">
            <div :class="styles['kpi-inner']">
              <div :class="[styles['kpi-icon'], styles[`kpi-icon--${kpi.variant}`]]">
                <component :is="kpi.icon" :width="18" :height="18" />
              </div>
              <div :class="styles['kpi-data']">
                <span :class="styles['kpi-value']">{{ kpi.value }}</span>
                <span :class="styles['kpi-label']">{{ kpi.label }}</span>
              </div>
            </div>
          </UiCard>
        </div>
      </section>

      <!-- Main Stage -->
      <main :class="styles['stage']">
        <!-- Latest Prediction Spotlight -->
        <div v-if="currentPrediction" :class="styles['viewport-stage']">
          <UiCard variant="glass" padding="lg" :class="styles['spotlight-card']">
            <template #header>
              <div :class="styles['spotlight-header']">
                <div :class="styles['spotlight-title-group']">
                  <UiBadge variant="primary" dot>Live Forecast</UiBadge>
                  <h2 :class="styles['spotlight-title']">Current System Integrity</h2>
                </div>
                <UiBadge :variant="predictionStatusVariant" outline>{{ predictionStatus }}</UiBadge>
              </div>
            </template>

            <div :class="styles['spotlight-content']">
              <div :class="styles['integrity-summary']">
                <div :class="styles['main-metric']">
                  <div :class="styles['metric-circle']">
                    <div :class="styles['scanner-line']" v-if="predicting" />
                    <span :class="[styles['metric-value'], styles[`status--${predictionStatus.toLowerCase()}`]]">
                      {{ currentPrediction.remainingUsefulLife }}
                    </span>
                    <span :class="styles['metric-unit']">HOURS RUL</span>
                  </div>
                </div>

                <div :class="styles['metric-details']">
                  <div :class="styles['detail-row']">
                    <span :class="styles['detail-label']">Confidence Interval</span>
                    <span :class="[styles['detail-value'], styles[`conf--${getConfidenceLevel(currentPrediction.confidence)}`]]">
                      {{ (currentPrediction.confidence * 100).toFixed(1) }}%
                    </span>
                  </div>
                  <div :class="styles['detail-row']">
                    <span :class="styles['detail-label']">Inference Model</span>
                    <span :class="styles['detail-value']">XGBoost-V4-Hybrid</span>
                  </div>
                  <div :class="styles['detail-row']">
                    <span :class="styles['detail-label']">Last Analysis</span>
                    <span :class="styles['detail-value']">{{ formatDate(currentPrediction.timestamp) }}</span>
                  </div>
                  <div :class="styles['progress-bar-wrap']">
                    <div :class="styles['progress-track']">
                      <div 
                        :class="[styles['progress-fill'], styles[`fill--${predictionStatus.toLowerCase()}`]]"
                        :style="{ width: `${Math.min(100, (currentPrediction.remainingUsefulLife / 200) * 100)}%` }"
                      />
                    </div>
                  </div>
                </div>
              </div>

              <!-- Live Prediction Chart -->
              <div :class="styles['chart-container']">
                <div ref="predictionChartRef" :class="styles['chart-canvas']" />
              </div>
            </div>
          </UiCard>
        </div>

        <!-- Trend Analysis -->
        <div :class="styles['trend-section']">
          <div :class="styles['section-header']">
            <h2 :class="styles['section-title']">Temporal Trend Analysis</h2>
            <div :class="styles['section-controls']">
              <UiSelect v-model="historyRange" size="sm" :class="styles['range-select']">
                <option value="50">Last 50 Records</option>
                <option value="100">Last 100 Records</option>
                <option value="all">Full History</option>
              </UiSelect>
            </div>
          </div>

          <div :class="styles['trend-grid']">
            <UiCard variant="default" padding="md" :class="styles['trend-main']">
              <div ref="historyChartRef" :class="styles['history-canvas']" />
            </UiCard>
            <UiCard variant="default" padding="md" :class="styles['trend-side']">
              <div ref="confidenceChartRef" :class="styles['confidence-canvas']" />
            </UiCard>
          </div>
        </div>

        <!-- History Log -->
        <div v-if="predictionHistory.length > 0" :class="styles['log-section']">
          <UiCard variant="default" padding="none">
            <div :class="styles['table-wrap']">
              <table :class="styles['table']">
                <thead>
                  <tr>
                    <th>Analysis Timestamp</th>
                    <th>RUL Estimate</th>
                    <th>Confidence</th>
                    <th>Risk Profile</th>
                    <th />
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="prediction in predictionHistory.slice(0, 10)" :key="prediction.id">
                    <td>
                      <div :class="styles['cell-timestamp']">
                        <Calendar :width="12" :height="12" />
                        {{ formatDate(prediction.timestamp) }}
                      </div>
                    </td>
                    <td>
                      <span :class="[styles['cell-rul'], styles[`status--${normalizeStatus(prediction.remainingUsefulLife).toLowerCase()}`]]">
                        {{ prediction.remainingUsefulLife }}h
                      </span>
                    </td>
                    <td>
                      <div :class="styles['cell-conf']">
                        <div :class="styles['conf-bar-track']">
                          <div :class="[styles['conf-bar-fill'], styles[`conf-bg--${getConfidenceLevel(prediction.confidence)}`]]" :style="{ width: `${prediction.confidence * 100}%` }" />
                        </div>
                        <span>{{ (prediction.confidence * 100).toFixed(0) }}%</span>
                      </div>
                    </td>
                    <td>
                      <UiBadge :variant="getStatusVariant(prediction.remainingUsefulLife)" size="sm">
                        {{ normalizeStatus(prediction.remainingUsefulLife) }}
                      </UiBadge>
                    </td>
                    <td :class="styles['cell-actions']">
                      <UiButton variant="ghost" size="sm">Details</UiButton>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </UiCard>
        </div>
      </main>
    </div>
  </div>
</template>

<style module>
.dashboard-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  padding: var(--space-24);
  max-width: 1600px;
  margin: 0 auto;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-20);
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
  letter-spacing: 0.1em;
  color: var(--color-primary);
  margin-bottom: var(--space-4);
}

.title {
  font-size: var(--font-size-4xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}

.description {
  color: var(--color-text-muted);
  font-size: var(--font-size-sm);
  margin-top: var(--space-4);
}

.header-actions {
  display: flex;
  gap: var(--space-12);
}

/* Layout Sections */
.content-layout {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.top-strip {
  display: grid;
  grid-template-columns: 1fr 1.5fr;
  gap: var(--space-20);
}

.selection-card {
  height: 100%;
}

.selection-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.selection-form {
  display: flex;
  gap: var(--space-12);
  align-items: flex-end;
}

.target-input {
  flex: 1;
}

/* KPI Grid */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-12);
}

.kpi-inner {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.kpi-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.kpi-icon--primary { background: var(--color-primary-muted); color: var(--color-primary); }
.kpi-icon--success { background: var(--color-success-muted); color: var(--color-success); }
.kpi-icon--danger { background: var(--color-danger-muted); color: var(--color-danger); }
.kpi-icon--info { background: var(--color-info-muted); color: var(--color-info); }

.kpi-data {
  display: flex;
  flex-direction: column;
}

.kpi-value {
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1;
}

.kpi-label {
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-top: 2px;
}

/* Spotlight Stage */
.viewport-stage {
  margin-bottom: var(--space-24);
}

.spotlight-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.spotlight-title-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.spotlight-title {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.spotlight-content {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-40);
  align-items: center;
}

.integrity-summary {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.main-metric {
  display: flex;
  justify-content: center;
}

.metric-circle {
  width: 200px;
  height: 200px;
  border-radius: 50%;
  border: 4px solid var(--color-border);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  background: radial-gradient(circle at center, rgba(var(--color-primary-rgb), 0.05) 0%, transparent 70%);
  box-shadow: inset 0 0 40px rgba(0, 0, 0, 0.5);
}

.scanner-line {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background: var(--color-primary);
  box-shadow: 0 0 15px var(--color-primary);
  animation: scan 2s linear infinite;
  z-index: 10;
}

@keyframes scan {
  0% { top: 10%; opacity: 0; }
  10% { opacity: 1; }
  90% { opacity: 1; }
  100% { top: 90%; opacity: 0; }
}

.metric-value {
  font-size: 64px;
  font-weight: 900;
  line-height: 1;
  font-family: var(--font-mono);
  letter-spacing: -0.05em;
}

.metric-unit {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.2em;
}

.metric-details {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-8) 0;
  border-bottom: 1px solid var(--color-border-subtle);
}

.detail-label {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.detail-value {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
}

.progress-bar-wrap {
  margin-top: var(--space-8);
}

.progress-track {
  height: 6px;
  background: var(--color-depth-0);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: var(--radius-full);
  transition: width 1s var(--ease-spring);
}

/* Status colors */
.status--critical { color: var(--color-danger); text-shadow: 0 0 20px rgba(239, 68, 68, 0.3); }
.status--warning { color: var(--color-warning); text-shadow: 0 0 20px rgba(245, 158, 11, 0.3); }
.status--healthy { color: var(--color-success); text-shadow: 0 0 20px rgba(16, 185, 129, 0.3); }

.fill--critical { background: var(--color-danger); box-shadow: 0 0 10px var(--color-danger); }
.fill--warning { background: var(--color-warning); box-shadow: 0 0 10px var(--color-warning); }
.fill--healthy { background: var(--color-success); box-shadow: 0 0 10px var(--color-success); }

/* Trend Analysis */
.trend-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.trend-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: var(--space-20);
}

.history-canvas, .confidence-canvas {
  width: 100%;
  height: 320px;
}

.chart-canvas {
  width: 100%;
  height: 300px;
}

/* Table */
.table-wrap {
  overflow-x: auto;
}

.table {
  width: 100%;
  border-collapse: collapse;
}

.table th {
  text-align: left;
  padding: var(--space-12) var(--space-20);
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.1em;
  border-bottom: 1px solid var(--color-border);
}

.table td {
  padding: var(--space-16) var(--space-20);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  border-bottom: 1px solid var(--color-border-subtle);
}

.table tr:hover td {
  background: rgba(255, 255, 255, 0.01);
}

.cell-timestamp {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-family: var(--font-mono);
  font-size: var(--font-size-xs);
}

.cell-rul {
  font-weight: 700;
  font-family: var(--font-mono);
}

.cell-conf {
  display: flex;
  align-items: center;
  gap: var(--space-12);
  font-size: var(--font-size-xs);
}

.conf-bar-track {
  width: 60px;
  height: 4px;
  background: var(--color-depth-0);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.conf-bar-fill {
  height: 100%;
}

.conf-bg--high { background: var(--color-success); }
.conf-bg--med { background: var(--color-warning); }
.conf-bg--low { background: var(--color-danger); }

@media (max-width: 1024px) {
  .top-strip { grid-template-columns: 1fr; }
  .spotlight-content { grid-template-columns: 1fr; gap: var(--space-24); }
  .trend-grid { grid-template-columns: 1fr; }
}
</style>