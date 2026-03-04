using DigitalTwinPlatform.Infrastructure.Persistence.SeedData;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.API.Extensions;

/// <summary>
/// Extension methods for demo data seeding.
/// </summary>
public static class DemoDataSeedingExtensions
{
    /// <summary>
    /// Seeds demo data using the DemoDataSeeder. Only runs in Development; no-op in Production/Staging.
    /// </summary>
    public static async Task SeedDemoDataAsync(this IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken = default)
    {
        var env = serviceProvider.GetService<IHostEnvironment>();
        if (env != null && !env.IsDevelopment())
        {
            logger.LogInformation("Demo data seeding skipped (only allowed in Development)");
            return;
        }

        try
        {
            var seeder = serviceProvider.GetRequiredService<DemoDataSeeder>();
            await seeder.SeedAsync(cancellationToken);
            logger.LogInformation("Demo data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding demo data");
            throw;
        }
    }
}
