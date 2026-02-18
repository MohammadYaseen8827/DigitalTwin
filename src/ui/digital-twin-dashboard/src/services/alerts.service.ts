import axiosClient from '@/api/axiosClient'
import type { AlertDto } from '@/api/types/index'

/**
 * Fetches all active alerts, optionally filtered by machine ID.
 */
export async function fetchActiveAlerts(machineId?: string): Promise<AlertDto[]> {
    const params = machineId ? { machineId } : {}
    return axiosClient.get('/Alerts', { params }) as any
}

/**
 * Acknowledges an alert by its ID.
 */
export async function acknowledgeAlert(alertId: string): Promise<void> {
    return axiosClient.post(`/Alerts/${alertId}/acknowledge`) as any
}
