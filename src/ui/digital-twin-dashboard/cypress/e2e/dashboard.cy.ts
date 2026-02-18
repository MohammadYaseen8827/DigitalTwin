/// <reference types="cypress" />

// Import custom commands
import '../../support/commands'

describe('Digital Twin Dashboard - Authentication', () => {
  beforeEach(() => {
    cy.visit('/')
  })

  it('should display login page for unauthenticated users', () => {
    cy.url().should('include', '/login')
    cy.get('[data-testid="login-form"]').should('be.visible')
    cy.get('[data-testid="username-input"]').should('be.visible')
    cy.get('[data-testid="password-input"]').should('be.visible')
    cy.get('[data-testid="login-button"]').should('be.visible')
  })

  it('should allow user to login with valid credentials', () => {
    cy.get('[data-testid="username-input"]').type(Cypress.env('testUser'))
    cy.get('[data-testid="password-input"]').type(Cypress.env('testPassword'))
    cy.get('[data-testid="login-button"]').click()
    
    cy.url().should('not.include', '/login')
    cy.get('[data-testid="dashboard-header"]').should('be.visible')
  })

  it('should show error for invalid credentials', () => {
    cy.get('[data-testid="username-input"]').type('invalid@example.com')
    cy.get('[data-testid="password-input"]').type('wrongpassword')
    cy.get('[data-testid="login-button"]').click()
    
    cy.get('[data-testid="error-message"]').should('be.visible')
    cy.url().should('include', '/login')
  })
})

describe('Digital Twin Dashboard - Navigation', () => {
  beforeEach(() => {
    // Login before each test
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    cy.visit('/')
  })

  it('should navigate to maintenance management dashboard', () => {
    cy.get('[data-testid="nav-maintenance"]').click()
    cy.url().should('include', '/maintenance')
    cy.get('[data-testid="maintenance-dashboard"]').should('be.visible')
  })

  it('should navigate to telemetry monitoring dashboard', () => {
    cy.get('[data-testid="nav-telemetry"]').click()
    cy.url().should('include', '/telemetry')
    cy.get('[data-testid="telemetry-dashboard"]').should('be.visible')
  })

  it('should navigate to reporting dashboard', () => {
    cy.get('[data-testid="nav-reports"]').click()
    cy.url().should('include', '/reports')
    cy.get('[data-testid="reports-dashboard"]').should('be.visible')
  })
})

describe('Digital Twin Dashboard - Core Functionality', () => {
  beforeEach(() => {
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    cy.visit('/machines')
  })

  it('should display machine management dashboard', () => {
    cy.get('[data-testid="machine-dashboard"]').should('be.visible')
    cy.get('[data-testid="machine-list"]').should('be.visible')
    cy.get('[data-testid="create-machine-button"]').should('be.visible')
  })

  it('should allow creating a new machine', () => {
    cy.get('[data-testid="create-machine-button"]').click()
    cy.get('[data-testid="machine-form"]').should('be.visible')
    
    cy.get('[data-testid="machine-name-input"]').type('Test Machine')
    cy.get('[data-testid="machine-type-select"]').select('CNC')
    cy.get('[data-testid="machine-location-input"]').type('Factory Floor 1')
    
    cy.get('[data-testid="save-machine-button"]').click()
    
    cy.get('[data-testid="success-message"]').should('be.visible')
    cy.get('[data-testid="machine-list"]').should('contain', 'Test Machine')
  })

  it('should allow filtering machines', () => {
    cy.get('[data-testid="search-input"]').type('CNC')
    cy.get('[data-testid="machine-list"]')
      .find('[data-testid="machine-item"]')
      .each(($el) => {
        cy.wrap($el).should('contain', 'CNC')
      })
  })
})

describe('Digital Twin Dashboard - Alerts Management', () => {
  beforeEach(() => {
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    cy.visit('/alerts')
  })

  it('should display alerts dashboard', () => {
    cy.get('[data-testid="alerts-dashboard"]').should('be.visible')
    cy.get('[data-testid="alerts-list"]').should('be.visible')
  })

  it('should allow acknowledging alerts', () => {
    cy.get('[data-testid="alert-item"]')
      .first()
      .find('[data-testid="acknowledge-button"]')
      .click()
    
    cy.get('[data-testid="confirmation-dialog"]').should('be.visible')
    cy.get('[data-testid="confirm-acknowledge"]').click()
    
    cy.get('[data-testid="success-message"]').should('be.visible')
  })
})

describe('Digital Twin Dashboard - Performance Monitoring', () => {
  beforeEach(() => {
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    cy.visit('/performance')
  })

  it('should display performance metrics', () => {
    cy.get('[data-testid="performance-dashboard"]').should('be.visible')
    cy.get('[data-testid="cpu-usage-chart"]').should('be.visible')
    cy.get('[data-testid="memory-usage-chart"]').should('be.visible')
    cy.get('[data-testid="network-traffic-chart"]').should('be.visible')
  })

  it('should show system health status', () => {
    cy.get('[data-testid="system-health-indicator"]')
      .should('have.class', 'status-green')
      .and('contain', 'Healthy')
  })
})

describe('Digital Twin Dashboard - Responsive Design', () => {
  it('should adapt layout for mobile devices', () => {
    cy.viewport('iphone-6')
    cy.visit('/')
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    
    cy.get('[data-testid="mobile-menu-button"]').should('be.visible')
    cy.get('[data-testid="desktop-nav"]').should('not.be.visible')
  })

  it('should maintain functionality on tablet devices', () => {
    cy.viewport('ipad-2')
    cy.visit('/')
    cy.login(Cypress.env('testUser'), Cypress.env('testPassword'))
    
    cy.get('[data-testid="dashboard-grid"]').should('be.visible')
    cy.get('[data-testid="chart-container"]').should('be.visible')
  })
})
