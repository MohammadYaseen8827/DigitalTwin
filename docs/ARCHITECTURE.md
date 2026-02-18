# Architecture View: Unified Digital Twin Platform

## Overview
The Digital Twin Platform is designed for Small and Medium Enterprises (SMEs) to monitor equipment health, predict failures, and optimize maintenance schedules without requiring a massive data science infrastructure. 

It combines **Real-time Telemetry**, **Mathematical Degradation Modeling**, and **Interpretable Machine Learning** into a unified ecosystem.

## System Components

### 1. Backend (.NET 9 Web API)
The core logic resides in a set of specialized services:
- **Simulation Engine**: Generates synthetic telemetry and simulates degradation using ODE solvers.
- **Analytics Engine**: Provides Remaining Useful Life (RUL) predictions and Anomaly Detection.
- **ML Pipeline**: Uses ML.NET for FastForest regression and Quantile regression (for confidence intervals).
- **Math Modeling**: Implements numerical methods like Runge-Kutta 4th Order and Euler-Maruyama for SDEs.
- **Synthetic Data Generator**: Creates physics-informed sensor data with statistical validation.
- **Alert Management**: Handles real-time notifications and maintenance recommendations.

### 2. Frontend (Vue 3 + TypeScript)
A premium dashboard designed for non-technical operators:
- **Real-time Monitoring**: Visualizes telemetry spikes and health trends with <100ms updates.
- **Predictive Horizons**: Shows future forecasts and confidence envelopes.
- **XAI (Explainable AI)**: Uses SHAP visualizations to show *why* a machine is predicted to fail.
- **Prescriptive Insights**: Recommends maintenance actions based on optimized cost-risk models.
- **Synthetic Data Interface**: Allows operators to generate and validate synthetic sensor data.
- **Mathematical Modeling Dashboard**: Interactive ODE system builder with real-time plotting.

### 3. Data Flow
1. **Devices/Simulation** → **Telemetry Hub** (SignalR) → **Storage** (PostgreSQL/JSONB) → **Real-time Analytics**.
2. **Telemetry** → **ML Predictor** → **RUL Prediction** with 95% confidence intervals.
3. **Historical Data** → **Drift Detection** → **Automated Retraining**.
4. **Synthetic Data Generator** → **Statistical Validation** → **Training Data Augmentation**.
5. **Mathematical Models** → **Parameter Estimation** → **Degradation Simulation**.

## Key Technologies
- **Framework**: .NET 9, ASP.NET Core.
- **ML**: ML.NET (FastForest, Quantile Regression, SHAP Explainer).
- **Numerical Math**: Custom ODE Solvers (RK4, Euler-Maruyama) with MathNet.Numerics.
- **Frontend**: Vue 3, Pinia, ECharts, TailwindCSS.
- **Database**: PostgreSQL with JSONB for flexible telemetry storage.
- **Real-time**: SignalR with connection pooling.
- **Validation**: Kolmogorov-Smirnov tests, Chi-square tests, Anderson-Darling tests.

## SME-Specific Logic
- **Synthetic Foundations**: Can run without historical data by using physics-informed degradation models to bootstrap ML training.
- **Interpretable ML**: Focuses on *why* (Feature Importance, SHAP values) to build operator trust.
- **Software-only**: No mandatory cloud dependencies; runs on local servers or Docker.
- **Statistical Validation**: Ensures synthetic data quality meets industry standards before use.
- **Performance Optimized**: <500ms end-to-end prediction latency, <100ms dashboard updates.
