<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SidebarNav from '@/components/dashboard/SidebarNav.vue'
import HeaderBar from '@/components/dashboard/HeaderBar.vue'
import Card from '@/components/common/Card.vue'
import Spinner from '@/components/common/Spinner.vue'
import MachineHealthGauge from '@/components/machines/MachineHealthGauge.vue'
import MachineStatusIndicator from '@/components/machines/MachineStatusIndicator.vue'
import { useMachinesStore } from '@/stores/machines'
import { useAlertsStore } from '@/stores/alerts'
import { usePredictionsStore } from '@/stores/predictions'

const route = useRoute()
const router = useRouter()
const machinesStore = useMachinesStore()
const alertsStore = useAlertsStore()
const predictionsStore = usePredictionsStore()

const machineId = route.params.id as string
const sidebarCollapsed = ref(false)
const activeTab = ref<'overview' | 'telemetry' | 'predictions' | 'maintenance'>('overview')

const isLoading = computed(() => machinesStore.isLoading)

const machine = computed(() => machinesStore.machines.find(m => m.id === machineId))

const machineAlerts = computed(() => 
    alertsStore.alerts.filter(a => a.machineId === machineId)
)

const rulPrediction = computed(() => 
    predictionsStore.rulPredictions[machineId]
)

const handleRefresh = async () => {
    await Promise.all([
        machinesStore.fetchMachineById(machineId),
        alertsStore.fetchAlerts(machineId),
        predictionsStore.fetchRULPrediction(machineId)
    ])
}

onMounted(async () => {
    if (!machine.value) {
        await machinesStore.fetchMachineById(machineId)
    }
    await Promise.all([
        alertsStore.fetchAlerts(machineId),
        predictionsStore.fetchRULPrediction(machineId)
    ])
})
</script>

<template>
    <div class="min-h-screen bg-gray-50">
        <SidebarNav v-model:collapsed="sidebarCollapsed" />
        
        <div :class="['transition-all duration-300', sidebarCollapsed ? 'ml-16' : 'ml-64']">
            <HeaderBar 
                :title="machine?.name || 'Machine Details'" 
                :subtitle="`Machine ID: ${machineId}`"
            />
            
            <main class="p-6">
                <!-- Back Button -->
                <div class="mb-4">
                    <button 
                        @click="router.back()"
                        class="flex items-center gap-2 text-sm text-gray-600 hover:text-gray-900"
                    >
                        <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
                        </svg>
                        Back to Machines
                    </button>
                </div>
                
                <div v-if="isLoading" class="flex items-center justify-center h-64">
                    <Spinner size="lg" />
                </div>
                
                <div v-else-if="!machine" class="text-center py-12">
                    <p class="text-gray-500">Machine not found</p>
                </div>
                
                <div v-else class="space-y-6">
                    <!-- Machine Header -->
                    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                        <!-- Health & Status -->
                        <Card class="p-6">
                            <div class="flex items-center justify-between mb-4">
                                <h3 class="text-lg font-semibold text-gray-900">Health Status</h3>
                                <MachineStatusIndicator :status="machine.status" />
                            </div>
                            <MachineHealthGauge :health-score="machine.healthScore" size="lg" />
                        </Card>
                        
                        <!-- RUL Prediction -->
                        <Card class="p-6">
                            <template #header>
                                <h3 class="text-lg font-semibold text-gray-900">RUL Prediction</h3>
                            </template>
                            <div v-if="rulPrediction" class="text-center">
                                <p class="text-4xl font-bold text-blue-600">{{ Math.round(rulPrediction.currentRUL) }}</p>
                                <p class="text-sm text-gray-500 mt-1">hours remaining</p>
                                <p class="text-xs text-gray-400 mt-2">
                                    Confidence: {{ Math.round(rulPrediction.confidenceLower) }} - {{ Math.round(rulPrediction.confidenceUpper) }}
                                </p>
                            </div>
                            <div v-else class="text-center py-4">
                                <p class="text-gray-500">No prediction available</p>
                            </div>
                        </Card>
                        
                        <!-- Quick Stats -->
                        <Card class="p-6">
                            <template #header>
                                <h3 class="text-lg font-semibold text-gray-900">Machine Info</h3>
                            </template>
                            <div class="space-y-3 text-sm">
                                <div class="flex justify-between">
                                    <span class="text-gray-500">Type:</span>
                                    <span class="font-medium">{{ machine.type || 'Unknown' }}</span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-gray-500">Location:</span>
                                    <span class="font-medium">{{ machine.location || 'Unknown' }}</span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-gray-500">Last Maintenance:</span>
                                    <span class="font-medium">
                                        {{ machine.lastMaintenanceDate ? new Date(machine.lastMaintenanceDate).toLocaleDateString() : 'N/A' }}
                                    </span>
                                </div>
                                <div class="flex justify-between">
                                    <span class="text-gray-500">Install Date:</span>
                                    <span class="font-medium">
                                        {{ machine.installDate ? new Date(machine.installDate).toLocaleDateString() : 'N/A' }}
                                    </span>
                                </div>
                            </div>
                        </Card>
                    </div>
                    
                    <!-- Tabs -->
                    <div class="border-b border-gray-200">
                        <nav class="flex gap-8">
                            <button
                                v-for="tab in ['overview', 'telemetry', 'predictions', 'maintenance']"
                                :key="tab"
                                class="py-3 text-sm font-medium border-b-2 capitalize transition-colors"
                                :class="[activeTab === tab ? 'border-primary-500 text-primary-600' : 'border-transparent text-gray-500 hover:text-gray-700']"
                                @click="activeTab = tab"
                            >
                                {{ tab }}
                            </button>
                        </nav>
                    </div>
                    
                    <!-- Tab Content -->
                    <div class="space-y-6">
                        <!-- Overview Tab -->
                        <div v-if="activeTab === 'overview'" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                            <!-- Active Alerts -->
                            <Card class="p-6">
                                <template #header>
                                    <h3 class="text-lg font-semibold text-gray-900">Active Alerts</h3>
                                </template>
                                <div v-if="machineAlerts.length === 0" class="text-center py-4 text-gray-500">
                                    No active alerts
                                </div>
                                <div v-else class="space-y-3">
                                    <div 
                                        v-for="alert in machineAlerts.slice(0, 5)" 
                                        :key="alert.id"
                                        :class="[
                                            'p-3 rounded-lg border-l-4',
                                            alert.severity === 'critical' ? 'bg-red-50 border-red-500' :
                                            alert.severity === 'warning' ? 'bg-yellow-50 border-yellow-500' :
                                            'bg-blue-50 border-blue-500'
                                        ]"
                                    >
                                        <p class="font-medium text-gray-900">{{ alert.title }}</p>
                                        <p class="text-sm text-gray-500">{{ alert.description }}</p>
                                        <p class="text-xs text-gray-400 mt-1">
                                            {{ new Date(alert.timestamp).toLocaleString() }}
                                        </p>
                                    </div>
                                </div>
                            </Card>
                            
                            <!-- Specifications -->
                            <Card class="p-6">
                                <template #header>
                                    <h3 class="text-lg font-semibold text-gray-900">Specifications</h3>
                                </template>
                                <div v-if="machine.specifications && Object.keys(machine.specifications).length > 0" class="space-y-3 text-sm">
                                    <div v-for="(value, key) in machine.specifications" :key="key" class="flex justify-between">
                                        <span class="text-gray-500 capitalize">{{ key }}:</span>
                                        <span class="font-medium">{{ value }}</span>
                                    </div>
                                </div>
                                <div v-else class="text-center py-4 text-gray-500">
                                    No specifications available
                                </div>
                            </Card>
                        </div>
                        
                        <!-- Telemetry Tab -->
                        <div v-if="activeTab === 'telemetry'" class="card p-6">
                            <p class="text-gray-500 text-center">Telemetry charts will be displayed here</p>
                        </div>
                        
                        <!-- Predictions Tab -->
                        <div v-if="activeTab === 'predictions'" class="card p-6">
                            <div v-if="rulPrediction" class="space-y-4">
                                <h3 class="text-lg font-semibold text-gray-900">Prediction Details</h3>
                                <div class="grid grid-cols-2 gap-4 text-sm">
                                    <div class="flex justify-between">
                                        <span class="text-gray-500">Predicted Failure Date:</span>
                                        <span class="font-medium">
                                            {{ new Date(rulPrediction.estimatedFailureDate).toLocaleDateString() }}
                                        </span>
                                    </div>
                                    <div class="flex justify-between">
                                        <span class="text-gray-500">Degradation Rate:</span>
                                        <span class="font-medium">{{ rulPrediction.degradationRate.toFixed(2) }}%/hour</span>
                                    </div>
                                    <div class="flex justify-between">
                                        <span class="text-gray-500">Model Type:</span>
                                        <span class="font-medium">{{ rulPrediction.modelType }}</span>
                                    </div>
                                    <div class="flex justify-between">
                                        <span class="text-gray-500">Model Version:</span>
                                        <span class="font-medium">{{ rulPrediction.modelVersion }}</span>
                                    </div>
                                </div>
                            </div>
                            <div v-else class="text-center py-8 text-gray-500">
                                No prediction data available
                            </div>
                        </div>
                        
                        <!-- Maintenance Tab -->
                        <div v-if="activeTab === 'maintenance'" class="card p-6">
                            <p class="text-gray-500 text-center">Maintenance history will be displayed here</p>
                        </div>
                    </div>
                </div>
            </main>
        </div>
    </div>
</template>
