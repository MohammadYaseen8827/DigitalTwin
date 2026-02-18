<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import { useToast } from '@/composables/useToast'
import { 
  monteCarloAnalysis,
  bayesianAnalysis,
  bootstrapIntervals,
  modelUncertainty,
  comprehensiveReport,
  quickMonteCarlo,
  quickModelUncertainty,
  type UncertaintyAnalysisResult,
  type ConfidenceInterval,
  type ModelUncertaintyResult,
  type ComprehensiveUncertaintyReport,
  type MonteCarloRequest,
  type BayesianRequest,
  type ModelUncertaintyRequest
} from '@/services/uncertaintyQuantification.service'
import { 
  BarChart3, 
  TrendingUp, 
  Settings,
  Play,
  Download,
  Calculator,
  Shield,
  AlertTriangle,
  CheckCircle
} from 'lucide-vue-next'

// Import ECharts
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
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
  EChartsBar,
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
const activeTab = ref('monte-carlo')
const analyzing = ref(false)
const machineId = ref('00000000-0000-0000-0000-000000000001')

// Monte Carlo State
const mcIterations = ref(1000)
const mcNoiseLevel = ref(0.1)
const mcBaseFeatures = ref('temperature:75,vibration:0.5,pressure:100,rpm:1200')
const monteCarloResult: any = ref<UncertaintyAnalysisResult | null>(null)

// Bayesian State
const bayesianSamples = ref(2000)
const bayesianObservedData = ref('75,76,74,77,75,76,74,78,75,77')
const bayesianResult: any = ref<UncertaintyAnalysisResult | null>(null)

// Bootstrap State
const bootstrapSamples = ref(1000)
const bootstrapConfidenceLevel = ref(0.95)
const bootstrapResult: any = ref<ConfidenceInterval | null>(null)

// Model Uncertainty State
const muFeatures = ref('temperature:75,vibration:0.5,pressure:100,rpm:1200')
const modelUncertaintyResult: any = ref<ModelUncertaintyResult | null>(null)

// Comprehensive Report State
const comprehensiveResult: any = ref<ComprehensiveUncertaintyReport | null>(null)

// Chart refs
const uncertaintyChartRef = ref<HTMLDivElement | null>(null)
const confidenceChartRef = ref<HTMLDivElement | null>(null)
const componentsChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let uncertaintyChartInstance: echarts.ECharts | null = null
let confidenceChartInstance: echarts.ECharts | null = null
let componentsChartInstance: echarts.ECharts | null = null

// Computed
const parsedMcFeatures = computed(() => {
  const features: Record<string, number> = {}
  mcBaseFeatures.value.split(',').forEach(pair => {
    const [key, value] = pair.split(':')
    if (key && value) {
      features[key.trim()] = parseFloat(value.trim())
    }
  })
  return features
})

const parsedMuFeatures = computed(() => {
  const features: Record<string, number> = {}
  muFeatures.value.split(',').forEach(pair => {
    const [key, value] = pair.split(':')
    if (key && value) {
      features[key.trim()] = parseFloat(value.trim())
    }
  })
  return features
})

const parsedBayesianData = computed(() => {
  return bayesianObservedData.value
    .split(',')
    .map(val => parseFloat(val.trim()))
    .filter(val => !isNaN(val))
})

const uncertaintyRiskLevel = computed(() => {
  if (!modelUncertaintyResult.value) return 'Unknown'
  
  const totalUncertainty = modelUncertaintyResult.value.totalUncertainty
  if (totalUncertainty < 0.1) return 'Low'
  if (totalUncertainty < 0.3) return 'Medium'
  return 'High'
})

const uncertaintyRiskColor = computed(() => {
  const level = uncertaintyRiskLevel.value
  if (level === 'Low') return 'text-green-600'
  if (level === 'Medium') return 'text-yellow-600'
  return 'text-red-600'
})

// Methods
const performMonteCarlo = async () => {
  try {
    analyzing.value = true
    
    const request: MonteCarloRequest = {
      baseFeatures: parsedMcFeatures.value,
      iterations: mcIterations.value,
      noiseLevel: mcNoiseLevel.value
    }
    
    monteCarloResult.value = await monteCarloAnalysis(machineId.value, request)
    renderUncertaintyChart()
    
    toast.success('Monte Carlo analysis completed')
  } catch (error) {
    console.error('Failed to perform Monte Carlo analysis:', error)
    toast.error('Failed to perform Monte Carlo analysis')
  } finally {
    analyzing.value = false
  }
}

const performBayesian = async () => {
  if (parsedBayesianData.value.length === 0) {
    toast.error('Please enter valid observed data')
    return
  }
  
  try {
    analyzing.value = true
    
    const request: BayesianRequest = {
      observedData: parsedBayesianData.value,
      priorDistributions: {
        temperature: { distributionType: 'normal', parameters: { mean: 75, std: 5 } },
        vibration: { distributionType: 'normal', parameters: { mean: 0.5, std: 0.1 } }
      },
      samples: bayesianSamples.value
    }
    
    bayesianResult.value = await bayesianAnalysis(machineId.value, request)
    renderUncertaintyChart()
    
    toast.success('Bayesian analysis completed')
  } catch (error) {
    console.error('Failed to perform Bayesian analysis:', error)
    toast.error('Failed to perform Bayesian analysis')
  } finally {
    analyzing.value = false
  }
}

const calculateBootstrap = async () => {
  try {
    analyzing.value = true
    
    bootstrapResult.value = await bootstrapIntervals(
      machineId.value,
      bootstrapSamples.value,
      bootstrapConfidenceLevel.value
    )
    
    renderConfidenceChart()
    
    toast.success('Bootstrap intervals calculated')
  } catch (error) {
    console.error('Failed to calculate bootstrap intervals:', error)
    toast.error('Failed to calculate bootstrap intervals')
  } finally {
    analyzing.value = false
  }
}

const quantifyModelUncertainty = async () => {
  try {
    analyzing.value = true
    
    const request: ModelUncertaintyRequest = {
      features: parsedMuFeatures.value
    }
    
    modelUncertaintyResult.value = await modelUncertainty(machineId.value, request)
    renderComponentsChart()
    
    toast.success('Model uncertainty quantified')
  } catch (error) {
    console.error('Failed to quantify model uncertainty:', error)
    toast.error('Failed to quantify model uncertainty')
  } finally {
    analyzing.value = false
  }
}

const generateComprehensiveReport = async () => {
  try {
    analyzing.value = true
    
    comprehensiveResult.value = await comprehensiveReport(machineId.value)
    
    toast.success('Comprehensive uncertainty report generated')
  } catch (error) {
    console.error('Failed to generate comprehensive report:', error)
    toast.error('Failed to generate comprehensive report')
  } finally {
    analyzing.value = false
  }
}

const quickAnalyze = async () => {
  try {
    analyzing.value = true
    
    const [mcResult, muResult] = await Promise.all([
      quickMonteCarlo(machineId.value),
      quickModelUncertainty(machineId.value)
    ])
    
    monteCarloResult.value = mcResult
    modelUncertaintyResult.value = muResult
    
    renderUncertaintyChart()
    renderComponentsChart()
    
    toast.success('Quick uncertainty analysis completed')
  } catch (error) {
    console.error('Failed to perform quick analysis:', error)
    toast.error('Failed to perform quick analysis')
  } finally {
    analyzing.value = false
  }
}

const renderUncertaintyChart = () => {
  if (!uncertaintyChartRef.value) return
  
  if (!uncertaintyChartInstance) {
    uncertaintyChartInstance = echarts.init(uncertaintyChartRef.value)
  }
  
  const data = []
  if (monteCarloResult.value) {
    data.push({
      name: 'Monte Carlo',
      mean: monteCarloResult.value.meanPrediction,
      std: monteCarloResult.value.standardDeviation,
      lower: monteCarloResult.value.confidenceInterval.lowerBound,
      upper: monteCarloResult.value.confidenceInterval.upperBound
    })
  }
  
  if (bayesianResult.value) {
    data.push({
      name: 'Bayesian',
      mean: bayesianResult.value.meanPrediction,
      std: bayesianResult.value.standardDeviation,
      lower: bayesianResult.value.confidenceInterval.lowerBound,
      upper: bayesianResult.value.confidenceInterval.upperBound
    })
  }
  
  if (data.length === 0) return
  
  const option = {
    title: {
      text: 'Uncertainty Analysis Comparison',
      left: 'center'
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      }
    },
    legend: {
      top: 30
    },
    xAxis: {
      type: 'category',
      data: data.map(item => item.name)
    },
    yAxis: {
      type: 'value',
      name: 'Prediction Value'
    },
    series: [
      {
        name: 'Mean Prediction',
        type: 'bar',
        data: data.map(item => item.mean),
        itemStyle: { color: '#3b82f6' }
      },
      {
        name: 'Standard Deviation',
        type: 'bar',
        data: data.map(item => item.std),
        itemStyle: { color: '#ef4444' }
      }
    ]
  }
  
  uncertaintyChartInstance.setOption(option, true)
}

const renderConfidenceChart = () => {
  if (!confidenceChartRef.value || !bootstrapResult.value) return
  
  if (!confidenceChartInstance) {
    confidenceChartInstance = echarts.init(confidenceChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Bootstrap Confidence Intervals',
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'bar',
      data: [
        { name: 'Lower Bound', value: bootstrapResult.value.lowerBound },
        { name: 'Upper Bound', value: bootstrapResult.value.upperBound }
      ],
      itemStyle: {
        color: (params: any) => params.dataIndex === 0 ? '#10b981' : '#ef4444'
      }
    }],
    xAxis: {
      type: 'category'
    },
    yAxis: {
      type: 'value',
      name: 'Prediction Value'
    }
  }
  
  confidenceChartInstance.setOption(option, true)
}

const renderComponentsChart = () => {
  if (!componentsChartRef.value || !modelUncertaintyResult.value) return
  
  if (!componentsChartInstance) {
    componentsChartInstance = echarts.init(componentsChartRef.value)
  }
  
  const option = {
    title: {
      text: 'Uncertainty Components Breakdown',
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'pie',
      radius: ['40%', '70%'],
      data: [
        {
          name: 'Aleatoric (Data Noise)',
          value: modelUncertaintyResult.value.aleatoricUncertainty * 100
        },
        {
          name: 'Epistemic (Model Variance)',
          value: modelUncertaintyResult.value.epistemicUncertainty * 100
        }
      ],
      label: {
        show: true,
        formatter: '{b}: {c}%'
      }
    }]
  }
  
  componentsChartInstance.setOption(option, true)
}

const downloadReport = (data: any, filename: string) => {
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
  
  toast.success('Report downloaded successfully')
}

const resizeCharts = () => {
  uncertaintyChartInstance?.resize()
  confidenceChartInstance?.resize()
  componentsChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  uncertaintyChartInstance?.dispose()
  confidenceChartInstance?.dispose()
  componentsChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="uncertainty-quantification space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Uncertainty Quantification</h1>
          <p class="text-gray-600 mt-2">Analyze and quantify prediction uncertainties using multiple methods</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="quickAnalyze" 
            :disabled="analyzing"
          >
            <Play class="w-4 h-4 mr-2" />
            Quick Analyze
          </BaseButton>
          <BaseButton 
            variant="outline" 
            @click="generateComprehensiveReport" 
            :disabled="analyzing"
          >
            <Calculator class="w-4 h-4 mr-2" />
            Full Report
          </BaseButton>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in [
              { id: 'monte-carlo', name: 'Monte Carlo', icon: BarChart3 },
              { id: 'bayesian', name: 'Bayesian', icon: TrendingUp },
              { id: 'bootstrap', name: 'Bootstrap', icon: Settings },
              { id: 'model', name: 'Model Uncertainty', icon: Shield }
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

      <!-- Monte Carlo Tab -->
      <div v-show="activeTab === 'monte-carlo'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Monte Carlo Uncertainty Analysis</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseInput
                v-model.number="mcIterations"
                label="Iterations"
                type="number"
                min="100"
                max="10000"
              />
              
              <BaseInput
                v-model.number="mcNoiseLevel"
                label="Noise Level"
                type="number"
                step="0.01"
                min="0.01"
                max="1"
              />
              
              <BaseInput
                v-model="machineId"
                label="Machine ID"
                placeholder="Enter machine UUID"
              />
            </div>
            
            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Base Features (key:value pairs, comma separated)
              </label>
              <BaseInput
                v-model="mcBaseFeatures"
                placeholder="temperature:75,vibration:0.5,pressure:100,rpm:1200"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="performMonteCarlo"
              :disabled="analyzing"
              class="w-full md:w-auto"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': analyzing }" />
              {{ analyzing ? 'Analyzing...' : 'Run Monte Carlo' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="monteCarloResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Monte Carlo Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadReport(monteCarloResult, 'monte-carlo-results')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Mean Prediction</p>
                    <p class="text-2xl font-bold">{{ monteCarloResult.meanPrediction.toFixed(2) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Std Deviation</p>
                    <p class="text-2xl font-bold">{{ monteCarloResult.standardDeviation.toFixed(3) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Samples</p>
                    <p class="text-2xl font-bold">{{ monteCarloResult.sampleCount }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Confidence</p>
                    <p class="text-2xl font-bold">{{ (monteCarloResult.confidenceInterval.confidenceLevel * 100).toFixed(0) }}%</p>
                  </div>
                </BaseCard>
              </div>
              
              <div class="bg-gray-50 p-4 rounded-lg">
                <h4 class="font-medium mb-2">Confidence Interval</h4>
                <p class="text-sm">
                  [{{ monteCarloResult.confidenceInterval.lowerBound.toFixed(2) }}, 
                  {{ monteCarloResult.confidenceInterval.upperBound.toFixed(2) }}]
                </p>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Bayesian Tab -->
      <div v-show="activeTab === 'bayesian'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Bayesian Uncertainty Analysis</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseInput
                v-model.number="bayesianSamples"
                label="Samples"
                type="number"
                min="100"
                max="10000"
              />
              
              <BaseInput
                v-model="machineId"
                label="Machine ID"
                placeholder="Enter machine UUID"
              />
            </div>
            
            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Observed Data (comma separated values)
              </label>
              <BaseInput
                v-model="bayesianObservedData"
                placeholder="75,76,74,77,75,76,74,78,75,77"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="performBayesian"
              :disabled="analyzing"
              class="w-full md:w-auto"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': analyzing }" />
              {{ analyzing ? 'Analyzing...' : 'Run Bayesian Analysis' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="bayesianResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Bayesian Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadReport(bayesianResult, 'bayesian-results')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Mean Prediction</p>
                    <p class="text-2xl font-bold">{{ bayesianResult.meanPrediction.toFixed(2) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Std Deviation</p>
                    <p class="text-2xl font-bold">{{ bayesianResult.standardDeviation.toFixed(3) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Samples</p>
                    <p class="text-2xl font-bold">{{ bayesianResult.sampleCount }}</p>
                  </div>
                </BaseCard>
              </div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Bootstrap Tab -->
      <div v-show="activeTab === 'bootstrap'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Bootstrap Confidence Intervals</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseInput
                v-model.number="bootstrapSamples"
                label="Samples"
                type="number"
                min="100"
                max="10000"
              />
              
              <BaseInput
                v-model.number="bootstrapConfidenceLevel"
                label="Confidence Level"
                type="number"
                step="0.01"
                min="0.8"
                max="0.99"
              />
              
              <BaseInput
                v-model="machineId"
                label="Machine ID"
                placeholder="Enter machine UUID"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="calculateBootstrap"
              :disabled="analyzing"
              class="w-full md:w-auto"
            >
              <Calculator class="w-4 h-4 mr-2" :class="{ 'animate-spin': analyzing }" />
              {{ analyzing ? 'Calculating...' : 'Calculate Intervals' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="bootstrapResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Bootstrap Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadReport(bootstrapResult, 'bootstrap-results')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="bg-gray-50 p-4 rounded-lg">
                <h4 class="font-medium mb-2">Confidence Interval</h4>
                <p class="text-sm">
                  [{{ bootstrapResult.lowerBound.toFixed(2) }}, 
                  {{ bootstrapResult.upperBound.toFixed(2) }}]
                </p>
                <p class="text-xs text-gray-600 mt-1">
                  Confidence Level: {{ (bootstrapResult.confidenceLevel * 100).toFixed(0) }}%
                </p>
              </div>
              
              <div ref="confidenceChartRef" class="w-full h-80 mt-6"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Model Uncertainty Tab -->
      <div v-show="activeTab === 'model'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Model Uncertainty Quantification</h2>
            
            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Features (key:value pairs, comma separated)
              </label>
              <BaseInput
                v-model="muFeatures"
                placeholder="temperature:75,vibration:0.5,pressure:100,rpm:1200"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="quantifyModelUncertainty"
              :disabled="analyzing"
              class="w-full md:w-auto"
            >
              <Shield class="w-4 h-4 mr-2" :class="{ 'animate-spin': analyzing }" />
              {{ analyzing ? 'Quantifying...' : 'Quantify Uncertainty' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="modelUncertaintyResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Model Uncertainty Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadReport(modelUncertaintyResult, 'model-uncertainty-results')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Risk Level</p>
                    <p class="text-2xl font-bold" :class="uncertaintyRiskColor">
                      {{ uncertaintyRiskLevel }}
                    </p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Total Uncertainty</p>
                    <p class="text-2xl font-bold">{{ (modelUncertaintyResult.totalUncertainty * 100).toFixed(1) }}%</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4 text-center">
                    <p class="text-sm text-gray-600">Model Confidence</p>
                    <p class="text-2xl font-bold">{{ (modelUncertaintyResult.modelConfidence * 100).toFixed(1) }}%</p>
                  </div>
                </BaseCard>
              </div>
              
              <div ref="componentsChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Charts Section -->
      <div v-if="monteCarloResult || bayesianResult" class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <BaseCard>
          <div class="p-6">
            <div ref="uncertaintyChartRef" class="w-full h-80"></div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.uncertainty-quantification {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .uncertainty-quantification {
    padding: 0.5rem;
  }
}
</style>