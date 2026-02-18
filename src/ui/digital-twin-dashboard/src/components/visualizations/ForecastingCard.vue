<template>
  <BaseCard class="forecasting-card overflow-hidden" variant="glass">
    <div class="p-8">
      <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-10">
        <div class="flex items-center gap-4">
          <div class="p-3 bg-cyan-500/20 rounded-2xl text-cyan-400 shadow-lg shadow-cyan-500/10">
            <TrendingUp class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-xl font-black tracking-tight text-primary">Predictive Horizon</h3>
            <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">Time-Series Extrapolation</p>
          </div>
        </div>
        
        <div class="flex items-center gap-3">
          <select 
            v-model="selectedMetric" 
            class="bg-white/5 border border-white/10 text-[10px] font-black uppercase tracking-widest rounded-xl px-4 py-2 hover:bg-white/[0.08] transition-colors outline-none cursor-pointer appearance-none pr-8 relative"
            style="background-image: url('data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 fill=%22none%22 viewBox=%220 0 24 24%22 stroke=%22%236b7280%22%3E%3Cpath stroke-linecap=%22round%22 stroke-linejoin=%22round%22 stroke-width=%222%22 d=%22M19 9l-7 7-7-7%22%3E%3C/path%3E%3C/svg%3E'); background-repeat: no-repeat; background-position: right 0.75rem center; background-size: 0.75rem"
          >
            <option v-for="m in metrics" :key="m" :value="m">{{ m }}</option>
          </select>

          <BaseButton 
            variant="primary" 
            size="sm" 
            :loading="loading" 
            class="shadow-lg shadow-cyan-500/20 px-6 h-10 font-bold uppercase tracking-widest text-[10px]"
            @click="runForecast"
          >
            <Zap class="w-4 h-4 mr-2" />
            Project
          </BaseButton>
        </div>
      </header>

      <div class="card-content min-h-[350px]">
        <template v-if="loading">
          <div class="flex flex-col items-center justify-center py-20 gap-4">
            <div class="w-12 h-12 border-4 border-cyan-500/20 border-t-cyan-500 rounded-full animate-spin"></div>
            <p class="text-sm font-bold text-secondary animate-pulse uppercase tracking-widest">Calculating confidence envelopes...</p>
          </div>
        </template>

        <template v-else-if="!forecastResult">
          <div class="py-20 flex flex-col items-center justify-center text-center space-y-6">
            <div class="w-16 h-16 bg-white/5 rounded-[24px] flex items-center justify-center border border-white/10">
              <LineChart class="w-8 h-8 text-secondary opacity-40" />
            </div>
            <div class="space-y-2">
              <h3 class="text-lg font-bold">No Active Forecast</h3>
              <p class="text-secondary text-sm max-w-[280px]">Run a projection to extrapolate future sensor states and identify pre-failure trends.</p>
            </div>
          </div>
        </template>

        <template v-else>
          <div ref="chartRef" class="w-full h-80 mb-8"></div>

          <footer class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div v-for="method in forecastResult.methods" :key="method.method" 
                 class="p-4 rounded-2xl bg-white/[0.03] border border-white/5 flex flex-col gap-1">
              <span class="text-[9px] uppercase font-black text-secondary tracking-widest">{{ method.method }}</span>
              <div class="flex justify-between items-end">
                <span class="text-xs font-bold text-primary">Model Accuracy</span>
                <span class="text-sm font-black text-cyan-400">{{ (method.accuracy * 100).toFixed(1) }}%</span>
              </div>
            </div>
          </footer>
        </template>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import { forecastTimeSeries, type ForecastResult } from '@/services/advancedAnalytics.service'
import BaseButton from '../base/BaseButton.vue'
import BaseCard from '../base/BaseCard.vue'
import { TrendingUp, Zap, LineChart } from 'lucide-vue-next'
import * as echarts from 'echarts/core'
import { LineChart as ELineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent, VisualMapComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

echarts.use([ELineChart, GridComponent, TooltipComponent, LegendComponent, VisualMapComponent, CanvasRenderer])

const props = defineProps<{
  machineId: string
}>()

const toast = useToast()
const loading = ref(false)
const selectedMetric = ref('Temperature')
const metrics = ['Temperature', 'Vibration', 'Pressure', 'Voltage']
const forecastResult = ref<ForecastResult | null>(null)
const chartRef = ref<HTMLElement | null>(null)
let chart: echarts.ECharts | null = null

const initChart = () => {
  if (!chartRef.value || !forecastResult.value) return
  
  if (!chart) {
    chart = echarts.init(chartRef.value)
  }

  const { timestamps, forecastedValues, confidenceIntervals } = forecastResult.value
  const lowerBounds = confidenceIntervals.map(ci => ci.lower)
  const upperBounds = confidenceIntervals.map(ci => ci.upper)

  const option = {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      borderColor: 'rgba(255, 255, 255, 0.1)',
      textStyle: { color: '#fff' }
    },
    grid: {
      left: '3%',
      right: '4%',
      top: '10%',
      bottom: '10%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: timestamps.map(ts => new Date(ts).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })),
      axisLine: { lineStyle: { color: 'rgba(255,255,255,0.1)' } },
      axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
    },
    yAxis: {
      type: 'value',
      splitLine: { lineStyle: { color: 'rgba(255,255,255,0.05)', type: 'dashed' } },
      axisLabel: { color: 'rgba(255,255,255,0.4)', fontSize: 10 }
    },
    series: [
      {
        name: 'Upper Bound',
        type: 'line',
        data: upperBounds,
        lineStyle: { opacity: 0 },
        stack: 'confidence',
        symbol: 'none'
      },
      {
        name: 'Confidence Interval',
        type: 'line',
        data: lowerBounds.map((l, i) => upperBounds[i] - l),
        stack: 'confidence',
        areaStyle: { color: 'rgba(6, 182, 212, 0.1)' },
        lineStyle: { opacity: 0 },
        symbol: 'none'
      },
      {
        name: 'Forecast',
        type: 'line',
        data: forecastedValues,
        smooth: true,
        itemStyle: { color: '#06b6d4' },
        lineStyle: { width: 3, shadowBlur: 10, shadowColor: 'rgba(6, 182, 212, 0.4)' },
        symbolSize: 6
      }
    ]
  }

  chart.setOption(option)
}

const runForecast = async () => {
  if (!props.machineId) return
  loading.value = true
  try {
    const res = await forecastTimeSeries(props.machineId, selectedMetric.value, { forecastHorizon: 12 })
    forecastResult.value = res
    nextTick(() => {
      initChart()
    })
    toast.success('Regression horizon expanded')
  } catch (err) {
    toast.error('Forecasting engine busy')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  runForecast()
  window.addEventListener('resize', () => chart?.resize())
})

watch(() => props.machineId, () => {
  runForecast()
})
</script>

<style scoped>
.forecasting-card {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.forecasting-card:hover {
  border-color: rgba(6, 182, 212, 0.2);
}
</style>
