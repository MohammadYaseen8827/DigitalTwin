using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class ShapServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public int TimeoutSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
}

public class ShapServiceClient : IShapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ShapServiceClient> _logger;
    private readonly ShapServiceOptions _options;

    public ShapServiceClient(
        HttpClient httpClient,
        IOptions<ShapServiceOptions> options,
        ILogger<ShapServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    public async Task<ShapResult> CalculateShapValuesAsync(
        Dictionary<string, double> features,
        Dictionary<string, double>[] backgroundData,
        string modelPath,
        CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("SHAP service is disabled. Returning empty contributions.");
            return new ShapResult();
        }

        try
        {
            var featureArray = features.Values.ToArray();
            var featureNames = features.Keys.ToArray();
            var backgroundArray = backgroundData.Select(d => d.Values.ToArray()).ToArray();

            var request = new
            {
                model_path = modelPath,
                features = featureArray,
                feature_names = featureNames,
                background_data = backgroundArray
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/calculate-shap", content, ct);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError("SHAP service returned error: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"SHAP service error: {response.StatusCode} - {errorContent}");
            }

            var resultJson = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ShapServiceResponse>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to deserialize SHAP service response");
            }

            return new ShapResult
            {
                Contributions = result.Contributions ?? new Dictionary<string, double>(),
                BaseValue = result.BaseValue,
                ShapValues = result.ShapValues ?? new List<double>()
            };
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "SHAP service request timed out after {Timeout}s", _options.TimeoutSeconds);
            throw new TimeoutException($"SHAP service request timed out after {_options.TimeoutSeconds}s", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "SHAP service HTTP error");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling SHAP service");
            throw;
        }
    }

    public async Task<bool> UpdateBackgroundDataAsync(
        string modelPath,
        Dictionary<string, double>[] backgroundData,
        CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return false;
        }

        try
        {
            var backgroundArray = backgroundData.Select(d => d.Values.ToArray()).ToArray();

            var request = new
            {
                model_path = modelPath,
                background_data = backgroundArray
            };

            var jsonContent = JsonSerializer.Serialize(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/update-background", content, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update background data in SHAP service");
            return false;
        }
    }

    public async Task<bool> IsServiceAvailableAsync(CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return false;
        }

        try
        {
            var response = await _httpClient.GetAsync("/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SHAP service health check failed");
            return false;
        }
    }
}

internal class ShapServiceResponse
{
    public Dictionary<string, double>? Contributions { get; set; }
    public double BaseValue { get; set; }
    public List<double>? ShapValues { get; set; }
}
