using Microsoft.Extensions.DependencyInjection;
using DigitalTwinPlatform.Application.Workflows.Services;
using DigitalTwinPlatform.Application.Workflows.Handlers;
using MediatR;

namespace DigitalTwinPlatform.Application.Workflows
{
    public static class WorkflowsDependencyInjection
    {
        public static IServiceCollection AddWorkflowsServices(this IServiceCollection services)
        {
            // Register workflow service
            services.AddScoped<IWorkflowService, WorkflowService>();

            // Register MediatR handlers for workflows
            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssemblyContaining<WorkflowQueryHandlers>();
                cfg.RegisterServicesFromAssemblyContaining<WorkflowCommandHandlers>();
            });

            return services;
        }
    }
}