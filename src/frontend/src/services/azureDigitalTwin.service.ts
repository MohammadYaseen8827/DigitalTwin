import axiosClient from '@/api/axiosClient'
import type { Ref } from 'vue'
import { ref } from 'vue'

// Define TypeScript interfaces
export interface DigitalTwinSyncRequest {
  machineId?: string
  lineId?: string
  syncType: 'Machine' | 'ProductionLine' | 'Full'
}

export interface DigitalTwinSyncResult {
  success: boolean
  message: string
  syncedEntities: number
  errors: string[]
  timestamp: string
}

export interface DigitalTwinStatus {
  isConnected: boolean
  lastSync: string
  totalMachinesSynced: number
  totalLinesSynced: number
  connectionStatus: 'Connected' | 'Disconnected' | 'Error'
  adtInstanceUrl: string
}

export interface TwinEntity {
  id: string
  name: string
  type: 'Machine' | 'ProductionLine'
  properties: Record<string, any>
  relationships: string[]
  lastUpdated: string
}

// Azure Digital Twin Service Class
class AzureDigitalTwinService {
  private baseUrl = '/AzureDigitalTwin'

  async syncMachine(machineId: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axiosClient.post<DigitalTwinSyncResult>(`${this.baseUrl}/machines/${machineId}/sync`)
      return response as unknown as DigitalTwinSyncResult
    } catch (error) {
      console.error(`Failed to sync machine ${machineId}:`, error)
      throw error
    }
  }

  async syncProductionLine(lineId: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axiosClient.post<DigitalTwinSyncResult>(`${this.baseUrl}/production-lines/${lineId}/sync`)
      return response as unknown as DigitalTwinSyncResult
    } catch (error) {
      console.error(`Failed to sync production line ${lineId}:`, error)
      throw error
    }
  }

  async upsertTwin(twinData: any): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axiosClient.post<DigitalTwinSyncResult>(`${this.baseUrl}/twins/upsert`, twinData)
      return response as unknown as DigitalTwinSyncResult
    } catch (error) {
      console.error('Failed to upsert twin:', error)
      throw error
    }
  }

  // NOTE: The following 9 methods are removed as they have no corresponding backend endpoints
  // - syncAll
  // - getStatus
  // - getTwinEntities
  // - getMachineTwin
  // - getLineTwin
  // - updateTwinProperties
  // - createRelationship
  // - deleteTwin
  // - queryTwins
  // These can be implemented in future phases when Azure Digital Twin endpoints are available
}

// Create singleton instance
export const azureDigitalTwinService = new AzureDigitalTwinService()

// Composable for Azure Digital Twin management
export function useAzureDigitalTwin() {
  const status: Ref<DigitalTwinStatus | null> = ref(null)
  const entities: Ref<TwinEntity[]> = ref([])
  const loading: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)

  const fetchStatus = async () => {
    // TODO: Implement when backend endpoint is available
    console.warn('fetchStatus: Backend endpoint not implemented')
  }

  const fetchEntities = async () => {
    // TODO: Implement when backend endpoint is available
    console.warn('fetchEntities: Backend endpoint not implemented')
  }

  const syncAll = async () => {
    // TODO: Implement when backend endpoint is available
    console.warn('syncAll: Backend endpoint not implemented')
  }

  const syncMachine = async (machineId: string) => {
    loading.value = true
    error.value = null
    try {
      const result = await azureDigitalTwinService.syncMachine(machineId)
      return result
    } catch (err) {
      error.value = 'Failed to sync machine'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const syncProductionLine = async (lineId: string) => {
    loading.value = true
    error.value = null
    try {
      const result = await azureDigitalTwinService.syncProductionLine(lineId)
      return result
    } catch (err) {
      error.value = 'Failed to sync production line'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    status,
    entities,
    loading,
    error,
    fetchStatus,
    fetchEntities,
    syncMachine,
    syncProductionLine
  }
}