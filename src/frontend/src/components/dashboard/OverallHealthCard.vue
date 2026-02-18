<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  healthScore: number
  totalMachines: number
  healthyMachines: number
  warningMachines: number
  criticalMachines: number
}

const props = defineProps<Props>()

const healthColor = computed(() => {
  if (props.healthScore >= 80) return 'text-green-500'
  if (props.healthScore >= 60) return 'text-yellow-500'
  return 'text-red-500'
})

const circumference = 2 * Math.PI * 40
const strokeDashoffset = computed(() => {
  return circumference - (props.healthScore / 100) * circumference
})
</script>

<template>
  <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
    <h2 class="text-lg font-semibold text-gray-900 mb-4">Platform Health</h2>
    <div class="flex items-center justify-between">
      <div class="relative w-32 h-32">
        <svg class="w-full h-full transform -rotate-90" viewBox="0 0 100 100">
          <circle cx="50" cy="50" r="40" fill="none" stroke="#e5e7eb" stroke-width="8" />
          <circle cx="50" cy="50" r="40" fill="none" :stroke="healthScore >= 80 ? '#10B981' : healthScore >= 60 ? '#F59E0B' : '#EF4444'" stroke-width="8" stroke-linecap="round" :stroke-dasharray="circumference" :stroke-dashoffset="strokeDashoffset" class="transition-all duration-500" />
        </svg>
        <div class="absolute inset-0 flex items-center justify-center">
          <span :class="['text-2xl font-bold', healthColor]">{{ healthScore }}%</span>
        </div>
      </div>
      <div class="flex-1 ml-6 space-y-3">
        <div class="flex items-center justify-between">
          <span class="text-sm text-gray-600">Total Machines</span>
          <span class="font-semibold text-gray-900">{{ totalMachines }}</span>
        </div>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-green-500" />
            <span class="text-sm text-gray-600">Healthy</span>
          </div>
          <span class="font-semibold text-gray-900">{{ healthyMachines }}</span>
        </div>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-yellow-500" />
            <span class="text-sm text-gray-600">Warning</span>
          </div>
          <span class="font-semibold text-gray-900">{{ warningMachines }}</span>
        </div>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-red-500" />
            <span class="text-sm text-gray-600">Critical</span>
          </div>
          <span class="font-semibold text-gray-900">{{ criticalMachines }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
