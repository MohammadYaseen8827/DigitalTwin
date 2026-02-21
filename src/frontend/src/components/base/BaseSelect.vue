<template>
  <div class="base-select" :class="[`size-${size}`, { 
    'has-error': error, 
    'has-warning': warning,
    'has-success': success,
    'is-focused': isFocused,
    'is-disabled': disabled 
  }]">
    <label v-if="label" :for="selectId" class="select-label">
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>
    
    <div class="select-wrapper">
      <span v-if="$slots.prefix" class="select-prefix">
        <slot name="prefix" />
      </span>
      
      <select
        :id="selectId"
        v-bind="componentAttrs"
        :disabled="disabled"
        :value="modelValue"
        @change="onChange"
        @blur="onBlur"
        @focus="onFocus"
        :aria-invalid="!!error"
        :aria-describedby="hintId"
        :class="{ 'has-value': modelValue !== '' && modelValue !== null }"
      >
        <option v-if="placeholder" value="" disabled selected hidden>
          {{ placeholder }}
        </option>
        <slot />
      </select>
      
      <span class="select-suffix">
        <slot name="suffix">
          <ChevronDown 
            class="select-chevron w-4 h-4" 
            :class="{ 'is-open': isFocused }" 
            aria-hidden="true" 
          />
        </slot>
      </span>
      
      <span v-if="showStatusIcon" class="select-status-icon" :class="statusIconClass" aria-hidden="true">
        <component :is="statusIcon" class="w-4 h-4" />
      </span>
    </div>
    
    <div v-if="hint || error" class="select-message" :id="hintId">
      <span v-if="error" class="select-error" role="alert">
        <AlertCircle class="w-3.5 h-3.5 mr-1" />
        {{ error }}
      </span>
      <span v-else-if="hint" class="select-hint">
        {{ hint }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, useAttrs, useSlots } from 'vue'
import { AlertCircle, CheckCircle, AlertTriangle, ChevronDown } from 'lucide-vue-next'

const props = defineProps({
  modelValue: {
    type: [String, Number, Array, Object],
    default: ''
  },
  label: {
    type: String,
    default: ''
  },
  placeholder: {
    type: String,
    default: ''
  },
  size: {
    type: String as () => 'sm' | 'md' | 'lg',
    default: 'md'
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
  warning: {
    type: String,
    default: ''
  },
  success: {
    type: Boolean,
    default: false
  },
  id: {
    type: String,
    default: ''
  }
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | number | Array<any> | Object): void
  (e: 'blur', event: FocusEvent): void
  (e: 'focus', event: FocusEvent): void
  (e: 'change', event: Event): void
}>()

const attrs = useAttrs()
const slots = useSlots()
const isFocused = ref(false)
const selectId = computed(() => props.id || (attrs.id as string) || `select-${Math.random().toString(36).slice(2)}`)
const hintId = computed(() => props.hint || props.error || props.warning ? `${selectId.value}-hint` : undefined)

// Status icon handling
const showStatusIcon = computed(() => {
  return props.error || props.warning || props.success
})

const statusIcon = computed(() => {
  if (props.error) return AlertCircle
  if (props.warning) return AlertTriangle
  if (props.success) return CheckCircle
  return null
})

const statusIconClass = computed(() => ({
  'status-error': props.error,
  'status-warning': props.warning,
  'status-success': props.success
}))

const componentAttrs = computed(() => {
  const { 
    id: _omittedId, 
    'aria-describedby': attrDescribedBy, 
    ...rest 
  } = attrs as Record<string, unknown>
  
  const tokens = [
    hintId.value, 
    typeof attrDescribedBy === 'string' ? attrDescribedBy : undefined
  ].filter((value): value is string => typeof value === 'string' && value.length > 0)

  const describedBy = tokens.join(' ').trim()

  return {
    ...rest,
    required: props.required,
    'aria-invalid': !!props.error,
    'aria-describedby': describedBy || undefined
  }
})

const onChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  const value = target.multiple 
    ? Array.from(target.selectedOptions).map(option => option.value)
    : target.value
  
  emit('update:modelValue', value)
  emit('change', event)
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
.base-select {
  --select-bg: var(--color-surface);
  --select-border: var(--color-border);
  --select-text: var(--color-text-primary);
  --select-placeholder: var(--color-text-tertiary);
  --select-focus-ring: color-mix(in srgb, var(--color-primary) 50%, transparent);
  --select-hover-border: var(--color-border-hover);
  --select-disabled-bg: var(--color-bg-subtle);
  --select-disabled-border: var(--color-border-subtle);
  --select-disabled-text: var(--color-text-disabled);
  --select-error: var(--color-error);
  --select-warning: var(--color-warning);
  --select-success: var(--color-success);
  --select-transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
  width: 100%;
}

.select-label {
  display: inline-flex;
  align-items: center;
  gap: var(--space-4);
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-secondary);
  margin: 0;
  line-height: 1.25;
}

.required-indicator {
  color: var(--color-error);
  font-weight: 600;
}

.select-wrapper {
  position: relative;
  display: inline-flex;
  align-items: center;
  width: 100%;
  min-height: 2.5rem;
  border: 1px solid var(--select-border);
  border-radius: var(--radius-md);
  background: var(--select-bg);
  transition: var(--select-transition);
  box-shadow: var(--shadow-sm);
}

.select-wrapper:focus-within {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--select-focus-ring);
  outline: none;
}

.select-wrapper:hover:not(:focus-within) {
  border-color: var(--select-hover-border);
}

/* Status states */
.base-select.has-error .select-wrapper {
  border-color: color-mix(in srgb, var(--select-error) 70%, var(--select-border) 30%);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--select-error) 15%, transparent);
}

.base-select.has-warning .select-wrapper {
  border-color: color-mix(in srgb, var(--select-warning) 60%, var(--select-border) 40%);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--select-warning) 10%, transparent);
}

.base-select.has-success .select-wrapper {
  border-color: color-mix(in srgb, var(--select-success) 60%, var(--select-border) 40%);
}

.base-select.is-disabled .select-wrapper {
  background-color: var(--select-disabled-bg);
  border-color: var(--select-disabled-border);
  cursor: not-allowed;
  opacity: 0.7;
}

/* Select element */
select {
  flex: 1;
  width: 100%;
  height: 100%;
  min-height: 100%;
  padding: 0.5rem 2.5rem 0.5rem 0.75rem;
  margin: 0;
  border: none;
  background: transparent;
  color: var(--select-text);
  font-size: var(--font-size-base);
  font-family: inherit;
  line-height: 1.5;
  appearance: none;
  outline: none;
  cursor: pointer;
}

select:disabled {
  color: var(--select-disabled-text);
  cursor: not-allowed;
  opacity: 1; /* Fix for iOS */
  -webkit-text-fill-color: var(--select-disabled-text);
}

select:not([multiple]) {
  padding-right: 2.5rem;
  background-image: none; /* Remove default arrow in Firefox */
}

/* Hide default arrow in IE10+ */
select::-ms-expand {
  display: none;
}

/* Placeholder styling */
select:invalid:not(:focus):not(:disabled) {
  color: var(--select-placeholder);
}

/* Prefix and suffix */
.select-prefix {
  display: inline-flex;
  align-items: center;
  padding-left: 0.75rem;
  color: var(--color-text-tertiary);
}

.select-suffix {
  display: inline-flex;
  align-items: center;
  padding: 0 0.75rem;
  color: var(--color-text-tertiary);
  pointer-events: none;
}

/* Status icon */
.select-status-icon {
  position: absolute;
  right: 2.5rem;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: none;
}

.status-error {
  color: var(--select-error);
}

.status-warning {
  color: var(--select-warning);
}

.status-success {
  color: var(--select-success);
}

/* Chevron icon */
.select-chevron {
  transition: transform 0.2s ease;
  color: var(--color-text-tertiary);
}

.select-chevron.is-open {
  transform: rotate(180deg);
}

/* Sizes */
.base-select.size-sm .select-wrapper {
  min-height: 2rem;
  border-radius: var(--radius-sm);
}

.base-select.size-sm select {
  font-size: var(--font-size-sm);
  padding: 0.25rem 2rem 0.25rem 0.5rem;
}

.base-select.size-lg .select-wrapper {
  min-height: 3rem;
  border-radius: var(--radius-lg);
}

.base-select.size-lg select {
  font-size: var(--font-size-lg);
  padding: 0.75rem 3rem 0.75rem 1rem;
}

/* Message container */
.select-message {
  min-height: 1.25rem;
  margin-top: 0.25rem;
  font-size: var(--font-size-xs);
  line-height: 1.25;
}

.select-hint {
  color: var(--color-text-tertiary);
  margin: 0;
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}

.select-error {
  color: var(--select-error);
  margin: 0;
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
}

/* Dark mode */
@media (prefers-color-scheme: dark) {
  .base-select {
    --select-bg: color-mix(in srgb, var(--color-surface) 95%, var(--color-bg));
    --select-border: var(--color-border-subtle);
    --select-hover-border: var(--color-border-hover);
  }
  
  select::placeholder {
    opacity: 0.7;
  }
}

/* Focus styles for keyboard navigation */
select:focus-visible {
  outline: none;
}

/* Custom scrollbar for dropdown (works in WebKit browsers) */
select::-webkit-scrollbar {
  width: 8px;
}

select::-webkit-scrollbar-track {
  background: var(--color-bg-subtle);
  border-radius: 4px;
}

select::-webkit-scrollbar-thumb {
  background: var(--color-border);
  border-radius: 4px;
}

select::-webkit-scrollbar-thumb:hover {
  background: var(--color-border-hover);
}
</style>