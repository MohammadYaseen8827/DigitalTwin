// Alert types expanded for UI components
export interface Alert {
    id: string
    machineId: string
    machineName?: string
    title: string
    description: string
    severity: 'info' | 'warning' | 'critical' | 'error'
    status: 'active' | 'acknowledged' | 'resolved'
    timestamp: string | Date
    acknowledgedBy?: string
    resolvedAt?: string | Date
    category?: 'sensor' | 'rul' | 'maintenance' | 'system' | 'performance'
    suggestedActions?: string[]
    relatedPredictionId?: string
}

export interface AlertFilter {
    severity?: Alert['severity'][]
    status?: Alert['status'][]
    machineId?: string
    searchQuery?: string
    dateRange?: {
        start: Date
        end: Date
    }
}

export interface AlertRule {
    id: string
    name: string
    description: string
    enabled: boolean
    severity: Alert['severity']
    condition: {
        type: 'threshold' | 'rate_of_change' | 'deviation' | 'pattern'
        sensor?: string
        threshold?: number
        operator?: 'gt' | 'lt' | 'gte' | 'lte' | 'eq' | 'ne'
        window?: number // time window in minutes
    }
    notificationChannels: ('email' | 'sms' | 'push' | 'webhook')[]
    escalationEnabled: boolean
    escalationDelay?: number // minutes
    createdAt: string | Date
    updatedAt: string | Date
}

export interface AlertStats {
    total: number
    active: number
    acknowledged: number
    resolved: number
    bySeverity: {
        info: number
        warning: number
        critical: number
        error: number
    }
}

export interface ToastNotification {
    id: string
    alert: Alert
    duration: number
    position: 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'
    createdAt: Date
    dismissed: boolean
}

// Severity color mapping
export const SEVERITY_COLORS = {
    info: '#3B82F6',    // Blue
    warning: '#F59E0B',  // Yellow
    critical: '#EF4444', // Red
    error: '#8B5CF6'     // Purple
} as const

export const SEVERITY_LABELS = {
    info: 'Info',
    warning: 'Warning',
    critical: 'Critical',
    error: 'Error'
} as const
