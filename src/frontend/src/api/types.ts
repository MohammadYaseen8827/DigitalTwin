// Re-export base types from types/index.ts
export * from './types/index'

// Degradation Modeling types
export type DegradationModelType =
  | 'MarkovChain'
  | 'Weibull'
  | 'Exponential'
  | 'Linear'
  | 'Polynomial'
  | 'NeuralNetwork'
  | 'RandomForest'
  | 'LSTM'

export interface DegradationModelDto {
  id: string
  name: string
  description: string
  modelType: DegradationModelType
  parameters: Record<string, number>
  accuracy: number
  createdAt: string
  updatedAt: string
  isActive: boolean
}

export interface CreateDegradationModelDto {
  name: string
  description: string
  modelType: DegradationModelType
  parameters: Record<string, number>
}

export interface UpdateDegradationModelDto {
  name?: string
  description?: string
  parameters?: Record<string, number>
  isActive?: boolean
}

export interface DegradationPredictionDto {
  timestamp: string
  predictedValue: number
  confidenceInterval: { lower: number; upper: number }
  healthIndex: number
}

export interface DegradationAnalysisDto {
  modelId: string
  analysisPeriod: { startDate: string; endDate: string }
  degradationRate: number
  remainingUsefulLife: number
  predictions: DegradationPredictionDto[]
  recommendations: string[]
}

// Mathematical Modeling types
export type ModelType =
  | 'DifferentialEquations'
  | 'LinearAlgebra'
  | 'Optimization'
  | 'Statistics'
  | 'ControlTheory'
  | 'SignalProcessing'
  | 'MachineLearning'
  | 'Custom'

export interface MathematicalModelDto {
  id: string
  name: string
  description: string
  modelType: ModelType
  equations: string[]
  parameters: ModelParameterDto[]
  createdAt: string
  updatedAt: string
  isActive: boolean
}

export interface CreateMathematicalModelDto {
  name: string
  description: string
  modelType: ModelType
  equations: string[]
  parameters: ModelParameterDto[]
}

export interface UpdateMathematicalModelDto {
  name?: string
  description?: string
  equations?: string[]
  parameters?: ModelParameterDto[]
  isActive?: boolean
}

export interface ModelParameterDto {
  name: string
  value: number
  unit?: string
  description?: string
  bounds?: { min: number; max: number }
}

export interface ModelValidationDto {
  modelId: string
  validationScore: number
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  validationData: any[]
  predictions: any[]
  errors: string[]
}

export interface ModelSimulationDto {
  modelId: string
  simulationId: string
  timePoints: number[]
  results: Record<string, number[]>
  metadata: Record<string, any>
}

export interface ModelOptimizationDto {
  modelId: string
  optimizationId: string
  optimalParameters: ModelParameterDto[]
  objectiveValue: number
  iterations: number
  convergenceStatus: 'converged' | 'max_iterations' | 'failed'
}

// Model Lifecycle types
export interface ModelLifecycleDto {
  id: string
  modelId: string
  version: string
  status: 'development' | 'training' | 'validation' | 'deployment' | 'retired'
  stage: string
  metadata: Record<string, any>
  createdAt: string
  updatedAt: string
}

export interface CreateModelLifecycleDto {
  modelId: string
  version: string
  stage: string
  metadata?: Record<string, any>
}

export interface UpdateModelLifecycleDto {
  status?: string
  stage?: string
  metadata?: Record<string, any>
}

export interface ModelLifecycleTransitionDto {
  fromStage: string
  toStage: string
  transitionDate: string
  reason: string
  approvedBy: string
}

export interface ModelLifecycleMetricsDto {
  modelId: string
  version: string
  trainingTime: number
  validationScore: number
  deploymentPerformance: number
  usageStatistics: Record<string, number>
  errorRate: number
}

// Benchmark Validation types
export interface BenchmarkValidationResult {
  id: string
  modelVersionId: string
  benchmarkDataset: string
  validationDate: string
  metrics: {
    MAPE: number
    RMSE: number
    R2: number
    MAE: number
  }
  predictions: PredictionComparison[]
  summary: {
    totalSamples: number
    passedTests: number
    failedTests: number
    overallScore: number
  }
  status: 'pending' | 'running' | 'completed' | 'failed'
}

export interface BenchmarkDatasetInfo {
  id: string
  name: string
  description: string
  source: string
  size: number
  features: string[]
  targetVariable: string
  createdAt: string
  metadata: Record<string, any>
}

export interface PredictionComparison {
  actual: number
  predicted: number
  error: number
  absoluteError: number
  percentageError: number
}

export interface ValidationSummary {
  modelVersionId: string
  totalValidations: number
  averageScore: number
  bestScore: number
  worstScore: number
  recentValidations: BenchmarkValidationResult[]
}

// AI Model types
export interface AIModelDto {
  id: string
  name: string
  description: string
  version: string
  modelType: string
  algorithm: string
  accuracy: number
  precision: number
  recall: number
  f1Score: number
  trainingDataSize: number
  features: string[]
  deployedMachines: string[]
  tags: string[]
  status: 'draft' | 'training' | 'deployed' | 'archived'
  createdAt: string
  updatedAt: string
  lastTraining: string
}

export interface CreateAIModelDto {
  name: string
  description: string
  version: string
  modelType: string
  algorithm: string
  features: string[]
  tags: string[]
}

export interface UpdateAIModelDto {
  name?: string
  description?: string
  version?: string
  status?: string
  accuracy?: number
  precision?: number
  recall?: number
  f1Score?: number
  trainingDataSize?: number
  features?: string[]
  tags?: string[]
}

export interface AIModelTrainingRequestDto {
  modelId: string
  trainingData: any[]
  hyperparameters?: Record<string, any>
  validationSplit?: number
}

export interface AIModelTrainingResultDto {
  trainingId: string
  modelId: string
  status: 'pending' | 'running' | 'completed' | 'failed'
  progress: number
  metrics?: {
    accuracy: number
    precision: number
    recall: number
    f1Score: number
    loss: number
  }
  errors: string[]
  startedAt: string
  completedAt?: string
}

export interface AIModelValidationResultDto {
  modelId: string
  validationId: string
  status: 'pending' | 'running' | 'completed' | 'failed'
  results?: {
    accuracy: number
    precision: number
    recall: number
    f1Score: number
    confusionMatrix: number[][]
  }
  errors: string[]
}

export interface AIModelDeploymentDto {
  modelId: string
  deploymentId: string
  environment: 'development' | 'staging' | 'production'
  endpoint: string
  status: 'pending' | 'deploying' | 'deployed' | 'failed'
  deployedAt?: string
}

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

// Pagination - Re-exported from types/index.ts
// PaginatedResponse is now imported from './types/index'

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
  modelType: string
  algorithm: string
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

export interface AlertDto {
  id: string
  machineId: string
  message: string
  title: string
  description: string
  severity: 'info' | 'warning' | 'critical' | 'error'
  status: 'active' | 'acknowledged' | 'resolved' | 'closed'
  createdAt: string
  acknowledgedAt?: string
  resolvedAt?: string
  isAcknowledged: boolean
  acknowledgedBy?: string
  relatedPredictionId?: string
  category: string
  recommendedAction: string
  suggestedActions: string
}

export interface AlertStatsDto {
  totalAlerts: number
  activeAlerts: number
  acknowledgedAlerts: number
  resolvedAlerts: number
  criticalAlerts: number
  warningAlerts: number
  infoAlerts: number
  averageResolutionTime: number
  alertsByMachine: Array<{ machineId: string; machineName: string; count: number }>
  alertsBySeverity: Array<{ severity: string; count: number; percentage: number }>
  recentTrend: Array<{ date: string; count: number }>
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

// Run-to-Failure Analysis Types
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
  generatedTelemetry: TelemetryDataPoint[]
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

// External Systems Integration Types
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

export enum ExternalSystemStatus {
  Disconnected = 'Disconnected',
  Connected = 'Connected',
  Error = 'Error'
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

// Benchmark Dataset Types
export interface BenchmarkDatasetInfo {
  id: string
  name: string
  description: string
  source: string
  size: number
  features: string[]
  targetVariable: string
  createdAt: string
  metadata: Record<string, any>
}

export interface SaveSearchRequestDto {
  name: string
  query: string
  entityTypes?: string[]
}
