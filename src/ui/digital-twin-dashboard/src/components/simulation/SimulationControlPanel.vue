<template>
  <BaseCard variant="soft" class="simulation-panel">
    <template #header>Simulation Control</template>

    <div v-if="!simulation" class="empty-state">
      <p>No active simulation for this machine.</p>
      <BaseButton variant="primary" @click="showCreateDialog = true">
        Create Simulation
      </BaseButton>
    </div>

    <div v-else class="simulation-content">
      <div class="status-row">
        <div>
          <p class="label">Status</p>
          <p class="value" :class="`status-${simulation.status}`">
            {{ simulation.status }}
          </p>
        </div>
        <div>
          <p class="label">Progress</p>
          <p class="value">
            {{ simulation.currentStep }} / {{ simulation.totalSteps }}
          </p>
        </div>
      </div>

      <div class="progress">
        <div
          class="fill"
          :style="{ width: progressPercentage + '%' }"
        ></div>
      </div>

      <div class="actions">
        <BaseButton
          variant="primary"
          :disabled="simulation.status !== 'idle'"
          :loading="actionLoading"
          @click="handleRun"
        >
          Run Step
        </BaseButton>
        <BaseButton
          variant="warning"
          :disabled="simulation.status !== 'running'"
          :loading="actionLoading"
          @click="handlePause"
        >
          Pause
        </BaseButton>
        <BaseButton
          variant="primary"
          :disabled="simulation.status !== 'paused'"
          :loading="actionLoading"
          @click="handleResume"
        >
          Resume
        </BaseButton>
        <BaseButton
          variant="critical"
          :disabled="!['running', 'paused'].includes(simulation.status)"
          :loading="actionLoading"
          @click="handleCancel"
        >
          Cancel
        </BaseButton>
      </div>

      <div v-if="simulation.metrics" class="metrics-grid">
        <div class="metric-item" :class="{ 'metric-valid': simulation.metrics.data_valid }">
          <p class="label">KS Statistic</p>
          <p class="value-sm">{{ formatMetric(simulation.metrics.ks_statistic) }}</p>
        </div>
        <div class="metric-item">
          <p class="label">Autocorrelation</p>
          <p class="value-sm">{{ formatMetric(simulation.metrics.autocorrelation) }}</p>
        </div>
        <div class="metric-item">
          <p class="label">Data Valid</p>
          <p class="value-sm badge" :class="simulation.metrics.data_valid ? 'valid' : 'invalid'">
            {{ simulation.metrics.data_valid ? 'PASS' : 'FAIL' }}
          </p>
        </div>
      </div>
    </div>

    <SimulationCreateDialog
      :open="showCreateDialog"
      :machine-id="machineId"
      @cancel="showCreateDialog = false"
      @success="handleSimulationCreated"
    />
  </BaseCard>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import SimulationCreateDialog from './SimulationCreateDialog.vue'
import type { SimulationStateDto } from '@/services/simulation.service'
import {
  createSimulation,
  getSimulation,
  runSimulationStep,
  pauseSimulation,
  resumeSimulation,
  cancelSimulation
} from '@/services/simulation.service'

interface Props {
  machineId: string
  simulationId?: string
}

interface Emits {
  (e: 'updated', simulation: SimulationStateDto | null): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const toast = useToast()
const simulation = ref<SimulationStateDto | null>(null)
const showCreateDialog = ref(false)
const actionLoading = ref(false)

const progressPercentage = computed(() => {
  if (!simulation.value || simulation.value.totalSteps === 0) {
    return 0
  }
  return Math.min(100, Math.round((simulation.value.currentStep / simulation.value.totalSteps) * 100))
})

async function loadSimulation() {
  if (!props.simulationId) {
    simulation.value = null
    return
  }

  try {
    simulation.value = await getSimulation(props.simulationId, props.machineId)
    emit('updated', simulation.value)
  } catch (error) {
    console.error('Failed to load simulation:', error)
    toast.error('Unable to load simulation data')
    simulation.value = null
  }
}

async function handleSimulationCreated(newSimulation: SimulationStateDto) {
  showCreateDialog.value = false
  simulation.value = newSimulation
  emit('updated', newSimulation)
}

async function executeAction(action: () => Promise<SimulationStateDto | void>, successMessage: string) {
  actionLoading.value = true
  try {
    const result = await action()
    if (result) {
      simulation.value = result
      emit('updated', result)
    }
    toast.success(successMessage)
  } catch (error) {
    console.error('Simulation action failed:', error)
    toast.error('Simulation action failed')
  } finally {
    actionLoading.value = false
  }
}

async function handleRun() {
  await executeAction(async () => {
    if (!simulation.value) return
    const result = await runSimulationStep(simulation.value.id, props.machineId)
    return {
      ...simulation.value,
      currentStep: result.step,
      metrics: result.data?.validation || simulation.value.metrics,
      status: result.step >= simulation.value.totalSteps ? 'completed' : 'running'
    }
  }, 'Simulation step executed')
}

async function handlePause() {
  await executeAction(async () => {
    if (!simulation.value) return
    return pauseSimulation(simulation.value.id, props.machineId)
  }, 'Simulation paused')
}

async function handleResume() {
  await executeAction(async () => {
    if (!simulation.value) return
    return resumeSimulation(simulation.value.id, props.machineId)
  }, 'Simulation resumed')
}

async function handleCancel() {
  await executeAction(async () => {
    if (!simulation.value) return
    return cancelSimulation(simulation.value.id, props.machineId)
  }, 'Simulation cancelled')
}

function formatMetric(val: any) {
  if (typeof val === 'number') return val.toFixed(4)
  return val ?? 'N/A'
}

// Watch for simulationId changes
watch(
  () => props.simulationId,
  () => {
    loadSimulation()
  },
  { immediate: true }
)
</script>

<style scoped>
.simulation-panel {
  padding: var(--spacing-lg);
  display: grid;
  gap: var(--spacing-lg);
}

.empty-state {
  display: grid;
  gap: var(--spacing-md);
  text-align: center;
}

.simulation-content {
  display: grid;
  gap: var(--spacing-lg);
}

.status-row {
  display: flex;
  justify-content: space-between;
  gap: var(--spacing-lg);
}

.label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.value {
  font-size: 1.5rem;
  font-weight: 600;
}

.value.status-running {
  color: var(--color-success-500);
}

.value.status-paused {
  color: var(--color-warning-500);
}

.value.status-completed {
  color: var(--color-primary-500);
}

.value.status-cancelled,
.value.status-failed {
  color: var(--color-critical-500);
}

.progress {
  position: relative;
  height: 10px;
  border-radius: var(--radius-full);
  background: var(--color-surface-600);
  overflow: hidden;
}

.fill {
  position: absolute;
  inset: 0;
  width: 0;
  background: linear-gradient(90deg, var(--color-primary-500), var(--color-primary-300));
  transition: width 0.4s ease;
}

.actions {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-md);
}

.actions .base-button {
  min-width: 120px;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--spacing-md);
  margin-top: var(--spacing-md);
  padding-top: var(--spacing-md);
  border-top: 1px solid var(--color-surface-400);
}

.value-sm {
  font-size: 1rem;
  font-weight: 500;
}

.badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 700;
}

.badge.valid {
  background: var(--color-success-500);
  color: white;
}

.badge.invalid {
  background: var(--color-critical-500);
  color: white;
}
</style>
