<template>
  <UiCard variant="default" padding="none" hover :class="styles['shap-panel']">
    <div :class="styles['shap-panel__header']">
      <div :class="styles['title-group']">
        <h3 :class="styles['shap-panel__title']">
          <Binary :width="16" :height="16" :class="styles['title-icon']" />
          Neural Feature Attribution
        </h3>
        <p :class="styles['shap-panel__subtitle']">SHAP-based model interpretability</p>
      </div>
      <button :class="styles['shap-panel__toggle']" @click="expanded = !expanded">
        <component :is="expanded ? ChevronUp : ChevronDown" :width="16" :height="16" />
      </button>
    </div>

    <div :class="styles['shap-panel__body']">
      <!-- Summary Matrix -->
      <div :class="styles['shap-panel__summary']">
        <div :class="styles['summary-box']">
          <span :class="styles['summary-label']">Target State</span>
          <span :class="[styles['summary-val'], styles[`summary-val--${prediction}`]]">
            {{ predictionLabel }}
          </span>
        </div>
        <div :class="styles['summary-box']">
          <span :class="styles['summary-label']">Neural Confidence</span>
          <span :class="styles['summary-val-mono']">{{ (confidence * 100).toFixed(1) }}%</span>
        </div>
      </div>

      <!-- Expandable Forensic Depth -->
      <Transition name="expand">
        <div v-show="expanded" :class="styles['shap-panel__details']">
          <div v-if="features.length > 0" :class="styles['shap-panel__chart-wrap']">
            <div ref="chartRef" :class="styles['shap-panel__chart-container']" />
          </div>

          <div v-if="topPositive.length > 0 || topNegative.length > 0" :class="styles['interpretation-grid']">
            <div :class="styles['interpretation-col']">
              <h4 :class="[styles['col-title'], styles['col-title--positive']]">
                <TrendingUp :width="14" :height="14" />
                Risk Escalation
              </h4>
              <div :class="styles['factor-list']">
                <div v-for="item in topPositive" :key="item.feature" :class="styles['factor-item']">
                  <span :class="styles['factor-name']">{{ item.feature }}</span>
                  <span :class="[styles['factor-val'], styles['factor-val--positive']]">
                    +{{ item.value.toFixed(3) }}
                  </span>
                </div>
              </div>
            </div>

            <div :class="styles['interpretation-col']">
              <h4 :class="[styles['col-title'], styles['col-title--negative']]">
                <TrendingDown :width="14" :height="14" />
                Risk Mitigation
              </h4>
              <div :class="styles['factor-list']">
                <div v-for="item in topNegative" :key="item.feature" :class="styles['factor-item']">
                  <span :class="styles['factor-name']">{{ item.feature }}</span>
                  <span :class="[styles['factor-val'], styles['factor-val--negative']]">
                    {{ item.value.toFixed(3) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </Transition>
    </div>
  </UiCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, useCssModule } from 'vue'
import UiCard from '../ui/UiCard.vue'
import { Binary, ChevronUp, ChevronDown, TrendingUp, TrendingDown } from 'lucide-vue-next'
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
const styles = useCssModule()
const chartRef = ref<HTMLElement>()
const expanded = ref(true)

const predictionLabel = computed(() => {
  switch (props.prediction) {
    case 'failure': return 'FAIL PREDICT'
    case 'healthy': return 'NOMINAL'
    case 'degrading': return 'DEGRADING'
    default: return 'UNKNOWN'
  }
})

const topPositive = computed(() => props.features.filter(f => f.value > 0).sort((a, b) => b.value - a.value).slice(0, 4))
const topNegative = computed(() => props.features.filter(f => f.value < 0).sort((a, b) => a.value - b.value).slice(0, 4))

let chart: echarts.ECharts | null = null

function initChart() {
  if (!chartRef.value || props.features.length === 0) return
  if (chart) chart.dispose()
  chart = echarts.init(chartRef.value, 'hub-dark')
  const option = createSHAPChartOption(props.features.slice(0, 8))
  chart.setOption({
    ...option,
    backgroundColor: 'transparent',
    grid: { left: '3%', right: '5%', top: '5%', bottom: '5%', containLabel: true }
  })
}

watch(() => props.features, initChart, { deep: true })

onMounted(() => {
  initChart()
  const ro = new ResizeObserver(() => chart?.resize())
  if (chartRef.value) ro.observe(chartRef.value)
})
</script>

<style module>
.shap-panel {
  position: relative;
}

.shap-panel__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-20) var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.title-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.shap-panel__title {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  margin: 0;
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.title-icon { color: var(--color-primary); }

.shap-panel__subtitle {
  margin: 0;
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.shap-panel__toggle {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.shap-panel__toggle:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.shap-panel__body {
  padding: var(--space-24);
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.shap-panel__summary {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-16);
}

.summary-box {
  padding: var(--space-12) var(--space-16);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  text-align: center;
}

.summary-label {
  font-size: 9px;
  font-weight: 800;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.summary-val { font-size: var(--font-size-sm); font-weight: 800; }
.summary-val--failure { color: var(--color-danger); }
.summary-val--degrading { color: var(--color-warning); }
.summary-val--healthy { color: var(--color-success); }

.summary-val-mono {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  font-weight: 800;
  color: var(--color-text-primary);
}

.shap-panel__details {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.shap-panel__chart-wrap {
  background: rgba(255, 255, 255, 0.01);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-8);
}

.shap-panel__chart-container {
  width: 100%;
  height: 200px;
}

.interpretation-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-24);
}

.interpretation-col {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.col-title {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0;
}

.col-title--positive { color: #f97316; }
.col-title--negative { color: var(--color-success); }

.factor-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.factor-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: var(--space-8);
  border-bottom: 1px solid var(--color-border-subtle);
}

.factor-name {
  font-size: 11px;
  color: var(--color-text-secondary);
  font-weight: 600;
}

.factor-val {
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 700;
}

.factor-val--positive { color: #f97316; }
.factor-val--negative { color: var(--color-success); }

@keyframes expand {
  from { max-height: 0; opacity: 0; }
  to { max-height: 800px; opacity: 1; }
}

.expand-enter-active { animation: expand 0.4s var(--ease-premium); overflow: hidden; }
.expand-leave-active { animation: expand 0.4s var(--ease-premium) reverse; overflow: hidden; }
</style>
