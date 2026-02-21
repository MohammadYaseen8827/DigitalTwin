<template>
  <div class="alert-panel">
    <div class="bg-white rounded-lg shadow p-6">
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-lg font-semibold text-gray-900">Maintenance Alerts</h3>
        <div class="flex items-center space-x-2">
          <span class="text-sm text-gray-500">{{ activeAlerts.length }} active alerts</span>
          <button 
            @click="refreshAlerts"
            :disabled="loading"
            class="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            <span v-if="!loading">Refresh</span>
            <span v-else>Loading...</span>
          </button>
        </div>
      </div>
      
      <div v-if="loading" class="flex items-center justify-center h-32">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
      
      <div v-else-if="activeAlerts.length > 0" class="space-y-4">
        <div 
          v-for="alert in sortedAlerts" 
          :key="alert.id"
          class="border rounded-lg p-4"
          :class="getAlertBorderClass(alert.severity)"
        >
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center space-x-3">
                <div class="w-3 h-3 rounded-full" :class="getSeverityClass(alert.severity)"></div>
                <div>
                  <h4 class="font-semibold text-gray-900">{{ alert.title }}</h4>
                  <p class="text-sm text-gray-600 mt-1">{{ alert.machineName }}</p>
                </div>
              </div>
              
              <div class="text-right">
                <span class="text-xs text-gray-500">{{ formatTime(alert.createdAt) }}</span>
              </div>
            </div>
          </div>
          
          <div class="mt-3">
            <p class="text-gray-700">{{ alert.message }}</p>
            <div v-if="alert.recommendation" class="mt-2 p-3 bg-blue-50 rounded-md">
              <p class="text-sm text-blue-800">
                <strong>Recommendation:</strong> {{ alert.recommendation }}
              </p>
            </div>
          </div>
          
          <div class="flex items-center justify-between mt-4 pt-3 border-t">
            <div class="flex items-center space-x-4">
              <span class="text-sm text-gray-500">Confidence: {{ (alert.confidence * 100).toFixed(1) }}%</span>
              <span class="text-sm text-gray-500">Severity: {{ alert.severity }}</span>
            </div>
            
            <div class="flex space-x-2">
              <button 
                @click="acknowledgeAlert(alert.id)"
                :disabled="alert.acknowledged"
                class="px-3 py-1 bg-green-600 text-white rounded-md hover:bg-green-700 disabled:opacity-50 text-sm"
              >
                {{ alert.acknowledged ? 'Acknowledged' : 'Acknowledge' }}
              </button>
              
              <button 
                @click="dismissAlert(alert.id)"
                class="px-3 py-1 bg-gray-600 text-white rounded-md hover:bg-gray-700 text-sm"
              >
                Dismiss
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <div v-else class="text-center text-gray-500 py-8">
        <p>No active alerts</p>
        <div class="mt-4 text-sm text-gray-600">
          All systems are operating within normal parameters
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAlertStore } from '@/stores/alerts'

interface Alert {
  id: string
  title: string
  message: string
  machineName: string
  severity: 'low' | 'medium' | 'high' | 'critical'
  confidence: number
  recommendation?: string
  createdAt: Date
  acknowledged: boolean
  acknowledgedAt?: Date
}

const alertStore = useAlertStore()
const loading = ref(false)

const activeAlerts = computed(() => {
  return alertStore.activeAlerts
})

const sortedAlerts = computed(() => {
  return [...activeAlerts.value].sort((a, b) => {
    const severityOrder = { critical: 4, high: 3, medium: 2, low: 1 }
    const severityDiff = severityOrder[b.severity] - severityOrder[a.severity]
    return severityDiff // Sort by severity (critical first)
  })
})

const getAlertBorderClass = (severity: string) => {
  switch (severity) {
    case 'critical': return 'border-red-200 bg-red-50'
    case 'high': return 'border-orange-200 bg-orange-50'
    case 'medium': return 'border-yellow-200 bg-yellow-50'
    case 'low': return 'border-blue-200 bg-blue-50'
    default: return 'border-gray-200 bg-gray-50'
  }
}

const getSeverityClass = (severity: string) => {
  switch (severity) {
    case 'critical': return 'bg-red-600'
    case 'high': return 'bg-orange-600'
    case 'medium': return 'bg-yellow-600'
    case 'low': return 'bg-blue-600'
    default: return 'bg-gray-600'
  }
}

const formatTime = (date: Date) => {
  return date.toLocaleString()
}

const refreshAlerts = async () => {
  loading.value = true
  try {
    await alertStore.fetchActiveAlerts()
  } catch (error) {
    console.error('Failed to refresh alerts:', error)
  } finally {
    loading.value = false
  }
}

const acknowledgeAlert = async (alertId: string) => {
  try {
    await alertStore.acknowledgeAlert(alertId)
  } catch (error) {
    console.error('Failed to acknowledge alert:', error)
  }
}

const dismissAlert = async (alertId: string) => {
  try {
    await alertStore.dismissAlert(alertId)
  } catch (error) {
    console.error('Failed to dismiss alert:', error)
  }
}

onMounted(() => {
  refreshAlerts()
})
</script>

<style scoped>
.alert-panel {
  @apply p-6;
}

.animate-spin {
  @apply animate-spin;
  border-top-color: #3498db;
  border-right-color: #3498db;
  border-bottom-color: #3498db;
  border-left-color: #3498db;
}

.animate-spin {
  border: 2px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
