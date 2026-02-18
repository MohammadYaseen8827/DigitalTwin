<template>
  <BaseCard class="stat-card" :class="[trendClass, `stat-card--${intent}`]" :variant="variant" :hoverable="hoverable">
    <!-- Decorative background accent -->
    <div class="stat-card__accent"></div>

    <div class="stat-card__icon" v-if="$slots.icon">
      <slot name="icon" />
    </div>

    <div class="stat-card__content">
      <p v-if="label" class="stat-card__label">{{ label }}</p>
      <div class="stat-card__value">
        <slot />
      </div>
      <p v-if="caption" class="stat-card__caption">{{ caption }}</p>
    </div>

    <!-- Trend indicator with icon -->
    <div class="stat-card__trend" v-if="trend !== null">
      <div class="stat-card__trend-icon" :class="trendIconClass">
        <svg v-if="trend > 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
          <polyline points="17 6 23 6 23 12"></polyline>
        </svg>
        <svg v-else-if="trend < 0" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <polyline points="23 18 13.5 8.5 8.5 13.5 1 6"></polyline>
          <polyline points="17 18 23 18 23 12"></polyline>
        </svg>
        <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <line x1="12" y1="5" x2="12" y2="19"></line>
          <polyline points="19 12 12 19 5 12"></polyline>
        </svg>
      </div>
      <span class="stat-card__trend-value">{{ Math.abs(trend) }}%</span>
    </div>

    <div class="stat-card__meta" v-if="$slots.meta">
      <slot name="meta" />
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseCard from './BaseCard.vue'

const props = defineProps({
  label: {
    type: String,
    default: ''
  },
  caption: {
    type: String,
    default: ''
  },
  trend: {
    type: Number as () => number | null,
    default: null
  },
  intent: {
    type: String as () => 'default' | 'success' | 'warning' | 'critical',
    default: 'default'
  },
  variant: {
    type: String as () => 'solid' | 'soft' | 'glass' | 'bordered',
    default: 'solid'
  },
  hoverable: {
    type: Boolean,
    default: true
  }
})

const trendClass = computed(() => {
  if (props.trend === null) return ''
  return props.trend > 0 ? 'stat-card--trend-up' : props.trend < 0 ? 'stat-card--trend-down' : ''
})

const trendIconClass = computed(() => {
  if (props.trend === null) return ''
  if (props.trend > 0) return 'stat-card__trend-icon--up'
  if (props.trend < 0) return 'stat-card__trend-icon--down'
  return 'stat-card__trend-icon--neutral'
})
</script>

<style scoped>
.stat-card {
  position: relative;
  display: grid;
  grid-template-columns: auto 1fr auto;
  gap: var(--space-16);
  align-items: center;
  padding: var(--space-20) var(--space-24);
  overflow: hidden;
}

.stat-card__accent {
  position: absolute;
  top: 0;
  right: 0;
  width: 120px;
  height: 120px;
  border-radius: 50%;
  opacity: 0.08;
  pointer-events: none;
  transition: transform 0.3s ease;
}

.stat-card:hover .stat-card__accent {
  transform: scale(1.1);
}

/* Intent-based accent colors */
.stat-card--default .stat-card__accent {
  background: var(--color-primary);
}

.stat-card--success .stat-card__accent {
  background: var(--color-success);
}

.stat-card--warning .stat-card__accent {
  background: var(--color-warning);
}

.stat-card--critical .stat-card__accent {
  background: var(--color-error);
}

.stat-card__icon {
  position: relative;
  z-index: 1;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-primary) 20%, transparent);
  color: var(--color-primary);
  font-size: var(--font-size-xl);
  transition: all 0.25s ease;
}

.stat-card:hover .stat-card__icon {
  transform: translateY(-2px);
  background: color-mix(in srgb, var(--color-primary) 28%, transparent);
}

.stat-card--success .stat-card__icon {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.stat-card--warning .stat-card__icon {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.stat-card--critical .stat-card__icon {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.stat-card__content {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.stat-card__label {
  margin: 0;
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--color-text-secondary);
  transition: color 0.25s ease;
}

.stat-card:hover .stat-card__label {
  color: var(--color-text-primary);
}

.stat-card__value {
  font-size: var(--font-size-3xl);
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: var(--font-lineheight-tight);
  transition: transform 0.25s ease;
}

.stat-card:hover .stat-card__value {
  transform: translateY(-1px);
}

.stat-card__caption {
  margin: 0;
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.stat-card__trend {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: var(--space-6);
  padding: var(--space-8) var(--space-12);
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-primary);
  transition: all 0.25s ease;
}

.stat-card--trend-up .stat-card__trend {
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  color: var(--color-success);
}

.stat-card--trend-down .stat-card__trend {
  background: color-mix(in srgb, var(--color-error) 12%, transparent);
  color: var(--color-error);
}

.stat-card:hover .stat-card__trend {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.stat-card__trend-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.stat-card__trend-icon svg {
  width: 100%;
  height: 100%;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.stat-card__trend-icon--up svg {
  animation: slideUp 0.6s ease-out;
}

.stat-card__trend-icon--down svg {
  animation: slideDown 0.6s ease-out;
}

.stat-card__trend-value {
  font-variant-numeric: tabular-nums;
}

.stat-card__meta {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  font-size: var(--font-size-xs);
  font-weight: 600;
  gap: var(--space-4);
  color: var(--color-text-secondary);
}

@keyframes slideUp {
  from {
    transform: translateY(4px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

@keyframes slideDown {
  from {
    transform: translateY(-4px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

@media (max-width: 640px) {
  .stat-card {
    grid-template-columns: 1fr;
    text-align: center;
  }

  .stat-card__icon {
    margin: 0 auto;
  }

  .stat-card__trend {
    justify-content: center;
    margin: 0 auto;
  }
}</style>
