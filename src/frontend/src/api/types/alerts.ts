/**
 * Alert type definitions matching backend DTOs.
 * Auto-generated from DigitalTwinPlatform.Application.Alerts.Models
 */

// Alert severity levels
export type AlertSeverity = 'Info' | 'Warning' | 'Critical' | 'Error'
export type AlertStatus = 'Active' | 'Acknowledged' | 'Resolved' | 'Dismissed'

export interface AlertDto {
  id: string
  machineId: string
  message: string
  title: string
  description: string
  severity: AlertSeverity
  status: AlertStatus | string
  createdAt: string
  acknowledgedAt?: string | null
  resolvedAt?: string | null
  isAcknowledged: boolean
  acknowledgedBy?: string | null
  relatedPredictionId?: string | null
  category?: string | null
  recommendedAction?: string | null
  suggestedActions?: string | null
}

export interface AlertStatsDto {
  totalAlerts: number
  activeAlerts: number
  acknowledgedAlerts: number
  resolvedAlerts: number
  criticalCount: number
  warningCount: number
  infoCount: number
  averageResolutionTime?: number | null
  alertsByCategory: Record<string, number>
  alertsByMachine: Record<string, number>
}

// Alert Rules
export interface AlertRuleDto {
  id: number
  name: string
  description: string
  machineType?: string | null
  machineId?: string | null
  condition: AlertConditionDto
  severity: AlertSeverity
  enabled: boolean
  cooldownMinutes: number
  notificationChannels: string[]
  createdAt: string
  updatedAt: string
  lastTriggered?: string | null
  triggerCount: number
}

export interface AlertConditionDto {
  field: string
  operator: 'equals' | 'not_equals' | 'greater_than' | 'less_than' | 'greater_than_or_equal' | 'less_than_or_equal' | 'contains' | 'between'
  value: unknown
  secondValue?: unknown | null // For 'between' operator
  dataType: 'number' | 'string' | 'boolean' | 'datetime'
}

// Request DTOs
export interface CreateAlertRuleRequest {
  name: string
  description: string
  machineType?: string | null
  machineId?: string | null
  condition: AlertConditionDto
  severity: AlertSeverity
  enabled?: boolean
  cooldownMinutes?: number
  notificationChannels?: string[]
}

export interface UpdateAlertRuleRequest extends CreateAlertRuleRequest {
  id: number
}

export interface AcknowledgeAlertRequest {
  acknowledgedBy: string
  notes?: string | null
}

export interface AlertSearchParams {
  query?: string
  severity?: AlertSeverity
  status?: AlertStatus
  machineId?: string
  startDate?: string
  endDate?: string
  page?: number
  pageSize?: number
}

// Notification types for alerts
export interface AlertNotificationDto {
  id: string
  alertId: string
  channel: string // email, sms, webhook, push
  recipient: string
  sentAt: string
  status: 'pending' | 'sent' | 'failed'
  errorMessage?: string | null
}
