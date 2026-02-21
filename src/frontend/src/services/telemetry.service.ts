import axiosClient from '@/api/axiosClient'
import type { TelemetryDto } from '@/api/types'

export interface TelemetryRangeParams {
  range?: string
}

export interface MachineTelemetryParams extends TelemetryRangeParams {
  take?: number
}

export interface RecentTelemetryParams extends TelemetryRangeParams {
  machineId?: string
  limit?: number
}

export interface TelemetryIngestDto {
  machineId: string
  timestamp: string
  metrics: Record<string, number>
  metadata?: Record<string, unknown>
}

export interface TelemetryMetricsDto {
  machineId: string
  timestamp: string
  temperature?: number
  vibration?: number
  pressure?: number
  rpm?: number
  humidity?: number
  powerConsumption?: number
  [key: string]: unknown
}

export interface TelemetrySearchParams {
  query: string
  range?: string
}

export async function fetchTelemetryByMachine(
  machineId: string,
  params: MachineTelemetryParams = {}
): Promise<TelemetryDto[]> {
  const { range, take = 500 } = params

  return axiosClient.get<TelemetryDto[], TelemetryDto[]>(
    `/Telemetry/${encodeURIComponent(machineId)}`,
    {
      params: {
        take,
        range
      }
    }
  )
}

export async function fetchRecentTelemetry(params: RecentTelemetryParams = {}): Promise<TelemetryDto[]> {
  const { range, machineId, limit = 100 } = params

  return axiosClient.get<TelemetryDto[], TelemetryDto[]>('/Telemetry/recent', {
    params: {
      limit,
      range,
      machineId
    }
  })
}

export async function ingestTelemetry(payload: TelemetryIngestDto): Promise<void> {
  await axiosClient.post('/Telemetry', payload)
}

/**
 * Get the latest telemetry reading for a specific machine.
 */
export async function fetchLatestTelemetry(machineId: string): Promise<TelemetryDto | null> {
  try {
    const response = await axiosClient.get<TelemetryDto>(
      `/Telemetry/${encodeURIComponent(machineId)}/latest`
    )
    return response as unknown as TelemetryDto
  } catch (error: any) {
    if (error?.response?.status === 404) {
      return null
    }
    throw error
  }
}

/**
 * Get the latest flattened telemetry metrics for a machine.
 */
export async function fetchLatestMetrics(machineId: string): Promise<TelemetryMetricsDto | null> {
  try {
    const response = await axiosClient.get<TelemetryMetricsDto>(
      `/Telemetry/${encodeURIComponent(machineId)}/latest/metrics`
    )
    return response as unknown as TelemetryMetricsDto
  } catch (error: any) {
    if (error?.response?.status === 404) {
      return null
    }
    throw error
  }
}

/**
 * Search telemetry data across all machines.
 */
export async function searchTelemetry(params: TelemetrySearchParams): Promise<TelemetryDto[]> {
  const response = await axiosClient.get<TelemetryDto[]>('/Telemetry/search', {
    params: {
      query: params.query,
      range: params.range
    }
  })
  return response as unknown as TelemetryDto[]
}

// Export service object for convenience
export const telemetryService = {
  fetchTelemetryByMachine,
  fetchRecentTelemetry,
  ingestTelemetry,
  fetchLatestTelemetry,
  fetchLatestMetrics,
  searchTelemetry
}

export default telemetryService
