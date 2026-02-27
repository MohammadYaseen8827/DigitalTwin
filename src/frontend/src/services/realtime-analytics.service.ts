import * as signalR from '@microsoft/signalr'
import type { Ref } from 'vue'
import { ref } from 'vue'
import { toast } from 'vue-sonner'
import { errorReporter } from './errorReporter.service'

import { apiConfig } from '@/utils/apiConfig'

interface RealTimeDataPoint {
  timestamp: Date
  value: number
  metric: string
  machineId?: string
}

interface StreamSubscription {
  stream: string
  machineId?: string
}

export class RealTimeAnalyticsService {
  private connection: signalR.HubConnection | null = null
  private apiUrl = apiConfig.getBaseUrl()
  private hubUrl = this.apiUrl.endsWith('/')
    ? this.apiUrl.replace(/\/api\/$/, '/hubs/realtime-analytics')
    : this.apiUrl.replace('/api', '/hubs/realtime-analytics')

  // Reactive state
  public isConnected = ref(false)
  public subscriptions = ref<StreamSubscription[]>([])
  public telemetryData = ref<RealTimeDataPoint[]>([])
  public predictionData = ref<any[]>([])
  public alertData = ref<any[]>([])
  public systemHealthData = ref<any[]>([])

  // Callback handlers
  private onTelemetryCallback: ((data: any) => void) | null = null
  private onPredictionCallback: ((data: any) => void) | null = null
  private onAlertCallback: ((data: any) => void) | null = null
  private onSystemHealthCallback: ((data: any) => void) | null = null

  /**
   * Initialize the SignalR connection
   */
  public async initialize(): Promise<void> {
    if (this.connection) return

    try {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build()

      // Setup event handlers
      this.setupEventHandlers()

      // Start connection
      await this.connection.start()
      this.isConnected.value = true
      toast.success('Real-time analytics connected')

    } catch (error) {
      errorReporter.error('Failed to connect to real-time analytics:', error)
      toast.error('Failed to connect to real-time analytics')
      throw error
    }
  }

  /**
   * Setup SignalR event handlers
   */
  private setupEventHandlers(): void {
    if (!this.connection) return

    // Connection events
    const dev = typeof import.meta !== 'undefined' && import.meta.env?.DEV
    this.connection.on('connected', (data: { connectionId: string }) => {
      if (dev) console.debug('Connected to real-time analytics hub:', data.connectionId)
    })

    this.connection.on('streamSubscribed', (data: { stream: string; machineId?: string }) => {
      if (dev) console.debug(`Subscribed to ${data.stream} stream`, data.machineId ? `for machine ${data.machineId}` : '')
      this.subscriptions.value.push({ stream: data.stream, machineId: data.machineId })
    })

    this.connection.on('streamUnsubscribed', (data: { stream: string; machineId?: string }) => {
      if (dev) console.debug(`Unsubscribed from ${data.stream} stream`, data.machineId ? `for machine ${data.machineId}` : '')
      this.subscriptions.value = this.subscriptions.value.filter(
        sub => !(sub.stream === data.stream && sub.machineId === data.machineId)
      )
    })

    this.connection.on('allStreamsSubscribed', (data: { machineId: string }) => {
      if (dev) console.debug(`Subscribed to all streams for machine ${data.machineId}`)
    })

    this.connection.on('allStreamsUnsubscribed', (data: { machineId: string }) => {
      if (dev) console.debug(`Unsubscribed from all streams for machine ${data.machineId}`)
      this.subscriptions.value = []
    })

    // Data streaming events
    this.connection.on('telemetryUpdate', (data: any) => {
      const dataPoint: RealTimeDataPoint = {
        timestamp: new Date(),
        value: data.value,
        metric: data.metric,
        machineId: data.machineId
      }

      this.telemetryData.value.unshift(dataPoint)
      // Keep only last 1000 data points
      if (this.telemetryData.value.length > 1000) {
        this.telemetryData.value = this.telemetryData.value.slice(0, 1000)
      }

      this.onTelemetryCallback?.(data)
    })

    this.connection.on('predictionUpdate', (data: any) => {
      this.predictionData.value.unshift(data)
      if (this.predictionData.value.length > 100) {
        this.predictionData.value = this.predictionData.value.slice(0, 100)
      }

      this.onPredictionCallback?.(data)
    })

    this.connection.on('alertUpdate', (data: any) => {
      this.alertData.value.unshift(data)
      if (this.alertData.value.length > 50) {
        this.alertData.value = this.alertData.value.slice(0, 50)
      }

      this.onAlertCallback?.(data)
    })

    this.connection.on('systemHealthUpdate', (data: any) => {
      this.systemHealthData.value.unshift(data)
      if (this.systemHealthData.value.length > 50) {
        this.systemHealthData.value = this.systemHealthData.value.slice(0, 50)
      }

      this.onSystemHealthCallback?.(data)
    })

    // Connection lifecycle events
    this.connection.onclose((error?: Error) => {
      this.isConnected.value = false
      if (dev) console.debug('Real-time analytics connection closed', error)
      if (error) {
        toast.error('Real-time analytics connection lost')
      }
    })

    this.connection.onreconnecting((error?: Error) => {
      if (dev) console.debug('Reconnecting to real-time analytics...', error)
      toast.loading('Reconnecting to real-time analytics...')
    })

    this.connection.onreconnected((connectionId?: string) => {
      this.isConnected.value = true
      if (dev) console.debug('Reconnected to real-time analytics:', connectionId)
      toast.success('Reconnected to real-time analytics')
    })
  }

  /**
   * Subscribe to machine telemetry stream
   */
  public async subscribeToTelemetry(machineId: string): Promise<void> {
    if (!this.connection) await this.initialize()
    await this.connection?.invoke('JoinMachineTelemetryStream', machineId)
  }

  /**
   * Unsubscribe from machine telemetry stream
   */
  public async unsubscribeFromTelemetry(machineId: string): Promise<void> {
    if (!this.connection) return
    await this.connection.invoke('LeaveMachineTelemetryStream', machineId)
  }

  /**
   * Subscribe to prediction stream
   */
  public async subscribeToPredictions(machineId: string): Promise<void> {
    if (!this.connection) await this.initialize()
    await this.connection?.invoke('JoinPredictionStream', machineId)
  }

  /**
   * Unsubscribe from prediction stream
   */
  public async unsubscribeFromPredictions(machineId: string): Promise<void> {
    if (!this.connection) return
    await this.connection.invoke('LeavePredictionStream', machineId)
  }

  /**
   * Subscribe to alerts stream
   */
  public async subscribeToAlerts(): Promise<void> {
    if (!this.connection) await this.initialize()
    await this.connection?.invoke('JoinAlertsStream')
  }

  /**
   * Unsubscribe from alerts stream
   */
  public async unsubscribeFromAlerts(): Promise<void> {
    if (!this.connection) return
    await this.connection.invoke('LeaveAlertsStream')
  }

  /**
   * Subscribe to system health stream
   */
  public async subscribeToSystemHealth(): Promise<void> {
    if (!this.connection) await this.initialize()
    await this.connection?.invoke('JoinSystemHealthStream')
  }

  /**
   * Unsubscribe from system health stream
   */
  public async unsubscribeFromSystemHealth(): Promise<void> {
    if (!this.connection) return
    await this.connection.invoke('LeaveSystemHealthStream')
  }

  /**
   * Subscribe to all streams for a machine
   */
  public async subscribeToAllStreams(machineId: string): Promise<void> {
    if (!this.connection) await this.initialize()
    await this.connection?.invoke('JoinAllStreams', machineId)
  }

  /**
   * Unsubscribe from all streams
   */
  public async unsubscribeFromAllStreams(machineId: string): Promise<void> {
    if (!this.connection) return
    await this.connection.invoke('LeaveAllStreams', machineId)
  }

  /**
   * Set callback for telemetry updates
   */
  public onTelemetry(callback: (data: any) => void): void {
    this.onTelemetryCallback = callback
  }

  /**
   * Set callback for prediction updates
   */
  public onPrediction(callback: (data: any) => void): void {
    this.onPredictionCallback = callback
  }

  /**
   * Set callback for alert updates
   */
  public onAlert(callback: (data: any) => void): void {
    this.onAlertCallback = callback
  }

  /**
   * Set callback for system health updates
   */
  public onSystemHealth(callback: (data: any) => void): void {
    this.onSystemHealthCallback = callback
  }

  /**
   * Disconnect from the hub
   */
  public async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      this.isConnected.value = false
      this.subscriptions.value = []
      toast.info('Disconnected from real-time analytics')
    }
  }

  /**
   * Get current connection status
   */
  public getStatus(): {
    isConnected: boolean;
    subscriptions: StreamSubscription[];
    connectionId?: string
  } {
    return {
      isConnected: this.isConnected.value,
      subscriptions: [...this.subscriptions.value],
      connectionId: this.connection?.connectionId ?? undefined
    }
  }

  /**
   * Clear data buffers
   */
  public clearData(): void {
    this.telemetryData.value = []
    this.predictionData.value = []
    this.alertData.value = []
    this.systemHealthData.value = []
  }
}

// Singleton instance
export const realTimeAnalyticsService = new RealTimeAnalyticsService()