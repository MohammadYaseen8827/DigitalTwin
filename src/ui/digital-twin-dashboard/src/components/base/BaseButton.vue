<template>
  <component
    :is="as"
    class="base-button"
    :class="[variantClass, sizeClass, { 'is-disabled': disabled || loading, 'is-loading': loading }]"
    v-bind="componentAttrs"
    @click="handleClick"
  >
    <span v-if="loading" class="loading-spinner"></span>
    <span class="button-content" :class="{ 'loading-content': loading }">
      <slot />
    </span>
  </component>
</template>

<script setup lang="ts">
import { computed, useAttrs, type Component, type PropType } from 'vue'

const props = defineProps({
  variant: {
    type: String as () => 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger' | 'warning' | 'critical',
    default: 'primary'
  },
  size: {
    type: String as () => 'sm' | 'md' | 'lg',
    default: 'md'
  },
  as: {
    type: [String, Object] as PropType<string | Component>,
    default: 'button'
  },
  disabled: {
    type: Boolean,
    default: false
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits<{ (e: 'click', event: MouseEvent): void }>()

const attrs = useAttrs()

const variantClass = computed(() => `variant-${props.variant}`)
const sizeClass = computed(() => `size-${props.size}`)

const componentAttrs = computed(() => ({
  ...attrs,
  type: attrs.type ?? (props.as === 'button' ? 'button' : undefined),
  disabled: props.disabled || props.loading || attrs.disabled
}))

const handleClick = (event: MouseEvent) => {
  if (props.disabled) {
    event.preventDefault()
    event.stopPropagation()
    return
  }
  emit('click', event)
}
</script>

<style scoped>
.base-button {
  --btn-padding-y: var(--space-12);
  --btn-padding-x: var(--space-20);
  --btn-radius: var(--radius-md);
  --btn-font-size: var(--font-size-base);
  --btn-line-height: var(--font-lineheight-snug);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
  padding: var(--btn-padding-y) var(--btn-padding-x);
  border-radius: var(--btn-radius);
  font-size: var(--btn-font-size);
  line-height: var(--btn-line-height);
  font-weight: 600;
  border: 1px solid transparent;
  cursor: pointer;
  transition:
    background-color 0.25s ease,
    color 0.25s ease,
    border-color 0.25s ease,
    transform 0.2s ease,
    box-shadow 0.2s ease;
  text-decoration: none;
  color: inherit;
  min-height: 44px;
  box-shadow: var(--shadow-subtle);
  background: var(--color-surface-alt);
}

.base-button:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-medium);
}

.base-button:focus-visible {
  outline: 3px solid color-mix(in srgb, var(--color-primary) 45%, transparent);
  outline-offset: 2px;
}

.base-button:active {
  transform: translateY(0);
  box-shadow: var(--shadow-subtle);
}

.base-button.is-disabled,
.base-button:disabled {
  cursor: not-allowed;
  opacity: 0.55;
  box-shadow: none;
}

.base-button.is-loading {
  cursor: wait;
  pointer-events: none;
}

.loading-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid currentColor;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.loading-content {
  opacity: 0.7;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.variant-primary {
  background: linear-gradient(135deg, var(--color-primary), color-mix(in srgb, var(--color-primary-dark) 80%, var(--color-primary) 20%));
  color: var(--color-text-primary);
  border-color: color-mix(in srgb, var(--color-primary-dark) 60%, var(--color-primary) 40%);
}

.variant-primary:hover {
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-primary) 60%, var(--color-primary-dark) 40%), var(--color-primary-dark));
}

.variant-secondary {
  background: color-mix(in srgb, var(--color-surface-alt) 60%, var(--color-primary) 40%);
  color: var(--color-text-primary);
  border-color: color-mix(in srgb, var(--color-primary) 45%, var(--color-border) 55%);
}

.variant-secondary:hover {
  background: color-mix(in srgb, var(--color-primary) 50%, var(--color-surface-alt) 50%);
}

.variant-outline {
  background: transparent;
  color: var(--color-primary);
  border-color: color-mix(in srgb, var(--color-primary) 60%, var(--color-border) 40%);
  box-shadow: none;
}

.variant-outline:hover {
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  box-shadow: var(--shadow-subtle);
}

.variant-ghost {
  background: transparent;
  color: var(--color-text-secondary);
  border-color: transparent;
  box-shadow: none;
}

.variant-ghost:hover {
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
  color: var(--color-text-primary);
}

.variant-danger {
  background: linear-gradient(135deg, var(--color-error), color-mix(in srgb, var(--color-error) 60%, #8b1d1d 40%));
  color: var(--color-text-primary);
  border-color: color-mix(in srgb, var(--color-error) 70%, #8b1d1d 30%);
}

.variant-danger:hover {
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-error) 60%, #8b1d1d 40%), #8b1d1d);
}

.variant-warning {
  background: linear-gradient(135deg, var(--color-warning-500, #f59e0b), color-mix(in srgb, var(--color-warning-600, #d97706) 70%, var(--color-warning-500, #f59e0b) 30%));
  color: var(--color-text-primary);
  border-color: color-mix(in srgb, var(--color-warning-600, #d97706) 60%, var(--color-warning-500, #f59e0b) 40%);
}

.variant-warning:hover {
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-warning-600, #d97706) 60%, var(--color-warning-500, #f59e0b) 40%), var(--color-warning-700, #b45309));
}

.variant-critical,
.variant-danger {
  color: var(--color-text-primary);
}

.variant-critical {
  background: linear-gradient(135deg, var(--color-critical-500, #ef4444), color-mix(in srgb, var(--color-critical-600, #dc2626) 70%, var(--color-critical-500, #ef4444) 30%));
  border-color: color-mix(in srgb, var(--color-critical-600, #dc2626) 60%, var(--color-critical-500, #ef4444) 40%);
}

.variant-critical:hover {
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-critical-600, #dc2626) 60%, var(--color-critical-500, #ef4444) 40%), var(--color-critical-700, #b91c1c));
}

.size-sm {
  --btn-padding-y: var(--space-8);
  --btn-padding-x: var(--space-16);
  --btn-radius: var(--radius-sm);
  --btn-font-size: var(--font-size-sm);
  min-height: 36px;
}

.size-lg {
  --btn-padding-y: var(--space-16);
  --btn-padding-x: var(--space-24);
  --btn-radius: var(--radius-lg);
  --btn-font-size: var(--font-size-lg);
  min-height: 48px;
}

.button-content {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
}
</style>
