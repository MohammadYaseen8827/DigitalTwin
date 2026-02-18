<template>
  <main class="advanced-analytics-page min-h-screen pb-20 space-y-12">
    <!-- Premium Header -->
    <div class="page-header relative overflow-hidden p-12 mb-10 border-b border-white/5 bg-gradient-to-b from-indigo-500/[0.03] to-transparent">
      <div class="absolute top-0 left-0 w-full h-full pointer-events-none overflow-hidden">
        <div class="absolute -top-24 -right-24 w-96 h-96 bg-indigo-500/10 blur-[120px] rounded-full"></div>
        <div class="absolute top-1/2 -left-24 w-64 h-64 bg-emerald-500/5 blur-[100px] rounded-full"></div>
      </div>

      <div class="relative z-10 max-w-7xl mx-auto flex flex-col lg:flex-row justify-between items-start lg:items-end gap-10">
        <div class="space-y-4">
          <div class="flex items-center gap-4">
            <div class="p-3.5 bg-indigo-500 rounded-3xl shadow-xl shadow-indigo-500/20 border border-white/10">
              <Brain class="w-8 h-8 text-white" />
            </div>
            <div>
              <p class="text-[10px] uppercase font-black tracking-[0.4em] text-indigo-400 mb-1.5">Intelligence Layer</p>
              <h1 class="text-4xl font-black tracking-tighter text-primary">Advanced Analytics Suite</h1>
            </div>
          </div>
          <p class="text-secondary-alt max-w-xl text-lg font-bold leading-relaxed">
            Cutting-edge health forecasting powered by neural ensembles and prescriptive optimization models.
          </p>
        </div>

        <div class="w-full lg:w-96 space-y-3">
          <label class="text-[10px] uppercase font-black text-secondary tracking-[0.2em] pl-1">Target Asset Selection</label>
          <div class="relative group">
            <Search class="absolute left-5 top-1/2 -translate-y-1/2 w-4 h-4 text-secondary group-focus-within:text-indigo-400 transition-colors" />
            <div v-if="machinesStore.loading && machines.length === 0" class="h-16 w-full rounded-[20px] bg-white/5 animate-pulse border border-white/10"></div>
            <select 
              v-else
              v-model="selectedMachineId" 
              class="w-full bg-white/5 border border-white/10 rounded-[20px] pl-14 pr-10 h-16 text-[13px] font-black tracking-tight focus:ring-2 focus:ring-indigo-500/50 outline-none transition-all appearance-none cursor-pointer hover:bg-white/[0.08]"
            >
              <option :value="null" class="bg-slate-900">Global Overview</option>
              <option v-for="machine in machines" :key="machine.id" :value="machine.id" class="bg-slate-900">
                {{ machine.name }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content Selection -->
    <div class="max-w-7xl mx-auto px-10">
      <div v-if="!selectedMachineId" class="py-32 flex flex-col items-center justify-center text-center space-y-10 bg-white/[0.01] border border-dashed border-white/10 rounded-[64px]">
        <div class="relative">
          <div class="absolute inset-0 bg-indigo-500/20 blur-[80px] rounded-full scale-150"></div>
          <div class="relative p-10 bg-white/5 rounded-full border border-white/10 shadow-2xl">
            <ScanSearch class="w-20 h-20 text-indigo-400 animate-pulse" />
          </div>
        </div>
        <div class="space-y-4 max-w-md">
          <h2 class="text-3xl font-black tracking-tighter text-primary">Awaiting Asset Signal</h2>
          <p class="text-secondary-alt font-medium text-lg">Select a specific machine to unlock detailed ensemble predictions, anomaly scanning, and automated maintenance scheduling.</p>
        </div>
        <div class="flex gap-4">
          <div v-for="i in 3" :key="i" class="w-2.5 h-2.5 rounded-full bg-indigo-500/30 animate-bounce" :style="{ animationDelay: `${i * 0.15}s` }"></div>
        </div>
      </div>

      <template v-else>
        <!-- Dynamic Analytics Grid -->
        <div class="grid grid-cols-1 xl:grid-cols-12 gap-10 mb-20 animate-in fade-in slide-in-from-bottom-5 duration-700">
          <!-- Left Column: Primary Predictions -->
          <div class="xl:col-span-8 space-y-12">
            <section class="space-y-6">
              <div class="flex items-center justify-between px-2">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-1 bg-indigo-500 rounded-full"></div>
                  <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-secondary">Predictive Models</h3>
                </div>
                <BaseBadge variant="emerald" class="text-[9px] font-black uppercase tracking-widest px-3 py-1.5 bg-emerald-500/10">Ensemble Active</BaseBadge>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
                <EnsemblePredictionCard :machine-id="selectedMachineId" class="hover-lift border-white/5" />
                <DeepLearningPredictionCard :machine-id="selectedMachineId" class="hover-lift border-white/5" />
              </div>
            </section>

            <section class="space-y-6">
              <div class="flex items-center justify-between px-2">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-1 bg-amber-500 rounded-full"></div>
                  <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-secondary">Pattern Discovery</h3>
                </div>
                <BaseBadge variant="warning" class="text-[9px] font-black uppercase tracking-widest px-3 py-1.5 bg-amber-500/10">Scanning real-time</BaseBadge>
              </div>
              <AnomalyDetectionCard :machine-id="selectedMachineId" class="border-white/5" />
            </section>
          </div>

          <!-- Right Column: Optimization & Planning -->
          <div class="xl:col-span-4 space-y-10">
            <section class="space-y-6">
              <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-secondary px-2">Prescriptive Logic</h3>
              <PrescriptiveAnalyticsCard :machine-id="selectedMachineId" class="border-white/5" />
            </section>
            
            <section class="space-y-6">
              <h3 class="text-[10px] font-black uppercase tracking-[0.3em] text-secondary px-2">Future Horizons</h3>
              <ForecastingCard :machine-id="selectedMachineId" class="border-white/5" />
            </section>

            <div class="p-10 rounded-[40px] bg-gradient-to-br from-indigo-500/10 to-transparent border border-white/10 relative overflow-hidden group">
              <Sparkles class="absolute -right-6 -bottom-6 w-40 h-40 text-indigo-500/5 group-hover:scale-110 transition-transform duration-1000" />
              <h4 class="text-[10px] font-black uppercase tracking-[0.3em] text-indigo-400 mb-4">AI Recommendation</h4>
              <p class="text-sm text-indigo-100/80 leading-relaxed relative z-10 font-bold tracking-tight">
                Our ensemble models suggest that <span class="text-indigo-400">Optimization Mode v4.2</span> should be enabled for this asset to reduce unexpected vibration drift by 12%.
              </p>
            </div>
          </div>
        </div>
      </template>

      <!-- Bottom Capabilities Showcase -->
      <footer class="mt-32 pt-32 border-t border-white/5">
        <div class="text-center mb-20 space-y-4">
          <div class="w-12 h-1.5 bg-indigo-500 rounded-full mx-auto mb-6"></div>
          <h2 class="text-4xl font-black tracking-tighter text-primary">Suite Architecture</h2>
          <p class="text-secondary-alt max-w-xl mx-auto font-bold text-lg leading-relaxed">Enterprise-grade algorithms designed for SME digital twin scalability.</p>
        </div>
        
        <div class="grid grid-cols-1 md:grid-cols-3 gap-10">
          <div v-for="capability in capabilities" :key="capability.title" class="p-10 rounded-[48px] bg-white/[0.01] border border-white/5 hover:border-indigo-500/30 transition-all group relative overflow-hidden">
             <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/[0.02] to-transparent pointer-events-none"></div>
            <div class="text-5xl mb-8 group-hover:scale-110 group-hover:-rotate-6 transition-all inline-block">{{ capability.icon }}</div>
            <h3 class="text-2xl font-black tracking-tight text-primary mb-4">{{ capability.title }}</h3>
            <p class="text-sm text-secondary-alt leading-relaxed font-bold">{{ capability.description }}</p>
          </div>
        </div>
      </footer>
    </div>
  </main>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useMachinesStore } from '@/stores/machines.store'
import EnsemblePredictionCard from '@/components/visualizations/EnsemblePredictionCard.vue'
import DeepLearningPredictionCard from '@/components/visualizations/DeepLearningPredictionCard.vue'
import AnomalyDetectionCard from '@/components/visualizations/AnomalyDetectionCard.vue'
import ForecastingCard from '@/components/visualizations/ForecastingCard.vue'
import PrescriptiveAnalyticsCard from '@/components/visualizations/PrescriptiveAnalyticsCard.vue'
import BaseBadge from '@/components/base/BaseBadge.vue'
import { Brain, Search, ScanSearch, Sparkles } from 'lucide-vue-next'

const machinesStore = useMachinesStore()
const selectedMachineId = ref<string | null>(null)

const machines = computed(() => machinesStore.machines)

const capabilities = [
  { icon: '🤖', title: 'Deep Learning', description: 'Long Short-Term Memory (LSTM) networks specialized in finding temporal correlations in noisy sensor data.' },
  { icon: '📊', title: 'Ensemble Weights', description: 'Dynamic weighting algorithm that prioritizes models based on their historical accuracy for specific machine types.' },
  { icon: '🎯', title: 'Prescriptive Ops', description: 'Genetic algorithms that simulate 10,000+ maintenance scenarios to find the optimal cost-vs-risk balance.' }
]

// Initialize store if needed
if (machines.value.length === 0) {
  machinesStore.loadMachines()
}
</script>

<style scoped>
.advanced-analytics-page {
  background: radial-gradient(circle at 100% 0%, rgba(99, 102, 241, 0.03) 0%, transparent 45%);
}

.hover-lift {
  transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
}

.hover-lift:hover {
  transform: translateY(-10px);
  box-shadow: 0 30px 60px -15px rgba(0, 0, 0, 0.5);
}

select {
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%2364748b'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='M19 9l-7 7-7-7'%3E%3C/path%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 1.5rem center;
  background-size: 1rem;
}
</style>
