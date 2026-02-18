## Title

A Modular, Simulation-Oriented Digital Twin Platform for Predictive and Prescriptive Maintenance in Small and Medium-Sized Enterprises Using Synthetic Data and Interpretable Machine Learning

---

## Abstract

Small and Medium-sized Enterprises represent the backbone of industrial economies, yet they remain largely excluded from advanced Industry 4.0 technologies. Predictive maintenance systems promise significant reductions in downtime, maintenance cost, and asset degradation, but their adoption in SMEs is constrained by structural limitations. These limitations include the absence of historical sensor data, high infrastructure and integration costs, and the technical complexity of existing solutions that assume mature data and IT ecosystems.

This thesis presents the design, implementation, and validation of a modular, simulation-oriented digital twin platform explicitly tailored to the operational realities of SMEs. The platform adopts a software-first approach, replacing mandatory physical sensor infrastructure with mathematically grounded degradation modeling and statistically validated synthetic data generation. Synthetic sensor data is used to train interpretable machine learning models implemented in ML.NET, enabling Remaining Useful Life estimation, fault classification, and uncertainty-aware prediction within a real-time digital twin simulation environment.

Beyond predictive maintenance, the platform is extended to support prescriptive maintenance decision support. Through simulation-based what-if analysis, the system evaluates maintenance timing, operational uncertainty, and cost trade-offs, transforming prognostic outputs into actionable maintenance recommendations. The architecture emphasizes modularity, configuration over code, reproducibility, and extensibility, allowing researchers and practitioners to experiment with machine types, degradation models, and maintenance strategies without modifying core system components.

The novelty of this work lies in the unified and rigorous integration of synthetic data generation, statistical validation, interpretable ML.NET models, and digital twin simulation within a single coherent architecture optimized for data-scarce SME environments. Validation is conducted using established benchmark datasets such as NASA C-MAPSS and the FEMTO Bearing Dataset, standard prognostic metrics including MAPE and RMSE, real-time performance constraints with latency below 500 milliseconds, and usability evaluation achieving a System Usability Scale score above 70. The resulting platform functions both as a research-grade experimental framework and as a practical, low-cost entry point for SMEs seeking to adopt predictive and prescriptive maintenance without prohibitive investment.

---

## 1. Introduction

Small and Medium-sized Enterprises form the majority of manufacturing and industrial organizations worldwide. Despite their economic importance, SMEs consistently lag behind large enterprises in the adoption of Industry 4.0 technologies. While large organizations deploy predictive maintenance systems supported by dense sensor networks, cloud-scale analytics, and specialized data science teams, SMEs operate under constraints of limited capital, sparse instrumentation, and reduced technical expertise.

Predictive maintenance aims to anticipate equipment failures before they occur by monitoring degradation processes and estimating Remaining Useful Life. Conventional predictive maintenance approaches assume continuous condition monitoring and large volumes of historical sensor data. These assumptions do not hold in typical SME environments, where equipment often operates without condition sensors and maintenance records are incomplete or inconsistent. As a result, SMEs rely primarily on reactive or time-based maintenance strategies, leading to unexpected failures, excessive spare-part usage, reduced asset lifespan, and production losses estimated between 5 and 20 percent of capacity.

Digital twin technology offers a conceptual framework for addressing these challenges by creating virtual representations of physical assets capable of simulating behavior, degradation, and failure mechanisms. However, most existing digital twin implementations remain hardware-centric and data-intensive, assuming real-time sensor streams and cloud infrastructure. Such designs implicitly exclude SMEs and fail to align with their operational constraints.

This research proposes a simulation-first digital twin platform in which synthetic sensor data generated from mathematically grounded degradation models replaces the need for extensive physical instrumentation. By integrating synthetic data generation, interpretable machine learning, and real-time simulation within a modular architecture, the platform enables predictive maintenance capabilities aligned with SME realities while preserving scientific rigor, transparency, and extensibility.

---

## 2. Problem Statement

### 2.1 Operational Misalignment

There exists a fundamental misalignment between existing predictive maintenance solutions and the operational realities of SMEs. Current systems are designed around assumptions of abundant data, mature IT infrastructure, and specialized expertise, none of which are typically available in SME contexts.

### 2.2 Identified Challenges

**Data Scarcity**
Most SMEs lack historical condition-monitoring data required to train prognostic models, as equipment is rarely instrumented with sensors from the outset.

**High Cost of Infrastructure**
IoT sensors, networking, cloud platforms, and specialized personnel impose significant upfront and recurring costs that exceed SME budgets.

**Technical Complexity**
Many predictive maintenance platforms are monolithic, difficult to configure, and require expertise in data science, industrial engineering, and system integration.

**Maintenance Inefficiency**
Reliance on reactive and scheduled maintenance results in
• 30–50 percent higher downtime
• premature component replacement
• reduced equipment lifespan
• production capacity losses of 5–20 percent

**Lack of SME-Focused Digital Twin Platforms**
Most digital twin frameworks target large enterprises and assume extensive resources, leaving SMEs without accessible alternatives.

### 2.3 Core Research Gap

There is a lack of lightweight, modular, simulation-driven digital twin platforms that operate under data scarcity, provide interpretable predictions, and remain economically and technically accessible to SMEs.

---

## 3. Research Goal and Objectives

### 3.1 Overall Goal

To design, implement, and validate a modular digital twin platform that enables predictive maintenance through Remaining Useful Life estimation and extends naturally to prescriptive maintenance decision support using simulation-based synthetic data and interpretable machine learning.

### 3.2 Research Objectives

**Foundational Objectives**
• Develop mathematically grounded degradation models using Wiener processes, exponential models, Markov and semi-Markov chains, and physics-informed surrogate models
• Generate high-quality synthetic IoT sensor data representing normal operation and multiple fault modes
• Statistically validate synthetic data using Kolmogorov–Smirnov tests, Maximum Mean Discrepancy, and autocorrelation analysis
• Implement ML.NET-based regression and classification models for RUL estimation and fault detection
• Achieve RUL prediction accuracy with Mean Absolute Percentage Error below 15 percent
• Integrate explainable AI techniques, including SHAP-based feature attribution
• Design a real-time digital twin simulation engine with update latency below 500 milliseconds
• Develop an intuitive Vue.js dashboard for non-technical operators, validated with a System Usability Scale score above 70

**Extended Objectives**
• Introduce uncertainty-aware RUL estimation with confidence intervals
• Extend predictive maintenance into prescriptive decision support through simulation
• Enable what-if analysis of maintenance strategies and operational scenarios
• Translate prognostic outputs into business impact metrics such as cost, risk, and downtime
• Support reproducible experimentation, model lineage, and lifecycle management

---

## 4. Scope and Limitations

### 4.1 Scope

The scope of this research includes
• Software-based digital twin simulation
• Synthetic data generation without mandatory physical sensors
• Prognostic analytics using ML.NET
• Real-time visualization using Vue.js and SignalR
• Persistent storage using PostgreSQL
• SME-scale, single-site deployment
• Decision-support-oriented usage rather than autonomous control

### 4.2 Limitations

• Physical IoT sensor integration is deferred to future work
• Validation is primarily simulation-based and benchmark-driven
• Initial implementation focuses on two to three representative machine types
• Industry-specific regulatory certification is not addressed
• Prescriptive maintenance recommendations remain advisory rather than autonomous

---

## 5. Theoretical Framework and Mathematical Foundations

### 5.1 Digital Twin Theory

The platform follows established digital twin principles by maintaining a continuous virtual representation of asset state, degradation, and maintenance actions. State evolution is driven by simulation and model inference rather than real-time sensor ingestion.

### 5.2 Prognostics and Health Management

Prognostics and Health Management concepts guide the modeling of degradation processes, estimation of Remaining Useful Life, handling of uncertainty, and evaluation of maintenance strategies.

### 5.3 Mathematical Degradation Models

**Wiener Processes** model stochastic degradation with drift and diffusion components.
**Exponential Degradation Models** represent accelerating failure dynamics.
**Physics-Informed Surrogate Models** encode domain knowledge and physical constraints.
**Markov and Semi-Markov Models** represent discrete and multi-stage degradation processes.

These models define ground truth degradation behavior, generate synthetic sensor data, and constrain machine learning predictions to physically plausible regimes.

---

## 6. System Architecture and Methodology

### 6.1 Architectural Overview

The platform adopts a modular, domain-oriented architecture consisting of
• Synthetic Data Generation Domain
• Digital Twin Simulation Core
• Prognostic Analytics Domain
• Interpretability Layer
• Data and API Layer
• Web-Based Visualization Layer

An event-driven interaction model enables loose coupling, scalability, and independent evolution of system components.

### 6.2 Synthetic Data Generation and Validation

Machine behavior and degradation processes are defined through JSON-based configuration.
Run-to-failure simulations generate diverse degradation trajectories.
Synthetic data is statistically validated against real benchmark datasets.
Validation ensures distributional similarity, temporal coherence, and prognostic relevance.

### 6.3 Machine Learning and Interpretability

ML.NET regression and classification models provide RUL estimation and fault detection.
Confidence intervals quantify predictive uncertainty.
SHAP-based feature attribution explains model behavior.
Drift detection monitors divergence between simulation, training data, and inference outputs.
Automated retraining triggers maintain model relevance.

### 6.4 Prescriptive Maintenance Extension

Prescriptive maintenance is implemented as an additive layer.
Maintenance actions are evaluated through simulation.
Delayed versus immediate intervention strategies are compared.
Costs, risks, and downtime are quantified.
Predictions are translated into decision-relevant metrics for operators and managers.

---

## 7. Validation and Evaluation

**Preserved Evaluation Criteria**
• Prognostic accuracy using MAPE, RMSE, and R²
• System latency and throughput
• End-to-end integration testing
• Usability evaluation using SUS

**Extended Evaluation**
• Scenario-based maintenance optimization
• Comparative analysis against reactive and scheduled maintenance
• Cost-benefit and risk trade-off assessment
• Sensitivity analysis under uncertainty and model error

---

## 8. Expected Contributions

### 8.1 Scientific Contributions

• Hybrid modeling framework for prognostics under data scarcity
• Rigorous methodology for synthetic data validation
• Operationalization of explainable AI within digital twin systems

### 8.2 Technical Contributions

• Modular, extensible digital twin platform
• Reproducible experimentation and model lifecycle infrastructure
• Prescriptive maintenance simulation engine
• Interpretable ML.NET deployment patterns for industrial systems

### 8.3 Practical Contributions

• Low-cost predictive and prescriptive maintenance for SMEs
• Reduced downtime and maintenance waste
• Improved trust in AI-assisted maintenance decisions
• Clear demonstration of return on investment prior to hardware deployment

---

## 9. Conclusion

This thesis integrates simulation-based predictive maintenance with decision-oriented prescriptive capabilities in a unified digital twin platform designed for SMEs. Mathematical rigor is preserved through explicit degradation modeling. Interpretability remains central to user trust. Prescriptive logic is layered incrementally rather than imposed.

The resulting system predicts failure, evaluates maintenance decisions, and supports long-term evolution without reliance on large-scale infrastructure. It aligns academic depth with industrial constraints and prepares SMEs for maintenance intelligence through software rather than scale.

The platform is designed to answer critical questions before deployment.
Will operators trust the recommendations.
Will the system learn from incorrect decisions.
Will it scale without rewriting its core.

This work addresses those questions by design.
