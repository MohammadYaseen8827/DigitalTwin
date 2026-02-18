<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { usePredictionsStore } from '@/stores/predictions'
import type { ApexOptions } from 'apexcharts'

interface Props {
    machineId: string
    currentRul?: number
    predictedRul?: number
    threshold?: number
    showConfidence?: boolean
    height?: number | string
    width?: number | string
}

const props = withDefaults(defineProps<Props>(), {
    currentRul: 0,
    predictedRul: 0,
    threshold: 20,
    showConfidence: true,
    height: 300,
    width: '100%'
})

const predictionsStore = usePredictionsStore()

const rulData = computed(() => predictionsStore.rulPredictions[props.machineId])

const gaugeValue = computed(() => {
    if (props.currentRul > 0) return props.currentRul
    if (rulData.value) return rulData.value.currentRUL
    return 0
})

const predictedValue = computed(() => {
    if (props.predictedRul > 0) return props.predictedRul
    if (rulData.value) return rulData.value.predictedRUL
    return 0
})

const confidenceLower = computed(() => {
    if (rulData.value) return rulData.value.confidenceLower
    return Math.max(0, gaugeValue.value - 10)
})

const confidenceUpper = computed(() => {
    if (rulData.value) return rulData.value.confidenceUpper
    return gaugeValue.value + 10
})

const healthScore = computed(() => {
    const maxRUL = 100 // Assume 100% health at 100 RUL
    return Math.min((gaugeValue.value / maxRUL) * 100, 100)
})

const gaugeColor = computed(() => {
    if (healthScore.value >= 70) return '#10B981' // Green
    if (healthScore.value >= 40) return '#F59E0B' // Yellow
    return '#EF4444' // Red
})

const series = computed(() => [healthScore.value])

const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'radialBar',
        height: props.height,
        fontFamily: 'inherit'
    },
    plotOptions: {
        radialBar: {
            startAngle: -135,
            endAngle: 135,
            hollow: {
                margin: 15,
                size: '60%',
                background: 'transparent'
            },
            track: {
                background: '#E5E7EB',
                strokeWidth: '100%'
            },
            dataLabels: {
                name: {
                    fontSize: '14px',
                    color: '#6B7280',
                    offsetY: 60
                },
                value: {
                    fontSize: '28px',
                    fontWeight: 700,
                    color: '#111827',
                    offsetY: -10,
                    formatter: (val: number) => `${val.toFixed(0)}%`
                },
                total: {
                    show: true,
                    label: 'RUL',
                    fontSize: '14px',
                    color: '#6B7280',
                    formatter: () => `${gaugeValue.value.toFixed(0)} hrs`
                }
            }
        }
    },
    colors: [gaugeColor.value],
    fill: {
        type: 'solid',
        gradient: {
            shade: 'dark',
            shadeIntensity: 0.4,
            inverseColors: false,
            opacityFrom: 1,
            opacityTo: 1,
            stops: [0, 50, 65, 91]
        }
    },
    stroke: {
        lineCap: 'round'
    },
    labels: ['Remaining Useful Life'],
    title: {
        text: 'Remaining Useful Life (RUL)',
        align: 'center',
        style: {
            fontSize: '16px',
            fontWeight: 600
        }
    },
    subtitle: {
        text: props.showConfidence ? `Confidence: ${confidenceLower.value.toFixed(0)} - ${confidenceUpper.value.toFixed(0)} hrs` : '',
        align: 'center',
        style: {
            fontSize: '12px',
            color: '#6B7280'
        }
    }
}))

// Threshold indicator
const thresholdLine = computed(() => ({
    y: (props.threshold / 100) * 100,
    borderColor: '#EF4444',
    strokeDashArray: 5,
    label: {
        text: `Threshold: ${props.threshold}%`,
        style: {
            color: '#fff',
            background: '#EF4444'
        }
    }
}))
</script>

<template>
    <div class="rul-gauge-chart">
        <div v-if="gaugeValue === 0" class="flex items-center justify-center p-8 text-gray-500">
            <svg class="animate-spin h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Loading RUL prediction...
        </div>

        <VueApexCharts
            v-else
            type="radialBar"
            :height="height"
            :width="width"
            :options="chartOptions"
            :series="series"
        />

        <!-- Status indicator -->
        <div v-if="gaugeValue > 0" class="mt-4 flex justify-center">
            <span
                :class="[
                    'px-3 py-1 rounded-full text-sm font-medium',
                    healthScore >= 70 ? 'bg-green-100 text-green-800' :
                    healthScore >= 40 ? 'bg-yellow-100 text-yellow-800' :
                    'bg-red-100 text-red-800'
                ]"
            >
                {{ healthScore >= 70 ? 'Healthy' : healthScore >= 40 ? 'Warning' : 'Critical' }}
            </span>
        </div>

        <!-- Prediction vs Current -->
        <div v-if="predictedValue > 0 && showConfidence" class="mt-4 grid grid-cols-3 gap-4 text-center">
            <div>
                <div class="text-xs text-gray-500">Current RUL</div>
                <div class="text-lg font-semibold">{{ gaugeValue.toFixed(0) }}h</div>
            </div>
            <div>
                <div class="text-xs text-gray-500">Predicted</div>
                <div class="text-lg font-semibold">{{ predictedValue.toFixed(0) }}h</div>
            </div>
            <div>
                <div class="text-xs text-gray-500">Confidence</div>
                <div class="text-lg font-semibold text-blue-600">{{ ((gaugeValue - confidenceLower) / (confidenceUpper - confidenceLower) * 100).toFixed(0) }}%</div>
            </div>
        </div>
    </div>
</template>
