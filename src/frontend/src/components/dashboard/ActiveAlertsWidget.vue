<script setup lang="ts">
import { computed } from 'vue'

interface Alert {
  id: string
  machine: string
  type: 'critical' | 'warning' | 'info'
  message: string
  timestamp: string
}

interface Props {
  alerts: Alert[]
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const alertCounts = computed(() => ({
  critical: props.alerts.filter(a => a.type === 'critical').length,
  warning: props.alerts.filter(a => a.type === 'warning').length,
  info: props.alerts.filter(a => a.type === 'info').length
}))

const formatTime = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins}m ago`
  if (diffMins < 1440) return `${Math.floor(diffMins / 60)}h ago`
  return date.toLocaleDateString()
}
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-semibold text-gray-900">Active Alerts</h2>
      <router-link to="/alerts" class="text-sm text-primary-600 hover:text-primary-700 font-medium">View all</router-link>
    </div>
    <div class="flex gap-4 mb-4">
      <div class="flex-1 bg-red-50 rounded-lg p-3 text-center">
        <div class="text-2xl font-bold text-red-600">{{ alertCounts.critical }}</div>
        <div class="text-xs text-red-700">Critical</div>
      </div>
      <div class="flex-1 bg-yellow-50 rounded-lg p-3 text-center">
        <div class="text-2xl font-bold text-yellow-600">{{ alertCounts.warning }}</div>
        <div class="text-xs text-yellow-700">Warning</div>
      </div>
      <div class="flex-1 bg-blue-50 rounded-lg p-3 text-center">
        <div class="text-2xl font-bold text-blue-600">{{ alertCounts.info }}</div>
        <div class="text-xs text-blue-700">Info</div>
      </div>
    </div>
    <div v-if="loading" class="flex justify-center py-8"><Spinner size="md" /></div>
    <div v-else-if="alerts.length === 0" class="text-center py-8 text-gray-500">No active alerts</div>
    <div v-else class="space-y-3 max-h-64 overflow-y-auto">
      <div v-for="alert in alerts.slice(0, 5)" :key="alert.id" :class="['p-3 rounded-lg border', alert.type === 'critical' ? 'bg-red-50 border-red-200' : alert.type === 'warning' ? 'bg-yellow-50 border-yellow-200' : 'bg-blue-50 border-blue-200']">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium text-gray-900 truncate">{{ alert.machine }}</p>
            <p class="text-xs text-gray-600 mt-1">{{ alert.message }}</p>
          </div>
          <span class="text-xs text-gray-500 flex-shrink-0">{{ formatTime(alert.timestamp) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
