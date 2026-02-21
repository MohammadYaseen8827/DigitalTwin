import { test, expect } from '@playwright/test'

test.describe('Digital Twin Platform - End-to-End Integration Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Setup authentication
    await page.goto('/login')
    await page.fill('[data-testid="email-input"]', 'admin@digitaltwin.com')
    await page.fill('[data-testid="password-input"]', 'SecurePassword123!')
    await page.click('[data-testid="login-button"]')
    await page.waitForURL('/dashboard')
  })

  test('Complete Predictive Maintenance Workflow', async ({ page }) => {
    // 1. Navigate to machines page
    await page.click('[data-testid="machines-nav"]')
    await page.waitForURL('/machines')
    
    // 2. Create a new machine
    await page.click('[data-testid="add-machine-button"]')
    await page.fill('[data-testid="machine-name"]', 'Integration Test Machine')
    await page.selectOption('[data-testid="machine-type"]', 'CNC-Milling')
    await page.fill('[data-testid="machine-location"]', 'Production Line A')
    await page.click('[data-testid="save-machine-button"]')
    
    // 3. Verify machine appears in list
    await expect(page.locator('[data-testid="machine-list"]')).toContainText('Integration Test Machine')
    
    // 4. Navigate to predictions
    await page.click('[data-testid="predictions-nav"]')
    await page.waitForURL('/predictions')
    
    // 5. Request a prediction for the new machine
    await page.selectOption('[data-testid="machine-selector"]', 'Integration Test Machine')
    await page.click('[data-testid="request-prediction-button"]')
    
    // 6. Wait for prediction to complete
    await page.waitForSelector('[data-testid="prediction-result"]', { timeout: 10000 })
    
    // 7. Verify prediction data
    await expect(page.locator('[data-testid="rul-value"]')).toBeVisible()
    await expect(page.locator('[data-testid="confidence-score"]')).toBeVisible()
    
    // 8. Navigate to maintenance scheduling
    await page.click('[data-testid="maintenance-nav"]')
    await page.waitForURL('/maintenance')
    
    // 9. Schedule maintenance based on prediction
    await page.click('[data-testid="schedule-maintenance-button"]')
    await page.selectOption('[data-testid="maintenance-type"]', 'Preventive')
    await page.fill('[data-testid="maintenance-date"]', new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0])
    await page.click('[data-testid="confirm-schedule-button"]')
    
    // 10. Verify maintenance is scheduled
    await expect(page.locator('[data-testid="maintenance-list"]')).toContainText('Preventive')
  })

  test('Mathematical Degradation Modeling Workflow', async ({ page }) => {
    // 1. Navigate to mathematical modeling
    await page.click('[data-testid="mathematical-modeling-nav"]')
    await page.waitForURL('/mathematical-modeling')
    
    // 2. Configure exponential degradation model
    await page.selectOption('[data-testid="model-type"]', 'Exponential')
    await page.fill('[data-testid="initial-value"]', '100')
    await page.fill('[data-testid="degradation-rate"]', '0.05')
    await page.fill('[data-testid="time-horizon"]', '365')
    await page.click('[data-testid="run-simulation-button"]')
    
    // 3. Wait for simulation results
    await page.waitForSelector('[data-testid="simulation-results"]', { timeout: 15000 })
    
    // 4. Verify simulation data
    await expect(page.locator('[data-testid="degradation-chart"]')).toBeVisible()
    await expect(page.locator('[data-testid="final-value"]')).toBeVisible()
    
    // 5. Test parameter estimation
    await page.click('[data-testid="parameter-estimation-tab"]')
    await page.fill('[data-testid="historical-data-points"]', '100,95,90,85,80')
    await page.click('[data-testid="estimate-parameters-button"]')
    
    // 6. Verify estimation results
    await page.waitForSelector('[data-testid="estimation-results"]')
    await expect(page.locator('[data-testid="estimated-parameters"]')).toBeVisible()
    await expect(page.locator('[data-testid="confidence-intervals"]')).toBeVisible()
  })

  test('Real-time Telemetry and Alerting Workflow', async ({ page }) => {
    // 1. Navigate to telemetry monitoring
    await page.click('[data-testid="telemetry-nav"]')
    await page.waitForURL('/telemetry')
    
    // 2. Start real-time monitoring
    await page.click('[data-testid="start-monitoring-button"]')
    
    // 3. Verify real-time data flow
    await page.waitForSelector('[data-testid="telemetry-stream"]', { timeout: 5000 })
    await expect(page.locator('[data-testid="live-metrics"]')).toBeVisible()
    
    // 4. Simulate alert condition
    await page.click('[data-testid="simulate-alert-button"]')
    
    // 5. Verify alert is triggered
    await page.waitForSelector('[data-testid="alert-notification"]')
    await expect(page.locator('[data-testid="alert-message"]')).toContainText('Temperature threshold exceeded')
    
    // 6. Navigate to alerts management
    await page.click('[data-testid="alerts-nav"]')
    await page.waitForURL('/alerts')
    
    // 7. Verify alert appears in list
    await expect(page.locator('[data-testid="alerts-list"]')).toContainText('Temperature threshold exceeded')
    
    // 8. Acknowledge and resolve alert
    await page.click('[data-testid="acknowledge-alert-button"]')
    await page.click('[data-testid="resolve-alert-button"]')
    
    // 9. Verify alert status change
    await expect(page.locator('[data-testid="alert-status"]')).toContainText('Resolved')
  })

  test('Reporting and Analytics Workflow', async ({ page }) => {
    // 1. Navigate to reporting
    await page.click('[data-testid="reports-nav"]')
    await page.waitForURL('/reports')
    
    // 2. Generate a comprehensive report
    await page.selectOption('[data-testid="report-template"]', 'Predictive Maintenance Summary')
    await page.fill('[data-testid="report-start-date"]', new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0])
    await page.fill('[data-testid="report-end-date"]', new Date().toISOString().split('T')[0])
    await page.click('[data-testid="generate-report-button"]')
    
    // 3. Wait for report generation
    await page.waitForSelector('[data-testid="report-preview"]', { timeout: 20000 })
    
    // 4. Verify report content
    await expect(page.locator('[data-testid="report-summary"]')).toBeVisible()
    await expect(page.locator('[data-testid="report-charts"]')).toBeVisible()
    
    // 5. Export report
    await page.click('[data-testid="export-pdf-button"]')
    
    // 6. Verify download starts (check for download indicator)
    await expect(page.locator('[data-testid="download-indicator"]')).toBeVisible()
    
    // 7. Schedule recurring report
    await page.click('[data-testid="schedule-report-tab"]')
    await page.selectOption('[data-testid="schedule-template"]', 'Predictive Maintenance Summary')
    await page.fill('[data-testid="schedule-cron"]', '0 9 * * 1') // Every Monday at 9 AM
    await page.fill('[data-testid="schedule-recipients"]', 'admin@company.com')
    await page.click('[data-testid="save-schedule-button"]')
    
    // 8. Verify schedule is saved
    await expect(page.locator('[data-testid="schedule-confirmation"]')).toContainText('Report scheduled successfully')
  })

  test('System Performance and Error Handling', async ({ page }) => {
    // 1. Test system under load
    const promises = []
    for (let i = 0; i < 5; i++) {
      promises.push(
        page.goto('/predictions').then(() => {
          return page.click('[data-testid="request-prediction-button"]')
        })
      )
    }
    await Promise.all(promises)
    
    // 2. Verify system handles concurrent requests
    await expect(page.locator('[data-testid="prediction-result"]')).toHaveCount(5)
    
    // 3. Test error handling - invalid input
    await page.goto('/machines')
    await page.click('[data-testid="add-machine-button"]')
    await page.fill('[data-testid="machine-name"]', '') // Empty name
    await page.click('[data-testid="save-machine-button"]')
    
    // 4. Verify validation error
    await expect(page.locator('[data-testid="validation-error"]')).toContainText('Machine name is required')
    
    // 5. Test network error handling
    await page.route('/api/machines', route => route.abort())
    await page.goto('/machines')
    await page.click('[data-testid="add-machine-button"]')
    await page.fill('[data-testid="machine-name"]', 'Test Machine')
    await page.click('[data-testid="save-machine-button"]')
    
    // 6. Verify error message
    await expect(page.locator('[data-testid="network-error"]')).toContainText('Failed to connect to server')
  })

  test('Cross-Browser Compatibility', async ({ page, browserName }) => {
    // Test critical workflows across different browsers
    await page.goto('/dashboard')
    
    // Verify dashboard loads correctly
    await expect(page.locator('[data-testid="dashboard-title"]')).toBeVisible()
    await expect(page.locator('[data-testid="summary-cards"]')).toHaveCount(4)
    
    // Test responsive design
    await page.setViewportSize({ width: 768, height: 1024 }) // Tablet
    await expect(page.locator('[data-testid="mobile-menu"]')).toBeVisible()
    
    await page.setViewportSize({ width: 375, height: 667 }) // Mobile
    await expect(page.locator('[data-testid="mobile-navigation"]')).toBeVisible()
    
    // Reset to desktop
    await page.setViewportSize({ width: 1920, height: 1080 })
  })

  test('Data Persistence and Session Management', async ({ page, context }) => {
    // 1. Perform some actions
    await page.goto('/predictions')
    await page.selectOption('[data-testid="machine-selector"]', 'CNC-001')
    await page.click('[data-testid="request-prediction-button"]')
    await page.waitForSelector('[data-testid="prediction-result"]')
    
    // 2. Verify session persistence
    const cookies = await context.cookies()
    expect(cookies.some(cookie => cookie.name === 'auth_token')).toBeTruthy()
    
    // 3. Test session timeout
    await page.waitForTimeout(31000) // Wait for session timeout (30 seconds)
    await page.reload()
    
    // 4. Verify re-authentication required
    await expect(page).toHaveURL('/login')
  })
})

test.describe('API Integration Tests', () => {
  test('Backend API Endpoints', async ({ request }) => {
    // Test machine management API
    const machineResponse = await request.post('/api/machines', {
      data: {
        name: 'API Test Machine',
        type: 'CNC-Milling',
        location: 'Test Location',
        specifications: {
          maxLoad: 1000,
          operatingTemp: 80
        }
      }
    })
    expect(machineResponse.ok()).toBeTruthy()
    const machineData = await machineResponse.json()
    expect(machineData.name).toBe('API Test Machine')
    
    // Test prediction API
    const predictionResponse = await request.post('/api/predictions/request', {
      data: {
        machineId: machineData.id,
        modelType: 'exponential'
      }
    })
    expect(predictionResponse.ok()).toBeTruthy()
    const predictionData = await predictionResponse.json()
    expect(predictionData.remainingUsefulLifeDays).toBeGreaterThan(0)
    
    // Test mathematical modeling API
    const modelingResponse = await request.post('/api/degradation-modeling/solve/exponential', {
      data: {
        initialValue: 100,
        degradationRate: 0.05,
        timeHorizon: 365,
        timePoints: 100,
        solverMethod: 'runge-kutta-4'
      }
    })
    expect(modelingResponse.ok()).toBeTruthy()
    const modelingData = await modelingResponse.json()
    expect(modelingData.timePoints).toHaveLength(100)
    
    // Cleanup
    await request.delete(`/api/machines/${machineData.id}`)
  })
})
