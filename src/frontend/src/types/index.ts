// Main types index for Digital Twin Platform
export * from './chart'

// Re-export types used in stores and services
export type MachineStatus = 'Running' | 'Stopped' | 'Maintenance' | 'Error' | 'Offline'

export interface Machine {
    id: string
    name: string
    type: string
    location: string
    status: MachineStatus
    healthScore: number
    lastMaintenanceDate: string | Date
    nextMaintenanceDate: string | Date
    installDate: string | Date
    specifications: Record<string, string>
}

export interface TelemetryData {
    machineId: string
    timestamp: string | Date
    sensors: {
        temperature: number
        vibration: number
        pressure: number
        [key: string]: number
    }
}

export interface TelemetryMetrics {
    machineId: string
    temperature: number
    vibration: number
    pressure: number
    humidity?: number
    powerConsumption?: number
    rpm?: number
    operatingHours?: number
    lastUpdated: string | Date
}

export interface TelemetryDataPoint {
    timestamp: string | Date
    value: number
    machineId: string
    sensorType: string
}

export interface DashboardStats {
    totalMachines: number
    runningMachines: number
    maintenanceMachines: number
    errorMachines: number
    averageHealthScore: number
    activeAlerts: number
    pendingMaintenance: number
}

export interface Alert {
    id: string
    machineId: string
    title: string
    description: string
    severity: 'info' | 'warning' | 'critical' | 'error'
    status: 'active' | 'acknowledged' | 'resolved'
    timestamp: string | Date
    acknowledgedBy?: string
    resolvedAt?: string | Date
    relatedPredictionId?: string
}

export interface Prediction {
    id: string
    machineId: string
    type: 'RUL' | 'anomaly' | 'failure' | 'performance'
    result: string
    confidence: number
    timestamp: string | Date
    modelVersion: string
    factors?: Record<string, number>
}

export interface RULPrediction {
    machineId: string
    currentRUL: number
    predictedRUL: number
    confidenceLower: number
    confidenceUpper: number
    degradationRate: number
    estimatedFailureDate: string | Date
    modelType: string
    modelVersion: string
    lastUpdated: string | Date
}

export interface AnomalyPrediction {
    machineId: string
    isAnomaly: boolean
    anomalyScore: number
    affectedSensors: string[]
    description: string
    recommendedActions: string[]
    lastUpdated: string | Date
}
