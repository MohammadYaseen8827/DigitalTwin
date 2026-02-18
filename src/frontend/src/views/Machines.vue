<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import SidebarNav from '@/components/dashboard/SidebarNav.vue'
import HeaderBar from '@/components/dashboard/HeaderBar.vue'
import MachineCard from '@/components/machines/MachineCard.vue'
import Card from '@/components/common/Card.vue'
import Button from '@/components/common/Button.vue'
import EmptyState from '@/components/common/EmptyState.vue'
import Spinner from '@/components/common/Spinner.vue'
import { useMachinesStore } from '@/stores/machines'
import type { Machine } from '@/components/dashboard/MachineStatusGrid.vue'

const router = useRouter()
const machinesStore = useMachinesStore()
const isLoading = ref(true)
const sidebarCollapsed = ref(false)
const searchQuery = ref('')
const statusFilter = ref<'all' | 'healthy' | 'warning' | 'critical'>('all')

// Convert store machines to Machine type for the view
const machines = computed<Machine[]>(() => {
  return machinesStore.machines.map(m => ({
    id: m.id,
    name: m.name,
    status: m.status === 'Running' ? 'healthy' : m.status === 'Maintenance' ? 'warning' : m.status === 'Error' ? 'critical' : 'unknown',
    healthScore: m.healthScore,
    rul: 120, // Default RUL - would come from predictions store
    temperature: 45, // Default temp - would come from telemetry store
    vibration: 0.5, // Default vibration - would come from telemetry store
    lastUpdate: new Date().toISOString(),
    location: m.location || 'Unknown',
    type: m.type || 'Unknown'
  }))
})

const filteredMachines = computed(() => {
  return machines.value.filter(m => {
    const matchesSearch = m.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                         m.location?.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
                         m.type?.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesStatus = statusFilter.value === 'all' || m.status === statusFilter.value
    return matchesSearch && matchesStatus
  })
})

const stats = computed(() => [
  { label: 'Total Machines', value: machines.value.length },
  { label: 'Healthy', value: machines.value.filter(m => m.status === 'healthy').length, status: 'normal' as const },
  { label: 'Warning', value: machines.value.filter(m => m.status === 'warning').length, status: 'warning' as const },
  { label: 'Critical', value: machines.value.filter(m => m.status === 'critical').length, status: 'critical' as const }
])

const handleMachineClick = (machine: Machine) => {
  router.push(`/machines/${machine.id}`)
}

const handleRefresh = async () => {
  isLoading.value = true
  try {
    await machinesStore.fetchMachines()
  } finally {
    isLoading.value = false
  }
}

onMounted(async () => {
  try {
    await machinesStore.fetchMachines()
  } catch (err) {
    console.error('Error loading machines:', err)
  } finally {
    isLoading.value = false
  }
})
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <SidebarNav v-model:collapsed="sidebarCollapsed" />
    
    <div :class="['transition-all duration-300', sidebarCollapsed ? 'ml-16' : 'ml-64']">
      <HeaderBar title="Machines" subtitle="Manage and monitor your machines" />
      
      <main class="p-6">
        <div v-if="isLoading" class="flex items-center justify-center h-64">
          <Spinner size="lg" />
        </div>
        
        <div v-else class="space-y-6">
          <!-- Stats Row -->
          <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card v-for="stat in stats" :key="stat.label" class="p-4">
              <div class="flex items-center justify-between">
                <span class="text-sm text-gray-600">{{ stat.label }}</span>
                <span :class="[
                  'text-2xl font-bold',
                  stat.status === 'critical' ? 'text-red-600' : stat.status === 'warning' ? 'text-yellow-600' : 'text-green-600'
                ]">{{ stat.value }}</span>
              </div>
            </Card>
          </div>
          
          <Card :padding="'sm'">
            <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div class="flex items-center gap-4">
                <div class="relative">
                  <input
                    v-model="searchQuery"
                    type="text"
                    placeholder="Search machines..."
                    class="pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent w-64"
                  />
                  <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                  </svg>
                </div>
                
                <div class="flex items-center gap-2">
                  <button
                    v-for="status in ['all', 'healthy', 'warning', 'critical']"
                    :key="status"
                    :class="[
                      'px-3 py-1.5 rounded-lg text-sm font-medium transition-colors',
                      statusFilter === status
                        ? 'bg-primary-600 text-white'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    ]"
                    @click="statusFilter = status as typeof statusFilter"
                  >
                    {{ status.charAt(0).toUpperCase() + status.slice(1) }}
                  </button>
                </div>
              </div>
              
              <Button variant="secondary" @click="handleRefresh">
                <svg class="w-4 h-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                Refresh
              </Button>
            </div>
          </Card>
          
          <div v-if="filteredMachines.length === 0">
            <EmptyState
              title="No machines found"
              description="Try adjusting your search or filters"
              action-label="Clear filters"
              @action="() => { searchQuery = ''; statusFilter = 'all' }"
            />
          </div>
          
          <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            <MachineCard
              v-for="machine in filteredMachines"
              :key="machine.id"
              :machine="machine"
              @click="handleMachineClick"
            />
          </div>
        </div>
      </main>
    </div>
  </div>
</template>
