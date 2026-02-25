using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using DigitalTwinPlatform.API.Services.Analytics;
using DigitalTwinPlatform.API.Services.Analytics.ML;
using DigitalTwinPlatform.API.Services.Core;
using DigitalTwinPlatform.API.Services.Infrastructure;
using DigitalTwinPlatform.API.Services.Simulation;
using DigitalTwinPlatform.API.Services.Telemetry;
using DigitalTwinPlatform.API.Services.Maintenance;
using DigitalTwinPlatform.API.Services.Simulation.DegradationModels;
using DigitalTwinPlatform.Application.Abstractions.Services;
using DigitalTwinPlatform.Application.Abstractions.Analytics;
using DigitalTwinPlatform.Infrastructure.Exporters;
using DigitalTwinPlatform.Application.Maintenance;
using DigitalTwinPlatform.Application.Services;
using DigitalTwinPlatform.Application.Tenants.Services;
using Microsoft.AspNetCore.Identity;
using DigitalTwinPlatform.Domain.Entities.Auth;
using DigitalTwinPlatform.Infrastructure.Persistence;
using Asp.Versioning;
using DigitalTwinPlatform.API.Hubs;
using DigitalTwinPlatform.API.Services.MathematicalModeling;
using DigitalTwinPlatform.API.Services.Analytics.Advanced;
using DigitalTwinPlatform.Application.ExternalSystems.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using IHubPublisher = DigitalTwinPlatform.API.Services.Infrastructure.IHubPublisher;

namespace DigitalTwinPlatform.API.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures security-related services including authentication and authorization.
    /// </summary>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<DigitalTwinDbContext>()
            .AddDefaultTokenProviders();

        // Configure JWT Authentication
        ConfigureJwtAuthentication(services, configuration);

        // Configure Antiforgery protection
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.FormFieldName = "__RequestVerificationToken";
            options.Cookie.Name = "XSRF-TOKEN";
            options.Cookie.HttpOnly = false;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        return services;
    }

    /// <summary>
    /// Configures CORS policies with dynamic origin detection.
    /// Allows flexible origins in development and Docker environments.
    /// Permissive policy is never used in production.
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        var isProduction = environment?.IsProduction() ?? false;
        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCorsPolicy", policy =>
            {
                var configEnv = configuration.GetSection("Environment").Value?.ToLower() ?? "development";

                if (!isProduction && (configEnv == "development" || configEnv == "staging"))
                {
                    // Use configured origins for development
                    var allowedOrigins = configuration
                                             .GetSection("Cors:AllowedOrigins")
                                             .Get<string[]>() ??
                                         [
                                             "http://localhost:3000",
                                             "http://127.0.0.1:3000",
                                             "http://localhost:5173",
                                             "http://localhost:5174",
                                             "http://127.0.0.1:5173",
                                             "http://127.0.0.1:5174",
                                             "http://localhost:7000",
                                             "http://127.0.0.1:7000"
                                         ];

                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    // Production: never use permissive. Other environments: allow permissive only if explicitly configured.
                    var corsPolicy = configuration.GetSection("Cors:Policy").Value?.ToLower();
                    var usePermissive = !isProduction && (corsPolicy == "permissive" || corsPolicy == "docker");

                    if (usePermissive)
                    {
                        policy.SetIsOriginAllowed(_ => true);
                    }
                    else
                    {
                        policy.SetIsOriginAllowed(origin =>
                        {
                            if (origin.StartsWith("http://localhost") ||
                                origin.StartsWith("http://127.0.0.1") ||
                                origin.StartsWith("https://localhost") ||
                                origin.StartsWith("https://127.0.0.1"))
                            {
                                return true;
                            }

                            var allowedOrigins = configuration
                                                     .GetSection("Cors:AllowedOrigins")
                                                     .Get<string[]>();

                            return allowedOrigins != null &&
                                   allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                        });
                    }
                }

                policy.AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Configures SignalR services with proper serialization and performance settings.
    /// </summary>
    public static IServiceCollection AddSignalRServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = null;
                options.PayloadSerializerOptions.DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                options.PayloadSerializerOptions.WriteIndented = false;
            })
            .AddHubOptions<RealTimeAnalyticsHub>(options =>
            {
                options.EnableDetailedErrors = configuration.GetValue<bool>("SignalR:EnableDetailedErrors", false);
                options.KeepAliveInterval =
                    TimeSpan.FromSeconds(configuration.GetValue("SignalR:KeepAliveInterval", 15));
                options.HandshakeTimeout = TimeSpan.FromSeconds(configuration.GetValue("SignalR:HandshakeTimeout", 15));
                options.MaximumReceiveMessageSize = configuration.GetValue("SignalR:MaxMessageSize", 64) * 1024;
                options.MaximumParallelInvocationsPerClient =
                    configuration.GetValue("SignalR:MaxParallelInvocations", 10);
            })
            .AddHubOptions<TelemetryHub>(options =>
            {
                options.EnableDetailedErrors = configuration.GetValue<bool>("SignalR:EnableDetailedErrors", false);
                options.KeepAliveInterval =
                    TimeSpan.FromSeconds(configuration.GetValue("SignalR:KeepAliveInterval", 15));
                options.HandshakeTimeout = TimeSpan.FromSeconds(configuration.GetValue("SignalR:HandshakeTimeout", 15));
                options.MaximumReceiveMessageSize = configuration.GetValue("SignalR:MaxMessageSize", 64) * 1024;
            });

        return services;
    }

    /// <summary>
    /// Configures API versioning.
    /// </summary>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI documentation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "Digital Twin Platform API",
                Version = "v1",
                Description = @"## Digital Twin Platform for Predictive Maintenance API

This API provides endpoints for managing machines, telemetry data, RUL predictions, and alerts.

### Key Features:
- **Machine Management**: CRUD operations for industrial machines
- **Telemetry**: Real-time sensor data ingestion and retrieval
- **Predictions**: Remaining Useful Life (RUL) predictions and failure probability analysis
- **Alerts**: Automated alert generation based on prediction thresholds
- **Health Monitoring**: System health and readiness checks

### Authentication:
All endpoints (except health and auth) require JWT Bearer token authentication.
Use the `/api/auth/login` endpoint to obtain tokens.

### Base URL:
```
https://api.digitaltwin.example.com/v1
```
",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Digital Twin Platform Team",
                    Email = "support@digitaltwin.example.com"
                },
                License = new Microsoft.OpenApi.Models.OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add JWT Bearer authentication to Swagger
            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Include XML comments if generated
            var xmlFile = Path.ChangeExtension(typeof(Program).Assembly.Location, ".xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
        });

        return services;
    }

    /// <summary>
    /// Registers application-specific services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Machine configuration service
        services.Configure<ConfigurationOptions>(configuration.GetSection("Configuration"));
        services.AddScoped<IMachineConfigurationService, MachineConfigurationService>();
        services.AddSingleton<IDegradationModelFactory, DegradationModelFactory>();
        services.AddScoped<ITransferFunctionEvaluator, TransferFunctionEvaluator>();
        services.AddScoped<ISensorDataGenerator, SensorDataGenerator>();
        services.AddScoped<IRunToFailureOrchestrator, RunToFailureOrchestrator>();
        services.AddScoped<Application.Services.ISyntheticDataGenerator, Application.Services.SyntheticDataGenerator>();

        // Performance monitoring
        services.Configure<PerformanceOptions>(configuration.GetSection("Performance"));
        services.AddSingleton<IPerformanceMetricsCollector, PerformanceMetricsCollector>();

        // Simulation services
        services.Configure<SimulationOptions>(configuration.GetSection("Simulation"));
        services.AddSingleton<ISimulationEngine, SimulationEngine>();
        services.AddScoped<ISimulationService, SimulationService>();
        services.AddHostedService<SimulationHostedService>();
        services.AddScoped<ISimulationSchedulerService, SimulationSchedulerService>();

        // ML services
        services.Configure<MlOptions>(configuration.GetSection("ML"));
        services.AddSingleton<IRulPredictor, RulPredictor>();
        services.AddSingleton<RulPredictor>(sp => (RulPredictor)sp.GetRequiredService<IRulPredictor>());
        services.AddSingleton<IHealthClassifier, HealthClassifier>();
        services.AddSingleton<HealthClassifier>(sp => (HealthClassifier)sp.GetRequiredService<IHealthClassifier>());
        services.AddScoped<ModelTrainer>();

        // SHAP service for XAI
        services.Configure<ShapServiceOptions>(configuration.GetSection("ShapService"));
        services.AddHttpClient<IShapService, ShapServiceClient>();
        services.AddScoped<IPredictiveXaiService, PredictiveXaiService>();
        services.AddSingleton<IDataValidationService, DataValidationService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddHttpClient<INotificationService, NotificationService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        // Twin Engine Service (used by both API and Application layers)
        services.AddScoped<TwinEngineService>();
        services.AddScoped<DigitalTwinPlatform.Application.Abstractions.Services.ITwinEngineService>(sp =>
            sp.GetRequiredService<TwinEngineService>());
        services.AddScoped<IPredictiveAnalyticsService, PredictiveAnalyticsService>();
        services.AddScoped<PredictionService>();
        services.AddScoped<IPredictionService, PredictionServiceAdapter>();
        services.AddScoped<IModelRetrainingService, ModelRetrainingService>();
        services.AddScoped<IMlExperimentLogger, MlExperimentLogger>();
        services.AddScoped<IModelLifecycleService, ModelLifecycleService>();
        services.AddScoped<IBenchmarkDatasetLoader, BenchmarkDatasetLoader>();
        services.AddScoped<IBenchmarkValidationService, BenchmarkValidationService>();
        services.AddScoped<IPrescriptiveService, PrescriptiveService>();
        services.AddSingleton<IExporter, ParquetExporter>();
        services.AddScoped<IFeatureExtractionService, FeatureExtractionService>();

        // Auth services
        services.AddScoped<DigitalTwinPlatform.API.Services.Auth.RefreshTokenService>();

        // Drift detection services
        services.Configure<DriftServiceOptions>(configuration.GetSection("DriftDetection"));
        services.AddScoped<IDataDriftService, DataDriftService>();

        // Uncertainty quantification services
        services.AddScoped<IUncertaintyQuantificationService, UncertaintyQuantificationService>();

        // Mathematical modeling services
        services.AddScoped<IDifferentialEquationSolver, DifferentialEquationSolver>();
        services.AddScoped<IOptimizationService, OptimizationService>();

        // Advanced analytics services
        services.AddScoped<IAdvancedPredictiveService, AdvancedPredictiveService>();
        services.AddScoped<IPrescriptiveAnalyticsService, PrescriptiveAnalyticsService>();
        services
            .AddScoped<Application.Workflows.Services.IWorkflowService,
                Application.Workflows.Services.WorkflowService>();

        // External System Integration: mock in development; use real implementation for production.
        var environment = configuration.GetSection("Environment").Value?.ToLower() ?? "development";
        if (environment == "development" || environment == "staging")
        {
            services.AddScoped<IExternalSystemService, MockExternalSystemService>();
            // Use mock tenant service for development
            services.AddScoped<Application.Tenants.Services.ITenantService, MockTenantService>();
        }
        else
        {
            // In production, register the real implementation
            services.AddHttpClient<IExternalSystemService, ExternalSystemService>(client =>
            {
                // Configure base address from configuration
                client.BaseAddress = new Uri(configuration["ExternalSystems:BaseUrl"] ??
                                             configuration.GetConnectionString("ExternalSystemsApi") ??
                                             "https://localhost:8080/api/");
            });
            services.AddScoped<IExternalSystemService, ExternalSystemService>();
            // Use mock tenant service
            services
                .AddScoped<ITenantService,
                    MockTenantService>();
        }

        return services;
    }

    /// <summary>
    /// Validates JWT configuration and throws appropriate exceptions.
    /// </summary>
    private static void ValidateJwtConfiguration(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT Key is not configured. Please set Jwt:Key in configuration or environment variables.");
        }

        if (jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Key must be at least 32 characters long for security.");
        }
    }
    /// <summary>
    /// Configures JWT authentication.
    /// </summary>
    private static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        ValidateJwtConfiguration(configuration);

        var jwtKey = configuration["Jwt:Key"];

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
                };
            });
    }
    
    /// <summary>
    /// Configures infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environment">Hosting environment; when not Development, TelemetryMockHostedService is not registered.</param>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        // Hub publishers
        services.AddSingleton<IHubPublisher, HubPublisher>();

        // Data archival
        services.AddScoped<IDataArchivalService, DataArchivalService>();

        // Only seed mock telemetry in Development; production should use real ingestion only.
        if (environment == null || environment.IsDevelopment())
        {
            services.AddHostedService<TelemetryMockHostedService>();
        }

        return services;
    }
}
