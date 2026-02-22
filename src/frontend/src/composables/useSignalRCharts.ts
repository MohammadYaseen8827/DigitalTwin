/**
 * SignalR Charts Composable
 * Provides reactive sensor data from SignalR real-time updates
 */
import { ref, onMounted, onUnmounted } from 'vue'
import { signalRService } from '@/services/signalr.service'

interface SensorDataPoint {
  timestamp: string | Date
  value: number
}

interface UseRealtimeSensorChartOptions {
  machineId: string
  sensorType: string
}

export function useRealtimeSensorChart(machineId: string, sensorType: string) {
  const isConnected = ref(false)
  const isLoading = ref(true)
  const lastUpdate = ref<Date | null>(null)
  const sensorData = ref<SensorDataPoint[]>([])
  const error = ref<string | null>(null)

  // Callback for new data from SignalR
  function handleTelemetryUpdate(data: any) {
    if (data.machineId === machineId && data.sensorType === sensorType) {
      const newPoint: SensorDataPoint = {
        timestamp: new Date(data.timestamp || Date.now()),
        value: data.value
      }
      
      // Keep last 100 points for performance
      sensorData.value = [...sensorData.value.slice(-99), newPoint]
      lastUpdate.value = new Date()
    }
  }

  async function loadData(_timeRangeMinutes: number = 60) {
    isLoading.value = true
    error.value = null
    
    try {
      // Simulate initial data load - in real app would call API
      const now = Date.now()
      const mockData: SensorDataPoint[] = []
      let baseValue = sensorType === 'temperature' ? 65 : sensorType === 'vibration' ? 5 : 50
      
      for (let i = 60; i >= 0; i--) {
        mockData.push({
          timestamp: new Date(now - i * 60000),
          value: baseValue + (Math.random() - 0.5) * 10
        })
      }
      
      sensorData.value = mockData
      lastUpdate.value = new Date()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to load data'
    } finally {
      isLoading.value = false
    }
  }

  function subscribeToMachine() {
    // Subscribe to SignalR updates
    // In real implementation, this would hook into signalRService
  }

  function unsubscribeFromMachine() {
    // Unsubscribe from SignalR updates
  }

  // Initialize connection status
  isConnected.value = signalRService.isConnected.value

  return {
    isConnected,
    isLoading,
    lastUpdate,
    sensorData,
    error,
    loadData,
    subscribeToMachine,
    unsubscribeFromMachine
  }
}
