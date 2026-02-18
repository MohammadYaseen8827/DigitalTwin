<template>
  <div class="alert-center">
    <button class="alert-trigger" @click="toggle" :class="{ 'has-alerts': activeAlertsCount > 0 }">
      <span class="icon">🔔</span>
      <span v-if="activeAlertsCount > 0" class="badge">{{ activeAlertsCount }}</span>
    </button>

    <div v-if="isOpen" class="alert-dropdown glass-panel animate-fade-in">
      <header class="dropdown-header">
        <h3>Active Alerts</h3>
        <span class="count">{{ activeAlertsCount }} items</span>
      </header>

      <div class="alerts-list">
        <div v-if="alerts.length === 0" class="empty-state">
          No active maintenance alerts.
        </div>
        
        <div 
          v-for="alert in alerts" 
          :key="alert.id" 
          class="alert-item"
          :class="`severity-${alert.severity.toLowerCase()}`"
        >
          <div class="alert-content">
            <p class="alert-message">{{ alert.message }}</p>
            <span class="alert-time">{{ formatTime(alert.createdAt) }}</span>
          </div>
          <div class="alert-actions">
            <button class="plan-btn" @click="handlePlanMaintenance(alert)">Plan Maintenance</button>
            <button class="ack-btn" @click="resolveAlert(alert.id)">Acknowledge</button>
          </div>
        </div>
      </div>
    </div>

    <MaintenancePlanDialog
      v-if="planningAlert"
      :open="true"
      :machine-id="planningAlert.machineId"
      :alert-id="planningAlert.id"
      :alert-message="planningAlert.message"
      @cancel="planningAlert = null"
      @success="handlePlanSuccess"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { storeToRefs } from 'pinia'
import { useAlertsStore } from '@/stores/alerts.store'
import MaintenancePlanDialog from '@/components/machines/MaintenancePlanDialog.vue'
import type { AlertDto } from '@/api/types'

const alertsStore = useAlertsStore()
const { alerts, activeAlertsCount } = storeToRefs(alertsStore)
const { loadAlerts, resolveAlert } = alertsStore

const isOpen = ref(false)
const planningAlert = ref<AlertDto | null>(null)

function toggle() {
  isOpen.value = !isOpen.value
}

function handlePlanMaintenance(alert: AlertDto) {
  planningAlert.value = alert
}

function handlePlanSuccess() {
  planningAlert.value = null
  isOpen.value = false
  loadAlerts()
}

function formatTime(timestamp: string) {
  return new Date(timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

onMounted(() => {
  loadAlerts()
})
</script>

<style scoped>
.alert-center {
  position: relative;
}

.alert-trigger {
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 8px;
  border-radius: 50%;
  position: relative;
  transition: background 0.2s;
}

.alert-trigger:hover {
  background: rgba(255, 255, 255, 0.1);
}

.badge {
  position: absolute;
  top: 0;
  right: 0;
  background: var(--color-error, #f5222d);
  color: white;
  font-size: 0.7rem;
  padding: 2px 6px;
  border-radius: 10px;
  border: 2px solid white;
}

.alert-dropdown {
  position: absolute;
  top: calc(100% + 12px);
  right: 0;
  width: 320px;
  max-height: 400px;
  background: white;
  z-index: 1000;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
  border-radius: var(--radius-lg, 12px);
  display: flex;
  flex-direction: column;
}

.dropdown-header {
  padding: 1rem;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.dropdown-header h3 {
  margin: 0;
  font-size: 1rem;
}

.count {
  font-size: 0.8rem;
  color: #8c8c8c;
}

.alerts-list {
  overflow-y: auto;
  flex: 1;
}

.empty-state {
  padding: 2rem;
  text-align: center;
  color: #bfbfbf;
}

.alert-item {
  padding: 1rem;
  border-bottom: 1px solid #f0f0f0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.alert-item.severity-critical {
  border-left: 4px solid var(--color-error, #f5222d);
  background: rgba(245, 34, 45, 0.02);
}

.alert-item.severity-warning {
  border-left: 4px solid var(--color-warning, #faad14);
}

.alert-message {
  margin: 0;
  font-size: 0.9rem;
  line-height: 1.4;
}

.alert-time {
  font-size: 0.75rem;
  color: #8c8c8c;
}

.alert-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
}

.ack-btn, .plan-btn {
  background: transparent;
  border: 1px solid var(--primary-color, #1890ff);
  color: var(--primary-color, #1890ff);
  font-size: 0.75rem;
  padding: 2px 8px;
  border-radius: 4px;
  cursor: pointer;
}

.ack-btn:hover, .plan-btn:hover {
  background: var(--primary-color, #1890ff);
  color: white;
}

.plan-btn {
  border-color: var(--color-warning, #faad14);
  color: var(--color-warning, #faad14);
}

.plan-btn:hover {
  background: var(--color-warning, #faad14);
}
</style>
