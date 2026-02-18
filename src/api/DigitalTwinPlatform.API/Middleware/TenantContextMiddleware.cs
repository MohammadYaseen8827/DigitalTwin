using DigitalTwinPlatform.Application.Abstractions.Tenancy;

namespace DigitalTwinPlatform.API.Middleware;

public class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(tenantId))
        {
            tenantService.SetTenantContext(tenantId);
        }

        await next(context);
    }
}
