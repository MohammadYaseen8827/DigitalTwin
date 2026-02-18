<template>
  <div class="prediction-explanation-premium space-y-6" v-if="hasContributions">
    <div v-for="(impact, feature) in sortedContributions" :key="feature" 
         class="feature-row group p-4 rounded-2xl bg-white/[0.02] border border-white/5 hover:bg-white/[0.05] hover:border-indigo-500/30 transition-all">
      <div class="flex justify-between items-center mb-3">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 rounded-xl bg-white/5 flex items-center justify-center border border-white/10 group-hover:scale-110 transition-transform">
            <Activity v-if="feature.toLowerCase().includes('vibration')" class="w-4 h-4 text-amber-400" />
            <Thermometer v-else-if="feature.toLowerCase().includes('temp')" class="w-4 h-4 text-red-400" />
            <Zap v-else-if="feature.toLowerCase().includes('volt') || feature.toLowerCase().includes('power')" class="w-4 h-4 text-blue-400" />
            <Settings v-else class="w-4 h-4 text-indigo-400" />
          </div>
          <div>
            <div class="text-[11px] font-black text-primary tracking-tight uppercase">{{ feature }}</div>
            <div class="text-[9px] font-bold text-secondary-alt uppercase tracking-widest mt-0.5">Contribution Analysis</div>
          </div>
        </div>
        <div class="text-right">
          <div class="text-xs font-black" :class="impact >= 0 ? 'text-emerald-400' : 'text-red-400'">
            {{ impact >= 0 ? '+' : '' }}{{ (impact * 100).toFixed(1) }}%
          </div>
          <div class="text-[8px] font-black text-secondary uppercase tracking-[0.2em] mt-0.5">Impact</div>
        </div>
      </div>
      
      <div class="relative h-1.5 w-full bg-slate-900 rounded-full overflow-hidden">
        <div 
          class="absolute h-full rounded-full transition-all duration-1000 ease-out" 
          :style="{ 
            width: `${Math.abs(impact) * 100}%`,
            background: impact >= 0 
              ? 'linear-gradient(90deg, #10b98120 0%, #10b981 100%)' 
              : 'linear-gradient(90deg, #ef444420 0%, #ef4444 100%)'
          }"
        ></div>
        <!-- Impact Glow -->
        <div 
          class="absolute h-full blur-[4px] opacity-30 transition-all duration-1000"
          :style="{ 
            width: `${Math.abs(impact) * 100}%`,
            background: impact >= 0 ? '#10b981' : '#ef4444'
          }"
        ></div>
      </div>
    </div>

    <div class="p-6 rounded-3xl bg-indigo-500/[0.03] border border-indigo-500/10 flex items-start gap-4 mt-8">
      <Info class="w-5 h-5 text-indigo-400 shrink-0 mt-0.5" />
      <p class="text-[11px] text-secondary leading-relaxed font-medium">
        Consensus weights derived from <span class="text-indigo-400">SHAP (SHapley Additive exPlanations)</span> values. 
        Factors in <span class="text-red-400 font-bold">RED</span> are accelerating degradation, while 
        <span class="text-emerald-400 font-bold">GREEN</span> factors indicate baseline operational health.
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { Activity, Thermometer, Zap, Settings, Info } from 'lucide-vue-next'

const props = defineProps<{
  contributions?: Record<string, number>
}>()

const hasContributions = computed(() => 
  props.contributions && Object.keys(props.contributions).length > 0
)

const sortedContributions = computed(() => {
  if (!props.contributions) return {}
  return Object.entries(props.contributions)
    .sort(([, a], [, b]) => Math.abs(b) - Math.abs(a)) // Sort by absolute impact
    .reduce((r, [k, v]) => ({ ...r, [k]: v }), {})
})
</script>

<style scoped>
.feature-row {
  transform: translateZ(0);
}

.feature-row:hover {
  background: rgba(255, 255, 255, 0.04);
}

@keyframes barGrow {
  from { width: 0; }
}

.feature-row div[style*="width"] {
  animation: barGrow 1.5s cubic-bezier(0.16, 1, 0.3, 1);
}
</style>
