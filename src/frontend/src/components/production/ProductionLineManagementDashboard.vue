<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  Factory, 
  Play, 
  Pause, 
  Settings,
  Users,
  Activity,
  Filter,
  Search,
  Plus,
  Edit,
  Trash2,
  Box,
  Cpu,
  Database,
  Link,
  ChevronRight
} from 'lucide-vue-next'

const styles = useCssModule()
const toast = useToast()

// State
const productionLines = ref<any[]>([
  {
    id: 'PL001',
    name: 'Main Assembly Line',
    status: 'operational',
    machineCount: 8,
    currentOutput: 1250,
    targetOutput: 1500,
    efficiency: 83.3,
    machines: ['M001', 'M002', 'M003', 'M004', 'M005', 'M006', 'M007', 'M008'],
    configuration: {
      shiftHours: '24/7',
      operators: 12,
      supervisor: 'John Smith'
    },
    lastUpdated: new Date().toISOString()
  },
  {
    id: 'PL002',
    name: 'Quality Control Line',
    status: 'warning',
    machineCount: 5,
    currentOutput: 850,
    targetOutput: 1000,
    efficiency: 85,
    machines: ['M009', 'M010', 'M011', 'M012', 'M013'],
    configuration: {
      shiftHours: '8AM-4PM',
      operators: 6,
      supervisor: 'Sarah Johnson'
    },
    lastUpdated: new Date(Date.now() - 3600000).toISOString()
  }
])

const machines = ref<MachineDto[]>([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('all')

// Modal state
const isManageModalOpen = ref(false)
const isAssignModalOpen = ref(false)
const selectedLine = ref<any>(null)
const isEditing = computed(() => !!selectedLine.value)

// Form state
const lineForm = ref({
  name: '',
  configuration: { supervisor: '', shiftHours: '', operators: 0 },
  machineIds: [] as string[]
})

const assignForm = ref({
  machineId: '',
  lineId: ''
})

// Computed
const filteredLines = computed(() => {
  let filtered = [...productionLines.value]
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    filtered = filtered.filter(l => l.name.toLowerCase().includes(q) || l.id.toLowerCase().includes(q))
  }
  if (statusFilter.value !== 'all') filtered = filtered.filter(l => l.status === statusFilter.value)
  return filtered
})

const availableMachines = computed(() => {
  const assigned = productionLines.value.flatMap(l => l.machines || [])
  return machines.value.filter(m => !assigned.includes(m.id))
})

const statusOptions = [
  { label: 'ALL STATUSES', value: 'all' },
  { label: 'OPERATIONAL', value: 'operational' },
  { label: 'WARNING', value: 'warning' },
  { label: 'MAINTENANCE', value: 'maintenance' },
  { label: 'OFFLINE', value: 'offline' }
]

// Methods
const loadData = async () => {
  try {
    loading.value = true
    machines.value = await fetchMachines()
  } catch {
    toast.error('Registry link failed')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  selectedLine.value = null
  lineForm.value = { name: '', configuration: { supervisor: '', shiftHours: '', operators: 0 }, machineIds: [] }
  isManageModalOpen.value = true
}

const openEditModal = (line: any) => {
  selectedLine.value = line
  lineForm.value = {
    name: line.name,
    configuration: { ...line.configuration },
    machineIds: [...(line.machines || [])]
  }
  isManageModalOpen.value = true
}

const openAssignModal = (line: any) => {
  selectedLine.value = line
  assignForm.value.lineId = line.id
  assignForm.value.machineId = ''
  isAssignModalOpen.value = true
}

const handleSaveLine = async () => {
  toast.success(isEditing.value ? 'Protocol updated' : 'Line provisioned')
  isManageModalOpen.value = false
}

const handleAssignMachine = async () => {
  if (!selectedLine.value || !assignForm.value.machineId) return
  const line = productionLines.value.find(l => l.id === selectedLine.value.id)
  if (line) {
    if (!line.machines) line.machines = []
    line.machines.push(assignForm.value.machineId)
    line.machineCount += 1
  }
  toast.success('Asset mapped')
  isAssignModalOpen.value = false
}

const removeMachine = (lineId: string, machineId: string) => {
  if (!confirm('Unlink this asset from protocol?')) return
  const line = productionLines.value.find(l => l.id === lineId)
  if (line) {
    line.machines = line.machines.filter((id: string) => id !== machineId)
    line.machineCount -= 1
    toast.success('Asset unlinked')
  }
}

onMounted(loadData)
</script>

<template>
  <div :class="styles['mgmt-container']">
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Activity :width="14" :height="14" />
          <span>Operational Dashboard</span>
        </div>
        <h1 :class="styles['title']">Production Streams</h1>
        <p :class="styles['description']">Real-time orchestration and telemetry mapping for digital clusters</p>
      </div>
      <UiButton variant="primary" @click="openCreateModal">
        <Plus :width="16" :height="16" />
        Provision Stream
      </UiButton>
    </header>

    <!-- Filters -->
    <section :class="styles['filter-section']">
      <UiCard variant="glass" padding="sm">
        <div :class="styles['filter-grid']">
          <div :class="styles['search-wrap']">
            <Search :class="styles['search-icon']" :width="14" :height="14" />
            <UiInput v-model="searchQuery" placeholder="Search operational streams..." :class="styles['search-input']" />
          </div>
          <UiSelect v-model="statusFilter" :options="statusOptions" />
        </div>
      </UiCard>
    </section>

    <!-- Grid -->
    <main :class="styles['feed-container']">
      <div v-if="filteredLines.length === 0" :class="styles['state-box']">
        <Factory :width="48" :height="48" :class="styles['state-icon']" />
        <h3>No Active Streams</h3>
        <UiButton variant="secondary" size="sm" @click="openCreateModal">Initialize Stream</UiButton>
      </div>

      <div v-else :class="styles['line-grid']">
        <UiCard v-for="line in filteredLines" :key="line.id" variant="default" hover :class="styles['line-card']">
          <div :class="styles['card-body']">
            <div :class="styles['card-header']">
              <div :class="styles['line-id-block']">
                <div :class="styles['line-icon']"><Box :width="18" :height="18" /></div>
                <div :class="styles['line-titles']">
                  <h4 :class="styles['line-name']">{{ line.name }}</h4>
                  <span :class="styles['line-sn']">ID: {{ line.id }}</span>
                </div>
              </div>
              <UiBadge :variant="line.status" size="sm" dot>{{ line.status.toUpperCase() }}</UiBadge>
            </div>

            <div :class="styles['metrics-row']">
              <div :class="styles['metric-pill']">
                <span :class="styles['m-label']">NODES</span>
                <span :class="styles['m-val']">{{ line.machineCount }}</span>
              </div>
              <div :class="styles['metric-pill']">
                <span :class="styles['m-label']">EFFICIENCY</span>
                <span :class="[styles['m-val'], line.efficiency > 80 ? styles['v-success'] : styles['v-warning']]">{{ line.efficiency }}%</span>
              </div>
            </div>

            <div :class="styles['machine-list-wrap']">
              <span :class="styles['meta-label']">LINKED NODES</span>
              <div :class="styles['tag-cloud']">
                <div v-for="mid in line.machines" :key="mid" :class="styles['machine-tag']">
                  <span>{{ mid }}</span>
                  <button @click="removeMachine(line.id, mid)" :class="styles['tag-close']">&times;</button>
                </div>
                <button @click="openAssignModal(line)" :class="styles['tag-add']"><Plus :width="12" :height="12" /></button>
              </div>
            </div>

            <div :class="styles['card-footer']">
               <UiButton variant="ghost" size="sm" @click="openEditModal(line)">
                 <Edit :width="14" :height="14" />
                 Modify Protocol
               </UiButton>
            </div>
          </div>
        </UiCard>
      </div>
    </main>

    <!-- Modals -->
    <UiModal :is-open="isManageModalOpen" maxWidth="md" @close="isManageModalOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">STREAM CONFIGURATION</div>
            <h2 :class="styles['modal-title']">{{ isEditing ? 'Sync Stream' : 'Initialize Stream' }}</h2>
          </div>
        </template>
        <form @submit.prevent="handleSaveLine" :class="styles['modal-form']">
          <UiInput v-model="lineForm.name" label="Stream Identifier" required />
          <div :class="styles['field-grid']">
            <UiInput v-model="lineForm.configuration.supervisor" label="Lead Architect" />
            <UiInput v-model="lineForm.configuration.shiftHours" label="Operational Window" />
          </div>
          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="isManageModalOpen = false">Abort</UiButton>
            <UiButton variant="primary" type="submit">Commit Synchronization</UiButton>
          </div>
        </form>
      </UiCard>
    </UiModal>

    <UiModal :is-open="isAssignModalOpen" maxWidth="sm" @close="isAssignModalOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">ASSET MAPPING</div>
            <h2 :class="styles['modal-title']">Link Node</h2>
          </div>
        </template>
        <div :class="styles['modal-form']">
          <UiSelect v-model="assignForm.machineId" label="Available Registry Nodes" :options="availableMachines.map(m => ({ label: `${m.name} (${m.id})`, value: m.id }))" />
          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="isAssignModalOpen = false">Cancel</UiButton>
            <UiButton variant="primary" @click="handleAssignMachine" :disabled="!assignForm.machineId">Synchronize Mapping</UiButton>
          </div>
        </div>
      </UiCard>
    </UiModal>
  </div>
</template>

<style module>
.mgmt-container { display: flex; flex-direction: column; gap: var(--space-32); }
.page-header { display: flex; justify-content: space-between; align-items: flex-end; padding-bottom: var(--space-24); border-bottom: 1px solid var(--color-border-subtle); }
.eyebrow { display: flex; align-items: center; gap: var(--space-8); font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.15em; color: var(--color-primary); margin-bottom: var(--space-6); }
.title { font-size: var(--font-size-3xl); font-weight: 800; color: var(--color-text-primary); margin: 0; }
.description { font-size: var(--font-size-sm); color: var(--color-text-muted); }

.filter-grid { display: grid; grid-template-columns: 1fr 240px; gap: var(--space-16); align-items: center; }
.search-wrap { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: var(--space-12); color: var(--color-text-dim); z-index: 10; }
.search-input :global(.input) { padding-left: var(--space-32); }

.line-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(400px, 1fr)); gap: var(--space-24); }
.card-body { padding: var(--space-24); display: flex; flex-direction: column; gap: var(--space-20); }
.card-header { display: flex; justify-content: space-between; align-items: flex-start; }
.line-id-block { display: flex; align-items: center; gap: var(--space-16); }
.line-icon { width: 40px; height: 40px; border-radius: 10px; background: var(--color-depth-1); border: 1px solid var(--color-border); display: flex; align-items: center; justify-content: center; color: var(--color-primary); }
.line-name { margin: 0; font-size: var(--font-size-lg); font-weight: 750; color: var(--color-text-primary); }
.line-sn { font-size: 10px; color: var(--color-text-dim); font-family: var(--font-mono); }

.metrics-row { display: flex; gap: var(--space-12); }
.metric-pill { padding: 8px 16px; background: var(--color-depth-1); border: 1px solid var(--color-border); border-radius: 8px; flex: 1; display: flex; flex-direction: column; gap: 2px; }
.m-label { font-size: 9px; font-weight: 900; color: var(--color-text-dim); }
.m-val { font-size: var(--font-size-md); font-weight: 800; }
.v-success { color: var(--color-success); }
.v-warning { color: var(--color-warning); }

.meta-label { font-size: 10px; font-weight: 900; color: var(--color-text-dim); margin-bottom: var(--space-8); display: block; }
.tag-cloud { display: flex; flex-wrap: wrap; gap: 6px; }
.machine-tag { font-size: 10px; font-weight: 700; padding: 4px 8px; background: var(--color-surface-alt); border: 1px solid var(--color-border); border-radius: 4px; display: flex; align-items: center; gap: 6px; }
.tag-close { background: none; border: none; color: var(--color-text-dim); cursor: pointer; padding: 0; font-size: 14px; }
.tag-close:hover { color: var(--color-danger); }
.tag-add { width: 24px; height: 24px; border-radius: 4px; border: 1px dashed var(--color-border-strong); background: none; color: var(--color-text-muted); cursor: pointer; display: flex; align-items: center; justify-content: center; }
.tag-add:hover { border-color: var(--color-primary); color: var(--color-primary); }

.card-footer { display: flex; justify-content: flex-end; padding-top: var(--space-16); border-top: 1px solid var(--color-border-subtle); }

.modal-header { margin-bottom: var(--space-24); }
.modal-eyebrow { font-size: 10px; font-weight: 900; color: var(--color-primary); letter-spacing: 0.2em; margin-bottom: 4px; }
.modal-title { font-size: var(--font-size-xl); font-weight: 800; color: var(--color-text-primary); margin: 0; }
.modal-form { display: flex; flex-direction: column; gap: var(--space-24); }
.field-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-16); }
.modal-footer { display: flex; justify-content: flex-end; gap: var(--space-12); padding-top: var(--space-24); border-top: 1px solid var(--color-border-subtle); }

.state-box { display: flex; flex-direction: column; align-items: center; padding: var(--space-64); gap: var(--space-20); color: var(--color-text-dim); }

@media (max-width: 768px) {
  .line-grid { grid-template-columns: 1fr; }
  .field-grid { grid-template-columns: 1fr; }
  .filter-grid { grid-template-columns: 1fr; }
}
</style>