<template>
  <div class="prescriptive-panel glass-panel" v-if="machineId">
    <header class="panel-header">
      <div class="header-content">
        <h3 class="title">Prescriptive Maintenance</h3>
        <p class="subtitle">What-if analysis and cost optimization</p>
      </div>
      <div class="recommendation-badge" :class="optimalWindow ? 'active' : ''" v-if="optimalWindow">
        Optimal: {{ new Date(optimalWindow.scheduledDate).toLocaleDateString() }}
      </div>
    </header>

    <div class="analysis-content" v-if="loading">
      <div class="loader">Calculating optimal windows...</div>
    </div>

    <div class="analysis-content" v-else-if="analysisData.length">
      <div class="chart-container">
        <!-- We use a custom SVG chart for lightness if ECharts is not available here -->
        <div class="cost-chart">
          <div class="y-axis">
            <span>Cost ($)</span>
            <span>Risk (%)</span>
          </div>
          <div class="bars">
            <div v-for="(item, index) in analysisData" :key="index" class="bar-group" :title="item.recommendation">
              <div class="bar cost-bar" :style="{ height: `${(item.estimatedCost / 5000) * 100}%` }"></div>
              <div class="bar risk-bar" :style="{ height: `${item.riskScore * 100}%` }"></div>
              <span class="date-label">{{ new Date(item.scheduledDate).getDate() }}/{{ new Date(item.scheduledDate).getMonth() + 1 }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="summary-cards">
        <div class="summary-card">
          <span class="label">Total Expected Cost</span>
          <span class="value">${{ optimalWindow?.estimatedCost.toFixed(0) }}</span>
        </div>
        <div class="summary-card">
          <span class="label">Failure Probability</span>
          <span class="value">{{ (optimalWindow?.riskScore || 0 * 100).toFixed(1) }}%</span>
        </div>
      </div>

      <div class="recommendation-text" v-if="optimalWindow">
        <InfoIcon class="h-4 w-4" />
        {{ optimalWindow.recommendation }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { prescriptiveService, type MaintenanceWindow } from '@/services/prescriptive.service'
import { Info as InfoIcon } from 'lucide-vue-next'

const props = defineProps<{
  machineId: string
}>()

const loading = ref(false)
const analysisData = ref<MaintenanceWindow[]>([])
const optimalWindow = ref<MaintenanceWindow | null>(null)

async function fetchAnalysis() {
  if (!props.machineId) return
  
  loading.value = true
  try {
    const [analysis, optimal] = await Promise.all([
      prescriptiveService.getAnalysis(props.machineId),
      prescriptiveService.getOptimal(props.machineId)
    ])
    analysisData.value = analysis
    optimalWindow.value = optimal
  } finally {
    loading.value = false
  }
}

onMounted(fetchAnalysis)
watch(() => props.machineId, fetchAnalysis)
</script>

<style scoped>
.prescriptive-panel {
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.panel-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.title { margin: 0; font-size: 1.25rem; color: #fff; }
.subtitle { margin: 0.25rem 0 0; font-size: 0.85rem; color: #8c8c8c; }

.recommendation-badge {
  padding: 0.4rem 0.8rem;
  background: rgba(24, 144, 255, 0.1);
  border: 1px solid #1890ff;
  border-radius: 4px;
  color: #1890ff;
  font-size: 0.85rem;
  font-weight: 600;
}

.chart-container {
  height: 200px;
  margin-top: 1rem;
}

.cost-chart {
  height: 100%;
  display: flex;
  gap: 1rem;
  align-items: flex-end;
  padding-left: 2rem;
  position: relative;
}

.bars {
  display: flex;
  justify-content: space-between;
  width: 100%;
  height: 100%;
  align-items: flex-end;
}

.bar-group {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  width: 20px;
  height: 100%;
  justify-content: flex-end;
}

.bar { width: 100%; border-radius: 2px 2px 0 0; transition: height 0.3s ease; }
.cost-bar { background: rgba(24, 144, 255, 0.6); }
.risk-bar { background: rgba(245, 34, 45, 0.6); }

.date-label { font-size: 0.65rem; color: #595959; margin-top: 4px; }

.summary-cards {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.summary-card {
  padding: 1rem;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  display: flex;
  flex-direction: column;
}

.label { font-size: 0.75rem; color: #8c8c8c; margin-bottom: 0.25rem; }
.value { font-size: 1.1rem; font-weight: 600; color: #fff; }

.recommendation-text {
  padding: 1rem;
  background: rgba(250, 173, 20, 0.1);
  border-left: 4px solid #faad14;
  font-size: 0.9rem;
  color: #faad14;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
</style>
