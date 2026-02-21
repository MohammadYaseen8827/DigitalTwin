<script setup lang="ts">
import { ref, onMounted, nextTick, computed } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import MLPredictions from '@/components/MLPredictions.vue'
import StatCard from '@/components/base/StatCard.vue'
import MetricValue from '@/components/base/MetricValue.vue'
import { 
  Brain, 
  History, 
  Settings, 
  Thermometer, 
  Zap, 
  Layers, 
  BarChart,
  Cpu,
  RefreshCw,
  Search,
  CheckCircle2,
  AlertCircle
} from 'lucide-vue-next'
import {
  retrainModels,
  fetchPredictionHistory
} from '@/services/predictions.service'
import { useMachinesStore } from '@/stores/machines.store'
import type { MachineDto } from '@/api/types'
import * as echarts from 'echarts/core'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { useToast } from '@/composables/useToast'

echarts.use([LineChart, GridComponent, TooltipComponent, LegendComponent, CanvasRenderer])

const machinesStore = useMachinesStore()
const toast = useToast()
const loading = computed(() => machinesStore.loading)
const retraining = ref(false)
const machines = computed(() => machinesStore.machines)
const selectedMachineId = ref<string>('')
const lastUpdatedText = computed(() => machinesStore.lastFetched ? machinesStore.lastFetched.toLocaleTimeString() : '--:--')
const performanceChartRef = ref<HTMLElement | null>(null)
let performanceChart: echarts.ECharts | null = null

const triggerRetraining = async () => {
  retraining.value = true
  try {
    await retrainModels(true)
    toast.success('Retraining complete: Model v1.2.4 deployed')
  } catch (err) {
    toast.error('Retraining failed: System busy or insufficient data')
  } finally {
    retraining.value = false
  }
}
const fetchInitialData = async () => {
  try {
    await machinesStore.loadMachines()
    if (machines.value.length > 0) {
      selectedMachineId.value = machines.value[0].id
    }

    nextTick(() => {
      initPerformanceChart()
    })
  } catch (err) {
    console.error('Failed to init dashboard:', err)
  }
}

const initPerformanceChart = () => {
  if (!performanceChartRef.value) return
  if (!performanceChart) {
    performanceChart = echarts.init(performanceChartRef.value)
  }

  const option = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      borderColor: 'rgba(255, 255, 255, 0.1)',
      textStyle: { color: '#fff' }
    },
    legend: {
      data: ['R2 Score', 'MAPE'],
      textStyle: { color: 'rgba(255,255,255,0.6)', fontSize: 10 },
      bottom: 0
    },
    grid: {
      left: '3%',
      right: '4%',
      top: '10%',
      bottom: '15%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: ['v1.0.0', 'v1.0.5', 'v1.1.0', 'v1.1.2', 'v1.2.0'],
      axisLine: { lineStyle: { color: 'rgba(255,255,255,0.1)' } },
      axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
    },
    yAxis: {
      type: 'value',
      max: 1.0,
      splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)', type: 'dashed' } },
      axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
    },
    series: [
      {
        name: 'R2 Score',
        type: 'line',
        data: [0.82, 0.85, 0.88, 0.91, 0.92],
        smooth: true,
        itemStyle: { color: '#6366f1' },
        lineStyle: { width: 3 }
      },
      {
        name: 'MAPE',
        type: 'line',
        data: [0.18, 0.15, 0.12, 0.09, 0.08],
        smooth: true,
        itemStyle: { color: '#10b981' },
        lineStyle: { width: 3 }
      }
    ]
  }

  performanceChart.setOption(option)
}

onMounted(() => {
  fetchInitialData()
  window.addEventListener('resize', () => performanceChart?.resize())
})
</script>

<template>
  <div class="ml-insights-container p-8 space-y-10">
    <!-- Hero Header with Unified Discovery -->
    <header class="flex flex-col lg:flex-row justify-between items-start lg:items-end gap-8">
      <div>
        <div class="flex items-center gap-2 mb-3">
          <div class="w-8 h-1 bg-indigo-500 rounded-full"></div>
          <p class="text-[10px] uppercase font-black tracking-[0.3em] text-indigo-400">Enterprise AI Ops</p>
        </div>
        <h1 class="text-4xl font-black tracking-tighter text-primary flex items-center gap-3">
          Model Intelligence
          <BaseBadge variant="outline" class="text-[9px] font-black tracking-widest bg-indigo-500/10 border-indigo-500/20">STABLE v1.2.0</BaseBadge>
        </h1>
      </div>
      
      <div class="flex flex-wrap items-center gap-4 bg-white/5 p-2 rounded-2xl border border-white/10 w-full lg:w-auto backdrop-blur-md">
        <div class="flex-1 lg:flex-none relative group">
          <Search class="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary group-hover:text-indigo-400 transition-colors" />
          <select 
            v-model="selectedMachineId" 
            class="w-full lg:w-72 bg-transparent border-none text-[12px] font-bold tracking-tight focus:ring-0 pl-12 pr-10 py-3 rounded-xl hover:bg-white/5 transition-all cursor-pointer appearance-none"
            style="background-image: url('data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 fill=%22none%22 viewBox=%220 0 24 24%22 stroke=%22%236b7280%22%3E%3Cpath stroke-linecap=%22round%22 stroke-linejoin=%22round%22 stroke-width=%222%22 d=%22M19 9l-7 7-7-7%22%3E%3C/path%3E%3C/svg%3E'); background-repeat: no-repeat; background-position: right 1rem center; background-size: 0.85rem"
          >
            <option v-for="m in machines" :key="m.id" :value="m.id" class="bg-slate-900">
              {{ m.name }}
            </option>
          </select>
        </div>
        <div class="h-8 w-px bg-white/10 hidden lg:block"></div>
        <BaseButton variant="primary" class="h-12 px-8 font-black uppercase tracking-widest text-[10px] shadow-lg shadow-indigo-500/20">
          Analyze Unit
        </BaseButton>
      </div>
    </header>

    <!-- Executive Summary Dashboard -->
    <div v-if="loading && machines.length === 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
      <div v-for="i in 4" :key="i" class="h-32 rounded-[32px] bg-white/[0.03] border border-white/10 animate-pulse"></div>
    </div>
    <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-6">
      <StatCard label="Global Accuracy" variant="glass" transition class="border-white/5">
        <MetricValue :value="92.4" suffix="%" />
        <template #icon><BarChart class="text-blue-500 w-5 h-5" /></template>
      </StatCard>
      
      <StatCard label="Prediction Latency" variant="glass" class="border-white/5">
        <MetricValue :value="42" suffix="ms" />
        <template #icon><Zap class="text-emerald-500 w-5 h-5" /></template>
      </StatCard>
      
      <StatCard label="Active Versions" variant="glass" class="border-white/5">
        <div class="flex items-baseline gap-2">
          <MetricValue :value="4" />
          <span class="text-[10px] font-black text-secondary tracking-widest uppercase">STABLE</span>
        </div>
        <template #icon><Layers class="text-indigo-500 w-5 h-5" /></template>
      </StatCard>

      <StatCard label="ML Pipeline Health" variant="glass" intent="success" class="border-white/5">
        <div class="flex items-center gap-2 text-emerald-500 font-black uppercase tracking-widest text-[10px]">
          <CheckCircle2 class="w-4 h-4" />
          Nominal
        </div>
        <template #icon><Cpu class="text-amber-500 w-5 h-5" /></template>
      </StatCard>
    </div>

    <!-- Main Analytics Framework -->
    <div v-if="loading && machines.length === 0" class="grid grid-cols-1 xl:grid-cols-12 gap-8">
      <div class="xl:col-span-8 h-[600px] rounded-[48px] bg-white/[0.03] border border-white/10 animate-pulse"></div>
      <div class="xl:col-span-4 h-[600px] rounded-[48px] bg-white/[0.03] border border-white/10 animate-pulse"></div>
    </div>
    <div v-else class="grid grid-cols-1 xl:grid-cols-12 gap-8">
      <!-- Left Column: Insights -->
      <div class="xl:col-span-8 space-y-10">
        <MLPredictions v-if="selectedMachineId" :machine-id="selectedMachineId" />
        
        <!-- Lifecycle & Performance Detailed -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <BaseCard variant="glass" class="p-8 border-white/5 group overflow-hidden">
             <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.03] to-transparent pointer-events-none"></div>
            <div class="flex justify-between items-start mb-10 relative z-10">
              <div>
                <h3 class="text-xl font-black tracking-tight flex items-center gap-3">
                  <div class="p-2 bg-indigo-500/10 rounded-xl">
                    <History class="w-5 h-5 text-indigo-400" />
                  </div>
                  Performance History
                </h3>
                <p class="text-secondary text-[10px] font-black uppercase tracking-[0.2em] mt-2">Metric stability across deployments</p>
              </div>
            </div>
            <div ref="performanceChartRef" class="w-full h-72 relative z-10"></div>
          </BaseCard>

          <BaseCard variant="glass" class="p-8 border-white/5 relative overflow-hidden group">
            <div class="absolute inset-0 bg-gradient-to-br from-emerald-500/[0.03] to-transparent pointer-events-none"></div>
            <h3 class="text-xl font-black tracking-tight mb-10 flex items-center gap-3 relative z-10">
              <div class="p-2 bg-emerald-500/10 rounded-xl text-emerald-400">
                <Settings class="w-5 h-5" />
              </div>
              Retraining Orchestration
            </h3>
            <div class="space-y-6 relative z-10">
              <div class="orchestration-item p-5 rounded-2xl border border-white/5 bg-white/[0.03] hover:border-indigo-500/30 transition-all">
                <div class="flex justify-between items-center mb-3">
                  <span class="text-[10px] font-black text-secondary uppercase tracking-[0.2em]">Model Drift</span>
                  <span class="text-[10px] font-mono font-black px-2.5 py-1 bg-emerald-500/20 text-emerald-400 rounded-lg">0.024 / 0.050</span>
                </div>
                <div class="h-2 w-full bg-slate-900 rounded-full overflow-hidden">
                  <div class="h-full bg-gradient-to-r from-emerald-600 to-emerald-400" style="width: 42%"></div>
                </div>
              </div>

              <div class="orchestration-item p-5 rounded-2xl border border-white/5 bg-white/[0.03] hover:border-indigo-500/30 transition-all">
                <div class="flex justify-between items-center mb-3">
                  <span class="text-[10px] font-black text-secondary uppercase tracking-[0.2em]">Retraining Buffer</span>
                  <span class="text-[10px] font-mono font-black px-2.5 py-1 bg-indigo-500/20 text-indigo-400 rounded-lg">842 / 1000 samples</span>
                </div>
                <div class="h-2 w-full bg-slate-900 rounded-full overflow-hidden">
                  <div class="h-full bg-gradient-to-r from-indigo-600 to-indigo-400" style="width: 84%"></div>
                </div>
              </div>

              <div class="flex gap-4 mt-10">
                <BaseButton variant="primary" class="flex-1 shadow-lg shadow-indigo-500/20 h-12 text-[10px] font-black uppercase tracking-widest">
                  Force Train
                </BaseButton>
                <BaseButton variant="outline" class="flex-1 h-12 text-[10px] font-black uppercase tracking-widest border-white/10">
                  Configure
                </BaseButton>
              </div>
            </div>
          </BaseCard>
        </div>
      </div>

      <!-- Right Column: System Controls -->
      <div class="xl:col-span-4 space-y-8">
        <BaseCard variant="glass" class="p-8 border-white/5">
          <header class="flex justify-between items-center mb-8">
             <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-secondary">Active Deployments</h3>
             <div class="w-8 h-8 rounded-full bg-emerald-500/10 flex items-center justify-center">
                <div class="w-2 h-2 bg-emerald-500 rounded-full animate-pulse shadow-[0_0_8px_#10b981]"></div>
             </div>
          </header>
          
          <div class="space-y-4">
            <div v-for="i in 3" :key="i" class="p-5 rounded-2xl border border-white/5 bg-white/[0.02] hover:bg-white/[0.05] hover:border-indigo-500/30 transition-all cursor-pointer group">
              <div class="flex justify-between items-start mb-4">
                <div>
                  <div class="font-black text-indigo-100 group-hover:text-indigo-400 transition-colors text-sm tracking-tight">FastForest-R-{{ i }}</div>
                  <div class="text-[9px] uppercase font-black text-secondary tracking-widest mt-1">deployed on 42 units</div>
                </div>
                <BaseBadge variant="emerald" class="text-[8px] font-black">STABLE</BaseBadge>
              </div>
              <div class="flex justify-between items-center mt-6 pt-4 border-t border-white/5">
                <div class="text-[9px] font-black text-secondary-alt uppercase tracking-widest">Latency: <span class="text-indigo-400">{{ 12 + i * 4 }}ms</span></div>
                <div class="text-[9px] font-black text-secondary-alt uppercase tracking-widest">Acc: <span class="text-emerald-400">94.2%</span></div>
              </div>
            </div>
          </div>
          <BaseButton variant="ghost" class="w-full mt-8 text-[10px] font-black uppercase tracking-widest text-indigo-400 hover:bg-indigo-500/5 h-12">
            View All Deployments
          </BaseButton>
        </BaseCard>

        <BaseCard variant="glass" class="p-8 bg-indigo-500/[0.02] border-indigo-500/20">
          <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-indigo-400 mb-8">Pipeline Health</h3>
          <div class="space-y-8">
            <div class="flex gap-5 group">
              <div class="p-3 bg-indigo-500/10 rounded-2xl h-fit border border-indigo-500/20 text-indigo-400 group-hover:scale-110 transition-transform">
                <Thermometer class="w-5 h-5" />
              </div>
              <div>
                <div class="text-sm font-black text-primary">Uncertainty Guard</div>
                <p class="text-[11px] text-secondary leading-relaxed mt-1.5 opacity-80">Quantile Regression v2 actively monitoring variance thresholds.</p>
              </div>
            </div>
            
            <div class="flex gap-5 group">
              <div class="p-3 bg-emerald-500/10 rounded-2xl h-fit border border-emerald-500/20 text-emerald-400 group-hover:scale-110 transition-transform">
                <AlertCircle class="w-5 h-5" />
              </div>
              <div>
                <div class="text-sm font-black text-primary">Promotion System</div>
                <p class="text-[11px] text-secondary leading-relaxed mt-1.5 opacity-80">Auto-deploying versions with >5% accuracy improvement.</p>
              </div>
            </div>
          </div>
        </BaseCard>

        <div class="p-8 rounded-[32px] bg-gradient-to-br from-indigo-500/10 to-transparent border border-white/10 relative overflow-hidden group">
          <div class="absolute -right-6 -bottom-6 opacity-[0.03] group-hover:opacity-10 transition-opacity duration-700">
            <Brain class="w-40 h-40 text-white" />
          </div>
          <h4 class="text-[10px] font-black uppercase tracking-[0.3em] text-indigo-400 mb-3">AI Intelligence Insight</h4>
          <p class="text-sm text-indigo-200/80 leading-relaxed relative z-10 font-bold tracking-tight">
            Current model stable. Drift is within operational boundaries. Next scheduled evaluation in 14 hours.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ml-insights-container {
  max-width: 1700px;
  margin: 0 auto;
}

@keyframes fadeInScale {
  from { opacity: 0; transform: translateY(20px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.StatCard, .BaseCard {
  animation: fadeInScale 0.6s cubic-bezier(0.16, 1, 0.3, 1) backwards;
}

.StatCard:nth-child(1) { animation-delay: 0.1s; }
.StatCard:nth-child(2) { animation-delay: 0.2s; }
.StatCard:nth-child(3) { animation-delay: 0.3s; }
.StatCard:nth-child(4) { animation-delay: 0.4s; }
</style>

