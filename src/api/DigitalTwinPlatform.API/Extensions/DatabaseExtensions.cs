using DigitalTwinPlatform.Domain.Entities.Auth;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace DigitalTwinPlatform.API.Extensions;

/// <summary>
/// Extension methods for database operations including migrations and seeding.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Applies database migrations with retry logic.
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(this IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DigitalTwinDbContext>();
        
        await ApplyMigrationsWithRetryAsync(dbContext, logger, cancellationToken);
    }

    /// <summary>
    /// Seeds the database with initial data.
    /// </summary>
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        
        await serviceProvider.SeedIdentityAsync(logger, cancellationToken);
    }



    /// <summary>
    /// Applies migrations with retry logic for database connectivity issues.
    /// </summary>
    private static async Task ApplyMigrationsWithRetryAsync(
        DigitalTwinDbContext dbContext,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 5;
        var delay = TimeSpan.FromSeconds(5);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                logger.LogInformation("Applying database migrations (attempt {Attempt}/{MaxAttempts})", attempt, maxAttempts);
                await dbContext.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied successfully");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Database migration attempt {Attempt} failed. Retrying in {Delay}s...", attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        // Final attempt - let exception bubble up
        logger.LogInformation("Final database migration attempt");
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// Seeds identity roles and users only.
    /// </summary>
    private static async Task SeedIdentityAsync(this IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var env = serviceProvider.GetService<IHostEnvironment>();

        // Seed Roles
        var roles = new[] { "Administrator", "Engineer", "Operator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                try
                {
                    logger.LogInformation("Creating role: {Role}", role);
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Could not create role {Role}, it might already exist", role);
                }
            }
        }

        // Seed default admin user only in Development to avoid predictable credentials in production
        if (env?.IsDevelopment() != true)
            return;

        if (await userManager.FindByEmailAsync("admin@example.com") == null)
        {
            try
            {
                logger.LogInformation("Creating default admin user...");
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "System Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                    await userManager.AddToRoleAsync(admin, "Engineer");
                    logger.LogInformation("Admin user created successfully");
                }
                else
                {
                    logger.LogWarning("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not create admin user, it might already exist");
            }
        }
    }

    /// <summary>
    /// Executes SQL seed script.
    /// </summary>
    private static async Task ExecuteSeedScriptAsync(IServiceProvider serviceProvider, ILogger logger, CancellationToken cancellationToken)
    {
        try
        {
            var dbContext = serviceProvider.GetRequiredService<DigitalTwinDbContext>();
            var seedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Infrastructure", "Persistence", "Migrations", "SeedData", "ExtendedSeedDataUnified.sql");
            
            if (!File.Exists(seedFilePath))
            {
                logger.LogWarning("Seed file not found at {Path}", seedFilePath);
                return;
            }

            logger.LogInformation("Executing seed script: {Path}", seedFilePath);
            var seedSql = await File.ReadAllTextAsync(seedFilePath, cancellationToken);
            await dbContext.Database.ExecuteSqlRawAsync(seedSql, cancellationToken);
            
            logger.LogInformation("Seed script executed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute seed script");
        }
    }
}