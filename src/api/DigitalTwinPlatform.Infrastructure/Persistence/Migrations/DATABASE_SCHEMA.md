# Digital Twin Platform Database Schema

## Overview
This document describes the enhanced database schema for the Digital Twin Platform, implemented with Entity Framework Core 8 and PostgreSQL.

## Entity Relationships

```
ProductionLine (1) ----< (N) Machine
Machine (1) ----< (N) TelemetryData
Machine (1) ----< (N) Prediction
Machine (1) ----< (N) MaintenanceRecord
Machine (1) ----< (N) Alert
Prediction (1) ----< (N) Alert (via RelatedPredictionId)
```

## Database Entities

### 1. Machine
Equipment definition with enhanced configuration fields.

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| Name | varchar(100) | Machine name (value object) |
| Type | varchar(50) | Machine type (value object) |
| Location | varchar(200) | Physical location (NEW) |
| InstallationDate | timestamp with time zone | Installation date (NEW) |
| LastMaintenanceDate | timestamp with time zone | Last maintenance date (NEW) |
| Configuration | jsonb | Operational parameters (NEW) |
| Properties | jsonb | Extended properties |
| Status | text | Equipment status |
| IsActive | boolean | Active flag |
| RemainingUsefulLifeDays | double precision | RUL in days |
| FailureProbability | double precision | Failure probability (0-1) |
| HealthStatus | text | Health classification |
| ProductionLineId | uuid | Foreign key |
| TenantId | uuid | Multi-tenant support |
| CreatedAt | timestamp with time zone | Creation timestamp |
| UpdatedAt | timestamp with time zone | Last update timestamp |

**Indexes:**
- IX_Machines_Status
- IX_Machines_IsActive
- IX_Machines_Location
- IX_Machines_HealthStatus
- IX_Machines_RemainingUsefulLifeDays

### 2. TelemetryData
Sensor readings with explicit and flexible data storage.

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| MachineId | uuid | Foreign key |
| DataType | varchar(50) | Type of telemetry |
| Temperature | double precision | Temperature reading (NEW) |
| Vibration | double precision | Vibration reading (NEW) |
| Pressure | double precision | Pressure reading (NEW) |
| Rpm | double precision | RPM reading (NEW) |
| HealthScore | double precision | Computed health score (NEW) |
| Data | jsonb | Additional metrics |
| Timestamp | timestamp with time zone | Reading timestamp |

**Indexes:**
- IX_TelemetryData_MachineId
- IX_TelemetryData_Timestamp
- IX_TelemetryData_MachineId_Timestamp (composite)
- IX_TelemetryData_HealthScore

### 3. Prediction
RUL predictions with confidence intervals and contributing factors.

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| MachineId | uuid | Foreign key |
| RemainingUsefulLifeDays | decimal(18,4) | RUL prediction |
| RulLowerBound | decimal(18,4) | Confidence lower bound (NEW) |
| RulUpperBound | decimal(18,4) | Confidence upper bound (NEW) |
| Confidence | decimal(5,4) | Prediction confidence (NEW) |
| FailureProbability | decimal(5,4) | Failure probability |
| HealthStatus | text | Health classification |
| ContributingFactors | jsonb | Contributing factors (NEW) |
| FeatureContributions | jsonb | Feature importance scores |
| ModelVersion | varchar(50) | Model version |
| ModelVersionId | uuid | Model version reference |
| PredictionTime | timestamp with time zone | Prediction timestamp (NEW) |
| CreatedAt | timestamp with time zone | Creation timestamp |

**Indexes:**
- IX_Predictions_MachineId
- IX_Predictions_CreatedAt
- IX_Predictions_Confidence
- IX_Predictions_HealthStatus
- IX_Predictions_MachineId_CreatedAt (composite)
- IX_Predictions_HealthStatus_FailureProbability (composite)

### 4. Alert
System alerts linked to predictions.

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| MachineId | uuid | Foreign key |
| RelatedPredictionId | uuid | Related prediction (NEW) |
| Message | varchar(1000) | Alert message |
| Severity | varchar(20) | Alert severity |
| CreatedAt | timestamp with time zone | Creation timestamp |
| IsAcknowledged | boolean | Acknowledgment flag |
| AcknowledgedBy | varchar(200) | Acknowledged by user |
| AcknowledgedAt | timestamp with time zone | Acknowledgment timestamp |

**Indexes:**
- IX_Alerts_MachineId
- IX_Alerts_CreatedAt
- IX_Alerts_Severity
- IX_Alerts_IsAcknowledged
- IX_Alerts_MachineId_IsAcknowledged (composite)
- IX_Alerts_RelatedPredictionId

### 5. MaintenanceRecord
Maintenance records with parts and cost tracking.

**Columns:**
| Column | Type | Description |
|--------|------|-------------|
| Id | uuid | Primary key |
| MachineId | uuid | Foreign key |
| AlertId | uuid | Related alert |
| Date | timestamp with time zone | Maintenance date |
| PlannedDate | timestamp with time zone | Planned date |
| CompletionDate | timestamp with time zone | Completion date |
| Status | varchar(20) | Maintenance status |
| Type | varchar(50) | Maintenance type (NEW) |
| Description | varchar(1000) | Description (NEW) |
| Technician | varchar(200) | Technician name (NEW) |
| PartsReplaced | varchar(1000) | Parts replaced (NEW) |
| Cost | decimal(18,2) | Maintenance cost (NEW) |
| CostCurrency | varchar(10) | Currency (NEW) |
| Notes | varchar(2000) | Additional notes |
| WorkOrderDetails | jsonb | Work order data |

**Indexes:**
- IX_MaintenanceRecords_MachineId
- IX_MaintenanceRecords_Date
- IX_MaintenanceRecords_Status
- IX_MaintenanceRecords_Type (NEW)
- IX_MaintenanceRecords_Technician (NEW)

## JSONB Columns

### Machine.Configuration
```json
{
  "operationalParams": {
    "minTemperature": 20,
    "maxTemperature": 80,
    "optimalRpm": 2000,
    "maxPressure": 150
  },
  "scheduledMaintenanceIntervalDays": 30,
  "lastCalibration": "2024-01-15T10:30:00Z"
}
```

### Prediction.ContributingFactors
```json
{
  "temperatureWear": 0.25,
  "vibrationImpact": 0.30,
  "operatingHours": 0.20,
  "maintenanceHistory": 0.15,
  "environmentalFactors": 0.10
}
```

### TelemetryData.Data
```json
{
  "sensorType": "multi_sensor",
  "rawValues": {
    "temperature": 65.5,
    "vibration": 2.3,
    "pressure": 95.0,
    "rpm": 2100
  },
  "calibration": {
    "lastCalibrated": "2024-01-10T08:00:00Z",
    "calibrationStatus": "valid"
  }
}
```

## Migrations

### Migration History
| Migration | Description |
|-----------|-------------|
| 20260119135900_Initial | Initial schema |
| 20260119142719_UpdateModel | Model updates |
| 20260119202419_AddModelVersion | Added ModelVersion entity |
| 20260208000000_EnhancedEntities | Added enhanced fields |

### Applying Migrations
```bash
dotnet ef database update --project DigitalTwinPlatform.Infrastructure
```

### Creating New Migrations
```bash
dotnet ef migrations add <Name> --project DigitalTwinPlatform.Infrastructure
```

## Seed Data

Seed data is located in `Migrations/SeedData/`:
- `ExtendedSeedDataUnified.sql` - Original unified seed data
- `EnhancedSeedData.sql` - Enhanced seed data with new fields

**Seed Data Summary:**
- 10 Production Lines
- 150 Machines (15 per production line)
- 5 Predictions per machine
- 4 Maintenance records per machine
- 12 Telemetry records per hour for 7 days
- Alerts linked to predictions

## Performance Considerations

### Indexes
All foreign key columns are indexed. Additional composite indexes are created for common query patterns:

- `(MachineId, Timestamp)` for time-series queries
- `(MachineId, CreatedAt)` for prediction history
- `(HealthStatus, FailureProbability)` for filtering

### Query Optimization
- Use JSONB columns for flexible data storage
- Consider partitioning for large telemetry tables
- Use materialized views for aggregations if needed

## Backup and Restore

```bash
# Backup
pg_dump -h localhost -U postgres -d DigitalTwinPlatform > backup.sql

# Restore
psql -h localhost -U postgres -d DigitalTwinPlatform < backup.sql
```
