<template>
  <transition name="dialog-fade">
    <div v-if="open" class="dialog-overlay" role="dialog" aria-modal="true" aria-labelledby="create-simulation-title" @click.self="$emit('cancel')">
      <div class="dialog-card">
        <header class="dialog-header">
          <h3 id="create-simulation-title">Create Simulation</h3>
          <p class="dialog-subtitle">Configure parameters for the next run on this machine.</p>
        </header>

        <form class="dialog-form" @submit.prevent="handleSubmit">
          <BaseSelect
            v-model="form.degradationModel"
            label="Degradation Model"
            :options="degradationModels"
            required
          />

          <BaseInput
            v-model.number="form.totalSteps"
            type="number"
            label="Total Steps"
            min="1"
            max="500"
            required
          />

          <BaseInput
            v-model.number="form.intervalSeconds"
            type="number"
            label="Interval (seconds)"
            min="1"
            max="3600"
            required
          />

          <BaseInput
            v-model.number="form.maxRuntimeSeconds"
            type="number"
            label="Max Runtime (seconds)"
            min="1"
            max="86400"
            hint="Optional limit for long-running simulations"
          />

          <label class="checkbox-field">
            <input v-model="form.persistTelemetry" type="checkbox" />
            <span>Persist telemetry output</span>
          </label>

          <footer class="dialog-footer">
            <BaseButton type="button" variant="ghost" @click="$emit('cancel')">
              Cancel
            </BaseButton>
            <BaseButton type="submit" variant="primary" :loading="submitting">
              Create Simulation
            </BaseButton>
          </footer>
        </form>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { computed, reactive, watch, ref } from 'vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { createSimulation, type SimulationStateDto } from '@/services/simulation.service'

interface Props {
  open: boolean
  machineId: string
}

interface Emits {
  (e: 'cancel'): void
  (e: 'success', simulation: SimulationStateDto): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const submitting = ref(false)

const initialForm = () => ({
  degradationModel: 'wiener' as 'wiener' | 'markov' | 'physics_based',
  totalSteps: 50,
  intervalSeconds: 5,
  maxRuntimeSeconds: 120,
  persistTelemetry: true
})

const form = reactive(initialForm())

watch(
  () => props.open,
  value => {
    if (value) {
      Object.assign(form, initialForm())
    }
  }
)

const degradationModels = computed(() => [
  { label: 'Wiener Process', value: 'wiener' },
  { label: 'Markov Chain', value: 'markov' },
  { label: 'Physics-based', value: 'physics_based' }
])

async function handleSubmit() {
  submitting.value = true
  try {
    const simulation = await createSimulation(props.machineId, {
      totalSteps: form.totalSteps,
      intervalSeconds: form.intervalSeconds,
      maxRuntimeSeconds: form.maxRuntimeSeconds,
      persistTelemetry: form.persistTelemetry,
      degradationModel: form.degradationModel
    })
    emit('success', simulation)
  } catch (error) {
    console.error('Failed to create simulation:', error)
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(8px);
  display: grid;
  place-items: center;
  padding: var(--spacing-xl);
  z-index: 3000;
}

.dialog-card {
  width: min(520px, 100%);
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-large);
  padding: var(--spacing-xl);
  display: grid;
  gap: var(--spacing-lg);
}

.dialog-header {
  display: grid;
  gap: var(--spacing-xs);
}

.dialog-header h3 {
  margin: 0;
  font-size: 1.5rem;
  color: var(--color-text-primary);
}

.dialog-subtitle {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 0.95rem;
}

.dialog-form {
  display: grid;
  gap: var(--spacing-md);
}

.checkbox-field {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  font-size: 0.95rem;
  color: var(--color-text-secondary);
}

.checkbox-field input[type='checkbox'] {
  width: 18px;
  height: 18px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-md);
  margin-top: var(--spacing-sm);
}

.dialog-fade-enter-active,
.dialog-fade-leave-active {
  transition: opacity 0.2s ease;
}

.dialog-fade-enter-from,
.dialog-fade-leave-to {
  opacity: 0;
}
</style>
