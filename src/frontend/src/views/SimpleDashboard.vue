<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import SidebarNav from '@/components/dashboard/SidebarNav.vue'
import HeaderBar from '@/components/dashboard/HeaderBar.vue'
import OverallHealthCard from '@/components/dashboard/OverallHealthCard.vue'
import ActiveAlertsWidget from '@/components/dashboard/ActiveAlertsWidget.vue'
import MachineStatusGrid from '@/components/dashboard/MachineStatusGrid.vue'
import Card from '@/components/common/Card.vue'
import Spinner from '@/components/common/Spinner.vue'
import { useMachinesStore } from '@/stores/machines'
import { useAlertsStore } from '@/stores/alerts'
import type { MachineDto, AlertDto, EquipmentStatus } from '@/api/types'

const router = useRouter()
const machinesStore = useMachinesStore()
const alertsStore = useAlertsStore()
const isLoading = ref(true)
const sidebarCollapsed = ref(false)

// Computed properties for real data from stores
const healthScore = computed(() => machinesStore.averageHealthScore || 87)
const totalMachines = computed(() => machinesStore.machines.length || 12)
const healthyMachines = computed(() => machinesStore.runningMachines.length)
const warningMachines = computed(() => machinesStore.maintenanceMachines.length)
const criticalMachines = computed(() => machinesStore.errorMachines.length)

const machines = computed(() => machinesStore.machines.map((m: MachineDto) => ({
    id: m.id,
    name: m.name,
    status: convertStatus(m.status) as 'healthy' | 'warning' | 'critical',
    healthScore: m.healthStatus ? healthClassificationToScore(m.healthStatus) : 50,
    rul: m.remainingUsefulLifeDays || 120,
    temperature: 45,
    vibration: 0.5,
    lastUpdate: new Date().toISOString()
})))

// Helper functions for status conversion
function convertStatus(status: EquipmentStatus): 'healthy' | 'warning' | 'error' {
  const statusStr = String(status)
  if (statusStr === 'Operational') return 'healthy'
  if (statusStr === 'Maintenance' || statusStr === 'Warning') return 'warning'
  return 'error'
}

function healthClassificationToScore(health: string | number): number {
  const scores: Record<string, number> = {
    'Healthy': 100,
    'Normal': 80,
    'MinorDegradation': 60,
    'SignificantDegradation': 40,
    'FailureImminent': 20
  }
  return scores[health as string] || 50
}

const alerts = computed(() => alertsStore.activeAlerts.map((a: AlertDto) => ({
    id: a.id,
    machine: machinesStore.machines.find((m: MachineDto) => m.id === a.machineId)?.name || 'Unknown',
    type: convertAlertSeverity(a.severity),
    message: a.description || a.message,
    timestamp: a.createdAt
})))

function convertAlertSeverity(severity: string): 'critical' | 'warning' | 'info' {
  if (severity === 'Critical' || severity === 'Error') return 'critical'
  if (severity === 'Warning') return 'warning'
  return 'info'
}

const handleMachineClick = (machine: { id: string; name: string }) => {
  router.push(`/machines/${machine.id}`)
}

const handleQuickAction = (actionId: string) => {
  switch (actionId) {
    case 'view-machines':
      router.push('/machines')
      break
    case 'view-alerts':
      router.push('/alerts')
      break
    case 'settings':
      router.push('/settings')
      break
    case 'export-report':
      console.log('Export report')
      break
    case 'run-prediction':
      router.push('/predictions')
      break
    case 'schedule-maintenance':
      console.log('Schedule maintenance')
      break
  }
}

onMounted(async () => {
  isLoading.value = true
  try {
    await Promise.all([
      machinesStore.fetchMachines(),
      alertsStore.fetchAlerts()
    ])
  } catch (err) {
    console.error('Error loading dashboard data:', err)
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <SidebarNav v-model:collapsed="sidebarCollapsed" />
    
    <div :class="['transition-all duration-300', sidebarCollapsed ? 'ml-16' : 'ml-64']">
      <HeaderBar title="Dashboard" subtitle="Overview of your digital twin system" />
      
      <main class="p-6">
        <!-- Loading State -->
        <div v-if="isLoading" class="flex items-center justify-center h-64">
          <Spinner size="lg" />
        </div>
        
        <!-- Dashboard Content -->
        <div v-else class="space-y-6">
          <!-- Top Stats Row -->
          <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <OverallHealthCard
              :health-score="healthScore"
              :total-machines="totalMachines"
              :healthy-machines="healthyMachines"
              :warning-machines="warningMachines"
              :critical-machines="criticalMachines"
            />
            
            <ActiveAlertsWidget :alerts="alerts" />
            
            <Card>
              <template #header>
                <h2 class="text-lg font-semibold text-gray-900">Quick Actions</h2>
              </template>
              <template #body>
                <div class="grid grid-cols-2 gap-3">
                  <button 
                    @click="handleQuickAction('view-machines')"
                    class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm font-medium"
                  >
                    View Machines
                  </button>
                  <button 
                    @click="handleQuickAction('view-alerts')"
                    class="px-4 py-2 bg-yellow-500 text-white rounded-lg hover:bg-yellow-600 transition-colors text-sm font-medium"
                  >
                    View Alerts
                  </button>
                  <button 
                    @click="handleQuickAction('run-prediction')"
                    class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors text-sm font-medium"
                  >
                    Run Prediction
                  </button>
                  <button 
                    @click="handleQuickAction('settings')"
                    class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors text-sm font-medium"
                  >
                    Settings
                  </button>
                </div>
              </template>
            </Card>
          </div>
          
          <!-- Machine Status Grid -->
          <MachineStatusGrid :machines="machines" @machine-click="handleMachineClick" />
          
          <!-- System Overview -->
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <template #header>
                <h2 class="text-lg font-semibold text-gray-900">Recent Activity</h2>
              </template>
              <template #body>
                <div class="space-y-3">
                  <div v-if="alerts.length === 0" class="text-center py-8 text-gray-500">
                    No recent activity
                  </div>
                  <div v-for="alert in alerts.slice(0, 4)" :key="alert.id" class="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                    <div :class="[
                      'w-2 h-2 rounded-full mt-2',
                      alert.type === 'critical' ? 'bg-red-500' : alert.type === 'warning' ? 'bg-yellow-500' : 'bg-blue-500'
                    ]" />
                    <div>
                      <p class="text-sm font-medium text-gray-900">{{ alert.message }}</p>
                      <p class="text-xs text-gray-500">{{ alert.machine }} • {{ new Date(alert.timestamp).toLocaleString() }}</p>
                    </div>
                  </div>
                </div>
              </template>
            </Card>
            
            <Card>
              <template #header>
                <h2 class="text-lg font-semibold text-gray-900">System Overview</h2>
              </template>
              <template #body>
                <div class="space-y-4">
                  <div class="flex items-center justify-between py-3 border-b border-gray-100">
                    <span class="text-sm text-gray-600">Active Machines</span>
                    <span class="font-semibold text-gray-900">{{ totalMachines }}</span>
                  </div>
                  <div class="flex items-center justify-between py-3 border-b border-gray-100">
                    <span class="text-sm text-gray-600">Last Data Sync</span>
                    <span class="font-semibold text-gray-900">Just now</span>
                  </div>
                  <div class="flex items-center justify-between py-3 border-b border-gray-100">
                    <span class="text-sm text-gray-600">SignalR Connection</span>
                    <span class="inline-flex items-center gap-2">
                      <span class="w-2 h-2 bg-green-500 rounded-full animate-pulse" />
                      <span class="text-sm text-gray-900">Connected</span>
                    </span>
                  </div>
                  <div class="flex items-center justify-between py-3">
                    <span class="text-sm text-gray-600">Uptime</span>
                    <span class="font-semibold text-gray-900">99.9%</span>
                  </div>
                </div>
              </template>
            </Card>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
