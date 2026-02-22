<template>
  <div class="chart-container" :class="{ 'has-title': title || $slots.title, 'is-loading': loading }">
    <div v-if="title || $slots.title" class="chart-header">
      <slot name="title">
        <h3 class="chart-title">{{ title }}</h3>
      </slot>
      <div v-if="$slots.actions" class="chart-actions">
        <slot name="actions" />
      </div>
    </div>
    <div class="chart-body" :style="{ height: computedHeight }">
      <div v-if="loading" class="chart-loading">
        <div class="chart-spinner"></div>
        <span class="loading-label">Loading data...</span>
      </div>
      <slot v-else />
    </div>
    <div v-if="legend || $slots.legend" class="chart-legend">
      <slot name="legend" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  height: {
    type: [String, Number],
    default: '300px'
  },
  loading: {
    type: Boolean,
    default: false
  },
  legend: {
    type: Boolean,
    default: false
  }
})

const computedHeight = computed(() => {
  if (typeof props.height === 'number') return `${props.height}px`
  return props.height
})
</script>

<style scoped>
.chart-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
  width: 100%;
}

.chart-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-12);
  padding-bottom: var(--space-8);
  border-bottom: 1px solid var(--color-border-subtle);
}

.chart-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.chart-actions {
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.chart-body {
  position: relative;
  width: 100%;
  min-height: 100px;
}

.chart-loading {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-12);
  background: var(--color-surface);
  border-radius: var(--radius-md);
}

.chart-spinner {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: 3px solid var(--color-border-subtle);
  border-top-color: var(--color-primary);
  animation: spin 0.9s linear infinite;
}

.loading-label {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.chart-legend {
  padding-top: var(--space-8);
  border-top: 1px solid var(--color-border-subtle);
}

.chart-container.is-loading .chart-body {
  opacity: 0.5;
}
</style>
