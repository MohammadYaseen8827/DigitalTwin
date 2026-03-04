<script setup lang="ts">
import { useCssModule, computed } from 'vue'
import { ChevronDown } from 'lucide-vue-next'

interface Props {
  modelValue?: any
  label?: string
  placeholder?: string
  disabled?: boolean
  error?: string
  size?: 'sm' | 'md' | 'lg'
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md'
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: any): void
  (e: 'change', event: Event): void
}>()

const styles = useCssModule()

const onChange = (event: Event) => {
  const target = event.target as HTMLSelectElement
  emit('update:modelValue', target.value)
  emit('change', event)
}

const classes = computed(() => [
  styles['select-group'],
  styles[`select--${props.size}`],
  props.error && styles['select--error'],
  props.disabled && styles['select--disabled']
])
</script>

<template>
  <div :class="classes">
    <label v-if="label" :class="styles['label']">{{ label }}</label>
    <div :class="styles['wrapper']">
      <select
        :value="modelValue"
        :disabled="disabled"
        :class="styles['select']"
        @change="onChange"
      >
        <option v-if="placeholder" value="" disabled selected hidden>
          {{ placeholder }}
        </option>
        <slot />
      </select>
      <div :class="styles['icon']">
        <ChevronDown :width="14" :height="14" />
      </div>
    </div>
    <span v-if="error" :class="styles['error-text']">{{ error }}</span>
  </div>
</template>

<style module>
.select-group {
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
  transition: all var(--transition-fast);
  overflow: hidden;
  height: var(--row-height-base);
}

.wrapper:focus-within {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 2px var(--color-primary-muted);
}

.select {
  flex: 1;
  background: transparent;
  border: none;
  color: var(--color-text-primary);
  font-family: var(--font-family);
  font-size: var(--font-size-sm);
  outline: none;
  padding: var(--space-10) var(--space-32) var(--space-10) var(--space-12);
  appearance: none;
  cursor: pointer;
  width: 100%;
}

.select option {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.icon {
  position: absolute;
  right: var(--space-10);
  pointer-events: none;
  color: var(--color-text-muted);
  display: flex;
  align-items: center;
}

/* Sizes */
.select--sm .select {
  padding: var(--space-6) var(--space-28) var(--space-6) var(--space-10);
  font-size: var(--font-size-xs);
}

.select--lg .select {
  padding: var(--space-14) var(--space-40) var(--space-14) var(--space-16);
  font-size: var(--font-size-base);
}

/* States */
.select--error .wrapper {
  border-color: var(--color-danger);
}

.select--disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.select--disabled .select {
  cursor: not-allowed;
}
</style>
