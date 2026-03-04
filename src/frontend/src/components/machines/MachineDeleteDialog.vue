<script setup lang="ts">
import { ref, watch, useCssModule } from 'vue'
import { useToast } from '@/composables/useToast'
import type { MachineDto } from '@/api/types'
import { deleteMachine } from '@/services/machines.service'
import UiModal from '@/components/ui/UiModal.vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import { AlertTriangle, Trash2 } from 'lucide-vue-next'

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

const styles = useCssModule()
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
  if (!props.machine) return

  deleting.value = true
  try {
    await deleteMachine(props.machine.id)
    toast.success('Asset decommissioned')
    emit('success')
  } catch (error) {
    toast.error('Decommission failure')
  } finally {
    deleting.value = false
  }
}
</script>

<template>
  <UiModal :is-open="open" maxWidth="sm" @close="$emit('cancel')">
    <UiCard variant="glass" padding="xl">
      <template #header>
        <div :class="styles['header']">
          <div :class="styles['icon-wrap']">
            <AlertTriangle :width="20" :height="20" />
          </div>
          <div :class="styles['titles']">
            <h3 :class="styles['title']">Decommission Asset</h3>
            <p :class="styles['sub']">Protocol termination sequence</p>
          </div>
        </div>
      </template>

      <div :class="styles['content']">
        <p :class="styles['message']">
          Are you sure you want to terminate the digital twin for <strong>{{ machine?.name || 'this node' }}</strong>?
        </p>
        <div :class="styles['warning-box']">
          <Trash2 :width="14" :height="14" />
          <span>This action is irreversible and will purge all telemetry history.</span>
        </div>
      </div>

      <div :class="styles['footer']">
        <UiButton variant="ghost" @click="$emit('cancel')">Abort</UiButton>
        <UiButton variant="primary" :loading="deleting" @click="handleDelete" :class="styles['btn-danger']">
          Confirm Termination
        </UiButton>
      </div>
    </UiCard>
  </UiModal>
</template>

<style module>
.header {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.icon-wrap {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--color-danger-muted);
  color: var(--color-danger);
  display: flex;
  align-items: center;
  justify-content: center;
}

.title {
  font-size: var(--font-size-lg);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
  letter-spacing: -0.01em;
}

.sub {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.content {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.message {
  font-size: var(--font-size-sm);
  line-height: var(--font-lineheight-relaxed);
  color: var(--color-text-secondary);
}

.warning-box {
  padding: var(--space-12);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  display: flex;
  gap: var(--space-12);
  font-size: 11px;
  font-weight: 600;
  color: var(--color-danger);
  line-height: 1.4;
}

.footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
  margin-top: var(--space-24);
  padding-top: var(--space-20);
  border-top: 1px solid var(--color-border-subtle);
}

.btn-danger {
  background: var(--color-danger) !important;
  color: white !important;
}

.btn-danger:hover {
  background: color-mix(in srgb, var(--color-danger) 85%, black) !important;
}
</style>
