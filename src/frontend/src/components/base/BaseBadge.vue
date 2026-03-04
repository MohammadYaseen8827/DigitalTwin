<script setup lang="ts">
import { computed, useCssModule } from 'vue'

type BadgeVariant = 'default' | 'success' | 'warning' | 'error' | 'info' | 'gray' | 'primary' | 'danger'
type BadgeSize = 'xs' | 'sm' | 'md' | 'lg'

const props = withDefaults(defineProps<{
  variant?: BadgeVariant
  size?: BadgeSize
  color?: string
}>(), {
  variant: 'default',
  size: 'sm'
})

const styles = useCssModule()

const classes = computed(() => [
  styles['badge'],
  styles[`badge--${props.variant}`],
  styles[`badge--${props.size}`],
])
</script>

<template>
  <span :class="classes" :style="color ? { backgroundColor: color, color: 'white', borderColor: 'transparent' } : undefined">
    <slot />
  </span>
</template>

<style module>
.badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-full);
  font-weight: 600;
  letter-spacing: 0.04em;
  white-space: nowrap;
  border: 1px solid transparent;
}

/* Sizes */
.badge--xs { font-size: 10px; padding: 1px 6px; }
.badge--sm { font-size: var(--font-size-xs); padding: 2px 8px; }
.badge--md { font-size: var(--font-size-sm); padding: var(--space-4) var(--space-10); }
.badge--lg { font-size: var(--font-size-base); padding: var(--space-6) var(--space-12); }

/* Variants */
.badge--default, .badge--primary {
  background: var(--color-primary-muted);
  color: var(--color-primary);
  border-color: rgba(59, 130, 246, 0.2);
}

.badge--success {
  background: var(--color-success-muted);
  color: var(--color-success);
  border-color: rgba(34, 197, 94, 0.2);
}

.badge--warning {
  background: var(--color-warning-muted);
  color: var(--color-warning);
  border-color: rgba(245, 158, 11, 0.2);
}

.badge--error, .badge--danger {
  background: var(--color-danger-muted);
  color: var(--color-danger);
  border-color: rgba(239, 68, 68, 0.2);
}

.badge--info {
  background: var(--color-info-muted);
  color: var(--color-telemetry);
  border-color: rgba(56, 189, 248, 0.2);
}

.badge--gray {
  background: rgba(71, 85, 105, 0.15);
  color: var(--color-text-secondary);
  border-color: var(--color-border-subtle);
}
</style>
