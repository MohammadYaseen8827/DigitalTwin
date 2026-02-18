<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  generateSyntheticData,
  validateSyntheticData,
  getGenerationStatistics,
  quickGenerate,
  type SyntheticDataGenerationResult,
  type GenerationStatistics,
  type GenerateDataRequest,
  type ValidateDataRequest
} from '@/services/syntheticData.service'
import { 
  Database, 
  Zap, 
  BarChart3, 
  CheckCircle,
  AlertTriangle,
  Play,
  Download,
  Upload,
  Settings,
  RefreshCw
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
  TitleComponent
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
  CanvasRenderer
])

const toast = useToast()

// State
const generationResult: any = ref<SyntheticDataGenerationResult | null>(null)
const statistics: any = ref<GenerationStatistics | null>(null)
const generating = ref(false)
const validating = ref(false)
const machineType = ref('CNC')
const numberOfTrajectories = ref(10)
const timeRange = ref('P1D')
const randomSeed = ref<string | number>('')

// Chart refs
const trajectoryChartRef = ref<HTMLDivElement | null>(null)
const qualityChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let trajectoryChartInstance: echarts.ECharts | null = null
let qualityChartInstance: echarts.ECharts | null = null

// Options
const machineTypes = [
  { label: 'CNC Machine', value: 'CNC' },
  { label: 'Injection Molder', value: 'InjectionMolder' },
  { label: 'Press Machine', value: 'Press' },
  { label: 'Robot Arm', value: 'Robot' },
  { label: 'Conveyor System', value: 'Conveyor' }
]

const timeRanges = [
  { label: '1 Day', value: 'P1D' },
  { label: '1 Week', value: 'P7D' },
  { label: '1 Month', value: 'P30D' },
  { label: '3 Months', value: 'P90D' }
]

// Computed
const dataQualityPercentage = computed(() => {
  return statistics.value ? Math.round(statistics.value.dataQualityScore * 100) : 0
})

const generationStats = computed(() => {
  if (!statistics.value) return null
  
  return {
    trajectories: statistics.value.totalTrajectories,
    dataPoints: statistics.value.dataPointsGenerated.toLocaleString(),
    sensors: statistics.value.sensorsCovered.length,
    avgLength: Math.round(statistics.value.averageTrajectoryLength),
    quality: dataQualityPercentage.value,
    time: `${statistics.value.generationTimeMs}ms`
  }
})

// Methods
const generateData = async () => {
  try {
    generating.value = true
    
    const seedValue = randomSeed.value === '' ? undefined : Number(randomSeed.value)
    
    const request: GenerateDataRequest = {
      machineType: machineType.value,
      numberOfTrajectories: numberOfTrajectories.value,
      timeRange: timeRange.value,
      randomSeed: seedValue
    }
    
    generationResult.value = await generateSyntheticData(request)
    statistics.value = generationResult.value.statistics
    
    renderCharts()
    
    toast.success(`Generated ${generationResult.value.trajectories.length} synthetic trajectories`)
  } catch (error) {
    console.error('Failed to generate synthetic data:', error)
    toast.error('Failed to generate synthetic data')
  } finally {
    generating.value = false
  }
}

const quickGenerateData = async (type: string) => {
  try {
    generating.value = true
    machineType.value = type
    
    generationResult.value = await quickGenerate(type)
    statistics.value = generationResult.value.statistics
    
    renderCharts()
    
    toast.success(`Quick generated data for ${type}`)
  } catch (error) {
    console.error('Failed to quick generate:', error)
    toast.error('Failed to quick generate data')
  } finally {
    generating.value = false
  }
}

const validateData = async () => {
  if (!generationResult.value) {
    toast.warning('Please generate data first')
    return
  }
  
  try {
    validating.value = true
    
    // Flatten trajectory data for validation
    const telemetryData: any[] = []
    generationResult.value.trajectories.forEach((trajectory: any) => {
      trajectory.sensorData.forEach((sensor: any) => {
        sensor.values.forEach((value: number, index: number) => {
          telemetryData.push({
            sensorId: sensor.sensorId,
            value: value,
            timestamp: trajectory.timestamps[index],
            machineType: trajectory.machineType
          })
        })
      })
    })
    
    const request: ValidateDataRequest = {
      telemetryData,
      benchmarkDataset: 'default'
    }
    
    const validationReport = await validateSyntheticData(request)
    
    if (validationReport.isValid) {
      toast.success('Data validation passed!')
    } else {
      toast.warning(`Data validation found ${validationReport.deviations.length} issues`)
    }
    
    // Update result with validation report
    generationResult.value.validationReport = validationReport
    
  } catch (error) {
    console.error('Failed to validate data:', error)
    toast.error('Failed to validate synthetic data')
  } finally {
    validating.value = false
  }
}

const downloadData = () => {
  if (!generationResult.value) {
    toast.warning('No data to download')
    return
  }
  
  const dataStr = JSON.stringify(generationResult.value, null, 2)
  const blob = new Blob([dataStr], { type: 'application/json' })
  const url = URL.createObjectURL(blob)
  
  const a = document.createElement('a')
  a.href = url
  a.download = `synthetic-data-${machineType.value}-${new Date().toISOString().split('T')[0]}.json`
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
  
  toast.success('Data downloaded successfully')
}

const renderCharts = () => {
  renderTrajectoryChart()
  renderQualityChart()
}

const renderTrajectoryChart = () => {
  if (!trajectoryChartRef.value || !generationResult.value) return
  
  if (!trajectoryChartInstance) {
    trajectoryChartInstance = echarts.init(trajectoryChartRef.value)
  }
  
  // Sample data for visualization (first few trajectories)
  const sampleTrajectories = generationResult.value.trajectories.slice(0, 3)
  const series = sampleTrajectories.map((traj: any, index: number) => {
    // Take first sensor data for visualization
    const sensorData = traj.sensorData[0] || { values: [], timestamps: [] }
    
    return {
      name: `Trajectory ${index + 1}`,
      type: 'line',
      data: sensorData.values.slice(0, 50).map((value: number, i: number) => [i, value]),
      smooth: true
    }
  })
  
  const option = {
    title: {
      text: 'Sample Trajectory Data',
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
      name: 'Time Points'
    },
    yAxis: {
      type: 'value',
      name: 'Sensor Values'
    },
    series: series
  }
  
  trajectoryChartInstance.setOption(option, true)
}

const renderQualityChart = () => {
  if (!qualityChartRef.value || !generationResult.value?.validationReport) return
  
  if (!qualityChartInstance) {
    qualityChartInstance = echarts.init(qualityChartRef.value)
  }
  
  const metrics = generationResult.value.validationReport.qualityMetrics
  
  const option = {
    title: {
      text: 'Data Quality Metrics',
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      data: metrics.map((metric: any) => ({
        name: metric.name,
        value: metric.passed ? 100 : Math.max(0, metric.value * 100),
        itemStyle: {
          color: metric.passed ? '#10b981' : '#ef4444'
        }
      })),
      label: {
        show: true,
        formatter: '{b}: {c}%'
      }
    }]
  }
  
  qualityChartInstance.setOption(option, true)
}

const resizeCharts = () => {
  trajectoryChartInstance?.resize()
  qualityChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  trajectoryChartInstance?.dispose()
  qualityChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="synthetic-data space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Synthetic Data Generator</h1>
          <p class="text-gray-600 mt-2">Generate and validate synthetic telemetry data for testing and simulation</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="downloadData" 
            :disabled="!generationResult"
          >
            <Download class="w-4 h-4 mr-2" />
            Download Data
          </BaseButton>
        </div>
      </div>

      <!-- Quick Generate Buttons -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Quick Generate</h2>
          <div class="grid grid-cols-2 md:grid-cols-5 gap-3">
            <BaseButton
              v-for="type in machineTypes"
              :key="type.value"
              variant="outline"
              @click="quickGenerateData(type.value)"
              :disabled="generating"
              class="h-16 flex flex-col items-center justify-center"
            >
              <Zap class="w-5 h-5 mb-1" />
              <span class="text-sm">{{ type.label.split(' ')[0] }}</span>
            </BaseButton>
          </div>
        </div>
      </BaseCard>

      <!-- Generation Controls -->
      <BaseCard>
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Custom Generation</h2>
          
          <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            <BaseSelect
              v-model="machineType"
              :options="machineTypes"
              label="Machine Type"
            />
            
            <BaseInput
              v-model.number="numberOfTrajectories"
              label="Number of Trajectories"
              type="number"
              min="1"
              max="100"
            />
            
            <BaseSelect
              v-model="timeRange"
              :options="timeRanges"
              label="Time Range"
            />
            
            <BaseInput
              v-model="randomSeed"
              label="Random Seed (optional)"
              type="number"
              placeholder="Leave blank for random"
            />
          </div>
          
          <div class="flex gap-3">
            <BaseButton 
              variant="primary" 
              @click="generateData"
              :disabled="generating"
              class="flex-1"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': generating }" />
              {{ generating ? 'Generating...' : 'Generate Data' }}
            </BaseButton>
            
            <BaseButton 
              variant="outline" 
              @click="validateData"
              :disabled="validating || !generationResult"
            >
              <CheckCircle class="w-4 h-4 mr-2" :class="{ 'animate-spin': validating }" />
              {{ validating ? 'Validating...' : 'Validate Data' }}
            </BaseButton>
          </div>
        </div>
      </BaseCard>

      <!-- Statistics -->
      <div v-if="statistics" class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-6 gap-4">
        <BaseCard>
          <div class="p-4 text-center">
            <Database class="w-8 h-8 text-blue-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.trajectories }}</p>
            <p class="text-sm text-gray-600">Trajectories</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <BarChart3 class="w-8 h-8 text-green-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.dataPoints }}</p>
            <p class="text-sm text-gray-600">Data Points</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Settings class="w-8 h-8 text-purple-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.sensors }}</p>
            <p class="text-sm text-gray-600">Sensors</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <RefreshCw class="w-8 h-8 text-yellow-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.avgLength }}</p>
            <p class="text-sm text-gray-600">Avg Length</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <CheckCircle class="w-8 h-8 text-teal-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.quality }}%</p>
            <p class="text-sm text-gray-600">Quality</p>
          </div>
        </BaseCard>
        
        <BaseCard>
          <div class="p-4 text-center">
            <Zap class="w-8 h-8 text-indigo-500 mx-auto mb-2" />
            <p class="text-2xl font-bold">{{ generationStats?.time }}</p>
            <p class="text-sm text-gray-600">Gen Time</p>
          </div>
        </BaseCard>
      </div>

      <!-- Charts Section -->
      <div v-if="generationResult" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <!-- Trajectory Chart -->
        <BaseCard>
          <div class="p-6">
            <div ref="trajectoryChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
        
        <!-- Quality Chart -->
        <BaseCard v-if="generationResult.validationReport">
          <div class="p-6">
            <div ref="qualityChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>

      <!-- Validation Results -->
      <BaseCard v-if="generationResult?.validationReport">
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-4">Validation Results</h2>
          
          <div class="mb-6">
            <div class="flex items-center gap-2 mb-2">
              <CheckCircle v-if="generationResult.validationReport.isValid" class="w-5 h-5 text-green-500" />
              <AlertTriangle v-else class="w-5 h-5 text-red-500" />
              <span class="font-medium">
                {{ generationResult.validationReport.isValid ? 'Validation Passed' : 'Validation Issues Found' }}
              </span>
            </div>
            
            <div class="text-sm text-gray-600">
              {{ generationResult.validationReport.qualityMetrics.filter((m: any) => m.passed).length }}/{{ generationResult.validationReport.qualityMetrics.length }} quality metrics passed
            </div>
          </div>
          
          <!-- Quality Metrics -->
          <div class="mb-6">
            <h3 class="font-medium mb-3">Quality Metrics</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div
                v-for="metric in generationResult.validationReport.qualityMetrics"
                :key="metric.name"
                class="p-3 rounded-lg border"
                :class="metric.passed ? 'border-green-200 bg-green-50' : 'border-red-200 bg-red-50'"
              >
                <div class="flex justify-between items-center">
                  <span class="font-medium">{{ metric.name }}</span>
                  <span class="text-sm font-mono">{{ (metric.value * 100).toFixed(1) }}%</span>
                </div>
                <div class="w-full bg-gray-200 rounded-full h-2 mt-2">
                  <div
                    class="h-2 rounded-full"
                    :class="metric.passed ? 'bg-green-500' : 'bg-red-500'"
                    :style="{ width: `${metric.value * 100}%` }"
                  ></div>
                </div>
                <div class="text-xs text-gray-600 mt-1">
                  Threshold: {{ (metric.threshold * 100).toFixed(0) }}%
                </div>
              </div>
            </div>
          </div>
          
          <!-- Deviations (if any) -->
          <div v-if="generationResult.validationReport.deviations.length > 0">
            <h3 class="font-medium mb-3">Deviations</h3>
            <div class="space-y-2">
              <div
                v-for="(deviation, index) in generationResult.validationReport.deviations"
                :key="index"
                class="p-3 rounded-lg border border-orange-200 bg-orange-50"
              >
                <div class="flex justify-between items-start">
                  <div>
                    <span class="font-medium text-orange-800">{{ deviation.field }}</span>
                    <div class="text-sm text-orange-700 mt-1">
                      Expected: {{ JSON.stringify(deviation.expectedValue) }}<br>
                      Actual: {{ JSON.stringify(deviation.actualValue) }}
                    </div>
                  </div>
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium capitalize"
                    :class="{
                      'bg-red-100 text-red-800': deviation.severity === 'high',
                      'bg-yellow-100 text-yellow-800': deviation.severity === 'medium',
                      'bg-blue-100 text-blue-800': deviation.severity === 'low'
                    }"
                  >
                    {{ deviation.severity }}
                  </span>
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
.synthetic-data {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .synthetic-data {
    padding: 0.5rem;
  }
}
</style>