import axiosClient from '@/api/axiosClient'
import type { 
  ReportGenerationRequest, 
  ReportGenerationResponse,
  ExportFormat,
  ReportTemplate,
  ReportSchedule
} from '@/types/reporting.types.ts'

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
 * Get scheduled reports
 */
export async function getScheduledReports(): Promise<ReportSchedule[]> {
  return axiosClient.get<ReportSchedule[], ReportSchedule[]>(
    '/Reports/schedules'
  )
}

/**
 * Cancel scheduled report
 */
export async function cancelScheduledReport(scheduleId: string): Promise<void> {
  await axiosClient.delete(`/Reports/schedules/${scheduleId}`)
}

/**
 * Get report generation history
 */
export async function getReportHistory(
  page: number = 1,
  pageSize: number = 20
): Promise<any> {
  return axiosClient.get<any, any>(
    '/Reports/history',
    {
      params: { page, pageSize }
    }
  )
}