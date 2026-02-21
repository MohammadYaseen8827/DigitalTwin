import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { SimulationStateDto } from '@/services/simulation.service'
import {
  listSimulations,
  createSimulation,
  getSimulation,
  runSimulationStep,
  pauseSimulation,
  resumeSimulation,
  cancelSimulation
} from '@/services/simulation.service'
import { useToast } from '@/lib/magic-mcp-ui'

export const useSimulationsStore = defineStore('simulations', () => {
  const toast = useToast()
  
  // State
  const simulations = ref<Record<string, SimulationStateDto>>({})
  const loading = ref(false)
  const error = ref<string | null>(null)

  // Getters
  const getSimulationByMachineId = computed(() => {
    return (machineId: string) => simulations.value[machineId]
  })

  const activeSimulations = computed(() => 
    (Object.values(simulations.value) as SimulationStateDto[]).filter(s => 
      s.status === 'running' || s.status === 'paused'
    )
  )

  const completedSimulations = computed(() => 
    (Object.values(simulations.value) as SimulationStateDto[]).filter(s => s.status === 'completed')
  )

  const runningSimulations = computed(() => 
    (Object.values(simulations.value) as SimulationStateDto[]).filter(s => s.status === 'running')
  )

  // Actions
  async function loadSimulations(machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const machineSimulations = await listSimulations(machineId)
      
      // Store the most recent simulation for this machine
      if (machineSimulations.length > 0) {
        const latest = machineSimulations.sort((a, b) => 
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        )[0]
        simulations.value[machineId] = latest
      }
      
      return machineSimulations
    } catch (err) {
      error.value = `Failed to load simulations for machine ${machineId}`
      console.error('Failed to load simulations:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function loadSimulation(simulationId: string, machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const simulation = await getSimulation(simulationId, machineId)
      simulations.value[machineId] = simulation
      return simulation
    } catch (err) {
      error.value = `Failed to load simulation ${simulationId}`
      console.error('Failed to load simulation:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function createNewSimulation(machineId: string, parameters: Record<string, any>) {
    loading.value = true
    error.value = null
    
    try {
      const simulation = await createSimulation(machineId, parameters)
      simulations.value[machineId] = simulation
      return simulation
    } catch (err) {
      error.value = 'Failed to create simulation'
      console.error('Failed to create simulation:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function runStep(simulationId: string, machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const result = await runSimulationStep(simulationId, machineId)
      
      // Refresh simulation state
      await loadSimulation(simulationId, machineId)
      
      return result
    } catch (err) {
      error.value = 'Failed to run simulation step'
      console.error('Failed to run simulation step:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function pause(simulationId: string, machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const simulation = await pauseSimulation(simulationId, machineId)
      simulations.value[machineId] = simulation
      return simulation
    } catch (err) {
      error.value = 'Failed to pause simulation'
      console.error('Failed to pause simulation:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function resume(simulationId: string, machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const simulation = await resumeSimulation(simulationId, machineId)
      simulations.value[machineId] = simulation
      return simulation
    } catch (err) {
      error.value = 'Failed to resume simulation'
      console.error('Failed to resume simulation:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  async function cancel(simulationId: string, machineId: string) {
    loading.value = true
    error.value = null
    
    try {
      const simulation = await cancelSimulation(simulationId, machineId)
      simulations.value[machineId] = simulation
      return simulation
    } catch (err) {
      error.value = 'Failed to cancel simulation'
      console.error('Failed to cancel simulation:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  function clearError() {
    error.value = null
  }

  function $reset() {
    simulations.value = {}
    loading.value = false
    error.value = null
  }

  return {
    // State
    simulations,
    loading,
    error,
    
    // Getters
    getSimulationByMachineId,
    activeSimulations,
    completedSimulations,
    runningSimulations,
    
    // Actions
    loadSimulations,
    loadSimulation,
    createNewSimulation,
    runStep,
    pause,
    resume,
    cancel,
    clearError,
    $reset
  }
})
