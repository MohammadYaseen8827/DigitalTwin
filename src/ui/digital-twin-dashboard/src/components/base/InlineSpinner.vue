<template>
  <span
    class="inline-spinner"
    role="status"
    aria-live="polite"
  >
    <span class="inline-spinner__icon" aria-hidden="true"></span>
    <slot>Loading…</slot>
  </span>
</template>

<script setup lang="ts">
</script>

<style scoped>
.inline-spinner {
  display: inline-flex;
  align-items: center;
  gap: var(--space-8);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.inline-spinner__icon {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid color-mix(in srgb, var(--color-primary) 20%, var(--color-border) 80%);
  border-top-color: var(--color-primary);
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
