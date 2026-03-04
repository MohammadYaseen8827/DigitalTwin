<script setup lang="ts">
import { useCssModule } from 'vue'
import {
  TransitionRoot,
  TransitionChild,
  Dialog,
  DialogPanel,
} from '@headlessui/vue'
import { X } from 'lucide-vue-next'

interface Props {
  isOpen: boolean
  title?: string
  description?: string
  width?: 'sm' | 'md' | 'lg' | 'xl' | '2xl'
}

const props = withDefaults(defineProps<Props>(), {
  isOpen: false,
  width: 'md'
})

const emit = defineEmits<{
  (e: 'close'): void
}>()

const styles = useCssModule()

function closePanel() {
  emit('close')
}
</script>

<template>
  <TransitionRoot appear :show="isOpen" as="template">
    <Dialog as="div" @close="closePanel" :class="styles['dialog']">
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
        <TransitionChild
          as="template"
          :enter="styles['transition-enter-panel']"
          enter-from="translateX-full"
          enter-to="translateX-0"
          :leave="styles['transition-leave-panel']"
          leave-from="translateX-0"
          leave-to="translateX-full"
        >
          <DialogPanel :class="[styles['panel'], styles[`width--${width}`]]">
            <div :class="styles['panel-content']">
              <header :class="styles['header']">
                <div :class="styles['title-group']">
                  <h2 v-if="title" :class="styles['title']">{{ title }}</h2>
                  <p v-if="description" :class="styles['description']">{{ description }}</p>
                </div>
                <button :class="styles['close-btn']" @click="closePanel">
                  <X :width="18" :height="18" />
                </button>
              </header>
              <div :class="styles['body']">
                <slot />
              </div>
            </div>
          </DialogPanel>
        </TransitionChild>
      </div>
    </Dialog>
  </TransitionRoot>
</template>

<style module>
.dialog {
  position: fixed;
  inset: 0;
  z-index: 5000;
  overflow: hidden;
}

.backdrop {
  position: fixed;
  inset: 0;
  background: var(--color-surface-overlay);
  backdrop-filter: var(--backdrop-blur-sm);
}

.panel-container {
  position: fixed;
  inset: 0;
  display: flex;
  justify-content: flex-end;
  pointer-events: none;
}

.panel {
  width: 100%;
  pointer-events: auto;
  background: var(--color-depth-1);
  border-left: 1px solid var(--color-border);
  box-shadow: var(--shadow-overlay);
  display: flex;
  flex-direction: column;
}

.panel-content {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.header {
  padding: var(--space-24);
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  background: var(--color-depth-0);
  border-bottom: 1px solid var(--color-border-subtle);
}

.title {
  margin: 0;
  font-size: var(--font-size-lg);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: var(--font-tracking-tight);
}

.description {
  margin: 4px 0 0;
  font-size: var(--font-size-xs);
  color: var(--color-text-dim);
  font-weight: 500;
}

.close-btn {
  background: transparent;
  border: none;
  color: var(--color-text-dim);
  cursor: pointer;
  transition: all var(--transition-fast);
  padding: var(--space-4);
  border-radius: var(--radius-sm);
}

.close-btn:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.body {
  flex: 1;
  overflow-y: auto;
  padding: var(--space-24);
}

/* Transitions */
.transition-enter { transition: opacity var(--transition-normal) var(--ease-premium); }
.transition-leave { transition: opacity var(--transition-fast) var(--ease-premium); }

.transition-enter-panel { transition: transform var(--transition-normal) var(--ease-premium); }
.transition-leave-panel { transition: transform var(--transition-fast) var(--ease-premium); }

/* Widths */
.width--sm { max-width: 320px; }
.width--md { max-width: 480px; }
.width--lg { max-width: 640px; }
.width--xl { max-width: 800px; }
.width--2xl { max-width: 960px; }

/* Animation Utility States */
:global(.translateX-full) { transform: translateX(100%); }
:global(.translateX-0) { transform: translateX(0); }
</style>
