<template>
  <div :class="styles['manual-data-form']">
    <header :class="styles['form-header']">
      <h3 :class="styles['form-title']">Manual Telemetry Ingestion</h3>
      <p :class="styles['form-sub']">Directly inject technical observations into the active digital cluster.</p>
    </header>

    <form @submit.prevent="handleSubmit" :class="styles['form-content']">
      <div :class="styles['form-grid']">
        <div :class="styles['form-column']">
          <label :class="styles['label']">Target Node</label>
          <UiSelect v-model="form.machineId" required :class="styles['select']">
            <option v-for="m in machines" :key="m.id" :value="m.id">{{ m.name }}</option>
          </UiSelect>
        </div>

        <div :class="styles['form-column']">
          <label :class="styles['label']">Stream Type</label>
          <UiSelect v-model="form.dataType" required :class="styles['select']">
            <option value="Temperature">Temperature (°C)</option>
            <option value="Vibration">Vibration (mm/s)</option>
            <option value="Pressure">Pressure (PSI)</option>
            <option value="NoiseLevel">Noise Level (dB)</option>
          </UiSelect>
        </div>

        <div :class="styles['form-column']">
          <UiInput
            v-model="form.value"
            type="number"
            step="0.01"
            label="Metric Value"
            required
          >
            <template #prefix><Zap :width="14" :height="14" /></template>
          </UiInput>
        </div>

        <div :class="styles['form-column']">
          <UiInput
            v-model="form.timestamp"
            type="datetime-local"
            label="Historical Timestamp"
            required
          >
            <template #prefix><Clock :width="14" :height="14" /></template>
          </UiInput>
        </div>
      </div>

      <div :class="styles['form-actions']">
        <UiButton 
          type="submit" 
          variant="primary" 
          :loading="submitting"
          :class="styles['submit-btn']"
        >
          Inject Stream Record
        </UiButton>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, useCssModule } from 'vue'
import { useToast } from '@/composables/useToast'
import UiButton from '../ui/UiButton.vue'
import UiInput from '../ui/UiInput.vue'
import UiSelect from '../ui/UiSelect.vue'
import { Zap, Clock } from 'lucide-vue-next'
import { fetchMachines } from '@/services/machines.service'
import { ingestTelemetry } from '@/services/telemetry.service'
import type { MachineDto } from '@/api/types'

const styles = useCssModule()
const toast = useToast()
const machines = ref<MachineDto[]>([])
const submitting = ref(false)

const form = reactive({
  machineId: '',
  dataType: 'Temperature',
  value: 0,
  timestamp: new Date().toISOString().slice(0, 16)
})

async function handleSubmit() {
  if (!form.machineId) {
    toast.error('Target node must be identified')
    return
  }
  submitting.value = true
  try {
    await ingestTelemetry({
      machineId: form.machineId,
      dataType: form.dataType,
      data: { value: form.value },
      timestamp: new Date(form.timestamp).toISOString()
    })
    toast.success(`${form.dataType} telemetry synchronized`)
    form.value = 0
  } catch (err) {
    toast.error('Asynchronous injection failed')
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  try {
    machines.value = await fetchMachines()
    if (machines.value.length > 0) form.machineId = machines.value[0].id
  } catch (err) {
    toast.error('Failed to resolve cluster nodes')
  }
})
</script>

<style module>
.manual-data-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.form-header {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.form-title {
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.form-sub {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  margin: 0;
}

.form-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-20);
}

.form-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-dim);
  margin-left: var(--space-2);
}

.select {
  width: 100%;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: var(--space-8);
  border-top: 1px solid var(--color-border-subtle);
}

.submit-btn {
  min-width: 160px;
}

@media (max-width: 640px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>
