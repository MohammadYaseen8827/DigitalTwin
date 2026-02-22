<template>
  <Transition name="alert-slide">
    <div v-if="visible && alerts.length > 0" class="alert-banner" :class="`alert-banner--${severity}`">
      <div class="alert-banner__icon">
        <svg v-if="severity === 'critical'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" />
          <line x1="12" y1="9" x2="12" y2="13" />
          <line x1="12" y1="17" x2="12.01" y2="17" />
        </svg>
        <svg v-else-if="severity === 'warning'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="8" x2="12" y2="12" />
          <line x1="12" y1="16" x2="12.01" y2="16" />
        </svg>
        <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10" />
          <line x1="12" y1="16" x2="12" y2="12" />
          <line x1="12" y1="8" x2="12.01" y2="8" />
        </svg>
      </div>

      <div class="alert-banner__content">
        <div class="alert-banner__title">{{ title }}</div>
        <div class="alert-banner__message">
          <span v-for="(alert, idx) in alerts" :key="alert.id" class="alert-banner__alert-item">
            <strong>{{ alert.machineName || 'System' }}</strong>: {{ alert.message }}
            <span v-if="idx < alerts.length - 1" class="alert-banner__separator">|</span>
          </span>
        </div>
      </div>

      <div class="alert-banner__actions">
        <button
          v-if="showAcknowledge"
          class="alert-banner__action"
          @click="$emit('acknowledge', alerts)"
        >
          Acknowledge
        </button>
        <button class="alert-banner__close" @click="$emit('dismiss')" aria-label="Dismiss">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <line x1="18" y1="6" x2="6" y2="18" />
            <line x1="6" y1="6" x2="18" y2="18" />
          </svg>
        </button>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from 'vue'

export interface Alert {
  id: string
  machineName?: string
  message: string
  timestamp: Date | string
  severity: 'critical' | 'warning' | 'info'
}

interface Props {
  alerts: Alert[]
  visible?: boolean
  showAcknowledge?: boolean
  title?: string
}

const props = withDefaults(defineProps<Props>(), {
  visible: true,
  showAcknowledge: true,
  title: 'Active Alerts'
})

defineEmits<{
  acknowledge: [alerts: Alert[]]
  dismiss: []
}>()

const severity = computed(() => {
  if (!props.alerts.length) return 'info'
  const hasCritical = props.alerts.some(a => a.severity === 'critical')
  if (hasCritical) return 'critical'
  const hasWarning = props.alerts.some(a => a.severity === 'warning')
  if (hasWarning) return 'warning'
  return 'info'
})
</script>

<style scoped>
.alert-banner {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  padding: var(--space-12) var(--space-16);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  border: 1px solid;
  animation: alert-enter 0.3s ease;
}

@keyframes alert-enter {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Severity variants */
.alert-banner--critical {
  border-color: var(--color-danger);
  background: rgba(239, 68, 68, 0.1);
}

.alert-banner--warning {
  border-color: var(--color-warning);
  background: rgba(245, 158, 11, 0.1);
}

.alert-banner--info {
  border-color: var(--color-primary);
  background: rgba(59, 130, 246, 0.1);
}

/* Icon */
.alert-banner__icon {
  flex-shrink: 0;
  width: 24px;
  height: 24px;
}

.alert-banner__icon svg {
  width: 100%;
  height: 100%;
}

.alert-banner--critical .alert-banner__icon {
  color: var(--color-danger);
}

.alert-banner--warning .alert-banner__icon {
  color: var(--color-warning);
}

.alert-banner--info .alert-banner__icon {
  color: var(--color-primary);
}

/* Content */
.alert-banner__content {
  flex: 1;
  min-width: 0;
}

.alert-banner__title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: var(--space-2);
}

.alert-banner__message {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-4);
  align-items: center;
}

.alert-banner__alert-item {
  display: inline-flex;
  align-items: center;
  gap: var(--space-4);
}

.alert-banner__separator {
  color: var(--color-text-muted);
  margin: 0 var(--space-4);
}

/* Actions */
.alert-banner__actions {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  flex-shrink: 0;
}

.alert-banner__action {
  padding: var(--space-6) var(--space-12);
  border: none;
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.alert-banner--critical .alert-banner__action {
  background: var(--color-danger);
  color: white;
}

.alert-banner--critical .alert-banner__action:hover {
  background: #dc2626;
}

.alert-banner--warning .alert-banner__action {
  background: var(--color-warning);
  color: #1a1a1a;
}

.alert-banner--warning .alert-banner__action:hover {
  background: #d97706;
}

.alert-banner__close {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border: none;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.alert-banner__close:hover {
  background: rgba(255, 255, 255, 0.1);
  color: var(--color-text-primary);
}

.alert-banner__close svg {
  width: 16px;
  height: 16px;
}

/* Transition */
.alert-slide-enter-active,
.alert-slide-leave-active {
  transition: all 0.3s ease;
}

.alert-slide-enter-from,
.alert-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>
