<template>
  <div class="form-section" :class="[variant, { 'is-required': required, 'is-disabled': disabled }]">
    <div v-if="title || $slots.header" class="form-section-header">
      <h3 v-if="title" class="form-section-title">
        {{ title }}
        <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
      </h3>
      <div v-if="$slots.header" class="form-section-header-actions">
        <slot name="header" />
      </div>
    </div>
    
    <div class="form-section-content">
      <slot />
    </div>
    
    <div v-if="description" class="form-section-description">
      {{ description }}
    </div>
    
    <div v-if="$slots.footer" class="form-section-footer">
      <slot name="footer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  description: {
    type: String,
    default: ''
  },
  required: {
    type: Boolean,
    default: false
  },
  variant: {
    type: String as () => 'default' | 'bordered' | 'elevated',
    default: 'default'
  },
  disabled: {
    type: Boolean,
    default: false
  }
});
</script>

<style scoped>
.form-section {
  --form-section-bg: var(--color-surface);
  --form-section-border: var(--color-border-subtle);
  --form-section-padding: var(--space-24);
  --form-section-gap: var(--space-16);
  --form-section-radius: var(--radius-lg);
  --form-section-shadow: var(--shadow-sm);
  
  display: flex;
  flex-direction: column;
  gap: var(--form-section-gap);
  background: var(--form-section-bg);
  transition: all 0.2s ease;
}

/* Variants */
.form-section.bordered {
  border: 1px solid var(--form-section-border);
  border-radius: var(--form-section-radius);
  padding: var(--form-section-padding);
}

.form-section.elevated {
  background: var(--color-surface-elevated);
  border-radius: var(--form-section-radius);
  box-shadow: var(--form-section-shadow);
  padding: var(--form-section-padding);
}

/* Header */
.form-section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-16);
  margin-bottom: var(--space-4);
}

.form-section-title {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
  line-height: 1.4;
}

.required-indicator {
  color: var(--color-error);
  margin-left: 0.25em;
}

/* Content */
.form-section-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

/* Description */
.form-section-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin-top: calc(-1 * var(--space-8));
}

/* Footer */
.form-section-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
  padding-top: var(--space-8);
  border-top: 1px solid var(--color-border-subtle);
}

/* Disabled state */
.form-section:has(.is-disabled) {
  opacity: 0.7;
  pointer-events: none;
}

/* Responsive */
@media (max-width: 768px) {
  .form-section {
    --form-section-padding: var(--space-16);
    --form-section-gap: var(--space-12);
  }
  
  .form-section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-8);
  }
  
  .form-section-footer {
    flex-direction: column;
  }
  
  .form-section-footer > * {
    width: 100%;
  }
}

/* Dark mode */
@media (prefers-color-scheme: dark) {
  .form-section {
    --form-section-bg: var(--color-surface-elevated);
    --form-section-border: var(--color-border-subtle);
  }
}
</style>
