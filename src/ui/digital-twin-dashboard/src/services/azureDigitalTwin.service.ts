import axios from 'axios'
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
  private baseUrl = '/api/azuredigitaltwin'

  async syncMachine(machineId: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.post<DigitalTwinSyncResult>(`${this.baseUrl}/machines/${machineId}/sync`)
      return response.data
    } catch (error) {
      console.error(`Failed to sync machine ${machineId}:`, error)
      throw error
    }
  }

  async syncProductionLine(lineId: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.post<DigitalTwinSyncResult>(`${this.baseUrl}/lines/${lineId}/sync`)
      return response.data
    } catch (error) {
      console.error(`Failed to sync production line ${lineId}:`, error)
      throw error
    }
  }

  async syncAll(): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.post<DigitalTwinSyncResult>(`${this.baseUrl}/sync-all`)
      return response.data
    } catch (error) {
      console.error('Failed to sync all entities:', error)
      throw error
    }
  }

  async getStatus(): Promise<DigitalTwinStatus> {
    try {
      const response = await axios.get<DigitalTwinStatus>(`${this.baseUrl}/status`)
      return response.data
    } catch (error) {
      console.error('Failed to get Azure Digital Twin status:', error)
      throw error
    }
  }

  async getTwinEntities(): Promise<TwinEntity[]> {
    try {
      const response = await axios.get<TwinEntity[]>(`${this.baseUrl}/entities`)
      return response.data
    } catch (error) {
      console.error('Failed to get twin entities:', error)
      throw error
    }
  }

  async getMachineTwin(machineId: string): Promise<TwinEntity> {
    try {
      const response = await axios.get<TwinEntity>(`${this.baseUrl}/machines/${machineId}`)
      return response.data
    } catch (error) {
      console.error(`Failed to get machine twin ${machineId}:`, error)
      throw error
    }
  }

  async getLineTwin(lineId: string): Promise<TwinEntity> {
    try {
      const response = await axios.get<TwinEntity>(`${this.baseUrl}/lines/${lineId}`)
      return response.data
    } catch (error) {
      console.error(`Failed to get line twin ${lineId}:`, error)
      throw error
    }
  }

  async updateTwinProperties(entityId: string, properties: Record<string, any>): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.patch<DigitalTwinSyncResult>(`${this.baseUrl}/entities/${entityId}`, { properties })
      return response.data
    } catch (error) {
      console.error(`Failed to update twin properties for ${entityId}:`, error)
      throw error
    }
  }

  async createRelationship(sourceId: string, targetId: string, relationshipType: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.post<DigitalTwinSyncResult>(`${this.baseUrl}/relationships`, {
        sourceId,
        targetId,
        relationshipType
      })
      return response.data
    } catch (error) {
      console.error(`Failed to create relationship ${sourceId} -> ${targetId}:`, error)
      throw error
    }
  }

  async deleteTwin(entityId: string): Promise<DigitalTwinSyncResult> {
    try {
      const response = await axios.delete<DigitalTwinSyncResult>(`${this.baseUrl}/entities/${entityId}`)
      return response.data
    } catch (error) {
      console.error(`Failed to delete twin ${entityId}:`, error)
      throw error
    }
  }

  async queryTwins(query: string): Promise<TwinEntity[]> {
    try {
      const response = await axios.post<TwinEntity[]>(`${this.baseUrl}/query`, { query })
      return response.data
    } catch (error) {
      console.error('Failed to query twins:', error)
      throw error
    }
  }
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
    loading.value = true
    error.value = null
    try {
      status.value = await azureDigitalTwinService.getStatus()
    } catch (err) {
      error.value = 'Failed to fetch Azure Digital Twin status'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchEntities = async () => {
    loading.value = true
    error.value = null
    try {
      entities.value = await azureDigitalTwinService.getTwinEntities()
    } catch (err) {
      error.value = 'Failed to fetch twin entities'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const syncMachine = async (machineId: string) => {
    loading.value = true
    error.value = null
    try {
      const result = await azureDigitalTwinService.syncMachine(machineId)
      await fetchStatus() // Refresh status after sync
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
      await fetchStatus() // Refresh status after sync
      return result
    } catch (err) {
      error.value = 'Failed to sync production line'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const syncAll = async () => {
    loading.value = true
    error.value = null
    try {
      const result = await azureDigitalTwinService.syncAll()
      await fetchStatus() // Refresh status after sync
      return result
    } catch (err) {
      error.value = 'Failed to sync all entities'
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
    syncProductionLine,
    syncAll
  }
}