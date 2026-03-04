<script setup lang="ts">
import { ref, onMounted, useCssModule } from 'vue'
import { 
  Activity, 
  Terminal, 
  AlertTriangle, 
  CheckCircle2, 
  Info,
  Clock,
  ChevronRight
} from 'lucide-vue-next'

interface ActivityLog {
  id: string
  type: 'info' | 'success' | 'warning' | 'error'
  message: string
  timestamp: string
  source: string
}

const styles = useCssModule()
const logs = ref<ActivityLog[]>([
  { id: '1', type: 'info', message: 'Neural cluster synchronization initialized', timestamp: new Date().toISOString(), source: 'SYSTEM' },
  { id: '2', type: 'success', message: 'Asset NODE-01-CNC verified with 99.8% integrity', timestamp: new Date(Date.now() - 50000).toISOString(), source: 'VALIDATOR' },
  { id: '3', type: 'warning', message: 'Thermal drift detected in sector 4', timestamp: new Date(Date.now() - 120000).toISOString(), source: 'SENSOR_ARRAY' },
  { id: '4', type: 'error', message: 'Protocol handshake failed for remote gateway', timestamp: new Date(Date.now() - 300000).toISOString(), source: 'GATEWAY' },
  { id: '5', type: 'info', message: 'Maintenance cycle scheduled for ASSEMBLY-LINE-A', timestamp: new Date(Date.now() - 600000).toISOString(), source: 'SCHEDULER' }
])

function formatTime(iso: string) {
  const date = new Date(iso)
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}
</script>

<template>
  <div :class="styles['activity-panel']">
    <div :class="styles['header']">
      <div :class="styles['title-group']">
        <Terminal :width="14" :height="14" :class="styles['icon-primary']" />
        <h3 :class="styles['title']">System Stream</h3>
      </div>
      <div :class="styles['live-indicator']">
        <div :class="styles['pulse']" />
        <span>LIVE</span>
      </div>
    </div>

    <div :class="styles['feed']">
      <div 
        v-for="log in logs" 
        :key="log.id" 
        :class="[styles['log-item'], styles[`log-item--${log.type}`]]"
      >
        <div :class="styles['log-header']">
          <div :class="styles['log-meta']">
            <span :class="styles['log-source']">[{{ log.source }}]</span>
            <span :class="styles['log-time']">{{ formatTime(log.timestamp) }}</span>
          </div>
          <component 
            :is="log.type === 'error' ? AlertTriangle : log.type === 'success' ? CheckCircle2 : log.type === 'warning' ? Info : Activity" 
            :width="12" 
            :height="12" 
            :class="styles['log-icon']"
          />
        </div>
        <p :class="styles['log-message']">{{ log.message }}</p>
      </div>
    </div>

    <button :class="styles['view-all']">
      <span>View Full Protocol History</span>
      <ChevronRight :width="12" :height="12" />
    </button>
  </div>
</template>

<style module>
.activity-panel {
  display: flex;
  flex-direction: column;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  overflow: hidden;
  height: 100%;
  box-shadow: var(--shadow-card);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-16) var(--space-20);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-depth-0);
}

.title-group {
  display: flex;
  align-items: center;
  gap: var(--space-10);
}

.icon-primary { color: var(--color-primary); }

.title {
  margin: 0;
  font-size: var(--font-size-xs);
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.1em;
  color: var(--color-text-primary);
}

.live-indicator {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: 9px;
  font-weight: 900;
  color: var(--color-success);
  letter-spacing: 0.05em;
}

.pulse {
  width: 6px;
  height: 6px;
  background: currentColor;
  border-radius: 50%;
  box-shadow: 0 0 8px currentColor;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.5); opacity: 0.4; }
  100% { transform: scale(1); opacity: 1; }
}

.feed {
  flex: 1;
  overflow-y: auto;
  padding: var(--space-12);
  display: flex;
  flex-direction: column;
  gap: var(--space-8);
  scrollbar-width: none;
}

.feed::-webkit-scrollbar { display: none; }

.log-item {
  padding: var(--space-12) var(--space-16);
  background: var(--color-depth-0);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  transition: all var(--transition-normal);
}

.log-item:hover {
  border-color: var(--color-border);
  background: var(--color-surface-alt);
  transform: translateX(2px);
}

.log-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: var(--space-6);
}

.log-meta {
  display: flex;
  gap: var(--space-8);
  align-items: center;
}

.log-source {
  font-family: var(--font-mono);
  font-size: 9px;
  font-weight: 800;
  color: var(--color-text-dim);
}

.log-time {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-muted);
}

.log-message {
  margin: 0;
  font-size: 11px;
  color: var(--color-text-secondary);
  line-height: 1.5;
  font-weight: 500;
  letter-spacing: var(--font-tracking-tight);
}

.log-icon { color: var(--color-text-dim); }

.log-item--error { border-left: 2px solid var(--color-danger); }
.log-item--error .log-icon { color: var(--color-danger); }
.log-item--error:hover { background: var(--color-danger-muted); }

.log-item--warning { border-left: 2px solid var(--color-warning); }
.log-item--warning .log-icon { color: var(--color-warning); }
.log-item--warning:hover { background: var(--color-warning-muted); }

.log-item--success { border-left: 2px solid var(--color-success); }
.log-item--success .log-icon { color: var(--color-success); }
.log-item--success:hover { background: var(--color-success-muted); }

.log-item--info { border-left: 2px solid var(--color-primary); }
.log-item--info .log-icon { color: var(--color-primary); }
.log-item--info:hover { background: var(--color-primary-muted); }

.view-all {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-8);
  padding: var(--space-12);
  background: var(--color-depth-0);
  border: none;
  border-top: 1px solid var(--color-border-subtle);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.view-all:hover {
  background: var(--color-surface-alt);
  color: var(--color-primary);
}
</style>
