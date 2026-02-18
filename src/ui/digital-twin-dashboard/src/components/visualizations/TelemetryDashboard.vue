<template>
  <div class="telemetry-dashboard space-y-10">
    <header class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-8">
      <div>
        <div class="flex items-center gap-2 mb-2">
          <div class="w-8 h-1 bg-emerald-500 rounded-full"></div>
          <p class="text-[10px] uppercase font-black tracking-[0.3em] text-emerald-400">Live Sensory Network</p>
        </div>
        <h2 class="text-3xl font-black tracking-tighter text-primary">Real-time Telemetry</h2>
        <p v-if="lastUpdated" class="text-xs font-bold text-secondary-alt mt-1">
          Last synchronization {{ lastUpdatedLabel }}
        </p>
      </div>

      <div class="flex flex-wrap items-center gap-3 bg-white/5 p-2 rounded-2xl border border-white/10 w-full lg:w-auto">
        <div class="relative flex-1 lg:flex-none min-w-[140px]">
          <Clock class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary" />
          <select 
            v-model="timeRange" 
            class="w-full bg-transparent border-none text-xs font-bold focus:ring-0 pl-10 pr-8 py-2 rounded-xl hover:bg-white/5 transition-colors cursor-pointer appearance-none"
            @change="fetchTelemetryData"
          >
            <option value="1h">Last 1 Hour</option>
            <option value="6h">Last 6 Hours</option>
            <option value="12h">Last 12 Hours</option>
            <option value="24h">Last 24 Hours</option>
          </select>
        </div>
        
        <div class="w-px h-6 bg-white/10 hidden lg:block"></div>

        <div class="relative flex-1 lg:flex-none min-w-[200px]">
          <Cpu class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary" />
          <select 
            v-model="selectedMachineId" 
            class="w-full bg-transparent border-none text-xs font-bold focus:ring-0 pl-10 pr-8 py-2 rounded-xl hover:bg-white/5 transition-colors cursor-pointer appearance-none"
            @change="fetchTelemetryData"
          >
            <option value="">All Machines</option>
            <option v-for="machine in machines" :key="machine.id" :value="machine.id">
              {{ machine.name }}
            </option>
          </select>
        </div>
      </div>
    </header>

    <div v-if="!loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <StatCard label="Telemetry Points" variant="glass" transition>
        <MetricValue :value="telemetryData.length" />
        <template #icon><Activity class="text-emerald-500" /></template>
      </StatCard>
      
      <StatCard label="Active Nodes" variant="glass">
        <MetricValue :value="reportingMachineCount" />
        <template #icon><Layers class="text-indigo-500" /></template>
      </StatCard>

      <StatCard label="Network Health" variant="glass" intent="success">
        <div class="text-xs font-black uppercase tracking-widest text-emerald-400">Stable</div>
        <template #icon><Zap class="text-amber-500" /></template>
      </StatCard>

      <StatCard label="Sync Latency" variant="glass">
        <div class="flex items-baseline gap-1">
          <MetricValue :value="12" />
          <span class="text-xs font-bold text-secondary">ms</span>
        </div>
        <template #icon><History class="text-cyan-500" /></template>
      </StatCard>
    </div>

    <div v-if="!loading && !hasTelemetry" class="py-24 text-center space-y-6 bg-white/[0.02] border-2 border-dashed border-white/5 rounded-[40px]">
      <div class="w-20 h-20 bg-white/5 rounded-full flex items-center justify-center mx-auto">
        <Radio class="w-10 h-10 text-secondary opacity-20" />
      </div>
      <div class="max-w-xs mx-auto space-y-2">
        <h3 class="text-xl font-bold">Signal Lost</h3>
        <p class="text-secondary text-sm">No telemetry packets found for the current selection. Adjusted search parameters may be required.</p>
      </div>
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
      <BaseCard v-for="chart in chartConfigs" :key="chart.type" class="p-8 overflow-hidden group hover:border-white/10 transition-colors">
        <div class="flex justify-between items-start mb-8">
          <div>
            <h4 class="text-sm font-black uppercase tracking-widest text-secondary mb-1">{{ chart.label }}</h4>
            <p class="text-[10px] font-bold text-secondary-alt">{{ chart.description }}</p>
          </div>
          <div class="p-2 transition-transform group-hover:scale-110" :style="{ color: chart.color, backgroundColor: `${chart.color}15`, borderRadius: '12px' }">
            <component :is="chart.icon" class="w-5 h-5" />
          </div>
        </div>
        
        <div v-if="loading" class="h-64 flex items-center justify-center">
           <div class="w-8 h-8 border-2 border-white/10 border-t-secondary animate-spin rounded-full"></div>
        </div>
        <div v-else :ref="el => setChartRef(el, chart.type)" class="w-full h-64"></div>
      </BaseCard>
    </div>

    <div class="pt-12 border-t border-white/5">
       <div class="flex items-center gap-3 mb-8">
          <div class="w-10 h-10 bg-white/5 rounded-xl flex items-center justify-center border border-white/10">
             <Plus class="w-5 h-5 text-secondary" />
          </div>
          <div>
             <h3 class="text-lg font-bold">Manual Injection</h3>
             <p class="text-xs text-secondary-alt">Manually record sensor data for legacy untracked assets.</p>
          </div>
       </div>
       <ManualTelemetryForm class="max-w-4xl" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch, computed, shallowRef } from 'vue'
import { useToast } from '@/composables/useToast'
import * as echarts from 'echarts/core'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, TitleComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

import BaseCard from '../base/BaseCard.vue'
import StatCard from '../base/StatCard.vue'
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
  Plus
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

const chartConfigs = [
  { type: 'pressure', label: 'Hydraulic Pressure', description: 'Internal system PSI stability', icon: Boxes, color: '#3b82f6' },
  { type: 'humidity', label: 'Ambient Humidity', description: 'Relative environment % moisture', icon: Wind, color: '#06b6d4' },
  { type: 'power_consumption', label: 'Energy Load', description: 'Active kilowatt draw footprint', icon: Power, color: '#f59e0b' },
  { type: 'temperature', label: 'Core Temperature', description: 'Machine thermal equilibrium °C', icon: Thermometer, color: '#ef4444' },
  { type: 'vibration', label: 'Acoustic Vibration', description: 'Harmonic resonance in mm/s', icon: Activity, color: '#8b5cf6' },
  { type: 'production_count', label: 'Unit Throughput', description: 'Total cycles per interval', icon: Target, color: '#10b981' },
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
      chartInstances.value[config.type] = echarts.init(el)
    }

    const { timestamps, values } = processTelemetryData(config.type)
    
    chartInstances.value[config.type].setOption({
      backgroundColor: 'transparent',
      tooltip: {
        trigger: 'axis',
        backgroundColor: 'rgba(15, 23, 42, 0.9)',
        borderColor: 'rgba(255, 255, 255, 0.1)',
        textStyle: { color: '#fff' }
      },
      grid: { left: '3%', right: '3%', top: '5%', bottom: '5%', containLabel: true },
      xAxis: {
        type: 'category',
        data: timestamps,
        axisLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } },
        axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
      },
      yAxis: {
        type: 'value',
        splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)', type: 'dashed' } },
        axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
      },
      series: [{
        data: values,
        type: 'line',
        smooth: true,
        symbol: 'none',
        lineStyle: { width: 3, color: config.color },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: `${config.color}25` },
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

<style scoped>
select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%2364748b'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'%3E%3C/path%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 0.5rem center;
  background-size: 0.8rem;
}
</style>

```
