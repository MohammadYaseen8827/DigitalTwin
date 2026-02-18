# Machine Learning Pipeline & Explainability

## 1. Predictive Models
The platform uses **ML.NET** for high-performance, interpretable predictions.

### FastForest Regression
- **Algorithm**: Simplified Gradient Boosting (FastForest).
- **Purpose**: Provides the primary Remaining Useful Life (RUL) estimate.
- **Why**: Excellent balance between training speed and predictive power on structured sensor data.
- **Performance**: Sub-second training times on synthetic datasets.

### Quantile Regression
- **Purpose**: Generates 95% confidence intervals (Lower/Upper bounds).
- **Mechanism**: Trains separate models for specific quantiles (0.05, 0.95) to create a prediction "envelope".
- **Benefit**: Technicians can see the "worst-case" and "best-case" scenarios, not just a single point estimate.
- **Visualization**: Confidence bands displayed in ECharts with interactive tooltips.

## 2. Explainable AI (XAI)
To ensure SME technicians trust the AI, we provide deep attributions via **SHAP (SHapley Additive exPlanations)**.

### SHAP Explainer
- **Local Attribution**: Each prediction is broken down into feature contributions (e.g., "Temperature increased risk by 15%").
- **Visualization**: Frontend renders "Force Plots" showing protective vs. degrading factors.
- **Implementation**: `ShapExplainer.cs` calculates the marginal contribution of each feature across a background dataset.
- **Summary Plots**: Global feature importance rankings with uncertainty quantification.

### Feature Importance
- **Permutation Importance**: Measures decrease in model performance when features are randomly shuffled.
- **Gini Importance**: Tree-based feature importance from FastForest models.
- **Correlation Analysis**: Identifies redundant features and multicollinearity issues.

## 3. Drift & Lifecycle
- **Drift Detection**: The system monitors prediction residuals and feature distributions. If "Concept Drift" is detected (e.g., sensor calibration shift), it triggers an automated retraining alert.
- **Automated Retraining**: Models can be retrained on-the-fly using the latest telemetry combined with the synthetic foundation.
- **Model Versioning**: Semantic versioning for deployed models with rollback capability.
- **Performance Monitoring**: Continuous tracking of MAE, RMSE, and calibration metrics.

## 4. Feature Engineering
The pipeline automatically extracts statistical features from raw telemetry:
- **Rolling Means/StdDev**: Captures noise and stability over configurable windows.
- **Trend Slopes**: Identifies gradual degradation using linear regression on sliding windows.
- **RMS (Root Mean Square)**: Specifically for vibration analysis to detect bearing wear.
- **Skewness/Kurtosis**: Higher-order statistics for anomaly detection.
- **Frequency Domain Features**: FFT-based analysis for periodic fault detection.

## 5. Model Deployment
- **Containerized**: Docker images with pre-trained models for easy deployment.
- **Health Checks**: Automated validation of model inputs and outputs.
- **Fallback Mechanism**: Graceful degradation to physics-based models when ML fails.
- **A/B Testing**: Compare new models against baseline in production traffic.
