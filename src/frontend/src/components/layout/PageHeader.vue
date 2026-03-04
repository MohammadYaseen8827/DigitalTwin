<script setup lang="ts">
import { useCssModule } from 'vue'

defineProps<{
  title: string
  eyebrow?: string
  description?: string
}>()

const styles = useCssModule()
</script>

<template>
  <div :class="styles['page-header']">
    <div :class="styles['copy']">
      <p v-if="eyebrow" :class="styles['eyebrow']">{{ eyebrow }}</p>
      <h1 :class="styles['title']">{{ title }}</h1>
      <p v-if="description" :class="styles['description']">{{ description }}</p>
    </div>
    <div v-if="$slots.actions" :class="styles['actions']">
      <slot name="actions" />
    </div>
  </div>
</template>

<style module>
.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-20);
  padding: var(--space-8) 0 var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
  flex-wrap: wrap;
  position: relative;
}

.copy {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  position: relative;
  padding-left: var(--space-20);
}

.copy::before {
  content: '';
  position: absolute;
  left: 0;
  top: var(--space-4);
  bottom: var(--space-4);
  width: 2px;
  background: var(--gradient-primary);
  border-radius: var(--radius-full);
  box-shadow: var(--glow-sm);
}

.eyebrow {
  margin: 0;
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--color-primary);
  font-weight: 700;
  opacity: 0.9;
}

.title {
  margin: 0;
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
  line-height: 1.1;
}

.description {
  margin: var(--space-4) 0 0 0;
  font-size: var(--font-size-sm);
  font-weight: 450;
  color: var(--color-text-secondary);
  max-width: 520px;
  line-height: 1.5;
}

.actions {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  flex-shrink: 0;
  margin-bottom: var(--space-4);
}
</style>
