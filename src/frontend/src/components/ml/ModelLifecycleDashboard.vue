<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseTextarea from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { Package, Upload, GitBranch, BarChart3, Calendar, CheckCircle, Clock, AlertTriangle, XCircle, GitCommit, Tag, FileText, RefreshCw } from 'lucide-vue-next'
import { modelLifecycleService } from '@/services/modelLifecycle.service'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  BarChart as EChartsBar,
  PieChart as EChartsPie
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsLine,
  EChartsBar,
  EChartsPie,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const activeTab = ref('models')
const registering = ref(false)
const promoting = ref(false)
const comparing = ref(false)
const loadingModels = ref(false)
const models = ref<any[]>([])
const selectedModel = ref<any>(null)
const comparisonResult = ref<any>(null)

// Registration State
const modelType = ref('RUL')
const modelPath = ref('')
const trainingMetrics = ref('')
const datasetHash = ref('')
const notes = ref('')

// Promotion State
const targetStatus = ref('Staging')
const promotionNotes = ref('')

// Comparison State
const compareModel1 = ref('')
const compareModel2 = ref('')

// Chart refs
const statusChartRef = ref<HTMLDivElement | null>(null)
const performanceChartRef = ref<HTMLDivElement | null>(null)
const timelineChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let statusChartInstance: echarts.ECharts | null = null
let performanceChartInstance: echarts.ECharts | null = null
let timelineChartInstance: echarts.ECharts | null = null

// Options
const modelTypeOptions = [
  { label: 'RUL (Remaining Useful Life)', value: 'RUL' },
  { label: 'Anomaly Detection', value: 'Anomaly' },
  { label: 'Classification', value: 'Classification' },
  { label: 'Regression', value: 'Regression' }
]

const statusOptions = [
  { label: 'Development', value: 'Development' },
  { label: 'Testing', value: 'Testing' },
  { label: 'Staging', value: 'Staging' },
  { label: 'Production', value: 'Production' },
  { label: 'Archived', value: 'Archived' }
]

// Computed
const isValidRegistration = computed(() => {
  return modelType.value && modelPath.value && trainingMetrics.value
})

const productionModels = computed(() => {
  return models.value.filter(m => m.status === 'Production')
})

const developmentModels = computed(() => {
  return models.value.filter(m => m.status === 'Development')
})

const getModelById = (id: string) => {
  return models.value.find(m => m.id === id)
}

const getStatusColor = (status: string) => {
  const colors: Record<string, string> = {
    'Development': 'text-blue-600',
    'Testing': 'text-yellow-600', 
    'Staging': 'text-purple-600',
    'Production': 'text-green-600',
    'Archived': 'text-gray-600'
  }
  return colors[status] || 'text-gray-600'
}

const getStatusBgColor = (status: string) => {
  const colors: Record<string, string> = {
    'Development': 'bg-blue-100',
    'Testing': 'bg-yellow-100',
    'Staging': 'bg-purple-100', 
    'Production': 'bg-green-100',
    'Archived': 'bg-gray-100'
  }
  return colors[status] || 'bg-gray-100'
}

// Methods
const loadModels = async () => {
  try {
    loadingModels.value = true
    models.value = await modelLifecycleService.fetchModelLifecycles()
    
    if (models.value.length > 0) {
      if (!selectedModel.value) {
        selectedModel.value = models.value[0]
      } else {
        // Update selected model with new data
        selectedModel.value = models.value.find(m => m.id === selectedModel.value.id) || models.value[0]
      }
      renderStatusChart()
      renderPerformanceChart()
      renderTimelineChart()
    }
  } catch (error) {
    console.error('Failed to load models:', error)
    toast.error('Failed to load model versions')
  } finally {
    loadingModels.value = false
  }
}

const registerModel = async () => {
  try {
    registering.value = true
    
    let metricsObj = {}
    try {
      metricsObj = JSON.parse(trainingMetrics.value)
    } catch (e) {
      toast.error('Invalid JSON format for training metrics')
      return
    }
    
    const newModel = await modelLifecycleService.registerModelVersion({
      modelType: modelType.value,
      modelPath: modelPath.value,
      metrics: metricsObj,
      trainingDatasetHash: datasetHash.value || undefined,
      notes: notes.value || undefined
    })
    
    toast.success(`Model registered successfully`)
    
    // Reset form
    modelPath.value = ''
    trainingMetrics.value = ''
    datasetHash.value = ''
    notes.value = ''
    
    await loadModels()
    activeTab.value = 'models'
    selectedModel.value = newModel
  } catch (error) {
    console.error('Failed to register model:', error)
    toast.error('Failed to register model version')
  } finally {
    registering.value = false
  }
}

const promoteModel = async (modelId: string, status: string) => {
  try {
    promoting.value = true
    await modelLifecycleService.promoteModelVersion(modelId, {
      targetStatus: status,
      notes: promotionNotes.value || undefined
    })
    
    toast.success(`Model status updated to ${status}`)
    promotionNotes.value = ''
    await loadModels()
  } catch (error) {
    console.error('Failed to promote model:', error)
    toast.error('Failed to promote model version')
  } finally {
    promoting.value = false
  }
}

const compareModels = async () => {
  try {
    comparing.value = true
    
    if (!compareModel1.value || !compareModel2.value) {
      toast.error('Please select two models to compare')
      return
    }
    
    comparisonResult.value = await modelLifecycleService.compareModelVersions({
      modelVersionId1: compareModel1.value,
      modelVersionId2: compareModel2.value
    })
    
    toast.success('Models compared successfully')
  } catch (error) {
    console.error('Failed to compare models:', error)
    toast.error('Failed to compare models')
  } finally {
    comparing.value = false
  }
}

const renderStatusChart = () => {
  if (!statusChartRef.value) return
  
  if (!statusChartInstance) {
    statusChartInstance = echarts.init(statusChartRef.value)
  }
  
  const statusCounts: Record<string, number> = {}
  models.value.forEach(m => {
    statusCounts[m.status] = (statusCounts[m.status] || 0) + 1
  })
  
  const statuses = Object.keys(statusCounts)
  const counts = Object.values(statusCounts)
  
  const option = {
    title: {
      text: 'Model Status Distribution',
      left: 'center'
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 10,
      top: 50,
      data: statuses
    },
    series: [
      {
        name: 'Status',
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['60%', '50%'],
        data: statuses.map((status, index) => ({
          name: status,
          value: counts[index],
          itemStyle: {
            color: index === 0 ? '#3b82f6' : 
                  index === 1 ? '#10b981' : 
                  index === 2 ? '#f59e0b' : 
                  index === 3 ? '#8b5cf6' : '#6b7280'
          }
        })),
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }
  
  statusChartInstance.setOption(option, true)
}

const renderPerformanceChart = () => {
  if (!performanceChartRef.value || !selectedModel.value) return
  
  if (!performanceChartInstance) {
    performanceChartInstance = echarts.init(performanceChartRef.value)
  }
  
  const metrics = selectedModel.value.metrics
  const metricNames = Object.keys(metrics)
  const metricValues = Object.values(metrics)
  
  const option = {
    title: {
      text: `Performance Metrics - ${selectedModel.value.version}`,
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: metricNames
    },
    yAxis: {
      type: 'value'
    },
    series: [{
      name: 'Metrics',
      type: 'bar',
      data: metricValues.map((value: any, index: number) => ({
        value,
        itemStyle: { 
          color: index === 0 ? '#3b82f6' : 
                index === 1 ? '#10b981' : 
                index === 2 ? '#f59e0b' : '#8b5cf6'
        }
      }))
    }]
  }
  
  performanceChartInstance.setOption(option, true)
}

const renderTimelineChart = () => {
  if (!timelineChartRef.value) return
  
  if (!timelineChartInstance) {
    timelineChartInstance = echarts.init(timelineChartRef.value)
  }
  
  // Sort models by creation date
  const sortedModels = [...models.value].sort((a, b) => 
    new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
  )
  
  const dates = sortedModels.map(m => {
    const date = new Date(m.createdAt)
    return `${date.getMonth() + 1}/${date.getDate()}`
  })
  
  const versions = sortedModels.map(m => m.version)
  
  const option = {
    title: {
      text: 'Model Development Timeline',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    xAxis: {
      type: 'category',
      data: dates
    },
    yAxis: {
      type: 'value',
      name: 'Version'
    },
    series: [{
      name: 'Versions',
      type: 'line',
      data: versions.map((v, i) => i + 1),
      smooth: true,
      itemStyle: { color: '#8b5cf6' }
    }]
  }
  
  timelineChartInstance.setOption(option, true)
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

const formatDateTime = (dateString: string) => {
  return new Date(dateString).toLocaleString()
}

const resizeCharts = () => {
  statusChartInstance?.resize()
  performanceChartInstance?.resize()
  timelineChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  loadModels()
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  statusChartInstance?.dispose()
  performanceChartInstance?.dispose()
  timelineChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="model-lifecycle space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Model Lifecycle Management</h1>
          <p class="text-gray-600 mt-2">Manage ML model versions through their complete lifecycle</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="loadModels"
            :disabled="loadingModels"
          >
            <RefreshCw class="w-4 h-4 mr-2" :class="{ 'animate-spin': loadingModels }" />
            Refresh
          </BaseButton>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in [
              { id: 'models', name: 'Model Versions', icon: Package },
              { id: 'register', name: 'Register New', icon: Upload },
              { id: 'compare', name: 'Compare Models', icon: GitBranch }
            ]"
            :key="tab.id"
            @click="activeTab = tab.id"
            class="whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm"
            :class="[
              activeTab === tab.id
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            ]"
          >
            <component :is="tab.icon" class="w-4 h-4 inline mr-2" />
            {{ tab.name }}
          </button>
        </nav>
      </div>

      <!-- Model Versions Tab -->
      <div v-show="activeTab === 'models'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Model Versions</h2>
            
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
              <BaseCard>
                <div class="p-4 text-center">
                  <Package class="w-8 h-8 text-blue-500 mx-auto mb-2" />
                  <p class="text-2xl font-bold">{{ models.length }}</p>
                  <p class="text-sm text-gray-600">Total Models</p>
                </div>
              </BaseCard>
              
              <BaseCard>
                <div class="p-4 text-center">
                  <CheckCircle class="w-8 h-8 text-green-500 mx-auto mb-2" />
                  <p class="text-2xl font-bold text-green-600">{{ productionModels.length }}</p>
                  <p class="text-sm text-gray-600">Production</p>
                </div>
              </BaseCard>
              
              <BaseCard>
                <div class="p-4 text-center">
                  <Clock class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
                  <p class="text-2xl font-bold text-yellow-600">{{ developmentModels.length }}</p>
                  <p class="text-sm text-gray-600">In Development</p>
                </div>
              </BaseCard>
            </div>
            
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <div ref="statusChartRef" class="w-full h-80"></div>
              <div ref="timelineChartRef" class="w-full h-80"></div>
            </div>
            
            <!-- Models List -->
            <div class="mt-6">
              <h3 class="font-medium mb-3">All Model Versions</h3>
              <div class="space-y-3">
                <BaseCard
                  v-for="model in models"
                  :key="model.id"
                  class="hover:shadow-md transition-shadow cursor-pointer"
                  :class="{ 'ring-2 ring-blue-500': selectedModel?.id === model.id }"
                  @click="selectedModel = model"
                >
                  <div class="p-4">
                    <div class="flex justify-between items-start">
                      <div>
                        <h4 class="font-medium flex items-center gap-2">
                          <GitCommit class="w-4 h-4" />
                          {{ model.id }}
                          <Tag class="w-3 h-3 text-gray-400" />
                          <span class="text-sm font-mono">{{ model.version }}</span>
                        </h4>
                        <p class="text-sm text-gray-600 mt-1">{{ model.modelType }} Model</p>
                        <p class="text-xs text-gray-500 mt-1">Trained: {{ formatDate(model.trainedAt) }}</p>
                      </div>
                      
                      <div class="flex items-center gap-2">
                        <span 
                          class="px-2 py-1 rounded-full text-xs font-medium"
                          :class="[getStatusBgColor(model.status), getStatusColor(model.status)]"
                        >
                          {{ model.status }}
                        </span>
                        
                        <BaseButton 
                          v-if="model.status !== 'Production'"
                          variant="outline" 
                          size="sm"
                          @click.stop="promoteModel(model.id, 'Production')"
                          :disabled="promoting"
                        >
                          <CheckCircle class="w-3 h-3 mr-1" />
                          Promote
                        </BaseButton>
                      </div>
                    </div>
                    
                    <!-- Metrics Preview -->
                    <div class="mt-3 flex flex-wrap gap-3 text-xs">
                      <span v-for="(value, key) in model.metrics" :key="key" class="bg-gray-100 px-2 py-1 rounded">
                        {{ String(key).toUpperCase() }}: {{ typeof value === 'number' ? value.toFixed(2) : value }}
                      </span>
                    </div>
                  </div>
                </BaseCard>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Register New Model Tab -->
      <div v-show="activeTab === 'register'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Register New Model Version</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <BaseSelect
                v-model="modelType"
                :options="modelTypeOptions"
                label="Model Type"
              />
              
              <BaseInput
                v-model="modelPath"
                label="Model Path"
                placeholder="Enter path to model file"
              />
            </div>
            
            <BaseTextarea
              v-model="trainingMetrics"
              label="Training Metrics (JSON)"
              placeholder='{"rmse": 15.2, "mae": 12.1, "r2": 0.87}'
              rows="4"
              type="textarea"
            />
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
              <BaseInput
                v-model="datasetHash"
                label="Training Dataset Hash (Optional)"
                placeholder="Enter dataset hash"
              />
              
              <BaseInput
                v-model="notes"
                label="Notes (Optional)"
                placeholder="Additional information about this model version"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="registerModel"
              :disabled="!isValidRegistration || registering"
              class="w-full md:w-auto mt-6"
            >
              <Upload class="w-4 h-4 mr-2" :class="{ 'animate-spin': registering }" />
              {{ registering ? 'Registering...' : 'Register Model' }}
            </BaseButton>
          </div>
        </BaseCard>
      </div>

      <!-- Compare Models Tab -->
      <div v-show="activeTab === 'compare'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Compare Model Versions</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
              <BaseSelect
                v-model="compareModel1"
                :options="models.map(m => ({ label: `${m.id} (${m.version})`, value: m.id }))"
                label="First Model"
              />
              
              <BaseSelect
                v-model="compareModel2"
                :options="models.map(m => ({ label: `${m.id} (${m.version})`, value: m.id }))"
                label="Second Model"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="compareModels"
              :disabled="!compareModel1 || !compareModel2 || comparing"
              class="w-full md:w-auto"
            >
              <GitBranch class="w-4 h-4 mr-2" :class="{ 'animate-spin': comparing }" />
              {{ comparing ? 'Comparing...' : 'Compare Models' }}
            </BaseButton>
            
            <!-- Comparison Results -->
            <div v-if="comparisonResult" class="mt-6">
              <h3 class="font-medium mb-4">Comparison Results</h3>
              
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                <BaseCard>
                  <div class="p-4">
                    <h4 class="font-medium mb-2">Model 1: {{ comparisonResult.model1.id }}</h4>
                    <div class="text-sm space-y-1">
                      <div v-for="(value, key) in comparisonResult.model1.metrics" :key="key">
                        <span class="font-medium">{{ String(key).toUpperCase() }}:</span> {{ value }}
                      </div>
                    </div>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <h4 class="font-medium mb-2">Model 2: {{ comparisonResult.model2.id }}</h4>
                    <div class="text-sm space-y-1">
                      <div v-for="(value, key) in comparisonResult.model2.metrics" :key="key">
                        <span class="font-medium">{{ String(key).toUpperCase() }}:</span> {{ value }}
                      </div>
                    </div>
                  </div>
                </BaseCard>
              </div>
              
              <BaseCard>
                <div class="p-4">
                  <h4 class="font-medium mb-2">Differences</h4>
                  <div class="grid grid-cols-3 gap-4 text-center">
                    <div>
                      <p class="text-sm text-gray-600">RMSE Diff</p>
                      <p class="text-lg font-bold" :class="comparisonResult.differences.rmse >= 0 ? 'text-green-600' : 'text-red-600'">
                        {{ comparisonResult.differences.rmse.toFixed(2) }}
                      </p>
                    </div>
                    <div>
                      <p class="text-sm text-gray-600">MAE Diff</p>
                      <p class="text-lg font-bold" :class="comparisonResult.differences.mae >= 0 ? 'text-green-600' : 'text-red-600'">
                        {{ comparisonResult.differences.mae.toFixed(2) }}
                      </p>
                    </div>
                    <div>
                      <p class="text-sm text-gray-600">R² Diff</p>
                      <p class="text-lg font-bold" :class="comparisonResult.differences.r2 >= 0 ? 'text-green-600' : 'text-red-600'">
                        {{ comparisonResult.differences.r2.toFixed(3) }}
                      </p>
                    </div>
                  </div>
                  
                  <div class="mt-4 p-3 bg-green-50 rounded border border-green-200">
                    <p class="text-center font-medium text-green-800">
                      Winner: {{ comparisonResult.winner }}
                    </p>
                  </div>
                </div>
              </BaseCard>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Selected Model Details -->
      <BaseCard v-if="selectedModel">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Selected Model Details</h2>
          
          <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div>
              <h3 class="font-medium mb-3">Basic Information</h3>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">ID:</span> <span class="font-mono">{{ selectedModel.id }}</span></div>
                <div><span class="text-gray-600">Version:</span> {{ selectedModel.version }}</div>
                <div><span class="text-gray-600">Type:</span> {{ selectedModel.modelType }}</div>
                <div><span class="text-gray-600">Path:</span> {{ selectedModel.modelPath }}</div>
                <div><span class="text-gray-600">Status:</span> 
                  <span 
                    class="px-2 py-1 rounded-full text-xs font-medium"
                    :class="[getStatusBgColor(selectedModel.status), getStatusColor(selectedModel.status)]"
                  >
                    {{ selectedModel.status }}
                  </span>
                </div>
              </div>
            </div>
            
            <div>
              <h3 class="font-medium mb-3">Training Info</h3>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">Trained:</span> {{ formatDateTime(selectedModel.trainedAt) }}</div>
                <div v-if="selectedModel.promotedAt"><span class="text-gray-600">Promoted:</span> {{ formatDateTime(selectedModel.promotedAt) }}</div>
                <div v-if="selectedModel.trainingDatasetHash"><span class="text-gray-600">Dataset Hash:</span> {{ selectedModel.trainingDatasetHash }}</div>
                <div v-if="selectedModel.notes"><span class="text-gray-600">Notes:</span> {{ selectedModel.notes }}</div>
              </div>
            </div>
            
            <div>
              <h3 class="font-medium mb-3">Actions</h3>
              <div class="space-y-2">
                <BaseSelect
                  v-model="targetStatus"
                  :options="statusOptions.filter(s => s.value !== selectedModel.status)"
                  label="Change Status"
                />
                <BaseInput
                  v-model="promotionNotes"
                  placeholder="Notes for status change"
                  type="text"
                />
                <BaseButton 
                  variant="outline" 
                  @click="promoteModel(selectedModel.id, targetStatus)"
                  :disabled="promoting"
                  class="w-full"
                >
                  <GitBranch class="w-4 h-4 mr-2" />
                  Update Status
                </BaseButton>
              </div>
            </div>
          </div>
          
          <div ref="performanceChartRef" class="w-full h-64 mt-6"></div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.model-lifecycle {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .model-lifecycle {
    padding: 0.5rem;
  }
}
</style>