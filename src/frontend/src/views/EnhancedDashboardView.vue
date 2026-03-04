const lastUpdatedLabel = computed(() => {
  if (!lastRefreshed.value) return 'Never'

  const now = new Date()
  const diffMs = now.getTime() - lastRefreshed.value.getTime()
  const diffSeconds = Math.floor(diffMs / 1000)

  if (diffSeconds < 5) return 'Just now'
  if (diffSeconds < 60) return `${diffSeconds}s ago`

  const diffMinutes = Math.floor(diffSeconds / 60)
  if (diffMinutes < 60) return `${diffMinutes}m ago`

  const diffHours = Math.floor(diffMinutes / 60)
  if (diffHours < 24) return `${diffHours}h ago`

  return lastRefreshed.value.toLocaleString()
})

const chartHasData = computed(() => machines.value.some(machine => normalizeStatus(machine.status) !== 'unknown'))

<template>
  <SectionContainer title="Operations Intelligence" eyebrow="Digital Twin Ecosystem" maxWidth="full">
    <template #actions>
      <div class="header-actions">
        <div class="refresh-meta">
          <span class="label">Last updated</span>
          <span class="value">{{ lastUpdatedLabel }}</span>
        </div>
        <UiButton variant="outline" size="sm" :loading="refreshing" @click="refreshAllData">
          <RefreshCw :width="14" :height="14" :class="{ 'spin': refreshing }" />
          Refresh Workspace
        </UiButton>
      </div>
    </template>

    <!-- Top Status Bar -->
    <UiCard variant="glass" padding="sm" class="navigation-bar">
      <div class="system-time">
        <div class="time-label">System Time</div>
        <div class="time-value">{{ currentTime }}</div>
      </div>
      
      <div class="tab-scroller">
        <div class="tab-list" role="tablist">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            :class="['tab-item', { 'tab-item--active': activeTab === tab.id }]"
            role="tab"
            :aria-selected="activeTab === tab.id"
            @click="activeTab = tab.id"
          >
            <component :is="tab.icon" :width="14" :height="14" />
            <span>{{ tab.name }}</span>
            <div v-if="activeTab === tab.id" class="tab-indicator" />
          </button>
        </div>
      </div>
    </UiCard>

    <div class="viewport-stage">
      <!-- OVERVIEW PANEL -->
      <div v-if="activeTab === 'overview'" class="stage-panel">
        <div class="bento-grid">
          <!-- Metrics Section -->
          <UiCard variant="default" padding="lg" class="bento-item metrics-panel">
            <template #header>
              <div class="panel-header">
                <LayoutDashboard :width="16" :height="16" />
                <span>Performance KPIs</span>
              </div>
            </template>
            
            <div class="metrics-container">
              <div class="stat-box">
                <span class="stat-label">Total Fleet</span>
                <MetricValue :value="totalMachines" class="stat-number" />
              </div>
              <div class="stat-box">
                <span class="stat-label">Active Units</span>
                <MetricValue :value="activeMachines" class="stat-number" />
              </div>
              <div class="stat-box">
                <span class="stat-label">Efficiency</span>
                <MetricValue :value="avgEfficiency" :precision="1" suffix="%" class="stat-number" />
              </div>
              <div class="stat-box">
                <span class="stat-label">Temperature</span>
                <MetricValue :value="avgTemperature" :precision="0" suffix="°C" class="stat-number" />
              </div>
            </div>
          </UiCard>

          <!-- Distribution Chart -->
          <UiCard variant="default" padding="lg" class="bento-item chart-panel">
            <template #header>
              <div class="panel-header">
                <Activity :width="16" :height="16" />
                <span>Status Distribution</span>
              </div>
            </template>
            <div v-if="chartHasData && !loading" ref="statusChart" class="status-chart" role="img"></div>
            <div v-else class="empty-chart">
              <Database :width="32" :height="32" />
              <p>Waiting for telemetry stream...</p>
            </div>
          </UiCard>

          <!-- Quick Actions -->
          <UiCard variant="default" padding="lg" class="bento-item actions-panel">
            <template #header>
              <div class="panel-header">
                <Zap :width="16" :height="16" />
                <span>Quick Orchestration</span>
              </div>
            </template>
            <div class="action-stack">
              <UiButton variant="primary" size="md" @click="startAllSimulations">
                <Play :width="14" :height="14" />
                Start All Simulations
              </UiButton>
              <UiButton variant="outline" size="md" @click="stopAllSimulations">
                <Square :width="14" :height="14" />
                Stop All Processes
              </UiButton>
              <UiButton variant="ghost" size="md" @click="runPredictiveAnalytics">
                <BrainCircuit :width="14" :height="14" />
                Execute Prediction Cycle
              </UiButton>
            </div>
          </UiCard>
        </div>
      </div>

      <UiCard v-else-if="activeTab === '3d-view'" variant="glass" padding="none" class="viz-stage">
        <Machine3DViewer />
      </UiCard>

      <UiCard v-else-if="activeTab === 'telemetry'" variant="glass" padding="none" class="viz-stage">
        <TelemetryDashboard />
      </UiCard>

      <UiCard v-else-if="activeTab === 'floor-plan'" variant="glass" padding="none" class="viz-stage">
        <FloorPlanHeatmap />
      </UiCard>

      <UiCard v-else-if="activeTab === 'analytics'" variant="glass" padding="none" class="viz-stage">
        <PredictiveAnalyticsDashboard />
      </UiCard>

      <UiCard v-else-if="activeTab === 'maintenance-lifecycle'" variant="glass" padding="none" class="viz-stage">
        <MaintenanceDashboard />
      </UiCard>

      <div v-else-if="activeTab === 'prescriptive'" class="prescriptive-stage">
        <UiCard variant="default" padding="sm" class="machine-selector">
          <template #header>Analyze Asset</template>
          <div class="selector-list">
            <button 
              v-for="m in machines" 
              :key="m.id" 
              :class="['selector-item', { active: selectedMachine?.id === m.id }]"
              @click="selectedMachine = m"
            >
              {{ m.name }}
            </button>
          </div>
        </UiCard>
        <PrescriptiveAnalysis v-if="selectedMachine" :machine-id="selectedMachine.id" />
        <div v-else class="empty-viz">
          <BrainCircuit :width="48" :height="48" />
          <p>Select a digital twin to initialize prescriptive analysis.</p>
        </div>
      </div>

      <div v-else-if="activeTab === 'machine-management'" class="stage-panel">
        <UiCard variant="default" padding="none" class="management-card">
          <template #header>
            <div class="panel-header-row">
              <div class="header-copy">
                <h3>Industrial Fleet Inventory</h3>
                <p>Manage and orchestrate machine digital twins</p>
              </div>
              <UiButton variant="primary" size="sm" @click="openCreateMachine">
                <Plus :width="14" :height="14" />
                Register Machine
              </UiButton>
            </div>
          </template>

          <div v-if="loading" class="skeleton-list">
            <BaseSkeleton v-for="i in 4" :key="i" height="84px" radius="12px" />
          </div>

          <div v-else-if="machines.length === 0" class="empty-list">
            <Database :width="40" :height="40" />
            <p>Fleet database is empty.</p>
          </div>

          <div v-else class="data-list">
            <div v-for="machine in machines" :key="machine.id" class="list-row">
              <div class="row-main">
                <div class="row-icon">
                  <SettingsIcon :width="18" :height="18" />
                </div>
                <div class="row-info">
                  <div class="row-title">{{ machine.name }}</div>
                  <div class="row-meta">
                    <span class="meta-tag">{{ machine.type }}</span>
                    <UiBadge :variant="normalizeStatus(machine.status) as any" size="sm" dot>
                      {{ normalizeStatus(machine.status) }}
                    </UiBadge>
                    <span v-if="machine.location" class="meta-loc">📍 {{ machine.location }}</span>
                  </div>
                </div>
              </div>
              
              <div class="row-actions">
                <UiButton variant="outline" size="sm" @click="openEditMachine(machine)">
                  Configure
                </UiButton>
                <UiButton variant="danger" size="sm" @click="openDeleteMachine(machine)">
                  Decommission
                </UiButton>
              </div>
            </div>
          </div>
        </UiCard>

        <SimulationControlPanel
          v-if="selectedMachine"
          :machine-id="selectedMachine.id"
          :simulation-id="simulationMap[selectedMachine.id]"
          @updated="handleSimulationUpdated"
        />
      </div>
    </div>

    <MachineForm
      v-if="showMachineForm"
      :machine="isEditing ? selectedMachine : undefined"
      @cancel="closeMachineForm"
      @success="handleMachineSuccess"
    />

    <MachineDeleteDialog
      :open="showDeleteDialog"
      :machine="selectedMachine"
      @cancel="closeDeleteDialog"
      @success="handleDeleteSuccess"
    />
  </SectionContainer>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import * as echarts from 'echarts'

import SectionContainer from '../components/base/SectionContainer.vue'
import UiCard from '../components/ui/UiCard.vue'
import UiButton from '../components/ui/UiButton.vue'
import UiBadge from '../components/ui/UiBadge.vue'
import BaseSkeleton from '../components/base/BaseSkeleton.vue'
import BaseStatSkeleton from '../components/base/BaseStatSkeleton.vue'
import MetricValue from '../components/base/MetricValue.vue'

import MachineForm from '@/components/machines/MachineForm.vue'
import MachineDeleteDialog from '@/components/machines/MachineDeleteDialog.vue'
import SimulationControlPanel from '@/components/simulation/SimulationControlPanel.vue'
import Machine3DViewer from '../components/visualizations/Machine3DViewer.vue'
import TelemetryDashboard from '../components/visualizations/TelemetryDashboard.vue'
import FloorPlanHeatmap from '../components/visualizations/FloorPlanHeatmap.vue'
import PredictiveAnalyticsDashboard from '../components/visualizations/PredictiveAnalyticsDashboard.vue'
import MaintenanceDashboard from '../components/visualizations/MaintenanceDashboard.vue'
import PrescriptiveAnalysis from '../components/visualizations/PrescriptiveAnalysis.vue'

import { fetchMachines as fetchMachinesService } from '@/services/machines.service'
import {
  listSimulations,
  createSimulation,
  cancelSimulation,
  runSimulationStep,
  type SimulationStateDto
} from '@/services/simulation.service'
import { dashboardService, type DashboardStatsDto } from '@/services/dashboard.service'
import { requestPrediction } from '@/services/predictions.service'
import type { MachineDto, EquipmentStatus } from '@/api/types'
import { RefreshCw, Play, Square, Activity, Database, Zap, Map as MapIcon, BarChart3, Settings as SettingsIcon, Plus, LayoutDashboard, BrainCircuit, Wrench } from 'lucide-vue-next'

// Reactive references
const machines = ref<MachineDto[]>([])
const dashboardStats = ref<DashboardStatsDto | null>(null)
const selectedMachine = ref<MachineDto | null>(null)
const showMachineForm = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const simulationMap = ref<Record<string, string>>({})

const activeTab = ref('overview')
const refreshing = ref(false)
const loading = ref(false)
const currentTime = ref(new Date().toLocaleTimeString())
const lastRefreshed = ref<Date | null>(null)

const statusChart = ref<HTMLDivElement | null>(null)
let statusChartInstance: echarts.ECharts | null = null
let timeInterval: number | null = null

const toast = useToast()

// Updated Tabs with Icons
const tabs = [
  { id: 'overview', name: 'Overview', icon: LayoutDashboard },
  { id: 'machine-management', name: 'Fleet', icon: SettingsIcon },
  { id: '3d-view', name: '3D Spatial', icon: Zap },
  { id: 'telemetry', name: 'Signals', icon: Activity },
  { id: 'floor-plan', name: 'Factory Map', icon: MapIcon },
  { id: 'analytics', name: 'Predictive', icon: BarChart3 },
  { id: 'prescriptive', name: 'Optimization', icon: BrainCircuit },
  { id: 'maintenance-lifecycle', name: 'Maintenance', icon: Wrench }
]

// Computed properties for metrics
const totalMachines = computed(() => machines.value.length)
const activeMachines = computed(() =>
  machines.value.filter(m => normalizeStatus(m.status) === 'operational').length
)
const avgEfficiency = computed(() => dashboardStats.value?.overallEfficiency ?? 0)
const avgTemperature = computed(() => dashboardStats.value?.averageTemperature ?? 0)

// Methods
const fetchData = async () => {
  try {
    loading.value = true
    const [machinesData, statsData] = await Promise.all([
      fetchMachinesService(),
      dashboardService.getDashboardStats()
    ])
    machines.value = machinesData
    dashboardStats.value = statsData
    updateStatusChart()
    lastRefreshed.value = new Date()
  } catch (error) {
    console.error('Error fetching dashboard data:', error)
    toast.error('Unable to load dashboard data right now.')
  } finally {
    loading.value = false
  }
}

const refreshAllData = async () => {
  refreshing.value = true
  try {
    await fetchData()
    toast.success('Dashboard data refreshed.')
  } catch (error) {
    console.error('Error refreshing data:', error)
    toast.error('Failed to refresh dashboard data.')
  } finally {
    refreshing.value = false
  }
}

const startAllSimulations = async () => {
  try {
    await Promise.all(
      machines.value.map(async machine => {
        const simulation = await createSimulation(machine.id, {
          degradationModel: 'wiener',
          totalSteps: 50,
          intervalSeconds: 5,
          persistTelemetry: true
        })
        simulationMap.value[machine.id] = simulation.id
        return runSimulationStep(simulation.id, machine.id)
      })
    )
    toast.success('Simulations started across all machines.')
    await fetchData()
  } catch (error) {
    console.error('Failed to start simulations:', error)
    toast.error('Unable to start simulations.')
  }
}

const stopAllSimulations = async () => {
  try {
    const cancelPromises = Object.entries(simulationMap.value).map(([machineId, simulationId]) => {
      if (simulationId) return cancelSimulation(simulationId, machineId)
      return Promise.resolve()
    })
    await Promise.all(cancelPromises)
    simulationMap.value = {}
    toast.info('Stopped all active simulations.')
    await fetchData()
  } catch (error) {
    console.error('Failed to stop simulations:', error)
    toast.error('Unable to cancel simulations.')
  }
}

const generateAllData = async () => {
  try {
    await Promise.all(
      machines.value.map(async machine => {
        const simulation = await createSimulation(machine.id, {
          degradationModel: 'wiener',
          totalSteps: 10,
          intervalSeconds: 15,
          persistTelemetry: true
        })
        return runSimulationStep(simulation.id, machine.id)
      })
    )
    toast.success('Telemetry generation started.')
  } catch (error) {
    console.error('Failed to generate telemetry:', error)
    toast.error('Unable to generate telemetry.')
  }
}

const runPredictiveAnalytics = async () => {
  try {
    await Promise.all(machines.value.map(m => requestPrediction(m.id)))
    toast.success('Predictive analytics triggered.')
  } catch (error) {
    console.error('Failed to run analytics:', error)
  }
}

const updateStatusChart = () => {
  if (!statusChart.value) return
  if (!statusChartInstance) statusChartInstance = echarts.init(statusChart.value, 'hub-dark')
  
  const statusCounts: Record<string, number> = {}
  machines.value.forEach(m => {
    const normalized = normalizeStatus(m.status)
    statusCounts[normalized] = (statusCounts[normalized] || 0) + 1
  })

  const statuses = Object.keys(statusCounts)
  const data = statuses.map(s => ({
    value: statusCounts[s],
    name: formatStatus(s),
    itemStyle: { color: statusColorMap[s as keyof typeof statusColorMap] || statusColorMap.unknown }
  }))

  statusChartInstance.setOption({
    tooltip: { trigger: 'item' },
    legend: { top: '5%', left: 'center', textStyle: { color: '#ffffff' } },
    series: [{
      name: 'Machine Status',
      type: 'pie',
      radius: ['40%', '70%'],
      avoidLabelOverlap: false,
      itemStyle: { borderRadius: 10, borderColor: '#fff', borderWidth: 2 },
      label: { show: false, position: 'center' },
      emphasis: { label: { show: true, fontSize: 20, fontWeight: 'bold' } },
      data
    }]
  })
}

const updateTime = () => currentTime.value = new Date().toLocaleTimeString()
const handleResize = () => statusChartInstance?.resize()

const normalizeStatus = (status: EquipmentStatus): string => {
  const s = status.toLowerCase()
  if (['operational', 'maintenance', 'warning', 'critical', 'offline'].includes(s)) return s
  return 'unknown'
}

const formatStatus = (status: string) => status.charAt(0).toUpperCase() + status.slice(1)

const statusColorMap = {
  operational: '#22c55e',
  maintenance: '#3b82f6',
  warning: '#facc15',
  critical: '#ef4444',
  offline: '#64748b',
  unknown: '#0ea5e9'
}

const openCreateMachine = () => {
  selectedMachine.value = null
  isEditing.value = false
  showMachineForm.value = true
}

const openEditMachine = (machine: MachineDto) => {
  selectedMachine.value = machine
  isEditing.value = true
  showMachineForm.value = true
}

const openDeleteMachine = (machine: MachineDto) => {
  selectedMachine.value = machine
  showDeleteDialog.value = true
}

const closeMachineForm = () => {
  showMachineForm.value = false
  selectedMachine.value = null
  isEditing.value = false
}

const closeDeleteDialog = () => {
  showDeleteDialog.value = false
  selectedMachine.value = null
}

const handleMachineSuccess = async () => {
  closeMachineForm()
  await fetchData()
  toast.success(isEditing.value ? 'Machine updated' : 'Machine created')
}

const handleDeleteSuccess = async () => {
  closeDeleteDialog()
  await fetchData()
}

const handleSimulationUpdated = (simulation: SimulationStateDto | null) => {
  if (simulation) simulationMap.value[simulation.machineId] = simulation.id
}

onMounted(() => {
  fetchData()
  updateTime()
  timeInterval = window.setInterval(updateTime, 1000)
  window.addEventListener('resize', handleResize)
})

onBeforeUnmount(() => {
  if (timeInterval) clearInterval(timeInterval)
  window.removeEventListener('resize', handleResize)
  statusChartInstance?.dispose()
})
</script>

<style scoped>
.header-actions {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.refresh-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.refresh-meta .label {
  font-size: 9px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.refresh-meta .value {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
}

.navigation-bar {
  display: flex;
  align-items: center;
  gap: var(--space-32);
  border-radius: var(--radius-md);
  border-color: var(--color-border-subtle);
}

.system-time {
  padding: 0 var(--space-8);
  border-right: 1px solid var(--color-border-subtle);
  flex-shrink: 0;
}

.time-label {
  font-size: 9px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.time-value {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-primary);
  font-family: var(--font-mono);
}

.tab-scroller {
  flex: 1;
  overflow-x: auto;
  scrollbar-width: none;
}

.tab-scroller::-webkit-scrollbar { display: none; }

.tab-list {
  display: flex;
  gap: var(--space-4);
}

.tab-item {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-8) var(--space-16);
  background: transparent;
  border: none;
  border-radius: var(--radius-sm);
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-fast);
  white-space: nowrap;
  position: relative;
}

.tab-item:hover {
  color: var(--color-text-primary);
  background: rgba(255, 255, 255, 0.03);
}

.tab-item--active {
  color: var(--color-primary);
  background: var(--color-primary-muted);
}

.tab-indicator {
  position: absolute;
  bottom: 4px;
  left: var(--space-16);
  right: var(--space-16);
  height: 2px;
  background: var(--color-primary);
  border-radius: var(--radius-full);
  box-shadow: var(--glow-sm);
}

.viewport-stage {
  min-height: 600px;
}

.stage-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  animation: fade-rise 0.4s ease-out;
}

/* Bento Grid */
.bento-grid {
  display: grid;
  grid-template-columns: repeat(12, 1fr);
  gap: var(--space-20);
}

.metrics-panel { grid-column: span 12; }
.chart-panel { grid-column: span 8; }
.actions-panel { grid-column: span 4; }

@media (max-width: 1200px) {
  .chart-panel { grid-column: span 12; }
  .actions-panel { grid-column: span 12; }
}

.panel-header {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.metrics-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-24);
}

.stat-box {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.stat-label {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-muted);
}

.stat-number {
  font-size: var(--font-size-4xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
}

.status-chart {
  height: 320px;
  width: 100%;
}

.action-stack {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

/* Management List */
.management-card {
  border-color: var(--color-border-subtle);
}

.panel-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-4) 0;
}

.header-copy h3 {
  margin: 0;
  font-size: var(--font-size-lg);
  font-weight: 700;
  color: var(--color-text-primary);
}

.header-copy p {
  margin: 0;
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
}

.data-list {
  display: flex;
  flex-direction: column;
}

.list-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-16) var(--space-20);
  border-bottom: 1px solid var(--color-border-subtle);
  transition: background var(--transition-fast);
}

.list-row:hover {
  background: rgba(255, 255, 255, 0.01);
}

.row-main {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.row-icon {
  width: 40px;
  height: 40px;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-dim);
}

.row-title {
  font-size: var(--font-size-base);
  font-weight: 600;
  color: var(--color-text-primary);
}

.row-meta {
  display: flex;
  align-items: center;
  gap: var(--space-12);
  margin-top: 2px;
}

.meta-tag {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.meta-loc {
  font-size: 11px;
  color: var(--color-text-muted);
}

.row-actions {
  display: flex;
  gap: var(--space-8);
}

.viz-stage {
  height: 700px;
  border-color: var(--color-border-subtle);
  background: var(--color-depth-0);
}

/* Prescriptive */
.prescriptive-stage {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: var(--space-24);
  align-items: start;
}

.selector-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.selector-item {
  width: 100%;
  text-align: left;
  padding: var(--space-10) var(--space-12);
  background: transparent;
  border: 1px solid transparent;
  border-radius: var(--radius-md);
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.selector-item:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.selector-item.active {
  background: var(--color-primary-muted);
  color: var(--color-primary);
  border-color: var(--color-border-focus);
}

.empty-viz {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-64);
  color: var(--color-text-dim);
  text-align: center;
  gap: var(--space-16);
}

.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@keyframes fade-rise {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
