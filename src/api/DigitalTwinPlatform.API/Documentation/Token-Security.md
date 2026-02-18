# Secure Token Storage and Transmission

## Overview
This document describes the secure token handling implementation including HTTPS enforcement, secure cookies, and token refresh mechanisms.

## Implementation Details

### HTTPS Enforcement
- Automatic HTTPS redirection in production environments
- Secure cookie flags (HttpOnly, SameSite, Secure)
- Cookie policy configuration for enhanced security

### Token Refresh Mechanism
- Access tokens with configurable expiration (default: 24 hours)
- Refresh tokens with longer lifetime (7 days)
- Automatic token regeneration and storage

### API Endpoints

**Token Refresh**: `POST /api/Token/refresh`
```json
{
    "refreshToken": "refresh-token-string"
}
```

**Token Revocation**: `POST /api/Token/revoke`
```json
{
    "refreshToken": "refresh-token-string"
}
```

## Security Features

### Transport Security
- HTTPS enforced in production
- Secure cookie flags automatically set
- SameSite=Strict policy prevents CSRF

### Token Security
- Access tokens: Short-lived JWT tokens
- Refresh tokens: Longer-lived opaque tokens
- Automatic token invalidation on logout

### Storage Security
- Refresh tokens stored in user database
- User session tracking
- Token expiration enforcement

## Frontend Integration

### Token Management
Frontend applications should:
1. Store refresh token securely (HttpOnly cookie or secure storage)
2. Automatically refresh access tokens before expiration
3. Handle 401 responses by attempting token refresh
4. Clear all tokens on logout

### Error Handling
- 401 Unauthorized: Attempt token refresh
- 400 Bad Request: Invalid token format
- 401 on refresh: Full re-authentication required

## Best Practices Implemented

- Separation of access and refresh token lifecycles
- Automatic HTTPS enforcement
- Secure cookie configuration
- Proper CORS settings
- Comprehensive error handling
- Logging of security events