<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { useMachinesStore } from '@/stores/machines'
import type { ApexOptions } from 'apexcharts'
import type { SensorReading } from '@/types'

interface Props {
    machineId: string
    height?: number | string
    width?: number | string
    showThresholds?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    height: 400,
    width: '100%',
    showThresholds: true
})

const machinesStore = useMachinesStore()
const isLoading = ref(true)
const sensorReadings = ref<Record<string, SensorReading>>({})

const series = computed(() => [{
    name: 'Sensor Value',
    data: Object.values(sensorReadings.value).map(sensor => ({
        x: sensor.sensor,
        y: sensor.normalizedValue
    }))
}])

const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'radar',
        height: props.height,
        fontFamily: 'inherit',
        toolbar: {
            show: true,
            tools: {
                download: true,
                zoom: true,
                reset: true
            }
        },
        animations: {
            enabled: true,
            speed: 800
        }
    },
    colors: ['#3B82F6'],
    fill: {
        opacity: 0.4
    },
    stroke: {
        width: 2
    },
    title: {
        text: 'Sensor Distribution',
        align: 'left',
        style: {
            fontSize: '16px',
            fontWeight: 600
        }
    },
    xaxis: {
        categories: Object.keys(sensorReadings.value),
        labels: {
            style: {
                colors: '#6B7280',
                fontSize: '12px'
            }
        }
    },
    yaxis: {
        show: false,
        min: 0,
        max: 100
    },
    grid: {
        background: '#F3F4F6',
        borderColor: '#E5E7EB',
        borderWidth: 1,
        strokeDashArray: 4
    },
    theme: {
        mode: 'dark'
    },
    markers: {
        size: 4,
        colors: ['#3B82F6'],
        strokeColors: '#fff',
        strokeWidth: 2,
        hover: {
            size: 6
        }
    },
    tooltip: {
        y: {
            formatter: (val: number, opts: { dataPointIndex: number }) => {
                const sensor = Object.values(sensorReadings.value)[opts.dataPointIndex]
                if (sensor) {
                    return `${val.toFixed(1)}% (${sensor.value.toFixed(2)} ${getUnit(sensor.sensor)})`
                }
                return `${val}%`
            }
        }
    },
    plotOptions: {
        radar: {
            polygons: {
                strokeColors: '#E5E7EB',
                strokeWidth: 1,
                fill: {
                    colors: ['transparent', 'transparent']
                }
            }
        }
    }
}))

// Add threshold circles if enabled
const chartOptionsWithThresholds = computed<ApexOptions>(() => ({
    ...chartOptions.value,
    annotations: props.showThresholds ? {
        points: Object.entries(sensorReadings.value).map(([name, sensor]) => ({
            x: name,
            y: (sensor.normalizedValue / 100) * 100,
            marker: {
                size: sensor.normalizedValue > sensor.threshold ? 8 : 0,
                fillColor: sensor.normalizedValue > sensor.threshold ? '#EF4444' : undefined,
                strokeColor: '#fff'
            },
            label: {
                text: sensor.normalizedValue > sensor.threshold ? '⚠️' : '',
                offsetY: -20
            }
        }))
    } : undefined
}))

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

function getThresholdColor(value: number, threshold: number): string {
    if (value > threshold) return '#EF4444'
    if (value > threshold * 0.8) return '#F59E0B'
    return '#10B981'
}

async function fetchSensorReadings() {
    isLoading.value = true
    try {
        await machinesStore.fetchMachineMetrics(props.machineId)
        updateSensorReadings()
    } catch (error) {
        console.error('Error fetching sensor readings:', error)
        sensorReadings.value = {}
    } finally {
        isLoading.value = false
    }
}

function updateSensorReadings() {
    const metrics = machinesStore.currentMetrics[props.machineId]
    if (metrics) {
        const readings: Record<string, SensorReading> = {}
        
        Object.entries(metrics).forEach(([key, value]) => {
            if (typeof value === 'number' && !['operatingHours'].includes(key)) {
                const maxVal = getMaxValue(key)
                readings[key] = {
                    sensor: key,
                    value,
                    min: 0,
                    max: maxVal,
                    threshold: getThreshold(key),
                    normalizedValue: Math.min((value / maxVal) * 100, 100)
                }
            }
        })
        
        sensorReadings.value = readings
    }
}


function getMaxValue(sensorType: string): number {
    const maxValues: Record<string, number> = {
        temperature: 150,
        vibration: 20,
        pressure: 10,
        humidity: 100,
        rpm: 5000
    }
    return maxValues[sensorType] || 100
}

function getThreshold(sensorType: string): number {
    const thresholds: Record<string, number> = {
        temperature: 80,
        vibration: 10,
        pressure: 8,
        humidity: 70,
        rpm: 4000
    }
    return thresholds[sensorType] || 80
}

watch(() => machinesStore.currentMetrics[props.machineId], () => {
    updateSensorReadings()
}, { deep: true })

onMounted(() => {
    fetchSensorReadings()
})
</script>

<template>
    <div class="sensor-distribution-chart">
        <div v-if="isLoading" class="flex items-center justify-center p-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500" />
        </div>

        <VueApexCharts
            v-else
            type="radar"
            :height="height"
            :width="width"
            :options="chartOptionsWithThresholds"
            :series="series"
        />

        <!-- Legend with threshold status -->
        <div v-if="Object.keys(sensorReadings).length > 0" class="mt-4 flex flex-wrap gap-4 justify-center text-sm">
            <div v-for="sensor in Object.values(sensorReadings)" :key="sensor.sensor" class="flex items-center">
                <span 
                    class="w-3 h-3 rounded-full mr-2"
                    :style="{ backgroundColor: getThresholdColor(sensor.normalizedValue, (sensor.threshold / sensor.max) * 100) }"
                />
                <span class="capitalize">{{ sensor.sensor }}: {{ sensor.normalizedValue.toFixed(1) }}%</span>
            </div>
        </div>
    </div>
</template>
