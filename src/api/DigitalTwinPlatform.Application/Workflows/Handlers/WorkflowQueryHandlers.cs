using MediatR;
using DigitalTwinPlatform.Application.Workflows.Models;
using DigitalTwinPlatform.Application.Workflows.Queries;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Workflows.Handlers
{
    public class WorkflowQueryHandlers :
        IRequestHandler<GetAllWorkflowsQuery, IEnumerable<WorkflowDefinitionDto>>,
        IRequestHandler<GetWorkflowByIdQuery, WorkflowDefinitionDto?>
    {
        private readonly ILogger<WorkflowQueryHandlers> _logger;

        public WorkflowQueryHandlers(ILogger<WorkflowQueryHandlers> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<WorkflowDefinitionDto>> Handle(GetAllWorkflowsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetAllWorkflowsQuery");
            
            await Task.Delay(100); // Simulate database call
            
            // Return mock workflows for demonstration
            return new List<WorkflowDefinitionDto>
            {
                new()
                {
                    Id = "wf_001",
                    Name = "Daily Maintenance Check",
                    Description = "Automated daily maintenance checks for all machines",
                    Type = "maintenance",
                    Enabled = true,
                    Status = "active",
                    Trigger = new WorkflowTriggerDto
                    {
                        Type = "schedule",
                        CronExpression = "0 0 9 * * *"
                    },
                    Actions = new List<WorkflowActionDto>
                    {
                        new()
                        {
                            Type = "notification",
                            Order = 1,
                            Configuration = new Dictionary<string, object>
                            {
                                { "recipients", "maintenance@company.com" },
                                { "subject", "Daily Maintenance Report" }
                            }
                        }
                    },
                    Target = new WorkflowTargetDto
                    {
                        Type = "all"
                    },
                    Tags = new List<string> { "maintenance", "daily", "automated" },
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    LastRun = DateTime.UtcNow.AddDays(-1).AddHours(9),
                    NextRun = DateTime.UtcNow.AddDays(1).AddHours(9),
                    ExecutionCount = 30,
                    CreatedBy = "admin"
                },
                new()
                {
                    Id = "wf_002",
                    Name = "Critical Alert Handler",
                    Description = "Handles critical machine status alerts",
                    Type = "alerting",
                    Enabled = true,
                    Status = "active",
                    Trigger = new WorkflowTriggerDto
                    {
                        Type = "condition",
                        Condition = new WorkflowConditionDto
                        {
                            Field = "status",
                            Operator = "equals",
                            Value = "critical"
                        }
                    },
                    Actions = new List<WorkflowActionDto>
                    {
                        new()
                        {
                            Type = "notification",
                            Order = 1,
                            Configuration = new Dictionary<string, object>
                            {
                                { "recipients", "alerts@company.com,supervisor@company.com" },
                                { "subject", "Critical Machine Alert - {{machine.name}}" }
                            }
                        },
                        new()
                        {
                            Type = "create_ticket",
                            Order = 2,
                            Configuration = new Dictionary<string, object>
                            {
                                { "priority", "high" },
                                { "category", "emergency" }
                            }
                        }
                    },
                    Target = new WorkflowTargetDto
                    {
                        Type = "all"
                    },
                    Tags = new List<string> { "alerting", "critical", "realtime" },
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    LastRun = DateTime.UtcNow.AddMinutes(-30),
                    ExecutionCount = 12,
                    CreatedBy = "system"
                },
                new()
                {
                    Id = "wf_003",
                    Name = "Weekly Performance Report",
                    Description = "Generates weekly performance reports",
                    Type = "reporting",
                    Enabled = false,
                    Status = "inactive",
                    Trigger = new WorkflowTriggerDto
                    {
                        Type = "schedule",
                        CronExpression = "0 0 12 * * 1"
                    },
                    Actions = new List<WorkflowActionDto>
                    {
                        new()
                        {
                            Type = "generate_report",
                            Order = 1,
                            Configuration = new Dictionary<string, object>
                            {
                                { "report_type", "weekly_performance" },
                                { "format", "pdf" }
                            }
                        },
                        new()
                        {
                            Type = "send_email",
                            Order = 2,
                            Configuration = new Dictionary<string, object>
                            {
                                { "recipients", "management@company.com" },
                                { "subject", "Weekly Performance Report" }
                            }
                        }
                    },
                    Target = new WorkflowTargetDto
                    {
                        Type = "by_type",
                        MachineTypes = new List<string> { "cnc", "press", "injection_molder" }
                    },
                    Tags = new List<string> { "reporting", "weekly", "performance" },
                    CreatedAt = DateTime.UtcNow.AddDays(-45),
                    UpdatedAt = DateTime.UtcNow.AddDays(-7),
                    ExecutionCount = 6,
                    CreatedBy = "analyst"
                }
            };
        }

        public async Task<WorkflowDefinitionDto?> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GetWorkflowByIdQuery for ID: {WorkflowId}", request.Id);
            
            await Task.Delay(50); // Simulate database call
            
            // In real implementation, this would fetch from database
            // For now, return one of the mock workflows based on ID
            var allWorkflows = await Handle(new GetAllWorkflowsQuery(), cancellationToken);
            return allWorkflows.FirstOrDefault(w => w.Id == request.Id);
        }
    }
}