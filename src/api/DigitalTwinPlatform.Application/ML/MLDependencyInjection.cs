using Microsoft.Extensions.DependencyInjection;
using DigitalTwinPlatform.Application.ML.Services;
using DigitalTwinPlatform.Application.ML.Handlers;
using DigitalTwinPlatform.Application.ML.Queries;
using DigitalTwinPlatform.Application.ML.Commands;
using DigitalTwinPlatform.Application.Predictions.Models;
using MediatR;

namespace DigitalTwinPlatform.Application.ML
{
    public static class MLDependencyInjection
    {
        public static IServiceCollection AddMLServices(this IServiceCollection services)
        {
            // Register AI Service
            services.AddScoped<IAIService, AIService>();
            
            // Register ML Pipeline Components
            services.AddScoped<FastForestPredictor>();
            services.AddScoped<QuantileRegression>();
            services.AddScoped<ShapExplainer>();
            services.AddScoped<FeatureImportanceExtractor>();
            services.AddScoped<DigitalTwinPlatform.Application.Services.IMLModelService, DigitalTwinPlatform.Application.Services.MLModelService>();

            // Register Query Handlers
            services.AddScoped<IRequestHandler<GetAllAIModelsQuery, IEnumerable<Models.AIModelDto>>, GetAllAIModelsHandler>();
            services.AddScoped<IRequestHandler<GetAIModelByIdQuery, Models.AIModelDto?>, GetAIModelByIdHandler>();
            
            // Register Command Handlers
            services.AddScoped<IRequestHandler<DeployAIModelCommand, Models.AIModelDto>, DeployAIModelHandler>();
            services.AddScoped<IRequestHandler<UpdateAIModelCommand, Models.AIModelDto?>, UpdateAIModelHandler>();
            services.AddScoped<IRequestHandler<DeleteAIModelCommand>, DeleteAIModelHandler>();
            services.AddScoped<IRequestHandler<RetrainModelCommand, Models.AIModelDto?>, RetrainModelHandler>();
            services.AddScoped<IRequestHandler<TrainModelCommand, Models.TrainingResultDto>, TrainModelCommandHandler>();
            
            return services;
        }
    }
}
