// RUL Prediction types expanded for UI components
export interface RULPrediction {
    machineId: string
    machineName?: string
    currentRUL: number
    predictedRUL: number
    confidenceLower: number
    confidenceUpper: number
    confidenceScore: number
    degradationRate: number
    trend: 'improving' | 'declining' | 'stable'
    estimatedFailureDate: string | Date
    modelType: string
    modelVersion: string
    lastUpdated: string | Date
    healthScore?: number
}

export interface PredictionHistory {
    machineId: string
    predictions: {
        timestamp: string | Date
        rul: number
        confidenceLower: number
        confidenceUpper: number
    }[]
}

export interface FeatureImportance {
    feature: string
    importance: number
    direction: 'positive' | 'negative'
    shapValue: number
}

export interface PredictionDetail {
    machineId: string
    machineName?: string
    currentPrediction: RULPrediction
    history: PredictionHistory
    featureImportance: FeatureImportance[]
    modelMetrics: {
        accuracy: number
        precision: number
        recall: number
        f1Score: number
        mae: number
        rmse: number
    }
    contributingFactors: {
        factor: string
        impact: number
        value: number
        unit: string
    }[]
}

export interface WhatIfScenario {
    id: string
    name: string
    description: string
    parameters: {
        temperature?: number
        vibration?: number
        pressure?: number
        load?: number
        operatingHours?: number
    }
    predictedRUL: number
    confidenceLower: number
    confidenceUpper: number
    maintenanceRecommendation?: string
    estimatedImpact: {
        rulChange: number
        percentChange: number
        newFailureDate: string | Date
    }
}

export interface WhatIfComparison {
    baseline: RULPrediction
    scenarios: WhatIfScenario[]
    bestScenario?: WhatIfScenario
    recommendation: string
}

export interface TimelineDataPoint {
    timestamp: string | Date
    rul: number
    failureProbability: number
    confidenceLower?: number
    confidenceUpper?: number
    maintenanceWindow?: {
        start: string | Date
        end: string | Date
        recommendation: string
    }
}

export interface RULTimeline {
    machineId: string
    timeHorizon: number // days
    data: TimelineDataPoint[]
    maintenanceRecommendations: {
        date: string | Date
        type: 'optimal' | 'minimum' | 'emergency'
        estimatedRUL: number
        description: string
    }[]
}
