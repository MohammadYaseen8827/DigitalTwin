// Common types for the Digital Twin Platform

export interface MachineDto {
  id: string
  name: string
  serialNumber?: string
  type: string
  manufacturer?: string
  model?: string
  status: EquipmentStatus
  criticality?: number
  location: string
  installationDate?: string
  warrantyExpiry?: string
  lastMaintenance?: string
  nextMaintenance?: string
  maintenanceInterval?: number
  degradationModel?: string
  threshold?: number
  isActive?: boolean
  remainingUsefulLifeDays?: number | null
  rulLowerBound?: number | null
  rulUpperBound?: number | null
  failureProbability?: number | null
  healthStatus?: string | null
  createdAt: string
  updatedAt: string
  metadata?: Record<string, unknown>
}

export type EquipmentStatus =
  | 'operational'
  | 'warning'
  | 'critical'
  | 'maintenance'
  | 'offline'
  | number // For backward compatibility with numeric status codes

export interface MachineCreateDto {
  name: string
  serialNumber?: string
  type: string
  manufacturer?: string
  model?: string
  status: string
  criticality?: number
  location?: string
  installationDate?: string
  warrantyExpiry?: string
  lastMaintenance?: string
  nextMaintenance?: string
  maintenanceInterval?: number
  degradationModel?: string
  threshold?: number
  isActive?: boolean
  metadata?: Record<string, unknown>
}

export interface MachineUpdateDto {
  name?: string
  serialNumber?: string
  type?: string
  manufacturer?: string
  model?: string
  status?: string
  criticality?: number
  location?: string
  installationDate?: string
  warrantyExpiry?: string
  lastMaintenance?: string
  nextMaintenance?: string
  maintenanceInterval?: number
  degradationModel?: string
  threshold?: number
  isActive?: boolean
  metadata?: Record<string, unknown>
}

// Simulation types
export interface SimulationRequestDto {
  machineId: string
  degradationModel: 'wiener' | 'markov' | 'physics_based'
  steps: number
  persistTelemetry: boolean
  maxRuntimeSeconds?: number
}

export interface SimulationScheduleDto {
  id: string
  machineId: string
  startTime: string
  endTime?: string
  intervalSeconds: number
  isActive: boolean
  parameters: Record<string, unknown>
  createdAt: string
  updatedAt: string
}

export interface SimulationStatusDto {
  isRunning: boolean
  isScheduled: boolean
  machineId: string
  status: 'idle' | 'running' | 'scheduled' | 'error'
  nextRun?: string
  lastRun?: string
  error?: string
}

// Telemetry types
export interface TelemetryDataPoint {
  timestamp: string
  machineId: string
  metrics: Record<string, number>
  metadata?: Record<string, unknown>
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
  timestamp?: string
}

// Prediction types
export interface PredictionDto {
  id: string
  machineId: string
  remainingUsefulLifeDays: number
  rulLowerBound?: number
  rulUpperBound?: number
  failureProbability: number
  healthStatus: string
  featureContributions?: Record<string, number>
  createdAt: string
  updatedAt: string
}

export interface PredictionResult {
  timestamp: string
  machineId: string
  predictionWindow: number
  predictions: {
    metric: string
    values: number[]
    timestamps: string[]
    confidenceIntervals?: {
      upper: number[]
      lower: number[]
    }
  }[]
}

// Error response
export interface ApiError {
  message: string
  code?: string
  details?: Record<string, unknown>
  timestamp?: string
}

// Generic response wrapper
export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
  errors?: ApiError[]
}

// Pagination
export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

// Filtering and sorting
export interface FilterCriteria {
  field: string
  operator: 'eq' | 'ne' | 'gt' | 'lt' | 'gte' | 'lte' | 'contains' | 'in'
  value: unknown
}

export interface SortOption {
  field: string
  direction: 'asc' | 'desc'
}

export interface QueryOptions {
  filters?: FilterCriteria[]
  sort?: SortOption[]
  page?: number
  pageSize?: number
  search?: string
}

// AI Model Management Types
export interface AIModelDto {
  id: string
  name: string
  description: string
  version: string
  status: 'draft' | 'training' | 'deployed' | 'archived'
  modelType: 'degradation' | 'failure' | 'performance' | 'quality'
  algorithm: 'random_forest' | 'neural_network' | 'svm' | 'xgboost' | 'lstm'
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  trainingDataSize: number
  features: string[]
  deployedMachines: string[]
  tags: string[]
  createdAt: string
  updatedAt: string
  lastTraining: string
}

export interface ModelPredictionDto {
  remainingUsefulLife: number
  confidence: number
  riskLevel: 'low' | 'medium' | 'high'
  featureImportance: Record<string, unknown>
  predictionTime: string
}

export interface ModelMetricsDto {
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  meanAbsoluteError: number
  rootMeanSquareError: number
  classMetrics: Record<string, number>
  lastEvaluated: string
}

export interface ModelInputDto {
  features: Record<string, number>
}

export interface ModelValidationResultDto {
  isValid: boolean
  message: string
  errors: string[]
  compatibility: ModelCompatibilityDto
}

export interface ModelCompatibilityDto {
  isCompatible: boolean
  compatibleMachineTypes: string[]
  requiredFeatures: string[]
  framework: string
}

export interface DeploymentStatusDto {
  status: 'pending' | 'deploying' | 'deployed' | 'failed'
  totalMachines: number
  successfulDeployments: number
  failedDeployments: number
  machineStatuses: MachineDeploymentStatus[]
  lastUpdated: string
}

export interface MachineDeploymentStatus {
  machineId: string
  status: 'success' | 'failed' | 'pending'
  errorMessage: string
  deployedAt: string
}

// Workflow Automation Types
export interface WorkflowDefinitionDto {
  id: string
  name: string
  description: string
  type: string
  enabled: boolean
  status: string
  trigger: WorkflowTriggerDto
  actions: WorkflowActionDto[]
  target: WorkflowTargetDto
  tags: string[]
  createdAt: string
  updatedAt: string
  lastRun?: string
  nextRun?: string
  executionCount: number
  createdBy: string
}

export interface WorkflowTriggerDto {
  type: string
  cronExpression?: string
  condition?: WorkflowConditionDto
  eventType?: string
  configuration: Record<string, unknown>
}

export interface WorkflowConditionDto {
  field: string
  operator: string
  value: unknown
  dataType?: string
}

export interface WorkflowActionDto {
  type: string
  configuration: Record<string, unknown>
  order: number
  enabled: boolean
}

export interface WorkflowTargetDto {
  type: string
  machineIds: string[]
  machineTypes: string[]
  statuses: string[]
  locations: string[]
  filters: Record<string, unknown>
}

export interface WorkflowExecutionDto {
  id: string
  workflowId: string
  workflowName: string
  status: string
  startedAt: string
  completedAt?: string
  duration?: number
  inputContext: Record<string, unknown>
  actionExecutions: WorkflowActionExecutionDto[]
  errorMessage?: string
  triggeredBy: string
}

export interface WorkflowActionExecutionDto {
  actionOrder: number
  actionType: string
  status: string
  startedAt: string
  completedAt?: string
  input: Record<string, unknown>
  output: Record<string, unknown>
  errorMessage?: string
}

export interface WorkflowStatisticsDto {
  workflowId: string
  totalExecutions: number
  successfulExecutions: number
  failedExecutions: number
  successRate: number
  averageDuration: number
  firstExecution: string
  lastExecution: string
  executionsByDay: Record<string, number>
  performanceMetrics: Record<string, unknown>
}

export interface WorkflowTemplateDto {
  id: string
  name: string
  description: string
  category: string
  definition: WorkflowDefinitionDto
  tags: string[]
  usageCount: number
  createdAt: string
  createdBy: string
}

export interface WorkflowValidationResultDto {
  isValid: boolean
  message: string
  errors: string[]
  warnings: string[]
  compatibility: WorkflowCompatibilityDto
}

export interface WorkflowCompatibilityDto {
  isCompatible: boolean
  compatibleTriggers: string[]
  compatibleActions: string[]
  requiredPermissions: string[]
}

// Maintenance Management Types
export interface MaintenanceRecordDto {
  id: string
  machineId: string
  machineName?: string
  type: 'preventive' | 'corrective' | 'emergency'
  status: 'planned' | 'inprogress' | 'completed' | 'cancelled'
  plannedDate: string
  startedAt?: string
  completedAt?: string
  cancelledAt?: string
  performedBy?: string
  notes?: string
  finalNotes?: string
  alertId?: string
  createdAt: string
  updatedAt: string
}

export interface PlanMaintenanceRequest {
  machineId: string
  type: 'preventive' | 'corrective' | 'emergency'
  plannedDate: string
  notes?: string
  alertId?: string
}

export interface CompleteMaintenanceRequest {
  performedBy: string
  finalNotes?: string
}

// Search types
export interface SearchRequestDto {
  query: string
  entityTypes?: string[]
  dateRange?: string
  statusFilter?: string
  advancedFilters?: AdvancedFiltersDto
  sortBy?: string
  page?: number
  pageSize?: number
}

export interface AdvancedFiltersDto {
  minTemperature?: number
  maxTemperature?: number
  minPressure?: number
  maxPressure?: number
  location?: string
  manufacturer?: string
  model?: string
}

export interface SearchResultDto {
  items: SearchResultItemDto[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  executionTimeMs: number
}

export interface SearchResultItemDto {
  id: string
  type: string
  title: string
  subtitle: string
  status?: string
  createdAt: string
  matchScore: number
  metadata?: Record<string, unknown>
}

export interface SearchSuggestionDto {
  text: string
  entityType: string
  score: number
}

export interface SavedSearchDto {
  id: string
  name: string
  query: string
  entityTypes?: string[]
  createdAt: string
  lastUsed?: string
}

export interface SaveSearchRequestDto {
  name: string
  query: string
  entityTypes?: string[]
}
