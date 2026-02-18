<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import type { Alert } from '@/types/alert'
import { SEVERITY_COLORS, SEVERITY_LABELS } from '@/types/alert'
import { alertSound, type AlertSeverity } from '@/utils/alertSound'

interface Props {
    alert: Alert
    duration?: number
    position?: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'
    autoDismiss?: boolean
}

const props = withDefaults(defineProps<Props>(), {
    duration: 5000,
    position: 'top-right',
    autoDismiss: true
})

const emit = defineEmits<{
    acknowledge: [id: string]
    dismiss: [id: string]
    click: [alert: Alert]
}>()

const remainingTime = ref(props.duration)
const dismissed = ref(false)
const progress = ref(100)
let intervalId: ReturnType<typeof setInterval> | null = null

const severity = computed<AlertSeverity>(() => props.alert.severity)

const positionClasses = computed(() => {
    const positions: Record<string, string> = {
        'top-right': 'top-4 right-4',
        'top-left': 'top-4 left-4',
        'bottom-right': 'bottom-4 right-4',
        'bottom-left': 'bottom-4 left-4'
    }
    return positions[props.position]
})

const severityColor = computed(() => SEVERITY_COLORS[severity.value])
const severityLabel = computed(() => SEVERITY_LABELS[severity.value])

const formattedTime = computed(() => {
    const date = new Date(props.alert.timestamp)
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
})

const isCritical = computed(() => severity.value === 'critical' || severity.value === 'error')

const startCountdown = () => {
    if (!props.autoDismiss || isCritical.value) return
    
    intervalId = setInterval(() => {
        remainingTime.value -= 100
        progress.value = (remainingTime.value / props.duration) * 100
        
        if (remainingTime.value <= 0) {
            dismiss()
        }
    }, 100)
}

const stopCountdown = () => {
    if (intervalId) {
        clearInterval(intervalId)
        intervalId = null
    }
}

const acknowledge = () => {
    stopCountdown()
    emit('acknowledge', props.alert.id)
}

const dismiss = () => {
    stopCountdown()
    dismissed.value = true
    emit('dismiss', props.alert.id)
}

const handleClick = () => {
    emit('click', props.alert)
}

onMounted(async () => {
    // Play sound for critical alerts
    if (isCritical.value) {
        await alertSound.playAlertSound(severity.value)
    }
    startCountdown()
})

onUnmounted(() => {
    stopCountdown()
})
</script>

<template>
    <Transition name="toast">
        <div
            v-if="!dismissed"
            :class="['fixed z-50 max-w-sm w-full', positionClasses]"
            role="alert"
            aria-live="polite"
        >
            <div
                class="bg-white rounded-lg shadow-lg border-l-4 overflow-hidden cursor-pointer hover:shadow-xl transition-shadow duration-200"
                :style="{ borderLeftColor: severityColor }"
                @click="handleClick"
            >
                <!-- Progress bar -->
                <div 
                    v-if="autoDismiss && !isCritical" 
                    class="h-1 bg-gray-100"
                >
                    <div 
                        class="h-full transition-all duration-100 ease-linear"
                        :style="{ 
                            width: `${progress}%`,
                            backgroundColor: severityColor 
                        }"
                    />
                </div>
                
                <div class="p-4">
                    <div class="flex items-start">
                        <!-- Icon -->
                        <div class="flex-shrink-0">
                            <div 
                                class="w-10 h-10 rounded-full flex items-center justify-center"
                                :style="{ backgroundColor: `${severityColor}20` }"
                            >
                                <svg 
                                    class="w-6 h-6"
                                    :style="{ color: severityColor }"
                                    fill="none" 
                                    viewBox="0 0 24 24" 
                                    stroke="currentColor"
                                >
                                    <template v-if="severity === 'info'">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </template>
                                    <template v-else-if="severity === 'warning'">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </template>
                                    <template v-else-if="severity === 'critical'">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </template>
                                    <template v-else>
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                    </template>
                                </svg>
                            </div>
                        </div>
                        
                        <!-- Content -->
                        <div class="ml-3 flex-1">
                            <div class="flex items-center justify-between">
                                <p 
                                    class="text-sm font-semibold"
                                    :style="{ color: severityColor }"
                                >
                                    {{ severityLabel }}
                                </p>
                                <span class="text-xs text-gray-500">
                                    {{ formattedTime }}
                                </span>
                            </div>
                            
                            <p class="mt-1 text-sm font-medium text-gray-900">
                                {{ alert.title }}
                            </p>
                            
                            <p class="mt-1 text-sm text-gray-600 line-clamp-2">
                                {{ alert.description }}
                            </p>
                            
                            <!-- Actions -->
                            <div class="mt-3 flex items-center gap-2">
                                <button
                                    v-if="alert.status === 'active'"
                                    type="button"
                                    class="text-xs font-medium px-3 py-1.5 rounded-lg transition-colors duration-150"
                                    :style="{ 
                                        backgroundColor: `${severityColor}15`,
                                        color: severityColor 
                                    }"
                                    @click.stop="acknowledge"
                                >
                                    Acknowledge
                                </button>
                                <button
                                    type="button"
                                    class="text-xs font-medium px-3 py-1.5 rounded-lg bg-gray-100 text-gray-700 hover:bg-gray-200 transition-colors duration-150"
                                    @click.stop="dismiss"
                                >
                                    Dismiss
                                </button>
                            </div>
                        </div>
                        
                        <!-- Close button -->
                        <button
                            type="button"
                            class="ml-3 flex-shrink-0 text-gray-400 hover:text-gray-500 focus:outline-none"
                            @click.stop="dismiss"
                        >
                            <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </Transition>
</template>

<style scoped>
.toast-enter-active {
    animation: slideIn 0.3s ease-out;
}
.toast-leave-active {
    animation: slideOut 0.2s ease-in forwards;
}

@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateX(100%);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

@keyframes slideOut {
    from {
        opacity: 1;
        transform: translateX(0);
    }
    to {
        opacity: 0;
        transform: translateX(100%);
    }
}
</style>
