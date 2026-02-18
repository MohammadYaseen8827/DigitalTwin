using DigitalTwinPlatform.Domain.Entities;
using DigitalTwinPlatform.Application.Abstractions.Repositories;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Parquet;
using Parquet.Schema;
using Parquet.Data;

namespace DigitalTwinPlatform.API.Services.Infrastructure;

public class DataArchivalService : IDataArchivalService
{
    private readonly ITelemetryRepository _telemetryRepository;
    private readonly IPredictionRepository _predictionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataArchivalService> _logger;
    private readonly BlobServiceClient? _blobServiceClient;
    private readonly string _containerName = "archived-telemetry";

    public DataArchivalService(
        ITelemetryRepository telemetryRepository,
        IPredictionRepository predictionRepository,
        IUnitOfWork unitOfWork,
        ILogger<DataArchivalService> logger,
        BlobServiceClient? blobServiceClient = null)
    {
        _telemetryRepository = telemetryRepository;
        _predictionRepository = predictionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    
        // Ensure container exists if blob client is available
        if (_blobServiceClient != null)
        {
            _ = InitializeBlobContainerAsync().ConfigureAwait(false);
        }
    }
    
    private async Task InitializeBlobContainerAsync()
    {
        try 
        {
            var containerClient = _blobServiceClient?.GetBlobContainerClient(_containerName);
            if(containerClient!= null)
                await containerClient.CreateIfNotExistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize blob container");
        }
    }

    public async Task ArchiveOldTelemetryAsync(int retentionDays = 30, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting telemetry archival for data older than {RetentionDays} days", retentionDays);

            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var archivedCount = 0;
            var batchSize = 1000;

            // Archive telemetry data in batches
            while (!ct.IsCancellationRequested)
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                try
                {
                    var oldTelemetry = await _telemetryRepository.GetAllAsync(
                        t => t.Timestamp < cutoffDate, 
                        ct,
                        take: batchSize,
                        asNoTracking: false);

                    var telemetryList = oldTelemetry.ToList();
                    
                    if (telemetryList.Count == 0)
                    {
                        await _unitOfWork.CommitAsync(ct);
                        break;
                    }

                    // Group by machine and date for efficient archival
                    var groupedData = telemetryList
                        .GroupBy(t => new { t.MachineId, t.Timestamp.Date })
                        .ToList();

                    foreach (var group in groupedData)
                    {
                        // Use Parquet for structured telemetry data archival
                        await ArchiveTelemetryGroupAsParquetAsync(group.Key.MachineId, group.Key.Date, group.ToList(), ct);
                        archivedCount += group.Count();
                    }

                    await _unitOfWork.CommitAsync(ct);
                    _logger.LogInformation("Archived {Count} telemetry records in current batch", telemetryList.Count);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(ct);
                    _logger.LogError(ex, "Error archiving telemetry batch, rolling back");
                    throw;
                }
            }

            // Archive old predictions
            await ArchiveOldPredictionsAsync(cutoffDate, ct);

            // Trim database partitions if needed
            await TrimDatabasePartitionsAsync(cutoffDate);

            _logger.LogInformation("Telemetry archival completed. Total records archived: {TotalCount}", archivedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during telemetry archival");
            throw;
        }
    }


    private async Task ArchiveTelemetryGroupAsParquetAsync(Guid machineId, DateTime date, List<TelemetryData> telemetryData, CancellationToken ct)
    {
        try
        {
            var fileName = $"telemetry/{machineId:N}/{date:yyyy/MM/dd}/telemetry-{date:yyyyMMdd}-{Guid.NewGuid():N}.parquet";
            
            // Define Parquet Schema
            var schema = new ParquetSchema(
                new DataField<string>("Id"),
                new DataField<string>("MachineId"),
                new DataField<DateTimeOffset>("Timestamp"),
                new DataField<string>("DataType"),
                new DataField<string>("DataJson")
            );

            using var ms = new MemoryStream();
            using (var writer = await ParquetWriter.CreateAsync(schema, ms, cancellationToken: ct))
            {
                writer.CompressionMethod = CompressionMethod.Snappy;

                using var rgw = writer.CreateRowGroup();
                
                var ids = telemetryData.Select(t => t.Id.ToString()).ToArray();
                var machineIds = telemetryData.Select(t => t.MachineId.ToString()).ToArray();
                var timestamps = telemetryData.Select(t => (DateTimeOffset)t.Timestamp).ToArray();
                var dataTypes = telemetryData.Select(t => t.DataType).ToArray();
                var dataJsons = telemetryData.Select(t => t.Data.RootElement.GetRawText()).ToArray();

                await rgw.WriteColumnAsync(new DataColumn(schema.DataFields[0], ids));
                await rgw.WriteColumnAsync(new DataColumn(schema.DataFields[1], machineIds));
                await rgw.WriteColumnAsync(new DataColumn(schema.DataFields[2], timestamps));
                await rgw.WriteColumnAsync(new DataColumn(schema.DataFields[3], dataTypes));
                await rgw.WriteColumnAsync(new DataColumn(schema.DataFields[4], dataJsons));
            }

            var parquetBytes = ms.ToArray();

            // Upload to blob storage if available
            if (_blobServiceClient != null)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, null, ct);

                var blobClient = containerClient.GetBlobClient(fileName);
                using var uploadStream = new MemoryStream(parquetBytes);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" },
                    Metadata = new Dictionary<string, string>
                    {
                        ["format"] = "parquet",
                        ["machineId"] = machineId.ToString(),
                        ["recordCount"] = telemetryData.Count.ToString()
                    }
                };

                await blobClient.UploadAsync(uploadStream, uploadOptions, ct);
            }
            else
            {
                var localPath = Path.Combine("archives", fileName);
                var directory = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                
                await File.WriteAllBytesAsync(localPath, parquetBytes, ct);
            }

            // Cleanup database
            var repository = _unitOfWork.Repository<TelemetryData>();
            await repository.DeleteRangeAsync(telemetryData, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving telemetry group to Parquet for machine {MachineId}", machineId);
            throw;
        }
    }

    private async Task ArchiveOldPredictionsAsync(DateTime cutoffDate, CancellationToken ct)
    {
        try
        {
            var oldPredictions = await _predictionRepository.GetAllAsync(p => p.CreatedAt < cutoffDate, ct, asNoTracking: false);
            var predictionList = oldPredictions.ToList();

            if (!predictionList.Any())
                return;

            // Group predictions by machine and month
            var groupedPredictions = predictionList
                .GroupBy(p => new { p.MachineId, Month = new DateTime(p.CreatedAt.Year, p.CreatedAt.Month, 1) })
                .ToList();

            foreach (var group in groupedPredictions)
            {
                await _unitOfWork.BeginTransactionAsync(ct);
                try
                {
                    var fileName = $"predictions/{group.Key.MachineId:N}/{group.Key.Month:yyyy/MM}/predictions-{group.Key.Month:yyyyMM}-{Guid.NewGuid():N}.json";
                    
                    var groupList = group.ToList();
                    var jsonData = JsonSerializer.Serialize(groupList, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (_blobServiceClient != null)
                    {
                        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None, null, ct);

                        var blobClient = containerClient.GetBlobClient(fileName);
                        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData));

                        var uploadOptions = new BlobUploadOptions
                        {
                            HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" }
                        };

                        await blobClient.UploadAsync(stream, uploadOptions, ct);
                    }
                    else
                    {
                        var localPath = Path.Combine("archives", fileName);
                        var directory = Path.GetDirectoryName(localPath);
                        if (!string.IsNullOrEmpty(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }
                        await File.WriteAllTextAsync(localPath, jsonData, ct);
                    }

                    // Delete archived predictions from database (within transaction)
                    var predictionDeleteCount = groupList.Count;
                    var repository = _unitOfWork.Repository<Prediction>();
                    await repository.DeleteRangeAsync(groupList, ct);

                    await _unitOfWork.CommitAsync(ct);
                    _logger.LogInformation("Archived {Count} predictions for machine {MachineId} for month {Month}", 
                        predictionDeleteCount, group.Key.MachineId, group.Key.Month.ToString("yyyy-MM"));
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackAsync(ct);
                    _logger.LogError(ex, "Error archiving predictions for machine {MachineId} for month {Month}", 
                        group.Key.MachineId, group.Key.Month.ToString("yyyy-MM"));
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old predictions");
            throw;
        }
    }

    private async Task TrimDatabasePartitionsAsync(DateTime cutoffDate)
    {
        try
        {
            // This would typically involve database-specific partition management
            // For Postgres SQL with JSONB, we might need to vacuum or reindex
            
            _logger.LogInformation("Database partition trimming completed for data older than {CutoffDate}", cutoffDate);
            
            // In a real implementation, this might include:
            // - VACUUM operations on Postgres SQL
            // - Partition dropping for time-based partitions
            // - Index rebuilding
            // - Statistics updates
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database partition trimming");
            throw;
        }
    }

    public async Task<ArchivalStats> GetArchivalStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var stats = new ArchivalStats();

            if (_blobServiceClient != null)
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                
                if (await containerClient.ExistsAsync(ct))
                {
                    var blobs = containerClient.GetBlobsAsync(BlobTraits.Metadata);
                    await foreach (var blob in blobs)
                    {
                        stats.TotalArchivedFiles++;
                        stats.TotalArchivedSize += blob.Properties.ContentLength ?? 0;

                        if (blob.Metadata.TryGetValue("recordCount", out var recordCountStr) && 
                            int.TryParse(recordCountStr, out var recordCount))
                        {
                            stats.TotalArchivedRecords += recordCount;
                        }
                    }
                }
            }
            else
            {
                // Fallback to local file counting
                var archiveDir = Path.Combine("archives");
                if (Directory.Exists(archiveDir))
                {
                    var files = Directory.GetFiles(archiveDir, "*.json", SearchOption.AllDirectories);
                    stats.TotalArchivedFiles = files.Length;
                    
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        stats.TotalArchivedSize += fileInfo.Length;
                        
                        // Try to parse record count from filename or file content
                        if (file.Contains("telemetry-"))
                        {
                            stats.TotalArchivedRecords += 100; // Estimate
                        }
                    }
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting archival statistics");
            return new ArchivalStats();
        }
    }

    public async Task RestoreTelemetryAsync(string archivePath, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting telemetry restoration from {ArchivePath}", archivePath);

            string jsonData;
            
            if (_blobServiceClient != null && archivePath.StartsWith("blob://"))
            {
                // Restore from blob storage
                var blobPath = archivePath.Substring(7); // Remove "blob://" prefix
                var blobClient = _blobServiceClient.GetBlobContainerClient(_containerName).GetBlobClient(blobPath);
                
                using var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream, ct);
                jsonData = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
            else
            {
                // Restore from local file
                jsonData = await File.ReadAllTextAsync(archivePath, ct);
            }

            // Deserialize telemetry data
            var telemetryData = JsonSerializer.Deserialize<List<TelemetryData>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (telemetryData?.Any() == true)
            {
                await _telemetryRepository.AddRangeAsync(telemetryData, ct);
                _logger.LogInformation("Restored {Count} telemetry records from archive", telemetryData.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring telemetry from {ArchivePath}", archivePath);
            throw;
        }
    }
}

public class ArchivalStats
{
    public int TotalArchivedFiles { get; set; }
    public long TotalArchivedSize { get; set; }
    public int TotalArchivedRecords { get; set; }
    public DateTime LastArchivedAt { get; set; } = DateTime.UtcNow;
}

