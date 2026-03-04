import axiosClient from '@/api/axiosClient'
import { retryWithBackoff, type RetryOptions } from '@/composables/useRetryBackoff'
import { useToast } from '@/lib/magic-mcp-ui'
import { errorReporter } from './errorReporter.service'
import type { SimulationStatusDto } from '@/api/types'

const toast = useToast()

const RETRYABLE_STATUS_CODES = new Set([408, 425, 429, 500, 502, 503, 504])

interface AxiosLikeError {
  isAxiosError?: boolean
  response?: {
    status?: number
  }
}

function isAxiosLikeError(error: unknown): error is AxiosLikeError {
  if (typeof error !== 'object' || error === null) {
    return false
  }

  return 'isAxiosError' in error
}

const DEFAULT_RETRY_OPTIONS: RetryOptions = {
  retries: 2,
  baseDelay: 700,
  maxDelay: 5000,
  jitter: true,
  shouldRetry: error => {
    if (isAxiosLikeError(error)) {
      const status = error.response?.status
      if (status === undefined) {
        return true
      }
      return RETRYABLE_STATUS_CODES.has(status)
    }
    return true
  }
}

async function withRetry<T>(
  operation: (signal: AbortSignal) => Promise<T>,
  options: RetryOptions = {}
): Promise<T> {
  return retryWithBackoff(operation, { ...DEFAULT_RETRY_OPTIONS, ...options })
}

export interface SimulationStateDto {
  id: string
  machineId: string
  status: 'idle' | 'running' | 'paused' | 'completed' | 'failed' | 'cancelled'
  currentStep: number
  totalSteps: number
  createdAt: string
  updatedAt?: string
  completedAt?: string
  parameters?: Record<string, unknown>
  metrics?: Record<string, unknown>
}

export interface SimulationResultDto {
  simulationId: string
  machineId: string
  step: number
  data: Record<string, unknown>
  timestamp: string
}

export type { SimulationStatusDto as SimulationStatus }

/**
 * Create a new simulation for a machine
 */
export async function createSimulation(
  machineId: string,
  parameters: Record<string, object> = {}
): Promise<SimulationStateDto> {
  try {
    const response = await withRetry<SimulationStateDto>(signal =>
      axiosClient.post(
        'Simulation',
        parameters,
        {
          params: { machineId },
          signal
        }
      )
    )
    toast.success('Simulation created successfully')
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to create simulation for machine ${machineId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to create simulation'
    toast.error(message)
    throw error
  }
}

/**
 * Get simulation state by ID
 */
export async function getSimulation(
  simulationId: string,
  machineId: string
): Promise<SimulationStateDto> {
  try {
    return await withRetry<SimulationStateDto>(signal =>
      axiosClient.get(
        `/Simulation/${encodeURIComponent(simulationId)}`,
        {
          params: { machineId },
          signal
        }
      ) as any
    )
  } catch (error) {
    errorReporter.error(`Failed to get simulation ${simulationId}:`, error)
    throw error
  }
}

/**
 * Run the next step of a simulation
 */
export async function runSimulationStep(
  simulationId: string,
  machineId: string
): Promise<SimulationResultDto> {
  try {
    const response = await withRetry<SimulationResultDto>(signal =>
      axiosClient.post(
        `/Simulation/${encodeURIComponent(simulationId)}/run`,
        {},
        {
          params: { machineId },
          signal
        }
      )
    )
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to run simulation step for ${simulationId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to run simulation'
    toast.error(message)
    throw error
  }
}

/**
 * Pause a running simulation
 */
export async function pauseSimulation(
  simulationId: string,
  machineId: string
): Promise<SimulationStateDto> {
  try {
    const response = await withRetry<SimulationStateDto>(signal =>
      axiosClient.post(
        `/Simulation/${encodeURIComponent(simulationId)}/pause`,
        {},
        {
          params: { machineId },
          signal
        }
      )
    )
    toast.success('Simulation paused')
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to pause simulation ${simulationId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to pause simulation'
    toast.error(message)
    throw error
  }
}

/**
 * Resume a paused simulation
 */
export async function resumeSimulation(
  simulationId: string,
  machineId: string
): Promise<SimulationStateDto> {
  try {
    const response = await withRetry<SimulationStateDto>(signal =>
      axiosClient.post(
        `/Simulation/${encodeURIComponent(simulationId)}/resume`,
        {},
        {
          params: { machineId },
          signal
        }
      )
    )
    toast.success('Simulation resumed')
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to resume simulation ${simulationId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to resume simulation'
    toast.error(message)
    throw error
  }
}

/**
 * Cancel a running simulation
 */
export async function cancelSimulation(
  simulationId: string,
  machineId: string
): Promise<SimulationStateDto> {
  try {
    const response = await withRetry<SimulationStateDto>(signal =>
      axiosClient.post(
        `/Simulation/${encodeURIComponent(simulationId)}/cancel`,
        {},
        {
          params: { machineId },
          signal
        }
      )
    )
    toast.success('Simulation cancelled')
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to cancel simulation ${simulationId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to cancel simulation'
    toast.error(message)
    throw error
  }
}

/**
 * Cancel a running simulation (alternative endpoint)
 */
export async function cancelSimulationById(
  simulationId: string,
  machineId: string
): Promise<SimulationStateDto> {
  try {
    const response = await withRetry<SimulationStateDto>(signal =>
      axiosClient.post(
        `/Simulation/${encodeURIComponent(simulationId)}/cancel`,
        {},
        {
          params: { machineId },
          signal
        }
      )
    )
    toast.success('Simulation cancelled')
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to cancel simulation ${simulationId}:`, error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to cancel simulation'
    toast.error(message)
    throw error
  }
}

/**
 * List all simulations for a machine
 */
export async function listSimulations(
  machineId: string
): Promise<SimulationStateDto[]> {
  try {
    const response = await withRetry<SimulationStateDto[]>(signal =>
      axiosClient.get(
        'Simulation',
        {
          params: { machineId },
          signal
        }
      )
    )
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to list simulations for machine ${machineId}:`, error)
    return []
  }
}

/**
 * Get simulation status for a specific machine
 */
export async function getSimulationStatus(
  machineId: string
): Promise<SimulationStatusDto> {
  try {
    const response = await withRetry<SimulationStatusDto>(signal =>
      axiosClient.get(
        `/Simulation/status/${encodeURIComponent(machineId)}`,
        {
          signal
        }
      )
    )
    return response as any
  } catch (error) {
    errorReporter.error(`Failed to get simulation status for machine ${machineId}:`, error)
    return {
      machineId,
      isRunning: false,
      isScheduled: false,
      status: 'idle',
      error: 'Failed to fetch status'
    }
  }
}

/**
 * Get simulation status for all machines
 */
export async function getAllSimulationStatuses(): Promise<Record<string, SimulationStatusDto>> {
  try {
    const response = await withRetry<Record<string, SimulationStatusDto>>(signal =>
      axiosClient.get('Simulation/status', {
        signal
      })
    )
    return response as any
  } catch (error) {
    errorReporter.error('Failed to get all simulation statuses:', error)
    return {}
  }
}
