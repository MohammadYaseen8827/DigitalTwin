<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import PageHeader from '@/components/layout/PageHeader.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import MetricValue from '@/components/base/MetricValue.vue'
import { useMachinesStore } from '@/stores/machines'
import { useAlertsStore } from '@/stores/alerts'
import { usePredictionsStore } from '@/stores/predictions'
import { 
  ArrowLeft, Cpu, Activity, Clock, ShieldAlert, Settings, Info, 
  Calendar, MapPin, Tool, Zap, Microscope, TrendingUp 
} from 'lucide-vue-next'

const styles = useCssModule()
const route = useRoute()
const router = useRouter()
const machinesStore = useMachinesStore()
const alertsStore = useAlertsStore()
const predictionsStore = usePredictionsStore()

const machineId = route.params.id as string
const activeTab = ref<'overview' | 'telemetry' | 'predictions' | 'maintenance'>('overview')

const isLoading = computed(() => machinesStore.isLoading)
const machine = computed(() => machinesStore.machines.find(m => m.id === machineId))
const machineAlerts = computed(() => alertsStore.alerts.filter(a => a.machineId === machineId))
const rulPrediction = computed(() => predictionsStore.rulPredictions[machineId])

const tabs = [
  { id: 'overview', name: 'Neural Overview', icon: Info },
  { id: 'telemetry', name: 'Telemetry Stream', icon: Activity },
  { id: 'predictions', name: 'AI Forensic', icon: Microscope },
  { id: 'maintenance', name: 'Protocol History', icon: History }
]

onMounted(async () => {
    if (!machine.value) await machinesStore.fetchMachineById(machineId)
    await Promise.all([
        alertsStore.fetchAlerts(machineId),
        predictionsStore.fetchRULPrediction(machineId)
    ])
})
</script>

<template>
  <div :class="styles['detail-container']">
    <UiButton variant="ghost" size="sm" :class="styles['back-btn']" @click="router.back()">
      <ArrowLeft :width="14" :height="14" />
      Back to Fleet Hub
    </UiButton>

    <PageHeader
      v-if="machine"
      :title="machine.name"
      :eyebrow="`NEURAL NODE ID: ${machineId.slice(0, 12).toUpperCase()}`"
      :description="`Deep analytical stream for ${machine.type} deployed at ${machine.location || 'Distributed Node'}.`"
    >
      <template #actions>
        <UiBadge :variant="String(machine.status).toLowerCase() as any" size="md" dot>
          {{ String(machine.status).toUpperCase() }}
        </UiBadge>
        <UiButton variant="outline" size="sm">
          <Settings :width="14" :height="14" />
          Configure Twin
        </UiButton>
      </template>
    </PageHeader>

    <div v-if="isLoading" :class="styles['skeleton-stage']">
      <BaseSkeleton width="100%" height="180px" radius="20px" />
      <div :class="styles['skeleton-grid']">
        <BaseSkeleton v-for="i in 3" :key="i" height="140px" radius="20px" />
      </div>
    </div>

    <div v-else-if="!machine" :class="styles['empty-stage']">
      <Cpu :width="64" :height="64" :class="styles['empty-icon']" />
      <h3>Neural Link Severed</h3>
      <p>The requested digital twin asset could not be localized in the active cluster registry.</p>
      <UiButton variant="primary" @click="router.push('/machines')">Return to Fleet</UiButton>
    </div>

    <div v-else :class="styles['content']">
      <!-- High-Tech Stats Bento -->
      <div :class="styles['stats-bento']">
        <UiCard variant="glass" padding="lg" hover :class="styles['stat-card']">
          <div :class="styles['stat-inner']">
            <div :class="styles['stat-header']">
              <div :class="styles['stat-icon-box']"><Activity :width="16" :height="16" /></div>
              <span :class="styles['stat-label']">Integrity Index</span>
            </div>
            <div :class="styles['health-display']">
               <div :class="styles['health-val']">{{ machine.healthScore }}<span :class="styles['unit']">%</span></div>
               <div :class="styles['health-bar-container']">
                 <div :class="styles['health-bar-fill']" :style="{ width: machine.healthScore + '%' }" />
               </div>
            </div>
          </div>
        </UiCard>

        <UiCard variant="glass" padding="lg" hover :class="styles['stat-card']">
          <div :class="styles['stat-inner']">
            <div :class="styles['stat-header']">
              <div :class="[styles['stat-icon-box'], styles['icon--blue']]"><Zap :width="16" :height="16" /></div>
              <span :class="styles['stat-label']">Predictive Horizon</span>
            </div>
            <div v-if="rulPrediction" :class="styles['prediction-display']">
               <div :class="styles['prediction-val']">{{ rulPrediction.currentRUL }}<span :class="styles['unit']">HRS</span></div>
               <div :class="styles['prediction-meta']">
                 <TrendingUp :width="12" :height="12" />
                 <span>Stable trajectory</span>
               </div>
            </div>
            <div v-else :class="styles['empty-data']">Awaiting AI Sync...</div>
          </div>
        </UiCard>

        <UiCard variant="glass" padding="lg" hover :class="styles['stat-card']">
          <div :class="styles['stat-inner']">
            <div :class="styles['stat-header']">
              <div :class="[styles['stat-icon-box'], styles['icon--amber']]"><ShieldAlert :width="16" :height="16" /></div>
              <span :class="styles['stat-label']">Anomaly Risk</span>
            </div>
            <div :class="styles['risk-display']">
               <div :class="styles['risk-level']">LOW</div>
               <div :class="styles['risk-details']">0 Pending Crit Alerts</div>
            </div>
          </div>
        </UiCard>
      </div>

      <!-- Main Interaction Depth -->
      <div :class="styles['interaction-zone']">
        <nav :class="styles['tab-strip']">
          <button
            v-for="tab in tabs"
            :key="tab.id"
            :class="[styles['tab-btn'], activeTab === tab.id && styles['tab-btn--active']]"
            @click="activeTab = (tab.id as any)"
          >
            <component :is="tab.icon" :width="14" :height="14" />
            <span>{{ tab.name }}</span>
          </button>
        </nav>

        <div :class="styles['viewport']">
          <Transition name="fade-fast" mode="out-in">
            <div v-if="activeTab === 'overview'" :class="styles['overview-grid']">
              <UiCard variant="default" padding="lg" hover :class="styles['alerts-panel']">
                <template #header>
                  <div :class="styles['panel-header']">
                    <h3>Anomaly Forensic Feed</h3>
                    <UiBadge v-if="machineAlerts.length" variant="danger" size="sm">{{ machineAlerts.length }}</UiBadge>
                  </div>
                </template>
                
                <div v-if="machineAlerts.length === 0" :class="styles['empty-panel']">
                  <div :class="styles['panel-icon']"><ShieldAlert :width="32" :height="32" /></div>
                  <p>System operating within optimal neural parameters.</p>
                </div>
                
                <div v-else :class="styles['alerts-stack']">
                  <div 
                    v-for="alert in machineAlerts" 
                    :key="alert.id"
                    :class="[styles['alert-item'], styles[`alert--${alert.severity}`]]"
                  >
                    <div :class="styles['alert-head']">
                      <span :class="styles['alert-title']">{{ alert.title }}</span>
                      <span :class="styles['alert-time']">{{ getTimeAgo(alert.timestamp) }}</span>
                    </div>
                    <p :class="styles['alert-desc']">{{ alert.description }}</p>
                  </div>
                </div>
              </UiCard>

              <UiCard variant="default" padding="lg" hover :class="styles['specs-panel']">
                <template #header>
                  <h3 :class="styles['panel-title']">Technical Architecture</h3>
                </template>
                <div v-if="machine.specifications" :class="styles['specs-list']">
                  <div v-for="(val, key) in machine.specifications" :key="key" :class="styles['spec-row']">
                    <span :class="styles['spec-key']">{{ key }}</span>
                    <span :class="styles['spec-val']">{{ val }}</span>
                  </div>
                </div>
                <div v-else :class="styles['empty-panel']">No hardware signature detected.</div>
              </UiCard>
            </div>

            <div v-else :class="styles['placeholder-stage']">
               <UiCard variant="glass" padding="xl" :class="styles['placeholder-card']">
                 <div :class="styles['spinner-glow']" />
                 <h3>Establishing Secure Stream</h3>
                 <p>Connecting to neural cluster for real-time {{ activeTab }} ingestion...</p>
               </UiCard>
            </div>
          </Transition>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
export default { name: 'MachineDetail' }
function getTimeAgo(d?: string) {
  if (!d) return 'SYNC'
  const diff = Date.now() - new Date(d).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'JUST NOW'
  if (mins < 60) return `${mins}M AGO`
  return `${Math.floor(mins / 60)}H AGO`
}
</script>

<style module>
.detail-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.back-btn {
  align-self: flex-start;
  color: var(--color-text-dim);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.content {
  display: flex;
  flex-direction: column;
  gap: var(--space-40);
}

/* Bento Stats */
.stats-bento {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-20);
}

.stat-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.stat-header {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.stat-icon-box {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
}

.icon--blue { color: var(--color-telemetry); }
.icon--amber { color: var(--color-warning); }

.stat-label {
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--color-text-dim);
}

.health-display, .prediction-display, .risk-display {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.health-val, .prediction-val, .risk-level {
  font-size: var(--font-size-4xl);
  font-weight: 800;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
  line-height: 1;
  letter-spacing: -0.04em;
}

.unit { font-size: var(--font-size-lg); color: var(--color-text-dim); margin-left: 4px; }

.health-bar-container {
  height: 4px;
  background: var(--color-depth-1);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.health-bar-fill {
  height: 100%;
  background: var(--color-primary);
  box-shadow: 0 0 10px var(--color-primary-glow);
  transition: width 1.5s var(--ease-premium);
}

.prediction-meta, .risk-details {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-dim);
}

/* Interaction Zone */
.interaction-zone {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.tab-strip {
  display: flex;
  gap: var(--space-4);
  background: var(--color-depth-1);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-subtle);
  width: fit-content;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  padding: var(--space-8) var(--space-20);
  border-radius: var(--radius-md);
  border: none;
  background: transparent;
  color: var(--color-text-dim);
  font-size: var(--font-size-xs);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition-fast);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.tab-btn:hover { color: var(--color-text-primary); }
.tab-btn--active {
  background: var(--color-surface-elevated);
  color: var(--color-primary);
  box-shadow: var(--shadow-sm);
}

.overview-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-24);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.panel-header h3 { margin: 0; font-size: var(--font-size-md); font-weight: 700; color: var(--color-text-primary); }

.alerts-stack { display: flex; flex-direction: column; gap: var(--space-12); }
.alert-item {
  padding: var(--space-16);
  background: var(--color-depth-1);
  border-radius: var(--radius-md);
  border-left: 3px solid transparent;
}
.alert--critical { border-left-color: var(--color-danger); background: color-mix(in srgb, var(--color-danger) 5%, var(--color-depth-1)); }
.alert--warning { border-left-color: var(--color-warning); }

.alert-head { display: flex; justify-content: space-between; margin-bottom: var(--space-4); }
.alert-title { font-weight: 700; color: var(--color-text-primary); font-size: var(--font-size-sm); }
.alert-time { font-size: 9px; font-weight: 800; color: var(--color-text-dim); font-family: var(--font-mono); }
.alert-desc { font-size: var(--font-size-xs); color: var(--color-text-secondary); margin: 0; line-height: 1.5; }

.specs-list { display: flex; flex-direction: column; }
.spec-row {
  display: flex;
  justify-content: space-between;
  padding: var(--space-12) 0;
  border-bottom: 1px solid var(--color-border-subtle);
}
.spec-key { font-size: 10px; font-weight: 800; text-transform: uppercase; color: var(--color-text-dim); }
.spec-val { font-size: var(--font-size-xs); font-weight: 700; color: var(--color-text-primary); font-family: var(--font-mono); }

.placeholder-stage { min-height: 400px; display: flex; }
.placeholder-card { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: var(--space-16); text-align: center; }

.spinner-glow {
  width: 48px;
  height: 48px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
  box-shadow: 0 0 20px var(--color-primary-glow);
}

@keyframes spin { to { transform: rotate(360deg); } }

.empty-stage {
  padding: var(--space-64);
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-16);
  background: var(--color-depth-1);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-2xl);
}

.empty-icon { opacity: 0.3; }

@media (max-width: 1200px) {
  .stats-bento { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 768px) {
  .stats-bento { grid-template-columns: 1fr; }
  .overview-grid { grid-template-columns: 1fr; }
  .tab-strip { width: 100%; overflow-x: auto; }
}
</style>
