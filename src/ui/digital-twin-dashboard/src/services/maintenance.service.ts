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
