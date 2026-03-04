<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { 
  getHealthStatus,
  getDatabaseHealth,
  isSystemHealthy,
  type HealthCheckResult
} from '@/services/health.service'
import { RefreshCw, Heart, Database, Wifi, Cloud, CheckCircle, XCircle, AlertTriangle, Activity, ShieldCheck, Cpu, HardDrive, Network, Zap } from 'lucide-vue-next'

const styles = useCssModule()

// Import ECharts
import * as echarts from 'echarts/core'
import {
  GaugeChart as EChartsGauge,
  BarChart as EChartsBar
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsGauge,
  EChartsBar,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const healthData = ref<HealthCheckResult | null>(null)
const databaseHealth = ref<any>(null)
const loading = ref(false)
const autoRefreshEnabled = ref(true)
const refreshInterval = ref<NodeJS.Timeout | null>(null)
const lastChecked = ref<string>('')

// Chart refs
const overallHealthChartRef = ref<HTMLDivElement | null>(null)
const componentStatusChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let overallHealthChartInstance: echarts.ECharts | null = null
let componentStatusChartInstance: echarts.ECharts | null = null

// Computed
const overallStatus = computed(() => {
  if (!healthData.value) return 'Unknown'
  return healthData.value.status
})

const kpiCards = computed(() => [
  { label: 'Core Components', value: totalComponents.value, icon: Cpu, variant: 'primary', progress: '100%' },
  { label: 'Stable Streams', value: healthyComponents.value, icon: Wifi, variant: 'success', progress: `${healthPercentage.value}%` },
  { label: 'Active Anomalies', value: totalComponents.value - healthyComponents.value, icon: AlertTriangle, variant: 'danger', progress: `${100 - healthPercentage.value}%` },
  { label: 'Data Latency', value: '4ms', icon: Network, variant: 'info', progress: '85%' },
])

function getStatusVariant(status: string) {
  if (status === 'Healthy') return 'success'
  if (status === 'Degraded') return 'warning'
  return 'danger'
}

const renderOverallHealthChart = () => {
  if (!overallHealthChartRef.value) return
  
  if (!overallHealthChartInstance) {
    overallHealthChartInstance = echarts.init(overallHealthChartRef.value, 'hub-dark')
  }

  const status = overallStatus.value
  let value = 0
  
  if (status === 'Healthy') value = 100
  else if (status === 'Degraded') value = 50
  
  const option = {
    backgroundColor: 'transparent',
    series: [{
      type: 'gauge',
      startAngle: 200,
      endAngle: -20,
      center: ['50%', '65%'],
      radius: '100%',
      min: 0,
      max: 100,
      splitNumber: 5,
      axisLine: {
        lineStyle: {
          width: 8,
          color: [
            [0.3, '#ef4444'],
            [0.7, '#f59e0b'],
            [1, '#10b981']
          ]
        }
      },
      pointer: {
        icon: 'path://M12.8,0.7l12,40.1H0.7L12.8,0.7z',
        length: '12%',
        width: 12,
        offsetCenter: [0, '-55%'],
        itemStyle: { color: 'auto' }
      },
      axisTick: { show: false },
      splitLine: { show: false },
      axisLabel: { show: false },
      title: { show: false },
      detail: {
        fontSize: 32,
        offsetCenter: [0, '15%'],
        valueAnimation: true,
        formatter: '{value}%',
        color: '#F8FAFC',
        fontFamily: 'var(--font-mono)'
      },
      data: [{ value: value }]
    }]
  }
  
  overallHealthChartInstance.setOption(option, true)
}

const renderComponentStatusChart = () => {
  if (!componentStatusChartRef.value || !healthData.value?.entries) return
  
  if (!componentStatusChartInstance) {
    componentStatusChartInstance = echarts.init(componentStatusChartRef.value, 'dark')
  }
  
  const entries = componentEntries.value
  
  const data = entries.map(entry => ({
    name: entry.name,
    value: entry.status === 'Healthy' ? 100 : entry.status === 'Degraded' ? 50 : 20,
    itemStyle: {
      color: entry.status === 'Healthy' ? '#10b981' : entry.status === 'Degraded' ? '#f59e0b' : '#ef4444',
      borderRadius: [4, 4, 0, 0]
    }
  }))
  
  const option = {
    backgroundColor: 'transparent',
    grid: { left: '2%', right: '2%', bottom: '10%', containLabel: true },
    tooltip: { trigger: 'axis' },
    xAxis: {
      type: 'category',
      data: entries.map(e => e.name),
      axisLabel: { color: 'rgba(255, 255, 255, 0.4)', fontSize: 10, rotate: 30 },
      axisLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.1)' } }
    },
    yAxis: {
      type: 'value',
      max: 100,
      splitLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.05)' } }
    },
    series: [{
      name: 'Integrity',
      type: 'bar',
      barWidth: '30%',
      data: data
    }]
  }
  
  componentStatusChartInstance.setOption(option, true)
}

const getStatusIcon = (status: string) => {
  if (status === 'Healthy') return CheckCircle
  if (status === 'Degraded') return AlertTriangle
  return XCircle
}

const getStatusColor = (status: string) => {
  if (status === 'Healthy') return 'text-green-600'
  if (status === 'Degraded') return 'text-yellow-600'
  return 'text-red-600'
}

const getStatusBgColor = (status: string) => {
  if (status === 'Healthy') return 'bg-green-100'
  if (status === 'Degraded') return 'bg-yellow-100'
  return 'bg-red-100'
}

const resizeCharts = () => {
  overallHealthChartInstance?.resize()
  componentStatusChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  checkHealth()
  if (autoRefreshEnabled.value) {
    startAutoRefresh()
  }
  window.addEventListener('resize', resizeCharts)
})

onUnmounted(() => {
  stopAutoRefresh()
  window.removeEventListener('resize', resizeCharts)
  overallHealthChartInstance?.dispose()
  componentStatusChartInstance?.dispose()
})
</script>

<template>
  <div :class="styles['health-container']">
    <!-- Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Activity :width="14" :height="14" />
          <span>Infrastructure Integrity</span>
        </div>
        <h1 :class="styles['title']">System Health</h1>
        <p :class="styles['description']">Real-time telemetry and component heartbeat monitoring</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton 
          variant="secondary" 
          @click="toggleAutoRefresh"
          :class="{ [styles['active-refresh']]: autoRefreshEnabled }"
        >
          <RefreshCw :width="16" :height="16" :class="{ [styles['spin']]: autoRefreshEnabled }" />
          Auto-Sync: {{ autoRefreshEnabled ? 'LIVE' : 'OFF' }}
        </UiButton>
        <UiButton 
          variant="primary" 
          @click="checkHealth"
          :loading="loading"
        >
          <ShieldCheck :width="16" :height="16" />
          Manual Probe
        </UiButton>
      </div>
    </header>

    <div :class="styles['content-layout']">
      <!-- Top Health Spotlight -->
      <section :class="styles['spotlight-grid']">
        <UiCard variant="glass" padding="lg" :class="styles['overall-card']">
          <div :class="styles['status-spotlight']">
            <div :class="[styles['status-blob'], styles[`status-blob--${overallStatus.toLowerCase()}`]]">
              <component :is="overallStatusIcon" :width="48" :height="48" />
            </div>
            <div :class="styles['status-info']">
              <span :class="styles['status-label']">Operational Status</span>
              <h2 :class="[styles['status-value'], styles[`text--${overallStatus.toLowerCase()}`]]">
                {{ overallStatus }}
              </h2>
              <div :class="styles['heartbeat-line']">
                <div v-for="i in 20" :key="i" :class="[styles['beat'], styles[`beat--${overallStatus.toLowerCase()}`]]" :style="{ animationDelay: `${i * 100}ms` }" />
              </div>
            </div>
          </div>
        </UiCard>

        <UiCard variant="default" padding="md" :class="styles['gauge-card']">
          <template #header>
            <div :class="styles['card-header']">
              <h3 :class="styles['card-title']">Global Health Index</h3>
              <UiBadge variant="primary" size="sm">Telemetric Score</UiBadge>
            </div>
          </template>
          <div ref="overallHealthChartRef" :class="styles['gauge-canvas']" />
        </UiCard>
      </section>

      <!-- Metric Bento Grid -->
      <section :class="styles['bento-metrics']">
        <UiCard v-for="kpi in kpiCards" :key="kpi.label" variant="glass" padding="md" :class="styles['bento-card']">
          <div :class="styles['kpi-content']">
            <div :class="[styles['kpi-icon-box'], styles[`icon--${kpi.variant}`]]">
              <component :is="kpi.icon" :width="20" :height="20" />
            </div>
            <div :class="styles['kpi-details']">
              <span :class="styles['kpi-value']">{{ kpi.value }}</span>
              <span :class="styles['kpi-label']">{{ kpi.label }}</span>
            </div>
          </div>
          <div :class="[styles['kpi-track'], styles[`track--${kpi.variant}`]]">
            <div :class="styles['kpi-progress']" :style="{ width: kpi.progress }" />
          </div>
        </UiCard>
      </section>

      <!-- Main Analysis Area -->
      <div :class="styles['analysis-grid']">
        <!-- Component Integrity Chart -->
        <UiCard variant="default" padding="lg" :class="styles['chart-card']">
          <template #header>
            <h3 :class="styles['card-title']">System Pulse Propagation</h3>
          </template>
          <div ref="componentStatusChartRef" :class="styles['bar-canvas']" />
        </UiCard>

        <!-- Detailed Component Feed -->
        <UiCard variant="default" padding="none" :class="styles['feed-card']">
          <template #header>
            <div :class="styles['feed-header']">
              <h3 :class="styles['card-title']">Component Heartbeats</h3>
              <span :class="styles['timestamp']">Updated: {{ lastChecked }}</span>
            </div>
          </template>
          
          <div :class="styles['component-feed']">
            <div
              v-for="entry in componentEntries"
              :key="entry.name"
              :class="[styles['feed-item'], styles[`feed-item--${entry.status.toLowerCase()}`]]"
            >
              <div :class="styles['item-main']">
                <div :class="[styles['item-indicator'], styles[`indicator--${entry.status.toLowerCase()}`]]" />
                <div :class="styles['item-info']">
                  <h4 :class="styles['item-name']">{{ entry.name.replace(/([A-Z])/g, ' $1').trim() }}</h4>
                  <p :class="styles['item-sub']">{{ entry.description || 'System component active' }}</p>
                </div>
              </div>
              <div :class="styles['item-stats']">
                <span :class="styles['latency']">{{ entry.duration }}</span>
                <UiBadge :variant="getStatusVariant(entry.status)" size="sm">{{ entry.status }}</UiBadge>
              </div>
            </div>
          </div>
        </UiCard>
      </div>

      <!-- Infrastructure Spotlight -->
      <section v-if="databaseHealth" :class="styles['infra-spotlight']">
        <UiCard variant="glass" padding="md">
          <div :class="styles['infra-content']">
            <div :class="styles['infra-brand']">
              <Database :width="24" :height="24" :class="[styles['infra-icon'], styles[`text--${databaseHealth.status.toLowerCase()}`]]" />
              <div :class="styles['infra-info']">
                <h4 :class="styles['infra-title']">Primary Data persistence</h4>
                <p :class="styles['infra-sub']">{{ databaseHealth.description || 'Core Database Cluster' }}</p>
              </div>
            </div>
            <div :class="styles['infra-status']">
              <div :class="styles['status-pill-group']">
                <div :class="[styles['status-dot'], styles[`dot--${databaseHealth.status.toLowerCase()}`]]" />
                <span :class="styles['status-text']">{{ databaseHealth.status }}</span>
              </div>
              <UiButton variant="ghost" size="sm">Inspect Logs</UiButton>
            </div>
          </div>
        </UiCard>
      </section>
    </div>
  </div>
</template>

<style module>
.health-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  padding: var(--space-24);
  max-width: 1600px;
  margin: 0 auto;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-20);
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.eyebrow {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--color-primary);
  margin-bottom: var(--space-4);
}

.title {
  font-size: var(--font-size-4xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}

.description {
  color: var(--color-text-muted);
  font-size: var(--font-size-sm);
  margin-top: var(--space-4);
}

.header-actions {
  display: flex;
  gap: var(--space-12);
}

.active-refresh {
  background: var(--color-primary-muted) !important;
  color: var(--color-primary) !important;
  border-color: var(--color-primary-glow) !important;
}

/* Spotlight Grid */
.spotlight-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-20);
}

.status-spotlight {
  display: flex;
  align-items: center;
  gap: var(--space-32);
}

.status-blob {
  width: 96px;
  height: 96px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
}

.status-blob--healthy { color: var(--color-success); box-shadow: 0 0 30px rgba(16, 185, 129, 0.2); }
.status-blob--degraded { color: var(--color-warning); box-shadow: 0 0 30px rgba(245, 158, 11, 0.2); }
.status-blob--unhealthy { color: var(--color-danger); box-shadow: 0 0 30px rgba(239, 68, 68, 0.2); }

.status-info {
  display: flex;
  flex-direction: column;
}

.status-label {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.status-value {
  font-size: var(--font-size-4xl);
  font-weight: 800;
  letter-spacing: -0.02em;
}

.heartbeat-line {
  display: flex;
  gap: 3px;
  margin-top: var(--space-12);
}

.beat {
  width: 4px;
  height: 12px;
  border-radius: 1px;
  background: var(--color-border-strong);
  animation: pulse-beat 2s infinite;
}

.beat--healthy { background: var(--color-success); opacity: 0.3; }
.beat--degraded { background: var(--color-warning); opacity: 0.3; }
.beat--unhealthy { background: var(--color-danger); opacity: 0.3; }

@keyframes pulse-beat {
  0%, 100% { transform: scaleY(1); opacity: 0.3; }
  50% { transform: scaleY(1.8); opacity: 1; }
}

.gauge-canvas {
  width: 100%;
  height: 180px;
}

/* Bento Metrics */
.bento-metrics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: var(--space-16);
}

.kpi-content {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  margin-bottom: var(--space-16);
}

.kpi-icon-box {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
}

.icon--primary { color: var(--color-primary); }
.icon--success { color: var(--color-success); }
.icon--danger { color: var(--color-danger); }
.icon--info { color: var(--color-info); }

.kpi-details {
  display: flex;
  flex-direction: column;
}

.kpi-value {
  font-size: var(--font-size-xl);
  font-weight: 700;
  color: var(--color-text-primary);
  line-height: 1;
}

.kpi-label {
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-top: 2px;
}

.kpi-track {
  height: 3px;
  background: var(--color-depth-0);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.kpi-progress {
  height: 100%;
  border-radius: var(--radius-full);
}

.track--primary .kpi-progress { background: var(--color-primary); }
.track--success .kpi-progress { background: var(--color-success); }
.track--danger .kpi-progress { background: var(--color-danger); }
.track--info .kpi-progress { background: var(--color-info); }

/* Analysis Grid */
.analysis-grid {
  display: grid;
  grid-template-columns: 1.5fr 1fr;
  gap: var(--space-24);
}

.bar-canvas {
  width: 100%;
  height: 360px;
}

/* Feed */
.feed-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.timestamp {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-dim);
}

.component-feed {
  display: flex;
  flex-direction: column;
}

.feed-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-16) var(--space-20);
  border-bottom: 1px solid var(--color-border-subtle);
  transition: all var(--transition-fast);
}

.feed-item:hover {
  background: rgba(255, 255, 255, 0.02);
}

.item-main {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.item-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

.indicator--healthy { background: var(--color-success); box-shadow: 0 0 10px var(--color-success); }
.indicator--degraded { background: var(--color-warning); box-shadow: 0 0 10px var(--color-warning); }
.indicator--unhealthy { background: var(--color-danger); box-shadow: 0 0 10px var(--color-danger); }

.item-info {
  display: flex;
  flex-direction: column;
}

.item-name {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.item-sub {
  font-size: 11px;
  color: var(--color-text-muted);
  margin: 0;
}

.item-stats {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: var(--space-4);
}

.latency {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-dim);
}

/* Infra spotlight */
.infra-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.infra-brand {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.infra-title {
  font-size: var(--font-size-sm);
  font-weight: 600;
  color: var(--color-text-primary);
  margin: 0;
}

.infra-sub {
  font-size: 11px;
  color: var(--color-text-muted);
  margin: 0;
}

.status-pill-group {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-4) var(--space-12);
  background: var(--color-depth-0);
  border-radius: var(--radius-full);
  border: 1px solid var(--color-border);
}

.status-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}

.dot--healthy { background: var(--color-success); }
.dot--degraded { background: var(--color-warning); }
.dot--unhealthy { background: var(--color-danger); }

.status-text {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-secondary);
}

.infra-status {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.text--healthy { color: var(--color-success); }
.text--degraded { color: var(--color-warning); }
.text--unhealthy { color: var(--color-danger); }

.spin {
  animation: spin 2s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (max-width: 1024px) {
  .spotlight-grid { grid-template-columns: 1fr; }
  .analysis-grid { grid-template-columns: 1fr; }
}
</style>