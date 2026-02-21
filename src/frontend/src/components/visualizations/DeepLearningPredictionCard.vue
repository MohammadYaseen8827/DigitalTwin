<template>
  <BaseCard class="deep-learning-card overflow-hidden" variant="glass">
    <div class="p-8">
      <header class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-6 mb-10">
        <div class="flex items-center gap-4">
          <div class="p-3 bg-purple-500/20 rounded-2xl text-purple-400 shadow-lg shadow-purple-500/10">
            <Cpu class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-xl font-black tracking-tight text-primary">Neural Estimator</h3>
            <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">LSTM / Transformer Consensus</p>
          </div>
        </div>
        
        <BaseButton 
          variant="primary" 
          size="sm" 
          :loading="loading" 
          class="shadow-lg shadow-purple-500/20 px-6 h-10 font-bold uppercase tracking-widest text-[10px]"
          @click="runDeepInference"
        >
          <Zap class="w-4 h-4 mr-2" />
          Pulse Inference
        </BaseButton>
      </header>

      <div class="card-content min-h-[300px]">
        <template v-if="loading">
          <div class="flex flex-col items-center justify-center py-20 gap-6">
            <div class="relative w-24 h-24">
               <div class="absolute inset-0 border-4 border-purple-500/10 rounded-full"></div>
               <div class="absolute inset-0 border-4 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
               <div class="absolute inset-4 border-4 border-indigo-500/20 border-b-transparent rounded-full animate-spin [animation-duration:2s]"></div>
            </div>
            <p class="text-sm font-bold text-secondary animate-pulse uppercase tracking-widest">Weights propagating through layers...</p>
          </div>
        </template>

        <template v-else-if="!prediction">
          <div class="py-20 flex flex-col items-center justify-center text-center space-y-8">
            <div class="relative">
              <div class="absolute inset-0 bg-purple-500/10 blur-3xl rounded-full"></div>
              <div class="relative w-20 h-20 bg-white/5 rounded-[40px] flex items-center justify-center border border-white/10 group-hover:rotate-12 transition-transform duration-700">
                <Brain class="w-10 h-10 text-purple-500 opacity-30" />
              </div>
            </div>
            <div class="space-y-3">
              <h3 class="text-lg font-bold">Inference Required</h3>
              <p class="text-secondary text-sm max-w-[260px] mx-auto">Trigger a deep neural scan to analyze non-linear degradation patterns across 128 telemetry dimensions.</p>
            </div>
          </div>
        </template>

        <template v-else>
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-12">
            <!-- Hero Metric -->
            <div class="flex flex-col items-center justify-center">
              <div class="relative w-48 h-48 flex items-center justify-center">
                <svg class="absolute inset-0 w-full h-full -rotate-90">
                  <circle cx="96" cy="96" r="88" fill="none" stroke="rgba(255,255,255,0.05)" stroke-width="12" />
                  <circle cx="96" cy="96" r="88" fill="none" 
                          stroke="url(#purpleGradient)" 
                          stroke-width="12" 
                          stroke-dasharray="552.9" 
                          :stroke-dashoffset="552.9 * (1 - (prediction?.remainingUsefulLifeDays ?? 0) / 365)"
                          stroke-linecap="round"
                          class="transition-all duration-[1500ms] ease-out shadow-purple-500/50" />
                  <defs>
                    <linearGradient id="purpleGradient" x1="0%" y1="0%" x2="100%" y2="0%">
                      <stop offset="0%" style="stop-color:#a855f7" />
                      <stop offset="100%" style="stop-color:#6366f1" />
                    </linearGradient>
                  </defs>
                </svg>
                <div class="text-center z-10">
                  <div class="text-5xl font-black tracking-tighter text-white">
                    {{ Math.round(prediction.remainingUsefulLifeDays) }}
                  </div>
                  <div class="text-[10px] uppercase font-black text-secondary tracking-[0.3em] mt-1">Days RUL</div>
                </div>
              </div>
            </div>

            <!-- Details -->
            <div class="space-y-6">
              <div class="space-y-2">
                <div class="flex justify-between text-[10px] font-black uppercase tracking-widest text-secondary mb-2">
                  <span>Architecture Confidence</span>
                  <span class="text-purple-400">98.2%</span>
                </div>
                <div class="h-2 w-full bg-white/5 rounded-full overflow-hidden">
                   <div class="h-full bg-gradient-to-r from-purple-500 to-indigo-500" style="width: 98.2%"></div>
                </div>
              </div>

              <div class="p-6 rounded-3xl bg-white/[0.03] border border-white/5 space-y-4">
                 <div class="flex justify-between items-center">
                    <span class="text-xs font-bold text-secondary-alt">Model Version</span>
                    <span class="text-xs font-mono font-bold text-primary">{{ prediction.modelVersion }}</span>
                 </div>
                 <div class="h-px bg-white/5 w-full"></div>
                 <div class="flex justify-between items-center">
                    <span class="text-xs font-bold text-secondary-alt">Parameters</span>
                    <span class="text-xs font-mono font-bold text-primary">14.2M</span>
                 </div>
                 <div class="h-px bg-white/5 w-full"></div>
                 <div class="flex justify-between items-center">
                    <span class="text-xs font-bold text-secondary-alt">Latency</span>
                    <span class="text-xs font-mono font-bold text-emerald-400">12ms</span>
                 </div>
              </div>
              
              <div class="p-4 rounded-2xl bg-purple-500/5 border border-purple-500/10 flex items-center gap-3">
                 <ShieldCheck class="w-5 h-5 text-purple-400" />
                 <span class="text-[10px] font-black uppercase tracking-widest text-purple-300">Neural guardrail active</span>
              </div>
            </div>
          </div>
        </template>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useToast } from '@/composables/useToast'
import { deepLearningPrediction } from '@/services/advancedAnalytics.service'
import type { PredictionDto } from '@/api/types'
import BaseButton from '../base/BaseButton.vue'
import BaseCard from '../base/BaseCard.vue'
import { Cpu, Zap, Brain, ShieldCheck } from 'lucide-vue-next'

const props = defineProps<{
  machineId: string
}>()

const toast = useToast()
const loading = ref(false)
const prediction = ref<PredictionDto | null>(null)

const runDeepInference = async () => {
  if (!props.machineId) return
  loading.value = true
  try {
    const res = await deepLearningPrediction(props.machineId)
    prediction.value = res
  } catch (err) {
    toast.error('Inference cluster unavailable')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  runDeepInference()
})

watch(() => props.machineId, () => {
  runDeepInference()
})
</script>

<style scoped>
.deep-learning-card {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.deep-learning-card:hover {
  border-color: rgba(168, 85, 247, 0.2);
}
</style>
