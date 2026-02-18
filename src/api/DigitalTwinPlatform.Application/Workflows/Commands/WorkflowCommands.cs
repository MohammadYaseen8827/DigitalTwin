using MediatR;
using DigitalTwinPlatform.Application.Workflows.Models;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.Application.Workflows.Queries
{
    public class GetAllWorkflowsQuery : IRequest<IEnumerable<WorkflowDefinitionDto>>
    {
    }

    public class GetWorkflowByIdQuery : IRequest<WorkflowDefinitionDto?>
    {
        public string Id { get; set; }

        public GetWorkflowByIdQuery(string id)
        {
            Id = id;
        }
    }
}

namespace DigitalTwinPlatform.Application.Workflows.Commands
{
    public class CreateWorkflowCommand : IRequest<WorkflowDefinitionDto>
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        public WorkflowTriggerDto Trigger { get; set; } = new();
        public IEnumerable<WorkflowActionDto> Actions { get; set; } = new List<WorkflowActionDto>();
        public WorkflowTargetDto Target { get; set; } = new();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class UpdateWorkflowCommand : IRequest<WorkflowDefinitionDto?>
    {
        public string Id { get; set; } = string.Empty;
        
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public bool? Enabled { get; set; }
        public WorkflowTriggerDto? Trigger { get; set; }
        public IEnumerable<WorkflowActionDto>? Actions { get; set; }
        public WorkflowTargetDto? Target { get; set; }
        public IEnumerable<string>? Tags { get; set; }
    }

    public class DeleteWorkflowCommand : IRequest
    {
        public string Id { get; set; }

        public DeleteWorkflowCommand(string id)
        {
            Id = id;
        }
    }

    public class ToggleWorkflowCommand : IRequest<WorkflowDefinitionDto?>
    {
        public string Id { get; set; }

        public ToggleWorkflowCommand(string id)
        {
            Id = id;
        }
    }

    public class DuplicateWorkflowCommand : IRequest<WorkflowDefinitionDto>
    {
        public string SourceWorkflowId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
        public string? NewDescription { get; set; }
    }
}