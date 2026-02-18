using MediatR;
using DigitalTwinPlatform.Application.Workflows.Models;
using DigitalTwinPlatform.Application.Workflows.Commands;
using Microsoft.Extensions.Logging;

namespace DigitalTwinPlatform.Application.Workflows.Handlers
{
    public class WorkflowCommandHandlers :
        IRequestHandler<CreateWorkflowCommand, WorkflowDefinitionDto>,
        IRequestHandler<UpdateWorkflowCommand, WorkflowDefinitionDto?>,
        IRequestHandler<DeleteWorkflowCommand>,
        IRequestHandler<ToggleWorkflowCommand, WorkflowDefinitionDto?>,
        IRequestHandler<DuplicateWorkflowCommand, WorkflowDefinitionDto>
    {
        private readonly ILogger<WorkflowCommandHandlers> _logger;

        public WorkflowCommandHandlers(ILogger<WorkflowCommandHandlers> logger)
        {
            _logger = logger;
        }

        public async Task<WorkflowDefinitionDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateWorkflowCommand: {WorkflowName}", request.Name);
            
            await Task.Delay(150); // Simulate database operation
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Workflow name is required");
                
            if (string.IsNullOrWhiteSpace(request.Type))
                throw new ArgumentException("Workflow type is required");

            // Create new workflow
            var workflow = new WorkflowDefinitionDto
            {
                Id = $"wf_{Guid.NewGuid().ToString("N")[..8]}",
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Type = request.Type.ToLower(),
                Enabled = false, // Newly created workflows start disabled
                Status = "inactive",
                Trigger = request.Trigger ?? new WorkflowTriggerDto { Type = "manual" },
                Actions = request.Actions ?? new List<WorkflowActionDto>(),
                Target = request.Target ?? new WorkflowTargetDto { Type = "all" },
                Tags = request.Tags ?? new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = request.CreatedBy ?? "unknown"
            };

            _logger.LogInformation("Created workflow {WorkflowId}: {WorkflowName}", workflow.Id, workflow.Name);
            return workflow;
        }

        public async Task<WorkflowDefinitionDto?> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling UpdateWorkflowCommand for ID: {WorkflowId}", request.Id);
            
            await Task.Delay(100); // Simulate database operation
            
            // In real implementation, this would fetch from database and update
            // For demo purposes, return a modified mock workflow
            
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("Workflow ID is required");

            // Simulate fetching existing workflow
            var existingWorkflow = new WorkflowDefinitionDto
            {
                Id = request.Id,
                Name = "Original Workflow Name",
                Description = "Original description",
                Type = "maintenance",
                Enabled = true,
                Status = "active",
                Trigger = new WorkflowTriggerDto { Type = "schedule" },
                Actions = new List<WorkflowActionDto>(),
                Target = new WorkflowTargetDto { Type = "all" },
                Tags = new List<string>(),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "admin"
            };

            // Apply updates
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingWorkflow.Name = request.Name;
                
            if (request.Description != null)
                existingWorkflow.Description = request.Description;
                
            if (!string.IsNullOrWhiteSpace(request.Type))
                existingWorkflow.Type = request.Type.ToLower();
                
            if (request.Enabled.HasValue)
                existingWorkflow.Enabled = request.Enabled.Value;
                
            if (request.Trigger != null)
                existingWorkflow.Trigger = request.Trigger;
                
            if (request.Actions != null)
                existingWorkflow.Actions = request.Actions;
                
            if (request.Target != null)
                existingWorkflow.Target = request.Target;
                
            if (request.Tags != null)
                existingWorkflow.Tags = request.Tags;

            existingWorkflow.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation("Updated workflow {WorkflowId}", request.Id);
            return existingWorkflow;
        }

        public async Task Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DeleteWorkflowCommand for ID: {WorkflowId}", request.Id);
            
            await Task.Delay(75); // Simulate database operation
            
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("Workflow ID is required");

            // In real implementation, this would delete from database
            _logger.LogInformation("Deleted workflow {WorkflowId}", request.Id);
        }

        public async Task<WorkflowDefinitionDto?> Handle(ToggleWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling ToggleWorkflowCommand for ID: {WorkflowId}", request.Id);
            
            await Task.Delay(50); // Simulate database operation
            
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("Workflow ID is required");

            // Simulate fetching and toggling workflow
            var workflow = new WorkflowDefinitionDto
            {
                Id = request.Id,
                Name = "Toggle Test Workflow",
                Description = "Workflow for toggle testing",
                Type = "test",
                Enabled = false, // Will be toggled
                Status = "active",
                Trigger = new WorkflowTriggerDto { Type = "manual" },
                Actions = new List<WorkflowActionDto>(),
                Target = new WorkflowTargetDto { Type = "all" },
                Tags = new List<string>(),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "tester"
            };

            // Toggle the enabled state
            workflow.Enabled = !workflow.Enabled;
            workflow.Status = workflow.Enabled ? "active" : "inactive";

            _logger.LogInformation("Toggled workflow {WorkflowId} to {Status}", request.Id, workflow.Enabled ? "enabled" : "disabled");
            return workflow;
        }

        public async Task<WorkflowDefinitionDto> Handle(DuplicateWorkflowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DuplicateWorkflowCommand: {SourceId} -> {NewName}", request.SourceWorkflowId, request.NewName);
            
            await Task.Delay(120); // Simulate database operation
            
            if (string.IsNullOrWhiteSpace(request.SourceWorkflowId))
                throw new ArgumentException("Source workflow ID is required");
                
            if (string.IsNullOrWhiteSpace(request.NewName))
                throw new ArgumentException("New workflow name is required");

            // Simulate fetching source workflow
            var sourceWorkflow = new WorkflowDefinitionDto
            {
                Id = request.SourceWorkflowId,
                Name = "Source Workflow",
                Description = "Original workflow description",
                Type = "maintenance",
                Enabled = false,
                Status = "inactive",
                Trigger = new WorkflowTriggerDto { Type = "schedule" },
                Actions = new List<WorkflowActionDto>
                {
                    new()
                    {
                        Type = "notification",
                        Order = 1,
                        Configuration = new Dictionary<string, object>()
                    }
                },
                Target = new WorkflowTargetDto { Type = "all" },
                Tags = new List<string> { "template" },
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                CreatedBy = "admin"
            };

            // Create duplicate with new ID and name
            var duplicatedWorkflow = new WorkflowDefinitionDto
            {
                Id = $"wf_{Guid.NewGuid().ToString("N")[..8]}",
                Name = request.NewName,
                Description = request.NewDescription ?? sourceWorkflow.Description,
                Type = sourceWorkflow.Type,
                Enabled = false, // Duplicated workflows start disabled
                Status = "inactive",
                Trigger = sourceWorkflow.Trigger,
                Actions = sourceWorkflow.Actions.ToList(), // Clone actions
                Target = sourceWorkflow.Target,
                Tags = sourceWorkflow.Tags.ToList(), // Clone tags
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system" // Indicate this was duplicated
            };

            _logger.LogInformation("Duplicated workflow {SourceId} to {NewId}: {NewName}", 
                request.SourceWorkflowId, duplicatedWorkflow.Id, duplicatedWorkflow.Name);
            return duplicatedWorkflow;
        }
    }
}