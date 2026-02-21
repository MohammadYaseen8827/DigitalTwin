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
  <SectionContainer title="Enhanced Operations Dashboard" eyebrow="Simulation Insights" maxWidth="full">
    <template #actions>
      <div class="refresh-controls">
        <div class="refresh-meta">
          <span class="label">Last updated</span>
          <span class="value">{{ lastUpdatedLabel }}</span>
        </div>
        <BaseButton variant="primary" size="sm" :disabled="refreshing" @click="refreshAllData">
          <span v-if="refreshing">Refreshing...</span>
          <span v-else>↻ Refresh All</span>
        </BaseButton>
      </div>
    </template>

    <BaseCard variant="glass" class="status-summary">
      <div class="status-time">
        <span class="label">Current Time</span>
        <span class="value">{{ currentTime }}</span>
      </div>
      <div class="tab-group" role="tablist">
        <BaseButton
          v-for="tab in tabs"
          :key="tab.id"
          size="sm"
          :variant="activeTab === tab.id ? 'primary' : 'ghost'"
          class="tab-pill"
          role="tab"
          :aria-selected="activeTab === tab.id"
          @click="activeTab = tab.id"
        >
          <span class="tab-label">{{ tab.name }}</span>
          <span v-if="tab.caption" class="tab-caption">{{ tab.caption }}</span>
        </BaseButton>
      </div>
    </BaseCard>

    <div class="tab-panels">
      <div v-if="activeTab === 'overview'" class="tab-panel">
        <div class="overview-grid">
          <BaseCard :loading="loading" variant="soft" class="metrics-card">
            <template #header>Key Metrics</template>
            <template #loading>
              <div class="metric-skeleton-grid">
                <BaseStatSkeleton v-for="i in 4" :key="`metric-skeleton-${i}`" />
              </div>
            </template>
            <div v-if="!loading" class="metrics-grid">
              <BaseCard variant="solid" class="stat-card">
                <div class="metric-icon">🏭</div>
                <MetricValue :value="totalMachines" />
                <div class="metric-label">Total Machines</div>
              </BaseCard>
              <BaseCard variant="solid" class="stat-card">
                <div class="metric-icon">⚙️</div>
                <MetricValue :value="activeMachines" />
                <div class="metric-label">Active Machines</div>
              </BaseCard>
              <BaseCard variant="solid" class="stat-card">
                <div class="metric-icon">⚡</div>
                <MetricValue :value="avgEfficiency" :precision="1" suffix="%" />
                <div class="metric-label">Avg. Efficiency</div>
              </BaseCard>
              <BaseCard variant="solid" class="stat-card">
                <div class="metric-icon">🌡️</div>
                <MetricValue :value="avgTemperature" :precision="0" suffix="°C" />
                <div class="metric-label">Avg. Temperature</div>
              </BaseCard>
            </div>
          </BaseCard>

          <BaseCard :loading="loading" variant="soft" class="chart-card">
            <template #header>Machine Status Distribution</template>
            <template #loading>
              <div class="loading-stack">
                <BaseSkeleton height="220px" radius="var(--radius-lg)" />
              </div>
            </template>
            <div v-if="chartHasData && !loading" ref="statusChart" class="chart-container" role="img" aria-label="Machine status distribution chart"></div>
            <div v-else-if="!loading" class="chart-empty">
              <div class="chart-empty-icon">📈</div>
              <h4>No machine telemetry yet</h4>
              <p>Refresh the dashboard or start simulations to populate the status distribution.</p>
            </div>
          </BaseCard>

          <BaseCard :loading="loading" variant="soft" class="actions-card">
            <template #header>Quick Actions</template>
            <template #loading>
              <div class="actions-skeleton">
                <BaseSkeleton v-for="i in 4" :key="`action-skeleton-${i}`" height="48px" />
              </div>
            </template>
            <div class="actions-grid" v-if="!loading">
              <BaseButton class="action-button" variant="primary" size="md" @click="startAllSimulations">
                <span class="action-icon">▶️</span>
                <span class="action-label">Start All Simulations</span>
              </BaseButton>
              <BaseButton class="action-button" variant="secondary" size="md" @click="stopAllSimulations">
                <span class="action-icon">⏹️</span>
                <span class="action-label">Stop All Simulations</span>
              </BaseButton>
              <BaseButton class="action-button" variant="outline" size="md" @click="generateAllData">
                <span class="action-icon">📊</span>
                <span class="action-label">Generate Telemetry Data</span>
              </BaseButton>
              <BaseButton class="action-button" variant="ghost" size="md" @click="runPredictiveAnalytics">
                <span class="action-icon">🔮</span>
                <span class="action-label">Run Predictive Analytics</span>
              </BaseButton>
            </div>
          </BaseCard>
        </div>
      </div>

      <BaseCard v-else-if="activeTab === '3d-view'" variant="soft" class="tab-card">
        <Machine3DViewer />
      </BaseCard>

      <BaseCard v-else-if="activeTab === 'telemetry'" variant="soft" class="tab-card">
        <TelemetryDashboard />
      </BaseCard>

      <BaseCard v-else-if="activeTab === 'floor-plan'" variant="soft" class="tab-card">
        <FloorPlanHeatmap />
      </BaseCard>

      <BaseCard v-else-if="activeTab === 'analytics'" variant="soft" class="tab-card">
        <PredictiveAnalyticsDashboard />
      </BaseCard>

      <BaseCard v-else-if="activeTab === 'maintenance-lifecycle'" variant="soft" class="tab-card">
        <MaintenanceDashboard />
      </BaseCard>

      <div v-else-if="activeTab === 'prescriptive'" class="tab-panel">
        <div class="prescriptive-layout">
          <BaseCard variant="soft" class="machine-picker">
            <template #header>Select Machine for Analysis</template>
            <div class="picker-list">
              <div 
                v-for="m in machines" 
                :key="m.id" 
                class="picker-item" 
                :class="{ active: selectedMachine?.id === m.id }"
                @click="selectedMachine = m"
              >
                {{ m.name }}
              </div>
            </div>
          </BaseCard>
          <PrescriptiveAnalysis v-if="selectedMachine" :machine-id="selectedMachine.id" />
          <div v-else class="empty-state glass-panel">
            <p>Please select a machine to run prescriptive what-if simulations.</p>
          </div>
        </div>
      </div>

      <div v-else-if="activeTab === 'machine-management'" class="tab-panel">
        <BaseCard variant="soft" class="machine-management-card">
          <template #header>
            <div class="card-header-row">
              <span>Machine Inventory</span>
              <BaseButton variant="primary" size="sm" @click="openCreateMachine">
                + Add Machine
              </BaseButton>
            </div>
          </template>

          <div v-if="loading" class="machine-list-skeleton">
            <BaseSkeleton v-for="i in 3" :key="`machine-skeleton-${i}`" height="80px" />
          </div>

          <div v-else-if="machines.length === 0" class="empty-machines">
            <p>No machines found. Create your first machine to get started.</p>
          </div>

          <div v-else class="machine-list">
            <div v-for="machine in machines" :key="machine.id" class="machine-item">
              <div class="machine-info">
                <h4>{{ machine.name }}</h4>
                <p class="machine-meta">
                  <span class="machine-type">{{ machine.type }}</span>
                  <span class="machine-status" :class="`status-${normalizeStatus(machine.status)}`">
                    {{ formatStatus(normalizeStatus(machine.status)) }}
                  </span>
                  <span v-if="machine.location" class="machine-location">📍 {{ machine.location }}</span>
                </p>
              </div>
              <div class="machine-actions">
                <BaseButton variant="outline" size="sm" @click="openEditMachine(machine)">
                  Edit
                </BaseButton>
                <BaseButton variant="critical" size="sm" @click="openDeleteMachine(machine)">
                  Delete
                </BaseButton>
              </div>
            </div>
          </div>
        </BaseCard>

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
import BaseCard from '../components/base/BaseCard.vue'
import BaseButton from '../components/base/BaseButton.vue'
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

import { requestPrediction } from '@/services/predictions.service'
import type { MachineDto, EquipmentStatus } from '@/api/types'

// Reactive references
const machines = ref<MachineDto[]>([])
const selectedMachine = ref<MachineDto | null>(null)
const showMachineForm = ref(false)
const showDeleteDialog = ref(false)
const isEditing = ref(false)
const simulationMap = ref<Record<string, string>>({})
const showSimulationPanel = ref(false)

const activeTab = ref('overview')
const refreshing = ref(false)
const loading = ref(false)
const currentTime = ref(new Date().toLocaleTimeString())
const lastRefreshed = ref<Date | null>(null)

const statusChart = ref<HTMLDivElement | null>(null)
let statusChartInstance: echarts.ECharts | null = null
let timeInterval: number | null = null

const toast = useToast()

// Tabs configuration
const tabs = [
  { id: 'overview', name: 'Operations Overview', caption: 'Fleet status & KPIs' },
  { id: 'machine-management', name: 'Machine Management', caption: 'CRUD & simulations' },
  { id: '3d-view', name: '3D Layout', caption: 'Spatial awareness' },
  { id: 'telemetry', name: 'Telemetry', caption: 'Live signals' },
  { id: 'floor-plan', name: 'Factory Map', caption: 'Floor heatmap' },
  { id: 'analytics', name: 'Predictive Analytics', caption: 'Forecasts & risks' },
  { id: 'prescriptive', name: 'Prescriptive', caption: 'Optimization' },
  { id: 'maintenance-lifecycle', name: 'Maintenance Hub', caption: 'Job tracking' }
]

// Computed properties for metrics
const totalMachines = computed(() => machines.value.length)
const activeMachines = computed(() =>
  machines.value.filter(m => normalizeStatus(m.status) === 'operational').length
)
const avgEfficiency = computed(() => {
  // Simulate average efficiency
  return Math.floor(Math.random() * 20 + 80)
})
const avgTemperature = computed(() => {
  // Simulate average temperature
  return Math.floor(Math.random() * 30 + 180)
})

// Methods
const fetchMachines = async () => {
  try {
    loading.value = true
    machines.value = await fetchMachinesService()
    updateStatusChart()
    lastRefreshed.value = new Date()
  } catch (error) {
    console.error('Error fetching machines:', error)
    toast.error('Unable to load machine overview right now.')
  } finally {
    loading.value = false
  }
}

const refreshAllData = async () => {
  refreshing.value = true
  try {
    await fetchMachines()
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
    await fetchMachines()
  } catch (error) {
    console.error('Failed to start simulations for all machines:', error)
    toast.error('Unable to start simulations for some machines.')
  }
}

const stopAllSimulations = async () => {
  try {
    const cancelPromises = Object.entries(simulationMap.value).map(([machineId, simulationId]) => {
      if (simulationId) {
        return cancelSimulation(simulationId, machineId)
      }
      return Promise.resolve()
    })
    await Promise.all(cancelPromises)
    toast.info('Stopped all active simulations.')
    await fetchMachines()
  } catch (error) {
    console.error('Failed to stop simulations for all machines:', error)
    toast.error('Unable to cancel simulations for some machines.')
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
        simulationMap.value[machine.id] = simulation.id
        return runSimulationStep(simulation.id, machine.id)
      })
    )
    toast.success('Telemetry generation started across the fleet.')
    await fetchMachines()
  } catch (error) {
    console.error('Failed to generate telemetry:', error)
    toast.error('Unable to generate telemetry right now.')
  }
}

const runPredictiveAnalytics = async () => {
  try {
    await Promise.all(
      machines.value.map(machine => requestPrediction(machine.id))
    )
    toast.success('Predictive analytics pipeline triggered for all machines.')
  } catch (error) {
    console.error('Failed to run predictive analytics for all machines:', error)
    toast.error('Unable to trigger predictive analytics at the moment.')
  }
}

// Initialize status distribution chart
const updateStatusChart = () => {
  if (!statusChart.value) return
  
  if (!statusChartInstance) {
    statusChartInstance = echarts.init(statusChart.value)
  }
  
  // Count machines by status
  const statusCounts: Record<string, number> = {}
  machines.value.forEach(machine => {
    const normalized = normalizeStatus(machine.status)
    statusCounts[normalized] = (statusCounts[normalized] || 0) + 1
  })

  const hasData = Object.keys(statusCounts).length > 0 && Object.values(statusCounts).some(count => count > 0)
  if (!hasData) {
    statusChartInstance.clear()
    return
  }

  const statuses = Object.keys(statusCounts)
  const counts = Object.values(statusCounts)

  statusChartInstance.setOption({
    tooltip: {
      trigger: 'item'
    },
    legend: {
      top: '5%',
      left: 'center',
      textStyle: {
        color: '#ffffff'
      }
    },
    series: [
      {
        name: 'Machine Status',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 20,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: statuses.map((normalizedStatus, index) => {
          const color = statusColorMap[normalizedStatus] ?? statusColorMap.unknown

          return {
            value: counts[index],
            name: formatStatus(normalizedStatus),
            itemStyle: { color }
          }
        })
      }
    ]
  })
}

// Update current time
const updateTime = () => {
  currentTime.value = new Date().toLocaleTimeString()
}

// Handle window resize
const handleResize = () => {
  if (statusChartInstance) {
    statusChartInstance.resize()
  }
}

// Normalize machine status
const normalizeStatus = (status: EquipmentStatus): string => {
  switch (status) {
    case 'operational':
    case 'Operational':
      return 'operational'
    case 'maintenance':
    case 'Maintenance':
      return 'maintenance'
    case 'warning':
    case 'Warning':
      return 'warning'
    case 'critical':
    case 'Critical':
      return 'critical'
    case 'offline':
    case 'Offline':
      return 'offline'
    default:
      return 'unknown'
  }
}

// Format machine status for display
const formatStatus = (status: string): string => {
  switch (status) {
    case 'operational':
      return 'Operational'
    case 'maintenance':
      return 'Maintenance'
    case 'warning':
      return 'Warning'
    case 'critical':
      return 'Critical'
    case 'offline':
      return 'Offline'
    default:
      return 'Unknown'
  }
}

// Status color map
const statusColorMap = {
  operational: '#22c55e',
  maintenance: '#3b82f6',
  warning: '#facc15',
  critical: '#ef4444',
  offline: '#64748b',
  unknown: '#0ea5e9'
}

// Machine CRUD handlers
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

const handleMachineSuccess = async (machine: MachineDto) => {
  closeMachineForm()
  await fetchMachines()
  toast.success(isEditing.value ? 'Machine updated successfully' : 'Machine created successfully')
}

const handleDeleteSuccess = async () => {
  closeDeleteDialog()
  await fetchMachines()
}

const handleSimulationUpdated = (simulation: SimulationStateDto | null) => {
  if (simulation) {
    simulationMap.value[simulation.machineId] = simulation.id
  }
}

// Initialize
onMounted(() => {
  fetchMachines()
  updateTime()
  timeInterval = window.setInterval(updateTime, 1000)
  window.addEventListener('resize', handleResize)
})

// Cleanup
onBeforeUnmount(() => {
  if (timeInterval) {
    clearInterval(timeInterval)
  }
  window.removeEventListener('resize', handleResize)
  if (statusChartInstance) {
    statusChartInstance.dispose()
  }
})
</script>

<style scoped>
.status-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-20);
  align-items: center;
}

.status-time {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.status-time .label {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.status-time .value {
  font-size: var(--font-size-xl);
  color: var(--color-text-primary);
  font-weight: 600;
}

.tab-group {
  display: inline-flex;
  flex-wrap: wrap;
  gap: var(--space-8);
}

.tab-pill {
  min-width: 130px;
  justify-content: center;
}

.tab-panels {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.tab-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.overview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--space-24);
  align-items: start;
}

.metrics-card {
  display: flex;
  flex-direction: column;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-16);
}

.stat-card {
  text-align: center;
  padding: var(--space-24);
  gap: var(--space-16);
  align-items: center;
}

.metric-icon {
  font-size: var(--font-size-2xl);
}

.metric-value {
  font-size: var(--font-size-3xl);
  font-weight: 700;
  color: var(--color-text-primary);
}

.metric-label {
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--color-text-secondary);
}

.chart-card {
  min-height: 420px;
}

.chart-container {
  width: 100%;
  height: 360px;
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-16);
}

.action-button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-12);
  width: 100%;
}

.action-icon {
  font-size: var(--font-size-xl);
}

.tab-card {
  padding: var(--space-24);
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.machine-list-skeleton {
  display: grid;
  gap: var(--space-16);
}

.empty-machines {
  text-align: center;
  padding: var(--space-32);
  color: var(--color-text-secondary);
}

.machine-list {
  display: grid;
  gap: var(--space-16);
}

.machine-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-20);
  background: var(--color-surface-alt);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  transition: all 0.2s ease;
}

.machine-item:hover {
  background: var(--color-surface-hover);
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.machine-info h4 {
  margin: 0 0 var(--space-8) 0;
  font-size: var(--font-size-lg);
  color: var(--color-text-primary);
}

.machine-meta {
  display: flex;
  gap: var(--space-12);
  flex-wrap: wrap;
  margin: 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.machine-type {
  padding: var(--space-4) var(--space-8);
  background: var(--color-surface);
  border-radius: var(--radius-sm);
  text-transform: capitalize;
}

.machine-status {
  padding: var(--space-4) var(--space-8);
  border-radius: var(--radius-sm);
  font-weight: 600;
}

.machine-status.status-operational {
  background: color-mix(in srgb, #22c55e 20%, transparent);
  color: #22c55e;
}

.machine-status.status-warning {
  background: color-mix(in srgb, #facc15 20%, transparent);
  color: #facc15;
}

.machine-status.status-critical {
  background: color-mix(in srgb, #ef4444 20%, transparent);
  color: #ef4444;
}

.machine-status.status-maintenance {
  background: color-mix(in srgb, #3b82f6 20%, transparent);
  color: #3b82f6;
}

.machine-status.status-offline {
  background: color-mix(in srgb, #64748b 20%, transparent);
  color: #64748b;
}

.machine-location {
  display: flex;
  align-items: center;
  gap: var(--space-4);
}

.machine-actions {
  display: flex;
  gap: var(--space-8);
}

@media (max-width: 768px) {
  .status-summary {
    grid-template-columns: 1fr;
    text-align: center;
  }

  .tab-group {
    justify-content: center;
  }

  .overview-grid {
    grid-template-columns: 1fr;
  }

  .chart-container {
    height: 300px;
  }
}

.prescriptive-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 1.5rem;
  align-items: start;
}

.machine-picker {
  max-height: 600px;
  overflow-y: auto;
}

.picker-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.picker-item {
  padding: 0.75rem 1rem;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
}

.picker-item:hover {
  background: rgba(255, 255, 255, 0.08);
}

.picker-item.active {
  background: rgba(24, 144, 255, 0.15);
  border: 1px solid rgba(24, 144, 255, 0.3);
  color: #1890ff;
}

.empty-state {
  padding: 4rem;
  text-align: center;
  color: #8c8c8c;
  display: flex;
  justify-content: center;
  align-items: center;
}
</style>
