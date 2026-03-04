<script setup lang="ts">
import { useCssModule, computed, useAttrs } from 'vue'

interface Props {
  modelValue?: string | number
  label?: string
  placeholder?: string
  disabled?: boolean
  error?: string
  size?: 'sm' | 'md' | 'lg'
  type?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: '',
  size: 'md',
  type: 'text'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: string | number): void
  (e: 'blur', event: FocusEvent): void
  (e: 'focus', event: FocusEvent): void
}>()

const styles = useCssModule()
const attrs = useAttrs()

const onInput = (event: Event) => {
  const target = event.target as HTMLInputElement
  emit('update:modelValue', props.type === 'number' ? Number(target.value) : target.value)
}

const classes = computed(() => [
  styles['input-group'],
  styles[`input--${props.size}`],
  props.error && styles['input--error'],
  props.disabled && styles['input--disabled']
])
</script>

<template>
  <div :class="classes">
    <label v-if="label" :class="styles['label']">{{ label }}</label>
    <div :class="styles['wrapper']">
      <div v-if="$slots.prefix" :class="styles['prefix']">
        <slot name="prefix" />
      </div>
      <input
        v-bind="attrs"
        :type="type"
        :value="modelValue"
        :placeholder="placeholder"
        :disabled="disabled"
        :class="styles['input']"
        @input="onInput"
        @blur="emit('blur', $event)"
        @focus="emit('focus', $event)"
      />
      <div v-if="$slots.suffix" :class="styles['suffix']">
        <slot name="suffix" />
      </div>
    </div>
    <span v-if="error" :class="styles['error-text']">{{ error }}</span>
  </div>
</template>

<style module>
.input-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
  width: 100%;
}

.label {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.wrapper {
  position: relative;
  display: flex;
  align-items: center;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  transition: all var(--transition-normal);
  overflow: hidden;
  height: var(--row-height-base);
}

.wrapper::after {
  content: '';
  position: absolute;
  inset: 0;
  border: 1px solid var(--color-primary);
  border-radius: inherit;
  opacity: 0;
  transition: opacity var(--transition-normal);
  pointer-events: none;
}

.wrapper:focus-within {
  border-color: var(--color-primary-hover);
  box-shadow: 0 0 0 3px var(--color-primary-muted);
}

.wrapper:focus-within::after {
  opacity: 0.1;
}

.input {
  flex: 1;
  background: transparent;
  border: none;
  color: var(--color-text-primary);
  font-family: var(--font-family);
  font-size: var(--font-size-sm);
  outline: none;
  padding: var(--space-10) var(--space-12);
  transition: transform var(--transition-fast);
}

.input:focus {
  transform: translateX(2px);
}

.input::placeholder {
  color: var(--color-text-dim);
}

.prefix, .suffix {
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  padding: 0 var(--space-8);
}

/* Sizes */
.input--sm .input {
  padding: var(--space-6) var(--space-10);
  font-size: var(--font-size-xs);
}

.input--lg .input {
  padding: var(--space-14) var(--space-16);
  font-size: var(--font-size-base);
}

/* States */
.input--error .wrapper {
  border-color: var(--color-danger);
}

.input--error .error-text {
  font-size: 11px;
  color: var(--color-danger);
}

.input--disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.input--disabled .input {
  cursor: not-allowed;
}
</style>
