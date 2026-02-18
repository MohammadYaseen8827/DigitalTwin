import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AlertDto } from '@/api/types/index'
import { fetchActiveAlerts, acknowledgeAlert } from '@/services/alerts.service'
import { useToast } from '@/lib/magic-mcp-ui'

export const useAlertsStore = defineStore('alerts', () => {
    const toast = useToast()

    // State
    const alerts = ref<AlertDto[]>([])
    const loading = ref(false)
    const error = ref<string | null>(null)

    // Getters
    const activeAlertsCount = computed(() => alerts.value.length)
    const criticalAlertsCount = computed(() =>
        alerts.value.filter(a => a.severity === 'Critical').length
    )

    // Actions
    async function loadAlerts(machineId?: string) {
        loading.value = true
        error.value = null
        try {
            alerts.value = await fetchActiveAlerts(machineId)
        } catch (err) {
            error.value = 'Failed to load alerts'
            console.error('Failed to load alerts:', err)
        } finally {
            loading.value = false
        }
    }

    async function resolveAlert(alertId: string) {
        try {
            await acknowledgeAlert(alertId)
            // Remove from local state
            alerts.value = alerts.value.filter(a => a.id !== alertId)
            toast.success('Alert acknowledged')
        } catch (err) {
            console.error('Failed to acknowledge alert:', err)
            toast.error('Failed to acknowledge alert')
        }
    }

    function addAlert(alert: AlertDto) {
        // Check if duplicate
        if (!alerts.value.find(a => a.id === alert.id)) {
            alerts.value.unshift(alert)
            toast.info(`New alert: ${alert.message}`)
        }
    }

    return {
        alerts,
        loading,
        error,
        activeAlertsCount,
        criticalAlertsCount,
        loadAlerts,
        resolveAlert,
        addAlert
    }
})
