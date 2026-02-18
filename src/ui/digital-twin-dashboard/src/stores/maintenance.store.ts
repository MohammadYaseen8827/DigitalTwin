import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { MaintenanceRecordDto, PlanMaintenanceRequest, CompleteMaintenanceRequest } from '@/api/types/index'
import * as maintenanceService from '@/services/maintenance.service'
import { useToast } from '@/lib/magic-mcp-ui'

export const useMaintenanceStore = defineStore('maintenance', () => {
    const toast = useToast()
    const records = ref<MaintenanceRecordDto[]>([])
    const loading = ref(false)

    async function loadActiveMaintenance() {
        loading.value = true
        try {
            records.value = await maintenanceService.fetchActiveMaintenance()
        } catch (err) {
            console.error('Failed to load active maintenance:', err)
        } finally {
            loading.value = false
        }
    }

    async function loadHistory(machineId: string) {
        loading.value = true
        try {
            return await maintenanceService.fetchMaintenanceHistory(machineId)
        } catch (err) {
            console.error('Failed to load history:', err)
            return []
        } finally {
            loading.value = false
        }
    }

    async function planNewMaintenance(request: PlanMaintenanceRequest) {
        try {
            const newRecord = await maintenanceService.planMaintenance(request)
            records.value.push(newRecord)
            toast.success('Maintenance planned successfully')
            return newRecord
        } catch (err) {
            toast.error('Failed to plan maintenance')
            throw err
        }
    }

    async function startJob(id: string) {
        try {
            const updated = await maintenanceService.startMaintenance(id)
            const index = records.value.findIndex(r => r.id === id)
            if (index !== -1) records.value[index] = updated
            toast.info('Maintenance job started')
        } catch (err) {
            toast.error('Failed to start maintenance')
        }
    }

    async function completeJob(id: string, request: CompleteMaintenanceRequest) {
        try {
            const updated = await maintenanceService.completeMaintenance(id, request)
            records.value = records.value.filter(r => r.id !== id)
            toast.success('Maintenance completed')
        } catch (err) {
            toast.error('Failed to complete maintenance')
        }
    }

    return {
        records,
        loading,
        loadActiveMaintenance,
        loadHistory,
        planNewMaintenance,
        startJob,
        completeJob
    }
})
