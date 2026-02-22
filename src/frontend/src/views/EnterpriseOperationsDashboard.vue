<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import EnterpriseDashboardLayout from '@/components/enterprise/EnterpriseDashboardLayout.vue'
import EnterpriseMetricCard from '@/components/enterprise/EnterpriseMetricCard.vue'
import TelemetryChart from '@/components/enterprise/TelemetryChart.vue'
import RULPredictionPanel from '@/components/enterprise/RULPredictionPanel.vue'
import SHAPExplanationPanel from '@/components/enterprise/SHAPExplanationPanel.vue'
import AlertBanner from '@/components/enterprise/AlertBanner.vue'

// Connection status
const connectionStatus = ref<'connected' | 'connecting' | 'disconnected'>('connected')
const lastUpdate = ref(new Date())

// Mock alerts
const alerts = ref([
  {
    id: '1',
    machineName: 'CNC Machine #12',
    message: 'Temperature exceeded 85°C threshold',
    timestamp: new Date(),
    severity: 'critical' as const
  },
  {
    id: '2',
    machineName: 'Injection Molder #3',
    message: 'Vibration anomaly detected',
    timestamp: new Date(),
    severity: 'warning' as const
  }
])

// Mock metrics
const metrics = computed(() => [
  {
    title: 'Total Machines',
    value: 24,
    status: 'healthy' as const,
    lastUpdated: lastUpdate.value
  },
  {
    title: 'Active',
    value: 22,
    status: 'healthy' as const,
    lastUpdated: lastUpdate.value
  },
  {
    title: 'Avg Efficiency',
    value: 87.5,
    unit: '%',
    status: 'healthy' as const,
    delta: 2.3,
    lastUpdated: lastUpdate.value
  },
  {
    title: 'Active Alerts',
    value: alerts.value.length,
    status: alerts.value.length > 0 ? 'critical' as const : 'healthy' as const,
    lastUpdated: lastUpdate.value
  }
])

// Generate mock telemetry data
function generateTelemetryData(points: number = 100): Array<[number, number]> {
  const data: Array<[number, number]> = []
  const now = Date.now()
  let value = 65

  for (let i = points; i >= 0; i--) {
    const timestamp = now - i * 1000
    // Add some realistic variation
    value = value + (Math.random() - 0.5) * 2
    value = Math.max(40, Math.min(90, value))
    data.push([timestamp, value])
  }

  return data
}

const telemetryData = ref<Array<[number, number]>>([])

// RUL prediction data
const rulPrediction = ref({
  machineId: 'CNC-001',
  machineName: 'CNC Machine #1',
  rul: 168,
  confidenceInterval: {
    lower: 120,
    upper: 220
  },
  chartData: [] as Array<{ timestamp: number; prediction: number; lower: number; upper: number }>
})

// SHAP data
const shapFeatures = ref([
  { feature: 'Temperature', value: 0.45 },
  { feature: 'Vibration', value: 0.32 },
  { feature: 'Pressure', value: 0.18 },
  { feature: 'Rotation Speed', value: -0.12 },
  { feature: 'Power Consumption', value: 0.08 },
  { feature: 'Operating Hours', value: -0.05 },
  { feature: 'Maintenance Age', value: 0.03 },
  { feature: 'Ambient Temp', value: -0.02 }
])

// Update telemetry periodically
let intervalId: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  // Initialize with mock data
  telemetryData.value = generateTelemetryData(100)

  // Generate RUL chart data
  const now = Date.now()
  rulPrediction.value.chartData = Array.from({ length: 24 }, (_, i) => ({
    timestamp: now + i * 3600000,
    prediction: Math.max(0, 168 - i * 7),
    lower: Math.max(0, 120 - i * 5),
    upper: 220 - i * 4
  }))

  // Simulate real-time updates
  intervalId = setInterval(() => {
    // Add new data point
    const lastPoint = telemetryData.value[telemetryData.value.length - 1]
    const newValue = lastPoint[1] + (Math.random() - 0.5) * 3
    const clampedValue = Math.max(40, Math.min(90, newValue))

    telemetryData.value = [
      ...telemetryData.value.slice(1),
      [Date.now(), clampedValue]
    ] as Array<[number, number]>

    lastUpdate.value = new Date()
  }, 2000)
})

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})

function handleAcknowledge(acknowledgedAlerts: any[]) {
  alerts.value = alerts.value.filter(
    a => !acknowledgedAlerts.find(ack => ack.id === a.id)
  )
}

function dismissAlerts() {
  alerts.value = []
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
