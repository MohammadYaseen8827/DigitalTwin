<template>
  <div :class="styles['maintenance-container']">
    <!-- Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Wrench :width="14" :height="14" />
          <span>Execution Framework</span>
        </div>
        <h1 :class="styles['title']">Maintenance Lifecycle</h1>
        <p :class="styles['description']">Fleet-wide maintenance orchestration and certification board</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton variant="secondary" @click="loadActiveMaintenance">
          <RefreshCw :width="16" :height="16" />
          Synchronize Board
        </UiButton>
      </div>
    </header>

    <!-- Kanban Board -->
    <div :class="styles['kanban-board']">
      <!-- Planned -->
      <section :class="styles['board-column']">
        <div :class="styles['column-header']">
          <div :class="styles['column-title']">
            <div :class="[styles['dot'], styles['dot--planned']]" />
            <span>Planned Operations</span>
          </div>
          <UiBadge variant="default" size="sm">{{ plannedRecords.length }}</UiBadge>
        </div>
        
        <div :class="styles['column-content']">
          <UiCard 
            v-for="record in plannedRecords" 
            :key="record.id"
            variant="default"
            padding="md"
            hover
            :class="styles['job-card']"
          >
            <div :class="styles['job-header']">
              <UiBadge variant="primary" size="sm" outline>{{ record.type }}</UiBadge>
              <div :class="styles['job-date']">
                <Calendar :width="12" :height="12" />
                <span>{{ formatDate(record.plannedDate) }}</span>
              </div>
            </div>
            <p :class="styles['job-notes']">{{ record.notes }}</p>
            <div :class="styles['job-footer']">
              <UiButton variant="outline" size="sm" @click="startJobAction(record.id)" :class="styles['action-btn']">
                Initialize Job
              </UiButton>
            </div>
          </UiCard>

          <div v-if="plannedRecords.length === 0" :class="styles['empty-state']">
            <div :class="styles['empty-icon']"><Calendar :width="24" :height="24" /></div>
            <span>No jobs queued</span>
          </div>
        </div>
      </section>

      <!-- In Progress -->
      <section :class="styles['board-column']">
        <div :class="styles['column-header']">
          <div :class="styles['column-title']">
            <div :class="[styles['dot'], styles['dot--active'], styles['animate-pulse']]" />
            <span>Active Diagnostics</span>
          </div>
          <UiBadge variant="warning" size="sm">{{ inProgressRecords.length }}</UiBadge>
        </div>
        
        <div :class="styles['column-content']">
          <UiCard 
            v-for="record in inProgressRecords" 
            :key="record.id"
            variant="glass"
            padding="md"
            hover
            :class="[styles['job-card'], styles['job-card--active']]"
          >
            <div :class="styles['job-header']">
              <UiBadge variant="warning" size="sm">{{ record.type }}</UiBadge>
              <div :class="styles['job-date']">
                <Clock :width="12" :height="12" />
                <span>{{ formatTime(record.date) }}</span>
              </div>
            </div>
            <p :class="styles['job-notes']">{{ record.notes }}</p>
            <div :class="styles['job-footer']">
              <UiButton variant="primary" size="sm" @click="openCompleteModal(record)" :class="styles['action-btn']">
                Certify & Close
              </UiButton>
            </div>
          </UiCard>

          <div v-if="inProgressRecords.length === 0" :class="styles['empty-state']">
            <div :class="styles['empty-icon']"><Activity :width="24" :height="24" /></div>
            <span>No active repairs</span>
          </div>
        </div>
      </section>

      <!-- Completed -->
      <section :class="styles['board-column']">
        <div :class="styles['column-header']">
          <div :class="styles['column-title']">
            <CheckCircle2 :width="14" :height="14" :class="styles['text--success']" />
            <span>Verified Closed</span>
          </div>
          <UiBadge variant="success" size="sm">Last 5</UiBadge>
        </div>
        
        <div :class="styles['column-content']">
          <UiCard 
            v-for="record in completedRecords" 
            :key="record.id"
            variant="default"
            padding="md"
            hover
            :class="[styles['job-card'], styles['job-card--closed']]"
          >
            <div :class="styles['job-header']">
              <UiBadge variant="success" size="sm" outline>{{ record.type }}</UiBadge>
              <span :class="styles['job-date-text']">{{ formatDate(record.completionDate) }}</span>
            </div>
            <div :class="styles['technician-info']">
              <div :class="styles['tech-avatar']">{{ record.performedBy?.charAt(0) ?? 'T' }}</div>
              <div :class="styles['tech-meta']">
                <span :class="styles['tech-name']">{{ record.performedBy }}</span>
                <span :class="styles['tech-status']">Certified</span>
              </div>
            </div>
          </UiCard>

          <div v-if="completedRecords.length === 0" :class="styles['empty-state']">
            <div :class="styles['empty-icon']"><History :width="24" :height="24" /></div>
            <span>No archival history</span>
          </div>
        </div>
      </section>
    </div>

    <!-- Certification Modal -->
    <UiModal :is-open="isModalOpen" maxWidth="lg" @close="isModalOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-title-group']">
              <div :class="styles['modal-eyebrow']">CERTIFICATION PROTOCOL</div>
              <h2 :class="styles['modal-title']">Resolution Sign-off</h2>
              <p :class="styles['modal-sub']">Finalize technical resolution and certify asset state</p>
            </div>
          </div>
        </template>

        <div v-if="completingRecord" :class="styles['modal-body']">
          <div :class="styles['context-box']">
            <div :class="styles['context-row']">
              <span :class="styles['context-label']">Asset ID</span>
              <span :class="styles['context-val-mono']">{{ completingRecord.machineId || 'N/A' }}</span>
            </div>
            <div :class="styles['context-row']">
              <span :class="styles['context-label']">Task Type</span>
              <UiBadge variant="primary" size="sm">{{ completingRecord.type }}</UiBadge>
            </div>
          </div>

          <div :class="styles['form-stack']">
            <UiInput
              v-model="completionForm.performedBy"
              label="Authorized Technician"
              placeholder="Enter full credential name..."
              required
            >
              <template #prefix><UserCheck :width="14" :height="14" /></template>
            </UiInput>

            <UiInput
              v-model="completionForm.finalNotes"
              label="Resolution Narrative"
              placeholder="Describe technical actions and outcomes..."
              required
            >
              <template #prefix><ClipboardCheck :width="14" :height="14" /></template>
            </UiInput>
          </div>

          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="isModalOpen = false">Abort Certification</UiButton>
            <UiButton variant="primary" @click="handleComplete">Certify & Close Job</UiButton>
          </div>
        </div>
      </UiCard>
    </UiModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import { storeToRefs } from 'pinia'
import { useMaintenanceStore } from '@/stores/maintenance.store'
import { useToast } from '@/composables/useToast'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiModal from '@/components/ui/UiModal.vue'
import { 
  Wrench, 
  RefreshCw, 
  Calendar, 
  Activity, 
  CheckCircle2, 
  History, 
  ClipboardCheck, 
  UserCheck, 
  Clock,
  ShieldCheck,
  Cpu,
  Layers
} from 'lucide-vue-next'

const styles = useCssModule()
const maintenanceStore = useMaintenanceStore()
const { records } = storeToRefs(maintenanceStore)
const { loadActiveMaintenance, startJob, completeJob } = maintenanceStore
const toast = useToast()

const completingRecord = ref<any>(null)
const isModalOpen = ref(false)
const completionForm = ref({ performedBy: '', finalNotes: '' })

const plannedRecords = computed(() => records.value.filter(r => r.status === 'Planned'))
const inProgressRecords = computed(() => records.value.filter(r => r.status === 'InProgress'))
const completedRecords = computed(() => records.value.filter(r => r.status === 'Completed').slice(0, 5))

function formatDate(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function formatTime(dateString?: string) {
  if (!dateString) return 'N/A'
  return new Date(dateString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

async function startJobAction(id: string) {
  try {
    await startJob(id)
    toast.success('Maintenance job initialized')
    loadActiveMaintenance()
  } catch (err) {
    toast.error('Failed to start job')
  }
}

function openCompleteModal(record: any) {
  completingRecord.value = record
  completionForm.value = { performedBy: '', finalNotes: '' }
  isModalOpen.value = true
}

async function handleComplete() {
  if (!completingRecord.value) return
  if (!completionForm.value.performedBy) {
    toast.warning('Authorized technician name required')
    return
  }
  
  try {
    await completeJob(completingRecord.value.id, completionForm.value)
    isModalOpen.value = false
    loadActiveMaintenance()
    toast.success('Work certified and closed')
  } catch (err) {
    toast.error('Sync error during certification')
  }
}

onMounted(() => {
  loadActiveMaintenance()
})
</script>

<style module>
.maintenance-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  padding: 0;
  max-width: 1400px;
  margin: 0 auto;
}

/* Page Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-20);
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
  letter-spacing: 0.12em;
  color: var(--color-primary);
  margin-bottom: var(--space-6);
}

.title {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: -0.03em;
  margin: 0;
}

.description {
  color: var(--color-text-muted);
  font-size: var(--font-size-sm);
  margin-top: var(--space-4);
}

/* Kanban Board */
.kanban-board {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-24);
  align-items: start;
}

.board-column {
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  min-height: 500px;
}

.column-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 var(--space-4);
}

.column-title {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: -0.01em;
}

.dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
}

.dot--planned { background: var(--color-text-dim); }
.dot--active { background: var(--color-warning); box-shadow: 0 0 8px var(--color-warning); }

.animate-pulse {
  animation: pulse-simple 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}

@keyframes pulse-simple {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: .5; transform: scale(1.2); }
}

.column-content {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
  padding: var(--space-4);
}

/* Job Card */
.job-card {
  border-color: var(--color-border-subtle);
}

.job-card--active {
  border-color: var(--color-warning-muted);
}

.job-card--closed {
  opacity: 0.8;
}

.job-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-12);
}

.job-date {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-dim);
}

.job-notes {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
  line-height: var(--font-lineheight-relaxed);
  margin: 0;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.job-footer {
  margin-top: var(--space-16);
  padding-top: var(--space-12);
  border-top: 1px solid var(--color-border-subtle);
}

.action-btn {
  width: 100%;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-48) var(--space-16);
  gap: var(--space-12);
  color: var(--color-text-dim);
  font-size: var(--font-size-xs);
  font-weight: 600;
  background: rgba(255, 255, 255, 0.01);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-lg);
}

.empty-icon {
  opacity: 0.3;
}

.text--success { color: var(--color-success); }
.job-date-text { font-size: 10px; font-weight: 700; color: var(--color-text-dim); }

/* Technician Info */
.technician-info {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  margin-top: var(--space-16);
  padding-top: var(--space-12);
  border-top: 1px solid var(--color-border-subtle);
}

.tech-avatar {
  width: 24px;
  height: 24px;
  border-radius: var(--radius-full);
  background: var(--gradient-primary);
  color: white;
  font-size: 10px;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tech-meta {
  display: flex;
  flex-direction: column;
}

.tech-name {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.tech-status {
  font-size: 9px;
  color: var(--color-success);
  font-weight: 600;
  text-transform: uppercase;
}

/* Modal Improvements */
.modal-header {
  margin-bottom: var(--space-24);
}

.modal-eyebrow {
  font-size: 10px;
  font-weight: 800;
  color: var(--color-primary);
  letter-spacing: 0.2em;
  margin-bottom: var(--space-4);
}

.modal-title {
  font-size: var(--font-size-2xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
  letter-spacing: -0.02em;
}

.modal-sub {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin-top: var(--space-4);
}

.modal-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.form-stack {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.context-box {
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-16);
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.context-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.context-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-dim);
  letter-spacing: 0.05em;
}

.context-val-mono {
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 700;
  color: var(--color-primary);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
  padding-top: var(--space-24);
  border-top: 1px solid var(--color-border-subtle);
}

@media (max-width: 1024px) {
  .kanban-board { grid-template-columns: 1fr; }
  .board-column { min-height: auto; }
}
</style>
