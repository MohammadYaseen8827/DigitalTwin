import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/services/api'
import type { Prediction, RULPrediction, AnomalyPrediction } from '@/types'

/** Backend RUL response (POST api/predictions/rul/{machineId}) */
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

export const usePredictionsStore = defineStore('predictions', () => {
    const predictions = ref<Prediction[]>([])
    const rulPredictions = ref<Record<string, RULPrediction>>({})
    const anomalyPredictions = ref<Record<string, AnomalyPrediction>>({})
    const isLoading = ref(false)
    const error = ref<string | null>(null)

    const criticalPredictions = computed(() =>
        predictions.value.filter(p => p.result === 'Critical' || p.result === 'Failure')
    )

    /** No backend list endpoint yet; keep predictions from SignalR/other sources only. */
    async function fetchPredictions(_machineId?: string): Promise<void> {
        isLoading.value = true
        error.value = null
        try {
            // Backend does not expose GET /api/predictions; do not call it.
            predictions.value = []
        } finally {
            isLoading.value = false
        }
    }

    async function fetchRULPrediction(machineId: string): Promise<RULPrediction | null> {
        try {
            const response = await api.post<RulPredictionResult>(
                `/api/predictions/rul/${machineId}`,
                {}
            )
            const mapped = mapRulResultToFrontend(response.data)
            rulPredictions.value[machineId] = mapped
            return mapped
        } catch (err) {
            console.error('Error fetching RUL prediction:', err)
            return null
        }
    }

    /** No backend GET anomaly endpoint yet; return null until implemented. */
    async function fetchAnomalyPrediction(_machineId: string): Promise<AnomalyPrediction | null> {
        return null
    }

    function addPrediction(prediction: Prediction): void {
        predictions.value.unshift(prediction)
        if (predictions.value.length > 100) {
            predictions.value.pop()
        }
    }

    function updateRULPrediction(machineId: string, prediction: RULPrediction): void {
        rulPredictions.value[machineId] = prediction
    }

    function updateAnomalyPrediction(machineId: string, prediction: AnomalyPrediction): void {
        anomalyPredictions.value[machineId] = prediction
    }

    function clearPredictions(): void {
        predictions.value = []
    }

    return {
        predictions,
        rulPredictions,
        anomalyPredictions,
        isLoading,
        error,
        criticalPredictions,
        fetchPredictions,
        fetchRULPrediction,
        fetchAnomalyPrediction,
        addPrediction,
        updateRULPrediction,
        updateAnomalyPrediction,
        clearPredictions
    }
})
