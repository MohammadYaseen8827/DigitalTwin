<script setup lang="ts">
import { useCssModule } from 'vue'

interface Props {
  variant?: 'default' | 'success' | 'warning' | 'danger' | 'info' | 'primary'
  size?: 'sm' | 'md'
  outline?: boolean
  dot?: boolean
}

withDefaults(defineProps<Props>(), {
  variant: 'default',
  size: 'md',
  outline: false,
  dot: false
})

const styles = useCssModule()
</script>

<template>
  <span :class="[
    styles['badge'],
    styles[`badge--${variant}`],
    styles[`badge--${size}`],
    outline && styles['badge--outline']
  ]">
    <span v-if="dot" :class="styles['dot']" />
    <slot />
  </span>
</template>

<style module>
.badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-6);
  font-weight: 700;
  border-radius: var(--radius-full);
  line-height: 1;
  white-space: nowrap;
  transition: all var(--transition-fast);
}

/* Sizes */
.badge--sm {
  padding: 3px 8px;
  font-size: var(--font-size-2xs);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}
.badge--md {
  padding: 5px 12px;
  font-size: var(--font-size-xs);
}

/* Variants */
.badge--default { 
  background: var(--color-surface-elevated); 
  color: var(--color-text-secondary);
  border: 1px solid var(--color-border);
}
.badge--primary { 
  background: var(--color-primary-muted); 
  color: var(--color-primary); 
  box-shadow: 0 0 10px rgba(var(--color-primary-rgb), 0.1);
}
.badge--success { 
  background: var(--color-success-muted); 
  color: var(--color-success); 
  box-shadow: 0 0 10px rgba(16, 185, 129, 0.1);
}
.badge--warning { 
  background: var(--color-warning-muted); 
  color: var(--color-warning); 
  box-shadow: 0 0 10px rgba(245, 158, 11, 0.1);
}
.badge--danger { 
  background: var(--color-danger-muted); 
  color: var(--color-danger); 
  box-shadow: 0 0 10px rgba(239, 68, 68, 0.1);
}
.badge--info { 
  background: var(--color-primary-muted); 
  color: var(--color-primary); 
  box-shadow: 0 0 10px rgba(var(--color-primary-rgb), 0.1);
}

.badge--outline {
  background: transparent;
  border: 1px solid currentColor;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
  box-shadow: 0 0 8px currentColor;
}
</style>
