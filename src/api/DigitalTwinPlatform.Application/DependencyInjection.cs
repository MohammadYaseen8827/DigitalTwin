using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using DigitalTwinPlatform.Application.Behaviors;
using DigitalTwinPlatform.Application.ML;
using DigitalTwinPlatform.Application.Workflows;

namespace DigitalTwinPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add memory cache for caching behavior
        services.AddMemoryCache();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Phase 3: Core behaviors
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            
            // Phase 4: Advanced behaviors
            cfg.AddOpenBehavior(typeof(RetryBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(AuditBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add ML Services
        services.AddMLServices();
        
        // Add Workflows Services
        // Add Workflows Services
        services.AddWorkflowsServices();
        
        // Add Numerical Solvers
        services.AddScoped<DigitalTwinPlatform.Application.Mathematics.RungeKutta>();
        services.AddScoped<DigitalTwinPlatform.Application.Mathematics.IODESolver, DigitalTwinPlatform.Application.Mathematics.RungeKutta>();
        
        // Add Core Services
        services.AddScoped<DigitalTwinPlatform.Application.Services.IParameterEstimation, DigitalTwinPlatform.Application.Services.ParameterEstimationService>();

        return services;
    }
}
