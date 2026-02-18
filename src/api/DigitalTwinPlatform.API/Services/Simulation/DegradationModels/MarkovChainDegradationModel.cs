using System;

namespace DigitalTwinPlatform.API.Services.Simulation.DegradationModels;

/// <summary>
/// Implements a simplified Markov Chain degradation model.
/// Represents health as discrete states (0=Failure, 1=Critical, 2=Warning, 3=Healthy).
/// </summary>
public class MarkovChainDegradationModel : IDegradationModel
{
    private const double NoiseFactor = 0.0001;
    private const double MinStateBuffer = 0.01;
    
    // State Thresholds
    private const double ThresholdHealthy = 0.9;
    private const double ThresholdWarning = 0.7;
    private const double ThresholdCritical = 0.3;

    // Continuous Values
    private const double ValueHealthy = 0.95;
    private const double ValueWarning = 0.80;
    private const double ValueCritical = 0.50;
    private const double ValueFailure = 0.0;

    private readonly double[][] _transitionMatrix;
    private readonly double _timeScale;

    public MarkovChainDegradationModel(Dictionary<string, object>? parameters = null)
    {
        _transitionMatrix = new double[4][];
        _transitionMatrix[3] = new double[] { 0, 0, 0.01, 0.99 }; // From Healthy
        _transitionMatrix[2] = new double[] { 0, 0.02, 0.98, 0 }; // From Warning
        _transitionMatrix[1] = new double[] { 0.05, 0.95, 0, 0 }; // From Critical
        _transitionMatrix[0] = new double[] { 1.0, 0, 0, 0 };    // From Failure

        _timeScale = parameters != null && parameters.TryGetValue("timeScale", out var ts) 
            ? Convert.ToDouble(ts) 
            : 1.0;
    }

    public double Step(double currentState, TimeSpan delta, Random random)
    {
        int discreteState = GetDiscreteState(currentState);
        if (discreteState == 0) return ValueFailure;

        double[] probs = _transitionMatrix[discreteState];
        double roll = random.NextDouble();
        
        double stayProb = probs[discreteState];
        double degradeProb = 1.0 - stayProb;
        double adjustedDegradeProb = degradeProb * (delta.TotalHours * _timeScale);
        
        if (roll < adjustedDegradeProb)
        {
            return GetContinuousValue(discreteState - 1);
        }

        // Slight drift within the current state band to show continuous activity
        return Math.Max(currentState - NoiseFactor, GetContinuousValue(discreteState - 1) + MinStateBuffer);
    }

    private int GetDiscreteState(double health)
    {
        if (health >= ThresholdHealthy) return 3;
        if (health >= ThresholdWarning) return 2;
        if (health >= ThresholdCritical) return 1;
        return 0;
    }

    private double GetContinuousValue(int discreteState)
    {
        return discreteState switch
        {
            3 => ValueHealthy,
            2 => ValueWarning,
            1 => ValueCritical,
            _ => ValueFailure
        };
    }
}
