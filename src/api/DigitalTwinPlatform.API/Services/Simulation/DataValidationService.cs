using MathNet.Numerics.Statistics;
using MathNet.Numerics.Distributions;

namespace DigitalTwinPlatform.API.Services.Simulation;

public interface IDataValidationService
{
    ValidationResult ValidateBatch(IEnumerable<double> data, string dataType);
    ValidationResult ValidateAgainstBenchmark(IEnumerable<double> data, string benchmarkDataset);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public double KsStatistic { get; set; }
    public double PValue { get; set; }
    public double Autocorrelation { get; set; }
    public double KlDivergence { get; set; } // KL Divergence from benchmark
    public double MmdStatistic { get; set; } // Maximum Mean Discrepancy
    public string Message { get; set; } = string.Empty;
}

public class DataValidationService : IDataValidationService
{
    public ValidationResult ValidateBatch(IEnumerable<double> data, string dataType)
    {
        var values = data.ToArray();
        if (values.Length < 10)
        {
            return new ValidationResult { IsValid = true, Message = "Insufficient data for full validation" };
        }

        // 1. Kolmogorov-Smirnov Test (against Normal Distribution as baseline)
        var mean = values.Average();
        var stdDev = Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));
        
        // Simple KS-test implementation check against normal
        var ks = CalculateKsStatistic(values, mean, stdDev);
        
        // 2. Autocorrelation (Lag 1)
        var autocorrelation = CalculateAutocorrelation(values, 1);

        var isValid = ks < 0.2 && Math.Abs(autocorrelation) < 0.95;

        return new ValidationResult
        {
            IsValid = isValid,
            KsStatistic = ks,
            Autocorrelation = autocorrelation,
            PValue = 1.0 - ks, // Approximation
            Message = isValid ? "Data quality passed validation" : "Data quality warnings detected"
        };
    }

    private double CalculateKsStatistic(double[] values, double mean, double stdDev)
    {
        if (stdDev == 0) return 1.0;
        
        var sorted = values.OrderBy(x => x).ToArray();
        var n = sorted.Length;
        var maxDiff = 0.0;

        for (int i = 0; i < n; i++)
        {
            var empiricalCdf = (double)(i + 1) / n;
            var theoreticalCdf = Normal.CDF(mean, stdDev, sorted[i]);
            var diff = Math.Abs(empiricalCdf - theoreticalCdf);
            if (diff > maxDiff) maxDiff = diff;
        }

        return maxDiff;
    }

    private double CalculateAutocorrelation(double[] values, int lag)
    {
        if (values.Length <= lag) return 0;
        
        var mean = values.Average();
        var numerator = 0.0;
        var denominator = 0.0;

        for (int i = 0; i < values.Length; i++)
        {
            denominator += Math.Pow(values[i] - mean, 2);
            if (i >= lag)
            {
                numerator += (values[i] - mean) * (values[i - lag] - mean);
            }
        }

        return denominator == 0 ? 0 : numerator / denominator;
    }

    public ValidationResult ValidateAgainstBenchmark(IEnumerable<double> data, string benchmarkDataset)
    {
        var values = data.ToArray();
        
        var (benchmarkMean, benchmarkStd) = DigitalTwinPlatform.Domain.Constants.ModelConstants.Benchmarks.GetStats(benchmarkDataset);

        var mean = values.Average();
        var std = Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));

        // KL Divergence (Approximated for two Normal distributions)
        var kl = Math.Log(benchmarkStd / std) + (Math.Pow(std, 2) + Math.Pow(mean - benchmarkMean, 2)) / (2 * Math.Pow(benchmarkStd, 2)) - 0.5;

        // Calculate MMD with a fixed sigma (heuristic)
        var mmd = CalculateMmd(values, benchmarkMean, benchmarkStd);

        return new ValidationResult
        {
            IsValid = kl < 1.0 && mmd < 0.5, 
            KlDivergence = kl,
            MmdStatistic = mmd,
            Message = (kl < 1.0 && mmd < 0.5) ? $"PASSED: Matches {benchmarkDataset} distribution" : $"FAILED: Diverges from {benchmarkDataset}"
        };
    }

    private double CalculateMmd(double[] sample, double benchmarkMean, double benchmarkStd)
    {
        // For efficiency in a large simulation, we use a sampled MMD or 
        // compare the sample against a synthetic benchmark population
        var rnd = new Random(42);
        var benchmarkSample = Enumerable.Range(0, Math.Min(sample.Length, 100))
            .Select(_ => Normal.Sample(benchmarkMean, benchmarkStd))
            .ToArray();

        var n = sample.Length;
        var m = benchmarkSample.Length;

        // Simplified RBF Kernel MMD
        double sigma = 1.0; 
        
        double kXX = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                kXX += RbfKernel(sample[i], sample[j], sigma);
        kXX /= (n * n);

        double kYY = 0;
        for (int i = 0; i < m; i++)
            for (int j = 0; j < m; j++)
                kYY += RbfKernel(benchmarkSample[i], benchmarkSample[j], sigma);
        kYY /= (m * m);

        double kXY = 0;
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                kXY += RbfKernel(sample[i], benchmarkSample[j], sigma);
        kXY /= (n * m);

        return Math.Sqrt(Math.Max(0, kXX + kYY - 2 * kXY));
    }

    private double RbfKernel(double x, double y, double sigma)
    {
        return Math.Exp(-Math.Pow(x - y, 2) / (2 * Math.Pow(sigma, 2)));
    }
}
