<template>
  <component
    :is="as"
    class="base-button"
    :class="[variantClass, sizeClass, { 'is-disabled': disabled || loading, 'is-loading': loading }]"
    v-bind="componentAttrs"
    :aria-label="ariaLabel || undefined"
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
  ariaLabel: {
    type: String,
    default: ''
  },
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
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
  padding: var(--btn-padding-y) var(--btn-padding-x);
  border-radius: var(--btn-radius);
  font-size: var(--btn-font-size);
  font-weight: 600;
  border: 1px solid transparent;
  cursor: pointer;
  transition:
    background-color var(--transition-normal),
    color var(--transition-normal),
    border-color var(--transition-normal),
    transform var(--transition-fast),
    box-shadow var(--transition-normal);
  text-decoration: none;
  color: inherit;
  min-height: 44px;
  box-shadow: var(--shadow-card);
  background: var(--color-surface);
}

.base-button:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-elevated);
}

.base-button:focus-visible {
  outline: 3px solid var(--color-primary);
  outline-offset: 2px;
}

.base-button:active {
  transform: translateY(0);
  box-shadow: var(--shadow-card);
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
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
}

.variant-primary:hover {
  background: #2563eb;
}

.variant-secondary {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  border-color: var(--color-border);
}

.variant-secondary:hover {
  background: var(--color-surface);
  border-color: var(--color-primary);
}

.variant-outline {
  background: transparent;
  color: var(--color-primary);
  border-color: var(--color-primary);
  box-shadow: none;
}

.variant-outline:hover {
  background: rgba(59, 130, 246, 0.1);
}

.variant-ghost {
  background: transparent;
  color: var(--color-text-secondary);
  border-color: transparent;
  box-shadow: none;
}

.variant-ghost:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.variant-danger {
  background: var(--color-danger);
  color: white;
  border-color: var(--color-danger);
}

.variant-danger:hover {
  background: #dc2626;
}

.variant-warning {
  background: var(--color-warning);
  color: #1a1a1a;
  border-color: var(--color-warning);
}

.variant-warning:hover {
  background: #d97706;
}

.variant-critical {
  background: var(--color-danger);
  color: white;
  border-color: var(--color-danger);
}

.variant-critical:hover {
  background: #dc2626;
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
