<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'

interface Props {
  show: boolean
  title?: string
  size?: 'sm' | 'md' | 'lg' | 'xl'
  closeable?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  closeable: true
})

const emit = defineEmits<{
  close: []
}>()

const sizes = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl'
}

const close = () => {
  if (props.closeable) {
    emit('close')
  }
}

const handleEscape = (e: KeyboardEvent) => {
  if (e.key === 'Escape' && props.closeable) {
    close()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleEscape)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscape)
})
</script>

<template>
  <Teleport to="body">
    <Transition name="modal">
      <div v-if="show" class="fixed inset-0 z-50 overflow-y-auto" role="dialog" aria-modal="true" aria-labelledby="modal-title">
        <div class="fixed inset-0 bg-gray-900/50 transition-opacity" @click="close" />
        <div class="flex min-h-full items-center justify-center p-4">
          <div :class="[sizes[size], 'relative w-full bg-white rounded-xl shadow-xl transform transition-all']">
            <div v-if="title || $slots.header" class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
              <slot name="header">
                <h2 id="modal-title" class="text-lg font-semibold text-gray-900">{{ title }}</h2>
              </slot>
              <button v-if="closeable" type="button" class="text-gray-400 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-primary-500 rounded-lg p-1" @click="close">
                <svg class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            <div class="px-6 py-4">
              <slot />
            </div>
            <div v-if="$slots.footer" class="px-6 py-4 border-t border-gray-200 bg-gray-50 rounded-b-xl">
              <slot name="footer" />
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: opacity 0.3s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-active .relative, .modal-leave-active .relative { transition: transform 0.3s ease; }
.modal-enter-from .relative, .modal-leave-to .relative { transform: scale(0.95); }
</style>
