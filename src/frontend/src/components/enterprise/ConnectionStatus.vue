<template>
  <div class="connection-status" :class="`connection-status--${status}`">
    <div class="connection-status__indicator" />
    <div class="connection-status__content">
      <span class="connection-status__label">{{ label }}</span>
      <span v-if="lastUpdate" class="connection-status__timestamp">{{ formattedTime }}</span>
    </div>
    <button
      v-if="status === 'disconnected'"
      class="connection-status__reconnect"
      @click="$emit('reconnect')"
    >
      Reconnect
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  status: 'connected' | 'connecting' | 'disconnected'
  label?: string
  lastUpdate?: Date | string
}

const props = withDefaults(defineProps<Props>(), {
  label: 'Live'
})

defineEmits<{
  reconnect: []
}>()

const formattedTime = computed(() => {
  if (!props.lastUpdate) return ''
  const d = props.lastUpdate instanceof Date ? props.lastUpdate : new Date(props.lastUpdate)
  return d.toLocaleTimeString()
})
</script>

<style scoped>
.connection-status {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-6) var(--space-12);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  font-size: var(--font-size-sm);
}

.connection-status__indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

/* Status variants */
.connection-status--connected .connection-status__indicator {
  background: var(--color-success);
  box-shadow: 0 0 8px var(--color-success);
}

.connection-status--connecting .connection-status__indicator {
  background: var(--color-warning);
  animation: pulse 1s infinite;
}

.connection-status--disconnected .connection-status__indicator {
  background: var(--color-danger);
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

.connection-status__content {
  display: flex;
  flex-direction: column;
  gap: var(--space-1);
}

.connection-status__label {
  font-weight: 600;
  color: var(--color-text-primary);
}

.connection-status--connected .connection-status__label {
  color: var(--color-success);
}

.connection-status--connecting .connection-status__label {
  color: var(--color-warning);
}

.connection-status--disconnected .connection-status__label {
  color: var(--color-danger);
}

.connection-status__timestamp {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  font-family: var(--font-mono);
}

.connection-status__reconnect {
  padding: var(--space-4) var(--space-8);
  border: none;
  border-radius: var(--radius-sm);
  background: var(--color-primary);
  color: white;
  font-size: var(--font-size-xs);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.connection-status__reconnect:hover {
  background: #2563eb;
}
</style>
