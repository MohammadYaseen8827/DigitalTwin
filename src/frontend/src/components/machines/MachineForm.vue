<template>
  <BaseCard variant="soft" class="machine-form">
    <template #header>
      <div class="header-content">
        <span>{{ isEdit ? 'Edit Machine' : 'Create Machine' }}</span>
        <span v-if="isEdit" class="machine-id">ID: {{ props.machine?.id.substring(0, 8) }}...</span>
      </div>
    </template>

    <form class="form-grid" @submit.prevent="handleSubmit">
      <!-- Basic Information -->
      <FormSection title="Basic Information" description="Core machine identification and classification">
        <BaseInput
          v-model="form.name"
          label="Machine Name"
          placeholder="Enter machine name"
          required
          :error="errors.name"
        />

        <BaseInput
          v-model="form.serialNumber"
          label="Serial Number"
          placeholder="Enter serial number"
          :error="errors.serialNumber"
        />

        <BaseSelect
          v-model="form.type"
          label="Machine Type"
          :options="machineTypes"
          required
          :error="errors.type"
        />

        <BaseSelect
          v-model="form.manufacturer"
          label="Manufacturer"
          :options="manufacturers"
          :error="errors.manufacturer"
        />

        <BaseInput
          v-model="form.model"
          label="Model"
          placeholder="Enter model number"
          :error="errors.model"
        />

        <BaseInput
          v-model="form.location"
          label="Location"
          placeholder="Enter location"
          :error="errors.location"
        />
      </FormSection>

      <!-- Operational Details -->
      <FormSection title="Operational Details" description="Current status and operational parameters">
        <BaseSelect
          v-model="form.status"
          label="Status"
          :options="statusOptions"
          required
          :error="errors.status"
        />

        <BaseSelect
          v-model="form.criticality"
          label="Criticality Level"
          :options="criticalityLevels"
          :error="errors.criticality"
        />

        <BaseInput
          v-model="form.installationDate"
          type="date"
          label="Installation Date"
          :error="errors.installationDate"
        />

        <BaseInput
          v-model="form.warrantyExpiry"
          type="date"
          label="Warranty Expiry"
          :error="errors.warrantyExpiry"
        />
      </FormSection>

      <!-- Maintenance Schedule -->
      <FormSection title="Maintenance Schedule" description="Planned and historical maintenance dates">
        <BaseInput
          v-model="form.lastMaintenance"
          type="datetime-local"
          label="Last Maintenance"
          :error="errors.lastMaintenance"
        />

        <BaseInput
          v-model="form.nextMaintenance"
          type="datetime-local"
          label="Next Maintenance"
          :error="errors.nextMaintenance"
        />

        <BaseInput
          v-model="form.maintenanceInterval"
          type="number"
          label="Maintenance Interval (days)"
          placeholder="30"
          :min="1"
          :max="365"
          :error="errors.maintenanceInterval"
        />
      </FormSection>

      <!-- Configuration -->
      <FormSection title="Configuration" description="Machine configuration and settings">
        <BaseSelect
          v-model="form.degradationModel"
          label="Degradation Model"
          :options="degradationModels"
          :error="errors.degradationModel"
        />

        <BaseInput
          v-model="form.threshold"
          type="number"
          label="Failure Threshold"
          placeholder="0.8"
          :step="0.01"
          :min="0"
          :max="1"
          :error="errors.threshold"
        />

        <div class="checkbox-group">
          <label class="checkbox-label">
            <input 
              v-model="form.isActive" 
              type="checkbox" 
              class="checkbox"
            />
            Active Machine
          </label>
        </div>
      </FormSection>

      <!-- Metadata -->
      <FormSection title="Metadata" description="Optional JSON metadata to store custom properties">
        <BaseInput
          v-model="metadataText"
          type="textarea"
          placeholder='{"shift": "night", "operator": "Alicia", "department": "Production"}'
          :error="errors.metadata"
          rows="4"
        />
        <p class="help-text">Enter valid JSON to store additional machine properties</p>
      </FormSection>

      <div class="form-actions">
        <BaseButton type="submit" variant="primary" :loading="submitting">
          {{ isEdit ? 'Update Machine' : 'Create Machine' }}
        </BaseButton>
        <BaseButton type="button" variant="ghost" @click="$emit('cancel')">
          Cancel
        </BaseButton>
      </div>
    </form>
  </BaseCard>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import FormSection from '@/components/base/FormSection.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import type { MachineCreateDto, MachineDto, MachineUpdateDto } from '@/api/types'
import { createMachine, updateMachine } from '@/services/machines.service'

interface Props {
  machine?: MachineDto
}

interface Emits {
  (e: 'cancel'): void
  (e: 'success', machine: MachineDto): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const isEdit = computed(() => !!props.machine)
const toast = useToast()
const submitting = ref(false)
const errors = reactive<Record<string, string>>({})
const metadataText = ref('')

const defaultForm = () => ({
  name: props.machine?.name ?? '',
  serialNumber: props.machine?.serialNumber ?? '',
  type: props.machine?.type ?? '',
  manufacturer: props.machine?.manufacturer ?? '',
  model: props.machine?.model ?? '',
  status: typeof props.machine?.status === 'string' ? props.machine?.status : 'operational',
  criticality: props.machine?.criticality ?? 3,
  location: props.machine?.location ?? '',
  installationDate: props.machine?.installationDate ? formatDateForDateInput(props.machine.installationDate) : '',
  warrantyExpiry: props.machine?.warrantyExpiry ? formatDateForDateInput(props.machine.warrantyExpiry) : '',
  lastMaintenance: props.machine?.lastMaintenance ? formatDateForInput(props.machine.lastMaintenance) : '',
  nextMaintenance: props.machine?.nextMaintenance ? formatDateForInput(props.machine.nextMaintenance) : '',
  maintenanceInterval: props.machine?.maintenanceInterval ?? 30,
  degradationModel: props.machine?.degradationModel ?? 'wiener',
  threshold: props.machine?.threshold ?? 0.8,
  isActive: props.machine?.isActive ?? true
})

const form = reactive(defaultForm())

watch(
  () => props.machine,
  newMachine => {
    Object.assign(form, {
      name: newMachine?.name ?? '',
      serialNumber: newMachine?.serialNumber ?? '',
      type: newMachine?.type ?? '',
      manufacturer: newMachine?.manufacturer ?? '',
      model: newMachine?.model ?? '',
      status: typeof newMachine?.status === 'string' ? newMachine.status : 'operational',
      criticality: newMachine?.criticality ?? 3,
      location: newMachine?.location ?? '',
      installationDate: newMachine?.installationDate ? formatDateForDateInput(newMachine.installationDate) : '',
      warrantyExpiry: newMachine?.warrantyExpiry ? formatDateForDateInput(newMachine.warrantyExpiry) : '',
      lastMaintenance: newMachine?.lastMaintenance ? formatDateForInput(newMachine.lastMaintenance) : '',
      nextMaintenance: newMachine?.nextMaintenance ? formatDateForInput(newMachine.nextMaintenance) : '',
      maintenanceInterval: newMachine?.maintenanceInterval ?? 30,
      degradationModel: newMachine?.degradationModel ?? 'wiener',
      threshold: newMachine?.threshold ?? 0.8,
      isActive: newMachine?.isActive ?? true
    })
    metadataText.value = newMachine?.metadata ? JSON.stringify(newMachine.metadata, null, 2) : ''
  },
  { immediate: true }
)

const machineTypes = [
  { label: 'CNC Machine', value: 'cnc' },
  { label: 'Injection Molder', value: 'injection_molder' },
  { label: 'Press', value: 'press' },
  { label: 'Robot', value: 'robot' },
  { label: 'Conveyor', value: 'conveyor' },
  { label: 'Pump', value: 'pump' },
  { label: 'Motor', value: 'motor' },
  { label: 'Compressor', value: 'compressor' },
  { label: 'Turbine', value: 'turbine' }
]

const manufacturers = [
  { label: 'Siemens', value: 'siemens' },
  { label: 'ABB', value: 'abb' },
  { label: 'Fanuc', value: 'fanuc' },
  { label: 'KUKA', value: 'kuka' },
  { label: 'Universal Robots', value: 'ur' },
  { label: 'Mazak', value: 'mazak' },
  { label: 'Haas', value: 'haas' },
  { label: 'DMG Mori', value: 'dmg_mori' }
]

const criticalityLevels = [
  { label: 'Low (1)', value: 1 },
  { label: 'Medium Low (2)', value: 2 },
  { label: 'Medium (3)', value: 3 },
  { label: 'Medium High (4)', value: 4 },
  { label: 'High (5)', value: 5 }
]

const degradationModels = [
  { label: 'Wiener Process', value: 'wiener' },
  { label: 'Exponential', value: 'exponential' },
  { label: 'Linear', value: 'linear' },
  { label: 'Physics-Informed', value: 'physics' }
]

const statusOptions = [
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

function parseMetadata(): Record<string, unknown> | undefined {
  if (!metadataText.value.trim()) {
    delete errors.metadata
    return undefined
  }

  try {
    delete errors.metadata
    return JSON.parse(metadataText.value)
  } catch {
    errors.metadata = 'Metadata must be valid JSON'
    return undefined
  }
}

function isoOrUndefined(value: string): string | undefined {
  return value ? new Date(value).toISOString() : undefined
}

function formatDateForInput(date: string): string {
  const parsed = new Date(date)
  if (Number.isNaN(parsed.getTime())) {
    return ''
  }

  return `${parsed.getFullYear()}-${String(parsed.getMonth() + 1).padStart(2, '0')}-${String(parsed.getDate()).padStart(2, '0')}T${String(parsed.getHours()).padStart(2, '0')}:${String(parsed.getMinutes()).padStart(2, '0')}`
}

function formatDateForDateInput(date: string): string {
  const parsed = new Date(date)
  if (Number.isNaN(parsed.getTime())) {
    return ''
  }

  return `${parsed.getFullYear()}-${String(parsed.getMonth() + 1).padStart(2, '0')}-${String(parsed.getDate()).padStart(2, '0')}`
}

async function handleSubmit() {
  submitting.value = true
  Object.keys(errors).forEach(key => delete errors[key])

  try {
    const metadata = parseMetadata()
    if (errors.metadata) {
      return
    }

    const payload = {
      name: form.name,
      serialNumber: form.serialNumber,
      type: form.type,
      manufacturer: form.manufacturer,
      model: form.model,
      status: form.status,
      criticality: form.criticality,
      location: form.location,
      installationDate: form.installationDate ? new Date(form.installationDate).toISOString() : undefined,
      warrantyExpiry: form.warrantyExpiry ? new Date(form.warrantyExpiry).toISOString() : undefined,
      lastMaintenance: isoOrUndefined(form.lastMaintenance),
      nextMaintenance: isoOrUndefined(form.nextMaintenance),
      maintenanceInterval: form.maintenanceInterval,
      degradationModel: form.degradationModel,
      threshold: form.threshold,
      isActive: form.isActive,
      metadata
    }

    let result: MachineDto

    if (isEdit.value && props.machine) {
      result = await updateMachine(props.machine.id, payload as MachineUpdateDto)
    } else {
      result = await createMachine(payload as MachineCreateDto)
    }

    emit('success', result)
  } catch (error: any) {
    if (error.response?.data?.errors) {
      const fieldErrors = Object.entries(error.response.data.errors).reduce<Record<string, string>>((acc, [field, messages]) => {
        acc[field] = Array.isArray(messages) ? messages[0] : String(messages)
        return acc
      }, {})
      Object.assign(errors, fieldErrors)
    } else {
      toastError(error)
    }
  } finally {
    submitting.value = false
  }
}

function toastError(error: unknown) {
  console.error('Machine form submission failed:', error)
  const message = (error as any)?.response?.data?.message ?? 'Unable to process machine request'
  toast.error(message)
}
</script>

<style scoped>
.machine-form {
  padding: var(--spacing-lg);
  max-width: 1200px;
  margin: 0 auto;
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--spacing-sm);
}

.machine-id {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  font-family: monospace;
}

.form-grid {
  display: grid;
  gap: var(--spacing-xl);
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
}

.form-section {
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border-subtle);
}

.form-section-header {
  margin-bottom: var(--spacing-md);
}

.form-section-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--color-text-primary);
  margin-bottom: var(--spacing-xs);
}

.form-section-description {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.checkbox-group {
  display: flex;
  align-items: center;
  margin-top: var(--spacing-sm);
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  cursor: pointer;
  font-weight: 500;
  color: var(--color-text-primary);
}

.checkbox {
  width: 18px;
  height: 18px;
  accent-color: var(--color-primary);
}

.help-text {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin-top: var(--spacing-xs);
  font-style: italic;
}

.form-actions {
  display: flex;
  gap: var(--spacing-md);
  justify-content: flex-end;
  padding-top: var(--spacing-xl);
  border-top: 1px solid var(--color-border-subtle);
  margin-top: var(--spacing-xl);
}

.form-actions .base-button {
  min-width: 160px;
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .header-content {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .form-actions .base-button {
    width: 100%;
  }
}
</style>
