// Auto-generated API contract mirrors for backend DTOs to keep frontend and ASP.NET aligned.
export type EquipmentStatus =
  | 'Operational'
  | 'Warning'
  | 'Critical'
  | 'Maintenance'
  | 'Offline'
  | number

export type HealthClassification =
  | 'Healthy'
  | 'Normal'
  | 'MinorDegradation'
  | 'SignificantDegradation'
  | 'FailureImminent'
  | number

export interface MachineDto {
  id: string
  name: string
  type: string
  status: EquipmentStatus
  properties: Record<string, unknown> | null
  remainingUsefulLifeDays?: number | null
  failureProbability?: number | null
  healthStatus?: HealthClassification | null
  createdAt: string
  updatedAt: string
}

export interface MachineCreateDto {
  name: string
  type: string
  status: EquipmentStatus
  properties: Record<string, unknown>
}

export interface MachineUpdateDto extends MachineCreateDto {
  remainingUsefulLifeDays?: number | null
  failureProbability?: number | null
  healthStatus?: HealthClassification | null
}

export interface ProductionLineDto {
  id: string
  name: string
  configuration: Record<string, unknown>
  machineIds: string[]
}

export interface ProductionLineCreateDto {
  name: string
  configuration: Record<string, unknown>
  machineIds?: string[]
}

export type ProductionLineUpdateDto = ProductionLineCreateDto & {
  machineIds?: string[]
}

export interface TelemetryDto {
  id: string
  machineId: string
  dataType: string
  data: Record<string, unknown>
  timestamp: string
}

export interface TelemetryIngestDto {
  machineId: string
  dataType: string
  data: Record<string, unknown>
  timestamp?: string | null
}

export interface PredictionDto {
  id: string
  machineId: string
  remainingUsefulLifeDays: number
  failureProbability: number
  healthStatus: HealthClassification
  featureContributions?: Record<string, number>
  createdAt: string
  modelVersion: string
}

export interface PredictionRequestDto {
  machineId: string
}

export interface HealthStatusDto {
  status: string
  timestampUtc: string
  database: string
  signalR: string
  azureDigitalTwins: string
}

export interface DataArchivalRequestDto {
  retentionDays: number
}

export interface SimulationRequestDto {
  machineId: string
  degradationModel: string
  steps: number
  stepInterval: string
  persistTelemetry?: boolean
}

export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

// Alert types
export type AlertSeverity = 'Info' | 'Warning' | 'Critical'

export interface AlertDto {
  id: string
  machineId: string
  message: string
  severity: AlertSeverity
  createdAt: string
  isAcknowledged: boolean
  acknowledgedBy?: string
  acknowledgedAt?: string
}

// Maintenance types
export type MaintenanceStatus = 'Planned' | 'InProgress' | 'Completed' | 'Cancelled'

export interface MaintenanceRecordDto {
  id: string
  machineId: string
  alertId?: string
  date: string
  plannedDate?: string
  completionDate?: string
  status: MaintenanceStatus
  type: string
  notes: string
  performedBy: string
}

export interface PlanMaintenanceRequest {
  machineId: string
  type: string
  plannedDate: string
  notes: string
  alertId?: string
}

export interface CompleteMaintenanceRequest {
  performedBy: string
  finalNotes: string
}
