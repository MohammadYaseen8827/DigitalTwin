using DigitalTwinPlatform.Application.Abstractions.Tenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DigitalTwinPlatform.Infrastructure.Tenancy;

public class TenantService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration) : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection missing");
    private static readonly AsyncLocal<string?> TenantContext = new();

    public string GetCurrentTenantId()
    {
        if (!string.IsNullOrWhiteSpace(TenantContext.Value))
        {
            return TenantContext.Value!;
        }

        var ctx = _httpContextAccessor.HttpContext;
        return ctx?.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "public";
    }

    public async Task EnsureTenantSchemaAsync(string tenantId)
    {
        if (tenantId.Equals("public", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        var safeTenant = tenantId.Replace("\"", string.Empty);
        var schemaName = $"tenant_{safeTenant}";
        await using var cmd = new NpgsqlCommand($@"CREATE SCHEMA IF NOT EXISTS ""{schemaName}"";", conn);
        await cmd.ExecuteNonQueryAsync();
    }

    public void SetTenantContext(string tenantId)
    {
        TenantContext.Value = tenantId;
    }
}
