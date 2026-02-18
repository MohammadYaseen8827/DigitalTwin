/**
 * Run-to-Failure Simulation Service
 * Handles run-to-failure simulation operations and trajectory generation
 */

import axiosClient from '@/api/axiosClient'

export interface RunToFailureOptions {
  maxSteps?: number
  maxSimulationTime?: string // TimeSpan format
  stepInterval?: string // TimeSpan format
  generateTelemetry?: boolean
  storeTrajectory?: boolean
  randomSeed?: number
}

export interface DegradationSnapshot {
  step: number
  timestamp: string
  degradationState: number
  sensorReadings: Record<string, number>
}

export interface RunToFailureResult {
  machineId: string
  timeToFailure: string // TimeSpan format
  stepsToFailure: number
  finalDegradationState: number
  trajectory: DegradationSnapshot[]
  generatedTelemetry: any[] // TelemetryData[]
  reachedFailureThreshold: boolean
  terminationReason?: string
}

export interface DegradationTrajectory {
  machineType: string
  trajectoryId: string
  snapshot: DegradationSnapshot[]
  timeToFailure: string
}

export interface GenerateTrajectoriesRequest {
  machineType: string
  count: number
  options?: RunToFailureOptions
}

/**
 * Run a single run-to-failure simulation for a machine
 */
export async function runToFailureSimulation(
  machineId: string,
  options?: RunToFailureOptions
): Promise<RunToFailureResult> {
  return axiosClient.post<RunToFailureResult, RunToFailureResult>(
    `/RunToFailure/${machineId}`,
    options || {}
  )
}

/**
 * Generate multiple degradation trajectories for a machine type
 */
export async function generateTrajectories(
  request: GenerateTrajectoriesRequest
): Promise<DegradationTrajectory[]> {
  return axiosClient.post<DegradationTrajectory[], DegradationTrajectory[]>(
    '/RunToFailure/generate-trajectories',
    request
  )
}

/**
 * Get run-to-failure results for a machine (if stored)
 * Note: Backend currently returns 501 Not Implemented
 */
export async function getRunToFailureResults(machineId: string): Promise<any> {
  try {
    return await axiosClient.get(`/RunToFailure/${machineId}/results`)
  } catch (error: any) {
    if (error.response?.status === 501) {
      throw new Error('Result storage not yet implemented on the backend')
    }
    throw error
  }
}
