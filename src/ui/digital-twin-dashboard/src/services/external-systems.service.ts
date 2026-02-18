import axios from 'axios'
import type { Ref } from 'vue'
import { ref, reactive } from 'vue'

// Define TypeScript interfaces
export interface ExternalSystem {
  id: string
  name: string
  systemType: string
  connectionUrl: string
  apiKey?: string
  username?: string
  status: ExternalSystemStatus
  lastConnected: string
  createdAt: string
  updatedAt: string
}

export interface SystemIntegration {
  id: string
  externalSystemId: string
  entityId: string
  entityType: EntityType
  integrationType: IntegrationType
  isEnabled: boolean
  syncIntervalMinutes: number
  createdAt: string
  updatedAt: string
}

export interface DataSynchronization {
  id: string
  externalSystemId: string
  entityId: string
  entityType: EntityType
  direction: SyncDirection
  status: SyncStatus
  dataPayload?: string
  errorMessage?: string
  startedAt: string
  completedAt?: string
  createdAt: string
  updatedAt: string
}

export enum ExternalSystemStatus {
  Disconnected = 'Disconnected',
  Connected = 'Connected',
  Error = 'Error'
}

export enum EntityType {
  Machine = 'Machine',
  MaintenanceRecord = 'MaintenanceRecord',
  ProductionLine = 'ProductionLine',
  TelemetryData = 'TelemetryData'
}

export enum IntegrationType {
  ReadOnly = 'ReadOnly',
  ReadWrite = 'ReadWrite',
  WriteOnly = 'WriteOnly'
}

export enum SyncDirection {
  Inbound = 'Inbound',
  Outbound = 'Outbound',
  Bidirectional = 'Bidirectional'
}

export enum SyncStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Completed = 'Completed',
  Failed = 'Failed'
}

export interface ExternalSystemCreateDto {
  name: string
  systemType: string
  connectionUrl: string
  apiKey?: string
  username?: string
  password?: string
}

export interface ExternalSystemUpdateDto {
  name?: string
  systemType?: string
  connectionUrl?: string
  apiKey?: string
  username?: string
  password?: string
}

export interface SystemIntegrationCreateDto {
  externalSystemId: string
  entityId: string
  entityType: EntityType
  integrationType?: IntegrationType
  syncIntervalMinutes?: number
}

export interface SystemIntegrationUpdateDto {
  integrationType?: IntegrationType
  syncIntervalMinutes?: number
  isEnabled?: boolean
}

export interface DataSynchronizationCreateDto {
  externalSystemId: string
  entityId: string
  entityType: EntityType
  direction?: SyncDirection
  dataPayload?: string
}

// External System Service Class
class ExternalSystemService {
  private baseUrl = '/api/externalsystems'

  // External System Management
  async getAllExternalSystems(): Promise<ExternalSystem[]> {
    try {
      const response = await axios.get<ExternalSystem[]>(this.baseUrl)
      return response.data
    } catch (error) {
      console.error('Failed to fetch external systems:', error)
      throw error
    }
  }

  async getConnectedExternalSystems(): Promise<ExternalSystem[]> {
    try {
      const response = await axios.get<ExternalSystem[]>(`${this.baseUrl}/connected`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch connected external systems:', error)
      throw error
    }
  }

  async getExternalSystemsByType(systemType: string): Promise<ExternalSystem[]> {
    try {
      const response = await axios.get<ExternalSystem[]>(`${this.baseUrl}/type/${systemType}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch external systems by type ${systemType}:`, error)
      throw error
    }
  }

  async getExternalSystemById(id: string): Promise<ExternalSystem> {
    try {
      const response = await axios.get<ExternalSystem>(`${this.baseUrl}/${id}`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch external system ${id}:`, error)
      throw error
    }
  }

  async getExternalSystemStatus(id: string): Promise<ExternalSystemStatus> {
    try {
      const response = await axios.get<ExternalSystemStatus>(`${this.baseUrl}/${id}/status`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch status for external system ${id}:`, error)
      throw error
    }
  }

  async createExternalSystem(system: ExternalSystemCreateDto): Promise<ExternalSystem> {
    try {
      const response = await axios.post<ExternalSystem>(this.baseUrl, system)
      return response.data
    } catch (error) {
      console.error('Failed to create external system:', error)
      throw error
    }
  }

  async updateExternalSystem(id: string, system: ExternalSystemUpdateDto): Promise<ExternalSystem> {
    try {
      const response = await axios.put<ExternalSystem>(`${this.baseUrl}/${id}`, system)
      return response.data
    } catch (error) {
      console.error(`Failed to update external system ${id}:`, error)
      throw error
    }
  }

  async deleteExternalSystem(id: string): Promise<void> {
    try {
      await axios.delete(`${this.baseUrl}/${id}`)
    } catch (error) {
      console.error(`Failed to delete external system ${id}:`, error)
      throw error
    }
  }

  async connectExternalSystem(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/${id}/connect`)
    } catch (error) {
      console.error(`Failed to connect external system ${id}:`, error)
      throw error
    }
  }

  async disconnectExternalSystem(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/${id}/disconnect`)
    } catch (error) {
      console.error(`Failed to disconnect external system ${id}:`, error)
      throw error
    }
  }

  async testExternalSystemConnection(id: string): Promise<boolean> {
    try {
      const response = await axios.post<boolean>(`${this.baseUrl}/${id}/test`)
      return response.data
    } catch (error) {
      console.error(`Failed to test connection for external system ${id}:`, error)
      throw error
    }
  }

  // System Integration Management
  async getSystemIntegrations(): Promise<SystemIntegration[]> {
    try {
      const response = await axios.get<SystemIntegration[]>(`${this.baseUrl}/integrations`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch system integrations:', error)
      throw error
    }
  }

  async getSystemIntegrationsBySystem(systemId: string): Promise<SystemIntegration[]> {
    try {
      const response = await axios.get<SystemIntegration[]>(`${this.baseUrl}/${systemId}/integrations`)
      return response.data
    } catch (error) {
      console.error(`Failed to fetch integrations for system ${systemId}:`, error)
      throw error
    }
  }

  async getActiveSystemIntegrations(): Promise<SystemIntegration[]> {
    try {
      const response = await axios.get<SystemIntegration[]>(`${this.baseUrl}/integrations/active`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch active system integrations:', error)
      throw error
    }
  }

  async createSystemIntegration(integration: SystemIntegrationCreateDto): Promise<SystemIntegration> {
    try {
      const response = await axios.post<SystemIntegration>(`${this.baseUrl}/integrations`, integration)
      return response.data
    } catch (error) {
      console.error('Failed to create system integration:', error)
      throw error
    }
  }

  async updateSystemIntegration(id: string, integration: SystemIntegrationUpdateDto): Promise<SystemIntegration> {
    try {
      const response = await axios.put<SystemIntegration>(`${this.baseUrl}/integrations/${id}`, integration)
      return response.data
    } catch (error) {
      console.error(`Failed to update system integration ${id}:`, error)
      throw error
    }
  }

  async deleteSystemIntegration(id: string): Promise<void> {
    try {
      await axios.delete(`${this.baseUrl}/integrations/${id}`)
    } catch (error) {
      console.error(`Failed to delete system integration ${id}:`, error)
      throw error
    }
  }

  async enableSystemIntegration(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/integrations/${id}/enable`)
    } catch (error) {
      console.error(`Failed to enable system integration ${id}:`, error)
      throw error
    }
  }

  async disableSystemIntegration(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/integrations/${id}/disable`)
    } catch (error) {
      console.error(`Failed to disable system integration ${id}:`, error)
      throw error
    }
  }

  // Data Synchronization Management
  async getDataSynchronizations(): Promise<DataSynchronization[]> {
    try {
      const response = await axios.get<DataSynchronization[]>(`${this.baseUrl}/synchronizations`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch data synchronizations:', error)
      throw error
    }
  }

  async getPendingSynchronizations(): Promise<DataSynchronization[]> {
    try {
      const response = await axios.get<DataSynchronization[]>(`${this.baseUrl}/synchronizations/pending`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch pending synchronizations:', error)
      throw error
    }
  }

  async getFailedSynchronizations(): Promise<DataSynchronization[]> {
    try {
      const response = await axios.get<DataSynchronization[]>(`${this.baseUrl}/synchronizations/failed`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch failed synchronizations:', error)
      throw error
    }
  }

  async getRecentSynchronizations(limit: number = 50): Promise<DataSynchronization[]> {
    try {
      const response = await axios.get<DataSynchronization[]>(`${this.baseUrl}/synchronizations/recent?limit=${limit}`)
      return response.data
    } catch (error) {
      console.error('Failed to fetch recent synchronizations:', error)
      throw error
    }
  }

  async createDataSynchronization(sync: DataSynchronizationCreateDto): Promise<DataSynchronization> {
    try {
      const response = await axios.post<DataSynchronization>(`${this.baseUrl}/synchronizations`, sync)
      return response.data
    } catch (error) {
      console.error('Failed to create data synchronization:', error)
      throw error
    }
  }

  async processDataSynchronizations(): Promise<number> {
    try {
      const response = await axios.post<number>(`${this.baseUrl}/synchronizations/process`)
      return response.data
    } catch (error) {
      console.error('Failed to process data synchronizations:', error)
      throw error
    }
  }

  async cancelDataSynchronization(id: string): Promise<void> {
    try {
      await axios.post(`${this.baseUrl}/synchronizations/${id}/cancel`)
    } catch (error) {
      console.error(`Failed to cancel data synchronization ${id}:`, error)
      throw error
    }
  }

  async retryFailedSynchronization(id: string): Promise<DataSynchronization> {
    try {
      const response = await axios.post<DataSynchronization>(`${this.baseUrl}/synchronizations/${id}/retry`)
      return response.data
    } catch (error) {
      console.error(`Failed to retry failed synchronization ${id}:`, error)
      throw error
    }
  }
}

// Create singleton instance
export const externalSystemService = new ExternalSystemService()

// Composable for external system management
export function useExternalSystems() {
  const externalSystems: Ref<ExternalSystem[]> = ref([])
  const systemIntegrations: Ref<SystemIntegration[]> = ref([])
  const dataSynchronizations: Ref<DataSynchronization[]> = ref([])
  const currentSystem: Ref<ExternalSystem | null> = ref(null)
  const loading: Ref<boolean> = ref(false)
  const error: Ref<string | null> = ref(null)

  const externalSystemState = reactive({
    externalSystems,
    systemIntegrations,
    dataSynchronizations,
    currentSystem,
    loading,
    error
  })

  const fetchExternalSystems = async () => {
    loading.value = true
    error.value = null
    try {
      externalSystems.value = await externalSystemService.getAllExternalSystems()
    } catch (err) {
      error.value = 'Failed to fetch external systems'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchConnectedSystems = async () => {
    loading.value = true
    error.value = null
    try {
      externalSystems.value = await externalSystemService.getConnectedExternalSystems()
    } catch (err) {
      error.value = 'Failed to fetch connected systems'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchSystemIntegrations = async () => {
    loading.value = true
    error.value = null
    try {
      systemIntegrations.value = await externalSystemService.getSystemIntegrations()
    } catch (err) {
      error.value = 'Failed to fetch system integrations'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const fetchDataSynchronizations = async () => {
    loading.value = true
    error.value = null
    try {
      dataSynchronizations.value = await externalSystemService.getDataSynchronizations()
    } catch (err) {
      error.value = 'Failed to fetch data synchronizations'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const selectExternalSystem = async (systemId: string) => {
    loading.value = true
    error.value = null
    try {
      currentSystem.value = await externalSystemService.getExternalSystemById(systemId)
    } catch (err) {
      error.value = 'Failed to select external system'
      console.error(err)
    } finally {
      loading.value = false
    }
  }

  const createExternalSystem = async (systemData: ExternalSystemCreateDto) => {
    loading.value = true
    error.value = null
    try {
      const newSystem = await externalSystemService.createExternalSystem(systemData)
      externalSystems.value.push(newSystem)
      return newSystem
    } catch (err) {
      error.value = 'Failed to create external system'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateExternalSystem = async (id: string, systemData: ExternalSystemUpdateDto) => {
    loading.value = true
    error.value = null
    try {
      const updatedSystem = await externalSystemService.updateExternalSystem(id, systemData)
      const index = externalSystems.value.findIndex(s => s.id === id)
      if (index !== -1) {
        externalSystems.value[index] = updatedSystem
      }
      if (currentSystem.value?.id === id) {
        currentSystem.value = updatedSystem
      }
      return updatedSystem
    } catch (err) {
      error.value = 'Failed to update external system'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteExternalSystem = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await externalSystemService.deleteExternalSystem(id)
      externalSystems.value = externalSystems.value.filter(s => s.id !== id)
      if (currentSystem.value?.id === id) {
        currentSystem.value = null
      }
    } catch (err) {
      error.value = 'Failed to delete external system'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const connectExternalSystem = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await externalSystemService.connectExternalSystem(id)
      const system = externalSystems.value.find(s => s.id === id)
      if (system) {
        system.status = ExternalSystemStatus.Connected
        system.lastConnected = new Date().toISOString()
        system.updatedAt = new Date().toISOString()
      }
    } catch (err) {
      error.value = 'Failed to connect external system'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const disconnectExternalSystem = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      await externalSystemService.disconnectExternalSystem(id)
      const system = externalSystems.value.find(s => s.id === id)
      if (system) {
        system.status = ExternalSystemStatus.Disconnected
        system.updatedAt = new Date().toISOString()
      }
    } catch (err) {
      error.value = 'Failed to disconnect external system'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const testExternalSystemConnection = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      const isConnected = await externalSystemService.testExternalSystemConnection(id)
      const system = externalSystems.value.find(s => s.id === id)
      if (system) {
        system.status = isConnected ? ExternalSystemStatus.Connected : ExternalSystemStatus.Error
        if (isConnected) {
          system.lastConnected = new Date().toISOString()
        }
        system.updatedAt = new Date().toISOString()
      }
      return isConnected
    } catch (err) {
      error.value = 'Failed to test external system connection'
      console.error(err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    ...externalSystemState,
    fetchExternalSystems,
    fetchConnectedSystems,
    fetchSystemIntegrations,
    fetchDataSynchronizations,
    selectExternalSystem,
    createExternalSystem,
    updateExternalSystem,
    deleteExternalSystem,
    connectExternalSystem,
    disconnectExternalSystem,
    testExternalSystemConnection
  }
}