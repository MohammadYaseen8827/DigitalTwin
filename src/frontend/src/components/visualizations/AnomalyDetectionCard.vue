<template>
  <BaseCard class="anomaly-detection-card overflow-hidden" variant="glass">
    <div class="p-8">
      <header class="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-6 mb-10">
        <div class="flex items-center gap-4">
          <div class="p-3 bg-amber-500/20 rounded-2xl text-amber-500 shadow-lg shadow-amber-500/10">
            <Search class="w-6 h-6" />
          </div>
          <div>
            <h3 class="text-xl font-black tracking-tight text-primary">Anomaly Intelligence</h3>
            <p class="text-xs font-bold text-secondary uppercase tracking-widest mt-0.5">Automated Pattern Discovery</p>
          </div>
        </div>
        
        <div class="flex items-center gap-3">
          <div class="flex bg-white/5 p-1 rounded-xl border border-white/10">
            <button 
              v-for="range in ranges" 
              :key="range.label"
              @click="setTimeRange(range.value)"
              class="px-3 py-1.5 text-[10px] font-black uppercase tracking-widest rounded-lg transition-all"
              :class="activeRange === range.value ? 'bg-indigo-500 text-white shadow-lg' : 'text-secondary hover:text-primary'"
            >
              {{ range.label }}
            </button>
          </div>
          <BaseButton 
            variant="primary" 
            size="sm" 
            :loading="loading" 
            class="shadow-lg shadow-indigo-500/20 px-6 h-10 font-bold uppercase tracking-widest text-[10px]"
            @click="detectAnomaliesAction"
          >
            Run Scan
          </BaseButton>
        </div>
      </header>

      <div class="grid grid-cols-1 xl:grid-cols-12 gap-10">
        <!-- Risk & Density Column -->
        <div class="xl:col-span-4 space-y-8">
          <div class="risk-radial-container flex flex-col items-center justify-center p-8 bg-white/5 rounded-3xl border border-white/10 relative overflow-hidden group">
            <div class="absolute inset-0 bg-gradient-to-br from-amber-500/5 to-transparent pointer-events-none"></div>
            
            <div class="relative w-40 h-40 flex items-center justify-center mb-6">
              <svg class="absolute inset-0 w-full h-full -rotate-90">
                <circle cx="80" cy="80" r="74" fill="none" stroke="rgba(255,255,255,0.05)" stroke-width="12" />
                <circle cx="80" cy="80" r="74" fill="none" 
                        :stroke="riskColor" 
                        stroke-width="12" 
                        stroke-dasharray="464.9" 
                        :stroke-dashoffset="464.9 * (1 - (anomalyResult?.overallRiskScore ?? 0) / 100)"
                        stroke-linecap="round"
                        class="transition-all duration-[2000ms] ease-out" />
              </svg>
              <div class="text-center z-10">
                <div class="text-4xl font-black tracking-tighter" :style="{ color: riskColor }">
                  {{ anomalyResult?.overallRiskScore?.toFixed(0) ?? '--' }}
                </div>
                <div class="text-[9px] uppercase font-black text-secondary tracking-[0.2em] mt-1">Risk Score</div>
              </div>
            </div>
            
            <p class="text-sm text-center text-secondary leading-relaxed px-4">
              {{ riskDescription }}
            </p>
          </div>

          <!-- Anomaly Density Strip -->
          <div class="p-6 bg-white/5 rounded-3xl border border-white/10">
            <div class="flex justify-between items-center mb-4">
              <h4 class="text-[10px] font-black uppercase tracking-widest text-secondary">Temporal Density</h4>
              <span class="text-[10px] font-bold text-amber-400">{{ anomalyResult?.anomalies.length ?? 0 }} spikes</span>
            </div>
            <div class="density-strip h-12 flex gap-1 items-end py-2">
              <div 
                v-for="(val, idx) in temporalDensity" 
                :key="idx"
                class="flex-1 rounded-t-sm transition-all duration-[800ms] ease-out"
                :class="val > 0 ? 'bg-amber-500 shadow-[0_0_8px_rgba(245,158,11,0.3)]' : 'bg-white/5'"
                :style="{ 
                  height: val > 0 ? `${Math.max(20, (val / maxDensity) * 100)}%` : '20%',
                  opacity: val > 0 ? 0.4 + (val / maxDensity) * 0.6 : 1
                }"
              ></div>
            </div>
            <div class="flex justify-between mt-2 text-[8px] font-bold text-secondary-alt uppercase tracking-tighter">
              <span>Start</span>
              <span>Timeline</span>
              <span>Now</span>
            </div>
          </div>
        </div>

        <!-- Anomaly Feed Column -->
        <div class="xl:col-span-8 space-y-6">
          <div v-if="loading" class="space-y-4 py-10">
            <div class="flex flex-col items-center justify-center gap-4">
              <div class="w-12 h-12 border-4 border-indigo-500/20 border-t-indigo-500 rounded-full animate-spin"></div>
              <p class="text-sm font-bold text-secondary animate-pulse uppercase tracking-widest">Neural Pattern Scan In Progress...</p>
            </div>
          </div>
          
          <div v-else-if="!anomalyResult" class="flex flex-col items-center justify-center py-20 text-center space-y-6 bg-white/5 rounded-3xl border border-dashed border-white/10">
            <div class="p-6 bg-white/5 rounded-full">
              <ScanSearch class="w-12 h-12 text-secondary opacity-30" />
            </div>
            <div>
              <h4 class="text-lg font-bold">Awaiting Scan</h4>
              <p class="text-sm text-secondary max-w-xs mx-auto mt-2">Historical telemetry analysis will identify hidden failure patterns and sensor drifts.</p>
            </div>
          </div>

          <div v-else class="space-y-4">
            <div class="flex justify-between items-end mb-4 px-2">
              <h4 class="text-[10px] font-black uppercase tracking-[0.2em] text-secondary">Detected Sensor Spikes</h4>
              <BaseButton variant="ghost" size="sm" class="text-[10px] font-bold uppercase tracking-widest text-indigo-400 h-6">Clear All</BaseButton>
            </div>
            
            <div class="anomaly-scroll-area max-h-[500px] overflow-y-auto pr-2 space-y-3 custom-scrollbar">
              <div 
                v-for="(anomaly, index) in anomalyResult.anomalies" 
                :key="index"
                class="anomaly-card p-5 rounded-2xl border border-white/5 bg-white/5 hover:bg-white/[0.08] transition-all cursor-pointer group"
                :class="`border-l-4 border-l-${getSeverityColor(anomaly.severity)}`"
              >
                <div class="flex justify-between items-start mb-3">
                  <div class="flex items-center gap-3">
                    <div class="p-2 rounded-lg bg-white/5">
                      <Zap class="w-4 h-4" :style="{ color: getSeverityHex(anomaly.severity) }" />
                    </div>
                    <div>
                      <div class="text-sm font-black text-primary">{{ anomaly.metric }}</div>
                      <div class="text-[10px] font-bold text-secondary uppercase tracking-tight">{{ formatTimestamp(anomaly.timestamp) }}</div>
                    </div>
                  </div>
                  <BaseBadge :style="{ backgroundColor: getSeverityHex(anomaly.severity) + '20', color: getSeverityHex(anomaly.severity) }" class="text-[9px] font-black uppercase">
                    {{ anomaly.severity }}
                  </BaseBadge>
                </div>
                
                <div class="flex items-center gap-6 mt-4">
                  <div class="flex-1 overflow-hidden">
                    <div class="text-[9px] uppercase font-bold text-secondary mb-1">Deviation Value</div>
                    <div class="text-lg font-mono font-black text-primary">{{ anomaly.value.toFixed(2) }}</div>
                  </div>
                  <div class="flex-1 overflow-hidden">
                    <div class="text-[9px] uppercase font-bold text-secondary mb-1">Confidence Range</div>
                    <div class="text-xs font-mono font-bold text-secondary">{{ anomaly.expectedRange.min.toFixed(1) }} - {{ anomaly.expectedRange.max.toFixed(1) }}</div>
                  </div>
                  <BaseButton variant="outline" size="sm" class="h-8 w-8 p-0 hover:bg-indigo-500/20 rounded-lg shrink-0">
                    <History class="w-3.5 h-3.5" />
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Logic Explanation Footer -->
      <footer class="mt-12 pt-8 border-t border-white/5 grid grid-cols-1 md:grid-cols-3 gap-6">
        <div v-for="method in methods" :key="method.title" class="flex gap-4 p-4 rounded-2xl bg-white/[0.02] border border-white/5 group hover:border-indigo-500/20 transition-all">
          <div class="text-xl group-hover:scale-110 transition-transform">{{ method.icon }}</div>
          <div>
            <h5 class="text-xs font-black uppercase tracking-widest text-primary mb-1">{{ method.title }}</h5>
            <p class="text-[10px] text-secondary leading-relaxed">{{ method.description }}</p>
          </div>
        </div>
      </footer>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { detectAnomalies } from '@/services/advancedAnalytics.service'
import type { AnomalyDetectionResult, AnomalyDetectionRequest } from '@/services/advancedAnalytics.service'
import { ScanSearch, AlertTriangle, CheckCircle, Search, Zap, History, Filter } from 'lucide-vue-next'
import BaseButton from '../base/BaseButton.vue'
import BaseCard from '../base/BaseCard.vue'
import BaseBadge from '../base/BaseBadge.vue'

const props = defineProps<{
  machineId: string
}>()

const loading = ref(false)
const anomalyResult = ref<AnomalyDetectionResult | null>(null)
const activeRange = ref(168) // 7 days in hours

const ranges = [
  { label: '24h', value: 24 },
  { label: '7d', value: 168 },
  { label: '30d', value: 720 },
]

const methods = [
  { icon: '📊', title: 'Statistical', description: 'Identifies outliers using 2.5-sigma dynamic thresholds.' },
  { icon: '🔗', title: 'Multivariate', description: 'Detects breakdown in sensor cross-correlations.' },
  { icon: '⚡', title: 'Change-Point', description: 'neural analysis of sudden distribution shifts.' },
]

const riskColor = computed(() => {
  if (!anomalyResult.value) return 'rgba(255,255,255,0.1)'
  const score = anomalyResult.value.overallRiskScore
  if (score >= 70) return '#ef4444'
  if (score >= 40) return '#f59e0b'
  return '#10b981'
})

const riskDescription = computed(() => {
  if (!anomalyResult.value) return 'No scan data available. Perform scan to analyze risk patterns.'
  const score = anomalyResult.value.overallRiskScore
  if (score >= 70) return 'Critical pattern deviations detected. High probability of impending component failure.'
  if (score >= 40) return 'Moderate anomalies observed. Increased monitoring or maintenance recommended.'
  return 'System behavior within nominal operational bounds.'
})

const getSeverityColor = (sev: string) => {
  switch (sev.toLowerCase()) {
    case 'critical': return 'red-500'
    case 'high': return 'amber-500'
    case 'medium': return 'amber-400'
    default: return 'emerald-500'
  }
}

const getSeverityHex = (sev: string) => {
  switch (sev.toLowerCase()) {
    case 'critical': return '#ef4444'
    case 'high': return '#f59e0b'
    case 'medium': return '#fbbf24'
    default: return '#10b981'
  }
}

const formatTimestamp = (ts: string) => {
  return new Date(ts).toLocaleString('en-US', {
    month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit'
  })
}

const detectAnomaliesAction = async () => {
  if (!props.machineId) return
  
  loading.value = true
  try {
    const start = new Date(Date.now() - activeRange.value * 60 * 60 * 1000).toISOString()
    const res = await detectAnomalies(props.machineId, { startTime: start })
    anomalyResult.value = res
  } catch (err) {
    console.error('Failed to detect anomalies:', err)
  } finally {
    loading.value = false
  }
}

const setTimeRange = (val: number) => {
  activeRange.value = val
  detectAnomaliesAction()
}

onMounted(() => {
  detectAnomaliesAction()
})
const temporalDensity = computed(() => {
  const buckets = Array(30).fill(0)
  if (!anomalyResult.value || anomalyResult.value.anomalies.length === 0) return buckets
  
  const now = Date.now()
  const start = now - activeRange.value * 60 * 60 * 1000
  const bucketSize = (now - start) / 30
  
  anomalyResult.value.anomalies.forEach(a => {
    const ts = new Date(a.timestamp).getTime()
    const idx = Math.floor((ts - start) / bucketSize)
    if (idx >= 0 && idx < 30) buckets[idx]++
  })
  
  return buckets
})

const maxDensity = computed(() => Math.max(1, ...temporalDensity.value))
</script>

<style scoped>
.anomaly-detection-card {
  border: 1px solid rgba(255, 255, 255, 0.05);
  background: linear-gradient(165deg, rgba(30, 41, 59, 0.2) 0%, rgba(15, 23, 42, 0) 100%);
}

.risk-radial-container {
  box-shadow: 0 12px 24px -10px rgba(0, 0, 0, 0.4);
}

.anomaly-card {
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.anomaly-card:hover {
  transform: translateX(4px);
}

.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.02);
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}
</style>

