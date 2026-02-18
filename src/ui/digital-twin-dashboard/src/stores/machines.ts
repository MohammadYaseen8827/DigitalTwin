import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { machinesService } from '@/services/machines.service'
import type { MachineDto } from '@/api/types'

export const useMachinesStore = defineStore('machines', () => {
  const machines = ref<MachineDto[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)
  const selectedMachine = ref<MachineDto | null>(null)

  const machinesByStatus = computed(() => {
    const grouped = machines.value.reduce((acc, machine) => {
      const status = machine.status || 'unknown'
      if (!acc[status]) {
        acc[status] = []
      }
      acc[status].push(machine)
      return acc
    }, {} as Record<string, MachineDto[]>)
    
    return grouped
  })

  const onlineMachines = computed(() => machinesByStatus.value['online'] || [])
  const offlineMachines = computed(() => machinesByStatus.value['offline'] || [])
  const maintenanceMachines = computed(() => machinesByStatus.value['maintenance'] || [])
  const errorMachines = computed(() => machinesByStatus.value['error'] || [])
  const degradedMachines = computed(() => machinesByStatus.value['degraded'] || [])

  const healthyMachines = computed(() => {
    return machines.value.filter(machine => machine.status === 'online')
  })

  const unhealthyMachines = computed(() => {
    return machines.value.filter(machine => 
      machine.status === 'offline' || 
      machine.status === 'error' || 
      machine.status === 'degraded'
    )
  })

  const fetchMachines = async () => {
    loading.value = true
    error.value = null
    
    try {
      machines.value = await machinesService.getMachines()
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to fetch machines'
    } finally {
      loading.value = false
    }
  }

  const selectMachine = (machine: MachineDto) => {
    selectedMachine.value = machine
  }

  const clearSelection = () => {
    selectedMachine.value = null
  }

  // Actions
  const refreshMachines = async () => {
    await fetchMachines()
  }

  return {
    // State
    machines: readonly(machines),
    loading: readonly(loading),
    error: readonly(error),
    selectedMachine: readonly(selectedMachine),
    
    // Computed
    machinesByStatus,
    onlineMachines,
    offlineMachines,
    maintenanceMachines,
    errorMachines,
    degradedMachines,
    healthyMachines,
    unhealthyMachines,
    
    // Actions
    fetchMachines,
    selectMachine,
    clearSelection
  }
})
