<script setup lang="ts">
import { ref, reactive, useCssModule } from 'vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import { useMaintenanceStore } from '@/stores/maintenance.store'
import { Calendar, Wrench, Info } from 'lucide-vue-next'

const props = defineProps<{
  open: boolean
  machineId: string
  alertId?: string
  alertMessage?: string
}>()

const emit = defineEmits(['cancel', 'success'])

const styles = useCssModule()
const maintenanceStore = useMaintenanceStore()
const submitting = ref(false)

const form = reactive({
  type: 'Preventive',
  plannedDate: new Date(Date.now() + 86400000).toISOString().slice(0, 16), // Default to tomorrow
  notes: ''
})

const typeOptions = [
  { label: 'PREVENTIVE', value: 'Preventive' },
  { label: 'CORRECTIVE', value: 'Corrective' },
  { label: 'INSPECTION', value: 'Inspection' },
  { label: 'PART REPLACEMENT', value: 'PartReplacement' }
]

async function handleSubmit() {
  submitting.value = true
  try {
    await maintenanceStore.planNewMaintenance({
      machineId: props.machineId,
      type: form.type,
      plannedDate: new Date(form.plannedDate).toISOString(),
      notes: form.notes,
      alertId: props.alertId
    })
    emit('success')
  } catch (err) {
    console.error(err)
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <UiModal :is-open="open" maxWidth="md" @close="$emit('cancel')">
    <UiCard variant="glass" padding="xl">
      <template #header>
        <div :class="styles['modal-header']">
          <div :class="styles['modal-eyebrow']">MAINTENANCE SCHEDULER</div>
          <h2 :class="styles['modal-title']">Plan Maintenance</h2>
          <p :class="styles['modal-sub']">Provisioning technical intervention for asset optimization</p>
        </div>
      </template>

      <form @submit.prevent="handleSubmit" :class="styles['modal-form']">
        <div :class="styles['form-grid']">
          <UiSelect v-model="form.type" label="Intervention Type" required>
            <option v-for="opt in typeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </UiSelect>

          <UiInput 
            v-model="form.plannedDate" 
            type="datetime-local" 
            label="Target Schedule" 
            required 
          />

          <div :class="styles['full-width']">
            <label :class="styles['field-label']">Technical Notes</label>
            <textarea 
              v-model="form.notes" 
              placeholder="Describe the maintenance protocol and required components..." 
              rows="4"
              :class="styles['textarea']"
            ></textarea>
          </div>
        </div>

        <div v-if="alertMessage" :class="styles['alert-info']">
          <Info :width="14" :height="14" />
          <div :class="styles['alert-text']">
            <strong>LINKED ANOMALY:</strong> {{ alertMessage }}
          </div>
        </div>

        <div :class="styles['modal-footer']">
          <UiButton variant="ghost" type="button" @click="$emit('cancel')">Abort</UiButton>
          <UiButton variant="primary" type="submit" :loading="submitting">
            <Wrench :width="16" :height="16" />
            Schedule Protocol
          </UiButton>
        </div>
      </form>
    </UiCard>
  </UiModal>
</template>

<style module>
.modal-header {
  margin-bottom: var(--space-32);
}

.modal-eyebrow {
  font-size: 10px;
  font-weight: 900;
  color: var(--color-primary);
  letter-spacing: 0.2em;
  margin-bottom: var(--space-6);
}

.modal-title {
  font-size: var(--font-size-2xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
  letter-spacing: -0.02em;
}

.modal-sub {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin-top: var(--space-6);
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-20);
}

.full-width {
  grid-column: 1 / -1;
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.field-label {
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.textarea {
  width: 100%;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-12);
  color: var(--color-text-primary);
  font-family: var(--font-family);
  font-size: var(--font-size-sm);
  resize: none;
  transition: all var(--transition-fast);
}

.textarea:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 2px var(--color-primary-muted);
}

.alert-info {
  padding: var(--space-16);
  background: var(--color-primary-muted);
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-lg);
  display: flex;
  gap: var(--space-12);
  color: var(--color-primary);
  font-size: 11px;
}

.alert-text {
  line-height: 1.4;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
  padding-top: var(--space-24);
  border-top: 1px solid var(--color-border-subtle);
}

@media (max-width: 640px) {
  .form-grid { grid-template-columns: 1fr; }
}
</style>
