<template>
  <div class="tabs-container">
    <div class="tabs-header" role="tablist">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        :class="['tab-button', { active: modelValue === tab.id }]"
        @click="$emit('update:modelValue', tab.id)"
        role="tab"
        :aria-selected="modelValue === tab.id"
      >
        {{ tab.label }}
      </button>
    </div>
    <div class="tabs-content">
      <slot></slot>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

defineProps({
  modelValue: {
    type: [String, Number],
    required: true,
  },
  tabs: {
    type: Array as () => Array<{ id: string | number; label: string }>,
    required: true,
  },
});

defineEmits(['update:modelValue']);
</script>

<style scoped>
.tabs-container {
  width: 100%;
}

.tabs-header {
  display: flex;
  border-bottom: 1px solid var(--color-border);
  margin-bottom: var(--space-16);
  overflow-x: auto;
  scrollbar-width: none; /* Firefox */
}

.tabs-header::-webkit-scrollbar {
  display: none; /* Chrome, Safari, Edge */
}

.tab-button {
  padding: var(--space-12) var(--space-20);
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  color: var(--color-text-secondary);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
  font-size: var(--font-size-sm);
}

.tab-button:hover {
  color: var(--color-primary);
  background-color: var(--color-surface);
}

.tab-button.active {
  color: var(--color-primary);
  border-bottom-color: var(--color-primary);
  font-weight: 600;
}

.tabs-content {
  padding: var(--space-8) 0;
}
</style>
