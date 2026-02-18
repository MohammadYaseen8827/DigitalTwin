import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { TelemetryData, TelemetryDataPoint } from '@/types'

export const useTelemetryStore = defineStore('telemetry', () => {
    const realtimeData = ref<Record<string, TelemetryData>>({})
    const historicalData = ref<Record<string, TelemetryDataPoint[]>>({})
    const isConnected = ref(false)
    const lastUpdate = ref<Date | null>(null)

    function setRealtimeData(machineId: string, data: TelemetryData): void {
        realtimeData.value[machineId] = data
        lastUpdate.value = new Date()
    }

    function addHistoricalDataPoint(machineId: string, dataPoint: TelemetryDataPoint): void {
        if (!historicalData.value[machineId]) {
            historicalData.value[machineId] = []
        }
        historicalData.value[machineId].push(dataPoint)

        if (historicalData.value[machineId].length > 1000) {
            historicalData.value[machineId].shift()
        }
    }

    function setHistoricalData(machineId: string, data: TelemetryDataPoint[]): void {
        historicalData.value[machineId] = data
    }

    function setConnected(connected: boolean): void {
        isConnected.value = connected
    }

    function clearMachineData(machineId: string): void {
        delete realtimeData.value[machineId]
        delete historicalData.value[machineId]
    }

    function clearAllData(): void {
        realtimeData.value = {}
        historicalData.value = {}
    }

    return {
        realtimeData,
        historicalData,
        isConnected,
        lastUpdate,
        setRealtimeData,
        addHistoricalDataPoint,
        setHistoricalData,
        setConnected,
        clearMachineData,
        clearAllData
    }
})
