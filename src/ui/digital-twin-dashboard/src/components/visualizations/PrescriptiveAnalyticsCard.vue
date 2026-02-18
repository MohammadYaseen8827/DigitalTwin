<template>
  <BaseCard class="prescriptive-analytics-card overflow-hidden" variant="glass">
    <div class="p-8">
      <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-10">
        <div class="flex items-center gap-4">
          <div class="p-3 bg-pink-500/20 rounded-2xl text-pink-500 shadow-lg shadow-pink-500/10">
            <Target class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-xl font-black tracking-tight text-primary">Prescriptive Logic</h3>
            <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">Automated Action Planning</p>
          </div>
        </div>
        
        <BaseButton 
          variant="primary" 
          size="sm" 
          :loading="loading" 
          class="shadow-lg shadow-pink-500/20 px-6 h-10 font-bold uppercase tracking-widest text-[10px]"
          @click="generateRecommendations"
        >
          <Lightbulb class="w-4 h-4 mr-2" />
          Plan Actions
        </BaseButton>
      </header>

      <div class="card-content min-h-[400px]">
        <template v-if="loading">
          <div class="flex flex-col items-center justify-center py-20 gap-6">
            <div class="relative w-16 h-16">
              <div class="absolute inset-0 border-4 border-pink-500/20 rounded-full"></div>
              <div class="absolute inset-0 border-4 border-pink-500 border-t-transparent rounded-full animate-spin"></div>
            </div>
            <p class="text-sm font-bold text-secondary animate-pulse uppercase tracking-widest">Running optimization sims...</p>
          </div>
        </template>

        <template v-else-if="error">
          <div class="p-10 rounded-[40px] bg-red-500/5 border border-red-500/20 text-center space-y-4">
            <AlertTriangle class="w-12 h-12 text-red-500 mx-auto" />
            <h4 class="text-red-500 font-bold">Optimization Error</h4>
            <p class="text-sm text-secondary">{{ error }}</p>
            <BaseButton variant="outline" size="sm" @click="generateRecommendations">Retry Engine</BaseButton>
          </div>
        </template>

        <template v-else-if="recommendation">
          <div class="space-y-8">
            <!-- Primary Recommendation Hero -->
            <div class="p-8 rounded-[40px] bg-gradient-to-br from-pink-500/10 to-transparent border border-pink-500/20 relative overflow-hidden group">
              <div class="absolute top-0 right-0 p-6">
                <BaseBadge :variant="priorityVariant" class="font-black uppercase tracking-widest text-[10px] px-4 py-1.5 h-auto">
                   {{ getPriorityLabel(recommendation.priorityScore) }}
                </BaseBadge>
              </div>

              <div class="flex items-start gap-6 relative z-10">
                <div class="p-5 bg-white/10 rounded-3xl border border-white/10 shadow-xl group-hover:scale-110 transition-transform">
                  <Calendar class="w-8 h-8 text-pink-400" />
                </div>
                <div>
                  <h4 class="text-2xl font-black text-primary mb-2 line-clamp-1">Optimal Action Found</h4>
                  <div class="text-lg font-bold text-pink-100 mb-1">{{ recommendation.recommendedType }}</div>
                  <p class="text-sm text-secondary font-medium">{{ formatDate(recommendation.recommendedDate) }}</p>
                </div>
              </div>

              <div class="grid grid-cols-2 gap-4 mt-10">
                <div class="p-5 bg-white/5 rounded-3xl border border-white/5">
                  <div class="text-[9px] uppercase font-black text-secondary tracking-widest mb-1">Expected Cost</div>
                  <div class="text-xl font-black text-primary">${{ recommendation.expectedCost.toLocaleString() }}</div>
                </div>
                <div class="p-5 bg-white/5 rounded-3xl border border-white/5">
                  <div class="text-[9px] uppercase font-black text-secondary tracking-widest mb-1">Risk Reduction</div>
                  <div class="text-xl font-black text-emerald-400">{{ (recommendation.riskReduction * 100).toFixed(0) }}%</div>
                </div>
              </div>
            </div>

            <!-- Rationale & Insights -->
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="p-6 rounded-3xl bg-white/[0.03] border border-white/5 relative group">
                <div class="flex items-center gap-2 mb-4">
                  <span class="text-lg">🧠</span>
                  <h4 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary">Decision Rationale</h4>
                </div>
                <p class="text-sm text-secondary leading-relaxed font-medium">
                  {{ recommendation.reasoning }}
                </p>
              </div>

              <div class="space-y-4">
                <h4 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary px-2">Alternative Paths</h4>
                <div class="space-y-3">
                  <div v-for="(alt, index) in recommendation.alternatives.slice(0, 2)" :key="index" 
                       class="p-4 rounded-2xl bg-white/[0.03] border border-white/5 flex justify-between items-center group hover:bg-white/[0.06] transition-colors cursor-pointer">
                    <div class="flex items-center gap-3">
                      <div class="w-1.5 h-1.5 rounded-full bg-pink-500"></div>
                      <span class="text-xs font-bold text-secondary">{{ formatShortDate(alt.date) }}</span>
                    </div>
                    <span class="text-xs font-black text-primary">${{ alt.cost.toLocaleString() }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Optimization Config (Simplified) -->
            <div class="p-6 rounded-3xl bg-white/[0.02] border border-white/5">
              <div class="flex justify-between items-center mb-6">
                <h4 class="text-[10px] font-black uppercase tracking-widest text-secondary">Optimization Constraints</h4>
                <button @click="resetDefaults" class="text-[9px] uppercase font-black text-pink-400 hover:text-pink-300 transition-colors">Reset Global Defaults</button>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-3 gap-6">
                <div v-for="(val, key) in costFactorsDisplay" :key="key" class="space-y-2">
                  <label class="text-[9px] font-black text-secondary-alt uppercase tracking-widest">{{ val.label }}</label>
                  <div class="relative group">
                    <span class="absolute left-3 top-1/2 -translate-y-1/2 text-[10px] font-black text-secondary">$</span>
                    <input 
                      type="number" 
                      v-model.number="costFactors[key as keyof typeof costFactors]"
                      class="w-full bg-white/5 border border-white/5 rounded-xl py-2 pl-7 pr-3 text-xs font-bold focus:ring-1 focus:ring-pink-500/50 outline-none"
                    >
                  </div>
                </div>
              </div>
            </div>
          </div>
        </template>

        <template v-else>
          <div class="py-20 flex flex-col items-center justify-center text-center space-y-8">
            <div class="relative">
              <div class="absolute inset-0 bg-pink-500/10 blur-3xl rounded-full"></div>
              <div class="relative w-20 h-20 bg-white/5 rounded-[32px] flex items-center justify-center border border-white/10 group-hover:scale-105 transition-transform duration-500">
                <Lightbulb class="w-10 h-10 text-pink-500 opacity-40" />
              </div>
            </div>
            <div class="space-y-3 max-w-sm">
              <h3 class="text-xl font-black tracking-tight text-primary">Awaiting Constraints</h3>
              <p class="text-secondary text-sm font-medium leading-relaxed">Optimization engine is ready to simulate maintenance scenarios based on your current cost factors.</p>
            </div>
            <div class="flex gap-2">
              <div v-for="i in 3" :key="i" class="w-1.5 h-1.5 rounded-full bg-pink-500/20"></div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import { generateMaintenanceRecommendation } from '@/services/advancedAnalytics.service'
import type { 
  PrescriptiveRecommendation, 
  MaintenanceRecommendationRequest 
} from '@/services/advancedAnalytics.service'
import BaseButton from '../base/BaseButton.vue'
import BaseCard from '../base/BaseCard.vue'
import BaseBadge from '../base/BaseBadge.vue'
import { Lightbulb, AlertTriangle, Calendar, Target } from 'lucide-vue-next'

const props = defineProps<{
  machineId: string
}>()

const toast = useToast()
const loading = ref(false)
const error = ref<string | null>(null)
const recommendation = ref<PrescriptiveRecommendation | null>(null)

const costFactors = ref({
  preventiveMaintenanceCost: 5000,
  reactiveFailureCost: 25000,
  downtimeCostPerHour: 2000
})

const costFactorsDisplay = {
  preventiveMaintenanceCost: { label: 'Prev. Cost' },
  reactiveFailureCost: { label: 'Reactive Cost' },
  downtimeCostPerHour: { label: 'Downtime / Hr' }
}

const priorityVariant = computed(() => {
  const score = recommendation.value?.priorityScore ?? 0
  if (score >= 80) return 'danger'
  if (score >= 60) return 'warning'
  return 'success'
})

const getPriorityLabel = (score: number) => {
  if (score >= 80) return 'Critical Priority'
  if (score >= 60) return 'High Priority'
  if (score >= 40) return 'Standard Priority'
  return 'Low Priority'
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    weekday: 'long',
    month: 'long',
    day: 'numeric',
    year: 'numeric'
  })
}

const formatShortDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric'
  })
}

const resetDefaults = () => {
  costFactors.value = {
    preventiveMaintenanceCost: 5000,
    reactiveFailureCost: 25000,
    downtimeCostPerHour: 2000
  }
  toast.info('Factory defaults restored')
}

const generateRecommendations = async () => {
  if (!props.machineId) return
  loading.value = true
  error.value = null
  
  try {
    const request: MaintenanceRecommendationRequest = {
      costFactors: costFactors.value,
      businessImpact: {
        productionLossPerHour: costFactors.value.downtimeCostPerHour,
        customerImpact: 5
      },
      timeConstraints: {}
    }
    
    const result = await generateMaintenanceRecommendation(props.machineId, request)
    recommendation.value = result
    toast.success('Optimal maintenance plan synthesized')
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Optimization engine timeout'
    toast.error('Synthesis failed')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  generateRecommendations()
})

watch(() => props.machineId, () => {
  generateRecommendations()
})
</script>

<style scoped>
.prescriptive-analytics-card {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.prescriptive-analytics-card:hover {
  border-color: rgba(236, 72, 153, 0.2);
}

input[type=number]::-webkit-inner-spin-button, 
input[type=number]::-webkit-outer-spin-button { 
  -webkit-appearance: none; 
  margin: 0; 
}
</style>

