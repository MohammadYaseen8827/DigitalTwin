<template>
  <EnterpriseDashboardLayout
    title="Operations Control Hub"
    subtitle="Strategic orchestration of factory floor digital clusters and neural assets"
    :connection-status="connectionStatus"
    :last-update="lastUpdate"
    :alerts="alerts"
    :total-machines="24"
    @acknowledge-alerts="handleAcknowledge"
    @dismiss-alerts="dismissAlerts"
  >
    <!-- Top KPI Matrix -->
    <div :class="styles['control-deck']">
      <EnterpriseMetricCard
        v-for="metric in metrics"
        :key="metric.title"
        v-bind="metric"
        :class="styles['kpi-card']"
      />
    </div>

    <!-- Main Strategic Grid -->
    <div :class="styles['strategic-grid']">
      <!-- Left: Neural RUL Forensics -->
      <div :class="styles['hero-sector']">
        <RULPredictionPanel
          v-bind="rulPrediction"
          interpretation="Neural patterns indicate potential thermal drift in sector 4. Forensic protocols recommend predictive maintenance within 48 operational hours."
        />
        
        <UiCard variant="glass" padding="lg" :class="styles['integrity-card']" dots>
          <template #header>
            <div :class="styles['integrity-header']">
              <Activity :width="16" :height="16" :class="styles['header-icon']" />
              <h3 :class="styles['integrity-title']">Cluster Integrity Matrix</h3>
            </div>
          </template>
          <div :class="styles['health-matrix']">
            <div v-for="node in nodeIntegrity" :key="node.label" :class="styles['health-node']">
              <div :class="styles['node-meta']">
                <span :class="styles['node-label']">{{ node.label }}</span>
                <span :class="styles['node-val']">{{ node.value }}%</span>
              </div>
              <div :class="styles['node-track']">
                <div 
                  :class="[styles['node-fill'], styles[`node-fill--${node.status}`]]" 
                  :style="{ width: node.value + '%' }" 
                />
              </div>
            </div>
          </div>
        </UiCard>
      </div>

      <!-- Right: Real-time Telemetry & Intelligence -->
      <div :class="styles['intelligence-sector']">
        <UiCard variant="default" padding="none" :class="styles['telemetry-sector']" dots>
          <template #header>
             <div :class="styles['card-header-inner']">
               <div :class="styles['header-main']">
                 <h3 :class="styles['card-title']">Neural Telemetry Stream</h3>
                 <p :class="styles['card-sub']">Real-time sensor synthesis for CORE ASSET 01</p>
               </div>
               <div :class="styles['live-sync-badge']">
                 <div :class="styles['live-dot']" />
                 <span>LIVE SYNC</span>
               </div>
             </div>
          </template>
          <div :class="styles['chart-wrap']">
            <TelemetryChart
              title=""
              unit="°C"
              color="var(--color-primary)"
              :data="telemetryData"
              :current-value="telemetryData[telemetryData.length - 1]?.[1]"
              :is-live="true"
              :min-value="30"
              :max-value="100"
            />
          </div>
        </UiCard>

        <div :class="styles['sub-grid']">
          <SHAPExplanationPanel
            :features="shapFeatures"
            :confidence="0.94"
            prediction="degrading"
            :class="styles['shap-card']"
          />
          <ActivityFeed :class="styles['activity-feed']" />
        </div>
      </div>
    </div>
  </EnterpriseDashboardLayout>
</template>

<style module>
.control-deck {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-20);
}

.strategic-grid {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-24);
}

.hero-sector {
  grid-column: span 5;
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.intelligence-sector {
  grid-column: span 7;
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

/* Integrity Card */
.integrity-header {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.header-icon { color: var(--color-primary); }

.integrity-title {
  margin: 0;
  font-size: var(--font-size-base);
  font-weight: 800;
  letter-spacing: var(--font-tracking-tight);
}

.health-matrix {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.node-meta {
  display: flex;
  justify-content: space-between;
  margin-bottom: var(--space-6);
}

.node-label { font-size: 10px; font-weight: 800; color: var(--color-text-dim); text-transform: uppercase; letter-spacing: 0.1em; }
.node-val { font-family: var(--font-mono); font-size: var(--font-size-sm); font-weight: 700; color: var(--color-text-primary); }

.node-track {
  height: 2px;
  background: var(--color-border);
  border-radius: var(--radius-full);
  overflow: visible;
  position: relative;
}

.node-fill {
  height: 100%;
  border-radius: inherit;
  transition: width 1s var(--ease-premium);
  position: relative;
}

.node-fill::after {
  content: '';
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 4px;
  border-radius: 50%;
  background: inherit;
  box-shadow: 0 0 10px currentColor;
}

.node-fill--healthy { background: var(--color-success); color: var(--color-success); }
.node-fill--warning { background: var(--color-warning); color: var(--color-warning); }

/* Telemetry Card */
.card-header-inner {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: var(--space-20) var(--space-24) 0;
}

.card-title {
  margin: 0;
  font-size: var(--font-size-lg);
  font-weight: 800;
  letter-spacing: var(--font-tracking-tight);
}

.card-sub {
  margin: 2px 0 0;
  font-size: var(--font-size-xs);
  color: var(--color-text-dim);
  font-weight: 500;
}

.live-sync-badge {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: 4px 10px;
  background: var(--color-success-muted);
  color: var(--color-success);
  border-radius: var(--radius-full);
  font-size: 9px;
  font-weight: 900;
  letter-spacing: 0.08em;
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.live-dot {
  width: 6px;
  height: 6px;
  background: currentColor;
  border-radius: 50%;
  box-shadow: 0 0 8px currentColor;
  animation: pulse-ring 2s cubic-bezier(0.215, 0.61, 0.355, 1) infinite;
}

@keyframes pulse-ring {
  0% { transform: scale(0.5); opacity: 0.8; }
  80%, 100% { transform: scale(2.5); opacity: 0; }
}

.chart-wrap {
  padding: var(--space-12) var(--space-24) var(--space-24);
}

.sub-grid {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: var(--space-24);
  flex: 1;
}

.activity-feed {
  height: 100%;
}

@media (max-width: 1400px) {
  .hero-sector { grid-column: span 12; }
  .intelligence-sector { grid-column: span 12; }
}

@media (max-width: 1024px) {
  .control-deck { grid-template-columns: repeat(2, 1fr); }
  .sub-grid { grid-template-columns: 1fr; }
}
</style>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, useCssModule } from 'vue'
import EnterpriseDashboardLayout from '@/components/enterprise/EnterpriseDashboardLayout.vue'
import EnterpriseMetricCard from '@/components/enterprise/EnterpriseMetricCard.vue'
import TelemetryChart from '@/components/enterprise/TelemetryChart.vue'
import RULPredictionPanel from '@/components/enterprise/RULPredictionPanel.vue'
import SHAPExplanationPanel from '@/components/enterprise/SHAPExplanationPanel.vue'
import ActivityFeed from '@/components/layout/ActivityFeed.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { Zap, Activity } from 'lucide-vue-next'
import { dashboardService } from '@/services/dashboard.service'
import { alertsService } from '@/services/alerts.service'
import { telemetryService } from '@/services/telemetry.service'
import { predictionsService } from '@/services/predictions.service'
import type { DashboardStatsDto } from '@/services/dashboard.service'
import type { AlertDto } from '@/api/types'

const styles = useCssModule()

const connectionStatus = ref<'connected' | 'connecting' | 'disconnected'>('connected')
const lastUpdate = ref(new Date())
const isLoading = ref(true)

const dashboardStats = ref<DashboardStatsDto | null>(null)
const alerts = ref<AlertDto[]>([])
const telemetryData = ref<Array<[number, number]>>([])

const metrics = computed(() => {
  if (!dashboardStats.value) return []
  return [
    { title: 'Fleet Velocity', value: dashboardStats.value.totalMachines, status: 'healthy' as const, lastUpdated: lastUpdate.value },
    { title: 'Neural Nodes', value: dashboardStats.value.activeMachines, status: 'healthy' as const, lastUpdated: lastUpdate.value },
    { title: 'Cluster Efficiency', value: dashboardStats.value.averageHealthScore, unit: '%', status: dashboardStats.value.averageHealthScore > 80 ? 'healthy' as const : 'warning' as const, delta: 2.3, lastUpdated: lastUpdate.value },
    { title: 'Anomalies', value: dashboardStats.value.activeAlerts, status: dashboardStats.value.activeAlerts > 0 ? 'critical' as const : 'healthy' as const, lastUpdated: lastUpdate.value }
  ]
})

const nodeIntegrity = [
  { label: 'Neural CNC Cluster', value: 92, status: 'healthy' },
  { label: 'Injection Logic Nodes', value: 78, status: 'warning' },
  { label: 'Force Press Array', value: 85, status: 'healthy' },
  { label: 'Kinematic Robots', value: 95, status: 'healthy' }
]

const rulPrediction = ref({
  machineId: '0xFC-82',
  machineName: 'CORE ASSET 01',
  rul: 84,
  confidenceInterval: { lower: 72, upper: 96 },
  chartData: [] as any[]
})

const shapFeatures = ref<any[]>([
  { feature: 'Core Temp', value: 0.12 },
  { feature: 'Vibration Z', value: 0.08 },
  { feature: 'Hydraulic PSI', value: -0.04 }
])

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
    if (recentTelemetry?.length > 0) {
      telemetryData.value = recentTelemetry.map(t => [new Date(t.timestamp).getTime(), t.temperature ?? t.vibration ?? 0])
    }
    lastUpdate.value = new Date()
  } catch (error) {
    connectionStatus.value = 'disconnected'
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  await fetchDashboardData()
  refreshInterval = setInterval(fetchDashboardData, 30000)
})

onUnmounted(() => refreshInterval && clearInterval(refreshInterval))

async function handleAcknowledge(acknowledgedAlerts: any[]) {
  for (const alert of acknowledgedAlerts) await alertsService.acknowledgeAlert(alert.id)
  await fetchDashboardData()
}

async function dismissAlerts() {
  for (const alert of alerts.value) await alertsService.acknowledgeAlert(alert.id)
  await fetchDashboardData()
}
</script>

<style module>
.metrics-row {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-20);
}

.grid-item { min-width: 0; }
.rul-panel { grid-column: span 8; }
.health-summary { grid-column: span 4; }
.telemetry { grid-column: span 8; }
.shap-panel { grid-column: span 4; }

.health-card { height: 100%; }

.health-card__title {
  margin: 0;
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.health-bars {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.health-bar__header {
  display: flex;
  justify-content: space-between;
  margin-bottom: var(--space-6);
}

.health-bar__label { font-size: 11px; font-weight: 700; color: var(--color-text-secondary); text-transform: uppercase; }
.health-bar__value { font-family: var(--font-mono); font-size: var(--font-size-sm); font-weight: 700; color: var(--color-text-primary); }

.health-bar__track {
  height: 4px;
  background: var(--color-depth-1);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.health-bar__fill {
  height: 100%;
  border-radius: inherit;
  transition: width 1s var(--ease-premium);
}

.health-bar__fill--healthy { background: var(--color-success); box-shadow: 0 0 10px var(--color-success-muted); }
.health-bar__fill--warning { background: var(--color-warning); }

@media (max-width: 1280px) {
  .metrics-row { grid-template-columns: repeat(2, 1fr); }
  .rul-panel, .telemetry, .health-summary, .shap-panel { grid-column: span 6; }
}

@media (max-width: 1024px) {
  .rul-panel, .health-summary, .telemetry, .shap-panel { grid-column: span 12; }
}
</style>
