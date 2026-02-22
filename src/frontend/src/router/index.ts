import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'
import LoginView from '@/views/LoginView.vue'
import HomeView from '@/views/HomeView.vue'
import EnhancedDashboardView from '@/views/EnhancedDashboardView.vue'
import AdvancedAnalyticsView from '@/views/AdvancedAnalyticsView.vue'
import AdvancedAnalyticsDashboardView from '@/views/AdvancedAnalyticsDashboardView.vue'
import EnhancedPrescriptiveAnalyticsView from '@/views/EnhancedPrescriptiveAnalyticsView.vue'
import PredictiveAnalyticsView from '@/views/PredictiveAnalyticsView.vue'
import PredictionHistoryView from '@/views/PredictionHistoryView.vue'
import ReportingView from '@/views/ReportingView.vue'
import SettingsView from '@/views/SettingsView.vue'
import AboutView from '@/views/AboutView.vue'

// Phase 10 Dashboard Views
import MachineManagementView from '@/views/MachineManagementView.vue'
import MaintenanceManagementView from '@/views/MaintenanceManagementView.vue'
import AlertManagementView from '@/views/AlertManagementView.vue'
import SimulationManagementView from '@/views/SimulationManagementView.vue'
import ProductionLineManagementView from '@/views/ProductionLineManagementView.vue'
import TelemetryMonitoringView from '@/views/TelemetryMonitoringView.vue'
import AdvancedAnalyticsVisualizationView from '@/views/AdvancedAnalyticsVisualizationView.vue'
import ConfigurationTemplatesView from '@/views/ConfigurationTemplatesView.vue'
import AdvancedSearchView from '@/views/AdvancedSearchView.vue'
import DataArchivalView from '@/views/DataArchivalView.vue'
import PerformanceMonitoringView from '@/views/PerformanceMonitoringView.vue'
import SyntheticDataGeneratorView from '@/views/SyntheticDataGeneratorView.vue'
import DriftDetectionView from '@/views/DriftDetectionView.vue'
import MathematicalModelingView from '@/views/MathematicalModelingView.vue'
import UncertaintyQuantificationView from '@/views/UncertaintyQuantificationView.vue'
import RunToFailureView from '@/views/RunToFailureView.vue'
import SystemHealthView from '@/views/SystemHealthView.vue'
import PrescriptiveMaintenanceView from '@/views/PrescriptiveMaintenanceView.vue'
import ReportingDashboardView from '@/views/ReportingDashboardView.vue'
import PredictiveAnalyticsDashboardView from '@/views/PredictiveAnalyticsDashboardView.vue'
import ModelValidationView from '@/views/ModelValidationView.vue'
import ModelLifecycleView from '@/views/ModelLifecycleView.vue'
import TenantManagementView from '@/views/TenantManagementView.vue'
import ExternalSystemIntegrationView from '@/views/ExternalSystemIntegrationView.vue'
import RealTimeAnalyticsView from '@/views/RealTimeAnalyticsView.vue'
import BenchmarkValidationView from '@/views/BenchmarkValidationView.vue'
import MLInsightsView from '@/views/MLInsights.vue'
import EnterpriseOperationsDashboard from '@/views/EnterpriseOperationsDashboard.vue'

// Type augmentation for RouteMeta
declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean
    roles?: string[]
  }
}

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Home',
    component: HomeView,
    meta: { requiresAuth: true }
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: EnhancedDashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/login',
    name: 'Login',
    component: LoginView
  },
  {
    path: '/predictive-analytics',
    name: 'PredictiveAnalytics',
    component: PredictiveAnalyticsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/prediction-history',
    name: 'PredictionHistory',
    component: PredictionHistoryView,
    meta: { requiresAuth: true }
  },
  {
    path: '/advanced-analytics',
    name: 'AdvancedAnalytics',
    component: AdvancedAnalyticsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/advanced-analytics-dashboard',
    name: 'AdvancedAnalyticsDashboard',
    component: AdvancedAnalyticsDashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/enhanced-prescriptive',
    name: 'EnhancedPrescriptive',
    component: EnhancedPrescriptiveAnalyticsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/reporting',
    name: 'Reporting',
    component: ReportingView,
    meta: { requiresAuth: true }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: SettingsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/about',
    name: 'About',
    component: AboutView,
    meta: { requiresAuth: true }
  },
  // Phase 10 Dashboard Routes
  {
    path: '/machines',
    name: 'MachineManagement',
    component: MachineManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/maintenance',
    name: 'MaintenanceManagement',
    component: MaintenanceManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/alerts',
    name: 'AlertManagement',
    component: AlertManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/simulations',
    name: 'SimulationManagement',
    component: SimulationManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/production-lines',
    name: 'ProductionLineManagement',
    component: ProductionLineManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/telemetry',
    name: 'TelemetryMonitoring',
    component: TelemetryMonitoringView,
    meta: { requiresAuth: true }
  },
  {
    path: '/advanced-analytics-viz',
    name: 'AdvancedAnalyticsVisualization',
    component: AdvancedAnalyticsVisualizationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/configuration',
    name: 'ConfigurationTemplates',
    component: ConfigurationTemplatesView,
    meta: { requiresAuth: true }
  },
  {
    path: '/search',
    name: 'AdvancedSearch',
    component: AdvancedSearchView,
    meta: { requiresAuth: true }
  },
  {
    path: '/data-archival',
    name: 'DataArchival',
    component: DataArchivalView,
    meta: { requiresAuth: true }
  },
  {
    path: '/performance-monitoring',
    name: 'PerformanceMonitoring',
    component: PerformanceMonitoringView,
    meta: { requiresAuth: true }
  },
  {
    path: '/synthetic-data',
    name: 'SyntheticDataGenerator',
    component: SyntheticDataGeneratorView,
    meta: { requiresAuth: true }
  },
  {
    path: '/drift-detection',
    name: 'DriftDetection',
    component: DriftDetectionView,
    meta: { requiresAuth: true }
  },
  {
    path: '/mathematical-modeling',
    name: 'MathematicalModeling',
    component: MathematicalModelingView,
    meta: { requiresAuth: true }
  },
  {
    path: '/uncertainty',
    name: 'UncertaintyQuantification',
    component: UncertaintyQuantificationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/run-to-failure',
    name: 'RunToFailure',
    component: RunToFailureView,
    meta: { requiresAuth: true }
  },
  {
    path: '/system-health',
    name: 'SystemHealth',
    component: SystemHealthView,
    meta: { requiresAuth: true }
  },
  {
    path: '/prescriptive-maintenance',
    name: 'PrescriptiveMaintenance',
    component: PrescriptiveMaintenanceView,
    meta: { requiresAuth: true }
  },
  {
    path: '/reporting-dashboard',
    name: 'ReportingDashboard',
    component: ReportingDashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/predictive-analytics-dashboard',
    name: 'PredictiveAnalyticsDashboard',
    component: PredictiveAnalyticsDashboardView,
    meta: { requiresAuth: true }
  },
  {
    path: '/model-validation',
    name: 'ModelValidation',
    component: ModelValidationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/model-lifecycle',
    name: 'ModelLifecycle',
    component: ModelLifecycleView,
    meta: { requiresAuth: true }
  },
  // Phase 9 Enterprise Integration Routes
  {
    path: '/tenants',
    name: 'TenantManagement',
    component: TenantManagementView,
    meta: { requiresAuth: true }
  },
  {
    path: '/external-systems',
    name: 'ExternalSystemIntegration',
    component: ExternalSystemIntegrationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/real-time-analytics',
    name: 'RealTimeAnalytics',
    component: RealTimeAnalyticsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/benchmark-validation',
    name: 'BenchmarkValidation',
    component: BenchmarkValidationView,
    meta: { requiresAuth: true }
  },
  {
    path: '/ml-insights',
    name: 'MLInsights',
    component: MLInsightsView,
    meta: { requiresAuth: true }
  },
  {
    path: '/enterprise-dashboard',
    name: 'EnterpriseOperations',
    component: EnterpriseOperationsDashboard,
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }
    return { top: 0 }
  }
})

// Navigation guard
router.beforeEach((to, from, next) => {
  const authStore = useAuthStore()
  
  // Skip auth check for login page
  if (to.path === '/login') {
    next()
    return
  }
  
  // Auth check for protected routes
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next('/login')
    return
  }
  
  next()
})

export default router