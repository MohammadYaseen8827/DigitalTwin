<template>
  <transition name="fade">
    <div v-if="open" class="dialog-overlay" role="alertdialog" aria-modal="true" @click.self="$emit('cancel')">
      <div class="dialog-card">
        <header class="dialog-header">
          <h3>Delete Machine</h3>
        </header>

        <div class="dialog-content">
          <p>
            Are you sure you want to delete <strong>{{ machine?.name ?? 'this machine' }}</strong>?
          </p>
          <p class="warning">This action cannot be undone.</p>
        </div>

        <footer class="dialog-footer">
          <BaseButton variant="ghost" @click="$emit('cancel')">
            Cancel
          </BaseButton>
          <BaseButton variant="critical" :loading="deleting" @click="handleDelete">
            Delete
          </BaseButton>
        </footer>
      </div>
    </div>
  </transition>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import type { MachineDto } from '@/api/types'
import { deleteMachine } from '@/services/machines.service'

interface Props {
  open: boolean
  machine?: MachineDto
}

interface Emits {
  (e: 'cancel'): void
  (e: 'success'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const toast = useToast()
const deleting = ref(false)

watch(
  () => props.open,
  value => {
    if (!value) {
      deleting.value = false
    }
  }
)

async function handleDelete() {
  if (!props.machine) {
    return
  }

  deleting.value = true
  try {
    await deleteMachine(props.machine.id)
    toast.success('Machine deleted successfully')
    emit('success')
  } catch (error) {
    console.error('Failed to delete machine:', error)
    toast.error('Unable to delete machine right now')
  } finally {
    deleting.value = false
  }
}
</script>

<style scoped>
.dialog-overlay {
  position: fixed;
  inset: 0;
  background: rgba(8, 15, 35, 0.65);
  backdrop-filter: blur(6px);
  display: grid;
  place-items: center;
  z-index: 2000;
  padding: var(--spacing-xl);
}

.dialog-card {
  width: min(420px, 100%);
  background: var(--color-surface);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-large);
  display: grid;
  gap: var(--spacing-lg);
  padding: var(--spacing-xl);
}

.dialog-header h3 {
  margin: 0;
  font-size: 1.25rem;
  color: var(--color-text-primary);
}

.dialog-content {
  display: grid;
  gap: var(--spacing-sm);
  color: var(--color-text-secondary);
}

.warning {
  color: var(--color-critical-500);
  font-weight: 600;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--spacing-md);
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
