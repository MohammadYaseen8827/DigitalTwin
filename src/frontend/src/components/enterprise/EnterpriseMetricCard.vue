<template>
  <UiCard
    variant="glass"
    padding="md"
    hover
    :class="[
      styles['metric-card'],
      styles[`metric-card--${status}`],
      compact && styles['metric-card--compact']
    ]"
  >
    <div :class="styles['metric-card__header']">
      <span :class="styles['metric-card__title']">{{ title }}</span>
      <div v-if="status" :class="[styles['metric-card__status-indicator'], styles[`metric-card__status-indicator--${status}`]]" />
    </div>

    <div :class="styles['metric-card__body']">
      <div :class="styles['metric-card__value-container']">
        <span v-if="prefix" :class="styles['metric-card__prefix']">{{ prefix }}</span>
        <span :class="[styles['metric-card__value'], mono && styles['metric-card__value--mono']]">
          {{ displayValue }}
        </span>
        <span v-if="unit" :class="styles['metric-card__unit']">{{ unit }}</span>
      </div>

      <div v-if="delta !== undefined" :class="[styles['metric-card__delta'], deltaClass]">
        <component :is="delta > 0 ? ChevronUp : ChevronDown" :width="14" :height="14" />
        <span>{{ Math.abs(delta).toFixed(1) }}%</span>
      </div>
    </div>

    <div v-if="lastUpdated" :class="styles['metric-card__footer']">
      <div :class="styles['refresh-wrap']">
        <RefreshCw :width="10" :height="10" />
        <span :class="styles['metric-card__timestamp']">{{ formatTime(lastUpdated) }}</span>
      </div>
    </div>
  </UiCard>
</template>

<script setup lang="ts">
import { computed, ref, watch, useCssModule } from 'vue'
import UiCard from '../ui/UiCard.vue'
import { ChevronUp, ChevronDown, RefreshCw } from 'lucide-vue-next'

interface Props {
  title: string
  value: number | string
  unit?: string
  prefix?: string
  status?: 'healthy' | 'warning' | 'critical' | 'offline'
  trend?: 'up' | 'down' | 'neutral'
  delta?: number
  lastUpdated?: Date | string
  precision?: number
  mono?: boolean
  compact?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  precision: 0,
  mono: true,
  compact: false
})

const styles = useCssModule()

const displayValue = ref(formatNumber(props.value))

function formatNumber(val: number | string): string {
  if (typeof val === 'string') return val
  return val.toLocaleString('en-US', {
    minimumFractionDigits: props.precision,
    maximumFractionDigits: props.precision
  })
}

watch(() => props.value, (newVal) => {
  displayValue.value = formatNumber(newVal)
})

const deltaClass = computed(() => {
  if (props.delta === undefined) return ''
  if (props.delta > 0) return styles['metric-card__delta--up']
  if (props.delta < 0) return styles['metric-card__delta--down']
  return styles['metric-card__delta--neutral']
})

function formatTime(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date
  const now = new Date()
  const diffMs = now.getTime() - d.getTime()
  const diffSeconds = Math.floor(diffMs / 1000)

  if (diffSeconds < 5) return 'SYNCED'
  if (diffSeconds < 60) return `${diffSeconds}S AGO`

  const diffMinutes = Math.floor(diffSeconds / 60)
  if (diffMinutes < 60) return `${diffMinutes}M AGO`

  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }).toUpperCase()
}
</script>

<style module>
.metric-card {
  position: relative;
  border-radius: var(--radius-xl);
}

/* Header */
.metric-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-16);
}

.metric-card__title {
  font-size: 10px;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.12em;
  font-weight: 800;
}

.metric-card__status-indicator {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  position: relative;
}

.metric-card__status-indicator--healthy {
  background: var(--color-success);
  box-shadow: 0 0 8px var(--color-success);
}

.metric-card__status-indicator--warning {
  background: var(--color-warning);
  box-shadow: 0 0 8px var(--color-warning);
}

.metric-card__status-indicator--critical {
  background: var(--color-danger);
  box-shadow: 0 0 10px var(--color-danger);
  animation: pulse-critical 2s infinite;
}

@keyframes pulse-critical {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.4; transform: scale(1.5); }
}

/* Body */
.metric-card__body {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-12);
}

.metric-card__value-container {
  display: flex;
  align-items: baseline;
  gap: var(--space-2);
}

.metric-card__prefix {
  font-size: var(--font-size-lg);
  color: var(--color-text-muted);
}

.metric-card__value {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  line-height: 1;
  letter-spacing: var(--font-tracking-tight);
}

.metric-card__value--mono {
  font-family: var(--font-mono);
}

.metric-card__unit {
  font-size: var(--font-size-sm);
  color: var(--color-text-dim);
  font-weight: 700;
  margin-left: var(--space-4);
}

/* Delta indicator */
.metric-card__delta {
  display: flex;
  align-items: center;
  gap: var(--space-2);
  font-size: var(--font-size-xs);
  font-weight: 700;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
}

.metric-card__delta--up {
  color: var(--color-success);
  background: var(--color-success-muted);
}

.metric-card__delta--down {
  color: var(--color-danger);
  background: var(--color-danger-muted);
}

/* Footer */
.metric-card__footer {
  margin-top: var(--space-16);
  padding-top: var(--space-12);
  border-top: 1px solid var(--color-border-subtle);
}

.refresh-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  color: var(--color-text-dim);
}

.metric-card__timestamp {
  font-size: 9px;
  font-weight: 800;
  letter-spacing: 0.05em;
}

/* Compact variant */
.metric-card--compact {
  padding: var(--space-12) var(--space-16);
}

.metric-card--compact .metric-card__value {
  font-size: var(--font-size-xl);
}
</style>
