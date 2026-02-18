<template>
  <BaseCard variant="soft" class="workflow-automation">
    <template #header>
      <div class="header-content">
        <div>
          <h3>Workflow Automation</h3>
          <p>Automate machine management processes and maintenance workflows</p>
        </div>
        <div class="header-actions">
          <BaseButton 
            variant="primary" 
            @click="openCreateWorkflow"
            :disabled="processing"
          >
            <Plus class="icon" />
            New Workflow
          </BaseButton>
          <BaseButton 
            variant="outline" 
            @click="refreshWorkflows"
            :loading="loading"
          >
            <RefreshCw class="icon" />
            Refresh
          </BaseButton>
        </div>
      </div>
    </template>

    <div class="workflows-container">
      <!-- Workflow List -->
      <div v-if="workflows.length > 0" class="workflows-grid">
        <BaseCard
          v-for="workflow in workflows"
          :key="workflow.id"
          variant="bordered"
          class="workflow-card"
          :class="{ 'active': selectedWorkflow?.id === workflow.id }"
        >
          <div class="workflow-header">
            <div class="workflow-info">
              <h4>{{ workflow.name }}</h4>
              <span class="workflow-type">{{ workflow.type }}</span>
              <span 
                class="workflow-status" 
                :class="`status-${workflow.status.toLowerCase()}`"
              >
                {{ workflow.status }}
              </span>
            </div>
            <div class="workflow-actions">
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click="toggleWorkflow(workflow)"
                :disabled="processing"
                :title="workflow.enabled ? 'Disable workflow' : 'Enable workflow'"
              >
                <component 
                  :is="workflow.enabled ? Pause : Play" 
                  class="icon" 
                />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click="editWorkflow(workflow)"
              >
                <Edit class="icon" />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click="duplicateWorkflow(workflow)"
              >
                <Copy class="icon" />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click="deleteWorkflow(workflow)"
                :disabled="processing"
              >
                <Trash2 class="icon" />
              </BaseButton>
            </div>
          </div>
          
          <div class="workflow-details">
            <p class="description">{{ workflow.description }}</p>
            
            <div class="workflow-stats">
              <div class="stat">
                <span class="stat-label">Trigger:</span>
                <span class="stat-value">{{ workflow.trigger.type }}</span>
              </div>
              <div class="stat">
                <span class="stat-label">Last Run:</span>
                <span class="stat-value">{{ formatDate(workflow.lastRun) || 'Never' }}</span>
              </div>
              <div class="stat">
                <span class="stat-label">Next Run:</span>
                <span class="stat-value">{{ formatDate(workflow.nextRun) || 'N/A' }}</span>
              </div>
            </div>
            
            <div class="workflow-tags">
              <span 
                v-for="tag in workflow.tags" 
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
      <div v-else-if="!loading" class="empty-state">
        <div class="empty-content">
          <Workflow class="empty-icon" />
          <h3>No Workflows Configured</h3>
          <p>Create automated workflows to streamline machine management tasks.</p>
          <BaseButton variant="primary" @click="openCreateWorkflow">
            Create First Workflow
          </BaseButton>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <BaseSkeleton v-for="i in 3" :key="i" width="100%" height="150px" />
      </div>
    </div>

    <!-- Workflow Editor Modal -->
    <div v-if="showEditor" class="modal-overlay" @click="closeEditor">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>{{ isEditing ? 'Edit Workflow' : 'Create Workflow' }}</h3>
          <BaseButton variant="ghost" @click="closeEditor">
            <X class="icon" />
          </BaseButton>
        </div>
        
        <div class="modal-body">
          <form @submit.prevent="saveWorkflow" class="workflow-form">
            <div class="form-section">
              <h4>Basic Information</h4>
              
              <BaseInput
                v-model="editorForm.name"
                label="Workflow Name"
                placeholder="e.g., Monthly Maintenance Reminder"
                required
                :error="formErrors.name"
              />
              
              <BaseInput
                v-model="editorForm.description"
                label="Description"
                type="textarea"
                placeholder="Describe what this workflow does..."
                rows="3"
              />
              
              <BaseSelect
                v-model="editorForm.type"
                :options="workflowTypes"
                label="Workflow Type"
                required
                :error="formErrors.type"
              />
              
              <BaseInput
                v-model="editorTags"
                label="Tags"
                placeholder="comma, separated, tags"
                hint="Enter tags separated by commas"
              />
            </div>

            <div class="form-section">
              <h4>Trigger Configuration</h4>
              
              <BaseSelect
                v-model="editorForm.trigger.type"
                :options="triggerTypes"
                label="Trigger Type"
                required
              />
              
              <div v-if="editorForm.trigger.type === 'schedule'" class="trigger-options">
                <BaseInput
                  v-model="editorForm.trigger.cronExpression"
                  label="Cron Expression"
                  placeholder="0 0 9 * * * (daily at 9 AM)"
                  hint="Unix-style cron expression"
                  required
                />
              </div>
              
              <div v-else-if="editorForm.trigger.type === 'condition'" class="trigger-options">
                <BaseSelect
                  v-model="editorForm.trigger.conditionField"
                  :options="conditionFields"
                  label="Condition Field"
                  required
                />
                
                <BaseSelect
                  v-model="editorForm.trigger.conditionOperator"
                  :options="conditionOperators"
                  label="Operator"
                  required
                />
                
                <BaseInput
                  v-model="editorForm.trigger.conditionValue"
                  label="Condition Value"
                  required
                />
              </div>
              
              <div v-else-if="editorForm.trigger.type === 'event'" class="trigger-options">
                <BaseSelect
                  v-model="editorForm.trigger.eventType"
                  :options="eventTypes"
                  label="Event Type"
                  required
                />
              </div>
            </div>

            <div class="form-section">
              <h4>Actions</h4>
              
              <div 
                v-for="(action, index) in editorForm.actions" 
                :key="index"
                class="action-item"
              >
                <div class="action-header">
                  <span>Action {{ index + 1 }}</span>
                  <BaseButton 
                    variant="ghost" 
                    size="sm"
                    @click="removeAction(index)"
                    :disabled="editorForm.actions.length <= 1"
                  >
                    <Trash2 class="icon" />
                  </BaseButton>
                </div>
                
                <div class="action-fields">
                  <BaseSelect
                    v-model="action.type"
                    :options="actionTypes"
                    label="Action Type"
                    required
                  />
                  
                  <div v-if="action.type === 'notification'" class="action-options">
                    <BaseInput
                      v-model="action.recipients"
                      label="Recipients"
                      placeholder="email1@example.com, email2@example.com"
                      hint="Comma-separated email addresses"
                      required
                    />
                    
                    <BaseInput
                      v-model="action.subject"
                      label="Subject"
                      placeholder="Maintenance reminder for {{machine.name}}"
                      required
                    />
                    
                    <BaseInput
                      v-model="action.message"
                      label="Message"
                      type="textarea"
                      placeholder="Machine {{machine.name}} requires maintenance..."
                      rows="3"
                      required
                    />
                  </div>
                  
                  <div v-else-if="action.type === 'update_status'" class="action-options">
                    <BaseSelect
                      v-model="action.newStatus"
                      :options="statusOptions"
                      label="New Status"
                      required
                    />
                  </div>
                  
                  <div v-else-if="action.type === 'create_ticket'" class="action-options">
                    <BaseInput
                      v-model="action.title"
                      label="Ticket Title"
                      placeholder="Maintenance required for {{machine.name}}"
                      required
                    />
                    
                    <BaseInput
                      v-model="action.priority"
                      label="Priority"
                      placeholder="High"
                      required
                    />
                  </div>
                </div>
              </div>
              
              <BaseButton 
                variant="outline" 
                @click="addAction"
                class="add-action-button"
              >
                <Plus class="icon" />
                Add Action
              </BaseButton>
            </div>

            <div class="form-section">
              <h4>Target Machines</h4>
              
              <BaseSelect
                v-model="editorForm.target.type"
                :options="targetTypes"
                label="Target Type"
                required
              />
              
              <div v-if="editorForm.target.type === 'specific'" class="target-options">
                <BaseSelect
                  v-model="editorForm.target.machineIds"
                  :options="availableMachines"
                  label="Select Machines"
                  multiple
                  placeholder="Choose machines..."
                />
              </div>
              
              <div v-else-if="editorForm.target.type === 'by_type'" class="target-options">
                <BaseSelect
                  v-model="editorForm.target.machineTypes"
                  :options="machineTypes"
                  label="Machine Types"
                  multiple
                  placeholder="Choose machine types..."
                />
              </div>
              
              <div v-else-if="editorForm.target.type === 'by_status'" class="target-options">
                <BaseSelect
                  v-model="editorForm.target.statuses"
                  :options="statusOptions"
                  label="Statuses"
                  multiple
                  placeholder="Choose statuses..."
                />
              </div>
            </div>

            <div class="modal-actions">
              <BaseButton 
                variant="primary" 
                type="submit"
                :loading="processing"
                :disabled="!isFormValid"
              >
                {{ isEditing ? 'Update Workflow' : 'Create Workflow' }}
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

    <!-- Delete Confirmation -->
    <div v-if="showDeleteConfirm" class="confirmation-dialog glass-panel">
      <div class="dialog-content">
        <AlertTriangle class="warning-icon" />
        <h3>Delete Workflow</h3>
        <p>
          Are you sure you want to delete "{{ workflowToDelete?.name }}"? 
          This action cannot be undone.
        </p>
        <div class="dialog-actions">
          <BaseButton 
            variant="critical" 
            @click="confirmDelete"
            :loading="processing"
          >
            Delete Workflow
          </BaseButton>
          <BaseButton 
            variant="ghost" 
            @click="cancelDelete"
          >
            Cancel
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseCard>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { useToast } from '@/composables/useToast'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  Plus, 
  Edit, 
  Copy, 
  Trash2, 
  Play, 
  Pause, 
  RefreshCw, 
  Workflow, 
  X, 
  AlertTriangle 
} from 'lucide-vue-next'

const toast = useToast()

// Reactive state
const workflows = ref<WorkflowDefinition[]>([])
const selectedWorkflow = ref<WorkflowDefinition | null>(null)
const loading = ref(false)
const processing = ref(false)

// Modal state
const showEditor = ref(false)
const showDeleteConfirm = ref(false)
const isEditing = ref(false)
const workflowToDelete = ref<WorkflowDefinition | null>(null)

// Form state
const editorForm = reactive<WorkflowDefinition>({
  id: '',
  name: '',
  description: '',
  type: 'maintenance',
  status: 'inactive',
  enabled: false,
  trigger: {
    type: 'schedule',
    cronExpression: '0 0 9 * * *'
  },
  actions: [
    {
      type: 'notification',
      recipients: '',
      subject: 'Maintenance reminder',
      message: 'Machine requires attention'
    }
  ],
  target: {
    type: 'all'
  },
  tags: [],
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  lastRun: null,
  nextRun: null
})

const formErrors = reactive<Record<string, string>>({})
const editorTags = ref('')

// Machine data for selectors
const availableMachines = ref<Array<{ label: string; value: string }>>([])

// Computed properties
const isFormValid = computed(() => {
  return editorForm.name && 
         editorForm.type && 
         editorForm.trigger.type &&
         editorForm.actions.length > 0
})

// Interfaces
interface WorkflowDefinition {
  id: string
  name: string
  description: string
  type: string
  status: 'active' | 'inactive' | 'error'
  enabled: boolean
  trigger: {
    type: 'schedule' | 'condition' | 'event'
    cronExpression?: string
    conditionField?: string
    conditionOperator?: string
    conditionValue?: string
    eventType?: string
  }
  actions: Array<{
    type: 'notification' | 'update_status' | 'create_ticket'
    recipients?: string
    subject?: string
    message?: string
    newStatus?: string
    title?: string
    priority?: string
  }>
  target: {
    type: 'all' | 'specific' | 'by_type' | 'by_status'
    machineIds?: string[]
    machineTypes?: string[]
    statuses?: string[]
  }
  tags: string[]
  createdAt: string
  updatedAt: string
  lastRun: string | null
  nextRun: string | null
}

// Options
const workflowTypes = [
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Monitoring', value: 'monitoring' },
  { label: 'Reporting', value: 'reporting' },
  { label: 'Alerting', value: 'alerting' }
]

const triggerTypes = [
  { label: 'Scheduled', value: 'schedule' },
  { label: 'Conditional', value: 'condition' },
  { label: 'Event-based', value: 'event' }
]

const conditionFields = [
  { label: 'Status', value: 'status' },
  { label: 'Criticality', value: 'criticality' },
  { label: 'Last Maintenance', value: 'lastMaintenance' },
  { label: 'RUL Days', value: 'remainingUsefulLifeDays' }
]

const conditionOperators = [
  { label: 'Equals', value: 'equals' },
  { label: 'Greater Than', value: 'greater_than' },
  { label: 'Less Than', value: 'less_than' },
  { label: 'Contains', value: 'contains' }
]

const eventTypes = [
  { label: 'Machine Status Change', value: 'status_change' },
  { label: 'Maintenance Due', value: 'maintenance_due' },
  { label: 'Anomaly Detected', value: 'anomaly_detected' },
  { label: 'Prediction Available', value: 'prediction_available' }
]

const actionTypes = [
  { label: 'Send Notification', value: 'notification' },
  { label: 'Update Status', value: 'update_status' },
  { label: 'Create Ticket', value: 'create_ticket' }
]

const targetTypes = [
  { label: 'All Machines', value: 'all' },
  { label: 'Specific Machines', value: 'specific' },
  { label: 'By Machine Type', value: 'by_type' },
  { label: 'By Status', value: 'by_status' }
]

const statusOptions = [
  { label: 'Operational', value: 'operational' },
  { label: 'Warning', value: 'warning' },
  { label: 'Critical', value: 'critical' },
  { label: 'Maintenance', value: 'maintenance' },
  { label: 'Offline', value: 'offline' }
]

const machineTypes = [
  { label: 'CNC Machine', value: 'cnc' },
  { label: 'Injection Molder', value: 'injection_molder' },
  { label: 'Press', value: 'press' },
  { label: 'Robot', value: 'robot' },
  { label: 'Conveyor', value: 'conveyor' }
]

// Methods
const loadWorkflows = async () => {
  try {
    loading.value = true
    // Mock workflows - in real implementation, this would come from API
    workflows.value = [
      {
        id: 'wf_1',
        name: 'Monthly Maintenance Reminder',
        description: 'Sends maintenance reminders for all machines monthly',
        type: 'maintenance',
        status: 'active',
        enabled: true,
        trigger: {
          type: 'schedule',
          cronExpression: '0 0 9 1 * *'
        },
        actions: [
          {
            type: 'notification',
            recipients: 'maintenance@company.com',
            subject: 'Monthly Maintenance Reminder',
            message: 'Review maintenance schedules for all machines'
          }
        ],
        target: {
          type: 'all'
        },
        tags: ['maintenance', 'reminder'],
        createdAt: new Date(Date.now() - 86400000).toISOString(),
        updatedAt: new Date().toISOString(),
        lastRun: new Date(Date.now() - 3600000).toISOString(),
        nextRun: new Date(Date.now() + 86400000).toISOString()
      },
      {
        id: 'wf_2',
        name: 'Critical Machine Alert',
        description: 'Alerts when machines reach critical status',
        type: 'alerting',
        status: 'active',
        enabled: true,
        trigger: {
          type: 'condition',
          conditionField: 'status',
          conditionOperator: 'equals',
          conditionValue: 'critical'
        },
        actions: [
          {
            type: 'notification',
            recipients: 'alerts@company.com, supervisor@company.com',
            subject: 'Critical Machine Alert - {{machine.name}}',
            message: 'Machine {{machine.name}} has reached critical status'
          }
        ],
        target: {
          type: 'all'
        },
        tags: ['alerting', 'critical'],
        createdAt: new Date(Date.now() - 172800000).toISOString(),
        updatedAt: new Date().toISOString(),
        lastRun: null,
        nextRun: null
      }
    ]
  } catch (error) {
    console.error('Failed to load workflows:', error)
    toast.error('Unable to load workflows')
  } finally {
    loading.value = false
  }
}

const loadMachines = async () => {
  try {
    const machines = await fetchMachines()
    availableMachines.value = machines.map(machine => ({
      label: `${machine.name} (${machine.type})`,
      value: machine.id
    }))
  } catch (error) {
    console.error('Failed to load machines:', error)
  }
}

const refreshWorkflows = async () => {
  await loadWorkflows()
  toast.success('Workflows refreshed')
}

const openCreateWorkflow = () => {
  isEditing.value = false
  resetEditorForm()
  showEditor.value = true
}

const editWorkflow = (workflow: WorkflowDefinition) => {
  isEditing.value = true
  Object.assign(editorForm, JSON.parse(JSON.stringify(workflow)))
  editorTags.value = workflow.tags.join(', ')
  showEditor.value = true
}

const duplicateWorkflow = (workflow: WorkflowDefinition) => {
  isEditing.value = false
  const duplicated = JSON.parse(JSON.stringify(workflow))
  duplicated.id = `wf_${Date.now()}`
  duplicated.name = `${workflow.name} (Copy)`
  Object.assign(editorForm, duplicated)
  editorTags.value = duplicated.tags.join(', ')
  showEditor.value = true
}

const toggleWorkflow = async (workflow: WorkflowDefinition) => {
  try {
    processing.value = true
    // In real implementation, this would call API to toggle workflow
    const updatedWorkflow = { ...workflow, enabled: !workflow.enabled }
    const index = workflows.value.findIndex(w => w.id === workflow.id)
    if (index !== -1) {
      workflows.value[index] = updatedWorkflow
    }
    toast.success(`Workflow ${updatedWorkflow.enabled ? 'enabled' : 'disabled'}`)
  } catch (error) {
    console.error('Toggle failed:', error)
    toast.error('Failed to toggle workflow')
  } finally {
    processing.value = false
  }
}

const deleteWorkflow = (workflow: WorkflowDefinition) => {
  workflowToDelete.value = workflow
  showDeleteConfirm.value = true
}

const confirmDelete = async () => {
  if (!workflowToDelete.value) return
  
  try {
    processing.value = true
    workflows.value = workflows.value.filter(w => w.id !== workflowToDelete.value!.id)
    toast.success('Workflow deleted successfully')
    showDeleteConfirm.value = false
    workflowToDelete.value = null
  } catch (error) {
    console.error('Delete failed:', error)
    toast.error('Failed to delete workflow')
  } finally {
    processing.value = false
  }
}

const cancelDelete = () => {
  showDeleteConfirm.value = false
  workflowToDelete.value = null
}

const saveWorkflow = async () => {
  try {
    processing.value = true
    
    // Validate form
    const errors: Record<string, string> = {}
    if (!editorForm.name) errors.name = 'Name is required'
    if (!editorForm.type) errors.type = 'Type is required'
    
    if (Object.keys(errors).length > 0) {
      Object.assign(formErrors, errors)
      return
    }
    
    // Clear previous errors
    Object.keys(formErrors).forEach(key => delete formErrors[key])
    
    // Add tags
    editorForm.tags = editorTags.value
      .split(',')
      .map(tag => tag.trim())
      .filter(tag => tag)
    
    // Save workflow
    if (isEditing.value) {
      const index = workflows.value.findIndex(w => w.id === editorForm.id)
      if (index !== -1) {
        editorForm.updatedAt = new Date().toISOString()
        workflows.value[index] = { ...editorForm }
        toast.success('Workflow updated successfully')
      }
    } else {
      editorForm.id = `wf_${Date.now()}`
      editorForm.createdAt = new Date().toISOString()
      editorForm.updatedAt = new Date().toISOString()
      workflows.value.push({ ...editorForm })
      toast.success('Workflow created successfully')
    }
    
    closeEditor()
    
  } catch (error) {
    console.error('Save failed:', error)
    toast.error('Failed to save workflow')
  } finally {
    processing.value = false
  }
}

const closeEditor = () => {
  showEditor.value = false
  resetEditorForm()
}

const resetEditorForm = () => {
  Object.assign(editorForm, {
    id: '',
    name: '',
    description: '',
    type: 'maintenance',
    status: 'inactive',
    enabled: false,
    trigger: {
      type: 'schedule',
      cronExpression: '0 0 9 * * *'
    },
    actions: [
      {
        type: 'notification',
        recipients: '',
        subject: 'Maintenance reminder',
        message: 'Machine requires attention'
      }
    ],
    target: {
      type: 'all'
    },
    tags: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    lastRun: null,
    nextRun: null
  })
  editorTags.value = ''
  Object.keys(formErrors).forEach(key => delete formErrors[key])
}

const addAction = () => {
  editorForm.actions.push({
    type: 'notification',
    recipients: '',
    subject: '',
    message: ''
  })
}

const removeAction = (index: number) => {
  if (editorForm.actions.length > 1) {
    editorForm.actions.splice(index, 1)
  }
}

const formatDate = (dateString?: string | null): string => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Lifecycle
onMounted(() => {
  loadWorkflows()
  loadMachines()
})
</script>

<style scoped>
.workflow-automation {
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

.header-actions {
  display: flex;
  gap: var(--spacing-sm);
}

.workflows-container {
  margin-top: var(--spacing-lg);
}

.workflows-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--spacing-lg);
}

.workflow-card {
  transition: all 0.2s ease;
  border: 1px solid var(--color-border-subtle);
}

.workflow-card:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.workflow-card.active {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 5%, transparent);
}

.workflow-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--spacing-md);
}

.workflow-info h4 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.workflow-type {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  background: var(--color-surface-alt);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  text-transform: uppercase;
  margin-right: var(--spacing-sm);
}

.workflow-status {
  font-size: 0.75rem;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-weight: 500;
  text-transform: capitalize;
}

.workflow-status.status-active {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.workflow-status.status-inactive {
  background: color-mix(in srgb, var(--color-text-secondary) 20%, transparent);
  color: var(--color-text-secondary);
}

.workflow-status.status-error {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.workflow-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.workflow-details .description {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  line-height: 1.5;
}

.workflow-stats {
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
  font-size: 0.875rem;
}

.workflow-tags {
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
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--spacing-lg);
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

.workflow-form {
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

.trigger-options,
.action-options,
.target-options {
  margin-top: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
}

.action-item {
  background: var(--color-surface);
  border-radius: var(--radius-md);
  padding: var(--spacing-md);
  margin-bottom: var(--spacing-md);
  border: 1px solid var(--color-border-subtle);
}

.action-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--spacing-md);
  font-weight: 500;
  color: var(--color-text-primary);
}

.action-fields {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.add-action-button {
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

.warning-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-warning);
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
  
  .workflows-grid {
    grid-template-columns: 1fr;
  }
  
  .workflow-header {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .workflow-actions {
    width: 100%;
    justify-content: flex-end;
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