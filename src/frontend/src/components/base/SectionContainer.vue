<template>
  <section :class="['section-container', `max-${maxWidth}`, { 'is-bordered': bordered }]">
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
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.max-lg { max-width: 960px; }
.max-xl { max-width: 1240px; }
.max-full { max-width: none; }

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-20);
  border-bottom: 1px solid var(--color-border-subtle);
  padding-bottom: var(--space-12);
}

.section-header h2 {
  margin: 0;
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
}

.section-eyebrow {
  margin: 0 0 var(--space-2) 0;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--color-primary);
  opacity: 0.8;
}

.section-actions {
  display: inline-flex;
  gap: var(--space-8);
  align-items: center;
}

.section-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.section-footer {
  padding-top: var(--space-20);
  border-top: 1px solid var(--color-border-subtle);
}

.section-container.is-bordered {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  background: var(--color-depth-1);
  padding: var(--space-24);
  box-shadow: var(--shadow-sm);
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