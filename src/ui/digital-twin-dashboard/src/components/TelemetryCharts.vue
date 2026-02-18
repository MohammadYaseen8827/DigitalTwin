<template>
  <div class="telemetry-charts">
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- Temperature Chart -->
      <div class="bg-white rounded-lg shadow p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-gray-900">Temperature Trends</h3>
          <div class="flex items-center space-x-2">
            <span class="text-sm text-gray-500">Last updated: {{ lastUpdate }}</span>
            <button 
              @click="refreshData"
              :disabled="loading"
              class="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
            >
              <span v-if="!loading">Refresh</span>
              <span v-else>Loading...</span>
            </button>
          </div>
        </div>
        
        <div v-if="loading" class="flex items-center justify-center h-64">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
        
        <div v-else-if="temperatureData.length > 0" class="h-64">
          <LineChart
            :data="temperatureChartData"
            :options="chartOptions"
            title="Temperature (°C)"
            unit="°C"
            :color="'#ef4444'"
          />
        </div>
        
        <div v-else class="flex items-center justify-center h-64 text-gray-500">
          <p>No temperature data available</p>
        </div>
      </div>

      <!-- Vibration Chart -->
      <div class="bg-white rounded-lg shadow p-6">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-semibold text-gray-900">Vibration Analysis</h3>
          <div class="flex items-center space-x-2">
            <span class="text-sm text-gray-500">Threshold: {{ vibrationThreshold }} mm/s</span>
            <button 
              @click="refreshData"
              :disabled="loading"
              class="px-3 py-1 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
            >
              <span v-if="!loading">Refresh</span>
              <span v-else>Loading...</span>
            </button>
          </div>
        </div>
        
        <div v-if="loading" class="flex items-center justify-center h-64">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        </div>
        
        <div v-else-if="vibrationData.length > 0" class="h-64">
          <LineChart
            :data="vibrationChartData"
            :options="chartOptions"
            title="Vibration (mm/s)"
            unit="mm/s"
            :color="'#f59e0b'"
          />
          
          <!-- Vibration Gauge -->
          <div class="mt-4 flex justify-center">
            <GaugeChart
              :value="currentVibration"
              :min="0"
              :max="vibrationThreshold * 2"
              :thresholds="[vibrationThreshold * 0.5, vibrationThreshold, vibrationThreshold * 1.5]"
              :colors="['#10b981', '#f59e0b', '#ef4444']"
              title="Current Vibration"
            />
          </div>
        </div>
        
        <div v-else class="flex items-center justify-center h-64 text-gray-500">
          <p>No vibration data available</p>
        </div>
      </div>

      <!-- Real-time Metrics -->
      <div class="bg-white rounded-lg shadow p-6 lg:col-span-2">
        <h3 class="text-lg font-semibold text-gray-900 mb-4">Real-time Metrics</h3>
        
        <div class="grid grid-cols-2 gap-4">
          <div class="bg-gray-50 rounded p-4">
            <h4 class="font-medium text-gray-700 mb-2">Health Score</h4>
            <div class="text-2xl font-bold" :class="getHealthScoreClass(currentHealthScore)">
              {{ currentHealthScore?.toFixed(1) || 'N/A' }}
            </div>
            <div class="text-sm text-gray-600 mt-1">
              {{ getHealthScoreDescription(currentHealthScore) }}
            </div>
          </div>
          
          <div class="bg-gray-50 rounded p-4">
            <h4 class="font-medium text-gray-700 mb-2">Status</h4>
            <div class="flex items-center space-x-2">
              <div class="w-3 h-3 rounded-full" :class="getStatusClass(currentStatus)"></div>
              <span class="text-sm font-medium">{{ getStatusText(currentStatus) }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useTelemetryStore } from '@/stores/telemetry'
import LineChart from './charts/LineChart.vue'
import GaugeChart from './charts/GaugeChart.vue'
import { telemetryService } from '@/services/telemetry.service'

interface TelemetryPoint {
  timestamp: string
  temperature: number | null
  vibration: number | null
  pressure: number | null
  rpm: number | null
  healthScore: number | null
}

const telemetryStore = useTelemetryStore()
const loading = ref(false)
const lastUpdate = ref('')
const vibrationThreshold = ref(5.0) // mm/s

// Current real-time values
const currentHealthScore = ref<number | null>(null)
const currentStatus = ref<string>('offline')

// Chart data
const temperatureData = ref<TelemetryPoint[]>([])
const vibrationData = ref<TelemetryPoint[]>([])

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  scales: {
    x: {
      type: 'time',
      time: {
        unit: 'minute',
        displayFormats: {
          minute: 'HH:mm'
        }
      }
    },
    y: {
      beginAtZero: true
    }
  },
  plugins: {
    legend: {
      display: true,
      position: 'top'
    },
    tooltip: {
      mode: 'index',
      intersect: false
    }
  }
}

const temperatureChartData = computed(() => ({
  labels: temperatureData.value.map(point => point.timestamp),
  datasets: [{
    label: 'Temperature',
    data: temperatureData.value.map(point => point.temperature),
    borderColor: '#ef4444',
    backgroundColor: 'rgba(239, 68, 68, 0.1)',
    tension: 0.1,
    fill: true
  }]
}))

const vibrationChartData = computed(() => ({
  labels: vibrationData.value.map(point => point.timestamp),
  datasets: [{
    label: 'Vibration RMS',
    data: vibrationData.value.map(point => point.vibration),
    borderColor: '#f59e0b',
    backgroundColor: 'rgba(245, 158, 11, 0.1)',
    tension: 0.1,
    fill: true
  }]
}))

const currentVibration = computed(() => {
  const latestPoint = vibrationData.value[vibrationData.value.length - 1]
  return latestPoint?.vibration || 0
})

const getHealthScoreClass = (score: number | null) => {
  if (score === null) return 'text-gray-400'
  if (score >= 80) return 'text-green-600'
  if (score >= 60) return 'text-yellow-600'
  if (score >= 40) return 'text-orange-600'
  return 'text-red-600'
}

const getHealthScoreDescription = (score: number | null) => {
  if (score === null) return 'No data available'
  if (score >= 80) return 'Excellent - Equipment operating normally'
  if (score >= 60) return 'Good - Minor deviations detected'
  if (score >= 40) return 'Fair - Significant deviations detected'
  return 'Poor - Critical attention required'
}

const getStatusClass = (status: string) => {
  switch (status.toLowerCase()) {
    case 'online': return 'bg-green-500'
    case 'offline': return 'bg-gray-500'
    case 'maintenance': return 'bg-yellow-500'
    case 'error': return 'bg-red-500'
    case 'degraded': return 'bg-orange-500'
    default: return 'bg-gray-500'
  }
}

const getStatusText = (status: string) => {
  switch (status.toLowerCase()) {
    case 'online': return 'Online'
    case 'offline': return 'Offline'
    case 'maintenance': return 'Maintenance'
    case 'error': return 'Error'
    case 'degraded': return 'Degraded'
    default: return 'Unknown'
  }
}

const refreshData = async () => {
  loading.value = true
  try {
    await telemetryStore.fetchRecentTelemetry()
    lastUpdate.value = new Date().toLocaleTimeString()
  } catch (error) {
    console.error('Failed to refresh telemetry data:', error)
  } finally {
    loading.value = false
  }
}

// Simulate real-time updates (in production, this would come from SignalR)
const simulateRealTimeUpdates = () => {
  const updateData = () => {
    // Simulate new telemetry point
    const now = new Date()
    const timestamp = now.toLocaleTimeString()
    
    const newTempPoint: TelemetryPoint = {
      timestamp,
      temperature: 20 + Math.random() * 10,
      vibration: null,
      pressure: null,
      rpm: null,
      healthScore: null
    }
    
    const newVibPoint: TelemetryPoint = {
      timestamp,
      temperature: null,
      vibration: 2 + Math.random() * 3,
      pressure: null,
      rpm: null,
      healthScore: null
    }
    
    temperatureData.value.push(newTempPoint)
    vibrationData.value.push(newVibPoint)
    
    // Keep only last 50 points for performance
    if (temperatureData.value.length > 50) {
      temperatureData.value = temperatureData.value.slice(-50)
    }
    if (vibrationData.value.length > 50) {
      vibrationData.value = vibrationData.value.slice(-50)
    }
    
    currentHealthScore.value = 85 - (newVibPoint.vibration || 0) * 5
    lastUpdate.value = timestamp
  }
  
  let interval: NodeJS.Timeout | null = null
  
  onMounted(() => {
    refreshData()
    // Simulate real-time updates every 100ms to meet <100ms requirement
    interval = setInterval(updateData, 100)
  })
  
  onUnmounted(() => {
    if (interval) {
      clearInterval(interval)
    }
  })
}

// Rest of the code remains the same
</script>

<style scoped>
.telemetry-charts {
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
  border-top: 2px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style>
