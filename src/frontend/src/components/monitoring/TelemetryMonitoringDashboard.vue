<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  Activity, 
  Thermometer, 
  Gauge, 
  Zap,
  Wifi,
  WifiOff,
  Filter,
  Search,
  Play,
  Pause,
  RotateCcw
} from 'lucide-vue-next'

const toast = useToast()

// State
const telemetryData = ref<any[]>([
  {
    machineId: 'M001',
    machineName: 'CNC Machine #1',
    temperature: 72.5,
    pressure: 45.2,
    vibration: 0.3,
    power: 2.1,
    status: 'online',
    lastUpdate: new Date().toISOString(),
    alerts: []
  },
  {
    machineId: 'M002',
    machineName: 'Injection Molder #2',
    temperature: 85.3,
    pressure: 62.8,
    vibration: 0.7,
    power: 3.4,
    status: 'online',
    lastUpdate: new Date(Date.now() - 30000).toISOString(),
    alerts: [{ type: 'warning', message: 'High vibration detected' }]
  },
  {
    machineId: 'M003',
    machineName: 'Conveyor System #1',
    temperature: 45.1,
    pressure: 28.9,
    vibration: 0.1,
    power: 1.8,
    status: 'offline',
    lastUpdate: new Date(Date.now() - 300000).toISOString(),
    alerts: []
  },
  {
    machineId: 'M004',
    machineName: 'Robot Arm #1',
    temperature: 68.7,
    pressure: 38.4,
    vibration: 0.2,
    power: 2.6,
    status: 'online',
    lastUpdate: new Date(Date.now() - 15000).toISOString(),
    alerts: []
  }
])

const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const selectedMachine = ref<string>('all')
const isStreaming = ref(false)
const updateInterval = ref<NodeJS.Timeout | null>(null)

// Chart data for visualization
const chartData = ref({
  timestamps: [] as string[],
  temperatures: [] as number[],
  pressures: [] as number[],
  vibrations: [] as number[]
})

// Computed
const filteredTelemetry = computed(() => {
  let filtered = [...telemetryData.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(data => 
      data.machineName.toLowerCase().includes(query) ||
      data.machineId.toLowerCase().includes(query)
    )
  }
  
  // Apply status filter
  if (statusFilter.value !== 'all') {
    filtered = filtered.filter(data => data.status === statusFilter.value)
  }
  
  // Apply machine filter
  if (selectedMachine.value !== 'all') {
    filtered = filtered.filter(data => data.machineId === selectedMachine.value)
  }
  
  return filtered
})

const machineOptions = computed(() => {
  const options = [{ label: 'All Machines', value: 'all' }]
  machines.value.forEach(machine => {
    options.push({ label: `${machine.name} (${machine.id})`, value: machine.id })
  })
  return options
})

const statusOptions = [
  { label: 'All Statuses', value: 'all' },
  { label: 'Online', value: 'online' },
  { label: 'Offline', value: 'offline' }
]

// Methods
const loadTelemetry = async () => {
  try {
    loading.value = true
    // In a real implementation, this would fetch from the telemetry service
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Telemetry data loaded successfully')
  } catch (error) {
    console.error('Error loading telemetry:', error)
    toast.error('Failed to load telemetry data')
  } finally {
    loading.value = false
  }
}

const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Error loading machines:', error)
  }
}

const startStreaming = () => {
  if (isStreaming.value) return
  
  isStreaming.value = true
  updateInterval.value = setInterval(updateTelemetryData, 5000)
  toast.success('Telemetry streaming started')
}

const pauseStreaming = () => {
  if (!isStreaming.value) return
  
  isStreaming.value = false
  if (updateInterval.value) {
    clearInterval(updateInterval.value)
    updateInterval.value = null
  }
  toast.success('Telemetry streaming paused')
}

const resetDataStream = () => {
  chartData.value = {
    timestamps: [],
    temperatures: [],
    pressures: [],
    vibrations: []
  }
  toast.success('Data stream reset')
}

const updateTelemetryData = () => {
  // Simulate real-time data updates
  telemetryData.value = telemetryData.value.map(data => ({
    ...data,
    temperature: data.temperature + (Math.random() - 0.5) * 2,
    pressure: data.pressure + (Math.random() - 0.5) * 3,
    vibration: Math.max(0, data.vibration + (Math.random() - 0.5) * 0.1),
    power: data.power + (Math.random() - 0.5) * 0.5,
    lastUpdate: new Date().toISOString()
  }))
  
  // Update chart data
  const now = new Date().toLocaleTimeString()
  chartData.value.timestamps.push(now)
  chartData.value.temperatures.push(telemetryData.value[0]?.temperature || 0)
  chartData.value.pressures.push(telemetryData.value[0]?.pressure || 0)
  chartData.value.vibrations.push(telemetryData.value[0]?.vibration || 0)
  
  // Keep only last 20 data points
  if (chartData.value.timestamps.length > 20) {
    chartData.value.timestamps.shift()
    chartData.value.temperatures.shift()
    chartData.value.pressures.shift()
    chartData.value.vibrations.shift()
  }
}

const getStatusIcon = (status: string) => {
  return status === 'online' ? Wifi : WifiOff
}

const getStatusColor = (status: string) => {
  return status === 'online' ? 'text-green-600' : 'text-red-600'
}

const getAlertLevel = (alerts: any[]) => {
  if (alerts.length === 0) return 'normal'
  return alerts.some(a => a.type === 'critical') ? 'critical' : 'warning'
}

const getAlertColor = (level: string) => {
  switch (level) {
    case 'critical': return 'text-red-600'
    case 'warning': return 'text-yellow-600'
    default: return 'text-green-600'
  }
}

const formatTime = (timestamp: string) => {
  return new Date(timestamp).toLocaleTimeString()
}

const getTimeSinceUpdate = (timestamp: string) => {
  const now = new Date()
  const update = new Date(timestamp)
  const diffSeconds = Math.floor((now.getTime() - update.getTime()) / 1000)
  
  if (diffSeconds < 60) return `${diffSeconds}s ago`
  if (diffSeconds < 3600) return `${Math.floor(diffSeconds / 60)}m ago`
  return `${Math.floor(diffSeconds / 3600)}h ago`
}

onMounted(() => {
  loadTelemetry()
  loadMachines()
  startStreaming()
})

onBeforeUnmount(() => {
  if (updateInterval.value) {
    clearInterval(updateInterval.value)
  }
})
</script>

<template>
  <BaseCard class="telemetry-monitoring">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Activity class="header-icon" />
          Telemetry Monitoring
        </h2>
        <p class="header-subtitle">Real-time monitoring of machine sensor data and metrics</p>
      </div>
      <div class="header-controls">
        <BaseButton
          :variant="isStreaming ? 'outline' : 'primary'"
          @click="isStreaming ? pauseStreaming() : startStreaming()"
        >
          <component :is="isStreaming ? Pause : Play" class="button-icon" />
          {{ isStreaming ? 'Pause' : 'Start' }} Streaming
        </BaseButton>
        <BaseButton variant="outline" @click="resetDataStream">
          <RotateCcw class="button-icon" />
          Reset Data
        </BaseButton>
      </div>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search machines..."
          class="search-input"
        >
          <template #prefix>
            <Search class="input-icon" />
          </template>
        </BaseInput>
        
        <BaseSelect
          v-model="statusFilter"
          :options="statusOptions"
          class="filter-select"
        />
        
        <BaseSelect
          v-model="selectedMachine"
          :options="machineOptions"
          class="filter-select"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <BaseSkeleton v-for="i in 6" :key="i" height="120px" class="mb-4" />
    </div>

    <!-- Telemetry Dashboard -->
    <div v-else class="telemetry-container">
      <div class="stats-overview">
        <BaseCard class="stat-card">
          <div class="stat-content">
            <Wifi class="stat-icon text-green-500" />
            <div>
              <div class="stat-value">{{ filteredTelemetry.filter(d => d.status === 'online').length }}</div>
              <div class="stat-label">Online</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <WifiOff class="stat-icon text-red-500" />
            <div>
              <div class="stat-value">{{ filteredTelemetry.filter(d => d.status === 'offline').length }}</div>
              <div class="stat-label">Offline</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <Thermometer class="stat-icon text-orange-500" />
            <div>
              <div class="stat-value">{{ Math.max(...filteredTelemetry.map(d => d.temperature)).toFixed(1) }}°C</div>
              <div class="stat-label">Max Temperature</div>
            </div>
          </div>
        </BaseCard>
        
        <BaseCard class="stat-card">
          <div class="stat-content">
            <Zap class="stat-icon text-blue-500" />
            <div>
              <div class="stat-value">{{ filteredTelemetry.reduce((sum, d) => sum + d.power, 0).toFixed(1) }}kW</div>
              <div class="stat-label">Total Power</div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Telemetry Cards Grid -->
      <div class="telemetry-grid">
        <BaseCard
          v-for="data in filteredTelemetry"
          :key="data.machineId"
          class="telemetry-card"
        >
          <div class="card-header">
            <div class="machine-info">
              <h3>{{ data.machineName }}</h3>
              <p class="machine-id">{{ data.machineId }}</p>
            </div>
            <div class="status-indicators">
              <component 
                :is="getStatusIcon(data.status)" 
                class="status-icon"
                :class="getStatusColor(data.status)"
              />
              <span 
                class="alert-indicator"
                :class="getAlertColor(getAlertLevel(data.alerts))"
              >
                {{ data.alerts.length }} alerts
              </span>
            </div>
          </div>
          
          <div class="metrics-grid">
            <div class="metric-item">
              <Thermometer class="metric-icon text-red-500" />
              <div class="metric-data">
                <div class="metric-value">{{ data.temperature.toFixed(1) }}°C</div>
                <div class="metric-label">Temperature</div>
              </div>
            </div>
            
            <div class="metric-item">
              <Gauge class="metric-icon text-blue-500" />
              <div class="metric-data">
                <div class="metric-value">{{ data.pressure.toFixed(1) }} PSI</div>
                <div class="metric-label">Pressure</div>
              </div>
            </div>
            
            <div class="metric-item">
              <Activity class="metric-icon text-purple-500" />
              <div class="metric-data">
                <div class="metric-value">{{ data.vibration.toFixed(2) }}</div>
                <div class="metric-label">Vibration</div>
              </div>
            </div>
            
            <div class="metric-item">
              <Zap class="metric-icon text-yellow-500" />
              <div class="metric-data">
                <div class="metric-value">{{ data.power.toFixed(1) }} kW</div>
                <div class="metric-label">Power</div>
              </div>
            </div>
          </div>
          
          <div class="card-footer">
            <div class="last-update">
              Last update: {{ formatTime(data.lastUpdate) }}
            </div>
            <div class="time-ago">
              {{ getTimeSinceUpdate(data.lastUpdate) }}
            </div>
          </div>
          
          <div v-if="data.alerts.length > 0" class="alerts-section">
            <div 
              v-for="(alert, index) in data.alerts" 
              :key="index"
              class="alert-item"
              :class="`alert-${alert.type}`"
            >
              <span class="alert-type">{{ alert.type.toUpperCase() }}</span>
              <span class="alert-message">{{ alert.message }}</span>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.telemetry-monitoring {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

.header-content {
  flex: 1;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.header-icon {
  width: 1.5rem;
  height: 1.5rem;
  color: #3b82f6;
}

.header-subtitle {
  color: #64748b;
  font-size: 1rem;
}

.header-controls {
  display: flex;
  gap: 0.75rem;
}

.button-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
}

.filters-section {
  margin: 1.5rem 0;
}

.filter-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  align-items: center;
}

.search-input {
  flex: 1;
  min-width: 250px;
}

.input-icon {
  width: 1rem;
  height: 1rem;
  color: #94a3b8;
}

.filter-select {
  min-width: 150px;
}

.loading-container {
  padding: 2rem;
}

.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  padding: 1rem;
}

.stat-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  width: 2rem;
  height: 2rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
}

.stat-label {
  font-size: 0.875rem;
  color: #64748b;
}

.telemetry-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.telemetry-card {
  padding: 1.5rem;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.machine-info h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.machine-id {
  font-size: 0.75rem;
  color: #64748b;
}

.status-indicators {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.status-icon {
  width: 1.25rem;
  height: 1.25rem;
}

.alert-indicator {
  font-size: 0.75rem;
  font-weight: 500;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  margin-bottom: 1rem;
}

.metric-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  background: #f8fafc;
  border-radius: 0.5rem;
}

.metric-icon {
  width: 1.5rem;
  height: 1.5rem;
  flex-shrink: 0;
}

.metric-data {
  flex: 1;
}

.metric-value {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
}

.metric-label {
  font-size: 0.75rem;
  color: #64748b;
  text-transform: uppercase;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
  font-size: 0.75rem;
  color: #64748b;
}

.alerts-section {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

.alert-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem;
  border-radius: 0.25rem;
  margin-bottom: 0.25rem;
  font-size: 0.75rem;
}

.alert-item:last-child {
  margin-bottom: 0;
}

.alert-critical {
  background: #fee2e2;
  color: #dc2626;
}

.alert-warning {
  background: #ffedd5;
  color: #ea580c;
}

.alert-type {
  font-weight: 600;
  text-transform: uppercase;
}

.alert-message {
  flex: 1;
}

@media (max-width: 768px) {
  .telemetry-monitoring {
    padding: 0.5rem;
  }
  
  .header-controls {
    flex-direction: column;
    width: 100%;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .stats-overview {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .telemetry-grid {
    grid-template-columns: 1fr;
  }
  
  .metrics-grid {
    grid-template-columns: 1fr;
  }
  
  .card-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .status-indicators {
    flex-direction: row;
    align-items: center;
    width: 100%;
    justify-content: space-between;
  }
}
</style>