/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE_URL: string
  readonly VITE_ENVIRONMENT: 'development' | 'staging' | 'production'
  readonly VITE_ERROR_LOGGING_ENABLED: string
  readonly VITE_ERROR_SAMPLE_RATE: string
  readonly VITE_SENTRY_DSN?: string
  readonly VITE_ENABLE_HEALTH_MONITORING: string
  readonly VITE_HEALTH_CHECK_INTERVAL: string
  readonly VITE_DEBUG: string
  readonly DEV: boolean
  readonly PROD: boolean
  readonly MODE: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
