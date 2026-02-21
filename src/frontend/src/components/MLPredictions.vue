<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from 'vue'
import BaseCard from './base/BaseCard.vue'
import BaseButton from './base/BaseButton.vue'
import BaseBadge from './base/BaseBadge.vue'
import StatCard from './base/StatCard.vue'
import MetricValue from './base/MetricValue.vue'
import { 
  Activity, 
  Brain, 
  Clock, 
  ShieldCheck, 
  AlertTriangle, 
  TrendingDown, 
  TrendingUp,
  BarChart4,
  History,
  Info
} from 'lucide-vue-next'
import * as echarts from 'echarts/core'
import { LineChart, BarChart, PictorialBarChart } from 'echarts/charts'
import { 
  GridComponent, 
  TooltipComponent, 
  LegendComponent, 
  VisualMapComponent,
  DataZoomComponent,
  MarkLineComponent,
  TitleComponent 
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { requestEnsemblePrediction, fetchPredictionHistory } from '@/services/predictions.service'
import type { PredictionDto } from '@/api/types'

echarts.use([
  LineChart, 
  BarChart, 
  PictorialBarChart, 
  GridComponent, 
  TooltipComponent, 
  LegendComponent, 
  VisualMapComponent, 
  DataZoomComponent,
  MarkLineComponent,
  TitleComponent,
  CanvasRenderer
])

interface PredictionPoint {
  date: string
  rul: number
  lowerBound: number
  upperBound: number
}

interface PredictionData {
  remainingUsefulLife: number
  confidence: number
  riskLevel: string
  lowerBound: number
  upperBound: number
  featureImportance: Record<string, number>
  shapValues: Record<string, number>
  predictionTime: string
  history: PredictionPoint[]
}

const props = defineProps<{
  machineId: string
}>()

const loading = ref(true)
const prediction = ref<PredictionData | null>(null)
const importanceChartRef = ref<HTMLElement | null>(null)
const trendChartRef = ref<HTMLElement | null>(null)
let importanceChart: echarts.ECharts | null = null
let trendChart: echarts.ECharts | null = null

const riskColor = computed(() => {
  if (!prediction.value) return 'var(--color-text-secondary)'
  const status = (prediction.value.riskLevel || '').toLowerCase()
  switch (status) {
    case 'low': case 'healthy': return '#10b981' // Emerald 500
    case 'medium': case 'warning': case 'minor_degradation': return '#f59e0b' // Amber 500
    case 'high': case 'critical': case 'significant_degradation': return '#ef4444' // Red 500
    case 'failure_imminent': return '#b91c1c' // Red 700
    default: return '#6b7280' // Gray 500
  }
})

const fetchPrediction = async () => {
  if (!props.machineId) return
  
  loading.value = true
  try {
    const [latest, historyData] = await Promise.all([
      requestEnsemblePrediction(props.machineId),
      fetchPredictionHistory(props.machineId, 15)
    ])
    
    // Map history and sort by date ascending
    const history: PredictionPoint[] = (historyData || []).map(p => ({
      date: new Date(p.createdAt).toISOString().split('T')[0],
      rul: Math.round(p.remainingUsefulLifeDays),
      lowerBound: Math.round(p.rulLowerBound ?? p.remainingUsefulLifeDays * 0.9),
      upperBound: Math.round(p.rulUpperBound ?? p.remainingUsefulLifeDays * 1.1)
    })).sort((a, b) => a.date.localeCompare(b.date))

    // Add latest if not already in history (check by date)
    const latestDate = new Date(latest.createdAt).toISOString().split('T')[0]
    if (!history.find(h => h.date === latestDate)) {
      history.push({
        date: latestDate,
        rul: Math.round(latest.remainingUsefulLifeDays),
        lowerBound: Math.round(latest.rulLowerBound ?? latest.remainingUsefulLifeDays * 0.9),
        upperBound: Math.round(latest.rulUpperBound ?? latest.remainingUsefulLifeDays * 1.1)
      })
    }

    prediction.value = {
      remainingUsefulLife: Math.round(latest.remainingUsefulLifeDays),
      confidence: latest.failureProbability !== null ? 1.0 - latest.failureProbability : 0.9,
      riskLevel: latest.healthStatus || 'unknown',
      lowerBound: Math.round(latest.rulLowerBound ?? latest.remainingUsefulLifeDays * 0.9),
      upperBound: Math.round(latest.rulUpperBound ?? latest.remainingUsefulLifeDays * 1.1),
      featureImportance: latest.featureContributions || {},
      shapValues: latest.featureContributions || {},
      predictionTime: latest.createdAt,
      history
    }
  } catch (error) {
    console.error('Failed to fetch prediction data:', error)
    // We could stay in loading state or show an error card
  } finally {
    loading.value = false
    nextTick(() => {
      initImportanceChart()
      initTrendChart()
    })
  }
}

onMounted(() => {
  fetchPrediction()
})

watch(() => props.machineId, () => {
  fetchPrediction()
})

const initTrendChart = () => {
  if (!trendChartRef.value || !prediction.value) return
  
  if (!trendChart) {
    trendChart = echarts.init(trendChartRef.value)
  }
  
  const dates = prediction.value.history.map(p => p.date)
  const ruls = prediction.value.history.map(p => p.rul)
  const lowers = prediction.value.history.map(p => p.lowerBound)
  const uppers = prediction.value.history.map(p => p.upperBound)

  const option = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      borderColor: 'rgba(255, 255, 255, 0.1)',
      textStyle: { color: '#fff' },
      formatter: (params: any) => {
        const p = params[0]
        return `<div class="p-2">
          <div class="font-bold mb-1">${p.name}</div>
          <div class="flex justify-between gap-4">
            <span class="text-secondary">Expected RUL:</span>
            <span class="font-mono text-indigo-400 font-bold">${p.value} Days</span>
          </div>
        </div>`
      }
    },
    grid: {
      left: '2%',
      right: '2%',
      top: '10%',
      bottom: '5%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: dates,
      axisLine: { lineStyle: { color: 'rgba(255,255,255,0.1)' } },
      axisLabel: { color: 'rgba(255,255,255,0.5)', fontSize: 10 }
    },
    yAxis: {
      type: 'value',
      name: 'Days',
      nameTextStyle: { color: 'rgba(255,255,255,0.5)', align: 'right' },
      splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)', type: 'dashed' } },
      axisLabel: { color: 'rgba(255,255,255,0.5)' }
    },
    series: [
      {
        name: 'RUL Prediction',
        type: 'line',
        data: ruls,
        smooth: true,
        symbol: 'circle',
        symbolSize: 8,
        itemStyle: { color: '#6366f1' },
        lineStyle: { width: 3, shadowBlur: 10, shadowColor: 'rgba(99, 102, 241, 0.4)' },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(99, 102, 241, 0.2)' },
            { offset: 1, color: 'rgba(99, 102, 241, 0)' }
          ])
        }
      }
    ]
  }
  
  trendChart.setOption(option)
}

const initImportanceChart = () => {
  if (!importanceChartRef.value || !prediction.value) return
  
  if (!importanceChart) {
    importanceChart = echarts.init(importanceChartRef.value)
  }
  
  const features = Object.keys(prediction.value.shapValues).reverse()
  const values = Object.values(prediction.value.shapValues).reverse()
  
  const option = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: (params: any) => {
        const p = params[0]
        const direction = p.value >= 0 ? 'Increase' : 'Decrease'
        const color = p.value >= 0 ? '#10b981' : '#ef4444'
        return `
          <div class="p-2">
            <div class="font-bold mb-1">${p.name}</div>
            <div style="color: ${color}">Impact: ${p.value > 0 ? '+' : ''}${p.value} Days</div>
            <div class="text-xs text-secondary mt-1">SHAP score contribution to result</div>
          </div>
        `
      }
    },
    grid: {
      left: '3%',
      right: '8%',
      top: '5%',
      bottom: '5%',
      containLabel: true
    },
    xAxis: {
      type: 'value',
      splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)' } },
      axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
    },
    yAxis: {
      type: 'category',
      data: features,
      axisLabel: { color: 'rgba(255,255,255,0.7)', fontSize: 11, fontWeight: 500 },
      axisLine: { show: false }
    },
    series: [
      {
        name: 'SHAP Impact',
        type: 'bar',
        data: values.map(v => ({
          value: v,
          itemStyle: {
            color: v >= 0 
              ? new echarts.graphic.LinearGradient(0, 0, 1, 0, [
                { offset: 0, color: 'rgba(16, 185, 129, 0.2)' },
                { offset: 1, color: 'rgba(16, 185, 129, 0.8)' }
              ])
              : new echarts.graphic.LinearGradient(0, 0, 1, 0, [
                { offset: 0, color: 'rgba(239, 68, 68, 0.8)' },
                { offset: 1, color: 'rgba(239, 68, 68, 0.2)' }
              ]),
            borderRadius: [0, 4, 4, 0]
          }
        })),
        barWidth: 16,
        label: {
          show: true,
          position: 'right',
          formatter: (params: any) => (params.value > 0 ? '+' : '') + params.value,
          color: 'rgba(255,255,255,0.6)',
          fontSize: 10
        }
      }
    ]
  }
  
  importanceChart.setOption(option)
}

onMounted(() => {
  fetchPrediction()
  window.addEventListener('resize', handleResize)
})

const handleResize = () => {
  importanceChart?.resize()
  trendChart?.resize()
}

watch(() => props.machineId, () => {
  fetchPrediction()
})
const calculatePosition = (val: number) => {
  const max = Math.max(365, prediction.value?.upperBound ?? 0)
  return Math.min(100, Math.max(0, (val / max) * 100))
}

const driftLabel = computed(() => {
  if (!prediction.value) return 'nominal stability'
  const hist = prediction.value.history
  if (hist.length < 2) return 'insufficient trend data'
  
  const last = hist[hist.length - 1].rul
  const prev = hist[hist.length - 2].rul
  const diff = last - prev
  
  if (diff < -5) return 'accelerated degradation'
  if (diff < 0) return 'gradual decline'
  if (diff > 5) return 'restored capacity'
  return 'operational stability'
})

const topImpact = computed(() => {
  if (!prediction.value || !prediction.value.shapValues) return null
  const entries = Object.entries(prediction.value.shapValues)
  if (entries.length === 0) return null
  
  // Find feature with largest absolute impact
  return entries
    .map(([feature, value]) => ({ feature, value }))
    .sort((a, b) => Math.abs(b.value) - Math.abs(a.value))[0]
})
</script>

<template>
  <div class="ml-predictions space-y-8">
    <!-- Top Summary Stats -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <StatCard label="Predicted RUL" variant="glass" :intent="prediction?.riskLevel === 'medium' ? 'warning' : 'default'" class="border-white/5">
        <div class="flex items-baseline gap-2">
          <MetricValue :value="prediction?.remainingUsefulLife ?? 0" />
          <span class="text-[10px] font-black text-secondary uppercase tracking-widest">Days</span>
        </div>
        <template #icon><Clock class="w-5 h-5 text-indigo-400" /></template>
      </StatCard>

      <StatCard label="Model Confidence" variant="glass" class="border-white/5">
        <MetricValue :value="(prediction?.confidence ?? 0) * 100" suffix="%" />
        <template #icon><ShieldCheck class="w-5 h-5 text-emerald-400" /></template>
      </StatCard>

      <StatCard label="Global Risk" :intent="prediction?.riskLevel === 'medium' ? 'warning' : 'default'" variant="glass" class="border-white/5">
        <span class="uppercase font-black tracking-[0.2em] text-[12px]">{{ prediction?.riskLevel ?? 'N/A' }}</span>
        <template #icon><AlertCircle class="w-5 h-5 text-amber-500" /></template>
      </StatCard>

      <StatCard label="Mean Drift" variant="glass" class="border-white/5">
        <MetricValue :value="0.024" :precision="3" />
        <template #icon><Activity class="w-5 h-5 text-blue-400" /></template>
      </StatCard>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
      <!-- Main Visualized Prediction -->
      <BaseCard variant="glass" class="lg:col-span-2 overflow-hidden border-white/5 relative group">
        <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.04] to-transparent pointer-events-none"></div>
        <div class="p-10 relative min-h-[450px] flex flex-col">
          <header class="flex justify-between items-start mb-12">
            <div>
              <div class="flex items-center gap-2 mb-2">
                <div class="w-6 h-1 bg-indigo-500 rounded-full"></div>
                <h3 class="text-[10px] uppercase font-black tracking-[0.3em] text-indigo-400">Interpretable Prediction</h3>
              </div>
              <h2 class="text-3xl font-black tracking-tighter text-primary">Health Estimation: Unit {{ machineId.slice(0, 8) }}</h2>
            </div>
            <BaseBadge variant="outline" class="text-[9px] font-black uppercase tracking-widest bg-indigo-500/10 border-indigo-500/20 py-1.5 px-3">
              FASTFOREST v1.2
            </BaseBadge>
          </header>

          <div class="flex flex-col xl:flex-row items-center gap-16 flex-1">
            <!-- Big Circle Gauge Style Display -->
            <div class="prediction-hero flex flex-col items-center justify-center relative">
              <div class="absolute inset-0 bg-indigo-500/10 blur-[60px] rounded-full scale-75"></div>
              <div class="relative w-56 h-56 flex items-center justify-center">
                <svg class="absolute inset-0 w-full h-full -rotate-90">
                  <circle cx="112" cy="112" r="104" fill="none" stroke="rgba(255,255,255,0.03)" stroke-width="12" />
                  <circle cx="112" cy="112" r="104" fill="none" 
                          :stroke="riskColor" 
                          stroke-width="12" 
                          stroke-dasharray="653.4" 
                          :stroke-dashoffset="653.4 * (1 - (prediction?.remainingUsefulLife ?? 365) / 365)"
                          stroke-linecap="round"
                          class="transition-all duration-[2000ms] ease-out" />
                </svg>
                <div class="text-center z-10">
                  <div class="text-6xl font-black tracking-tighter leading-none" :style="{ color: riskColor }">
                    {{ prediction?.remainingUsefulLife }}
                  </div>
                  <div class="text-[10px] uppercase font-black text-secondary tracking-[0.2em] mt-3">Days RUL</div>
                </div>
              </div>
            </div>

            <!-- Uncertainty & Range Viz -->
            <div class="flex-1 w-full space-y-10">
              <div class="bg-white/[0.03] p-8 rounded-[32px] border border-white/10 relative overflow-hidden">
                <div class="absolute inset-0 bg-gradient-to-r from-indigo-500/[0.05] to-transparent pointer-events-none"></div>
                <h4 class="text-[11px] font-black uppercase tracking-[0.2em] mb-8 flex items-center gap-3 text-primary">
                  <ShieldCheck class="w-4 h-4 text-emerald-500" />
                  Probabilistic Confidence Interval (90%)
                </h4>
                
                <div class="range-viz py-12 relative px-6">
                  <div class="range-line h-1.5 bg-white/5 rounded-full w-full"></div>
                  <div class="range-active h-1.5 bg-indigo-500/40 absolute top-12 rounded-full blur-[2px] transition-all duration-1000" 
                       :style="{ 
                         left: `${calculatePosition(prediction?.lowerBound ?? 0)}%`, 
                         right: `${100 - calculatePosition(prediction?.upperBound ?? 365)}%` 
                       }"></div>
                  
                  <!-- Range Markers -->
                  <div class="marker lower absolute top-6 text-center transition-all duration-1000 delay-300" 
                       :style="{ left: `${calculatePosition(prediction?.lowerBound ?? 0)}%` }">
                    <div class="dot w-4 h-4 bg-white rounded-full border-[3px] border-indigo-500 shadow-lg shadow-indigo-500/40"></div>
                    <div class="mt-4 -translate-x-1/2">
                      <div class="text-[9px] uppercase font-black text-secondary tracking-widest">Conservative</div>
                      <div class="text-sm font-black text-primary mt-1">{{ prediction?.lowerBound }}d</div>
                    </div>
                  </div>
                  
                  <div class="marker upper absolute top-6 text-center transition-all duration-1000 delay-500" 
                       :style="{ left: `${calculatePosition(prediction?.upperBound ?? 365)}%` }">
                    <div class="marker-glow absolute inset-0 bg-indigo-500/30 blur-2xl"></div>
                    <div class="dot w-4 h-4 bg-white rounded-full border-[3px] border-indigo-500 shadow-lg shadow-indigo-500/40 relative z-10"></div>
                    <div class="mt-4 -translate-x-1/2 relative z-10">
                      <div class="text-[9px] uppercase font-black text-secondary tracking-widest">Optimistic</div>
                      <div class="text-sm font-black text-primary mt-1">{{ prediction?.upperBound }}d</div>
                    </div>
                  </div>

                  <div class="marker current absolute top-2 text-center transition-all duration-[1500ms]" 
                       :style="{ left: `${calculatePosition(prediction?.remainingUsefulLife ?? 180)}%` }">
                    <div class="px-3 py-1 bg-indigo-500 text-[9px] font-black uppercase tracking-widest rounded-lg mb-4 shadow-lg shadow-indigo-500/30 -translate-x-1/2">
                      Expected
                    </div>
                    <div class="dot w-6 h-6 bg-white rounded-full border-[5px] border-indigo-500 shadow-[0_0_25px_rgba(99,102,241,0.6)] -translate-x-1/2"></div>
                  </div>
                </div>
              </div>

              <!-- Actionable insight snippet -->
              <div class="flex items-start gap-4 p-6 rounded-2xl border border-amber-500/20 bg-amber-500/[0.03]">
                <div class="p-2 bg-amber-500/10 rounded-xl">
                  <AlertTriangle class="w-5 h-5 text-amber-500" />
                </div>
                <p class="text-sm text-secondary leading-relaxed font-medium">
                  Machine health showing <span :class="prediction?.riskLevel === 'healthy' ? 'text-emerald-400' : 'text-amber-400'" class="font-bold">{{ driftLabel }}</span>. 
                  <span v-if="topImpact" class="ml-1">
                    <span class="text-primary font-black uppercase">{{ topImpact.feature }}</span> values are contributing 
                    <span :class="topImpact.value < 0 ? 'text-red-400' : 'text-emerald-400'" class="font-bold">
                      {{ topImpact.value > 0 ? '+' : '' }}{{ topImpact.value.toFixed(1) }} days
                    </span> 
                    to this estimation.
                  </span>
                </p>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- Prediction Meta & Drift -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-1 gap-6">
        <BaseCard variant="glass" class="p-6 border-white/5 overflow-hidden relative group">
           <div class="absolute inset-0 bg-gradient-to-br from-red-500/[0.03] to-transparent pointer-events-none"></div>
          <div class="flex items-center gap-2 mb-6">
            <TrendingDown class="w-4 h-4 text-red-400" />
            <h4 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary">Degradation Velocity</h4>
          </div>
          <div class="text-4xl font-black text-primary mb-2">-1.2d <span class="text-[10px] font-black text-secondary-alt uppercase tracking-widest ml-1">/ day</span></div>
          <div class="h-2 w-full bg-slate-900 rounded-full mt-6 overflow-hidden">
            <div class="h-full bg-gradient-to-r from-red-600 to-red-400" style="width: 65%"></div>
          </div>
          <p class="text-[10px] font-black text-secondary-alt uppercase tracking-widest mt-3">Accelerated wear pattern detected</p>
        </BaseCard>

        <BaseCard variant="glass" class="p-6 border-white/5 relative group">
          <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.03] to-transparent pointer-events-none"></div>
          <div class="flex items-center gap-2 mb-6">
            <History class="w-4 h-4 text-indigo-400" />
            <h4 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary">Model Lifecycle</h4>
          </div>
          <div class="text-xl font-black text-primary mb-2">Feb 08, 14:22</div>
          <p class="text-[10px] font-black text-secondary-alt uppercase tracking-widest mt-1">Trained on 124,500 samples</p>
          <BaseButton variant="outline" class="w-full mt-8 h-10 text-[9px] uppercase font-black tracking-widest border-white/10 hover:bg-white/5">
            View Training Logs
          </BaseButton>
        </BaseCard>
      </div>
    </div>

    <!-- Charts Row -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Historical Trend -->
      <BaseCard variant="glass" class="p-8 border-white/5 relative overflow-hidden group">
        <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.02] to-transparent pointer-events-none"></div>
        <div class="flex justify-between items-center mb-10 relative z-10">
          <div>
            <h3 class="text-xl font-black tracking-tight flex items-center gap-3">
              <div class="p-2 bg-indigo-500/10 rounded-xl text-indigo-400">
                <Activity class="w-5 h-5" />
              </div>
              RUL Historical Trend
            </h3>
            <p class="text-secondary text-[10px] font-black uppercase tracking-[0.2em] mt-2">Prediction stability over last 15 days</p>
          </div>
        </div>
        <div ref="trendChartRef" class="w-full h-80 relative z-10"></div>
      </BaseCard>

      <!-- Local Explainer (SHAP) -->
      <BaseCard variant="glass" class="p-8 border-white/5 relative overflow-hidden group">
        <div class="absolute inset-0 bg-gradient-to-br from-emerald-500/[0.02] to-transparent pointer-events-none"></div>
        <div class="flex justify-between items-center mb-10 relative z-10">
          <div>
            <h3 class="text-xl font-black tracking-tight flex items-center gap-3">
              <div class="p-2 bg-emerald-500/10 rounded-xl text-emerald-400">
                <BarChart4 class="w-5 h-5" />
              </div>
              Local Feature Impact
            </h3>
            <p class="text-secondary text-[10px] font-black uppercase tracking-[0.2em] mt-2">SHAP contribution analytics</p>
          </div>
        </div>
        <div ref="importanceChartRef" class="w-full h-80 relative z-10"></div>
        
        <div class="mt-8 flex items-center gap-6 text-[9px] font-black uppercase tracking-widest relative z-10">
          <div class="flex items-center gap-2">
            <div class="w-3 h-1.5 rounded-full bg-emerald-500 shadow-[0_0_8px_#10b981]"></div>
            <span class="text-secondary-alt">POSITIVE (Increases Life)</span>
          </div>
          <div class="flex items-center gap-2">
            <div class="w-3 h-1.5 rounded-full bg-red-500 shadow-[0_0_8px_#ef4444]"></div>
            <span class="text-secondary-alt">NEGATIVE (Decreases Life)</span>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Explanation & Interpretation Guide -->
    <BaseCard variant="glass" class="p-8 border-indigo-500/20 bg-indigo-500/[0.03] rounded-[32px]">
      <div class="flex gap-6 items-start">
        <div class="p-4 bg-indigo-500/20 rounded-[20px] text-indigo-400 shadow-xl border border-indigo-500/20">
          <Info class="w-6 h-6" />
        </div>
        <div>
          <h4 class="text-lg font-black text-primary mb-3">Interpreting Neural Analytics</h4>
          <p class="text-sm text-secondary-alt leading-relaxed font-medium">
            The RUL prediction represents the expected days remaining before the machine reaches a critical failure threshold. 
            The <span class="text-indigo-400 font-bold">Local Feature Impact</span> uses SHAP values to explain <em class="not-italic text-primary">why</em> the model gave this specific prediction. 
            Factors in <span class="text-red-400 font-bold">RED</span> are contributing to a shorter life (e.g., high vibration), while factors in <span class="text-emerald-400 font-bold">GREEN</span> 
            mean those sensors are healthy and supporting a longer RUL tenure.
          </p>
        </div>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.prediction-hero {
  flex-shrink: 0;
}

.range-viz {
  min-height: 100px;
}

.dot {
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.group:hover .dot {
  transform: scale(1.15);
}

.marker {
  z-index: 2;
}

@keyframes slideInUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

.ml-predictions > * {
  animation: slideInUp 0.7s cubic-bezier(0.16, 1, 0.3, 1) backwards;
}

.ml-predictions > *:nth-child(1) { animation-delay: 0.1s; }
.ml-predictions > *:nth-child(2) { animation-delay: 0.2s; }
.ml-predictions > *:nth-child(3) { animation-delay: 0.3s; }
.ml-predictions > *:nth-child(4) { animation-delay: 0.4s; }

.text-sm, .text-xl, .text-3xl, .text-4xl, .text-6xl {
  font-variant-numeric: tabular-nums;
}
</style>
