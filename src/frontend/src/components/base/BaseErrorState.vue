<script setup lang="ts">
import { computed } from 'vue'
import { AlertCircle, RefreshCw } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  title?: string
  description?: string
  error?: string | Error
  retryLabel?: string
  size?: 'sm' | 'md' | 'lg'
}>(), {
  title: 'Something went wrong',
  description: 'An error occurred while loading this content.',
  retryLabel: 'Try Again',
  size: 'md'
})

const emit = defineEmits<{
  (e: 'retry'): void
}>()

const errorMessage = computed(() => {
  if (!props.error) return ''
  return props.error instanceof Error ? props.error.message : props.error
})
</script>

<template>
  <div class="error-state" :class="`error-${size}`">
    <div class="error-icon">
      <AlertCircle :size="size === 'sm' ? 24 : size === 'lg' ? 48 : 32" />
    </div>
    <h3 class="error-title">{{ title }}</h3>
    <p v-if="description || errorMessage" class="error-description">
      {{ errorMessage || description }}
    </p>
    <button 
      v-if="retryLabel" 
      class="error-action"
      @click="emit('retry')"
    >
      <RefreshCw :size="16" />
      {{ retryLabel }}
    </button>
    <slot />
  </div>
</template>

<style scoped>
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: var(--space-32);
  min-height: 200px;
}

.error-sm { min-height: 120px; padding: var(--space-16); }
.error-lg { min-height: 320px; padding: var(--space-48); }

.error-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--color-error) 15%, transparent);
  color: var(--color-error);
  margin-bottom: var(--space-16);
}

.error-sm .error-icon {
  width: 40px;
  height: 40px;
  margin-bottom: var(--space-8);
}

.error-lg .error-icon {
  width: 80px;
  height: 80px;
  margin-bottom: var(--space-20);
}

.error-title {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-8);
}

.error-sm .error-title {
  font-size: var(--font-size-base);
}

.error-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0 0 var(--space-20);
  max-width: 400px;
}

.error-sm .error-description {
  font-size: var(--font-size-xs);
  margin-bottom: var(--space-12);
}

.error-action {
  display: inline-flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-10) var(--space-20);
  background: var(--color-surface-alt);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.error-action:hover {
  background: var(--color-surface);
  border-color: var(--color-primary);
  color: var(--color-primary);
}
</style>
