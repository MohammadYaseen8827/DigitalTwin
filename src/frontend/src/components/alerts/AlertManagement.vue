<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { fetchActiveAlerts, acknowledgeAlert } from '@/services/alerts.service'
import { 
  Bell, CheckCircle, AlertTriangle, Info, Clock, Search, RefreshCw, Eye, 
  Activity, History, ShieldAlert, Terminal, Calendar, TrendingUp
} from 'lucide-vue-next'

const styles = useCssModule()
const toast = useToast()

const alerts = ref<any[]>([])
const loading = ref(false)
const searchQuery = ref('')
const severityFilter = ref('all')
const statusFilter = ref('all')

const filteredAlerts = computed(() => {
  let filtered = [...alerts.value]
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    filtered = filtered.filter(a => a.message?.toLowerCase().includes(q) || a.machineName?.toLowerCase().includes(q))
  }
  if (severityFilter.value !== 'all') filtered = filtered.filter(a => a.severity?.toLowerCase() === severityFilter.value)
  if (statusFilter.value !== 'all') {
    const isAck = statusFilter.value === 'acknowledged'
    filtered = filtered.filter(a => a.isAcknowledged === isAck)
  }
  return filtered
})

const kpiCards = computed(() => [
  { label: 'Active Anomalies', value: alerts.value.filter(a => !a.isAcknowledged).length, icon: ShieldAlert, variant: 'danger' },
  { label: 'System Risk Index', value: 'High', icon: TrendingUp, variant: 'warning' },
  { label: 'Mean Time to ACK', value: '14m', icon: Clock, variant: 'info' },
  { label: 'Resolved (24h)', value: alerts.value.filter(a => a.isAcknowledged).length, icon: CheckCircle, variant: 'success' },
])

const loadAlerts = async () => {
  try {
    loading.value = true
    alerts.value = await fetchActiveAlerts('')
  } catch { toast.error('Anomaly link failure') }
  finally { loading.value = false }
}

const handleAcknowledge = async (id: string) => {
  try {
    await acknowledgeAlert(id)
    toast.success('Integrity verified')
    await loadAlerts()
  } catch { toast.error('Verification failure') }
}

const getTimeAgo = (d?: string) => {
  if (!d) return 'Unknown'
  const diff = Date.now() - new Date(d).getTime()
  const mins = Math.floor(diff / 60000)
  if (mins < 1) return 'Just now'
  if (mins < 60) return `${mins}m ago`
  return `${Math.floor(mins / 60)}h ago`
}

onMounted(loadAlerts)
</script>

<template>
  <div :class="styles['alerts-container']">
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <ShieldAlert :width="14" :height="14" />
          <span>Threat Detection Network</span>
        </div>
        <h1 :class="styles['title']">Anomaly Center</h1>
        <p :class="styles['description']">Forensic stream analysis and neural threat mitigation</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton variant="secondary" @click="loadAlerts" :loading="loading">
          <RefreshCw :width="16" :height="16" />
          Resync
        </UiButton>
      </div>
    </header>

    <div :class="styles['kpi-grid']">
      <UiCard v-for="kpi in kpiCards" :key="kpi.label" variant="glass" padding="md" hover :class="styles['kpi-card']">
        <div :class="styles['kpi-inner']">
          <div :class="[styles['kpi-icon'], styles[`icon--${kpi.variant}`]]">
            <component :is="kpi.icon" :width="20" :height="20" />
          </div>
          <div :class="styles['kpi-data']">
            <span :class="styles['kpi-label']">{{ kpi.label }}</span>
            <div :class="styles['kpi-value']">{{ kpi.value }}</div>
          </div>
        </div>
      </UiCard>
    </div>

    <section :class="styles['filter-bar']">
      <UiCard variant="default" padding="sm">
        <div :class="styles['filter-row']">
          <div :class="styles['search-wrap']">
            <Search :class="styles['search-icon']" :width="14" :height="14" />
            <UiInput v-model="searchQuery" placeholder="Search forensic logs..." :class="styles['search-input']" />
          </div>
          <UiSelect v-model="severityFilter" :class="styles['filter-select']">
            <option value="all">ALL SEVERITIES</option>
            <option value="critical">CRITICAL</option>
            <option value="warning">WARNING</option>
          </UiSelect>
        </div>
      </UiCard>
    </section>

    <main :class="styles['feed']">
      <div v-if="loading" :class="styles['state-box']">
        <div :class="styles['spinner']" />
        <span>Intercepting Anomaly Stream...</span>
      </div>

      <div v-else-if="filteredAlerts.length === 0" :class="styles['state-box']">
        <CheckCircle :width="48" :height="48" :class="styles['state-icon-success']" />
        <h3>Neural Network Silent</h3>
        <p>No anomalies detected within the specified cognitive parameters.</p>
      </div>

      <div v-else :class="styles['feed-list']">
        <div 
          v-for="alert in filteredAlerts" 
          :key="alert.id" 
          :class="[styles['alert-item'], alert.severity === 'critical' && styles['alert-item--critical']]"
        >
          <div :class="[styles['severity-tag'], styles[`tag--${alert.severity?.toLowerCase()}`]]" />
          <div :class="styles['alert-body']">
            <div :class="styles['alert-header']">
              <div :class="styles['machine-ref']">
                <Terminal :width="12" :height="12" />
                <span>{{ alert.machineName }}</span>
              </div>
              <span :class="styles['timestamp']">{{ getTimeAgo(alert.createdAt) }}</span>
            </div>
            <h3 :class="styles['message']">{{ alert.message }}</h3>
            <div :class="styles['alert-footer']">
               <UiBadge :variant="alert.severity === 'critical' ? 'danger' : 'warning'" size="sm" dot>
                 {{ alert.severity }}
               </UiBadge>
               <div :class="styles['actions']">
                  <UiButton v-if="!alert.isAcknowledged" variant="primary" size="sm" @click="handleAcknowledge(alert.id)">
                    Verify Integrity
                  </UiButton>
                  <UiButton variant="ghost" size="sm"><Eye :width="14" :height="14" /></UiButton>
               </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script lang="ts">
export default { name: 'AlertManagement' }
</script>

<style module>
.alerts-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.eyebrow {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--color-primary);
  margin-bottom: var(--space-6);
}

.title { font-size: var(--font-size-3xl); font-weight: 800; letter-spacing: -0.03em; color: var(--color-text-primary); margin: 0; }
.description { font-size: var(--font-size-sm); color: var(--color-text-muted); }

.kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: var(--space-16); }
.kpi-inner { display: flex; align-items: center; gap: var(--space-16); }
.kpi-icon { width: 40px; height: 40px; border-radius: 10px; display: flex; align-items: center; justify-content: center; background: var(--color-depth-1); border: 1px solid var(--color-border); }
.icon--danger { color: var(--color-danger); }
.icon--warning { color: var(--color-warning); }
.icon--info { color: var(--color-info); }
.icon--success { color: var(--color-success); }

.kpi-label { font-size: 10px; font-weight: 700; text-transform: uppercase; color: var(--color-text-dim); }
.kpi-value { font-size: var(--font-size-xl); font-weight: 800; color: var(--color-text-primary); }

.filter-row { display: grid; grid-template-columns: 1fr 240px; gap: var(--space-16); align-items: center; }
.search-wrap { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: var(--space-12); color: var(--color-text-dim); z-index: 5; }
.search-input :global(.input) { padding-left: var(--space-32); }

.feed-list { display: flex; flex-direction: column; gap: var(--space-12); }
.alert-item {
  display: flex;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  transition: all var(--transition-normal);
}

.alert-item:hover { border-color: var(--color-border-hover); transform: translateX(4px); background: var(--color-surface-alt); }
.alert-item--critical { border-color: rgba(239,68,68,0.2); background: linear-gradient(90deg, rgba(239,68,68,0.03), transparent); }

.severity-tag { width: 4px; flex-shrink: 0; }
.tag--critical { background: var(--color-danger); box-shadow: 0 0 10px var(--color-danger-muted); }
.tag--warning { background: var(--color-warning); }

.alert-body { padding: var(--space-16) var(--space-20); flex: 1; display: flex; flex-direction: column; gap: var(--space-12); }
.alert-header { display: flex; justify-content: space-between; align-items: center; }
.machine-ref { display: flex; align-items: center; gap: var(--space-8); font-size: 10px; font-weight: 800; color: var(--color-text-dim); font-family: var(--font-mono); text-transform: uppercase; }
.timestamp { font-size: 10px; font-weight: 700; color: var(--color-text-dim); }

.message { font-size: var(--font-size-md); font-weight: 600; color: var(--color-text-primary); margin: 0; letter-spacing: -0.01em; }
.alert-footer { display: flex; justify-content: space-between; align-items: center; }
.actions { display: flex; gap: var(--space-8); }

.state-box { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: var(--space-64); gap: var(--space-16); color: var(--color-text-dim); text-align: center; }
.spinner { width: 24px; height: 24px; border: 2px solid var(--color-border); border-top-color: var(--color-primary); border-radius: 50%; animation: spin 0.8s linear infinite; }
.state-icon-success { color: var(--color-success); opacity: 0.5; }

@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1024px) { .kpi-grid { grid-template-columns: repeat(2, 1fr); } .filter-row { grid-template-columns: 1fr; } }
@media (max-width: 768px) { .page-header { flex-direction: column; align-items: flex-start; gap: var(--space-20); } .kpi-grid { grid-template-columns: 1fr; } }
</style>
