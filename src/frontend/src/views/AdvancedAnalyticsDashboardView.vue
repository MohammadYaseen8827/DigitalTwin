<template>
  <main class="advanced-dashboard-page container">
    <div class="page-header glass-panel">
      <div>
        <p class="eyebrow">Unified Intelligence</p>
        <h1>Advanced Analytics Dashboard</h1>
        <p class="subtitle">
          Comprehensive view of all predictive and prescriptive analytics for selected machine
        </p>
      </div>
      <div class="header-actions">
        <div class="machine-selector">
          <label for="machine-select">Select Machine:</label>
          <select 
            id="machine-select"
            v-model="selectedMachineId" 
            @change="loadDashboardData"
            class="machine-dropdown"
          >
            <option value="">Choose a machine...</option>
            <option v-for="machine in machines" :key="machine.id" :value="machine.id">
              {{ machine.name }} ({{ machine.type }})
            </option>
          </select>
        </div>
        <BaseButton 
          size="md" 
          variant="primary" 
          :disabled="!selectedMachineId || loading" 
          @click="refreshDashboard"
          :loading="loading"
        >
          <template #icon>
            <RefreshCw class="h-4 w-4" />
          </template>
          Refresh Dashboard
        </BaseButton>
      </div>
    </div>

    <div v-if="!selectedMachineId" class="selection-prompt glass-panel">
      <div class="prompt-content">
        <LayoutDashboard class="prompt-icon" />
        <h2>Select a Machine to View Analytics</h2>
        <p>Choose a machine from the dropdown to see a comprehensive dashboard of all advanced analytics capabilities including predictions, anomaly detection, and maintenance recommendations.</p>
      </div>
    </div>

    <div v-else-if="loading" class="loading-state glass-panel">
      <div class="spinner"></div>
      <p>Loading advanced analytics dashboard...</p>
    </div>

    <div v-else-if="error" class="error-state glass-panel">
      <AlertCircle class="error-icon" />
      <div>
        <h3>Failed to Load Dashboard</h3>
        <p>{{ error }}</p>
      </div>
      <BaseButton size="sm" variant="outline" @click="loadDashboardData">
        Try Again
      </BaseButton>
    </div>

    <template v-else-if="dashboardData">
      <!-- Health Overview -->
      <div class="health-overview glass-panel">
        <div class="overview-header">
          <h2>Machine Health Status</h2>
          <div class="health-score" :class="getHealthScoreClass(dashboardData.healthScore)">
            <Gauge class="gauge-icon" />
            <div class="score-content">
              <span class="score-value">{{ Math.round(dashboardData.healthScore) }}</span>
              <span class="score-label">Health Score</span>
            </div>
          </div>
        </div>
        <div class="health-indicators">
          <div class="indicator" :class="getIndicatorClass('prediction')">
            <div class="indicator-icon">
              <Zap :class="{ active: hasRecentPrediction }" />
            </div>
            <div class="indicator-text">
              <span class="indicator-label">Prediction Status</span>
              <span class="indicator-value">{{ predictionStatus }}</span>
            </div>
          </div>
          <div class="indicator" :class="getIndicatorClass('anomaly')">
            <div class="indicator-icon">
              <AlertTriangle :class="{ active: hasAnomalies }" />
            </div>
            <div class="indicator-text">
              <span class="indicator-label">Anomaly Detection</span>
              <span class="indicator-value">{{ anomalyStatus }}</span>
            </div>
          </div>
          <div class="indicator" :class="getIndicatorClass('recommendation')">
            <div class="indicator-icon">
              <Lightbulb :class="{ active: hasRecommendations }" />
            </div>
            <div class="indicator-text">
              <span class="indicator-label">Maintenance Recommendations</span>
              <span class="indicator-value">{{ recommendationStatus }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Dashboard Grid -->
      <div class="dashboard-grid">
        <!-- Prediction Card -->
        <div class="dashboard-card glass-panel">
          <div class="card-header">
            <div class="header-icon prediction">
              <Zap class="icon" />
            </div>
            <h3>Prediction Analysis</h3>
            <BaseButton 
              size="sm" 
              variant="ghost" 
              @click="goToAdvancedAnalytics"
            >
              <ExternalLink class="h-4 w-4" />
            </BaseButton>
          </div>
          <div class="card-content">
            <template v-if="dashboardData.prediction">
              <div class="prediction-summary">
                <div class="rul-display">
                  <span class="rul-value">{{ Math.round(dashboardData.prediction.remainingUsefulLifeDays) }}</span>
                  <span class="rul-unit">days</span>
                </div>
                <div class="health-status" :class="getHealthClass(dashboardData.prediction.healthStatus)">
                  {{ dashboardData.prediction.healthStatus }}
                </div>
              </div>
              <div class="prediction-details">
                <div class="detail-item">
                  <span class="label">Failure Probability:</span>
                  <span class="value">{{ (dashboardData.prediction.failureProbability * 100).toFixed(1) }}%</span>
                </div>
                <div class="detail-item">
                  <span class="label">Model Version:</span>
                  <span class="value">v{{ dashboardData.prediction.modelVersion }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Generated:</span>
                  <span class="value">{{ timeAgo(dashboardData.prediction.createdAt) }}</span>
                </div>
              </div>
            </template>
            <div v-else class="no-data">
              <Info class="info-icon" />
              <p>No prediction data available</p>
              <BaseButton size="sm" variant="outline" @click="generatePrediction">
                Generate Prediction
              </BaseButton>
            </div>
          </div>
        </div>

        <!-- Anomaly Detection Card -->
        <div class="dashboard-card glass-panel">
          <div class="card-header">
            <div class="header-icon anomaly">
              <AlertTriangle class="icon" />
            </div>
            <h3>Anomaly Detection</h3>
            <BaseButton 
              size="sm" 
              variant="ghost" 
              @click="goToAdvancedAnalytics"
            >
              <ExternalLink class="h-4 w-4" />
            </BaseButton>
          </div>
          <div class="card-content">
            <div class="anomaly-summary">
              <div class="risk-score">
                <span class="score-label">Risk Level</span>
                <span class="score-value" :class="getRiskClass(dashboardData.anomalyDetection.overallRiskScore)">
                  {{ dashboardData.anomalyDetection.overallRiskScore.toFixed(1) }}/100
                </span>
              </div>
              <div class="anomaly-count">
                <span class="count">{{ dashboardData.anomalyDetection.anomalies.length }}</span>
                <span class="count-label">Anomalies Detected</span>
              </div>
            </div>
            <div class="recent-anomalies">
              <h4>Recent Anomalies</h4>
              <div v-if="dashboardData.anomalyDetection.anomalies.length === 0" class="no-anomalies">
                <CheckCircle class="success-icon" />
                <span>No anomalies detected</span>
              </div>
              <div v-else class="anomalies-list">
                <div 
                  v-for="(anomaly, index) in dashboardData.anomalyDetection.anomalies.slice(0, 3)" 
                  :key="index"
                  class="anomaly-item"
                  :class="`severity-${anomaly.severity}`"
                >
                  <div class="anomaly-metric">{{ anomaly.metric }}</div>
                  <div class="anomaly-value">{{ anomaly.value.toFixed(2) }}</div>
                  <div class="anomaly-time">{{ timeAgo(anomaly.timestamp) }}</div>
                </div>
                <div v-if="dashboardData.anomalyDetection.anomalies.length > 3" class="more-anomalies">
                  +{{ dashboardData.anomalyDetection.anomalies.length - 3 }} more
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Maintenance Recommendations Card -->
        <div class="dashboard-card glass-panel">
          <div class="card-header">
            <div class="header-icon recommendation">
              <Lightbulb class="icon" />
            </div>
            <h3>Maintenance Recommendations</h3>
            <BaseButton 
              size="sm" 
              variant="ghost" 
              @click="goToEnhancedPrescriptive"
            >
              <ExternalLink class="h-4 w-4" />
            </BaseButton>
          </div>
          <div class="card-content">
            <template v-if="dashboardData.maintenanceRecommendation">
              <div class="recommendation-summary">
                <div class="priority" :class="getPriorityClass(dashboardData.maintenanceRecommendation.priorityScore)">
                  <span class="priority-label">Priority</span>
                  <span class="priority-value">{{ getPriorityLabel(dashboardData.maintenanceRecommendation.priorityScore) }}</span>
                </div>
                <div class="recommendation-type">
                  <span class="type-label">Recommended Action</span>
                  <span class="type-value">{{ dashboardData.maintenanceRecommendation.recommendedType }}</span>
                </div>
              </div>
              <div class="recommendation-details">
                <div class="detail-item">
                  <span class="label">Scheduled Date:</span>
                  <span class="value">{{ formatDate(dashboardData.maintenanceRecommendation.recommendedDate) }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Estimated Cost:</span>
                  <span class="value">${{ dashboardData.maintenanceRecommendation.expectedCost.toLocaleString() }}</span>
                </div>
                <div class="detail-item">
                  <span class="label">Risk Reduction:</span>
                  <span class="value">{{ (dashboardData.maintenanceRecommendation.riskReduction * 100).toFixed(0) }}%</span>
                </div>
              </div>
              <div class="recommendation-actions">
                <BaseButton size="sm" variant="primary" @click="viewRecommendationDetails">
                  View Details
                </BaseButton>
                <BaseButton size="sm" variant="outline" @click="generateNewRecommendation">
                  New Recommendation
                </BaseButton>
              </div>
            </template>
            <div v-else class="no-data">
              <Info class="info-icon" />
              <p>No recommendations available</p>
              <BaseButton size="sm" variant="outline" @click="generateRecommendation">
                Generate Recommendation
              </BaseButton>
            </div>
          </div>
        </div>

        <!-- Quick Actions Card -->
        <div class="dashboard-card glass-panel">
          <div class="card-header">
            <div class="header-icon actions">
              <Play class="icon" />
            </div>
            <h3>Quick Actions</h3>
          </div>
          <div class="card-content">
            <div class="quick-actions">
              <BaseButton 
                size="md" 
                variant="primary" 
                class="action-button"
                @click="runAllAnalyses"
                :disabled="runningAnalyses"
                :loading="runningAnalyses"
              >
                <template #icon>
                  <Zap class="h-4 w-4" />
                </template>
                Run All Analyses
              </BaseButton>
              
              <BaseButton 
                size="md" 
                variant="secondary" 
                class="action-button"
                @click="generateReport"
              >
                <template #icon>
                  <FileText class="h-4 w-4" />
                </template>
                Generate Report
              </BaseButton>
              
              <BaseButton 
                size="md" 
                variant="outline" 
                class="action-button"
                @click="exportData"
              >
                <template #icon>
                  <Download class="h-4 w-4" />
                </template>
                Export Data
              </BaseButton>
            </div>
          </div>
        </div>
      </div>

      <!-- Last Updated -->
      <div class="last-updated">
        <span>Dashboard refreshed: {{ timeAgo(dashboardData.generatedAt) }}</span>
      </div>
    </template>
  </main>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useMachinesStore } from '@/stores/machines.store'
import { getAdvancedAnalyticsDashboard } from '@/services/advancedAnalytics.service'
import type { AdvancedAnalyticsDashboard } from '@/services/advancedAnalytics.service'
import { 
  LayoutDashboard,
  RefreshCw,
  AlertCircle,
  Gauge,
  Zap,
  AlertTriangle,
  Lightbulb,
  Info,
  CheckCircle,
  ExternalLink,
  Play,
  FileText,
  Download
} from 'lucide-vue-next'

const router = useRouter()
const machinesStore = useMachinesStore()
const loading = ref(false)
const error = ref<string | null>(null)
const selectedMachineId = ref('')
const dashboardData = ref<AdvancedAnalyticsDashboard | null>(null)
const runningAnalyses = ref(false)

const machines = computed(() => machinesStore.machines)

// Initialize machines if needed
if (machines.value.length === 0) {
  machinesStore.fetchMachines()
}

// Computed properties for dashboard indicators
const hasRecentPrediction = computed(() => 
  dashboardData.value?.prediction && 
  new Date(dashboardData.value.prediction.createdAt) > new Date(Date.now() - 24 * 60 * 60 * 1000)
)

const hasAnomalies = computed(() => 
  dashboardData.value?.anomalyDetection.anomalies.length > 0
)

const hasRecommendations = computed(() => 
  dashboardData.value?.maintenanceRecommendation !== null
)

const predictionStatus = computed(() => 
  hasRecentPrediction.value ? 'Current' : 'Stale'
)

const anomalyStatus = computed(() => 
  hasAnomalies.value ? `${dashboardData.value?.anomalyDetection.anomalies.length} detected` : 'None'
)

const recommendationStatus = computed(() => 
  hasRecommendations.value ? 'Available' : 'None'
)

// Helper functions
const getHealthScoreClass = (score: number) => {
  if (score >= 80) return 'excellent'
  if (score >= 60) return 'good'
  if (score >= 40) return 'fair'
  return 'poor'
}

const getIndicatorClass = (type: string) => {
  switch (type) {
    case 'prediction':
      return hasRecentPrediction.value ? 'active' : 'inactive'
    case 'anomaly':
      return hasAnomalies.value ? 'warning' : 'active'
    case 'recommendation':
      return hasRecommendations.value ? 'active' : 'inactive'
    default:
      return 'inactive'
  }
}

const getHealthClass = (status: string | undefined) => {
  if (!status) return 'unknown'
  const s = status.toLowerCase()
  if (s.includes('healthy')) return 'healthy'
  if (s.includes('degraded')) return 'degraded'
  if (s.includes('critical')) return 'critical'
  return 'unknown'
}

const getRiskClass = (score: number) => {
  if (score >= 80) return 'critical'
  if (score >= 60) return 'high'
  if (score >= 40) return 'medium'
  return 'low'
}

const getPriorityClass = (score: number) => {
  if (score >= 80) return 'critical'
  if (score >= 60) return 'high'
  if (score >= 40) return 'medium'
  return 'low'
}

const getPriorityLabel = (score: number) => {
  if (score >= 80) return 'CRITICAL'
  if (score >= 60) return 'HIGH'
  if (score >= 40) return 'MEDIUM'
  return 'LOW'
}

const timeAgo = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  
  if (diffHours < 1) return 'Just now'
  if (diffHours < 24) return `${diffHours}h ago`
  const diffDays = Math.floor(diffHours / 24)
  return `${diffDays}d ago`
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}

// Action handlers
const loadDashboardData = async () => {
  if (!selectedMachineId.value) return
  
  loading.value = true
  error.value = null
  
  try {
    const data = await getAdvancedAnalyticsDashboard(selectedMachineId.value)
    dashboardData.value = data
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Failed to load dashboard data'
    console.error('Dashboard load error:', err)
  } finally {
    loading.value = false
  }
}

const refreshDashboard = () => {
  loadDashboardData()
}

const goToAdvancedAnalytics = () => {
  router.push('/advanced-analytics')
}

const goToEnhancedPrescriptive = () => {
  router.push('/enhanced-prescriptive')
}

const generatePrediction = () => {
  // Would trigger prediction generation
  // Generating prediction
}

const generateRecommendation = () => {
  // Would trigger recommendation generation
  // Generating recommendation
}

const viewRecommendationDetails = () => {
  // Would show detailed recommendation view
  // Viewing recommendation details
}

const generateNewRecommendation = () => {
  // Would generate new recommendation
  // Generating new recommendation
}

const runAllAnalyses = async () => {
  runningAnalyses.value = true
  try {
    // Simulate running all analyses
    await new Promise(resolve => setTimeout(resolve, 3000))
    await loadDashboardData()
  } finally {
    runningAnalyses.value = false
  }
}

const generateReport = () => {
  // Would generate comprehensive report
  // Generating report
}

const exportData = () => {
  // Would export dashboard data
  // Exporting data
}

// Load initial data when machine is selected
loadDashboardData()
</script>

<style scoped>
.advanced-dashboard-page {
  padding: clamp(var(--space-16), 3vw, var(--space-24)) 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: var(--space-20);
  gap: var(--space-16);
}

.eyebrow {
  margin: 0 0 var(--space-4);
  font-size: var(--font-size-sm);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-secondary);
}

.page-header h1 {
  margin: 0 0 var(--space-8);
  font-size: clamp(1.75rem, 4vw, 2.5rem);
  font-weight: 700;
  background: linear-gradient(135deg, var(--primary-color), #096dd9);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.subtitle {
  margin: 0;
  font-size: var(--font-size-base);
  color: var(--color-text-secondary);
  max-width: 600px;
  line-height: 1.6;
}

.header-actions {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
  min-width: 250px;
}

.machine-selector {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.machine-selector label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.machine-dropdown {
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.machine-dropdown:focus {
  outline: none;
  border-color: var(--primary-color);
}

.selection-prompt {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.prompt-content {
  max-width: 500px;
}

.prompt-icon {
  width: 4rem;
  height: 4rem;
  margin-bottom: 1.5rem;
  opacity: 0.7;
}

.prompt-content h2 {
  margin: 0 0 1rem;
  font-size: 1.5rem;
  color: var(--color-text-primary);
}

.loading-state, .error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 3rem 2rem;
  gap: 1rem;
  text-align: center;
}

.spinner {
  width: 3rem;
  height: 3rem;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top: 3px solid var(--primary-color);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.error-icon {
  width: 3rem;
  height: 3rem;
  color: var(--color-error);
}

.health-overview {
  padding: var(--space-20);
}

.overview-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-16);
  flex-wrap: wrap;
  gap: var(--space-8);
}

.overview-header h2 {
  margin: 0;
  color: var(--color-text-primary);
}

.health-score {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-12);
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.1), rgba(255, 255, 255, 0.05));
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.health-score.excellent {
  background: linear-gradient(135deg, rgba(82, 196, 26, 0.2), rgba(82, 196, 26, 0.1));
  border-color: rgba(82, 196, 26, 0.3);
}

.health-score.good {
  background: linear-gradient(135deg, rgba(250, 173, 20, 0.2), rgba(250, 173, 20, 0.1));
  border-color: rgba(250, 173, 20, 0.3);
}

.health-score.fair {
  background: linear-gradient(135deg, rgba(245, 108, 35, 0.2), rgba(245, 108, 35, 0.1));
  border-color: rgba(245, 108, 35, 0.3);
}

.health-score.poor {
  background: linear-gradient(135deg, rgba(245, 34, 45, 0.2), rgba(245, 34, 45, 0.1));
  border-color: rgba(245, 34, 45, 0.3);
}

.gauge-icon {
  width: 2rem;
  height: 2rem;
  color: var(--color-text-primary);
}

.score-content {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.score-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--color-text-primary);
}

.score-label {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.health-indicators {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-12);
}

.indicator {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  padding: var(--space-12);
  border-radius: 8px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.indicator.active {
  background: rgba(82, 196, 26, 0.1);
  border-color: rgba(82, 196, 26, 0.3);
}

.indicator.warning {
  background: rgba(250, 173, 20, 0.1);
  border-color: rgba(250, 173, 20, 0.3);
}

.indicator.inactive {
  opacity: 0.6;
}

.indicator-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.1);
}

.indicator-icon .active {
  color: var(--color-success);
}

.indicator-text {
  flex: 1;
}

.indicator-label {
  display: block;
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin-bottom: var(--space-1);
}

.indicator-value {
  display: block;
  font-weight: 600;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: var(--space-16);
}

.dashboard-card {
  padding: var(--space-16);
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.card-header {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  margin-bottom: var(--space-12);
}

.header-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-icon.prediction {
  background: linear-gradient(135deg, #1890ff, #096dd9);
}

.header-icon.anomaly {
  background: linear-gradient(135deg, #fa8c16, #d46b08);
}

.header-icon.recommendation {
  background: linear-gradient(135deg, #722ed1, #531dab);
}

.header-icon.actions {
  background: linear-gradient(135deg, #52c41a, #389e0d);
}

.icon {
  width: 1.25rem;
  height: 1.25rem;
  color: white;
}

.card-header h3 {
  margin: 0;
  color: var(--color-text-primary);
  font-size: 1.1rem;
  flex: 1;
}

.card-content {
  min-height: 200px;
}

.prediction-summary {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-12);
  padding: var(--space-12);
  background: rgba(24, 144, 255, 0.1);
  border-radius: 8px;
}

.rul-display {
  text-align: center;
}

.rul-value {
  display: block;
  font-size: 2rem;
  font-weight: 700;
  color: #1890ff;
}

.rul-unit {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.health-status {
  padding: var(--space-4) var(--space-12);
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  text-transform: uppercase;
}

.health-status.healthy {
  background: rgba(82, 196, 26, 0.2);
  color: var(--color-success);
}

.health-status.degraded {
  background: rgba(250, 173, 20, 0.2);
  color: var(--color-warning);
}

.health-status.critical {
  background: rgba(245, 34, 45, 0.2);
  color: var(--color-error);
}

.prediction-details {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-8);
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.03);
  border-radius: 6px;
  font-size: 0.875rem;
}

.label {
  color: var(--color-text-secondary);
}

.value {
  font-weight: 500;
  color: var(--color-text-primary);
}

.no-data {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 200px;
  text-align: center;
  gap: var(--space-8);
}

.info-icon {
  width: 2rem;
  height: 2rem;
  opacity: 0.5;
}

.anomaly-summary {
  display: flex;
  justify-content: space-between;
  margin-bottom: var(--space-12);
}

.risk-score, .anomaly-count {
  text-align: center;
  padding: var(--space-12);
  border-radius: 8px;
}

.risk-score {
  background: rgba(250, 140, 22, 0.1);
  flex: 1;
  margin-right: var(--space-8);
}

.anomaly-count {
  background: rgba(255, 255, 255, 0.05);
  min-width: 120px;
}

.score-label, .count-label {
  display: block;
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin-bottom: var(--space-2);
}

.score-value {
  display: block;
  font-size: 1.25rem;
  font-weight: 700;
}

.score-value.critical {
  color: var(--color-error);
}

.score-value.high {
  color: #fa8c16;
}

.score-value.medium {
  color: var(--color-warning);
}

.score-value.low {
  color: var(--color-success);
}

.count {
  display: block;
  font-size: 2rem;
  font-weight: 700;
  color: var(--primary-color);
}

.recent-anomalies h4 {
  margin: 0 0 var(--space-8) 0;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.no-anomalies {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-12);
  background: rgba(82, 196, 26, 0.1);
  border-radius: 6px;
  color: var(--color-success);
  font-size: 0.875rem;
}

.success-icon {
  width: 1rem;
  height: 1rem;
}

.anomalies-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.anomaly-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-8);
  border-radius: 6px;
  font-size: 0.8rem;
}

.anomaly-item.severity-critical {
  background: rgba(245, 34, 45, 0.1);
}

.anomaly-item.severity-high {
  background: rgba(250, 140, 22, 0.1);
}

.anomaly-item.severity-medium {
  background: rgba(250, 173, 20, 0.1);
}

.anomaly-item.severity-low {
  background: rgba(82, 196, 26, 0.1);
}

.anomaly-metric {
  font-weight: 500;
  color: var(--color-text-primary);
}

.anomaly-value {
  font-weight: 600;
  color: var(--color-text-primary);
}

.anomaly-time {
  color: var(--color-text-secondary);
  font-size: 0.75rem;
}

.more-anomalies {
  text-align: center;
  padding: var(--space-8);
  font-size: 0.8rem;
  color: var(--color-text-secondary);
  font-style: italic;
}

.recommendation-summary {
  display: flex;
  justify-content: space-between;
  margin-bottom: var(--space-12);
}

.priority, .recommendation-type {
  padding: var(--space-12);
  border-radius: 8px;
}

.priority {
  background: rgba(114, 46, 209, 0.1);
  flex: 1;
  margin-right: var(--space-8);
}

.recommendation-type {
  background: rgba(255, 255, 255, 0.05);
  min-width: 150px;
}

.priority-label, .type-label {
  display: block;
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  margin-bottom: var(--space-2);
}

.priority-value {
  display: block;
  font-size: 1rem;
  font-weight: 700;
  color: #722ed1;
}

.priority-value.CRITICAL {
  color: var(--color-error);
}

.priority-value.HIGH {
  color: #fa8c16;
}

.priority-value.MEDIUM {
  color: var(--color-warning);
}

.priority-value.LOW {
  color: var(--color-success);
}

.type-value {
  display: block;
  font-weight: 600;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.recommendation-actions {
  display: flex;
  gap: var(--space-8);
  margin-top: var(--space-12);
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
}

.action-button {
  justify-content: center;
}

.last-updated {
  text-align: center;
  padding: var(--space-12);
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .header-actions {
    min-width: auto;
  }
  
  .overview-header {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .health-indicators {
    grid-template-columns: 1fr;
  }
  
  .dashboard-grid {
    grid-template-columns: 1fr;
  }
  
  .prediction-summary {
    flex-direction: column;
    gap: var(--space-8);
    text-align: center;
  }
  
  .anomaly-summary {
    flex-direction: column;
    gap: var(--space-8);
  }
  
  .recommendation-summary {
    flex-direction: column;
    gap: var(--space-8);
  }
  
  .recommendation-actions {
    flex-direction: column;
  }
  
  .quick-actions .action-button {
    width: 100%;
  }
}
</style>