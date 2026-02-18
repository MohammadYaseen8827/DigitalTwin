<template>
  <div class="floor-plan-heatmap space-y-8">
    <header class="flex flex-col md:flex-row justify-between items-start md:items-end gap-6">
      <div>
        <div class="flex items-center gap-2 mb-2">
          <div class="w-8 h-1 bg-cyan-500 rounded-full"></div>
          <p class="text-[10px] uppercase font-black tracking-[0.3em] text-cyan-400">Spatial Monitoring</p>
        </div>
        <h2 class="text-3xl font-black tracking-tighter text-primary">Factory Floor Intelligence</h2>
      </div>
      
      <div class="flex flex-wrap items-center gap-3 bg-white/5 p-2 rounded-2xl border border-white/10">
        <select v-model="selectedMetric" class="bg-transparent border-none text-[10px] font-black uppercase tracking-widest px-4 py-2 cursor-pointer outline-none hover:bg-white/5 rounded-xl transition-colors appearance-none pr-8 relative" style="background-image: url('data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 fill=%22none%22 viewBox=%220 0 24 24%22 stroke=%22%236b7280%22%3E%3Cpath stroke-linecap=%22round%22 stroke-linejoin=%22round%22 stroke-width=%222%22 d=%22M19 9l-7 7-7-7%22%3E%3C/path%3E%3C/svg%3E'); background-repeat: no-repeat; background-position: right 0.75rem center; background-size: 0.75rem">
          <option value="status">Status</option>
          <option value="efficiency">Efficiency</option>
          <option value="temperature">Temperature</option>
          <option value="power">Power Consumption</option>
        </select>
        
        <div class="flex bg-white/5 p-1 rounded-xl">
          <button 
            v-for="mode in ['heatmap', 'grid']" 
            :key="mode"
            @click="viewMode = mode"
            class="px-4 py-1.5 text-[9px] font-black uppercase tracking-widest rounded-lg transition-all"
            :class="viewMode === mode ? 'bg-cyan-500 text-white shadow-lg' : 'text-secondary hover:text-primary'"
          >
            {{ mode }}
          </button>
        </div>
      </div>
    </header>

    <BaseCard :loading="loading" class="overflow-hidden border-white/5">
      <div class="p-8">
        <BaseTabs v-model="activeTab" :tabs="productionLineTabs" />
        
        <div class="mt-8 min-h-[500px] relative rounded-[32px] bg-slate-950/20 border border-white/5 overflow-hidden">
          <!-- Tech Grid Background -->
          <div class="absolute inset-0 opacity-[0.03]" style="background-image: radial-gradient(circle at 1px 1px, white 1px, transparent 0); background-size: 24px 24px;"></div>
          
          <div 
            v-for="line in productionLines" 
            :key="line.id"
            v-show="activeTab === line.id"
            class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-500"
          >
            <div class="flex justify-between items-center mb-10">
              <div>
                 <h3 class="text-xl font-black text-primary underline decoration-cyan-500/30 underline-offset-8">{{ line.name }}</h3>
              </div>
              <div class="flex gap-8">
                <div class="text-right">
                  <div class="text-[9px] font-black uppercase tracking-widest text-secondary">Operational Units</div>
                  <div class="text-xl font-black text-primary">{{ getMachinesByLine(line.id).length }}</div>
                </div>
                <div class="text-right">
                  <div class="text-[9px] font-black uppercase tracking-widest text-secondary">Line Efficiency</div>
                  <div class="text-xl font-black text-cyan-400">{{ calculateLineEfficiency(line.id) }}%</div>
                </div>
              </div>
            </div>

            <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-6">
              <div
                v-for="machine in getMachinesByLine(line.id)"
                :key="machine.id"
                class="machine-tile group relative p-6 rounded-3xl bg-white/[0.02] border border-white/5 hover:border-cyan-500/30 transition-all cursor-pointer overflow-hidden"
                @click="selectMachine(machine)"
              >
                <div class="absolute inset-0 bg-gradient-to-br from-cyan-500/[0.02] to-transparent pointer-events-none"></div>
                
                <div class="relative z-10">
                  <div class="flex justify-between items-start mb-4">
                    <div class="w-10 h-10 rounded-xl flex items-center justify-center text-xs font-black shadow-lg shadow-black/20"
                         :class="getStatusBg(machine.status)">
                      {{ machine.name.split(' ')[1] || machine.name[0] }}
                    </div>
                    <div class="flex flex-col items-end">
                      <div class="w-2 h-2 rounded-full animate-pulse" :class="getStatusDot(machine.status)"></div>
                      <span class="text-[8px] font-black uppercase tracking-tighter text-secondary mt-1">{{ machine.status }}</span>
                    </div>
                  </div>
                  
                  <div class="mb-4">
                    <div class="text-[10px] font-black uppercase tracking-widest text-secondary truncate">{{ machine.name }}</div>
                    <div class="text-[9px] font-bold text-secondary-alt">{{ machine.type }}</div>
                  </div>

                  <div class="grid grid-cols-2 gap-2">
                    <div class="p-2 bg-white/5 rounded-xl border border-white/5">
                      <div class="text-[8px] font-black uppercase text-secondary-alt mb-1">Eff.</div>
                      <div class="text-xs font-black text-primary">{{ getMachineMetric(machine, 'efficiency') }}%</div>
                    </div>
                    <div class="p-2 bg-white/5 rounded-xl border border-white/5">
                      <div class="text-[8px] font-black uppercase text-secondary-alt mb-1">Temp.</div>
                      <div class="text-xs font-black text-primary">{{ getMachineMetric(machine, 'temperature') }}°C</div>
                    </div>
                  </div>
                </div>
                
                <!-- Selection Highlight -->
                <div class="absolute bottom-0 left-0 h-1 bg-cyan-500 transition-all duration-300"
                     :class="selectedMachineId === machine.id ? 'w-full' : 'w-0'"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </BaseCard>

    <!-- Detailed Inspection Panel -->
    <transition name="slide-up">
      <BaseCard v-if="selectedMachine" class="p-8 border-cyan-500/20 bg-cyan-500/[0.02]">
        <div class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-8">
          <div class="flex items-center gap-6">
            <div class="w-16 h-16 rounded-[24px] bg-white/5 flex items-center justify-center border border-white/10 shadow-2xl">
               <Boxes class="w-8 h-8 text-cyan-400" />
            </div>
            <div>
              <h3 class="text-2xl font-black text-primary">{{ selectedMachine.name }}</h3>
              <div class="flex items-center gap-2 mt-1">
                <BaseBadge :variant="normalizeStatus(selectedMachine.status)" class="uppercase text-[9px] font-black">{{ selectedMachine.status }}</BaseBadge>
                <div class="text-[10px] font-bold text-secondary-alt uppercase tracking-widest">Serial: {{ selectedMachine.id.slice(0, 8) }}</div>
              </div>
            </div>
          </div>

          <div class="grid grid-cols-2 sm:grid-cols-4 gap-8 flex-1 lg:max-w-3xl">
            <div v-for="metric in selectedMachineMetrics" :key="metric.label" class="space-y-1">
               <div class="text-[9px] font-black uppercase text-secondary tracking-widest">{{ metric.label }}</div>
               <div class="text-xl font-black text-primary">{{ metric.value }}<span class="text-xs text-secondary-alt ml-1">{{ metric.suffix }}</span></div>
               <div class="h-1 w-full bg-white/5 rounded-full overflow-hidden">
                  <div class="h-full bg-cyan-500" :style="{ width: `${metric.percent}%` }"></div>
               </div>
            </div>
          </div>
          
          <BaseButton variant="primary" class="h-12 px-8 font-black uppercase tracking-widest text-[10px] shadow-lg shadow-cyan-500/20">
            Open Telemetry
          </BaseButton>
        </div>
      </BaseCard>
    </transition>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '../base/BaseCard.vue'
import BaseButton from '../base/BaseButton.vue'
import BaseTabs from '../base/BaseTabs.vue'
import BaseBadge from '../base/BaseBadge.vue'
import { Boxes, Info as InfoIcon } from 'lucide-vue-next'

import { fetchMachines } from '@/services/machines.service'
import { fetchProductionLines } from '@/services/productionLines.service'
import type { MachineDto, ProductionLineDto } from '@/api/types'
import { normalizeStatus } from '@/utils/statusFormatter'

const machines = ref<MachineDto[]>([])
const productionLines = ref<ProductionLineDto[]>([])
const selectedMachineId = ref('')
const selectedMetric = ref('status')
const viewMode = ref('heatmap')
const loading = ref(false)
const activeTab = ref('')
const toast = useToast()

const productionLineTabs = computed(() => productionLines.value.map(line => ({ id: line.id, label: line.name })))

const getMachinesByLine = (lineId: string) => {
  const line = productionLines.value.find(l => l.id === lineId)
  return line ? machines.value.filter(m => line.machineIds.includes(m.id)) : []
}

const calculateLineEfficiency = (lineId: string) => {
  const lineMachines = getMachinesByLine(lineId)
  if (!lineMachines.length) return 0
  return Math.round(lineMachines.reduce((s, m) => s + getMachineMetric(m, 'efficiency'), 0) / lineMachines.length)
}

const getMachineMetric = (machine: MachineDto, metric: string): number => {
  if (!machine.telemetry) return 0
  switch (metric) {
    case 'efficiency': return machine.telemetry.efficiency || 75
    case 'temperature': return machine.telemetry.temperature || 180
    case 'power': return machine.telemetry.powerConsumption || 12
    default: return 0
  }
}

const getStatusBg = (status: string) => {
  const s = normalizeStatus(status)
  if (s === 'operational') return 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/20'
  if (s === 'warning') return 'bg-amber-500/20 text-amber-500 border border-amber-500/20'
  if (s === 'critical') return 'bg-red-500/20 text-red-400 border border-red-500/20'
  return 'bg-white/5 text-secondary border border-white/10'
}

const getStatusDot = (status: string) => {
  const s = normalizeStatus(status)
  if (s === 'operational') return 'bg-emerald-500'
  if (s === 'warning') return 'bg-amber-500'
  if (s === 'critical') return 'bg-red-500'
  return 'bg-secondary'
}

const selectedMachine = computed(() => machines.value.find(m => m.id === selectedMachineId.value))

const selectedMachineMetrics = computed(() => {
  if (!selectedMachine.value) return []
  return [
    { label: 'Efficiency', value: getMachineMetric(selectedMachine.value, 'efficiency'), suffix: '%', percent: getMachineMetric(selectedMachine.value, 'efficiency') },
    { label: 'Temp', value: getMachineMetric(selectedMachine.value, 'temperature'), suffix: '°C', percent: (getMachineMetric(selectedMachine.value, 'temperature') / 300) * 100 },
    { label: 'Power', value: getMachineMetric(selectedMachine.value, 'power'), suffix: 'kW', percent: (getMachineMetric(selectedMachine.value, 'power') / 50) * 100 },
    { label: 'Utilization', value: 88, suffix: '%', percent: 88 }
  ]
})

const selectMachine = (machine: MachineDto) => {
  selectedMachineId.value = machine.id
}

const refreshData = async () => {
  try {
    loading.value = true
    const [m, l] = await Promise.all([fetchMachines(), fetchProductionLines()])
    machines.value = m
    productionLines.value = l
    if (l.length && !activeTab.value) activeTab.value = l[0].id
  } catch (err) {
    toast.error('Factory layout sync failed')
  } finally {
    loading.value = false
  }
}

let ticker: number
onMounted(() => {
  refreshData()
  ticker = window.setInterval(refreshData, 30000)
})
onBeforeUnmount(() => window.clearInterval(ticker))
</script>

<style scoped>
.machine-tile {
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}
.machine-tile:hover {
  transform: translateY(-4px);
  background: rgba(255, 255, 255, 0.04);
}

.slide-up-enter-active, .slide-up-leave-active {
  transition: all 0.5s ease;
}
.slide-up-enter-from, .slide-up-leave-to {
  opacity: 0;
  transform: translateY(40px);
}
</style>

