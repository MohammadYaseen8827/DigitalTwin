<template>
  <BaseCard variant="soft" class="ai-model-management">
    <template #header>
      <div class="header-content">
        <div>
          <h3>AI Model Management</h3>
          <p>Manage and monitor predictive maintenance AI models</p>
        </div>
        <div class="header-actions">
          <BaseButton 
            variant="primary" 
            @click="openDeployModel"
            :disabled="processing"
          >
            <Upload class="icon" />
            Deploy Model
          </BaseButton>
          <BaseButton 
            variant="outline" 
            @click="refreshModels"
            :loading="loading"
          >
            <RefreshCw class="icon" />
            Refresh
          </BaseButton>
        </div>
      </div>
    </template>

    <div class="models-container">
      <!-- Model List -->
      <div v-if="models.length > 0" class="models-grid">
        <BaseCard
          v-for="model in models"
          :key="model.id"
          variant="bordered"
          class="model-card"
          :class="{ 'active': selectedModel?.id === model.id }"
          @click="selectModel(model)"
        >
          <div class="model-header">
            <div class="model-info">
              <h4>{{ model.name }}</h4>
              <span class="model-version">v{{ model.version }}</span>
              <span 
                class="model-status" 
                :class="`status-${model.status.toLowerCase()}`"
              >
                {{ model.status }}
              </span>
            </div>
            <div class="model-actions">
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click.stop="testModel(model)"
                :disabled="processing || model.status !== 'deployed'"
                title="Test model predictions"
              >
                <Activity class="icon" />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click.stop="editModel(model)"
              >
                <Edit class="icon" />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click.stop="duplicateModel(model)"
              >
                <Copy class="icon" />
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                size="sm"
                @click.stop="deleteModel(model)"
                :disabled="processing"
              >
                <Trash2 class="icon" />
              </BaseButton>
            </div>
          </div>
          
          <div class="model-details">
            <p class="description">{{ model.description }}</p>
            
            <div class="model-stats">
              <div class="stat">
                <span class="stat-label">Accuracy:</span>
                <span class="stat-value">{{ formatPercentage(model.accuracy) }}</span>
              </div>
              <div class="stat">
                <span class="stat-label">Last Training:</span>
                <span class="stat-value">{{ formatDate(model.lastTraining) }}</span>
              </div>
              <div class="stat">
                <span class="stat-label">Machines:</span>
                <span class="stat-value">{{ model.deployedMachines?.length || 0 }}</span>
              </div>
            </div>
            
            <div class="model-metrics">
              <div class="metric">
                <div class="metric-bar">
                  <div 
                    class="metric-fill" 
                    :style="{ width: `${model.accuracy * 100}%` }"
                  ></div>
                </div>
                <span class="metric-label">Model Accuracy</span>
              </div>
            </div>
            
            <div class="model-tags">
              <span 
                v-for="tag in model.tags" 
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
          <Brain class="empty-icon" />
          <h3>No AI Models Deployed</h3>
          <p>Deploy predictive maintenance models to enable smart analytics.</p>
          <BaseButton variant="primary" @click="openDeployModel">
            Deploy First Model
          </BaseButton>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="loading" class="loading-state">
        <BaseSkeleton v-for="i in 3" :key="i" width="100%" height="180px" />
      </div>
    </div>

    <!-- Model Deployment Modal -->
    <div v-if="showDeploymentModal" class="modal-overlay" @click="closeDeploymentModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>Deploy AI Model</h3>
          <BaseButton variant="ghost" @click="closeDeploymentModal">
            <X class="icon" />
          </BaseButton>
        </div>
        
        <div class="modal-body">
          <form @submit.prevent="deployModel" class="deployment-form">
            <div class="form-section">
              <h4>Model Information</h4>
              
              <BaseInput
                v-model="deploymentForm.name"
                label="Model Name"
                placeholder="e.g., CNC Degradation Predictor v2"
                required
                :error="formErrors.name"
              />
              
              <BaseInput
                v-model="deploymentForm.description"
                label="Description"
                type="textarea"
                placeholder="Describe the model's purpose and capabilities..."
                rows="3"
              />
              
              <BaseInput
                v-model="deploymentForm.version"
                label="Version"
                placeholder="e.g., 2.1.0"
                required
              />
              
              <BaseInput
                v-model="deploymentTags"
                label="Tags"
                placeholder="predictive, cnc, degradation"
                hint="Enter tags separated by commas"
              />
            </div>

            <div class="form-section">
              <h4>Model Configuration</h4>
              
              <BaseSelect
                v-model="deploymentForm.modelType"
                :options="modelTypes"
                label="Model Type"
                required
              />
              
              <BaseSelect
                v-model="deploymentForm.algorithm"
                :options="algorithms"
                label="Algorithm"
                required
              />
              
              <BaseInput
                v-model="deploymentForm.trainingDataSize"
                label="Training Data Size"
                type="number"
                placeholder="Number of training samples"
                required
              />
              
              <BaseInput
                v-model="deploymentForm.features"
                label="Features"
                placeholder="temperature,vibration,pressure,current"
                hint="Comma-separated feature names used by the model"
                required
              />
            </div>

            <div class="form-section">
              <h4>Performance Metrics</h4>
              
              <div class="metrics-grid">
                <BaseInput
                  v-model="deploymentForm.accuracy"
                  label="Accuracy"
                  type="number"
                  :step="0.01"
                  :min="0"
                  :max="1"
                  placeholder="0.95"
                  required
                />
                
                <BaseInput
                  v-model="deploymentForm.precision"
                  label="Precision"
                  type="number"
                  :step="0.01"
                  :min="0"
                  :max="1"
                  placeholder="0.92"
                />
                
                <BaseInput
                  v-model="deploymentForm.recall"
                  label="Recall"
                  type="number"
                  :step="0.01"
                  :min="0"
                  :max="1"
                  placeholder="0.88"
                />
                
                <BaseInput
                  v-model="deploymentForm.f1Score"
                  label="F1 Score"
                  type="number"
                  :step="0.01"
                  :min="0"
                  :max="1"
                  placeholder="0.90"
                />
              </div>
            </div>

            <div class="form-section">
              <h4>Deployment Targets</h4>
              
              <BaseSelect
                v-model="deploymentForm.targetMachines"
                :options="availableMachines"
                label="Target Machines"
                multiple
                placeholder="Select machines to deploy this model to..."
                hint="Leave empty to deploy to all compatible machines"
              />
              
              <div class="compatibility-info">
                <Info class="info-icon" />
                <p>This model is compatible with {{ deploymentForm.modelType }} machines</p>
              </div>
            </div>

            <div class="form-section">
              <h4>Model File</h4>
              
              <div 
                class="file-upload-area"
                :class="{ 'drag-over': isDragging }"
                @drop.prevent="handleFileDrop"
                @dragover.prevent="isDragging = true"
                @dragleave.prevent="isDragging = false"
              >
                <input
                  ref="fileInput"
                  type="file"
                  accept=".pkl,.joblib,.h5,.onnx"
                  @change="handleFileSelect"
                  class="file-input"
                />
                <Upload class="upload-icon" />
                <p>Drag and drop model file or click to browse</p>
                <p class="file-types">Supported formats: .pkl, .joblib, .h5, .onnx</p>
                <BaseButton 
                  variant="outline" 
                  @click="triggerFileSelect"
                >
                  Select File
                </BaseButton>
              </div>
              
              <div v-if="selectedFile" class="selected-file">
                <FileText class="file-icon" />
                <div class="file-info">
                  <span class="file-name">{{ selectedFile.name }}</span>
                  <span class="file-size">{{ formatFileSize(selectedFile.size) }}</span>
                </div>
                <button 
                  type="button" 
                  class="remove-file"
                  @click="removeSelectedFile"
                >
                  <X class="icon" />
                </button>
              </div>
            </div>

            <div class="modal-actions">
              <BaseButton 
                variant="primary" 
                type="submit"
                :loading="processing"
                :disabled="!isDeploymentFormValid"
              >
                Deploy Model
              </BaseButton>
              <BaseButton 
                variant="ghost" 
                @click="closeDeploymentModal"
              >
                Cancel
              </BaseButton>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Model Testing Modal -->
    <div v-if="showTestingModal" class="modal-overlay" @click="closeTestingModal">
      <div class="modal-content testing-modal" @click.stop>
        <div class="modal-header">
          <h3>Test Model: {{ selectedModel?.name }}</h3>
          <BaseButton variant="ghost" @click="closeTestingModal">
            <X class="icon" />
          </BaseButton>
        </div>
        
        <div class="modal-body">
          <div class="testing-interface">
            <div class="test-inputs">
              <h4>Test Inputs</h4>
              <div class="input-grid">
                <BaseInput
                  v-for="feature in selectedModel?.features || []"
                  :key="feature"
                  v-model="testInputs[feature]"
                  :label="feature"
                  type="number"
                  :step="0.1"
                  placeholder="Enter value..."
                />
              </div>
            </div>
            
            <div class="test-results">
              <h4>Prediction Results</h4>
              <div v-if="predictionResult" class="results-display">
                <div class="result-item">
                  <span class="result-label">Remaining Useful Life:</span>
                  <span class="result-value">{{ predictionResult.rul }} days</span>
                </div>
                <div class="result-item">
                  <span class="result-label">Confidence:</span>
                  <span class="result-value">{{ formatPercentage(predictionResult.confidence) }}</span>
                </div>
                <div class="result-item">
                  <span class="result-label">Risk Level:</span>
                  <span 
                    class="risk-badge"
                    :class="`risk-${predictionResult.riskLevel}`"
                  >
                    {{ predictionResult.riskLevel }}
                  </span>
                </div>
              </div>
              <div v-else class="no-results">
                <p>Enter test values and click "Run Prediction" to see results</p>
              </div>
            </div>
            
            <div class="test-actions">
              <BaseButton 
                variant="primary" 
                @click="runPrediction"
                :loading="testing"
                :disabled="!canRunPrediction"
              >
                Run Prediction
              </BaseButton>
              <BaseButton 
                variant="outline" 
                @click="resetTestInputs"
              >
                Reset
              </BaseButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation -->
    <div v-if="showDeleteConfirm" class="confirmation-dialog glass-panel">
      <div class="dialog-content">
        <AlertTriangle class="warning-icon" />
        <h3>Delete Model</h3>
        <p>
          Are you sure you want to delete "{{ modelToDelete?.name }}"? 
          This will remove the model from all deployed machines.
        </p>
        <div class="dialog-actions">
          <BaseButton 
            variant="critical" 
            @click="confirmDelete"
            :loading="processing"
          >
            Delete Model
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
import { 
  fetchAllAIModels, 
  deployAIModel, 
  deleteAIModel, 
  predictWithModel,
  getModelMetrics,
  retrainModel
} from '@/services/aiModels.service'
import type { MachineDto, AIModelDto, ModelInputDto } from '@/api/types'
import { 
  Upload, 
  Edit, 
  Copy, 
  Trash2, 
  Activity, 
  RefreshCw, 
  Brain, 
  X, 
  AlertTriangle,
  Info,
  FileText
} from 'lucide-vue-next'

const toast = useToast()

// Reactive state
const models = ref<ModelDefinition[]>([])
const selectedModel = ref<ModelDefinition | null>(null)
const loading = ref(false)
const processing = ref(false)
const testing = ref(false)

// Modal state
const showDeploymentModal = ref(false)
const showTestingModal = ref(false)
const showDeleteConfirm = ref(false)
const isDragging = ref(false)
const modelToDelete = ref<ModelDefinition | null>(null)

// Form state
const deploymentForm = reactive({
  id: '',
  name: '',
  description: '',
  version: '',
  status: 'draft' as 'draft' | 'training' | 'deployed' | 'archived',
  modelType: 'degradation' as 'degradation' | 'failure' | 'performance' | 'quality',
  algorithm: 'random_forest' as 'random_forest' | 'neural_network' | 'svm' | 'xgboost' | 'lstm',
  accuracy: 0,
  precision: 0,
  recall: 0,
  f1Score: 0,
  trainingDataSize: 0,
  features: '', // String input for features
  deployedMachines: [] as string[],
  tags: [] as string[],
  targetMachines: [] as string[], // Add missing property
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
  lastTraining: new Date().toISOString()
})

const formErrors = reactive<Record<string, string>>({})
const deploymentTags = ref('')
const selectedFile = ref<File | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)

// Test state
const testInputs = reactive<Record<string, number>>({})
const predictionResult = ref<{
  rul: number
  confidence: number
  riskLevel: 'low' | 'medium' | 'high'
} | null>(null)

// Machine data for selectors
const availableMachines = ref<Array<{ label: string; value: string }>>([])

// Computed properties
const isDeploymentFormValid = computed(() => {
  return deploymentForm.name && 
         deploymentForm.version && 
         deploymentForm.modelType &&
         deploymentForm.algorithm &&
         deploymentForm.accuracy > 0 &&
         selectedFile.value
})

const canRunPrediction = computed(() => {
  return selectedModel.value && 
         Object.values(testInputs).every(val => val !== undefined && val !== null)
})

// Interfaces
interface ModelDefinition {
  id: string
  name: string
  description: string
  version: string
  status: 'draft' | 'training' | 'deployed' | 'archived'
  modelType: 'degradation' | 'failure' | 'performance' | 'quality'
  algorithm: 'random_forest' | 'neural_network' | 'svm' | 'xgboost' | 'lstm'
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  trainingDataSize: number
  features: string[]
  deployedMachines: string[]
  tags: string[]
  createdAt: string
  updatedAt: string
  lastTraining: string
}

// Options
const modelTypes = [
  { label: 'Degradation Prediction', value: 'degradation' },
  { label: 'Failure Prediction', value: 'failure' },
  { label: 'Performance Optimization', value: 'performance' },
  { label: 'Quality Control', value: 'quality' }
]

const algorithms = [
  { label: 'Random Forest', value: 'random_forest' },
  { label: 'Neural Network', value: 'neural_network' },
  { label: 'Support Vector Machine', value: 'svm' },
  { label: 'XGBoost', value: 'xgboost' },
  { label: 'LSTM', value: 'lstm' }
]

// Methods
const loadModels = async () => {
  try {
    loading.value = true
    models.value = await fetchAllAIModels()
  } catch (error) {
    console.error('Failed to load models:', error)
    toast.error('Unable to load AI models')
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

const refreshModels = async () => {
  await loadModels()
  toast.success('Models refreshed')
}

const openDeployModel = () => {
  resetDeploymentForm()
  showDeploymentModal.value = true
}

const closeDeploymentModal = () => {
  showDeploymentModal.value = false
  resetDeploymentForm()
}

const editModel = (model: ModelDefinition) => {
  // In a real implementation, this would open an edit modal
  toast.info('Model editing functionality coming soon')
}

const duplicateModel = (model: ModelDefinition) => {
  // In a real implementation, this would create a copy
  toast.info('Model duplication functionality coming soon')
}

const deleteModel = (model: AIModelDto) => {
  modelToDelete.value = model
  showDeleteConfirm.value = true
}

const confirmDelete = async () => {
  if (!modelToDelete.value) return
  
  try {
    processing.value = true
    await deleteAIModel(modelToDelete.value.id)
    models.value = models.value.filter(m => m.id !== modelToDelete.value!.id)
    toast.success('Model deleted successfully')
    showDeleteConfirm.value = false
    modelToDelete.value = null
  } catch (error) {
    console.error('Delete failed:', error)
    toast.error('Failed to delete model')
  } finally {
    processing.value = false
  }
}

const cancelDelete = () => {
  showDeleteConfirm.value = false
  modelToDelete.value = null
}

const deployModel = async () => {
  try {
    processing.value = true
    
    // Validate form
    const errors: Record<string, string> = {}
    if (!deploymentForm.name) errors.name = 'Name is required'
    
    if (Object.keys(errors).length > 0) {
      Object.assign(formErrors, errors)
      return
    }
    
    // Clear previous errors
    Object.keys(formErrors).forEach(key => delete formErrors[key])
    
    // Add tags
    deploymentForm.tags = deploymentTags.value
      .split(',')
      .map(tag => tag.trim())
      .filter(tag => tag)
    
    // Parse features
    const featuresArray = deploymentForm.features
      .split(',')
      .map((feature: string) => feature.trim())
      .filter((feature: string) => feature)
    
    // Deploy model
    deploymentForm.id = `mdl_${Date.now()}`
    deploymentForm.createdAt = new Date().toISOString()
    deploymentForm.updatedAt = new Date().toISOString()
    deploymentForm.status = 'deployed'
    
    const modelToDeploy: ModelDefinition = {
      ...deploymentForm,
      features: featuresArray,
      tags: deploymentForm.tags
    }
    
    models.value.push(modelToDeploy)
    toast.success('Model deployed successfully')
    
    closeDeploymentModal()
    
  } catch (error) {
    console.error('Deployment failed:', error)
    toast.error('Failed to deploy model')
  } finally {
    processing.value = false
  }
}

const testModel = (model: ModelDefinition) => {
  selectedModel.value = model
  initializeTestInputs(model)
  showTestingModal.value = true
}

const closeTestingModal = () => {
  showTestingModal.value = false
  selectedModel.value = null
  predictionResult.value = null
  Object.keys(testInputs).forEach(key => delete testInputs[key])
}

const runPrediction = async () => {
  if (!selectedModel.value) return
  
  try {
    testing.value = true
    
    // Convert test inputs to the format expected by the API
    const modelInput: ModelInputDto = {
      features: { ...testInputs }
    }
    
    const prediction = await predictWithModel(selectedModel.value.id, modelInput)
    
    predictionResult.value = {
      rul: prediction.remainingUsefulLife,
      confidence: prediction.confidence,
      riskLevel: prediction.riskLevel as 'low' | 'medium' | 'high'
    }
    
    toast.success('Prediction completed')
    
  } catch (error) {
    console.error('Prediction failed:', error)
    toast.error('Failed to run prediction')
  } finally {
    testing.value = false
  }
}

const selectModel = (model: ModelDefinition) => {
  selectedModel.value = model
}

const resetDeploymentForm = () => {
  Object.assign(deploymentForm, {
    id: '',
    name: '',
    description: '',
    version: '',
    status: 'draft',
    modelType: 'degradation',
    algorithm: 'random_forest',
    accuracy: 0,
    precision: 0,
    recall: 0,
    f1Score: 0,
    trainingDataSize: 0,
    features: '',
    deployedMachines: [],
    tags: [],
    targetMachines: [],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    lastTraining: new Date().toISOString()
  })
  deploymentTags.value = ''
  selectedFile.value = null
  Object.keys(formErrors).forEach(key => delete formErrors[key])
}

const initializeTestInputs = (model: ModelDefinition) => {
  model.features.forEach(feature => {
    testInputs[feature] = 0
  })
}

const resetTestInputs = () => {
  Object.keys(testInputs).forEach(key => {
    testInputs[key] = 0
  })
  predictionResult.value = null
}

const triggerFileSelect = () => {
  fileInput.value?.click()
}

const handleFileSelect = (event: Event) => {
  const input = event.target as HTMLInputElement
  if (input.files && input.files[0]) {
    selectedFile.value = input.files[0]
  }
}

const handleFileDrop = (event: DragEvent) => {
  isDragging.value = false
  if (event.dataTransfer?.files && event.dataTransfer.files[0]) {
    selectedFile.value = event.dataTransfer.files[0]
  }
}

const removeSelectedFile = () => {
  selectedFile.value = null
  if (fileInput.value) {
    fileInput.value.value = ''
  }
}

const formatPercentage = (value: number): string => {
  return `${(value * 100).toFixed(1)}%`
}

const formatDate = (dateString: string): string => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

const formatFileSize = (bytes: number): string => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

// Lifecycle
onMounted(() => {
  loadModels()
  loadMachines()
})
</script>

<style scoped>
.ai-model-management {
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

.models-container {
  margin-top: var(--spacing-lg);
}

.models-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: var(--spacing-lg);
}

.model-card {
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid var(--color-border-subtle);
}

.model-card:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-medium);
}

.model-card.active {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 5%, transparent);
}

.model-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: var(--spacing-md);
}

.model-info h4 {
  margin: 0 0 var(--spacing-xs) 0;
  color: var(--color-text-primary);
}

.model-version {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  background: var(--color-surface-alt);
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  margin-right: var(--spacing-sm);
}

.model-status {
  font-size: 0.75rem;
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-weight: 500;
  text-transform: capitalize;
}

.model-status.status-draft {
  background: color-mix(in srgb, var(--color-text-secondary) 20%, transparent);
  color: var(--color-text-secondary);
}

.model-status.status-training {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.model-status.status-deployed {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.model-status.status-archived {
  background: color-mix(in srgb, var(--color-text-secondary) 20%, transparent);
  color: var(--color-text-secondary);
}

.model-actions {
  display: flex;
  gap: var(--spacing-xs);
}

.model-details .description {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  line-height: 1.5;
}

.model-stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(100px, 1fr));
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

.model-metrics {
  margin-bottom: var(--spacing-md);
}

.metric {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.metric-bar {
  height: 6px;
  background: var(--color-surface-alt);
  border-radius: var(--radius-full);
  overflow: hidden;
}

.metric-fill {
  height: 100%;
  background: var(--color-primary);
  border-radius: var(--radius-full);
  transition: width 0.3s ease;
}

.metric-label {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.model-tags {
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

.testing-modal {
  max-width: 600px;
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

.deployment-form,
.testing-interface {
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

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--spacing-md);
}

.compatibility-info {
  display: flex;
  align-items: center;
  gap: var(--spacing-sm);
  padding: var(--spacing-md);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
  margin-top: var(--spacing-md);
}

.info-icon {
  width: 1rem;
  height: 1rem;
  color: var(--color-primary);
  flex-shrink: 0;
}

.compatibility-info p {
  margin: 0;
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.file-upload-area {
  border: 2px dashed var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--spacing-xl);
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
  background: var(--color-surface);
}

.file-upload-area:hover,
.file-upload-area.drag-over {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 5%, transparent);
}

.upload-icon {
  width: 3rem;
  height: 3rem;
  margin-bottom: var(--spacing-md);
  color: var(--color-text-secondary);
}

.file-upload-area p {
  margin: 0 0 var(--spacing-sm) 0;
  color: var(--color-text-primary);
}

.file-types {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin-bottom: var(--spacing-lg);
}

.file-input {
  display: none;
}

.selected-file {
  display: flex;
  align-items: center;
  gap: var(--spacing-md);
  padding: var(--spacing-md);
  background: var(--color-surface-alt);
  border-radius: var(--radius-md);
  margin-top: var(--spacing-md);
  border: 1px solid var(--color-border-subtle);
}

.file-icon {
  width: 1.5rem;
  height: 1.5rem;
  color: var(--color-primary);
  flex-shrink: 0;
}

.file-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-xs);
}

.file-name {
  font-weight: 500;
  color: var(--color-text-primary);
}

.file-size {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.remove-file {
  background: none;
  border: none;
  color: var(--color-error);
  cursor: pointer;
  padding: var(--spacing-xs);
  border-radius: var(--radius-full);
}

.remove-file:hover {
  background: color-mix(in srgb, var(--color-error) 10%, transparent);
}

.test-inputs,
.test-results {
  background: var(--color-surface-alt);
  border-radius: var(--radius-lg);
  padding: var(--spacing-lg);
  border: 1px solid var(--color-border-subtle);
}

.test-inputs h4,
.test-results h4 {
  margin: 0 0 var(--spacing-md) 0;
  color: var(--color-text-primary);
}

.input-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--spacing-md);
}

.results-display {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-md);
}

.result-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--spacing-sm);
  background: var(--color-surface);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
}

.result-label {
  font-weight: 500;
  color: var(--color-text-secondary);
}

.result-value {
  font-weight: 500;
  color: var(--color-text-primary);
}

.risk-badge {
  padding: var(--spacing-xs) var(--spacing-sm);
  border-radius: var(--radius-full);
  font-weight: 500;
  text-transform: capitalize;
}

.risk-badge.risk-low {
  background: color-mix(in srgb, var(--color-success) 20%, transparent);
  color: var(--color-success);
}

.risk-badge.risk-medium {
  background: color-mix(in srgb, var(--color-warning) 20%, transparent);
  color: var(--color-warning);
}

.risk-badge.risk-high {
  background: color-mix(in srgb, var(--color-error) 20%, transparent);
  color: var(--color-error);
}

.no-results {
  text-align: center;
  padding: var(--spacing-xl);
  color: var(--color-text-secondary);
}

.test-actions {
  display: flex;
  gap: var(--spacing-md);
  justify-content: flex-end;
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
  
  .models-grid {
    grid-template-columns: 1fr;
  }
  
  .model-header {
    flex-direction: column;
    gap: var(--spacing-sm);
  }
  
  .model-actions {
    width: 100%;
    justify-content: flex-end;
  }
  
  .modal-content {
    margin: var(--spacing-sm);
    max-height: 95vh;
  }
  
  .metrics-grid,
  .input-grid {
    grid-template-columns: 1fr;
  }
  
  .modal-actions,
  .test-actions,
  .dialog-actions {
    flex-direction: column;
  }
}
</style>