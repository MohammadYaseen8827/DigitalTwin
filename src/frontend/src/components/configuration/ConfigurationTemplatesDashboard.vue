<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { 
  Copy, 
  Edit, 
  Trash2, 
  Download,
  Upload,
  Settings,
  Filter,
  Search,
  Plus,
  Save
} from 'lucide-vue-next'

const toast = useToast()

// State
const templates = ref<any[]>([
  {
    id: 'TPL001',
    name: 'Standard CNC Configuration',
    type: 'machine',
    category: 'manufacturing',
    version: '1.2.0',
    author: 'Admin',
    description: 'Default configuration template for CNC machining centers',
    parameters: {
      spindle_speed: 12000,
      feed_rate: 800,
      coolant_pressure: 45,
      tool_offset: 0.1
    },
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    updatedAt: new Date().toISOString(),
    isPublic: true
  },
  {
    id: 'TPL002',
    name: 'Injection Molding Profile',
    type: 'machine',
    category: 'manufacturing',
    version: '2.1.0',
    author: 'Engineering',
    description: 'Optimized settings for injection molding machines',
    parameters: {
      injection_pressure: 120,
      mold_temperature: 85,
      cycle_time: 45,
      cooling_time: 15
    },
    createdAt: new Date(Date.now() - 172800000).toISOString(),
    updatedAt: new Date(Date.now() - 86400000).toISOString(),
    isPublic: true
  },
  {
    id: 'TPL003',
    name: 'Predictive Maintenance Settings',
    type: 'system',
    category: 'maintenance',
    version: '1.0.0',
    author: 'Maintenance',
    description: 'Configuration for predictive maintenance algorithms',
    parameters: {
      sampling_interval: 300,
      threshold_vibration: 0.5,
      threshold_temperature: 80,
      prediction_window: 30
    },
    createdAt: new Date(Date.now() - 259200000).toISOString(),
    updatedAt: new Date().toISOString(),
    isPublic: false
  }
])

const loading = ref(false)
const searchQuery = ref('')
const typeFilter = ref('all')
const categoryFilter = ref('all')
const showCreateModal = ref(false)
const showEditModal = ref(false)
const selectedTemplate = ref<any>(null)

// Form state
const templateForm = ref({
  name: '',
  type: 'machine',
  category: 'manufacturing',
  description: '',
  parameters: '{}',
  isPublic: true
})

// Computed
const filteredTemplates = computed(() => {
  let filtered = [...templates.value]
  
  // Apply search filter
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    filtered = filtered.filter(template => 
      template.name.toLowerCase().includes(query) ||
      template.description.toLowerCase().includes(query) ||
      template.author.toLowerCase().includes(query)
    )
  }
  
  // Apply type filter
  if (typeFilter.value !== 'all') {
    filtered = filtered.filter(template => template.type === typeFilter.value)
  }
  
  // Apply category filter
  if (categoryFilter.value !== 'all') {
    filtered = filtered.filter(template => template.category === categoryFilter.value)
  }
  
  return filtered
})

const typeOptions = [
  { label: 'All Types', value: 'all' },
  { label: 'Machine', value: 'machine' },
  { label: 'System', value: 'system' },
  { label: 'Process', value: 'process' }
]

const categoryOptions = [
  { label: 'All Categories', value: 'all' },
  { label: 'Manufacturing', value: 'manufacturing' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Quality', value: 'quality' },
  { label: 'Safety', value: 'safety' }
]

// Methods
const loadTemplates = async () => {
  try {
    loading.value = true
    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    toast.success('Templates loaded successfully')
  } catch (error) {
    console.error('Error loading templates:', error)
    toast.error('Failed to load templates')
  } finally {
    loading.value = false
  }
}

const openCreateModal = () => {
  resetForm()
  showCreateModal.value = true
}

const openEditModal = (template: any) => {
  selectedTemplate.value = template
  templateForm.value = {
    name: template.name,
    type: template.type,
    category: template.category,
    description: template.description,
    parameters: JSON.stringify(template.parameters, null, 2),
    isPublic: template.isPublic
  }
  showEditModal.value = true
}

const closeModals = () => {
  showCreateModal.value = false
  showEditModal.value = false
  selectedTemplate.value = null
  resetForm()
}

const resetForm = () => {
  templateForm.value = {
    name: '',
    type: 'machine',
    category: 'manufacturing',
    description: '',
    parameters: '{}',
    isPublic: true
  }
}

const handleSubmit = async () => {
  try {
    let parameters = {}
    try {
      parameters = JSON.parse(templateForm.value.parameters)
    } catch (e) {
      toast.error('Invalid JSON in parameters')
      return
    }

    if (selectedTemplate.value) {
      // Update existing template
      const index = templates.value.findIndex(t => t.id === selectedTemplate.value.id)
      if (index !== -1) {
        templates.value[index] = {
          ...templates.value[index],
          ...templateForm.value,
          parameters,
          updatedAt: new Date().toISOString()
        }
      }
      toast.success('Template updated successfully')
    } else {
      // Create new template
      const newTemplate = {
        id: `TPL${String(templates.value.length + 1).padStart(3, '0')}`,
        ...templateForm.value,
        parameters,
        author: 'Current User', // Would come from auth context
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
      templates.value.push(newTemplate)
      toast.success('Template created successfully')
    }
    
    closeModals()
  } catch (error) {
    console.error('Error saving template:', error)
    toast.error('Failed to save template')
  }
}

const cloneTemplate = (template: any) => {
  const clonedTemplate = {
    ...template,
    id: `TPL${String(templates.value.length + 1).padStart(3, '0')}`,
    name: `${template.name} (Copy)`,
    author: 'Current User',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  }
  templates.value.push(clonedTemplate)
  toast.success('Template cloned successfully')
}

const deleteTemplate = (templateId: string) => {
  if (!confirm('Are you sure you want to delete this template?')) {
    return
  }
  
  templates.value = templates.value.filter(t => t.id !== templateId)
  toast.success('Template deleted successfully')
}

const exportTemplate = (template: any) => {
  const dataStr = JSON.stringify(template, null, 2)
  const dataBlob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(dataBlob)
  
  const link = document.createElement('a')
  link.href = url
  link.download = `${template.name.replace(/\s+/g, '_')}_${template.version}.json`
  link.click()
  
  URL.revokeObjectURL(url)
  toast.success('Template exported successfully')
}

const importTemplate = () => {
  const input = document.createElement('input')
  input.type = 'file'
  input.accept = '.json'
  input.onchange = (event) => {
    const file = (event.target as HTMLInputElement).files?.[0]
    if (file) {
      const reader = new FileReader()
      reader.onload = (e) => {
        try {
          const template = JSON.parse(e.target?.result as string)
          template.id = `TPL${String(templates.value.length + 1).padStart(3, '0')}`
          template.author = 'Imported'
          template.createdAt = new Date().toISOString()
          template.updatedAt = new Date().toISOString()
          templates.value.push(template)
          toast.success('Template imported successfully')
        } catch (error) {
          toast.error('Invalid template file')
        }
      }
      reader.readAsText(file)
    }
  }
  input.click()
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

onMounted(() => {
  loadTemplates()
})
</script>

<template>
  <BaseCard class="configuration-templates">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <Settings class="header-icon" />
          Configuration Templates
        </h2>
        <p class="header-subtitle">Manage reusable configuration templates for machines and systems</p>
      </div>
      <div class="header-controls">
        <BaseButton variant="outline" @click="importTemplate">
          <Upload class="button-icon" />
          Import
        </BaseButton>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="button-icon" />
          New Template
        </BaseButton>
      </div>
    </template>

    <!-- Filters -->
    <div class="filters-section">
      <div class="filter-row">
        <BaseInput
          v-model="searchQuery"
          placeholder="Search templates..."
          class="search-input"
        >
          <template #prefix>
            <Search class="input-icon" />
          </template>
        </BaseInput>
        
        <BaseSelect
          v-model="typeFilter"
          :options="typeOptions"
          class="filter-select"
        />
        
        <BaseSelect
          v-model="categoryFilter"
          :options="categoryOptions"
          class="filter-select"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <BaseSkeleton v-for="i in 6" :key="i" height="140px" class="mb-4" />
    </div>

    <!-- Templates List -->
    <div v-else class="templates-container">
      <div v-if="filteredTemplates.length === 0" class="empty-state">
        <Settings class="empty-icon" />
        <h3>No templates found</h3>
        <p>Create your first configuration template to get started</p>
        <BaseButton variant="primary" @click="openCreateModal">
          <Plus class="button-icon" />
          Create Template
        </BaseButton>
      </div>
      
      <div v-else class="templates-grid">
        <BaseCard
          v-for="template in filteredTemplates"
          :key="template.id"
          class="template-card"
        >
          <div class="template-header">
            <div class="template-title">
              <h3>{{ template.name }}</h3>
              <p class="template-id">ID: {{ template.id }}</p>
            </div>
            <div class="template-meta">
              <span class="template-version">v{{ template.version }}</span>
              <span 
                class="visibility-badge"
                :class="{ 'public': template.isPublic, 'private': !template.isPublic }"
              >
                {{ template.isPublic ? 'Public' : 'Private' }}
              </span>
            </div>
          </div>
          
          <div class="template-details">
            <div class="detail-row">
              <span class="detail-label">Type:</span>
              <span class="detail-value">{{ template.type }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Category:</span>
              <span class="detail-value">{{ template.category }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Author:</span>
              <span class="detail-value">{{ template.author }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Updated:</span>
              <span class="detail-value">{{ formatDate(template.updatedAt) }}</span>
            </div>
          </div>
          
          <p class="template-description">{{ template.description }}</p>
          
          <div class="template-actions">
            <BaseButton
              variant="outline"
              size="sm"
              @click="cloneTemplate(template)"
            >
              <Copy class="action-icon" />
              Clone
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="openEditModal(template)"
            >
              <Edit class="action-icon" />
              Edit
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="exportTemplate(template)"
            >
              <Download class="action-icon" />
              Export
            </BaseButton>
            <BaseButton
              variant="outline"
              size="sm"
              @click="deleteTemplate(template.id)"
              class="delete-button"
            >
              <Trash2 class="action-icon" />
              Delete
            </BaseButton>
          </div>
        </BaseCard>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div 
      v-if="showCreateModal || showEditModal" 
      class="modal-overlay"
      @click="closeModals"
    >
      <BaseCard class="modal-content" @click.stop>
        <template #header>
          <h3>{{ selectedTemplate ? 'Edit Template' : 'Create Template' }}</h3>
        </template>
        
        <form @submit.prevent="handleSubmit" class="modal-form">
          <div class="form-grid">
            <BaseInput
              v-model="templateForm.name"
              label="Template Name"
              required
            />
            
            <BaseSelect
              v-model="templateForm.type"
              label="Type"
              :options="[
                { label: 'Machine', value: 'machine' },
                { label: 'System', value: 'system' },
                { label: 'Process', value: 'process' }
              ]"
              required
            />
            
            <BaseSelect
              v-model="templateForm.category"
              label="Category"
              :options="[
                { label: 'Manufacturing', value: 'manufacturing' },
                { label: 'Maintenance', value: 'maintenance' },
                { label: 'Quality', value: 'quality' },
                { label: 'Safety', value: 'safety' }
              ]"
              required
            />
            
            <BaseInput
              v-model="templateForm.description"
              label="Description"
              type="textarea"
              class="full-width"
            />
            
            <BaseInput
              v-model="templateForm.parameters"
              label="Parameters (JSON)"
              type="textarea"
              class="full-width json-editor"
              placeholder='{
  "parameter1": "value1",
  "parameter2": "value2"
}'
            />
            
            <div class="checkbox-wrapper">
              <input 
                id="isPublic" 
                v-model="templateForm.isPublic" 
                type="checkbox" 
                class="checkbox-input"
              />
              <label for="isPublic" class="checkbox-label">Public Template</label>
            </div>
          </div>
          
          <div class="modal-actions">
            <BaseButton variant="ghost" @click="closeModals">
              Cancel
            </BaseButton>
            <BaseButton variant="primary" type="submit">
              <Save class="button-icon" />
              {{ selectedTemplate ? 'Update' : 'Create' }} Template
            </BaseButton>
          </div>
        </form>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.configuration-templates {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

.header-content {
  flex: 1;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.5rem;
}

.header-icon {
  width: 1.5rem;
  height: 1.5rem;
  color: #3b82f6;
}

.header-subtitle {
  color: #64748b;
  font-size: 1rem;
}

.header-controls {
  display: flex;
  gap: 0.75rem;
}

.button-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.5rem;
}

.filters-section {
  margin: 1.5rem 0;
}

.filter-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  align-items: center;
}

.search-input {
  flex: 1;
  min-width: 250px;
}

.input-icon {
  width: 1rem;
  height: 1rem;
  color: #94a3b8;
}

.filter-select {
  min-width: 150px;
}

.loading-container {
  padding: 2rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #64748b;
}

.empty-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 1rem;
}

.template-card {
  padding: 1.5rem;
}

.template-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.template-title h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 0.25rem;
}

.template-id {
  font-size: 0.75rem;
  color: #64748b;
}

.template-meta {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 0.25rem;
}

.template-version {
  font-size: 0.75rem;
  color: #64748b;
  background: #f1f5f9;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
}

.visibility-badge {
  font-size: 0.75rem;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
  font-weight: 500;
}

.visibility-badge.public {
  background: #dcfce7;
  color: #16a34a;
}

.visibility-badge.private {
  background: #fee2e2;
  color: #dc2626;
}

.template-details {
  margin-bottom: 1rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 0.125rem 0;
  font-size: 0.875rem;
}

.detail-label {
  color: #64748b;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
}

.template-description {
  color: #64748b;
  font-size: 0.875rem;
  line-height: 1.5;
  margin-bottom: 1rem;
}

.template-actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.action-icon {
  width: 1rem;
  height: 1rem;
  margin-right: 0.25rem;
}

.delete-button {
  color: #dc2626 !important;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
  z-index: 50;
}

.modal-content {
  width: 100%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-form {
  padding: 1rem 0;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.full-width {
  grid-column: 1 / -1;
}

.json-editor {
  font-family: 'Courier New', monospace;
  font-size: 0.875rem;
}

.checkbox-input {
  grid-column: 1 / -1;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

@media (max-width: 768px) {
  .configuration-templates {
    padding: 0.5rem;
  }
  
  .header-controls {
    flex-direction: column;
    width: 100%;
  }
  
  .filter-row {
    flex-direction: column;
    align-items: stretch;
  }
  
  .search-input {
    min-width: auto;
  }
  
  .templates-grid {
    grid-template-columns: 1fr;
  }
  
  .template-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .template-meta {
    align-items: flex-start;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}
</style>