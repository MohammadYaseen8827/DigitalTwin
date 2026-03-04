<script setup lang="ts">
import { computed, useCssModule, ref } from 'vue'
import {
  Home, BarChart3, LayoutDashboard, Server, Wrench, Bell, Play, Factory,
  Activity, Eye, TrendingUp, Settings, Archive, Database, Calculator,
  Shuffle, Cpu, Network, Layers, Cloud, Building, Plug, Radio, Target,
  Heart, FileText, Filter, ChevronLeft, ChevronRight, LogOut, Zap, FileBarChart
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth.store'
import { useRouter } from 'vue-router'
import NavGroup from './NavGroup.vue'

const props = defineProps<{ 
  collapsed: boolean
  open?: boolean
}>()
const emit = defineEmits<{
  (e: 'toggle'): void
  (e: 'close'): void
}>()

const isHovered = ref(false)
const styles = useCssModule()
const authStore = useAuthStore()
const router = useRouter()

const activeCollapsed = computed(() => props.collapsed && !isHovered.value)

const navGroups = [
  {
    label: 'Command',
    items: [
      { name: 'Overview', path: '/', icon: Home },
      { name: 'Analytics Hub', path: '/dashboard', icon: BarChart3 },
      { name: 'Enterprise Hub', path: '/enterprise-dashboard', icon: LayoutDashboard },
    ]
  },
  {
    label: 'Infrastructure',
    items: [
      { name: 'Node Registry', path: '/machines', icon: Server },
      { name: 'Factory Topology', path: '/production-lines', icon: Factory },
      { name: 'Service Protocols', path: '/maintenance', icon: Wrench },
      { name: 'Anomaly Center', path: '/alerts', icon: Bell },
    ]
  },
  {
    label: 'Intelligence',
    items: [
      { name: 'Live Telemetry', path: '/telemetry', icon: Activity },
      { name: 'Neural Analytics', path: '/advanced-analytics-viz', icon: Eye },
      { name: 'Predictive Suite', path: '/predictive-analytics-dashboard', icon: TrendingUp },
      { name: 'Simulations', path: '/simulations', icon: Play },
    ]
  },
  {
    label: 'Control',
    items: [
      { name: 'System Settings', path: '/settings', icon: Settings },
      { name: 'Global Config', path: '/configuration', icon: Zap },
      { name: 'System Health', path: '/system-health', icon: Heart },
    ]
  }
]

const userInitials = computed(() => {
  const name = authStore.user?.fullName || authStore.user?.userName || 'U'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const userEmail = computed(() => authStore.user?.email || '')
const userName = computed(() => authStore.user?.fullName || authStore.user?.userName || 'User')

function handleLogout() {
  authStore.clearAuth()
  router.push('/login')
}
</script>

<template>
  <aside 
    :class="[
      styles['sidebar'], 
      activeCollapsed && styles['sidebar--collapsed'],
      open && styles['sidebar--open'],
      isHovered && props.collapsed && styles['sidebar--hover-expand']
    ]"
    @mouseenter="isHovered = true"
    @mouseleave="isHovered = false"
  >
    <!-- Brand -->
    <div :class="styles['brand']">
      <div :class="styles['brand-icon']">
        <Zap :width="16" :height="16" fill="white" />
      </div>
      <Transition name="slide-fade">
        <div v-if="!activeCollapsed" :class="styles['brand-text']">
          <span :class="styles['brand-name']">Digital Twin</span>
          <span :class="styles['brand-sub']">Command Hub</span>
        </div>
      </Transition>
    </div>

    <!-- Toggle -->
    <button :class="styles['toggle-btn']" @click="emit('toggle')" :aria-label="activeCollapsed ? 'Expand sidebar' : 'Collapse sidebar'">
      <component :is="activeCollapsed ? ChevronRight : ChevronLeft" :width="12" :height="12" />
    </button>

    <!-- Nav -->
    <nav :class="styles['nav']" role="navigation" aria-label="Main navigation">
      <div v-for="group in navGroups" :key="group.label" :class="styles['nav-group']">
        <Transition name="fade">
          <h3 v-if="!activeCollapsed" :class="styles['group-label']">{{ group.label }}</h3>
          <div v-else :class="styles['group-divider']" />
        </Transition>
        
        <NavGroup
          :items="group.items"
          :collapsed="activeCollapsed"
        />
      </div>
    </nav>

    <!-- User footer -->
    <div :class="styles['user-footer']">
      <div :class="styles['user-row']">
        <div :class="styles['avatar']">{{ userInitials }}</div>
        <Transition name="fade">
          <div v-if="!activeCollapsed" :class="styles['user-info']">
            <span :class="styles['user-name']">{{ userName }}</span>
            <span :class="styles['user-email']">{{ userEmail }}</span>
          </div>
        </Transition>
        <Transition name="fade">
          <button
            v-if="!activeCollapsed"
            :class="styles['logout-btn']"
            @click="handleLogout"
            title="Sign out"
            aria-label="Sign out"
          >
            <LogOut :width="14" :height="14" />
          </button>
        </Transition>
      </div>
      <Transition name="fade">
        <button
          v-if="activeCollapsed"
          :class="styles['logout-icon-btn']"
          @click="handleLogout"
          title="Sign out"
          aria-label="Sign out"
        >
          <LogOut :width="14" :height="14" />
        </button>
      </Transition>
    </div>
  </aside>
</template>

<style module>
.sidebar {
  position: fixed;
  left: 0;
  top: 0;
  bottom: 0;
  z-index: 1000;
  width: var(--sidebar-width);
  background: var(--color-depth-1);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  transition: transform var(--transition-sidebar), width var(--transition-sidebar);
  overflow: hidden;
  box-shadow: 1px 0 0 rgba(0, 0, 0, 0.5);
}

@media (max-width: 1024px) {
  .sidebar {
    transform: translateX(-100%);
  }
  .sidebar--open {
    transform: translateX(0);
  }
}

.sidebar--collapsed {
  width: var(--sidebar-collapsed-width);
}

.sidebar--hover-expand {
  width: var(--sidebar-width) !important;
  box-shadow: var(--shadow-elevated);
}

/* Brand */
.brand {
  display: flex;
  align-items: center;
  gap: var(--space-12);
  padding: var(--space-20) var(--space-16);
  min-height: 64px;
  overflow: hidden;
  flex-shrink: 0;
}

.brand-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--gradient-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-primary);
  position: relative;
}

.brand-icon::after {
  content: '';
  position: absolute;
  inset: -1px;
  border-radius: inherit;
  background: var(--gradient-primary);
  opacity: 0.4;
  filter: blur(4px);
  z-index: -1;
}

.brand-text {
  display: flex;
  flex-direction: column;
  gap: 0;
  overflow: hidden;
  white-space: nowrap;
}

.brand-name {
  font-size: var(--font-size-base);
  font-weight: 800;
  color: var(--color-text-primary);
  letter-spacing: var(--font-tracking-tight);
  font-family: var(--font-family);
  line-height: var(--font-lineheight-tight);
}

.brand-sub {
  font-size: 9px;
  font-weight: 800;
  color: var(--color-primary);
  letter-spacing: 0.15em;
  text-transform: uppercase;
  opacity: 0.9;
}

/* Toggle button */
.toggle-btn {
  position: absolute;
  top: 20px;
  right: 12px;
  width: 24px;
  height: 24px;
  border-radius: var(--radius-sm);
  background: var(--color-surface-elevated);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
  transition: all var(--transition-fast);
  opacity: 0;
}

.sidebar:hover .toggle-btn {
  opacity: 1;
}

.toggle-btn:hover {
  background: var(--color-primary);
  color: white;
  border-color: var(--color-primary);
  box-shadow: var(--glow-sm);
}

/* Nav */
.nav {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: var(--space-8) var(--space-12);
  display: flex;
  flex-direction: column;
  gap: var(--space-16);
  scrollbar-width: none;
}

.nav::-webkit-scrollbar {
  display: none;
}

.nav-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.group-label {
  font-size: 10px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.12em;
  padding: 0 var(--space-8);
  margin-bottom: var(--space-4);
}

.group-divider {
  height: 1px;
  background: var(--color-border-subtle);
  margin: var(--space-8) 0;
}

/* User footer */
.user-footer {
  border-top: 1px solid var(--color-border-subtle);
  padding: var(--space-12) var(--space-16);
  background: rgba(255, 255, 255, 0.01);
  flex-shrink: 0;
}

.user-row {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  overflow: hidden;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-full);
  background: var(--gradient-primary);
  color: white;
  font-size: var(--font-size-xs);
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  letter-spacing: -0.5px;
}

.user-info {
  flex: 1;
  min-width: 0;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.user-name {
  font-size: var(--font-size-xs);
  font-weight: 500;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-email {
  font-size: 10px;
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.logout-btn {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: background var(--transition-fast), color var(--transition-fast);
  flex-shrink: 0;
}

.logout-btn:hover {
  background: var(--color-danger-muted);
  color: var(--color-danger);
}

.logout-icon-btn {
  width: 100%;
  height: 32px;
  border-radius: var(--radius-sm);
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-top: var(--space-6);
  transition: background var(--transition-fast), color var(--transition-fast);
}

.logout-icon-btn:hover {
  background: var(--color-danger-muted);
  color: var(--color-danger);
}
</style>

<style>
.fade-enter-active, .fade-leave-active {
  transition: opacity 180ms ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
