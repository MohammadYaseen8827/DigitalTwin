<script setup lang="ts">
import { useCssModule, computed } from 'vue'

interface Props {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'success' | 'warning' | 'info'
  size?: 'sm' | 'md' | 'lg'
  disabled?: boolean
  loading?: boolean
  type?: 'button' | 'submit' | 'reset'
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'primary',
  size: 'md',
  type: 'button'
})

const styles = useCssModule()

const classes = computed(() => [
  styles['btn'],
  styles[`btn--${props.variant}`],
  styles[`btn--${props.size}`],
  props.loading && styles['btn--loading']
])
</script>

<template>
  <button
    :type="type"
    :class="classes"
    :disabled="disabled || loading"
  >
    <span v-if="loading" :class="styles['loader']" />
    <span :class="[styles['content'], loading && styles['content--hidden']]">
      <slot />
    </span>
  </button>
</template>

<style module>
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
  font-family: var(--font-family);
  font-weight: 600;
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all var(--transition-normal);
  border: 1px solid transparent;
  outline: none;
  white-space: nowrap;
  user-select: none;
  position: relative;
  overflow: hidden;
}

.btn::before {
  content: '';
  position: absolute;
  inset: 0;
  background: white;
  opacity: 0;
  transition: opacity var(--transition-fast);
}

.btn:hover:not(:disabled)::before {
  opacity: 0.1;
}

.btn:active:not(:disabled) {
  transform: scale(0.96);
}

/* Variants */
.btn--primary {
  background: var(--color-primary);
  color: white;
  box-shadow: var(--glow-sm);
  border-color: rgba(255, 255, 255, 0.1);
}
.btn--primary:hover:not(:disabled) {
  background: var(--color-primary-hover);
  box-shadow: var(--glow-md);
}

.btn--secondary {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  border-color: var(--color-border);
}
.btn--secondary:hover:not(:disabled) {
  background: var(--color-surface-alt);
  border-color: var(--color-border-hover);
}

.btn--outline {
  background: transparent;
  border-color: var(--color-border);
  color: var(--color-text-primary);
}
.btn--outline:hover:not(:disabled) {
  background: var(--color-surface-alt);
  border-color: var(--color-border-hover);
}

.btn--ghost {
  background: transparent;
  color: var(--color-text-secondary);
}
.btn--ghost:hover:not(:disabled) {
  background: var(--color-surface-alt);
  color: var(--color-text-primary);
}

.btn--danger {
  background: var(--color-danger-muted);
  color: var(--color-danger);
  border-color: rgba(239, 68, 68, 0.2);
}
.btn--danger:hover:not(:disabled) {
  background: var(--color-danger);
  color: white;
}

.btn--success {
  background: var(--color-success-muted);
  color: var(--color-success);
  border-color: rgba(16, 185, 129, 0.2);
}
.btn--success:hover:not(:disabled) {
  background: var(--color-success);
  color: white;
}

.btn--warning {
  background: var(--color-warning-muted);
  color: var(--color-warning);
  border-color: rgba(245, 158, 11, 0.2);
}
.btn--warning:hover:not(:disabled) {
  background: var(--color-warning);
  color: white;
}

.btn--info {
  background: var(--color-primary-muted);
  color: var(--color-primary);
  border-color: rgba(59, 130, 246, 0.2);
}
.btn--info:hover:not(:disabled) {
  background: var(--color-primary);
  color: white;
}

/* Sizes */
.btn--sm {
  height: 32px;
  padding: 0 var(--space-12);
  font-size: var(--font-size-xs);
}
.btn--md {
  height: 40px;
  padding: 0 var(--space-16);
  font-size: var(--font-size-sm);
}
.btn--lg {
  height: 48px;
  padding: 0 var(--space-24);
  font-size: var(--font-size-base);
}

/* Loader */
.loader {
  position: absolute;
  width: 16px;
  height: 16px;
  border: 2px solid currentColor;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.content--hidden {
  opacity: 0;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
