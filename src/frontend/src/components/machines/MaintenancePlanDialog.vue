<template>
  <div v-if="open" class="modal-overlay">
    <div class="modal-content glass-panel animate-slide-up">
      <header class="modal-header">
        <h2>Plan Maintenance</h2>
        <button class="close-btn" @click="$emit('cancel')">&times;</button>
      </header>

      <form @submit.prevent="handleSubmit" class="maintenance-form">
        <div class="form-group">
          <label>Maintenance Type</label>
          <select v-model="form.type" required>
            <option value="Preventive">Preventive</option>
            <option value="Corrective">Corrective</option>
            <option value="Inspection">Inspection</option>
            <option value="PartReplacement">Part Replacement</option>
          </select>
        </div>

        <div class="form-group">
          <label>Planned Date</label>
          <input type="datetime-local" v-model="form.plannedDate" required />
        </div>

        <div class="form-group">
          <label>Notes</label>
          <textarea v-model="form.notes" placeholder="Describe the maintenance tasks..." rows="4"></textarea>
        </div>

        <div v-if="alertMessage" class="alert-info">
          <strong>Linked Alert:</strong> {{ alertMessage }}
        </div>

        <div class="form-actions">
          <BaseButton type="button" variant="ghost" @click="$emit('cancel')">Cancel</BaseButton>
          <BaseButton type="submit" variant="primary" :loading="submitting">Schedule Maintenance</BaseButton>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import BaseButton from '../base/BaseButton.vue'
import { useMaintenanceStore } from '@/stores/maintenance.store'

const props = defineProps<{
  open: boolean
  machineId: string
  alertId?: string
  alertMessage?: string
}>()

const emit = defineEmits(['cancel', 'success'])

const maintenanceStore = useMaintenanceStore()
const submitting = ref(false)

const form = reactive({
  type: 'Preventive',
  plannedDate: new Date(Date.now() + 86400000).toISOString().slice(0, 16), // Default to tomorrow
  notes: ''
})

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

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
}

.modal-content {
  width: 100%;
  max-width: 500px;
  padding: 2rem;
  background: white;
  border-radius: var(--radius-lg, 12px);
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: var(--color-text-secondary);
}

.maintenance-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 600;
  font-size: 0.9rem;
}

.form-group input,
.form-group select,
.form-group textarea {
  padding: 0.75rem;
  border: 1px solid var(--color-border, #d9d9d9);
  border-radius: var(--radius-md, 8px);
  font-size: 1rem;
}

.alert-info {
  padding: 1rem;
  background: rgba(24, 144, 255, 0.1);
  border-radius: 8px;
  font-size: 0.85rem;
  color: var(--primary-color, #1890ff);
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}
</style>
