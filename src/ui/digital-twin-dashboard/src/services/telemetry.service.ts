import axiosClient from '@/api/axiosClient'
import { useToast } from '@/lib/magic-mcp-ui'
import type { TelemetryDto } from '@/api/types'

const toast = useToast()

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
  try {
    await axiosClient.post('/Telemetry', payload)
    toast.success('Telemetry ingested successfully')
  } catch (error) {
    console.error('Failed to ingest telemetry:', error)
    const message = (error as any)?.response?.data?.message ?? 'Failed to ingest telemetry'
    toast.error(message)
    throw error
  }
}
