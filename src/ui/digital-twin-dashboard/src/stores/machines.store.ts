import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { MachineDto } from '@/api/types'
import { fetchMachines, fetchMachine, createMachine, updateMachine, deleteMachine } from '@/services/machines.service'
import { useToast } from '@/lib/magic-mcp-ui'

export const useMachinesStore = defineStore('machines', () => {
  const toast = useToast()

  // State
  const machines = ref<MachineDto[]>([])
  const selectedMachine = ref<MachineDto | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const lastFetched = ref<Date | null>(null)

  // Getters
  const totalMachines = computed(() => machines.value.length)

  const machinesByStatus = computed(() => {
    return machines.value.reduce((acc: Record<string, MachineDto[]>, machine: MachineDto) => {
      const status = String(machine.status).toLowerCase()
      if (!acc[status]) {
        acc[status] = []
      }
      acc[status].push(machine)
      return acc
    }, {} as Record<string, MachineDto[]>)
  })

  const operationalMachines = computed(() =>
    machines.value.filter((m: MachineDto) => String(m.status).toLowerCase() === 'operational')
  )

  const criticalMachines = computed(() =>
    machines.value.filter((m: MachineDto) => String(m.status).toLowerCase() === 'critical')
  )

  const getMachineById = computed(() => {
    return (id: string) => machines.value.find((m: MachineDto) => m.id === id)
  })

  // Actions
  async function loadMachines(force = false) {
    // Use cache if available and not forcing refresh
    if (!force && machines.value.length > 0 && lastFetched.value) {
      const cacheAge = Date.now() - lastFetched.value.getTime()
      if (cacheAge < 60000) { // 1 minute cache
        return
      }
    }

    loading.value = true
    error.value = null

    try {
      machines.value = await fetchMachines()
      lastFetched.value = new Date()
    } catch (err) {
      error.value = 'Failed to load machines'
      console.error('Failed to load machines:', err)
      toast.error('Unable to load machines')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function loadMachine(id: string) {
    loading.value = true
    error.value = null

    try {
      const machine = await fetchMachine(id)
      selectedMachine.value = machine

      // Update in list if exists
      const index = machines.value.findIndex((m: MachineDto) => m.id === id)
      if (index !== -1) {
        machines.value[index] = machine
      }

      return machine
    } catch (err) {
      error.value = `Failed to load machine ${id}`
      console.error('Failed to load machine:', err)
      toast.error('Unable to load machine details')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function addMachine(payload: any) {
    loading.value = true
    error.value = null

    try {
      const newMachine = await createMachine(payload)
      machines.value.push(newMachine)
      toast.success('Machine created successfully')
      return newMachine
    } catch (err) {
      error.value = 'Failed to create machine'
      console.error('Failed to create machine:', err)
      toast.error('Unable to create machine')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function modifyMachine(id: string, payload: any) {
    loading.value = true
    error.value = null

    try {
      const updatedMachine = await updateMachine(id, payload)

      // Update in list
      const index = machines.value.findIndex((m: MachineDto) => m.id === id)
      if (index !== -1) {
        machines.value[index] = updatedMachine
      }

      // Update selected if it's the same machine
      if (selectedMachine.value?.id === id) {
        selectedMachine.value = updatedMachine
      }

      toast.success('Machine updated successfully')
      return updatedMachine
    } catch (err) {
      error.value = 'Failed to update machine'
      console.error('Failed to update machine:', err)
      toast.error('Unable to update machine')
      throw err
    } finally {
      loading.value = false
    }
  }

  async function removeMachine(id: string) {
    loading.value = true
    error.value = null

    try {
      await deleteMachine(id)

      // Remove from list
      machines.value = machines.value.filter((m: MachineDto) => m.id !== id)

      // Clear selected if it's the same machine
      if (selectedMachine.value?.id === id) {
        selectedMachine.value = null
      }

      toast.success('Machine deleted successfully')
    } catch (err) {
      error.value = 'Failed to delete machine'
      console.error('Failed to delete machine:', err)
      toast.error('Unable to delete machine')
      throw err
    } finally {
      loading.value = false
    }
  }

  function selectMachine(machine: MachineDto | null) {
    selectedMachine.value = machine
  }

  function clearError() {
    error.value = null
  }

  function updatePrediction(machineId: string, prediction: any) {
    const index = machines.value.findIndex((m: MachineDto) => m.id === machineId)
    if (index !== -1) {
      machines.value[index] = {
        ...machines.value[index],
        remainingUsefulLifeDays: prediction.remainingUsefulLifeDays,
        failureProbability: prediction.failureProbability,
        healthStatus: prediction.healthStatus
      }
    }

    if (selectedMachine.value?.id === machineId) {
      selectedMachine.value = {
        ...selectedMachine.value,
        remainingUsefulLifeDays: prediction.remainingUsefulLifeDays,
        failureProbability: prediction.failureProbability,
        healthStatus: prediction.healthStatus
      }
    }
  }

  function patchMachine(id: string, partial: Partial<MachineDto>) {
    const index = machines.value.findIndex((m: MachineDto) => m.id === id)
    if (index !== -1) {
      machines.value[index] = { ...machines.value[index], ...partial }
    }
    if (selectedMachine.value?.id === id) {
      selectedMachine.value = { ...selectedMachine.value, ...partial }
    }
  }

  function $reset() {
    machines.value = []
    selectedMachine.value = null
    loading.value = false
    error.value = null
    lastFetched.value = null
  }

  return {
    // State
    machines,
    selectedMachine,
    loading,
    error,
    lastFetched,

    // Getters
    totalMachines,
    machinesByStatus,
    operationalMachines,
    criticalMachines,
    getMachineById,

    // Actions
    loadMachines,
    loadMachine,
    addMachine,
    modifyMachine,
    removeMachine,
    selectMachine,
    updatePrediction,
    patchMachine,
    clearError,
    $reset
  }
})
