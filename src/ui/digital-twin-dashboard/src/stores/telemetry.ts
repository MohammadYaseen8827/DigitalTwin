import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { telemetryService } from '@/services/telemetry.service'
import type { TelemetryDto } from '@/api/types'

export const useTelemetryStore = defineStore('telemetry', () => {
  const telemetryData = ref<TelemetryDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedTimeRange = ref({ start: null, end: null })

  // Computed properties
  const latestTelemetry = computed(() => {
    return telemetryData.value[0] || null
  })

  const telemetryByTimestamp = computed(() => {
    const grouped = telemetryData.value.reduce((acc, point) => {
      const timestamp = point.timestamp.split('T')[0] // Group by date
      if (!acc[timestamp]) {
        acc[timestamp] = []
      }
      acc[timestamp].push(point)
      return acc
    }, {} as Record<string, TelemetryDto[]>)
    
    return grouped
  })

  const temperatureData = computed(() => {
    return telemetryData.value
      .filter(point => point.temperature !== null)
      .map(point => ({
        timestamp: point.timestamp,
        value: point.temperature,
        unit: '°C'
      }))
      .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
  })

  const vibrationData = computed(() => {
    return telemetryData.value
      .filter(point => point.vibration !== null)
      .map(point => ({
        timestamp: point.timestamp,
        value: point.vibration,
        unit: 'mm/s'
      }))
      .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
  })

  const pressureData = computed(() => {
    return telemetryData.value
      .filter(point => point.pressure !== null)
      .map(point => ({
        timestamp: point.timestamp,
        value: point.pressure,
        unit: 'PSI'
      }))
      .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
  })

  const healthScoreData = computed(() => {
    return telemetryData.value
      .filter(point => point.healthScore !== null)
      .map(point => ({
        timestamp: point.timestamp,
        value: point.healthScore,
        unit: 'Score'
      }))
      .sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime())
  })

  const recentTelemetry = computed(() => {
    return telemetryData.value.slice(0, 100) // Last 100 points
  })

  // Actions
  const fetchTelemetry = async (machineId: string, timeRange?: { start: Date, end: Date }) => {
    loading.value = true
    error.value = null
    
    try {
      if (timeRange) {
        telemetryData.value = await telemetryService.getTelemetryByTimeRangeAsync(machineId, timeRange.start, timeRange.end)
      } else {
        telemetryData.value = await telemetryService.getRecentTelemetryAsync(machineId, 100)
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch telemetry'
    } finally {
      loading.value = false
    }
  }

  const setTimeRange = (start: Date, end: Date) => {
    selectedTimeRange.value = { start, end }
  }

  const clearTimeRange = () => {
    selectedTimeRange.value = { start: null, end: null }
  }

  return {
    // State
    telemetryData: readonly(telemetryData),
    loading: readonly(loading),
    error: readonly(error),
    selectedTimeRange: readonly(selectedTimeRange),
    
    // Computed
    latestTelemetry,
    telemetryByTimestamp,
    temperatureData,
    vibrationData,
    pressureData,
    healthScoreData,
    recentTelemetry,
    
    // Actions
    fetchTelemetry,
    setTimeRange,
    clearTimeRange
  }
})
