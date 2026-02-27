using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinPlatform.Application.Workflows.Models;

public class ExecuteWorkflowDto
{
    public Dictionary<string, object> Context { get; set; } = new();
}

public class DuplicateWorkflowDto
{
    [Required]
    public string NewName { get; set; } = string.Empty;
    
    public string? NewDescription { get; set; }
}

public class ValidateWorkflowDto
{
    [Required]
    public object Definition { get; set; } = new {};
}

public class WorkflowDefinitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Status { get; set; } = string.Empty;
    public WorkflowTriggerDto? Trigger { get; set; }
    public List<WorkflowActionDto> Actions { get; set; } = new();
    public WorkflowTargetDto? Target { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime? NextRun { get; set; }
    public int ExecutionCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class WorkflowTriggerDto
{
    public string Type { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public WorkflowConditionDto? Condition { get; set; }
    public string? EventType { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class WorkflowConditionDto
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string? DataType { get; set; }
}

public class WorkflowActionDto
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public int Order { get; set; }
    public bool Enabled { get; set; }
}

public class WorkflowTargetDto
{
    public string Type { get; set; } = string.Empty;
    public List<string> MachineIds { get; set; } = new();
    public List<string> MachineTypes { get; set; } = new();
    public List<string> Statuses { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public Dictionary<string, object> Filters { get; set; } = new();
}

public class WorkflowExecutionDto
{
    public string Id { get; set; } = string.Empty;
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public Dictionary<string, object> InputContext { get; set; } = new();
    public List<WorkflowActionExecutionDto> ActionExecutions { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string TriggeredBy { get; set; } = string.Empty;
}

public class WorkflowActionExecutionDto
{
    public int ActionOrder { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object> Output { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class WorkflowStatisticsDto
{
    public string WorkflowId { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public double SuccessRate { get; set; }
    public TimeSpan? AverageDuration { get; set; }
    public DateTime FirstExecution { get; set; }
    public DateTime LastExecution { get; set; }
    public Dictionary<string, int> ExecutionsByDay { get; set; } = new();
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}

public class WorkflowTemplateDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public WorkflowDefinitionDto? Definition { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class WorkflowValidationResultDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public WorkflowCompatibilityDto? Compatibility { get; set; }
}

public class WorkflowCompatibilityDto
{
    public bool IsCompatible { get; set; }
    public string[] CompatibleTriggers { get; set; } = Array.Empty<string>();
    public string[] CompatibleActions { get; set; } = Array.Empty<string>();
    public string[] RequiredPermissions { get; set; } = Array.Empty<string>();
}