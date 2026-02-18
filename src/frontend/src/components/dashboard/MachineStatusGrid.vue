<script setup lang="ts">
import { computed } from 'vue'

export interface Machine {
  id: string
  name: string
  status: 'healthy' | 'warning' | 'critical' | 'unknown'
  healthScore: number
  rul: number
  temperature: number
  vibration: number
  lastUpdate: string
}

interface Props {
  machines: Machine[]
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  machineClick: [machine: Machine]
}>()

const statusColors = {
  healthy: { bg: 'bg-green-50 border-green-200', badge: 'healthy', dot: 'bg-green-500' },
  warning: { bg: 'bg-yellow-50 border-yellow-200', badge: 'warning', dot: 'bg-yellow-500' },
  critical: { bg: 'bg-red-50 border-red-200', badge: 'critical', dot: 'bg-red-500' },
  unknown: { bg: 'bg-gray-50 border-gray-200', badge: 'unknown', dot: 'bg-gray-500' }
}

const healthScoreColor = (score: number) => {
  if (score >= 80) return 'text-green-600'
  if (score >= 60) return 'text-yellow-600'
  return 'text-red-600'
}
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-lg font-semibold text-gray-900">Machine Status</h2>
      <router-link to="/machines" class="text-sm text-primary-600 hover:text-primary-700 font-medium">View all</router-link>
    </div>

    <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div v-for="i in 6" :key="i" class="animate-pulse">
        <div class="bg-gray-100 rounded-lg p-4 h-32" />
      </div>
    </div>

    <div v-else-if="machines.length === 0" class="text-center py-8 text-gray-500">
      No machines found
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
      <div
        v-for="machine in machines"
        :key="machine.id"
        :class="['rounded-lg p-4 border cursor-pointer transition-all hover:shadow-md', statusColors[machine.status].bg]"
        @click="emit('machineClick', machine)"
      >
        <div class="flex items-start justify-between mb-3">
          <div class="flex items-center gap-2">
            <span :class="['w-2 h-2 rounded-full', statusColors[machine.status].dot]" />
            <span class="font-semibold text-gray-900">{{ machine.name }}</span>
          </div>
        </div>
        
        <div class="grid grid-cols-3 gap-2 text-sm">
          <div>
            <p class="text-gray-500 text-xs">Health</p>
            <p :class="['font-semibold', healthScoreColor(machine.healthScore)]">{{ machine.healthScore }}%</p>
          </div>
          <div>
            <p class="text-gray-500 text-xs">RUL</p>
            <p class="font-semibold text-gray-900">{{ machine.rul }}h</p>
          </div>
          <div>
            <p class="text-gray-500 text-xs">Temp</p>
            <p class="font-semibold text-gray-900">{{ machine.temperature }}°C</p>
          </div>
        </div>
        
        <div class="mt-3 pt-3 border-t border-gray-200">
          <div class="flex items-center justify-between text-xs text-gray-500">
            <span>Vib: {{ machine.vibration }}mm/s</span>
            <span>{{ new Date(machine.lastUpdate).toLocaleTimeString() }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
