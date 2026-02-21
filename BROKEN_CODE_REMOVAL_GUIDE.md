# Broken Code Removal Guide
# 32 Unused Frontend API Methods to Remove

**Status:** ✅ Verified - None of these methods are used in components/views
**Action:** Safe to remove without breaking UI

## Summary

**Total Broken Methods:** 32
**Services Affected:** 8

| Service | Count | Status |
|---------|-------|--------|
| alertRules.service | 1 | ⚠️ REMOVE |
| azureDigitalTwin.service | 9 | ⚠️ REMOVE |
| benchmarkValidation.service | 12 | ⚠️ REMOVE |
| csrf.service | 1 | ⚠️ REMOVE |
| dataArchival.service | 2 | ⚠️ REMOVE |
| external-systems.service | 2 | ⚠️ REMOVE |
| predictions.service | 1 | ⚠️ REMOVE |
| reporting.service | 4 | ⚠️ REMOVE |

---

## Detailed Breakdown

### alertRules.service

**Methods to Remove:** 1

#### `deleteAlertRule`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/alertrules/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### azureDigitalTwin.service

**Methods to Remove:** 9

#### `syncAll`

- **HTTP Verb:** POST
- **Endpoint:** `api/azuredigitaltwin/sync-all`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getStatus`

- **HTTP Verb:** GET
- **Endpoint:** `api/azuredigitaltwin/status`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getTwinEntities`

- **HTTP Verb:** GET
- **Endpoint:** `api/azuredigitaltwin/entities`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getMachineTwin`

- **HTTP Verb:** GET
- **Endpoint:** `api/azuredigitaltwin/machines/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getLineTwin`

- **HTTP Verb:** GET
- **Endpoint:** `api/azuredigitaltwin/lines/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `updateTwinProperties`

- **HTTP Verb:** PATCH
- **Endpoint:** `api/azuredigitaltwin/entities/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `createRelationship`

- **HTTP Verb:** POST
- **Endpoint:** `api/azuredigitaltwin/relationships`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `deleteTwin`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/azuredigitaltwin/entities/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `queryTwins`

- **HTTP Verb:** POST
- **Endpoint:** `api/azuredigitaltwin/query`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### benchmarkValidation.service

**Methods to Remove:** 12

#### `getValidationResults`

- **HTTP Verb:** GET
- **Endpoint:** `api/benchmarkvalidation/results`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getValidationResultById`

- **HTTP Verb:** GET
- **Endpoint:** `api/benchmarkvalidation/results/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `compareModels`

- **HTTP Verb:** POST
- **Endpoint:** `api/benchmarkvalidation/compare`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `generateValidationReport`

- **HTTP Verb:** POST
- **Endpoint:** `api/benchmarkvalidation/reports`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getValidationReports`

- **HTTP Verb:** GET
- **Endpoint:** `api/benchmarkvalidation/reports`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getValidationReportById`

- **HTTP Verb:** GET
- **Endpoint:** `api/benchmarkvalidation/reports/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `deleteValidationReport`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/benchmarkvalidation/reports/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `uploadBenchmarkDataset`

- **HTTP Verb:** POST
- **Endpoint:** `api/benchmarkvalidation/datasets/upload`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `deleteBenchmarkDataset`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/benchmarkvalidation/datasets/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getValidationHistory`

- **HTTP Verb:** GET
- **Endpoint:** `api/benchmarkvalidation/history/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `cancelValidation`

- **HTTP Verb:** POST
- **Endpoint:** `api/benchmarkvalidation/results/{param}/cancel`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `retryValidation`

- **HTTP Verb:** POST
- **Endpoint:** `api/benchmarkvalidation/results/{param}/retry`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### csrf.service

**Methods to Remove:** 1

#### `fetchToken`

- **HTTP Verb:** GET
- **Endpoint:** `api/antiforgery/tokens`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### dataArchival.service

**Methods to Remove:** 2

#### `getArchivalStatus`

- **HTTP Verb:** GET
- **Endpoint:** `api/dataarchival/status/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getArchivalHistory`

- **HTTP Verb:** GET
- **Endpoint:** `api/dataarchival/history`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### external-systems.service

**Methods to Remove:** 2

#### `cancelDataSynchronization`

- **HTTP Verb:** POST
- **Endpoint:** `api/externalsystems/synchronizations/{param}/cancel`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `retryFailedSynchronization`

- **HTTP Verb:** POST
- **Endpoint:** `api/externalsystems/synchronizations/{param}/retry`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### predictions.service

**Methods to Remove:** 1

#### `fetchPredictionHistory`

- **HTTP Verb:** GET
- **Endpoint:** `api/predictions/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

### reporting.service

**Methods to Remove:** 4

#### `cancelScheduledReport`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/reports/schedules/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `getReportById`

- **HTTP Verb:** GET
- **Endpoint:** `api/reports/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `deleteReport`

- **HTTP Verb:** DELETE
- **Endpoint:** `api/reports/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

#### `updateScheduledReport`

- **HTTP Verb:** PUT
- **Endpoint:** `api/reports/schedules/{param}`
- **Status:** Orphaned (no backend match)
- **Used in:** NONE (verified)

**Removal Steps:**
1. Remove the method definition from the service file
2. Remove from service export object
3. Run `python analyze_integrity.py` to verify

---

## Removal Procedure

### Option 1: Manual Removal (Recommended)

For each service file:

1. **Find the file:**
   ```bash
   # Navigate to service file
   d:\Work\Diploma\src\ui\digital-twin-dashboard\src\services\{SERVICE_NAME}.service.ts
   ```

2. **Remove the method:**
   - Delete the entire function/method implementation
   - Delete any type imports specific to that method

3. **Remove from exports:**
   - Find the service export object (e.g., `export const alertRulesService = {...}`)
   - Remove the method name from the export

4. **Verify:**
   ```bash
   # Run analysis to verify removal
   python analyze_integrity.py
   ```

### Option 2: Automated Removal Script

Run the cleanup script (generates diffs for review):
```bash
python cleanup_broken_methods.py --dry-run  # Preview changes
python cleanup_broken_methods.py --apply    # Apply changes
```

---

## Service-by-Service Removal Guide

### alertRules.service

**File:** `src/ui/digital-twin-dashboard/src/services/alertRules.service.service.ts`

**Methods to Remove:** deleteAlertRule

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function deleteAlertRule(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // deleteAlertRule,
  // Keep the working methods
}
```

### azureDigitalTwin.service

**File:** `src/ui/digital-twin-dashboard/src/services/azureDigitalTwin.service.service.ts`

**Methods to Remove:** syncAll, getStatus, getTwinEntities, getMachineTwin, getLineTwin, updateTwinProperties, createRelationship, deleteTwin, queryTwins

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function syncAll(...) {}
// export async function getStatus(...) {}
// export async function getTwinEntities(...) {}
// export async function getMachineTwin(...) {}
// export async function getLineTwin(...) {}
// export async function updateTwinProperties(...) {}
// export async function createRelationship(...) {}
// export async function deleteTwin(...) {}
// export async function queryTwins(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // syncAll,
  // getStatus,
  // getTwinEntities,
  // getMachineTwin,
  // getLineTwin,
  // updateTwinProperties,
  // createRelationship,
  // deleteTwin,
  // queryTwins,
  // Keep the working methods
}
```

### benchmarkValidation.service

**File:** `src/ui/digital-twin-dashboard/src/services/benchmarkValidation.service.service.ts`

**Methods to Remove:** getValidationResults, getValidationResultById, compareModels, generateValidationReport, getValidationReports, getValidationReportById, deleteValidationReport, uploadBenchmarkDataset, deleteBenchmarkDataset, getValidationHistory, cancelValidation, retryValidation

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function getValidationResults(...) {}
// export async function getValidationResultById(...) {}
// export async function compareModels(...) {}
// export async function generateValidationReport(...) {}
// export async function getValidationReports(...) {}
// export async function getValidationReportById(...) {}
// export async function deleteValidationReport(...) {}
// export async function uploadBenchmarkDataset(...) {}
// export async function deleteBenchmarkDataset(...) {}
// export async function getValidationHistory(...) {}
// export async function cancelValidation(...) {}
// export async function retryValidation(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // getValidationResults,
  // getValidationResultById,
  // compareModels,
  // generateValidationReport,
  // getValidationReports,
  // getValidationReportById,
  // deleteValidationReport,
  // uploadBenchmarkDataset,
  // deleteBenchmarkDataset,
  // getValidationHistory,
  // cancelValidation,
  // retryValidation,
  // Keep the working methods
}
```

### csrf.service

**File:** `src/ui/digital-twin-dashboard/src/services/csrf.service.service.ts`

**Methods to Remove:** fetchToken

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function fetchToken(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // fetchToken,
  // Keep the working methods
}
```

### dataArchival.service

**File:** `src/ui/digital-twin-dashboard/src/services/dataArchival.service.service.ts`

**Methods to Remove:** getArchivalStatus, getArchivalHistory

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function getArchivalStatus(...) {}
// export async function getArchivalHistory(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // getArchivalStatus,
  // getArchivalHistory,
  // Keep the working methods
}
```

### external-systems.service

**File:** `src/ui/digital-twin-dashboard/src/services/external-systems.service.service.ts`

**Methods to Remove:** cancelDataSynchronization, retryFailedSynchronization

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function cancelDataSynchronization(...) {}
// export async function retryFailedSynchronization(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // cancelDataSynchronization,
  // retryFailedSynchronization,
  // Keep the working methods
}
```

### predictions.service

**File:** `src/ui/digital-twin-dashboard/src/services/predictions.service.service.ts`

**Methods to Remove:** fetchPredictionHistory

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function fetchPredictionHistory(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // fetchPredictionHistory,
  // Keep the working methods
}
```

### reporting.service

**File:** `src/ui/digital-twin-dashboard/src/services/reporting.service.service.ts`

**Methods to Remove:** cancelScheduledReport, getReportById, deleteReport, updateScheduledReport

**File Structure:**
```typescript
// 1. Remove these imports if no longer used:
// import { SomeInterface } from '@/...'  // Check if used elsewhere

// 2. Remove these methods:
// export async function cancelScheduledReport(...) {}
// export async function getReportById(...) {}
// export async function deleteReport(...) {}
// export async function updateScheduledReport(...) {}

// 3. Update the service export:
export const {SERVICE}Service = {{
  // Remove these lines:
  // cancelScheduledReport,
  // getReportById,
  // deleteReport,
  // updateScheduledReport,
  // Keep the working methods
}
```

---

## Post-Removal Testing

### 1. Frontend Build
```bash
cd src/ui/digital-twin-dashboard
npm run build
# Should complete without errors
```

### 2. Run Tests
```bash
npm run test
# Verify no tests reference removed methods
```

### 3. API Integrity Check
```bash
python scan_frontend.py
python scan_backend_v2.py
python analyze_integrity.py
# Verify broken_calls count decreases by 32
```

---

## Impact Analysis

### Risk Level: 🟢 LOW

- ✅ No components use these methods (verified)
- ✅ No stores reference these methods
- ✅ No other services depend on these methods
- ✅ Removal will not affect UI functionality

### Benefits of Removal

- Reduced code size (~50-100 lines per service)
- Cleaner service files
- Reduced maintenance burden
- Improved code clarity
- Easier onboarding for new developers

### Timeline

- **Estimated Effort:** 2-4 hours (manual) or 30 minutes (automated)
- **Can be done:** Post-deployment (non-critical path)
- **Recommended Sprint:** Sprint 2 or during code cleanup phase

---

