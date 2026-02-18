<template>
  <BaseCard class="machine-card group overflow-hidden" variant="glass" hoverable>
    <div class="card-inner p-6">
      <div class="flex justify-between items-start mb-6">
        <div class="flex gap-4 items-center">
          <div class="machine-icon-wrapper p-3 rounded-2xl bg-white/5 border border-white/10 group-hover:border-indigo-500/50 transition-all duration-500">
            <Cpu class="w-6 h-6 text-indigo-400 group-hover:scale-110 transition-transform" />
          </div>
          <div>
            <h3 class="text-lg font-black tracking-tight text-primary leading-tight">{{ machine.name }}</h3>
            <p class="text-[10px] uppercase font-bold tracking-[0.2em] text-secondary mt-0.5">{{ machine.type }}</p>
          </div>
        </div>
        <BaseBadge :variant="statusBadgeVariant" class="uppercase font-bold tracking-widest text-[10px]">
          {{ statusLabel }}
        </BaseBadge>
      </div>

      <!-- Health Visualization -->
      <div class="health-section flex items-center gap-6 mb-8 mt-2">
        <div class="relative w-20 h-20 shrink-0">
          <svg class="w-full h-full -rotate-90">
            <circle cx="40" cy="40" r="34" fill="none" stroke="rgba(255,255,255,0.05)" stroke-width="6" />
            <circle cx="40" cy="40" r="34" fill="none" 
                    :stroke="healthColor" 
                    stroke-width="6" 
                    stroke-dasharray="213.6" 
                    :stroke-dashoffset="213.6 * (1 - healthPercentage / 100)"
                    stroke-linecap="round"
                    class="transition-all duration-[1500ms] ease-out" />
          </svg>
          <div class="absolute inset-0 flex flex-col items-center justify-center">
            <span class="text-xl font-black leading-none" :style="{ color: healthColor }">{{ healthPercentage }}%</span>
            <span class="text-[8px] font-bold text-secondary uppercase tracking-tighter">Health</span>
          </div>
        </div>

        <div class="flex-1 space-y-4">
          <div class="prediction-snip">
            <div class="text-[10px] uppercase font-black text-secondary tracking-widest mb-1 flex justify-between">
              <span>Predicted RUL</span>
              <span class="text-indigo-400 group-hover:translate-x-1 transition-transform">Details →</span>
            </div>
            <div class="flex items-baseline gap-1">
              <span class="text-2xl font-black text-primary">{{ machine.remainingUsefulLifeDays ?? '--' }}</span>
              <span class="text-xs font-bold text-secondary uppercase italic">Days</span>
            </div>
          </div>
          <div class="h-1 w-full bg-white/5 rounded-full overflow-hidden">
            <div class="h-full bg-gradient-to-r from-indigo-500/50 to-indigo-500" :style="{ width: Math.min(100, (machine.remainingUsefulLifeDays ?? 0) / 365 * 100) + '%' }"></div>
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-4 mb-8">
        <div class="p-3 rounded-xl bg-white/5 border border-white/5 group-hover:bg-white/[0.08] transition-colors">
          <div class="text-[9px] uppercase font-bold text-secondary tracking-wider mb-1">Last Event</div>
          <div class="text-xs font-bold text-primary truncate">{{ machine.lastMaintenance ? formatDateShort(machine.lastMaintenance) : 'No Records' }}</div>
        </div>
        <div class="p-3 rounded-xl bg-white/5 border border-white/5 group-hover:bg-white/[0.08] transition-colors">
          <div class="text-[9px] uppercase font-bold text-secondary tracking-wider mb-1">Line Location</div>
          <div class="text-xs font-bold text-primary truncate">{{ machine.location }}</div>
        </div>
      </div>

      <div class="flex gap-2">
        <BaseButton variant="primary" size="sm" class="flex-1 font-bold text-[10px] uppercase tracking-widest h-10 shadow-lg shadow-indigo-500/10" @click="$emit('view-details', machine.id)">
          View Analytics
        </BaseButton>
        <BaseButton variant="outline" size="sm" class="h-10 w-10 p-0 rounded-xl" @click="showSchedulerForm = !showSchedulerForm">
          <Settings class="w-4 h-4" />
        </BaseButton>
      </div>

      <!-- Expandable Scheduler -->
      <transition 
        enter-active-class="transition-[max-height,opacity,margin] duration-500 ease-out"
        leave-active-class="transition-[max-height,opacity,margin] duration-300 ease-in"
        enter-from-class="max-h-0 opacity-0 mt-0"
        leave-to-class="max-h-0 opacity-0 mt-0"
        enter-to-class="max-h-[300px] opacity-100 mt-6"
      >
        <div v-if="showSchedulerForm" class="scheduler-drawer space-y-4 pt-6 border-t border-white/10 overflow-hidden">
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1.5">
              <label class="text-[10px] uppercase font-black text-secondary tracking-widest pl-1">Interval (s)</label>
              <input v-model="scheduleInterval" type="number" class="w-full bg-white/5 border border-white/10 rounded-xl px-3 py-2 text-sm font-bold focus:ring-1 focus:ring-indigo-500 transition-all outline-none" />
            </div>
            <div class="space-y-1.5">
              <label class="text-[10px] uppercase font-black text-secondary tracking-widest pl-1">Duration (m)</label>
              <input v-model="scheduleDuration" type="number" class="w-full bg-white/5 border border-white/10 rounded-xl px-3 py-2 text-sm font-bold focus:ring-1 focus:ring-indigo-500 transition-all outline-none" />
            </div>
          </div>
          <BaseButton variant="primary" size="sm" class="w-full h-10 font-black uppercase tracking-widest text-[10px]" @click="saveSchedule">
            Commit Simulation
          </BaseButton>
        </div>
      </transition>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { format } from 'date-fns'
import axiosClient from '@/api/axiosClient'
import type { MachineDto, EquipmentStatus } from '@/api/types'

import BaseButton from './base/BaseButton.vue'
import BaseCard from './base/BaseCard.vue'
import BaseBadge from './base/BaseBadge.vue'
import { Cpu, Settings, Thermometer, Zap, AlertTriangle } from 'lucide-vue-next'

const props = defineProps<{
  machine: MachineDto
}>()

const emit = defineEmits<{
  'view-details': [string]
  'simulation-updated': []
}>()

const toast = useToast()
const showSchedulerForm = ref(false)
const scheduleInterval = ref(60)
const scheduleDuration = ref(120)

const healthPercentage = computed(() => {
  // Mock health calculation based on status and RUL
  if (normalizeStatus(props.machine.status) === 'critical') return 18
  if (normalizeStatus(props.machine.status) === 'warning') return 45
  if (!props.machine.remainingUsefulLifeDays) return 92
  
  const rul = props.machine.remainingUsefulLifeDays
  if (rul < 30) return 25
  if (rul < 90) return 60
  return 95
})

const healthColor = computed(() => {
  const h = healthPercentage.value
  if (h < 30) return '#ef4444' // Red
  if (h < 70) return '#f59e0b' // Amber
  return '#10b981' // Emerald
})

const statusLabel = computed(() => formatStatus(props.machine.status))
const statusBadgeVariant = computed(() => {
  const s = normalizeStatus(props.machine.status)
  if (s === 'operational') return 'success'
  if (s === 'warning') return 'warning'
  if (s === 'critical') return 'danger'
  return 'default'
})

const formatDateShort = (value: string) => {
  try {
    return format(new Date(value), 'MMM dd, HH:mm')
  } catch {
    return '—'
  }
}

function formatStatus(status: EquipmentStatus | undefined): string {
  if (!status && status !== 0) return 'Unknown'
  if (typeof status === 'number') return `Status ${status}`
  return status.toUpperCase()
}

function normalizeStatus(status: EquipmentStatus | undefined): string {
  if (!status && status !== 0) return 'unknown'
  if (typeof status === 'number') return 'unknown'
  return status.toLowerCase()
}

const saveSchedule = async () => {
  try {
    const startTime = new Date().toISOString()
    const endTime = new Date(Date.now() + scheduleDuration.value * 60000).toISOString()
    await axiosClient.post(`/SimulationScheduler/schedule/${props.machine.id}`, {
      machineName: props.machine.name,
      intervalSeconds: scheduleInterval.value,
      startTime,
      endTime,
      isActive: true,
      parameters: { degradationModel: 'wiener' }
    })
    showSchedulerForm.value = false
    toast.success('Simulation schedule updated')
    emit('simulation-updated')
  } catch {
    toast.error('Scheduling failed')
  }
}
</script>

<style scoped>
.machine-card {
  border: 1px solid rgba(255, 255, 255, 0.05);
  transition: all 0.5s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.machine-card:hover {
  border-color: rgba(99, 102, 241, 0.3);
  transform: translateY(-8px);
  box-shadow: 0 20px 40px -12px rgba(0, 0, 0, 0.5);
}

.machine-icon-wrapper {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

@keyframes dash {
  from { stroke-dashoffset: 213.6; }
}

svg circle:last-child {
  animation: dash 1.5s ease-out forwards;
}

.StatCard:hover {
  transform: translateY(-4px);
}
</style>
