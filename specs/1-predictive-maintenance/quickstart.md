# Quick Start Guide: Predictive Maintenance for SME Digital Twin Platform

**Purpose**: Rapid deployment and configuration guide for SME predictive maintenance
**Created**: 2025-02-08
**Feature**: Predictive Maintenance for SME Digital Twin Platform

## Prerequisites

### System Requirements
- **Operating System**: Windows 10/11, Linux (Ubuntu 20.04+), macOS 10.15+
- **.NET 8 SDK**: Latest version with ASP.NET Core runtime
- **Node.js 18+**: With npm 8+ for frontend development
- **PostgreSQL 15+**: Database server with JSONB support
- **Docker Desktop**: For containerized deployment (optional but recommended)
- **Git**: For source code management

### Hardware Requirements (SME Deployment)
- **Minimum**: 4GB RAM, 2 CPU cores, 20GB storage
- **Recommended**: 8GB RAM, 4 CPU cores, 50GB storage
- **Network**: Stable internet connection for updates and support

## Installation Steps

### 1. Repository Setup

```bash
# Clone the repository
git clone https://github.com/your-org/digital-twin-platform.git
cd digital-twin-platform

# Checkout the predictive maintenance feature branch
git checkout 1-predictive-maintenance

# Verify branch
git branch --show-current
```

### 2. Backend Configuration

```bash
# Navigate to backend directory
cd src/api/DigitalTwinPlatform.API

# Restore .NET dependencies
dotnet restore

# Copy environment configuration
cp appsettings.example.json appsettings.Development.json

# Edit configuration file
notepad appsettings.Development.json
```

**Configuration Settings**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=DigitalTwinPlatform;Username=postgres;Password=your_password"
  },
  "SignalR": {
    "EnableDetailedErrors": true,
    "EnableMessagePack": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "DigitalTwinPlatform": "Debug"
    }
  },
  "SyntheticData": {
    "DefaultTrajectories": 100,
    "ValidationThreshold": 0.8,
    "BenchmarkDatasets": ["NASA_CMAPSS", "FEMTO"]
  },
  "MachineLearning": {
    "ModelRetrainingInterval": "24:00:00",
    "MinimumDataPoints": 1000,
    "ConfidenceThreshold": 0.7
  }
}
```

### 3. Database Setup

```bash
# Using Docker (recommended)
docker run --name postgres-digital-twin \
  -e POSTGRES_DB=DigitalTwinPlatform \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=your_password \
  -p 5432:5432 \
  -d postgres:15

# Or install PostgreSQL locally
# Follow PostgreSQL installation guide for your OS
```

### 4. Frontend Setup

```bash
# Navigate to frontend directory
cd src/ui/digital-twin-dashboard

# Install Node.js dependencies
npm install

# Copy environment configuration
cp .env.example .env

# Edit environment file
notepad .env
```

**Frontend Environment**:
```env
VITE_API_BASE_URL=http://localhost:5000
VITE_SIGNALR_URL=http://localhost:5000/hubs
VITE_APP_TITLE=Digital Twin Platform
VITE_REFRESH_INTERVAL=5000
VITE_MAX_TELEMETRY_POINTS=1000
```

## Running the Application

### Development Mode

```bash
# Terminal 1: Start backend
cd src/api/DigitalTwinPlatform.API
dotnet run

# Terminal 2: Start frontend
cd src/ui/digital-twin-dashboard
npm run dev
```

### Production Mode

```bash
# Using Docker Compose (recommended for SMEs)
docker-compose up -d

# Or build and run manually
# Backend
cd src/api/DigitalTwinPlatform.API
dotnet publish -c Release
./bin/Release/net8.0/publish/DigitalTwinPlatform.API

# Frontend
cd src/ui/digital-twin-dashboard
npm run build
npm run preview
```

## Initial Configuration

### 1. Access the Application

- **Frontend Dashboard**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger

### 2. Create First Machine

**Via API**:
```bash
curl -X POST "http://localhost:5000/api/machines" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Production Line Motor 1",
    "type": "Motor",
    "location": "Production Area A",
    "configuration": {
      "powerRating": 150,
      "ratedSpeed": 1800,
      "operatingTemperature": 75,
      "vibrationThreshold": 5.0
    }
  }'
```

**Via Dashboard**:
1. Navigate to "Machines" section
2. Click "Add New Machine"
3. Fill in machine details
4. Select machine type from dropdown
5. Configure operational parameters
6. Click "Save"

### 3. Configure Degradation Model

```bash
curl -X POST "http://localhost:5000/api/mathematical-modeling/configure" \
  -H "Content-Type: application/json" \
  -d '{
    "machineId": "your-machine-id",
    "modelType": "Wiener",
    "parameters": {
      "drift": 0.001,
      "diffusion": 0.01,
      "initialCondition": 0.0,
      "failureThreshold": 1.0
    }
  }'
```

### 4. Generate Synthetic Training Data

```bash
curl -X POST "http://localhost:5000/api/synthetic-data/generate" \
  -H "Content-Type: application/json" \
  -d '{
    "machineType": "Motor",
    "numberOfTrajectories": 100,
    "timeRange": "72:00:00",
    "randomSeed": 12345
  }'
```

### 5. Validate Synthetic Data

```bash
curl -X POST "http://localhost:5000/api/synthetic-data/validate" \
  -H "Content-Type: application/json" \
  -d '{
    "telemetryData": [...],
    "benchmarkDataset": "NASA_CMAPSS"
  }'
```

## Monitoring and Operations

### Real-Time Dashboard Features

1. **Machine Overview**: Status of all monitored equipment
2. **Telemetry Visualization**: Real-time sensor data charts
3. **RUL Predictions**: Remaining useful life forecasts
4. **Alert Management**: Maintenance alerts and recommendations
5. **Model Performance**: Prediction accuracy and confidence metrics

### Alert Configuration

```bash
curl -X POST "http://localhost:5000/api/alerts/rules" \
  -H "Content-Type: application/json" \
  -d '{
    "machineId": "your-machine-id",
    "alertType": "RULThreshold",
    "threshold": "72:00:00",
    "severity": "High",
    "enabled": true
  }'
```

### Performance Monitoring

- **System Health**: http://localhost:5000/health
- **Metrics**: http://localhost:5000/metrics
- **Logs**: Check application logs for errors and warnings

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Verify PostgreSQL is running
   - Check connection string in appsettings.json
   - Ensure database exists and user has permissions

2. **Frontend Cannot Connect to API**
   - Verify backend is running on correct port
   - Check CORS configuration
   - Ensure API base URL is correct in .env file

3. **SignalR Connection Issues**
   - Check firewall settings
   - Verify SignalR hub configuration
   - Ensure WebSocket support is enabled

4. **Synthetic Data Generation Slow**
   - Increase available memory
   - Check database performance
   - Reduce number of trajectories for testing

### Performance Optimization

1. **Database Optimization**
   ```sql
   -- Create recommended indexes
   CREATE INDEX idx_telemetry_machine_time ON "TelemetryData" ("MachineId", "Timestamp" DESC);
   
   -- Update statistics
   ANALYZE "TelemetryData";
   ```

2. **Application Caching**
   ```json
   // Add to appsettings.json
   "Caching": {
     "MemoryCacheSize": 128,
     "PredictionCacheDuration": "00:05:00"
   }
   ```

3. **Frontend Performance**
   - Use browser developer tools to identify bottlenecks
   - Implement virtual scrolling for large datasets
   - Optimize chart rendering with debounced updates

## Support and Maintenance

### Regular Maintenance Tasks

1. **Weekly**: Review prediction accuracy and model performance
2. **Monthly**: Update machine configurations based on operational changes
3. **Quarterly**: Retrain ML models with new data
4. **Annually**: Review and update degradation model parameters

### Getting Help

- **Documentation**: Check inline API documentation at /swagger
- **Logs**: Review application logs for error details
- **Community**: Post issues to project GitHub repository
- **Support**: Contact support team for SME-specific assistance

### Backup and Recovery

```bash
# Database backup
pg_dump -h localhost -U postgres DigitalTwinPlatform > backup_$(date +%Y%m%d).sql

# Configuration backup
cp appsettings.json backup/appsettings_$(date +%Y%m%d).json

# Restore database
psql -h localhost -U postgres DigitalTwinPlatform < backup_20240208.sql
```

## Next Steps

After successful setup:

1. **Add More Machines**: Configure additional equipment types
2. **Fine-tune Models**: Adjust degradation model parameters
3. **Set Up Alerts**: Configure maintenance alert thresholds
4. **Train Operators**: Provide training for maintenance staff
5. **Monitor Performance**: Track prediction accuracy and system health

This quick start guide enables rapid deployment of the predictive maintenance Digital Twin Platform for SMEs, with step-by-step instructions for configuration, operation, and maintenance.
