<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import { Toaster } from 'vue-sonner'
import {
  Home,
  BarChart3,
  Zap,
  Brain,
  TrendingUp,
  History,
  LayoutDashboard,
  FileText,
  Settings,
  Info,
  LogOut,
  // Core Operations Icons
  Server,
  Wrench,
  Bell,
  Play,
  Factory,
  // Monitoring Icons
  Activity,
  Eye,
  Search,
  Database,
  // ML/AI Icons
  Cpu,
  Network,
  Calculator,
  Shuffle,
  // System Icons
  Heart,
  Shield,
  Cloud,
  // Additional Icons
  Layers,
  Filter,
  Archive,
  FileBarChart,
  Building,
  Radio,
  Plug,
  Target
} from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()

const navItems = ref([
  // Core Dashboard
  { name: 'Home', path: '/', icon: Home },
  { name: 'Dashboard', path: '/dashboard', icon: BarChart3 },
  { name: 'Enterprise', path: '/enterprise-dashboard', icon: LayoutDashboard },
  
  // Core Operations
  { name: 'Machines', path: '/machines', icon: Server },
  { name: 'Maintenance', path: '/maintenance', icon: Wrench },
  { name: 'Alerts', path: '/alerts', icon: Bell },
  { name: 'Simulations', path: '/simulations', icon: Play },
  { name: 'Production Lines', path: '/production-lines', icon: Factory },
  
  // Monitoring & Analytics
  { name: 'Telemetry', path: '/telemetry', icon: Activity },
  { name: 'Advanced Analytics', path: '/advanced-analytics-viz', icon: Eye },
  { name: 'Performance', path: '/performance-monitoring', icon: BarChart3 },
  { name: 'Drift Detection', path: '/drift-detection', icon: TrendingUp },
  
  // Configuration & Management
  { name: 'Configuration', path: '/configuration', icon: Settings },
  { name: 'Advanced Search', path: '/search', icon: Search },
  { name: 'Data Archival', path: '/data-archival', icon: Archive },
  
  // ML & AI Systems
  { name: 'Synthetic Data', path: '/synthetic-data', icon: Database },
  { name: 'Mathematical Modeling', path: '/mathematical-modeling', icon: Calculator },
  { name: 'Uncertainty', path: '/uncertainty', icon: Shuffle },
  { name: 'Run-to-Failure', path: '/run-to-failure', icon: Cpu },
  { name: 'Prescriptive', path: '/prescriptive-maintenance', icon: Network },
  { name: 'Predictive Dashboard', path: '/predictive-analytics-dashboard', icon: TrendingUp },
  { name: 'Model Validation', path: '/model-validation', icon: FileBarChart },
  { name: 'Model Lifecycle', path: '/model-lifecycle', icon: Layers },
  { name: 'Azure Digital Twin', path: '/azure-digital-twin', icon: Cloud },
  
  // Enterprise Integration
  { name: 'Tenant Management', path: '/tenants', icon: Building },
  { name: 'External Systems', path: '/external-systems', icon: Plug },
  
  // Real-time Analytics
  { name: 'Real-time Analytics', path: '/real-time-analytics', icon: Radio },
  
  // Benchmark Validation
  { name: 'Benchmark Validation', path: '/benchmark-validation', icon: Target },
  
  // System & Reports
  { name: 'System Health', path: '/system-health', icon: Heart },
  { name: 'Reporting Dashboard', path: '/reporting-dashboard', icon: FileText },
  
  // Legacy Routes
  { name: 'Prediction History', path: '/prediction-history', icon: History },
  { name: 'Analytics Dashboard', path: '/advanced-analytics-dashboard', icon: LayoutDashboard },
  { name: 'Enhanced Prescriptive', path: '/enhanced-prescriptive', icon: Zap },
  { name: 'Reporting', path: '/reporting', icon: FileText },
  
  // System
  { name: 'Settings', path: '/settings', icon: Settings },
  { name: 'About', path: '/about', icon: Info }
])

const handleLogout = () => {
  authStore.clearAuth()
  router.push('/login')
}
</script>

<template>
  <div class="app-container">
    <template v-if="authStore.isAuthenticated">
    <nav class="sidebar glass-panel">
      <div class="logo">
        <div class="logo-icon">🏭</div>
        <span class="logo-text">Digital Twin</span>
      </div>
      
      <div class="nav-menu">
        <router-link 
          v-for="item in navItems" 
          :key="item.name"
          :to="item.path"
          class="nav-item"
          active-class="active"
        >
          <component :is="item.icon" class="nav-icon" />
          <span class="nav-text">{{ item.name }}</span>
        </router-link>
      </div>
      
      <div class="nav-footer">
        <button class="logout-btn" @click="handleLogout">
          <LogOut class="logout-icon" />
          <span>Logout</span>
        </button>
      </div>
    </nav>
    
    <main class="main-content">
      <router-view v-slot="{ Component }">
        <transition name="page-fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    </template>
    <template v-else>
      <main class="main-content full-width">
        <router-view v-slot="{ Component }">
          <transition name="page-fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </template>
    
    <!-- Toast Container -->
    <Toaster position="top-right" />
  </div>
</template>

<style scoped>
.app-container {
  display: flex;
  min-height: 100vh;
  background: var(--color-background);
}

.sidebar {
  width: 280px;
  background: var(--color-surface);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  position: fixed;
  height: 100vh;
  left: 0;
  top: 0;
  z-index: 1000;
  overflow-y: auto;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 1.5rem;
  border-bottom: 1px solid var(--color-border);
}

.logo-icon {
  font-size: 1.5rem;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-primary);
  border-radius: 8px;
  color: white;
}

.logo-text {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--color-text-primary);
}

.nav-menu {
  flex: 1;
  padding: 1rem 0;
  overflow-y: auto;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0.75rem 1.5rem;
  color: var(--color-text-secondary);
  text-decoration: none;
  transition: all 0.2s ease;
  border-left: 3px solid transparent;
}

.nav-item:hover {
  background: var(--color-background-secondary);
  color: var(--color-text-primary);
}

.nav-item.active {
  background: var(--color-primary);
  color: white;
  border-left-color: var(--color-primary);
}

.nav-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.nav-text {
  font-size: 0.875rem;
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.nav-footer {
  padding: 1rem;
  border-top: 1px solid var(--color-border);
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
  padding: 0.75rem 1rem;
  background: var(--color-error);
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 0.875rem;
  font-weight: 500;
  transition: background-color 0.2s ease;
}

.logout-btn:hover {
  background: #dc2626;
}

.logout-icon {
  width: 20px;
  height: 20px;
}

.main-content {
  flex: 1;
  margin-left: 280px;
  padding: 2rem;
  background: var(--color-background);
  min-height: 100vh;
}

.main-content.full-width {
  margin-left: 0;
}

/* Page Transitions */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 250ms ease-out, transform 250ms ease-out;
}

.page-fade-enter-from {
  opacity: 0;
  transform: translateY(15px);
}

.page-fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}

/* Responsive Design */
@media (max-width: 768px) {
  .sidebar {
    transform: translateX(-100%);
    transition: transform 0.3s ease;
  }
  
  .sidebar.open {
    transform: translateX(0);
  }
  
  .main-content {
    margin-left: 0;
    padding: 1rem;
  }
}

/* Legacy styles for other components */
.app-shell {
  position: relative;
  min-height: 100vh;
  padding: clamp(var(--space-16), 4vw, var(--space-32)) 0 var(--space-32);
}

.bg-accents {
  position: fixed;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
  z-index: 0;
}
.orb {
  position: absolute;
  width: 42vw;
  height: 42vw;
  border-radius: 50%;
  filter: blur(70px);
  opacity: 0.35;
}
.orb-a {
  top: -10%;
  left: -12%;
  background: radial-gradient(circle, rgba(56, 232, 255, 0.65), transparent 60%);
  animation: drift 16s ease-in-out infinite alternate;
}
.orb-b {
  bottom: -16%;
  right: -12%;
  background: radial-gradient(circle, rgba(124, 77, 255, 0.55), transparent 60%);
  animation: drift 18s ease-in-out infinite alternate;
}

.topbar {
  position: sticky;
  top: clamp(var(--space-8), 2vw, var(--space-16));
  margin: 0 auto clamp(var(--space-20), 4vw, var(--space-32));
  width: min(1360px, calc(100% - 24px));
  z-index: 2;
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: var(--space-16);
  padding: var(--space-12) var(--space-20);
  background: color-mix(in srgb, var(--color-glass) 90%, transparent);
}

.brand {
  display: flex;
  align-items: center;
  gap: var(--space-12);
}
.logo {
  filter: drop-shadow(0 6px 18px rgba(0, 0, 0, 0.4));
}
.eyebrow {
  margin: 0;
  font-size: var(--font-size-xs);
  letter-spacing: 0.08em;
  color: var(--color-text-secondary);
  text-transform: uppercase;
}
.title {
  margin: 2px 0 0;
  font-size: var(--font-size-lg);
  letter-spacing: 0.01em;
}

.nav-pills {
  display: inline-flex;
  justify-content: center;
  gap: var(--space-8);
  padding: 6px;
  background: color-mix(in srgb, var(--color-surface-alt) 80%, transparent);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-xl);
  box-shadow: var(--shadow-subtle);
}
.nav-pills a {
  padding: 10px 14px;
  border-radius: var(--radius-lg);
  color: var(--color-text-secondary);
  font-size: var(--font-size-sm);
  transition: 180ms ease;
}
.nav-pills a:hover {
  color: var(--color-text-primary);
  background: color-mix(in srgb, var(--color-primary) 18%, transparent);
}
.nav-pills a.router-link-exact-active {
  color: var(--color-text-primary);
  background: color-mix(in srgb, var(--color-primary) 28%, transparent);
  box-shadow: var(--glow-primary);
}

.actions {
  display: flex;
  gap: var(--space-8);
  align-items: center;
}

.main {
  position: relative;
  z-index: 1;
  display: grid;
  gap: var(--space-20);
}

.hero {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: clamp(var(--space-16), 3vw, var(--space-32));
  padding: clamp(var(--space-20), 6vw, var(--space-40));
  border-radius: var(--radius-2xl);
  background: var(--gradient-card);
}
.hero-copy h2 {
  margin: 6px 0 10px;
  font-size: clamp(var(--font-size-2xl), 2.4vw, 2.6rem);
}
.hero-copy .lede {
  margin: 0 0 var(--space-16);
  color: var(--color-text-secondary);
  max-width: 560px;
}
.hero-actions {
  display: flex;
  gap: var(--space-12);
  flex-wrap: wrap;
}
.hero-visual {
  position: relative;
  min-height: 220px;
  border-radius: var(--radius-2xl);
  border: 1px solid var(--color-border-subtle);
  background: color-mix(in srgb, var(--color-surface) 80%, transparent);
  overflow: hidden;
}
.glow,
.pulse {
  position: absolute;
  inset: 0;
  border-radius: inherit;
  filter: blur(24px);
}
.glow {
  background: radial-gradient(circle at 30% 30%, rgba(56, 232, 255, 0.4), transparent 55%),
    radial-gradient(circle at 80% 20%, rgba(124, 77, 255, 0.35), transparent 55%),
    radial-gradient(circle at 60% 80%, rgba(107, 229, 155, 0.3), transparent 55%);
  animation: shimmer 10s ease-in-out infinite alternate;
}
.pulse {
  background: radial-gradient(circle at 50% 50%, rgba(56, 232, 255, 0.16), transparent 70%);
  animation: pulse 3s ease-in-out infinite;
}

.content {
  padding: clamp(var(--space-16), 3vw, var(--space-24));
  border-radius: var(--radius-2xl);
  border: 1px solid var(--color-border-subtle);
}

@keyframes drift {
  to {
    transform: translate(12px, -14px) scale(1.05);
  }
}

@keyframes shimmer {
  to {
    transform: scale(1.04) translate(-6px, 6px);
    opacity: 0.92;
  }
}

@keyframes pulse {
  0% {
    opacity: 0.5;
  }
  50% {
    opacity: 0.9;
  }
  100% {
    opacity: 0.5;
  }
}

@media (max-width: 960px) {
  .topbar {
    grid-template-columns: 1fr;
    justify-items: start;
  }
  .actions {
    width: 100%;
    justify-content: flex-start;
  }
}
</style>