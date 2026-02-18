import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import type { Alert } from '@/types'

// Backend alert response DTO interface
interface BackendAlertDto {
    id: string
    machineId: string
    message: string
    severity: 'Info' | 'Warning' | 'Critical' | 'error' | 'warning' | 'critical' | 'info'
    createdAt: string
    isAcknowledged: boolean
    acknowledgedBy?: string
    relatedPredictionId?: string
}

// Map backend alert to frontend Alert interface
function mapBackendAlert(dto: BackendAlertDto): Alert {
    // Parse message to extract title and description
    // Assuming message format: "TYPE: Description"
    const colonIndex = dto.message.indexOf(':')
    const title = colonIndex > -1 ? dto.message.substring(0, colonIndex).trim() : dto.message
    const description = colonIndex > -1 ? dto.message.substring(colonIndex + 1).trim() : dto.message

    // Map backend severity to frontend severity
    const severity = dto.severity.toLowerCase() as Alert['severity']

    // Determine status from IsAcknowledged
    const status = dto.isAcknowledged
        ? ('acknowledged' as const)
        : ('active' as const)

    return {
        id: dto.id,
        machineId: dto.machineId,
        title,
        description,
        severity,
        status,
        timestamp: dto.createdAt,
        acknowledgedBy: dto.acknowledgedBy,
        relatedPredictionId: dto.relatedPredictionId
    }
}

export const useAlertsStore = defineStore('alerts', () => {
    const alerts = ref<Alert[]>([])
    const activeAlerts = computed(() => alerts.value.filter(a => a.status === 'active'))
    const criticalAlerts = computed(() => alerts.value.filter(a => a.severity === 'critical' && a.status === 'active'))
    const acknowledgedAlerts = computed(() => alerts.value.filter(a => a.status === 'acknowledged'))
    const isLoading = ref(false)
    const error = ref<string | null>(null)

    async function fetchAlerts(machineId?: string): Promise<void> {
        isLoading.value = true
        error.value = null

        try {
            const url = machineId
                ? `/api/alerts?machineId=${machineId}`
                : '/api/alerts'
            const response = await api.get<BackendAlertDto[]>(url)
            // Map backend DTOs to frontend Alert interface
            alerts.value = response.data.map(mapBackendAlert)
        } catch (err) {
            error.value = 'Failed to fetch alerts'
            console.error('Error fetching alerts:', err)
        } finally {
            isLoading.value = false
        }
    }

    function addAlert(alert: Alert): void {
        alerts.value.unshift(alert)
        // Keep only last 100 alerts
        if (alerts.value.length > 100) {
            alerts.value.pop()
        }
    }

    function updateAlert(id: string, updatedAlert: Alert): void {
        const index = alerts.value.findIndex(a => a.id === id)
        if (index !== -1) {
            alerts.value[index] = updatedAlert
        }
    }

    async function acknowledgeAlert(id: string): Promise<void> {
        try {
            await api.put(`/api/alerts/${id}/acknowledge`)
            const alert = alerts.value.find(a => a.id === id)
            if (alert) {
                alert.status = 'acknowledged'
                alert.acknowledgedBy = 'current-user' // Would come from auth
            }
        } catch (err) {
            console.error('Error acknowledging alert:', err)
            throw err
        }
    }

    async function resolveAlert(id: string): Promise<void> {
        try {
            await api.delete(`/api/alerts/${id}`)
            const alert = alerts.value.find(a => a.id === id)
            if (alert) {
                alert.status = 'resolved'
                alert.resolvedAt = new Date()
            }
        } catch (err) {
            console.error('Error resolving alert:', err)
            throw err
        }
    }

    function clearAlerts(): void {
        alerts.value = []
    }

    return {
        alerts,
        activeAlerts,
        criticalAlerts,
        acknowledgedAlerts,
        isLoading,
        error,
        fetchAlerts,
        addAlert,
        updateAlert,
        acknowledgeAlert,
        resolveAlert,
        clearAlerts
    }
})
