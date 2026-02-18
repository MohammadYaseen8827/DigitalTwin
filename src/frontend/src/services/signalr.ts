import * as signalR from '@microsoft/signalr-client'
import { ref } from 'vue'
import { useTelemetryStore } from '@/stores/telemetry'
import { useAlertsStore } from '@/stores/alerts'
import { useMachinesStore } from '@/stores/machines'
import { usePredictionsStore } from '@/stores/predictions'
import type { TelemetryData, Alert, Prediction } from '@/types'

// Backend SignalR data types (matching backend TelemetryData, AlertData, PredictionData)
interface BackendTelemetryData {
    machineId: string
    timestamp: string
    temperature?: number
    pressure?: number
    vibration?: number
    rulPrediction?: number
    message?: string
    metrics?: Record<string, number>
}

interface BackendAlertData {
    alertId: string
    machineId: string
    timestamp: string
    severity: string
    category: string
    message: string
    recommendedAction?: string
    acknowledged: boolean
}

interface BackendPredictionData {
    machineId: string
    timestamp: string
    rulRemaining?: number
    failureProbability?: number
    confidenceLevel?: number
    predictionType?: string
    featureImportance?: Record<string, number>
    message?: string
}

class SignalRService {
    private connection: signalR.HubConnection | null = null
    private subscribedMachines = new Set<string>()

    isConnected = ref(false)
    connectionState = ref<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected)
    lastError = ref<Error | null>(null)

    get connectionId() {
        return this.connection?.connectionId || null
    }

    async connect(): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return
        }

        const hubUrl = import.meta.env.VITE_HUB_URL || '/hub'

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                accessTokenFactory: () => localStorage.getItem('token') || ''
            })
            .withAutomaticReconnect([0, 1000, 5000, 10000, 30000])
            .configureLogging(signalR.LogLevel.Information)
            .build()

        this.connection.onreconnecting((error: Error) => {
            console.warn('SignalR reconnecting:', error)
            this.connectionState.value = signalR.HubConnectionState.Reconnecting
            this.lastError.value = error || null
        })

        this.connection.onreconnected((connectionId: string) => {
            console.log('SignalR reconnected:', connectionId)
            this.connectionState.value = signalR.HubConnectionState.Connected
            this.isConnected.value = true
            this.lastError.value = null
            this.subscribedMachines.forEach(machineId => {
                this.subscribeToMachine(machineId)
            })
        })

        this.connection.onclose((error: Error) => {
            console.warn('SignalR connection closed:', error)
            this.connectionState.value = signalR.HubConnectionState.Disconnected
            this.isConnected.value = false
            this.lastError.value = error || null
        })

        this.registerTelemetryHandlers()
        this.registerAlertHandlers()
        this.registerPredictionHandlers()
        this.registerMachineStatusHandlers()

        try {
            await this.connection.start()
            this.connectionState.value = signalR.HubConnectionState.Connected
            this.isConnected.value = true
            console.log('SignalR connected successfully')
        } catch (error) {
            console.error('SignalR connection failed:', error)
            this.connectionState.value = signalR.HubConnectionState.Disconnected
            this.isConnected.value = false
            this.lastError.value = error as Error
            throw error
        }
    }

    async disconnect(): Promise<void> {
        if (this.connection) {
            await this.connection.stop()
            this.connection = null
            this.isConnected.value = false
            this.connectionState.value = signalR.HubConnectionState.Disconnected
            this.subscribedMachines.clear()
        }
    }

    subscribeToMachine(machineId: string): void {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            this.connection.invoke('SubscribeToMachine', machineId)
                .then(() => {
                    this.subscribedMachines.add(machineId)
                    console.log(`Subscribed to machine: ${machineId}`)
                })
                .catch((error: Error) => {
                    console.error(`Failed to subscribe to machine ${machineId}:`, error)
                })
        }
    }

    unsubscribeFromMachine(machineId: string): void {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            this.connection.invoke('UnsubscribeFromMachine', machineId)
                .then(() => {
                    this.subscribedMachines.delete(machineId)
                    console.log(`Unsubscribed from machine: ${machineId}`)
                })
                .catch((error: Error) => {
                    console.error(`Failed to unsubscribe from machine ${machineId}:`, error)
                })
        }
    }

    private registerTelemetryHandlers(): void {
        if (!this.connection) return

        // Backend sends 'TelemetryUpdate' event
        this.connection.on('TelemetryUpdate', (data: BackendTelemetryData) => {
            const telemetryStore = useTelemetryStore()
            const frontendData: TelemetryData = this.mapBackendTelemetryToFrontend(data)
            telemetryStore.setRealtimeData(data.machineId, frontendData)
        })

        // Backend sends 'TelemetryUpdate' for batch data
        this.connection.on('TelemetryBatch', (data: BackendTelemetryData[]) => {
            const telemetryStore = useTelemetryStore()
            data.forEach(item => {
                const frontendData = this.mapBackendTelemetryToFrontend(item)
                telemetryStore.setRealtimeData(item.machineId, frontendData)
            })
        })
    }

    private registerAlertHandlers(): void {
        if (!this.connection) return

        // Backend sends 'NewAlert' event
        this.connection.on('NewAlert', (data: BackendAlertData) => {
            const alertsStore = useAlertsStore()
            const alert = this.mapBackendAlertToFrontend(data)
            alertsStore.addAlert(alert)
        })

        // Backend sends 'AlertUpdated' for updates
        this.connection.on('AlertUpdated', (data: BackendAlertData) => {
            const alertsStore = useAlertsStore()
            const alert = this.mapBackendAlertToFrontend(data)
            alertsStore.updateAlert(alert.id, alert)
        })
    }

    private registerPredictionHandlers(): void {
        if (!this.connection) return

        // Backend sends 'PredictionUpdate' event
        this.connection.on('PredictionUpdate', (data: BackendPredictionData) => {
            const predictionsStore = usePredictionsStore()
            const prediction = this.mapBackendPredictionToFrontend(data)
            predictionsStore.addPrediction(prediction)
        })
    }

    private registerMachineStatusHandlers(): void {
        if (!this.connection) return

        // Listen for machine status changes via TelemetryUpdate
        this.connection.on('TelemetryUpdate', (data: BackendTelemetryData) => {
            // Status is not in the current backend TelemetryData, skip
        })
    }

    // Map backend TelemetryData to frontend TelemetryData
    private mapBackendTelemetryToFrontend(data: BackendTelemetryData): TelemetryData {
        return {
            machineId: data.machineId,
            timestamp: data.timestamp || new Date().toISOString(),
            sensors: {
                temperature: data.temperature ?? 0,
                pressure: data.pressure ?? 0,
                vibration: data.vibration ?? 0,
                rulPrediction: data.rulPrediction ?? 0
            }
        }
    }

    // Map backend AlertData to frontend Alert interface
    private mapBackendAlertToFrontend(data: BackendAlertData): Alert {
        return {
            id: data.alertId || data.machineId,
            machineId: data.machineId,
            title: data.message?.split(':')[0] || 'Alert',
            description: data.message?.split(':').slice(1).join(':') || data.message || '',
            severity: (data.severity?.toLowerCase() || 'info') as Alert['severity'],
            status: data.acknowledged ? 'acknowledged' as const : 'active' as const,
            timestamp: data.timestamp || new Date().toISOString(),
            category: data.category
        }
    }

    // Map backend PredictionData to frontend Prediction interface
    private mapBackendPredictionToFrontend(data: BackendPredictionData): Prediction {
        return {
            id: `${data.machineId}-${Date.now()}`,
            machineId: data.machineId,
            type: (data.predictionType?.toLowerCase() || 'rul') as Prediction['type'],
            result: `RUL: ${data.rulRemaining?.toFixed(1) || 'N/A'} hours`,
            confidence: data.confidenceLevel || 0,
            timestamp: data.timestamp || new Date().toISOString(),
            modelVersion: '1.0.0',
            factors: data.featureImportance
        }
    }
}

export const signalRService = new SignalRService()
export default signalRService
