<template>
  <BaseCard class="ensemble-prediction-card overflow-hidden" variant="glass">
    <div class="p-8">
      <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-10">
        <div class="flex items-center gap-4">
          <div class="p-3 bg-indigo-500/20 rounded-2xl text-indigo-400 shadow-lg shadow-indigo-500/10">
            <Layers class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-xl font-black tracking-tight text-primary">Ensemble Intelligence</h3>
            <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">Multi-Model Consensus</p>
          </div>
        </div>
        
        <BaseButton 
          variant="primary" 
          size="sm" 
          :loading="loading" 
          class="shadow-lg shadow-indigo-500/20 px-6 h-10 font-bold uppercase tracking-widest text-[10px]"
          @click="generateEnsemblePrediction"
        >
          <Zap class="w-4 h-4 mr-2" />
          Run Analysis
        </BaseButton>
      </header>

      <div class="card-content min-h-[300px]">
        <template v-if="loading">
          <div class="flex flex-col items-center justify-center py-20 gap-4">
            <div class="w-12 h-12 border-4 border-indigo-500/20 border-t-indigo-500 rounded-full animate-spin"></div>
            <p class="text-sm font-bold text-secondary animate-pulse uppercase tracking-widest">Aggregating Model Consensus...</p>
          </div>
        </template>

        <template v-else-if="error">
          <div class="p-8 rounded-3xl bg-red-500/5 border border-red-500/20 text-center space-y-4">
            <AlertCircle class="w-12 h-12 text-red-500 mx-auto" />
            <h4 class="text-red-500 font-bold">Analysis Failed</h4>
            <p class="text-sm text-secondary">{{ error }}</p>
            <BaseButton variant="outline" size="sm" @click="generateEnsemblePrediction">Retry Sync</BaseButton>
          </div>
        </template>

        <template v-else-if="prediction">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <!-- Left: Hero Metric -->
            <div class="flex flex-col items-center justify-center p-8 bg-white/5 rounded-3xl border border-white/10 relative overflow-hidden group">
              <div class="absolute inset-0 bg-gradient-to-br from-indigo-500/5 to-transparent pointer-events-none"></div>
              
              <div class="relative w-40 h-40 flex items-center justify-center mb-6">
                <svg class="absolute inset-0 w-full h-full -rotate-90">
                  <circle cx="80" cy="80" r="74" fill="none" stroke="rgba(255,255,255,0.05)" stroke-width="12" />
                  <circle cx="80" cy="80" r="74" fill="none" 
                          :stroke="riskColor" 
                          stroke-width="12" 
                          stroke-dasharray="464.9" 
                          :stroke-dashoffset="464.9 * (1 - (prediction?.remainingUsefulLifeDays ?? 0) / 365)"
                          stroke-linecap="round"
                          class="transition-all duration-[2000ms] ease-out" />
                </svg>
                <div class="text-center z-10">
                  <div class="text-4xl font-black tracking-tighter" :style="{ color: riskColor }">
                    {{ Math.round(prediction.remainingUsefulLifeDays) }}
                  </div>
                  <div class="text-[9px] uppercase font-black text-secondary tracking-[0.2em] mt-1">Days RUL</div>
                </div>
              </div>
              
              <div class="flex items-center gap-2 px-4 py-1.5 bg-white/5 rounded-full border border-white/10 text-[10px] font-black uppercase tracking-widest" :class="confidenceClass">
                Confidence: {{ confidencePercentage }}%
              </div>
            </div>

            <!-- Right: Secondary Metrics -->
            <div class="space-y-4">
              <div class="p-4 bg-white/5 rounded-2xl border border-white/5 flex justify-between items-center group hover:bg-white/[0.08] transition-colors">
                <span class="text-[10px] uppercase font-black text-secondary tracking-widest">Failure Risk</span>
                <span class="text-lg font-black" :class="riskLabelClass">{{ (prediction.failureProbability * 100).toFixed(1) }}%</span>
              </div>
              
              <div class="p-4 bg-white/5 rounded-2xl border border-white/5 flex justify-between items-center group hover:bg-white/[0.08] transition-colors">
                <span class="text-[10px] uppercase font-black text-secondary tracking-widest">Health State</span>
                <BaseBadge :variant="healthStatusVariant" class="uppercase font-black tracking-widest text-[9px]">
                  {{ prediction.healthStatus }}
                </BaseBadge>
              </div>

              <div class="p-4 bg-white/5 rounded-2xl border border-white/5 space-y-3">
                <span class="text-[10px] uppercase font-black text-secondary tracking-widest">Prediction Range</span>
                <div class="flex justify-between items-end">
                  <div class="text-center">
                    <div class="text-[8px] text-secondary-alt uppercase font-bold">Min</div>
                    <div class="text-sm font-black">{{ Math.round(prediction.rulLowerBound) }}d</div>
                  </div>
                  <div class="flex-1 h-px bg-white/10 mb-2 mx-4 relative">
                    <div class="absolute inset-0 bg-indigo-500/30"></div>
                  </div>
                  <div class="text-center">
                    <div class="text-[8px] text-secondary-alt uppercase font-bold">Max</div>
                    <div class="text-sm font-black">{{ Math.round(prediction.rulUpperBound) }}d</div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Feature Explainability -->
          <div class="mt-8 pt-8 border-t border-white/5" v-if="prediction.featureContributions">
            <h4 class="text-[10px] font-black uppercase tracking-widest text-secondary mb-6">Attribution Insights</h4>
            <PredictionExplanation :contributions="prediction.featureContributions" />
          </div>

          <!-- Ensemble Meta -->
          <footer class="mt-8 grid grid-cols-2 gap-4">
             <div class="p-4 rounded-2xl bg-indigo-500/5 border border-indigo-500/10">
                <div class="text-[8px] uppercase font-black text-indigo-400 tracking-widest mb-1">Architecture</div>
                <div class="text-xs font-bold">Consensus Weights v{{ prediction.modelVersion }}</div>
             </div>
             <div class="p-4 rounded-2xl bg-white/[0.03] border border-white/5">
                <div class="text-[8px] uppercase font-black text-secondary tracking-widest mb-1">Last Sync</div>
                <div class="text-xs font-bold">{{ formatDate(prediction.createdAt) }}</div>
             </div>
          </footer>
        </template>

        <template v-else>
          <div class="py-20 flex flex-col items-center justify-center text-center space-y-6">
            <div class="w-16 h-16 bg-white/5 rounded-[24px] flex items-center justify-center border border-white/10 group-hover:scale-110 transition-transform">
              <BarChart3 class="w-8 h-8 text-secondary opacity-40" />
            </div>
            <div class="space-y-2">
              <h3 class="text-lg font-bold">No Active Prediction</h3>
              <p class="text-secondary text-sm max-w-[240px]">Aggregated multi-model analysis is required for enterprise-grade accuracy.</p>
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
import { ensemblePrediction } from '@/services/advancedAnalytics.service'
import type { PredictionDto } from '@/api/types'
import PredictionExplanation from './PredictionExplanation.vue'
import BaseButton from '../base/BaseButton.vue'
import BaseCard from '../base/BaseCard.vue'
import BaseBadge from '../base/BaseBadge.vue'
import { Zap, AlertCircle, BarChart3, Layers } from 'lucide-vue-next'

const props = defineProps<{
  machineId: string
}>()

const toast = useToast()
const loading = ref(false)
const error = ref<string | null>(null)
const prediction = ref<PredictionDto | null>(null)

const confidencePercentage = computed(() => {
  if (!prediction.value?.rulLowerBound || !prediction.value?.rulUpperBound) return 0
  const range = prediction.value.rulUpperBound - prediction.value.rulLowerBound
  const confidence = Math.max(0, 100 - (range / (prediction.value.remainingUsefulLifeDays || 1)) * 50)
  return Math.round(confidence)
})

const confidenceClass = computed(() => {
  const conf = confidencePercentage.value
  if (conf >= 80) return 'text-emerald-400'
  if (conf >= 60) return 'text-amber-400'
  return 'text-red-400'
})

const riskColor = computed(() => {
  if (!prediction.value) return 'rgba(255,255,255,0.1)'
  const prob = prediction.value.failureProbability
  if (prob > 0.7) return '#ef4444'
  if (prob > 0.4) return '#f59e0b'
  if (prob > 0.2) return '#fbbf24'
  return '#10b981'
})

const riskLabelClass = computed(() => {
  if (!prediction.value) return ''
  const prob = prediction.value.failureProbability
  if (prob > 0.7) return 'text-red-500'
  if (prob > 0.4) return 'text-amber-500'
  if (prob > 0.2) return 'text-amber-400'
  return 'text-emerald-500'
})

const healthStatusVariant = computed(() => {
  if (!prediction.value) return 'default'
  const status = prediction.value.healthStatus?.toLowerCase()
  if (status?.includes('healthy')) return 'success'
  if (status?.includes('degraded')) return 'warning'
  if (status?.includes('critical')) return 'danger'
  return 'default'
})

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

const generateEnsemblePrediction = async () => {
  if (!props.machineId) return
  loading.value = true
  error.value = null
  
  try {
    const result = await ensemblePrediction(props.machineId)
    prediction.value = result
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Network sync error'
    toast.error('Prediction engine unreachable')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  generateEnsemblePrediction()
})

watch(() => props.machineId, () => {
  generateEnsemblePrediction()
})
</script>

<style scoped>
.ensemble-prediction-card {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.ensemble-prediction-card:hover {
  border-color: rgba(99, 102, 241, 0.2);
}
</style>

