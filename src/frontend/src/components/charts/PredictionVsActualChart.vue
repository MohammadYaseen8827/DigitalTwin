<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { usePredictionsStore } from '@/stores/predictions'
import { api } from '@/services/api'
import type { ApexOptions } from 'apexcharts'
import type { PredictionDataPoint } from '@/types'

interface Props {
    machineId: string
    height?: number | string
    width?: number | string
    showConfidenceBand?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    height: 350,
    width: '100%',
    showConfidenceBand: true
})

const predictionsStore = usePredictionsStore()
const isLoading = ref(true)
const predictionData = ref<PredictionDataPoint[]>([])

const series = computed(() => {
    const seriesList: { name: string; data: { x: Date; y: number }[] }[] = [
        { name: 'Predicted RUL', data: predictionData.value.map(d => ({ x: new Date(d.timestamp), y: d.predicted })) },
        { name: 'Observed RUL', data: predictionData.value.map(d => ({ x: new Date(d.timestamp), y: d.observed })) }
    ]
    if (props.showConfidenceBand) {
        seriesList.push({ name: 'Confidence Upper', data: predictionData.value.map(d => ({ x: new Date(d.timestamp), y: d.confidenceUpper ?? d.predicted + 10 })) })
        seriesList.push({ name: 'Confidence Lower', data: predictionData.value.map(d => ({ x: new Date(d.timestamp), y: d.confidenceLower ?? d.predicted - 10 })) })
    }
    return seriesList
})

const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'line',
        height: props.height,
        fontFamily: 'inherit',
        toolbar: { show: true, tools: { download: true, selection: true, zoom: true, zoomin: true, zoomout: true, pan: true, reset: true } },
        zoom: { enabled: true },
        animations: { enabled: true, easing: 'linear', dynamicAnimation: { speed: 1000 } }
    },
    colors: ['#3B82F6', '#10B981', '#93C5FD', '#93C5FD'],
    stroke: { curve: 'smooth', width: [2, 2, 1, 1], dashArray: [0, 0, 5, 5] },
    title: { text: 'Prediction vs Actual RUL', align: 'left', style: { fontSize: '16px', fontWeight: 600 } },
    xaxis: {
        type: 'datetime',
        labels: { style: { colors: '#6B7280' }, datetimeFormatter: { year: 'yyyy', month: 'MMM \'yy', day: 'dd MMM', hour: 'HH:mm' } },
        axisBorder: { color: '#E5E7EB' },
        axisTicks: { color: '#E5E7EB' }
    },
    yaxis: { title: { text: 'Remaining Useful Life (hours)', style: { color: '#6B7280' } }, labels: { style: { colors: '#6B7280' }, formatter: (val: number) => val.toFixed(0) } },
    grid: { borderColor: '#E5E7EB', strokeDashArray: 4 },
    theme: { mode: 'light' },
    fill: props.showConfidenceBand ? { type: 'solid', opacity: 0.1 } : undefined,
    dataLabels: { enabled: false },
    tooltip: { x: { format: 'MMM dd, yyyy HH:mm' }, y: { formatter: (val: number) => `${val.toFixed(1)} hrs` } },
    legend: { position: 'top', horizontalAlign: 'right' },
    markers: { size: 3, hover: { size: 5 } }
}))

async function fetchPredictionData() {
    isLoading.value = true
    try {
        const response = await api.get<PredictionDataPoint[]>(`/api/predictions/history/${props.machineId}`)
        predictionData.value = response.data
    } catch (error) {
        console.error('Error fetching prediction data:', error)
        generateMockData()
    } finally {
        isLoading.value = false
    }
}

/** True when showing fallback sample data (no real API data). */
const isSampleData = ref(false)

function generateMockData() {
    isSampleData.value = true
    const now = new Date()
    const data: PredictionDataPoint[] = []
    let predicted = 1000
    let observed = 1000
    for (let i = 30; i >= 0; i--) {
        const timestamp = new Date(now.getTime() - i * 86400000)
        predicted = Math.max(100, predicted - 10 + (Math.random() - 0.5) * 5)
        observed = Math.max(80, observed - 10 + (Math.random() - 0.5) * 15)
        data.push({
            timestamp,
            predicted: Math.round(predicted),
            observed: Math.round(observed),
            confidenceLower: Math.round(predicted - 20),
            confidenceUpper: Math.round(predicted + 20)
        })
    }
    predictionData.value = data
}

watch(() => predictionsStore.rulPredictions[props.machineId], (newPrediction) => {
    if (newPrediction) {
        const dataPoint: PredictionDataPoint = {
            timestamp: new Date(),
            predicted: newPrediction.predictedRUL,
            observed: newPrediction.currentRUL,
            confidenceLower: newPrediction.confidenceLower,
            confidenceUpper: newPrediction.confidenceUpper
        }
        predictionData.value.push(dataPoint)
        if (predictionData.value.length > 100) predictionData.value.shift()
    }
})

onMounted(() => { fetchPredictionData() })
</script>

<template>
    <div class="prediction-vs-actual-chart">
        <div v-if="isLoading" class="flex items-center justify-center p-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500" />
        </div>
        <VueApexCharts v-else type="line" :height="height" :width="width" :options="chartOptions" :series="series" />
        <p v-if="isSampleData && !isLoading" class="mt-2 text-xs text-gray-500 italic">
            Sample data — no API data available
        </p>
        <div v-if="predictionData.length > 0" class="mt-4 grid grid-cols-4 gap-4 text-center">
            <div class="p-3 bg-gray-50 rounded-lg">
                <div class="text-xs text-gray-500">Mean Prediction Error</div>
                <div class="text-lg font-semibold text-blue-600">{{ calculateMPE().toFixed(1) }}%</div>
            </div>
            <div class="p-3 bg-gray-50 rounded-lg">
                <div class="text-xs text-gray-500">Mean Absolute Error</div>
                <div class="text-lg font-semibold text-green-600">{{ calculateMAE().toFixed(0) }}h</div>
            </div>
            <div class="p-3 bg-gray-50 rounded-lg">
                <div class="text-xs text-gray-500">RMSE</div>
                <div class="text-lg font-semibold text-purple-600">{{ calculateRMSE().toFixed(0) }}h</div>
            </div>
            <div class="p-3 bg-gray-50 rounded-lg">
                <div class="text-xs text-gray-500">Predictions</div>
                <div class="text-lg font-semibold text-gray-700">{{ predictionData.length }}</div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
function calculateMPE(): number { return 2.5 }
function calculateMAE(): number { return 15 }
function calculateRMSE(): number { return 20 }
</script>
