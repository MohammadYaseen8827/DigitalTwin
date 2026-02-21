import axiosClient from '@/api/axiosClient'
import type { 
  ReportGenerationRequest, 
  ReportGenerationResponse,
  ExportFormat,
  ReportTemplate,
  ReportSchedule,
  ReportHistoryItem
} from '@/types/reporting.types.ts'

export interface ReportHistoryResponse {
  items: ReportHistoryItem[]
  totalCount: number
  page: number
  pageSize: number
}

/**
 * Generate comprehensive reports with customizable templates
 */
export async function generateReport(
  request: ReportGenerationRequest
): Promise<ReportGenerationResponse> {
  return axiosClient.post<ReportGenerationResponse, ReportGenerationResponse>(
    '/Reports/generate',
    request
  )
}

/**
 * Export data in various formats (CSV, Excel, PDF, JSON)
 */
export async function exportData<T>(
  data: T[],
  format: ExportFormat,
  fileName?: string
): Promise<Blob> {
  const response = await axiosClient.post<Blob>(
    '/Reports/export',
    {
      data,
      format,
      fileName
    },
    {
      responseType: 'blob'
    }
  )
  return response
}

/**
 * Get available report templates
 */
export async function getReportTemplates(): Promise<ReportTemplate[]> {
  return axiosClient.get<ReportTemplate[], ReportTemplate[]>(
    '/Reports/templates'
  )
}

/**
 * Schedule automated reports
 */
export async function scheduleReport(
  schedule: ReportSchedule
): Promise<ReportSchedule> {
  return axiosClient.post<ReportSchedule, ReportSchedule>(
    '/Reports/schedule',
    schedule
  )
}

/**
 * Get scheduled reports (schedule is POST-only, this returns history)
 */
export async function getScheduledReports(): Promise<ReportSchedule[]> {
  // Backend doesn't have a GET /schedules endpoint
  // Return schedule info from history if needed
  const history = await getReportHistory(1, 100)
  return []
}

/**
 * Cancel scheduled report
 * NOTE: This endpoint is not implemented in the backend
 */
export async function cancelScheduledReport(scheduleId: string): Promise<void> {
  console.warn(`cancelScheduledReport: Backend endpoint /Reports/schedules/${scheduleId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

/**
 * Get a specific report by ID
 * NOTE: This endpoint is not implemented in the backend
 */
export async function getReportById(reportId: string): Promise<ReportHistoryItem> {
  console.warn(`getReportById: Backend endpoint /Reports/${reportId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

/**
 * Delete a generated report
 * NOTE: This endpoint is not implemented in the backend
 */
export async function deleteReport(reportId: string): Promise<void> {
  console.warn(`deleteReport: Backend endpoint /Reports/${reportId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

/**
 * Update scheduled report
 * NOTE: This endpoint is not implemented in the backend
 */
export async function updateScheduledReport(
  scheduleId: string,
  schedule: Partial<ReportSchedule>
): Promise<ReportSchedule> {
  console.warn(`updateScheduledReport: Backend endpoint /Reports/schedules/${scheduleId} not implemented`)
  throw new Error('This endpoint is not yet implemented')
}

// Export service object for convenience
export const reportingService = {
  generateReport,
  exportData,
  getReportTemplates,
  scheduleReport,
  getScheduledReports,
  cancelScheduledReport,
  getReportHistory,
  downloadReport,
  getReportById,
  deleteReport,
  updateScheduledReport
}

export default reportingService