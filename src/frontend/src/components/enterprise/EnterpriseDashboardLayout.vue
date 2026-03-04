<template>
  <div :class="styles['dashboard-layout']">
    <!-- Header -->
    <header :class="styles['dashboard-layout__header']">
      <div :class="styles['header-left']">
        <div :class="styles['title-wrap']">
           <h1 :class="styles['dashboard-layout__title']">{{ title }}</h1>
           <p v-if="subtitle" :class="styles['dashboard-layout__subtitle']">{{ subtitle }}</p>
        </div>
      </div>

      <div :class="styles['header-right']">
        <slot name="header-actions" />
        <ConnectionStatus
          :status="connectionStatus"
          :last-update="lastUpdate"
          @reconnect="handleReconnect"
        />
      </div>
    </header>

    <!-- Alerts Layer -->
    <Transition name="fade">
      <AlertBanner
        v-if="alerts.length > 0"
        :alerts="alerts"
        :title="alertTitle"
        :show-acknowledge="showAlertAcknowledge"
        @acknowledge="handleAcknowledge"
        @dismiss="dismissAlerts"
      />
    </Transition>

    <!-- Neural Grid Matrix -->
    <div :class="styles['dashboard-layout__grid']">
      <slot />
    </div>

    <!-- Data Freshness Footer -->
    <footer v-if="showFooter" :class="styles['dashboard-layout__footer']">
      <div :class="styles['footer-group']">
        <div :class="styles['footer-item']">
          <Clock :width="12" :height="12" />
          <span>Synchronized: {{ formattedLastUpdate }}</span>
        </div>
        <div v-if="totalMachines !== undefined" :class="styles['footer-item']">
          <Activity :width="12" :height="12" />
          <span>{{ totalMachines }} Active Node Clusters</span>
        </div>
      </div>
      <div :class="styles['footer-meta']">
        <span>Digital Twin OS v4.2.0</span>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, useCssModule } from 'vue'
import { Clock, Activity } from 'lucide-vue-next'
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
  alertTitle: 'ACTIVE ANOMALIES',
  showAlertAcknowledge: true,
  showFooter: true
})

const styles = useCssModule()

const emit = defineEmits<{
  reconnect: []
  acknowledgeAlerts: [alerts: Alert[]]
  dismissAlerts: []
}>()

const formattedLastUpdate = computed(() => {
  if (!props.lastUpdate) return 'PROTOCOL DESYNC'
  const d = props.lastUpdate instanceof Date ? props.lastUpdate : new Date(props.lastUpdate)
  return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
})

function handleReconnect() { emit('reconnect') }
function handleAcknowledge(alerts: Alert[]) { emit('acknowledgeAlerts', alerts) }
function dismissAlerts() { emit('dismissAlerts') }
</script>

<style module>
.dashboard-layout {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  padding: 0;
  min-height: 100%;
}

/* Header */
.dashboard-layout__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
  gap: var(--space-24);
}

.header-left { flex: 1; }

.dashboard-layout__title {
  margin: 0;
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
}

.dashboard-layout__subtitle {
  margin: var(--space-4) 0 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  font-weight: 500;
}

.header-right {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

/* Grid */
.dashboard-layout__grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-24);
}

/* Footer */
.dashboard-layout__footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: var(--space-24);
  margin-top: var(--space-12);
  border-top: 1px solid var(--color-border-subtle);
}

.footer-group {
  display: flex;
  gap: var(--space-24);
}

.footer-item {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 800;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.footer-meta {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  opacity: 0.4;
}

@media (max-width: 1400px) {
  .dashboard-layout__grid { grid-template-columns: repeat(12, 1fr); }
}

@media (max-width: 1024px) {
  .dashboard-layout__grid { grid-template-columns: 1fr; }
  .dashboard-layout__header { flex-direction: column; align-items: flex-start; }
}
</style>
