<template>
  <div :class="styles['ml-container']">
    <!-- Intelligent Header -->
    <header :class="styles['page-header']">
      <div :class="styles['header-main']">
        <div :class="styles['eyebrow']">
          <Binary :width="14" :height="14" />
          <span>Model Registry & Lifecycle</span>
        </div>
        <h1 :class="styles['title']">AI Model Governance</h1>
        <p :class="styles['description']">End-to-end lifecycle orchestration for predictive neural networks</p>
      </div>

      <div :class="styles['header-actions']">
        <div :class="styles['tab-strip']">
          <button 
            v-for="tab in ['models', 'register', 'compare']" 
            :key="tab"
            :class="[styles['tab-item'], activeTab === tab && styles['tab-item--active']]"
            @click="activeTab = tab"
          >
            {{ tab.charAt(0).toUpperCase() + tab.slice(1) }}
          </button>
        </div>
      </div>
    </header>

    <!-- Artifact Bento Grid -->
    <div :class="styles['artifact-grid']">
      <UiCard v-for="kpi in kpiCards" :key="kpi.label" variant="glass" hover padding="md" :class="styles['artifact-card']">
        <div :class="styles['artifact-inner']">
          <div :class="[styles['artifact-icon'], styles[`artifact-icon--${kpi.variant}`]]">
            <component :is="kpi.icon" :width="20" :height="20" />
          </div>
          <div :class="styles['artifact-data']">
            <span :class="styles['artifact-label']">{{ kpi.label }}</span>
            <div :class="styles['artifact-value']">{{ kpi.value }}</div>
          </div>
        </div>
      </UiCard>
    </div>

    <!-- Main Workspace -->
    <div :class="styles['workspace']">
      <!-- Model Explorer -->
      <div v-if="activeTab === 'models'" :class="styles['explorer-layout']">
        <div :class="styles['explorer-sidebar']">
          <UiCard variant="default" padding="none" :class="styles['list-card']">
            <template #header>
              <div :class="styles['sidebar-header']">
                <h3 :class="styles['sidebar-title']">Neural Artifacts</h3>
                <UiButton variant="ghost" size="sm" @click="loadModels"><RefreshCw :width="14" :height="14" /></UiButton>
              </div>
            </template>
            <div :class="styles['model-list']">
              <div 
                v-for="model in models" 
                :key="model.id"
                :class="[styles['model-item'], selectedModel?.id === model.id && styles['model-item--selected']]"
                @click="selectedModel = model"
              >
                <div :class="styles['model-item-main']">
                  <span :class="styles['model-name']">{{ model.modelType }}</span>
                  <UiBadge :variant="getStatusVariant(model.status)" size="sm">{{ model.status }}</UiBadge>
                </div>
                <div :class="styles['model-item-meta']">
                  <span>v{{ model.version }}</span>
                  <span :class="styles['dot-sep']" />
                  <span>{{ new Date(model.createdAt).toLocaleDateString() }}</span>
                </div>
              </div>
            </div>
          </UiCard>
        </div>

        <div :class="styles['explorer-content']">
          <template v-if="selectedModel">
            <UiCard variant="default" padding="lg" hover :class="styles['detail-card']">
              <template #header>
                <div :class="styles['detail-header']">
                  <div :class="styles['detail-title-wrap']">
                    <h2 :class="styles['detail-title']">{{ selectedModel.modelType }} <span :class="styles['version-tag']">v{{ selectedModel.version }}</span></h2>
                    <p :class="styles['detail-sub']">ID: {{ selectedModel.id }}</p>
                  </div>
                  <div :class="styles['detail-actions']">
                    <UiButton variant="outline" size="sm" @click="promoting = true">Promote Version</UiButton>
                  </div>
                </div>
              </template>

              <div :class="styles['detail-body']">
                <div :class="styles['metrics-grid']">
                  <div v-for="(val, key) in selectedModel.metrics" :key="key" :class="styles['metric-box']">
                    <span :class="styles['metric-key']">{{ key }}</span>
                    <span :class="styles['metric-val']">{{ typeof val === 'number' ? val.toFixed(4) : val }}</span>
                  </div>
                </div>

                <div :class="styles['chart-section']">
                  <h4 :class="styles['section-title']">Performance Trajectory</h4>
                  <div ref="performanceChartRef" :class="styles['chart-canvas']" />
                </div>
              </div>
            </UiCard>
          </template>
        </div>
      </div>

      <!-- Registration Panel -->
      <div v-if="activeTab === 'register'" :class="styles['registration-view']">
        <UiCard variant="default" padding="lg" :class="styles['form-card']">
          <template #header>
            <h3 :class="styles['card-title']">Register Neural Artifact</h3>
          </template>
          <div :class="styles['form-grid']">
            <div :class="styles['form-group']">
              <label :class="styles['field-label']">Model Domain</label>
              <UiSelect v-model="modelType">
                <option v-for="opt in modelTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
              </UiSelect>
            </div>
            <UiInput v-model="modelPath" label="Artifact Repository Path" placeholder="s3://models/rul-v4-final.onnx" />
            <UiInput v-model="trainingMetrics" label="Telemetry Metrics (JSON)" type="textarea" placeholder='{"accuracy": 0.985}' />
            <div :class="styles['form-footer']">
              <UiButton variant="primary" :loading="registering" @click="registerModel">Deploy to Staging</UiButton>
            </div>
          </div>
        </UiCard>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, useCssModule, shallowRef } from 'vue'
import UiCard from '@/components/ui/UiCard.vue'
import UiButton from '@/components/ui/UiButton.vue'
import UiInput from '@/components/ui/UiInput.vue'
import UiSelect from '@/components/ui/UiSelect.vue'
import UiBadge from '@/components/ui/UiBadge.vue'
import { useToast } from '@/composables/useToast'
import { Package, Zap, Binary, Layers, RefreshCw, GitBranch, ShieldCheck, Database, Search } from 'lucide-vue-next'
import { modelLifecycleService } from '@/services/modelLifecycle.service'

const styles = useCssModule()
const toast = useToast()

import * as echarts from 'echarts/core'
import { LineChart, PieChart } from 'echarts/charts'
import { GridComponent, TooltipComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'

echarts.use([LineChart, PieChart, GridComponent, TooltipComponent, CanvasRenderer])

// State
const activeTab = ref('models')
const registering = ref(false)
const models = ref<any[]>([])
const selectedModel = ref<any>(null)
const performanceChartRef = ref<HTMLDivElement | null>(null)
const chartInstances = shallowRef<Record<string, echarts.ECharts>>({})

// Form
const modelType = ref('RUL')
const modelPath = ref('')
const trainingMetrics = ref('')

const modelTypeOptions = [
  { label: 'Predictive RUL', value: 'RUL' },
  { label: 'Anomaly Forensics', value: 'Anomaly' },
  { label: 'Classification Node', value: 'Classification' }
]

const kpiCards = computed(() => [
  { label: 'Artifact Vault', value: models.value.length, icon: Package, variant: 'primary' },
  { label: 'Live Inference', value: models.value.filter(m => m.status === 'Production').length, icon: Zap, variant: 'success' },
  { label: 'Active R&D', value: models.value.filter(m => m.status === 'Development').length, icon: Binary, variant: 'info' },
  { label: 'Integrity Failures', value: '0', icon: ShieldCheck, variant: 'danger' },
])

const loadModels = async () => {
  models.value = await modelLifecycleService.fetchModelLifecycles()
  if (models.value.length > 0 && !selectedModel.value) selectedModel.value = models.value[0]
  setTimeout(() => renderPerformance(), 50)
}

const renderPerformance = () => {
  if (!performanceChartRef.value || !selectedModel.value) return
  if (!chartInstances.value.perf) chartInstances.value.perf = echarts.init(performanceChartRef.value, 'hub-dark')
  
  chartInstances.value.perf.setOption({
    backgroundColor: 'transparent',
    tooltip: { trigger: 'axis' },
    grid: { left: '3%', right: '4%', top: '10%', bottom: '5%', containLabel: true },
    xAxis: { type: 'category', data: ['Epoch 1', 'Epoch 2', 'Epoch 3', 'Epoch 4', 'Epoch 5'], axisLine: { show: false } },
    yAxis: { type: 'value', splitLine: { lineStyle: { type: 'dashed', color: 'rgba(255,255,255,0.03)' } } },
    series: [{
      type: 'line',
      smooth: true,
      data: [0.65, 0.78, 0.84, 0.91, 0.95],
      lineStyle: { width: 3, color: '#10B981' },
      symbol: 'circle',
      itemStyle: { color: '#10B981' }
    }]
  })
}

const getStatusVariant = (s: string) => {
  const status = s.toLowerCase()
  if (status === 'production') return 'success'
  if (status === 'staging') return 'primary'
  if (status === 'development') return 'info'
  return 'warning'
}

const registerModel = async () => {
  registering.value = true
  try {
    await modelLifecycleService.registerModelVersion({ modelType: modelType.value, modelPath: modelPath.value, metrics: {} })
    toast.success('Neural artifact committed to registry')
    await loadModels()
    activeTab.value = 'models'
  } finally { registering.value = false }
}

onMounted(() => loadModels())
</script>

<style module>
.ml-container {
  display: flex;
  flex-direction: column;
  gap: var(--space-32);
  max-width: 1400px;
  margin: 0 auto;
}

/* Header */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  padding-bottom: var(--space-24);
  border-bottom: 1px solid var(--color-border-subtle);
}

.eyebrow {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.15em;
  color: var(--color-primary);
  margin-bottom: var(--space-6);
}

.title {
  font-size: var(--font-size-3xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
}

.description {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
}

.tab-strip {
  display: flex;
  background: var(--color-depth-1);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-subtle);
}

.tab-item {
  padding: var(--space-6) var(--space-16);
  border-radius: var(--radius-md);
  font-size: var(--font-size-xs);
  font-weight: 600;
  color: var(--color-text-muted);
  background: transparent;
  border: none;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.tab-item--active {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

/* Artifact Grid */
.artifact-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-16);
}

.artifact-inner {
  display: flex;
  align-items: center;
  gap: var(--space-16);
}

.artifact-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
}

.artifact-icon--primary { color: var(--color-primary); }
.artifact-icon--success { color: var(--color-success); }
.artifact-icon--info { color: var(--color-telemetry); }
.artifact-icon--danger { color: var(--color-danger); }

.artifact-data {
  display: flex;
  flex-direction: column;
}

.artifact-label {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
}

.artifact-value {
  font-size: var(--font-size-xl);
  font-weight: 800;
  color: var(--color-text-primary);
}

/* Workspace */
.workspace {
  min-height: 600px;
}

.explorer-layout {
  display: grid;
  grid-template-columns: 320px 1fr;
  gap: var(--space-24);
}

.model-list {
  display: flex;
  flex-direction: column;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-4);
}

.sidebar-title {
  font-size: var(--font-size-sm);
  font-weight: 700;
  color: var(--color-text-secondary);
  margin: 0;
}

.model-item {
  padding: var(--space-12) var(--space-20);
  border-bottom: 1px solid var(--color-border-subtle);
  cursor: pointer;
  transition: all var(--transition-fast);
}

.model-item:hover {
  background: rgba(255, 255, 255, 0.02);
}

.model-item--selected {
  background: var(--color-surface-elevated);
  border-left: 2px solid var(--color-primary);
}

.model-item-main {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-4);
}

.model-name {
  font-weight: 700;
  color: var(--color-text-primary);
  font-size: var(--font-size-sm);
}

.model-item-meta {
  font-size: 10px;
  color: var(--color-text-dim);
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-weight: 600;
}

.dot-sep {
  width: 2px;
  height: 2px;
  border-radius: 50%;
  background: currentColor;
}

/* Detail Card */
.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.detail-title {
  font-size: var(--font-size-xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
}

.version-tag {
  font-size: var(--font-size-sm);
  color: var(--color-primary);
  font-family: var(--font-mono);
  margin-left: var(--space-8);
}

.detail-sub {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-dim);
  margin: var(--space-2) 0 0;
}

.metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-16);
  margin-bottom: var(--space-32);
}

.metric-box {
  background: var(--color-depth-1);
  padding: var(--space-12);
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.metric-key {
  font-size: 9px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-dim);
}

.metric-val {
  font-size: var(--font-size-md);
  font-weight: 800;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
}

.chart-canvas {
  height: 340px;
  width: 100%;
}

.section-title {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--color-text-dim);
  letter-spacing: 0.08em;
  margin-bottom: var(--space-20);
}

/* Form */
.form-grid {
  display: flex;
  flex-direction: column;
  gap: var(--space-24);
  max-width: 800px;
}

.field-label {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-dim);
  margin-bottom: var(--space-8);
  display: block;
}

@media (max-width: 1200px) {
  .artifact-grid { grid-template-columns: repeat(2, 1fr); }
  .explorer-layout { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
  .artifact-grid { grid-template-columns: 1fr; }
}
</style>
