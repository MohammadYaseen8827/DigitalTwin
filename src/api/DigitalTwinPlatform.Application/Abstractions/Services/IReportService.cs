using DigitalTwinPlatform.Application.Reports.Models;

namespace DigitalTwinPlatform.Application.Abstractions.Services;

public interface IReportService
{
    Task<ReportGenerationResponse> GenerateReportAsync(ReportGenerationRequest request, CancellationToken ct = default);
    Task<byte[]> ExportDataAsync(ExportDataRequest request, CancellationToken ct = default);
    Task<IEnumerable<ReportTemplate>> GetTemplatesAsync(CancellationToken ct = default);
    Task<ReportSchedule> ScheduleReportAsync(ReportSchedule schedule, CancellationToken ct = default);
    Task<IEnumerable<ReportHistoryItem>> GetHistoryAsync(int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<(byte[] Content, string ContentType, string FileName)> DownloadReportAsync(string reportId, CancellationToken ct = default);
}
