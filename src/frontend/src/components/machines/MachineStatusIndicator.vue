<script setup lang="ts">
import { computed } from 'vue'

type Status = 'healthy' | 'warning' | 'critical' | 'unknown'

interface Props {
  status: Status
  size?: 'sm' | 'md' | 'lg'
  pulse?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  pulse: true
})

const sizes = {
  sm: 'w-2 h-2',
  md: 'w-3 h-3',
  lg: 'w-4 h-4'
}

const statusColors: Record<Status, string> = {
  healthy: 'bg-green-500',
  warning: 'bg-yellow-500',
  critical: 'bg-red-500',
  unknown: 'bg-gray-500'
}

const statusLabels: Record<Status, string> = {
  healthy: 'Healthy',
  warning: 'Warning',
  critical: 'Critical',
  unknown: 'Unknown'
}
</script>

<template>
  <span class="inline-flex items-center gap-1.5" :title="statusLabels[status]">
    <span
      :class="[sizes[size], statusColors[status], 'rounded-full', pulse ? 'animate-pulse' : '']"
      :style="{ animationDuration: pulse ? '2s' : undefined }"
      aria-hidden="true"
    />
    <span class="sr-only">{{ statusLabels[status] }}</span>
  </span>
</template>
