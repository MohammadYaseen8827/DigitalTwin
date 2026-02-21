import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { alertsService } from '@/services/alerts.service'
import type { AlertDto } from '@/api/types'

export const useAlertsStore = defineStore('alerts', () => {
  const alerts = ref<AlertDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedAlert = ref<AlertDto | null>(null)

  // Computed properties
  const activeAlerts = computed(() => {
    return alerts.value.filter(alert => !alert.acknowledged)
  })

  const criticalAlerts = computed(() => {
    return activeAlerts.value.filter(alert => alert.severity === 'critical')
  })

  const highAlerts = computed(() => {
    return activeAlerts.value.filter(alert => alert.severity === 'high')
  })

  const mediumAlerts = computed(() => {
    return activeAlerts.value.filter(alert => alert.severity === 'medium')
  })

  const lowAlerts = computed(() => {
    return activeAlerts.value.filter(alert => alert.severity === 'low')
  })

  const alertsByMachine = computed(() => {
    const grouped = alerts.value.reduce((acc, alert) => {
      const machineId = alert.machineId
      if (!acc[machineId]) {
        acc[machineId] = []
      }
      acc[machineId].push(alert)
      return acc
    }, {} as Record<string, AlertDto[]>)
    
    return grouped
  })

  const unreadCount = computed(() => {
    return activeAlerts.value.length
  })

  const sortedAlerts = computed(() => {
    return [...alerts.value].sort((a, b) => {
      const severityOrder = { critical: 4, high: 3, medium: 2, low: 1 }
      const severityDiff = severityOrder[b.severity] - severityOrder[a.severity]
      return severityDiff // Sort by severity (critical first)
    })
  })

  // Actions
  const fetchAlerts = async () => {
    loading.value = true
    error.value = null
    
    try {
      alerts.value = await alertsService.getActiveAlerts()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch alerts'
    } finally {
      loading.value = false
    }
  }

  const acknowledgeAlert = async (alertId: string) => {
    try {
      await alertsService.acknowledgeAlert(alertId)
      // Update local state
      const alert = alerts.value.find(a => a.id === alertId)
      if (alert) {
        alert.acknowledged = true
        alert.acknowledgedAt = new Date()
      }
    } catch (error) {
      console.error('Failed to acknowledge alert:', error)
    }
  }

  const dismissAlert = async (alertId: string) => {
    try {
      await alertsService.dismissAlert(alertId)
      // Remove from local state
      const index = alerts.value.findIndex(a => a.id === alertId)
      if (index !== -1) {
        alerts.value.splice(index, 1)
      }
    } catch (error) {
      console.error('Failed to dismiss alert:', error)
    }
  }

  const selectAlert = (alert: AlertDto) => {
    selectedAlert.value = alert
  }

  const clearSelection = () => {
    selectedAlert.value = null
  }

  return {
    // State
    alerts: readonly(alerts),
    loading: readonly(loading),
    error: readonly(error),
    selectedAlert: readonly(selectedAlert),
    
    // Computed
    activeAlerts,
    criticalAlerts,
    highAlerts,
    mediumAlerts,
    lowAlerts,
    alertsByMachine,
    unreadCount,
    sortedAlerts,
    
    // Actions
    fetchAlerts,
    acknowledgeAlert,
    dismissAlert,
    selectAlert,
    clearSelection
  }
})
