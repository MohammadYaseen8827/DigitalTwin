using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Services.Analytics;

public class BenchmarkDatasetLoader : IBenchmarkDatasetLoader
{
    private readonly ILogger<BenchmarkDatasetLoader> _logger;
    private readonly string _benchmarkDataPath;

    public BenchmarkDatasetLoader(
        ILogger<BenchmarkDatasetLoader> logger)
    {
        _logger = logger;
        _benchmarkDataPath = Path.Combine("data", "benchmarks");
    }

    public async Task<List<BenchmarkDataPoint>> LoadDatasetAsync(
        string datasetName,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Loading benchmark dataset: {Dataset}", datasetName);

        var datasetPath = GetDatasetPath(datasetName);
        
        if (!File.Exists(datasetPath))
        {
            _logger.LogWarning(
                "Benchmark dataset file not found: {Path}. Generating synthetic benchmark data.",
                datasetPath);
            
            // Generate synthetic benchmark data if actual dataset not available
            return GenerateSyntheticBenchmarkData(datasetName, 1000);
        }

        try
        {
            // Load dataset from file (CSV format expected)
            var lines = await File.ReadAllLinesAsync(datasetPath, ct);
            var dataPoints = new List<BenchmarkDataPoint>();

            // Skip header if present
            var startIndex = lines[0].StartsWith("Feature") || lines[0].StartsWith("feature") ? 1 : 0;

            for (int i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(',');
                if (values.Length < 11) continue; // Need at least 10 features + RUL

                var features = new Dictionary<string, double>();
                for (int j = 0; j < 10; j++)
                {
                    if (double.TryParse(values[j], out var value))
                    {
                        features[$"Feature{j + 1}"] = value;
                    }
                }

                if (double.TryParse(values[10], out var rul))
                {
                    dataPoints.Add(new BenchmarkDataPoint
                    {
                        Index = i - startIndex,
                        Features = features,
                        ActualRul = rul,
                        Metadata = new Dictionary<string, object>
                        {
                            ["Dataset"] = datasetName,
                            ["RowIndex"] = i
                        }
                    });
                }
            }

            _logger.LogInformation(
                "Loaded {Count} data points from benchmark dataset: {Dataset}",
                dataPoints.Count, datasetName);

            return dataPoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading benchmark dataset: {Dataset}", datasetName);
            // Fallback to synthetic data
            return GenerateSyntheticBenchmarkData(datasetName, 1000);
        }
    }

    public Task<List<string>> GetAvailableDatasetsAsync(CancellationToken ct = default)
    {
        var datasets = new List<string> { "NASA_CMAPSS", "FEMTO_Bearing", "Synthetic" };
        
        // Check if dataset files exist
        var availableDatasets = datasets.Where(d => 
            File.Exists(GetDatasetPath(d)) || d == "Synthetic").ToList();

        return Task.FromResult(availableDatasets);
    }

    public Task<BenchmarkDatasetInfo> GetDatasetInfoAsync(
        string datasetName,
        CancellationToken ct = default)
    {
        var info = new BenchmarkDatasetInfo
        {
            Name = datasetName,
            IsAvailable = File.Exists(GetDatasetPath(datasetName)) || datasetName == "Synthetic"
        };

        switch (datasetName.ToUpper())
        {
            case "NASA_CMAPSS":
                info.Description = "NASA Commercial Modular Aero-Propulsion System Simulation dataset";
                info.SampleCount = 10000; // Approximate
                info.Statistics = new Dictionary<string, double>
                {
                    ["MeanRUL"] = 150.0,
                    ["StdRUL"] = 50.0
                };
                break;
            case "FEMTO_BEARING":
                info.Description = "FEMTO Bearing Dataset for Prognostics";
                info.SampleCount = 2800; // Approximate
                info.Statistics = new Dictionary<string, double>
                {
                    ["MeanRUL"] = 200.0,
                    ["StdRUL"] = 60.0
                };
                break;
            case "SYNTHETIC":
                info.Description = "Synthetic benchmark dataset for testing";
                info.SampleCount = 1000;
                info.Statistics = new Dictionary<string, double>
                {
                    ["MeanRUL"] = 180.0,
                    ["StdRUL"] = 45.0
                };
                break;
            default:
                info.Description = "Unknown benchmark dataset";
                break;
        }

        info.DataPath = GetDatasetPath(datasetName);
        return Task.FromResult(info);
    }

    private string GetDatasetPath(string datasetName)
    {
        return Path.Combine(_benchmarkDataPath, $"{datasetName}.csv");
    }

    private List<BenchmarkDataPoint> GenerateSyntheticBenchmarkData(
        string datasetName,
        int sampleCount)
    {
        _logger.LogInformation(
            "Generating synthetic benchmark data for {Dataset}: {Count} samples",
            datasetName, sampleCount);

        var random = new Random(42); // Fixed seed for reproducibility
        var dataPoints = new List<BenchmarkDataPoint>();

        // Generate synthetic data with realistic RUL distribution
        var meanRul = datasetName.ToUpper() switch
        {
            "NASA_CMAPSS" => 150.0,
            "FEMTO_BEARING" => 200.0,
            _ => 180.0
        };

        var stdRul = datasetName.ToUpper() switch
        {
            "NASA_CMAPSS" => 50.0,
            "FEMTO_BEARING" => 60.0,
            _ => 45.0
        };

        for (int i = 0; i < sampleCount; i++)
        {
            // Generate features with some correlation to RUL
            var rul = Math.Max(1, meanRul + (random.NextDouble() - 0.5) * stdRul * 2);
            
            var features = new Dictionary<string, double>();
            for (int j = 1; j <= 10; j++)
            {
                // Features correlated with degradation (inverse correlation with RUL)
                var baseValue = 50.0 + j * 5.0;
                var degradationFactor = (300 - rul) / 300.0; // Higher degradation = lower RUL
                var noise = (random.NextDouble() - 0.5) * 10.0;
                features[$"Feature{j}"] = baseValue + degradationFactor * 30.0 + noise;
            }

            dataPoints.Add(new BenchmarkDataPoint
            {
                Index = i,
                Features = features,
                ActualRul = rul,
                Metadata = new Dictionary<string, object>
                {
                    ["Dataset"] = datasetName,
                    ["Synthetic"] = true
                }
            });
        }

        return dataPoints;
    }
}
