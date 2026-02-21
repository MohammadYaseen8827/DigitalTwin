import { defineConfig } from 'cypress'

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:5173',
    viewportWidth: 1280,
    viewportHeight: 720,
    defaultCommandTimeout: 10000,
    requestTimeout: 15000,
    responseTimeout: 30000,
    pageLoadTimeout: 60000,
    execTimeout: 60000,
    taskTimeout: 60000,
    watchForFileChanges: true,
    chromeWebSecurity: false,
    experimentalStudio: true,
    experimentalWebKitSupport: false,
    numTestsKeptInMemory: 50,
    scrollBehavior: 'top',
    waitForAnimations: true,
    animationDistanceThreshold: 5,
    video: true,
    videoCompression: 32,
    videosFolder: 'cypress/videos',
    screenshotsFolder: 'cypress/screenshots',
    fixturesFolder: 'cypress/fixtures',
    specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}',
    excludeSpecPattern: [
      'cypress/e2e/examples/**/*',
      'cypress/e2e/utils/**/*'
    ],
    supportFile: 'cypress/support/e2e.ts',
    downloadsFolder: 'cypress/downloads',
    trashAssetsBeforeRuns: true,
    blockHosts: [
      '*.google-analytics.com',
      '*.googletagmanager.com',
      '*.doubleclick.net'
    ],
    retries: {
      runMode: 2,
      openMode: 0
    },
    env: {
      apiUrl: 'http://localhost:5000',
      testUser: 'test@example.com',
      testPassword: 'password123'
    }
  }
})