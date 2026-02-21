# System Integrity Audit Report

## Production Readiness Verdict: **FAIL**

> **Audit Status:** Critical mismatches and unimplemented logic detected. Deployment not recommended.

## 1. Backend Feature Map
### Controller: AdvancedAnalyticsController
- **Endpoint:** `EnsemblePrediction`
  - Route: `/api/AdvancedAnalytics/predictions/{machineId}/ensemble`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `PredictionDto`
  - Used In Frontend: Yes

- **Endpoint:** `DeepLearningPrediction`
  - Route: `/api/AdvancedAnalytics/predictions/{machineId}/deep-learning`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `PredictionDto`
  - Used In Frontend: Yes

- **Endpoint:** `DetectAnomalies`
  - Route: `/api/AdvancedAnalytics/anomaly-detection/{machineId}`
  - Method: `HttpPost`
  - Request DTO: `AnomalyDetectionRequest`
  - Response DTO: `AnomalyDetectionResult`
  - Used In Frontend: Yes

- **Endpoint:** `ForecastTimeSeries`
  - Route: `/api/AdvancedAnalytics/forecasting/{machineId}/{metric}`
  - Method: `HttpPost`
  - Request DTO: `ForecastRequest`
  - Response DTO: `ForecastResult`
  - Used In Frontend: Yes

- **Endpoint:** `GenerateMaintenanceRecommendation`
  - Route: `/api/AdvancedAnalytics/prescriptive/maintenance/{machineId}`
  - Method: `HttpPost`
  - Request DTO: `MaintenanceRecommendationRequest`
  - Response DTO: `PrescriptiveRecommendation`
  - Used In Frontend: Yes

- **Endpoint:** `OptimizeProductionSchedule`
  - Route: `/api/AdvancedAnalytics/prescriptive/scheduling`
  - Method: `HttpPost`
  - Request DTO: `SchedulingOptimizationRequest`
  - Response DTO: `SchedulingRecommendation`
  - Used In Frontend: Yes

- **Endpoint:** `OptimizeResourceAllocation`
  - Route: `/api/AdvancedAnalytics/prescriptive/resource-allocation`
  - Method: `HttpPost`
  - Request DTO: `ResourceAllocationRequest`
  - Response DTO: `ResourceAllocationPlan`
  - Used In Frontend: Yes

- **Endpoint:** `OptimizeMaintenanceCosts`
  - Route: `/api/AdvancedAnalytics/prescriptive/cost-optimization`
  - Method: `HttpPost`
  - Request DTO: `CostOptimizationRequest`
  - Response DTO: `CostOptimizationResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetAdvancedAnalyticsDashboard`
  - Route: `/api/AdvancedAnalytics/dashboard/{machineId}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `AdvancedAnalyticsDashboard`
  - Used In Frontend: Yes

### Controller: AIModelController
- **Endpoint:** `GetAllModels`
  - Route: `/api/AIModel`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<AIModelDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetModel`
  - Route: `/api/AIModel/{id}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `AIModelDto`
  - Used In Frontend: Yes

- **Endpoint:** `DeployModel`
  - Route: `/api/AIModel`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `AIModelDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateModel`
  - Route: `/api/AIModel/{id}`
  - Method: `HttpPut`
  - Request DTO: `UpdateAIModelCommand`
  - Response DTO: `AIModelDto`
  - Used In Frontend: Yes

- **Endpoint:** `Predict`
  - Route: `/api/AIModel/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteModel(string id)
        {
            try
            {
                var command = new DeleteAIModelCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AI model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Test model prediction
        // </summary>
        [HttpPost("{id}/predict`
  - Method: `HttpDelete`
  - Request DTO: `ModelInputDto`
  - Response DTO: `ModelPredictionDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetModelMetrics`
  - Route: `/api/AIModel/{id}/metrics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ModelMetricsDto`
  - Used In Frontend: Yes

- **Endpoint:** `RetrainModel`
  - Route: `/api/AIModel/{id}/retrain`
  - Method: `HttpPost`
  - Request DTO: `RetrainModelCommand`
  - Response DTO: `AIModelDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetCompatibleMachines`
  - Route: `/api/AIModel/{id}/compatible-machines`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<string>`
  - Used In Frontend: Yes

- **Endpoint:** `GetDeploymentStatus`
  - Route: `/api/AIModel/{id}/deploy-to-machines")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeployToMachines(string id, [FromBody] DeployToMachinesDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _aiService.DeployModelToMachines(id, request.MachineIds);
                return Ok(new { Message = "Model deployed to specified machines successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying model {ModelId} to machines", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Get model deployment status
        // </summary>
        [HttpGet("{id}/deployment-status`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `DeploymentStatusDto`
  - Used In Frontend: Yes

- **Endpoint:** `ValidateModel`
  - Route: `/api/AIModel/validate-model`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `ModelValidationResultDto`
  - Used In Frontend: Yes

### Controller: AlertRulesController
### Controller: AlertsController
- **Endpoint:** `GetActiveAlerts`
  - Route: `/api/Alerts`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<AlertDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetAlert`
  - Route: `/api/Alerts/{id:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `AlertDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetAllAlerts`
  - Route: `/api/Alerts/{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await alertService.AcknowledgeAlertAsync(id, userId);
        return NoContent();
    }

    // <summary>
    // Resolves (deletes) an alert from the system.
    // Used when alerts have been addressed and are no longer relevant.
    // </summary>
    // <param name="id">The unique identifier of the alert to resolve.</param>
    // <returns>No content on successful resolution.</returns>
    // <response code="204">Alert resolved successfully.</response>
    // <response code="401">Unauthorized - Authentication required.</response>
    // <response code="404">Alert not found.</response>
    // <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await alertService.ResolveAlertAsync(id);
        return NoContent();
    }

    // <summary>
    // Retrieves all alerts including acknowledged ones for a machine.
    // </summary>
    // <param name="machineId">Optional machine ID filter.</param>
    // <returns>List of all alerts for the specified machine.</returns>
    // <response code="200">Returns list of all alerts successfully.</response>
    // <response code="401">Unauthorized - Authentication required.</response>
    // <response code="500">Internal server error.</response>
    [HttpGet("all`
  - Method: `HttpPut`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<AlertDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetStats`
  - Route: `/api/Alerts/stats`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `AlertStatsDto`
  - Used In Frontend: Yes

- **Endpoint:** `Search`
  - Route: `/api/Alerts/search`
  - Method: `HttpGet`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<AlertDto>`
  - Used In Frontend: Yes

### Controller: AntiForgeryController
### Controller: AuthController
### Controller: AzureDigitalTwinController
### Controller: BenchmarkValidationController
- **Endpoint:** `ValidateModel`
  - Route: `/api/BenchmarkValidation/validate`
  - Method: `HttpPost`
  - Request DTO: `ValidateModelRequest`
  - Response DTO: `BenchmarkValidationResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetAvailableBenchmarks`
  - Route: `/api/BenchmarkValidation/benchmarks`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<string>`
  - Used In Frontend: Yes

- **Endpoint:** `GetBenchmarkInfo`
  - Route: `/api/BenchmarkValidation/benchmarks/{datasetName}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `BenchmarkDatasetInfo`
  - Used In Frontend: Yes

### Controller: DashboardController
- **Endpoint:** `GetStats`
  - Route: `/api/Dashboard/stats`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `DashboardStatsDto`
  - Used In Frontend: Yes

### Controller: DataArchivalController
### Controller: DegradationModelingController
- **Endpoint:** `SolveExponentialDegradation`
  - Route: `/api/DegradationModeling/solve/exponential`
  - Method: `HttpPost`
  - Request DTO: `ExponentialDegradationRequest`
  - Response DTO: `ODESolution`
  - Used In Frontend: Yes

- **Endpoint:** `SolvePowerLawDegradation`
  - Route: `/api/DegradationModeling/solve/powerlaw`
  - Method: `HttpPost`
  - Request DTO: `PowerLawDegradationRequest`
  - Response DTO: `ODESolution`
  - Used In Frontend: Yes

- **Endpoint:** `SolveMultiVariableDegradation`
  - Route: `/api/DegradationModeling/solve/multivariable`
  - Method: `HttpPost`
  - Request DTO: `MultiVariableDegradationRequest`
  - Response DTO: `ODESolution`
  - Used In Frontend: Yes

- **Endpoint:** `SolveStochasticDegradation`
  - Route: `/api/DegradationModeling/solve/stochastic`
  - Method: `HttpPost`
  - Request DTO: `StochasticDegradationRequest`
  - Response DTO: `StochasticSolutionResult`
  - Used In Frontend: Yes

- **Endpoint:** `EstimateExponentialParameters`
  - Route: `/api/DegradationModeling/estimate/exponential`
  - Method: `HttpPost`
  - Request DTO: `ParameterEstimationRequest`
  - Response DTO: `ParameterEstimationResult`
  - Used In Frontend: Yes

- **Endpoint:** `EstimatePowerLawParameters`
  - Route: `/api/DegradationModeling/estimate/powerlaw`
  - Method: `HttpPost`
  - Request DTO: `ParameterEstimationRequest`
  - Response DTO: `ParameterEstimationResult`
  - Used In Frontend: Yes

- **Endpoint:** `EstimateMultiVariableParameters`
  - Route: `/api/DegradationModeling/estimate/multivariable`
  - Method: `HttpPost`
  - Request DTO: `MultiVariableEstimationRequest`
  - Response DTO: `ParameterEstimationResult`
  - Used In Frontend: Yes

- **Endpoint:** `CompareModels`
  - Route: `/api/DegradationModeling/compare`
  - Method: `HttpPost`
  - Request DTO: `ModelComparisonRequest`
  - Response DTO: `ModelComparisonResult`
  - Used In Frontend: Yes

- **Endpoint:** `ValidateParameters`
  - Route: `/api/DegradationModeling/validate`
  - Method: `HttpPost`
  - Request DTO: `ParameterValidationRequest`
  - Response DTO: `ParameterValidationResult`
  - Used In Frontend: Yes

### Controller: DriftController
- **Endpoint:** `GetCurrentDriftStatus`
  - Route: `/api/Drift/status`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `Dictionary<string, DriftDetectionResult>`
  - Used In Frontend: Yes

- **Endpoint:** `GetDriftHistory`
  - Route: `/api/Drift/history/{modelName}`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<DriftMetrics>`
  - Used In Frontend: Yes

- **Endpoint:** `GetThresholds`
  - Route: `/api/Drift/thresholds/{modelName}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `DriftThresholds`
  - Used In Frontend: Yes

- **Endpoint:** `GenerateReport`
  - Route: `/api/Drift/thresholds/{modelName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConfigureThresholds(
        string modelName,
        [FromBody] DriftThresholds thresholds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            / Validate thresholds
            if (thresholds.FeatureDriftThreshold <= 0 || thresholds.FeatureDriftThreshold > 1)
            {
                return BadRequest(new { error = "Feature drift threshold must be between 0 and 1" });
            }

            if (thresholds.PredictionDriftThreshold <= 0 || thresholds.PredictionDriftThreshold > 1)
            {
                return BadRequest(new { error = "Prediction drift threshold must be between 0 and 1" });
            }

            await driftService.ConfigureThresholdsAsync(modelName, thresholds, cancellationToken);
            
            return Ok(new { message = $"Thresholds configured successfully for model '{modelName}'" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to configure thresholds", details = ex.Message });
        }
    }

    // <summary>
    // Generates drift report for specified period
    // </summary>
    // <param name="modelName">Model identifier (optional - if null, all models)</param>
    // <param name="days">Period in days (default: 7)</param>
    // <param name="cancellationToken">Cancellation token</param>
    // <returns>Drift report</returns>
    [HttpGet("report`
  - Method: `HttpPut`
  - Request DTO: `Query:int`
  - Response DTO: `DriftReport`
  - Used In Frontend: Yes

- **Endpoint:** `DetectDrift`
  - Route: `/api/Drift/detect`
  - Method: `HttpPost`
  - Request DTO: `DriftDetectionRequest`
  - Response DTO: `DriftDetectionResult`
  - Used In Frontend: Yes

### Controller: ExternalSystemsController
- **Endpoint:** `GetAllExternalSystems`
  - Route: `/api/ExternalSystems`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<ExternalSystemDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetConnectedExternalSystems`
  - Route: `/api/ExternalSystems/connected`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<ExternalSystemDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetExternalSystemsByType`
  - Route: `/api/ExternalSystems/type/{systemType}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<ExternalSystemDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetExternalSystemById`
  - Route: `/api/ExternalSystems/{id:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ExternalSystemDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetExternalSystemStatus`
  - Route: `/api/ExternalSystems/{id:guid}/status`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ExternalSystemStatus`
  - Used In Frontend: Yes

- **Endpoint:** `CreateExternalSystem`
  - Route: `/api/ExternalSystems`
  - Method: `HttpPost`
  - Request DTO: `ExternalSystemCreateDto`
  - Response DTO: `ExternalSystemDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateExternalSystem`
  - Route: `/api/ExternalSystems/{id:guid}`
  - Method: `HttpPut`
  - Request DTO: `ExternalSystemUpdateDto`
  - Response DTO: `ExternalSystemDto`
  - Used In Frontend: Yes

- **Endpoint:** `TestExternalSystemConnection`
  - Route: `/api/ExternalSystems/{id:guid}")]
public async Task<ActionResult> DeleteExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/connect
[HttpPost("{id:guid}/connect")]
public async Task<ActionResult> ConnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.ConnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/disconnect
[HttpPost("{id:guid}/disconnect")]
public async Task<ActionResult> DisconnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisconnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/test
[HttpPost("{id:guid}/test`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `bool`
  - Used In Frontend: Yes

- **Endpoint:** `GetSystemIntegrations`
  - Route: `/api/ExternalSystems/integrations`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<SystemIntegrationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetSystemIntegrationsBySystem`
  - Route: `/api/ExternalSystems/{systemId:guid}/integrations`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<SystemIntegrationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetActiveSystemIntegrations`
  - Route: `/api/ExternalSystems/integrations/active`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<SystemIntegrationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `CreateSystemIntegration`
  - Route: `/api/ExternalSystems/integrations`
  - Method: `HttpPost`
  - Request DTO: `SystemIntegrationCreateDto`
  - Response DTO: `SystemIntegrationDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateSystemIntegration`
  - Route: `/api/ExternalSystems/integrations/{id:guid}`
  - Method: `HttpPut`
  - Request DTO: `SystemIntegrationUpdateDto`
  - Response DTO: `SystemIntegrationDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetDataSynchronizations`
  - Route: `/api/ExternalSystems/integrations/{id:guid}")]
public async Task<ActionResult> DeleteSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/integrations/{id}/enable
[HttpPost("integrations/{id:guid}/enable")]
public async Task<ActionResult> EnableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.EnableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/integrations/{id}/disable
[HttpPost("integrations/{id:guid}/disable")]
public async Task<ActionResult> DisableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ Data Synchronization endpoints
/ GET: api/externalsystems/synchronizations
[HttpGet("synchronizations`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `List<DataSynchronizationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetPendingSynchronizations`
  - Route: `/api/ExternalSystems/synchronizations/pending`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<DataSynchronizationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetFailedSynchronizations`
  - Route: `/api/ExternalSystems/synchronizations/failed`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<DataSynchronizationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetRecentSynchronizations`
  - Route: `/api/ExternalSystems/synchronizations/recent`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `List<DataSynchronizationDto>`
  - Used In Frontend: Yes

- **Endpoint:** `CreateDataSynchronization`
  - Route: `/api/ExternalSystems/synchronizations`
  - Method: `HttpPost`
  - Request DTO: `DataSynchronizationCreateDto`
  - Response DTO: `DataSynchronizationDto`
  - Used In Frontend: Yes

- **Endpoint:** `ProcessPendingSynchronizations`
  - Route: `/api/ExternalSystems/synchronizations/process`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `int`
  - Used In Frontend: Yes

### Controller: HealthController
- **Endpoint:** `Get`
  - Route: `/api/Health`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `HealthStatusDto`
  - Used In Frontend: Yes

- **Endpoint:** `Ready`
  - Route: `/api/Health/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        return Ok(new { Status = "Alive", Timestamp = DateTime.UtcNow });
    }

    // <summary>
    // Detailed health check with readiness information.
    // Useful for Kubernetes liveness/readiness probes.
    // </summary>
    // <returns>Detailed health information.</returns>
    // <response code="200">System is ready to accept traffic.</response>
    // <response code="503">System is not ready.</response>
    [HttpGet("ready`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ReadinessResponse`
  - Used In Frontend: Yes

### Controller: MachineConfigurationController
- **Endpoint:** `GetAllConfigurations`
  - Route: `/api/MachineConfiguration`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<MachineConfiguration>`
  - Used In Frontend: Yes

- **Endpoint:** `GetConfiguration`
  - Route: `/api/MachineConfiguration/{machineType}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `MachineConfiguration`
  - Used In Frontend: Yes

### Controller: MachinesController
- **Endpoint:** `GetMachines`
  - Route: `/api/Machines`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<MachineDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetMachine`
  - Route: `/api/Machines/{id:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `MachineDto`
  - Used In Frontend: Yes

- **Endpoint:** `Create`
  - Route: `/api/Machines`
  - Method: `HttpPost`
  - Request DTO: `MachineCreateDto`
  - Response DTO: `MachineDto`
  - Used In Frontend: Yes

- **Endpoint:** `Update`
  - Route: `/api/Machines/{id:guid}`
  - Method: `HttpPut`
  - Request DTO: `MachineUpdateDto`
  - Response DTO: `MachineDto`
  - Used In Frontend: Yes

### Controller: MaintenanceController
- **Endpoint:** `Plan`
  - Route: `/api/Maintenance/plan`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `MaintenanceRecord`
  - Used In Frontend: Yes

- **Endpoint:** `Start`
  - Route: `/api/Maintenance/{id:guid}/start`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `MaintenanceRecord`
  - Used In Frontend: Yes

- **Endpoint:** `Complete`
  - Route: `/api/Maintenance/{id:guid}/complete`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `MaintenanceRecord`
  - Used In Frontend: Yes

- **Endpoint:** `Cancel`
  - Route: `/api/Maintenance/{id:guid}/cancel`
  - Method: `HttpPost`
  - Request DTO: `string`
  - Response DTO: `MaintenanceRecord`
  - Used In Frontend: Yes

- **Endpoint:** `GetHistory`
  - Route: `/api/Maintenance/machine/{machineId:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<MaintenanceRecord>`
  - Used In Frontend: Yes

- **Endpoint:** `GetActive`
  - Route: `/api/Maintenance/active`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<MaintenanceRecord>`
  - Used In Frontend: Yes

- **Endpoint:** `Search`
  - Route: `/api/Maintenance/search`
  - Method: `HttpGet`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<MaintenanceRecord>`
  - Used In Frontend: Yes

### Controller: MathematicalModelingController
- **Endpoint:** `SolveOde`
  - Route: `/api/MathematicalModeling/ode/solve`
  - Method: `HttpPost`
  - Request DTO: `OdeSolveRequest`
  - Response DTO: `OdeSolutionDto`
  - Used In Frontend: Yes

- **Endpoint:** `SolveSystemDynamics`
  - Route: `/api/MathematicalModeling/system-dynamics/solve`
  - Method: `HttpPost`
  - Request DTO: `SystemDynamicsRequest`
  - Response DTO: `SystemDynamicsSolutionDto`
  - Used In Frontend: Yes

- **Endpoint:** `GradientOptimization`
  - Route: `/api/MathematicalModeling/optimization/gradient`
  - Method: `HttpPost`
  - Request DTO: `GradientOptimizationRequest`
  - Response DTO: `OptimizationResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `GeneticOptimization`
  - Route: `/api/MathematicalModeling/optimization/genetic`
  - Method: `HttpPost`
  - Request DTO: `GeneticOptimizationRequest`
  - Response DTO: `OptimizationResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `MultiObjectiveOptimization`
  - Route: `/api/MathematicalModeling/optimization/multi-objective`
  - Method: `HttpPost`
  - Request DTO: `MultiObjectiveOptimizationRequest`
  - Response DTO: `MultiObjectiveResultDto`
  - Used In Frontend: Yes

### Controller: ModelLifecycleController
- **Endpoint:** `RegisterModelVersion`
  - Route: `/api/ModelLifecycle/register`
  - Method: `HttpPost`
  - Request DTO: `RegisterModelVersionRequest`
  - Response DTO: `ModelVersionDto`
  - Used In Frontend: Yes

- **Endpoint:** `PromoteModel`
  - Route: `/api/ModelLifecycle/{id}/promote`
  - Method: `HttpPost`
  - Request DTO: `PromoteModelRequest`
  - Response DTO: `ModelVersionDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetModelVersions`
  - Route: `/api/ModelLifecycle`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<ModelVersionDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetProductionVersion`
  - Route: `/api/ModelLifecycle/production/{modelType}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ModelVersionDto`
  - Used In Frontend: Yes

- **Endpoint:** `CompareModels`
  - Route: `/api/ModelLifecycle/compare`
  - Method: `HttpPost`
  - Request DTO: `CompareModelsRequest`
  - Response DTO: `ModelComparisonResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetModelPerformance`
  - Route: `/api/ModelLifecycle/{id}/performance`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ModelPerformanceSummary`
  - Used In Frontend: Yes

### Controller: PerformanceMetricsController
- **Endpoint:** `GetMetrics`
  - Route: `/api/PerformanceMetrics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<PerformanceMetrics>`
  - Used In Frontend: Yes

- **Endpoint:** `GetStatistics`
  - Route: `/api/PerformanceMetrics/statistics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `PerformanceStatistics`
  - Used In Frontend: Yes

- **Endpoint:** `GetThresholdViolations`
  - Route: `/api/PerformanceMetrics/threshold-violations`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<PerformanceMetrics>`
  - Used In Frontend: Yes

### Controller: PredictionsController
- **Endpoint:** `GetRulPrediction`
  - Route: `/api/Predictions/rul/{machineId}`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `RulPredictionResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetHealthClassification`
  - Route: `/api/Predictions/health/{machineId}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `HealthClassificationResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetRulSummary`
  - Route: `/api/Predictions/rul/{machineId}/summary`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `object`
  - Used In Frontend: Yes

- **Endpoint:** `TrainModels`
  - Route: `/api/Predictions/train`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `TrainingResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `TrainModelsAiEndpoint`
  - Route: `/api/Predictions/ai/train`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `TrainingResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetPredictions`
  - Route: `/api/Predictions/status")]
    [ProducesResponseType(typeof(ModelStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ModelStatusDto> GetModelStatus()
    {
        return Ok(new ModelStatusDto
        {
            RulModelLoaded = rulPredictor.IsModelLoaded,
            HealthModelLoaded = healthClassifier.IsModelLoaded,
            ModelVersion = "1.0.0",
            LastUpdated = DateTime.UtcNow
        });
    }

    // <summary>
    // Requests a new prediction for a machine.
    // </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PredictionDto>> RequestPrediction([FromBody] PredictionRequestDto request, CancellationToken ct)
    {
        var result = await predictionService.PredictAsync(request, ct);
        return Ok(result);
    }

    private async Task<List<TelemetryData>> GetTelemetryForMachine(Guid machineId, CancellationToken ct)
    {
        var telemetry = await telemetryRepository.GetRecentAsync(
            machineId: machineId,
            limit: 100,
            ct: ct);

        return telemetry.ToList();
    }

    // <summary>
    // Gets list of predictions for all machines or a specific machine.
    // </summary>
    [HttpGet]
    [HttpGet("{machineId:guid}`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<PredictionDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetRulPredictionGet`
  - Route: `/api/Predictions/rul/{machineId}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `RulPredictionResult`
  - Used In Frontend: Yes

- **Endpoint:** `GetAnomalyPrediction`
  - Route: `/api/Predictions/anomaly/{machineId}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `DigitalTwinPlatform.API.Services.Analytics.Advanced.AnomalyDetectionResult`
  - Used In Frontend: Yes

- **Endpoint:** `Search`
  - Route: `/api/Predictions/search`
  - Method: `HttpGet`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<RulPredictionResult>`
  - Used In Frontend: Yes

- **Endpoint:** `RequestPrediction`
  - Route: `/api/Predictions`
  - Method: `HttpPost`
  - Request DTO: `PredictionRequestDto`
  - Response DTO: `PredictionDto`
  - Used In Frontend: Yes

### Controller: PrescriptiveController
- **Endpoint:** `GetAnalysis`
  - Route: `/api/Prescriptive/{machineId}/analysis`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<MaintenanceWindow>`
  - Used In Frontend: Yes

- **Endpoint:** `GetOptimal`
  - Route: `/api/Prescriptive/{machineId}/optimal`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `MaintenanceWindow`
  - Used In Frontend: Yes

### Controller: ProductionLinesController
- **Endpoint:** `Get`
  - Route: `/api/ProductionLines`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<ProductionLineDto>`
  - Used In Frontend: Yes

- **Endpoint:** `Get`
  - Route: `/api/ProductionLines/{id:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ProductionLineDto`
  - Used In Frontend: Yes

- **Endpoint:** `Create`
  - Route: `/api/ProductionLines`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `ProductionLineDto`
  - Used In Frontend: Yes

- **Endpoint:** `Update`
  - Route: `/api/ProductionLines/{id:guid}`
  - Method: `HttpPut`
  - Request DTO: `None`
  - Response DTO: `ProductionLineDto`
  - Used In Frontend: Yes

- **Endpoint:** `Search`
  - Route: `/api/ProductionLines/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductionLineCommand(id), ct);
        return NoContent();
    }

    [HttpGet("search`
  - Method: `HttpDelete`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<ProductionLineDto>`
  - Used In Frontend: Yes

### Controller: ReportsController
### Controller: RunToFailureController
- **Endpoint:** `RunSimulation`
  - Route: `/api/RunToFailure/{machineId:guid}`
  - Method: `HttpPost`
  - Request DTO: `None`
  - Response DTO: `RunToFailureResult`
  - Used In Frontend: Yes

- **Endpoint:** `GenerateTrajectories`
  - Route: `/api/RunToFailure/generate-trajectories`
  - Method: `HttpPost`
  - Request DTO: `GenerateTrajectoriesRequest`
  - Response DTO: `List<DegradationTrajectory>`
  - Used In Frontend: Yes

- **Endpoint:** `GetResults`
  - Route: `/api/RunToFailure/{machineId:guid}/results`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<RunToFailureResult>`
  - Used In Frontend: Yes

### Controller: SearchController
- **Endpoint:** `Search`
  - Route: `/api/Search`
  - Method: `HttpPost`
  - Request DTO: `SearchRequestDto`
  - Response DTO: `SearchResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetSuggestions`
  - Route: `/api/Search/suggestions`
  - Method: `HttpGet`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<SearchSuggestionDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetSavedSearches`
  - Route: `/api/Search/saved`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<SavedSearchDto>`
  - Used In Frontend: Yes

- **Endpoint:** `SaveSearch`
  - Route: `/api/Search/saved`
  - Method: `HttpPost`
  - Request DTO: `SaveSearchRequestDto`
  - Response DTO: `SavedSearchDto`
  - Used In Frontend: Yes

### Controller: SimulationController
### Controller: SyntheticDataController
- **Endpoint:** `GenerateSyntheticData`
  - Route: `/api/SyntheticData/generate`
  - Method: `HttpPost`
  - Request DTO: `SyntheticDataGenerationRequest`
  - Response DTO: `DigitalTwinPlatform.Domain.Entities.SyntheticDataGeneration`
  - Used In Frontend: Yes

- **Endpoint:** `ValidateSyntheticData`
  - Route: `/api/SyntheticData/validate`
  - Method: `HttpPost`
  - Request DTO: `ValidateSyntheticDataRequest`
  - Response DTO: `DigitalTwinPlatform.Domain.Entities.DataValidationReport`
  - Used In Frontend: Yes

- **Endpoint:** `GetGenerationStatistics`
  - Route: `/api/SyntheticData/statistics/{machineType}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `DigitalTwinPlatform.Domain.Entities.GenerationStatistics`
  - Used In Frontend: Yes

- **Endpoint:** `GetAllGenerationStatistics`
  - Route: `/api/SyntheticData/statistics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<DigitalTwinPlatform.Domain.Entities.GenerationStatistics>`
  - Used In Frontend: Yes

### Controller: TelemetryController
- **Endpoint:** `GetForMachine`
  - Route: `/api/Telemetry/{machineId:guid}`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<TelemetryDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetRecent`
  - Route: `/api/Telemetry/recent`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<TelemetryDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetLatest`
  - Route: `/api/Telemetry/{machineId:guid}/latest`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `TelemetryDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetLatestMetrics`
  - Route: `/api/Telemetry/{machineId:guid}/latest/metrics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `TelemetryMetricsDto`
  - Used In Frontend: Yes

- **Endpoint:** `Search`
  - Route: `/api/Telemetry/search`
  - Method: `HttpGet`
  - Request DTO: `Query:string`
  - Response DTO: `IEnumerable<TelemetryDto>`
  - Used In Frontend: Yes

### Controller: TenantsController
- **Endpoint:** `GetAllTenants`
  - Route: `/api/Tenants`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<TenantDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetActiveTenants`
  - Route: `/api/Tenants/active`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `List<TenantDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetTenantById`
  - Route: `/api/Tenants/{id:guid}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `TenantDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetTenantBySlug`
  - Route: `/api/Tenants/slug/{slug}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `TenantDto`
  - Used In Frontend: Yes

- **Endpoint:** `CreateTenant`
  - Route: `/api/Tenants`
  - Method: `HttpPost`
  - Request DTO: `TenantCreateDto`
  - Response DTO: `TenantDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateTenant`
  - Route: `/api/Tenants/{id:guid}`
  - Method: `HttpPut`
  - Request DTO: `TenantUpdateDto`
  - Response DTO: `TenantDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetTenantUsers`
  - Route: `/api/Tenants/{id:guid}")]
    public async Task<ActionResult> DeleteTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantAsync(id, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / POST: api/tenants/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> ActivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.ActivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / POST: api/tenants/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeactivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / Tenant Users endpoints
    / GET: api/tenants/{tenantId}/users
    [HttpGet("{tenantId:guid}/users`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `List<TenantUserDto>`
  - Used In Frontend: Yes

- **Endpoint:** `AddUserToTenant`
  - Route: `/api/Tenants/{tenantId:guid}/users`
  - Method: `HttpPost`
  - Request DTO: `TenantUserCreateDto`
  - Response DTO: `TenantUserDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetTenantSettings`
  - Route: `/api/Tenants/{tenantId:guid}/users/{userId:guid}")]
    public async Task<ActionResult> RemoveUserFromTenant(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var result = await _tenantService.RemoveUserFromTenantAsync(tenantId, userId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / PUT: api/tenants/{tenantId}/users/{userId}/role
    [HttpPut("{tenantId:guid}/users/{userId:guid}/role")]
    public async Task<ActionResult> UpdateTenantUserRole(Guid tenantId, Guid userId, [FromBody] UserRole role, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateTenantUserRoleAsync(tenantId, userId, role, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / Tenant Settings endpoints
    / GET: api/tenants/{tenantId}/settings
    [HttpGet("{tenantId:guid}/settings`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `List<TenantSettingDto>`
  - Used In Frontend: Yes

- **Endpoint:** `CreateTenantSetting`
  - Route: `/api/Tenants/{tenantId:guid}/settings`
  - Method: `HttpPost`
  - Request DTO: `TenantSettingCreateDto`
  - Response DTO: `TenantSettingDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateTenantSetting`
  - Route: `/api/Tenants/{tenantId:guid}/settings/{settingId:guid}`
  - Method: `HttpPut`
  - Request DTO: `TenantSettingUpdateDto`
  - Response DTO: `TenantSettingDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetTenantSettingByKey`
  - Route: `/api/Tenants/{tenantId:guid}/settings/{settingId:guid}")]
    public async Task<ActionResult> DeleteTenantSetting(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantSettingAsync(tenantId, settingId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / GET: api/tenants/{tenantId}/settings/key/{key}
    [HttpGet("{tenantId:guid}/settings/key/{key}`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `TenantSettingDto`
  - Used In Frontend: Yes

### Controller: TokenController
### Controller: UncertaintyController
- **Endpoint:** `MonteCarloAnalysis`
  - Route: `/api/Uncertainty/{machineId}/monte-carlo`
  - Method: `HttpPost`
  - Request DTO: `MonteCarloRequest`
  - Response DTO: `UncertaintyAnalysisResponse`
  - Used In Frontend: Yes

- **Endpoint:** `BayesianAnalysis`
  - Route: `/api/Uncertainty/{machineId}/bayesian`
  - Method: `HttpPost`
  - Request DTO: `BayesianRequest`
  - Response DTO: `UncertaintyAnalysisResponse`
  - Used In Frontend: Yes

- **Endpoint:** `BootstrapIntervals`
  - Route: `/api/Uncertainty/{machineId}/bootstrap-intervals`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `ConfidenceIntervalDto`
  - Used In Frontend: Yes

- **Endpoint:** `ModelUncertainty`
  - Route: `/api/Uncertainty/{machineId}/model-uncertainty`
  - Method: `HttpPost`
  - Request DTO: `ModelUncertaintyRequest`
  - Response DTO: `ModelUncertaintyResponse`
  - Used In Frontend: Yes

- **Endpoint:** `ComprehensiveReport`
  - Route: `/api/Uncertainty/{machineId}/comprehensive-report`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `ComprehensiveUncertaintyReport`
  - Used In Frontend: Yes

### Controller: WorkflowController
- **Endpoint:** `GetAllWorkflows`
  - Route: `/api/Workflow`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<WorkflowDefinitionDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetWorkflow`
  - Route: `/api/Workflow/{id}`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `WorkflowDefinitionDto`
  - Used In Frontend: Yes

- **Endpoint:** `CreateWorkflow`
  - Route: `/api/Workflow`
  - Method: `HttpPost`
  - Request DTO: `CreateWorkflowCommand`
  - Response DTO: `WorkflowDefinitionDto`
  - Used In Frontend: Yes

- **Endpoint:** `UpdateWorkflow`
  - Route: `/api/Workflow/{id}`
  - Method: `HttpPut`
  - Request DTO: `UpdateWorkflowCommand`
  - Response DTO: `WorkflowDefinitionDto`
  - Used In Frontend: Yes

- **Endpoint:** `ToggleWorkflow`
  - Route: `/api/Workflow/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteWorkflow(string id)
        {
            try
            {
                var command = new DeleteWorkflowCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Enable/disable workflow
        // </summary>
        [HttpPost("{id}/toggle`
  - Method: `HttpDelete`
  - Request DTO: `None`
  - Response DTO: `WorkflowDefinitionDto`
  - Used In Frontend: Yes

- **Endpoint:** `ExecuteWorkflow`
  - Route: `/api/Workflow/{id}/execute`
  - Method: `HttpPost`
  - Request DTO: `ExecuteWorkflowDto`
  - Response DTO: `WorkflowExecutionDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetWorkflowExecutions`
  - Route: `/api/Workflow/{id}/executions`
  - Method: `HttpGet`
  - Request DTO: `Query:int`
  - Response DTO: `IEnumerable<WorkflowExecutionDto>`
  - Used In Frontend: Yes

- **Endpoint:** `GetWorkflowStatistics`
  - Route: `/api/Workflow/{id}/statistics`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `WorkflowStatisticsDto`
  - Used In Frontend: Yes

- **Endpoint:** `ValidateWorkflow`
  - Route: `/api/Workflow/validate`
  - Method: `HttpPost`
  - Request DTO: `ValidateWorkflowDto`
  - Response DTO: `WorkflowValidationResultDto`
  - Used In Frontend: Yes

- **Endpoint:** `GetWorkflowTemplates`
  - Route: `/api/Workflow/templates`
  - Method: `HttpGet`
  - Request DTO: `None`
  - Response DTO: `IEnumerable<WorkflowTemplateDto>`
  - Used In Frontend: Yes

- **Endpoint:** `DuplicateWorkflow`
  - Route: `/api/Workflow/{id}/duplicate`
  - Method: `HttpPost`
  - Request DTO: `DuplicateWorkflowDto`
  - Response DTO: `WorkflowDefinitionDto`
  - Used In Frontend: Yes

## 2. Frontend API Usage Map
### Service: advancedAnalytics
- **Method:** `generateMaintenanceRecommendation`
  - URL: `/api/AdvancedAnalytics/prescriptive/scheduling`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `optimizeProductionSchedule`
  - URL: `/api/AdvancedAnalytics/prescriptive/scheduling`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `optimizeResourceAllocation`
  - URL: `/api/AdvancedAnalytics/prescriptive/resource-allocation`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `optimizeMaintenanceCosts`
  - URL: `/api/AdvancedAnalytics/prescriptive/cost-optimization`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: aiModels
- **Method:** `fetchAllAIModels`
  - URL: `/api/AIModel`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getModelDeploymentStatus`
  - URL: `/api/AIModel/validate-model`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `validateModel`
  - URL: `/api/AIModel/validate-model`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: alerts
### Service: api
### Service: dashboard
- **Method:** `getStats`
  - URL: `/api/Dashboard/stats`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getMachineHealthSummary`
  - URL: `/api/Dashboard/machine-health`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getAlertsSummary`
  - URL: `/api/Dashboard/alerts`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getPerformanceSummary`
  - URL: `/api/Dashboard/performance`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getRecentActivity`
  - URL: `/api/Dashboard/recent-activity`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: dataArchival
- **Method:** `getPolicies`
  - URL: `/api/DataArchival/policies`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getPolicy`
  - URL: `/api/DataArchival/policies`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `createPolicy`
  - URL: `/api/DataArchival/policies`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `runArchival`
  - URL: `/api/DataArchival/stats`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getJobStatus`
  - URL: `/api/DataArchival/stats`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getStorageStats`
  - URL: `/api/DataArchival/stats`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `restore`
  - URL: `/api/DataArchival/restore`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: degradationModeling
- **Method:** `getAll`
  - URL: `/api/DegradationModeling`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getById`
  - URL: `/api/DegradationModeling`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `create`
  - URL: `/api/DegradationModeling`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: drift
- **Method:** `getCurrentDriftStatus`
  - URL: `/api/Drift/status`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getThresholds`
  - URL: `/api/Drift/detect`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `configureThresholds`
  - URL: `/api/Drift/detect`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `detectDrift`
  - URL: `/api/Drift/detect`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: externalSystems
- **Method:** `getAll`
  - URL: `/api/ExternalSystems`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getById`
  - URL: `/api/ExternalSystems`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `create`
  - URL: `/api/ExternalSystems`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: health
- **Method:** `getHealth`
  - URL: `/api/Health`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getMetrics`
  - URL: `/api/Health/metrics`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `isReady`
  - URL: `/api/Health/ready`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `isAlive`
  - URL: `/api/Health/live`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: index
### Service: machineConfiguration
- **Method:** `updateConfiguration`
  - URL: `/api/MachineConfiguration/templates`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `deleteConfiguration`
  - URL: `/api/MachineConfiguration/templates`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getTemplates`
  - URL: `/api/MachineConfiguration/templates`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `createTemplate`
  - URL: `/api/MachineConfiguration/templates`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: machines
- **Method:** `fetchMachines`
  - URL: `/api/Machines`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchMachine`
  - URL: `/api/Machines`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `createMachine`
  - URL: `/api/Machines`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: maintenance
- **Method:** `fetchMaintenanceHistory`
  - URL: `/api/Maintenance/active`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchActiveMaintenance`
  - URL: `/api/Maintenance/active`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchMaintenanceEvent`
  - URL: `/api/Maintenance/plan`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `planMaintenance`
  - URL: `/api/Maintenance/plan`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `updateMaintenance`
  - URL: `/api/Maintenance/stats`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `fetchMaintenanceStats`
  - URL: `/api/Maintenance/stats`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `fetchUpcomingMaintenance`
  - URL: `/api/Maintenance/overdue`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `fetchOverdueMaintenance`
  - URL: `/api/Maintenance/overdue`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: mathematicalModeling
- **Method:** `getAll`
  - URL: `/api/MathematicalModeling`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getById`
  - URL: `/api/MathematicalModeling`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `create`
  - URL: `/api/MathematicalModeling`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `validate`
  - URL: `/api/MathematicalModeling/optimize`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `optimize`
  - URL: `/api/MathematicalModeling/optimize`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: modelLifecycle
- **Method:** `getAll`
  - URL: `/api/ModelLifecycle`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getByModel`
  - URL: `/api/ModelLifecycle`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getByVersion`
  - URL: `/api/ModelLifecycle`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `create`
  - URL: `/api/ModelLifecycle`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `update`
  - URL: `/api/ModelLifecycle/transitions/available`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `transition`
  - URL: `/api/ModelLifecycle/transitions/available`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getAvailableTransitions`
  - URL: `/api/ModelLifecycle/transitions/available`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: performance
- **Method:** `getDashboard`
  - URL: `/api/BenchmarkValidation/validate`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `validate`
  - URL: `/api/BenchmarkValidation/validate`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getIndustryAverages`
  - URL: `/api/BenchmarkValidation/industry-averages`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: predictions
- **Method:** `getPredictionHistory`
  - URL: `/api/Predictions/train`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `retrainModels`
  - URL: `/api/Predictions/train`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getEnsemblePrediction`
  - URL: `/api/Predictions/models/status`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getModelStatus`
  - URL: `/api/Predictions/models/status`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: prescriptive
### Service: productionLines
- **Method:** `getAll`
  - URL: `/api/ProductionLines`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getById`
  - URL: `/api/ProductionLines`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `create`
  - URL: `/api/ProductionLines`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: runToFailure
- **Method:** `getAnalysis`
  - URL: `/api/RunToFailure/scenarios`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getScenarios`
  - URL: `/api/RunToFailure/scenarios`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `createScenario`
  - URL: `/api/RunToFailure/scenarios`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: search
- **Method:** `getSuggestions`
  - URL: `/api/Search/saved`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getSavedSearches`
  - URL: `/api/Search/saved`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `saveSearch`
  - URL: `/api/Search/saved`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: signalr
### Service: syntheticData
- **Method:** `generate`
  - URL: `/api/SyntheticData/generate`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getGenerationStatus`
  - URL: `/api/SyntheticData/profiles`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getProfiles`
  - URL: `/api/SyntheticData/profiles`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `getProfile`
  - URL: `/api/SyntheticData/profiles`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `createProfile`
  - URL: `/api/SyntheticData/profiles`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: telemetry
- **Method:** `fetchTelemetryHistory`
  - URL: `/api/Telemetry`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `ingestTelemetry`
  - URL: `/api/Telemetry`
  - Backend Match: No
  - DTO Compatible: N/A

- **Method:** `ingestTelemetryBatch`
  - URL: `/api/Telemetry/batch`
  - Backend Match: No
  - DTO Compatible: N/A

### Service: tenants
- **Method:** `getAll`
  - URL: `/api/Tenants`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getActive`
  - URL: `/api/Tenants/active`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getById`
  - URL: `/api/Tenants`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `getBySlug`
  - URL: `/api/Tenants`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `create`
  - URL: `/api/Tenants`
  - Backend Match: Yes
  - DTO Compatible: TBD

### Service: uncertainty
### Service: workflows
- **Method:** `fetchAllWorkflows`
  - URL: `/api/Workflow`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchWorkflowById`
  - URL: `/api/Workflow`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `createWorkflow`
  - URL: `/api/Workflow`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchWorkflowStatistics`
  - URL: `/api/Workflow/templates`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `fetchWorkflowTemplates`
  - URL: `/api/Workflow/templates`
  - Backend Match: Yes
  - DTO Compatible: TBD

- **Method:** `validateWorkflow`
  - URL: `/api/Workflow/validate`
  - Backend Match: Yes
  - DTO Compatible: TBD

## 3. Contract Mismatches
| DTO Name | Backend Field (Missing in FE) | Frontend Field (Extra in FE) | Issue |
| --- | --- | --- | --- |
| TelemetryDto |  | timestamp, data, machineId, dataType, id | Field Mismatch |
| ModelCompatibilityDto | IsCompatible | isCompatible, framework, requiredFeatures, compatibleMachineTypes | Field Mismatch |
| WorkflowConditionDto | Field | field, value, operator, dataType | Field Mismatch |
| TelemetryIngestDto |  | timestamp, dataType, machineId, data | Field Mismatch |
| ModelInputDto |  | features | Field Mismatch |
| SearchResultDto | Items | totalPages, pageSize, page, totalCount, executionTimeMs, items | Field Mismatch |
| WorkflowExecutionDto | Id | workflowId, duration, id, actionExecutions, workflowName, inputContext, errorMessage, status, triggeredBy, completedAt, startedAt | Field Mismatch |
| ModelValidationResultDto | IsValid | isValid, errors, message, compatibility | Field Mismatch |
| ModelPredictionDto | RemainingUsefulLife | featureImportance, predictionTime, confidence, remainingUsefulLife, riskLevel | Field Mismatch |
| WorkflowTemplateDto | Id | usageCount, name, tags, createdAt, id, category, createdBy, definition, description | Field Mismatch |
| SaveSearchRequestDto | Name | entityTypes, name, query | Field Mismatch |
| WorkflowValidationResultDto | IsValid | errors, message, isValid, warnings, compatibility | Field Mismatch |
| SearchRequestDto | Query | advancedFilters, dateRange, entityTypes, query, pageSize, statusFilter, page, sortBy | Field Mismatch |
| SavedSearchDto | Id | entityTypes, name, query, createdAt, id, lastUsed | Field Mismatch |
| AlertDto |  | message, resolvedAt, recommendedAction, suggestedActions, relatedPredictionId, acknowledgedAt, severity, isAcknowledged, createdAt, id, acknowledgedBy, category, machineId, status, title, description | Field Mismatch |
| SearchResultItemDto | Id | createdAt, id, type, subtitle, status, title, matchScore, metadata | Field Mismatch |
| WorkflowActionDto | Type | order, type, enabled, configuration | Field Mismatch |
| WorkflowStatisticsDto | WorkflowId | totalExecutions, successRate, workflowId, firstExecution, lastExecution, successfulExecutions, performanceMetrics, failedExecutions, averageDuration, executionsByDay | Field Mismatch |
| AIModelDto | Id | name, features, lastTraining, deployedMachines, f1Score, trainingDataSize, tags, createdAt, id, recall, updatedAt, status, modelType, accuracy, precision, description, version, algorithm | Field Mismatch |
| DeploymentStatusDto | Status | lastUpdated, failedDeployments, status, totalMachines, successfulDeployments, machineStatuses | Field Mismatch |
| WorkflowTargetDto | Type | machineTypes, machineIds, filters, statuses, locations, type | Field Mismatch |
| WorkflowActionExecutionDto | ActionOrder | output, actionOrder, input, actionType, errorMessage, status, completedAt, startedAt | Field Mismatch |
| WorkflowCompatibilityDto | IsCompatible | isCompatible, compatibleTriggers, requiredPermissions, compatibleActions | Field Mismatch |
| AdvancedFiltersDto | MinTemperature | model, minPressure, maxTemperature, minTemperature, location, manufacturer, maxPressure | Field Mismatch |
| ModelMetricsDto | Accuracy | f1Score, lastEvaluated, rootMeanSquareError, recall, accuracy, precision, classMetrics, meanAbsoluteError | Field Mismatch |
| WorkflowDefinitionDto | Id | name, actions, enabled, type, tags, createdAt, id, lastRun, target, createdBy, updatedAt, status, executionCount, trigger, description, nextRun | Field Mismatch |
| MachineDeploymentStatus | MachineId | machineId, errorMessage, status, deployedAt | Field Mismatch |
| SearchSuggestionDto | Text | entityType, score, text | Field Mismatch |
| WorkflowTriggerDto | Type | configuration, condition, cronExpression, type, eventType | Field Mismatch |

## 4. Implementation Integrity
### Unimplemented Interface Methods
- **Interface:** `IMlExperimentLogger`
  - Implementation: `NONE`
  - Missing Methods: LogExperimentAsync

- **Interface:** `IAlertRepository`
  - Implementation: `NONE`
  - Missing Methods: SearchAsync

- **Interface:** `IMachineRepository`
  - Implementation: `NONE`
  - Missing Methods: GetByProductionLineAsync, SearchAsync

- **Interface:** `IMaintenanceRepository`
  - Implementation: `NONE`
  - Missing Methods: SearchAsync

- **Interface:** `IModelVersionRepository`
  - Implementation: `NONE`
  - Missing Methods: GetAsync, GetByVersionAsync, GetProductionVersionAsync, GetByModelTypeAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync

- **Interface:** `IPredictionRepository`
  - Implementation: `NONE`
  - Missing Methods: GetByMachineIdAsync, SearchAsync

- **Interface:** `IProductionLineRepository`
  - Implementation: `NONE`
  - Missing Methods: SearchAsync

- **Interface:** `IRepository`
  - Implementation: `NONE`
  - Missing Methods: GetAsync, GetAllAsync, AddAsync, AddRangeAsync, UpdateAsync, DeleteAsync, DeleteRangeAsync, SaveChangesAsync

- **Interface:** `ISavedSearchRepository`
  - Implementation: `NONE`
  - Missing Methods: GetByUserAsync

- **Interface:** `ITelemetryRepository`
  - Implementation: `NONE`
  - Missing Methods: GetForMachineAsync, GetRecentAsync, SearchAsync

- **Interface:** `IHubPublisher`
  - Implementation: `NONE`
  - Missing Methods: BroadcastTelemetryAsync, BroadcastPredictionAsync, BroadcastAlertAsync

- **Interface:** `IMaintenanceService`
  - Implementation: `NONE`
  - Missing Methods: PlanMaintenanceAsync, StartMaintenanceAsync, CompleteMaintenanceAsync, CancelMaintenanceAsync, GetMaintenanceHistoryAsync, GetActiveMaintenanceAsync, SearchMaintenanceAsync

- **Interface:** `IPredictionPublisher`
  - Implementation: `NONE`
  - Missing Methods: BroadcastPredictionAsync

- **Interface:** `ITelemetryPublisher`
  - Implementation: `NONE`
  - Missing Methods: BroadcastTelemetryAsync

- **Interface:** `ITwinEngineService`
  - Implementation: `NONE`
  - Missing Methods: UpdateTwinPredictionAsync, GetMachineWithStateAsync

- **Interface:** `IUnitOfWork`
  - Implementation: `NONE`
  - Missing Methods: SaveChangesAsync, BeginTransactionAsync, CommitAsync, RollbackAsync

- **Interface:** `IPrescriptiveService`
  - Implementation: `NONE`
  - Missing Methods: RunWhatIfAnalysisAsync, GetOptimalMaintenanceDateAsync

- **Interface:** `IMachineConfigurationService`
  - Implementation: `MachineConfigurationService`
  - Missing Methods: ValidateConfigurationAsync, ConfigurationExistsAsync

- **Interface:** `IMLModelService`
  - Implementation: `MLModelService`
  - Missing Methods: GetFeatureImportanceAsync

### Dead Endpoints (Not used by Frontend)
- `POST /api/AdvancedAnalytics/predictions/{machineId}/ensemble` (AdvancedAnalyticsController)
- `POST /api/AdvancedAnalytics/predictions/{machineId}/deep-learning` (AdvancedAnalyticsController)
- `POST /api/AdvancedAnalytics/anomaly-detection/{machineId}` (AdvancedAnalyticsController)
- `POST /api/AdvancedAnalytics/forecasting/{machineId}/{metric}` (AdvancedAnalyticsController)
- `POST /api/AdvancedAnalytics/prescriptive/maintenance/{machineId}` (AdvancedAnalyticsController)
- `GET /api/AdvancedAnalytics/dashboard/{machineId}` (AdvancedAnalyticsController)
- `GET /api/AIModel/{id}` (AIModelController)
- `POST /api/AIModel` (AIModelController)
- `PUT /api/AIModel/{id}` (AIModelController)
- `DELETE /api/AIModel/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteModel(string id)
        {
            try
            {
                var command = new DeleteAIModelCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting AI model {ModelId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Test model prediction
        // </summary>
        [HttpPost("{id}/predict` (AIModelController)
- `GET /api/AIModel/{id}/metrics` (AIModelController)
- `POST /api/AIModel/{id}/retrain` (AIModelController)
- `GET /api/AIModel/{id}/compatible-machines` (AIModelController)
- `POST /api/AIModel/{id}/deploy-to-machines")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> DeployToMachines(string id, [FromBody] DeployToMachinesDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _aiService.DeployModelToMachines(id, request.MachineIds);
                return Ok(new { Message = "Model deployed to specified machines successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { Message = $"Model with ID {id} not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deploying model {ModelId} to machines", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Get model deployment status
        // </summary>
        [HttpGet("{id}/deployment-status` (AIModelController)
- `GET /api/Alerts` (AlertsController)
- `GET /api/Alerts/{id:guid}` (AlertsController)
- `PUT /api/Alerts/{id:guid}/acknowledge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Acknowledge(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await alertService.AcknowledgeAlertAsync(id, userId);
        return NoContent();
    }

    // <summary>
    // Resolves (deletes) an alert from the system.
    // Used when alerts have been addressed and are no longer relevant.
    // </summary>
    // <param name="id">The unique identifier of the alert to resolve.</param>
    // <returns>No content on successful resolution.</returns>
    // <response code="204">Alert resolved successfully.</response>
    // <response code="401">Unauthorized - Authentication required.</response>
    // <response code="404">Alert not found.</response>
    // <response code="500">Internal server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await alertService.ResolveAlertAsync(id);
        return NoContent();
    }

    // <summary>
    // Retrieves all alerts including acknowledged ones for a machine.
    // </summary>
    // <param name="machineId">Optional machine ID filter.</param>
    // <returns>List of all alerts for the specified machine.</returns>
    // <response code="200">Returns list of all alerts successfully.</response>
    // <response code="401">Unauthorized - Authentication required.</response>
    // <response code="500">Internal server error.</response>
    [HttpGet("all` (AlertsController)
- `GET /api/Alerts/stats` (AlertsController)
- `GET /api/Alerts/search` (AlertsController)
- `GET /api/BenchmarkValidation/benchmarks` (BenchmarkValidationController)
- `GET /api/BenchmarkValidation/benchmarks/{datasetName}` (BenchmarkValidationController)
- `POST /api/DegradationModeling/solve/exponential` (DegradationModelingController)
- `POST /api/DegradationModeling/solve/powerlaw` (DegradationModelingController)
- `POST /api/DegradationModeling/solve/multivariable` (DegradationModelingController)
- `POST /api/DegradationModeling/solve/stochastic` (DegradationModelingController)
- `POST /api/DegradationModeling/estimate/exponential` (DegradationModelingController)
- `POST /api/DegradationModeling/estimate/powerlaw` (DegradationModelingController)
- `POST /api/DegradationModeling/estimate/multivariable` (DegradationModelingController)
- `POST /api/DegradationModeling/compare` (DegradationModelingController)
- `POST /api/DegradationModeling/validate` (DegradationModelingController)
- `GET /api/Drift/history/{modelName}` (DriftController)
- `GET /api/Drift/thresholds/{modelName}` (DriftController)
- `PUT /api/Drift/thresholds/{modelName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConfigureThresholds(
        string modelName,
        [FromBody] DriftThresholds thresholds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            / Validate thresholds
            if (thresholds.FeatureDriftThreshold <= 0 || thresholds.FeatureDriftThreshold > 1)
            {
                return BadRequest(new { error = "Feature drift threshold must be between 0 and 1" });
            }

            if (thresholds.PredictionDriftThreshold <= 0 || thresholds.PredictionDriftThreshold > 1)
            {
                return BadRequest(new { error = "Prediction drift threshold must be between 0 and 1" });
            }

            await driftService.ConfigureThresholdsAsync(modelName, thresholds, cancellationToken);
            
            return Ok(new { message = $"Thresholds configured successfully for model '{modelName}'" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to configure thresholds", details = ex.Message });
        }
    }

    // <summary>
    // Generates drift report for specified period
    // </summary>
    // <param name="modelName">Model identifier (optional - if null, all models)</param>
    // <param name="days">Period in days (default: 7)</param>
    // <param name="cancellationToken">Cancellation token</param>
    // <returns>Drift report</returns>
    [HttpGet("report` (DriftController)
- `GET /api/ExternalSystems/connected` (ExternalSystemsController)
- `GET /api/ExternalSystems/type/{systemType}` (ExternalSystemsController)
- `GET /api/ExternalSystems/{id:guid}` (ExternalSystemsController)
- `GET /api/ExternalSystems/{id:guid}/status` (ExternalSystemsController)
- `PUT /api/ExternalSystems/{id:guid}` (ExternalSystemsController)
- `DELETE /api/ExternalSystems/{id:guid}")]
public async Task<ActionResult> DeleteExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/connect
[HttpPost("{id:guid}/connect")]
public async Task<ActionResult> ConnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.ConnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/disconnect
[HttpPost("{id:guid}/disconnect")]
public async Task<ActionResult> DisconnectExternalSystem(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisconnectExternalSystemAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/{id}/test
[HttpPost("{id:guid}/test` (ExternalSystemsController)
- `GET /api/ExternalSystems/integrations` (ExternalSystemsController)
- `GET /api/ExternalSystems/{systemId:guid}/integrations` (ExternalSystemsController)
- `GET /api/ExternalSystems/integrations/active` (ExternalSystemsController)
- `POST /api/ExternalSystems/integrations` (ExternalSystemsController)
- `PUT /api/ExternalSystems/integrations/{id:guid}` (ExternalSystemsController)
- `DELETE /api/ExternalSystems/integrations/{id:guid}")]
public async Task<ActionResult> DeleteSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DeleteSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => NoContent(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/integrations/{id}/enable
[HttpPost("integrations/{id:guid}/enable")]
public async Task<ActionResult> EnableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.EnableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ POST: api/externalsystems/integrations/{id}/disable
[HttpPost("integrations/{id:guid}/disable")]
public async Task<ActionResult> DisableSystemIntegration(Guid id, CancellationToken ct = default)
{
    var result = await externalSystemService.DisableSystemIntegrationAsync(id, ct);
    return result switch
    {
        Result.Success => Ok(),
        Result.Failure f => NotFound(f.Error),
        _ => throw new InvalidOperationException()
    };
}

/ Data Synchronization endpoints
/ GET: api/externalsystems/synchronizations
[HttpGet("synchronizations` (ExternalSystemsController)
- `GET /api/ExternalSystems/synchronizations/pending` (ExternalSystemsController)
- `GET /api/ExternalSystems/synchronizations/failed` (ExternalSystemsController)
- `GET /api/ExternalSystems/synchronizations/recent` (ExternalSystemsController)
- `POST /api/ExternalSystems/synchronizations` (ExternalSystemsController)
- `POST /api/ExternalSystems/synchronizations/process` (ExternalSystemsController)
- `GET /api/MachineConfiguration` (MachineConfigurationController)
- `GET /api/MachineConfiguration/{machineType}` (MachineConfigurationController)
- `GET /api/Machines/{id:guid}` (MachinesController)
- `PUT /api/Machines/{id:guid}` (MachinesController)
- `POST /api/Maintenance/{id:guid}/start` (MaintenanceController)
- `POST /api/Maintenance/{id:guid}/complete` (MaintenanceController)
- `POST /api/Maintenance/{id:guid}/cancel` (MaintenanceController)
- `GET /api/Maintenance/machine/{machineId:guid}` (MaintenanceController)
- `GET /api/Maintenance/search` (MaintenanceController)
- `POST /api/MathematicalModeling/ode/solve` (MathematicalModelingController)
- `POST /api/MathematicalModeling/system-dynamics/solve` (MathematicalModelingController)
- `POST /api/MathematicalModeling/optimization/gradient` (MathematicalModelingController)
- `POST /api/MathematicalModeling/optimization/genetic` (MathematicalModelingController)
- `POST /api/MathematicalModeling/optimization/multi-objective` (MathematicalModelingController)
- `POST /api/ModelLifecycle/register` (ModelLifecycleController)
- `POST /api/ModelLifecycle/{id}/promote` (ModelLifecycleController)
- `GET /api/ModelLifecycle/production/{modelType}` (ModelLifecycleController)
- `POST /api/ModelLifecycle/compare` (ModelLifecycleController)
- `GET /api/ModelLifecycle/{id}/performance` (ModelLifecycleController)
- `GET /api/PerformanceMetrics` (PerformanceMetricsController)
- `GET /api/PerformanceMetrics/statistics` (PerformanceMetricsController)
- `GET /api/PerformanceMetrics/threshold-violations` (PerformanceMetricsController)
- `POST /api/Predictions/rul/{machineId}` (PredictionsController)
- `GET /api/Predictions/health/{machineId}` (PredictionsController)
- `GET /api/Predictions/rul/{machineId}/summary` (PredictionsController)
- `POST /api/Predictions/ai/train` (PredictionsController)
- `GET /api/Predictions/status")]
    [ProducesResponseType(typeof(ModelStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ModelStatusDto> GetModelStatus()
    {
        return Ok(new ModelStatusDto
        {
            RulModelLoaded = rulPredictor.IsModelLoaded,
            HealthModelLoaded = healthClassifier.IsModelLoaded,
            ModelVersion = "1.0.0",
            LastUpdated = DateTime.UtcNow
        });
    }

    // <summary>
    // Requests a new prediction for a machine.
    // </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PredictionDto>> RequestPrediction([FromBody] PredictionRequestDto request, CancellationToken ct)
    {
        var result = await predictionService.PredictAsync(request, ct);
        return Ok(result);
    }

    private async Task<List<TelemetryData>> GetTelemetryForMachine(Guid machineId, CancellationToken ct)
    {
        var telemetry = await telemetryRepository.GetRecentAsync(
            machineId: machineId,
            limit: 100,
            ct: ct);

        return telemetry.ToList();
    }

    // <summary>
    // Gets list of predictions for all machines or a specific machine.
    // </summary>
    [HttpGet]
    [HttpGet("{machineId:guid}` (PredictionsController)
- `GET /api/Predictions/rul/{machineId}` (PredictionsController)
- `GET /api/Predictions/anomaly/{machineId}` (PredictionsController)
- `GET /api/Predictions/search` (PredictionsController)
- `POST /api/Predictions` (PredictionsController)
- `GET /api/Prescriptive/{machineId}/analysis` (PrescriptiveController)
- `GET /api/Prescriptive/{machineId}/optimal` (PrescriptiveController)
- `GET /api/ProductionLines/{id:guid}` (ProductionLinesController)
- `PUT /api/ProductionLines/{id:guid}` (ProductionLinesController)
- `DELETE /api/ProductionLines/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductionLineCommand(id), ct);
        return NoContent();
    }

    [HttpGet("search` (ProductionLinesController)
- `POST /api/RunToFailure/{machineId:guid}` (RunToFailureController)
- `POST /api/RunToFailure/generate-trajectories` (RunToFailureController)
- `GET /api/RunToFailure/{machineId:guid}/results` (RunToFailureController)
- `POST /api/Search` (SearchController)
- `GET /api/Search/suggestions` (SearchController)
- `POST /api/SyntheticData/validate` (SyntheticDataController)
- `GET /api/SyntheticData/statistics/{machineType}` (SyntheticDataController)
- `GET /api/SyntheticData/statistics` (SyntheticDataController)
- `GET /api/Telemetry/{machineId:guid}` (TelemetryController)
- `GET /api/Telemetry/recent` (TelemetryController)
- `GET /api/Telemetry/{machineId:guid}/latest` (TelemetryController)
- `GET /api/Telemetry/{machineId:guid}/latest/metrics` (TelemetryController)
- `GET /api/Telemetry/search` (TelemetryController)
- `GET /api/Tenants/{id:guid}` (TenantsController)
- `GET /api/Tenants/slug/{slug}` (TenantsController)
- `PUT /api/Tenants/{id:guid}` (TenantsController)
- `DELETE /api/Tenants/{id:guid}")]
    public async Task<ActionResult> DeleteTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantAsync(id, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / POST: api/tenants/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> ActivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.ActivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / POST: api/tenants/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateTenant(Guid id, CancellationToken ct = default)
    {
        var result = await _tenantService.DeactivateTenantAsync(id, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / Tenant Users endpoints
    / GET: api/tenants/{tenantId}/users
    [HttpGet("{tenantId:guid}/users` (TenantsController)
- `POST /api/Tenants/{tenantId:guid}/users` (TenantsController)
- `DELETE /api/Tenants/{tenantId:guid}/users/{userId:guid}")]
    public async Task<ActionResult> RemoveUserFromTenant(Guid tenantId, Guid userId, CancellationToken ct = default)
    {
        var result = await _tenantService.RemoveUserFromTenantAsync(tenantId, userId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / PUT: api/tenants/{tenantId}/users/{userId}/role
    [HttpPut("{tenantId:guid}/users/{userId:guid}/role")]
    public async Task<ActionResult> UpdateTenantUserRole(Guid tenantId, Guid userId, [FromBody] UserRole role, CancellationToken ct = default)
    {
        var result = await _tenantService.UpdateTenantUserRoleAsync(tenantId, userId, role, ct);
        return result switch
        {
            Result.Success => Ok(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / Tenant Settings endpoints
    / GET: api/tenants/{tenantId}/settings
    [HttpGet("{tenantId:guid}/settings` (TenantsController)
- `POST /api/Tenants/{tenantId:guid}/settings` (TenantsController)
- `PUT /api/Tenants/{tenantId:guid}/settings/{settingId:guid}` (TenantsController)
- `DELETE /api/Tenants/{tenantId:guid}/settings/{settingId:guid}")]
    public async Task<ActionResult> DeleteTenantSetting(Guid tenantId, Guid settingId, CancellationToken ct = default)
    {
        var result = await _tenantService.DeleteTenantSettingAsync(tenantId, settingId, ct);
        return result switch
        {
            Result.Success => NoContent(),
            Result.Failure f => NotFound(f.Error),
            _ => throw new InvalidOperationException()
        };
    }

    / GET: api/tenants/{tenantId}/settings/key/{key}
    [HttpGet("{tenantId:guid}/settings/key/{key}` (TenantsController)
- `POST /api/Uncertainty/{machineId}/monte-carlo` (UncertaintyController)
- `POST /api/Uncertainty/{machineId}/bayesian` (UncertaintyController)
- `GET /api/Uncertainty/{machineId}/bootstrap-intervals` (UncertaintyController)
- `POST /api/Uncertainty/{machineId}/model-uncertainty` (UncertaintyController)
- `GET /api/Uncertainty/{machineId}/comprehensive-report` (UncertaintyController)
- `GET /api/Workflow/{id}` (WorkflowController)
- `PUT /api/Workflow/{id}` (WorkflowController)
- `DELETE /api/Workflow/{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteWorkflow(string id)
        {
            try
            {
                var command = new DeleteWorkflowCommand(id);
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting workflow {WorkflowId}", id);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // <summary>
        // Enable/disable workflow
        // </summary>
        [HttpPost("{id}/toggle` (WorkflowController)
- `POST /api/Workflow/{id}/execute` (WorkflowController)
- `GET /api/Workflow/{id}/executions` (WorkflowController)
- `GET /api/Workflow/{id}/statistics` (WorkflowController)
- `POST /api/Workflow/{id}/duplicate` (WorkflowController)

### Broken Frontend Calls (No Backend Match)
- `GET api/dashboard/machine-health` in `dashboard.getMachineHealthSummary`
- `GET api/dashboard/alerts` in `dashboard.getAlertsSummary`
- `GET api/dashboard/performance` in `dashboard.getPerformanceSummary`
- `GET api/dashboard/recent-activity` in `dashboard.getRecentActivity`
- `GET api/dataarchival/policies` in `dataArchival.getPolicies`
- `POST api/dataarchival/policies` in `dataArchival.getPolicy`
- `POST api/dataarchival/policies` in `dataArchival.createPolicy`
- `GET api/dataarchival/stats` in `dataArchival.runArchival`
- `GET api/dataarchival/stats` in `dataArchival.getJobStatus`
- `GET api/dataarchival/stats` in `dataArchival.getStorageStats`
- `POST api/dataarchival/restore` in `dataArchival.restore`
- `GET api/degradationmodeling` in `degradationModeling.getAll`
- `POST api/degradationmodeling` in `degradationModeling.getById`
- `POST api/degradationmodeling` in `degradationModeling.create`
- `GET api/health/metrics` in `health.getMetrics`
- `GET api/health/ready` in `health.isReady`
- `GET api/machineconfiguration/templates` in `machineConfiguration.updateConfiguration`
- `GET api/machineconfiguration/templates` in `machineConfiguration.deleteConfiguration`
- `GET api/machineconfiguration/templates` in `machineConfiguration.getTemplates`
- `POST api/machineconfiguration/templates` in `machineConfiguration.createTemplate`
- `GET api/maintenance/stats` in `maintenance.updateMaintenance`
- `GET api/maintenance/stats` in `maintenance.fetchMaintenanceStats`
- `GET api/maintenance/overdue` in `maintenance.fetchUpcomingMaintenance`
- `GET api/maintenance/overdue` in `maintenance.fetchOverdueMaintenance`
- `GET api/mathematicalmodeling` in `mathematicalModeling.getAll`
- `POST api/mathematicalmodeling` in `mathematicalModeling.getById`
- `POST api/mathematicalmodeling` in `mathematicalModeling.create`
- `POST api/mathematicalmodeling/optimize` in `mathematicalModeling.validate`
- `POST api/mathematicalmodeling/optimize` in `mathematicalModeling.optimize`
- `POST api/modellifecycle` in `modelLifecycle.getByModel`
- `POST api/modellifecycle` in `modelLifecycle.getByVersion`
- `POST api/modellifecycle` in `modelLifecycle.create`
- `GET api/modellifecycle/transitions/available` in `modelLifecycle.update`
- `GET api/modellifecycle/transitions/available` in `modelLifecycle.transition`
- `GET api/modellifecycle/transitions/available` in `modelLifecycle.getAvailableTransitions`
- `GET api/benchmarkvalidation/industry-averages` in `performance.getIndustryAverages`
- `GET api/predictions/models/status` in `predictions.getEnsemblePrediction`
- `GET api/predictions/models/status` in `predictions.getModelStatus`
- `POST api/runtofailure/scenarios` in `runToFailure.getAnalysis`
- `POST api/runtofailure/scenarios` in `runToFailure.getScenarios`
- `POST api/runtofailure/scenarios` in `runToFailure.createScenario`
- `GET api/syntheticdata/profiles` in `syntheticData.getGenerationStatus`
- `GET api/syntheticdata/profiles` in `syntheticData.getProfiles`
- `POST api/syntheticdata/profiles` in `syntheticData.getProfile`
- `POST api/syntheticdata/profiles` in `syntheticData.createProfile`
- `POST api/telemetry` in `telemetry.fetchTelemetryHistory`
- `POST api/telemetry` in `telemetry.ingestTelemetry`
- `POST api/telemetry/batch` in `telemetry.ingestTelemetryBatch`

## 5. TODOs & Placeholders
- `api\DigitalTwinPlatform.API\Controllers\AuthController.cs:452`: // For now, return a placeholder that can be used by the frontend
- `api\DigitalTwinPlatform.API\Controllers\DegradationModelingController.cs:335`: // Create a mock problem for validation
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:42`: // Log a warning that this is a mock implementation
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:43`: _logger.LogWarning("Using mock report generation for template {TemplateId}. Production implementation required.", request.TemplateId);
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:56`: FileSize = 1024 * 1024, // Mock size
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:89`: // Mock implementation - return empty content with appropriate headers
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:123`: // Mock templates - in real implementation, these would come from database
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:211`: NextRun = DateTime.UtcNow.AddHours(24).ToString("o") // Mock next run
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:235`: // Mock history data
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:286`: // Mock file content
- `api\DigitalTwinPlatform.API\Controllers\ReportsController.cs:287`: var content = "Mock report content";
- `api\DigitalTwinPlatform.API\Extensions\ServiceCollectionExtensions.cs:301`: // External System Integration: mock in development; use real implementation for production.
- `api\DigitalTwinPlatform.API\Extensions\ServiceCollectionExtensions.cs:305`: services.AddScoped<IExternalSystemService, MockExternalSystemService>();
- `api\DigitalTwinPlatform.API\Extensions\ServiceCollectionExtensions.cs:328`: /// <param name="environment">Hosting environment; when not Development, TelemetryMockHostedService is not registered.</param>
- `api\DigitalTwinPlatform.API\Extensions\ServiceCollectionExtensions.cs:340`: // Only seed mock telemetry in Development; production should use real ingestion only.
- `api\DigitalTwinPlatform.API\Extensions\ServiceCollectionExtensions.cs:343`: services.AddHostedService<TelemetryMockHostedService>();
- `api\DigitalTwinPlatform.API\Hubs\RealTimeAnalyticsHub.cs:54`: _logger.LogDebug("Client {ConnectionId} subscribed to predictions for {MachineId}", connectionId, machineId);
- `api\DigitalTwinPlatform.API\Hubs\RealTimeAnalyticsHub.cs:99`: _logger.LogDebug("Client {ConnectionId} subscribed to alerts", connectionId);
- `api\DigitalTwinPlatform.API\Hubs\RealTimeAnalyticsHub.cs:215`: _logger.LogDebug("Broadcasted prediction for machine {MachineId}", machineId);
- `api\DigitalTwinPlatform.API\Hubs\TelemetryHub.cs:61`: _logger.LogDebug("Client {ConnectionId} subscribed to machine {MachineId}", connectionId, machineId);
- `api\DigitalTwinPlatform.API\Hubs\TelemetryHub.cs:102`: _logger.LogDebug("Client {ConnectionId} unsubscribed from machine {MachineId}", connectionId, machineId);
- `api\DigitalTwinPlatform.API\Hubs\TelemetryHub.cs:114`: _logger.LogDebug("Telemetry received from {ConnectionId}: {MachineId}", connectionId, telemetry.MachineId);
- `api\DigitalTwinPlatform.API\Infrastructure\HealthChecksConfiguration.cs:54`: _logger.LogDebug("Running application health check");
- `api\DigitalTwinPlatform.API\Infrastructure\LoggingConfiguration.cs:15`: builder.AddDebug();
- `api\DigitalTwinPlatform.API\Infrastructure\MetricsConfiguration.cs:57`: // This is a placeholder for future implementation
- `api\DigitalTwinPlatform.API\Infrastructure\MetricsConfiguration.cs:63`: // This is a placeholder for future implementation
- `api\DigitalTwinPlatform.API\Infrastructure\MetricsConfiguration.cs:69`: // This is a placeholder for future implementation
- `api\DigitalTwinPlatform.API\Services\Analytics\DataDriftService.cs:332`: // For now, we'll return an empty dictionary, but we'll log that this is a placeholder
- `api\DigitalTwinPlatform.API\Services\Analytics\DataDriftService.cs:333`: _logger.LogWarning("Historical data retrieval for model {ModelName} is not implemented - this is a placeholder", modelName);
- `api\DigitalTwinPlatform.API\Services\Analytics\PredictiveXaiService.cs:68`: logger.LogDebug("Using perturbation-based XAI method");
- `api\DigitalTwinPlatform.API\Services\Analytics\Advanced\AdvancedPredictiveService.cs:327`: Debug.Assert(t.Pressure != null);
- `api\DigitalTwinPlatform.API\Services\Analytics\Advanced\AdvancedPredictiveService.cs:398`: Debug.Assert(t.Pressure != null);
- `api\DigitalTwinPlatform.API\Services\Core\NotificationService.cs:125`: // For now, we'll try to post to a placeholder if it were configured
- `api\DigitalTwinPlatform.API\Services\Core\NotificationService.cs:137`: // This is a placeholder. If configured, we would do:
- `api\DigitalTwinPlatform.API\Services\Infrastructure\HubPublisher.cs:34`: _logger.LogDebug("Broadcasting telemetry for machine {MachineId}", machineId);
- `api\DigitalTwinPlatform.API\Services\Infrastructure\HubPublisher.cs:55`: _logger.LogDebug("Broadcasting prediction for machine {MachineId}", machineId);
- `api\DigitalTwinPlatform.API\Services\Infrastructure\HubPublisher.cs:74`: _logger.LogDebug("Broadcasting alert for machine {MachineId}: {Message}", machineId, alert.Message);
- `api\DigitalTwinPlatform.API\Services\Integration\AzureDigitalTwinService.cs:98`: _logger.LogDebug("Successfully upserted twin {TwinId} with model {ModelId}", twinId, modelId);
- `api\DigitalTwinPlatform.API\Services\Integration\AzureDigitalTwinService.cs:337`: values.Add(Convert.ToDouble(decimalValue));
- `api\DigitalTwinPlatform.API\Services\Simulation\RunToFailureOrchestrator.cs:141`: _logger.LogDebug("Run-to-failure step {Step}/{MaxSteps}. Degradation: {Degradation:F4}",
- `api\DigitalTwinPlatform.API\Services\Simulation\RunToFailureOrchestrator.cs:334`: _logger.LogDebug("Generating trajectory {Index}/{Count}", i + 1, count);
- `api\DigitalTwinPlatform.API\Services\Simulation\SimulationEngine.cs:31`: logger.LogDebug("Running simulation step {Step} for simulation {SimulationId}", step, state.Id);
- `api\DigitalTwinPlatform.API\Services\Simulation\SimulationEngine.cs:73`: logger.LogDebug("Completed simulation step {Step} for simulation {SimulationId}", step, state.Id);
- `api\DigitalTwinPlatform.API\Services\Simulation\SimulationHostedService.cs:81`: _logger.LogDebug("Simulation step completed for machine {MachineId}. Step: {Step}",
- `api\DigitalTwinPlatform.API\Services\Simulation\SimulationSchedulerService.cs:95`: _logger.LogDebug("Scheduled machine {MachineId} for simulation at {NextRun}",
- `api\DigitalTwinPlatform.API\Services\Simulation\SimulationSchedulerService.cs:220`: _logger.LogDebug("Unscheduled machine {MachineId} from simulation scheduling", machineId);
- `api\DigitalTwinPlatform.API\Services\Simulation\SyntheticDataGenerator.cs:100`: _logger.LogDebug(
- `api\DigitalTwinPlatform.API\Services\Simulation\SyntheticDataGenerator.cs:409`: // Placeholder - would load actual benchmark statistics
- `api\DigitalTwinPlatform.API\Services\Simulation\TransferFunctionEvaluator.cs:138`: return Convert.ToDouble(result);
- `api\DigitalTwinPlatform.API\Services\Simulation\DegradationModels\MarkovChainDegradationModel.cs:37`: ? Convert.ToDouble(ts)
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:7`: public class TelemetryMockHostedService(
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:9`: ILogger<TelemetryMockHostedService> logger)
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:37`: logger.LogWarning("TelemetryMockHostedService found no machines to seed telemetry for.");
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:46`: "Mock telemetry generated for machine {machineName}: {Count} records",
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:66`: "Mock telemetry batch {BatchNumber}  {Count} records",
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:73`: "TelemetryMockHostedService error when inserting telemetry batch {BatchNumber}  Batch skipped.",
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:84`: "TelemetryMockHostedService concurrency conflict when inserting telemetry for tenant. Batch skipped.");
- `api\DigitalTwinPlatform.API\Services\Telemetry\TelemetryMockHostedService.cs:93`: logger.LogError(ex, "TelemetryMockHostedService iteration failed");
- `api\DigitalTwinPlatform.Application\Behaviors\AuthorizationBehavior.cs:24`: logger.LogDebug("Command {CommandName} has no role requirement", commandName);
- `api\DigitalTwinPlatform.Application\Behaviors\PerformanceBehavior.cs:35`: logger.LogDebug(
- `api\DigitalTwinPlatform.Application\Configuration\EnvironmentConfigurationService.cs:61`: //             _logger.LogDebug("Environment value not found for key: {Key}, using default value", key);
- `api\DigitalTwinPlatform.Application\Configuration\EnvironmentConfigurationService.cs:81`: //         _logger.LogDebug("Feature {FeatureName} is {Enabled}", featureName, isEnabled ? "enabled" : "disabled");
- `api\DigitalTwinPlatform.Application\Configuration\EnvironmentConfigurationService.cs:97`: //             _logger.LogDebug("Using development database connection");
- `api\DigitalTwinPlatform.Application\ExternalSystems\Services\MockExternalSystemService.cs:7`: public class MockExternalSystemService : IExternalSystemService
- `api\DigitalTwinPlatform.Application\ExternalSystems\Services\MockExternalSystemService.cs:13`: public MockExternalSystemService()
- `api\DigitalTwinPlatform.Application\ML\FastForestPredictor.cs:102`: // Log when mock importance is returned
- `api\DigitalTwinPlatform.Application\ML\FeatureImportanceExtractor.cs:64`: // For simplicity in this implementation, we return mock importance based on a stable seed
- `api\DigitalTwinPlatform.Application\ML\ShapExplainer.cs:32`: // Log that mock SHAP values are being generated
- `api\DigitalTwinPlatform.Application\ML\ShapExplainer.cs:33`: _logger.LogWarning("Generating mock SHAP values as proper SHAP library not available");
- `api\DigitalTwinPlatform.Application\ML\ShapExplainer.cs:35`: // Generate mock SHAP values based on feature importance
- `api\DigitalTwinPlatform.Application\ML\Handlers\AIModelCommandHandlers.cs:40`: // Save model file logic (mocked path for now if file handling is not fully set up)
- `api\DigitalTwinPlatform.Application\ML\Handlers\AIModelQueryHandlers.cs:73`: Description = "Mock model returned by ID",
- `api\DigitalTwinPlatform.Application\ML\Handlers\AIModelQueryHandlers.cs:85`: Tags = new List<string> { "mock" },
- `api\DigitalTwinPlatform.Application\Services\PerformanceCacheService.cs:40`: _logger.LogDebug("Cache HIT for key: {Key}", key);
- `api\DigitalTwinPlatform.Application\Services\PerformanceCacheService.cs:44`: _logger.LogDebug("Cache MISS for key: {Key}, fetching from source", key);
- `api\DigitalTwinPlatform.Application\Services\PerformanceCacheService.cs:110`: _logger.LogDebug("Cache entry evicted: {Key} - Reason: {Reason}", cacheKey, reason);
- `api\DigitalTwinPlatform.Application\Services\PredictionService.cs:79`: FeatureContributions = mlResult.FeatureImportance.ToDictionary(k => k.Key, v => Convert.ToDouble(v.Value)) ?? features,
- `api\DigitalTwinPlatform.Application\Services\SyntheticDataGenerator.cs:422`: // Mock benchmark data (e.g. from NASA CMAPSS)
- `api\DigitalTwinPlatform.Application\Services\TelemetryService.cs:70`: _logger.LogDebug("Telemetry processed in {ProcessingTime}ms for machine {MachineId}",
- `api\DigitalTwinPlatform.Application\Simulations\Services\SimulationService.cs:79`: 0.0, // Duration placeholder
- `api\DigitalTwinPlatform.Application\Tenants\Services\MockTenantService.cs:6`: public class MockTenantService : ITenantService
- `api\DigitalTwinPlatform.Application\Tenants\Services\MockTenantService.cs:12`: public MockTenantService()
- `api\DigitalTwinPlatform.Application\Workflows\Handlers\WorkflowCommandHandlers.cs:64`: // For demo purposes, return a modified mock workflow
- `api\DigitalTwinPlatform.Application\Workflows\Handlers\WorkflowQueryHandlers.cs:25`: // Return mock workflows for demonstration
- `api\DigitalTwinPlatform.Application\Workflows\Handlers\WorkflowQueryHandlers.cs:175`: // For now, return one of the mock workflows based on ID
- `api\DigitalTwinPlatform.Application\Workflows\Services\IWorkflowService.cs:397`: private WorkflowExecutionDto CreateMockExecution(string workflowId, Dictionary<string, object> context)
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:13`: private readonly Mock<ILogger<MLModelService>> _mockLogger;
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:14`: private readonly Mock<FastForestPredictor> _mockFastForest;
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:15`: private readonly Mock<QuantileRegression> _mockQuantileRegression;
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:16`: private readonly Mock<ShapExplainer> _mockShapExplainer;
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:17`: private readonly Mock<FeatureImportanceExtractor> _mockImportanceExtractor;
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:22`: _mockLogger = new Mock<ILogger<MLModelService>>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:23`: _mockFastForest = new Mock<FastForestPredictor>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:24`: _mockQuantileRegression = new Mock<QuantileRegression>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:25`: _mockShapExplainer = new Mock<ShapExplainer>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:26`: _mockImportanceExtractor = new Mock<FeatureImportanceExtractor>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:29`: _mockFastForest.Object,
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:30`: _mockQuantileRegression.Object,
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:31`: _mockShapExplainer.Object,
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:32`: _mockImportanceExtractor.Object,
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:33`: _mockLogger.Object
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:43`: _mockFastForest.Setup(x => x.Train(It.IsAny<IEnumerable<ModelTrainingData>>()));
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:44`: _mockFastForest.Setup(x => x.Predict(It.IsAny<ModelInputData>()))
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:50`: _mockQuantileRegression.Setup(x => x.Train(It.IsAny<IEnumerable<ModelTrainingData>>()));
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:101`: var mockModel = new Mock<ML.Model>();
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:102`: _mockFastForest.Setup(x => x.Model).Returns(mockModel.Object);
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:103`: _mockFastForest.Setup(x => x.Predict(It.IsAny<ModelInputData>()))
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:117`: _mockQuantileRegression.Setup(x => x.PredictInterval(It.IsAny<ModelInputData>()))
- `api\DigitalTwinPlatform.Tests\Services\MLModelServiceTests.cs:120`: _mockShapExplainer.Setup(x => x.GetExplainerValues(It.IsAny<ML.Model>(), It.IsAny<ModelInputData>()))
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:14`: private readonly Mock<ILogger<SyntheticDataGenerator>> _mockLogger;
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:15`: private readonly Mock<IUnitOfWork> _mockUnitOfWork;
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:16`: private readonly Mock<ISyntheticDataGeneratorRepository> _mockRepository;
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:21`: _mockLogger = new Mock<ILogger<SyntheticDataGenerator>>();
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:22`: _mockUnitOfWork = new Mock<IUnitOfWork>();
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:23`: _mockRepository = new Mock<ISyntheticDataGeneratorRepository>();
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:25`: _mockUnitOfWork.Setup(x => x.Repository<Domain.Entities.SyntheticDataGeneration>())
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:26`: .Returns(_mockRepository.Object);
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:29`: _mockUnitOfWork.Object,
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:30`: _mockLogger.Object
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:46`: _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.SyntheticDataGeneration>(), It.IsAny<CancellationToken>()))
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:48`: _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:78`: _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.SyntheticDataGeneration>(), It.IsAny<CancellationToken>()))
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:80`: _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
- `api\DigitalTwinPlatform.Tests\Services\SyntheticDataGeneratorTests.cs:100`: _mockRepository.Setup(x => x.AddAsync(It.IsAny<Domain.Entities.DataValidationReport>(), It.IsAny<CancellationToken>()))
- `api\DigitalTwinPlatform.Tests\Services\Simulation\SimulationTest.cs:15`: private readonly Mock<ILogger<SimulationEngine>> _loggerMock;
- `api\DigitalTwinPlatform.Tests\Services\Simulation\SimulationTest.cs:16`: private readonly Mock<IDataValidationService> _validationServiceMock;
- `api\DigitalTwinPlatform.Tests\Services\Simulation\SimulationTest.cs:21`: _loggerMock = new Mock<ILogger<SimulationEngine>>();
- `api\DigitalTwinPlatform.Tests\Services\Simulation\SimulationTest.cs:22`: _validationServiceMock = new Mock<IDataValidationService>();
- `api\DigitalTwinPlatform.Tests\Services\Simulation\SimulationTest.cs:23`: _simulationEngine = new SimulationEngine(_loggerMock.Object, _validationServiceMock.Object);
- `frontend\src\components\alerts\AlertListPanel.vue:184`: placeholder="Search alerts..."
- `frontend\src\components\alerts\AlertRulesConfig.vue:204`: placeholder="e.g., High Vibration Alert"
- `frontend\src\components\alerts\AlertRulesConfig.vue:215`: placeholder="Describe when this alert triggers..."
- `frontend\src\components\auth\LoginForm.vue:87`: placeholder="you@example.com"
- `frontend\src\components\auth\LoginForm.vue:118`: placeholder="Enter your password"
- `frontend\src\components\auth\PasswordResetForm.vue:153`: placeholder="you@example.com"
- `frontend\src\components\auth\PasswordResetForm.vue:190`: placeholder="you@example.com"
- `frontend\src\components\auth\PasswordResetForm.vue:211`: placeholder="Enter new password"
- `frontend\src\components\auth\PasswordResetForm.vue:255`: placeholder="Confirm new password"
- `frontend\src\components\auth\RegisterForm.vue:131`: placeholder="John Doe"
- `frontend\src\components\auth\RegisterForm.vue:158`: placeholder="you@example.com"
- `frontend\src\components\auth\RegisterForm.vue:185`: placeholder="Create a strong password"
- `frontend\src\components\auth\RegisterForm.vue:246`: placeholder="Confirm your password"
- `frontend\src\components\charts\AlertsTimelineChart.vue:130`: generateMockData()
- `frontend\src\components\charts\AlertsTimelineChart.vue:139`: function generateMockData() {
- `frontend\src\components\charts\AlertsTimelineChart.vue:142`: const mockAlerts: AlertTimelineItem[] = []
- `frontend\src\components\charts\AlertsTimelineChart.vue:148`: mockAlerts.push({
- `frontend\src\components\charts\AlertsTimelineChart.vue:157`: alerts.value = mockAlerts.sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())
- `frontend\src\components\charts\HealthTrendChart.vue:187`: generateMockData()
- `frontend\src\components\charts\HealthTrendChart.vue:205`: function generateMockData() {
- `frontend\src\components\charts\PredictionVsActualChart.vue:73`: generateMockData()
- `frontend\src\components\charts\PredictionVsActualChart.vue:82`: function generateMockData() {
- `frontend\src\components\charts\SensorDistributionChart.vue:166`: generateMockData()
- `frontend\src\components\charts\SensorDistributionChart.vue:198`: function generateMockData() {
- `frontend\src\components\dashboard\HeaderBar.vue:51`: <input v-model="searchQuery" type="text" placeholder="Search machines, alerts..." class="w-full px-4 py-2 pl-10 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent" @keydown.esc="isSearchOpen = false" />
- `frontend\src\components\predictions\RulTimelineChart.vue:38`: const generateMockData = () => {
- `frontend\src\components\predictions\RulTimelineChart.vue:119`: generateMockData()
- `frontend\src\components\predictions\RulTimelineChart.vue:169`: <!-- Chart placeholder -->
- `frontend\src\views\Alerts.vue:19`: // Use alerts from store instead of mock data
- `frontend\src\views\Alerts.vue:51`: // Alert rules: empty until backend AlertRules API is used; show empty state instead of mock data
- `frontend\src\views\Machines.vue:111`: placeholder="Search machines..."
- `ui\digital-twin-dashboard\vite.config.optimized.ts:98`: drop_debugger: true,
- `ui\digital-twin-dashboard\vite.config.optimized.ts:99`: pure_funcs: ['console.info', 'console.debug', 'console.warn']
- `ui\digital-twin-dashboard\src\App.vue:52`: // Debug auth state on mount
- `ui\digital-twin-dashboard\src\vite-env.d.ts:11`: readonly VITE_DEBUG: string
- `ui\digital-twin-dashboard\src\components\MachineCard.vue:130`: // Mock health calculation based on status and RUL
- `ui\digital-twin-dashboard\src\components\PredictiveAnalytics.vue:54`: placeholder="Search equipment by serial, model or line..."
- `ui\digital-twin-dashboard\src\components\PredictiveAnalytics.vue:55`: class="w-full bg-transparent h-16 pl-16 pr-8 text-[13px] font-black tracking-tight placeholder:text-secondary-alt focus:outline-none"
- `ui\digital-twin-dashboard\src\components\admin\DataArchivalManagement.vue:268`: placeholder="Enter number of days to retain data"
- `ui\digital-twin-dashboard\src\components\admin\ExternalSystemIntegrationDashboard.vue:530`: placeholder="Enter system name"
- `ui\digital-twin-dashboard\src\components\admin\ExternalSystemIntegrationDashboard.vue:544`: placeholder="https://api.example.com"
- `ui\digital-twin-dashboard\src\components\admin\ExternalSystemIntegrationDashboard.vue:551`: placeholder="Enter API key (optional)"
- `ui\digital-twin-dashboard\src\components\admin\ExternalSystemIntegrationDashboard.vue:557`: placeholder="Enter username (optional)"
- `ui\digital-twin-dashboard\src\components\admin\ExternalSystemIntegrationDashboard.vue:564`: placeholder="Enter password (optional)"
- `ui\digital-twin-dashboard\src\components\admin\TenantManagementDashboard.vue:360`: placeholder="Enter tenant name"
- `ui\digital-twin-dashboard\src\components\admin\TenantManagementDashboard.vue:367`: placeholder="Enter tenant slug (lowercase, no spaces)"
- `ui\digital-twin-dashboard\src\components\admin\TenantManagementDashboard.vue:374`: placeholder="Enter tenant description"
- `ui\digital-twin-dashboard\src\components\admin\TenantManagementDashboard.vue:381`: placeholder="Enter database connection string"
- `ui\digital-twin-dashboard\src\components\alerts\AlertManagement.vue:294`: placeholder="Search alerts..."
- `ui\digital-twin-dashboard\src\components\alerts\AlertManagement.vue:462`: placeholder="Add any notes about this acknowledgment..."
- `ui\digital-twin-dashboard\src\components\analytics\AdvancedAnalyticsVisualizationDashboard.vue:74`: // Mock analytics data
- `ui\digital-twin-dashboard\src\components\analytics\AdvancedAnalyticsVisualizationDashboard.vue:468`: <BaseSkeleton v-for="i in 4" :key="i" height="300px" class="chart-placeholder" />
- `ui\digital-twin-dashboard\src\components\analytics\AdvancedAnalyticsVisualizationDashboard.vue:646`: .chart-placeholder {
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:166`: placeholder="e.g., CNC Degradation Predictor v2"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:175`: placeholder="Describe the model's purpose and capabilities..."
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:182`: placeholder="e.g., 2.1.0"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:189`: placeholder="predictive, cnc, degradation"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:215`: placeholder="Number of training samples"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:222`: placeholder="temperature,vibration,pressure,current"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:239`: placeholder="0.95"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:250`: placeholder="0.92"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:260`: placeholder="0.88"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:270`: placeholder="0.90"
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:283`: placeholder="Select machines to deploy this model to..."
- `ui\digital-twin-dashboard\src\components\analytics\AIModelManagement.vue:380`: placeholder="Enter value..."
- `ui\digital-twin-dashboard\src\components\analytics\PredictiveAnalyticsDashboard.vue:424`: placeholder="Enter machine UUID"
- `ui\digital-twin-dashboard\src\components\analytics\RealTimeAnalyticsDashboard.vue:329`: <div class="chart-placeholder">
- `ui\digital-twin-dashboard\src\components\analytics\RealTimeAnalyticsDashboard.vue:330`: <Activity class="placeholder-icon" />
- `ui\digital-twin-dashboard\src\components\analytics\RealTimeAnalyticsDashboard.vue:343`: <div class="chart-placeholder">
- `ui\digital-twin-dashboard\src\components\analytics\RealTimeAnalyticsDashboard.vue:344`: <TrendingUp class="placeholder-icon" />
- `ui\digital-twin-dashboard\src\components\analytics\RealTimeAnalyticsDashboard.vue:357`: <div class="chart-placeholder">

## 6. Recommendations
1. **Synchronize DTOs:** 29 DTOs have field mismatches. Update frontend types and backend DTOs to match contracts.
2. **Implement Missing Service Logic:** 20 interface methods are registered but lack implementation.
3. **Fix Routing:** 17 frontend calls point to non-existent or misconfigured routes.
4. **Cleanup Dead Code:** 75 endpoints are not consumed; verify if they are for future use or can be removed.
5. **Address TODOs:** 525 markers found. Many are in critical paths (ML retraining, synthetic data generation).
