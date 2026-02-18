<script setup lang="ts">
import { computed } from 'vue'
import MachineStatusIndicator from './MachineStatusIndicator.vue'
import MachineHealthGauge from './MachineHealthGauge.vue'
import Card from '@/components/common/Card.vue'

interface Machine {
  id: string
  name: string
  status: 'healthy' | 'warning' | 'critical' | 'unknown'
  healthScore: number
  rul: number
  temperature: number
  vibration: number
  lastUpdate: string
  location?: string
  type?: string
}

interface Props {
  machine: Machine
  compact?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  compact: false
})

const emit = defineEmits<{
  click: [machine: Machine]
}>()

const statusColors = {
  healthy: 'border-green-200 bg-green-50',
  warning: 'border-yellow-200 bg-yellow-50',
  critical: 'border-red-200 bg-red-50',
  unknown: 'border-gray-200 bg-gray-50'
}

const handleClick = () => {
  emit('click', props.machine)
}
</script>

<template>
  <Card :class="['cursor-pointer transition-all hover:shadow-lg', statusColors[machine.status]]" :padding="compact ? 'sm' : 'md'" hoverable @click="handleClick">
    <template #header>
      <div class="flex items-center gap-2">
        <MachineStatusIndicator :status="machine.status" :pulse="machine.status === 'critical'" />
        <span class="font-semibold text-gray-900">{{ machine.name }}</span>
      </div>
    </template>
    <template #body>
      <div :class="['flex gap-4', compact ? 'items-center' : 'items-start']">
        <MachineHealthGauge :value="machine.healthScore" :size="compact ? 'sm' : 'md'" />
        <div class="flex-1 grid grid-cols-3 gap-2 text-sm">
          <div>
            <p class="text-gray-500 text-xs">RUL</p>
            <p class="font-semibold text-gray-900">{{ machine.rul }}h</p>
          </div>
          <div>
            <p class="text-gray-500 text-xs">Temp</p>
            <p class="font-semibold text-gray-900">{{ machine.temperature }}°C</p>
          </div>
          <div>
            <p class="text-gray-500 text-xs">Vib</p>
            <p class="font-semibold text-gray-900">{{ machine.vibration }}mm/s</p>
          </div>
        </div>
      </div>
      <div v-if="!compact && (machine.location || machine.type)" class="mt-3 pt-3 border-t border-gray-200">
        <div class="flex gap-4 text-xs text-gray-500">
          <span v-if="machine.type">{{ machine.type }}</span>
          <span v-if="machine.location">{{ machine.location }}</span>
        </div>
      </div>
    </template>
    <template #footer>
      <div class="flex items-center justify-between text-xs text-gray-500">
        <span>Updated: {{ new Date(machine.lastUpdate).toLocaleString() }}</span>
      </div>
    </template>
  </Card>
</template>
