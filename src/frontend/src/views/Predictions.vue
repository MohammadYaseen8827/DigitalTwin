<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import SidebarNav from '@/components/dashboard/SidebarNav.vue'
import HeaderBar from '@/components/dashboard/HeaderBar.vue'
import Card from '@/components/common/Card.vue'
import Spinner from '@/components/common/Spinner.vue'
import { useMachinesStore } from '@/stores/machines'
import { usePredictionsStore } from '@/stores/predictions'

const router = useRouter()
const machinesStore = useMachinesStore()
const predictionsStore = usePredictionsStore()

const sidebarCollapsed = ref(false)
const selectedMachineId = ref<string | null>(null)

const isLoading = computed(() => predictionsStore.isLoading || machinesStore.isLoading)

const machinePredictions = computed(() => {
    if (!selectedMachineId.value) return null
    return predictionsStore.rulPredictions[selectedMachineId.value]
})

const handleMachineSelect = async (machineId: string) => {
    selectedMachineId.value = machineId
    await predictionsStore.fetchRULPrediction(machineId)
}

onMounted(async () => {
    try {
        await Promise.all([
            machinesStore.fetchMachines(),
            predictionsStore.fetchPredictions()
        ])
    } catch (err) {
        console.error('Error loading predictions:', err)
    }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <SidebarNav v-model:collapsed="sidebarCollapsed" />
    
    <div :class="['transition-all duration-300', sidebarCollapsed ? 'ml-16' : 'ml-64']">
      <HeaderBar title="Predictions" subtitle="AI-powered predictions and insights" />
      
      <main class="p-6">
        <div v-if="isLoading" class="flex items-center justify-center h-64">
          <Spinner size="lg" />
        </div>
        
        <div v-else class="space-y-6">
          <!-- Machine Selection -->
          <Card :padding="'sm'">
            <div class="flex items-center gap-4">
              <label class="text-sm font-medium text-gray-700">Select Machine:</label>
              <select 
                v-model="selectedMachineId"
                class="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                @change="handleMachineSelect(($event.target as HTMLSelectElement).value)"
              >
                <option :value="null">Select a machine</option>
                <option v-for="machine in machinesStore.machines" :key="machine.id" :value="machine.id">
                  {{ machine.name }}
                </option>
              </select>
            </div>
          </Card>
          
          <!-- RUL Prediction Card -->
          <div v-if="machinePredictions" class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <Card class="p-6">
              <template #header>
                <h3 class="text-lg font-semibold text-gray-900">Current RUL</h3>
              </template>
              <div class="text-center">
                <p class="text-4xl font-bold text-blue-600">{{ Math.round(machinePredictions.currentRUL) }}</p>
                <p class="text-sm text-gray-500 mt-1">hours remaining</p>
              </div>
            </Card>
            
            <Card class="p-6">
              <template #header>
                <h3 class="text-lg font-semibold text-gray-900">Confidence Interval</h3>
              </template>
              <div class="text-center">
                <p class="text-4xl font-bold text-green-600">
                  {{ Math.round(machinePredictions.confidenceLower) }} - {{ Math.round(machinePredictions.confidenceUpper) }}
                </p>
                <p class="text-sm text-gray-500 mt-1">hours (95% CI)</p>
              </div>
            </Card>
            
            <Card class="p-6">
              <template #header>
                <h3 class="text-lg font-semibold text-gray-900">Model Info</h3>
              </template>
              <div class="space-y-2 text-sm">
                <div class="flex justify-between">
                  <span class="text-gray-500">Model Type:</span>
                  <span class="font-medium">{{ machinePredictions.modelType }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Version:</span>
                  <span class="font-medium">{{ machinePredictions.modelVersion }}</span>
                </div>
                <div class="flex justify-between">
                  <span class="text-gray-500">Last Updated:</span>
                  <span class="font-medium">{{ new Date(machinePredictions.lastUpdated).toLocaleDateString() }}</span>
                </div>
              </div>
            </Card>
          </div>
          
          <!-- No Machine Selected -->
          <Card v-else-if="selectedMachineId" class="p-6">
            <div class="text-center py-8">
              <p class="text-gray-500">No predictions available for this machine</p>
            </div>
          </Card>
          
          <!-- Initial State -->
          <Card v-else class="p-6">
            <div class="text-center py-8">
              <svg class="w-16 h-16 mx-auto text-gray-400 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              <p class="text-gray-500">Select a machine to view predictions</p>
            </div>
          </Card>
          
          <!-- Critical Predictions -->
          <div v-if="predictionsStore.criticalPredictions.length > 0" class="space-y-4">
            <h2 class="text-lg font-semibold text-gray-900">Critical Predictions</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Card v-for="prediction in predictionsStore.criticalPredictions" :key="prediction.id" class="p-4 border-l-4 border-red-500">
                <div class="flex items-start justify-between">
                  <div>
                    <p class="font-medium text-gray-900">{{ prediction.type }}</p>
                    <p class="text-sm text-gray-500">{{ prediction.result }}</p>
                  </div>
                  <span class="text-xs bg-red-100 text-red-700 px-2 py-1 rounded-full">Critical</span>
                </div>
                <p class="text-sm text-gray-600 mt-2">{{ prediction.timestamp }}</p>
              </Card>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
