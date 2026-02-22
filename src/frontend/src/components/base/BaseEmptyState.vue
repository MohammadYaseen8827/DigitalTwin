<script setup lang="ts">
import { computed } from 'vue'
import { Search } from 'lucide-vue-next'

const props = withDefaults(defineProps<{
  title?: string
  description?: string
  icon?: any
  actionLabel?: string
  size?: 'sm' | 'md' | 'lg'
}>(), {
  title: 'No data',
  description: 'There is nothing to display here yet.',
  icon: Search,
  size: 'md'
})

const emit = defineEmits<{
  (e: 'action'): void
}>()

const iconSize = computed(() => {
  switch (props.size) {
    case 'sm': return 32
    case 'lg': return 64
    default: return 48
  }
})
</script>

<template>
  <div class="empty-state" :class="`empty-${size}`">
    <div class="empty-icon">
      <component :is="icon" :size="iconSize" />
    </div>
    <h3 class="empty-title">{{ title }}</h3>
    <p v-if="description" class="empty-description">{{ description }}</p>
    <button 
      v-if="actionLabel" 
      class="empty-action"
      @click="emit('action')"
    >
      {{ actionLabel }}
    </button>
    <slot />
  </div>
</template>

<style scoped>
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: var(--space-32);
  min-height: 200px;
}

.empty-sm { min-height: 120px; padding: var(--space-16); }
.empty-lg { min-height: 320px; padding: var(--space-48); }

.empty-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: var(--color-surface-alt);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-20);
}

.empty-sm .empty-icon {
  width: 48px;
  height: 48px;
  margin-bottom: var(--space-12);
}

.empty-lg .empty-icon {
  width: 100px;
  height: 100px;
  margin-bottom: var(--space-24);
}

.empty-title {
  font-size: var(--font-size-lg);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-8);
}

.empty-sm .empty-title {
  font-size: var(--font-size-base);
}

.empty-description {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  margin: 0 0 var(--space-20);
  max-width: 400px;
}

.empty-sm .empty-description {
  font-size: var(--font-size-xs);
  margin-bottom: var(--space-12);
}

.empty-action {
  padding: var(--space-10) var(--space-20);
  background: var(--color-primary);
  color: white;
  border: none;
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
}

.empty-action:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-medium);
}
</style>
