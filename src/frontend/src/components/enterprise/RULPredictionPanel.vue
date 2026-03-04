<template>
  <UiCard variant="default" padding="none" hover :class="styles['rul-panel']">
    <div :class="styles['rul-panel__header']">
      <div :class="styles['title-group']">
        <h3 :class="styles['rul-panel__title']">Predictive RUL</h3>
        <p :class="styles['rul-panel__machine-info']">
          {{ machineName }} <span :class="styles['sep']">/</span> {{ machineId }}
        </p>
      </div>
      <div :class="[styles['risk-badge'], styles[`risk-badge--${riskLevel.toLowerCase()}`]]">
        <div :class="styles['pulse-dot']" />
        <span>{{ riskLevel }} Risk</span>
      </div>
    </div>

    <div :class="styles['rul-panel__body']">
      <!-- Primary RUL Display -->
      <div :class="styles['rul-panel__main']">
        <div :class="styles['rul-panel__value-container']">
          <span :class="[styles['rul-panel__value'], styles[`text--${riskLevel.toLowerCase()}`]]">{{ formattedRUL }}</span>
          <span :class="styles['rul-panel__unit']">HOURS</span>
        </div>
        
        <div :class="styles['trend-indicator']">
          <TrendingDown :width="14" :height="14" />
          <span>-4.2% trajectory</span>
        </div>
      </div>

      <!-- Confidence Matrix -->
      <div v-if="confidenceInterval" :class="styles['confidence-matrix']">
        <div :class="styles['confidence-header']">
          <span :class="styles['confidence-label']">Neural Confidence Horizon</span>
          <span :class="styles['confidence-pct']">95% SIGMA</span>
        </div>
        <div :class="styles['confidence-bar-wrap']">
          <span :class="styles['confidence-val']">{{ confidenceInterval.lower }}</span>
          <div :class="styles['confidence-track']">
             <div :class="styles['confidence-fill']" :style="{ width: confidenceFillWidth }" />
             <div :class="styles['confidence-point']" :style="{ left: confidencePointPosition + '%' }" />
          </div>
          <span :class="styles['confidence-val']">{{ confidenceInterval.upper }}</span>
        </div>
      </div>

      <!-- Chart Forensic -->
      <div v-if="chartData.length > 0" :class="styles['chart-forensic']">
        <div ref="chartRef" :class="styles['chart-container']" />
      </div>

      <!-- Strategy Interpretation -->
      <div v-if="interpretation" :class="styles['strategy-box']">
        <div :class="styles['strategy-header']">
          <ShieldAlert :width="14" :height="14" />
          <span>FORENSIC STRATEGY</span>
        </div>
        <p :class="styles['strategy-text']">{{ interpretation }}</p>
      </div>
    </div>
  </UiCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, useCssModule } from 'vue'
import UiCard from '../ui/UiCard.vue'
import { TrendingDown, ShieldAlert } from 'lucide-vue-next'
import * as echarts from 'echarts'
import { createRULChartOption } from '@/utils/echartsConfig'

interface Props {
  machineId: string
  machineName: string
  rul: number
  confidenceInterval?: {
    lower: number
    upper: number
  }
  chartData?: Array<{
    timestamp: number
    prediction: number
    lower?: number
    upper?: number
  }>
  interpretation?: string
}

const props = withDefaults(defineProps<Props>(), {
  confidenceInterval: undefined,
  chartData: () => [],
  interpretation: ''
})

const styles = useCssModule()
const chartRef = ref<HTMLElement>()

const formattedRUL = computed(() => {
  return props.rul.toLocaleString('en-US', { maximumFractionDigits: 0 })
})

const riskLevel = computed(() => {
  if (props.rul <= 24) return 'Critical'
  if (props.rul <= 168) return 'High'
  if (props.rul <= 720) return 'Medium'
  return 'Low'
})

const confidenceFillWidth = computed(() => {
  if (!props.confidenceInterval) return '0%'
  const range = props.confidenceInterval.upper - props.confidenceInterval.lower
  if (range <= 0) return '50%'
  const mid = props.rul
  const fillWidth = (mid - props.confidenceInterval.lower) / range * 100
  return Math.min(100, Math.max(0, fillWidth)) + '%'
})

const confidencePointPosition = computed(() => {
  if (!props.confidenceInterval) return 50
  const range = props.confidenceInterval.upper - props.confidenceInterval.lower
  if (range <= 0) return 50
  const position = (props.rul - props.confidenceInterval.lower) / range * 100
  return Math.min(100, Math.max(0, position))
})

let chart: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value || props.chartData.length === 0) return
  if (chart) chart.dispose()
  chart = echarts.init(chartRef.value, 'hub-dark')
  const option = createRULChartOption({
    timestamps: props.chartData.map(d => d.timestamp),
    predictions: props.chartData.map(d => d.prediction),
    lowerBound: props.chartData[0]?.lower ? props.chartData.map(d => d.lower!) : undefined,
    upperBound: props.chartData[0]?.upper ? props.chartData.map(d => d.upper!) : undefined
  })
  chart.setOption({
    ...option,
    backgroundColor: 'transparent',
    grid: { left: '2%', right: '2%', top: '5%', bottom: '5%', containLabel: true }
  })
}

watch(() => props.chartData, initChart, { deep: true })

onMounted(() => {
  initChart()
  const ro = new ResizeObserver(() => chart?.resize())
  if (chartRef.value) ro.observe(chartRef.value)
})
</script>

<style module>
.rul-panel {
  position: relative;
}

.rul-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: var(--space-20) var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.title-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.rul-panel__title {
  margin: 0;
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.rul-panel__machine-info {
  margin: 0;
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  font-family: var(--font-mono);
  text-transform: uppercase;
}

.sep { opacity: 0.3; padding: 0 4px; }

.risk-badge {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: 4px 10px;
  border-radius: var(--radius-full);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
}

.risk-badge--critical { color: var(--color-danger); border-color: var(--color-danger-muted); }
.risk-badge--high { color: #f97316; border-color: rgba(249,115,22,0.2); }
.risk-badge--medium { color: var(--color-warning); border-color: var(--color-warning-muted); }
.risk-badge--low { color: var(--color-success); border-color: var(--color-success-muted); }

.pulse-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
  box-shadow: 0 0 8px currentColor;
}

.rul-panel__body {
  padding: var(--space-24);
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.rul-panel__main {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
}

.rul-panel__value-container {
  display: flex;
  align-items: baseline;
  gap: var(--space-8);
}

.rul-panel__value {
  font-size: 56px;
  font-weight: 800;
  font-family: var(--font-mono);
  line-height: 1;
  letter-spacing: -0.04em;
}

.rul-panel__unit {
  font-size: var(--font-size-sm);
  font-weight: 800;
  color: var(--color-text-dim);
  letter-spacing: 0.1em;
}

.trend-indicator {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-dim);
  margin-bottom: var(--space-4);
}

/* Confidence Matrix */
.confidence-matrix {
  display: flex;
  flex-direction: column;
  gap: var(--space-10);
}

.confidence-header {
  display: flex;
  justify-content: space-between;
  font-size: 9px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--color-text-dim);
  letter-spacing: 0.08em;
}

.confidence-bar-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.confidence-val {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-muted);
  min-width: 40px;
}

.confidence-track {
  flex: 1;
  height: 4px;
  background: var(--color-depth-1);
  border-radius: var(--radius-full);
  position: relative;
  overflow: visible;
}

.confidence-fill {
  position: absolute;
  left: 0;
  top: 0;
  height: 100%;
  background: var(--color-primary);
  opacity: 0.3;
  border-radius: inherit;
}

.confidence-point {
  position: absolute;
  top: 50%;
  transform: translate(-50%, -50%);
  width: 10px;
  height: 10px;
  background: var(--color-text-primary);
  border: 2px solid var(--color-primary);
  border-radius: 50%;
  box-shadow: 0 0 10px var(--color-primary);
  transition: left 0.8s var(--ease-premium);
}

.chart-forensic {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.01);
}

.chart-container {
  width: 100%;
  height: 180px;
}

.strategy-box {
  padding: var(--space-16);
  background: var(--color-primary-muted);
  border: 1px solid var(--color-primary-glow);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.strategy-header {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 9px;
  font-weight: 800;
  color: var(--color-primary);
  letter-spacing: 0.1em;
}

.strategy-text {
  margin: 0;
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  line-height: 1.6;
  font-weight: 500;
}

.text--critical { color: var(--color-danger); }
.text--high { color: #f97316; }
.text--medium { color: var(--color-warning); }
.text--low { color: var(--color-success); }
</style>
