using DigitalTwinPlatform.Application.Abstractions.Tenancy;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using System.Data.Common;

namespace DigitalTwinPlatform.Infrastructure.Tenancy;

public class TenantSchemaInterceptor(ITenantService tenantService) : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        ApplySchema(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
    {
        ApplySchema(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    private void ApplySchema(DbCommand command)
    {
        if (command is not NpgsqlCommand npgsqlCommand)
        {
            return;
        }

        var tenantId = tenantService.GetCurrentTenantId();
        var searchPath = tenantId.Equals("public", StringComparison.OrdinalIgnoreCase)
            ? "public"
            : $"tenant_{tenantId}, public";

        npgsqlCommand.CommandText = $"SET search_path TO {searchPath}; {command.CommandText}";
    }
}
