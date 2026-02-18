<template>
  <div class="simulation-controls">
    <BaseButton
      v-if="!isRunning && !isScheduled"
      :loading="isStarting"
      :disabled="isStarting || isStopping"
      variant="primary"
      size="sm"
      @click="handleStart"
    >
      <template #icon>
        <PlayIcon class="h-4 w-4" />
      </template>
      Start Simulation
    </BaseButton>

    <BaseButton
      v-if="isRunning"
      :loading="isStopping"
      :disabled="isStarting || isStopping"
      variant="secondary"
      size="sm"
      @click="handleStop"
    >
      <template #icon>
        <StopIcon class="h-4 w-4" />
      </template>
      Stop Simulation
    </BaseButton>

    <BaseButton
      v-if="!isScheduled"
      :disabled="isStarting || isStopping"
      variant="outline"
      size="sm"
      @click="openScheduleDialog"
    >
      <template #icon>
        <ClockIcon class="h-4 w-4" />
      </template>
      Schedule
    </BaseButton>

    <BaseButton
      v-else
      :loading="isCancelling"
      :disabled="isStarting || isStopping"
      variant="ghost"
      size="sm"
      @click="handleCancelSchedule"
    >
      <template #icon>
        <XMarkIcon class="h-4 w-4" />
      </template>
      Cancel Schedule
    </BaseButton>

    <BaseTooltip v-if="isScheduled && nextRun" :content="`Next run: ${formatDate(nextRun)}`">
      <div class="schedule-badge">
        <ClockIcon class="h-3 w-3" />
        <span>Scheduled</span>
      </div>
    </BaseTooltip>

    <!-- Schedule Simulation Dialog -->
    <BaseDialog
      v-model:open="showScheduleDialog"
      title="Schedule Simulation"
      description="Configure simulation parameters and schedule"
    >
      <form @submit.prevent="handleSchedule">
        <div class="space-y-4">
          <BaseSelect
            v-model="scheduleForm.degradationModel"
            label="Degradation Model"
            :options="[
              { value: 'wiener', label: 'Wiener Process' },
              { value: 'markov', label: 'Markov Chain' },
              { value: 'physics_based', label: 'Physics-based' },
            ]"
          />

          <BaseInput
            v-model.number="scheduleForm.steps"
            type="number"
            :min="1"
            :max="1000"
            label="Simulation Steps"
          />

          <BaseInput
            v-model.number="scheduleForm.intervalSeconds"
            type="number"
            :min="10"
            :step="10"
            label="Interval (seconds)"
            hint="Time between simulation steps"
          />

          <BaseDateTimePicker
            v-model="scheduleForm.startTime"
            :min="new Date()"
            label="Start Time"
          />

          <BaseCheckbox
            v-model="scheduleForm.persistTelemetry"
            label="Persist telemetry data"
          />
        </div>

        <div class="mt-6 flex justify-end space-x-3">
          <BaseButton
            type="button"
            variant="ghost"
            @click="showScheduleDialog = false"
          >
            Cancel
          </BaseButton>
          <BaseButton
            type="submit"
            variant="primary"
            :loading="isScheduling"
          >
            Schedule Simulation
          </BaseButton>
        </div>
      </form>
    </BaseDialog>

    <!-- Confirmation Dialog -->
    <BaseDialog
      v-model:open="showConfirmDialog"
      :title="confirmDialog.title"
      :description="confirmDialog.description"
      variant="danger"
    >
      <div class="flex justify-end space-x-3">
        <BaseButton variant="ghost" @click="showConfirmDialog = false">
          {{ $t('common.cancel') }}
        </BaseButton>
        <BaseButton
          variant="danger"
          :loading="confirmDialog.loading"
          @click="confirmDialog.action"
        >
          {{ confirmDialog.confirmText }}
        </BaseButton>
      </div>
    </BaseDialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { format } from 'date-fns'
import { PlayIcon, StopIcon, ClockIcon, XMarkIcon } from '@heroicons/vue/24/outline'
import { 
  createSimulation,
  runSimulationStep,
  cancelSimulation,
  listSimulations,
  getSimulationStatus
} from '@/services/simulation.service'

const props = defineProps<{
  machineId: string
  compact?: boolean
}>()

const emit = defineEmits<{
  (e: 'update:status', status: { isRunning: boolean; isScheduled: boolean }): void
  (e: 'simulation-started'): void
  (e: 'simulation-stopped'): void
  (e: 'simulation-scheduled'): void
  (e: 'schedule-cancelled'): void
  (e: 'error', error: Error): void
}>()

// State
const isStarting = ref(false)
const isStopping = ref(false)
const isScheduling = ref(false)
const isCancelling = ref(false)
const showScheduleDialog = ref(false)
const showConfirmDialog = ref(false)
const currentSimulationId = ref<string | null>(null)
const simulationStatus = ref<{
  isRunning: boolean
  isScheduled: boolean
  nextRun?: string
  lastRun?: string
  status: 'idle' | 'running' | 'scheduled' | 'error'
  error?: string
}>({
  isRunning: false,
  isScheduled: false,
  status: 'idle'
})

const confirmDialog = ref({
  title: '',
  description: '',
  confirmText: '',
  loading: false,
  action: () => {}
})

// Form
const scheduleForm = ref({
  degradationModel: 'wiener' as const,
  steps: 10,
  intervalSeconds: 60,
  startTime: new Date(Date.now() + 3600000), // 1 hour from now
  persistTelemetry: true
})

// Computed
const isRunning = computed(() => simulationStatus.value.isRunning)
const isScheduled = computed(() => simulationStatus.value.isScheduled)
const nextRun = computed(() => simulationStatus.value.nextRun)

// Methods
const loadStatus = async () => {
  try {
    const status = await getSimulationStatus(props.machineId)
    simulationStatus.value = status
    emit('update:status', {
      isRunning: status.isRunning,
      isScheduled: status.isScheduled
    })
  } catch (error) {
    console.error('Failed to load simulation status:', error)
    emit('error', error as Error)
  }
}

const handleStart = async () => {
  confirmDialog.value = {
    title: 'Start Simulation?',
    description: 'This will start the simulation for this machine.',
    confirmText: 'Start',
    loading: false,
    action: () => startSimulation()
  }
  showConfirmDialog.value = true
}

const startSimulation = async () => {
  try {
    isStarting.value = true
    // Create a new simulation
    const simulation = await createSimulation(props.machineId, {
      degradationModel: 'wiener',
      totalSteps: 100,
      intervalSeconds: 5,
      persistTelemetry: true
    })
    
    currentSimulationId.value = simulation.id
    
    // Run the first step
    await runSimulationStep(simulation.id, props.machineId)
    
    await loadStatus()
    emit('simulation-started')
  } catch (error) {
    console.error('Failed to start simulation:', error)
    emit('error', error as Error)
  } finally {
    isStarting.value = false
    showConfirmDialog.value = false
  }
}

const handleStop = async () => {
  confirmDialog.value = {
    title: 'Stop Simulation?',
    description: 'This will stop the running simulation.',
    confirmText: 'Stop',
    loading: false,
    action: () => stopSimulation()
  }
  showConfirmDialog.value = true
}

const stopSimulation = async () => {
  try {
    isStopping.value = true
    
    if (currentSimulationId.value) {
      await cancelSimulation(currentSimulationId.value, props.machineId)
      currentSimulationId.value = null
    }
    
    await loadStatus()
    emit('simulation-stopped')
  } catch (error) {
    console.error('Failed to stop simulation:', error)
    emit('error', error as Error)
  } finally {
    isStopping.value = false
    showConfirmDialog.value = false
  }
}

const openScheduleDialog = () => {
  scheduleForm.value = {
    degradationModel: 'wiener',
    steps: 10,
    intervalSeconds: 60,
    startTime: new Date(Date.now() + 3600000), // 1 hour from now
    persistTelemetry: true
  }
  showScheduleDialog.value = true
}

const handleSchedule = async () => {
  try {
    isScheduling.value = true
    // Create a scheduled simulation
    const simulation = await createSimulation(props.machineId, {
      degradationModel: scheduleForm.value.degradationModel,
      totalSteps: scheduleForm.value.steps,
      intervalSeconds: scheduleForm.value.intervalSeconds,
      persistTelemetry: scheduleForm.value.persistTelemetry,
      startTime: scheduleForm.value.startTime.toISOString()
    })
    
    currentSimulationId.value = simulation.id
    showScheduleDialog.value = false
    await loadStatus()
    emit('simulation-scheduled')
  } catch (error) {
    console.error('Failed to schedule simulation:', error)
    emit('error', error as Error)
  } finally {
    isScheduling.value = false
  }
}

const handleCancelSchedule = async () => {
  confirmDialog.value = {
    title: 'Cancel Schedule?',
    description: 'This will cancel the scheduled simulation.',
    confirmText: 'Cancel Schedule',
    loading: false,
    action: () => cancelSchedule()
  }
  showConfirmDialog.value = true
}

const cancelSchedule = async () => {
  try {
    isCancelling.value = true
    
    if (currentSimulationId.value) {
      await cancelSimulation(currentSimulationId.value, props.machineId)
      currentSimulationId.value = null
    }
    
    await loadStatus()
    emit('schedule-cancelled')
  } catch (error) {
    console.error('Failed to cancel schedule:', error)
    emit('error', error as Error)
  } finally {
    isCancelling.value = false
    showConfirmDialog.value = false
  }
}

const formatDate = (dateString: string) => {
  return format(new Date(dateString), 'PPpp')
}

// Lifecycle
onMounted(async () => {
  await loadStatus()
  
  // Try to find existing simulation for this machine
  try {
    const simulations = await listSimulations(props.machineId)
    if (simulations.length > 0) {
      const activeSimulation = simulations.find(s => s.status === 'running' || s.status === 'paused')
      if (activeSimulation) {
        currentSimulationId.value = activeSimulation.id
      }
    }
  } catch (error) {
    console.error('Failed to load existing simulations:', error)
  }
  
  // Poll for status updates every 30 seconds
  const interval = setInterval(loadStatus, 30000)
  return () => clearInterval(interval)
})
</script>

<style scoped>
.simulation-controls {
  @apply flex items-center gap-2 flex-wrap;
}

.schedule-badge {
  @apply inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400;
  @apply border border-blue-200 dark:border-blue-800/50;
}

/* Responsive adjustments */
@media (max-width: 640px) {
  .simulation-controls {
    @apply gap-1.5;
  }
  
  .schedule-badge {
    @apply px-2 py-0.5 text-2xs;
  }
}
</style>
