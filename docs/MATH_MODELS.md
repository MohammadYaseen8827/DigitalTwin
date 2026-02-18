# Mathematical Foundations: Simulation & Degradation

## 1. Numerical Integration
The platform implements rigid numerical solvers to simulate physics-informed machine aging.

### Runge-Kutta 4th Order (RK4)
- **Implementation**: `src/api/DigitalTwinPlatform.Application/Mathematics/RungeKutta.cs`.
- **Use Case**: Solving the Ordinary Differential Equations (ODEs) that govern heat dissipation and mechanical wear.
- **Accuracy**: Provides $O(h^4)$ local truncation error, making it suitable for stable long-term simulations of machine health.
- **Stability Analysis**: Built-in energy conservation checks and system stability verification.

### Euler-Maruyama
- **Implementation**: `src/api/DigitalTwinPlatform.Application/Mathematics/EulerMaruyama.cs`.
- **Use Case**: Stochastic Differential Equations (SDEs).
- **Purpose**: Adds "Real-world Noise" to mathematical models to simulate environmental fluctuations and measurement uncertainty.
- **Applications**: Bearing degradation, thermal cycling, and vibration analysis.

## 2. Degradation Models
We support four primary styles of degradation modeling:

| Model Type | Description | Best For |
|------------|-------------|----------|
| **Wiener Process** | Brownian motion with drift. | Bearings, cracks, and accumulative wear. |
| **Exponential** | Decay that accelerates over time. | High-speed motors and thermal runaway scenarios. |
| **Markov Chain** | Discrete state transitions (Healthy -> Warning -> Fail). | High-level executive reporting and spare parts planning. |
| **Physics-Informed** | Custom ODE sets (e.g., $dH/dt = -k \cdot H + Noise$). | Specific equipment types where dynamics are well-understood. |

## 3. Parameter Estimation
- **Mechanism**: Grid search and Least Squares optimization.
- **Goal**: Fits mathematical parameters (like degradation rate $\lambda$) to observed historical data, bridging the gap between math and reality.
- **Implementation**: `src/api/DigitalTwinPlatform.Application/Services/ParameterEstimationService.cs`

## 4. Interactive Modeling Dashboard
- **Feature**: Drag-and-drop ODE system builder with real-time ECharts plotting.
- **Controls**: Adjustable parameters, initial conditions, and noise levels.
- **Analysis**: Stability analysis, energy conservation checks, and bifurcation detection.
- **Export**: Save/load model configurations as JSON templates.
