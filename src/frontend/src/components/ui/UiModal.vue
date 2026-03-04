<script setup lang="ts">
import { useCssModule } from 'vue'
import {
  TransitionRoot,
  TransitionChild,
  Dialog,
  DialogPanel,
} from '@headlessui/vue'

interface Props {
  isOpen: boolean
  title?: string
  description?: string
  maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl'
}

const props = withDefaults(defineProps<Props>(), {
  isOpen: false,
  maxWidth: 'md'
})

const emit = defineEmits<{
  (e: 'close'): void
}>()

const styles = useCssModule()

function closeModal() {
  emit('close')
}
</script>

<template>
  <TransitionRoot appear :show="isOpen" as="template">
    <Dialog as="div" @close="closeModal" :class="styles['dialog']">
      <TransitionChild
        as="template"
        :enter="styles['transition-enter']"
        enter-from="opacity-0"
        enter-to="opacity-100"
        :leave="styles['transition-leave']"
        leave-from="opacity-100"
        leave-to="opacity-0"
      >
        <div :class="styles['backdrop']" aria-hidden="true" />
      </TransitionChild>

      <div :class="styles['panel-container']">
        <div :class="styles['panel-wrapper']">
          <TransitionChild
            as="template"
            :enter="styles['transition-enter-panel']"
            enter-from="opacity-0 scale-95 translateY-4"
            enter-to="opacity-100 scale-100 translateY-0"
            :leave="styles['transition-leave-panel']"
            leave-from="opacity-100 scale-100 translateY-0"
            leave-to="opacity-0 scale-95 translateY-4"
          >
            <DialogPanel :class="[styles['panel'], styles[`max-width--${maxWidth}`]]">
              <slot />
            </DialogPanel>
          </TransitionChild>
        </div>
      </div>
    </Dialog>
  </TransitionRoot>
</template>

<style module>
.dialog {
  position: fixed;
  inset: 0;
  z-index: 5000;
  overflow-y: auto;
}

.backdrop {
  position: fixed;
  inset: 0;
  background: var(--color-surface-overlay);
  backdrop-filter: var(--backdrop-blur);
}

.panel-container {
  position: fixed;
  inset: 0;
  overflow-y: auto;
}

.panel-wrapper {
  display: flex;
  min-height: 100%;
  align-items: center;
  justify-content: center;
  padding: var(--space-16);
  text-align: center;
}

.panel {
  width: 100%;
  text-align: left;
  vertical-align: middle;
}

/* Transitions */
.transition-enter {
  transition: opacity var(--transition-normal) var(--ease-premium);
}

.transition-leave {
  transition: opacity var(--transition-fast) var(--ease-premium);
}

.transition-enter-panel {
  transition: all var(--transition-normal) var(--ease-premium);
}

.transition-leave-panel {
  transition: all var(--transition-fast) var(--ease-premium);
}

/* Max Widths */
.max-width--sm { max-width: 384px; }
.max-width--md { max-width: 512px; }
.max-width--lg { max-width: 640px; }
.max-width--xl { max-width: 768px; }
.max-width--2xl { max-width: 896px; }

/* Animation Utility States */
:global(.opacity-0) { opacity: 0; }
:global(.opacity-100) { opacity: 1; }
:global(.scale-95) { transform: scale(0.95); }
:global(.scale-100) { transform: scale(1); }
:global(.translateY-4) { transform: translateY(var(--space-16)); }
:global(.translateY-0) { transform: translateY(0); }
</style>
