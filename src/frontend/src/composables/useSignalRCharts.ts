/**
 * SignalR Charts Composable
 * Provides real-time chart data updates via SignalR with buffering for performance
 */
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { signalRService } from '@/services/signalr'
import { useTelemetryStore } from '@/stores/telemetry'
import { usePredictionsStore } from '@/stores/predictions'
import { useMachinesStore } from '@/stores/machines'
import type { TelemetryDataPoint, PredictionDataPoint, DegradationModelData, SensorReading } from '@/types'

// Buffer configuration
const BUFFER_CONFIG = {
    maxUpdatesPerSecond: 10, // Update every 100ms max
    maxDataPoints: 1000,
    batchSize: 50
}

export interface UseSignalRChartsOptions {
    machineId: string
    sensorTypes?: string[]
    enableBuffering?: boolean
    bufferInterval?: number
}

export function useSignalRCharts(options: UseSignalRChartsOptions) {
    const { machineId, sensorTypes = ['temperature', 'vibration', 'pressure'], enableBuffering = true, bufferInterval = 100 } = options

    const telemetryStore = useTelemetryStore()
    const predictionsStore = usePredictionsStore()
    const machinesStore = useMachinesStore()

    // Reactive state
    const isConnected = ref(false)
    const isLoading = ref(false)
    const lastUpdate = ref<Date | null>(null)
    const chartDataBuffer = ref<{ type: string; data: unknown }[]>([])

    // Real-time data for charts
    const telemetryPoints = ref<Record<string, TelemetryDataPoint[]>>({})
    const predictionData = ref<PredictionDataPoint[]>([])
    const degradationData = ref<DegradationModelData[]>([])
    const sensorReadings = ref<Record<string, SensorReading>>({})

    // Buffer timeout reference
    let bufferTimeout: ReturnType<typeof setTimeout> | null = null
    let updateInterval: ReturnType<typeof setInterval> | null = null

    // Computed
    const currentRealtimeData = computed(() => telemetryStore.realtimeData[machineId])

    const temperatureData = computed(() => telemetryPoints.value.temperature || [])
    const vibrationData = computed(() => telemetryPoints.value.vibration || [])
    const pressureData = computed(() => telemetryPoints.value.pressure || [])

    // Initialize telemetry points storage
    function initializeTelemetryStorage(): void {
        sensorTypes.forEach(type => {
            if (!telemetryPoints.value[type]) {
                telemetryPoints.value[type] = []
            }
        })
    }

    // Process buffered updates
    function processBuffer(): void {
        if (chartDataBuffer.value.length === 0) return

        // Batch process buffered data
        const batch = chartDataBuffer.value.splice(0, BUFFER_CONFIG.batchSize)

        batch.forEach(item => {
            if (item.type === 'telemetry') {
                processTelemetryData(item.data as TelemetryDataPoint)
            } else if (item.type === 'prediction') {
                predictionData.value.push(item.data as PredictionDataPoint)
            }
        })

        // Trim old data points
        sensorTypes.forEach(type => {
            if (telemetryPoints.value[type]?.length > BUFFER_CONFIG.maxDataPoints) {
                telemetryPoints.value[type] = telemetryPoints.value[type].slice(-BUFFER_CONFIG.maxDataPoints)
            }
        })

        if (predictionData.value.length > BUFFER_CONFIG.maxDataPoints) {
            predictionData.value = predictionData.value.slice(-BUFFER_CONFIG.maxDataPoints)
        }

        lastUpdate.value = new Date()
    }

    // Process individual telemetry data point
    function processTelemetryData(data: TelemetryDataPoint): void {
        if (!telemetryPoints.value[data.sensorType]) {
            telemetryPoints.value[data.sensorType] = []
        }

        telemetryPoints.value[data.sensorType].push({
            ...data,
            timestamp: new Date(data.timestamp)
        })

        // Trim to max points
        if (telemetryPoints.value[data.sensorType].length > BUFFER_CONFIG.maxDataPoints) {
            telemetryPoints.value[data.sensorType].shift()
        }
    }

    // Add to buffer (called on each SignalR message)
    function addToBuffer(type: string, data: unknown): void {
        if (!enableBuffering) {
            // Process immediately
            if (type === 'telemetry') {
                processTelemetryData(data as TelemetryDataPoint)
            } else if (type === 'prediction') {
                predictionData.value.push(data as PredictionDataPoint)
            }
            lastUpdate.value = new Date()
            return
        }

        // Add to buffer
        chartDataBuffer.value.push({ type, data })

        // Set up buffer flush if not already set
        if (!bufferTimeout) {
            bufferTimeout = setTimeout(() => {
                processBuffer()
                bufferTimeout = null
            }, bufferInterval)
        }
    }

    // Subscribe to machine telemetry
    function subscribeToMachine(): void {
        signalRService.subscribeToMachine(machineId)
        isConnected.value = signalRService.isConnected.value
    }

    // Unsubscribe from machine
    function unsubscribeFromMachine(): void {
        signalRService.unsubscribeFromMachine(machineId)
    }

    // Watch for real-time telemetry updates
    function watchRealtimeData(): void {
        watch(() => currentRealtimeData.value, (newData) => {
            if (newData) {
                Object.entries(newData.sensors || {}).forEach(([sensorType, value]) => {
                    if (sensorTypes.includes(sensorType)) {
                        const dataPoint: TelemetryDataPoint = {
                            timestamp: new Date(newData.timestamp),
                            value: value as number,
                            machineId,
                            sensorType
                        }
                        addToBuffer('telemetry', dataPoint)
                    }
                })
            }
        }, { deep: true })
    }

    // Fetch historical data for charts
    async function fetchHistoricalData(sensorType: string, timeRangeMinutes: number): Promise<void> {
        isLoading.value = true
        try {
            // This would typically call an API endpoint
            // For now, we'll use the store's historical data if available
            const historical = telemetryStore.historicalData[machineId]
            if (historical) {
                telemetryPoints.value[sensorType] = historical
                    .filter(d => d.sensorType === sensorType)
                    .slice(-BUFFER_CONFIG.maxDataPoints)
            }
        } catch (error) {
            console.error('Error fetching historical data:', error)
        } finally {
            isLoading.value = false
        }
    }

    // Fetch prediction data
    async function fetchPredictionData(): Promise<void> {
        const rulPrediction = predictionsStore.rulPredictions[machineId]
        if (rulPrediction) {
            // Transform RUL prediction to chart format
            const dataPoint: PredictionDataPoint = {
                timestamp: new Date(rulPrediction.lastUpdated),
                predicted: rulPrediction.predictedRUL,
                observed: rulPrediction.currentRUL,
                confidenceLower: rulPrediction.confidenceLower,
                confidenceUpper: rulPrediction.confidenceUpper
            }
            addToBuffer('prediction', dataPoint)
        }
    }

    // Fetch sensor readings for radar chart
    async function fetchSensorReadings(): Promise<void> {
        const metrics = machinesStore.currentMetrics[machineId]
        if (metrics) {
            const readings: Record<string, SensorReading> = {}

            Object.entries(metrics).forEach(([key, value]) => {
                if (typeof value === 'number' && !['operatingHours'].includes(key)) {
                    const maxVal = getMaxValueForSensor(key)
                    readings[key] = {
                        sensor: key,
                        value,
                        min: 0,
                        max: maxVal,
                        threshold: getThresholdForSensor(key),
                        normalizedValue: Math.min((value / maxVal) * 100, 100)
                    }
                }
            })

            sensorReadings.value = readings
        }
    }

    // Helper: Get max value for sensor type
    function getMaxValueForSensor(sensorType: string): number {
        const maxValues: Record<string, number> = {
            temperature: 150, // °C
            vibration: 20, // mm/s
            pressure: 10, // bar
            humidity: 100, // %
            rpm: 5000
        }
        return maxValues[sensorType] || 100
    }

    // Helper: Get threshold for sensor type
    function getThresholdForSensor(sensorType: string): number {
        const thresholds: Record<string, number> = {
            temperature: 80,
            vibration: 10,
            pressure: 8,
            humidity: 70
        }
        return thresholds[sensorType] || 80
    }

    // Lifecycle hooks
    onMounted(() => {
        initializeTelemetryStorage()

        if (signalRService.isConnected.value) {
            subscribeToMachine()
        } else {
            // Wait for connection
            const unwatch = watch(() => signalRService.isConnected.value, (connected) => {
                if (connected) {
                    subscribeToMachine()
                    unwatch()
                }
            })
        }

        watchRealtimeData()
        fetchSensorReadings()
    })

    onUnmounted(() => {
        if (bufferTimeout) {
            clearTimeout(bufferTimeout)
        }
        if (updateInterval) {
            clearInterval(updateInterval)
        }
        unsubscribeFromMachine()
    })

    return {
        // State
        isConnected,
        isLoading,
        lastUpdate,

        // Data
        telemetryPoints,
        predictionData,
        degradationData,
        sensorReadings,

        // Computed
        temperatureData,
        vibrationData,
        pressureData,

        // Methods
        fetchHistoricalData,
        fetchPredictionData,
        fetchSensorReadings,
        subscribeToMachine,
        unsubscribeFromMachine,
        addToBuffer
    }
}

/**
 * Composable for single sensor real-time chart
 */
export function useRealtimeSensorChart(machineId: string, sensorType: string) {
    const {
        isConnected,
        isLoading,
        lastUpdate,
        telemetryPoints,
        fetchHistoricalData,
        subscribeToMachine,
        unsubscribeFromMachine
    } = useSignalRCharts({ machineId, sensorTypes: [sensorType] })

    const sensorData = computed(() => telemetryPoints.value[sensorType] || [])

    async function loadData(timeRangeMinutes: number = 60): Promise<void> {
        await fetchHistoricalData(sensorType, timeRangeMinutes)
    }

    return {
        isConnected,
        isLoading,
        lastUpdate,
        sensorData,
        loadData,
        subscribeToMachine,
        unsubscribeFromMachine
    }
}

/**
 * Composable for multi-sensor chart
 */
export function useMultiSensorChart(machineId: string, sensorTypes: string[]) {
    const charts = sensorTypes.map(type => ({
        type,
        ...useRealtimeSensorChart(machineId, type)
    }))

    return {
        charts,
        loadAll: async (timeRangeMinutes: number = 60) => {
            await Promise.all(charts.map(chart => chart.loadData(timeRangeMinutes)))
        }
    }
}
