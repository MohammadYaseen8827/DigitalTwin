using System.Text.Json.Serialization;

namespace DigitalTwinPlatform.Application.Workflows.Models
{
    public class WorkflowDefinitionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // maintenance, monitoring, reporting, alerting
        public bool Enabled { get; set; }
        public string Status { get; set; } = string.Empty; // active, inactive, error
        public WorkflowTriggerDto Trigger { get; set; } = new();
        public IEnumerable<WorkflowActionDto> Actions { get; set; } = new List<WorkflowActionDto>();
        public WorkflowTargetDto Target { get; set; } = new();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public int ExecutionCount { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    public class WorkflowTriggerDto
    {
        public string Type { get; set; } = string.Empty; // schedule, condition, event
        public string? CronExpression { get; set; }
        public WorkflowConditionDto? Condition { get; set; }
        public string? EventType { get; set; }
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    public class WorkflowConditionDto
    {
        public string Field { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty; // equals, greater_than, less_than, contains
        public object Value { get; set; } = new {};
        public string? DataType { get; set; } // string, number, boolean, datetime
    }

    public class WorkflowActionDto
    {
        public string Type { get; set; } = string.Empty; // notification, update_status, create_ticket, execute_script
        public Dictionary<string, object> Configuration { get; set; } = new();
        public int Order { get; set; }
        public bool Enabled { get; set; } = true;
    }

    public class WorkflowTargetDto
    {
        public string Type { get; set; } = string.Empty; // all, specific, by_type, by_status, by_location
        public IEnumerable<string> MachineIds { get; set; } = new List<string>();
        public IEnumerable<string> MachineTypes { get; set; } = new List<string>();
        public IEnumerable<string> Statuses { get; set; } = new List<string>();
        public IEnumerable<string> Locations { get; set; } = new List<string>();
        public Dictionary<string, object> Filters { get; set; } = new();
    }

    public class WorkflowExecutionDto
    {
        public string Id { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
        public string WorkflowName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // success, failed, running, cancelled
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public Dictionary<string, object> InputContext { get; set; } = new();
        public IEnumerable<WorkflowActionExecutionDto> ActionExecutions { get; set; } = new List<WorkflowActionExecutionDto>();
        public string? ErrorMessage { get; set; }
        public string TriggeredBy { get; set; } = string.Empty; // system, user, event
    }

    public class WorkflowActionExecutionDto
    {
        public int ActionOrder { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // success, failed, skipped
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
        public TimeSpan AverageDuration { get; set; }
        public DateTime FirstExecution { get; set; }
        public DateTime LastExecution { get; set; }
        public Dictionary<string, int> ExecutionsByDay { get; set; } = new();
        public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
    }

    public class WorkflowValidationResultDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Errors { get; set; } = new List<string>();
        public IEnumerable<string> Warnings { get; set; } = new List<string>();
        public WorkflowCompatibilityDto Compatibility { get; set; } = new();
    }

    public class WorkflowCompatibilityDto
    {
        public bool IsCompatible { get; set; }
        public IEnumerable<string> CompatibleTriggers { get; set; } = new List<string>();
        public IEnumerable<string> CompatibleActions { get; set; } = new List<string>();
        public IEnumerable<string> RequiredPermissions { get; set; } = new List<string>();
    }

    public class WorkflowTemplateDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // maintenance, monitoring, alerting
        public WorkflowDefinitionDto Definition { get; set; } = new();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public int UsageCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }

    // Enums for better type safety
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WorkflowType
    {
        Maintenance,
        Monitoring,
        Reporting,
        Alerting,
        Custom
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TriggerType
    {
        Schedule,
        Condition,
        Event,
        Manual
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActionType
    {
        Notification,
        UpdateStatus,
        CreateTicket,
        ExecuteScript,
        LogEvent,
        SendEmail,
        HttpCall
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TargetType
    {
        All,
        Specific,
        ByType,
        ByStatus,
        ByLocation,
        ByMetadata
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExecutionStatus
    {
        Success,
        Failed,
        Running,
        Cancelled,
        Pending
    }
}