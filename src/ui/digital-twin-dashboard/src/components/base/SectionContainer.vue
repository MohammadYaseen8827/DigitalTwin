<template>
  <section class="section-container" :class="[`max-${maxWidth}`, { 'is-bordered': bordered }]">
    <header v-if="title || $slots.header" class="section-header">
      <div>
        <p v-if="eyebrow" class="section-eyebrow">{{ eyebrow }}</p>
        <h2 v-if="title">{{ title }}</h2>
      </div>
      <div v-if="$slots.actions" class="section-actions">
        <slot name="actions" />
      </div>
      <slot v-else name="header" />
    </header>
    <div class="section-body">
      <slot />
    </div>
    <footer v-if="$slots.footer" class="section-footer">
      <slot name="footer" />
    </footer>
  </section>
</template>

<script setup lang="ts">
const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  eyebrow: {
    type: String,
    default: ''
  },
  maxWidth: {
    type: String as () => 'lg' | 'xl' | 'full',
    default: 'xl'
  },
  bordered: {
    type: Boolean,
    default: false
  }
})
</script>

<style scoped>
.section-container {
  width: 100%;
  margin: 0 auto;
  padding: var(--space-24) clamp(var(--space-16), 4vw, var(--space-32));
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.max-lg {
  max-width: 960px;
}

.max-xl {
  max-width: 1200px;
}

.max-full {
  max-width: none;
}

.section-header {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-16);
  border-bottom: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
  padding-bottom: var(--space-16);
}

.section-header h2 {
  margin: 0;
  font-size: var(--font-size-2xl);
  color: var(--color-text-primary);
  line-height: var(--font-lineheight-tight);
}

.section-eyebrow {
  margin: 0 0 var(--space-4) 0;
  font-size: var(--font-size-xs);
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: var(--color-text-secondary);
}

.section-actions {
  display: inline-flex;
  gap: var(--space-12);
  align-items: center;
}

.section-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.section-footer {
  padding-top: var(--space-16);
  border-top: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
}

.section-container.is-bordered {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-xl);
  background: color-mix(in srgb, var(--color-surface-alt) 60%, transparent);
  box-shadow: var(--shadow-subtle);
}

@media (max-width: 768px) {
  .section-container {
    padding: var(--space-20) var(--space-16);
  }

  .section-header h2 {
    font-size: var(--font-size-xl);
  }
}
</style>
