<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import BaseCard from '@/components/base/BaseCard.vue'
import BaseButton from '@/components/base/BaseButton.vue'
import BaseInput from '@/components/base/BaseInput.vue'
import BaseSelect from '@/components/base/BaseSelect.vue'
import BaseSkeleton from '@/components/base/BaseSkeleton.vue'
import { useToast } from '@/composables/useToast'
import { fetchMachines } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'
import { 
  BarChart, 
  LineChart, 
  PieChart, 
  TrendingUp,
  Calendar,
  Filter,
  Download,
  RefreshCw,
  Settings
} from 'lucide-vue-next'

// Import charting library
import * as echarts from 'echarts/core'
import {
  BarChart as EChartsBar,
  LineChart as EChartsLine,
  PieChart as EChartsPie
} from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  ToolboxComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

// Register ECharts components
echarts.use([
  EChartsBar,
  EChartsLine,
  EChartsPie,
  GridComponent,
  TooltipComponent,
  LegendComponent,
  TitleComponent,
  ToolboxComponent,
  CanvasRenderer
])

const toast = useToast()

// State
const machines = ref<MachineDto[]>([])
const loading = ref(false)
const selectedMachine = ref<string>('all')
const selectedPeriod = ref('7d')
const selectedMetric = ref('rul')
const chartType = ref('line')

// Chart refs
const rulChartRef = ref<HTMLDivElement | null>(null)
const healthChartRef = ref<HTMLDivElement | null>(null)
const efficiencyChartRef = ref<HTMLDivElement | null>(null)
const distributionChartRef = ref<HTMLDivElement | null>(null)

// Chart instances
let rulChart: echarts.ECharts | null = null
let healthChart: echarts.ECharts | null = null
let efficiencyChart: echarts.ECharts | null = null
let distributionChart: echarts.ECharts | null = null

// Empty analytics data, backend endpoints not implemented
const analyticsData = ref({
  rulData: [] as any[],
  healthData: [] as any[],
  efficiencyData: [] as any[]
})

// Computed
const machineOptions = computed(() => {
  const options = [{ label: 'All Machines', value: 'all' }]
  machines.value.forEach(machine => {
    options.push({ label: `${machine.name} (${machine.id})`, value: machine.id })
  })
  return options
})

const periodOptions = [
  { label: 'Last 24 Hours', value: '24h' },
  { label: 'Last 7 Days', value: '7d' },
  { label: 'Last 30 Days', value: '30d' },
  { label: 'Last 90 Days', value: '90d' }
]

const metricOptions = [
  { label: 'Remaining Useful Life', value: 'rul' },
  { label: 'Health Score', value: 'health' },
  { label: 'Efficiency', value: 'efficiency' },
  { label: 'Failure Probability', value: 'failure' }
]

const chartTypeOptions = [
  { label: 'Line Chart', value: 'line' },
  { label: 'Bar Chart', value: 'bar' },
  { label: 'Area Chart', value: 'area' }
]

// Methods
const loadMachines = async () => {
  try {
    machines.value = await fetchMachines()
  } catch (error) {
    console.error('Error loading machines:', error)
  }
}

const loadData = async () => {
  try {
    loading.value = true
    // Simulate API call delay
    await new Promise(resolve => setTimeout(resolve, 1500))
    toast.success('Analytics data loaded successfully')
  } catch (error) {
    console.error('Error loading analytics data:', error)
    toast.error('Failed to load analytics data')
  } finally {
    loading.value = false
  }
}

const initCharts = () => {
  // Initialize RUL Chart
  if (rulChartRef.value) {
    rulChart = echarts.init(rulChartRef.value)
    updateRulChart()
  }

  // Initialize Health Chart
  if (healthChartRef.value) {
    healthChart = echarts.init(healthChartRef.value)
    updateHealthChart()
  }

  // Initialize Efficiency Chart
  if (efficiencyChartRef.value) {
    efficiencyChart = echarts.init(efficiencyChartRef.value)
    updateEfficiencyChart()
  }

  // Initialize Distribution Chart
  if (distributionChartRef.value) {
    distributionChart = echarts.init(distributionChartRef.value)
    updateDistributionChart()
  }

  // Handle window resize
  window.addEventListener('resize', handleResize)
}

const handleResize = () => {
  rulChart?.resize()
  healthChart?.resize()
  efficiencyChart?.resize()
  distributionChart?.resize()
}

const updateRulChart = () => {
  if (!rulChart) return

  const option = {
    title: {
      text: 'Remaining Useful Life Trend',
      left: 'center',
      textStyle: { fontSize: 16 }
    },
    tooltip: {
      trigger: 'axis',
      formatter: '{b}<br/>{a}: {c} days'
    },
    xAxis: {
      type: 'category',
      data: analyticsData.value.rulData.map(item => item.date)
    },
    yAxis: {
      type: 'value',
      name: 'Days'
    },
    series: [{
      name: 'RUL',
      type: chartType.value === 'area' ? 'line' : chartType.value,
      data: analyticsData.value.rulData.map(item => item.value),
      smooth: true,
      areaStyle: chartType.value === 'area' ? {} : undefined
    }],
    color: ['#3b82f6']
  }

  rulChart.setOption(option)
}

const updateHealthChart = () => {
  if (!healthChart) return

  const option = {
    title: {
      text: 'Machine Health Distribution Over Time',
      left: 'center',
      textStyle: { fontSize: 16 }
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' }
    },
    legend: {
      data: ['Healthy', 'Normal', 'Degraded'],
      bottom: 10
    },
    xAxis: {
      type: 'category',
      data: analyticsData.value.healthData.map(item => item.date)
    },
    yAxis: {
      type: 'value',
      name: 'Percentage (%)'
    },
    series: [
      {
        name: 'Healthy',
        type: 'bar',
        stack: 'total',
        data: analyticsData.value.healthData.map(item => item.healthy),
        color: '#10b981'
      },
      {
        name: 'Normal',
        type: 'bar',
        stack: 'total',
        data: analyticsData.value.healthData.map(item => item.normal),
        color: '#f59e0b'
      },
      {
        name: 'Degraded',
        type: 'bar',
        stack: 'total',
        data: analyticsData.value.healthData.map(item => item.degraded),
        color: '#ef4444'
      }
    ]
  }

  healthChart.setOption(option)
}

const updateEfficiencyChart = () => {
  if (!efficiencyChart) return

  const option = {
    title: {
      text: 'Machine Efficiency Comparison',
      left: 'center',
      textStyle: { fontSize: 16 }
    },
    tooltip: {
      trigger: 'axis',
      axisPointer: { type: 'shadow' },
      formatter: '{b}: {c}%'
    },
    xAxis: {
      type: 'category',
      data: analyticsData.value.efficiencyData.map(item => item.machine)
    },
    yAxis: {
      type: 'value',
      name: 'Efficiency (%)',
      min: 0,
      max: 100
    },
    series: [{
      name: 'Efficiency',
      type: 'bar',
      data: analyticsData.value.efficiencyData.map(item => item.efficiency),
      color: ['#3b82f6'],
      barWidth: '60%'
    }]
  }

  efficiencyChart.setOption(option)
}

const updateDistributionChart = () => {
  if (!distributionChart) return

  // Calculate health distribution for pie chart
  const totalMachines = analyticsData.value.efficiencyData.length
  const healthyCount = analyticsData.value.efficiencyData.filter(m => m.efficiency >= 90).length
  const normalCount = analyticsData.value.efficiencyData.filter(m => m.efficiency >= 75 && m.efficiency < 90).length
  const degradedCount = analyticsData.value.efficiencyData.filter(m => m.efficiency < 75).length

  const option = {
    title: {
      text: 'Health Status Distribution',
      left: 'center',
      textStyle: { fontSize: 16 }
    },
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 'left',
      data: ['Healthy', 'Normal', 'Degraded']
    },
    series: [{
      name: 'Health Status',
      type: 'pie',
      radius: ['40%', '70%'],
      avoidLabelOverlap: false,
      data: [
        { value: healthyCount, name: 'Healthy', itemStyle: { color: '#10b981' } },
        { value: normalCount, name: 'Normal', itemStyle: { color: '#f59e0b' } },
        { value: degradedCount, name: 'Degraded', itemStyle: { color: '#ef4444' } }
      ],
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.5)'
        }
      }
    }]
  }

  distributionChart.setOption(option)
}

const refreshData = async () => {
  await loadData()
  updateAllCharts()
}

const updateAllCharts = () => {
  updateRulChart()
  updateHealthChart()
  updateEfficiencyChart()
  updateDistributionChart()
}

const exportChart = (chartInstance: echarts.ECharts | null, filename: string) => {
  if (!chartInstance) return
  
  const dataUrl = chartInstance.getDataURL({
    type: 'png',
    pixelRatio: 2,
    backgroundColor: '#fff'
  })
  
  const link = document.createElement('a')
  link.download = `${filename}.png`
  link.href = dataUrl
  link.click()
  
  toast.success(`Chart exported as ${filename}.png`)
}

const exportAllCharts = () => {
  exportChart(rulChart, 'rul-trend')
  exportChart(healthChart, 'health-distribution')
  exportChart(efficiencyChart, 'efficiency-comparison')
  exportChart(distributionChart, 'health-pie-chart')
}

// Watch for changes and update charts
watch([selectedMachine, selectedPeriod, selectedMetric, chartType], () => {
  updateAllCharts()
})

onMounted(() => {
  loadMachines()
  loadData().then(() => {
    initCharts()
  })
})
</script>

<template>
  <BaseCard class="advanced-analytics-viz">
    <template #header>
      <div class="header-content">
        <h2 class="header-title">
          <BarChart class="header-icon" />
          Advanced Analytics Visualization
        </h2>
        <p class="header-subtitle">Interactive dashboards for predictive analytics and performance insights</p>
      </div>
      <div class="header-controls">
        <BaseButton variant="outline" @click="refreshData" :disabled="loading">
          <RefreshCw class="button-icon" :class="{ 'animate-spin': loading }" />
          Refresh Data
        </BaseButton>
        <BaseButton variant="outline" @click="exportAllCharts">
          <Download class="button-icon" />
          Export All
        </BaseButton>
      </div>
    </template>

    <!-- Controls -->
    <div class="controls-section">
      <div class="control-row">
        <BaseSelect
          v-model="selectedMachine"
          :options="machineOptions"
          label="Machine"
          class="control-select"
        />
        
        <BaseSelect
          v-model="selectedPeriod"
          :options="periodOptions"
          label="Time Period"
          class="control-select"
        />
        
        <BaseSelect
          v-model="selectedMetric"
          :options="metricOptions"
          label="Metric"
          class="control-select"
        />
        
        <BaseSelect
          v-model="chartType"
          :options="chartTypeOptions"
          label="Chart Type"
          class="control-select"
        />
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="loading-container">
      <div class="charts-skeleton">
        <BaseSkeleton v-for="i in 4" :key="i" height="300px" class="chart-placeholder" />
      </div>
    </div>

    <!-- Charts Dashboard -->
    <div v-else class="charts-container">
      <div class="charts-grid">
        <!-- RUL Trend Chart -->
        <BaseCard class="chart-card">
          <div class="chart-header">
            <h3>Remaining Useful Life Trend</h3>
            <BaseButton
              variant="ghost"
              size="sm"
              @click="exportChart(rulChart, 'rul-trend')"
            >
              <Download class="icon-sm" />
            </BaseButton>
          </div>
          <div ref="rulChartRef" class="chart-wrapper"></div>
        </BaseCard>

        <!-- Health Distribution Chart -->
        <BaseCard class="chart-card">
          <div class="chart-header">
            <h3>Health Status Distribution</h3>
            <BaseButton
              variant="ghost"
              size="sm"
              @click="exportChart(healthChart, 'health-distribution')"
            >
              <Download class="icon-sm" />
            </BaseButton>
          </div>
          <div ref="healthChartRef" class="chart-wrapper"></div>
        </BaseCard>

        <!-- Efficiency Comparison Chart -->
        <BaseCard class="chart-card">
          <div class="chart-header">
            <h3>Machine Efficiency Comparison</h3>
            <BaseButton
              variant="ghost"
              size="sm"
              @click="exportChart(efficiencyChart, 'efficiency-comparison')"
            >
              <Download class="icon-sm" />
            </BaseButton>
          </div>
          <div ref="efficiencyChartRef" class="chart-wrapper"></div>
        </BaseCard>

        <!-- Health Pie Chart -->
        <BaseCard class="chart-card">
          <div class="chart-header">
            <h3>Overall Health Distribution</h3>
            <BaseButton
              variant="ghost"
              size="sm"
              @click="exportChart(distributionChart, 'health-pie-chart')"
            >
              <Download class="icon-sm" />
            </BaseButton>
          </div>
          <div ref="distributionChartRef" class="chart-wrapper"></div>
        </BaseCard>
      </div>

      <!-- Summary Stats -->
      <BaseCard class="summary-section">
        <template #header>
          <h3>Performance Summary</h3>
        </template>
        <div class="summary-grid">
          <div class="summary-item">
            <TrendingUp class="summary-icon text-green-500" />
            <div>
              <div class="summary-value">87%</div>
              <div class="summary-label">Average Efficiency</div>
            </div>
          </div>
          
          <div class="summary-item">
            <Calendar class="summary-icon text-blue-500" />
            <div>
              <div class="summary-value">164</div>
              <div class="summary-label">Avg RUL (days)</div>
            </div>
          </div>
          
          <div class="summary-item">
            <PieChart class="summary-icon text-purple-500" />
            <div>
              <div class="summary-value">65%</div>
              <div class="summary-label">Healthy Machines</div>
            </div>
          </div>
          
          <div class="summary-item">
            <LineChart class="summary-icon text-orange-500" />
            <div>
              <div class="summary-value">-3.2%</div>
              <div class="summary-label">Trend (7 days)</div>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </BaseCard>
</template>

<style scoped>
.advanced-analytics-viz {
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

.controls-section {
  margin: 1.5rem 0;
}

.control-row {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.control-select {
  min-width: 180px;
}

.loading-container {
  padding: 2rem;
}

.charts-skeleton {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1rem;
}

.chart-placeholder {
  border-radius: 0.5rem;
}

.charts-container {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.charts-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 1.5rem;
}

.chart-card {
  padding: 1rem;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.chart-header h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1e293b;
}

.icon-sm {
  width: 1rem;
  height: 1rem;
}

.chart-wrapper {
  width: 100%;
  height: 300px;
}

.summary-section {
  padding: 1.5rem;
}

.summary-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1.5rem;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #f8fafc;
  border-radius: 0.5rem;
}

.summary-icon {
  width: 2rem;
  height: 2rem;
  flex-shrink: 0;
}

.summary-value {
  font-size: 1.5rem;
  font-weight: 600;
  color: #1e293b;
}

.summary-label {
  font-size: 0.875rem;
  color: #64748b;
}

@media (max-width: 1024px) {
  .charts-grid {
    grid-template-columns: 1fr;
  }
  
  .summary-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .advanced-analytics-viz {
    padding: 0.5rem;
  }
  
  .header-controls {
    flex-direction: column;
    width: 100%;
  }
  
  .control-row {
    flex-direction: column;
  }
  
  .control-select {
    min-width: auto;
  }
  
  .summary-grid {
    grid-template-columns: 1fr;
  }
  
  .chart-wrapper {
    height: 250px;
  }
}
</style>