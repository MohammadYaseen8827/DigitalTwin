<script setup lang="ts">
import { computed } from 'vue'

type Status = 'healthy' | 'warning' | 'critical' | 'unknown' | 'info' | 'success'

interface Props {
  status: Status
  size?: 'sm' | 'md' | 'lg'
  pulse?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  pulse: false
})

const badgeClasses = computed(() => {
  const sizes: Record<string, string> = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-1 text-xs',
    lg: 'px-3 py-1.5 text-sm'
  }
  
  return `${sizes[props.size]} inline-flex items-center font-medium rounded-full`
})

const statusColors = computed(() => {
  const colors: Record<Status, { bg: string; text: string }> = {
    healthy: { bg: 'bg-green-100', text: 'text-green-800' },
    warning: { bg: 'bg-yellow-100', text: 'text-yellow-800' },
    critical: { bg: 'bg-red-100', text: 'text-red-800' },
    unknown: { bg: 'bg-gray-100', text: 'text-gray-800' },
    info: { bg: 'bg-blue-100', text: 'text-blue-800' },
    success: { bg: 'bg-emerald-100', text: 'text-emerald-800' }
  }
  
  return colors[props.status] || colors.unknown
})

const dotColors = computed(() => {
  const dots: Record<Status, string> = {
    healthy: 'bg-green-500',
    warning: 'bg-yellow-500',
    critical: 'bg-red-500',
    unknown: 'bg-gray-500',
    info: 'bg-blue-500',
    success: 'bg-emerald-500'
  }
  
  return dots[props.status] || dots.unknown
})

const statusLabels: Record<Status, string> = {
  healthy: 'Healthy',
  warning: 'Warning',
  critical: 'Critical',
  unknown: 'Unknown',
  info: 'Info',
  success: 'Success'
}
</script>

<template>
  <span :class="[badgeClasses, statusColors.bg, statusColors.text]" class="badge-component">
    <span
      v-if="pulse"
      :class="[dotColors, 'w-2 h-2 rounded-full mr-1.5 animate-pulse']"
      :style="{ animationDuration: '2s' }"
    />
    <span v-else :class="[dotColors, 'w-2 h-2 rounded-full mr-1.5']" />
    {{ statusLabels[status] }}
    <slot />
  </span>
</template>

<style scoped>
.badge-component {
  @apply transition-all duration-200;
}
</style>
