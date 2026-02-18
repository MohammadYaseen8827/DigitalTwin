<!--
Sync Impact Report:
- Version change: 1.0.0 → 1.1.0 (Added SME-focused principles and predictive maintenance requirements)
- Modified principles: Updated all principles to reflect SME constraints and PdM focus
- Added sections: Mathematical Modeling Requirements, SME Accessibility Standards
- Templates requiring updates: ✅ plan-template.md (Constitution Check section), ✅ spec-template.md (requirements alignment), ✅ tasks-template.md (task categorization)
- Follow-up TODOs: None - all proposal requirements integrated
-->

# Digital Twin Platform Constitution for Predictive Maintenance

## Core Principles

### I. SME-First Accessibility (NON-NEGOTIABLE)
Platform MUST be designed for resource-constrained SMEs (10-250 employees); MUST operate without extensive sensor infrastructure; MUST require minimal technical expertise; MUST provide low-cost entry point with software-only deployment; MUST ensure intuitive interfaces for non-technical operators (System Usability Scale >70).

### II. Predictive Maintenance Focus
System MUST specialize in Remaining Useful Life (RUL) prediction with <15% MAPE accuracy; MUST support fault classification with interpretable explanations; MUST target rotating/reciprocating machinery (motors, pumps, compressors, gearboxes, bearings); MUST deliver projected 30-45% downtime reduction and 20-35% maintenance cost savings.

### III. Mathematical Modeling Rigor
All degradation models MUST be mathematically grounded; MUST implement Wiener processes, exponential degradation, physics-informed surrogate models, and Markov chains; MUST include statistical validation against benchmark datasets (NASA C-MAPSS, FEMTO); MUST provide confidence intervals for all predictions.

### IV. Synthetic Data Validation
System MUST generate high-fidelity synthetic IoT sensor data; MUST validate statistical fidelity using Kolmogorov-Smirnov tests, MMD, and autocorrelation analysis; MUST compare synthetic vs real data performance; MUST support vibration, temperature, and load sensor simulation.

### V. Real-Time Telemetry (NON-NEGOTIABLE)
All telemetry data MUST flow through SignalR for real-time updates; Telemetry Service MUST persist to JSONB for analytics; Predictive analytics MUST update twin state within 500ms; Dashboard MUST reflect changes within 100ms for operator decision-making.

### VI. Integration Testing Excellence
Focus areas requiring integration tests: Telemetry pipeline end-to-end, Synthetic data validation workflows, ML.NET prediction accuracy, SignalR broadcast reliability, Mathematical degradation model verification, User interface usability testing.

### VII. Interpretable ML & Observability
ML.NET models MUST provide feature importance and explanations; All operations MUST be traceable with correlation IDs; Structured logging MUST include tenant context and prediction confidence; API versioning follows MAJOR.MINOR format with breaking change documentation.

## Technology Stack Requirements

### Backend Infrastructure
- .NET 8 Web API with Entity Framework Core 8 (upgraded from .NET 7 per proposal)
- PostgreSQL 15+ with JSONB for flexible machine configuration storage
- ML.NET for interpretable predictive analytics (tree-based ensembles with feature importance)
- SignalR for real-time telemetry broadcasting (<500ms prediction latency)
- MathNet.Numerics for mathematical modeling (Wiener processes, statistical validation)

### Frontend Requirements
- Vue.js 3 with Composition API and TypeScript (optimal for SME constraints)
- Pinia for state management (simpler than Vuex for non-technical users)
- ApexCharts/Chart.js for time-series visualization and RUL forecasts
- Responsive design targeting bandwidth-constrained SME deployments
- System Usability Scale >70 validation required for all interfaces

### Mathematical & Scientific Computing
- MathNet.Numerics for stochastic processes and statistical analysis
- Custom implementation of degradation models (Wiener, Exponential, Physics-informed)
- Statistical validation toolkit (Kolmogorov-Smirnov, MMD, autocorrelation)
- Benchmark dataset integration (NASA C-MAPSS, FEMTO bearing dataset)

### Deployment & Operations
- Docker containerization with multi-stage builds for SME deployment
- Environment-based configuration management
- Comprehensive health checks and performance monitoring
- Standalone software deployment (no cloud dependency for SME accessibility)

## SME Accessibility Standards

### User Experience Requirements
- Target users: Non-technical maintenance personnel in SMEs (10-250 employees)
- Interface complexity MUST be minimized for operators without data science expertise
- All predictive insights MUST include actionable, plain-language explanations
- System Usability Scale (SUS) score MUST exceed 70 for all user interfaces
- Training materials MUST be comprehensive for self-sufficient operation

### Resource Constraints
- MUST operate without extensive sensor infrastructure (synthetic data approach)
- MUST deploy on standard SME hardware without cloud dependencies
- MUST minimize bandwidth requirements for remote SME locations
- MUST provide offline capability for intermittent connectivity scenarios
- MUST support gradual adoption with modular configuration

### Economic Accessibility
- Software-only deployment with minimal upfront investment
- JSON-based machine configuration for non-programmer customization
- Clear ROI demonstration through simulation before real deployment
- Projected maintenance cost reduction of 20-35% MUST be achievable
- Unplanned downtime reduction of 30-45% MUST be demonstrable

## Mathematical Modeling Requirements

### Degradation Model Implementation
- Wiener Process: X(t) = X₀ + μt + σB(t) for continuous degradation
- Exponential Degradation: X(t) = α(e^(βt) - 1) + ε(t) for accelerating failure
- Physics-Informed Models: dX/dt = f(L, N, σ_contact, T) + noise for specific equipment
- Markov Chains: P(S_{t+1} = j | S_t = i) = p_ij for discrete state transitions

### Validation Requirements
- Statistical validation against NASA C-MAPSS and FEMTO benchmark datasets
- Kolmogorov-Smirnov tests for distribution similarity
- Maximum Mean Discrepancy (MMD) for synthetic data quality
- Autocorrelation analysis for time-series fidelity
- Cross-validation with <15% MAPE for RUL prediction accuracy

### Performance Targets
- RUL prediction accuracy: <15% Mean Absolute Percentage Error
- Prediction update latency: <500ms for real-time decision support
- Fault classification accuracy: >85% for common failure modes
- Confidence interval coverage: 95% for all probabilistic predictions
- Synthetic data generation throughput: >1000 samples/second

## Development Workflow

### Code Quality Standards
All code MUST pass static analysis before merge; Unit tests required for all business logic; Integration tests for cross-component interactions; Code reviews MUST verify constitution compliance.

### Feature Development Process
Features MUST start with specification in `.specify/`; Implementation MUST follow plan-template.md structure; Tasks MUST be organized by user story priority; Each story MUST be independently testable and deployable.

### Testing Strategy
Test-First Development mandatory for new features; Contract tests for all API endpoints; Integration tests for synthetic data validation; Performance tests for <500ms prediction latency; Usability tests for SUS >70 compliance; Statistical validation tests for mathematical models.

## Governance

This constitution supersedes all other development practices; Amendments require documented justification, team approval, and migration plan; All pull requests MUST verify compliance; Complexity beyond these principles MUST be explicitly justified; Use project README and docs for runtime development guidance.

**Version**: 1.1.0 | **Ratified**: 2025-02-08 | **Last Amended**: 2025-02-08
