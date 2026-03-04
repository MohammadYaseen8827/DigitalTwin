<script setup lang="ts">
import { ref, computed, watch, useCssModule } from 'vue'
import { useRouter } from 'vue-router'
import { useUIStore } from '@/stores/ui.store'
import {
  Combobox,
  ComboboxInput,
  ComboboxOptions,
  ComboboxOption
} from '@headlessui/vue'
import {
  Search, Home, BarChart3, LayoutDashboard, Server, Wrench, Bell,
  Play, Factory, Activity, Eye, TrendingUp, Settings, Archive, Database,
  Calculator, Shuffle, Cpu, Network, Layers, Cloud, Building, Plug,
  Radio, Target, Heart, FileText, History, Zap, FileBarChart, Filter,
  BrainCircuit
} from 'lucide-vue-next'

interface CommandItem {
  id: string
  name: string
  path?: string
  action?: () => void
  group: string
  icon: any
  keywords?: string[]
}

const uiStore = useUIStore()

const ALL_COMMANDS: CommandItem[] = [
  // Actions
  { id: 'toggle-sidebar', name: 'Toggle Sidebar', action: () => uiStore.toggleSidebarCollapsed(), group: 'System', icon: LayoutDashboard, keywords: ['collapse', 'expand'] },
  { id: 'toggle-theme', name: 'Toggle Dark/Light Mode', action: () => uiStore.toggleTheme(), group: 'System', icon: Zap, keywords: ['theme', 'color', 'dark', 'light'] },
  
  // Core
  { id: 'home', name: 'Home', path: '/', group: 'Core', icon: Home },
  { id: 'dashboard', name: 'Operations Dashboard', path: '/dashboard', group: 'Core', icon: BarChart3 },
  { id: 'enterprise', name: 'Enterprise Hub', path: '/enterprise-dashboard', group: 'Core', icon: LayoutDashboard },
  // Operations
  { id: 'machines', name: 'Fleet Management', path: '/machines', group: 'Operations', icon: Server, keywords: ['equipment', 'assets'] },
  { id: 'maintenance', name: 'Maintenance Schedule', path: '/maintenance', group: 'Operations', icon: Wrench, keywords: ['service', 'repair'] },
  { id: 'alerts', name: 'Anomaly Center', path: '/alerts', group: 'Operations', icon: Bell, keywords: ['notifications', 'warnings'] },
  { id: 'simulations', name: 'Predictive Simulations', path: '/simulations', group: 'Operations', icon: Play },
  { id: 'production', name: 'Line Monitoring', path: '/production-lines', group: 'Operations', icon: Factory },
  // Monitoring
  { id: 'telemetry', name: 'Live Telemetry Stream', path: '/telemetry', group: 'Monitoring', icon: Activity, keywords: ['sensors', 'data'] },
  { id: 'analytics-viz', name: 'Advanced Visual Analytics', path: '/advanced-analytics-viz', group: 'Monitoring', icon: Eye },
  { id: 'drift', name: 'Model Drift Detection', path: '/drift-detection', group: 'Monitoring', icon: TrendingUp },
  // ML/AI
  { id: 'predictive', name: 'AI Prediction Dashboard', path: '/predictive-analytics-dashboard', group: 'ML/AI', icon: BrainCircuit },
  { id: 'model-lifecycle', name: 'AI Model Lifecycle', path: '/model-lifecycle', group: 'ML/AI', icon: Layers },
  // System
  { id: 'settings', name: 'System Settings', path: '/settings', group: 'System', icon: Settings },
]

const props = defineProps<{ open: boolean }>()
const emit = defineEmits<{ (e: 'close'): void }>()

const styles = useCssModule()
const router = useRouter()
const query = ref('')
const selected = ref<CommandItem | null>(null)

const filteredCommands = computed(() => {
  const q = query.value.toLowerCase().trim()
  if (!q) return ALL_COMMANDS.slice(0, 8)
  return ALL_COMMANDS.filter(cmd => {
    const nameMatch = cmd.name.toLowerCase().includes(q)
    const groupMatch = cmd.group.toLowerCase().includes(q)
    const keywordMatch = cmd.keywords?.some(k => k.toLowerCase().includes(q))
    return nameMatch || groupMatch || keywordMatch
  }).slice(0, 12)
})

const groupedCommands = computed(() => {
  const groups: Record<string, CommandItem[]> = {}
  for (const cmd of filteredCommands.value) {
    if (!groups[cmd.group]) groups[cmd.group] = []
    groups[cmd.group].push(cmd)
  }
  return groups
})

watch(() => props.open, (val) => {
  if (!val) {
    setTimeout(() => { query.value = '' }, 200)
  }
})

watch(selected, (item) => {
  if (item) {
    if (item.action) {
      item.action()
    } else if (item.path) {
      router.push(item.path)
    }
    emit('close')
    selected.value = null
  }
})

function onClose() {
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <Transition name="cmd-overlay">
      <div v-if="open" :class="styles['overlay']" @click.self="onClose">
        <Transition name="cmd-panel">
          <div v-if="open" :class="styles['panel']">
            <Combobox v-model="selected">
              <div :class="styles['search-wrap']">
                <Search :class="styles['search-icon']" :width="16" :height="16" />
                <ComboboxInput
                  :class="styles['search-input']"
                  placeholder="Search pages, features, actions…"
                  :display-value="() => query"
                  @change="query = $event.target.value"
                  @keydown.escape="onClose"
                  autocomplete="off"
                />
                <kbd :class="styles['kbd']">ESC</kbd>
              </div>

              <ComboboxOptions static :class="styles['results']">
                <template v-if="filteredCommands.length > 0">
                  <template v-for="(items, group) in groupedCommands" :key="group">
                    <div :class="styles['result-group-label']">{{ group }}</div>
                    <ComboboxOption
                      v-for="cmd in items"
                      :key="cmd.id"
                      :value="cmd"
                      v-slot="{ active }"
                    >
                      <div :class="[styles['result-item'], active && styles['result-item--active']]">
                        <div :class="[styles['result-icon-wrap'], active && styles['result-icon-wrap--active']]">
                          <component :is="cmd.icon" :width="14" :height="14" />
                        </div>
                        <span :class="styles['result-name']">{{ cmd.name }}</span>
                        <span :class="styles['result-path']">{{ cmd.path }}</span>
                      </div>
                    </ComboboxOption>
                  </template>
                </template>
                <div v-else :class="styles['empty']">
                  No results for "<strong>{{ query }}</strong>"
                </div>
              </ComboboxOptions>

              <div :class="styles['footer']">
                <div :class="styles['hint-group']">
                  <span :class="styles['hint']"><kbd :class="styles['kbd-sm']">↑↓</kbd> Select</span>
                  <span :class="styles['hint']"><kbd :class="styles['kbd-sm']">↵</kbd> Open</span>
                </div>
                <div :class="styles['hint-group']">
                  <span :class="styles['hint']"><kbd :class="styles['kbd-sm']">ESC</kbd> Close</span>
                </div>
              </div>
            </Combobox>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<style module>
.overlay {
  position: fixed;
  inset: 0;
  z-index: 9000;
  background: rgba(5, 7, 10, 0.85);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding-top: clamp(80px, 15vh, 200px);
  padding-inline: var(--space-16);
}

.panel {
  width: 100%;
  max-width: 680px;
  background: var(--color-depth-1);
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-float);
  overflow: hidden;
  position: relative;
  display: flex;
  flex-direction: column;
}

.panel::before {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at 50% 0%, rgba(var(--color-primary-rgb), 0.1), transparent 60%);
  pointer-events: none;
  z-index: 10;
}

.search-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  padding: var(--space-20) var(--space-24);
  background: var(--color-depth-0);
  border-bottom: 1px solid var(--color-border-subtle);
  position: relative;
  z-index: 20;
}

.search-icon {
  color: var(--color-primary);
  flex-shrink: 0;
  filter: drop-shadow(0 0 10px var(--color-primary));
}

.search-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  font-size: var(--font-size-lg);
  color: var(--color-text-primary);
  font-family: var(--font-family);
  caret-color: var(--color-primary);
  font-weight: 600;
  letter-spacing: var(--font-tracking-tight);
}

.search-input::placeholder {
  color: var(--color-text-dim);
}

.kbd {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-dim);
  border: 1px solid var(--color-border);
  border-radius: 4px;
  padding: 3px 8px;
  background: var(--color-surface-elevated);
  flex-shrink: 0;
  font-weight: 800;
}

.results {
  max-height: 480px;
  overflow-y: auto;
  padding: var(--space-12);
  overscroll-behavior: contain;
  scrollbar-width: thin;
  position: relative;
  z-index: 20;
}

.result-group-label {
  padding: var(--space-16) var(--space-12) var(--space-6);
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 0.15em;
  text-transform: uppercase;
  color: var(--color-primary);
  opacity: 0.8;
}

.result-item {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  padding: var(--space-12);
  cursor: pointer;
  border-radius: var(--radius-xl);
  transition: all var(--transition-normal);
  border: 1px solid transparent;
}

.result-item--active {
  background: var(--color-surface-elevated);
  border-color: var(--color-border-hover);
  box-shadow: var(--shadow-medium);
  transform: scale(1.005);
}

.result-icon-wrap {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: var(--color-text-secondary);
  transition: all var(--transition-normal);
}

.result-item--active .result-icon-wrap {
  background: var(--gradient-primary);
  color: white;
  border-color: transparent;
  box-shadow: 0 0 20px rgba(var(--color-primary-rgb), 0.4);
  transform: rotate(-3deg) scale(1.05);
}

.result-item--active .result-name {
  color: var(--color-primary-hover);
}

.result-name {
  flex: 1;
  font-size: var(--font-size-base);
  color: var(--color-text-primary);
  font-weight: 700;
  letter-spacing: var(--font-tracking-tight);
}

.result-path {
  font-size: 10px;
  color: var(--color-text-dim);
  font-family: var(--font-mono);
  font-weight: 600;
  opacity: 0.6;
}

.empty {
  padding: var(--space-48) var(--space-20);
  text-align: center;
  color: var(--color-text-dim);
}

.empty strong { color: var(--color-text-primary); }

.footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-10) var(--space-20);
  border-top: 1px solid var(--color-border-subtle);
  background: var(--color-depth-0);
}

.hint-group {
  display: flex;
  gap: var(--space-16);
}

.hint {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.kbd-sm {
  font-size: 9px;
  font-family: var(--font-mono);
  color: var(--color-text-muted);
  border: 1px solid var(--color-border);
  border-radius: 3px;
  padding: 1px 4px;
  background: var(--color-surface-elevated);
}
</style>

<style>
.cmd-overlay-enter-active,
.cmd-overlay-leave-active {
  transition: opacity 180ms ease;
}
.cmd-overlay-enter-from,
.cmd-overlay-leave-to {
  opacity: 0;
}

.cmd-panel-enter-active,
.cmd-panel-leave-active {
  transition: opacity 180ms ease, transform 180ms cubic-bezier(0.4, 0, 0.2, 1);
}
.cmd-panel-enter-from,
.cmd-panel-leave-to {
  opacity: 0;
  transform: scale(0.97) translateY(-8px);
}
</style>
