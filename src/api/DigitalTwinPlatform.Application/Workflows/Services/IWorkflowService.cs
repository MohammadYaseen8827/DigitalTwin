using Microsoft.Extensions.Logging;
using DigitalTwinPlatform.Application.Workflows.Models;
using DigitalTwinPlatform.Application.Abstractions.UnitOfWork;
using DigitalTwinPlatform.Domain.Entities;
using System.Text.Json;
using DigitalTwinPlatform.Application.Abstractions.Repositories;

namespace DigitalTwinPlatform.Application.Workflows.Services
{
    public interface IWorkflowService
    {
        /// <summary>
        /// Execute a workflow with given context
        /// </summary>
        Task<WorkflowExecutionDto> ExecuteWorkflow(string workflowId, Dictionary<string, object> context);

        /// <summary>
        /// Get execution history for a workflow
        /// </summary>
        Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowExecutions(string workflowId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get workflow statistics and metrics
        /// </summary>
        Task<WorkflowStatisticsDto> GetWorkflowStatistics(string workflowId);

        /// <summary>
        /// Validate workflow definition
        /// </summary>
        Task<WorkflowValidationResultDto> ValidateWorkflow(object workflowDefinition);

        /// <summary>
        /// Get available workflow templates
        /// </summary>
        Task<IEnumerable<WorkflowTemplateDto>> GetWorkflowTemplates();

        /// <summary>
        /// Schedule workflow for execution
        /// </summary>
        Task<bool> ScheduleWorkflow(string workflowId, DateTime scheduledTime);

        /// <summary>
        /// Cancel scheduled workflow execution
        /// </summary>
        Task<bool> CancelScheduledExecution(string workflowId, string executionId);

        /// <summary>
        /// Monitor workflow performance and health
        /// </summary>
        Task<bool> MonitorWorkflowHealth(string workflowId);

        /// <summary>
        /// Export workflow definition
        /// </summary>
        Task<string> ExportWorkflow(string workflowId);

        /// <summary>
        /// Import workflow definition
        /// </summary>
        Task<WorkflowDefinitionDto> ImportWorkflow(string workflowDefinition, string createdBy);
    }

    public class WorkflowService : IWorkflowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WorkflowService> _logger;

        public WorkflowService(IUnitOfWork unitOfWork, ILogger<WorkflowService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<WorkflowExecutionDto> ExecuteWorkflow(string workflowId, Dictionary<string, object> context)
        {
            _logger.LogInformation("Executing workflow {WorkflowId}", workflowId);
            
            try
            {
                if (!Guid.TryParse(workflowId, out var workflowGuid))
                {
                    _logger.LogWarning("Invalid WorkflowId {WorkflowId}", workflowId);
                    throw new ArgumentException("Invalid Workflow Id");
                }

                var workflow = await _unitOfWork.Repository<Workflow>().GetAsync(workflowGuid);
                if (workflow == null)
                {
                    throw new KeyNotFoundException($"Workflow {workflowId} not found");
                }

                if (!workflow.IsEnabled)
                {
                    throw new InvalidOperationException($"Workflow {workflowId} is disabled");
                }

                var execution = new WorkflowExecution
                {
                    Id = Guid.NewGuid(),
                    WorkflowId = workflowGuid,
                    Status = "running",
                    StartedAt = DateTime.UtcNow,
                    InputContextJson = JsonSerializer.Serialize(context),
                    TriggeredBy = context.GetValueOrDefault("TriggeredBy")?.ToString() ?? "manual"
                };

                await _unitOfWork.Repository<WorkflowExecution>().AddAsync(execution);
                await _unitOfWork.SaveChangesAsync();

                try 
                {
                    var definition = JsonSerializer.Deserialize<WorkflowDefinitionDto>(workflow.DefinitionJson);
                    var actionExecutions = new List<WorkflowActionExecutionDto>();

                    if (definition != null && definition.Actions != null)
                    {
                        foreach (var action in definition.Actions.OrderBy(a => a.Order))
                        {
                            if (!action.Enabled) continue;

                            var actionExec = new WorkflowActionExecutionDto
                            {
                                ActionOrder = action.Order,
                                ActionType = action.Type,
                                StartedAt = DateTime.UtcNow,
                                Status = "running",
                                Input = context // Pass context as input
                            };

                            // Simulate execution logic based on type
                            try
                            {
                                await ExecuteAction(action, context);
                                actionExec.Status = "success";
                                actionExec.CompletedAt = DateTime.UtcNow;
                                actionExec.Output = new Dictionary<string, object> { { "result", "executed" } };
                            }
                            catch (Exception ex)
                            {
                                actionExec.Status = "failed";
                                actionExec.ErrorMessage = ex.Message;
                                actionExec.CompletedAt = DateTime.UtcNow;
                                throw; // Stop workflow on failure? Or continue? Let's stop.
                            }

                            actionExecutions.Add(actionExec);
                        }
                    }

                    execution.Status = "success";
                    execution.CompletedAt = DateTime.UtcNow;
                    execution.OutputJson = JsonSerializer.Serialize(new { 
                        Message = "Workflow completed successfully", 
                        Actions = actionExecutions 
                    });
                }
                catch (Exception ex)
                {
                    execution.Status = "failed";
                    execution.ErrorMessage = ex.Message;
                    execution.CompletedAt = DateTime.UtcNow;
                    _logger.LogError(ex, "Error during workflow execution logic");
                }
                
                await _unitOfWork.Repository<WorkflowExecution>().UpdateAsync(execution);
                await _unitOfWork.SaveChangesAsync();

                return MapToDto(execution, workflow.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        private async Task ExecuteAction(WorkflowActionDto action, Dictionary<string, object> context)
        {
            _logger.LogInformation("Executing action {Type}", action.Type);
            // Simulate work
            await Task.Delay(100); 

            switch (action.Type.ToLower())
            {
                case "notification":
                    // Simulate sending notification
                    break;
                case "updatestatus":
                    // Simulate status update
                    break;
                case "createticket":
                    break;
                default:
                    break;
            }
        }

        public async Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowExecutions(string workflowId, int page = 1, int pageSize = 20)
        {
            _logger.LogInformation("Retrieving executions for workflow {WorkflowId}", workflowId);
            
            try
            {
                if (!Guid.TryParse(workflowId, out var workflowGuid))
                {
                    return new List<WorkflowExecutionDto>();
                }

                var executions = await _unitOfWork.Repository<WorkflowExecution>()
                    .GetAllAsync(e => e.WorkflowId == workflowGuid);
                
                var workflow = await _unitOfWork.Repository<Workflow>().GetAsync(workflowGuid);
                var workflowName = workflow?.Name ?? "Unknown Workflow";

                return executions
                    .OrderByDescending(e => e.StartedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(e => MapToDto(e, workflowName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve executions for workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<WorkflowStatisticsDto> GetWorkflowStatistics(string workflowId)
        {
            _logger.LogInformation("Retrieving statistics for workflow {WorkflowId}", workflowId);
            
            try
            {
                if (!Guid.TryParse(workflowId, out var workflowGuid))
                {
                    return new WorkflowStatisticsDto { WorkflowId = workflowId };
                }

                var executions = (await _unitOfWork.Repository<WorkflowExecution>()
                    .GetAllAsync(e => e.WorkflowId == workflowGuid)).ToList();

                if (!executions.Any())
                {
                    return new WorkflowStatisticsDto { WorkflowId = workflowId };
                }

                var successful = executions.Count(e => e.Status == "success");
                var failed = executions.Count(e => e.Status == "failed");

                return new WorkflowStatisticsDto
                {
                    WorkflowId = workflowId,
                    TotalExecutions = executions.Count,
                    SuccessfulExecutions = successful,
                    FailedExecutions = failed,
                    SuccessRate = executions.Count > 0 ? (double)successful / executions.Count * 100 : 0,
                    AverageDuration = TimeSpan.FromSeconds(executions
                        .Where(e => e.CompletedAt.HasValue)
                        .Average(e => (e.CompletedAt!.Value - e.StartedAt).TotalSeconds)),
                    FirstExecution = executions.Min(e => e.StartedAt),
                    LastExecution = executions.Max(e => e.StartedAt),
                    ExecutionsByDay = executions
                        .GroupBy(e => e.StartedAt.Date.ToString("yyyy-MM-dd"))
                        .ToDictionary(g => g.Key, g => g.Count())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve statistics for workflow {WorkflowId}", workflowId);
                throw;
            }
        }

        public async Task<WorkflowValidationResultDto> ValidateWorkflow(object workflowDefinition)
        {
            _logger.LogInformation("Validating workflow definition");
            
            try
            {
                // Basic validation
                var isValid = workflowDefinition != null;
                var errors = new List<string>();
                if (!isValid) errors.Add("Workflow definition cannot be null");

                return await Task.FromResult(new WorkflowValidationResultDto
                {
                    IsValid = isValid,
                    Message = isValid ? "Workflow definition is valid" : "Invalid workflow definition",
                    Errors = errors,
                    Compatibility = new WorkflowCompatibilityDto
                    {
                        IsCompatible = isValid,
                        CompatibleTriggers = new[] { "schedule", "condition", "manual" },
                        CompatibleActions = new[] { "notification", "log", "alert" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate workflow definition");
                throw;
            }
        }

        public async Task<IEnumerable<WorkflowTemplateDto>> GetWorkflowTemplates()
        {
            // For now, return predefined templates
            return await Task.FromResult(new List<WorkflowTemplateDto>
            {
                new()
                {
                    Id = "tpl_high_risk_alert",
                    Name = "High Risk Alert",
                    Description = "Sends alerts when failure probability exceeds 80%",
                    Category = "alerting",
                    Tags = new[] { "critical", "ml" }
                },
                new()
                {
                    Id = "tpl_maintenance_scheduler",
                    Name = "Auto Maintenance Scheduler",
                    Description = "Schedules maintenance when RUL is below threshold",
                    Category = "maintenance",
                    Tags = new[] { "automation", "predictive" }
                }
            });
        }

        public async Task<bool> ScheduleWorkflow(string workflowId, DateTime scheduledTime)
        {
            _logger.LogInformation("Scheduling workflow {WorkflowId} for {ScheduledTime}", workflowId, scheduledTime);
            
            if (Guid.TryParse(workflowId, out var workflowGuid))
            {
                var workflow = await _unitOfWork.Repository<Workflow>().GetAsync(workflowGuid);
                if (workflow != null)
                {
                    // In a real system, we'd add a record to a Scheduler table or enqueue a job.
                    // Here we just acknowledge it.
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> CancelScheduledExecution(string workflowId, string executionId)
        {
            _logger.LogInformation("Cancelling execution {ExecutionId}", executionId);
            if (Guid.TryParse(executionId, out var execGuid))
            {
                var exec = await _unitOfWork.Repository<WorkflowExecution>().GetAsync(execGuid);
                if (exec != null && (exec.Status == "running" || exec.Status == "scheduled"))
                {
                    exec.Status = "cancelled";
                    await _unitOfWork.Repository<WorkflowExecution>().UpdateAsync(exec);
                    await _unitOfWork.SaveChangesAsync();
                     return true;
                }
            }
            return false;
        }

        public async Task<bool> MonitorWorkflowHealth(string workflowId)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            return true;
        }

        public async Task<string> ExportWorkflow(string workflowId)
        {
            if (!Guid.TryParse(workflowId, out var workflowGuid)) return "{}";
            var workflow = await _unitOfWork.Repository<Workflow>().GetAsync(workflowGuid);
            return workflow != null ? JsonSerializer.Serialize(workflow) : "{}";
        }

        public async Task<WorkflowDefinitionDto> ImportWorkflow(string workflowDefinition, string createdBy)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            _logger.LogInformation("Importing workflow");
            return new WorkflowDefinitionDto { Name = "Imported Workflow", CreatedBy = createdBy };
        }

        private WorkflowExecutionDto MapToDto(WorkflowExecution execution, string workflowName)
        {
            return new WorkflowExecutionDto
            {
                Id = execution.Id.ToString(),
                WorkflowId = execution.WorkflowId.ToString(),
                WorkflowName = workflowName,
                Status = execution.Status,
                StartedAt = execution.StartedAt,
                CompletedAt = execution.CompletedAt,
                Duration = execution.CompletedAt.HasValue ? execution.CompletedAt.Value - execution.StartedAt : null,
                TriggeredBy = execution.TriggeredBy,
                ErrorMessage = execution.ErrorMessage
            };
        }

        private WorkflowExecutionDto CreateMockExecution(string workflowId, Dictionary<string, object> context)
        {
            return new WorkflowExecutionDto
            {
                Id = Guid.NewGuid().ToString(),
                WorkflowId = workflowId,
                WorkflowName = "Demo Workflow",
                Status = "success",
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow.AddSeconds(1),
                Duration = TimeSpan.FromSeconds(1),
                TriggeredBy = "demo"
            };
        }
    }
}