<script setup lang="ts">
import { ref, computed, useCssModule } from 'vue'
import { useToast } from '@/composables/useToast'
import { format } from 'date-fns'
import axiosClient from '@/api/axiosClient'
import type { MachineDto, EquipmentStatus } from '@/api/types'
import { Cpu, Settings, ChevronDown, ChevronUp, MapPin, Clock, Activity, Box, Zap, Shield, Microscope } from 'lucide-vue-next'
import UiButton from './ui/UiButton.vue'
import UiBadge from './ui/UiBadge.vue'
import UiCard from './ui/UiCard.vue'

const props = defineProps<{ machine: MachineDto }>()

const emit = defineEmits<{
  'view-details': [string]
  'simulation-updated': []
  'start-simulation': [string]
  'stop-simulation': [string]
}>()

const styles = useCssModule()
const toast = useToast()
const showScheduler = ref(false)
const scheduleInterval = ref(60)
const scheduleDuration = ref(120)

/* ---- computed ---- */
const healthScore = computed(() => {
  const s = normalizeStatus(props.machine.status)
  if (s === 'critical') return 18
  if (s === 'warning') return 47
  const rul = props.machine.remainingUsefulLifeDays ?? 0
  if (rul < 30) return 28
  if (rul < 90) return 62
  return 94
})

const healthColor = computed(() => {
  const h = healthScore.value
  if (h < 30) return 'var(--color-danger)'
  if (h < 65) return 'var(--color-warning)'
  return 'var(--color-success)'
})

const rulDays = computed(() => props.machine.remainingUsefulLifeDays ?? null)

const lastMaint = computed(() => {
  const d = props.machine.lastMaintenance
  if (!d) return '—'
  try { return format(new Date(d), 'MMM d') } catch { return '—' }
})

/* ---- helpers ---- */
function normalizeStatus(s: EquipmentStatus | undefined): string {
  if (!s && s !== 0) return 'unknown'
  if (typeof s === 'number') return 'unknown'
  return s.toLowerCase()
}

async function saveSchedule() {
  try {
    const start = new Date().toISOString()
    const end = new Date(Date.now() + scheduleDuration.value * 60000).toISOString()
    await axiosClient.post(`/SimulationScheduler/schedule/${props.machine.id}`, {
      machineName: props.machine.name,
      intervalSeconds: scheduleInterval.value,
      startTime: start,
      endTime: end,
      isActive: true,
      parameters: { degradationModel: 'wiener' }
    })
    showScheduler.value = false
    toast.success('Simulation parameters committed.')
    emit('simulation-updated')
  } catch {
    toast.error('Protocol sync failure.')
  }
}
</script>

<template>
  <UiCard variant="default" padding="none" hover :class="styles['machine-card']">
    <!-- Visual Indicator Line -->
    <div :class="[styles['status-indicator'], styles[`status-indicator--${normalizeStatus(machine.status)}`]]" />
    
    <!-- Pulse Effect on Hover -->
    <div :class="styles['card-glow']" :style="{ '--glow-color': healthColor }" />

    <div :class="styles['card-content']">
      <!-- Top Section -->
      <div :class="styles['card-header']">
        <div :class="styles['machine-id-block']">
          <div :class="styles['icon-container']">
            <Box :width="18" :height="18" />
          </div>
          <div :class="styles['title-group']">
            <h3 :class="styles['machine-name']">{{ machine.name }}</h3>
            <span :class="styles['machine-type']">{{ machine.type }}</span>
          </div>
        </div>
        <UiBadge :variant="normalizeStatus(machine.status) as any" size="sm" dot>
          {{ normalizeStatus(machine.status) }}
        </UiBadge>
      </div>

      <!-- Health Intelligence -->
      <div :class="styles['health-intelligence']">
        <div :class="styles['health-header']">
          <div :class="styles['score-wrap']" :style="{ color: healthColor }">
            <span :class="styles['score-num']">{{ healthScore }}</span>
            <span :class="styles['score-unit']">%</span>
          </div>
          <div :class="styles['health-label-wrap']">
             <span :class="styles['health-label']">Core Integrity</span>
             <span :class="styles['health-desc']">Neural Assessment</span>
          </div>
        </div>
        
        <div :class="styles['health-meter']">
          <div :class="styles['meter-track']">
            <div 
              :class="styles['meter-fill']" 
              :style="{ width: healthScore + '%', backgroundColor: healthColor }"
            >
              <div :class="styles['meter-glow']" :style="{ backgroundColor: healthColor }" />
            </div>
          </div>
        </div>
      </div>

      <!-- Data Matrix -->
      <div :class="styles['data-matrix']">
        <div :class="styles['matrix-item']">
          <span :class="styles['matrix-label']">Predictive RUL</span>
          <div :class="styles['matrix-value-row']">
            <Clock :width="12" :height="12" :class="styles['matrix-icon']" />
            <span :class="styles['matrix-val']">{{ rulDays ?? '—' }}</span>
            <span :class="styles['matrix-unit']">Days</span>
          </div>
        </div>
        <div :class="styles['matrix-item']">
          <span :class="styles['matrix-label']">Last Sync</span>
          <div :class="styles['matrix-value-row']">
            <Activity :width="12" :height="12" :class="styles['matrix-icon']" />
            <span :class="styles['matrix-val']">{{ lastMaint }}</span>
          </div>
        </div>
      </div>

      <!-- Action Footer -->
      <div :class="styles['card-footer']">
        <div :class="styles['location-wrap']">
          <MapPin :width="12" :height="12" />
          <span>{{ machine.location || 'Distributed' }}</span>
        </div>
        <div :class="styles['action-group']">
          <UiButton variant="ghost" size="sm" @click="showScheduler = !showScheduler" :class="styles['settings-btn']">
            <Settings :width="14" :height="14" />
          </UiButton>
          <UiButton variant="primary" size="sm" @click="emit('view-details', machine.id)">
            Initialize Forensic
          </UiButton>
        </div>
      </div>
    </div>

    <!-- Expansion Layer (Scheduler) -->
    <Transition name="expand">
      <div v-if="showScheduler" :class="styles['expansion-panel']">
        <div :class="styles['expansion-inner']">
          <div :class="styles['form-compact']">
            <div :class="styles['input-field']">
               <label>Frequency (s)</label>
               <input v-model.number="scheduleInterval" type="number" />
            </div>
            <div :class="styles['input-field']">
               <label>Epochs (m)</label>
               <input v-model.number="scheduleDuration" type="number" />
            </div>
          </div>
          <UiButton variant="secondary" size="sm" @click="saveSchedule" style="width: 100%;">
            Commit Neural Model
          </UiButton>
        </div>
      </div>
    </Transition>
  </UiCard>
</template>

<style module>
.machine-card {
  position: relative;
  overflow: hidden;
}

.status-indicator {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 3px;
  z-index: 5;
}

.status-indicator--operational { background: var(--color-success); }
.status-indicator--warning { background: var(--color-warning); }
.status-indicator--critical { background: var(--color-danger); }
.status-indicator--maintenance { background: var(--color-info); }

.card-glow {
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, var(--glow-color), transparent 70%);
  opacity: 0;
  transition: opacity var(--transition-normal);
  pointer-events: none;
}

.machine-card:hover .card-glow {
  opacity: 0.04;
}

.card-content {
  padding: var(--space-24);
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.machine-id-block {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.icon-container {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-lg);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  transition: all var(--transition-fast);
}

.machine-card:hover .icon-container {
  border-color: var(--color-primary-glow);
  box-shadow: var(--glow-sm);
  transform: scale(1.05);
}

.machine-name {
  margin: 0;
  font-size: var(--font-size-md);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.machine-type {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

/* Health Intelligence */
.health-intelligence {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.health-header {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.score-wrap {
  display: flex;
  align-items: baseline;
  line-height: 1;
}

.score-num { font-size: var(--font-size-3xl); font-weight: 800; font-family: var(--font-mono); }
.score-unit { font-size: var(--font-size-sm); font-weight: 600; opacity: 0.6; margin-left: 2px; }

.health-label-wrap {
  display: flex;
  flex-direction: column;
}

.health-label { font-size: 11px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.05em; color: var(--color-text-secondary); }
.health-desc { font-size: 10px; color: var(--color-text-dim); font-weight: 600; }

.health-meter {
  height: 6px;
  position: relative;
}

.meter-track {
  height: 100%;
  background: var(--color-depth-1);
  border-radius: var(--radius-full);
  overflow: hidden;
  border: 1px solid var(--color-border-subtle);
}

.meter-fill {
  height: 100%;
  border-radius: inherit;
  position: relative;
  transition: width 1.2s var(--ease-premium);
}

.meter-glow {
  position: absolute;
  top: 0;
  right: 0;
  width: 40px;
  height: 100%;
  filter: blur(8px);
  opacity: 0.5;
}

/* Data Matrix */
.data-matrix {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-12);
}

.matrix-item {
  padding: var(--space-12);
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.matrix-label { font-size: 9px; font-weight: 700; text-transform: uppercase; color: var(--color-text-dim); letter-spacing: 0.05em; }
.matrix-value-row { display: flex; align-items: center; gap: var(--space-6); }
.matrix-icon { color: var(--color-primary); opacity: 0.7; }
.matrix-val { font-size: var(--font-size-sm); font-weight: 700; color: var(--color-text-secondary); }
.matrix-unit { font-size: 10px; color: var(--color-text-dim); font-weight: 600; }

/* Footer */
.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
}

.location-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  font-weight: 600;
}

.action-group {
  display: flex;
  gap: var(--space-8);
}

.settings-btn {
  color: var(--color-text-dim);
}

/* Expansion Panel */
.expansion-panel {
  background: var(--color-depth-1);
  border-top: 1px solid var(--color-border-subtle);
}

.expansion-inner {
  padding: var(--space-16) var(--space-24);
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
}

.form-compact {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-12);
}

.input-field {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.input-field label { font-size: 9px; font-weight: 700; text-transform: uppercase; color: var(--color-text-dim); }
.input-field input {
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: var(--space-8) var(--space-10);
  color: var(--color-text-primary);
  font-family: var(--font-mono);
  font-size: var(--font-size-xs);
  outline: none;
  transition: border-color var(--transition-fast);
}

.input-field input:focus { border-color: var(--color-primary); }

@keyframes expand {
  from { max-height: 0; opacity: 0; }
  to { max-height: 200px; opacity: 1; }
}

.expand-enter-active { animation: expand 0.3s var(--ease-premium); }
.expand-leave-active { animation: expand 0.3s var(--ease-premium) reverse; }
</style>
