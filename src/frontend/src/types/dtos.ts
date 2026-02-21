// Extended types that match backend DTOs exactly
// These use camelCase to match the backend's JSON serialization

// Workflow Types
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

export interface WorkflowTemplateDto {
    id: string
    name: string
    description: string
    category: string
    definition: Partial<WorkflowDefinitionDto>
    tags: string[]
    usageCount: number
    createdAt: string
    createdBy: string
}

// AI Model Types
export interface AIModelDto {
    id: string
    name: string
    description: string
    version: string
    status: string
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
    riskLevel: string
    featureImportance: Record<string, unknown>
    shapValues: Record<string, number>
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
    status: string
    totalMachines: number
    successfulDeployments: number
    failedDeployments: number
    machineStatuses: MachineDeploymentStatus[]
    lastUpdated: string
}

export interface MachineDeploymentStatus {
    machineId: string
    status: string
    errorMessage: string
    deployedAt: string
}

// Telemetry Types
export interface TelemetryDto {
    id: string
    machineId: string
    dataType: string
    data: Record<string, number>
    timestamp: string
}

export interface TelemetryIngestDto {
    machineId: string
    dataType: string
    data: Record<string, number>
    timestamp?: string
}

export interface TelemetryMetricsDto {
    machineId: string
    temperature: number
    vibration: number
    pressure?: number
    humidity?: number
    rpm?: number
    timestamp: string
    lastUpdated: string
}

// Alert Types
export interface AlertDto {
    id: string
    machineId: string
    message: string
    title: string
    description: string
    severity: 'Info' | 'Warning' | 'Critical' | 'Error'
    status: string
    createdAt: string
    acknowledgedAt?: string
    resolvedAt?: string
    isAcknowledged: boolean
    acknowledgedBy?: string
    relatedPredictionId?: string
    category?: string
    recommendedAction?: string
    suggestedActions?: string
}

export interface AlertStatsDto {
    totalActive: number
    totalAcknowledged: number
    totalResolved: number
    criticalCount: number
    warningCount: number
    infoCount: number
}

// Search Types
export interface SearchRequestDto {
    query: string
    advancedFilters?: Record<string, unknown>
    dateRange?: { startDate: string; endDate: string }
    entityTypes?: string[]
    pageSize?: number
    statusFilter?: string
    page?: number
    sortBy?: string
}

export interface SearchResultDto<T> {
    items: T[]
    totalPages: number
    pageSize: number
    page: number
    totalCount: number
    executionTimeMs: number
}

export interface SearchResultItemDto {
    id: string
    type: string
    title: string
    subtitle: string
    status: string
    createdAt: string
    matchScore: number
    metadata: Record<string, unknown>
}

export interface SavedSearchDto {
    id: string
    name: string
    query: string
    entityTypes: string[]
    createdAt: string
    lastUsed?: string
}

// Machine Types
export interface MachineDto {
    id: string
    name: string
    type: string
    status: string
    properties?: Record<string, unknown>
    remainingUsefulLifeDays?: number
    failureProbability?: number
    healthStatus?: string
    createdAt: string
    updatedAt: string
    location?: string
    installationDate?: string
    lastMaintenanceDate?: string
    healthScore?: number
}

export interface MachineHealthDto {
    machineId: string
    overallHealth: number
    healthStatus: string
    lastUpdated: string
    componentHealth: Array<{
        component: string
        health: number
        status: string
    }>
}

export interface MachineRULDto {
    machineId: string
    rul: number
    rulUnit: string
    confidence: number
    lowerBound: number
    upperBound: number
    predictionTime: string
    modelVersion: string
}

// Maintenance Types
export interface MaintenanceEventDto {
    id: string
    machineId: string
    type: string
    description: string
    scheduledDate: string
    completedDate?: string
    status: string
    priority: string
    assignedTo?: string
    estimatedDuration?: number
    actualDuration?: number
    parts?: Array<{
        partId: string
        name: string
        quantity: number
        cost: number
    }>
    notes?: string
    createdAt: string
    updatedAt: string
}

export interface MaintenanceStatsDto {
    totalPlanned: number
    totalInProgress: number
    totalCompleted: number
    totalCancelled: number
    averageCompletionTime: number
    upcomingMaintenanceCount: number
    overdueMaintenanceCount: number
}
