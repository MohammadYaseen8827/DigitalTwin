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
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--space-24);
  border: 1px solid var(--color-border-subtle);
  box-shadow: var(--shadow-subtle);
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  transition:
    transform 0.25s ease,
    box-shadow 0.25s ease,
    border-color 0.25s ease,
    background 0.25s ease;
}

.base-card.has-hover:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-medium);
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-12);
  padding-bottom: var(--space-12);
  border-bottom: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
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
  line-height: var(--font-lineheight-normal);
}

.card-overlay {
  position: absolute;
  inset: 0;
  background: color-mix(in srgb, var(--color-surface) 68%, transparent 32%);
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
  border: 3px solid color-mix(in srgb, var(--color-border-subtle) 60%, transparent);
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
  border-top: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
}

.variant-solid {
  background: linear-gradient(160deg, color-mix(in srgb, var(--color-primary) 15%, #020617), #020617);
  border-color: color-mix(in srgb, var(--color-primary) 30%, transparent);
  color: var(--color-text-primary);
}

.variant-soft {
  background: color-mix(in srgb, var(--color-surface-alt) 80%, var(--color-primary) 20%);
}

.variant-glass {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(16px) saturate(180%);
  -webkit-backdrop-filter: blur(16px) saturate(180%);
  box-shadow: 0 8px 32px 0 rgba(0, 0, 0, 0.37);
}

.variant-premium {
  background: linear-gradient(135deg, rgba(255,255,255,0.05) 0%, rgba(255,255,255,0.01) 100%);
  border: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(20px);
  box-shadow: 0 0 20px rgba(var(--color-primary-rgb), 0.1);
}

.variant-premium::before {
  content: '';
  position: absolute;
  inset: -1px;
  border-radius: inherit;
  padding: 1px;
  background: linear-gradient(135deg, rgba(255,255,255,0.2), transparent, rgba(var(--color-primary-rgb), 0.3));
  -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  -webkit-mask-composite: xor;
  mask-composite: exclude;
  pointer-events: none;
}

.variant-bordered {
  background: transparent;
  border-color: color-mix(in srgb, var(--color-border) 60%, var(--color-primary) 40%);
  box-shadow: none;
}

@media (max-width: 768px) {
  .base-card {
    padding: var(--space-20);
    border-radius: var(--radius-md);
  }
}
</style>
