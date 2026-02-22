<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import { useRouter } from 'vue-router'
import MachineCard from '../components/MachineCard.vue'
import BaseButton from '../components/base/BaseButton.vue'
import BaseCard from '../components/base/BaseCard.vue'
import BaseSkeleton from '../components/base/BaseSkeleton.vue'
import SectionContainer from '../components/base/SectionContainer.vue'
import { fetchMachinesPage } from '@/services/machines.service'
import InlineSpinner from '../components/base/InlineSpinner.vue'
import { createSimulation, runSimulationStep, cancelSimulation } from '@/services/simulation.service'
import type { MachineDto } from '@/api/types'

const router = useRouter()
const toast = useToast()

const machines = ref<MachineDto[]>([])
const loading = ref(true)
const error = ref<string | null>(null)
const loadingMore = ref(false)
const total = ref(0)
const page = ref(1)
const pageSize = 12

const hasMore = computed(() => machines.value.length < total.value)
const totalLabel = computed(() => (total.value ? total.value : machines.value.length))

const fetchPage = async (pageToLoad: number, append = false) => {
  const result = await fetchMachinesPage({ page: pageToLoad, pageSize })

  const items = result.items ?? []

  if (append) {
    machines.value = [...machines.value, ...items]
  } else {
    machines.value = items
  }

  total.value = typeof result.total === 'number' ? result.total : machines.value.length
  page.value = typeof result.page === 'number' ? result.page : pageToLoad
}

const loadMachines = async () => {
  try {
    loading.value = true
    error.value = null
    page.value = 1
    
    await fetchPage(1, false)
  } catch (err) {
    console.error('Failed to fetch machines:', err)
    error.value = 'Failed to load machines. Please check your connection and try again.'
    machines.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

const loadMore = async () => {
  if (loading.value || loadingMore.value || !hasMore.value) return

  const nextPage = page.value + 1

  try {
    loadingMore.value = true
    await fetchPage(nextPage, true)
  } catch (err) {
    console.error('Failed to load additional machines:', err)
    toast.error('Unable to load more machines right now.')
  } finally {
    loadingMore.value = false
  }
}

const handleStartSimulation = async (machineId: string) => {
  try {
    // Create a new simulation
    const simulation = await createSimulation(machineId, {
      degradationModel: 'wiener',
      totalSteps: 100,
      intervalSeconds: 5,
      persistTelemetry: true
    })
    
    // Run the first step
    await runSimulationStep(simulation.id, machineId)
    
    const machine = machines.value.find(m => m.id === machineId)
    if (machine) {
      machine.status = 'Running'
    }
    toast.success('Simulation triggered successfully. Refresh to see latest telemetry.')
  } catch (err) {
    console.error(`Failed to start simulation for machine ${machineId}:`, err)
    toast.error('Unable to start simulation. Please try again later.')
  }
}

const handleStopSimulation = async (machineId: string, simulationId?: string) => {
  try {
    if (simulationId) {
      await cancelSimulation(simulationId, machineId)
    }
    const machine = machines.value.find(m => m.id === machineId)
    if (machine) {
      machine.status = 'Idle'
    }
    toast.info('Cancelled simulation for this machine.')
  } catch (err) {
    console.error(`Failed to stop simulation for machine ${machineId}:`, err)
    toast.error('Unable to cancel simulation right now.')
  }
}

const handleScheduleSimulation = async (machineId: string) => {
  // This would be handled by the MachineCard component
  console.log(`Scheduling simulation for machine ${machineId}`)
}

const handleCancelSchedule = async (machineId: string) => {
  // This would be handled by the MachineCard component
  console.log(`Cancelling schedule for machine ${machineId}`)
}

onMounted(async () => {
  await loadMachines()
})
</script>

<template>
  <SectionContainer title="Digital Twin Dashboard" eyebrow="Overview" maxWidth="xl" bordered>

    <BaseCard v-if="error" variant="bordered" class="state-card">
      <template #header>Something went wrong</template>
      <p class="state-message">{{ error }}</p>
      <BaseButton size="sm" variant="outline" @click="loadMachines">Try again</BaseButton>
    </BaseCard>

    <div v-else>
      <div v-if="loading" class="machine-grid">
        <BaseCard
          v-for="i in 6"
          :key="`machine-skeleton-${i}`"
          variant="soft"
          class="machine-skeleton"
        >
          <div class="machine-skeleton__header">
            <BaseSkeleton width="48px" height="48px" radius="var(--radius-xl)" />
            <div class="machine-skeleton__meta">
              <BaseSkeleton width="60%" height="18px" />
              <BaseSkeleton width="40%" height="14px" />
            </div>
          </div>
          <div class="machine-skeleton__body">
            <BaseSkeleton width="100%" height="12px" />
            <BaseSkeleton width="80%" height="12px" />
            <BaseSkeleton width="70%" height="12px" />
          </div>
          <div class="machine-skeleton__footer">
            <BaseSkeleton width="120px" height="36px" radius="var(--radius-full)" />
          </div>
        </BaseCard>
      </div>

      <template v-else>
        <div v-if="machines.length" class="machine-grid">
          <MachineCard
            v-for="machine in machines"
            :key="machine.id"
            :machine="machine"
            @start-simulation="handleStartSimulation"
            @stop-simulation="handleStopSimulation"
          />
        </div>

        <BaseCard v-else variant="bordered" class="state-card">
          <template #header>No machines found</template>
          <p class="state-message">There aren’t any machines available for this tenant yet.</p>
          <BaseButton size="sm" variant="outline" @click="loadMachines">Refresh</BaseButton>
        </BaseCard>

        <div v-if="machines.length" class="machines-footer">
          <p class="machines-summary">Showing {{ machines.length }} of {{ totalLabel }}</p>
          <BaseButton
            v-if="hasMore"
            size="sm"
            variant="outline"
            :disabled="loadingMore"
            @click="loadMore"
          >
            <InlineSpinner v-if="loadingMore">Loading more</InlineSpinner>
            <span v-else>Load more</span>
          </BaseButton>
          <InlineSpinner v-else>All machines loaded</InlineSpinner>
        </div>
      </template>
    </div>
  </SectionContainer>
</template>

<style scoped>
.state-card {
  max-width: 480px;
  margin: 0 auto;
  text-align: center;
}

.state-message {
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
  margin: 0;
}

.machine-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: var(--space-20);
}

.machines-footer {
  margin-top: var(--space-16);
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
  align-items: center;
  text-align: center;
}

.machines-summary {
  margin: 0;
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.machine-skeleton {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.machine-skeleton__header {
  display: flex;
  gap: var(--space-12);
  align-items: center;
}

.machine-skeleton__meta {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.machine-skeleton__body {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.machine-skeleton__footer {
  display: flex;
  justify-content: flex-start;
}

@media (max-width: 640px) {
  .machine-grid {
    grid-template-columns: 1fr;
  }
}
</style>
