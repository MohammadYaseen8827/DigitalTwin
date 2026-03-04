<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/auth.store'
import { useUIStore } from '@/stores/ui.store'
import { Toaster } from 'vue-sonner'
import AppSidebar from '@/components/layout/AppSidebar.vue'
import AppTopBar from '@/components/layout/AppTopBar.vue'
import CommandPalette from '@/components/layout/CommandPalette.vue'

const authStore = useAuthStore()
const uiStore = useUIStore()

function handleResize() {
  const isTablet = window.innerWidth <= 1024 && window.innerWidth > 768
  const isMobile = window.innerWidth <= 768
  
  if (isTablet) {
    uiStore.setSidebarCollapsed(true)
    uiStore.setSidebarOpen(false)
  } else if (isMobile) {
    uiStore.setSidebarCollapsed(false)
    uiStore.setSidebarOpen(false)
  } else {
    // Desktop default
    uiStore.setSidebarCollapsed(false)
  }
}

function handleKeydown(e: KeyboardEvent) {
  // Command Palette: Cmd+K or Ctrl+K
  if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
    e.preventDefault()
    uiStore.toggleCommandPalette()
  }
  
  // Sidebar: Cmd+B or Ctrl+B
  if ((e.ctrlKey || e.metaKey) && e.key === 'b') {
    e.preventDefault()
    uiStore.toggleSidebarCollapsed()
  }

  // Quick Search: '/' (if not typing in input)
  if (e.key === '/' && !['INPUT', 'TEXTAREA'].includes((e.target as HTMLElement).tagName)) {
    e.preventDefault()
    uiStore.openCommandPalette()
  }
  
  // Escape — close command palette
  if (e.key === 'Escape' && uiStore.commandPaletteOpen) {
    uiStore.closeCommandPalette()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
  window.addEventListener('resize', handleResize)
  
  // Initial check
  handleResize()
  
  // Apply initial theme and density
  document.documentElement.setAttribute('data-theme', uiStore.theme)
  document.documentElement.setAttribute('data-density', uiStore.density)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
  window.removeEventListener('resize', handleResize)
})
</script>

<template>
  <div class="app-root">
    <!-- Authenticated shell -->
    <template v-if="authStore.isAuthenticated">
      <AppSidebar
        :collapsed="uiStore.sidebarCollapsed"
        :open="uiStore.sidebarOpen"
        @toggle="uiStore.toggleSidebarCollapsed()"
        @close="uiStore.setSidebarOpen(false)"
      />
      
      <!-- Overlay for mobile -->
      <Transition name="fade">
        <div 
          v-if="uiStore.sidebarOpen" 
          class="sidebar-overlay" 
          @click="uiStore.setSidebarOpen(false)"
        />
      </Transition>

      <div
        class="app-main"
        :class="{ 'app-main--collapsed': uiStore.sidebarCollapsed }"
      >
        <AppTopBar
          :sidebar-collapsed="uiStore.sidebarCollapsed"
          :notification-count="3"
          @open-command-palette="uiStore.openCommandPalette()"
        />

        <div class="app-content">
          <router-view v-slot="{ Component }">
            <transition name="page-fade" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </div>
      </div>

      <!-- Global Command Palette -->
      <CommandPalette
        :open="uiStore.commandPaletteOpen"
        @close="uiStore.closeCommandPalette()"
      />
    </template>

    <!-- Unauthenticated -->
    <template v-else>
      <div class="app-auth">
        <router-view v-slot="{ Component }">
          <transition name="page-fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </template>

    <!-- Toast notifications -->
    <Toaster position="top-right" :duration="4000" />
  </div>
</template>

<style>
/* ====== ROOT SHELL ====== */
.app-root {
  display: flex;
  min-height: 100vh;
  background: var(--color-bg);
  color: var(--color-text-primary);
  font-family: var(--font-family);
}

/* ====== MAIN COLUMN ====== */
.app-main {
  display: grid;
  grid-template-rows: var(--topbar-height) 1fr;
  flex: 1;
  min-height: 100vh;
  margin-left: var(--sidebar-width);
  transition: margin-left var(--transition-sidebar);
  min-width: 0;
}

.app-main--collapsed {
  margin-left: var(--sidebar-collapsed-width);
}

/* ====== CONTENT AREA ====== */
.app-content {
  overflow-y: auto;
  overflow-x: hidden;
  padding: var(--space-app-y) var(--space-app-x);
  width: 100%;
  max-width: 2000px;
  margin: 0 auto;
}

/* ====== AUTH LAYOUT ====== */
.app-auth {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

/* ====== PAGE TRANSITIONS ====== */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity var(--transition-normal) var(--ease-soft), transform var(--transition-normal) var(--ease-soft);
}

.page-fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}

.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

/* ====== OVERLAY ====== */
.sidebar-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  z-index: 999;
}

/* ====== RESPONSIVE ====== */
@media (max-width: 1024px) {
  .app-main {
    margin-left: var(--sidebar-collapsed-width);
  }
}

@media (max-width: 768px) {
  .app-main {
    margin-left: 0;
  }

  .app-content {
    padding: var(--space-16);
  }
}
</style>
