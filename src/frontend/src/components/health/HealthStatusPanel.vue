<template>
  <BaseCard variant="soft" class="health-panel">
    <template #header>
      <div class="header-row">
        <span>System Health</span>
        <BaseButton variant="ghost" size="sm" @click="refreshHealth" :loading="loading">
          ↻ Refresh
        </BaseButton>
      </div>
    </template>

    <div v-if="loading && !healthData" class="loading-state">
      <BaseSkeleton height="60px" />
      <BaseSkeleton height="40px" />
      <BaseSkeleton height="40px" />
    </div>

    <div v-else-if="healthData" class="health-content">
      <div class="overall-status" :class="`status-${healthData.status.toLowerCase()}`">
        <div class="status-icon">
          <span v-if="healthData.status === 'Healthy'">✓</span>
          <span v-else-if="healthData.status === 'Degraded'">⚠</span>
          <span v-else>✗</span>
        </div>
        <div class="status-info">
          <h3>{{ healthData.status }}</h3>
          <p class="duration">Response time: {{ healthData.totalDuration }}</p>
        </div>
      </div>

      <div class="health-entries">
        <div
          v-for="(entry, key) in healthData.entries"
          :key="key"
          class="health-entry"
          :class="`entry-${entry.status.toLowerCase()}`"
        >
          <div class="entry-header">
            <span class="entry-name">{{ formatEntryName(key) }}</span>
            <span class="entry-status">{{ entry.status }}</span>
          </div>
          <div class="entry-details">
            <span v-if="entry.description" class="entry-description">{{ entry.description }}</span>
            <span class="entry-duration">{{ entry.duration }}</span>
          </div>
          <div v-if="entry.exception" class="entry-exception">
            {{ entry.exception }}
          </div>
        </div>
      </div>

      <div class="last-checked">
        Last checked: {{ lastChecked }}
      </div>
    </div>

    <div v-else class="error-state">
      <p>Unable to retrieve health status</p>
      <BaseButton variant="primary" size="sm" @click="refreshHealth">
        Retry
      </BaseButton>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { getHealthStatus, type HealthCheckResult } from '@/services/health.service'

interface Props {
  autoRefresh?: boolean
  refreshInterval?: number
}

const props = withDefaults(defineProps<Props>(), {
  autoRefresh: true,
  refreshInterval: 30000 // 30 seconds
})

const healthData = ref<HealthCheckResult | null>(null)
const loading = ref(false)
const lastChecked = ref<string>('')
let refreshTimer: number | null = null

const refreshHealth = async () => {
  loading.value = true
  try {
    healthData.value = await getHealthStatus()
    lastChecked.value = new Date().toLocaleTimeString()
  } catch (error) {
    console.error('Failed to refresh health status:', error)
  } finally {
    loading.value = false
  }
}

const formatEntryName = (key: string): string => {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim()
}

onMounted(() => {
  refreshHealth()
  
  if (props.autoRefresh) {
    refreshTimer = window.setInterval(refreshHealth, props.refreshInterval)
  }
})

onBeforeUnmount(() => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
  }
})
</script>

<style scoped>
.health-panel {
  padding: var(--space-20);
}

.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.loading-state {
  display: grid;
  gap: var(--space-16);
}

.health-content {
  display: grid;
  gap: var(--space-20);
}

.overall-status {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  padding: var(--space-20);
  border-radius: var(--radius-lg);
  border: 2px solid;
}

.overall-status.status-healthy {
  background: color-mix(in srgb, #22c55e 10%, transparent);
  border-color: #22c55e;
}

.overall-status.status-degraded {
  background: color-mix(in srgb, #facc15 10%, transparent);
  border-color: #facc15;
}

.overall-status.status-unhealthy {
  background: color-mix(in srgb, #ef4444 10%, transparent);
  border-color: #ef4444;
}

.status-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  border-radius: var(--radius-full);
  background: var(--color-surface);
}

.status-healthy .status-icon {
  color: #22c55e;
}

.status-degraded .status-icon {
  color: #facc15;
}

.status-unhealthy .status-icon {
  color: #ef4444;
}

.status-info h3 {
  margin: 0;
  font-size: var(--font-size-xl);
  font-weight: 600;
  color: var(--color-text-primary);
}

.duration {
  margin: var(--space-4) 0 0 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.health-entries {
  display: grid;
  gap: var(--space-12);
}

.health-entry {
  padding: var(--space-16);
  border-radius: var(--radius-md);
  background: var(--color-surface-alt);
  border-left: 4px solid;
}

.health-entry.entry-healthy {
  border-color: #22c55e;
}

.health-entry.entry-degraded {
  border-color: #facc15;
}

.health-entry.entry-unhealthy {
  border-color: #ef4444;
}

.entry-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-8);
}

.entry-name {
  font-weight: 600;
  color: var(--color-text-primary);
}

.entry-status {
  font-size: var(--font-size-sm);
  padding: var(--space-4) var(--space-8);
  border-radius: var(--radius-sm);
  font-weight: 600;
}

.entry-healthy .entry-status {
  background: color-mix(in srgb, #22c55e 20%, transparent);
  color: #22c55e;
}

.entry-degraded .entry-status {
  background: color-mix(in srgb, #facc15 20%, transparent);
  color: #facc15;
}

.entry-unhealthy .entry-status {
  background: color-mix(in srgb, #ef4444 20%, transparent);
  color: #ef4444;
}

.entry-details {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.entry-exception {
  margin-top: var(--space-8);
  padding: var(--space-8);
  background: color-mix(in srgb, #ef4444 10%, transparent);
  border-radius: var(--radius-sm);
  font-size: var(--font-size-sm);
  color: #ef4444;
  font-family: monospace;
}

.last-checked {
  text-align: center;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  padding-top: var(--space-12);
  border-top: 1px solid var(--color-border);
}

.error-state {
  text-align: center;
  padding: var(--space-32);
  color: var(--color-text-secondary);
}

.error-state p {
  margin-bottom: var(--space-16);
}
</style>
