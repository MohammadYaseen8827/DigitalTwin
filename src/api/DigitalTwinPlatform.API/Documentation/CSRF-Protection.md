# CSRF Protection Implementation Guide

## Overview
This document describes the CSRF (Cross-Site Request Forgery) protection implementation for the Digital Twin Platform API.

## Implementation Details

### Backend Configuration

**1. Service Registration** (`Program.cs`)
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.FormFieldName = "__RequestVerificationToken";
    options.Cookie.Name = "XSRF-TOKEN";
    options.Cookie.HttpOnly = false; // Allow JavaScript access for SPA
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
```

**2. Middleware Pipeline**
Added `app.UseAntiforgery()` after `UseRouting()` and before authentication middleware.

**3. Controller Protection**
State-changing endpoints (POST, PUT, DELETE) are decorated with `[ValidateAntiForgeryToken]` attribute.

### Token Endpoint
A dedicated `/api/AntiForgery/tokens` endpoint provides CSRF tokens for SPA applications.

## Frontend Integration

### Token Retrieval
Frontend applications should call `/api/AntiForgery/tokens` to get the required CSRF token.

### Token Usage
Include the token in request headers:
```javascript
headers: {
    'X-CSRF-TOKEN': csrfToken
}
```

## Security Considerations

- Tokens are regenerated per session
- SameSite=Strict cookies prevent cross-origin requests
- Header-based validation suitable for SPA applications
- Automatic validation for form submissions

## Testing
Use the `/api/AntiForgery/validate` endpoint to test token validity during development.