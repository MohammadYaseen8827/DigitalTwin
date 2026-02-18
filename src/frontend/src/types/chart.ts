// Chart-related types for Digital Twin Platform

// Telemetry data point for real-time charts
export interface TelemetryDataPoint {
    timestamp: string | Date
    value: number
    machineId: string
    sensorType: string
}

// Multiple series telemetry data
export interface TelemetrySeries {
    name: string
    data: { x: string | Date; y: number }[]
    color?: string
}

// Threshold configuration for charts
export interface ThresholdConfig {
    warning: number
    critical: number
    warningColor?: string
    criticalColor?: string
}

// RUL Gauge configuration
export interface RULGaugeData {
    currentRUL: number
    predictedRUL: number
    confidenceLower: number
    confidenceUpper: number
    threshold: number
    unit?: string
}

// Health trend data point
export interface HealthTrendDataPoint {
    timestamp: string | Date
    healthScore: number
    machineId: string
}

// Maintenance event annotation
export interface MaintenanceEvent {
    id: string
    timestamp: string | Date
    type: 'repair' | 'inspection' | 'part_replacement' | 'calibration'
    description: string
    severity: 'low' | 'medium' | 'high'
}

// Prediction vs Actual data
export interface PredictionDataPoint {
    timestamp: string | Date
    predicted: number
    observed: number
    confidenceLower?: number
    confidenceUpper?: number
}

// Sensor reading for radar chart
export interface SensorReading {
    sensor: string
    value: number
    min: number
    max: number
    threshold: number
    normalizedValue: number // 0-100 scale
}

// Alert for timeline chart
export interface AlertTimelineItem {
    id: string
    timestamp: string | Date
    severity: 'info' | 'warning' | 'critical' | 'error'
    title: string
    machineId: string
    acknowledged: boolean
}

// Degradation model data
export interface DegradationModelData {
    timestamp: string | Date
    degradationLevel: number
    observedWear: number
    confidenceLower: number
    confidenceUpper: number
}

// Degradation model types
export type DegradationModelType = 'wiener' | 'exponential' | 'markov' | 'physics_informed'

// Chart configuration
export interface ChartConfig {
    theme: 'light' | 'dark'
    animations: boolean
    exportEnabled: boolean
    responsive: boolean
    height?: number | string
    width?: number | string
}

// Time range options
export type TimeRange = 5 | 10 | 15 | 30 | 60 | 120 | 240 | 480 | 1440 // minutes

// Chart update buffer settings
export interface BufferConfig {
    enabled: boolean
    maxUpdatesPerSecond: number
    maxDataPoints: number
}
