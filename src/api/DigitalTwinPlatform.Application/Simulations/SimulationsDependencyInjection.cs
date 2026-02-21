using Microsoft.Extensions.DependencyInjection;
using DigitalTwinPlatform.Application.Simulations.Services;

namespace DigitalTwinPlatform.Application.Simulations
{
    public static class SimulationsDependencyInjection
    {
        public static IServiceCollection AddSimulationServices(this IServiceCollection services)
        {
            services.AddScoped<ISimulationService, SimulationService>();
            
            // Register other simulation services if needed
            
            return services;
        }
    }
}
