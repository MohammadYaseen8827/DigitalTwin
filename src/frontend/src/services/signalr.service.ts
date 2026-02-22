import * as signalR from '@microsoft/signalr'
import { ref } from 'vue'
import { useMachinesStore } from '@/stores/machines.store'

class SignalRService {
    private connection: signalR.HubConnection | null = null
    private apiUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'
    private hubUrl = this.apiUrl.replace('/api', '/hubs/telemetry')
    
    // Connection status as reactive ref
    public isConnected = ref(false)

    public async connect(): Promise<void> {
        return this.start()
    }

    public async start(): Promise<void> {
        if (this.connection) return

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(this.hubUrl)
            .withAutomaticReconnect()
            .build()

        this.connection.on('telemetry', (telemetry: any) => {
            const store = useMachinesStore()
            // We could add telemetry to a list if we had a telemetry store, 
            // but for now, we just ensure the machine status is updated if needed
        })

        this.connection.on('prediction', (prediction: any) => {
            const store = useMachinesStore()
            store.updatePrediction(prediction.machineId, prediction)
        })

        try {
            await this.connection.start()
            this.isConnected.value = true
        } catch (err) {
            this.isConnected.value = false
            setTimeout(() => this.start(), 5000)
        }
    }

    public async joinMachineGroup(machineId: string): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinMachineGroup', machineId)
        }
    }

    public async leaveMachineGroup(machineId: string): Promise<void> {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveMachineGroup', machineId)
        }
    }

    public on(eventName: string, handler: (...args: any[]) => void): void {
        if (!this.connection) {
            // Queue if not started? Or just log.
            return
        }
        this.connection.on(eventName, handler)
    }

    public stop(): Promise<void> {
        if (!this.connection) return Promise.resolve()
        this.isConnected.value = false
        return this.connection.stop()
    }
}

export const signalRService = new SignalRService()
