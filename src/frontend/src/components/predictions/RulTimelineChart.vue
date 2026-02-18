<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import type { RULTimeline, TimelineDataPoint } from '@/types/prediction'
import Card from '@/components/common/Card.vue'

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
    'point-click': [point: TimelineDataPoint]
}>()

// Chart data
const timelineData = ref<TimelineDataPoint[]>([])
const maintenanceRecommendations = ref<RULTimeline['maintenanceRecommendations']>([])

// Time horizon options
const timeHorizonOptions = [
    { value: 30, label: '30 days' },
    { value: 60, label: '60 days' },
    { value: 90, label: '90 days' },
    { value: 180, label: '6 months' },
    { value: 365, label: '1 year' }
]

// This chart uses sample data until a timeline API is available
const isSampleData = true

const generateMockData = () => {
    const data: TimelineDataPoint[] = []
    const now = new Date()
    const startRUL = 120
    const endRUL = 30
    
    for (let i = 0; i <= props.timeHorizon; i++) {
        const date = new Date(now.getTime() + i * 24 * 60 * 60 * 1000)
        const progress = i / props.timeHorizon
        const rul = Math.round(startRUL - (startRUL - endRUL) * progress)
        const failureProb = Math.min(100, Math.round((progress * 100) + Math.random() * 10))
        
        data.push({
            timestamp: date,
            rul: rul,
            failureProbability: failureProb,
            confidenceLower: Math.max(0, rul - 10),
            confidenceUpper: rul + 10
        })
    }
    
    timelineData.value = data
    
    // Generate maintenance recommendations
    maintenanceRecommendations.value = [
        {
            date: new Date(now.getTime() + 30 * 24 * 60 * 60 * 1000),
            type: 'optimal',
            estimatedRUL: 90,
            description: 'Optimal maintenance window - schedule now for best results'
        },
        {
            date: new Date(now.getTime() + 60 * 24 * 60 * 60 * 1000),
            type: 'minimum',
            estimatedRUL: 60,
            description: 'Last recommended maintenance before risk increases'
        }
    ]
}

// Computed
const currentRUL = computed(() => {
    return timelineData.value[0]?.rul || 0
})

const failureRisk = computed(() => {
    return timelineData.value[0]?.failureProbability || 0
})

const criticalDate = computed(() => {
    const critical = timelineData.value.find(p => p.rul <= 30)
    return critical ? new Date(critical.timestamp) : null
})

const optimalMaintenanceDate = computed(() => {
    const optimal = maintenanceRecommendations.value.find(r => r.type === 'optimal')
    return optimal ? new Date(optimal.date) : null
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

const handlePointClick = (point: TimelineDataPoint) => {
    emit('point-click', point)
}

// Generate data on mount
generateMockData()
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
