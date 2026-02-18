<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  value: number
  size?: 'sm' | 'md' | 'lg'
  showLabel?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  showLabel: true
})

const sizes = {
  sm: { container: 'w-16 h-16', stroke: 4, font: 'text-sm' },
  md: { container: 'w-24 h-24', stroke: 6, font: 'text-lg' },
  lg: { container: 'w-32 h-32', stroke: 8, font: 'text-2xl' }
}

const radius = 40
const circumference = 2 * Math.PI * radius
const strokeDashoffset = computed(() => {
  return circumference - (props.value / 100) * circumference
})

const healthColor = computed(() => {
  if (props.value >= 80) return '#10B981'
  if (props.value >= 60) return '#F59E0B'
  return '#EF4444'
})
</script>

<template>
  <div :class="[sizes[size].container, 'relative inline-flex items-center justify-center']">
    <svg class="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
      <circle cx="50" cy="50" :r="radius" fill="none" stroke="#E5E7EB" :stroke-width="sizes[size].stroke" />
      <circle cx="50" cy="50" :r="radius" fill="none" :stroke="healthColor" :stroke-width="sizes[size].stroke" stroke-linecap="round" :stroke-dasharray="circumference" :stroke-dashoffset="strokeDashoffset" class="transition-all duration-500" />
    </svg>
    <div class="absolute inset-0 flex items-center justify-center">
      <span :class="['font-bold', healthColor, sizes[size].font]">{{ showLabel ? `${value}%` : value }}</span>
    </div>
  </div>
</template>
