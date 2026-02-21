# ML.NET Integration and Model Management

<cite>
**Referenced Files in This Document**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs)
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs)
- [MLDependencyInjection.cs](file://src/api/DigitalTwinPlatform.Application/ML/MLDependencyInjection.cs)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs)
- [IModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/IModelLifecycleService.cs)
- [ModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelLifecycleService.cs)
- [ModelRetrainingService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelRetrainingService.cs)
- [AIModelController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/AIModelController.cs)
- [ModelLifecycleController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/ModelLifecycleController.cs)
</cite>

## Table of Contents
1. [Introduction](#introduction)
2. [Project Structure](#project-structure)
3. [Core Components](#core-components)
4. [Architecture Overview](#architecture-overview)
5. [Detailed Component Analysis](#detailed-component-analysis)
6. [Dependency Analysis](#dependency-analysis)
7. [Performance Considerations](#performance-considerations)
8. [Troubleshooting Guide](#troubleshooting-guide)
9. [Conclusion](#conclusion)
10. [Appendices](#appendices)

## Introduction
This document explains the ML.NET integration and model management within the predictive maintenance engine. It focuses on the MLModelService orchestration, the FastForestPredictor ensemble model, QuantileRegression for uncertainty quantification, and the SHAP-style explainer for interpretability. It also covers model lifecycle management, training workflows, inference pipelines, performance optimization, model versioning, automated retraining triggers, and quality assurance processes.

## Project Structure
The ML stack spans the Application layer (ML pipeline components and services) and the API layer (controllers and lifecycle services). Key areas:
- Application ML components: FastForestPredictor, QuantileRegression, ShapExplainer, FeatureImportanceExtractor, MLModelService, PredictionService.
- API controllers: AIModelController for model deployment and management, ModelLifecycleController for versioning and comparisons.
- Analytics services: IModelLifecycleService/ModelLifecycleService for model versioning and performance, ModelRetrainingService for drift detection and retraining.

```mermaid
graph TB
subgraph "API Layer"
C1["AIModelController"]
C2["ModelLifecycleController"]
end
subgraph "Application Services"
S1["MLModelService"]
S2["PredictionService"]
L1["IModelLifecycleService"]
L2["ModelLifecycleService"]
R1["ModelRetrainingService"]
end
subgraph "ML Components"
M1["FastForestPredictor"]
M2["QuantileRegression"]
M3["ShapExplainer"]
M4["FeatureImportanceExtractor"]
end
C1 --> S1
C2 --> L2
S2 --> S1
S1 --> M1
S1 --> M2
S1 --> M3
S1 --> M4
L1 --> L2
R1 --> S1
```

**Diagram sources**
- [AIModelController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/AIModelController.cs#L1-L464)
- [ModelLifecycleController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/ModelLifecycleController.cs#L1-L220)
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L1-L248)
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L1-L335)
- [IModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/IModelLifecycleService.cs#L1-L69)
- [ModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelLifecycleService.cs#L1-L261)
- [ModelRetrainingService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelRetrainingService.cs#L1-L69)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L1-L158)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L1-L88)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L1-L62)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L1-L79)

**Section sources**
- [MLDependencyInjection.cs](file://src/api/DigitalTwinPlatform.Application/ML/MLDependencyInjection.cs#L1-L40)
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L1-L248)
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L1-L335)

## Core Components
- MLModelService orchestrates the end-to-end pipeline: trains FastForest and Quantile models, performs inference, computes uncertainty bounds, and generates SHAP-like explanations.
- FastForestPredictor encapsulates ML.NET FastForest regression training, evaluation, prediction, and persistence.
- QuantileRegression trains separate FastTree models for upper and lower quantiles to produce confidence intervals.
- ShapExplainer provides SHAP-style feature contributions via permutation feature importance fallback.
- FeatureImportanceExtractor supports both gain-based and permutation feature importance extraction.
- PredictionService integrates telemetry ingestion, feature extraction, and MLModelService to produce predictions with health classification and uncertainty.

**Section sources**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L14-L127)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L9-L127)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L7-L86)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L6-L54)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L8-L77)
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L14-L101)

## Architecture Overview
The ML pipeline follows a layered architecture:
- Data ingestion and feature extraction in PredictionService.
- Ensemble prediction via FastForest.
- Uncertainty quantification via QuantileRegression.
- Interpretability via SHAP-style contributions.
- Model lifecycle and retraining orchestrated by API controllers and services.

```mermaid
sequenceDiagram
participant Client as "Client"
participant Ctrl as "AIModelController"
participant Med as "MediatR"
participant Svc as "MLModelService"
participant FF as "FastForestPredictor"
participant QR as "QuantileRegression"
participant SH as "ShapExplainer"
Client->>Ctrl : "POST /api/ai-models/{id}/predict"
Ctrl->>Med : "Send Predict command"
Med->>Svc : "PredictRulAsync(machineId, features)"
Svc->>FF : "Predict(input)"
FF-->>Svc : "RUL prediction + feature importance"
Svc->>QR : "PredictInterval(input)"
QR-->>Svc : "Lower/Upper bounds"
Svc->>SH : "GetExplainerValues(model, input)"
SH-->>Svc : "SHAP-like contributions"
Svc-->>Med : "ModelPredictionDto"
Med-->>Ctrl : "ModelPredictionDto"
Ctrl-->>Client : "200 OK prediction"
```

**Diagram sources**
- [AIModelController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/AIModelController.cs#L213-L242)
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L36-L72)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L65-L86)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L46-L60)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L17-L54)

## Detailed Component Analysis

### MLModelService
Responsibilities:
- Coordinates FastForest and QuantileRegression training.
- Performs inference combining base prediction and uncertainty bounds.
- Generates SHAP-like feature contributions.
- Computes evaluation metrics (R2, MAE, RMSE, MAPE) for training results.

Key behaviors:
- Training pipeline: trains FastForest, then quantile models; returns metrics and timestamps.
- Inference pipeline: constructs input, predicts RUL, computes confidence intervals, extracts SHAP-like contributions, and packages results.
- Evaluation helpers: calculates R2, MAE, RMSE, and MAPE using FastForest predictions against training data.

```mermaid
flowchart TD
Start(["TrainModelsAsync"]) --> TrainFF["Train FastForest"]
TrainFF --> TrainQR["Train Quantile Regression"]
TrainQR --> Eval["Compute Metrics"]
Eval --> Done(["TrainingResultDto"])
subgraph "Inference"
IStart(["PredictRulAsync"]) --> Build["Build ModelInputData"]
Build --> PredFF["FastForest.Predict"]
PredFF --> Bounds["QuantileRegression.PredictInterval"]
Bounds --> Explain["ShapExplainer.GetExplainerValues"]
Explain --> Package["Package ModelPredictionDto"]
end
```

**Diagram sources**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L74-L127)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L23-L63)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L13-L60)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L17-L54)

**Section sources**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L14-L246)

### FastForestPredictor
Responsibilities:
- Loads training data, splits into train/test, builds FastForest regression pipeline, fits model, evaluates metrics, and persists model bytes.
- Produces predictions and risk levels; provides feature importance extraction (mocked in current implementation).
- Supports model load/save for deployment scenarios.

Implementation highlights:
- Concatenates six features: Temperature, Vibration, Pressure, Rpm, Age, CycleCount.
- Uses FastForest trainer with configurable hyperparameters.
- Exposes SaveModel/LoadModel for persistence.

```mermaid
classDiagram
class FastForestPredictor {
-MLContext _mlContext
-ITransformer _model
+Train(trainingData) Dictionary~string,double~
+Predict(input) ModelPredictionDto
+SaveModel() byte[]
+LoadModel(bytes) void
-ExtractFeatureImportance() Dictionary~string,object~
}
```

**Diagram sources**
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L9-L127)

**Section sources**
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L23-L127)

### QuantileRegression
Responsibilities:
- Trains two FastTree models for upper and lower quantiles to estimate prediction intervals.
- Provides prediction engines for interval bounds and supports model persistence.

Implementation highlights:
- Trains separate models per quantile (e.g., 0.05 and 0.95).
- Returns tuple of (lower, upper) bounds during inference.

```mermaid
classDiagram
class QuantileRegression {
-MLContext _mlContext
-ITransformer _model95Upper
-ITransformer _model95Lower
+Train(trainingData) void
+PredictInterval(input) (double,double)
+SaveUpperModel() byte[]
+SaveLowerModel() byte[]
+LoadModels(upper, lower) void
}
```

**Diagram sources**
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L7-L86)

**Section sources**
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L13-L86)

### ShapExplainer and FeatureImportanceExtractor
Responsibilities:
- ShapExplainer: Generates SHAP-like feature contributions using permutation feature importance fallback and logs warnings for unsupported SHAP library usage.
- FeatureImportanceExtractor: Computes permutation feature importance and gain-based importance extraction (with fallback to uniform importance).

Implementation highlights:
- Uses ML.NET’s PermutationFeatureImportance for robust feature attribution.
- Logs warnings and returns deterministic mock values when advanced SHAP integration is unavailable.

```mermaid
classDiagram
class ShapExplainer {
-MLContext _mlContext
+GetExplainerValues(model, input) Dictionary~string,double~
}
class FeatureImportanceExtractor {
+CalculatePermutationImportance(model, data) Dictionary~string,double~
+ExtractGainImportance(model) Dictionary~string,double~
}
```

**Diagram sources**
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L6-L54)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L8-L77)

**Section sources**
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L17-L54)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L12-L77)

### PredictionService
Responsibilities:
- Orchestrates end-to-end prediction: fetches recent telemetry, extracts features, invokes MLModelService, persists Prediction entity, and returns PredictionDto.
- Provides search, filtering, and broadcasting utilities for predictions.

Performance characteristics:
- Designed for low-latency inference with under 500ms target.
- Integrates health classification and confidence thresholds.

```mermaid
sequenceDiagram
participant Client as "Client"
participant PS as "PredictionService"
participant TR as "TelemetryRepository"
participant FE as "FeatureExtractionService"
participant MS as "MLModelService"
participant DB as "PredictionRepository"
Client->>PS : "CreatePredictionAsync(request)"
PS->>TR : "GetAll(machineId)"
TR-->>PS : "Recent telemetry"
PS->>FE : "ExtractFeatures(telemetry)"
FE-->>PS : "Features dict"
PS->>MS : "PredictRulAsync(machineId, features)"
MS-->>PS : "ModelPredictionDto"
PS->>DB : "Add(Prediction)"
DB-->>PS : "Saved"
PS-->>Client : "PredictionDto"
```

**Diagram sources**
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L39-L101)

**Section sources**
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L14-L335)

### Model Lifecycle Management
Responsibilities:
- Register model versions with metadata and metrics.
- Promote versions to production, deprecating previous production models.
- Compare models across metrics and compute performance summaries.
- Provide APIs for querying versions and performance.

```mermaid
sequenceDiagram
participant Admin as "Admin"
participant LC as "ModelLifecycleController"
participant LS as "ModelLifecycleService"
participant Repo as "ModelVersionRepository"
Admin->>LC : "POST /api/model-lifecycle/register"
LC->>LS : "RegisterModelVersionAsync(...)"
LS->>Repo : "Add(modelVersion)"
Repo-->>LS : "Saved"
LS-->>LC : "ModelVersion"
LC-->>Admin : "200 OK"
Admin->>LC : "POST /api/model-lifecycle/{id}/promote"
LC->>LS : "PromoteModelAsync(id, Production)"
LS->>Repo : "Update(status=Production)"
Repo-->>LS : "Updated"
LS-->>LC : "ModelVersion"
LC-->>Admin : "200 OK"
```

**Diagram sources**
- [ModelLifecycleController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/ModelLifecycleController.cs#L25-L73)
- [ModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelLifecycleService.cs#L28-L118)

**Section sources**
- [IModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/IModelLifecycleService.cs#L6-L69)
- [ModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelLifecycleService.cs#L28-L261)

### Automated Retraining and Drift Detection
Responsibilities:
- Detect distribution drift using benchmark validation and trigger retraining.
- Simulate retraining process and deploy new model versions.

```mermaid
flowchart TD
A["Collect recent predictions"] --> B["Validate against benchmark dataset"]
B --> C{"Is drift significant?"}
C -- "No" --> D["No action"]
C -- "Yes" --> E["StartRetrainingAsync"]
E --> F["Fetch historical data"]
F --> G["Train new pipeline (simulated)"]
G --> H["Deploy new model version"]
H --> I["Promote to production (optional)"]
```

**Diagram sources**
- [ModelRetrainingService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelRetrainingService.cs#L31-L67)

**Section sources**
- [ModelRetrainingService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelRetrainingService.cs#L9-L69)

## Dependency Analysis
ML services are registered via dependency injection and composed by MLModelService. Controllers delegate to MediatR handlers for model management operations.

```mermaid
graph LR
DI["MLDependencyInjection"] --> FF["FastForestPredictor"]
DI --> QR["QuantileRegression"]
DI --> SH["ShapExplainer"]
DI --> FIE["FeatureImportanceExtractor"]
DI --> IMS["IMLModelService/MLModelService"]
Ctrl1["AIModelController"] --> IMS
Ctrl2["ModelLifecycleController"] --> MLS["ModelLifecycleService"]
```

**Diagram sources**
- [MLDependencyInjection.cs](file://src/api/DigitalTwinPlatform.Application/ML/MLDependencyInjection.cs#L13-L37)
- [AIModelController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/AIModelController.cs#L1-L464)
- [ModelLifecycleController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/ModelLifecycleController.cs#L1-L220)

**Section sources**
- [MLDependencyInjection.cs](file://src/api/DigitalTwinPlatform.Application/ML/MLDependencyInjection.cs#L13-L37)

## Performance Considerations
- FastForestPredictor uses a single prediction engine per model; cache engines if repeated predictions are frequent.
- QuantileRegression maintains two models; consider lazy initialization and shared context if memory constrained.
- ShapExplainer falls back to permutation feature importance; ensure adequate sample sizes for reliable attribution.
- PredictionService targets sub-500ms latency; batch operations and feature caching can help reduce overhead.
- Persist models efficiently using SaveModel/LoadModel to avoid retraining on startup.

[No sources needed since this section provides general guidance]

## Troubleshooting Guide
Common issues and resolutions:
- Model not trained before prediction: Ensure TrainModelsAsync completes before PredictRulAsync.
- Missing SHAP values: SHAP integration is mocked; verify logging for warnings and consider integrating a proper SHAP library.
- Low telemetry count: PredictionService warns when insufficient telemetry is available; collect more recent telemetry windows.
- Drift detection false positives: Adjust benchmark validation thresholds and re-evaluate drift criteria.

**Section sources**
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L67-L70)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L48-L51)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L33-L53)
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L53-L59)

## Conclusion
The ML.NET stack integrates FastForest, quantile regression, and SHAP-style explanations to deliver accurate, interpretable, and efficient RUL predictions. The system supports robust model lifecycle management, automated retraining triggers, and performance monitoring, enabling continuous improvement and operational reliability.

[No sources needed since this section summarizes without analyzing specific files]

## Appendices

### Example Training Workflow
- Prepare training data with six features and labels.
- Call TrainModelsAsync on MLModelService to train FastForest and quantile models.
- Capture metrics and persist models using FastForestPredictor.SaveModel and QuantileRegression.SaveUpperModel/SaveLowerModel.

**Section sources**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L74-L113)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L113-L126)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L62-L85)

### Example Inference Pipeline
- Collect recent telemetry and extract features.
- Invoke PredictionService.CreatePredictionAsync to obtain PredictionDto with RUL, confidence bounds, and feature contributions.

**Section sources**
- [PredictionService.cs](file://src/api/DigitalTwinPlatform.Application/Services/PredictionService.cs#L39-L101)

### Model Versioning and Promotion
- Register new model versions with metrics and notes.
- Promote to production, automatically deprecating prior production versions.
- Compare versions and retrieve performance summaries.

**Section sources**
- [ModelLifecycleController.cs](file://src/api/DigitalTwinPlatform.API/Controllers/ModelLifecycleController.cs#L25-L118)
- [ModelLifecycleService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelLifecycleService.cs#L28-L261)

### Automated Retraining Triggers
- Monitor drift using ModelRetrainingService.CheckDriftAndRetrainAsync.
- Initiate retraining and deploy new versions when drift exceeds thresholds.

**Section sources**
- [ModelRetrainingService.cs](file://src/api/DigitalTwinPlatform.API/Services/Analytics/ModelRetrainingService.cs#L31-L67)

### Configuration Examples
- FastForestPredictor: Configure number of leaves and trees in the training pipeline.
- QuantileRegression: Set quantile levels for upper/lower bounds.
- ShapExplainer: Use permutation feature importance fallback; integrate SHAP library for advanced explanations.
- FeatureImportanceExtractor: Choose between gain-based and permutation importance extraction.

**Section sources**
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L41-L45)
- [QuantileRegression.cs](file://src/api/DigitalTwinPlatform.Application/ML/QuantileRegression.cs#L37-L41)
- [ShapExplainer.cs](file://src/api/DigitalTwinPlatform.Application/ML/ShapExplainer.cs#L23-L44)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L51-L76)

### Relationship Between ML Models and Prediction Algorithms
- FastForestPredictor provides the primary regression algorithm for RUL prediction.
- QuantileRegression augments predictions with uncertainty bounds.
- SHAP-style explainer offers feature contribution insights.
- FeatureImportanceExtractor supplies both gain-based and permutation-based importance metrics.

**Section sources**
- [MLModelService.cs](file://src/api/DigitalTwinPlatform.Application/Services/MLModelService.cs#L50-L71)
- [FastForestPredictor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FastForestPredictor.cs#L88-L111)
- [FeatureImportanceExtractor.cs](file://src/api/DigitalTwinPlatform.Application/ML/FeatureImportanceExtractor.cs#L12-L49)