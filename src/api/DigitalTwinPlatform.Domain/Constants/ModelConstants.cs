namespace DigitalTwinPlatform.Domain.Constants;

public static class ModelConstants
{
    public static class Features
    {
        public const string TemperatureMean = "temp_mean";
        public const string TemperatureStd = "temp_std";
        public const string VibrationRms = "vib_rms";
        public const string VibrationPeak = "vib_peak";
        public const string PressureMean = "pressure_mean";
        public const string PressureVariance = "pressure_var";
        public const string HumidityMean = "humidity_mean";
        public const string PowerConsumption = "power_consumption";
        public const string CycleCount = "cycle_count";
    }

    public static class Benchmarks
    {
        public const string NasaCmapss = "NASA_CMAPSS";
        public const string FemtoBearing = "FEMTO_BEARING";
        
        public static (double Mean, double Std) GetStats(string name) => name.ToUpperInvariant() switch
        {
            "FEMTO" or FemtoBearing => (0.4, 0.15),
            _ => (0.5, 0.2) // Default to NASA-like stats
        };
    }
}
