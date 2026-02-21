/**
 * Store Initializer
 * 
 * Centralized initialization for Pinia stores.
 * Call this from main.ts to set up all stores with initial data.
 */

import { useMachinesStore } from '@/stores/machines.store'
import { useSimulationsStore } from '@/stores/simulations.store'
import { useUIStore } from '@/stores/ui.store'

export async function initializeStores() {
  // Initialize UI store with theme from localStorage
  const uiStore = useUIStore()
  const savedTheme = localStorage.getItem('theme') as 'light' | 'dark' | null
  if (savedTheme) {
    uiStore.setTheme(savedTheme)
  }

  // Pre-load machines on app start
  const machinesStore = useMachinesStore()
  try {
    await machinesStore.loadMachines()
  } catch (error) {
    console.warn('Failed to pre-load machines on app start:', error)
  }

  // Simulations store doesn't need initialization
  // It will be loaded on-demand when users navigate to machine details
}

/**
 * Reset all stores (useful for logout or testing)
 */
export function resetAllStores() {
  const machinesStore = useMachinesStore()
  const simulationsStore = useSimulationsStore()
  const uiStore = useUIStore()

  machinesStore.$reset()
  simulationsStore.$reset()
  uiStore.$reset()
}
