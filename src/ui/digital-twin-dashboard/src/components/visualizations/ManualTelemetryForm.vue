<template>
  <div class="manual-data-form glass-panel">
    <header class="form-header">
      <h3>Manual Telemetry Logging</h3>
      <p>Ingest real-world observations directly into the twin</p>
    </header>

    <form @submit.prevent="handleSubmit" class="form-content">
      <div class="form-row">
        <div class="form-group">
          <label>Machine</label>
          <select v-model="form.machineId" required>
            <option v-for="m in machines" :key="m.id" :value="m.id">{{ m.name }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Data Type</label>
          <select v-model="form.dataType" required>
            <option value="Temperature">Temperature (°C)</option>
            <option value="Vibration">Vibration (mm/s)</option>
            <option value="Pressure">Pressure (PSI)</option>
            <option value="NoiseLevel">Noise Level (dB)</option>
          </select>
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label>Value</label>
          <input type="number" step="0.01" v-model="form.value" required />
        </div>
        <div class="form-group">
          <label>Timestamp</label>
          <input type="datetime-local" v-model="form.timestamp" />
        </div>
      </div>

      <div class="form-actions">
        <BaseButton type="submit" variant="primary" :loading="submitting">Log Telemetry</BaseButton>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseButton from '../base/BaseButton.vue'
import { fetchMachines } from '@/services/machines.service'
import { ingestTelemetry } from '@/services/telemetry.service'
import type { MachineDto } from '@/api/types'

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
  if (!form.machineId) return
  submitting.value = true
  try {
    await ingestTelemetry({
      machineId: form.machineId,
      dataType: form.dataType,
      data: { value: form.value },
      timestamp: new Date(form.timestamp).toISOString()
    })
    toast.success(`${form.dataType} logged successfully`)
    form.value = 0
  } catch (err) {
    toast.error('Failed to log telemetry')
  } finally {
    submitting.value = false
  }
}

onMounted(async () => {
  machines.value = await fetchMachines()
  if (machines.value.length > 0) form.machineId = machines.value[0].id
})
</script>

<style scoped>
.manual-data-form {
  padding: 1.5rem;
}

.form-header {
  margin-bottom: 1.5rem;
}

.form-header h3 {
  margin: 0;
  font-size: 1.1rem;
}

.form-header p {
  margin: 0.25rem 0 0;
  font-size: 0.85rem;
  color: #8c8c8c;
}

.form-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}

.form-group label {
  font-size: 0.8rem;
  font-weight: 600;
  color: #595959;
}

.form-group input, .form-group select {
  padding: 0.6rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  font-size: 0.9rem;
}

.form-actions {
  margin-top: 0.5rem;
  display: flex;
  justify-content: flex-end;
}
</style>
