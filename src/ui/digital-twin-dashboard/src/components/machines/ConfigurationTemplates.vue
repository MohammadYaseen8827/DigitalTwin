<template>
  <BaseCard variant="soft" class="configuration-templates">
    <template #header>
      <div class="header-content">
        <div>
          <h3>Machine Configuration Templates</h3>
          <p>Standardize and manage machine configuration templates</p>
        </div>
        <BaseButton 
          variant="primary" 
          @click="openCreateTemplate"
          :disabled="processing"
        >
          <Plus class="icon" />
          New Template
        </BaseButton>
      </div>
    </template>

    <!-- Template List -->
    <div class="templates-grid">
      <BaseCard
        v-for="template in templates"
        :key="template.id"
        variant="bordered"
        class="template-card"
        :class="{ 'active': selectedTemplate?.id === template.id }"
        @click="selectTemplate(template)"
      >
        <div class="template-header">
          <div class="template-info">
            <h4>{{ template.name }}</h4>
            <span class="template-type">{{ template.machineType }}</span>
          </div>
          <div class="template-actions">
            <BaseButton 
              variant="ghost" 
              size="sm"
              @click.stop="applyTemplate(template)"
              :disabled="!selectedMachineId || processing"
              title="Apply to selected machine"
            >
              <Play class="icon" />
            </BaseButton>
            <BaseButton 
              variant="ghost" 
              size="sm"
              @click.stop="editTemplate(template)"
            >
              <Edit class="icon" />
            </BaseButton>
            <BaseButton 
              variant="ghost" 
              size="sm"
              @click.stop="duplicateTemplate(template)"
            >
              <Copy class="icon" />
            </BaseButton>
            <BaseButton 
              variant="ghost" 
              size="sm"
              @click.stop="deleteTemplate(template)"
              :disabled="processing"
            >
              <Trash2 class="icon" />
            </BaseButton>
          </div>
        </div>
        
        <div class="template-details">
          <p class="description">{{ template.description }}</p>
          
          <div class="template-stats">
            <div class="stat">
              <span class="stat-label">Degradation Model:</span>
              <span class="stat-value">{{ template.degradationModel.type }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">Threshold:</span>
              <span class="stat-value">{{ template.failureThresholds.degradationThreshold }}</span>
            </div>
            <div class="stat">
              <span class="stat-label">Sensors:</span>
              <span class="stat-value">{{ template.sensorMappings.length }}</span>
            </div>
          </div>
          
          <div class="tags">
            <span 
              v-for="tag in template.tags" 
              :key="tag"
              class="tag"
            >
              {{ tag }}
            </span>
          </div>
        </div>
      </BaseCard>
    </div>

    <!-- Empty State -->
    <div v-if="templates.length === 0 && !loading" class="empty-state">
      <div class="empty-content">
        <Settings class="empty-icon" />
        <h3>No Configuration Templates</h3>
        <p>Create templates to standardize machine configurations across your fleet.</p>
        <BaseButton variant="primary" @click="openCreateTemplate">
          Create First Template
        </BaseButton>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <BaseSkeleton width="100%" height="200px" />
    </div>
  </BaseCard>

  <!-- Template Editor Modal -->
  <div v-if="showEditor" class="modal-overlay" @click="closeEditor">
    <div class="modal-content" @click.stop>
      <div class="modal-header">
        <h3>{{ isEditing ? 'Edit Template' : 'Create Template' }}</h3>
        <BaseButton variant="ghost" @click="closeEditor">
          <X class="icon" />
        </BaseButton>
      </div>
      
      <div class="modal-body">
        <form @submit.prevent="saveTemplate" class="template-form">
          <div class="form-section">
            <h4>Basic Information</h4>
            
            <BaseInput
              v-model="editorForm.name"
              label="Template Name"
              placeholder="e.g., Standard CNC Configuration"
              required
              :error="formErrors.name"
            />
            
            <BaseSelect
              v-model="editorForm.machineType"
              :options="machineTypes"
              label="Machine Type"
              required
              :error="formErrors.machineType"
            />
            
            <BaseInput
              v-model="editorForm.description"
              label="Description"
              type="textarea"
              placeholder="Describe this configuration template..."
              rows="3"
            />
            
            <BaseInput
              v-model="editorTags"
              label="Tags"
              placeholder="comma, separated, tags"
              hint="Enter tags separated by commas"
            />
          </div>

          <div class="form-section">
            <h4>Degradation Model</h4>
            
            <BaseSelect
              v-model="editorForm.degradationModel.type"
              :options="degradationModels"
              label="Model Type"
              required
            />
            
            <div class="parameters-grid">
              <BaseInput
                v-for="(value, key) in editorForm.degradationModel.parameters"
                :key="key"
                v-model="editorForm.degradationModel.parameters[key]"
                :label="formatParameterLabel(key)"
                type="number"
                :step="getParameterStep(key)"
                :min="0"
              />
            </div>
          </div>

          <div class="form-section">
            <h4>Failure Thresholds</h4>
            
            <BaseInput
              v-model="editorForm.failureThresholds.degradationThreshold"
              label="Degradation Threshold"
              type="number"
              :step="0.01"
              :min="0"
              :max="1"
              required
            />
            
            <BaseInput
              v-model="editorForm.failureThresholds.temperatureThreshold"
              label="Temperature Threshold (°C)"
              type="number"
              :step="0.1"
              :min="0"
            />
            
            <BaseInput
              v-model="editorForm.failureThresholds.vibrationThreshold"
              label="Vibration Threshold"
              type="number"
              :step="0.01"
              :min="0"
            />
          </div>

          <div class="form-section">
            <h4>Sensor Mappings</h4>
            
            <div 
              v-for="(mapping, index) in editorForm.sensorMappings" 
              :key="index"
              class="sensor-mapping"
            >
              <div class="mapping-header">
                <span>Sensor {{ index + 1 }}</span>
                <BaseButton 
                  variant="ghost" 
                  size="sm"
                  @click="removeSensorMapping(index)"
                  :disabled="editorForm.sensorMappings.length <= 1"
                >
                  <Trash2 class="icon" />
                </BaseButton>
              </div>
              
              <div class="mapping-fields">
                <BaseSelect
                  v-model="mapping.sensorType"
                  :options="sensorTypes"
                  label="Sensor Type"
                  required
                />
                
                <BaseInput
                  v-model="mapping.transferFunction"
                  label="Transfer Function"
                  placeholder="e.g., linear, exponential"
                  required
                />
                
                <div class="parameters-grid">
                  <BaseInput
                    v-for="(value, paramName) in mapping.parameters"
                    :key="paramName"
                    v-model="mapping.parameters[paramName]"
                    :label="formatParameterLabel(paramName)"
                    type="number"
                    :step="getParameterStep(paramName)"
                    :min="0"
                  />
                </div>
              </div>
            </div>
            
            <BaseButton 
              variant="outline" 
              @click="addSensorMapping"
              class="add-mapping-button"
            >
              <Plus class="icon" />
              Add Sensor Mapping
            </BaseButton>
          </div>

          <div class="form-section">
            <h4>Operating Parameters</h4>
            
            <div class="parameters-grid">
              <BaseInput
                v-for="(value, key) in editorForm.operatingParameters"
                :key="key"
                v-model="editorForm.operatingParameters[key]"
                :label="formatParameterLabel(key)"
                type="number"
                :step="getParameterStep(key)"
                :min="0"
              />
              
              <BaseButton 
                variant="ghost" 
                @click="addOperatingParameter"
              >
                <Plus class="icon" />
                Add Parameter
              </BaseButton>
            </div>
          </div>

          <div class="modal-actions">
            <BaseButton 
              variant="primary" 
              type="submit"
              :loading="processing"
              :disabled="!isFormValid"
            >
              {{ isEditing ? 'Update Template' : 'Create Template' }}
            </BaseButton>
            <BaseButton 
              variant="ghost" 
              @click="closeEditor"
            >
              Cancel
            </BaseButton>
          </div>
        </form>
      </div>
    </div>
  </div>

  <!-- Apply Template Confirmation -->
  <div v-if="showApplyConfirm" class="confirmation-dialog glass-panel">
    <div class="dialog-content">
      <Settings class="settings-icon" />
      <h3>Apply Configuration Template</h3>
      <p>
        Apply "{{ selectedTemplate?.name }}" template to 
        {{ selectedMachineName || 'selected machine' }}?
        This will overwrite existing configuration.
      </p>
      <div class="dialog-actions">
        <BaseButton 
          variant="primary" 
          @click="confirmApplyTemplate"
          :loading="processing"
        >
          Apply Template
        </BaseButton>
        <BaseButton 
          variant="ghost" 
          @click="cancelApplyTemplate"
        >
          Cancel
        </BaseButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
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
  X 
} from 'lucide-vue-next'

const props = defineProps<{
  selectedMachineId?: string
  selectedMachineName?: string
}>()

const emit = defineEmits<{
  (e: 'template-applied', template: MachineConfiguration): void
}>()

const toast = useToast()

// Reactive state
const templates = ref<MachineConfiguration[]>([])
const selectedTemplate = ref<MachineConfiguration | null>(null)
const loading = ref(false)
const processing = ref(false)

// Modal state
const showEditor = ref(false)
const showApplyConfirm = ref(false)
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
  { label: 'CNC Machine', value: 'cnc' },
  { label: 'Injection Molder', value: 'injection_molder' },
  { label: 'Press', value: 'press' },
  { label: 'Robot', value: 'robot' },
  { label: 'Conveyor', value: 'conveyor' },
  { label: 'Pump', value: 'pump' },
  { label: 'Motor', value: 'motor' }
]

const degradationModels = [
  { label: 'Wiener Process', value: 'wiener' },
  { label: 'Exponential', value: 'exponential' },
  { label: 'Linear', value: 'linear' },
  { label: 'Physics-Informed', value: 'physics' }
]

const sensorTypes = [
  { label: 'Temperature', value: 'temperature' },
  { label: 'Vibration', value: 'vibration' },
  { label: 'Pressure', value: 'pressure' },
  { label: 'Flow Rate', value: 'flow_rate' },
  { label: 'Voltage', value: 'voltage' },
  { label: 'Current', value: 'current' }
]

// Methods
const loadTemplates = async () => {
  try {
    loading.value = true
    const configs = await fetchAllConfigurations()
    templates.value = configs.map(config => ({
      ...config,
      id: config.machineType, // Use machineType as ID for templates
      name: `${config.machineType.replace('_', ' ').replace(/\b\w/g, l => l.toUpperCase())} Template`,
      description: `Standard configuration for ${config.machineType.replace('_', ' ')} machines`,
      tags: ['standard', config.machineType]
    }))
  } catch (error) {
    console.error('Failed to load templates:', error)
    toast.error('Unable to load configuration templates')
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
  showEditor.value = true
}

const editTemplate = (template: MachineConfiguration) => {
  isEditing.value = true
  Object.assign(editorForm, JSON.parse(JSON.stringify(template)))
  editorTags.value = template.tags?.join(', ') || ''
  showEditor.value = true
}

const duplicateTemplate = (template: MachineConfiguration) => {
  isEditing.value = false
  const duplicated = JSON.parse(JSON.stringify(template))
  duplicated.name = `${template.name} (Copy)`
  Object.assign(editorForm, duplicated)
  editorTags.value = duplicated.tags?.join(', ') || ''
  showEditor.value = true
}

const deleteTemplate = async (template: MachineConfiguration) => {
  if (!confirm(`Delete template "${template.name}"?`)) return
  
  try {
    processing.value = true
    // In a real implementation, this would call a delete API
    templates.value = templates.value.filter(t => t.id !== template.id)
    toast.success('Template deleted successfully')
  } catch (error) {
    console.error('Delete failed:', error)
    toast.error('Failed to delete template')
  } finally {
    processing.value = false
  }
}

const saveTemplate = async () => {
  try {
    processing.value = true
    
    // Validate form
    const errors: Record<string, string> = {}
    if (!editorForm.name) errors.name = 'Name is required'
    if (!editorForm.machineType) errors.machineType = 'Machine type is required'
    
    if (Object.keys(errors).length > 0) {
      Object.assign(formErrors, errors)
      return
    }
    
    // Clear previous errors
    Object.keys(formErrors).forEach(key => delete formErrors[key])
    
    // Add tags
    const tagArray = editorTags.value
      .split(',')
      .map(tag => tag.trim())
      .filter(tag => tag)
    ;(editorForm as any).tags = tagArray
    
    // Save configuration
    await saveConfiguration(editorForm)
    
    toast.success(isEditing.value ? 'Template updated successfully' : 'Template created successfully')
    
    closeEditor()
    await loadTemplates()
    
  } catch (error) {
    console.error('Save failed:', error)
    toast.error('Failed to save template')
  } finally {
    processing.value = false
  }
}

const applyTemplate = (template: MachineConfiguration) => {
  if (!props.selectedMachineId) {
    toast.error('Please select a machine first')
    return
  }
  
  selectedTemplate.value = template
  showApplyConfirm.value = true
}

const confirmApplyTemplate = async () => {
  if (!selectedTemplate.value || !props.selectedMachineId) return
  
  try {
    processing.value = true
    
    // In a real implementation, this would apply the template to the machine
    // await applyConfigurationToMachine(props.selectedMachineId, selectedTemplate.value)
    
    toast.success(`Applied "${selectedTemplate.value.name}" to ${props.selectedMachineName}`)
    emit('template-applied', selectedTemplate.value)
    
    showApplyConfirm.value = false
  } catch (error) {
    console.error('Apply failed:', error)
    toast.error('Failed to apply template')
  } finally {
    processing.value = false
  }
}

const cancelApplyTemplate = () => {
  showApplyConfirm.value = false
}

const closeEditor = () => {
  showEditor.value = false
  resetEditorForm()
}

const resetEditorForm = () => {
  Object.assign(editorForm, {
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
  editorTags.value = ''
  Object.keys(formErrors).forEach(key => delete formErrors[key])
}

const addSensorMapping = () => {
  editorForm.sensorMappings.push({
    sensorType: 'temperature',
    transferFunction: 'linear',
    parameters: {
      slope: 1,
      intercept: 0
    }
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
  return paramName
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, str => str.toUpperCase())
    .trim()
}

const getParameterStep = (paramName: string): string => {
  if (paramName.includes('threshold') || paramName.includes('Threshold')) {
    return '0.01'
  }
  return '1'
}

// Lifecycle
onMounted(() => {
  loadTemplates()
})
</script>

<style scoped>
.configuration-templates {
  padding: var(--spacing-lg);
}

.header-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: var(--spacing-md);
}

.header-content h3 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.header-content p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
}

.templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--spacing-lg);
  margin-top: var(--spacing-lg);
}

.template-card {
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid var(--color-border-subtle);
}

.template-card:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.template-card.active {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 5%, transparent);
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--spacing-md);
}

.template-info h4 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.template-type {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  background: var(--color-surface-alt);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  text-transform: uppercase;
}

.template-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.template-details .description {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  line-height: 1.5;
}

.template-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: var(--spacing-sm);
  margin-bottom: var(--spacing-md);
}

.stat {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.stat-label {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.stat-value {
  font-weight: 500;
  color: var(--color-text-primary);
}

.tags {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-xs);
}

.tag {
  font-size: 0.75rem;
  background: color-mix(in srgb, var(--color-primary) 20%, transparent);
  color: var(--color-primary);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
}

.empty-state {
  text-align: center;
  padding: var(--spacing-2xl) var(--spacing-lg);
}

.empty-content {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  width: 4rem;
  height: 4rem;
  margin-bottom: var(--spacing-lg);
  color: var(--color-text-secondary);
  opacity: 0.5;
}

.loading-state {
  padding: var(--spacing-xl);
}

/* Modal Styles */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: var(--spacing-md);
  backdrop-filter: blur(4px);
}

.modal-content {
  background: var(--color-surface);
  border-radius: var(--radius-xl);
  width: 100%;
  max-width: 800px;
  max-height: 90vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-lg);
  border-bottom: 1px solid var(--color-border-subtle);
}

.modal-header h3 {
  margin: 0;
  color: var(--color-text-primary);
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: var(--spacing-lg);
}

.template-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xl);
}

.form-section {
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border-subtle);
}

.form-section h4 {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-primary);
  font-size: 1.125rem;
}

.parameters-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--spacing-md);
  margin-top: var(--spacing-sm);
}

.sensor-mapping {
  background: var(--color-surface);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  margin-bottom: var(--spacing-md);
  border: 1px solid var(--color-border-subtle);
}

.mapping-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
  font-weight: 500;
  color: var(--color-text-primary);
}

.mapping-fields {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.add-mapping-button {
  width: 100%;
  justify-content: center;
}

.modal-actions {
  display: flex;
  gap: var(--spacing-md);
  justify-content: flex-end;
  padding-top: var(--spacing-xl);
  border-top: 1px solid var(--color-border-subtle);
  margin-top: var(--spacing-xl);
}

/* Confirmation Dialog */
.confirmation-dialog {
  position: fixed;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  z-index: 1001;
  padding: var(--spacing-xl);
  min-width: 400px;
  max-width: 90vw;
}

.dialog-content {
  text-align: center;
}

.settings-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-primary);
}

.dialog-content h3 {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-primary);
}

.dialog-content p {
  margin: 0 0 var(--spacing-lg) 0;
  color: var(--color-text-secondary);
}

.dialog-actions {
  display: flex;
  gap: var(--spacing-sm);
  justify-content: center;
}

.icon {
  width: 1rem;
  height: 1rem;
  margin-right: var(--spacing-xs);
}

.icon:last-child {
  margin-right: 0;
  margin-left: 0;
}

@media (max-width: 768px) {
  .header-content {
    flex-direction: column;
    align-items: stretch;
  }
  
  .templates-grid {
    grid-template-columns: 1fr;
  }
  
  .template-header {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .template-actions {
    width: 100%;
    justify-content: flex-end;
  }
  
  .parameters-grid {
    grid-template-columns: 1fr;
  }
  
  .modal-content {
    margin: var(--spacing-sm);
    max-height: 95vh;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .dialog-actions {
    flex-direction: column;
  }
}
</style>