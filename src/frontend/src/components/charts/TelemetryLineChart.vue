<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { useSignalRCharts } from '@/composables/useSignalRCharts'
import { signalRService } from '@/services/signalr'
import type { ApexOptions } from 'apexcharts'

interface Props {
    machineId: string
    sensorType: 'temperature' | 'vibration' | 'pressure' | 'humidity' | 'rpm'
    timeRange?: number // minutes
    showThresholds?: boolean
    height?: number | string
    width?: number | string
    title?: string
    color?: string
}

const props = withDefaults(defineProps<Props>(), {
    timeRange: 60,
    showThresholds: true,
    height: 350,
    width: '100%',
    color: '#3B82F6'
})

const { isConnected, isLoading, lastUpdate, sensorData, loadData, subscribeToMachine, unsubscribeFromMachine } = useRealtimeSensorChart(props.machineId, props.sensorType)

const series = ref<{ name: string; data: { x: Date; y: number }[] }[]>([])
const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'line',
        height: props.height,
        width: props.width,
        // PERFORMANCE: Disable animations for real-time streaming
        animations: {
            enabled: false
        },
        toolbar: {
            show: true,
            tools: {
                download: true,
                selection: true,
                zoom: true,
                zoomin: true,
                zoomout: true,
                pan: true,
                reset: true
            }
        },
        zoom: {
            enabled: true
        },
        background: 'transparent',
        fontFamily: 'inherit'
    },
    colors: [props.color],
    stroke: {
        curve: 'smooth',
        width: 2
    },
    title: {
        text: props.title || `${props.sensorType.charAt(0).toUpperCase() + props.sensorType.slice(1)} History`,
        align: 'left',
        style: {
            fontSize: '16px',
            fontWeight: 600
        }
    },
    xaxis: {
        type: 'datetime',
        labels: {
            style: {
                colors: '#6B7280'
            }
        },
        axisBorder: {
            color: '#E5E7EB'
        },
        axisTicks: {
            color: '#E5E7EB'
        }
    },
    yaxis: {
        labels: {
            style: {
                colors: '#6B7280'
            },
            formatter: (val: number) => val.toFixed(1)
        }
    },
    grid: {
        borderColor: '#E5E7EB',
        strokeDashArray: 4
    },
    theme: {
        mode: 'dark'
    },
    annotations: props.showThresholds ? {
        yaxis: [
            {
                y: getThresholdForSensor('warning'),
                borderColor: '#F59E0B',
                strokeDashArray: 5,
                label: {
                    text: 'Warning',
                    style: {
                        color: '#fff',
                        background: '#F59E0B'
                    }
                }
            },
            {
                y: getThresholdForSensor('critical'),
                borderColor: '#EF4444',
                strokeDashArray: 5,
                label: {
                    text: 'Critical',
                    style: {
                        color: '#fff',
                        background: '#EF4444'
                    }
                }
            }
        ]
    } : undefined,
    dataLabels: {
        enabled: false
    },
    tooltip: {
        theme: 'dark',
        x: {
            format: 'HH:mm:ss'
        },
        y: {
            formatter: (val: number) => `${val.toFixed(2)} ${getUnit(props.sensorType)}`
        }
    }
}))

function getThresholdForSensor(level: 'warning' | 'critical'): number {
    const thresholds: Record<string, { warning: number; critical: number }> = {
        temperature: { warning: 70, critical: 85 },
        vibration: { warning: 8, critical: 12 },
        pressure: { warning: 7, critical: 9 },
        humidity: { warning: 60, critical: 80 },
        rpm: { warning: 4000, critical: 4500 }
    }
    return thresholds[props.sensorType]?.[level] || 0
}

function getUnit(sensorType: string): string {
    const units: Record<string, string> = {
        temperature: '°C',
        vibration: 'mm/s',
        pressure: 'bar',
        humidity: '%',
        rpm: 'RPM'
    }
    return units[sensorType] || ''
}

// Watch for data changes - use shallow watch for performance
watch(sensorData, () => {
    series.value = [{
        name: props.sensorType,
        data: sensorData.value.map((d: { timestamp: string | Date; value: number }) => ({
            x: new Date(d.timestamp),
            y: d.value
        }))
    }]
}, { deep: false })

// Initial data load
onMounted(async () => {
    if (!signalRService.isConnected.value) {
        await signalRService.connect()
    }
    subscribeToMachine()
    await loadData(props.timeRange)
})

onUnmounted(() => {
    unsubscribeFromMachine()
})

// Expose refresh method
defineExpose({
    refresh: () => loadData(props.timeRange)
})
</script>

<template>
    <div class="telemetry-line-chart">
        <div v-if="!isConnected && !isLoading" class="flex items-center justify-center p-4 text-yellow-600">
            <svg class="animate-spin h-5 w-5 mr-2" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Connecting to real-time data...
        </div>

        <div v-else-if="isLoading" class="flex items-center justify-center p-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500" />
        </div>

        <VueApexCharts
            v-else
            type="line"
            height="350"
            :options="chartOptions"
            :series="series"
        />

        <div v-if="lastUpdate" class="text-xs text-gray-500 mt-2 text-right">
            Last updated: {{ lastUpdate.toLocaleTimeString() }}
        </div>
    </div>
</template>
