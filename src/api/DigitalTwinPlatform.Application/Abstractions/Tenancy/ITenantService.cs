namespace DigitalTwinPlatform.Application.Abstractions.Tenancy;

public interface ITenantService
{
    string GetCurrentTenantId();
    Task EnsureTenantSchemaAsync(string tenantId);
    void SetTenantContext(string tenantId);
}
