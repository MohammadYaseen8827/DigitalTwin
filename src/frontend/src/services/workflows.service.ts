import axiosClient from '@/api/axiosClient'
import type { 
  WorkflowDefinitionDto, 
  WorkflowExecutionDto, 
  WorkflowStatisticsDto, 
  WorkflowTemplateDto,
  WorkflowValidationResultDto
} from '@/api/types'

/**
 * Workflow Automation Service
 * Handles all API interactions for workflow management
 */

// Workflow CRUD Operations
export async function fetchAllWorkflows(): Promise<WorkflowDefinitionDto[]> {
  const response = await axiosClient.get<WorkflowDefinitionDto[]>('/Workflow')
  return response.data || []
}

export async function fetchWorkflowById(id: string): Promise<WorkflowDefinitionDto> {
  const response = await axiosClient.get<WorkflowDefinitionDto>(`/Workflow/${encodeURIComponent(id)}`)
  return response.data
}

export async function createWorkflow(workflowData: {
  name: string
  description: string
  type: string
  trigger: any
  actions: any[]
  target: any
  tags: string[]
  createdBy: string
}): Promise<WorkflowDefinitionDto> {
  const response = await axiosClient.post<WorkflowDefinitionDto>('/Workflow', workflowData)
  return response.data
}

export async function updateWorkflow(
  id: string, 
  updates: Partial<{
    name: string
    description: string
    type: string
    enabled: boolean
    trigger: any
    actions: any[]
    target: any
    tags: string[]
  }>
): Promise<WorkflowDefinitionDto> {
  const response = await axiosClient.put<WorkflowDefinitionDto>(
    `/Workflow/${encodeURIComponent(id)}`, 
    updates
  )
  return response.data
}

export async function deleteWorkflow(id: string): Promise<void> {
  await axiosClient.delete(`/Workflow/${encodeURIComponent(id)}`)
}

export async function toggleWorkflow(id: string): Promise<WorkflowDefinitionDto> {
  const response = await axiosClient.post<WorkflowDefinitionDto>(`/Workflow/${encodeURIComponent(id)}/toggle`)
  return response.data
}

export async function duplicateWorkflow(
  id: string,
  newName: string,
  newDescription?: string
): Promise<WorkflowDefinitionDto> {
  const response = await axiosClient.post<WorkflowDefinitionDto>(
    `/Workflow/${encodeURIComponent(id)}/duplicate`,
    { newName, newDescription }
  )
  return response.data
}

// Workflow Execution Operations
export async function executeWorkflow(
  id: string,
  context: Record<string, any> = {}
): Promise<WorkflowExecutionDto> {
  const response = await axiosClient.post<WorkflowExecutionDto>(
    `/Workflow/${encodeURIComponent(id)}/execute`,
    { context }
  )
  return response.data
}

export async function fetchWorkflowExecutions(
  id: string,
  page: number = 1,
  pageSize: number = 20
): Promise<WorkflowExecutionDto[]> {
  const response = await axiosClient.get<WorkflowExecutionDto[]>(
    `/Workflow/${encodeURIComponent(id)}/executions`,
    { params: { page, pageSize } }
  )
  return response.data || []
}

export async function fetchWorkflowStatistics(id: string): Promise<WorkflowStatisticsDto> {
  const response = await axiosClient.get<WorkflowStatisticsDto>(
    `/Workflow/${encodeURIComponent(id)}/statistics`
  )
  return response.data
}

// Workflow Templates and Validation
export async function fetchWorkflowTemplates(): Promise<WorkflowTemplateDto[]> {
  const response = await axiosClient.get<WorkflowTemplateDto[]>('/Workflow/templates')
  return response.data || []
}

export async function validateWorkflow(definition: any): Promise<WorkflowValidationResultDto> {
  const response = await axiosClient.post<WorkflowValidationResultDto>(
    '/Workflow/validate',
    { definition }
  )
  return response.data
}

// Utility functions
export function getWorkflowStatusColor(status: string): string {
  const statusColors: Record<string, string> = {
    'active': 'var(--color-success)',
    'inactive': 'var(--color-text-secondary)',
    'error': 'var(--color-error)',
    'paused': 'var(--color-warning)'
  }
  return statusColors[status.toLowerCase()] || 'var(--color-text-secondary)'
}

export function getWorkflowTypeIcon(type: string): string {
  const typeIcons: Record<string, string> = {
    'maintenance': 'hammer',
    'monitoring': 'activity',
    'reporting': 'file-text',
    'alerting': 'bell',
    'custom': 'settings'
  }
  return typeIcons[type.toLowerCase()] || 'workflow'
}

export function getWorkflowTypeDisplayName(type: string): string {
  const typeNames: Record<string, string> = {
    'maintenance': 'Maintenance',
    'monitoring': 'Monitoring',
    'reporting': 'Reporting',
    'alerting': 'Alerting',
    'custom': 'Custom'
  }
  return typeNames[type.toLowerCase()] || type
}

export function getTriggerTypeDisplayName(triggerType: string): string {
  const triggerNames: Record<string, string> = {
    'schedule': 'Scheduled',
    'condition': 'Conditional',
    'event': 'Event-based',
    'manual': 'Manual'
  }
  return triggerNames[triggerType.toLowerCase()] || triggerType
}

export function getActionTypeDisplayName(actionType: string): string {
  const actionNames: Record<string, string> = {
    'notification': 'Send Notification',
    'update_status': 'Update Status',
    'create_ticket': 'Create Ticket',
    'execute_script': 'Execute Script',
    'log_event': 'Log Event',
    'send_email': 'Send Email',
    'http_call': 'HTTP Request'
  }
  return actionNames[actionType.toLowerCase()] || actionType
}

export function formatExecutionTime(duration: number): string {
  if (duration < 1000) return `${Math.round(duration)}ms`
  if (duration < 60000) return `${(duration / 1000).toFixed(1)}s`
  return `${(duration / 60000).toFixed(1)}min`
}

export function calculateSuccessRate(successful: number, total: number): number {
  if (total === 0) return 0
  return Math.round((successful / total) * 1000) / 10
}