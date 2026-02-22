<template>
  <section :class="cardClasses">
    <header v-if="$slots.header" class="card-header">
      <slot name="header" />
    </header>
    <div class="card-body">
      <slot />
    </div>
    <div v-if="loading" class="card-overlay">
      <slot name="loading">
        <div class="card-loading">
          <div class="spinner"></div>
          <span class="loading-text">Loading…</span>
        </div>
      </slot>
    </div>
    <footer v-if="$slots.footer" class="card-footer">
      <slot name="footer" />
    </footer>
    <div v-if="status" :class="['card-status', `card-status--${status}`]">
      <slot name="status">
        <div class="card-status__icon"></div>
        <span class="card-status__text">{{ status }}</span>
      </slot>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps({
  variant: {
    type: String as () => 'solid' | 'soft' | 'glass' | 'bordered' | 'premium',
    default: 'soft'
  },
  hoverable: {
    type: Boolean,
    default: false
  },
  loading: {
    type: Boolean,
    default: false
  },
  status: {
    type: String as () => 'value' | 'zero' | 'not_available' | 'not_applicable' | 'missing' | '',
    default: ''
  }
})

const variantClass = computed(() => `variant-${props.variant}`)
const cardClasses = computed(() => [
  'base-card',
  variantClass.value,
  { 'has-hover': props.hoverable }
])
</script>

<style scoped>
.base-card {
  position: relative;
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  padding: var(--space-24);
  border: 1px solid var(--color-border-subtle);
  box-shadow: var(--shadow-card);
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  transition:
    transform var(--transition-normal),
    box-shadow var(--transition-normal),
    border-color var(--transition-normal),
    background var(--transition-normal);
}

.base-card.has-hover:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-elevated);
  border-color: var(--color-border);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-12);
  padding-bottom: var(--space-12);
  border-bottom: 1px solid var(--color-border-subtle);
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--color-text-primary);
}

.card-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  color: var(--color-text-secondary);
  font-size: var(--font-size-base);
  line-height: 1.5;
}

.card-overlay {
  position: absolute;
  inset: 0;
  background: var(--color-surface);
  opacity: 0.85;
  backdrop-filter: blur(8px);
  display: grid;
  place-items: center;
  border-radius: inherit;
  z-index: 2;
}

.card-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-8);
  color: var(--color-text-secondary);
}

.spinner {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 3px solid var(--color-border-subtle);
  border-top-color: var(--color-primary);
  animation: spin 0.9s linear infinite;
}

.loading-text {
  font-size: var(--font-size-sm);
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.card-footer {
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
}

.variant-solid {
  background: var(--gradient-card);
  border-color: var(--color-primary);
  color: var(--color-text-primary);
}

.variant-soft {
  background: var(--color-surface);
}

.variant-glass {
  background: var(--color-glass);
  border: 1px solid rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  box-shadow: var(--shadow-overlay);
}

.variant-premium {
  background: var(--gradient-card);
  border: 1px solid var(--color-border);
  backdrop-filter: blur(20px);
  box-shadow: var(--glow-primary);
}

.variant-bordered {
  background: transparent;
  border-color: var(--color-border);
  box-shadow: none;
}

@media (max-width: 768px) {
  .base-card {
    padding: var(--space-20);
    border-radius: var(--radius-md);
  }
}
</style>
