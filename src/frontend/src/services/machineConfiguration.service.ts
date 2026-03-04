/**
 * Machine Configuration Service
 * Handles CRUD operations for machine configurations
 */

import axiosClient from '@/api/axiosClient'

export interface MachineConfiguration {
  id?: string
  name?: string
  description?: string
  tags?: string[]
  machineType: string
  degradationModel: {
    type: string
    parameters: Record<string, number>
  }
  failureThresholds: {
    degradationThreshold: number
    temperatureThreshold?: number
    vibrationThreshold?: number
  }
  sensorMappings: Array<{
    sensorType: string
    transferFunction: string
    parameters: Record<string, number>
  }>
  operatingParameters: Record<string, number>
}

/**
 * Get all machine configurations
 */
export async function fetchAllConfigurations(): Promise<MachineConfiguration[]> {
  const response = await axiosClient.get<MachineConfiguration[]>(
    'MachineConfiguration'
  )
  return response.data || []
}

/**
 * Get configuration for a specific machine type
 */
export async function fetchConfiguration(
  machineType: string
): Promise<MachineConfiguration> {
  const response = await axiosClient.get<MachineConfiguration>(
    `/MachineConfiguration/${encodeURIComponent(machineType)}`
  )
  return response.data
}

/**
 * Create or update a machine configuration
 */
export async function saveConfiguration(
  configuration: MachineConfiguration
): Promise<MachineConfiguration> {
  const response = await axiosClient.post<MachineConfiguration>(
    'MachineConfiguration',
    configuration
  )
  return response.data
}

/**
 * Delete a machine configuration
 * Note: Backend currently returns 501 Not Implemented
 */
export async function deleteConfiguration(machineType: string): Promise<void> {
  try {
    await axiosClient.delete(`/MachineConfiguration/${encodeURIComponent(machineType)}`)
  } catch (error: any) {
    if (error.response?.status === 501) {
      throw new Error('Delete functionality not yet implemented on the backend')
    }
    throw error
  }
}

/**
 * Validate a JSON configuration without saving it
 */
export async function validateConfiguration(
  jsonContent: string
): Promise<{ valid: boolean; message: string }> {
  const response = await axiosClient.post<{ valid: boolean; message: string }>(
    '/MachineConfiguration/validate',
    jsonContent,
    {
      headers: {
        'Content-Type': 'application/json'
      }
    }
  )
  return response.data
}
