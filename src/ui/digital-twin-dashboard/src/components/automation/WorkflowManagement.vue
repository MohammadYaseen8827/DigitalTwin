<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import { useToast } from '@/composables/useToast'
import { 
  fetchAllWorkflows,
  deleteWorkflow,
  toggleWorkflow,
  duplicateWorkflow
} from '@/services/workflows.service'
import type { WorkflowDefinitionDto } from '@/api/types'
import { 
  Play, 
  Pause, 
  Plus, 
  Edit, 
  Trash2, 
  Copy, 
  Activity,
  Calendar
} from 'lucide-vue-next'

const toast = useToast()

// State
const workflows = ref<WorkflowDefinitionDto[]>([])
const loading = ref(false)

// Computed
const totalWorkflows = computed(() => workflows.value.length)
const activeWorkflows = computed(() => workflows.value.filter(w => w.enabled).length)

// Methods
const loadWorkflows = async () => {
  try {
    loading.value = true
    workflows.value = await fetchAllWorkflows()
  } catch (error) {
    console.error('Failed to load workflows:', error)
    toast.error('Unable to load workflows')
  } finally {
    loading.value = false
  }
}

const handleToggleWorkflow = async (workflow: WorkflowDefinitionDto) => {
  try {
    const updatedWorkflow = await toggleWorkflow(workflow.id)
    const index = workflows.value.findIndex(w => w.id === workflow.id)
    if (index !== -1) {
      workflows.value[index] = updatedWorkflow
    }
    toast.success(`Workflow ${updatedWorkflow.enabled ? 'enabled' : 'disabled'} successfully`)
  } catch (error) {
    console.error('Failed to toggle workflow:', error)
    toast.error('Failed to toggle workflow')
  }
}

const handleDeleteWorkflow = async (workflow: WorkflowDefinitionDto) => {
  if (!confirm(`Are you sure you want to delete workflow "${workflow.name}"?`)) {
    return
  }
  
  try {
    await deleteWorkflow(workflow.id)
    workflows.value = workflows.value.filter(w => w.id !== workflow.id)
    toast.success('Workflow deleted successfully')
  } catch (error) {
    console.error('Failed to delete workflow:', error)
    toast.error('Failed to delete workflow')
  }
}

const handleDuplicateWorkflow = async (workflow: WorkflowDefinitionDto) => {
  try {
    const newName = `${workflow.name} (Copy)`
    const duplicatedWorkflow = await duplicateWorkflow(workflow.id, newName, workflow.description)
    workflows.value.push(duplicatedWorkflow)
    toast.success('Workflow duplicated successfully')
  } catch (error) {
    console.error('Failed to duplicate workflow:', error)
    toast.error('Failed to duplicate workflow')
  }
}

const getTypeDisplay = (type: string): string => {
  const types: Record<string, string> = {
    'maintenance': 'Maintenance',
    'monitoring': 'Monitoring',
    'reporting': 'Reporting',
    'alerting': 'Alerting',
    'custom': 'Custom'
  }
  return types[type.toLowerCase()] || type
}

const formatTime = (dateString?: string): string => {
  if (!dateString) return 'Never'
  return new Date(dateString).toLocaleString()
}

// Lifecycle
onMounted(async () => {
  await loadWorkflows()
})
</script>

<template>
  <BaseCard>
    <div class="workflow-management space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Workflow Automation</h1>
          <p class="text-gray-600 mt-2">Manage and monitor automated workflows</p>
        </div>
        <BaseButton variant="primary" class="flex items-center gap-2">
          <Plus class="w-4 h-4" />
          Create Workflow
        </BaseButton>
      </div>

      <!-- Stats Overview -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Total Workflows</p>
                <p class="text-2xl font-bold">{{ totalWorkflows }}</p>
              </div>
              <Activity class="w-8 h-8 text-blue-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Active Workflows</p>
                <p class="text-2xl font-bold">{{ activeWorkflows }}</p>
              </div>
              <Play class="w-8 h-8 text-green-500" />
            </div>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-sm text-gray-600">Execution Rate</p>
                <p class="text-2xl font-bold">84.4%</p>
              </div>
              <Calendar class="w-8 h-8 text-purple-500" />
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Workflows List -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Workflows</h2>
          
          <div v-if="loading" class="text-center py-8">
            <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto"></div>
            <p class="mt-2 text-gray-600">Loading workflows...</p>
          </div>
          
          <div v-else-if="workflows.length === 0" class="text-center py-8">
            <Activity class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-gray-600">No workflows found</p>
            <p class="text-sm text-gray-500 mt-1">Create your first workflow to get started</p>
          </div>
          
          <div v-else class="space-y-4">
            <div
              v-for="workflow in workflows"
              :key="workflow.id"
              class="border border-gray-200 rounded-lg p-4 hover:bg-gray-50 transition-colors"
            >
              <div class="flex flex-col sm:flex-row justify-between gap-4">
                <div class="flex-1">
                  <div class="flex items-start justify-between gap-4">
                    <div>
                      <h3 class="text-lg font-semibold text-gray-900">{{ workflow.name }}</h3>
                      <p class="text-gray-600 mt-1">{{ workflow.description }}</p>
                      
                      <div class="flex flex-wrap gap-2 mt-3">
                        <span 
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          :class="workflow.enabled ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'"
                        >
                          {{ workflow.status }}
                        </span>
                        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                          {{ getTypeDisplay(workflow.type) }}
                        </span>
                        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                          {{ workflow.actions.length }} actions
                        </span>
                      </div>
                    </div>
                    
                    <div class="flex items-center gap-2 text-sm text-gray-500">
                      <Calendar class="w-4 h-4" />
                      <span>Created: {{ formatTime(workflow.createdAt) }}</span>
                    </div>
                  </div>
                  
                  <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4 text-sm">
                    <div>
                      <span class="text-gray-500">Last Run:</span>
                      <p class="font-medium">{{ formatTime(workflow.lastRun) }}</p>
                    </div>
                    <div>
                      <span class="text-gray-500">Next Run:</span>
                      <p class="font-medium">{{ formatTime(workflow.nextRun) }}</p>
                    </div>
                    <div>
                      <span class="text-gray-500">Executions:</span>
                      <p class="font-medium">{{ workflow.executionCount }}</p>
                    </div>
                    <div>
                      <span class="text-gray-500">Success Rate:</span>
                      <p class="font-medium">84.4%</p>
                    </div>
                  </div>
                </div>
                
                <div class="flex flex-col sm:flex-row gap-2">
                  <BaseButton
                    size="sm"
                    variant="outline"
                    @click="handleToggleWorkflow(workflow)"
                    :class="workflow.enabled ? 'text-red-600 hover:text-red-700' : 'text-green-600 hover:text-green-700'"
                  >
                    <component :is="workflow.enabled ? Pause : Play" class="w-4 h-4 mr-2" />
                    {{ workflow.enabled ? 'Disable' : 'Enable' }}
                  </BaseButton>
                  
                  <BaseButton size="sm" variant="outline">
                    <Edit class="w-4 h-4 mr-2" />
                    Edit
                  </BaseButton>
                  
                  <BaseButton
                    size="sm"
                    variant="outline"
                    @click="handleDuplicateWorkflow(workflow)"
                  >
                    <Copy class="w-4 h-4 mr-2" />
                    Duplicate
                  </BaseButton>
                  
                  <BaseButton
                    size="sm"
                    variant="outline"
                    @click="handleDeleteWorkflow(workflow)"
                    class="text-red-600 hover:text-red-700 hover:bg-red-50"
                  >
                    <Trash2 class="w-4 h-4 mr-2" />
                    Delete
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.workflow-management {
  max-width: 1200px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .workflow-management {
    padding: 0.5rem;
  }
}
</style>