<script setup lang="ts">
import { computed } from 'vue'
import type { RULPrediction } from '@/types/prediction'
import Card from '@/components/common/Card.vue'

interface Props {
    prediction: RULPrediction
    compact?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    compact: false
})

const emit = defineEmits<{
    click: []
    viewDetails: []
}>()

// Computed
const trendIcon = computed(() => {
    const trends = {
        improving: 'M5 15l7-7 7 7',
        declining: 'M19 9l-7 7-7-7',
        stable: 'M5 12h14'
    }
    return trends[props.prediction.trend]
})

const trendColor = computed(() => {
    return {
        improving: 'text-green-500',
        declining: 'text-red-500',
        stable: 'text-gray-500'
    }[props.prediction.trend]
})

const healthStatus = computed(() => {
    const score = props.prediction.healthScore || 0
    if (score >= 80) return { color: 'bg-green-500', label: 'Good' }
    if (score >= 50) return { color: 'bg-yellow-500', label: 'Fair' }
    return { color: 'bg-red-500', label: 'Poor' }
})

const formattedRUL = computed(() => {
    const rul = props.prediction.currentRUL
    if (rul > 365) {
        return `${(rul / 365).toFixed(1)} years`
    }
    return `${rul} days`
})

const formattedFailureDate = computed(() => {
    return new Date(props.prediction.estimatedFailureDate).toLocaleDateString()
})

const confidencePercent = computed(() => {
    return Math.round(props.prediction.confidenceScore * 100)
})
</script>

<template>
    <Card 
        :hoverable="!compact"
        :class="[
            compact ? 'p-4' : 'p-6',
            'cursor-pointer transition-all duration-200 hover:shadow-md'
        ]"
        @click="$emit('click')"
    >
        <div :class="['space-y-4', compact ? 'space-y-2' : '']">
            <!-- Header -->
            <div class="flex items-center justify-between">
                <div class="flex items-center gap-3">
                    <div 
                        class="w-10 h-10 rounded-lg flex items-center justify-center"
                        :class="{
                            'bg-green-100': prediction.trend === 'improving',
                            'bg-red-100': prediction.trend === 'declining',
                            'bg-gray-100': prediction.trend === 'stable'
                        }"
                    >
                        <svg 
                            class="w-5 h-5"
                            :class="trendColor"
                            fill="none" 
                            viewBox="0 0 24 24" 
                            stroke="currentColor"
                        >
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="trendIcon" />
                        </svg>
                    </div>
                    
                    <div v-if="!compact">
                        <h3 class="font-semibold text-gray-900">
                            {{ prediction.machineName || prediction.machineId }}
                        </h3>
                        <p class="text-xs text-gray-500">
                            Last updated: {{ new Date(prediction.lastUpdated).toLocaleTimeString() }}
                        </p>
                    </div>
                    
                    <span v-else class="font-medium text-gray-900">
                        {{ prediction.machineName || prediction.machineId }}
                    </span>
                </div>
                
                <!-- Health indicator -->
                <div 
                    v-if="prediction.healthScore !== undefined"
                    class="flex items-center gap-2"
                >
                    <div class="w-2 h-2 rounded-full" :class="healthStatus.color" />
                    <span class="text-xs text-gray-600">{{ healthStatus.label }}</span>
                </div>
            </div>
            
            <!-- RUL Value -->
            <div class="flex items-end gap-4">
                <div>
                    <p class="text-xs text-gray-500 uppercase tracking-wide">Current RUL</p>
                    <p class="text-3xl font-bold text-gray-900">
                        {{ formattedRUL }}
                    </p>
                </div>
                
                <!-- Confidence badge -->
                <div class="flex items-center gap-1 mb-1">
                    <svg class="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <span class="text-sm font-medium text-gray-600">
                        {{ confidencePercent }}% confidence
                    </span>
                </div>
            </div>
            
            <!-- Confidence interval -->
            <div class="space-y-1">
                <div class="flex justify-between text-xs text-gray-500">
                    <span>{{ prediction.confidenceLower }} days</span>
                    <span>{{ prediction.confidenceUpper }} days</span>
                </div>
                <div class="h-2 bg-gray-100 rounded-full overflow-hidden">
                    <div 
                        class="h-full bg-gradient-to-r from-primary-400 to-primary-600 rounded-full"
                        :style="{
                            width: `${confidencePercent}%`
                        }"
                    />
                </div>
            </div>
            
            <!-- Details grid -->
            <div 
                v-if="!compact"
                class="grid grid-cols-2 gap-4 pt-4 border-t border-gray-100"
            >
                <div>
                    <p class="text-xs text-gray-500">Degradation Rate</p>
                    <p class="font-medium text-gray-900">
                        {{ prediction.degradationRate.toFixed(2) }} days/day
                    </p>
                </div>
                
                <div>
                    <p class="text-xs text-gray-500">Est. Failure Date</p>
                    <p class="font-medium text-gray-900">
                        {{ formattedFailureDate }}
                    </p>
                </div>
                
                <div>
                    <p class="text-xs text-gray-500">Model</p>
                    <p class="font-medium text-gray-900">
                        {{ prediction.modelType }}
                    </p>
                </div>
                
                <div>
                    <p class="text-xs text-gray-500">Version</p>
                    <p class="font-medium text-gray-900">
                        {{ prediction.modelVersion }}
                    </p>
                </div>
            </div>
            
            <!-- Trend indicator -->
            <div 
                v-if="!compact"
                class="flex items-center justify-between pt-4"
            >
                <div class="flex items-center gap-2">
                    <span 
                        class="text-xs font-medium px-2 py-1 rounded-full"
                        :class="{
                            'bg-green-100 text-green-700': prediction.trend === 'improving',
                            'bg-red-100 text-red-700': prediction.trend === 'declining',
                            'bg-gray-100 text-gray-700': prediction.trend === 'stable'
                        }"
                    >
                        {{ prediction.trend.charAt(0).toUpperCase() + prediction.trend.slice(1) }}
                    </span>
                </div>
                
                <button
                    class="text-sm text-primary-600 hover:text-primary-700 font-medium flex items-center gap-1"
                    @click.stop="$emit('viewDetails')"
                >
                    View Details
                    <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                </button>
            </div>
        </div>
    </Card>
</template>
