/**
 * Workflow type definitions matching backend DTOs.
 * Auto-generated from DigitalTwinPlatform.Application.Workflows.Models
 */

// Workflow Types
export type WorkflowType = 'maintenance' | 'monitoring' | 'reporting' | 'alerting' | 'custom'
export type TriggerType = 'schedule' | 'condition' | 'event' | 'manual'
export type ActionType = 'notification' | 'update_status' | 'create_ticket' | 'execute_script' | 'log_event' | 'send_email' | 'http_call'
export type TargetType = 'all' | 'specific' | 'by_type' | 'by_status' | 'by_location' | 'by_metadata'
export type ExecutionStatus = 'success' | 'failed' | 'running' | 'cancelled' | 'pending'

export interface WorkflowDefinitionDto {
  id: string
  name: string
  description: string
  type: WorkflowType | string
  enabled: boolean
  status: string // active, inactive, error
  trigger: WorkflowTriggerDto
  actions: WorkflowActionDto[]
  target: WorkflowTargetDto
  tags: string[]
  createdAt: string
  updatedAt: string
  lastRun?: string | null
  nextRun?: string | null
  executionCount: number
  createdBy: string
}

export interface WorkflowTriggerDto {
  type: TriggerType | string
  cronExpression?: string | null
  condition?: WorkflowConditionDto | null
  eventType?: string | null
  configuration: Record<string, unknown>
}

export interface WorkflowConditionDto {
  field: string
  operator: string // equals, greater_than, less_than, contains
  value: unknown
  dataType?: string | null // string, number, boolean, datetime
}

export interface WorkflowActionDto {
  type: ActionType | string
  configuration: Record<string, unknown>
  order: number
  enabled: boolean
}

export interface WorkflowTargetDto {
  type: TargetType | string
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
  status: ExecutionStatus | string
  startedAt: string
  completedAt?: string | null
  duration?: number | null // TimeSpan serialized as seconds/ms
  inputContext: Record<string, unknown>
  actionExecutions: WorkflowActionExecutionDto[]
  errorMessage?: string | null
  triggeredBy: string // system, user, event
}

export interface WorkflowActionExecutionDto {
  actionOrder: number
  actionType: string
  status: string // success, failed, skipped
  startedAt: string
  completedAt?: string | null
  input: Record<string, unknown>
  output: Record<string, unknown>
  errorMessage?: string | null
}

export interface WorkflowStatisticsDto {
  workflowId: string
  totalExecutions: number
  successfulExecutions: number
  failedExecutions: number
  successRate: number
  averageDuration: number // TimeSpan as seconds
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
  category: string // maintenance, monitoring, alerting
  definition: Partial<WorkflowDefinitionDto>
  tags: string[]
  usageCount: number
  createdAt: string
  createdBy: string
}

// Request DTOs
export interface CreateWorkflowRequest {
  name: string
  description: string
  type: WorkflowType | string
  trigger: WorkflowTriggerDto
  actions: WorkflowActionDto[]
  target: WorkflowTargetDto
  tags?: string[]
  enabled?: boolean
}

export interface UpdateWorkflowRequest extends CreateWorkflowRequest {
  id: string
}

export interface ValidateWorkflowRequest {
  definition: Partial<WorkflowDefinitionDto>
}

export interface DuplicateWorkflowRequest {
  newName: string
}

export interface ExecuteWorkflowRequest {
  context?: Record<string, unknown>
}
