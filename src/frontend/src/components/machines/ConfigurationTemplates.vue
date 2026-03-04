<script setup lang="ts">
import { ref, computed, onMounted, reactive, useCssModule } from 'vue'
import { useToast } from '@/composables/useToast'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiModal from '@/components/ui/UiModal.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { 
  fetchAllConfigurations,
  fetchConfiguration,
  saveConfiguration,
  type MachineConfiguration
} from '@/services/machineConfiguration.service'
import { 
  Plus, 
  Edit, 
  Copy, 
  Trash2, 
  Play, 
  Settings, 
  X,
  Layers,
  Activity,
  Zap,
  Cpu,
  Database
} from 'lucide-vue-next'

const props = defineProps<{
  selectedMachineId?: string
  selectedMachineName?: string
}>()

const emit = defineEmits<{
  (e: 'template-applied', template: MachineConfiguration): void
}>()

const styles = useCssModule()
const toast = useToast()

// Reactive state
const templates = ref<MachineConfiguration[]>([])
const selectedTemplate = ref<MachineConfiguration | null>(null)
const loading = ref(false)
const processing = ref(false)

// Modal state
const isEditorOpen = ref(false)
const isApplyConfirmOpen = ref(false)
const isEditing = ref(false)

// Form state
const editorForm = reactive<MachineConfiguration>({
  machineType: '',
  degradationModel: {
    type: 'wiener',
    parameters: {
      drift: 0.01,
      volatility: 0.1
    }
  },
  failureThresholds: {
    degradationThreshold: 0.8,
    temperatureThreshold: 80,
    vibrationThreshold: 0.5
  },
  sensorMappings: [
    {
      sensorType: 'temperature',
      transferFunction: 'linear',
      parameters: {
        slope: 1,
        intercept: 0
      }
    }
  ],
  operatingParameters: {
    nominalSpeed: 1000,
    maxLoad: 500
  }
})

const formErrors = reactive<Record<string, string>>({})
const editorTags = ref('')

// Computed properties
const isFormValid = computed(() => {
  return editorForm.name && 
         editorForm.machineType && 
         editorForm.degradationModel.type &&
         editorForm.failureThresholds.degradationThreshold > 0
})

// Options
const machineTypes = [
  { label: 'CNC MACHINE', value: 'cnc' },
  { label: 'INJECTION MOLDER', value: 'injection_molder' },
  { label: 'PRESS', value: 'press' },
  { label: 'ROBOT', value: 'robot' },
  { label: 'CONVEYOR', value: 'conveyor' },
  { label: 'PUMP', value: 'pump' },
  { label: 'MOTOR', value: 'motor' }
]

const degradationModels = [
  { label: 'WIENER PROCESS', value: 'wiener' },
  { label: 'EXPONENTIAL', value: 'exponential' },
  { label: 'LINEAR', value: 'linear' },
  { label: 'PHYSICS-INFORMED', value: 'physics' }
]

const sensorTypes = [
  { label: 'TEMPERATURE', value: 'temperature' },
  { label: 'VIBRATION', value: 'vibration' },
  { label: 'PRESSURE', value: 'pressure' },
  { label: 'FLOW RATE', value: 'flow_rate' },
  { label: 'VOLTAGE', value: 'voltage' },
  { label: 'CURRENT', value: 'current' }
]

// Methods
const loadTemplates = async () => {
  try {
    loading.value = true
    const configs = await fetchAllConfigurations()
    templates.value = configs.map(config => ({
      ...config,
      id: config.machineType,
      name: `${config.machineType.replace('_', ' ').replace(/\b\w/g, l => l.toUpperCase())} Template`,
      description: `Standard configuration for ${config.machineType.replace('_', ' ')} machines`,
      tags: ['standard', config.machineType]
    }))
  } catch (error) {
    toast.error('Template registry offline')
  } finally {
    loading.value = false
  }
}

const selectTemplate = (template: MachineConfiguration) => {
  selectedTemplate.value = template
}

const openCreateTemplate = () => {
  isEditing.value = false
  resetEditorForm()
  isEditorOpen.value = true
}

const editTemplate = (template: MachineConfiguration) => {
  isEditing.value = true
  Object.assign(editorForm, JSON.parse(JSON.stringify(template)))
  editorTags.value = template.tags?.join(', ') || ''
  isEditorOpen.value = true
}

const duplicateTemplate = (template: MachineConfiguration) => {
  isEditing.value = false
  const duplicated = JSON.parse(JSON.stringify(template))
  duplicated.name = `${template.name} (Copy)`
  Object.assign(editorForm, duplicated)
  editorTags.value = duplicated.tags?.join(', ') || ''
  isEditorOpen.value = true
}

const deleteTemplate = async (template: MachineConfiguration) => {
  if (!confirm(`Delete template "${template.name}"?`)) return
  
  try {
    processing.value = true
    templates.value = templates.value.filter(t => t.id !== template.id)
    toast.success('Template purged')
  } catch {
    toast.error('Purge failure')
  } finally {
    processing.value = false
  }
}

const saveTemplate = async () => {
  try {
    processing.value = true
    
    const errors: Record<string, string> = {}
    if (!editorForm.name) errors.name = 'Required'
    if (!editorForm.machineType) errors.machineType = 'Required'
    
    if (Object.keys(errors).length > 0) {
      Object.assign(formErrors, errors)
      return
    }
    
    Object.keys(formErrors).forEach(key => delete formErrors[key])
    
    const tagArray = editorTags.value.split(',').map(tag => tag.trim()).filter(tag => tag)
    ;(editorForm as any).tags = tagArray
    
    await saveConfiguration(editorForm)
    toast.success(isEditing.value ? 'Template synchronized' : 'Template initialized')
    isEditorOpen.value = false
    await loadTemplates()
  } catch {
    toast.error('Commit failed')
  } finally {
    processing.value = false
  }
}

const applyTemplate = (template: MachineConfiguration) => {
  if (!props.selectedMachineId) {
    toast.error('No target asset selected')
    return
  }
  selectedTemplate.value = template
  isApplyConfirmOpen.value = true
}

const confirmApplyTemplate = async () => {
  if (!selectedTemplate.value || !props.selectedMachineId) return
  
  try {
    processing.value = true
    toast.success(`Protocol "${selectedTemplate.value.name}" applied`)
    emit('template-applied', selectedTemplate.value)
    isApplyConfirmOpen.value = false
  } catch {
    toast.error('Application failed')
  } finally {
    processing.value = false
  }
}

const closeEditor = () => {
  isEditorOpen.value = false
  resetEditorForm()
}

const resetEditorForm = () => {
  Object.assign(editorForm, {
    machineType: '',
    degradationModel: {
      type: 'wiener',
      parameters: { drift: 0.01, volatility: 0.1 }
    },
    failureThresholds: {
      degradationThreshold: 0.8,
      temperatureThreshold: 80,
      vibrationThreshold: 0.5
    },
    sensorMappings: [
      {
        sensorType: 'temperature',
        transferFunction: 'linear',
        parameters: { slope: 1, intercept: 0 }
      }
    ],
    operatingParameters: {
      nominalSpeed: 1000,
      maxLoad: 500
    }
  })
  editorTags.value = ''
  Object.keys(formErrors).forEach(key => delete formErrors[key])
}

const addSensorMapping = () => {
  editorForm.sensorMappings.push({
    sensorType: 'temperature',
    transferFunction: 'linear',
    parameters: { slope: 1, intercept: 0 }
  })
}

const removeSensorMapping = (index: number) => {
  if (editorForm.sensorMappings.length > 1) {
    editorForm.sensorMappings.splice(index, 1)
  }
}

const addOperatingParameter = () => {
  const paramName = `param_${Date.now()}`
  editorForm.operatingParameters[paramName] = 0
}

const formatParameterLabel = (paramName: string): string => {
  return paramName.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase()).trim()
}

onMounted(loadTemplates)
</script>

<template>
  <div :class="styles['templates-container']">
    <header :class="styles['section-header']">
      <div :class="styles['header-main']">
        <h3 :class="styles['title']">Configuration Blueprints</h3>
        <p :class="styles['description']">Standardized protocols for autonomous asset fleets</p>
      </div>
      <UiButton variant="primary" @click="openCreateTemplate">
        <Plus :width="16" :height="16" />
        New Blueprint
      </UiButton>
    </header>

    <!-- Template List -->
    <div v-if="templates.length > 0" :class="styles['templates-grid']">
      <UiCard
        v-for="template in templates"
        :key="template.id"
        variant="default"
        hover
        :class="[styles['template-card'], selectedTemplate?.id === template.id && styles['item--active']]"
        @click="selectTemplate(template)"
      >
        <div :class="styles['card-header']">
          <div :class="styles['template-info']">
            <h4 :class="styles['template-name']">{{ template.name }}</h4>
            <UiBadge variant="secondary" size="sm">{{ template.machineType.toUpperCase() }}</UiBadge>
          </div>
          <div :class="styles['template-actions']">
            <UiButton variant="ghost" size="sm" @click.stop="applyTemplate(template)" :disabled="!selectedMachineId">
              <Play :width="14" :height="14" />
            </UiButton>
            <UiButton variant="ghost" size="sm" @click.stop="editTemplate(template)">
              <Edit :width="14" :height="14" />
            </UiButton>
            <UiButton variant="ghost" size="sm" @click.stop="duplicateTemplate(template)">
              <Copy :width="14" :height="14" />
            </UiButton>
            <UiButton variant="ghost" size="sm" :class="styles['btn-danger']" @click.stop="deleteTemplate(template)">
              <Trash2 :width="14" :height="14" />
            </UiButton>
          </div>
        </div>
        
        <div :class="styles['card-body']">
          <p :class="styles['template-desc']">{{ template.description }}</p>
          
          <div :class="styles['stats-grid']">
            <div :class="styles['stat-item']">
              <span :class="styles['stat-label']">Model Class</span>
              <span :class="styles['stat-val']">{{ template.degradationModel.type }}</span>
            </div>
            <div :class="styles['stat-item']">
              <span :class="styles['stat-label']">Threshold</span>
              <span :class="styles['stat-val']">{{ template.failureThresholds.degradationThreshold }}</span>
            </div>
            <div :class="styles['stat-item']">
              <span :class="styles['stat-label']">Sensors</span>
              <span :class="styles['stat-val']">{{ template.sensorMappings.length }} Nodes</span>
            </div>
          </div>
        </div>
      </UiCard>
    </div>

    <!-- Empty State -->
    <div v-else-if="!loading" :class="styles['state-box']">
      <Settings :width="48" :height="48" :class="styles['state-icon']" />
      <h3>No Blueprints Defined</h3>
      <p>Initialize standardized configurations for fleet orchestration.</p>
      <UiButton variant="secondary" @click="openCreateTemplate">Initialize First Blueprint</UiButton>
    </div>

    <!-- Blueprint Editor Modal -->
    <UiModal :is-open="isEditorOpen" maxWidth="xl" @close="closeEditor">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">BLUEPRINT ARCHITECT</div>
            <h2 :class="styles['modal-title']">{{ isEditing ? 'Refine Blueprint' : 'Architect Blueprint' }}</h2>
            <p :class="styles['modal-sub']">Designing structural protocols for digital twin synchronization</p>
          </div>
        </template>

        <form @submit.prevent="saveTemplate" :class="styles['modal-form']">
          <div :class="styles['form-sections']">
            <!-- Basic Info -->
            <section :class="styles['form-section']">
              <h4 :class="styles['section-title']"><Layers :width="14" :height="14" /> Baseline Parameters</h4>
              <div :class="styles['field-grid']">
                <UiInput v-model="editorForm.name" label="Blueprint Identifier" placeholder="e.g., PRECISION-CNC-STND" required />
                <UiSelect v-model="editorForm.machineType" label="Asset Class" :options="machineTypes" required />
                <div :class="styles['full-width']">
                  <UiInput v-model="editorForm.description" label="Scope Description" />
                </div>
              </div>
            </section>

            <!-- Models -->
            <section :class="styles['form-section']">
              <h4 :class="styles['section-title']"><Activity :width="14" :height="14" /> Degradation Modeling</h4>
              <div :class="styles['field-grid']">
                <UiSelect v-model="editorForm.degradationModel.type" label="Analysis Engine" :options="degradationModels" required />
                <UiInput v-model.number="editorForm.failureThresholds.degradationThreshold" type="number" step="0.01" label="Critical Threshold" required />
              </div>
            </section>

            <!-- Sensors -->
            <section :class="styles['form-section']">
              <h4 :class="styles['section-title']"><Zap :width="14" :height="14" /> Telemetry Mappings</h4>
              <div :class="styles['sensor-list']">
                <div v-for="(mapping, index) in editorForm.sensorMappings" :key="index" :class="styles['sensor-item']">
                  <div :class="styles['sensor-header']">
                    <span>NODE {{ index + 1 }}</span>
                    <UiButton v-if="editorForm.sensorMappings.length > 1" variant="ghost" size="sm" @click="removeSensorMapping(index)">
                      <Trash2 :width="12" :height="12" />
                    </UiButton>
                  </div>
                  <div :class="styles['field-grid']">
                    <UiSelect v-model="mapping.sensorType" label="Sensor Class" :options="sensorTypes" required />
                    <UiInput v-model="mapping.transferFunction" label="Transfer Logic" required />
                  </div>
                </div>
                <UiButton variant="ghost" size="sm" @click="addSensorMapping" :class="styles['add-btn']">
                  <Plus :width="14" :height="14" />
                  Map Additional Node
                </UiButton>
              </div>
            </section>
          </div>

          <div :class="styles['modal-footer']">
            <UiButton variant="ghost" @click="closeEditor">Abort</UiButton>
            <UiButton variant="primary" type="submit" :loading="processing" :disabled="!isFormValid">
              {{ isEditing ? 'Commit Changes' : 'Initialize Blueprint' }}
            </UiButton>
          </div>
        </form>
      </UiCard>
    </UiModal>

    <!-- Apply Confirmation -->
    <UiModal :is-open="isApplyConfirmOpen" maxWidth="sm" @close="isApplyConfirmOpen = false">
      <UiCard variant="glass" padding="xl">
        <template #header>
          <div :class="styles['modal-header']">
            <div :class="styles['modal-eyebrow']">PROTOCOL DEPLOYMENT</div>
            <h2 :class="styles['modal-title']">Sync Blueprint</h2>
          </div>
        </template>
        <div :class="styles['confirm-body']">
          <p>Deploying blueprint <strong>{{ selectedTemplate?.name }}</strong> to <strong>{{ selectedMachineName }}</strong> will override current operational parameters.</p>
        </div>
        <div :class="styles['modal-footer']">
          <UiButton variant="ghost" @click="isApplyConfirmOpen = false">Abort</UiButton>
          <UiButton variant="primary" @click="confirmApplyTemplate" :loading="processing">Synchronize</UiButton>
        </div>
      </UiCard>
    </UiModal>
  </div>
</template>

<style module>
.templates-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: var(--space-16);
  border-bottom: 1px solid var(--color-border-subtle);
}

.title {
  font-size: var(--font-size-xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
  letter-spacing: -0.01em;
}

.description {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  margin-top: 4px;
}

/* Grid */
.templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
  gap: var(--space-20);
}

.template-card {
  height: 100%;
}

.card-header {
  padding: var(--space-20);
  border-bottom: 1px solid var(--color-border-subtle);
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.template-name {
  margin: 0 0 var(--space-8) 0;
  font-size: var(--font-size-base);
  font-weight: 750;
  color: var(--color-text-primary);
}

.template-actions {
  display: flex;
  gap: 4px;
}

.card-body {
  padding: var(--space-20);
  display: flex;
  flex-direction: column;
  gap: var(--space-20);
}

.template-desc {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  line-height: var(--font-lineheight-relaxed);
  margin: 0;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-12);
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.stat-label {
  font-size: 9px;
  font-weight: 800;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.stat-val {
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-primary);
}

.btn-danger { color: var(--color-danger); }

/* Modals */
.modal-header { margin-bottom: var(--space-32); }
.modal-eyebrow { font-size: 10px; font-weight: 900; color: var(--color-primary); letter-spacing: 0.2em; margin-bottom: var(--space-4); }
.modal-title { font-size: var(--font-size-2xl); font-weight: 800; color: var(--color-text-primary); margin: 0; }
.modal-sub { font-size: var(--font-size-sm); color: var(--color-text-muted); margin-top: var(--space-6); }

.modal-form { display: flex; flex-direction: column; gap: var(--space-32); }

.form-sections {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  max-height: 60vh;
  overflow-y: auto;
  padding-right: var(--space-8);
}

.form-section {
  padding: var(--space-20);
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.section-title {
  font-size: var(--font-size-xs);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-20) 0;
  display: flex;
  align-items: center;
  gap: var(--space-10);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.field-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-16); }
.full-width { grid-column: 1 / -1; }

.sensor-list { display: flex; flex-direction: column; gap: var(--space-12); }
.sensor-item { padding: var(--space-12); background: var(--color-surface); border: 1px solid var(--color-border-subtle); border-radius: var(--radius-md); }
.sensor-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: var(--space-12); font-size: 10px; font-weight: 900; color: var(--color-text-dim); }

.add-btn { width: 100%; border: 1px dashed var(--color-border-strong); margin-top: var(--space-8); }

.modal-footer { display: flex; justify-content: flex-end; gap: var(--space-12); padding-top: var(--space-24); border-top: 1px solid var(--color-border-subtle); }

.confirm-body { font-size: var(--font-size-sm); line-height: var(--font-lineheight-relaxed); color: var(--color-text-secondary); }

.state-box { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: var(--space-48); gap: var(--space-16); color: var(--color-text-dim); text-align: center; }

@media (max-width: 640px) {
  .field-grid { grid-template-columns: 1fr; }
}
</style>
