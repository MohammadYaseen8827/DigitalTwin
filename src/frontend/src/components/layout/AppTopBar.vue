<script setup lang="ts">
import { ref, computed, useCssModule } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUIStore } from '@/stores/ui.store'
import { 
  Search, 
  Bell, 
  ChevronRight, 
  Settings, 
  LogOut, 
  User,
  Sun,
  Moon,
  Layout,
  Maximize2,
  Minimize2,
  Info
} from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth.store'

const props = defineProps<{
  sidebarCollapsed: boolean
  notificationCount?: number
}>()

const uiStore = useUIStore()
const emit = defineEmits<{ (e: 'openCommandPalette'): void }>()

const styles = useCssModule()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const userMenuOpen = ref(false)

const breadcrumbs = computed(() => {
  const name = route.name as string | undefined
  const path = route.path

  if (path === '/') return [{ label: 'Home', path: '/' }]

  const segments = path.split('/').filter(Boolean)
  return segments.map((seg, i) => ({
    label: seg.replace(/-/g, ' ').replace(/\b\w/g, l => l.toUpperCase()),
    path: '/' + segments.slice(0, i + 1).join('/')
  }))
})

const userInitials = computed(() => {
  const name = authStore.user?.fullName || authStore.user?.userName || 'U'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})

const userName = computed(() => authStore.user?.fullName || authStore.user?.userName || 'User')

function handleLogout() {
  userMenuOpen.value = false
  authStore.clearAuth()
  router.push('/login')
}

function closeMenu() {
  userMenuOpen.value = false
}
</script>

<template>
  <header :class="styles['topbar']">
    <div :class="styles['topbar-left']">
      <!-- System Heartbeat -->
      <div :class="styles['heartbeat']" title="System Live">
        <div :class="styles['pulse']" />
      </div>

      <!-- Breadcrumbs -->
      <nav :class="styles['breadcrumbs']" aria-label="Breadcrumb">
        <template v-for="(crumb, i) in breadcrumbs" :key="crumb.path">
          <div v-if="i > 0" :class="styles['crumb-sep']">
            <ChevronRight :width="10" :height="10" />
          </div>
          <router-link
            :to="crumb.path"
            :class="[
              styles['crumb'], 
              i === breadcrumbs.length - 1 && styles['crumb--active']
            ]"
          >
            <div v-if="i === breadcrumbs.length - 1" :class="styles['active-indicator']" />
            <span :class="styles['crumb-text']">{{ crumb.label }}</span>
          </router-link>
        </template>
      </nav>
    </div>

    <!-- Right actions -->
    <div :class="styles['actions']">
      <!-- Search Trigger -->
      <button :class="styles['command-trigger']" @click="emit('openCommandPalette')" aria-label="Open command palette (Ctrl+K)">
        <Search :width="14" :height="14" />
        <span :class="styles['command-label']">Command Center</span>
        <div :class="styles['kbd-group']">
          <kbd :class="styles['kbd']">⌘</kbd>
          <kbd :class="styles['kbd']">K</kbd>
        </div>
      </button>

      <div :class="styles['divider']" />

      <!-- Density Toggle -->
      <button 
        :class="styles['icon-btn']" 
        @click="uiStore.toggleDensity()" 
        :title="uiStore.density === 'compact' ? 'Comfortable Mode' : 'Compact Mode'"
      >
        <Minimize2 v-if="uiStore.density === 'comfortable'" :width="16" :height="16" />
        <Maximize2 v-else :width="16" :height="16" />
      </button>

      <!-- Theme Toggle -->
      <button 
        :class="styles['icon-btn']" 
        @click="uiStore.toggleTheme()" 
        :title="uiStore.theme === 'dark' ? 'Light Theme' : 'Dark Theme'"
      >
        <Sun v-if="uiStore.theme === 'dark'" :width="16" :height="16" />
        <Moon v-else :width="16" :height="16" />
      </button>

      <div :class="styles['divider']" />

      <!-- Notifications -->
      <button :class="styles['icon-btn']" aria-label="Notifications">
        <Bell :width="16" :height="16" />
        <span v-if="notificationCount" :class="styles['badge']">{{ notificationCount }}</span>
      </button>

      <div :class="styles['divider']" />

      <!-- User Profile -->
      <div :class="styles['user-menu-wrap']">
        <button 
          :class="[styles['avatar-btn'], userMenuOpen && styles['avatar-btn--open']]" 
          @click="userMenuOpen = !userMenuOpen"
        >
          <div :class="styles['avatar']">{{ userInitials }}</div>
        </button>

        <Transition name="dropdown">
          <div v-if="userMenuOpen" :class="styles['dropdown']" v-click-outside="closeMenu">
            <div :class="styles['dropdown-header']">
               <div :class="styles['dropdown-avatar']">{{ userInitials }}</div>
               <div :class="styles['dropdown-user-info']">
                 <span :class="styles['dropdown-name']">{{ userName }}</span>
                 <span :class="styles['dropdown-email']">Level 4 Administrator</span>
               </div>
            </div>
            <div :class="styles['dropdown-divider']" />
            <button :class="styles['dropdown-item']" @click="router.push('/settings'); userMenuOpen = false">
              <Settings :width="14" :height="14" />
              <span>System Settings</span>
            </button>
            <button :class="styles['dropdown-item']" @click="router.push('/about'); userMenuOpen = false">
              <Info :width="14" :height="14" />
              <span>Protocol Documentation</span>
            </button>
            <div :class="styles['dropdown-divider']" />
            <button :class="[styles['dropdown-item'], styles['dropdown-item--danger']]" @click="handleLogout">
              <LogOut :width="14" :height="14" />
              <span>Terminate Session</span>
            </button>
          </div>
        </Transition>
      </div>
    </div>
  </header>
</template>

<style module>
.topbar {
  position: sticky;
  top: 0;
  z-index: 100;
  height: var(--topbar-height);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 var(--space-app-x);
  background: var(--color-glass);
  backdrop-filter: var(--backdrop-blur);
  -webkit-backdrop-filter: var(--backdrop-blur);
  border-bottom: 1px solid var(--color-border-subtle);
  gap: var(--space-20);
}

.topbar-left {
  display: flex;
  align-items: center;
  gap: var(--space-16);
  flex: 1;
  min-width: 0;
}

/* Heartbeat */
.heartbeat {
  width: 12px;
  height: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.pulse {
  width: 6px;
  height: 6px;
  background: var(--color-success);
  border-radius: 50%;
  position: relative;
  box-shadow: 0 0 8px var(--color-success);
}

.pulse::after {
  content: '';
  position: absolute;
  inset: -4px;
  border-radius: 50%;
  border: 1px solid var(--color-success);
  animation: pulse-ring 2s cubic-bezier(0.215, 0.61, 0.355, 1) infinite;
}

@keyframes pulse-ring {
  0% { transform: scale(0.5); opacity: 0.8; }
  80%, 100% { transform: scale(2.5); opacity: 0; }
}

/* Breadcrumbs */
.breadcrumbs {
  display: flex;
  align-items: center;
  gap: var(--space-8);
  overflow: hidden;
}

.crumb-sep {
  color: var(--color-text-dim);
  display: flex;
  align-items: center;
  user-select: none;
  opacity: 0.5;
}

.crumb {
  position: relative;
  font-size: 11px;
  font-weight: 700;
  color: var(--color-text-dim);
  text-transform: uppercase;
  letter-spacing: 0.12em;
  white-space: nowrap;
  transition: all var(--transition-normal);
  padding: var(--space-6) var(--space-12);
  border-radius: var(--radius-md);
  background: transparent;
  border: 1px solid transparent;
  display: flex;
  align-items: center;
  gap: var(--space-8);
}

.crumb:hover {
  color: var(--color-text-primary);
  background: var(--color-surface-elevated);
  border-color: var(--color-border);
}

.crumb--active {
  color: var(--color-primary);
  background: var(--color-depth-1);
  border-color: var(--color-border);
  box-shadow: var(--shadow-xs);
  pointer-events: none;
}

.active-indicator {
  width: 6px;
  height: 6px;
  background: var(--color-primary);
  border-radius: 50%;
  box-shadow: 0 0 8px var(--color-primary);
}

/* Actions */
.actions {
  display: flex;
  align-items: center;
  gap: var(--space-12);
  flex-shrink: 0;
}

.command-trigger {
  display: flex;
  align-items: center;
  gap: var(--space-12);
  padding: var(--space-8) var(--space-16);
  background: var(--color-depth-0);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  color: var(--color-text-secondary);
  font-size: var(--font-size-xs);
  font-weight: 600;
  cursor: pointer;
  transition: all var(--transition-normal);
  min-width: 240px;
}

.command-trigger:hover {
  border-color: var(--color-primary);
  background: var(--color-depth-1);
  color: var(--color-text-primary);
  box-shadow: var(--shadow-primary);
}

.command-label {
  flex: 1;
  text-align: left;
  letter-spacing: var(--font-tracking-tight);
}

.kbd-group {
  display: flex;
  gap: 2px;
  margin-left: auto;
}

.kbd {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-dim);
  background: var(--color-surface-elevated);
  border: 1px solid var(--color-border-subtle);
  border-radius: 4px;
  min-width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
}

.divider {
  width: 1px;
  height: 20px;
  background: var(--color-border-subtle);
}

.icon-btn {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  background: transparent;
  border: none;
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all var(--transition-fast);
}

.icon-btn:hover {
  background: var(--color-surface-alt);
  color: var(--color-text-primary);
}

.badge {
  position: absolute;
  top: 6px;
  right: 6px;
  width: 6px;
  height: 6px;
  background: var(--color-danger);
  border-radius: 50%;
  box-shadow: 0 0 0 2px var(--color-bg);
}

/* User menu */
.user-menu-wrap {
  position: relative;
}

.avatar-btn {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-full);
  background: var(--gradient-primary);
  border: 2px solid transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: border-color var(--transition-fast), box-shadow var(--transition-fast);
  padding: 0;
}

.avatar-btn:hover,
.avatar-btn--open {
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-muted);
}

.avatar {
  font-size: 11px;
  font-weight: 700;
  color: white;
  letter-spacing: -0.5px;
  pointer-events: none;
}

.dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 220px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-overlay);
  overflow: hidden;
  z-index: 500;
}

.dropdown-header {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  padding: var(--space-12) var(--space-14);
}

.dropdown-avatar {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-full);
  background: var(--gradient-primary);
  color: white;
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.dropdown-user-info {
  display: flex;
  flex-direction: column;
  gap: 1px;
  min-width: 0;
}

.dropdown-name {
  font-size: var(--font-size-sm);
  font-weight: 500;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dropdown-email {
  font-size: var(--font-size-xs);
  color: var(--color-text-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dropdown-divider {
  height: 1px;
  background: var(--color-border-subtle);
  margin: var(--space-4) 0;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: var(--space-10);
  width: 100%;
  padding: var(--space-8) var(--space-14);
  background: transparent;
  border: none;
  font-size: var(--font-size-sm);
  font-family: var(--font-family);
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: background var(--transition-fast), color var(--transition-fast);
  text-align: left;
}

.dropdown-item:hover {
  background: var(--color-surface-elevated);
  color: var(--color-text-primary);
}

.dropdown-item--danger:hover {
  background: var(--color-danger-muted);
  color: var(--color-danger);
}
</style>

<style>
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity 150ms ease, transform 150ms cubic-bezier(0.4, 0, 0.2, 1);
}
.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: scale(0.97) translateY(-6px);
}
</style>
