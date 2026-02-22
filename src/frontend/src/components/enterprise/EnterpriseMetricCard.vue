<template>
  <div
    class="metric-card"
    :class="[
      `metric-card--${status}`,
      { 'metric-card--compact': compact }
    ]"
  >
    <div class="metric-card__header">
      <span class="metric-card__title">{{ title }}</span>
      <span v-if="status" class="metric-card__status-indicator" :class="`metric-card__status-indicator--${status}`" />
    </div>

    <div class="metric-card__body">
      <div class="metric-card__value-container">
        <span v-if="prefix" class="metric-card__prefix">{{ prefix }}</span>
        <span class="metric-card__value" :class="{ 'metric-card__value--mono': mono }">
          {{ displayValue }}
        </span>
        <span v-if="unit" class="metric-card__unit">{{ unit }}</span>
      </div>

      <div v-if="delta !== undefined" class="metric-card__delta" :class="deltaClass">
        <svg v-if="delta > 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="18 15 12 9 6 15" />
        </svg>
        <svg v-else-if="delta < 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="6 9 12 15 18 9" />
        </svg>
        <span>{{ Math.abs(delta).toFixed(1) }}%</span>
      </div>
    </div>

    <div v-if="lastUpdated" class="metric-card__footer">
      <span class="metric-card__timestamp">{{ formatTime(lastUpdated) }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'

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

// Animated value display
const displayValue = ref(formatNumber(props.value))

function formatNumber(val: number | string): string {
  if (typeof val === 'string') return val
  return val.toLocaleString('en-US', {
    minimumFractionDigits: props.precision,
    maximumFractionDigits: props.precision
  })
}

// Watch for value changes
watch(() => props.value, (newVal) => {
  displayValue.value = formatNumber(newVal)
})

const deltaClass = computed(() => {
  if (props.delta === undefined) return ''
  if (props.delta > 0) return 'metric-card__delta--up'
  if (props.delta < 0) return 'metric-card__delta--down'
  return 'metric-card__delta--neutral'
})

function formatTime(date: Date | string): string {
  const d = typeof date === 'string' ? new Date(date) : date
  const now = new Date()
  const diffMs = now.getTime() - d.getTime()
  const diffSeconds = Math.floor(diffMs / 1000)

  if (diffSeconds < 5) return 'just now'
  if (diffSeconds < 60) return `${diffSeconds}s ago`

  const diffMinutes = Math.floor(diffSeconds / 60)
  if (diffMinutes < 60) return `${diffMinutes}m ago`

  return d.toLocaleTimeString()
}
</script>

<style scoped>
.metric-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  padding: var(--space-16);
  transition: all var(--transition-fast);
}

.metric-card:hover {
  border-color: var(--color-border);
  box-shadow: var(--shadow-elevated);
}

/* Status variants */
.metric-card--healthy {
  border-left: 3px solid var(--color-success);
}

.metric-card--warning {
  border-left: 3px solid var(--color-warning);
}

.metric-card--critical {
  border-left: 3px solid var(--color-danger);
}

.metric-card--offline {
  border-left: 3px solid var(--color-text-muted);
}

/* Compact variant */
.metric-card--compact {
  padding: var(--space-12);
}

.metric-card--compact .metric-card__value {
  font-size: var(--font-size-xl);
}

/* Header */
.metric-card__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-8);
}

.metric-card__title {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  font-weight: 500;
}

.metric-card__status-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
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
  box-shadow: 0 0 8px var(--color-danger);
  animation: pulse-critical 1.5s infinite;
}

.metric-card__status-indicator--offline {
  background: var(--color-text-muted);
}

@keyframes pulse-critical {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

/* Body */
.metric-card__body {
  display: flex;
  align-items: baseline;
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
  color: var(--color-text-secondary);
}

.metric-card__value {
  font-size: var(--font-size-2xl);
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1;
}

.metric-card__value--mono {
  font-family: var(--font-mono);
  font-variant-numeric: tabular-nums;
}

.metric-card__unit {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  font-weight: 500;
}

/* Delta indicator */
.metric-card__delta {
  display: flex;
  align-items: center;
  gap: var(--space-1);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.metric-card__delta svg {
  width: 14px;
  height: 14px;
}

.metric-card__delta--up {
  color: var(--color-success);
}

.metric-card__delta--down {
  color: var(--color-danger);
}

.metric-card__delta--neutral {
  color: var(--color-text-muted);
}

/* Footer */
.metric-card__footer {
  margin-top: var(--space-8);
  padding-top: var(--space-8);
  border-top: 1px solid var(--color-border-subtle);
}

.metric-card__timestamp {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}
</style>
