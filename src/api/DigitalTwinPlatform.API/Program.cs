using System.Text.Json.Serialization;
using DigitalTwinPlatform.API.Extensions;
using DigitalTwinPlatform.API.Hubs;
using DigitalTwinPlatform.API.Infrastructure;
using DigitalTwinPlatform.API.Middleware;
using DigitalTwinPlatform.Application;
using DigitalTwinPlatform.Infrastructure;
using FluentValidation.AspNetCore;
using FluentValidation;

// Configure and build the web application
var builder = WebApplication.CreateBuilder(args);

// Configure core services with camelCase JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAutoMapper(typeof(Program));

// Configure observability
builder.Services.AddStructuredLogging(builder.Configuration);
builder.Services.AddApplicationHealthChecks(builder.Configuration);
builder.Services.AddApplicationMetrics();

// Configure security services
builder.Services.AddSecurityServices(builder.Configuration);

// Configure infrastructure
builder.Services.AddCorsConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddSignalRServices(builder.Configuration);
builder.Services.AddApiVersioningConfiguration();

// Add application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configure application-specific services
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);

// Configure Swagger
builder.Services.AddSwaggerDocumentation();

// Build the application
var app = builder.Build();

// Initialize database
//await app.Services.InitializeDatabaseAsync(app.Services.GetRequiredService<ILogger<Program>>());

// Configure middleware pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseDevelopmentMiddleware();
app.UseMiddleware<TenantContextMiddleware>();
app.UseStructuredLogging();
app.UseApplicationMetrics();

// Expose Swagger in all environments
app.UseSwaggerDocumentation();

// Performance monitoring middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();
app.UseSecurityMiddleware();
app.UseCoreMiddleware();
app.MapHealthCheckEndpoints();
app.MapApplicationEndpoints();

// Map SignalR hubs
app.MapHub<TelemetryHub>("/hubs/telemetry");
app.MapHub<RealTimeAnalyticsHub>("/hubs/analytics");

app.Run();

/// <summary>
/// Entry point class for the Digital Twin Platform API.
/// Exposed as partial to allow WebApplicationFactory for integration testing.
/// </summary>
public partial class Program { }
