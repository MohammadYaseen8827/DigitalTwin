<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { api } from '@/services/api'
import type { ApexOptions } from 'apexcharts'
import type { HealthTrendDataPoint, MaintenanceEvent } from '@/types'

interface Props {
    machineId: string
    days?: number
    height?: number | string
    width?: number | string
}

const props = withDefaults(defineProps<Props>(), {
    days: 30,
    height: 350,
    width: '100%'
})

const isLoading = ref(true)
const healthData = ref<HealthTrendDataPoint[]>([])
const maintenanceEvents = ref<MaintenanceEvent[]>([])

const series = computed(() => [{
    name: 'Health Score',
    data: healthData.value.map(d => ({
        x: new Date(d.timestamp),
        y: d.healthScore
    }))
}])

const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'area',
        height: props.height,
        fontFamily: 'inherit',
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
        animations: {
            enabled: true,
            easing: 'easeinout',
            speed: 800
        }
    },
    colors: ['#10B981'],
    fill: {
        type: 'gradient',
        gradient: {
            shadeIntensity: 1,
            opacityFrom: 0.4,
            opacityTo: 0.1,
            stops: [0, 90, 100]
        }
    },
    stroke: {
        curve: 'smooth',
        width: 2
    },
    title: {
        text: 'Health Score Trend',
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
            },
            datetimeFormatter: {
                year: 'yyyy',
                month: 'MMM \'yy',
                day: 'dd MMM',
                hour: 'HH:mm'
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
        min: 0,
        max: 100,
        labels: {
            style: {
                colors: '#6B7280'
            },
            formatter: (val: number) => `${val}%`
        }
    },
    grid: {
        borderColor: '#E5E7EB',
        strokeDashArray: 4
    },
    theme: {
        mode: 'light'
    },
    annotations: {
        yaxis: [
            {
                y: 70,
                borderColor: '#F59E0B',
                strokeDashArray: 5,
                label: {
                    text: 'Warning threshold',
                    style: {
                        color: '#fff',
                        background: '#F59E0B'
                    }
                }
            },
            {
                y: 50,
                borderColor: '#EF4444',
                strokeDashArray: 5,
                label: {
                    text: 'Critical threshold',
                    style: {
                        color: '#fff',
                        background: '#EF4444'
                    }
                }
            }
        ],
        xaxis: maintenanceEvents.value.map(event => ({
            x: new Date(event.timestamp).getTime(),
            label: {
                text: `${event.type}: ${event.description}`,
                style: {
                    color: '#fff',
                    background: getEventColor(event.severity)
                }
            }
        }))
    },
    dataLabels: {
        enabled: false
    },
    tooltip: {
        x: {
            format: 'MMM dd, yyyy HH:mm'
        },
        y: {
            formatter: (val: number) => `${val.toFixed(1)}%`
        }
    }
}))

function getEventColor(severity: string): string {
    const colors: Record<string, string> = {
        low: '#10B981',
        medium: '#F59E0B',
        high: '#EF4444'
    }
    return colors[severity] || '#6B7280'
}

async function fetchHealthData() {
    isLoading.value = true
    try {
        const response = await api.get<HealthTrendDataPoint[]>(`/api/Health/trend/${props.machineId}`, {
            params: { days: props.days }
        })
        healthData.value = response.data
    } catch (error) {
        console.error('Error fetching health trend data:', error)
        generateMockData()
    } finally {
        isLoading.value = false
    }
}

async function fetchMaintenanceEvents() {
    try {
        const response = await api.get<MaintenanceEvent[]>(`/api/Maintenance/${props.machineId}`)
        maintenanceEvents.value = response.data
    } catch (error) {
        console.error('Error fetching maintenance events:', error)
    }
}

/** Set to true when showing fallback sample data (no real API data). */
const isSampleData = ref(false)

function generateMockData() {
    isSampleData.value = true
    const now = new Date()
    const data: HealthTrendDataPoint[] = []
    let health = 95

    for (let i = props.days * 24; i >= 0; i--) {
        const timestamp = new Date(now.getTime() - i * 3600000)
        health = Math.max(30, Math.min(100, health + (Math.random() - 0.5) * 5))
        
        if (i % 168 === 0) {
            health = Math.min(100, health + 10)
        } else if (i % 72 === 0) {
            health = health - 5
        }
        
        data.push({
            timestamp,
            healthScore: Math.round(health),
            machineId: props.machineId
        })
    }
    
    healthData.value = data
}

onMounted(() => {
    fetchHealthData()
    fetchMaintenanceEvents()
})
</script>

<template>
    <div class="health-trend-chart">
        <div v-if="isLoading" class="flex items-center justify-center p-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-green-500" />
        </div>

        <VueApexCharts
            v-else
            type="area"
            :height="height"
            :width="width"
            :options="chartOptions"
            :series="series"
        />

        <p v-if="isSampleData && !isLoading" class="mt-2 text-xs text-gray-500 italic">
            Sample data — no API data available
        </p>

        <div v-if="maintenanceEvents.length > 0" class="mt-4 flex gap-4 text-sm">
            <div class="flex items-center">
                <span class="w-3 h-3 rounded-full bg-green-500 mr-2" />
                <span>Healthy (≥70%)</span>
            </div>
            <div class="flex items-center">
                <span class="w-3 h-3 rounded-full bg-yellow-500 mr-2" />
                <span>Warning (50-70%)</span>
            </div>
            <div class="flex items-center">
                <span class="w-3 h-3 rounded-full bg-red-500 mr-2" />
                <span>Critical (<50%)</span>
            </div>
        </div>
    </div>
</template>
