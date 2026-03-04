<template>
  <div :class="styles['telemetry-dashboard']">
    <header :class="styles['dashboard-header']">
      <div :class="styles['header-main']">
        <div :class="styles['header-indicator']">
          <div :class="styles['pulse-dot']" />
          <span :class="styles['indicator-label']">Neural Telemetry Stream</span>
        </div>
        <h2 :class="styles['header-title']">Telemetric Intelligence</h2>
        <p v-if="lastUpdated" :class="styles['header-sub']">
          Gateway synchronized <span :class="styles['time-accent']">{{ lastUpdatedLabel }}</span>
        </p>
      </div>

      <div :class="styles['header-controls']">
        <div :class="styles['control-box']">
          <Clock :width="14" :height="14" />
          <select v-model="timeRange" @change="fetchTelemetryData">
            <option value="1h">Last Hour</option>
            <option value="6h">6 Hours</option>
            <option value="12h">12 Hours</option>
            <option value="24h">24 Hours</option>
          </select>
        </div>
        
        <div :class="styles['control-box']">
          <Cpu :width="14" :height="14" />
          <select v-model="selectedMachineId" @change="fetchTelemetryData">
            <option value="">Global Fleet</option>
            <option v-for="machine in machines" :key="machine.id" :value="machine.id">
              {{ machine.name }}
            </option>
          </select>
        </div>
      </div>
    </header>

    <!-- KPI Bento Grid -->
    <div v-if="!loading" :class="styles['kpi-grid']">
      <UiCard variant="glass" padding="md" hover :class="[styles['kpi-item'], styles['kpi-item--primary']]">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--emerald']]"><Activity :width="18" :height="18" /></div>
          <div :class="styles['kpi-data']">
            <MetricValue :value="telemetryData.length" :class="styles['kpi-value']" />
            <span :class="styles['kpi-label']">Data Packets / Period</span>
          </div>
          <div :class="styles['kpi-trend']">
            <TrendingUp :width="12" :height="12" />
            <span>+12.4%</span>
          </div>
        </div>
      </UiCard>
      
      <UiCard variant="glass" padding="md" hover :class="styles['kpi-item']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--indigo']]"><Layers :width="18" :height="18" /></div>
          <div :class="styles['kpi-data']">
            <MetricValue :value="reportingMachineCount" :class="styles['kpi-value']" />
            <span :class="styles['kpi-label']">Active Cluster Nodes</span>
          </div>
        </div>
      </UiCard>

      <UiCard variant="glass" padding="md" hover :class="styles['kpi-item']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--success']]"><Zap :width="18" :height="18" /></div>
          <div :class="styles['kpi-data']">
            <span :class="[styles['kpi-value'], styles['kpi-value--success']]">Optimal</span>
            <span :class="styles['kpi-label']">Link Stability</span>
          </div>
        </div>
      </UiCard>

      <UiCard variant="glass" padding="md" hover :class="styles['kpi-item']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles['kpi-icon--cyan']]"><History :width="18" :height="18" /></div>
          <div :class="styles['kpi-data']">
            <div :class="styles['latency-box']">
              <MetricValue :value="12" :class="styles['kpi-value']" />
              <span :class="styles['unit']">ms</span>
            </div>
            <span :class="styles['kpi-label']">End-to-End Latency</span>
          </div>
        </div>
      </UiCard>
    </div>

    <div v-if="!loading && !hasTelemetry" :class="styles['empty-stage']">
      <div :class="styles['empty-icon-wrap']">
        <Radio :width="40" :height="40" />
      </div>
      <h3>Signal Lost</h3>
      <p>No telemetry packets found. Adjusted search parameters may be required.</p>
      <UiButton variant="outline" size="sm" @click="fetchTelemetryData">Re-scan Gateway</UiButton>
    </div>

    <div v-else :class="styles['chart-grid']">
      <UiCard v-for="chart in chartConfigs" :key="chart.type" variant="default" padding="lg" hover :class="styles['chart-item']">
        <div :class="styles['chart-header-wrap']">
          <div :class="styles['chart-copy']">
            <h4 :class="styles['chart-title']">{{ chart.label }}</h4>
            <p :class="styles['chart-desc']">{{ chart.description }}</p>
          </div>
          <div :class="styles['chart-icon-box']" :style="{ color: chart.color, backgroundColor: `color-mix(in srgb, ${chart.color} 10%, transparent)` }">
            <component :is="chart.icon" :width="16" :height="16" />
          </div>
        </div>
        
        <div v-if="loading" :class="styles['chart-loader']">
           <div :class="styles['spinner']" />
        </div>
        <div v-else :ref="el => setChartRef(el, chart.type)" :class="styles['chart-canvas']"></div>
      </UiCard>
    </div>

    <!-- Manual Injection -->
    <div :class="styles['injection-section']">
       <div :class="styles['section-header']">
          <div :class="styles['header-icon']"><Plus :width="18" :height="18" /></div>
          <div :class="styles['header-text']">
             <h3>Data Ingestion Gateway</h3>
             <p>Asynchronous manual record injection for untracked hardware components.</p>
          </div>
       </div>
       <UiCard variant="default" padding="lg" :class="styles['injection-card']">
         <ManualTelemetryForm />
       </UiCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, computed, shallowRef, useCssModule } from 'vue'
import { useToast } from '@/composables/useToast'
import * as echarts from 'echarts/core'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

import UiCard from '../ui/UiCard.vue'
import UiButton from '../ui/UiButton.vue'
import UiBadge from '../ui/UiBadge.vue'
import MetricValue from '../base/MetricValue.vue'
import ManualTelemetryForm from './ManualTelemetryForm.vue'
import { 
  Activity, 
  Layers, 
  Zap, 
  History, 
  Clock, 
  Cpu, 
  Radio, 
  Thermometer, 
  Wind, 
  Zap as Power, 
  Boxes, 
  Target,
  Plus,
  TrendingUp
} from 'lucide-vue-next'

import { fetchMachines } from '@/services/machines.service'
import { fetchRecentTelemetry, fetchTelemetryByMachine } from '@/services/telemetry.service'
import type { MachineDto, TelemetryDto } from '@/api/types'

echarts.use([LineChart, GridComponent, TooltipComponent, TitleComponent, CanvasRenderer])

const props = defineProps<{
  machines?: MachineDto[]
  selectedMachineId?: string | null
}>()

const emit = defineEmits<{
  (e: 'machine-selected', machineId: string | ''): void
}>()

const localMachines = ref<MachineDto[]>([])
const computedMachines = computed(() => (props.machines?.length ? props.machines : localMachines.value))
const telemetryData = ref<TelemetryDto[]>([])
const timeRange = ref('1h')
const selectedMachineId = ref(props.selectedMachineId ?? '')
const lastUpdated = ref<Date | null>(null)
const loading = ref(false)

const machines = computed(() => computedMachines.value)
const chartInstances = shallowRef<Record<string, echarts.ECharts>>({})
const chartRefs = ref<Record<string, HTMLElement>>({})
const styles = useCssModule()

const chartConfigs = [
  { type: 'pressure', label: 'Hydraulic Pressure', description: 'PSI stability metrics', icon: Boxes, color: 'var(--color-primary)' },
  { type: 'humidity', label: 'Ambient Humidity', description: 'Relative environment %', icon: Wind, color: 'var(--color-cyan)' },
  { type: 'power_consumption', label: 'Energy Load', description: 'Active kilowatt footprint', icon: Power, color: 'var(--color-warning)' },
  { type: 'temperature', label: 'Core Temperature', description: 'Machine thermal equilibrium', icon: Thermometer, color: 'var(--color-danger)' },
  { type: 'vibration', label: 'Acoustic Vibration', description: 'Harmonic resonance in mm/s', icon: Activity, color: 'var(--color-violet)' },
  { type: 'production_count', label: 'Unit Throughput', description: 'Total cycles per interval', icon: Target, color: 'var(--color-success)' },
]

const hasTelemetry = computed(() => telemetryData.value.length > 0)
const reportingMachineCount = computed(() => new Set(telemetryData.value.map(d => d.machineId)).size)

const toast = useToast()

const setChartRef = (el: any, type: string) => {
  if (el) chartRefs.value[type] = el
}

const ensureSelection = () => {
  const list = computedMachines.value
  if (selectedMachineId.value && !list.some(m => m.id === selectedMachineId.value)) {
    selectedMachineId.value = list[0]?.id ?? ''
  }
}

const lastUpdatedLabel = computed(() => {
  if (!lastUpdated.value) return '—'
  const diffSeconds = Math.floor((Date.now() - lastUpdated.value.getTime()) / 1000)
  if (diffSeconds < 5) return 'just now'
  if (diffSeconds < 60) return `${diffSeconds}s ago`
  return `${Math.floor(diffSeconds / 60)}m ago`
})

const fetchTelemetryData = async () => {
  try {
    loading.value = true
    const res = selectedMachineId.value 
      ? await fetchTelemetryByMachine(selectedMachineId.value, { range: timeRange.value, take: 500 })
      : await fetchRecentTelemetry({ range: timeRange.value, limit: 500 })
    
    telemetryData.value = res
    lastUpdated.value = new Date()
    setTimeout(() => updateAllCharts(), 50)
  } catch (error) {
    toast.error('Telemetric gateway link disruption')
  } finally {
    loading.value = false
  }
}

const updateAllCharts = () => {
  chartConfigs.forEach(config => {
    const el = chartRefs.value[config.type]
    if (!el) return

    if (!chartInstances.value[config.type]) {
      chartInstances.value[config.type] = echarts.init(el, 'hub-dark')
    }

    const { timestamps, values } = processTelemetryData(config.type)
    
    chartInstances.value[config.type].setOption({
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        backgroundColor: 'rgba(5, 7, 10, 0.9)',
        borderColor: 'rgba(255, 255, 255, 0.1)',
        textStyle: { color: '#fff', fontSize: 11 },
        borderWidth: 1,
        padding: [8, 12]
      },
      grid: { left: '2%', right: '2%', top: '5%', bottom: '5%', containLabel: true },
      xAxis: {
        type: 'category',
        data: timestamps,
        axisLine: { lineStyle: { color: 'rgba(255,255,255,0.04)' } },
        axisLabel: { color: 'rgba(255,255,255,0.3)', fontSize: 10, interval: 'auto' },
        axisTick: { show: false }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(255,255,255,0.04)', type: 'dashed' } },
        axisLabel: { color: 'rgba(255,255,255,0.3)', fontSize: 10 },
        axisLine: { show: false }
      },
      series: [{
        data: values,
        type: 'line',
        smooth: 0.3,
        symbol: 'circle',
        symbolSize: 4,
        showSymbol: false,
        lineStyle: { width: 3, color: config.color },
        itemStyle: { color: config.color },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: `${config.color.replace('var(', 'color-mix(in srgb, var(').replace(')', '), transparent 70%)')}` },
            { offset: 1, color: 'transparent' }
          ])
        }
      }]
    })
  })
}

const processTelemetryData = (dataType: string) => {
  const filteredData = telemetryData.value
    .filter(d => d.dataType === dataType)
    .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
  
  return {
    timestamps: filteredData.map(d => new Date(d.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
    values: filteredData.map(d => (typeof d.data === 'object' ? d.data.value || 0 : Number(d.data) || 0))
  }
}

const handleResize = () => {
  Object.values(chartInstances.value).forEach(inst => inst.resize())
}

onMounted(async () => {
  if (!props.machines?.length) {
    localMachines.value = await fetchMachines()
  }
  ensureSelection()
  await fetchTelemetryData()
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  Object.values(chartInstances.value).forEach(inst => inst.dispose())
})

watch([timeRange, selectedMachineId], fetchTelemetryData)
watch(() => props.selectedMachineId, (id) => { if (id !== undefined) selectedMachineId.value = id ?? '' })
watch(selectedMachineId, (id) => emit('machine-selected', id))
</script>

<style module>
.telemetry-dashboard {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  max-width: 1400px;
  margin: 0 auto;
}

/* Header */
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-24);
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.header-indicator {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  margin-bottom: var(--space-6);
}

.pulse-dot {
  width: 6px;
  height: 6px;
  background: var(--color-primary);
  border-radius: 50%;
  box-shadow: var(--glow-status);
  position: relative;
}

.pulse-dot::after {
  content: '';
  position: absolute;
  inset: -2px;
  border: 1px solid var(--color-primary);
  border-radius: 50%;
  animation: pulse-out 2s infinite;
}

@keyframes pulse-out {
  0% { transform: scale(1); opacity: 0.8; }
  100% { transform: scale(2.5); opacity: 0; }
}

.indicator-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: var(--color-text-dim);
}

.header-title {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}

.header-sub {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin: var(--space-4) 0 0;
}

.time-accent {
  color: var(--color-primary);
  font-weight: 600;
  font-family: var(--font-mono);
}

.header-controls {
  display: flex;
  gap: var(--space-12);
}

.control-box {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-6) var(--space-12);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  transition: all var(--transition-fast);
}

.control-box:focus-within {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-muted);
}

.control-box select {
  background: transparent;
  border: none;
  outline: none;
  color: var(--color-text-primary);
  font-size: var(--font-size-xs);
  font-weight: 600;
  cursor: pointer;
  padding-right: var(--space-4);
}

/* KPI Bento Grid */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-16);
}

.kpi-item--primary {
  grid-column: span 1;
  background: radial-gradient(circle at top left, rgba(var(--color-primary-rgb), 0.05), transparent);
}

.kpi-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  height: 100%;
  position: relative;
}

.kpi-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
}

.kpi-icon--emerald { color: var(--color-emerald); }
.kpi-icon--indigo { color: var(--color-primary); }
.kpi-icon--success { color: var(--color-success); }
.kpi-icon--cyan { color: var(--color-cyan); }

.kpi-data {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.kpi-value {
  font-size: var(--font-size-2xl);
  font-weight: 800;
  letter-spacing: -0.02em;
  color: var(--color-text-primary);
}

.kpi-value--success { color: var(--color-success); }

.kpi-label {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.kpi-trend {
  position: absolute;
  top: 0;
  right: 0;
  display: flex;
  align-items: center;
  gap: var(--space-4);
  font-size: 10px;
  font-weight: 700;
  color: var(--color-success);
  background: var(--color-success-muted);
  padding: 2px 6px;
  border-radius: var(--radius-full);
}

/* Chart Grid */
.chart-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: var(--space-20);
}

.chart-item {
  min-height: 320px;
}

.chart-header-wrap {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--space-24);
}

.chart-title {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.chart-desc {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  margin: var(--space-2) 0 0;
}

.chart-icon-box {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--color-border-subtle);
}

.chart-canvas {
  height: 200px;
  width: 100%;
}

/* Empty State */
.empty-stage {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-64) var(--space-24);
  text-align: center;
  background: var(--color-depth-0);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-2xl);
  gap: var(--space-16);
}

.empty-icon-wrap {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: var(--color-depth-1);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-dim);
  opacity: 0.5;
}

/* Injection Section */
.injection-section {
  margin-top: var(--space-48);
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.section-header {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.header-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  box-shadow: var(--glow-sm);
}

.header-text h3 {
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.header-text p {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin: var(--space-2) 0 0;
}

.injection-card {
  max-width: 900px;
}

/* Utils */
.latency-box {
  display: flex;
  align-items: baseline;
  gap: var(--space-4);
}

.unit {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  font-weight: 600;
}

.spinner {
  width: 24px;
  height: 24px;
  border: 2px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 1200px) {
  .kpi-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 768px) {
  .dashboard-header { flex-direction: column; align-items: flex-start; }
  .kpi-grid { grid-template-columns: 1fr; }
  .chart-grid { grid-template-columns: 1fr; }
}
</style>
