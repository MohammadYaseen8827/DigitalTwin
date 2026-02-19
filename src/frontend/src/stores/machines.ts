import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import type { Machine, MachineStatus, TelemetryMetrics, RULPrediction, DashboardStats } from '@/types'

/** Backend machine response (GET api/machines, GET api/machines/{id}) */
interface MachineDto {
    id: string
    name: string
    type: string
    status: string
    properties: string
    remainingUsefulLifeDays?: number
    failureProbability?: number
    healthStatus?: string
    createdAt: string
    updatedAt: string
    location?: string
    installationDate?: string
    lastMaintenanceDate?: string
    healthScore?: number
}

/** Backend telemetry latest response (GET api/telemetry/{id}/latest) */
interface TelemetryDto {
    id: string
    machineId: string
    dataType: string
    data: Record<string, number>
    timestamp: string
}

/** Backend RUL response (POST api/predictions/rul/{id}) */
interface RulPredictionResult {
    machineId: string
    rul: number
    rulUnit: string
    confidence: number
    lowerBound: number
    upperBound: number
    predictionTime: string
    modelVersion: string
}

function mapMachineDtoToFrontend(dto: MachineDto): Machine {
    const status = mapBackendStatusToFrontend(dto.status)
    let specifications: Record<string, string> = {}
    try {
        if (dto.properties) {
            const parsed = JSON.parse(dto.properties) as Record<string, unknown>
            for (const [k, v] of Object.entries(parsed)) {
                specifications[k] = typeof v === 'string' ? v : String(v)
            }
        }
    } catch {
        // ignore invalid JSON
    }
    return {
        id: dto.id,
        name: dto.name,
        type: dto.type,
        location: dto.location ?? '',
        status,
        healthScore: dto.healthScore ?? 0,
        lastMaintenanceDate: dto.lastMaintenanceDate ?? '',
        nextMaintenanceDate: '',
        installDate: dto.installationDate ?? '',
        specifications
    }
}

function mapBackendStatusToFrontend(status: string): MachineStatus {
    const s = status?.toLowerCase() ?? ''
    switch (s) {
        case 'operational':
            return 'Running'
        case 'warning':
            return 'Running'  // Warning is still operational but with alerts
        case 'critical':
            return 'Error'
        case 'maintenance':
            return 'Maintenance'
        case 'offline':
            return 'Offline'
        default:
            return 'Running'  // Default to Running for unknown statuses
    }
}

function mapTelemetryDtoToMetrics(dto: TelemetryDto): TelemetryMetrics {
    const data = dto.data ?? {}
    return {
        machineId: dto.machineId,
        temperature: data.temperature ?? 0,
        vibration: data.vibration ?? 0,
        pressure: data.pressure ?? 0,
        humidity: data.humidity,
        powerConsumption: data.powerConsumption,
        rpm: data.rpm,
        operatingHours: data.operatingHours,
        lastUpdated: dto.timestamp
    }
}

function mapRulResultToFrontend(data: RulPredictionResult): RULPrediction {
    const lastUpdated = data.predictionTime ? new Date(data.predictionTime) : new Date()
    return {
        machineId: data.machineId,
        currentRUL: data.rul,
        predictedRUL: data.rul,
        confidenceLower: data.lowerBound,
        confidenceUpper: data.upperBound,
        degradationRate: 0,
        estimatedFailureDate: lastUpdated,
        modelType: 'rul',
        modelVersion: data.modelVersion || '1.0.0',
        lastUpdated
    }
}

export const useMachinesStore = defineStore('machines', () => {
    const machines = ref<Machine[]>([])
    const selectedMachine = ref<Machine | null>(null)
    const currentMetrics = ref<Record<string, TelemetryMetrics>>({})
    const rulPredictions = ref<Record<string, RULPrediction>>({})
    const isLoading = ref(false)
    const error = ref<string | null>(null)
    const stats = ref<DashboardStats | null>(null)

    const runningMachines = computed(() => machines.value.filter(m => m.status === 'Running'))
    const errorMachines = computed(() => machines.value.filter(m => m.status === 'Error'))
    const maintenanceMachines = computed(() => machines.value.filter(m => m.status === 'Maintenance'))

    const averageHealthScore = computed(() => {
        if (machines.value.length === 0) return 0
        return Math.round(machines.value.reduce((sum, m) => sum + m.healthScore, 0) / machines.value.length)
    })

    async function fetchMachines(): Promise<void> {
        isLoading.value = true
        error.value = null

        try {
            const response = await api.get<MachineDto[]>('/api/machines')
            machines.value = response.data.map(mapMachineDtoToFrontend)
        } catch (err) {
            error.value = 'Failed to fetch machines'
            console.error('Error fetching machines:', err)
        } finally {
            isLoading.value = false
        }
    }

    async function fetchMachineById(id: string): Promise<Machine | null> {
        isLoading.value = true
        error.value = null

        try {
            const response = await api.get<MachineDto>(`/api/machines/${id}`)
            const machine = mapMachineDtoToFrontend(response.data)
            selectedMachine.value = machine
            return machine
        } catch (err) {
            error.value = `Failed to fetch machine ${id}`
            console.error('Error fetching machine:', err)
            return null
        } finally {
            isLoading.value = false
        }
    }

    async function fetchDashboardStats(): Promise<void> {
        try {
            const response = await api.get<DashboardStats>('/api/dashboard/stats')
            stats.value = response.data
        } catch (err) {
            console.error('Error fetching dashboard stats:', err)
        }
    }

    async function fetchMachineMetrics(machineId: string): Promise<TelemetryMetrics | null> {
        try {
            const response = await api.get<TelemetryDto>(`/api/telemetry/${machineId}/latest`)
            const metrics = mapTelemetryDtoToMetrics(response.data)
            currentMetrics.value[machineId] = metrics
            return metrics
        } catch (err) {
            console.error('Error fetching metrics:', err)
            return null
        }
    }

    async function fetchRULPrediction(machineId: string): Promise<RULPrediction | null> {
        try {
            const response = await api.post<RulPredictionResult>(`/api/predictions/rul/${machineId}`, {})
            const mapped = mapRulResultToFrontend(response.data)
            rulPredictions.value[machineId] = mapped
            return mapped
        } catch (err) {
            console.error('Error fetching RUL prediction:', err)
            return null
        }
    }

    function updateMachineMetrics(machineId: string, metrics: TelemetryMetrics): void {
        currentMetrics.value[machineId] = metrics
    }

    function updateMachineStatus(machineId: string, status: Machine['status']): void {
        const machine = machines.value.find(m => m.id === machineId)
        if (machine) machine.status = status
        if (selectedMachine.value?.id === machineId) {
            selectedMachine.value.status = status
        }
    }

    return {
        machines,
        selectedMachine,
        currentMetrics,
        rulPredictions,
        isLoading,
        error,
        stats,
        runningMachines,
        errorMachines,
        maintenanceMachines,
        averageHealthScore,
        fetchMachines,
        fetchMachineById,
        fetchDashboardStats,
        fetchMachineMetrics,
        fetchRULPrediction,
        updateMachineMetrics,
        updateMachineStatus
    }
})
