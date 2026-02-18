# Data Model: Predictive Maintenance for SME Digital Twin Platform

**Purpose**: Entity definitions and relationships for predictive maintenance system
**Created**: 2025-02-08
**Feature**: Predictive Maintenance for SME Digital Twin Platform

## Entity Relationship Diagram

```
Machine (1) -----> (N) TelemetryData
Machine (1) -----> (N) RULPrediction
Machine (1) -----> (1) DegradationModel
Machine (1) -----> (N) MaintenanceAlert
Machine (1) -----> (N) SyntheticDataGeneration
```

## Core Entities

### Machine

Represents physical equipment being monitored in the SME facility.

**Fields**:
- `Id`: Guid (Primary Key) - Unique identifier for the machine
- `Name`: string (Required, Max 200) - Human-readable machine name
- `Type`: MachineType (Required) - Type of equipment (Motor, Pump, Compressor, Gearbox, Bearing)
- `Configuration`: JSONB (Required) - Operational parameters and settings
- `Location`: string (Max 500) - Physical location within facility
- `Status`: MachineStatus (Required) - Current operational state
- `CreatedAt`: DateTime (Required) - When machine was added to system
- `UpdatedAt`: DateTime (Required) - Last update timestamp
- `IsActive`: boolean (Required, Default: true) - Whether machine is actively monitored

**Enums**:
```csharp
public enum MachineType
{
    Motor = 1,
    Pump = 2,
    Compressor = 3,
    Gearbox = 4,
    Bearing = 5
}

public enum MachineStatus
{
    Online = 1,
    Offline = 2,
    Maintenance = 3,
    Error = 4,
    Degraded = 5
}
```

**Validation Rules**:
- Name must be unique within location
- Configuration must contain required parameters for machine type
- Location cannot be empty for active machines
- Status transitions must follow business rules

### TelemetryData

Time-series sensor readings from machines for real-time monitoring and prediction.

**Fields**:
- `Id`: Guid (Primary Key) - Unique telemetry record identifier
- `MachineId`: Guid (Foreign Key to Machine) - Source machine
- `Timestamp`: DateTime (Required) - When the reading was taken
- `SensorType`: SensorType (Required) - Type of sensor measurement
- `Value`: double (Required) - Sensor reading value
- `Unit`: string (Max 50) - Measurement unit (e.g., "Hz", "°C", "PSI")
- `Confidence`: double (Range: 0-1) - Confidence in measurement accuracy
- `Quality`: DataQuality (Required) - Quality assessment of reading

**Enums**:
```csharp
public enum SensorType
{
    Vibration = 1,
    Temperature = 2,
    Load = 3,
    Pressure = 4,
    FlowRate = 5,
    Current = 6,
    Voltage = 7,
    Speed = 8
}

public enum DataQuality
{
    Good = 1,
    Questionable = 2,
    Poor = 3,
    Interpolated = 4,
    Estimated = 5
}
```

**Validation Rules**:
- Timestamp must be within last 24 hours for real-time data
- Value must be within reasonable ranges for sensor type
- Confidence must be >= 0.7 for high-quality predictions
- MachineId must reference active machine

### DegradationModel

Mathematical models representing equipment degradation patterns for RUL prediction.

**Fields**:
- `Id`: Guid (Primary Key) - Unique model identifier
- `MachineId`: Guid (Foreign Key to Machine) - Associated machine
- `ModelType`: DegradationModelType (Required) - Type of mathematical model
- `Parameters`: JSONB (Required) - Model-specific parameters
- `ValidationMetrics`: JSONB - Statistical validation results
- `IsActive`: boolean (Required, Default: true) - Whether model is currently used
- `Version`: string (Max 20) - Model version for tracking
- `CreatedAt`: DateTime (Required) - Model creation timestamp
- `LastValidated`: DateTime? - Last validation timestamp

**Enums**:
```csharp
public enum DegradationModelType
{
    Wiener = 1,
    Exponential = 2,
    PhysicsInformed = 3,
    Markov = 4
}
```

**Parameter Schemas**:

**Wiener Process**:
```json
{
  "drift": 0.001,
  "diffusion": 0.01,
  "initialCondition": 0.0,
  "failureThreshold": 1.0
}
```

**Exponential Degradation**:
```json
{
  "alpha": 0.0001,
  "beta": 0.002,
  "initialCondition": 0.0,
  "failureThreshold": 1.0,
  "noiseStdDev": 0.01
}
```

**Physics-Informed**:
```json
{
  "loadFactor": 1.0,
  "speedFactor": 1.0,
  "temperatureFactor": 1.0,
  "wearCoefficient": 0.001,
  "failureThreshold": 1.0
}
```

**Markov Chain**:
```json
{
  "states": ["Healthy", "Minor", "Major", "Failure"],
  "transitionMatrix": [[0.99, 0.01, 0.0, 0.0], [0.0, 0.98, 0.02, 0.0], ...],
  "sojournTimes": [1000, 500, 100, 0]
}
```

**Validation Rules**:
- Parameters must be valid for specified model type
- Only one active model per machine
- Validation metrics must include statistical fidelity measures

### RULPrediction

Remaining Useful Life predictions with confidence intervals and explanations.

**Fields**:
- `Id`: Guid (Primary Key) - Unique prediction identifier
- `MachineId`: Guid (Foreign Key to Machine) - Target machine
- `PredictedRUL`: TimeSpan (Required) - Predicted remaining useful life
- `ConfidenceInterval`: JSONB (Required) - Statistical confidence bounds
- `FeatureImportance`: JSONB - Feature contribution analysis
- `ModelVersion`: string (Max 50) - Model version used for prediction
- `PredictionMethod`: string (Max 100) - Algorithm or method used
- `Confidence`: double (Range: 0-1) - Overall prediction confidence
- `PredictedAt`: DateTime (Required) - When prediction was generated
- `ValidUntil`: DateTime (Required) - Prediction validity period

**Confidence Interval Schema**:
```json
{
  "lowerBound": "72:00:00",
  "upperBound": "120:00:00",
  "confidenceLevel": 0.95,
  "method": "quantile_regression"
}
```

**Feature Importance Schema**:
```json
{
  "vibrationRMS": 0.35,
  "temperatureMean": 0.25,
  "loadVariation": 0.20,
  "pressureTrend": 0.15,
  "speedDeviation": 0.05
}
```

**Validation Rules**:
- RUL must be positive for active machines
- Confidence interval must have reasonable bounds
- Feature importance values must sum to 1.0
- ValidUntil must be after PredictedAt

### SyntheticDataGeneration

Records of synthetic data generation for training and validation.

**Fields**:
- `Id`: Guid (Primary Key) - Unique generation record
- `MachineType`: string (Required, Max 100) - Target machine type
- `NumberOfTrajectories`: int (Required, Min: 1) - Number of data paths generated
- `TimeRange`: TimeSpan (Required) - Simulation time period
- `RandomSeed`: int - Seed for reproducible generation
- `ValidationReport`: JSONB - Statistical validation results
- `GenerationStatistics`: JSONB - Generation performance metrics
- `Status`: GenerationStatus (Required) - Current generation status
- `CreatedAt`: DateTime (Required) - Generation initiation timestamp
- `CompletedAt`: DateTime? - Generation completion timestamp

**Enums**:
```csharp
public enum GenerationStatus
{
    Pending = 1,
    Running = 2,
    Completed = 3,
    Failed = 4,
    Validating = 5
}
```

**Validation Report Schema**:
```json
{
  "kolmogorovSmirnov": {
    "statistic": 0.12,
    "pValue": 0.85,
    "passed": true
  },
  "maximumMeanDiscrepancy": {
    "statistic": 0.08,
    "threshold": 0.1,
    "passed": true
  },
  "autocorrelation": {
    "lag1Correlation": 0.92,
    "lag5Correlation": 0.85,
    "passed": true
  },
  "benchmarkComparison": {
    "dataset": "NASA_CMAPSS",
    "similarity": 0.88,
    "passed": true
  }
}
```

**Validation Rules**:
- NumberOfTrajectories must be >= 10 for statistical significance
- TimeRange must be sufficient for degradation patterns
- Validation report must include all required statistical tests

### MaintenanceAlert

Predictive maintenance alerts and recommendations.

**Fields**:
- `Id`: Guid (Primary Key) - Unique alert identifier
- `MachineId`: Guid (Foreign Key to Machine) - Associated machine
- `AlertType`: AlertType (Required) - Category of alert
- `Severity`: AlertSeverity (Required) - Urgency level
- `Title`: string (Required, Max 200) - Alert title
- `Message`: string (Required) - Detailed alert description
- `Recommendation`: string - Suggested maintenance action
- `Confidence`: double (Range: 0-1) - Alert confidence level
- `ThresholdValue`: double - Value that triggered alert
- `CurrentValue`: double - Current measurement value
- `CreatedAt`: DateTime (Required) - Alert generation timestamp
- `AcknowledgedAt`: DateTime? - Alert acknowledgment timestamp
- `AcknowledgedBy`: string - User who acknowledged alert
- `ResolvedAt`: DateTime? - Alert resolution timestamp

**Enums**:
```csharp
public enum AlertType
{
    RULThreshold = 1,
    Anomaly = 2,
    ModelDrift = 3,
    SensorFailure = 4,
    PerformanceDegradation = 5
}

public enum AlertSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
```

**Validation Rules**:
- Alert type must match machine configuration
- Severity must align with confidence and threshold values
- Recommendation must be actionable for maintenance staff
- Cannot have resolved timestamp before acknowledged timestamp

## Database Indexes

### Performance Indexes

```sql
-- TelemetryData time-series queries
CREATE INDEX idx_telemetry_machine_time ON "TelemetryData" ("MachineId", "Timestamp" DESC);

-- RULPrediction latest queries
CREATE INDEX idx_predictions_machine_latest ON "RULPrediction" ("MachineId", "PredictedAt" DESC);

-- Active machines queries
CREATE INDEX idx_machines_active_location ON "Machine" ("IsActive", "Location") WHERE "IsActive" = true;

-- Alert management queries
CREATE INDEX idx_alerts_machine_severity ON "MaintenanceAlert" ("MachineId", "Severity", "CreatedAt" DESC);

-- JSONB configuration queries
CREATE INDEX idx_machines_config_gin ON "Machine" USING GIN ("Configuration");
CREATE INDEX idx_models_params_gin ON "DegradationModel" USING GIN ("Parameters");
```

## Data Validation Rules

### Business Logic Constraints

1. **Machine Configuration**: Each machine type must have required configuration parameters
2. **Telemetry Continuity**: Gaps in telemetry data must be flagged for data quality issues
3. **Prediction Freshness**: RUL predictions must be updated within defined intervals
4. **Alert Escalation**: Unacknowledged critical alerts must escalate within defined timeframes
5. **Model Validation**: Degradation models must pass statistical validation before activation

### Data Quality Rules

1. **Sensor Range Validation**: Telemetry values must be within physically possible ranges
2. **Temporal Consistency**: Timestamps must be monotonic and within reasonable bounds
3. **Statistical Outliers**: Extreme values must be flagged for review
4. **Confidence Thresholds**: Low-confidence predictions must trigger additional validation
5. **Synthetic Data Fidelity**: Generated data must meet minimum statistical similarity thresholds

## Migration Strategy

### Version 1.0 Schema
- Core entities for basic predictive maintenance functionality
- Support for single degradation model per machine
- Basic telemetry storage and retrieval
- Simple alert generation and management

### Future Enhancements
- Multi-model ensemble predictions
- Advanced anomaly detection algorithms
- Prescriptive maintenance recommendations
- Integration with external CMMS systems
- Mobile alert notifications

This data model provides a comprehensive foundation for implementing the predictive maintenance Digital Twin Platform while meeting all constitution requirements for SME accessibility, mathematical rigor, and real-time performance.
