export type ExportFormat = 'csv' | 'excel' | 'pdf' | 'json' | 'parquet'

export interface ReportGenerationRequest {
  templateId: string
  parameters: Record<string, any>
  format: ExportFormat
  fileName?: string
  machineIds?: string[]
  dateRange?: {
    startDate: string
    endDate: string
  }
  includeCharts?: boolean
  includeRawData?: boolean
}

export interface ReportGenerationResponse {
  reportId: string
  fileName: string
  downloadUrl: string
  fileSize: number
  generatedAt: string
  expiresAt: string
}

export interface ReportTemplate {
  id: string
  name: string
  description: string
  category: string
  parameters: ReportParameter[]
  supportedFormats: ExportFormat[]
  isDefault: boolean
}

export interface ReportParameter {
  name: string
  displayName: string
  type: 'string' | 'number' | 'date' | 'boolean' | 'machine-list' | 'date-range'
  required: boolean
  defaultValue?: any
  options?: string[]
}

export interface ReportSchedule {
  id: string
  templateId: string
  parameters: Record<string, any>
  format: ExportFormat
  cronExpression: string
  recipients: string[]
  isActive: boolean
  createdAt: string
  lastRun?: string
  nextRun?: string
}

export interface ReportHistoryItem {
  id: string
  templateName: string
  fileName: string
  fileSize: number
  status: 'completed' | 'failed' | 'processing'
  generatedAt: string
  downloadUrl?: string
  errorMessage?: string
}