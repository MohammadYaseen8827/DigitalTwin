<script setup lang="ts">
import { useCssModule } from 'vue'

interface Props {
  variant?: 'default' | 'glass' | 'elevated' | 'outline' | 'ghost'
  padding?: 'none' | 'sm' | 'md' | 'lg' | 'xl'
  hover?: boolean
  dots?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  padding: 'md',
  hover: false,
  dots: false
})

const styles = useCssModule()
</script>

<template>
  <div :class="[
    styles['card'], 
    styles[`card--${variant}`], 
    styles[`padding--${padding}`],
    hover && styles['card--hover'],
    dots && styles['card--dots']
  ]">
    <div v-if="$slots.header" :class="styles['header']">
      <slot name="header" />
    </div>
    <div :class="styles['body']">
      <slot />
    </div>
    <div v-if="$slots.footer" :class="styles['footer']">
      <slot name="footer" />
    </div>
  </div>
</template>

<style module>
.card {
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: all var(--transition-normal);
  display: flex;
  flex-direction: column;
  position: relative;
  background-clip: padding-box;
}

.card--dots::after {
  content: '';
  position: absolute;
  inset: 0;
  background-image: radial-gradient(var(--color-border) 1px, transparent 1px);
  background-size: 20px 20px;
  opacity: 0.15;
  pointer-events: none;
}

/* Variants */
.card--default {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-card);
}

.card--glass {
  background: var(--color-glass);
  backdrop-filter: var(--backdrop-blur);
  border: 1px solid var(--color-border-subtle);
  box-shadow: var(--shadow-subtle);
}

.card--elevated {
  background: var(--color-surface-elevated);
  border: 1px solid var(--color-border-strong);
  box-shadow: var(--shadow-elevated);
}

.card--outline {
  background: transparent;
  border: 1px solid var(--color-border);
}

.card--ghost {
  background: transparent;
  border: 1px solid transparent;
}

/* Hover effect */
.card--hover:hover {
  transform: translateY(-2px);
  border-color: var(--color-primary-hover);
  box-shadow: var(--shadow-elevated), var(--shadow-primary);
}

.card--hover::after {
  content: '';
  position: absolute;
  inset: -1px;
  border-radius: inherit;
  background: var(--gradient-primary);
  opacity: 0;
  z-index: -1;
  transition: opacity var(--transition-normal);
}

.card--hover:hover::after {
  opacity: 0.1;
}

/* Padding */
.padding--none { padding: 0; }
.padding--sm { padding: var(--space-12); }
.padding--md { padding: var(--space-card); }
.padding--lg { padding: var(--space-32); }
.padding--xl { padding: var(--space-48); }

.header {
  padding-bottom: var(--space-16);
  border-bottom: 1px solid var(--color-border-subtle);
  margin-bottom: var(--space-card);
  position: relative;
  z-index: 10;
}

.body {
  flex: 1;
  position: relative;
  z-index: 10;
}

.footer {
  margin-top: var(--space-20);
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
  position: relative;
  z-index: 10;
}
</style>
