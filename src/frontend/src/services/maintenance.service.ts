import axiosClient from '@/api/axiosClient'
import type {
    MaintenanceRecordDto,
    PlanMaintenanceRequest,
    CompleteMaintenanceRequest
} from '@/api/types/index'

export async function planMaintenance(request: PlanMaintenanceRequest): Promise<MaintenanceRecordDto> {
    return axiosClient.post('/Maintenance/plan', request) as any
}

export async function startMaintenance(id: string): Promise<MaintenanceRecordDto> {
    return axiosClient.post(`/Maintenance/${id}/start`) as any
}

export async function completeMaintenance(id: string, request: CompleteMaintenanceRequest): Promise<MaintenanceRecordDto> {
    return axiosClient.post(`/Maintenance/${id}/complete`, request) as any
}

export async function cancelMaintenance(id: string, reason: string): Promise<MaintenanceRecordDto> {
    return axiosClient.post(`/Maintenance/${id}/cancel`, { reason }) as any
}

export async function fetchMaintenanceHistory(machineId: string): Promise<MaintenanceRecordDto[]> {
    return axiosClient.get(`/Maintenance/machine/${machineId}`) as any
}

export async function fetchActiveMaintenance(): Promise<MaintenanceRecordDto[]> {
    return axiosClient.get('/Maintenance/active') as any
}

/**
 * Search maintenance records
 */
export async function searchMaintenance(query: string): Promise<MaintenanceRecordDto[]> {
    return axiosClient.get('/Maintenance/search', { params: { query } }) as any
}

// Export service object for convenience
export const maintenanceService = {
    planMaintenance,
    startMaintenance,
    completeMaintenance,
    cancelMaintenance,
    fetchMaintenanceHistory,
    fetchActiveMaintenance,
    searchMaintenance
}

export default maintenanceService
