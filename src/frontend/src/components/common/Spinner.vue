<script setup lang="ts">
import { computed } from 'vue'

type Size = 'sm' | 'md' | 'lg' | 'xl'

interface Props {
  size?: Size
  label?: string
  color?: 'primary' | 'white' | 'gray'
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  label: 'Loading...',
  color: 'primary'
})

const spinnerSizes: Record<Size, string> = {
  sm: 'h-4 w-4',
  md: 'h-8 w-8',
  lg: 'h-12 w-12',
  xl: 'h-16 w-16'
}

const spinnerColors: Record<string, string> = {
  primary: 'border-primary-600',
  white: 'border-white',
  gray: 'border-gray-600'
}

const spinnerClasses = computed(() => {
  return `${spinnerSizes[props.size]} ${spinnerColors[props.color]}`
})
</script>

<template>
  <div class="inline-flex items-center justify-center" role="status" :aria-label="label">
    <svg :class="[spinnerClasses, 'animate-spin rounded-full border-2 border-t-transparent']" viewBox="0 0 24 24">
      <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
      <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
    </svg>
    <span class="sr-only">{{ label }}</span>
  </div>
</template>
