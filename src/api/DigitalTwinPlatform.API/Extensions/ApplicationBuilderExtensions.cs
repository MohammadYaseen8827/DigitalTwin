using DigitalTwinPlatform.API.Hubs;
using DigitalTwinPlatform.API.Middleware;
using Microsoft.AspNetCore.CookiePolicy;

namespace DigitalTwinPlatform.API.Extensions;

/// <summary>
/// Extension methods for configuring the application pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Configures security-related middleware.
    /// </summary>
    public static WebApplication UseSecurityMiddleware(this WebApplication app)
    {
        // Configure secure cookie policy
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            HttpOnly = HttpOnlyPolicy.Always,
            MinimumSameSitePolicy = SameSiteMode.Strict,
        });

        // Enforce HTTPS in production environments
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // Add Antiforgery middleware for CSRF protection
        app.UseAntiforgery();

        return app;
    }

    /// <summary>
    /// Configures routing and core middleware.
    /// </summary>
    public static WebApplication UseCoreMiddleware(this WebApplication app)
    {
        app.UseRouting();
        app.UseCors("DefaultCorsPolicy");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }

    /// <summary>
    /// Configures development-specific middleware.
    /// </summary>
    public static WebApplication UseDevelopmentMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        return app;
    }

    /// <summary>
    /// Maps application endpoints.
    /// </summary>
    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapHub<TelemetryHub>("/hubs/telemetry");
        app.MapHub<RealTimeAnalyticsHub>("/hubs/realtime-analytics");

        return app;
    }

    /// <summary>
    /// Configures Swagger UI middleware for API documentation.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Digital Twin Platform API v1");
        });

        return app;
    }
}