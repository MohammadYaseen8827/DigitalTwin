import { ref, computed, watch, onMounted } from 'vue'
import type { TimelineDataPoint } from '@/api/types'
import Card from '@/components/common/Card.vue'
import { predictionsService } from '@/services/predictions.service'
import { prescriptiveService } from '@/services/prescriptive.service'
import type { MaintenanceWindow } from '@/services/prescriptive.service'

interface Props {
    machineId: string
    timeHorizon?: number // days
    showMaintenanceMarkers?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    timeHorizon: 90,
    showMaintenanceMarkers: true
})

const emit = defineEmits<{
    'maintenance-recommended': [date: Date]
    'point-click': [point: any]
}>()

// Chart data
const timelineData = ref<any[]>([])
const maintenanceRecommendations = ref<MaintenanceWindow[]>([])
const isLoading = ref(false)

// Time horizon options
const timeHorizonOptions = [
    { value: 30, label: '30 days' },
    { value: 60, label: '60 days' },
    { value: 90, label: '90 days' },
    { value: 180, label: '6 months' },
    { value: 365, label: '1 year' }
]

// This chart now uses real data
const isSampleData = ref(false)

const fetchData = async () => {
    if (!props.machineId) return
    
    try {
        isLoading.value = true
        const [history, analysis] = await Promise.all([
            predictionsService.getRulHistory(props.machineId, props.timeHorizon),
            prescriptiveService.getAnalysis(props.machineId, props.timeHorizon)
        ])
        
        timelineData.value = history.map(p => ({
            timestamp: new Date(p.predictionDate || Date.now()),
            rul: p.remainingUsefulLifeDays,
            failureProbability: p.failureProbability * 100,
            confidenceLower: p.confidenceInterval?.lower || 0,
            confidenceUpper: p.confidenceInterval?.upper || 0
        }))
        
        maintenanceRecommendations.value = analysis
        isSampleData.value = history.length === 0
    } catch (error) {
        console.error('Failed to fetch RUL history:', error)
        isSampleData.value = true
    } finally {
        isLoading.value = false
    }
}

// Computed
const currentRUL = computed(() => {
    return timelineData.value[0]?.rul || 0
})

const failureRisk = computed(() => {
    return Math.round(timelineData.value[0]?.failureProbability || 0)
})

const criticalDate = computed(() => {
    const critical = timelineData.value.find(p => p.rul <= 30)
    return critical ? new Date(critical.timestamp) : null
})

const optimalMaintenanceDate = computed(() => {
    const optimal = maintenanceRecommendations.value.find(r => r.priority === 'high' || r.priority === 'critical')
    return optimal ? new Date(optimal.scheduledDate) : null
})

// Methods
const formatDate = (date: Date): string => {
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

const formatRUL = (rul: number): string => {
    if (rul > 365) return `${(rul / 365).toFixed(1)}y`
    return `${rul}d`
}

const getRiskLevel = (probability: number): { color: string; label: string } => {
    if (probability < 25) return { color: 'bg-green-500', label: 'Low' }
    if (probability < 50) return { color: 'bg-yellow-500', label: 'Medium' }
    if (probability < 75) return { color: 'bg-orange-500', label: 'High' }
    return { color: 'bg-red-500', label: 'Critical' }
}

const handlePointClick = (point: any) => {
    emit('point-click', point)
}

// Watch for changes
watch(() => props.machineId, fetchData)
watch(() => props.timeHorizon, fetchData)

onMounted(fetchData)
</script>

<template>
    <Card class="h-full">
        <template #header>
            <div class="flex items-center justify-between">
                <div>
                    <h3 class="font-semibold text-gray-900">RUL Timeline</h3>
                    <p class="text-xs text-gray-500">{{ machineId }} • {{ timeHorizon }} day forecast</p>
                    <p v-if="isSampleData" class="text-xs text-gray-500 italic mt-0.5">Sample data — no API data available</p>
                </div>
                
                <select
                    :value="timeHorizon"
                    class="text-sm border-gray-300 rounded-lg focus:ring-primary-500 focus:border-primary-500"
                >
                    <option 
                        v-for="option in timeHorizonOptions" 
                        :key="option.value" 
                        :value="option.value"
                    >
                        {{ option.label }}
                    </option>
                </select>
            </div>
        </template>
        
        <template #default>
            <div class="space-y-6">
                <!-- Summary stats -->
                <div class="grid grid-cols-3 gap-4">
                    <div class="text-center">
                        <div class="text-2xl font-bold text-gray-900">{{ currentRUL }}</div>
                        <div class="text-xs text-gray-500">Current RUL</div>
                    </div>
                    <div class="text-center">
                        <div class="text-2xl font-bold" :class="getRiskLevel(failureRisk).color.replace('bg-', 'text-')">
                            {{ failureRisk }}%
                        </div>
                        <div class="text-xs text-gray-500">Failure Risk</div>
                    </div>
                    <div class="text-center">
                        <div class="text-sm font-bold text-gray-900">
                            {{ criticalDate ? formatDate(criticalDate) : 'N/A' }}
                        </div>
                        <div class="text-xs text-gray-500">Critical Date</div>
                    </div>
                </div>
                
                <!-- Chart placeholder -->
                <div class="relative h-48 bg-gray-50 rounded-lg overflow-hidden">
                    <!-- Grid lines -->
                    <div class="absolute inset-0 flex items-center justify-between px-4">
                        <div v-for="i in 5" :key="i" class="w-px h-full bg-gray-200"></div>
                    </div>
                    
                    <!-- RUL curve -->
                    <svg class="absolute inset-0 w-full h-full" preserveAspectRatio="none">
                        <defs>
                            <linearGradient id="rulGradient" x1="0" y1="0" x2="0" y2="1">
                                <stop offset="0%" stop-color="#3B82F6" stop-opacity="0.3" />
                                <stop offset="100%" stop-color="#3B82F6" stop-opacity="0" />
                            </linearGradient>
                        </defs>
                        
                        <!-- Confidence band -->
                        <path
                            v-if="timelineData.length > 0"
                            :d="`M ${timelineData.map((p, i) => `${(i / (timelineData.length - 1)) * 100}% ${100 - (p.confidenceUpper / 150) * 100}%`).join(' L ')} L ${timelineData.map((p, i) => `${(i / (timelineData.length - 1)) * 100}% ${100 - (p.confidenceLower / 150) * 100}%`).reverse().join(' L ')} Z`"
                            fill="url(#rulGradient)"
                            class="opacity-50"
                        />
                        
                        <!-- RUL line -->
                        <path
                            v-if="timelineData.length > 0"
                            :d="M 0 50% L 100% 50%"
                            stroke="#3B82F6"
                            stroke-width="3"
                            fill="none"
                            class="transition-all duration-500"
                        />
                    </svg>
                    
                    <!-- Maintenance markers -->
                    <template v-if="showMaintenanceMarkers && maintenanceRecommendations.length > 0">
                        <div
                            v-for="(rec, index) in maintenanceRecommendations"
                            :key="index"
                            class="absolute top-0 transform -translate-x-1/2"
                            :style="{ left: `${(new Date(rec.date).getTime() - new Date(timelineData[0]?.timestamp).getTime()) / (props.timeHorizon * 24 * 60 * 60 * 1000) * 100}%` }"
                        >
                            <div 
                                class="w-3 h-3 rounded-full border-2 border-white shadow"
                                :class="{
                                    'bg-green-500': rec.type === 'optimal',
                                    'bg-yellow-500': rec.type === 'minimum',
                                    'bg-red-500': rec.type === 'emergency'
                                }"
                            />
                            <div class="absolute top-4 left-1/2 transform -translate-x-1/2 text-xs whitespace-nowrap">
                                <span 
                                    class="px-2 py-1 rounded"
                                    :class="{
                                        'bg-green-100 text-green-700': rec.type === 'optimal',
                                        'bg-yellow-100 text-yellow-700': rec.type === 'minimum',
                                        'bg-red-100 text-red-700': rec.type === 'emergency'
                                    }"
                                >
                                    {{ formatDate(new Date(rec.date)) }}
                                </span>
                            </div>
                        </div>
                    </template>
                </div>
                
                <!-- Legend -->
                <div class="flex flex-wrap items-center justify-center gap-4 text-xs">
                    <div class="flex items-center gap-1">
                        <span class="w-3 h-3 rounded-full bg-blue-500"></span>
                        <span class="text-gray-600">RUL Prediction</span>
                    </div>
                    <div class="flex items-center gap-1">
                        <span class="w-3 h-3 rounded-full bg-green-500"></span>
                        <span class="text-gray-600">Optimal Maintenance</span>
                    </div>
                    <div class="flex items-center gap-1">
                        <span class="w-3 h-3 rounded-full bg-yellow-500"></span>
                        <span class="text-gray-600">Minimum Maintenance</span>
                    </div>
                </div>
                
                <!-- Recommendations -->
                <div v-if="optimalMaintenanceDate" class="bg-green-50 border border-green-200 rounded-lg p-4">
                    <div class="flex items-start gap-3">
                        <svg class="w-5 h-5 text-green-500 mt-0.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                        </svg>
                        <div>
                            <p class="font-medium text-green-800">Maintenance Recommended</p>
                            <p class="text-sm text-green-700 mt-1">
                                Schedule maintenance around <strong>{{ formatDate(optimalMaintenanceDate) }}</strong> for optimal results.
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </template>
    </Card>
</template>
