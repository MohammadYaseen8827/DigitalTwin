<template>
  <div class="predictive-analytics-dashboard">
    <header class="dashboard-header">
      <div>
        <p class="eyebrow">Predictive insights</p>
        <h2>Health & Maintenance Intelligence</h2>
      </div>
      <div class="actions">
        <BaseSelect v-model="selectedMachineId" size="sm" label="Select Machine">
          <option :value="undefined">All Machines</option>
          <option v-for="m in machines" :key="m.id" :value="m.id">{{ m.name }}</option>
        </BaseSelect>
        <BaseSelect v-model="timeRange" size="sm" label="Time Range">
          <option value="7d">Last 7 Days</option>
          <option value="30d">Last 30 Days</option>
          <option value="90d">Last 90 Days</option>
        </BaseSelect>
        <BaseButton size="sm" :disabled="loading" @click="refreshData">
          <span v-if="loading">Refreshing…</span>
          <span v-else>Refresh Data</span>
        </BaseButton>
      </div>
    </header>

    <div class="analytics-grid">
      <div class="metrics-summary">
        <StatCard label="Overall Equipment Health" :caption="`${Math.abs(healthTrend)}% from last week`">
          <template #icon>💡</template>
          <MetricValue :value="overallHealth" :precision="0" suffix="%" />
          <template #meta>
            <TrendChip :trend="healthTrend" />
          </template>
        </StatCard>
        <StatCard label="Predicted Failures (Next 7 Days)" :caption="`${failureTrend}% from last week`" variant="soft">
          <template #icon>⚠️</template>
          <MetricValue :value="predictedFailures" />
          <template #meta>
            <TrendChip :trend="failureTrend" intent="warning" />
          </template>
        </StatCard>
        <StatCard label="Maintenance Savings" :caption="`${savingsTrend}% from last month`" variant="soft">
          <template #icon>💰</template>
          <MetricValue :value="estimatedSavings" :precision="0">
            <template #suffix>
              <span class="metric-value__unit"> USD</span>
              <TooltipIcon tooltip="Estimated downtime savings derived from heuristic inputs.">
                <Info class="w-4 h-4" />
              </TooltipIcon>
            </template>
          </MetricValue>
          <template #meta>
            <TrendChip :trend="savingsTrend" intent="positive" />
          </template>
        </StatCard>
      </div>

      <BaseCard variant="soft" class="chart-card">
        <template #header>Failure Probability by Machine</template>
        <div ref="failureChart" class="chart-container" role="img" aria-label="Failure probability bar chart"></div>
      </BaseCard>

      <BaseCard variant="soft" class="schedule-card">
        <template #header>Recommended Maintenance Schedule</template>
        <div class="schedule-table">
          <div class="schedule-header">
            <div>Machine</div>
            <div>Recommended Date</div>
            <div>Confidence</div>
            <div class="actions-col">Actions</div>
          </div>
          <div
            v-for="schedule in maintenanceSchedule"
            :key="schedule.machineId"
            class="schedule-row"
            :class="{ 'high-priority': schedule.confidence > 0.8 }"
          >
            <div class="machine-cell">
              <span class="machine-name">{{ schedule.machineName }}</span>
            </div>
            <div>{{ formatDate(schedule.recommendedDate) }}</div>
            <div>
              <div class="confidence-bar">
                <div
                  class="confidence-fill"
                  :style="{ width: `${schedule.confidence * 100}%` }"
                  :class="getConfidenceClass(schedule.confidence)"
                ></div>
                <span class="confidence-text">{{ Math.round(schedule.confidence * 100) }}%</span>
              </div>
            </div>
            <div class="actions-col">
              <BaseButton size="sm" variant="outline" @click="scheduleMaintenance(schedule.machineId)">
                Schedule
              </BaseButton>
            </div>
          </div>
        </div>
      </BaseCard>

      <BaseCard variant="soft" class="anomaly-card">
        <template #header>Recent Anomalies</template>
        <div class="anomaly-list">
          <div
            v-for="anomaly in anomalies"
            :key="anomaly.id"
            class="anomaly-item"
            :class="{ critical: anomaly.severity === 'critical' }"
          >
            <div class="anomaly-header">
              <div class="anomaly-machine">{{ anomaly.machineName }}</div>
              <span class="anomaly-severity" :class="anomaly.severity">
                {{ anomaly.severity.toUpperCase() }}
              </span>
            </div>
            <div class="anomaly-description">{{ anomaly.description }}</div>
            <div class="anomaly-timestamp">{{ formatDateTime(anomaly.timestamp) }}</div>
          </div>
        </div>
      </BaseCard>

      <BaseCard v-if="selectedMachinePrediction" variant="soft" class="explanation-card">
        <template #header>Prediction Interpretation for {{ selectedMachinePrediction.machine.name }}</template>
        <div class="explanation-content">
          <div class="prediction-stats">
            <div class="stat">
              <span class="label">RUL</span>
              <span class="value">{{ selectedMachinePrediction.prediction.remainingUsefulLifeDays.toFixed(1) }} days</span>
              <span class="range" v-if="selectedMachinePrediction.prediction.rulLowerBound && selectedMachinePrediction.prediction.rulUpperBound">
                ±{{ ((selectedMachinePrediction.prediction.rulUpperBound - selectedMachinePrediction.prediction.rulLowerBound) / 2).toFixed(1) }}
              </span>
            </div>
            <div class="stat">
              <span class="label">Health</span>
              <span class="value">{{ selectedMachinePrediction.prediction.healthStatus }}</span>
            </div>
          </div>
          <PredictionExplanation :contributions="selectedMachinePrediction.prediction.featureContributions" />
        </div>
      </BaseCard>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed, onBeforeUnmount } from 'vue'
import { useToast } from '@/composables/useToast'

import * as echarts from 'echarts'
import BaseCard from '../base/BaseCard.vue'
import BaseButton from '../base/BaseButton.vue'
import BaseSelect from '../base/BaseSelect.vue'
import StatCard from '../base/StatCard.vue'
import TrendChip from '../base/TrendChip.vue'
import MetricValue from '../base/MetricValue.vue'
import TooltipIcon from '../base/TooltipIcon.vue'
import PredictionExplanation from './PredictionExplanation.vue'
import { Info } from 'lucide-vue-next'

import { planMaintenance } from '@/services/maintenance.service'
import { fetchMachines } from '@/services/machines.service'
import { fetchPredictionHistory, requestPrediction } from '@/services/predictions.service'
import type { MachineDto, PredictionDto } from '@/api/types'

// Reactive references
const timeRange = ref('7d')
const loading = ref(false)
const machines = ref<MachineDto[]>([])
const predictionHistory = ref<Record<string, PredictionDto[]>>({})
const failureChart = ref<HTMLDivElement | null>(null)
let failureChartInstance: echarts.ECharts | null = null

const selectedMachineId = ref<string | undefined>(undefined)
const selectedMachinePrediction = computed(() => {
  if (!selectedMachineId.value) return null
  return latestPredictions.value.find(p => p.machine.id === selectedMachineId.value) || null
})

const toast = useToast()

const scheduleMaintenance = async (machineId: string) => {
  try {
    const plannedDate = new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    await planMaintenance({
      machineId,
      type: 'Predictive',
      plannedDate,
      notes: `Scheduled from predictive analytics dashboard for machine ${machineId}`
    })
    toast.success('Maintenance scheduled successfully')
    await loadPredictionHistory()
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Failed to schedule maintenance'
    toast.error(message)
  }
}

const latestPredictions = computed(() =>
  machines.value
    .map(machine => {
      const history = [...(predictionHistory.value[machine.id] ?? [])].sort(
        (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      )
      const latest = history[0]
      return latest ? { machine, prediction: latest, history } : null
    })
    .filter(Boolean) as { machine: MachineDto; prediction: PredictionDto; history: PredictionDto[] }[]
)

const averageFailureChange = computed(() => {
  const deltas = latestPredictions.value
    .map(({ history }) => {
      if (history.length < 2) return 0
      const [previous, latest] = history.slice(0, 2)
      return latest.failureProbability - previous.failureProbability
    })
    .filter(delta => delta !== 0)

  if (!deltas.length) return 0
  return deltas.reduce((sum, delta) => sum + delta, 0) / deltas.length
})

const overallHealth = computed(() => {
  if (!latestPredictions.value.length) return 0
  const score = latestPredictions.value.reduce((total, { prediction }) => {
    return total + Math.max(0, 100 - prediction.failureProbability * 100)
  }, 0)
  return Math.round(score / latestPredictions.value.length)
})

const healthTrend = computed(() => Math.round(-averageFailureChange.value * 100))

const predictedFailures = computed(() =>
  latestPredictions.value.filter(({ prediction }) => prediction.failureProbability >= 0.55).length
)

const failureTrend = computed(() => Math.round(averageFailureChange.value * 100))

const estimatedSavings = computed(() => {
  const savings = latestPredictions.value.reduce((total, { prediction }) => {
    // Simple heuristic: lower failure probability implies higher savings potential
    const avoidedDowntime = Math.max(0, 1 - prediction.failureProbability)
    return total + avoidedDowntime * 2500
  }, 0)
  return savings
})

const maintenanceSavings = computed(() => `$${Math.round(estimatedSavings.value).toLocaleString()}`)

const savingsTrend = computed(() => Math.max(0, Math.round(healthTrend.value / 2)))

const maintenanceSchedule = computed(() =>
  latestPredictions.value
    .map(({ machine, prediction }) => {
      const days = Math.max(0, prediction.remainingUsefulLifeDays)
      const confidence = Math.min(1, Math.max(0, 1 - prediction.failureProbability))
      const recommendedDate = new Date()
      recommendedDate.setDate(recommendedDate.getDate() + Math.round(days))

      return {
        machineId: machine.id,
        machineName: machine.name,
        recommendedDate,
        confidence
      }
    })
    .sort((a, b) => a.recommendedDate.getTime() - b.recommendedDate.getTime())
)

const anomalies = computed(() => {
  if (!latestPredictions.value.length) return []

  return latestPredictions.value
    .filter(({ prediction }) => prediction.failureProbability >= 0.6)
    .map(({ machine, prediction }) => ({
      id: prediction.id,
      machineName: machine.name,
      severity: prediction.failureProbability > 0.8 ? 'critical' : 'high',
      description:
        prediction.failureProbability > 0.8
          ? 'Critical failure probability detected'
          : 'Elevated failure probability observed',
      timestamp: new Date(prediction.createdAt)
    }))
})

const healthTrendClass = computed(() => {
  if (healthTrend.value > 0) return 'positive'
  if (healthTrend.value < 0) return 'negative'
  return ''
})

const formatDate = (date: Date) =>
  date.toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric'
  })

const formatDateTime = (date: Date) =>
  date.toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })

const getConfidenceClass = (confidence: number) => {
  if (confidence > 0.8) return 'high'
  if (confidence > 0.6) return 'medium'
  return 'low'
}

const updateFailureChart = () => {
  if (!failureChart.value) return

  if (!failureChartInstance) {
    failureChartInstance = echarts.init(failureChart.value)
  }

  const labels = latestPredictions.value.map(({ machine }) => machine.name)
  const data = latestPredictions.value.map(({ prediction }) => Math.round(prediction.failureProbability * 100))

  failureChartInstance.setOption({
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: {
        color: '#64748b'
      }
    },
    yAxis: {
      type: 'value',
      name: 'Failure Probability (%)',
      min: 0,
      max: 100,
      axisLabel: {
        formatter: '{value}%',
        color: '#64748b'
      }
    },
    series: [
      {
        name: 'Failure Probability',
        type: 'bar',
        barWidth: '45%',
        itemStyle: {
          borderRadius: 6,
          color: '#ef4444',
          shadowBlur: 12,
          shadowColor: 'rgba(239, 68, 68, 0.35)'
        },
        data
      }
    ]
  })
}

const loadPredictionHistory = async () => {
  const historyEntries = await Promise.all(
    machines.value.map(async machine => {
      try {
        let history = await fetchPredictionHistory(machine.id, 20)

        if (!history.length) {
          const prediction = await requestPrediction(machine.id)
          history = [prediction]
        }

        return [machine.id, history] as const
      } catch (error) {
        if (import.meta.env.DEV) console.error(`Failed to load predictions for machine ${machine.name}`, error)
        toast.error(`Unable to load prediction history for ${machine.name}.`)
        return [machine.id, []] as const
      }
    })
  )

  predictionHistory.value = Object.fromEntries(historyEntries)
  updateFailureChart()
}

const refreshData = () => {
  // Reload data based on current time range
  loadPredictionHistory()
}

// Watch for changes
watch(timeRange, refreshData)

const handleResize = () => {
  if (failureChartInstance) {
    failureChartInstance.resize()
  }
}

// Cleanup
onBeforeUnmount(() => {
  window.removeEventListener('resize', handleResize)
  if (failureChartInstance) {
    failureChartInstance.dispose()
  }
})
</script>

<style scoped>
.predictive-analytics-dashboard {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.dashboard-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: var(--space-16);
}

.dashboard-header h2 {
  margin: 0;
  font-size: var(--font-size-2xl);
  color: var(--color-text-primary);
}

.eyebrow {
  margin: 0;
  font-size: var(--font-size-xs);
  text-transform: uppercase;
  letter-spacing: 0.12em;
  color: var(--color-text-secondary);
}

.actions {
  display: inline-flex;
  gap: var(--space-12);
  align-items: center;
}

.analytics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--space-24);
}

.metrics-summary {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--space-16);
}

.prediction-stats {
  display: flex;
  gap: 2rem;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: var(--radius-md);
}

.prediction-stats .stat {
  display: flex;
  flex-direction: column;
}

.prediction-stats .label {
  font-size: 0.75rem;
  text-transform: uppercase;
  color: var(--color-text-secondary);
}

.prediction-stats .value {
  font-size: 1.25rem;
  font-weight: 600;
}

.chart-card {
  min-height: 360px;
}

.chart-container {
  width: 100%;
  height: 320px;
}

.schedule-card {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.schedule-table {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.schedule-header,
.schedule-row {
  display: grid;
  grid-template-columns: 1.6fr 1fr 1fr 0.8fr;
  gap: var(--space-12);
  align-items: center;
}

.schedule-header {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.08em;
  padding-bottom: var(--space-8);
  border-bottom: 1px solid color-mix(in srgb, var(--color-border) 70%, transparent);
}

.schedule-row {
  padding: var(--space-12) 0;
  border-bottom: 1px solid color-mix(in srgb, var(--color-border) 40%, transparent);
  transition: background 0.2s ease;
}

.schedule-row:last-child {
  border-bottom: none;
}

.schedule-row.high-priority {
  background: color-mix(in srgb, var(--color-error) 10%, transparent);
}

.machine-name {
  font-weight: 600;
  color: var(--color-text-primary);
}

.confidence-bar {
  position: relative;
  height: 16px;
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-border) 30%, transparent);
  overflow: hidden;
}

.confidence-fill {
  height: 100%;
  border-radius: var(--radius-lg);
  transition: width 0.3s ease;
}

.confidence-fill.high {
  background: var(--color-error);
}

.confidence-fill.medium {
  background: var(--color-warning);
}

.confidence-fill.low {
  background: var(--color-success);
}

.confidence-text {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-primary);
}

.anomaly-list {
  display: grid;
  gap: var(--space-16);
}

.anomaly-item {
  border-radius: var(--radius-lg);
  padding: var(--space-16);
  border: 1px solid color-mix(in srgb, var(--color-border) 60%, transparent);
  background: color-mix(in srgb, var(--color-surface-alt) 50%, transparent);
}

.anomaly-item.critical {
  border-color: color-mix(in srgb, var(--color-error) 40%, var(--color-border) 60%);
  background: color-mix(in srgb, var(--color-error) 12%, transparent);
}

.anomaly-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-8);
}

.anomaly-machine {
  font-weight: 600;
  color: var(--color-text-primary);
}

.anomaly-severity {
  padding: var(--space-4) var(--space-8);
  border-radius: var(--radius-lg);
  font-size: var(--font-size-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--color-text-primary);
}

.anomaly-severity.high {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.anomaly-severity.critical {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.anomaly-severity.medium {
  background: color-mix(in srgb, var(--color-primary) 20%, transparent);
  color: var(--color-primary);
}

.anomaly-description {
  color: var(--color-text-secondary);
  margin-bottom: var(--space-6);
}

.anomaly-timestamp {
  font-size: var(--font-size-xs);
  color: color-mix(in srgb, var(--color-text-secondary) 80%, transparent);
}

@media (max-width: 768px) {
  .dashboard-header {
    align-items: flex-start;
  }

  .analytics-grid {
    gap: var(--space-20);
  }

  .schedule-header,
  .schedule-row {
    grid-template-columns: 1fr 1fr;
  }

  .actions-col {
    justify-self: flex-start;
  }

  .chart-container {
    height: 260px;
  }
}

@media (max-width: 540px) {
  .dashboard-header {
    gap: var(--space-12);
  }

  .actions {
    flex-direction: column;
    align-items: stretch;
    width: 100%;
  }

  .metrics-summary {
    grid-template-columns: 1fr;
  }

  .schedule-header,
  .schedule-row {
    grid-template-columns: 1fr;
    gap: var(--space-8);
    align-items: start;
  }

  .actions-col {
    width: 100%;
  }

  .chart-container {
    height: 220px;
  }

  .anomaly-item {
    padding: var(--space-12);
  }
}
.prediction-stats .range {
  font-size: 0.85rem;
  color: var(--color-text-secondary);
  font-weight: 500;
  margin-top: 0.2rem;
}
</style>
