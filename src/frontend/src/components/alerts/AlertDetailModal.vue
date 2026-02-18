<script setup lang="ts">
import { computed } from 'vue'
import type { Alert } from '@/types/alert'
import { SEVERITY_COLORS, SEVERITY_LABELS } from '@/types/alert'
import Modal from '@/components/common/Modal.vue'
import Card from '@/components/common/Card.vue'

interface Props {
    show: boolean
    alert: Alert | null
}

const props = defineProps<Props>()

const emit = defineEmits<{
    close: []
    acknowledge: [id: string]
    resolve: [id: string]
    viewPrediction: [predictionId: string]
}>()

const formattedDate = computed(() => {
    if (!props.alert) return ''
    return new Date(props.alert.timestamp).toLocaleString()
})

const formattedResolutionDate = computed(() => {
    if (!props.alert?.resolvedAt) return ''
    return new Date(props.alert.resolvedAt).toLocaleString()
})

const severityColor = computed(() => {
    return props.alert ? SEVERITY_COLORS[props.alert.severity] : '#3B82F6'
})

const isActionable = computed(() => {
    return props.alert?.status === 'active'
})
</script>

<template>
    <Modal
        :show="show"
        title="Alert Details"
        size="lg"
        @close="$emit('close')"
    >
        <template v-if="alert">
            <!-- Header with severity -->
            <div 
                class="flex items-center gap-3 p-4 rounded-lg mb-4"
                :style="{ backgroundColor: `${severityColor}10` }"
            >
                <div 
                    class="w-12 h-12 rounded-full flex items-center justify-center"
                    :style="{ backgroundColor: `${severityColor}20` }"
                >
                    <svg 
                        class="w-6 h-6"
                        :style="{ color: severityColor }"
                        fill="none" 
                        viewBox="0 0 24 24" 
                        stroke="currentColor"
                    >
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                    </svg>
                </div>
                <div class="flex-1">
                    <div class="flex items-center gap-2">
                        <h3 class="text-lg font-semibold text-gray-900">{{ alert.title }}</h3>
                        <span 
                            class="badge text-xs"
                            :style="{ 
                                backgroundColor: `${severityColor}20`,
                                color: severityColor
                            }"
                        >
                            {{ SEVERITY_LABELS[alert.severity] }}
                        </span>
                    </div>
                    <p class="text-sm text-gray-600">
                        {{ alert.machineName || alert.machineId }}
                    </p>
                </div>
            </div>
            
            <!-- Details grid -->
            <div class="grid grid-cols-2 gap-4 mb-4">
                <Card padding="sm">
                    <div class="text-xs text-gray-500 mb-1">Status</div>
                    <div class="flex items-center gap-2">
                        <span 
                            class="w-2 h-2 rounded-full"
                            :class="{
                                'bg-green-500': alert.status === 'resolved',
                                'bg-yellow-500': alert.status === 'acknowledged',
                                'bg-red-500': alert.status === 'active'
                            }"
                        />
                        <span class="font-medium capitalize">{{ alert.status }}</span>
                    </div>
                </Card>
                
                <Card padding="sm">
                    <div class="text-xs text-gray-500 mb-1">Created</div>
                    <div class="font-medium">{{ formattedDate }}</div>
                </Card>
                
                <Card v-if="alert.acknowledgedBy" padding="sm">
                    <div class="text-xs text-gray-500 mb-1">Acknowledged By</div>
                    <div class="font-medium">{{ alert.acknowledgedBy }}</div>
                </Card>
                
                <Card v-if="alert.resolvedAt" padding="sm">
                    <div class="text-xs text-gray-500 mb-1">Resolved At</div>
                    <div class="font-medium">{{ formattedResolutionDate }}</div>
                </Card>
            </div>
            
            <!-- Description -->
            <Card padding="sm" class="mb-4">
                <div class="text-xs text-gray-500 mb-2">Description</div>
                <p class="text-gray-700">{{ alert.description }}</p>
            </Card>
            
            <!-- Category -->
            <Card v-if="alert.category" padding="sm" class="mb-4">
                <div class="text-xs text-gray-500 mb-2">Category</div>
                <span class="badge badge-info capitalize">{{ alert.category }}</span>
            </Card>
            
            <!-- Suggested Actions -->
            <Card v-if="alert.suggestedActions && alert.suggestedActions.length > 0" padding="sm" class="mb-4">
                <div class="text-xs text-gray-500 mb-2">Suggested Actions</div>
                <ul class="space-y-2">
                    <li 
                        v-for="(action, index) in alert.suggestedActions"
                        :key="index"
                        class="flex items-start gap-2 text-sm text-gray-700"
                    >
                        <svg class="w-4 h-4 text-primary-500 mt-0.5 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                        </svg>
                        {{ action }}
                    </li>
                </ul>
            </Card>
            
            <!-- Related Prediction -->
            <Card 
                v-if="alert.relatedPredictionId" 
                padding="sm" 
                class="mb-4 hover:bg-gray-50 cursor-pointer"
                @click="$emit('viewPrediction', alert.relatedPredictionId)"
            >
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-2">
                        <svg class="w-5 h-5 text-primary-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                        </svg>
                        <span class="text-sm font-medium text-gray-700">View Related Prediction</span>
                    </div>
                    <svg class="w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                    </svg>
                </div>
            </Card>
        </template>
        
        <template #footer>
            <div class="flex justify-end gap-3">
                <button
                    class="btn-secondary"
                    @click="$emit('close')"
                >
                    Close
                </button>
                <button
                    v-if="isActionable"
                    class="btn-primary"
                    @click="$emit('acknowledge', alert.id)"
                >
                    Acknowledge
                </button>
                <button
                    v-if="alert?.status === 'acknowledged'"
                    class="btn-success"
                    @click="$emit('resolve', alert.id)"
                >
                    Mark Resolved
                </button>
            </div>
        </template>
    </Modal>
</template>
