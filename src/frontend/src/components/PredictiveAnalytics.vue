<template>
  <div class="predictive-analytics min-h-screen pb-20 space-y-10">
    <!-- Hero Header with Unified Flow -->
    <header class="pa-header relative overflow-hidden p-10 px-12 border-b border-white/5 bg-gradient-to-b from-white/[0.03] to-transparent">
      <div class="absolute top-0 right-0 w-1/2 h-full bg-indigo-500/5 blur-[120px] rounded-full -translate-y-1/2 translate-x-1/4 pointer-events-none"></div>
      
      <div class="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-end gap-8 max-w-7xl mx-auto">
        <div>
          <div class="flex items-center gap-2 mb-3">
            <div class="w-8 h-1 bg-indigo-500 rounded-full"></div>
            <p class="text-[10px] uppercase font-black tracking-[0.3em] text-indigo-400">Advanced ML Engine</p>
          </div>
          <h1 class="text-4xl font-black tracking-tighter text-primary mb-2">Predictive Maintenance</h1>
          <p class="text-[12px] font-black text-secondary-alt uppercase tracking-[0.2em]">Enterprise Analytics & Health Forecasting</p>
        </div>

        <div class="flex items-center gap-6">
          <div class="text-right hidden sm:block">
            <div class="text-[9px] uppercase font-black text-secondary tracking-widest leading-none mb-2">Last System Sync</div>
            <div class="text-[12px] font-mono font-black text-indigo-400/80">{{ lastUpdatedText }}</div>
          </div>
          <div class="flex gap-4">
            <BaseButton variant="outline" class="h-12 px-6 rounded-2xl border-white/10 hover:bg-white/5 transition-all text-[10px] font-black uppercase tracking-widest" @click="fetchAllPredictions">
              <RefreshCw class="w-4 h-4 mr-2" /> Sync Engine
            </BaseButton>
            <BaseButton variant="primary" class="h-12 px-8 rounded-2xl shadow-lg shadow-indigo-500/20 font-black uppercase tracking-widest text-[10px]" @click="retrainModel">
              <Database class="w-4 h-4 mr-2" /> Global Retrain
            </BaseButton>
          </div>
        </div>
      </div>
    </header>

    <div class="max-w-7xl mx-auto px-10 space-y-10">
      <!-- High-Level Stats -->
      <div v-if="loading && machines.length === 0" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div v-for="i in 4" :key="i" class="h-32 rounded-[32px] bg-white/[0.03] border border-white/10 animate-pulse"></div>
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard v-for="stat in mainStats" :key="stat.title" :label="stat.title" :variant="stat.variant" class="border-white/5 transition-transform hover:-translate-y-1 duration-300">
          <MetricValue :value="stat.value" :suffix="stat.suffix" />
          <template #icon>
             <component :is="stat.icon" class="w-5 h-5" :class="stat.iconColor" />
          </template>
        </StatCard>
      </div>

      <!-- Discovery & Filters bar -->
      <div class="flex flex-col xl:flex-row gap-8 bg-white/[0.03] p-3 rounded-[32px] border border-white/10 backdrop-blur-md">
        <div class="flex-1 relative group">
          <Search class="absolute left-6 top-1/2 -translate-y-1/2 w-5 h-5 text-secondary group-focus-within:text-indigo-400 transition-colors" />
          <input 
            v-model="searchQuery" 
            placeholder="Search equipment by serial, model or line..." 
            class="w-full bg-transparent h-16 pl-16 pr-8 text-[13px] font-black tracking-tight placeholder:text-secondary-alt focus:outline-none"
            @input="handleSearch"
          />
        </div>
        
        <div class="flex items-center gap-3 p-2 px-6 overflow-x-auto whitespace-nowrap border-l border-white/10">
          <div class="text-[9px] uppercase font-black text-secondary tracking-[0.2em] mr-4 select-none">Filters:</div>
          <button 
            v-for="status in statusFilters" 
            :key="status.value"
            @click="toggleStatusFilter(status.value)"
            class="px-5 py-2.5 text-[10px] font-black uppercase tracking-widest rounded-xl transition-all border border-transparent"
            :class="activeStatus === status.value ? 'bg-indigo-500 text-white shadow-xl shadow-indigo-500/20' : 'bg-white/5 text-secondary hover:bg-white/[0.1]'"
          >
            {{ status.label }}
          </button>
        </div>
      </div>

      <!-- Line Selection & Metrics -->
      <div class="space-y-8">
        <div class="flex items-center justify-between border-b border-white/5">
          <div class="flex gap-4">
            <button 
              v-for="line in productionLines" 
              :key="line.id"
              @click="activeLineId = line.id"
              class="group relative px-6 py-4 transition-all"
            >
              <span class="text-[11px] font-black uppercase tracking-[0.3em] transition-colors" :class="activeLineId === line.id ? 'text-primary' : 'text-secondary-alt group-hover:text-secondary'">
                {{ line.name }}
              </span>
              <div class="absolute bottom-0 left-0 h-1 bg-indigo-500 transition-all rounded-full" :class="activeLineId === line.id ? 'w-full opacity-100' : 'w-0 opacity-0'"></div>
            </button>
          </div>
          <BaseButton variant="ghost" class="text-[10px] font-black uppercase tracking-widest text-secondary hover:text-indigo-400 group">
            View All Lines <ChevronRight class="w-4 h-4 ml-1 group-hover:translate-x-1 transition-transform" />
          </BaseButton>
        </div>

        <transition name="fade" mode="out-in">
          <div v-if="lineMetrics" :key="activeLineId" class="grid grid-cols-2 lg:grid-cols-4 gap-6 animate-in fade-in duration-500">
            <div v-for="m in displayLineMetrics" :key="m.label" class="p-8 rounded-[24px] bg-white/[0.02] border border-white/5 hover:border-indigo-500/30 transition-all group relative overflow-hidden">
               <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.02] to-transparent pointer-events-none"></div>
              <div class="text-[10px] uppercase font-black text-secondary tracking-[0.2em] mb-3">{{ m.label }}</div>
              <div class="flex items-baseline gap-3 relative z-10">
                <span class="text-3xl font-black text-primary group-hover:scale-110 transition-transform origin-left inline-block">{{ m.value }}</span>
                <span class="text-[10px] font-black text-secondary-alt uppercase tracking-widest">{{ m.suffix }}</span>
              </div>
            </div>
          </div>
        </transition>
      </div>

      <!-- Main Machines Grid -->
      <div v-if="loading && machines.length === 0" class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-8">
        <div v-for="i in 8" :key="i" class="h-64 rounded-[40px] bg-white/[0.03] border border-white/10 animate-pulse"></div>
      </div>
      <div v-else class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 2xl:grid-cols-4 gap-8">
        <transition-group name="list">
          <MachineCard
            v-for="machine in filteredMachines"
            :key="machine.id"
            :machine="machine"
          />
        </transition-group>
      </div>

      <!-- Empty State -->
      <div v-if="!loading && filteredMachines.length === 0" class="py-24 text-center space-y-8 border border-white/10 border-dashed rounded-[48px] bg-white/[0.01]">
        <div class="mx-auto w-24 h-24 bg-white/5 rounded-full flex items-center justify-center border border-white/5 shadow-inner">
          <Box class="w-10 h-10 text-secondary opacity-30" />
        </div>
        <div>
          <h3 class="text-2xl font-black text-primary">No Assets Match Protocol</h3>
          <p class="text-secondary-alt text-sm mt-3 max-w-sm mx-auto font-medium">Try clearing your filters or changing your search query to see other equipment.</p>
        </div>
        <BaseButton variant="outline" class="h-12 px-10 rounded-2xl border-white/10 font-black uppercase tracking-widest text-[10px]" @click="clearAllFilters">
          Reset Exploration
        </BaseButton>
      </div>
    </div>

    <!-- Background Decoration -->
    <div class="fixed bottom-0 left-0 w-full h-1/4 bg-gradient-to-t from-indigo-500/[0.03] to-transparent pointer-events-none -z-10"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useToast } from '@/composables/useToast'
import { useSearchFilters } from '@/composables/useSearchFilters'

import BaseButton from './base/BaseButton.vue'
import StatCard from './base/StatCard.vue'
import MetricValue from './base/MetricValue.vue'
import MachineCard from './MachineCard.vue'
import BaseBadge from './base/BaseBadge.vue'
import { 
  RefreshCw, Search, Box, AlertTriangle, 
  Activity, Database, ChevronRight 
} from 'lucide-vue-next'
import { retrainModels } from '@/services/predictions.service'

import { fetchProductionLines } from '@/services/productionLines.service'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto, ProductionLineDto } from '@/api/types'
import { normalizeStatus } from '@/utils/statusFormatter'
import { getMachineUptime, getRiskLevel } from '@/utils/machineMetrics'

const toast = useToast()
const loading = ref(true)
const retraining = ref(false)
const machines = ref<MachineDto[]>([])
const productionLines = ref<ProductionLineDto[]>([])
const activeLineId = ref<string>('')
const lastUpdated = ref<Date | null>(null)

const {
  searchQuery,
  activeStatus,
  clearAllFilters: clearAllFiltersBase,
  toggleStatusFilter
} = useSearchFilters()

const mainStats = computed(() => [
  { title: 'Fleet Health', icon: Activity, iconColor: 'text-indigo-400', value: 94.2, suffix: '%', variant: 'default' },
  { title: 'Active Risk', icon: AlertTriangle, iconColor: 'text-amber-500', value: highRiskMachinesCount.value, suffix: 'UNITS', variant: highRiskMachinesCount.value > 0 ? 'warning' : 'default' },
  { title: 'Avg. RUL', icon: Box, iconColor: 'text-emerald-400', value: 142, suffix: 'DAYS', variant: 'default' },
  { title: 'Deployment', icon: Database, iconColor: 'text-blue-400', value: '2.4', suffix: 'STABLE', variant: 'default' }
])

const statusFilters = [
  { label: 'Operational', value: 'operational' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' }
]

const lastUpdatedText = computed(() => lastUpdated.value ? lastUpdated.value.toLocaleTimeString() : '--:--')
const activeLine = computed(() => productionLines.value.find(l => l.id === activeLineId.value))
const highRiskMachinesCount = computed(() => machines.value.filter(m => getRiskLevel(m.id) === 'High').length)

const filteredMachines = computed(() => {
  let f = [...machines.value]
  if (activeLineId.value) f = f.filter(m => activeLine.value?.machineIds.includes(m.id))
  if (activeStatus.value) f = f.filter(m => normalizeStatus(m.status) === activeStatus.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    f = f.filter(m => m.name.toLowerCase().includes(q) || m.id.toLowerCase().includes(q) || m.type.toLowerCase().includes(q))
  }
  return f
})

const displayLineMetrics = computed(() => {
  if (!lineMetrics.value) return []
  return [
    { label: 'Asset Count', value: lineMetrics.value.totalMachines, suffix: 'Units' },
    { label: 'Availability', value: lineMetrics.value.averageUptime, suffix: '%' },
    { label: 'Critical Drifts', value: lineMetrics.value.criticalMachines, suffix: 'Alerts' },
    { label: 'Line Load', value: '82', suffix: '% Cap' }
  ]
})

const lineMetrics = computed(() => {
  if (!activeLine.value) return null
  const lineMachines = machines.value.filter(m => activeLine.value?.machineIds.includes(m.id))
  return {
    totalMachines: lineMachines.length,
    criticalMachines: lineMachines.filter(m => normalizeStatus(m.status) === 'critical').length,
    averageUptime: Math.round(lineMachines.reduce((s, m) => s + getMachineUptime(m.id), 0) / (lineMachines.length || 1))
  }
})

const fetchAllPredictions = async () => {
  loading.value = true
  try {
    const [m, l] = await Promise.all([fetchMachines(), fetchProductionLines()])
    machines.value = m
    productionLines.value = l
    if (l.length && !activeLineId.value) activeLineId.value = l[0].id
    lastUpdated.value = new Date()
  } catch {
    toast.error('Network synchronization error')
  } finally {
    loading.value = false
  }
}

const retrainModel = async () => {
  retraining.value = true
  try {
    await retrainModels(true)
    toast.success('Enterprise model optimization complete')
  } catch (err) {
    toast.error('Retraining cluster synchronization failed')
  } finally {
    retraining.value = false
  }
}

const clearAllFilters = () => {
  clearAllFiltersBase()
}

onMounted(fetchAllPredictions)
</script>

<style scoped>
.predictive-analytics {
  background: radial-gradient(circle at 50% 0%, rgba(99, 102, 241, 0.05) 0%, transparent 50%);
}

.list-enter-active, .list-leave-active {
  transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}
.list-enter-from {
  opacity: 0;
  transform: translateY(30px) scale(0.9);
}
.list-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.4s ease, transform 0.4s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>

