# Frontend API Configuration System

## Overview
This document describes the robust API configuration system implemented for the Digital Twin Platform frontend.

## Configuration Hierarchy

### Environment Variables
The system supports the following environment variables:

```bash
# Base API URL
VITE_API_BASE_URL=http://localhost:5000/api

# Request timeout in milliseconds
VITE_API_TIMEOUT=30000

# API version parameter
VITE_API_VERSION=1.0

# Whether to send credentials with requests
VITE_API_WITH_CREDENTIALS=true
```

### Fallback Logic
1. **Development Mode**: Uses `http://localhost:5000/api` as default
2. **Production Mode**: Uses `/api` (relative path) as default
3. **Custom Configuration**: Respects `VITE_API_BASE_URL` when provided

## Usage

### Basic Usage
```typescript
import { apiConfig } from '@/utils/apiConfig'

// Get full configuration
const config = apiConfig.get()

// Get base URL
const baseUrl = apiConfig.getBaseUrl()

// Construct full endpoint URL
const fullUrl = apiConfig.getFullUrl('/machines')
```

### Environment Detection
```typescript
import { apiConfig } from '@/utils/apiConfig'

if (apiConfig.isDevelopment()) {
  // Development-specific logic
}

if (apiConfig.isProduction()) {
  // Production-specific logic
}
```

## Integration with Axios

The `axiosClient` automatically uses the configuration system:

```typescript
// axiosClient.ts - automatically configured
import { apiConfig } from '@/utils/apiConfig'

const axiosClient = axios.create({
  baseURL: apiConfig.getBaseUrl(),
  timeout: apiConfig.get().timeout,
  withCredentials: apiConfig.get().withCredentials,
  params: {
    'api-version': apiConfig.get().version
  }
})
```

## Configuration Files

### Development (.env)
```bash
# Development configuration
VITE_API_BASE_URL=http://localhost:5000/api

# Production configuration (uncomment when deploying)
# VITE_API_BASE_URL=/api
```

### Production
In production, the system automatically uses relative paths (`/api`) unless explicitly configured otherwise.

## Best Practices

1. **Environment-specific configuration**: Use `.env` files for different environments
2. **Fallback handling**: The system provides sensible defaults for all environments
3. **Consistent URL handling**: All API calls go through the centralized configuration
4. **Version management**: API version is centrally managed through configuration

## Troubleshooting

### Common Issues

1. **API calls failing**: Check that `VITE_API_BASE_URL` matches your backend configuration
2. **CORS errors**: Ensure the backend allows requests from your frontend origin
3. **Timeout errors**: Increase `VITE_API_TIMEOUT` value if needed

### Debugging
```typescript
// Log current configuration
console.log('API Config:', apiConfig.get())

// Check environment
console.log('Is Development:', apiConfig.isDevelopment())
console.log('Is Production:', apiConfig.isProduction())
```