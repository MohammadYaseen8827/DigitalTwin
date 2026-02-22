<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(defineProps<{
  loading?: boolean
  message?: string
  fullscreen?: boolean
}>(), {
  loading: true,
  message: 'Loading...',
  fullscreen: false
})

const spinnerSize = computed(() => 40)
</script>

<template>
  <Transition name="fade">
    <div v-if="loading" class="loading-overlay" :class="{ 'is-fullscreen': fullscreen }">
      <div class="loading-content">
        <div class="loading-spinner" :style="{ width: spinnerSize + 'px', height: spinnerSize + 'px' }"></div>
        <p v-if="message" class="loading-message">{{ message }}</p>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.loading-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, var(--color-surface) 85%, transparent);
  backdrop-filter: blur(2px);
  z-index: 10;
}

.loading-overlay.is-fullscreen {
  position: fixed;
  inset: 0;
  background: color-mix(in srgb, var(--color-surface) 90%, transparent);
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-16);
}

.loading-spinner {
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

.loading-message {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
