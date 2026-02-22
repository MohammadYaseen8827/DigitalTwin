<template>
  <div class="base-input" :class="[`size-${size}`, { 'has-error': error, 'is-focused': isFocused, 'is-disabled': disabled }]">
    <label v-if="label" :for="inputId" class="input-label">
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>
    <div class="input-wrapper">
      <span v-if="$slots.prefix" class="input-prefix">
        <slot name="prefix" />
      </span>
      <input
        :id="inputId"
        v-bind="componentAttrs"
        :type="type"
        :disabled="disabled"
        :placeholder="placeholder"
        :value="modelValue"
        @input="onInput"
        @blur="onBlur"
        @focus="onFocus"
        :aria-invalid="!!error"
        :aria-describedby="hintId"
      />
      <span v-if="$slots.suffix" class="input-suffix">
        <slot name="suffix" />
      </span>
      <span v-if="showStatusIcon" class="input-status-icon" :class="statusIconClass" aria-hidden="true">
        <component :is="statusIcon" class="w-4 h-4" />
      </span>
    </div>
    <p v-if="hint && !error" :id="hintId" class="input-hint">{{ hint }}</p>
    <p v-if="error" :id="hintId" class="input-error" role="alert">
      <AlertTriangle class="w-4 h-4 mr-1" />
      <span class="error-message">{{ error }}</span>
    </p>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, useAttrs } from 'vue'
import Check from '~icons/lucide/check'
import X from '~icons/lucide/x'
import AlertTriangle from '~icons/lucide/alert-triangle'
import Info from '~icons/lucide/info'

const props = defineProps({
  modelValue: {
    type: [String, Number],
    default: ''
  },
  label: {
    type: String,
    default: ''
  },
  size: {
    type: String as () => 'sm' | 'md' | 'lg',
    default: 'md'
  },
  type: {
    type: String,
    default: 'text'
  },
  placeholder: {
    type: String,
    default: ''
  },
  disabled: {
    type: Boolean,
    default: false
  },
  required: {
    type: Boolean,
    default: false
  },
  hint: {
    type: String,
    default: ''
  },
  error: {
    type: String,
    default: ''
  },
  success: {
    type: Boolean,
    default: false
  },
  warning: {
    type: Boolean,
    default: false
  },
  id: {
    type: String,
    default: ''
  },
  showStatus: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | number): void
  (e: 'blur', event: FocusEvent): void
  (e: 'focus', event: FocusEvent): void
}>()

const isFocused = ref(false)
const inputId = computed(() => props.id || `input-${Math.random().toString(36).slice(2)}`)
const hintId = computed(() => `${inputId.value}-hint`)

const attrs = useAttrs()

const componentAttrs = computed(() => ({
  ...attrs,
  disabled: props.disabled || attrs.disabled
}))

const hasValue = computed(() => {
  return props.modelValue !== null && props.modelValue !== undefined && props.modelValue !== ''
})

const showStatusIcon = computed(() => {
  return props.showStatus && (props.error || props.success || (props.warning && !props.error))
})

const statusIcon = computed(() => {
  if (props.error) return X
  if (props.success) return Check
  if (props.warning) return AlertTriangle
  return Info
})

const statusIconClass = computed(() => {
  if (props.error) return 'error'
  if (props.success) return 'success'
  if (props.warning) return 'warning'
  return 'info'
})

const onInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', props.type === 'number' ? Number(target.value) : target.value)
}

const onFocus = (event: FocusEvent) => {
  isFocused.value = true
  emit('focus', event)
}

const onBlur = (event: FocusEvent) => {
  isFocused.value = false
  emit('blur', event)
}
</script>

<style scoped>
.base-input {
  --input-border: var(--color-border);
  --input-bg: var(--color-surface);
  --input-text: var(--color-text-primary);
  --input-placeholder: var(--color-text-muted);
  --input-focus-ring: 0 0 0 3px rgba(59, 130, 246, 0.3);
  --input-focus-border: var(--color-primary);
  --input-hover-border: var(--color-primary);
  --input-disabled-bg: var(--color-surface-elevated);
  --input-disabled-text: var(--color-text-muted);
  --input-disabled-border: var(--color-border-subtle);
  --input-error: var(--color-danger);
  --input-warning: var(--color-warning);
  --input-success: var(--color-success);
  --input-info: var(--color-info);
  --input-transition: all var(--transition-fast) ease;

  display: flex;
  flex-direction: column;
  gap: var(--space-6);
  width: 100%;
}

.input-label {
  display: inline-flex;
  align-items: center;
  gap: var(--space-4);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-secondary);
  transition: color var(--transition-fast);
}

.required-indicator {
  color: var(--color-danger);
  margin-left: 2px;
}

.input-wrapper {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: var(--space-8);
  border: 1px solid var(--input-border);
  border-radius: var(--radius-md);
  background: var(--input-bg);
  transition: var(--input-transition);
  width: 100%;
}

.input-wrapper:hover:not(.is-disabled) {
  border-color: var(--input-hover-border);
}

.input-wrapper:focus-within {
  border-color: var(--input-focus-border);
  box-shadow: var(--input-focus-ring);
}

.input-prefix,
.input-suffix {
  display: inline-flex;
  align-items: center;
  color: var(--color-text-secondary);
  transition: color var(--transition-fast);
}

.input-prefix {
  padding-left: var(--space-4);
}

.input-suffix {
  padding-right: var(--space-4);
}

input {
  flex: 1;
  border: none;
  background: transparent;
  color: var(--input-text);
  font-size: var(--font-size-base);
  line-height: 1.5;
  padding: 0;
  outline: none;
  min-width: 0;
  height: 100%;
}

input::placeholder {
  color: var(--input-placeholder);
  opacity: 1;
  transition: color var(--transition-fast);
}

/* Sizes */
.size-sm .input-wrapper {
  padding: var(--space-6) var(--space-10);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
}

.size-md .input-wrapper {
  padding: var(--space-10) var(--space-12);
  border-radius: var(--radius-md);
}

.size-lg .input-wrapper {
  padding: var(--space-12) var(--space-16);
  border-radius: var(--radius-lg);
  font-size: var(--font-size-lg);
}

/* Status Icons */
.input-status-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-left: auto;
  transition: all var(--transition-fast);
}

.input-status-icon.error {
  color: var(--input-error);
}

.input-status-icon.warning {
  color: var(--input-warning);
}

.input-status-icon.success {
  color: var(--input-success);
}

.input-status-icon.info {
  color: var(--input-info);
}

/* Hints & Errors */
.input-hint {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.4;
}

.input-error {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  font-size: var(--font-size-xs);
  color: var(--input-error);
  margin: 0;
  line-height: 1.4;
}

.error-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--input-error);
  color: white;
  font-size: 10px;
  font-weight: bold;
  flex-shrink: 0;
}

/* States */
.has-error .input-wrapper {
  border-color: var(--input-error);
}

.has-error .input-wrapper:focus-within {
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.3);
}

.is-disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.is-disabled .input-wrapper {
  background: var(--input-disabled-bg);
  border-color: var(--input-disabled-border);
}

.is-disabled input {
  color: var(--input-disabled-text);
  cursor: not-allowed;
}

/* Focus state for keyboard navigation */
:focus-visible {
  outline: none;
}

/* Animation for focus state */
@keyframes pulse {
  0% {
    box-shadow: 0 0 0 0 rgba(59, 130, 246, 0.5);
  }
  70% {
    box-shadow: 0 0 0 4px rgba(59, 130, 246, 0);
  }
  100% {
    box-shadow: 0 0 0 0 rgba(59, 130, 246, 0);
  }
}

.input-wrapper:focus-within {
  animation: pulse 1.5s infinite;
}

/* Disable animation when reduced motion is preferred */
@media (prefers-reduced-motion: reduce) {
  .input-wrapper:focus-within {
    animation: none;
  }
}
</style>
