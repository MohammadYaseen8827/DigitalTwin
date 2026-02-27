<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import EnterpriseDashboardLayout from '@/components/enterprise/EnterpriseDashboardLayout.vue'
import EnterpriseMetricCard from '@/components/enterprise/EnterpriseMetricCard.vue'
import TelemetryChart from '@/components/enterprise/TelemetryChart.vue'
import RULPredictionPanel from '@/components/enterprise/RULPredictionPanel.vue'
import SHAPExplanationPanel from '@/components/enterprise/SHAPExplanationPanel.vue'
import AlertBanner from '@/components/enterprise/AlertBanner.vue'

// API Services
import { dashboardService } from '@/services/dashboard.service'
import { alertsService } from '@/services/alerts.service'
import { telemetryService } from '@/services/telemetry.service'
import { predictionsService } from '@/services/predictions.service'
import type { DashboardStatsDto } from '@/services/dashboard.service'
import type { AlertDto } from '@/api/types'

// Connection status
const connectionStatus = ref<'connected' | 'connecting' | 'disconnected'>('connected')
const lastUpdate = ref(new Date())
const isLoading = ref(true)

// Dashboard data
const dashboardStats = ref<DashboardStatsDto | null>(null)
const alerts = ref<AlertDto[]>([])
const telemetryData = ref<Array<[number, number]>>([])

// Metrics computed from dashboard stats
const metrics = computed(() => {
  if (!dashboardStats.value) return []
  
  return [
    {
      title: 'Total Machines',
      value: dashboardStats.value.totalMachines,
      status: 'healthy' as const,
      lastUpdated: lastUpdate.value
    },
    {
      title: 'Active',
      value: dashboardStats.value.activeMachines,
      status: 'healthy' as const,
      lastUpdated: lastUpdate.value
    },
    {
      title: 'Avg Efficiency',
      value: dashboardStats.value.averageHealthScore,
      unit: '%',
      status: dashboardStats.value.averageHealthScore > 80 ? 'healthy' as const : 'warning' as const,
      delta: dashboardStats.value.averageHealthScore > 85 ? 2.3 : -1.5,
      lastUpdated: lastUpdate.value
    },
    {
      title: 'Active Alerts',
      value: dashboardStats.value.activeAlerts,
      status: dashboardStats.value.activeAlerts > 0 ? 'critical' as const : 'healthy' as const,
      lastUpdated: lastUpdate.value
    }
  ]
})

// RUL prediction data
const rulPrediction = ref({
  machineId: 'loading...',
  machineName: 'Equipment #1',
  rul: 0,
  confidenceInterval: {
    lower: 0,
    upper: 0
  },
  chartData: [] as Array<{ timestamp: number; prediction: number; lower: number; upper: number }>
})

// SHAP data
const shapFeatures = ref<Array<{ feature: string; value: number }>>([])

// Interval for real-time updates
let refreshInterval: ReturnType<typeof setInterval> | null = null

async function fetchDashboardData() {
  try {
    const [stats, activeAlerts, recentTelemetry] = await Promise.all([
      dashboardService.getDashboardStats(),
      alertsService.fetchActiveAlerts(),
      telemetryService.fetchRecentTelemetry({ limit: 50 })
    ])
    
    dashboardStats.value = stats
    alerts.value = activeAlerts
    
    // Process telemetry data for chart
    if (recentTelemetry && recentTelemetry.length > 0) {
      telemetryData.value = recentTelemetry.map(t => [
        new Date(t.timestamp).getTime(),
        t.temperature ?? t.vibration ?? 0
      ])
    }
    
    // Fetch prediction for the first machine if available
    const machineId = activeAlerts[0]?.machineId || recentTelemetry[0]?.machineId
    if (machineId) {
      const prediction = await predictionsService.requestRulPrediction(machineId)
      const summary = await predictionsService.getRulSummary(machineId)
      
      rulPrediction.value = {
        machineId,
        machineName: `Machine ${machineId.substring(0, 8)}`,
        rul: prediction.remainingUsefulLifeDays,
        confidenceInterval: prediction.confidenceInterval,
        chartData: [] // History would be fetched here if needed
      }
      
      // Update SHAP features
      shapFeatures.value = Object.entries(prediction.featureContributions || {}).map(([feature, value]) => ({
        feature,
        value
      }))
    }
    
    lastUpdate.value = new Date()
    connectionStatus.value = 'connected'
  } catch (error) {
    console.error('Failed to fetch dashboard data:', error)
    connectionStatus.value = 'disconnected'
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await fetchDashboardData()
  
  // Refresh data every 30 seconds
  refreshInterval = setInterval(fetchDashboardData, 30000)
})

onUnmounted(() => {
  if (refreshInterval) {
    clearInterval(refreshInterval)
  }
})

async function handleAcknowledge(acknowledgedAlerts: any[]) {
  try {
    for (const alert of acknowledgedAlerts) {
      await alertsService.acknowledgeAlert(alert.id)
    }
    await fetchDashboardData()
  } catch (error) {
    console.error('Failed to acknowledge alerts:', error)
  }
}

async function dismissAlerts() {
  try {
    // Dismissing all alerts by acknowledging them
    for (const alert of alerts.value) {
      await alertsService.acknowledgeAlert(alert.id)
    }
    await fetchDashboardData()
  } catch (error) {
    console.error('Failed to dismiss alerts:', error)
  }
}
</script>

<template>
  <EnterpriseDashboardLayout
    title="Operations Dashboard"
    subtitle="Real-time equipment monitoring and predictive analytics"
    :connection-status="connectionStatus"
    :last-update="lastUpdate"
    :alerts="alerts"
    alert-title="Active Alerts"
    :total-machines="24"
    @acknowledge-alerts="handleAcknowledge"
    @dismiss-alerts="dismissAlerts"
  >
    <!-- Key Metrics Row -->
    <div class="metrics-row">
      <EnterpriseMetricCard
        v-for="metric in metrics"
        :key="metric.title"
        :title="metric.title"
        :value="metric.value"
        :unit="metric.unit"
        :status="metric.status"
        :delta="metric.delta"
        :last-updated="metric.lastUpdated"
      />
    </div>

    <!-- Main Content Grid -->
    <!-- RUL Prediction Panel - Left Column -->
    <div class="grid-item rul-panel">
      <RULPredictionPanel
        :machine-id="rulPrediction.machineId"
        :machine-name="rulPrediction.machineName"
        :rul="rulPrediction.rul"
        :confidence-interval="rulPrediction.confidenceInterval"
        :chart-data="rulPrediction.chartData"
        interpretation="High temperature and vibration levels are the primary factors reducing remaining useful life. Consider scheduling maintenance within the next 7 days."
      />
    </div>

    <!-- Equipment Health Summary - Right Column -->
    <div class="grid-item health-summary">
      <div class="health-card">
        <h3 class="health-card__title">Equipment Health Summary</h3>
        <div class="health-bars">
          <div class="health-bar">
            <div class="health-bar__header">
              <span class="health-bar__label">CNC Machines</span>
              <span class="health-bar__value">92%</span>
            </div>
            <div class="health-bar__track">
              <div class="health-bar__fill health-bar__fill--healthy" style="width: 92%" />
            </div>
          </div>
          <div class="health-bar">
            <div class="health-bar__header">
              <span class="health-bar__label">Injection Molders</span>
              <span class="health-bar__value">78%</span>
            </div>
            <div class="health-bar__track">
              <div class="health-bar__fill health-bar__fill--warning" style="width: 78%" />
            </div>
          </div>
          <div class="health-bar">
            <div class="health-bar__header">
              <span class="health-bar__label">Press Machines</span>
              <span class="health-bar__value">85%</span>
            </div>
            <div class="health-bar__track">
              <div class="health-bar__fill health-bar__fill--healthy" style="width: 85%" />
            </div>
          </div>
          <div class="health-bar">
            <div class="health-bar__header">
              <span class="health-bar__label">Robots</span>
              <span class="health-bar__value">95%</span>
            </div>
            <div class="health-bar__track">
              <div class="health-bar__fill health-bar__fill--healthy" style="width: 95%" />
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Live Telemetry Chart -->
    <div class="grid-item telemetry">
      <TelemetryChart
        title="Live Temperature"
        unit="°C"
        color="#38BDF8"
        :data="telemetryData"
        :current-value="telemetryData[telemetryData.length - 1]?.[1]"
        :is-live="true"
        :min-value="30"
        :max-value="100"
      />
    </div>

    <!-- SHAP Explanation Panel -->
    <div class="grid-item shap-panel">
      <SHAPExplanationPanel
        :features="shapFeatures"
        :confidence="0.87"
        prediction="degrading"
      />
    </div>
  </EnterpriseDashboardLayout>
</template>

<style scoped>
.metrics-row {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-16);
}

@media (max-width: 1280px) {
  .metrics-row {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 640px) {
  .metrics-row {
    grid-template-columns: 1fr;
  }
}

.grid-item {
  min-width: 0;
}

.rul-panel {
  grid-column: span 8;
}

.health-summary {
  grid-column: span 4;
}

.telemetry {
  grid-column: span 8;
}

.shap-panel {
  grid-column: span 4;
}

@media (max-width: 1280px) {
  .rul-panel,
  .telemetry {
    grid-column: span 6;
  }

  .health-summary,
  .shap-panel {
    grid-column: span 6;
  }
}

@media (max-width: 768px) {
  .rul-panel,
  .health-summary,
  .telemetry,
  .shap-panel {
    grid-column: span 12;
  }
}

/* Health Card Styles */
.health-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-16);
  height: 100%;
}

.health-card__title {
  margin: 0 0 var(--space-16);
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
}

.health-bars {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.health-bar__header {
  display: flex;
  justify-content: space-between;
  margin-bottom: var(--space-4);
}

.health-bar__label {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.health-bar__value {
  font-family: var(--font-mono);
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
}

.health-bar__track {
  height: 8px;
  background: var(--color-surface-elevated);
  border-radius: 4px;
  overflow: hidden;
}

.health-bar__fill {
  height: 100%;
  border-radius: 4px;
  transition: width var(--transition-normal);
}

.health-bar__fill--healthy {
  background: var(--color-success);
}

.health-bar__fill--warning {
  background: var(--color-warning);
}

.health-bar__fill--critical {
  background: var(--color-danger);
}
</style>
