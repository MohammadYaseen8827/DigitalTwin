<script setup lang="ts">
import { ref, computed, onMounted, useCssModule } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import UiModal from '@/components/ui/UiModal.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchProductionLines,
  createProductionLine,
  updateProductionLine,
  deleteProductionLine
} from '@/services/productionLines.service'
import { fetchMachines } from '@/services/machines.service'
import type { 
  ProductionLineDto, 
  ProductionLineCreateDto,
  ProductionLineUpdateDto
} from '@/api/types/index'
import { 
  Factory, 
  Plus, 
  Edit, 
  Trash2,
  Settings,
  Filter,
  Search,
  RefreshCw,
  Link,
  Unlink,
  Box,
  Cpu,
  Activity,
  ChevronRight,
  Database
} from 'lucide-vue-next'

const styles = useCssModule()
const toast = useToast()

// State
const productionLines = ref<any[]>([])
const machines = ref<any[]>([])
const loading = ref(false)
const searchQuery = ref('')
const machineFilter = ref('all')

// Modal state
const isManageModalOpen = ref(false)
const isAssignModalOpen = ref(false)
const selectedLine = ref<any>(null)
const isEditing = computed(() => !!selectedLine.value)

// Form state
const lineForm = ref({
  name: '',
  configuration: {} as Record<string, any>
})

const assignForm = ref({
  machineIds: [] as string[]
})

// Computed
const filteredLines = computed(() => {
  let filtered = [...productionLines.value]
  
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((line: any) => 
      line.name?.toLowerCase().includes(query) ||
      line.id?.toLowerCase().includes(query)
    )
  }
  
  if (machineFilter.value !== 'all') {
    filtered = filtered.filter((line: any) => 
      line.machineIds?.includes(machineFilter.value)
    )
  }
  
  return filtered
})

const machineOptions = computed(() => {
  return [
    { label: 'ALL CLUSTERS', value: 'all' },
    ...machines.value.map((m: any) => ({
      label: `${m.name.toUpperCase()} (${m.type})`,
      value: m.id
    }))
  ]
})

const availableMachines = computed(() => {
  return machines.value.map((m: any) => ({
    ...m,
    assigned: assignForm.value.machineIds.includes(m.id)
  }))
})

// Methods
const loadData = async () => {
  try {
    loading.value = true
    const [lines, machs] = await Promise.all([
      fetchProductionLines(),
      fetchMachines()
    ])
    productionLines.value = lines || []
    machines.value = machs || []
  } catch (error) {
    toast.error('Registry synchronization failed')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  selectedLine.value = null
  lineForm.value = { name: '', configuration: {} }
  isManageModalOpen.value = true
}

const openEditModal = (line: any) => {
  selectedLine.value = line
  lineForm.value = {
    name: line.name,
    configuration: { ...(line.configuration || {}) }
  }
  isManageModalOpen.value = true
}

const openAssignModal = (line: any) => {
  selectedLine.value = line
  assignForm.value.machineIds = [...(line.machineIds || [])]
  isAssignModalOpen.value = true
}

const handleSaveLine = async () => {
  try {
    if (isEditing.value) {
      await updateProductionLine(selectedLine.value.id, lineForm.value)
      toast.success('Protocol updated')
    } else {
      await createProductionLine(lineForm.value)
      toast.success('Line provisioned')
    }
    isManageModalOpen.value = false
    await loadData()
  } catch {
    toast.error('Commit failed')
  }
}

const handleDeleteLine = async (line: any) => {
  if (!confirm(`Decommission production line "${line.name}"?`)) return
  
  try {
    await deleteProductionLine(line.id)
    productionLines.value = productionLines.value.filter((l: any) => l.id !== line.id)
    toast.success('Line decommissioned')
  } catch {
    toast.error('Decommission failed')
  }
}

const handleUpdateAssignments = async () => {
  if (!selectedLine.value) return
  
  try {
    await updateProductionLine(selectedLine.value.id, {
      ...selectedLine.value,
      machineIds: assignForm.value.machineIds
    })
    toast.success('Cluster mapping synchronized')
    isAssignModalOpen.value = false
    await loadData()
  } catch {
    toast.error('Mapping update failed')
  }
}

const toggleMachine = (id: string) => {
  const idx = assignForm.value.machineIds.indexOf(id)
  if (idx > -1) assignForm.value.machineIds.splice(idx, 1)
  else assignForm.value.machineIds.push(id)
}

onMounted(loadData)
</script>

<template>
  <div :class="styles['mgmt-container']">
    <!-- Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Factory :width="14" :height="14" />
          <span>Line Management</span>
        </div>
        <h1 :class="styles['title']">Production Topology</h1>
        <p :class="styles['description']">Strategic orchestration of factory floor digital clusters</p>
      </div>
      <div :class="styles['header-actions']">
        <UiButton variant="secondary" @click="loadData" :loading="loading">
          <RefreshCw :width="16" :height="16" />
          Resync
        </UiButton>
        <UiButton variant="primary" @click="openCreateModal">
          <Plus :width="16" :height="16" />
          Provision Line
        </UiButton>
      </div>
    </header>

    <!-- Stats Overview -->
    <section :class="styles['stats-grid']">
      <UiCard variant="glass" padding="md" :class="styles['stat-card']">
        <div :class="styles['stat-content']">
          <div :class="[styles['stat-icon'], styles['icon--blue']]"><Box :width="20" :height="20" /></div>
          <div>
            <div :class="styles['stat-val']">{{ productionLines.length }}</div>
            <div :class="styles['stat-label']">ACTIVE PROTOCOLS</div>
          </div>
        </div>
      </UiCard>
      <UiCard variant="glass" padding="md" :class="styles['stat-card']">
        <div :class="styles['stat-content']">
          <div :class="[styles['stat-icon'], styles['icon--emerald']]"><Cpu :width="20" :height="20" /></div>
          <div>
            <div :class="styles['stat-val']">{{ machines.length }}</div>
            <div :class="styles['stat-label']">LINKED ASSETS</div>
          </div>
        </div>
      </UiCard>
      <UiCard variant="glass" padding="md" :class="styles['stat-card']">
        <div :class="styles['stat-content']">
          <div :class="[styles['stat-icon'], styles['icon--amber']]"><Database :width="20" :height="20" /></div>
          <div>
            <div :class="styles['stat-val']">{{ Object.keys(machines).length }}</div>
            <div :class="styles['stat-label']">CLUSTER NODES</div>
          </div>
        </div>
      </UiCard>
    </section>

    <!-- Filter Bar -->
    <section :class="styles['filter-section']">
      <UiCard variant="glass" padding="sm">
        <div :class="styles['filter-grid']">
          <div :class="styles['search-wrap']">
            <Search :class="styles['search-icon']" :width="14" :height="14" />
            <UiInput v-model="searchQuery" placeholder="Filter protocols by identifier..." :class="styles['search-input']" />
          </div>
          <UiSelect v-model="machineFilter" :options="machineOptions" />
        </div>
      </UiCard>
    </section>

    <!-- Main Feed -->
    <main :class="styles['feed-container']">
      <div v-if="loading" :class="styles['state-box']">
        <div :class="styles['spinner']" />
        <span>Syncing Data Grid...</span>
      </div>

      <div v-else-if="filteredLines.length === 0" :class="styles['state-box']">
        <Factory :width="48" :height="48" :class="styles['state-icon']" />
        <h3>Empty Registry</h3>
        <p>No production line records found in the current sector.</p>
        <UiButton variant="secondary" size="sm" @click="openCreateModal">Initialize First Protocol</UiButton>
      </div>

      <div v-else :class="styles['line-grid']">
        <UiCard 
          v-for="line in filteredLines" 
          :key="line.id" 
          variant="default" 
          padding="none"
          hover
          :class="styles['line-card']"
        >
          <div :class="styles['card-body']">
            <div :class="styles['card-header']">
              <div :class="styles['line-id-block']">
                <div :class="styles['line-icon']"><Factory :width="18" :height="18" /></div>
                <div :class="styles['line-titles']">
                  <h4 :class="styles['line-name']">{{ line.name }}</h4>
                  <span :class="styles['line-sn']">ID: {{ line.id }}</span>
                </div>
              </div>
              <UiBadge :variant="line.machineIds?.length ? 'operational' : 'critical'" size="sm" dot>
                {{ line.machineIds?.length || 0 }} NODES
              </UiBadge>
            </div>

            <div :class="styles['line-meta']">
              <div :class="styles['meta-item']">
                <span :class="styles['meta-label']">Asset Distribution</span>
                <div :class="styles['tag-cloud']">
                  <span v-for="mid in line.machineIds?.slice(0, 3)" :key="mid" :class="styles['machine-tag']">{{ mid }}</span>
                  <span v-if="line.machineIds?.length > 3" :class="styles['tag-more']">+{{ line.machineIds.length - 3 }}</span>
                </div>
              </div>
            </div>

            <div :class="styles['card-footer']">
               <UiButton variant="ghost" size="sm" @click="openAssignModal(line)">
                 <Link :width="14" :height="14" />
                 Map Assets
               </UiButton>
               <UiButton variant="ghost" size="sm" @click="openEditModal(line)">
                 <Edit :width="14" :height="14" />
                 Config
               </UiButton>
               <UiButton variant="ghost" size="sm" :class="styles['btn-danger']" @click="handleDeleteLine(line)">
                 <Trash2 :width="14" :height="14" />
               </UiButton>
            </div>
          </div>
        </UiCard>
      </div>
    </main>

    <!-- Create/Edit Modal -->
    <UiModal :is-open="isManageModalOpen" maxWidth="md" @close="isManageModalOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">TOPOLOGY CONFIG v2.4</div>
            <h2 :class="styles['modal-title']">{{ isEditing ? 'Update Protocol' : 'Provision Line' }}</h2>
            <p :class="styles['modal-sub']">Configure structural parameters for digital twin synchronization</p>
          </div>
        </template>

        <form @submit.prevent="handleSaveLine" :class="styles['modal-form']">
          <UiInput v-model="lineForm.name" label="Protocol Identifier" placeholder="e.g., SECTOR-A-ASSEMBLY" required />
          
          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="isManageModalOpen = false">Abort</UiButton>
            <UiButton variant="primary" type="submit">
              {{ isEditing ? 'Commit Changes' : 'Initialize Protocol' }}
            </UiButton>
          </div>
        </form>
      </UiCard>
    </UiModal>

    <!-- Assign Modal -->
    <UiModal :is-open="isAssignModalOpen" maxWidth="lg" @close="isAssignModalOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">CLUSTER MAPPING</div>
            <h2 :class="styles['modal-title']">Asset Integration</h2>
            <p :class="styles['modal-sub']">Mapping physical nodes to the {{ selectedLine?.name }} protocol</p>
          </div>
        </template>

        <div :class="styles['assign-grid']">
          <div v-if="machines.length === 0" :class="styles['empty-assign']">
            No assets available for mapping in the current registry.
          </div>
          <button 
            v-for="m in machines" 
            :key="m.id" 
            :class="[styles['assign-item'], assignForm.machineIds.includes(m.id) && styles['item--active']]"
            @click="toggleMachine(m.id)"
          >
            <div :class="styles['item-info']">
              <span :class="styles['item-name']">{{ m.name }}</span>
              <span :class="styles['item-meta']">{{ m.type }} • {{ m.location }}</span>
            </div>
            <div :class="styles['item-check']">
              <Plus v-if="!assignForm.machineIds.includes(m.id)" :width="14" :height="14" />
              <ChevronRight v-else :width="14" :height="14" />
            </div>
          </button>
        </div>

        <div :class="styles['modal-footer']">
          <div :class="styles['selection-info']">{{ assignForm.machineIds.length }} NODES LINKED</div>
          <div :class="styles['footer-actions']">
            <UiButton variant="ghost" @click="isAssignModalOpen = false">Cancel</UiButton>
            <UiButton variant="primary" @click="handleUpdateAssignments">Sync Mapping</UiButton>
          </div>
        </div>
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

/* Stats */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-20);
}

.stat-card {
  border: 1px solid var(--color-border-subtle);
}

.stat-content {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.stat-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-depth-1);
}

.icon--blue { color: var(--color-primary); }
.icon--emerald { color: var(--color-emerald); }
.icon--amber { color: var(--color-amber); }

.stat-val {
  font-size: var(--font-size-2xl);
  font-weight: 800;
  color: var(--color-text-primary);
  line-height: 1;
}

.stat-label {
  font-size: 9px;
  font-weight: 800;
  color: var(--color-text-dim);
  letter-spacing: 0.1em;
  margin-top: 4px;
}

/* Filters */
.filter-grid {
  display: grid;
  grid-template-columns: 1fr 240px;
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

/* Grid */
.line-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: var(--space-20);
}

.card-body {
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

.line-id-block {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.line-icon {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
}

.line-name {
  margin: 0;
  font-size: var(--font-size-lg);
  font-weight: 750;
  color: var(--color-text-primary);
  letter-spacing: -0.02em;
}

.line-sn {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  font-family: var(--font-mono);
}

.line-meta {
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.meta-label {
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--color-text-dim);
  margin-bottom: var(--space-8);
  display: block;
}

.tag-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-6);
}

.machine-tag {
  font-size: 10px;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 6px;
  background: var(--color-surface-alt);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  font-family: var(--font-mono);
}

.tag-more {
  font-size: 10px;
  font-weight: 800;
  color: var(--color-primary);
  display: flex;
  align-items: center;
  padding-left: 4px;
}

.card-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-8);
  padding-top: var(--space-16);
  border-top: 1px solid var(--color-border-subtle);
}

.btn-danger { color: var(--color-danger); }
.btn-danger:hover { background: var(--color-danger-muted) !important; }

/* Modals */
.modal-header {
  margin-bottom: var(--space-32);
}

.modal-eyebrow {
  font-size: 10px;
  font-weight: 900;
  color: var(--color-primary);
  letter-spacing: 0.2em;
  margin-bottom: var(--space-6);
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
  margin-top: var(--space-6);
  line-height: var(--font-lineheight-relaxed);
}

.modal-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: var(--space-16);
  padding-top: var(--space-24);
  border-top: 1px solid var(--color-border-subtle);
}

.assign-grid {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
  max-height: 400px;
  overflow-y: auto;
  padding-right: var(--space-8);
  margin-bottom: var(--space-32);
}

.assign-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-16);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  cursor: pointer;
  transition: all var(--transition-fast);
  text-align: left;
}

.assign-item:hover {
  background: var(--color-surface);
  border-color: var(--color-border-strong);
}

.item--active {
  border-color: var(--color-primary);
  background: var(--color-primary-muted);
}

.item-name {
  display: block;
  font-weight: 700;
  font-size: var(--font-size-sm);
  color: var(--color-text-primary);
}

.item-meta {
  font-size: 11px;
  color: var(--color-text-dim);
}

.item-check {
  color: var(--color-text-dim);
}

.item--active .item-check {
  color: var(--color-primary);
}

.selection-info {
  flex: 1;
  font-size: 10px;
  font-weight: 800;
  color: var(--color-text-dim);
  letter-spacing: 0.05em;
}

.footer-actions {
  display: flex;
  gap: var(--space-12);
}

/* States */
.state-box {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-64);
  gap: var(--space-20);
  color: var(--color-text-dim);
  text-align: center;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid var(--color-border);
  border-top-color: var(--color-primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1024px) {
  .stats-grid { grid-template-columns: 1fr; }
  .filter-grid { grid-template-columns: 1fr; }
  .line-grid { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 768px) {
  .line-grid { grid-template-columns: 1fr; }
  .page-header { flex-direction: column; align-items: flex-start; gap: var(--space-24); }
}
</style>