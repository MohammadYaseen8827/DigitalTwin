/**
 * Alert Rules Service
 * Handles CRUD operations for alert rule configurations.
 */
import axiosClient from '@/api/axiosClient'
import type { AlertSeverity } from '@/api/types/alerts'

// Types matching backend DTOs
export interface AlertConditionDto {
  type: string // threshold, rate_of_change, deviation, pattern
  sensor?: string | null
  threshold?: number | null
  operator?: string | null // gt, lt, gte, lte, eq, ne
  window?: number | null // time window in minutes
  field?: string | null
  value?: unknown
  secondValue?: unknown | null
  dataType?: string | null
}

export interface AlertRuleDto {
  id: number
  name: string
  description: string
  enabled: boolean
  severity: AlertSeverity
  condition: AlertConditionDto
  notificationChannels: string[]
  machineType?: string | null
  machineId?: string | null
  cooldownMinutes: number
  escalationEnabled: boolean
  escalationDelay?: number | null
  createdAt: string
  updatedAt: string
  lastTriggered?: string | null
  triggerCount: number
}

export interface AlertRuleCreateDto {
  name: string
  description: string
  enabled?: boolean
  severity: AlertSeverity
  condition: AlertConditionDto
  notificationChannels?: string[]
  machineType?: string | null
  machineId?: string | null
  cooldownMinutes?: number
  escalationEnabled?: boolean
  escalationDelay?: number | null
}

export interface AlertRuleUpdateDto extends AlertRuleCreateDto {
  id: number
}

/**
 * Fetch all alert rules.
 */
export async function getAllAlertRules(): Promise<AlertRuleDto[]> {
  const response = await axiosClient.get<AlertRuleDto[]>('/AlertRules')
  return response as unknown as AlertRuleDto[]
}

/**
 * Fetch a specific alert rule by ID.
 */
export async function getAlertRule(id: number): Promise<AlertRuleDto> {
  const response = await axiosClient.get<AlertRuleDto>(`/AlertRules/${id}`)
  return response as unknown as AlertRuleDto
}

/**
 * Create a new alert rule.
 */
export async function createAlertRule(request: AlertRuleCreateDto): Promise<AlertRuleDto> {
  const response = await axiosClient.post<AlertRuleDto>('/AlertRules', request)
  return response as unknown as AlertRuleDto
}

/**
 * Update an existing alert rule.
 */
export async function updateAlertRule(id: number, request: AlertRuleUpdateDto): Promise<AlertRuleDto> {
  const response = await axiosClient.put<AlertRuleDto>(`/AlertRules/${id}`, request)
  return response as unknown as AlertRuleDto
}

/**
 * Delete an alert rule.
 */
export async function deleteAlertRule(id: number): Promise<void> {
  await axiosClient.delete(`/api/AlertRules/${id}`)
}

/**
 * Toggle an alert rule's enabled state.
 */
export async function toggleAlertRule(id: number): Promise<AlertRuleDto> {
  const response = await axiosClient.patch<AlertRuleDto>(`/AlertRules/${id}/toggle`)
  return response as unknown as AlertRuleDto
}

// Export service object for convenience
export const alertRulesService = {
  getAllAlertRules,
  getAlertRule,
  createAlertRule,
  updateAlertRule,
  deleteAlertRule,
  toggleAlertRule
}

export default alertRulesService
