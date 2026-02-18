<template>
  <span
    class="metric-value"
    :class="[
      `metric-value--${formatted.status}`,
      { 'metric-value--with-icon': showIcon, 'metric-value--updated': wasUpdated }
    ]"
    role="text"
  >
    <component
      v-if="showIcon"
      :is="iconComponent"
      class="metric-value__icon"
      aria-hidden="true"
    />
    <span class="metric-value__text">{{ formatted.display }}</span>
    <slot name="suffix" />
  </span>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import type { FormatMetricOptions, FormattedMetricValue, ValueStatus } from '@/utils/valueFormatter'
import { formatMetricValue } from '@/utils/valueFormatter'

import { MinusCircle, HelpCircle, Ban, AlertTriangle, Info } from 'lucide-vue-next'

const iconMap: Partial<Record<ValueStatus, any>> = {
  zero: MinusCircle,
  not_available: HelpCircle,
  not_applicable: Ban,
  missing: AlertTriangle,
  value: Info
}

const props = withDefaults(
  defineProps<
    {
      value: unknown
      precision?: number
      suffix?: string
      notApplicable?: boolean
      tooltip?: string
      showIcon?: boolean
    }
  >(),
  {
    precision: 0,
    suffix: '',
    notApplicable: false,
    tooltip: '',
    showIcon: true
  }
)

const formatted = computed<FormattedMetricValue>(() =>
  formatMetricValue(props.value, {
    precision: props.precision,
    suffix: props.suffix,
    notApplicable: props.notApplicable,
    tooltip: props.tooltip
  } as FormatMetricOptions)
)

const iconComponent = computed(() => iconMap[formatted.value.status])
const showIcon = computed(() => props.showIcon && Boolean(iconComponent.value) && formatted.value.status !== 'value')
const tooltipText = computed(() => formatted.value.tooltip || '')

const wasUpdated = ref(false)

watch(
  () => formatted.value.display,
  () => {
    wasUpdated.value = false
    window.requestAnimationFrame(() => {
      wasUpdated.value = true
      window.setTimeout(() => {
        wasUpdated.value = false
      }, 600)
    })
  }
)
</script>

<style scoped>
.metric-value {
  display: inline-flex;
  align-items: center;
  gap: var(--space-6);
  font-variant-numeric: tabular-nums;
  font-weight: 600;
  color: var(--color-text-primary);
}

.metric-value--updated {
  animation: metricFlash 0.6s ease;
}

.metric-value__icon {
  width: 16px;
  height: 16px;
  opacity: 0.85;
}

.metric-value__text {
  line-height: 1.2;
}

.metric-value--zero {
  color: var(--color-warning);
}

.metric-value--not_available {
  color: var(--color-text-secondary);
}

.metric-value--not_applicable {
  color: var(--color-text-secondary);
  font-style: italic;
}

.metric-value--missing {
  color: var(--color-error);
}

.metric-value--with-icon .metric-value__text {
  margin-left: 0;
}

@keyframes metricFlash {
  0% {
    box-shadow: 0 0 0 rgba(99, 102, 241, 0.4);
    background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  }
  70% {
    box-shadow: 0 0 12px rgba(99, 102, 241, 0.35);
    background: color-mix(in srgb, var(--color-primary-light) 18%, transparent);
  }
  100% {
    box-shadow: none;
    background: transparent;
  }
}
</style>
