<script setup lang="ts">
import { computed, useCssModule } from 'vue'

type StatusVariant = 'operational' | 'warning' | 'critical' | 'maintenance' | 'offline' | 'unknown'
  | 'success' | 'danger' | 'info' | 'default'

const props = defineProps<{
  status: string
  dot?: boolean
  size?: 'xs' | 'sm' | 'md'
}>()

const styles = useCssModule()

const normalized = computed<StatusVariant>(() => {
  const s = props.status?.toLowerCase?.() ?? 'unknown'
  const map: Record<string, StatusVariant> = {
    operational: 'operational',
    running: 'operational',
    active: 'operational',
    ok: 'operational',
    success: 'operational',
    warning: 'warning',
    degraded: 'warning',
    maintenance: 'maintenance',
    critical: 'critical',
    error: 'critical',
    offline: 'offline',
    stopped: 'offline',
    idle: 'offline',
    info: 'info',
  }
  return map[s] ?? 'unknown'
})

const label = computed(() => {
  const s = props.status ?? 'Unknown'
  if (typeof s === 'string') return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase()
  return 'Unknown'
})
</script>

<template>
  <span :class="[styles['pill'], styles[`pill--${normalized}`], styles[`pill--${size ?? 'sm'}`]]">
    <span v-if="dot !== false" :class="[styles['dot'], styles[`dot--${normalized}`]]" />
    {{ label }}
  </span>
</template>

<style module>
.pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  border-radius: var(--radius-full);
  font-weight: 500;
  letter-spacing: 0.02em;
  white-space: nowrap;
  border: 1px solid transparent;
}

.pill--xs {
  font-size: 10px;
  padding: 2px 7px;
}

.pill--sm {
  font-size: var(--font-size-xs);
  padding: 3px 9px;
}

.pill--md {
  font-size: var(--font-size-sm);
  padding: var(--space-4) var(--space-12);
}

/* Status dot */
.dot {
  width: 6px;
  height: 6px;
  border-radius: var(--radius-full);
  flex-shrink: 0;
}

/* Variants */
.pill--operational {
  background: var(--color-success-muted);
  color: var(--color-success);
  border-color: rgba(34, 197, 94, 0.2);
}
.dot--operational { background: var(--color-success); animation: blink 2s ease-in-out infinite; }

.pill--warning {
  background: var(--color-warning-muted);
  color: var(--color-warning);
  border-color: rgba(245, 158, 11, 0.2);
}
.dot--warning { background: var(--color-warning); }

.pill--critical {
  background: var(--color-danger-muted);
  color: var(--color-danger);
  border-color: rgba(239, 68, 68, 0.2);
}
.dot--critical { background: var(--color-danger); animation: blink 1s ease-in-out infinite; }

.pill--maintenance {
  background: var(--color-info-muted);
  color: var(--color-telemetry);
  border-color: rgba(56, 189, 248, 0.2);
}
.dot--maintenance { background: var(--color-telemetry); }

.pill--offline {
  background: rgba(71, 85, 105, 0.15);
  color: var(--color-text-muted);
  border-color: rgba(71, 85, 105, 0.2);
}
.dot--offline { background: var(--color-text-muted); }

.pill--info {
  background: var(--color-info-muted);
  color: var(--color-telemetry);
  border-color: rgba(56, 189, 248, 0.2);
}
.dot--info { background: var(--color-telemetry); }

.pill--unknown, .pill--default {
  background: rgba(71, 85, 105, 0.12);
  color: var(--color-text-muted);
  border-color: var(--color-border-subtle);
}
.dot--unknown, .dot--default { background: var(--color-text-muted); }

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.35; }
}
</style>
