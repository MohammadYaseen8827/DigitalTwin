<template>
  <div class="rul-panel">
    <div class="rul-panel__header">
      <h3 class="rul-panel__title">Remaining Useful Life</h3>
      <div class="rul-panel__machine">
        <span class="rul-panel__machine-name">{{ machineName }}</span>
        <span class="rul-panel__machine-id">{{ machineId }}</span>
      </div>
    </div>

    <div class="rul-panel__body">
      <!-- Primary RUL Display -->
      <div class="rul-panel__main">
        <div class="rul-panel__value-container">
          <span class="rul-panel__value" :class="riskClass">{{ formattedRUL }}</span>
          <span class="rul-panel__unit">hours</span>
        </div>

        <div class="rul-panel__risk" :class="`rul-panel__risk--${riskLevel.toLowerCase()}`">
          <span class="rul-panel__risk-indicator" />
          <span class="rul-panel__risk-label">{{ riskLevel }} Risk</span>
        </div>
      </div>

      <!-- Confidence Interval -->
      <div v-if="confidenceInterval" class="rul-panel__confidence">
        <div class="rul-panel__confidence-label">Confidence Interval (95%)</div>
        <div class="rul-panel__confidence-range">
          <span class="rul-panel__confidence-value">{{ confidenceInterval.lower }}</span>
          <div class="rul-panel__confidence-bar">
            <div
              class="rul-panel__confidence-fill"
              :style="{ width: confidenceFillWidth }"
            />
            <div class="rul-panel__confidence-point" :style="{ left: confidencePointPosition + '%' }" />
          </div>
          <span class="rul-panel__confidence-value">{{ confidenceInterval.upper }}</span>
        </div>
      </div>

      <!-- Chart -->
      <div v-if="chartData.length > 0" class="rul-panel__chart">
        <div ref="chartRef" class="rul-panel__chart-container" />
      </div>

      <!-- Interpretation -->
      <div v-if="interpretation" class="rul-panel__interpretation">
        <div class="rul-panel__interpretation-header">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10" />
            <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
            <line x1="12" y1="17" x2="12.01" y2="17" />
          </svg>
          <span>Analysis</span>
        </div>
        <p class="rul-panel__interpretation-text">{{ interpretation }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
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

const riskClass = computed(() => {
  const level = riskLevel.value.toLowerCase()
  return `rul-panel__value--${level}`
})

const confidenceFillWidth = computed(() => {
  if (!props.confidenceInterval) return '0%'
  const range = props.confidenceInterval.upper - props.confidenceInterval.lower
  const mid = props.rul
  const fillWidth = (mid - props.confidenceInterval.lower) / range * 100
  return Math.min(100, Math.max(0, fillWidth)) + '%'
})

const confidencePointPosition = computed(() => {
  if (!props.confidenceInterval) return 50
  const range = props.confidenceInterval.upper - props.confidenceInterval.lower
  const position = (props.rul - props.confidenceInterval.lower) / range * 100
  return Math.min(100, Math.max(0, position))
})

// Initialize chart
let chart: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value || props.chartData.length === 0) return

  if (chart) {
    chart.dispose()
  }

  chart = echarts.init(chartRef.value)

  const option = createRULChartOption({
    timestamps: props.chartData.map(d => d.timestamp),
    predictions: props.chartData.map(d => d.prediction),
    lowerBound: props.chartData[0]?.lower ? props.chartData.map(d => d.lower!) : undefined,
    upperBound: props.chartData[0]?.upper ? props.chartData.map(d => d.upper!) : undefined
  })

  chart.setOption(option)
}

watch(() => props.chartData, initChart, { deep: true })

onMounted(() => {
  initChart()

  // Handle resize
  const resizeObserver = new ResizeObserver(() => {
    chart?.resize()
  })

  if (chartRef.value) {
    resizeObserver.observe(chartRef.value)
  }
})
</script>

<style scoped>
.rul-panel {
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.rul-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: var(--space-16);
  border-bottom: 1px solid var(--color-border-subtle);
}

.rul-panel__title {
  margin: 0;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
}

.rul-panel__machine {
  text-align: right;
}

.rul-panel__machine-name {
  display: block;
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
}

.rul-panel__machine-id {
  display: block;
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  font-family: var(--font-mono);
}

.rul-panel__body {
  padding: var(--space-16);
}

/* Main value display */
.rul-panel__main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-20);
}

.rul-panel__value-container {
  display: flex;
  align-items: baseline;
  gap: var(--space-4);
}

.rul-panel__value {
  font-size: 48px;
  font-weight: 700;
  font-family: var(--font-mono);
  line-height: 1;
  transition: color var(--transition-normal);
}

.rul-panel__value--low {
  color: var(--color-success);
}

.rul-panel__value--medium {
  color: var(--color-warning);
}

.rul-panel__value--high {
  color: #f97316;
}

.rul-panel__value--critical {
  color: var(--color-danger);
}

.rul-panel__unit {
  font-size: var(--font-size-lg);
  color: var(--color-text-muted);
}

/* Risk indicator */
.rul-panel__risk {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  padding: var(--space-6) var(--space-12);
  border-radius: var(--radius-sm);
}

.rul-panel__risk--low {
  background: rgba(34, 197, 94, 0.15);
}

.rul-panel__risk--medium {
  background: rgba(245, 158, 11, 0.15);
}

.rul-panel__risk--high {
  background: rgba(249, 115, 22, 0.15);
}

.rul-panel__risk--critical {
  background: rgba(239, 68, 68, 0.15);
}

.rul-panel__risk-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.rul-panel__risk--low .rul-panel__risk-indicator {
  background: var(--color-success);
}

.rul-panel__risk--medium .rul-panel__risk-indicator {
  background: var(--color-warning);
}

.rul-panel__risk--high .rul-panel__risk-indicator {
  background: #f97316;
}

.rul-panel__risk--critical .rul-panel__risk-indicator {
  background: var(--color-danger);
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.rul-panel__risk-label {
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.rul-panel__risk--low .rul-panel__risk-label {
  color: var(--color-success);
}

.rul-panel__risk--medium .rul-panel__risk-label {
  color: var(--color-warning);
}

.rul-panel__risk--high .rul-panel__risk-label {
  color: #f97316;
}

.rul-panel__risk--critical .rul-panel__risk-label {
  color: var(--color-danger);
}

/* Confidence interval */
.rul-panel__confidence {
  margin-bottom: var(--space-20);
}

.rul-panel__confidence-label {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  margin-bottom: var(--space-4);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.rul-panel__confidence-range {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.rul-panel__confidence-value {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  min-width: 48px;
}

.rul-panel__confidence-bar {
  flex: 1;
  height: 8px;
  background: var(--color-surface-elevated);
  border-radius: 4px;
  position: relative;
}

.rul-panel__confidence-fill {
  position: absolute;
  left: 0;
  top: 0;
  height: 100%;
  background: var(--color-confidence);
  border-radius: 4px;
  transition: width var(--transition-normal);
}

.rul-panel__confidence-point {
  position: absolute;
  top: 50%;
  transform: translate(-50%, -50%);
  width: 14px;
  height: 14px;
  background: var(--color-text-primary);
  border: 3px solid var(--color-confidence);
  border-radius: 50%;
  transition: left var(--transition-normal);
}

/* Chart */
.rul-panel__chart {
  margin-bottom: var(--space-16);
}

.rul-panel__chart-container {
  width: 100%;
  height: 180px;
}

/* Interpretation */
.rul-panel__interpretation {
  padding: var(--space-12);
  background: rgba(59, 130, 246, 0.05);
  border: 1px solid rgba(59, 130, 246, 0.1);
  border-radius: var(--radius-md);
}

.rul-panel__interpretation-header {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  margin-bottom: var(--space-8);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-primary);
}

.rul-panel__interpretation-header svg {
  width: 16px;
  height: 16px;
}

.rul-panel__interpretation-text {
  margin: 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.5;
}
</style>
