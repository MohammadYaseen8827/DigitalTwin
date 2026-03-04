<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchAllConfigurations,
  saveConfiguration,
  deleteConfiguration,
  validateConfiguration,
  type MachineConfiguration
} from '@/services/machineConfiguration.service'
import { 
  Settings, 
  Plus, 
  Edit, 
  Copy, 
  Trash2, 
  Play, 
  Filter,
  Search,
  RefreshCw,
  Upload,
  Download,
  Code,
  Tag,
  Database
} from 'lucide-vue-next'

const toast = useToast()

// State
const templates: any = ref<MachineConfiguration[]>([])
const loading = ref(false)
const searchQuery = ref('')
const typeFilter = ref('all')
const tagFilter = ref('all')

// Modal state
const showCreateModal = ref(false)
const showEditModal = ref(false)
const showImportModal = ref(false)
const selectedTemplate: any = ref(null)

// Form state
const templateForm: any = ref({
  name: '',
  machineType: '',
  description: '',
  tags: [] as string[],
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
  sensorMappings: [] as any[],
  operatingParameters: {}
})

const importForm = ref({
  jsonContent: '',
  validateOnly: false
})

// Computed
const filteredTemplates = computed(() => {
  let filtered = [...templates.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter((template: any) => 
      template.name?.toLowerCase().includes(query) ||
      template.machineType?.toLowerCase().includes(query) ||
      template.description?.toLowerCase().includes(query) ||
      template.tags?.some((tag: string) => tag.toLowerCase().includes(query))
    )
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter((template: any) => template.machineType === typeFilter.value)
  }
  
  // Apply tag filter
  if (tagFilter.value !== 'all') {
    filtered = filtered.filter((template: any) => template.tags?.includes(tagFilter.value))
  }
  
  return filtered
})

const typeOptions = computed(() => {
  const types = [...new Set(templates.value.map((t: any) => t.machineType))]
  return [
    { label: 'All Types', value: 'all' },
    ...types.map((type: unknown) => ({ label: String(type), value: String(type) }))
  ]
})

const tagOptions = computed(() => {
  const allTags = templates.value.flatMap((t: any) => t.tags || [])
  const uniqueTags = [...new Set(allTags)]
  return [
    { label: 'All Tags', value: 'all' },
    ...uniqueTags.map((tag: unknown) => ({ label: String(tag), value: String(tag) }))
  ]
})

const degradationModelOptions = [
  { label: 'Wiener Process', value: 'wiener' },
  { label: 'Exponential', value: 'exponential' },
  { label: 'Markov Chain', value: 'markov' },
  { label: 'Physics-Informed', value: 'physics' }
]

const sensorTypeOptions = [
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
    
    // Transform configurations to templates with additional metadata
    templates.value = configs.map((config: any) => ({
      ...config,
      id: config.machineType,
      name: config.name || `${config.machineType.replace('_', ' ').replace(/\b\w/g, (l: string) => l.toUpperCase())} Template`,
      description: config.description || `Standard configuration for ${config.machineType.replace('_', ' ')} machines`,
      tags: config.tags || ['standard', config.machineType],
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      usageCount: config.usageCount || 0
    }))
  } catch (error) {
    console.error('Failed to load templates:', error)
    toast.error('Unable to load configuration templates')
  } finally {
    loading.value = false
  }
}

const refreshTemplates = async () => {
  await loadTemplates()
  toast.success('Templates refreshed')
}

const openCreateModal = () => {
  resetTemplateForm()
  showCreateModal.value = true
}

const openEditModal = (template: any) => {
  selectedTemplate.value = template
  templateForm.value = JSON.parse(JSON.stringify(template))
  showEditModal.value = true
}

const openImportModal = () => {
  importForm.value = {
    jsonContent: '',
    validateOnly: false
  }
  showImportModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  showImportModal.value = false
  selectedTemplate.value = null
  resetTemplateForm()
}

const resetTemplateForm = () => {
  templateForm.value = {
    name: '',
    machineType: '',
    description: '',
    tags: [],
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
    sensorMappings: [],
    operatingParameters: {}
  }
}

const addSensorMapping = () => {
  templateForm.value.sensorMappings.push({
    sensorType: 'temperature',
    transferFunction: 'linear',
    parameters: {
      gain: 1.0,
      offset: 0.0
    }
  })
}

const removeSensorMapping = (index: number) => {
  templateForm.value.sensorMappings.splice(index, 1)
}

const addTag = (tag: string) => {
  if (tag && !templateForm.value.tags.includes(tag)) {
    templateForm.value.tags.push(tag)
  }
}

const removeTag = (tag: string) => {
  const index = templateForm.value.tags.indexOf(tag)
  if (index > -1) {
    templateForm.value.tags.splice(index, 1)
  }
}

const handleCreateTemplate = async () => {
  try {
    await saveConfiguration(templateForm.value)
    toast.success('Template created successfully')
    closeModals()
    await loadTemplates()
  } catch (error) {
    console.error('Failed to create template:', error)
    toast.error('Failed to create template')
  }
}

const handleUpdateTemplate = async () => {
  if (!selectedTemplate.value) return
  
  try {
    await saveConfiguration(templateForm.value)
    toast.success('Template updated successfully')
    closeModals()
    await loadTemplates()
  } catch (error) {
    console.error('Failed to update template:', error)
    toast.error('Failed to update template')
  }
}

const handleDeleteTemplate = async (template: any) => {
  if (!confirm(`Are you sure you want to delete template "${template.name}"?`)) {
    return
  }
  
  try {
    await deleteConfiguration(template.machineType)
    templates.value = templates.value.filter((t: any) => t.id !== template.id)
    toast.success('Template deleted successfully')
  } catch (error: any) {
    if (error.message?.includes('not yet implemented')) {
      toast.error('Delete functionality not available in backend')
    } else {
      console.error('Failed to delete template:', error)
      toast.error('Failed to delete template')
    }
  }
}

const handleDuplicateTemplate = async (template: any) => {
  const duplicated = JSON.parse(JSON.stringify(template))
  duplicated.name = `${template.name} (Copy)`
  duplicated.machineType = `${template.machineType}_copy_${Date.now()}`
  duplicated.tags = [...template.tags, 'copy']
  
  try {
    await saveConfiguration(duplicated)
    toast.success('Template duplicated successfully')
    await loadTemplates()
  } catch (error) {
    console.error('Failed to duplicate template:', error)
    toast.error('Failed to duplicate template')
  }
}

const handleImportJson = async () => {
  try {
    if (importForm.value.validateOnly) {
      const result = await validateConfiguration(importForm.value.jsonContent)
      if (result.valid) {
        toast.success('JSON is valid')
      } else {
        toast.error(`Invalid JSON: ${result.message}`)
      }
    } else {
      const config = JSON.parse(importForm.value.jsonContent)
      await saveConfiguration(config)
      toast.success('Configuration imported successfully')
      closeModals()
      await loadTemplates()
    }
  } catch (error: any) {
    if (error instanceof SyntaxError) {
      toast.error('Invalid JSON format')
    } else {
      console.error('Failed to import configuration:', error)
      toast.error('Failed to import configuration')
    }
  }
}

const exportTemplate = (template: any) => {
  const dataStr = JSON.stringify(template, null, 2)
  const dataBlob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(dataBlob)
  const link = document.createElement('a')
  link.href = url
  link.download = `${template.machineType}_template.json`
  link.click()
  URL.revokeObjectURL(url)
  toast.success('Template exported')
}

const formatParameterLabel = (key: string): string => {
  return key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase())
}

const getParameterStep = (key: string): string => {
  return key.includes('Threshold') ? '0.01' : '0.001'
}

// Lifecycle
onMounted(async () => {
  await loadTemplates()
})
</script>

<template>
  <BaseCard>
    <div class="configuration-templates-manager space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Configuration Templates Manager</h1>
          <p class="text-gray-600 mt-2">Create, manage, and deploy standardized machine configuration templates</p>
        </div>
        <div class="flex gap-2">
          <BaseButton variant="outline" @click="refreshTemplates">
            <RefreshCw class="w-4 h-4 mr-2" />
            Refresh
          </BaseButton>
          <BaseButton variant="outline" @click="openImportModal">
            <Upload class="w-4 h-4 mr-2" />
            Import
          </BaseButton>
          <BaseButton variant="primary" @click="openCreateModal">
            <Plus class="w-4 h-4 mr-2" />
            New Template
          </BaseButton>
        </div>
      </div>

      <!-- Summary Cards -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Templates</p>
                <p class="text-2xl font-bold">{{ templates.length }}</p>
              </div>
              <Database class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Machine Types</p>
                <p class="text-2xl font-bold">
                  {{ [...new Set(templates.map((t: any) => t.machineType))].length }}
                </p>
              </div>
              <Settings class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Tags</p>
                <p class="text-2xl font-bold">
                  {{ [...new Set(templates.flatMap((t: any) => t.tags || []))].length }}
                </p>
              </div>
              <Tag class="w-8 h-8 text-purple-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Avg Usage</p>
                <p class="text-2xl font-bold">
                  {{ Math.round(templates.reduce((sum: number, t: any) => sum + (t.usageCount || 0), 0) / templates.length) || 0 }}
                </p>
              </div>
              <Play class="w-8 h-8 text-orange-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Filters -->
      <BaseCard>
        <div class="p-4 space-y-4">
          <div class="flex flex-col sm:flex-row gap-4">
            <div class="flex-1">
              <div class="relative">
                <Search class="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                <BaseInput
                  v-model="searchQuery"
                  placeholder="Search templates..."
                  class="pl-10"
                />
              </div>
            </div>
            
            <BaseSelect
              v-model="typeFilter"
              :options="typeOptions"
            />
            
            <BaseSelect
              v-model="tagFilter"
              :options="tagOptions"
            />
          </div>
        </div>
      </BaseCard>

      <!-- Templates Grid -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">
            Configuration Templates ({{ filteredTemplates.length }})
          </h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading templates...</p>
          </div>
          
          <div v-else-if="filteredTemplates.length === 0" class="text-center py-8">
            <Settings class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No templates found</p>
            <p class="text-sm text-gray-500 mt-1">Create your first template or adjust your filters</p>
          </div>
          
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <BaseCard
              v-for="template in filteredTemplates"
              :key="template.id"
              class="hover:shadow-md transition-shadow cursor-pointer"
              @click="openEditModal(template)"
            >
              <div class="p-4">
                <div class="flex items-start justify-between mb-3">
                  <div>
                    <h3 class="text-lg font-semibold text-gray-900">{{ template.name }}</h3>
                    <p class="text-sm text-gray-600">{{ template.machineType }}</p>
                  </div>
                  <span class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                    {{ template.degradationModel.type }}
                  </span>
                </div>
                
                <p class="text-gray-600 text-sm mb-3 line-clamp-2">
                  {{ template.description }}
                </p>
                
                <div class="flex flex-wrap gap-1 mb-3">
                  <span 
                    v-for="tag in template.tags" 
                    :key="tag"
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
                  >
                    {{ tag }}
                  </span>
                </div>
                
                <div class="grid grid-cols-3 gap-2 text-xs text-gray-500 mb-3">
                  <div>
                    <span class="font-medium">Sensors:</span>
                    <p>{{ template.sensorMappings?.length || 0 }}</p>
                  </div>
                  <div>
                    <span class="font-medium">Threshold:</span>
                    <p>{{ template.failureThresholds?.degradationThreshold || 0 }}</p>
                  </div>
                  <div>
                    <span class="font-medium">Used:</span>
                    <p>{{ template.usageCount || 0 }}x</p>
                  </div>
                </div>
                
                <div class="flex justify-between items-center">
                  <div class="text-xs text-gray-500">
                    Updated: {{ new Date(template.updatedAt).toLocaleDateString() }}
                  </div>
                  <div class="flex gap-1">
                    <BaseButton
                      variant="ghost"
                      size="sm"
                      @click.stop="exportTemplate(template)"
                      title="Export template"
                    >
                      <Download class="w-3 h-3" />
                    </BaseButton>
                    <BaseButton
                      variant="ghost"
                      size="sm"
                      @click.stop="handleDuplicateTemplate(template)"
                      title="Duplicate template"
                    >
                      <Copy class="w-3 h-3" />
                    </BaseButton>
                    <BaseButton
                      variant="ghost"
                      size="sm"
                      @click.stop="handleDeleteTemplate(template)"
                      title="Delete template"
                      class="text-red-600 hover:text-red-700"
                    >
                      <Trash2 class="w-3 h-3" />
                    </BaseButton>
                  </div>
                </div>
              </div>
            </BaseCard>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>

  <!-- Create/Edit Template Modal -->
  <div 
    v-if="showCreateModal || showEditModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-4xl max-h-[90vh] overflow-hidden" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">
          {{ showEditModal ? 'Edit Template' : 'Create Template' }}
        </h2>
        
        <div class="overflow-y-auto max-h-[70vh] pr-4">
          <form @submit.prevent="showEditModal ? handleUpdateTemplate() : handleCreateTemplate()" class="space-y-6">
            <!-- Basic Information -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold border-b pb-2">Basic Information</h3>
              
              <BaseInput
                v-model="templateForm.name"
                label="Template Name"
                placeholder="e.g., Standard CNC Configuration"
                required
              />
              
              <BaseInput
                v-model="templateForm.machineType"
                label="Machine Type"
                placeholder="e.g., cnc_mill"
                required
              />
              
              <BaseInput
                v-model="templateForm.description"
                label="Description"
                type="textarea"
                placeholder="Describe this configuration template..."
                rows="3"
              />
              
              <div>
                <label class="block text-sm font-medium text-gray-700 mb-2">Tags</label>
                <div class="flex gap-2">
                  <input
                    type="text"
                    placeholder="Add tag and press Enter"
                    class="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                    @keyup.enter="addTag(($event.target as HTMLInputElement).value); ($event.target as HTMLInputElement).value = ''"
                  />
                </div>
                <div class="flex flex-wrap gap-2 mt-2">
                  <span 
                    v-for="tag in templateForm.tags" 
                    :key="tag"
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800"
                  >
                    {{ tag }}
                    <button 
                      type="button" 
                      @click="removeTag(tag)"
                      class="ml-1 text-blue-600 hover:text-blue-800"
                    >
                      ×
                    </button>
                  </span>
                </div>
              </div>
            </div>

            <!-- Degradation Model -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold border-b pb-2">Degradation Model</h3>
              
              <BaseSelect
                v-model="templateForm.degradationModel.type"
                :options="degradationModelOptions"
                label="Model Type"
                required
              />
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <BaseInput
                  v-for="(value, key) in templateForm.degradationModel.parameters"
                  :key="String(key)"
                  v-model.number="templateForm.degradationModel.parameters[key]"
                  :label="formatParameterLabel(String(key))"
                  type="number"
                  :step="getParameterStep(String(key))"
                  :min="0"
                />
              </div>
            </div>

            <!-- Failure Thresholds -->
            <div class="space-y-4">
              <h3 class="text-lg font-semibold border-b pb-2">Failure Thresholds</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <BaseInput
                  v-model.number="templateForm.failureThresholds.degradationThreshold"
                  label="Degradation Threshold"
                  type="number"
                  step="0.01"
                  min="0"
                  max="1"
                  required
                />
                <BaseInput
                  v-model.number="templateForm.failureThresholds.temperatureThreshold"
                  label="Temperature Threshold (°C)"
                  type="number"
                  min="0"
                />
                <BaseInput
                  v-model.number="templateForm.failureThresholds.vibrationThreshold"
                  label="Vibration Threshold"
                  type="number"
                  step="0.01"
                  min="0"
                />
              </div>
            </div>

            <!-- Sensor Mappings -->
            <div class="space-y-4">
              <div class="flex justify-between items-center">
                <h3 class="text-lg font-semibold border-b pb-2">Sensor Mappings</h3>
                <BaseButton
                  variant="outline"
                  size="sm"
                  @click="addSensorMapping"
                >
                  <Plus class="w-4 h-4 mr-1" />
                  Add Sensor
                </BaseButton>
              </div>
              
              <div 
                v-for="(mapping, index) in templateForm.sensorMappings" 
                :key="index"
                class="p-4 border rounded-lg space-y-3"
              >
                <div class="flex justify-between items-center">
                  <h4 class="font-medium">Sensor {{ Number(index) + 1 }}</h4>
                  <BaseButton
                    variant="ghost"
                    size="sm"
                    @click="removeSensorMapping(Number(index))"
                    class="text-red-600"
                  >
                    <Trash2 class="w-4 h-4" />
                  </BaseButton>
                </div>
                
                <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <BaseSelect
                    v-model="mapping.sensorType"
                    :options="sensorTypeOptions"
                    label="Sensor Type"
                  />
                  <BaseInput
                    v-model="mapping.transferFunction"
                    label="Transfer Function"
                    placeholder="e.g., linear"
                  />
                  <BaseInput
                    v-model.number="mapping.parameters.gain"
                    label="Gain"
                    type="number"
                    step="0.01"
                  />
                </div>
              </div>
            </div>

            <div class="flex justify-end gap-3 pt-6">
              <BaseButton variant="ghost" @click="closeModals">
                Cancel
              </BaseButton>
              <BaseButton variant="primary" type="submit">
                {{ showEditModal ? 'Update' : 'Create' }} Template
              </BaseButton>
            </div>
          </form>
        </div>
      </div>
    </BaseCard>
  </div>

  <!-- Import JSON Modal -->
  <div 
    v-if="showImportModal" 
    class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50"
    @click="closeModals"
  >
    <BaseCard class="w-full max-w-2xl" @click.stop>
      <div class="p-6">
        <h2 class="text-2xl font-bold mb-6">Import Configuration</h2>
        
        <form @submit.prevent="handleImportJson" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">
              JSON Configuration
            </label>
            <textarea
              v-model="importForm.jsonContent"
              rows="10"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
              placeholder='{&#10;  "machineType": "cnc_mill",&#10;  "degradationModel": {&#10;    "type": "wiener",&#10;    "parameters": {&#10;      "drift": 0.01,&#10;      "volatility": 0.1&#10;    }&#10;  }&#10;}'
              required
            ></textarea>
          </div>
          
          <div class="flex items-center">
            <input
              id="validate-only"
              v-model="importForm.validateOnly"
              type="checkbox"
              class="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <label for="validate-only" class="ml-2 block text-sm text-gray-900">
              Validate only (don't save)
            </label>
          </div>
          
          <div class="flex justify-end gap-3 pt-4">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              {{ importForm.validateOnly ? 'Validate' : 'Import' }} Configuration
            </BaseButton>
          </div>
        </form>
      </div>
    </BaseCard>
  </div>
</template>

<style scoped>
.configuration-templates-manager {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

@media (max-width: 640px) {
  .configuration-templates-manager {
    padding: 0.5rem;
  }
}
</style>