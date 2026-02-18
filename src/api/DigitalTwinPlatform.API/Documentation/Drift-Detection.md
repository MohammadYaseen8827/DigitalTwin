# Data Drift Detection Service

## Overview
This document describes the data drift detection implementation for monitoring model performance degradation in the Digital Twin Platform.

## Implementation Details

### Core Components

**1. IDataDriftService Interface**
- Defines contract for drift detection operations
- Supports multiple statistical methods (KS Test, Wasserstein, PSI)
- Provides historical tracking and alerting capabilities

**2. DataDriftService Class**
- Implements core drift detection algorithms
- Maintains drift history and monitoring state
- Provides configurable thresholds per model

**3. DriftController**
- Exposes RESTful endpoints for drift operations
- Supports real-time monitoring and historical analysis
- Enables configuration management

### Statistical Methods Implemented

**Kolmogorov-Smirnov Test (KS Test)**
- Measures maximum difference between cumulative distributions
- Good for detecting any type of distribution shift
- Computationally efficient

**Wasserstein Distance**
- Measures "earth mover's distance" between distributions
- Sensitive to both shape and location changes
- Provides intuitive distance interpretation

**Population Stability Index (PSI)**
- Compares population distributions across buckets
- Industry standard for financial services
- Good for categorical and continuous features

### API Endpoints

**GET /api/drift/status**
Returns current drift status for all monitored models

**GET /api/drift/history/{modelName}?hours=24**
Retrieves historical drift metrics for specified period

**GET /api/drift/thresholds/{modelName}**
Gets current drift detection thresholds

**PUT /api/drift/thresholds/{modelName}**
Configures drift detection thresholds

**GET /api/drift/report?days=7&modelName=RULModel**
Generates comprehensive drift report

**POST /api/drift/detect**
Manually triggers drift detection with custom data

### Configuration

```json
{
  "DriftDetection": {
    "DefaultWindowSize": 1000,
    "DefaultFeatureThreshold": 0.1,
    "DefaultPredictionThreshold": 0.15,
    "EnableAlerts": true,
    "AlertCooldownMinutes": 60,
    "ModelSpecificThresholds": {}
  }
}
```

### Drift Levels and Severity

**None (0.0 - 0.05)**: No significant drift
**Low (0.05 - 0.1)**: Minor drift, monitor closely
**Moderate (0.1 - 0.2)**: Noticeable drift, consider investigation
**High (0.2 - 0.3)**: Significant drift, retraining recommended
**Severe (> 0.3)**: Critical drift, immediate action required

### Alerting System

- Configurable cooldown periods to prevent alert spam
- Model-specific severity thresholds
- Automatic recommendation generation
- Integration-ready alert payload

## Usage Examples

### Basic Drift Detection
```csharp
var referenceData = new Dictionary<string, double[]>
{
    ["temperature"] = trainingTemperatures,
    ["pressure"] = trainingPressures
};

var currentData = new Dictionary<string, double[]>
{
    ["temperature"] = recentTemperatures,
    ["pressure"] = recentPressures
};

var result = await driftService.DetectDriftAsync(
    referenceData, 
    currentData, 
    new DriftThresholds { FeatureDriftThreshold = 0.1 });
```

### Monitoring Setup
```csharp
// Configure model-specific thresholds
await driftService.ConfigureThresholdsAsync(
    "RUL_Model_v1",
    new DriftThresholds 
    { 
        FeatureDriftThreshold = 0.08,
        PredictionDriftThreshold = 0.12 
    });

// Monitor continuously
var monitoringResult = await driftService.MonitorDriftAsync(
    "RUL_Model_v1",
    featureStream,
    windowSize: 500);
```

### API Integration
```javascript
// Get current drift status
const response = await fetch('/api/drift/status', {
    headers: { 'Authorization': `Bearer ${token}` }
});

// Configure thresholds
await fetch('/api/drift/thresholds/RUL_Model_v1', {
    method: 'PUT',
    headers: { 
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
        featureDriftThreshold: 0.08,
        predictionDriftThreshold: 0.12
    })
});
```

## Best Practices

### Threshold Configuration
- Start with conservative thresholds (0.1-0.15)
- Adjust based on domain knowledge and historical data
- Use different thresholds for different feature types
- Monitor false positive/negative rates

### Monitoring Strategy
- Implement continuous monitoring for production models
- Set up alerts for moderate+ drift levels
- Regular threshold review and adjustment
- Maintain reference datasets for comparison

### Performance Considerations
- Cache reference distributions when possible
- Use appropriate window sizes for your data frequency
- Consider computational cost of different statistical methods
- Implement sampling for large datasets

## Integration Points

### With ML Pipeline
- Automatic drift detection during model inference
- Integration with model retraining triggers
- Performance degradation monitoring
- A/B testing support

### With Alerting Systems
- Slack/Teams notifications
- Email alerts for critical drift
- PagerDuty integration for severe cases
- Custom webhook support

### With Monitoring Tools
- Prometheus/Grafana dashboard integration
- Custom metrics export
- Historical trend analysis
- Performance correlation analysis

## Future Enhancements

### Planned Features
- Concept drift detection for time-series data
- Multi-dimensional drift analysis
- Automated threshold tuning
- Drift root cause analysis
- Integration with AutoML retraining pipelines

### Advanced Analytics
- Feature importance in drift detection
- Drift prediction and forecasting
- Causal analysis of drift sources
- Counterfactual drift scenarios