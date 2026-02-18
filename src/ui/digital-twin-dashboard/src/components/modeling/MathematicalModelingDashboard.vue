<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseTextarea from '@/components/base/BaseInput.vue'
import { useToast } from '@/composables/useToast'
import { 
  solveOde,
  solveSystemDynamics,
  gradientOptimization,
  geneticOptimization,
  quickSolveOde,
  quickOptimize,
  type OdeSolution,
  type SystemDynamicsSolution,
  type OptimizationResult,
  type OdeSolveRequest,
  type SystemDynamicsRequest,
  type GradientOptimizationRequest,
  type GeneticOptimizationRequest
} from '@/services/mathematicalModeling.service'
import { 
  Calculator, 
  BarChart3, 
  TrendingUp, 
  Settings,
  Play,
  Download,
  FunctionSquare,
  Zap,
  Target
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
const activeTab = ref('ode')
const solving = ref(false)
const optimizing = ref(false)

// ODE State
const odeSystemName = ref('Simple Harmonic Oscillator')
const odeEquations = ref(['y1\' = y2', 'y2\' = -y1'])
const odeInitialConditions = ref('0, 1')
const odeStartTime = ref(0)
const odeEndTime = ref(10)
const odeStepSize = ref(0.01)
const odeSolution: any = ref<OdeSolution | null>(null)

// System Dynamics State
const sysSystemName = ref('Double Pendulum')
const sysEquations = ref(['theta1\' = omega1', 'theta2\' = omega2', 'omega1\' = -sin(theta1)', 'omega2\' = -sin(theta2)'])
const sysInitialConditions = ref('0.1, 0.2, 0, 0')
const sysStepSize = ref(0.01)
const sysSimulationTime = ref(20)
const sysSolution: any = ref<SystemDynamicsSolution | null>(null)

// Optimization State
const optObjectiveExpression = ref('(x - 3)^2 + (y - 2)^2')
const optInitialGuess = ref('1, 1')
const optLearningRate = ref(0.01)
const optMaxIterations = ref(1000)
const optTolerance = ref(1e-6)
const optMethod = ref('gradient')
const optPopulationSize = ref(50)
const optMaxGenerations = ref(100)
const optMutationRate = ref(0.1)
const optResult: any = ref<OptimizationResult | null>(null)

// Chart refs
const odeChartRef = ref<HTMLDivElement | null>(null)
const sysChartRef = ref<HTMLDivElement | null>(null)
const optChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let odeChartInstance: echarts.ECharts | null = null
let sysChartInstance: echarts.ECharts | null = null
let optChartInstance: echarts.ECharts | null = null

// Computed
const parsedOdeInitialConditions = computed(() => {
  return odeInitialConditions.value.split(',').map(val => parseFloat(val.trim())).filter(val => !isNaN(val))
})

const parsedSysInitialConditions = computed(() => {
  return sysInitialConditions.value.split(',').map(val => parseFloat(val.trim())).filter(val => !isNaN(val))
})

const parsedOptInitialGuess = computed(() => {
  return optInitialGuess.value.split(',').map(val => parseFloat(val.trim())).filter(val => !isNaN(val))
})

// Methods
const solveODE = async () => {
  if (parsedOdeInitialConditions.value.length === 0) {
    toast.error('Please enter valid initial conditions')
    return
  }
  
  try {
    solving.value = true
    
    const request: OdeSolveRequest = {
      systemName: odeSystemName.value,
      equations: odeEquations.value,
      initialConditions: parsedOdeInitialConditions.value,
      startTime: odeStartTime.value,
      endTime: odeEndTime.value,
      stepSize: odeStepSize.value
    }
    
    odeSolution.value = await solveOde(request)
    renderOdeChart()
    
    toast.success(`Solved ODE system: ${odeSystemName.value}`)
  } catch (error) {
    console.error('Failed to solve ODE:', error)
    toast.error('Failed to solve ODE system')
  } finally {
    solving.value = false
  }
}

const quickSolve = async () => {
  try {
    solving.value = true
    
    const equations = ['y\' = -2*y + sin(t)', 'y(0) = 1'] // Simple decay equation
    odeSolution.value = await quickSolveOde('Decay Model', equations)
    renderOdeChart()
    
    toast.success('Quick solved sample ODE system')
  } catch (error) {
    console.error('Failed to quick solve:', error)
    toast.error('Failed to quick solve ODE')
  } finally {
    solving.value = false
  }
}

const solveSystem = async () => {
  if (parsedSysInitialConditions.value.length === 0) {
    toast.error('Please enter valid initial conditions')
    return
  }
  
  try {
    solving.value = true
    
    const request: SystemDynamicsRequest = {
      systemName: sysSystemName.value,
      equations: sysEquations.value,
      initialConditions: parsedSysInitialConditions.value,
      stepSize: sysStepSize.value,
      simulationTime: sysSimulationTime.value,
      derivedQuantities: [] // Could add derived quantities here
    }
    
    sysSolution.value = await solveSystemDynamics(request)
    renderSysChart()
    
    toast.success(`Solved system dynamics: ${sysSystemName.value}`)
  } catch (error) {
    console.error('Failed to solve system:', error)
    toast.error('Failed to solve system dynamics')
  } finally {
    solving.value = false
  }
}

const optimize = async () => {
  if (parsedOptInitialGuess.value.length === 0) {
    toast.error('Please enter valid initial guess')
    return
  }
  
  try {
    optimizing.value = true
    
    if (optMethod.value === 'gradient') {
      const request: GradientOptimizationRequest = {
        objectiveExpression: optObjectiveExpression.value,
        initialGuess: parsedOptInitialGuess.value,
        learningRate: optLearningRate.value,
        maxIterations: optMaxIterations.value,
        tolerance: optTolerance.value,
        epsilon: 1e-8
      }
      
      optResult.value = await gradientOptimization(request)
    } else {
      const request: GeneticOptimizationRequest = {
        fitnessExpression: optObjectiveExpression.value,
        parameterCount: parsedOptInitialGuess.value.length,
        populationSize: optPopulationSize.value,
        maxGenerations: optMaxGenerations.value,
        mutationRate: optMutationRate.value,
        tournamentSize: 3,
        parameterBounds: parsedOptInitialGuess.value.map(() => ({ min: -10, max: 10 }))
      }
      
      optResult.value = await geneticOptimization(request)
    }
    
    renderOptChart()
    
    toast.success(`Optimization completed using ${optMethod.value} method`)
  } catch (error) {
    console.error('Failed to optimize:', error)
    toast.error('Failed to perform optimization')
  } finally {
    optimizing.value = false
  }
}

const quickOptimizeFunc = async () => {
  try {
    optimizing.value = true
    
    optResult.value = await quickOptimize('(x - 2)^2 + (y - 3)^2')
    renderOptChart()
    
    toast.success('Quick optimization completed')
  } catch (error) {
    console.error('Failed to quick optimize:', error)
    toast.error('Failed to quick optimize')
  } finally {
    optimizing.value = false
  }
}

const renderOdeChart = () => {
  if (!odeChartRef.value || !odeSolution.value) return
  
  if (!odeChartInstance) {
    odeChartInstance = echarts.init(odeChartRef.value)
  }
  
  const timePoints = odeSolution.value.timePoints
  const solutions = odeSolution.value.solutions
  
  const series = solutions.map((solution: any, index: number) => ({
    name: `y${index + 1}`,
    type: 'line',
    data: solution.map((val: number, i: number) => [timePoints[i], val]),
    smooth: true
  }))
  
  const option = {
    title: {
      text: `ODE Solution: ${odeSolution.value.systemName}`,
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
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'Values'
    },
    series: series
  }
  
  odeChartInstance.setOption(option, true)
}

const renderSysChart = () => {
  if (!sysChartRef.value || !sysSolution.value) return
  
  if (!sysChartInstance) {
    sysChartInstance = echarts.init(sysChartRef.value)
  }
  
  const timePoints = sysSolution.value.timePoints
  const stateVariables = sysSolution.value.stateVariables
  
  const series = stateVariables.map((variable: any, index: number) => ({
    name: `Variable ${index + 1}`,
    type: 'line',
    data: variable.map((val: number, i: number) => [timePoints[i], val]),
    smooth: true
  }))
  
  const option = {
    title: {
      text: `System Dynamics: ${sysSolution.value.systemName}`,
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
      name: 'Time'
    },
    yAxis: {
      type: 'value',
      name: 'State Variables'
    },
    series: series
  }
  
  sysChartInstance.setOption(option, true)
}

const renderOptChart = () => {
  if (!optChartRef.value || !optResult.value) return
  
  if (!optChartInstance) {
    optChartInstance = echarts.init(optChartRef.value)
  }
  
  const option = {
    title: {
      text: `Optimization Result (${optResult.value.method})`,
      left: 'center'
    },
    tooltip: {
      trigger: 'item'
    },
    series: [{
      type: 'scatter',
      data: [[optResult.value.optimalParameters[0] || 0, optResult.value.optimalParameters[1] || 0]],
      symbolSize: 20,
      itemStyle: {
        color: optResult.value.converged ? '#10b981' : '#ef4444'
      }
    }],
    xAxis: {
      type: 'value',
      name: 'Parameter 1'
    },
    yAxis: {
      type: 'value',
      name: 'Parameter 2'
    }
  }
  
  optChartInstance.setOption(option, true)
}

const downloadSolution = (data: any, filename: string) => {
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
  
  toast.success('Solution downloaded successfully')
}

const resizeCharts = () => {
  odeChartInstance?.resize()
  sysChartInstance?.resize()
  optChartInstance?.resize()
}

// Lifecycle
onMounted(() => {
  window.addEventListener('resize', resizeCharts)
})

// Cleanup
const cleanup = () => {
  window.removeEventListener('resize', resizeCharts)
  odeChartInstance?.dispose()
  sysChartInstance?.dispose()
  optChartInstance?.dispose()
}
</script>

<template>
  <BaseCard>
    <div class="mathematical-modeling space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900">Mathematical Modeling</h1>
          <p class="text-gray-600 mt-2">Solve differential equations and perform parameter optimization</p>
        </div>
        <div class="flex gap-2">
          <BaseButton 
            variant="outline" 
            @click="quickSolve" 
            :disabled="solving"
          >
            <Play class="w-4 h-4 mr-2" />
            Quick ODE Solve
          </BaseButton>
          <BaseButton 
            variant="outline" 
            @click="quickOptimizeFunc" 
            :disabled="optimizing"
          >
            <Target class="w-4 h-4 mr-2" />
            Quick Optimize
          </BaseButton>
        </div>
      </div>

      <!-- Tab Navigation -->
      <div class="border-b border-gray-200">
        <nav class="-mb-px flex space-x-8">
          <button
            v-for="tab in [
              { id: 'ode', name: 'ODE Solver', icon: FunctionSquare },
              { id: 'system', name: 'System Dynamics', icon: BarChart3 },
              { id: 'optimization', name: 'Optimization', icon: TrendingUp }
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

      <!-- ODE Solver Tab -->
      <div v-show="activeTab === 'ode'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Ordinary Differential Equation Solver</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              <BaseInput
                v-model="odeSystemName"
                label="System Name"
                placeholder="Enter system name"
              />
              
              <BaseInput
                v-model.number="odeStepSize"
                label="Step Size"
                type="number"
                step="0.001"
                min="0.001"
                max="1"
              />
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseInput
                v-model.number="odeStartTime"
                label="Start Time"
                type="number"
              />
              
              <BaseInput
                v-model.number="odeEndTime"
                label="End Time"
                type="number"
              />
              
              <BaseInput
                v-model="odeInitialConditions"
                label="Initial Conditions (comma separated)"
                placeholder="0, 1, 0"
              />
            </div>
            
            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                Differential Equations (one per line)
              </label>
              <textarea
                :value="odeEquations.join('\n')"
                @input="odeEquations = ($event.target as HTMLTextAreaElement).value.split('\n').filter(eq => eq.trim())"
                rows="4"
                placeholder="y1' = y2\ny2' = -y1"
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="solveODE"
              :disabled="solving"
              class="w-full md:w-auto"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': solving }" />
              {{ solving ? 'Solving...' : 'Solve ODE System' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="odeSolution" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Solution Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadSolution(odeSolution, 'ode-solution')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Steps</p>
                    <p class="text-2xl font-bold">{{ odeSolution.steps }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Final Time</p>
                    <p class="text-2xl font-bold">{{ odeSolution.finalTime.toFixed(2) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Variables</p>
                    <p class="text-2xl font-bold">{{ odeSolution.solutions.length }}</p>
                  </div>
                </BaseCard>
              </div>
              
              <div ref="odeChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- System Dynamics Tab -->
      <div v-show="activeTab === 'system'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">System Dynamics Solver</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              <BaseInput
                v-model="sysSystemName"
                label="System Name"
                placeholder="Enter system name"
              />
              
              <BaseInput
                v-model.number="sysStepSize"
                label="Step Size"
                type="number"
                step="0.001"
                min="0.001"
                max="1"
              />
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              <BaseInput
                v-model.number="sysSimulationTime"
                label="Simulation Time"
                type="number"
                min="1"
                max="1000"
              />
              
              <BaseInput
                v-model="sysInitialConditions"
                label="Initial Conditions (comma separated)"
                placeholder="0.1, 0.2, 0, 0"
              />
            </div>
            
            <div class="mb-6">
              <label class="block text-sm font-medium text-gray-700 mb-2">
                System Equations (one per line)
              </label>
              <textarea
                :value="sysEquations.join('\n')"
                @input="sysEquations = ($event.target as HTMLTextAreaElement).value.split('\n').filter(eq => eq.trim())"
                rows="6"
                placeholder="theta1' = omega1\ntheta2' = omega2\nomega1' = -sin(theta1)\nomega2' = -sin(theta2)"
                class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="solveSystem"
              :disabled="solving"
              class="w-full md:w-auto"
            >
              <Play class="w-4 h-4 mr-2" :class="{ 'animate-spin': solving }" />
              {{ solving ? 'Solving...' : 'Solve System Dynamics' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="sysSolution" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">System Dynamics Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadSolution(sysSolution, 'system-dynamics')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Stable</p>
                    <p class="text-2xl font-bold" :class="sysSolution.stability.isStable ? 'text-green-600' : 'text-red-600'">
                      {{ sysSolution.stability.isStable ? '✓' : '✗' }}
                    </p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Max Change</p>
                    <p class="text-2xl font-bold">{{ sysSolution.stability.maxChange.toFixed(4) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Energy Conserved</p>
                    <p class="text-2xl font-bold" :class="sysSolution.energyBalance.energyConserved ? 'text-green-600' : 'text-red-600'">
                      {{ sysSolution.energyBalance.energyConserved ? '✓' : '✗' }}
                    </p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Derived Quantities</p>
                    <p class="text-2xl font-bold">{{ Object.keys(sysSolution.derivedQuantities).length }}</p>
                  </div>
                </BaseCard>
              </div>
              
              <div ref="sysChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>

      <!-- Optimization Tab -->
      <div v-show="activeTab === 'optimization'">
        <BaseCard>
          <div class="p-6">
            <h2 class="text-xl font-semibold mb-4">Parameter Optimization</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
              <BaseInput
                v-model="optObjectiveExpression"
                label="Objective Expression"
                placeholder="Enter mathematical expression"
              />
              
              <BaseInput
                v-model="optInitialGuess"
                label="Initial Guess (comma separated)"
                placeholder="1, 1"
              />
            </div>
            
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
              <BaseSelect
                v-model="optMethod"
                :options="[
                  { label: 'Gradient Descent', value: 'gradient' },
                  { label: 'Genetic Algorithm', value: 'genetic' }
                ]"
                label="Optimization Method"
              />
              
              <BaseInput
                v-if="optMethod === 'gradient'"
                v-model.number="optLearningRate"
                label="Learning Rate"
                type="number"
                step="0.001"
                min="0.001"
                max="1"
              />
              
              <BaseInput
                v-if="optMethod === 'gradient'"
                v-model.number="optMaxIterations"
                label="Max Iterations"
                type="number"
                min="1"
                max="10000"
              />
              
              <BaseInput
                v-if="optMethod === 'genetic'"
                v-model.number="optPopulationSize"
                label="Population Size"
                type="number"
                min="10"
                max="1000"
              />
              
              <BaseInput
                v-if="optMethod === 'genetic'"
                v-model.number="optMaxGenerations"
                label="Max Generations"
                type="number"
                min="10"
                max="1000"
              />
              
              <BaseInput
                v-if="optMethod === 'genetic'"
                v-model.number="optMutationRate"
                label="Mutation Rate"
                type="number"
                step="0.01"
                min="0.01"
                max="0.5"
              />
            </div>
            
            <BaseButton 
              variant="primary" 
              @click="optimize"
              :disabled="optimizing"
              class="w-full md:w-auto"
            >
              <Zap class="w-4 h-4 mr-2" :class="{ 'animate-spin': optimizing }" />
              {{ optimizing ? 'Optimizing...' : 'Run Optimization' }}
            </BaseButton>
            
            <!-- Results -->
            <div v-if="optResult" class="mt-8">
              <div class="flex justify-between items-center mb-4">
                <h3 class="text-lg font-medium">Optimization Results</h3>
                <BaseButton 
                  variant="outline" 
                  size="sm"
                  @click="downloadSolution(optResult, 'optimization-result')"
                >
                  <Download class="w-4 h-4 mr-2" />
                  Download
                </BaseButton>
              </div>
              
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Method</p>
                    <p class="text-lg font-bold capitalize">{{ optResult.method }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Optimal Value</p>
                    <p class="text-lg font-bold">{{ optResult.optimalValue.toFixed(6) }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Iterations</p>
                    <p class="text-lg font-bold">{{ optResult.iterations }}</p>
                  </div>
                </BaseCard>
                
                <BaseCard>
                  <div class="p-4">
                    <p class="text-sm text-gray-600">Converged</p>
                    <p class="text-lg font-bold" :class="optResult.converged ? 'text-green-600' : 'text-red-600'">
                      {{ optResult.converged ? '✓' : '✗' }}
                    </p>
                  </div>
                </BaseCard>
              </div>
              
              <div class="mb-4">
                <h4 class="font-medium mb-2">Optimal Parameters:</h4>
                <div class="flex flex-wrap gap-2">
                  <span
                    v-for="(param, index) in optResult.optimalParameters"
                    :key="index"
                    class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800"
                  >
                    x{{ (index as number) + 1 }} = {{ param.toFixed(4) }}
                  </span>
                </div>
              </div>
              
              <div ref="optChartRef" class="w-full h-80"></div>
            </div>
          </div>
        </BaseCard>
      </div>
    </div>
  </BaseCard>
</template>

<style scoped>
.mathematical-modeling {
  max-width: 1400px;
  margin: 0 auto;
  padding: 1rem;
}

@media (max-width: 640px) {
  .mathematical-modeling {
    padding: 0.5rem;
  }
}
</style>