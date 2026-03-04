<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiTable from '@/components/ui/UiTable.vue'
import UiSlideOver from '@/components/ui/UiSlideOver.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchMachines, 
  createMachine, 
  updateMachine, 
  deleteMachine 
} from '@/services/machines.service'
import type { MachineDto, MachineCreateDto } from '@/api/types'
import { 
  Plus, 
  Edit, 
  Trash2, 
  Activity,
  Search,
  RefreshCw,
  Server,
  MapPin,
  Cpu,
  Filter,
  Layers,
  Terminal,
  Eye,
  Info,
  ShieldAlert
} from 'lucide-vue-next'

const styles = useCssModule()
const toast = useToast()

// State
const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')
const typeFilter = ref('all')

// Modal state
const isModalOpen = ref(false)
const isPanelOpen = ref(false)
const selectedMachine = ref<MachineDto | null>(null)
const focusedMachine = ref<MachineDto | null>(null)

// Form state
const machineForm = ref<MachineCreateDto>({
  name: '', serialNumber: '', type: '', manufacturer: '', model: '',
  status: 'operational', criticality: 3, location: '', installationDate: '',
  warrantyExpiry: '', lastMaintenance: '', nextMaintenance: '',
  maintenanceInterval: 30, degradationModel: 'linear', threshold: 80,
  isActive: true, metadata: {}
})

// Computed
const filteredMachines = computed(() => {
  const currentMachines = Array.isArray(machines.value) ? machines.value : []
  let filtered = [...currentMachines]
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    filtered = filtered.filter(m => m.name.toLowerCase().includes(q) || m.serialNumber?.toLowerCase().includes(q) || m.location.toLowerCase().includes(q))
  }
  if (statusFilter.value !== 'all') filtered = filtered.filter(m => m.status.toString().toLowerCase() === statusFilter.value.toLowerCase())
  if (typeFilter.value !== 'all') filtered = filtered.filter(m => m.type === typeFilter.value)
  return filtered
})

const typeOptions = computed(() => {
  const currentMachines = Array.isArray(machines.value) ? machines.value : []
  return [
    { label: 'ALL NODE TYPES', value: 'all' },
    ...[...new Set(currentMachines.map(m => m.type))].sort().map(t => ({ label: t.toUpperCase(), value: t }))
  ]
})

const statusOptions = [
  { label: 'ALL STATUSES', value: 'all' },
  { label: 'OPERATIONAL', value: 'operational' },
  { label: 'WARNING', value: 'warning' },
  { label: 'CRITICAL', value: 'critical' },
  { label: 'MAINTENANCE', value: 'maintenance' }
]

const columns = [
  { key: 'status', label: 'Status', width: '120px', sortable: true },
  { key: 'name', label: 'Node Identifier', sortable: true },
  { key: 'type', label: 'Kinematic Class', sortable: true },
  { key: 'location', label: 'Deployment Point', sortable: true },
  { key: 'criticality', label: 'Risk', width: '80px', align: 'center' as const, sortable: true },
  { key: 'actions', label: '', width: '160px', align: 'right' as const }
]

// Methods
const loadMachines = async () => {
  try {
    loading.value = true
    const data = await fetchMachines()
    machines.value = Array.isArray(data) ? data : []
  } catch (err) {
    console.error('[MachineManagement] Failed to fetch machines:', err)
    toast.error('Cluster resolution failed')
    machines.value = []
  } finally {
    loading.value = false
  }
}

const openEditModal = (m: MachineDto) => {
  selectedMachine.value = m
  Object.assign(machineForm.value, { ...m, status: typeof m.status === 'number' ? 'operational' : m.status })
  isModalOpen.value = true
}

const openForensics = (m: MachineDto) => {
  focusedMachine.value = m
  isPanelOpen.value = true
}

const handleSubmit = async () => {
  try {
    if (selectedMachine.value) await updateMachine(selectedMachine.value.id, machineForm.value)
    else await createMachine(machineForm.value)
    toast.success('Asset registry synchronized')
    isModalOpen.value = false
    await loadMachines()
  } catch { toast.error('Registry update failed') }
}

const handleDelete = async (m: MachineDto) => {
  if (!confirm(`Decommission asset "${m.name}"?`)) return
  try {
    await deleteMachine(m.id)
    machines.value = machines.value.filter(x => x.id !== m.id)
    toast.success('Asset decommissioned')
  } catch { toast.error('Decommission failure') }
}

onMounted(loadMachines)
</script>

<template>
  <div :class="styles['mgmt-container']">
    <!-- Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Layers :width="14" :height="14" />
          <span>Asset Orchestration</span>
        </div>
        <h1 :class="styles['title']">Infrastructure Registry</h1>
        <p :class="styles['description']">Provisioning and lifecycle management for industrial digital clusters</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton variant="secondary" @click="loadMachines" :loading="loading">
          <RefreshCw :width="16" :height="16" />
          Resync
        </UiButton>
        <UiButton variant="primary" @click="isModalOpen = true; selectedMachine = null">
          <Plus :width="16" :height="16" />
          Register Node
        </UiButton>
      </div>
    </header>

    <!-- Filters -->
    <section :class="styles['filter-section']">
      <UiCard variant="glass" padding="sm" :class="styles['filter-card']">
        <div :class="styles['filter-grid']">
          <div :class="styles['search-wrap']">
            <Search :class="styles['search-icon']" :width="14" :height="14" />
            <UiInput v-model="searchQuery" placeholder="Filter nodes by ID, name, or metadata..." :class="styles['search-input']" />
          </div>
          <UiSelect v-model="statusFilter">
            <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </UiSelect>
          <UiSelect v-model="typeFilter">
            <option v-for="opt in typeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </UiSelect>
        </div>
      </UiCard>
    </section>

    <!-- Asset Feed -->
    <main :class="styles['asset-feed']">
      <UiTable
        :columns="columns"
        :items="filteredMachines"
        :loading="loading"
      >
        <template #cell-status="{ item }">
          <UiBadge :variant="String(item.status).toLowerCase() as any" size="sm" dot>
            {{ String(item.status).toUpperCase() }}
          </UiBadge>
        </template>

        <template #cell-name="{ item }">
          <div :class="styles['asset-identity']">
            <span :class="styles['asset-name-main']">{{ item.name }}</span>
            <span :class="styles['asset-sn-sub']">{{ item.serialNumber || 'SN-UNKNOWN' }}</span>
          </div>
        </template>

        <template #cell-criticality="{ item }">
          <div :class="[styles['risk-indicator'], styles[`risk--${item.criticality}`]]">
            {{ item.criticality }}
          </div>
        </template>

        <template #cell-actions="{ item }">
          <div :class="styles['action-group']">
             <button :class="styles['icon-action']" @click="openForensics(item)" title="Forensic Insights">
               <Eye :width="14" :height="14" />
             </button>
             <button :class="styles['icon-action']" @click="openEditModal(item)" title="Modify Protocol">
               <Edit :width="14" :height="14" />
             </button>
             <button :class="[styles['icon-action'], styles['icon-action--danger']]" @click="handleDelete(item)" title="Decommission">
               <Trash2 :width="14" :height="14" />
             </button>
          </div>
        </template>
      </UiTable>
    </main>

    <!-- Context Panel (Forensics) -->
    <UiSlideOver
      :is-open="isPanelOpen"
      :title="focusedMachine?.name || 'Node Forensics'"
      description="In-depth artifact analysis and neural drift diagnostics"
      @close="isPanelOpen = false"
    >
      <div v-if="focusedMachine" :class="styles['panel-body']">
        <div :class="styles['forensic-grid']">
          <UiCard variant="elevated" padding="md" dots>
             <div :class="styles['f-header']">
               <ShieldAlert :width="16" :height="16" :class="styles['f-icon']" />
               <span :class="styles['f-label']">Integrity Score</span>
             </div>
             <div :class="styles['f-value']">{{ 100 - (focusedMachine.criticality * 5) }}%</div>
             <div :class="styles['f-meta']">Neural verification 99.8% accurate</div>
          </UiCard>

          <UiCard variant="outline" padding="md">
             <div :class="styles['f-header']">
               <Cpu :width="16" :height="16" :class="styles['f-icon-alt']" />
               <span :class="styles['f-label']">Deployment</span>
             </div>
             <div :class="styles['f-sub']">{{ focusedMachine.location }}</div>
             <div :class="styles['f-tag']">{{ focusedMachine.type }}</div>
          </UiCard>
        </div>

        <div :class="styles['f-section']">
          <h4 :class="styles['f-section-title']">Lifecycle Manifest</h4>
          <div :class="styles['f-manifest']">
            <div :class="styles['f-manifest-item']">
              <span>Installed</span>
              <span>{{ new Date(focusedMachine.installationDate).toLocaleDateString() }}</span>
            </div>
            <div :class="styles['f-manifest-item']">
              <span>Last Maintenance</span>
              <span>{{ focusedMachine.lastMaintenance ? new Date(focusedMachine.lastMaintenance).toLocaleDateString() : 'NEVER' }}</span>
            </div>
            <div :class="styles['f-manifest-item']">
              <span>Next Protocol Sync</span>
              <span>{{ focusedMachine.nextMaintenance ? new Date(focusedMachine.nextMaintenance).toLocaleDateString() : 'TBD' }}</span>
            </div>
          </div>
        </div>

        <div :class="styles['f-section']">
           <h4 :class="styles['f-section-title']">Neural Metadata</h4>
           <div :class="styles['f-metadata']">
             <div v-for="(val, key) in focusedMachine.metadata" :key="key" :class="styles['f-meta-tag']">
               {{ key }}: {{ val }}
             </div>
             <div v-if="!Object.keys(focusedMachine.metadata || {}).length" :class="styles['f-empty']">
               No extended metadata available for this node.
             </div>
           </div>
        </div>

        <div :class="styles['f-actions']">
           <UiButton variant="primary" size="lg" @click="openEditModal(focusedMachine)">
             Modify Node Registry
           </UiButton>
        </div>
      </div>
    </UiSlideOver>

    <!-- Protocol Modal (Create/Edit) -->
    <UiModal 
      :is-open="isModalOpen" 
      maxWidth="lg"
      @close="isModalOpen = false"
    >
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
             <div :class="styles['modal-title-group']">
               <div :class="styles['modal-eyebrow']">SYSTEM PROTOCOL v2.0</div>
               <h2 :class="styles['modal-title']">{{ selectedMachine ? 'Sync Asset Protocol' : 'Provision New Node' }}</h2>
               <p :class="styles['modal-sub']">Artifact registry synchronization for digital twins</p>
             </div>
          </div>
        </template>

        <form @submit.prevent="handleSubmit" :class="styles['modal-form']">
          <div :class="styles['form-grid']">
            <UiInput v-model="machineForm.name" label="Node Identifier" placeholder="e.g., NODE-01-CNC" required />
            <UiInput v-model="machineForm.serialNumber" label="Registry SN" placeholder="SN-XXXX-XXXX" />
            <UiInput v-model="machineForm.type" label="System Type" placeholder="Kinematic Class" required />
            <UiSelect v-model="machineForm.status" label="Protocol Status">
              <option v-for="opt in statusOptions.slice(1)" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
            </UiSelect>
            <UiInput v-model="machineForm.location" label="Deployment Point" placeholder="Sector/Bay" required />
            <UiInput v-model.number="machineForm.criticality" type="number" label="Risk Index (1-5)" min="1" max="5" />
          </div>

          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="isModalOpen = false">Cancel Mission</UiButton>
            <UiButton variant="primary" type="submit">
              {{ selectedMachine ? 'Commit Synchronization' : 'Initialize Provisioning' }}
            </UiButton>
          </div>
        </form>
      </UiCard>
    </UiModal>
  </div>
</template>

<style module>
.mgmt-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  max-width: 1400px;
  margin: 0 auto;
}

/* Forensic Panel Content */
.panel-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.forensic-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-16);
}

.f-header {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  margin-bottom: var(--space-12);
}

.f-icon { color: var(--color-danger); }
.f-icon-alt { color: var(--color-primary); }

.f-label {
  font-size: 10px;
  font-weight: 800;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.1em;
}

.f-value {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  line-height: 1;
}

.f-meta {
  font-size: 10px;
  color: var(--color-text-dim);
  margin-top: var(--space-8);
}

.f-sub {
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--color-text-primary);
}

.f-tag {
  display: inline-block;
  margin-top: var(--space-8);
  font-size: 9px;
  font-weight: 800;
  color: var(--color-primary);
  background: var(--color-primary-muted);
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
}

.f-section-title {
  font-size: 11px;
  font-weight: 800;
  color: var(--color-text-primary);
  text-transform: uppercase;
  letter-spacing: 0.15em;
  margin-bottom: var(--space-16);
  padding-bottom: var(--space-8);
  border-bottom: 1px solid var(--color-border-subtle);
}

.f-manifest {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.f-manifest-item {
  display: flex;
  justify-content: space-between;
  font-size: var(--font-size-sm);
}

.f-manifest-item span:first-child {
  color: var(--color-text-dim);
  font-weight: 500;
}

.f-manifest-item span:last-child {
  color: var(--color-text-secondary);
  font-weight: 700;
  font-family: var(--font-mono);
}

.f-metadata {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-8);
}

.f-meta-tag {
  font-size: 10px;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  color: var(--color-text-secondary);
}

.f-empty {
  font-size: var(--font-size-xs);
  color: var(--color-text-dim);
  font-style: italic;
}

.f-actions {
  margin-top: var(--space-20);
  padding-top: var(--space-32);
  border-top: 1px solid var(--color-border-subtle);
}

/* Modal Styling */
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

.modal-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-20);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-12);
  padding-top: var(--space-24);
  border-top: 1px solid var(--color-border-subtle);
}

/* Header */
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

.title {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  letter-spacing: -0.03em;
  color: var(--color-text-primary);
  margin: 0;
}

.description {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.header-actions {
  display: flex;
  gap: var(--space-12);
}

/* Filters */
.filter-grid {
  display: grid;
  grid-template-columns: 1fr 200px 200px;
  gap: var(--space-16);
  align-items: center;
}

.search-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: var(--space-12);
  color: var(--color-text-dim);
  z-index: 10;
}

.search-input :global(.input) {
  padding-left: var(--space-32);
}

/* Table Enhancements */
.asset-identity {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.asset-name-main {
  font-weight: 700;
  color: var(--color-text-primary);
  letter-spacing: var(--font-tracking-tight);
}

.asset-sn-sub {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  font-family: var(--font-mono);
}

.risk-indicator {
  width: 24px;
  height: 24px;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 800;
  font-family: var(--font-mono);
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
}

.risk--1, .risk--2 { color: var(--color-success); border-color: var(--color-success-muted); }
.risk--3 { color: var(--color-warning); border-color: var(--color-warning-muted); }
.risk--4, .risk--5 { color: var(--color-danger); border-color: var(--color-danger-muted); }

.action-group {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-4);
}

.icon-action {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: transparent;
  border: 1px solid transparent;
  color: var(--color-text-dim);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-action:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
  border-color: var(--color-border);
  box-shadow: var(--shadow-sm);
}

.icon-action--danger:hover {
  background: var(--color-danger-muted);
  color: var(--color-danger);
  border-color: rgba(239, 68, 68, 0.2);
}

/* States */
.state-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-64);
  gap: var(--space-16);
  color: var(--color-text-dim);
  text-align: center;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 2px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1024px) {
  .filter-grid { grid-template-columns: 1fr; }
  .asset-grid { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 768px) {
  .page-header { flex-direction: column; align-items: flex-start; gap: var(--space-20); }
  .asset-grid { grid-template-columns: 1fr; }
  .form-grid { grid-template-columns: 1fr; }
}
</style>
