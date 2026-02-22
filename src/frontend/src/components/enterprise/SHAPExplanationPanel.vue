<template>
  <div class="shap-panel">
    <div class="shap-panel__header">
      <h3 class="shap-panel__title">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z" />
          <polyline points="3.27 6.96 12 12.01 20.73 6.96" />
          <line x1="12" y1="22.08" x2="12" y2="12" />
        </svg>
        ML Explainability
      </h3>
      <button class="shap-panel__toggle" @click="expanded = !expanded">
        <svg :class="{ rotated: expanded }" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="6 9 12 15 18 9" />
        </svg>
      </button>
    </div>

    <div class="shap-panel__body">
      <!-- Summary -->
      <div class="shap-panel__summary">
        <div class="shap-panel__prediction">
          <span class="shap-panel__prediction-label">Prediction</span>
          <span class="shap-panel__prediction-value" :class="predictionClass">
            {{ predictionLabel }}
          </span>
        </div>
        <div class="shap-panel__confidence">
          <span class="shap-panel__confidence-label">Confidence</span>
          <span class="shap-panel__confidence-value">{{ (confidence * 100).toFixed(1) }}%</span>
        </div>
      </div>

      <!-- Expandable section -->
      <Transition name="expand">
        <div v-show="expanded" class="shap-panel__details">
          <!-- Feature importance chart -->
          <div v-if="features.length > 0" class="shap-panel__chart">
            <div class="shap-panel__chart-header">
              <span class="shap-panel__chart-title">Feature Contributions</span>
              <span class="shap-panel__chart-subtitle">SHAP values</span>
            </div>
            <div ref="chartRef" class="shap-panel__chart-container" />
          </div>

          <!-- Why this prediction -->
          <div v-if="topPositive.length > 0 || topNegative.length > 0" class="shap-panel__interpretation">
            <div class="shap-panel__interpretation-section">
              <h4 class="shap-panel__interpretation-title shap-panel__interpretation-title--positive">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="18 15 12 9 6 15" />
                </svg>
                Factors Increasing Failure Risk
              </h4>
              <ul class="shap-panel__factor-list">
                <li v-for="item in topPositive" :key="item.feature" class="shap-panel__factor-item">
                  <span class="shap-panel__factor-name">{{ item.feature }}</span>
                  <span class="shap-panel__factor-value shap-panel__factor-value--positive">
                    +{{ item.value.toFixed(3) }}
                  </span>
                </li>
              </ul>
            </div>

            <div class="shap-panel__interpretation-section">
              <h4 class="shap-panel__interpretation-title shap-panel__interpretation-title--negative">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                  <polyline points="6 9 12 15 18 9" />
                </svg>
                Factors Decreasing Failure Risk
              </h4>
              <ul class="shap-panel__factor-list">
                <li v-for="item in topNegative" :key="item.feature" class="shap-panel__factor-item">
                  <span class="shap-panel__factor-name">{{ item.feature }}</span>
                  <span class="shap-panel__factor-value shap-panel__factor-value--negative">
                    {{ item.value.toFixed(3) }}
                  </span>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </Transition>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import * as echarts from 'echarts'
import { createSHAPChartOption } from '@/utils/echartsConfig'

interface SHAPFeature {
  feature: string
  value: number
}

interface Props {
  features: SHAPFeature[]
  confidence: number
  prediction: 'failure' | 'healthy' | 'degrading'
}

const props = defineProps<Props>()

const chartRef = ref<HTMLElement>()
const expanded = ref(false)

const predictionLabel = computed(() => {
  switch (props.prediction) {
    case 'failure': return 'Failure Predicted'
    case 'healthy': return 'Healthy'
    case 'degrading': return 'Degrading'
    default: return 'Unknown'
  }
})

const predictionClass = computed(() => {
  return `shap-panel__prediction-value--${props.prediction}`
})

const topPositive = computed(() => {
  return props.features
    .filter(f => f.value > 0)
    .sort((a, b) => b.value - a.value)
    .slice(0, 5)
})

const topNegative = computed(() => {
  return props.features
    .filter(f => f.value < 0)
    .sort((a, b) => a.value - b.value)
    .slice(0, 5)
})

// Initialize chart
let chart: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value || props.features.length === 0) return

  if (chart) {
    chart.dispose()
  }

  chart = echarts.init(chartRef.value)

  const option = createSHAPChartOption(props.features.slice(0, 10))
  chart.setOption(option)
}

watch(() => props.features, initChart, { deep: true })

onMounted(() => {
  initChart()

  if (chartRef.value) {
    const resizeObserver = new ResizeObserver(() => {
      chart?.resize()
    })
    resizeObserver.observe(chartRef.value)
  }
})
</script>

<style scoped>
.shap-panel {
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.shap-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-16);
  border-bottom: 1px solid var(--color-border-subtle);
}

.shap-panel__title {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  margin: 0;
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
}

.shap-panel__title svg {
  width: 18px;
  height: 18px;
  color: var(--color-primary);
}

.shap-panel__toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.shap-panel__toggle:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.shap-panel__toggle svg {
  width: 16px;
  height: 16px;
  transition: transform var(--transition-normal);
}

.shap-panel__toggle svg.rotated {
  transform: rotate(180deg);
}

.shap-panel__body {
  padding: var(--space-16);
}

/* Summary section */
.shap-panel__summary {
  display: flex;
  justify-content: space-between;
  gap: var(--space-16);
}

.shap-panel__prediction,
.shap-panel__confidence {
  flex: 1;
  text-align: center;
  padding: var(--space-12);
  background: var(--color-surface-elevated);
  border-radius: var(--radius-md);
}

.shap-panel__prediction-label,
.shap-panel__confidence-label {
  display: block;
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: var(--space-4);
}

.shap-panel__prediction-value {
  font-size: var(--font-size-lg);
  font-weight: 700;
}

.shap-panel__prediction-value--failure {
  color: var(--color-danger);
}

.shap-panel__prediction-value--healthy {
  color: var(--color-success);
}

.shap-panel__prediction-value--degrading {
  color: var(--color-warning);
}

.shap-panel__confidence-value {
  font-family: var(--font-mono);
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--color-text-primary);
}

/* Details section */
.shap-panel__details {
  margin-top: var(--space-16);
}

/* Chart */
.shap-panel__chart {
  margin-bottom: var(--space-16);
}

.shap-panel__chart-header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  margin-bottom: var(--space-8);
}

.shap-panel__chart-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
}

.shap-panel__chart-subtitle {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.shap-panel__chart-container {
  width: 100%;
  height: 200px;
}

/* Interpretation */
.shap-panel__interpretation {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-16);
}

.shap-panel__interpretation-section {
  padding: var(--space-12);
  background: var(--color-surface-elevated);
  border-radius: var(--radius-md);
}

.shap-panel__interpretation-title {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  margin: 0 0 var(--space-8);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.shap-panel__interpretation-title svg {
  width: 14px;
  height: 14px;
}

.shap-panel__interpretation-title--positive {
  color: var(--color-shap-positive);
}

.shap-panel__interpretation-title--negative {
  color: var(--color-shap-negative);
}

.shap-panel__factor-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.shap-panel__factor-item {
  display: flex;
  justify-content: space-between;
  padding: var(--space-4) 0;
  border-bottom: 1px solid var(--color-border-subtle);
}

.shap-panel__factor-item:last-child {
  border-bottom: none;
}

.shap-panel__factor-name {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.shap-panel__factor-value {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.shap-panel__factor-value--positive {
  color: var(--color-shap-positive);
}

.shap-panel__factor-value--negative {
  color: var(--color-shap-negative);
}

/* Expand transition */
.expand-enter-active,
.expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
}

.expand-enter-to,
.expand-leave-from {
  opacity: 1;
  max-height: 600px;
}
</style>
