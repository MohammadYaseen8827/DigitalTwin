<script setup lang="ts">
import { ref, computed } from 'vue';
import BaseCard from './base/BaseCard.vue';
import TrendChip from './base/TrendChip.vue';
import { ChevronDown, ChevronUp } from 'lucide-vue-next';

defineProps({
  line: {
    type: Object as () => {
      id: string;
      name: string;
      status: string;
      machineIds?: string[];
    },
    required: true,
  },
  machines: {
    type: Array as () => Array<{
      machine: {
        id: string;
        name: string;
        status: string;
      };
      prediction?: {
        score: number;
        isAnomaly: boolean;
        recommendation: string;
        predictedAt: string;
      };
    }>,
    required: true,
  },
});

const isExpanded = ref(true);

const statusVariant = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'operational':
      return 'success';
    case 'warning':
      return 'warning';
    case 'critical':
      return 'critical';
    case 'maintenance':
      return 'info';
    default:
      return 'default';
  }
};

const riskIntent = (score: number) => {
  if (score > 0.7) return 'critical';
  if (score > 0.5) return 'warning';
  return 'success';
};

const riskLabel = (score: number) => {
  if (score > 0.7) return 'High';
  if (score > 0.5) return 'Medium';
  return 'Low';
};
</script>

<template>
  <BaseCard variant="glass" class="production-line-card" :class="{ 'is-expanded': isExpanded }">
    <template #header>
      <div class="line-header" @click="isExpanded = !isExpanded">
        <div class="line-title">
          <div class="line-toggle">
            <component :is="isExpanded ? ChevronUp : ChevronDown" class="chevron" />
          </div>
          <div>
            <p class="eyebrow">Production Line</p>
            <h3>{{ line.name }}</h3>
          </div>
        </div>
        
        <div class="line-meta">
          <span class="status-badge" :class="statusVariant(line.status)">
            {{ line.status }}
          </span>
          <span class="machine-count">
            {{ machines.length }} {{ machines.length === 1 ? 'Machine' : 'Machines' }}
          </span>
        </div>
      </div>
    </template>

    <transition name="slide-fade">
      <div v-if="isExpanded" class="machine-grid">
        <div 
          v-for="machine in machines" 
          :key="machine.machine.id"
          class="machine-card"
          :class="{ 
            'has-anomaly': machine.prediction?.isAnomaly,
            [riskIntent(machine.prediction?.score || 0)]: machine.prediction
          }"
        >
          <div class="machine-header">
            <div class="machine-title">
              <p class="eyebrow">Machine</p>
              <h4>{{ machine.machine.name }}</h4>
            </div>
            <TrendChip
              v-if="machine.prediction"
              :trend="(machine.prediction.score * 100) - 50"
              :intent="riskIntent(machine.prediction.score)"
            />
          </div>

          <div class="risk-indicator">
            <div class="risk-meta">
              <div class="risk-badge" :class="riskIntent(machine.prediction?.score || 0)">
                {{ machine.prediction ? riskLabel(machine.prediction.score) : 'No data' }}
                <span v-if="machine.prediction" class="risk-percent">
                  {{ (machine.prediction.score * 100).toFixed(1) }}%
                </span>
              </div>
              <div v-if="machine.prediction?.isAnomaly" class="anomaly-badge">
                Anomaly Detected
              </div>
            </div>
            
            <div class="risk-meter">
              <div 
                class="risk-meter-fill" 
                :style="{ 
                  width: `${machine.prediction ? Math.min(100, machine.prediction.score * 100) : 0}%`,
                  '--risk-color': `var(--color-${riskIntent(machine.prediction?.score || 0)})`
                }"
              ></div>
            </div>
          </div>

          <div v-if="machine.prediction?.recommendation" class="recommendation">
            <p class="recommendation-label">Recommendation</p>
            <p class="recommendation-text">{{ machine.prediction.recommendation }}</p>
          </div>

          <div class="machine-footer">
            <div class="last-updated">
              <p class="label">Last updated</p>
              <p class="timestamp">
                {{ machine.prediction ? new Date(machine.prediction.predictedAt).toLocaleString() : '—' }}
              </p>
            </div>
            <button class="action-button">
              View Details
            </button>
          </div>
        </div>
      </div>
    </transition>
  </BaseCard>
</template>

<style scoped>
.production-line-card {
  --transition-speed: 0.3s;
  overflow: hidden;
  transition: all var(--transition-speed) ease;
}

.line-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-12) var(--space-16);
  cursor: pointer;
  user-select: none;
}

.line-title {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}

.line-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform var(--transition-speed) ease;
}

.chevron {
  width: 18px;
  height: 18px;
  color: var(--color-text-secondary);
  transition: transform 0.2s ease;
}

.is-expanded .chevron {
  transform: rotate(0);
}

:not(.is-expanded) .chevron {
  transform: rotate(-90deg);
}

.line-meta {
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: var(--font-size-xs);
  font-weight: 600;
  text-transform: capitalize;
}

.status-badge.success {
  background: color-mix(in srgb, var(--color-success) 15%, transparent);
  color: var(--color-success);
}

.status-badge.warning {
  background: color-mix(in srgb, var(--color-warning) 15%, transparent);
  color: var(--color-warning);
}

.status-badge.critical {
  background: color-mix(in srgb, var(--color-error) 15%, transparent);
  color: var(--color-error);
}

.status-badge.info {
  background: color-mix(in srgb, var(--color-info) 15%, transparent);
  color: var(--color-info);
}

.machine-count {
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.machine-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-12);
  padding: 0 var(--space-16) var(--space-16);
}

.machine-card {
  background: color-mix(in srgb, var(--color-surface) 70%, transparent);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-16);
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  gap: var(--space-12);
}

.machine-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
  border-color: var(--color-border);
}

.machine-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-8);
}

.machine-title h4 {
  margin: 2px 0 0;
  font-size: var(--font-size-lg);
  font-weight: 600;
}

.risk-indicator {
  margin-top: var(--space-4);
}

.risk-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-8);
}

.risk-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-4);
  padding: 4px 8px;
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  font-weight: 600;
}

.risk-badge.success {
  background: color-mix(in srgb, var(--color-success) 15%, transparent);
  color: var(--color-success);
}

.risk-badge.warning {
  background: color-mix(in srgb, var(--color-warning) 15%, transparent);
  color: var(--color-warning);
}

.risk-badge.critical {
  background: color-mix(in srgb, var(--color-error) 15%, transparent);
  color: var(--color-error);
}

.risk-percent {
  font-weight: 700;
  opacity: 0.9;
}

.anomaly-badge {
  padding: 4px 8px;
  border-radius: var(--radius-md);
  font-size: var(--font-size-xs);
  font-weight: 600;
  background: color-mix(in srgb, var(--color-error) 15%, transparent);
  color: var(--color-error);
}

.risk-meter {
  width: 100%;
  height: 8px;
  background: var(--color-bg-subtle);
  border-radius: 4px;
  overflow: hidden;
}

.risk-meter-fill {
  height: 100%;
  border-radius: 4px;
  background: linear-gradient(90deg, 
    var(--risk-color, var(--color-primary)), 
    color-mix(in srgb, var(--risk-color, var(--color-primary)) 70%, black)
  );
  transition: width 0.5s ease-out;
}

.recommendation {
  margin-top: var(--space-8);
  padding: var(--space-12);
  background: color-mix(in srgb, var(--color-bg-subtle) 50%, transparent);
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
}

.recommendation-label {
  margin: 0 0 4px;
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.recommendation-text {
  margin: 0;
  font-weight: 500;
  line-height: 1.4;
}

.machine-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: var(--space-8);
  border-top: 1px solid var(--color-border-subtle);
}

.last-updated .label {
  font-size: var(--font-size-xs);
  color: var(--color-text-secondary);
  margin: 0 0 2px;
}

.timestamp {
  font-size: var(--font-size-sm);
  color: var(--color-text);
  margin: 0;
}

.action-button {
  padding: 6px 12px;
  border: 1px solid var(--color-border);
  background: transparent;
  border-radius: var(--radius-md);
  font-size: var(--font-size-sm);
  color: var(--color-text);
  cursor: pointer;
  transition: all 0.2s ease;
}

.action-button:hover {
  background: var(--color-bg-subtle);
  border-color: var(--color-border-hover);
}

/* Animations */
.slide-fade-enter-active {
  transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
  transition: all 0.2s cubic-bezier(1, 0.5, 0.8, 1);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateY(-10px);
  opacity: 0;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .machine-grid {
    grid-template-columns: 1fr;
  }
  
  .line-header {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--space-8);
  }
  
  .line-meta {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
