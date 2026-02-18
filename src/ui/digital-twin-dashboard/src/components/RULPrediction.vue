<template>
  <div class="rul-prediction">
    <div class="bg-white rounded-lg shadow p-6">
      <div class="flex items-center justify-between mb-6">
        <h3 class="text-lg font-semibold text-gray-900">Remaining Useful Life (RUL)</h3>
        <div class="flex items-center space-x-2">
          <span class="text-sm text-gray-500">Last prediction: {{ lastPredictionTime }}</span>
          <button 
            @click="refreshPrediction"
            :disabled="loading"
            class="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            <span v-if="!loading">Refresh</span>
            <span v-else>Loading...</span>
          </button>
        </div>
      </div>
      
      <div v-if="loading" class="flex items-center justify-center h-32">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      </div>
      
      <div v-else-if="prediction" class="space-y-6">
        <!-- RUL Display -->
        <div class="text-center mb-6">
          <div class="text-4xl font-bold" :class="getRULClass(prediction.remainingUsefulLifeDays)">
            {{ formatRUL(prediction.remainingUsefulLifeDays) }}
          </div>
          <div class="text-sm text-gray-600 mt-2">
            Remaining useful life based on current degradation patterns
          </div>
        </div>
        
        <!-- Confidence Interval -->
        <div class="bg-gray-50 rounded-lg p-6">
          <h4 class="text-lg font-semibold text-gray-900 mb-4">Confidence Interval</h4>
          <div class="grid grid-cols-3 gap-4 text-center">
            <div>
              <div class="text-2xl font-bold text-blue-600">
                {{ formatRUL(prediction.rulUpperBound) }}
              </div>
              <div class="text-sm text-gray-600">Upper Bound</div>
            </div>
            <div>
              <div class="text-2xl font-bold text-gray-900">
                {{ formatRUL(prediction.remainingUsefulLifeDays) }}
              </div>
              <div class="text-sm text-gray-600">Predicted RUL</div>
            </div>
            <div>
              <div class="text-2xl font-bold text-red-600">
                {{ formatRUL(prediction.rulLowerBound) }}
              </div>
              <div class="text-sm text-gray-600">Lower Bound</div>
            </div>
          </div>
          
          <div class="mt-4 text-center">
            <div class="text-sm text-gray-600">
              Confidence Level: {{ (prediction.confidence * 100).toFixed(1) }}%
            </div>
            <div class="w-full bg-gray-200 rounded-full h-2 mt-2">
              <div 
                class="bg-blue-600 h-2 rounded-full transition-all duration-300"
                :style="{ width: `${prediction.confidence * 100}%` }"
              ></div>
            </div>
          </div>
        </div>
        
        <!-- Feature Importance -->
        <div class="bg-gray-50 rounded-lg p-6">
          <h4 class="text-lg font-semibold text-gray-900 mb-4">Feature Importance</h4>
          <div v-if="prediction.featureContributions" class="space-y-3">
            <div 
              v-for="(importance, feature) in sortedFeatures" 
              :key="feature"
              class="flex items-center justify-between"
            >
              <div class="flex-1">
                <span class="text-sm font-medium text-gray-700">{{ feature }}</span>
                <div class="w-full bg-gray-200 rounded-full h-2 mt-1">
                  <div 
                    class="bg-blue-600 h-2 rounded-full transition-all duration-300"
                    :style="{ width: `${importance * 100}%` }"
                  ></div>
                </div>
              </div>
              <div class="text-right">
                <span class="text-sm font-bold text-gray-900">{{ (importance * 100).toFixed(1) }}%</span>
              </div>
            </div>
          </div>
          <div v-else class="text-sm text-gray-500">
            No feature importance data available
          </div>
        </div>
        
        <!-- Health Status -->
        <div class="bg-gray-50 rounded-lg p-6">
          <h4 class="text-lg font-semibold text-gray-900 mb-4">Health Classification</h4>
          <div class="text-center">
            <div class="inline-flex items-center px-4 py-2 rounded-full text-lg font-medium" :class="getHealthStatusClass(prediction.healthStatus)">
              {{ prediction.healthStatus }}
            </div>
            <div class="text-sm text-gray-600 mt-2">
              Current equipment health status
            </div>
          </div>
        </div>
        
        <!-- Model Info -->
        <div class="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <h4 class="text-lg font-semibold text-blue-900 mb-2">Model Information</h4>
          <div class="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span class="text-gray-600">Model Version:</span>
              <span class="font-medium">{{ prediction.modelVersion }}</span>
            </div>
            <div>
              <span class="text-gray-600">Failure Probability:</span>
              <span class="font-medium">{{ (prediction.failureProbability * 100).toFixed(1) }}%</span>
            </div>
          </div>
        </div>
      </div>
      
      <div v-else class="text-center text-gray-500 py-8">
        <p>No prediction data available</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { usePredictionStore } from '@/stores/predictions'
import { predictionService } from '@/services/prediction.service'

interface PredictionData {
  remainingUsefulLifeDays: number
  rulLowerBound: number
  rulUpperBound: number
  confidence: number
  healthStatus: string
  featureContributions: Record<string, number> | null
  modelVersion: string
  failureProbability: number
}

const predictionStore = usePredictionStore()
const loading = ref(false)
const lastPredictionTime = ref('')

const prediction = computed<PredictionData | null>(() => {
  return predictionStore.latestPrediction
})

const sortedFeatures = computed(() => {
  if (!prediction.value?.featureContributions) return []
  
  return Object.entries(prediction.value.featureContributions)
    .sort(([, a], [, b]) => b[1] - a[1])
    .slice(0, 5) // Top 5 features
    .map(([feature, importance]) => ({ feature, importance }))
})

const getRULClass = (rul: number) => {
  if (rul >= 180) return 'text-green-600' // > 6 months
  if (rul >= 90) return 'text-yellow-600' // > 3 months
  if (rul >= 30) return 'text-orange-600' // > 1 month
  return 'text-red-600' // < 1 month
}

const getHealthStatusClass = (status: string) => {
  switch (status.toLowerCase()) {
    case 'healthy': return 'bg-green-100 text-green-800'
    case 'minor': return 'bg-yellow-100 text-yellow-800'
    case 'major': return 'bg-orange-100 text-orange-800'
    case 'critical': return 'bg-red-100 text-red-800'
    default: return 'bg-gray-100 text-gray-800'
  }
}

const formatRUL = (days: number) => {
  if (days >= 365) return `${Math.floor(days / 365)}y ${Math.floor((days % 365) / 30)}m`
  if (days >= 30) return `${Math.floor(days / 30)}m ${days % 30}d`
  return `${days}d`
}

const refreshPrediction = async () => {
  loading.value = true
  try {
    await predictionStore.fetchLatestPrediction()
    lastPredictionTime.value = new Date().toLocaleTimeString()
  } catch (error) {
    console.error('Failed to refresh prediction:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  refreshPrediction()
})
</script>

<style scoped>
.rul-prediction {
  @apply p-6;
}

.animate-spin {
  @apply animate-spin;
  border-top-color: #3498db;
  border-right-color: #3498db;
  border-bottom-color: #3498db;
  border-left-color: #3498db;
}

.animate-spin {
  border: 2px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
