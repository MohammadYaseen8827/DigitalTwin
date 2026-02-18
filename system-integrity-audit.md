# System Integrity Audit Report

**Project:** Digital Twin Platform (ASP.NET Core + Vue.js)  
**Audit Date:** 2025-02-14  
**Scope:** Backend API, Frontend Vue app, contract alignment, implementation integrity

---

## 1. Backend Summary

### 1.1 Backend Feature Map

#### Controller: AuthController
- **Route:** `api/auth`
- **Endpoints:**
  - `POST api/auth/register` — Request: RegisterRequest | Response: RegisterResponse | Auth: AllowAnonymous (Register only) | **Used In Frontend:** Yes
  - `POST api/auth/login` — Request: LoginRequest | Response: TokenResponse | Auth: AllowAnonymous | **Used In Frontend:** Yes
  - `POST api/auth/refresh` — Request: RefreshTokenRequest | Response: TokenResponse | **Used In Frontend:** Yes (api interceptor + auth store)
  - `POST api/auth/revoke` — Request: RefreshTokenRequest | Response: object | **Used In Frontend:** No
  - `GET api/auth/me` — Response: CurrentUserResponse | **Used In Frontend:** Yes
  - `PUT api/auth/profile` — Request: UpdateProfileRequest | Response: CurrentUserResponse | **Used In Frontend:** Yes
  - `POST api/auth/change-password` — Request: ChangePasswordRequest | **Used In Frontend:** Yes
  - `POST api/auth/forgot-password` — Request: ForgotPasswordRequest | **Used In Frontend:** Yes
  - `POST api/auth/reset-password` — Request: ResetPasswordRequest | **Used In Frontend:** Yes
  - `POST api/auth/logout` — **Used In Frontend:** Yes
- **Service:** UserManager, RefreshTokenService. **2FA endpoints (enable/verify/disable):** Not implemented; frontend calls them → **404**.

#### Controller: AlertsController
- **Route:** `api/alerts`
- **Endpoints:**
  - `GET api/alerts` — Query: machineId? | Response: IEnumerable&lt;AlertDto&gt; | **Used In Frontend:** Yes
  - `GET api/alerts/{id}` — Response: AlertDto | **Used In Frontend:** No
  - `PUT api/alerts/{id}/acknowledge` — Response: 204 | **Used In Frontend:** Yes
  - `DELETE api/alerts/{id}` — Response: 204 | **Used In Frontend:** Yes
  - `GET api/alerts/all` — Query: machineId? | Response: IEnumerable&lt;AlertDto&gt; | **Used In Frontend:** No
  - `GET api/alerts/stats` — Response: AlertStatsDto | **Used In Frontend:** No
- **Service:** IAlertService (AlertService). All interface methods have implementation.

#### Controller: MachinesController
- **Route:** `api/machines`
- **Endpoints:**
  - `GET api/machines` — Response: IEnumerable&lt;MachineDto&gt; | **Used In Frontend:** Yes
  - `GET api/machines/{id}` — Response: MachineDto | **Used In Frontend:** Yes
  - `POST api/machines` — Request: MachineCreateDto | Response: MachineDto | **Used In Frontend:** No
  - `PUT api/machines/{id}` — Request: MachineUpdateDto | Response: MachineDto | **Used In Frontend:** No
  - `DELETE api/machines/{id}` — Response: 204 | **Used In Frontend:** No
- **Service:** MediatR (GetMachinesQuery, GetMachineQuery, CreateMachineCommand, UpdateMachineCommand, DeleteMachineCommand).

#### Controller: DashboardController
- **Route:** `api/dashboard`
- **Endpoints:**
  - `GET api/dashboard/stats` — Response: DashboardStatsDto | **Used In Frontend:** Yes
- **Service:** IAlertService, IMachineRepository.

#### Controller: TelemetryController
- **Route:** `api/telemetry`
- **Endpoints:**
  - `POST api/telemetry` — Request: TelemetryIngestDto | Response: 202 | **Used In Frontend:** No (ingestion only)
  - `GET api/telemetry/{machineId}` — Query: range?, take | Response: IEnumerable&lt;TelemetryDto&gt; | **Used In Frontend:** No
  - `GET api/telemetry/recent` — Query: range?, machineId?, limit | **Used In Frontend:** No
  - `GET api/telemetry/{machineId}/latest` — Response: TelemetryDto | **Used In Frontend:** Yes
- **Service:** MediatR (IngestTelemetryCommand, GetTelemetryForMachineQuery, GetRecentTelemetryQuery).

#### Controller: PredictionsController
- **Route:** `api/predictions`
- **Endpoints:**
  - `POST api/predictions/rul/{machineId}` — Response: RulPredictionResult | **Used In Frontend:** Yes (machines store)
  - `GET api/predictions/health/{machineId}` — Response: HealthClassificationResult | **Used In Frontend:** No
  - `GET api/predictions/rul/{machineId}/summary` — Response: object | **Used In Frontend:** No
  - `POST api/predictions/train` — Request: TrainModelRequestDto? | Response: TrainingResultDto | **Used In Frontend:** No
  - `POST api/predictions/ai/train` — **Used In Frontend:** No
  - `GET api/predictions/status` — Response: ModelStatusDto | **Used In Frontend:** No
- **Service:** IMediator, IRulPredictor, IHealthClassifier, ITelemetryRepository, IFeatureExtractionService.
- **Missing from backend:** `GET api/predictions` (list), `GET api/predictions/rul/{machineId}` (frontend predictions store uses GET; backend only has POST), `GET api/predictions/anomaly/{machineId}`. Frontend calls these → **404 / method mismatch**.

#### Other controllers (not called by current frontend)
- HealthController, TokenController, SearchController, RunToFailureController, AdvancedAnalyticsController, SyntheticDataController, MaintenanceController, AlertRulesController, ExternalSystemsController, TenantsController, WorkflowController, AIModelController, ReportsController, MathematicalModelingController, UncertaintyController, DriftController, AntiForgeryController, BenchmarkValidationController, ModelLifecycleController, PerformanceMetricsController, MachineConfigurationController, PrescriptiveController, SimulationController, AzureDigitalTwinController, DataArchivalController, ProductionLinesController, DegradationModelingController.

### 1.2 Services and Interfaces

- **IAlertService** → AlertService (implemented).
- **IRulPredictor** → RULPredictor (implemented).
- **IHealthClassifier** → HealthClassifier (implemented).
- **RefreshTokenService** (Auth) — used by AuthController.
- **Mock/placeholder in production:** MockExternalSystemService, MockTenantService, TelemetryMockHostedService registered in production DI. NotificationService contains placeholder/mock email logic. DataDriftService uses dummy reference data. ReportsController uses mock content/size. IAIService and workflow handlers return mock data. FeatureImportanceExtractor / ShapExplainer / FastForestPredictor use mock importance/SHAP in places.

---

## 2. Frontend Summary

### 2.1 Frontend API Usage Map

| Service Method (Store/Usage) | URL | HTTP Method | Used In Components | Backend Match | DTO Compatible |
|------------------------------|-----|-------------|---------------------|---------------|----------------|
| fetchMachines | `/api/machines` | GET | Machines view, dashboard, etc. | Yes | No (see contract table) |
| fetchMachineById | `/api/machines/{id}` | GET | Machine detail | Yes | No |
| fetchDashboardStats | `/api/dashboard/stats` | GET | Dashboard | Yes | Yes |
| fetchMachineMetrics | `/api/telemetry/{machineId}/latest` | GET | Machine cards, charts | Yes | No (TelemetryDto vs TelemetryMetrics) |
| fetchRULPrediction (machines) | `/api/predictions/rul/{machineId}` | POST | Machine RUL display | Yes | Partial (RulPredictionResult vs RULPrediction) |
| fetchAlerts | `/api/alerts` or `/api/alerts?machineId=` | GET | Alerts view, panels | Yes | Partial (AlertDto missing AcknowledgedBy, RelatedPredictionId) |
| acknowledgeAlert | `/api/alerts/{id}/acknowledge` | PUT | Alert list | Yes | N/A (204) |
| resolveAlert | `/api/alerts/{id}` | DELETE | Alert list | Yes | N/A (204) |
| login | `/api/auth/login` | POST | LoginForm | Yes | No (User.fullName vs name; see token naming) |
| register | `/api/auth/register` | POST | RegisterForm | Yes | No (FullName vs name; backend no confirmPassword/acceptTerms) |
| refresh (interceptor) | `/api/auth/refresh` | POST | api.ts interceptor | Yes | **No – backend returns accessToken; frontend expects token** |
| checkAuth | `/api/auth/me` | GET | App init | Yes | No (CurrentUserResponse.Name vs User.name; backend has no avatar/lastLogin) |
| updateProfile | `/api/auth/profile` | PUT | UserProfileSettings | Yes | Partial |
| requestPasswordReset | `/api/auth/forgot-password` | POST | PasswordResetForm | Yes | Yes |
| resetPassword | `/api/auth/reset-password` | POST | PasswordResetForm | No (backend expects email in body; frontend sends token, password, confirmPassword only) | No |
| logout | `/api/auth/logout` | POST | Header/session | Yes | Yes |
| changePassword | `/api/auth/change-password` | POST | UserProfileSettings | Yes | Yes |
| enableTwoFactor | `/api/auth/2fa/enable` | POST | UserProfileSettings | **No – endpoint does not exist** | N/A |
| verifyTwoFactor | `/api/auth/2fa/verify` | POST | UserProfileSettings | **No – endpoint does not exist** | N/A |
| disableTwoFactor | `/api/auth/2fa/disable` | POST | UserProfileSettings | **No – endpoint does not exist** | N/A |
| fetchPredictions | `/api/predictions` or `?machineId=` | GET | Predictions view | **No – endpoint does not exist** | N/A |
| fetchRULPrediction (predictions) | `/api/predictions/rul/{machineId}` | GET | Predictions store | **No – backend only has POST** | N/A |
| fetchAnomalyPrediction | `/api/predictions/anomaly/{machineId}` | GET | Predictions store | **No – endpoint does not exist** | N/A |

### 2.2 Frontend API client

- Single `api` client in `src/services/api.ts` (axios); base URL from `VITE_API_BASE_URL` or `/api`.
- Auth refresh interceptor uses `response.data.token`; backend returns `accessToken` → refresh will not set token correctly and subsequent requests may fail with 401.

---

## 3. Contract Validation (DTO vs Frontend)

| DTO Name | Backend Field | Frontend Field | Type Match | Issue |
|----------|---------------|----------------|-----------|--------|
| TokenResponse (login/refresh) | AccessToken | token (expected by interceptor) | No | Interceptor expects `token`; backend returns `accessToken`. Refresh flow broken. |
| TokenResponse | RefreshToken | refreshToken | Yes | |
| UserInfo / CurrentUserResponse | FullName / Name | name | Partial | Login response uses UserInfo.FullName → JSON `fullName`. Frontend User expects `name`. |
| CurrentUserResponse | Id, Email, Name, Role, TwoFactorEnabled, CreatedAt | User: id, email, name, avatar?, role, twoFactorEnabled, lastLogin?, createdAt | Partial | Backend has no avatar, lastLogin. Name vs fullName in login. |
| RegisterRequest | Email, Password, FullName | RegisterData: name, email, password, confirmPassword, acceptTerms | No | Backend has no confirmPassword, acceptTerms; uses FullName not name. |
| ResetPasswordRequest | Email, Token, Password | Frontend sends: token, password, confirmPassword | No | Backend requires Email in body; frontend does not send it. Reset will fail. |
| MachineDto | Id (Guid), Name, Type, Status, Properties (string), RemainingUsefulLifeDays, FailureProbability, HealthStatus, CreatedAt, UpdatedAt | Machine: id (string), name, type, location, status, healthScore, lastMaintenanceDate, nextMaintenanceDate, installDate, specifications (Record) | No | Backend has no location, healthScore, lastMaintenanceDate, nextMaintenanceDate, installDate. Properties is JSON string; frontend expects specifications object. Status values differ (e.g. Operational vs Running). |
| TelemetryDto | Id, MachineId, DataType, Data (JsonDocument), Timestamp | TelemetryMetrics: machineId, temperature, vibration, pressure, humidity?, etc., lastUpdated | No | Backend returns Data as JSON document; frontend expects flat TelemetryMetrics. Structure mismatch. |
| AlertDto | Id, MachineId, Message, Severity, CreatedAt, IsAcknowledged, Category?, RecommendedAction? | BackendAlertDto: + acknowledgedBy?, relatedPredictionId? | Partial | AlertDto.FromEntity does not map AcknowledgedBy or RelatedPredictionId (entity has them). Frontend optionally uses them. |
| AlertDto | Severity (enum: Info, Warning, Critical, Emergency) | severity: 'info' \| 'warning' \| 'critical' \| 'error' | Partial | Backend has no 'error'; frontend has 'error'. Case difference (backend PascalCase in JSON unless camelCase configured). |
| RulPredictionResult | MachineId, Rul, RulUnit, Confidence, LowerBound, UpperBound, PredictionTime, ContributingFactors, ModelVersion, ProcessingTimeMs | RULPrediction: currentRUL, predictedRUL, confidenceLower, confidenceUpper, confidenceScore, degradationRate, trend, estimatedFailureDate, modelType, lastUpdated | Partial | Backend has single Rul and bounds; frontend expects currentRUL, predictedRUL, trend, estimatedFailureDate, modelType. Missing fields on one side or the other. |
| DashboardStatsDto | TotalMachines, RunningMachines, MaintenanceMachines, ErrorMachines, AverageHealthScore, ActiveAlerts, PendingMaintenance | DashboardStats (same shape) | Yes | |

---

## 4. Implementation Integrity Check

### 4.1 Interface implementations

- IAlertService, IRulPredictor, IHealthClassifier, RefreshTokenService, MediatR handlers and other registered interfaces have concrete implementations. No unimplemented interface methods identified.

### 4.2 Controller → service

- Controllers used by the frontend call services/MediatR as expected. No controller action missing a service call.

### 4.3 Async/await

- No obvious missing `await` on async calls in the audited paths.

### 4.4 Empty or minimal catch blocks

- **PrescriptiveAnalyticsService:** `catch { /* Use default criticality */ }` and `catch { /* Use default */ }` — swallow exceptions with no logging.
- **HealthController:** catch returns "Unhealthy" with no log.
- **FeatureExtractionService, ShapServiceClient, MachineConfigurationService, AIModelCommandHandlers, ConfigurationExtensions, Domain User:** catch blocks return default/fallback with no logging (or rethrow in command handlers). Not all empty but several are minimal and hide failures.

### 4.5 TODO / FIXME / placeholder / mock in production code

- **Backend:** MockExternalSystemService and MockTenantService registered in production. TelemetryMockHostedService in production. NotificationService: placeholder email/SMTP. DataDriftService: dummy reference data. ReportsController: mock file content/size/templates. IAIService, WorkflowCommandHandlers/QueryHandlers, AIModelCommandHandlers/QueryHandlers: mock returns. FeatureImportanceExtractor, ShapExplainer, FastForestPredictor: mock importance/SHAP. SyntheticDataGenerator: mock validation report. MetricsConfiguration: placeholders.
- **Frontend:** Alerts.vue references "Mock rules for demo (no backend endpoint yet)". RulTimelineChart, PredictionVsActualChart, AlertsTimelineChart, SensorDistributionChart, HealthTrendChart use generateMockData() when no real data — acceptable for charts but effectively mock data in UI.

### 4.6 Dependency injection

- All controller and service dependencies referenced in the audited code are registered in ServiceCollectionExtensions. No unresolved DI for used endpoints.

---

## 5. Dead Code / Unused Endpoints

### 5.1 Backend endpoints not used by frontend

- Auth: `POST api/auth/revoke`
- Alerts: `GET api/alerts/{id}`, `GET api/alerts/all`, `GET api/alerts/stats`
- Machines: `POST api/machines`, `PUT api/machines/{id}`, `DELETE api/machines/{id}`
- Telemetry: `POST api/telemetry`, `GET api/telemetry/{machineId}`, `GET api/telemetry/recent`
- Predictions: `GET api/predictions/health/{machineId}`, `GET api/predictions/rul/{machineId}/summary`, `POST api/predictions/train`, `POST api/predictions/ai/train`, `GET api/predictions/status`
- All other controllers (Health, Token, Search, RunToFailure, AdvancedAnalytics, SyntheticData, Maintenance, AlertRules, ExternalSystems, Tenants, Workflow, AIModel, Reports, MathematicalModeling, Uncertainty, Drift, AntiForgery, BenchmarkValidation, ModelLifecycle, PerformanceMetrics, MachineConfiguration, Prescriptive, Simulation, AzureDigitalTwin, DataArchival, ProductionLines, DegradationModeling) have no usage in the current frontend.

### 5.2 Frontend calls to non-existent or wrong-method endpoints

- `GET /api/predictions` and `GET /api/predictions?machineId=` — no such endpoint.
- `GET /api/predictions/rul/{machineId}` — backend only has `POST`; predictions store uses GET → 404/method not allowed.
- `GET /api/predictions/anomaly/{machineId}` — no such endpoint (anomaly exists under AdvancedAnalytics as POST anomaly-detection/{machineId}).
- `POST /api/auth/2fa/enable`, `POST /api/auth/2fa/verify`, `POST /api/auth/2fa/disable` — not implemented on backend → 404.

---

## 6. Critical Errors

1. **Auth refresh token handling (api.ts):** Interceptor expects `response.data.token`; backend returns `accessToken`. After refresh, token is not stored and authenticated requests can fail. **Must fix for production.**
2. **Password reset:** Backend ResetPasswordRequest requires `Email` in body; frontend only sends token, password, confirmPassword. Reset will fail. **Must fix.**
3. **2FA endpoints:** Frontend calls enable/verify/disable 2FA; backend has no routes. Results in 404 and broken 2FA flow. **Either implement or remove from frontend.**
4. **Predictions list and RUL GET:** Frontend predictions store uses GET for list and for RUL; backend has no GET list and only POST for RUL. **Implement GET list and/or GET RUL, or change frontend to POST for RUL and remove list call.**
5. **Anomaly prediction:** Frontend calls GET `/api/predictions/anomaly/{machineId}`; endpoint does not exist. **Implement or remove usage.**
6. **Machine DTO mismatch:** Backend MachineDto does not match frontend Machine (e.g. status, location, healthScore, specifications). **Align DTOs or add mapping layer.**
7. **Login/me user shape:** Backend returns `fullName` (UserInfo) or `name` (CurrentUserResponse); frontend expects `name`. **Unify naming and ensure camelCase contract.**
8. **AlertDto:** Does not expose AcknowledgedBy, RelatedPredictionId even though entity has them. **Extend DTO and FromEntity for consistency.**

---

## 7. Refactoring Recommendations

1. **API client (api.ts):** Use `response.data.accessToken` (or a single contract field) in the refresh interceptor and store it where the rest of the app expects the token (e.g. same key as login). Optionally align backend to also return `token` for compatibility.
2. **Auth contracts:** Align login/me response: use one property name for display name (e.g. `name`) and document it. Add 2FA endpoints or remove 2FA UI until backend is ready.
3. **Reset password:** Backend should accept token + new password (and optionally email for lookup). Frontend should send the same; add email if backend keeps it required.
4. **Predictions API:** Add `GET api/predictions` (list) and `GET api/predictions/rul/{machineId}` if the product needs them; otherwise change frontend to use POST for RUL only and remove list/anomaly calls or point anomaly to AdvancedAnalytics POST.
5. **Machine/Telemetry/Alert DTOs:** Extend backend DTOs (or add view models) to match frontend needs (e.g. location, healthScore, specifications, AcknowledgedBy, RelatedPredictionId). Add mapping if backend model differs.
6. **RUL contract:** Define a single RUL response shape (backend + frontend) with currentRUL, predictedRUL, bounds, trend, estimatedFailureDate, etc., and implement mapping on backend or frontend.
7. **Mock/placeholder:** Replace MockExternalSystemService, MockTenantService, and TelemetryMockHostedService (or make them opt-in by environment). Replace placeholder/mock logic in NotificationService, DataDriftService, ReportsController, and ML/workflow mock returns with real implementations or feature flags.
8. **Error handling:** Replace minimal/empty catch blocks (e.g. PrescriptiveAnalyticsService, HealthController) with logging and structured error responses where appropriate.
9. **Register contract:** Add confirmPassword and acceptTerms to backend if registration flow requires them; align FullName vs name.

---

## 8. Production Readiness Verdict

**Verdict: FAIL**

**Reasons:**

- **Critical:** Auth refresh uses wrong property (`token` vs `accessToken`), breaking token refresh and potentially all authenticated requests after refresh.
- **Critical:** Password reset contract mismatch (missing email in request); reset flow will not work.
- **Critical:** Frontend calls non-existent or wrong-method endpoints (predictions list, GET RUL, anomaly GET, 2FA), leading to 404/method not allowed and broken features.
- **High:** Significant DTO mismatches (Machine, Telemetry, Alert, User, RUL) risk runtime errors or wrong UI data.
- **High:** Production DI and code use mocks/placeholders (external systems, tenants, telemetry mock, notifications, reports, ML/workflow mocks), which is not acceptable for production without clear boundaries and real implementations.
- **Medium:** Several catch blocks hide errors; dead endpoints and unused frontend calls increase maintenance and confusion.

**Recommendation:** Do not approve for production deployment until:

1. Auth refresh and reset-password contracts and client code are fixed and tested.
2. Predictions and 2FA are either implemented on backend and aligned with frontend or removed from frontend.
3. Machine, Telemetry, Alert, and User contracts are aligned or consistently mapped.
4. Mock/placeholder services are replaced or gated by environment/feature flags and documented.

---

*End of System Integrity Audit Report*
