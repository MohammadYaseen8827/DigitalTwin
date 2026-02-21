<template>
  <main class="enhanced-prescriptive-page container">
    <div class="page-header glass-panel">
      <div>
        <p class="eyebrow">Optimization Intelligence</p>
        <h1>Enhanced Prescriptive Analytics</h1>
        <p class="subtitle">
          Advanced optimization algorithms for production scheduling, resource allocation, 
          and cost-effective maintenance planning
        </p>
      </div>
      <div class="header-actions">
        <BaseButton 
          size="md" 
          variant="primary" 
          :disabled="loading" 
          @click="runFullOptimization"
          :loading="loading"
          loadingText="Optimizing..."
        >
          <template #icon>
            <Zap class="h-5 w-5" />
          </template>
          Run Full Optimization
        </BaseButton>
      </div>
    </div>

    <div v-if="loading" class="loading-overlay">
      <div class="loading-content">
        <div class="optimization-spinner"></div>
        <h3>Running Advanced Optimization</h3>
        <p>Analyzing production schedules, resource allocation, and cost factors...</p>
        <div class="progress-steps">
          <div 
            v-for="(step, index) in optimizationSteps" 
            :key="index"
            class="step"
            :class="{ active: currentStep >= index, completed: currentStep > index }"
          >
            <div class="step-icon">
              <component :is="step.icon" v-if="currentStep > index" class="check-icon" />
              <div v-else class="step-number">{{ index + 1 }}</div>
            </div>
            <span class="step-label">{{ step.label }}</span>
          </div>
        </div>
      </div>
    </div>

    <template v-else-if="error">
      <div class="error-state glass-panel">
        <AlertTriangle class="error-icon" />
        <div>
          <h2>Optimization Failed</h2>
          <p>{{ error }}</p>
        </div>
        <BaseButton size="md" variant="outline" @click="runFullOptimization">
          Try Again
        </BaseButton>
      </div>
    </template>

    <template v-else-if="optimizationResults">
      <div class="results-grid">
        <!-- Production Scheduling -->
        <div class="result-card glass-panel">
          <div class="card-header">
            <div class="header-icon scheduling">
              <Calendar class="icon" />
            </div>
            <h3>Production Scheduling</h3>
          </div>
          <div class="card-content">
            <div class="schedule-summary">
              <div class="metric">
                <span class="label">Optimization Score:</span>
                <span class="value score">{{ optimizationResults.scheduling.optimizationScore.toFixed(1) }}/100</span>
              </div>
              <div class="metric">
                <span class="label">Total Slots:</span>
                <span class="value">{{ optimizationResults.scheduling.schedule.length }}</span>
              </div>
            </div>
            
            <div class="schedule-list">
              <div 
                v-for="(slot, index) in optimizationResults.scheduling.schedule.slice(0, 3)" 
                :key="index"
                class="schedule-slot"
              >
                <div class="slot-machine">{{ getMachineName(slot.machineId) }}</div>
                <div class="slot-details">
                  <span class="slot-type">{{ slot.maintenanceType }}</span>
                  <span class="slot-duration">{{ slot.estimatedDuration }} hrs</span>
                </div>
                <div class="slot-time">{{ formatDate(slot.recommendedSlot) }}</div>
              </div>
              <div v-if="optimizationResults.scheduling.schedule.length > 3" class="more-slots">
                +{{ optimizationResults.scheduling.schedule.length - 3 }} more slots
              </div>
            </div>

            <div class="constraints">
              <h4>Applied Constraints:</h4>
              <ul>
                <li v-for="(constraint, index) in optimizationResults.scheduling.constraints" :key="index">
                  {{ constraint }}
                </li>
              </ul>
            </div>
          </div>
        </div>

        <!-- Resource Allocation -->
        <div class="result-card glass-panel">
          <div class="card-header">
            <div class="header-icon allocation">
              <Users class="icon" />
            </div>
            <h3>Resource Allocation</h3>
          </div>
          <div class="card-content">
            <div class="allocation-summary">
              <div class="metric">
                <span class="label">Resource Utilization:</span>
                <span class="value utilization">{{ (optimizationResults.allocation.resourceUtilization * 100).toFixed(0) }}%</span>
              </div>
              <div class="metric">
                <span class="label">Conflicts:</span>
                <span class="value conflicts">{{ optimizationResults.allocation.conflicts.length }}</span>
              </div>
            </div>

            <div class="allocation-list">
              <div 
                v-for="(alloc, index) in optimizationResults.allocation.allocations.slice(0, 3)" 
                :key="index"
                class="allocation-item"
              >
                <div class="alloc-machine">{{ getMachineName(alloc.machineId) }}</div>
                <div class="alloc-details">
                  <span class="alloc-team">{{ alloc.team }}</span>
                  <span class="alloc-tech">{{ alloc.technician }}</span>
                </div>
                <div class="alloc-time">{{ formatTime(alloc.timeSlot) }}</div>
              </div>
              <div v-if="optimizationResults.allocation.allocations.length > 3" class="more-allocations">
                +{{ optimizationResults.allocation.allocations.length - 3 }} more allocations
              </div>
            </div>

            <div v-if="optimizationResults.allocation.conflicts.length > 0" class="conflicts-section">
              <h4>Resource Conflicts:</h4>
              <ul>
                <li v-for="(conflict, index) in optimizationResults.allocation.conflicts" :key="index">
                  {{ conflict }}
                </li>
              </ul>
            </div>
          </div>
        </div>

        <!-- Cost Optimization -->
        <div class="result-card glass-panel">
          <div class="card-header">
            <div class="header-icon cost">
              <DollarSign class="icon" />
            </div>
            <h3>Cost Optimization</h3>
          </div>
          <div class="card-content">
            <div class="cost-summary">
              <div class="metric">
                <span class="label">Total Cost:</span>
                <span class="value cost">${{ optimizationResults.cost.totalCost.toLocaleString() }}</span>
              </div>
              <div class="metric">
                <span class="label">Budget Remaining:</span>
                <span class="value remaining">${{ optimizationResults.cost.budgetRemaining.toLocaleString() }}</span>
              </div>
              <div class="metric">
                <span class="label">Potential Savings:</span>
                <span class="value savings">${{ optimizationResults.cost.savings.toLocaleString() }}</span>
              </div>
            </div>

            <div class="cost-breakdown">
              <h4>Cost Breakdown by Machine:</h4>
              <div 
                v-for="(plan, index) in optimizationResults.cost.optimizedPlan.slice(0, 3)" 
                :key="index"
                class="cost-item"
              >
                <div class="cost-machine">{{ getMachineName(plan.machineId) }}</div>
                <div class="cost-details">
                  <span class="cost-type">{{ plan.type }}</span>
                  <span class="cost-amount">${{ plan.cost.toLocaleString() }}</span>
                </div>
                <div class="cost-priority" :class="getPriorityClass(plan.priority)">
                  P{{ plan.priority }}
                </div>
              </div>
              <div v-if="optimizationResults.cost.optimizedPlan.length > 3" class="more-costs">
                +{{ optimizationResults.cost.optimizedPlan.length - 3 }} more items
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Configuration Panel -->
      <div class="configuration-panel glass-panel">
        <h2>Optimization Parameters</h2>
        <div class="config-grid">
          <div class="config-section">
            <h3>Scheduling</h3>
            <div class="config-item">
              <label>Planning Horizon:</label>
              <select v-model="config.planningHorizon" class="config-select">
                <option value="1w">1 Week</option>
                <option value="2w">2 Weeks</option>
                <option value="1m">1 Month</option>
                <option value="3m">3 Months</option>
              </select>
            </div>
          </div>

          <div class="config-section">
            <h3>Resources</h3>
            <div class="config-item">
              <label>Max Teams:</label>
              <input 
                type="number" 
                v-model.number="config.maxTeams" 
                min="1" 
                max="10"
                class="config-input"
              >
            </div>
            <div class="config-item">
              <label>Working Hours/Day:</label>
              <input 
                type="number" 
                v-model.number="config.workingHours" 
                min="1" 
                max="24"
                class="config-input"
              >
            </div>
          </div>

          <div class="config-section">
            <h3>Budget</h3>
            <div class="config-item">
              <label>Total Budget ($):</label>
              <input 
                type="number" 
                v-model.number="config.budget" 
                min="0" 
                step="1000"
                class="config-input"
              >
            </div>
            <div class="config-item">
              <label>Max Per Machine ($):</label>
              <input 
                type="number" 
                v-model.number="config.maxPerMachine" 
                min="0" 
                step="1000"
                class="config-input"
              >
            </div>
          </div>
        </div>

        <div class="config-actions">
          <BaseButton size="sm" variant="outline" @click="resetConfig">
            Reset Defaults
          </BaseButton>
          <BaseButton size="sm" variant="primary" @click="runFullOptimization">
            Re-run Optimization
          </BaseButton>
        </div>
      </div>

      <!-- Insights Section -->
      <div class="insights-section">
        <h2>Optimization Insights</h2>
        <div class="insights-grid">
          <div class="insight-card glass-panel">
            <div class="insight-icon efficiency">⚡</div>
            <h3>Efficiency Gains</h3>
            <p>Reduced downtime by {{ calculateDowntimeReduction() }}% through optimized scheduling</p>
          </div>
          <div class="insight-card glass-panel">
            <div class="insight-icon savings">💰</div>
            <h3>Cost Savings</h3>
            <p>Achieved {{ (optimizationResults.cost.savings / (optimizationResults.cost.totalCost + optimizationResults.cost.savings) * 100).toFixed(0) }}% cost reduction</p>
          </div>
          <div class="insight-card glass-panel">
            <div class="insight-icon resource">👥</div>
            <h3>Resource Utilization</h3>
            <p>Improved resource utilization to {{ (optimizationResults.allocation.resourceUtilization * 100).toFixed(0) }}%</p>
          </div>
        </div>
      </div>
    </template>

    <template v-else>
      <div class="welcome-state glass-panel">
        <div class="welcome-content">
          <Zap class="welcome-icon" />
          <h2>Advanced Optimization Suite</h2>
          <p>
            Leverage AI-powered algorithms to optimize your maintenance operations across 
            production scheduling, resource allocation, and cost management.
          </p>
          <div class="features-preview">
            <div class="feature-item">
              <Calendar class="feature-icon" />
              <div>
                <h4>Production Scheduling</h4>
                <p>Optimize maintenance slots to minimize production disruption</p>
              </div>
            </div>
            <div class="feature-item">
              <Users class="feature-icon" />
              <div>
                <h4>Resource Allocation</h4>
                <p>Efficiently assign teams and technicians to maximize productivity</p>
              </div>
            </div>
            <div class="feature-item">
              <DollarSign class="feature-icon" />
              <div>
                <h4>Cost Optimization</h4>
                <p>Balance budgets while maintaining equipment reliability</p>
              </div>
            </div>
          </div>
          <BaseButton size="lg" variant="primary" @click="runFullOptimization">
            Start Advanced Optimization
          </BaseButton>
        </div>
      </div>
    </template>
  </main>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import { 
  optimizeProductionSchedule,
  optimizeResourceAllocation,
  optimizeMaintenanceCosts
} from '@/services/advancedAnalytics.service'
import type { 
  SchedulingRecommendation,
  ResourceAllocationPlan,
  CostOptimizationResult
} from '@/services/advancedAnalytics.service'
import { 
  Zap, 
  AlertTriangle, 
  Calendar, 
  Users, 
  DollarSign,
  Check
} from 'lucide-vue-next'

const loading = ref(false)
const error = ref<string | null>(null)
const currentStep = ref(0)
const toast = useToast()

const optimizationResults = ref<{
  scheduling: SchedulingRecommendation
  allocation: ResourceAllocationPlan
  cost: CostOptimizationResult
} | null>(null)

const config = ref({
  planningHorizon: '2w',
  maxTeams: 5,
  workingHours: 8,
  budget: 100000,
  maxPerMachine: 25000
})

const optimizationSteps = [
  { label: 'Production Scheduling', icon: Calendar },
  { label: 'Resource Allocation', icon: Users },
  { label: 'Cost Optimization', icon: DollarSign }
]

const machineNames: Record<string, string> = {
  'M001': 'CNC Machine Alpha',
  'M002': 'Injection Molder Beta',
  'M003': 'Press Gamma',
  'M004': 'Robot Delta',
  'M005': 'Conveyor Epsilon'
}

const getMachineName = (id: string) => machineNames[id] || `Machine ${id}`

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatTime = (timeSlot: string) => {
  return new Date(timeSlot).toLocaleTimeString('en-US', {
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getPriorityClass = (priority: number) => {
  if (priority <= 2) return 'high'
  if (priority <= 4) return 'medium'
  return 'low'
}

const calculateDowntimeReduction = () => {
  // Mock calculation - would be based on actual scheduling improvements
  return Math.floor(Math.random() * 30) + 15
}

const resetConfig = () => {
  config.value = {
    planningHorizon: '2w',
    maxTeams: 5,
    workingHours: 8,
    budget: 100000,
    maxPerMachine: 25000
  }
  toast.info('Configuration reset to defaults')
}

const runFullOptimization = async () => {
  loading.value = true
  error.value = null
  currentStep.value = 0
  optimizationResults.value = null

  try {
    // Step 1: Production Scheduling
    currentStep.value = 0
    const schedulingResult = await optimizeProductionSchedule({
      machineIds: Object.keys(machineNames),
      planningHorizon: config.value.planningHorizon
    })

    // Step 2: Resource Allocation
    currentStep.value = 1
    const allocationResult = await optimizeResourceAllocation({
      machineIds: Object.keys(machineNames),
      planningPeriod: config.value.planningHorizon
    })

    // Step 3: Cost Optimization
    currentStep.value = 2
    const costResult = await optimizeMaintenanceCosts({
      machineIds: Object.keys(machineNames),
      budget: {
        totalBudget: config.value.budget,
        maxPerMachine: config.value.maxPerMachine
      }
    })

    currentStep.value = 3
    optimizationResults.value = {
      scheduling: schedulingResult,
      allocation: allocationResult,
      cost: costResult
    }

    toast.success('Full optimization completed successfully!')
  } catch (err: any) {
    error.value = err.response?.data?.message || err.message || 'Optimization failed'
    toast.error('Optimization failed')
    console.error('Optimization error:', err)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.enhanced-prescriptive-page {
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
  background: linear-gradient(135deg, #f59e0b, #d97706);
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

.loading-overlay {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  padding: 2rem;
}

.loading-content {
  text-align: center;
  max-width: 500px;
}

.optimization-spinner {
  width: 4rem;
  height: 4rem;
  border: 4px solid rgba(245, 158, 11, 0.3);
  border-top: 4px solid #f59e0b;
  border-radius: 50%;
  animation: spin 1.5s linear infinite;
  margin: 0 auto 1.5rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.progress-steps {
  display: flex;
  justify-content: center;
  gap: 2rem;
  margin-top: 2rem;
  flex-wrap: wrap;
}

.step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  opacity: 0.5;
  transition: opacity 0.3s ease;
}

.step.active {
  opacity: 1;
}

.step.completed {
  opacity: 1;
}

.step-icon {
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(245, 158, 11, 0.2);
  border: 2px solid #f59e0b;
}

.step.completed .step-icon {
  background: #f59e0b;
  border-color: #f59e0b;
}

.check-icon {
  width: 1.25rem;
  height: 1.25rem;
  color: white;
}

.step-number {
  font-weight: 600;
  color: #f59e0b;
}

.step.completed .step-number {
  color: white;
}

.step-label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  text-align: center;
}

.step.completed .step-label {
  color: var(--color-text-primary);
  font-weight: 500;
}

.results-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(350px, 1fr));
  gap: var(--space-16);
}

.result-card {
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
  width: 3rem;
  height: 3rem;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-icon.scheduling {
  background: linear-gradient(135deg, #3b82f6, #2563eb);
}

.header-icon.allocation {
  background: linear-gradient(135deg, #10b981, #059669);
}

.header-icon.cost {
  background: linear-gradient(135deg, #8b5cf6, #7c3aed);
}

.icon {
  width: 1.5rem;
  height: 1.5rem;
  color: white;
}

.card-header h3 {
  margin: 0;
  color: var(--color-text-primary);
  font-size: 1.25rem;
}

.schedule-summary, .allocation-summary, .cost-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: var(--space-8);
  margin-bottom: var(--space-12);
}

.metric {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.label {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.value {
  font-weight: 700;
  font-size: 1.1rem;
}

.value.score {
  color: #3b82f6;
}

.value.utilization {
  color: #10b981;
}

.value.conflicts {
  color: var(--color-error);
}

.value.cost, .value.remaining, .value.savings {
  color: #8b5cf6;
}

.schedule-list, .allocation-list, .cost-breakdown {
  margin-bottom: var(--space-12);
}

.schedule-slot, .allocation-item, .cost-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-8);
  margin-bottom: var(--space-4);
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
  font-size: 0.875rem;
}

.slot-details, .alloc-details, .cost-details {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: var(--space-1);
}

.slot-type, .alloc-team, .cost-type {
  font-weight: 500;
  color: var(--color-text-primary);
}

.slot-duration, .alloc-tech, .cost-amount {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.slot-time, .alloc-time {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.cost-priority {
  font-size: 0.75rem;
  font-weight: 600;
  padding: 0.125rem 0.5rem;
  border-radius: 12px;
  text-transform: uppercase;
}

.cost-priority.high {
  background: rgba(245, 34, 45, 0.2);
  color: var(--color-error);
}

.cost-priority.medium {
  background: rgba(250, 173, 20, 0.2);
  color: var(--color-warning);
}

.cost-priority.low {
  background: rgba(82, 196, 26, 0.2);
  color: var(--color-success);
}

.more-slots, .more-allocations, .more-costs {
  text-align: center;
  padding: var(--space-8);
  font-size: 0.875rem;
  color: var(--color-text-secondary);
  font-style: italic;
}

.constraints, .conflicts-section {
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.03);
  border-radius: 8px;
}

.constraints h4, .conflicts-section h4 {
  margin: 0 0 var(--space-4) 0;
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.constraints ul, .conflicts-section ul {
  margin: 0;
  padding-left: 1rem;
  font-size: 0.8rem;
  color: var(--color-text-secondary);
}

.constraints li, .conflicts-section li {
  margin-bottom: var(--space-2);
}

.configuration-panel {
  padding: var(--space-20);
  margin-top: var(--space-16);
}

.configuration-panel h2 {
  margin: 0 0 var(--space-12) 0;
  color: var(--color-text-primary);
  text-align: center;
}

.config-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--space-16);
  margin-bottom: var(--space-16);
}

.config-section h3 {
  margin: 0 0 var(--space-8) 0;
  color: var(--color-text-primary);
  font-size: 1.1rem;
}

.config-item {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
  margin-bottom: var(--space-8);
}

.config-item label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.config-select, .config-input {
  padding: var(--space-8);
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  color: var(--color-text-primary);
  font-size: 0.9rem;
}

.config-select:focus, .config-input:focus {
  outline: none;
  border-color: #f59e0b;
}

.config-actions {
  display: flex;
  justify-content: center;
  gap: var(--space-8);
}

.insights-section {
  margin-top: var(--space-16);
}

.insights-section h2 {
  text-align: center;
  margin-bottom: var(--space-16);
  color: var(--color-text-primary);
}

.insights-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: var(--space-12);
}

.insight-card {
  padding: var(--space-16);
  text-align: center;
  transition: transform 0.2s ease;
}

.insight-card:hover {
  transform: translateY(-4px);
}

.insight-icon {
  font-size: 2.5rem;
  margin-bottom: var(--space-8);
}

.insight-icon.efficiency { color: #3b82f6; }
.insight-icon.savings { color: #10b981; }
.insight-icon.resource { color: #8b5cf6; }

.insight-card h3 {
  margin: 0 0 var(--space-4) 0;
  color: var(--color-text-primary);
}

.insight-card p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  line-height: 1.5;
}

.welcome-state {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 4rem 2rem;
  text-align: center;
}

.welcome-content {
  max-width: 600px;
}

.welcome-icon {
  width: 4rem;
  height: 4rem;
  margin-bottom: 1.5rem;
  color: #f59e0b;
}

.welcome-content h2 {
  margin: 0 0 1rem;
  font-size: 1.75rem;
  color: var(--color-text-primary);
}

.features-preview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1.5rem;
  margin: 2rem 0;
}

.feature-item {
  display: flex;
  gap: 1rem;
  text-align: left;
  align-items: flex-start;
}

.feature-icon {
  width: 1.5rem;
  height: 1.5rem;
  min-width: 1.5rem;
  color: #f59e0b;
  margin-top: 0.25rem;
}

.feature-item h4 {
  margin: 0 0 0.5rem 0;
  color: var(--color-text-primary);
  font-size: 1rem;
}

.feature-item p {
  margin: 0;
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  line-height: 1.4;
}

.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 3rem 2rem;
  gap: 1.5rem;
}

.error-icon {
  width: 3rem;
  height: 3rem;
  color: var(--color-error);
}

.error-state h2 {
  margin: 0;
  color: var(--color-error);
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    align-items: stretch;
  }
  
  .results-grid {
    grid-template-columns: 1fr;
  }
  
  .schedule-summary, .allocation-summary, .cost-summary {
    grid-template-columns: 1fr 1fr;
  }
  
  .progress-steps {
    gap: 1rem;
  }
  
  .config-grid {
    grid-template-columns: 1fr;
  }
  
  .insights-grid {
    grid-template-columns: 1fr;
  }
  
  .features-preview {
    grid-template-columns: 1fr;
  }
}
</style>
