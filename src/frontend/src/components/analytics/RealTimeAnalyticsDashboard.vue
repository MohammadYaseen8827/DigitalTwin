<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useMachinesStore } from '@/stores/machines.store'
import { realTimeAnalyticsService } from '@/services/realtime-analytics.service'
import { 
  Activity, 
  TrendingUp, 
  AlertTriangle, 
  Server,
  Play,
  Pause,
  RotateCcw,
  Wifi,
  WifiOff
} from 'lucide-vue-next'

interface ChartDataPoint {
  x: Date
  y: number
}

const machinesStore = useMachinesStore()
const selectedMachineId = ref<string>('')
const isStreaming = ref(false)
const isLoading = ref(false)

// Chart data
const temperatureData = ref<ChartDataPoint[]>([])
const vibrationData = ref<ChartDataPoint[]>([])
const pressureData = ref<ChartDataPoint[]>([])

// Stats
const stats = ref({
  totalTelemetryPoints: 0,
  activePredictions: 0,
  recentAlerts: 0,
  systemHealthScore: 98.5
})

// Initialize
onMounted(async () => {
  await machinesStore.loadMachines()
  if (machinesStore.machines.length > 0) {
    selectedMachineId.value = machinesStore.machines[0].id
  }
})

onUnmounted(() => {
  stopStreaming()
})

// Computed properties
const connectionStatus = computed(() => ({
  isConnected: realTimeAnalyticsService.isConnected.value,
  subscriptionCount: realTimeAnalyticsService.subscriptions.value.length
}))

const selectedMachine = computed(() => {
  return machinesStore.machines.find(m => m.id === selectedMachineId.value)
})

// Methods
const startStreaming = async () => {
  if (!selectedMachineId.value) return
  
  try {
    isLoading.value = true
    
    // Subscribe to all streams for selected machine
    await realTimeAnalyticsService.subscribeToAllStreams(selectedMachineId.value)
    
    // Setup callbacks for real-time data
    setupDataCallbacks()
    
    isStreaming.value = true
    isLoading.value = false
    
  } catch (error) {
    console.error('Failed to start streaming:', error)
    isLoading.value = false
  }
}

const stopStreaming = async () => {
  if (selectedMachineId.value) {
    await realTimeAnalyticsService.unsubscribeFromAllStreams(selectedMachineId.value)
  }
  isStreaming.value = false
  clearChartData()
}

const restartStreaming = async () => {
  await stopStreaming()
  await startStreaming()
}

const setupDataCallbacks = () => {
  // Telemetry data callback
  realTimeAnalyticsService.onTelemetry((data: any) => {
    const point: ChartDataPoint = {
      x: new Date(),
      y: data.value
    }
    
    // Add to appropriate chart based on metric
    switch (data.metric?.toLowerCase()) {
      case 'temperature':
        temperatureData.value.unshift(point)
        break
      case 'vibration':
        vibrationData.value.unshift(point)
        break
      case 'pressure':
        pressureData.value.unshift(point)
        break
    }
    
    // Limit data points to last 100
    trimChartData()
    
    stats.value.totalTelemetryPoints++
  })
  
  // Prediction data callback
  realTimeAnalyticsService.onPrediction((data: any) => {
    stats.value.activePredictions++
  })
  
  // Alert data callback
  realTimeAnalyticsService.onAlert((data: any) => {
    stats.value.recentAlerts++
  })
  
  // System health callback
  realTimeAnalyticsService.onSystemHealth((data: any) => {
    if (data.healthScore !== undefined) {
      stats.value.systemHealthScore = data.healthScore
    }
  })
}

const trimChartData = () => {
  const maxLength = 100
  if (temperatureData.value.length > maxLength) {
    temperatureData.value = temperatureData.value.slice(0, maxLength)
  }
  if (vibrationData.value.length > maxLength) {
    vibrationData.value = vibrationData.value.slice(0, maxLength)
  }
  if (pressureData.value.length > maxLength) {
    pressureData.value = pressureData.value.slice(0, maxLength)
  }
}

const clearChartData = () => {
  temperatureData.value = []
  vibrationData.value = []
  pressureData.value = []
  stats.value = {
    totalTelemetryPoints: 0,
    activePredictions: 0,
    recentAlerts: 0,
    systemHealthScore: 98.5
  }
}

const handleMachineChange = async () => {
  if (isStreaming.value) {
    await restartStreaming()
  }
}

// Chart configuration
const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  animation: false,
  scales: {
    x: {
      type: 'time',
      time: {
        unit: 'second',
        displayFormats: {
          second: 'HH:mm:ss'
        }
      },
      grid: {
        color: 'rgba(255, 255, 255, 0.1)'
      },
      ticks: {
        color: '#94a3b8'
      }
    },
    y: {
      grid: {
        color: 'rgba(255, 255, 255, 0.1)'
      },
      ticks: {
        color: '#94a3b8'
      }
    }
  },
  plugins: {
    legend: {
      labels: {
        color: '#94a3b8'
      }
    }
  }
}
</script>

<template>
  <div class="real-time-analytics-dashboard">
    <!-- Header -->
    <div class="dashboard-header">
      <div class="header-content">
        <h1 class="dashboard-title">
          <Activity class="title-icon" />
          Real-Time Analytics Dashboard
        </h1>
        <p class="dashboard-description">
          Live monitoring of machine telemetry, predictions, and system health
        </p>
      </div>
      
      <div class="header-controls">
        <div class="status-indicator" :class="{ connected: connectionStatus.isConnected }">
          <component :is="connectionStatus.isConnected ? Wifi : WifiOff" class="status-icon" />
          <span>{{ connectionStatus.isConnected ? 'Connected' : 'Disconnected' }}</span>
        </div>
        
        <select 
          v-model="selectedMachineId" 
          @change="handleMachineChange"
          class="machine-selector"
          :disabled="isLoading"
        >
          <option value="" disabled>Select Machine</option>
          <option 
            v-for="machine in machinesStore.machines" 
            :key="machine.id" 
            :value="machine.id"
          >
            {{ machine.name }} ({{ machine.serialNumber }})
          </option>
        </select>
        
        <div class="control-buttons">
          <button 
            v-if="!isStreaming"
            @click="startStreaming"
            class="btn btn-primary"
            :disabled="!selectedMachineId || isLoading"
          >
            <Play class="btn-icon" />
            Start Streaming
          </button>
          
          <button 
            v-else
            @click="stopStreaming"
            class="btn btn-danger"
            :disabled="isLoading"
          >
            <Pause class="btn-icon" />
            Stop Streaming
          </button>
          
          <button 
            @click="restartStreaming"
            class="btn btn-secondary"
            :disabled="!isStreaming || isLoading"
          >
            <RotateCcw class="btn-icon" />
            Restart
          </button>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-header">
          <Activity class="stat-icon" />
          <span class="stat-label">Telemetry Points</span>
        </div>
        <div class="stat-value">{{ stats.totalTelemetryPoints.toLocaleString() }}</div>
        <div class="stat-trend positive">+12.5%</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-header">
          <TrendingUp class="stat-icon" />
          <span class="stat-label">Active Predictions</span>
        </div>
        <div class="stat-value">{{ stats.activePredictions }}</div>
        <div class="stat-trend neutral">Stable</div>
      </div>
      
      <div class="stat-card">
        <div class="stat-header">
          <AlertTriangle class="stat-icon warning" />
          <span class="stat-label">Recent Alerts</span>
        </div>
        <div class="stat-value">{{ stats.recentAlerts }}</div>
        <div class="stat-trend" :class="{ negative: stats.recentAlerts > 0 }">
          {{ stats.recentAlerts > 0 ? `${stats.recentAlerts} alerts` : 'None' }}
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-header">
          <Server class="stat-icon" />
          <span class="stat-label">System Health</span>
        </div>
        <div class="stat-value">{{ stats.systemHealthScore }}%</div>
        <div class="stat-trend positive">Excellent</div>
      </div>
    </div>

    <!-- Charts Section -->
    <div class="charts-section">
      <div class="chart-container">
        <h3 class="chart-title">Temperature Monitoring</h3>
        <div class="chart-wrapper">
          <!-- Temperature Chart would go here -->
          <div class="chart-placeholder">
            <Activity class="placeholder-icon" />
            <p>Live Temperature Data Stream</p>
            <div class="data-points" v-if="temperatureData.length > 0">
              Latest: {{ temperatureData[0]?.y?.toFixed(2) }}°C
            </div>
          </div>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">Vibration Analysis</h3>
        <div class="chart-wrapper">
          <!-- Vibration Chart would go here -->
          <div class="chart-placeholder">
            <TrendingUp class="placeholder-icon" />
            <p>Live Vibration Data Stream</p>
            <div class="data-points" v-if="vibrationData.length > 0">
              Latest: {{ vibrationData[0]?.y?.toFixed(3) }} mm/s
            </div>
          </div>
        </div>
      </div>
      
      <div class="chart-container">
        <h3 class="chart-title">Pressure Monitoring</h3>
        <div class="chart-wrapper">
          <!-- Pressure Chart would go here -->
          <div class="chart-placeholder">
            <Server class="placeholder-icon" />
            <p>Live Pressure Data Stream</p>
            <div class="data-points" v-if="pressureData.length > 0">
              Latest: {{ pressureData[0]?.y?.toFixed(1) }} PSI
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Subscription Status -->
    <div class="subscriptions-panel">
      <h3 class="panel-title">Active Streams</h3>
      <div class="subscriptions-list">
        <div 
          v-for="subscription in realTimeAnalyticsService.subscriptions.value" 
          :key="`${subscription.stream}-${subscription.machineId || 'global'}`"
          class="subscription-item"
        >
          <span class="stream-name">{{ subscription.stream }}</span>
          <span v-if="subscription.machineId" class="machine-id">
            ({{ subscription.machineId.substring(0, 8) }}...)
          </span>
        </div>
        
        <div v-if="realTimeAnalyticsService.subscriptions.value.length === 0" class="no-subscriptions">
          No active streams
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.real-time-analytics-dashboard {
  padding: 2rem;
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
  gap: 2rem;
  flex-wrap: wrap;
}

.header-content {
  flex: 1;
}

.dashboard-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 2rem;
  font-weight: 600;
  color: #f1f5f9;
  margin-bottom: 0.5rem;
}

.title-icon {
  width: 2rem;
  height: 2rem;
  color: #38bdf8;
}

.dashboard-description {
  color: #94a3b8;
  font-size: 1.1rem;
}

.header-controls {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  min-width: 300px;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 0.5rem;
  background: rgba(239, 68, 68, 0.1);
  color: #f87171;
  font-weight: 500;
}

.status-indicator.connected {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.status-icon {
  width: 1.25rem;
  height: 1.25rem;
}

.machine-selector {
  padding: 0.75rem;
  border-radius: 0.5rem;
  border: 1px solid #334155;
  background: #1e293b;
  color: #f1f5f9;
  font-size: 1rem;
}

.machine-selector:focus {
  outline: none;
  border-color: #38bdf8;
  box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.1);
}

.control-buttons {
  display: flex;
  gap: 0.75rem;
  flex-wrap: wrap;
}

.btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1.25rem;
  border-radius: 0.5rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #38bdf8;
  color: #0f172a;
}

.btn-primary:hover:not(:disabled) {
  background: #0ea5e9;
  transform: translateY(-1px);
}

.btn-danger {
  background: #f87171;
  color: #0f172a;
}

.btn-danger:hover:not(:disabled) {
  background: #ef4444;
  transform: translateY(-1px);
}

.btn-secondary {
  background: #64748b;
  color: #f1f5f9;
}

.btn-secondary:hover:not(:disabled) {
  background: #475569;
  transform: translateY(-1px);
}

.btn-icon {
  width: 1rem;
  height: 1rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: linear-gradient(145deg, #1e293b, #0f172a);
  border: 1px solid #334155;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.stat-header {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.stat-icon {
  width: 1.5rem;
  height: 1.5rem;
  color: #38bdf8;
}

.stat-icon.warning {
  color: #fbbf24;
}

.stat-label {
  color: #94a3b8;
  font-size: 0.9rem;
  font-weight: 500;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: #f1f5f9;
  margin-bottom: 0.5rem;
}

.stat-trend {
  font-size: 0.875rem;
  font-weight: 500;
}

.stat-trend.positive {
  color: #10b981;
}

.stat-trend.negative {
  color: #f87171;
}

.stat-trend.neutral {
  color: #94a3b8;
}

.charts-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1.5rem;
  margin-bottom: 2rem;
}

.chart-container {
  background: linear-gradient(145deg, #1e293b, #0f172a);
  border: 1px solid #334155;
  border-radius: 1rem;
  padding: 1.5rem;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.chart-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #f1f5f9;
  margin-bottom: 1rem;
}

.chart-wrapper {
  height: 300px;
}

.chart-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #94a3b8;
  text-align: center;
}

.placeholder-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.data-points {
  margin-top: 1rem;
  padding: 0.5rem 1rem;
  background: rgba(56, 189, 248, 0.1);
  border-radius: 0.5rem;
  color: #38bdf8;
  font-weight: 500;
}

.subscriptions-panel {
  background: linear-gradient(145deg, #1e293b, #0f172a);
  border: 1px solid #334155;
  border-radius: 1rem;
  padding: 1.5rem;
}

.panel-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #f1f5f9;
  margin-bottom: 1rem;
}

.subscriptions-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.subscription-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: rgba(56, 189, 248, 0.1);
  border: 1px solid rgba(56, 189, 248, 0.3);
  border-radius: 0.5rem;
  color: #38bdf8;
  font-size: 0.875rem;
}

.stream-name {
  font-weight: 500;
}

.machine-id {
  color: #94a3b8;
  font-family: monospace;
}

.no-subscriptions {
  color: #94a3b8;
  font-style: italic;
  padding: 1rem;
}

@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
  }
  
  .header-controls {
    min-width: unset;
  }
  
  .charts-section {
    grid-template-columns: 1fr;
  }
  
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>