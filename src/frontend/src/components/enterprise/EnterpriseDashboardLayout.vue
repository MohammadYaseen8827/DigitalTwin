<template>
  <div class="dashboard-layout">
    <!-- Header -->
    <header class="dashboard-layout__header">
      <div class="dashboard-layout__header-left">
        <h1 class="dashboard-layout__title">{{ title }}</h1>
        <p v-if="subtitle" class="dashboard-layout__subtitle">{{ subtitle }}</p>
      </div>

      <div class="dashboard-layout__header-right">
        <slot name="header-actions" />
        <ConnectionStatus
          :status="connectionStatus"
          :last-update="lastUpdate"
          @reconnect="handleReconnect"
        />
      </div>
    </header>

    <!-- Alerts -->
    <AlertBanner
      v-if="alerts.length > 0"
      :alerts="alerts"
      :title="alertTitle"
      :show-acknowledge="showAlertAcknowledge"
      @acknowledge="handleAcknowledge"
      @dismiss="dismissAlerts"
    />

    <!-- Main grid -->
    <div class="dashboard-layout__grid">
      <slot />
    </div>

    <!-- Footer with data freshness -->
    <footer v-if="showFooter" class="dashboard-layout__footer">
      <span class="dashboard-layout__footer-item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10" />
          <polyline points="12 6 12 12 16 14" />
        </svg>
        Last updated: {{ formattedLastUpdate }}
      </span>
      <span v-if="totalMachines !== undefined" class="dashboard-layout__footer-item">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
        </svg>
        {{ totalMachines }} machines monitored
      </span>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import ConnectionStatus from './ConnectionStatus.vue'
import AlertBanner, { type Alert } from './AlertBanner.vue'

interface Props {
  title: string
  subtitle?: string
  connectionStatus?: 'connected' | 'connecting' | 'disconnected'
  lastUpdate?: Date | string
  alerts?: Alert[]
  alertTitle?: string
  showAlertAcknowledge?: boolean
  showFooter?: boolean
  totalMachines?: number
}

const props = withDefaults(defineProps<Props>(), {
  connectionStatus: 'connected',
  alerts: () => [],
  alertTitle: 'Active Alerts',
  showAlertAcknowledge: true,
  showFooter: true
})

const emit = defineEmits<{
  reconnect: []
  acknowledgeAlerts: [alerts: Alert[]]
  dismissAlerts: []
}>()

const formattedLastUpdate = computed(() => {
  if (!props.lastUpdate) return 'Never'
  const d = props.lastUpdate instanceof Date ? props.lastUpdate : new Date(props.lastUpdate)
  return d.toLocaleTimeString()
})

function handleReconnect() {
  emit('reconnect')
}

function handleAcknowledge(alerts: Alert[]) {
  emit('acknowledgeAlerts', alerts)
}

function dismissAlerts() {
  emit('dismissAlerts')
}
</script>

<style scoped>
.dashboard-layout {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  padding: var(--space-24);
  min-height: 100%;
}

/* Header */
.dashboard-layout__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-16);
}

.dashboard-layout__header-left {
  flex: 1;
}

.dashboard-layout__title {
  margin: 0;
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--color-text-primary);
}

.dashboard-layout__subtitle {
  margin: var(--space-4) 0 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.dashboard-layout__header-right {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

/* Grid */
.dashboard-layout__grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-16);
}

/* Responsive grid */
@media (max-width: 1280px) {
  .dashboard-layout__grid {
    grid-template-columns: repeat(6, 1fr);
  }
}

@media (max-width: 768px) {
  .dashboard-layout__grid {
    grid-template-columns: 1fr;
  }

  .dashboard-layout__header {
    flex-direction: column;
  }

  .dashboard-layout__header-right {
    width: 100%;
    justify-content: space-between;
  }
}

/* Footer */
.dashboard-layout__footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
}

.dashboard-layout__footer-item {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.dashboard-layout__footer-item svg {
  width: 14px;
  height: 14px;
}
</style>
