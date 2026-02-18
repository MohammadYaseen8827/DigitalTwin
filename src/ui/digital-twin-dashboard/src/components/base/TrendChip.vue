<template>
  <div class="trend-chip" :class="[`intent-${intent}`, trendClass]">
    <div class="trend-content">
      <div class="trend-icon-container">
        <component 
          :is="iconComponent" 
          :size="14" 
          stroke-width="2.5"
          class="trend-icon"
          :class="{ 'pulse-animation': isCritical && trend > 0 }"
        />
      </div>
      <span class="trend-value">
        <span class="trend-number">{{ formattedValue }}</span>
        <span v-if="showPercentage" class="trend-percentage">{{ formattedTrend }}</span>
        <span v-if="label" class="trend-label">{{ label }}</span>
      </span>
    </div>
    <div v-if="showGlow" class="trend-glow"></div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ArrowUp, ArrowDown, Minus, AlertTriangle, AlertCircle, CheckCircle2 } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  trend: number
  value?: number | string
  label?: string
  intent?: 'neutral' | 'positive' | 'warning' | 'critical'
  showPercentage?: boolean
  showGlow?: boolean
  size?: 'sm' | 'md' | 'lg'
}>(), {
  intent: 'neutral',
  showPercentage: true,
  showGlow: false,
  size: 'md',
  value: undefined
})

const isCritical = computed(() => props.intent === 'critical')
const isPositive = computed(() => props.intent === 'positive')
const isWarning = computed(() => props.intent === 'warning')

const trendClass = computed(() => ({
  'trend-flat': props.trend === 0,
  'trend-up': props.trend > 0,
  'trend-down': props.trend < 0,
  'size-sm': props.size === 'sm',
  'size-md': props.size === 'md',
  'size-lg': props.size === 'lg',
  'has-glow': props.showGlow,
  'has-label': !!props.label
}))

const iconComponent = computed(() => {
  if (isCritical.value) return AlertCircle
  if (isWarning.value) return AlertTriangle
  if (isPositive.value) return CheckCircle2
  if (props.trend > 0) return ArrowUp
  if (props.trend < 0) return ArrowDown
  return Minus
})

const formattedTrend = computed(() => {
  if (props.trend === 0) return '0%'
  return `${Math.abs(props.trend).toFixed(1)}%`
})

const formattedValue = computed(() => {
  if (props.value !== undefined) {
    if (typeof props.value === 'number') {
      return props.value.toLocaleString()
    }
    return props.value
  }
  return ''
})
</script>

<style scoped>
.trend-chip {
  --trend-chip-bg: color-mix(in srgb, var(--color-border-subtle) 60%, transparent);
  --trend-chip-color: var(--color-text-secondary);
  --trend-chip-glow: transparent;
  
  position: relative;
  display: inline-flex;
  align-items: center;
  border-radius: var(--radius-xl);
  font-weight: 600;
  letter-spacing: 0.01em;
  background: var(--trend-chip-bg);
  color: var(--trend-chip-color);
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 1px 2px 0 rgb(0 0 0 / 0.05);
  border: 1px solid color-mix(in srgb, currentColor 20%, transparent);
}

.trend-chip.size-sm {
  font-size: var(--font-size-xs);
  padding: var(--space-2) var(--space-8);
  height: 24px;
}

.trend-chip.size-md {
  font-size: var(--font-size-sm);
  padding: var(--space-4) var(--space-12);
  height: 32px;
}

.trend-chip.size-lg {
  font-size: var(--font-size-base);
  padding: var(--space-6) var(--space-16);
  height: 40px;
}

.trend-chip:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px -2px color-mix(in srgb, currentColor 15%, transparent);
}

.trend-chip:active {
  transform: translateY(0);
}

.trend-content {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-8);
  z-index: 2;
}

.trend-icon-container {
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform 0.2s ease;
}

.trend-icon {
  flex-shrink: 0;
  transition: all 0.2s ease;
}

.trend-value {
  display: flex;
  align-items: baseline;
  gap: var(--space-4);
  line-height: 1.2;
}

.trend-number {
  font-weight: 700;
  font-feature-settings: 'tnum' on, 'lnum' on;
}

.trend-percentage {
  font-size: 0.85em;
  opacity: 0.9;
}

.trend-label {
  font-size: 0.85em;
  opacity: 0.8;
  font-weight: 500;
  margin-left: 2px;
}

/* Intents */
.intent-positive {
  --trend-chip-bg: linear-gradient(
    135deg,
    color-mix(in srgb, var(--color-success) 20%, transparent) 0%,
    color-mix(in srgb, var(--color-success) 5%, transparent) 100%
  );
  --trend-chip-color: var(--color-success);
}

.intent-warning {
  --trend-chip-bg: linear-gradient(
    135deg,
    color-mix(in srgb, var(--color-warning) 20%, transparent) 0%,
    color-mix(in srgb, var(--color-warning) 5%, transparent) 100%
  );
  --trend-chip-color: var(--color-warning);
  --trend-chip-glow: color-mix(in srgb, var(--color-warning) 20%, transparent);
}

.intent-critical {
  --trend-chip-bg: linear-gradient(
    135deg,
    color-mix(in srgb, var(--color-error) 20%, transparent) 0%,
    color-mix(in srgb, var(--color-error) 5%, transparent) 100%
  );
  --trend-chip-color: var(--color-error);
  --trend-chip-glow: color-mix(in srgb, var(--color-error) 20%, transparent);
}

.intent-neutral {
  --trend-chip-bg: color-mix(in srgb, var(--color-primary) 12%, var(--color-bg-subtle));
  --trend-chip-color: var(--color-primary);
}

/* Trend directions */
.trend-up .trend-icon {
  color: var(--color-success);
  filter: drop-shadow(0 2px 8px color-mix(in srgb, var(--color-success) 30%, transparent));
}

.trend-down .trend-icon {
  color: var(--color-error);
  transform: translateY(1px);
}

.trend-flat .trend-icon {
  color: var(--color-text-tertiary);
}

/* Glow effect */
.trend-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(
    circle at center,
    var(--trend-chip-glow) 0%,
    transparent 70%
  );
  opacity: 0.3;
  z-index: 1;
  pointer-events: none;
}

/* Animations */
@keyframes pulse {
  0% { opacity: 0.6; }
  50% { opacity: 1; }
  100% { opacity: 0.6; }
}

.pulse-animation {
  animation: pulse 2s infinite ease-in-out;
}

/* Responsive */
@media (max-width: 480px) {
  .trend-chip {
    padding: var(--space-3) var(--space-10);
  }
  
  .trend-chip.has-label .trend-label {
    display: none;
  }
}
</style>
