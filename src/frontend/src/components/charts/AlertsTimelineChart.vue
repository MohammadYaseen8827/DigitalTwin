<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import VueApexCharts from 'vue3-apexcharts'
import { signalRService } from '@/services/signalr'
import { api } from '@/services/api'
import type { ApexOptions } from 'apexcharts'
import type { AlertTimelineItem } from '@/types'

interface Props {
    machineId?: string
    days?: number
    height?: number | string
    width?: number | string
}

const props = withDefaults(defineProps<Props>(), {
    days: 7,
    height: 350,
    width: '100%'
})

const isLoading = ref(true)
const alerts = ref<AlertTimelineItem[]>([])

const series = computed(() => [{
    name: 'Alerts',
    data: alerts.value.map(alert => ({
        x: alert.machineId,
        y: [
            new Date(alert.timestamp).getTime(),
            new Date(alert.timestamp).getTime() + 3600000
        ],
        fillColor: getSeverityColor(alert.severity),
        meta: alert
    }))
}])

const chartOptions = computed<ApexOptions>(() => ({
    chart: {
        type: 'rangeBar',
        height: props.height,
        fontFamily: 'inherit',
        toolbar: {
            show: true,
            tools: {
                download: true,
                selection: false,
                zoom: false,
                pan: false,
                reset: true
            }
        },
        animations: {
            enabled: true,
            speed: 800
        }
    },
    colors: ['#3B82F6'],
    plotOptions: {
        bar: {
            horizontal: true,
            distributed: true,
            dataLabels: {
                hideOverflowingLabels: false
            }
        }
    },
    title: {
        text: 'Alerts Timeline',
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
        axisBorder: { color: '#E5E7EB' },
        axisTicks: { color: '#E5E7EB' }
    },
    yaxis: { show: false },
    grid: {
        borderColor: '#E5E7EB',
        strokeDashArray: 4,
        row: { colors: ['#F9FAFB', 'transparent'], opacity: 0.5 }
    },
    theme: { mode: 'dark' },
    dataLabels: {
        enabled: true,
        formatter: (_val: number, opts: { dataPointIndex: number }) => {
            const alert = alerts.value[opts.dataPointIndex]
            return alert ? `${alert.title.substring(0, 20)}...` : ''
        },
        style: { colors: ['#fff'], fontWeight: 'bold' }
    },
    legend: { show: true, position: 'top', horizontalAlign: 'right' }
}))

function getSeverityColor(severity: string): string {
    const colors: Record<string, string> = {
        info: '#3B82F6',
        warning: '#F59E0B',
        critical: '#EF4444',
        error: '#DC2626'
    }
    return colors[severity] || '#6B7280'
}

async function fetchAlerts() {
    isLoading.value = true
    try {
        const url = props.machineId 
            ? `/api/Alerts?machineId=${props.machineId}&days=${props.days}`
            : `/api/Alerts?days=${props.days}`
        const response = await api.get<AlertTimelineItem[]>(url)
        alerts.value = response.data
    } catch (error) {
        console.error('Error fetching alerts:', error)
        alerts.value = []
    } finally {
        isLoading.value = false
    }
}


onMounted(() => {
    fetchAlerts()
})
</script>

<template>
    <div class="alerts-timeline-chart">
        <div v-if="isLoading" class="flex items-center justify-center p-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-orange-500" />
        </div>

        <VueApexCharts v-else type="rangeBar" :height="height" :width="width" :options="chartOptions" :series="series" />

        <div v-if="alerts.length > 0" class="mt-4 grid grid-cols-4 gap-4 text-center">
            <div class="p-2 bg-blue-50 rounded-lg">
                <div class="text-lg font-bold text-blue-600">{{ alerts.filter(a => a.severity === 'info').length }}</div>
                <div class="text-xs text-gray-600">Info</div>
            </div>
            <div class="p-2 bg-yellow-50 rounded-lg">
                <div class="text-lg font-bold text-yellow-600">{{ alerts.filter(a => a.severity === 'warning').length }}</div>
                <div class="text-xs text-gray-600">Warning</div>
            </div>
            <div class="p-2 bg-red-50 rounded-lg">
                <div class="text-lg font-bold text-red-600">{{ alerts.filter(a => a.severity === 'critical').length }}</div>
                <div class="text-xs text-gray-600">Critical</div>
            </div>
            <div class="p-2 bg-gray-50 rounded-lg">
                <div class="text-lg font-bold text-gray-600">{{ alerts.filter(a => a.acknowledged).length }}</div>
                <div class="text-xs text-gray-600">Acknowledged</div>
            </div>
        </div>
    </div>
</template>
