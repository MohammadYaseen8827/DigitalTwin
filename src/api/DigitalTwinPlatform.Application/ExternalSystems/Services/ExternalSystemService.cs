using DigitalTwinPlatform.Application.ExternalSystems.Dtos;
using DigitalTwinPlatform.Application.ExternalSystems.Services;
using DigitalTwinPlatform.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace DigitalTwinPlatform.Application.ExternalSystems.Services;

public class ExternalSystemService : IExternalSystemService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalSystemService> _logger;
    private readonly string _baseUrl;

    public ExternalSystemService(HttpClient httpClient, ILogger<ExternalSystemService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Get base URL from configuration
        _baseUrl = configuration.GetConnectionString("ExternalSystemsApi") 
                   ?? configuration["ExternalSystems:BaseUrl"]
                   ?? throw new InvalidOperationException("ExternalSystems:BaseUrl configuration is required");
    }

    // External System Management
    public async Task<Result<ExternalSystemDto>> CreateExternalSystemAsync(ExternalSystemCreateDto systemDto, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/external-systems", systemDto, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExternalSystemDto>(ct);
                return new Result<ExternalSystemDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to create external system: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<ExternalSystemDto>.Failure($"Failed to create external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating external system");
            return new Result<ExternalSystemDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<ExternalSystemDto>> UpdateExternalSystemAsync(Guid id, ExternalSystemUpdateDto systemDto, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/external-systems/{id}", systemDto, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExternalSystemDto>(ct);
                return new Result<ExternalSystemDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to update external system {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<ExternalSystemDto>.Failure($"Failed to update external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating external system {Id}", id);
            return new Result<ExternalSystemDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/external-systems/{id}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to delete external system {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to delete external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting external system {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result<ExternalSystemDto>> GetExternalSystemByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems/{id}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ExternalSystemDto>(ct);
                return new Result<ExternalSystemDto>.Success(result!);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<ExternalSystemDto>.Failure("External system not found");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get external system {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<ExternalSystemDto>.Failure($"Failed to get external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external system {Id}", id);
            return new Result<ExternalSystemDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<ExternalSystemDto>>> GetAllExternalSystemsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ExternalSystemDto>>(ct);
                return new Result<List<ExternalSystemDto>>.Success(result ?? new List<ExternalSystemDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get all external systems: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<ExternalSystemDto>>.Failure($"Failed to get external systems: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all external systems");
            return new Result<List<ExternalSystemDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<ExternalSystemDto>>> GetConnectedExternalSystemsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems/connected", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ExternalSystemDto>>(ct);
                return new Result<List<ExternalSystemDto>>.Success(result ?? new List<ExternalSystemDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get connected external systems: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<ExternalSystemDto>>.Failure($"Failed to get connected external systems: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connected external systems");
            return new Result<List<ExternalSystemDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<ExternalSystemDto>>> GetExternalSystemsByTypeAsync(string systemType, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems/type/{Uri.EscapeDataString(systemType)}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ExternalSystemDto>>(ct);
                return new Result<List<ExternalSystemDto>>.Success(result ?? new List<ExternalSystemDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get external systems by type {Type}: {StatusCode} - {Error}", systemType, response.StatusCode, errorContent);
            return new Result<List<ExternalSystemDto>>.Failure($"Failed to get external systems by type: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external systems by type {Type}", systemType);
            return new Result<List<ExternalSystemDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<ExternalSystemStatus>> GetExternalSystemStatusAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems/{id}/status", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var statusString = await response.Content.ReadAsStringAsync(ct);
                if (Enum.TryParse<ExternalSystemStatus>(statusString, true, out var status))
                {
                    return new Result<ExternalSystemStatus>.Success(status);
                }
                
                return new Result<ExternalSystemStatus>.Failure("Invalid status returned from external system");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get external system status {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<ExternalSystemStatus>.Failure($"Failed to get external system status: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting external system status {Id}", id);
            return new Result<ExternalSystemStatus>.Failure(ex.Message);
        }
    }

    public async Task<Result> ConnectExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/external-systems/{id}/connect", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to connect external system {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to connect external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting external system {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DisconnectExternalSystemAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/external-systems/{id}/disconnect", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to disconnect external system {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to disconnect external system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting external system {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result<bool>> TestExternalSystemConnectionAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/external-systems/{id}/test-connection", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>(ct);
                return new Result<bool>.Success(result);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to test external system connection {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<bool>.Failure($"Failed to test external system connection: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing external system connection {Id}", id);
            return new Result<bool>.Failure(ex.Message);
        }
    }

    // System Integration Management
    public async Task<Result<SystemIntegrationDto>> CreateSystemIntegrationAsync(SystemIntegrationCreateDto integrationDto, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/integrations", integrationDto, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SystemIntegrationDto>(ct);
                return new Result<SystemIntegrationDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to create system integration: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<SystemIntegrationDto>.Failure($"Failed to create system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system integration");
            return new Result<SystemIntegrationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<SystemIntegrationDto>> UpdateSystemIntegrationAsync(Guid id, SystemIntegrationUpdateDto integrationDto, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/integrations/{id}", integrationDto, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SystemIntegrationDto>(ct);
                return new Result<SystemIntegrationDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to update system integration {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<SystemIntegrationDto>.Failure($"Failed to update system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system integration {Id}", id);
            return new Result<SystemIntegrationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/integrations/{id}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to delete system integration {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to delete system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting system integration {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result<SystemIntegrationDto>> GetSystemIntegrationByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/integrations/{id}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SystemIntegrationDto>(ct);
                return new Result<SystemIntegrationDto>.Success(result!);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<SystemIntegrationDto>.Failure("System integration not found");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get system integration {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<SystemIntegrationDto>.Failure($"Failed to get system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system integration {Id}", id);
            return new Result<SystemIntegrationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/integrations", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SystemIntegrationDto>>(ct);
                return new Result<List<SystemIntegrationDto>>.Success(result ?? new List<SystemIntegrationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get system integrations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<SystemIntegrationDto>>.Failure($"Failed to get system integrations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system integrations");
            return new Result<List<SystemIntegrationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/integrations/system/{externalSystemId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SystemIntegrationDto>>(ct);
                return new Result<List<SystemIntegrationDto>>.Success(result ?? new List<SystemIntegrationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get system integrations by system {SystemId}: {StatusCode} - {Error}", externalSystemId, response.StatusCode, errorContent);
            return new Result<List<SystemIntegrationDto>>.Failure($"Failed to get system integrations by system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system integrations by system {SystemId}", externalSystemId);
            return new Result<List<SystemIntegrationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetSystemIntegrationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/integrations/entity/{entityId}/{entityType}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SystemIntegrationDto>>(ct);
                return new Result<List<SystemIntegrationDto>>.Success(result ?? new List<SystemIntegrationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get system integrations by entity {EntityId} {EntityType}: {StatusCode} - {Error}", entityId, entityType, response.StatusCode, errorContent);
            return new Result<List<SystemIntegrationDto>>.Failure($"Failed to get system integrations by entity: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system integrations by entity {EntityId} {EntityType}", entityId, entityType);
            return new Result<List<SystemIntegrationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<SystemIntegrationDto>>> GetActiveSystemIntegrationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/integrations/active", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<SystemIntegrationDto>>(ct);
                return new Result<List<SystemIntegrationDto>>.Success(result ?? new List<SystemIntegrationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get active system integrations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<SystemIntegrationDto>>.Failure($"Failed to get active system integrations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active system integrations");
            return new Result<List<SystemIntegrationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result> EnableSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/integrations/{id}/enable", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to enable system integration {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to enable system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling system integration {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DisableSystemIntegrationAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/integrations/{id}/disable", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to disable system integration {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to disable system integration: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling system integration {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    // Data Synchronization Management
    public async Task<Result<DataSynchronizationDto>> CreateDataSynchronizationAsync(DataSynchronizationCreateDto syncDto, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/synchronizations", syncDto, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DataSynchronizationDto>(ct);
                return new Result<DataSynchronizationDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to create data synchronization: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<DataSynchronizationDto>.Failure($"Failed to create data synchronization: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating data synchronization");
            return new Result<DataSynchronizationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<DataSynchronizationDto>> GetDataSynchronizationByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/{id}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DataSynchronizationDto>(ct);
                return new Result<DataSynchronizationDto>.Success(result!);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new Result<DataSynchronizationDto>.Failure("Data synchronization not found");
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get data synchronization {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<DataSynchronizationDto>.Failure($"Failed to get data synchronization: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data synchronization {Id}", id);
            return new Result<DataSynchronizationDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get data synchronizations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get data synchronizations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data synchronizations");
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetPendingSynchronizationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/pending", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get pending synchronizations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get pending synchronizations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending synchronizations");
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetFailedSynchronizationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/failed", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get failed synchronizations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get failed synchronizations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed synchronizations");
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsBySystemAsync(Guid externalSystemId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/system/{externalSystemId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get synchronizations by system {SystemId}: {StatusCode} - {Error}", externalSystemId, response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get synchronizations by system: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting synchronizations by system {SystemId}", externalSystemId);
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetDataSynchronizationsByEntityAsync(Guid entityId, EntityType entityType, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/entity/{entityId}/{entityType}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get synchronizations by entity {EntityId} {EntityType}: {StatusCode} - {Error}", entityId, entityType, response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get synchronizations by entity: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting synchronizations by entity {EntityId} {EntityType}", entityId, entityType);
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<List<DataSynchronizationDto>>> GetRecentSynchronizationsAsync(int limit = 50, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/synchronizations/recent?limit={limit}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<DataSynchronizationDto>>(ct);
                return new Result<List<DataSynchronizationDto>>.Success(result ?? new List<DataSynchronizationDto>());
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to get recent synchronizations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<List<DataSynchronizationDto>>.Failure($"Failed to get recent synchronizations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent synchronizations");
            return new Result<List<DataSynchronizationDto>>.Failure(ex.Message);
        }
    }

    public async Task<Result<int>> ProcessPendingSynchronizationsAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/synchronizations/process-pending", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<int>(ct);
                return new Result<int>.Success(result);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to process pending synchronizations: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return new Result<int>.Failure($"Failed to process pending synchronizations: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending synchronizations");
            return new Result<int>.Failure(ex.Message);
        }
    }

    public async Task<Result> CancelDataSynchronizationAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/synchronizations/{id}/cancel", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                return new Result.Success();
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to cancel data synchronization {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result.Failure($"Failed to cancel data synchronization: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling data synchronization {Id}", id);
            return new Result.Failure(ex.Message);
        }
    }

    public async Task<Result<DataSynchronizationDto>> RetryFailedSynchronizationAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/synchronizations/{id}/retry", null, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<DataSynchronizationDto>(ct);
                return new Result<DataSynchronizationDto>.Success(result!);
            }
            
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Failed to retry failed synchronization {Id}: {StatusCode} - {Error}", id, response.StatusCode, errorContent);
            return new Result<DataSynchronizationDto>.Failure($"Failed to retry failed synchronization: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed synchronization {Id}", id);
            return new Result<DataSynchronizationDto>.Failure(ex.Message);
        }
    }
}