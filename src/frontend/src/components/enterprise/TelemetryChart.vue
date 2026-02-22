<template>
  <div class="telemetry-chart">
    <div class="telemetry-chart__header">
      <div class="telemetry-chart__title-section">
        <h3 class="telemetry-chart__title">{{ title }}</h3>
        <span v-if="currentValue !== undefined" class="telemetry-chart__value">
          {{ formatValue(currentValue) }}
          <span v-if="unit" class="telemetry-chart__unit">{{ unit }}</span>
        </span>
      </div>

      <div class="telemetry-chart__controls">
        <!-- Pause/Resume -->
        <button
          class="telemetry-chart__control"
          :class="{ active: isPaused }"
          @click="togglePause"
          :title="isPaused ? 'Resume' : 'Pause'"
        >
          <svg v-if="!isPaused" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="6" y="4" width="4" height="16" />
            <rect x="14" y="4" width="4" height="16" />
          </svg>
          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <polygon points="5 3 19 12 5 21 5 3" />
          </svg>
        </button>

        <!-- Time window selector -->
        <select v-model="selectedWindow" class="telemetry-chart__select">
          <option value="1m">1 min</option>
          <option value="5m">5 min</option>
          <option value="15m">15 min</option>
          <option value="30m">30 min</option>
          <option value="1h">1 hour</option>
        </select>
      </div>
    </div>

    <div class="telemetry-chart__container" ref="chartRef" />

    <div v-if="anomalyRegions.length > 0" class="telemetry-chart__anomalies">
      <span class="telemetry-chart__anomaly-label">Anomalies detected:</span>
      <span
        v-for="region in anomalyRegions"
        :key="region.id"
        class="telemetry-chart__anomaly-tag"
      >
        {{ formatTime(region.start) }} - {{ formatTime(region.end) }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, computed } from 'vue'
import * as echarts from 'echarts'
import { createTelemetryOption, streamingChartBase } from '@/utils/echartsConfig'

interface AnomalyRegion {
  id: string
  start: number
  end: number
}

interface Props {
  title: string
  unit?: string
  color?: string
  data: Array<[number, number]>
  currentValue?: number
  isLive?: boolean
  minValue?: number
  maxValue?: number
  anomalyRegions?: AnomalyRegion[]
}

const props = withDefaults(defineProps<Props>(), {
  color: '#38BDF8',
  isLive: true,
  anomalyRegions: () => []
})

const chartRef = ref<HTMLElement>()
const isPaused = ref(false)
const selectedWindow = ref('5m')

let chart: echarts.ECharts | null = null
let animationFrameId: number | null = null

const windowMinutes = computed(() => {
  const map: Record<string, number> = {
    '1m': 1,
    '5m': 5,
    '15m': 15,
    '30m': 30,
    '1h': 60
  }
  return map[selectedWindow.value] || 5
})

function formatValue(value: number): string {
  return value.toLocaleString('en-US', {
    minimumFractionDigits: 1,
    maximumFractionDigits: 2
  })
}

function formatTime(timestamp: number): string {
  return new Date(timestamp).toLocaleTimeString()
}

function togglePause() {
  isPaused.value = !isPaused.value
  if (!isPaused.value && props.isLive) {
    startStreaming()
  }
}

function initChart() {
  if (!chartRef.value) return

  if (chart) {
    chart.dispose()
  }

  chart = echarts.init(chartRef.value)

  updateChart()

  // Handle resize
  const resizeObserver = new ResizeObserver(() => {
    chart?.resize()
  })
  resizeObserver.observe(chartRef.value)
}

function updateChart() {
  if (!chart) return

  // Filter data based on time window
  const now = Date.now()
  const windowMs = windowMinutes.value * 60 * 1000
  const filteredData = props.data.filter(([timestamp]) => timestamp >= now - windowMs)

  const option = createTelemetryOption({
    data: filteredData,
    color: props.color,
    yAxisLabel: props.unit,
    min: props.minValue,
    max: props.maxValue
  })

  chart.setOption(option, { notMerge: false })
}

function startStreaming() {
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }

  const tick = () => {
    if (!isPaused.value && props.isLive) {
      updateChart()
    }
    animationFrameId = requestAnimationFrame(tick)
  }

  tick()
}

// Watch for data changes
watch(() => props.data, () => {
  if (!isPaused.value) {
    updateChart()
  }
}, { deep: true })

// Watch for window changes
watch(selectedWindow, () => {
  updateChart()
})

onMounted(() => {
  initChart()
  if (props.isLive) {
    startStreaming()
  }
})

onUnmounted(() => {
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }
  if (chart) {
    chart.dispose()
  }
})
</script>

<style scoped>
.telemetry-chart {
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.telemetry-chart__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-12) var(--space-16);
  border-bottom: 1px solid var(--color-border-subtle);
}

.telemetry-chart__title-section {
  display: flex;
  align-items: baseline;
  gap: var(--space-12);
}

.telemetry-chart__title {
  margin: 0;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
}

.telemetry-chart__value {
  font-family: var(--font-mono);
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--color-telemetry);
}

.telemetry-chart__unit {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-muted);
  margin-left: var(--space-2);
}

.telemetry-chart__controls {
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.telemetry-chart__control {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.telemetry-chart__control:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.telemetry-chart__control.active {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
}

.telemetry-chart__control svg {
  width: 16px;
  height: 16px;
}

.telemetry-chart__select {
  padding: var(--space-6) var(--space-8);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-text-primary);
  font-size: var(--font-size-sm);
  cursor: pointer;
}

.telemetry-chart__container {
  width: 100%;
  height: 280px;
  padding: var(--space-8);
}

.telemetry-chart__anomalies {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-8) var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
  background: rgba(239, 68, 68, 0.05);
}

.telemetry-chart__anomaly-label {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.telemetry-chart__anomaly-tag {
  padding: var(--space-2) var(--space-8);
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-xs);
  font-family: var(--font-mono);
  color: var(--color-danger);
}
</style>
