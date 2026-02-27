<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  runToFailureSimulation,
  generateTrajectories,
  getRunToFailureResults,
  type RunToFailureResult,
  type DegradationTrajectory,
  type GenerateTrajectoriesRequest,
  type RunToFailureOptions
} from '@/services/runToFailure.service'
import { 
  Play, 
  BarChart3, 
  TrendingDown,
  Download,
  Settings,
  RotateCcw,
  Clock,
  Activity,
  AlertTriangle
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  LineChart as EChartsLine,
  ScatterChart as EChartsScatter
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
  EChartsScatter,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  DataZoomComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const activeTab = ref('single')
const simulating = ref(false)
const generating = ref(false)

// Single Simulation State
const singleMachineId = ref('')
const isValidMachineId = computed(() => {
  const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i
  return uuidRegex.test(singleMachineId.value)
})
const singleMaxSteps = ref(1000)
const singleMaxTime = ref('01:00:00') // 1 hour
const singleStepInterval = ref('00:00:01') // 1 second
const singleGenerateTelemetry = ref(true)
const singleStoreTrajectory = ref(false)
const singleRandomSeed = ref<string | number>('')
const singleResult: any = ref<RunToFailureResult | null>(null)

// Batch Generation State
const batchMachineType = ref('CNC')
const batchCount = ref(10)
const batchMaxSteps = ref(500)
const batchGenerateTelemetry = ref(false)
const batchTrajectories: any = ref<DegradationTrajectory[]>([])

// Chart refs
const degradationChartRef = ref<HTMLDivElement | null>(null)
const trajectoriesChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let degradationChartInstance: echarts.ECharts | null = null
let trajectoriesChartInstance: echarts.ECharts | null = null

// Options
const machineTypes = [
  { label: 'CNC Machine', value: 'CNC' },
  { label: 'Injection Molder', value: 'InjectionMolder' },
  { label: 'Press Machine', value: 'Press' },
  { label: 'Robot Arm', value: 'Robot' },
  { label: 'Conveyor System', value: 'Conveyor' }
]

// Computed
const failureRisk = computed(() => {
  if (!singleResult.value) return 'Unknown'
  
  const degradation = singleResult.value.finalDegradationState
  if (degradation < 0.3) return 'Low'
  if (degradation < 0.7) return 'Medium'
  return 'High'
})

const failureRiskColor = computed(() => {
  const risk = failureRisk.value
  if (risk === 'Low') return 'text-green-600'
  if (risk === 'Medium') return 'text-yellow-600'
  return 'text-red-600'
})

const timeToFailureFormatted = computed(() => {
  if (!singleResult.value) return 'N/A'
  
  // Parse TimeSpan format (hh:mm:ss)
  const timeParts = singleResult.value.timeToFailure.split(':')
  if (timeParts.length !== 3) return singleResult.value.timeToFailure
  
  const hours = parseInt(timeParts[0])
  const minutes = parseInt(timeParts[1])
  const seconds = parseInt(timeParts[2])
  
  if (hours > 0) return `${hours}h ${minutes}m`
  if (minutes > 0) return `${minutes}m ${seconds}s`
  return `${seconds}s`
})

// Methods
const runSingleSimulation = async () => {
  try {
    simulating.value = true
    
    const seedValue = singleRandomSeed.value === '' ? undefined : Number(singleRandomSeed.value)
    
    const options: RunToFailureOptions = {
      maxSteps: singleMaxSteps.value,
      maxSimulationTime: singleMaxTime.value,
      stepInterval: singleStepInterval.value,
      generateTelemetry: singleGenerateTelemetry.value,
      storeTrajectory: singleStoreTrajectory.value,
      randomSeed: seedValue
    }
    
    singleResult.value = await runToFailureSimulation(singleMachineId.value, options)
    renderDegradationChart()
    
    const status = singleResult.value.reachedFailureThreshold ? 'failure' : 'completed'
    toast.success(`Simulation ${status} - Time to failure: ${timeToFailureFormatted.value}`)
  } catch (error) {
    console.error('Failed to run simulation:', error)
    toast.error('Failed to run run-to-failure simulation')
  } finally {
    simulating.value = false
  }
}

const generateBatchTrajectories = async () => {
  try {
    generating.value = true
    
    const request: GenerateTrajectoriesRequest = {
      machineType: batchMachineType.value,
      count: batchCount.value,
      options: {
        maxSteps: batchMaxSteps.value,
        generateTelemetry: batchGenerateTelemetry.value
      }
    }
    
    batchTrajectories.value = await generateTrajectories(request)
    renderTrajectoriesChart()
    
    toast.success(`Generated ${batchTrajectories.value.length} degradation trajectories`)
  } catch (error) {
    console.error('Failed to generate trajectories:', error)
    toast.error('Failed to generate degradation trajectories')
  } finally {
    generating.value = false
  }
}

const resetSimulation = () => {
  singleResult.value = null
  if (degradationChartInstance) {
    degradationChartInstance.clear()
  }
}

const renderDegradationChart = () => {
  if (!degradationChartRef.value || !singleResult.value) return
  
  if (!degradationChartInstance) {
    degradationChartInstance = echarts.init(degradationChartRef.value)
  }
  
  const trajectory = singleResult.value.trajectory
  const timeData = trajectory.map((point: any) => point.step)
  const degradationData = trajectory.map((point: any) => point.degradationState * 100)
  
  // Extract sensor data for visualization
  const sensorKeys = Object.keys(trajectory[0]?.sensorReadings || {})
  const sensorSeries = sensorKeys.map(key => ({
    name: key,
    type: 'line',
    data: trajectory.map((point: any) => [
      point.step,
      point.sensorReadings[key]
    ]),
    smooth: true,
    yAxisIndex: 1
  }))
  
  const option = {
    title: {
      text: `Degradation Profile - Machine ${singleResult.value.machineId.substring(0, 8)}`,
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30,
      data: ['Degradation %', ...sensorKeys]
    },
    xAxis: {
      type: 'value',
      name: 'Simulation Steps'
    },
    yAxis: [
      {
        type: 'value',
        name: 'Degradation (%)',
        min: 0,
        max: 100
      },
      {
        type: 'value',
        name: 'Sensor Values',
        position: 'right'
      }
    ],
    series: [
      {
        name: 'Degradation %',
        type: 'line',
        data: timeData.map((step: number, index: number) => [step, degradationData[index]]),
        smooth: true,
        itemStyle: { color: '#ef4444' },
        markLine: {
          silent: true,
          lineStyle: {
            color: '#f59e0b',
            type: 'dashed'
          },
          data: [{
            yAxis: 80,
            name: 'Failure Threshold'
          }]
        }
      },
      ...sensorSeries
    ]
  }
  
  degradationChartInstance.setOption(option, true)
}

const renderTrajectoriesChart = () => {
  if (!trajectoriesChartRef.value || batchTrajectories.value.length === 0) return
  
  if (!trajectoriesChartInstance) {
    trajectoriesChartInstance = echarts.init(trajectoriesChartRef.value)
  }
  
  // Show first few trajectories for visualization
  const sampleTrajectories = batchTrajectories.value.slice(0, 5)
  
  const series = sampleTrajectories.map((trajectory: any, index: number) => {
    const degradationData = trajectory.snapshot.map((point: any) => [
      point.step,
      point.degradationState * 100
    ])
    
    return {
      name: `Trajectory ${index + 1}`,
      type: 'line',
      data: degradationData,
      smooth: true
    }
  })
  
  const option = {
    title: {
      text: `Degradation Trajectories (${batchMachineType.value})`,
      left: 'center'
    },
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      top: 30
    },
    xAxis: {
      type: 'value',
      name: 'Simulation Steps'
    },
    yAxis: {
      type: 'value',
      name: 'Degradation (%)',
      min: 0,
      max: 100
    },
    series: series
  }
  
  trajectoriesChartInstance.setOption(option, true)
}

const downloadResults = (data: any, filename: string) => {
  const dataStr = JSON.stringify(data, null, 2)
  const blob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  
  const a = document.createElement('a')
  a.href = url
  a.download = `${filename}-${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  toast.success('Results downloaded successfully')
}

const resizeCharts = () => {
  degradationChartInstance?.resize()
  trajectoriesChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  degradationChartInstance?.dispose()
  trajectoriesChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="run-to-failure space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Run-to-Failure Simulation</h1>
          <p class="text-gray-600 mt-2">Simulate equipment degradation and predict time to failure</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="resetSimulation"
            :disabled="!singleResult"
          >
            <RotateCcw class="w-4 h-4 mr-2" />
            Reset
          </BaseButton>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in [
              { id: 'single', name: 'Single Simulation', icon: Play },
              { id: 'batch', name: 'Batch Generation', icon: BarChart3 }
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

      <!-- Single Simulation Tab -->
      <div v-show="activeTab === 'single'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Single Machine Simulation</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              <BaseInput
                v-model="singleMachineId"
                label="Machine ID"
                placeholder="Enter machine UUID"
              />
              
              <BaseInput
                v-model.number="singleMaxSteps"
                label="Maximum Steps"
                type="number"
                min="1"
                max="10000"
              />
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseInput
                v-model="singleMaxTime"
                label="Max Simulation Time"
                placeholder="HH:MM:SS"
              />
              
              <BaseInput
                v-model="singleStepInterval"
                label="Step Interval"
                placeholder="HH:MM:SS"
              />
              
              <BaseInput
                v-model="singleRandomSeed"
                label="Random Seed (optional)"
                type="number"
                placeholder="Leave blank for random"
              />
            </div>
            
            <div class="flex items-center gap-6 mb-6">
              <label class="flex items-center">
                <input
                  v-model="singleGenerateTelemetry"
                  type="checkbox"
                  class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="ml-2 text-sm text-gray-700">Generate Telemetry Data</span>
              </label>
              
              <label class="flex items-center">
                <input
                  v-model="singleStoreTrajectory"
                  type="checkbox"
                  class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="ml-2 text-sm text-gray-700">Store Trajectory</span>
              </label>
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="runSingleSimulation"
              :disabled="simulating"
              class="w-full md:w-auto"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': simulating }" />
              {{ simulating ? 'Simulating...' : 'Run Simulation' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="singleResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Simulation Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadResults(singleResult, 'run-to-failure-results')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4 text-center">
                    <Clock class="w-8 h-8 text-blue-500 mx-auto mb-2" />
                    <p class="text-2xl font-bold">{{ timeToFailureFormatted }}</p>
                    <p class="text-sm text-gray-600">Time to Failure</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <Activity class="w-8 h-8 text-green-500 mx-auto mb-2" />
                    <p class="text-2xl font-bold">{{ singleResult.stepsToFailure }}</p>
                    <p class="text-sm text-gray-600">Steps to Failure</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <TrendingDown class="w-8 h-8 text-orange-500 mx-auto mb-2" />
                    <p class="text-2xl font-bold">{{ (singleResult.finalDegradationState * 100).toFixed(1) }}%</p>
                    <p class="text-sm text-gray-600">Final Degradation</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <AlertTriangle class="w-8 h-8 text-red-500 mx-auto mb-2" />
                    <p class="text-2xl font-bold" :class="failureRiskColor">{{ failureRisk }}</p>
                    <p class="text-sm text-gray-600">Risk Level</p>
                  </div>
                </BaseCard>
              </div>
              
              <div class="bg-gray-50 p-4 rounded-lg mb-6">
                <h4 class="font-medium mb-2">Termination Reason</h4>
                <p class="text-sm">{{ singleResult.terminationReason || 'Simulation completed' }}</p>
              </div>
              
              <div ref="degradationChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Batch Generation Tab -->
      <div v-show="activeTab === 'batch'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Batch Trajectory Generation</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseSelect
                v-model="batchMachineType"
                :options="machineTypes"
                label="Machine Type"
              />
              
              <BaseInput
                v-model.number="batchCount"
                label="Number of Trajectories"
                type="number"
                min="1"
                max="100"
              />
              
              <BaseInput
                v-model.number="batchMaxSteps"
                label="Max Steps per Trajectory"
                type="number"
                min="1"
                max="1000"
              />
            </div>
            
            <div class="flex items-center mb-6">
              <label class="flex items-center">
                <input
                  v-model="batchGenerateTelemetry"
                  type="checkbox"
                  class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                />
                <span class="ml-2 text-sm text-gray-700">Generate Telemetry Data</span>
              </label>
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="generateBatchTrajectories"
              :disabled="generating"
              class="w-full md:w-auto"
            >
              <BarChart3 class="w-4 h-4 mr-2" :class="{ 'animate-spin': generating }" />
              {{ generating ? 'Generating...' : 'Generate Trajectories' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="batchTrajectories.length > 0" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">
                  Generated Trajectories ({{ batchTrajectories.length }})
                </h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadResults(batchTrajectories, 'degradation-trajectories')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download All
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Average Time to Failure</p>
                    <p class="text-xl font-bold">
                      {{ Math.round(batchTrajectories.reduce((sum: number, t: any) => {
                        const parts = t.timeToFailure.split(':');
                        return sum + (parseInt(parts[0]) * 3600 + parseInt(parts[1]) * 60 + parseInt(parts[2]));
                      }, 0) / batchTrajectories.length) }}s
                    </p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Fastest Failure</p>
                    <p class="text-xl font-bold">
                      {{ Math.min(...batchTrajectories.map((t: any) => {
                        const parts = t.timeToFailure.split(':');
                        return parseInt(parts[0]) * 3600 + parseInt(parts[1]) * 60 + parseInt(parts[2]);
                      })) }}s
                    </p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Slowest Failure</p>
                    <p class="text-xl font-bold">
                      {{ Math.max(...batchTrajectories.map((t: any) => {
                        const parts = t.timeToFailure.split(':');
                        return parseInt(parts[0]) * 3600 + parseInt(parts[1]) * 60 + parseInt(parts[2]);
                      })) }}s
                    </p>
                  </div>
                </BaseCard>
              </div>
              
              <div ref="trajectoriesChartRef" class="w-full h-80"></div>
              
              <!-- Individual Trajectory List -->
              <div class="mt-6">
                <h4 class="font-medium mb-3">Individual Trajectories</h4>
                <div class="space-y-2 max-h-60 overflow-y-auto">
                  <BaseCard
                    v-for="(trajectory, index) in batchTrajectories"
                    :key="trajectory.trajectoryId"
                    class="hover:shadow-md transition-shadow"
                  >
                    <div class="p-3 flex justify-between items-center">
                      <div>
                        <span class="font-medium">Trajectory {{ (index as number) + 1 }}</span>
                        <span class="text-sm text-gray-600 ml-2">
                          ID: {{ trajectory.trajectoryId.substring(0, 8) }}
                        </span>
                      </div>
                      <div class="text-sm">
                        Time to failure: {{ trajectory.timeToFailure }}
                      </div>
                    </div>
                  </BaseCard>
                </div>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.run-to-failure {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .run-to-failure {
    padding: 0.5rem;
  }
}
</style>