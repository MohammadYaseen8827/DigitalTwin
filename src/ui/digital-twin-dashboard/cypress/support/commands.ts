/// <reference types="cypress" />

// Custom commands
Cypress.Commands.add('login', (username: string, password: string) => {
  cy.visit('/login')
  cy.get('[data-testid="username-input"]').type(username)
  cy.get('[data-testid="password-input"]').type(password)
  cy.get('[data-testid="login-button"]').click()
  cy.url().should('not.include', '/login')
})

// TypeScript declarations for custom commands
declare global {
  namespace Cypress {
    interface Chainable {
      login(username: string, password: string): Chainable<void>
    }
  }
}

export {}