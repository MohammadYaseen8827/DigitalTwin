# Prioritized Fix Plan (by Production Risk)

**Source:** System Integrity Audit Report  
**Order:** Critical → High → Medium → Low  
**Goal:** Address issues before production deployment.

---

## CRITICAL (P0) — Fix before any production deployment

These break core flows (auth, reset, predictions) and will cause immediate user-facing failures.

---

### P0-1. Auth refresh token property mismatch

| Field | Value |
|-------|--------|
| **Risk** | Critical |
| **Impact** | After first token expiry, refresh returns `accessToken` but interceptor expects `token`. Token is not stored; all subsequent API calls fail with 401. Users are effectively logged out on refresh. |
| **Area** | Frontend: `src/frontend/src/services/api.ts` |
| **Effort** | Small (single file) |

**Steps:**

1. In the 401 response interceptor, read `response.data.accessToken` (and optionally `response.data.refreshToken`) instead of `response.data.token`.
2. Store the value using the same key the rest of the app uses for the access token (e.g. `localStorage.setItem(TOKEN_KEY, response.data.accessToken)`).
3. Set `originalRequest.headers.Authorization` using that stored value.
4. Optionally: add a type for the refresh response (`{ accessToken: string; refreshToken?: string }`) and use it for the interceptor.

**Acceptance:** After token expiry, a refresh call succeeds, the new token is stored, and the retried request succeeds with 401 no longer recurring for valid sessions.

---

### P0-2. Password reset request contract

| Field | Value |
|-------|--------|
| **Risk** | Critical |
| **Impact** | Backend expects `Email` in body; frontend sends only `token`, `password`, `confirmPassword`. Reset always fails. |
| **Area** | Backend: `AuthController` / `ResetPasswordRequest`; Frontend: `stores/auth.ts`, `PasswordResetForm.vue` |
| **Effort** | Small |

**Steps:**

1. **Option A (recommended):** Backend accepts reset by token only (look up user by token). Change `ResetPasswordRequest` to require `Token`, `Password`; make `Email` optional for lookup if needed. Frontend continues sending token + password (and confirmPassword client-side only).
2. **Option B:** Keep backend requiring `Email`. Frontend: capture email on the same screen as token (or from the forgot-password step) and send `email`, `token`, `password` in the reset request. Align frontend type with backend DTO.
3. Ensure backend validation and error messages match the chosen contract.

**Acceptance:** User can complete “forgot password” → “reset password” flow; backend resets password and returns success when token and password are valid.

---

### P0-3. Predictions store: wrong method and missing endpoints

| Field | Value |
|-------|--------|
| **Risk** | Critical |
| **Impact** | Predictions view/store call GET list and GET RUL; backend has no list endpoint and only POST for RUL. GET anomaly does not exist. Results in 404/method not allowed and broken Predictions UI. |
| **Area** | Frontend: `stores/predictions.ts`; optionally Backend: `PredictionsController` |
| **Effort** | Medium (frontend-only) or larger (if adding backend GETs) |

**Steps:**

1. **RUL (quick fix):** In `stores/predictions.ts`, change `fetchRULPrediction` to use `api.post` to `POST /api/predictions/rul/{machineId}` with body `{}`, and map `RulPredictionResult` to frontend `RULPrediction` (e.g. `Rul` → `predictedRUL`/`currentRUL`, `LowerBound`/`UpperBound` → `confidenceLower`/`confidenceUpper`). Remove or avoid GET for RUL in predictions store.
2. **List (quick fix):** Stop calling GET `/api/predictions` (and `?machineId=`) until a backend list endpoint exists. Either remove `fetchPredictions` usage from the Predictions view or have it use another source (e.g. only RUL + health from existing endpoints) so the page does not depend on a non-existent list.
3. **Anomaly (quick fix):** Remove or stub `fetchAnomalyPrediction` (GET `/api/predictions/anomaly/{machineId}`). Optionally later: add backend GET anomaly or call existing `POST api/advancedanalytics/anomaly-detection/{machineId}` and adapt the store.
4. **Optional (backlog):** Add backend `GET api/predictions` (list) and `GET api/predictions/rul/{machineId}` if product requirements need them; then reintroduce GET list and GET RUL in the frontend.

**Acceptance:** Predictions view loads without 404/method errors; RUL is fetched via POST and displayed; list and anomaly either not used or wired to real endpoints.

---

### P0-4. 2FA endpoints missing

| Field | Value |
|-------|--------|
| **Risk** | Critical (if 2FA UI is reachable) |
| **Impact** | Frontend calls `POST /api/auth/2fa/enable`, `verify`, `disable`; backend has no routes → 404 and broken 2FA flow. |
| **Area** | Backend: `AuthController`; or Frontend: hide/disable 2FA |
| **Effort** | Medium (backend) or Small (frontend hide) |

**Steps:**

1. **Option A (short-term):** Hide or disable 2FA in the UI (e.g. `UserProfileSettings.vue`, any 2FA entry points) until backend is implemented. Remove or no-op calls to enable/verify/disable so no 404s.
2. **Option B (full fix):** Implement 2FA on backend (enable returns QR/secret, verify validates TOTP, disable turns off 2FA) and align request/response DTOs with frontend. Ensure `CurrentUserResponse.TwoFactorEnabled` is set correctly after verify/disable.

**Acceptance:** Either 2FA is not exposed in production, or 2FA enable/verify/disable work end-to-end without 404.

---

## HIGH (P1) — Fix before production; data and UX at risk

DTO and contract mismatches can cause runtime errors, wrong display, or fragile behavior.

---

### P1-1. Login / me user shape (name vs fullName)

| Field | Value |
|-------|--------|
| **Risk** | High |
| **Impact** | Backend returns `fullName` (UserInfo) or `name` (CurrentUserResponse); frontend expects `name`. Display name can be missing or wrong after login/me. |
| **Area** | Backend: Auth DTOs; Frontend: `stores/auth.ts`, types |
| **Effort** | Small |

**Steps:**

1. Standardize on one property name (e.g. `name`) for display name in all auth responses (TokenResponse.User, CurrentUserResponse). If backend must keep `FullName` for internal use, add a JSON property name attribute or mapping so the API contract exposes `name` (camelCase).
2. In frontend, use that single property (e.g. `response.data.user.name` or `response.data.name`) for display and in the User type.
3. Ensure both login and GET me return the same shape for the user.

**Acceptance:** After login and on page reload (me), user’s display name appears correctly in the UI.

---

### P1-2. Machine DTO alignment

| Field | Value |
|-------|--------|
| **Risk** | High |
| **Impact** | Backend MachineDto: Status string, Properties string, no location/healthScore/lastMaintenanceDate/specifications. Frontend expects different status values and object shape; can cause mapping errors or wrong display. |
| **Area** | Backend: MachineDto, GetMachine(s) mapping; Frontend: `types/index.ts`, `stores/machines.ts` |
| **Effort** | Medium |

**Steps:**

1. Define a single contract (e.g. API response type): id, name, type, status (enum or fixed set), location (optional), healthScore or healthStatus, lastMaintenanceDate, nextMaintenanceDate (optional), specifications as object, etc.
2. Backend: Either extend MachineDto to include missing fields (from entity or computed) or add a view-model/DTO that matches the contract; map Status to frontend enum (e.g. Operational → "Running") if needed. Expose specifications as object (e.g. deserialize Properties JSON).
3. Frontend: Use the contract type in the store and components; map from API response if backend sends different names (e.g. healthStatus → healthScore).
4. Add or update integration tests for GET machines and GET machine by id.

**Acceptance:** Machines list and detail load without errors; status, health, and specifications display correctly.

---

### P1-3. Telemetry latest → TelemetryMetrics mapping

| Field | Value |
|-------|--------|
| **Risk** | High |
| **Impact** | Backend returns TelemetryDto (Id, MachineId, DataType, Data as JsonDocument, Timestamp). Frontend expects flat TelemetryMetrics (temperature, vibration, pressure, etc.). Unmapped or wrong structure can break dashboards/charts. |
| **Area** | Backend: optional view DTO; Frontend: `stores/machines.ts`, types |
| **Effort** | Medium |

**Steps:**

1. Backend: Either add an endpoint or response shape that returns “latest metrics” as a flat object (e.g. machineId, temperature, vibration, pressure, lastUpdated), or document the structure of `Data` so the frontend can rely on it.
2. Frontend: In the flow that calls GET telemetry/{machineId}/latest, map the response to TelemetryMetrics: parse `Data` (or use the new flat DTO) and map to temperature, vibration, pressure, lastUpdated, etc. Handle missing or null fields safely.
3. Ensure components that use `currentMetrics` (e.g. machine cards, charts) receive the expected shape.

**Acceptance:** Latest telemetry for a machine loads and displays in the UI without runtime errors; values match backend.

---

### P1-4. AlertDto: AcknowledgedBy and RelatedPredictionId

| Field | Value |
|-------|--------|
| **Risk** | High (if UI shows who acknowledged or link to prediction) |
| **Impact** | Entity has AcknowledgedBy and RelatedPredictionId; AlertDto and FromEntity do not expose them. Frontend may show incomplete alert details. |
| **Area** | Backend: `AlertDto`, `AlertDto.FromEntity` |
| **Effort** | Small |

**Steps:**

1. Add `AcknowledgedBy` (string?) and `RelatedPredictionId` (Guid?) to AlertDto (or the record’s optional parameters).
2. In `FromEntity`, pass `alert.AcknowledgedBy` and `alert.RelatedPredictionId`.
3. Ensure API serialization uses camelCase so frontend receives `acknowledgedBy`, `relatedPredictionId`. Update frontend BackendAlertDto if needed.

**Acceptance:** GET alerts (and GET alert by id) include acknowledgedBy and relatedPredictionId when present; frontend can show them.

---

### P1-5. RUL response contract (RulPredictionResult ↔ RULPrediction)

| Field | Value |
|-------|--------|
| **Risk** | High |
| **Impact** | Backend returns Rul, LowerBound, UpperBound, etc.; frontend expects currentRUL, predictedRUL, confidenceLower, confidenceUpper, trend, estimatedFailureDate. Missing or wrong mapping can break RUL display. |
| **Area** | Frontend: `stores/machines.ts`, `stores/predictions.ts`; optional Backend: add fields if needed |
| **Effort** | Small–Medium |

**Steps:**

1. Document the canonical RUL response (backend + frontend): e.g. predictedRUL, confidenceLower, confidenceUpper, modelVersion, lastUpdated (or predictionTime), optional trend, estimatedFailureDate.
2. Frontend: In every place that consumes RUL (machines store POST, predictions store POST), map backend fields to frontend RULPrediction: e.g. Rul → predictedRUL (and optionally currentRUL if available), LowerBound/UpperBound → confidenceLower/confidenceUpper, PredictionTime → lastUpdated. Set defaults for missing fields (e.g. trend, estimatedFailureDate) if backend does not send them.
3. Optionally extend backend RulPredictionResult with trend and estimatedFailureDate if the product needs them.

**Acceptance:** RUL cards/charts show correct values and no undefined/null errors; confidence band and last updated time are correct.

---

### P1-6. Mock and placeholder services in production

| Field | Value |
|-------|--------|
| **Risk** | High |
| **Impact** | MockExternalSystemService, MockTenantService, TelemetryMockHostedService, and other mocks/placeholders are used in production; notifications, reports, and some ML paths use dummy data. Unacceptable for production without clear boundaries. |
| **Area** | Backend: `ServiceCollectionExtensions`, NotificationService, DataDriftService, ReportsController, IAIService/workflow/AIModel handlers, etc. |
| **Effort** | Large (split into sub-tasks) |

**Steps:**

1. **External systems / Tenants:** Replace MockExternalSystemService and MockTenantService with real implementations, or register them only in non-production (e.g. `if (!IsProduction) services.AddScoped<..., MockExternalSystemService>()`). Document behavior when external systems or tenancy are disabled.
2. **Telemetry mock:** Make TelemetryMockHostedService opt-in (e.g. configuration flag or only in Development). In production, disable it unless explicitly enabled for demos.
3. **Notifications:** Replace placeholder email/SMTP in NotificationService with real sending or a no-op that logs; do not leave “would send here” in production path without a clear contract.
4. **Data drift / Reports / ML mocks:** Replace dummy reference data and mock returns with real logic or feature-flagged implementations. If a feature is not ready, return 501 or hide the feature in the UI instead of returning mock data in production.
5. Add a checklist or test that fails if mocks are registered in production (e.g. environment assertion in startup or in tests).

**Acceptance:** Production deployment does not use mock external/tenant services or mock telemetry unless explicitly configured; notifications and critical analytics paths either work or are clearly disabled.

---

## MEDIUM (P2) — Fix for stability and maintainability

---

### P2-1. Register contract (FullName, confirmPassword, acceptTerms)

| Field | Value |
|-------|--------|
| **Risk** | Medium |
| **Impact** | Backend expects FullName; frontend sends name. Backend does not validate confirmPassword or acceptTerms; frontend may send them. Minor validation and UX inconsistency. |
| **Area** | Backend: RegisterRequest, AuthController; Frontend: RegisterForm, RegisterData |
| **Effort** | Small |

**Steps:**

1. Backend: Accept `Name` (or keep `FullName` and document it). Optionally add `ConfirmPassword` and `AcceptTerms` to RegisterRequest and validate (password match, terms true).
2. Frontend: Send the same property the backend expects for display name (e.g. fullName or name per contract). If backend adds confirmPassword/acceptTerms, send them.
3. Align validation messages (e.g. “Passwords do not match”, “You must accept the terms”).

**Acceptance:** Registration works; optional confirmPassword/acceptTerms are validated if present in the contract.

---

### P2-2. Empty or minimal catch blocks

| Field | Value |
|-------|--------|
| **Risk** | Medium |
| **Impact** | Exceptions swallowed or only default returned; failures are hard to diagnose in production. |
| **Area** | PrescriptiveAnalyticsService, HealthController, FeatureExtractionService, ShapServiceClient, others (see audit) |
| **Effort** | Small per location |

**Steps:**

1. In each identified catch block, add structured logging (e.g. logger.LogWarning/LogError with exception and context). Preserve rethrow where appropriate (e.g. command handlers).
2. Where a fallback is intentional (e.g. “use default criticality”), log at least at Information level so support can see that fallback was used.
3. Avoid empty catch blocks; if ignoring is required, log and comment why.

**Acceptance:** No silent swallows in critical paths; logs allow diagnosis of failures.

---

### P2-3. Frontend chart mock data

| Field | Value |
|-------|--------|
| **Risk** | Medium |
| **Impact** | RulTimelineChart, PredictionVsActualChart, AlertsTimelineChart, SensorDistributionChart, HealthTrendChart use generateMockData() when no real data. Users may see fake data without clear indication. |
| **Area** | Frontend: chart components (see audit) |
| **Effort** | Small–Medium |

**Steps:**

1. When using mock/fallback data, show an explicit “Demo data” or “No data – showing sample” state (label or empty state with message).
2. Prefer loading real data from API/SignalR and only use mock when no data is available and you want to show a sample; ensure the component does not claim the data is real.
3. Optionally feature-flag or environment-gate mock data (e.g. only in development).

**Acceptance:** Charts either show real data or a clear indication that data is sample/demo.

---

### P2-4. Alert rules “mock rules” in Alerts view

| Field | Value |
|-------|--------|
| **Risk** | Medium |
| **Impact** | Alerts.vue references “Mock rules for demo (no backend endpoint yet)”. Confusing or wrong behavior if users think rules are real. |
| **Area** | Frontend: `views/Alerts.vue` |
| **Effort** | Small |

**Steps:**

1. If alert rules backend exists (e.g. AlertRulesController), wire the view to it and remove mock rules.
2. If not, show an empty state or “Alert rules coming soon” and do not display mock rules as if they were real.
3. Remove or update the comment so it does not imply mock data is used as production data.

**Acceptance:** Alert rules section either uses the real API or shows a clear non-mock state.

---

## LOW (P3) — Cleanup and consistency

---

### P3-1. Unify auth storage keys

| Field | Value |
|-------|--------|
| **Risk** | Low |
| **Impact** | Auth store uses auth_token/refresh_token and also token/refreshToken; api interceptor uses token/refreshToken. Redundant and error-prone. |
| **Area** | Frontend: `stores/auth.ts`, `services/api.ts` |
| **Effort** | Small |

**Steps:**

1. Use a single set of keys (e.g. token, refreshToken) for both the auth store and the api client. When login/refresh succeed, write once to those keys.
2. On logout, clear the same keys in one place (or from a shared constant). Remove duplicate key names if any.
3. Document where the “source of truth” for the access token lives (e.g. localStorage key X used by api interceptor).

**Acceptance:** One consistent set of storage keys; no duplicate or conflicting token storage.

---

### P3-2. Dead backend endpoints (optional cleanup)

| Field | Value |
|-------|--------|
| **Risk** | Low |
| **Impact** | Many endpoints are never called by the current frontend; they add surface area and maintenance. |
| **Area** | Backend: various controllers (see audit Section 5.1) |
| **Effort** | Low (document only) or Medium (deprecate/remove) |

**Steps:**

1. Document which endpoints are “frontend-used” vs “internal/future” so future changes don’t break unused APIs by accident.
2. Optionally: add [Obsolete] or deprecation headers for endpoints that are confirmed unused and will be removed in a future version.
3. Do not remove endpoints that might be used by other clients (e.g. mobile, integrations) without product confirmation.

**Acceptance:** Clear list of used vs unused endpoints; no accidental breaking changes.

---

### P3-3. Severity enum alignment (Alert)

| Field | Value |
|-------|--------|
| **Risk** | Low |
| **Impact** | Backend has Info, Warning, Critical, Emergency; frontend has info, warning, critical, error. Possible mismatch for “error” vs “Emergency”. |
| **Area** | Backend: AlertSeverity; Frontend: Alert type severity; stores/alerts mapping |
| **Effort** | Small |

**Steps:**

1. Map backend enum to frontend union in one place (e.g. alerts store or a shared mapper): e.g. Emergency → "critical" or "error" and document the choice.
2. Ensure backend serializes as camelCase (info, warning, critical, emergency) so frontend can use as-is or map consistently.
3. If frontend uses "error", ensure backend either has an equivalent or maps to one value (e.g. Critical).

**Acceptance:** All backend severity values display correctly in the UI without unknown values.

---

## Summary matrix

| Priority | Item | Area | Effort | Blocks production? |
|----------|------|------|--------|--------------------|
| P0-1 | Auth refresh token property | Frontend | S | Yes |
| P0-2 | Password reset contract | Backend + Frontend | S | Yes |
| P0-3 | Predictions store GET/list/anomaly | Frontend (+ optional Backend) | M | Yes |
| P0-4 | 2FA endpoints or hide 2FA | Backend or Frontend | S–M | Yes (if 2FA visible) |
| P1-1 | Login/me user name | Backend + Frontend | S | Recommended |
| P1-2 | Machine DTO | Backend + Frontend | M | Recommended |
| P1-3 | Telemetry latest mapping | Backend + Frontend | M | Recommended |
| P1-4 | AlertDto AcknowledgedBy/RelatedPredictionId | Backend | S | Recommended |
| P1-5 | RUL response contract | Frontend (+ optional Backend) | S–M | Recommended |
| P1-6 | Mock/placeholder in production | Backend | L | Recommended |
| P2-1 | Register contract | Backend + Frontend | S | No |
| P2-2 | Catch blocks logging | Backend | S | No |
| P2-3 | Chart mock data labeling | Frontend | S–M | No |
| P2-4 | Alert rules mock | Frontend | S | No |
| P3-1 | Auth storage keys | Frontend | S | No |
| P3-2 | Dead endpoints doc/deprecate | Backend | L | No |
| P3-3 | Alert severity enum | Backend + Frontend | S | No |

**Recommended order of execution (by risk and dependency):**

1. **P0-1** (refresh token) — unblocks auth after expiry.  
2. **P0-2** (reset password) — unblocks password recovery.  
3. **P0-3** (predictions store) — unblocks Predictions page.  
4. **P0-4** (2FA) — remove 404s or implement.  
5. **P1-1** (login/me name) — quick contract fix.  
6. **P1-4** (AlertDto) — quick backend fix.  
7. **P1-5** (RUL mapping) — aligns with P0-3.  
8. **P1-2**, **P1-3** (Machine, Telemetry) — then **P1-6** (mocks) in phases.

---

*End of Prioritized Fix Plan*
