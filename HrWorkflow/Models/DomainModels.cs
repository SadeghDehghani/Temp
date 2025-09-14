using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HrWorkflow.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(200), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ApproverGroup
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ApproverGroupMember> Members { get; set; } = new List<ApproverGroupMember>();
    }

    public class ApproverGroupMember
    {
        public int ApproverGroupId { get; set; }
        public ApproverGroup ApproverGroup { get; set; } = null!;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
    }

    public class RequestType
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    public class Request
    {
        public int Id { get; set; }

        public int RequestTypeId { get; set; }
        public RequestType RequestType { get; set; } = null!;

        public int RequesterEmployeeId { get; set; }
        public Employee RequesterEmployee { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Content { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int? WorkflowInstanceId { get; set; }
        public WorkflowInstance? WorkflowInstance { get; set; }
    }

    public enum WorkflowStepType
    {
        Start = 0,
        Task = 1,
        Decision = 2,
        End = 3
    }

    public class WorkflowDefinition
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int RequestTypeId { get; set; }
        public RequestType RequestType { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ICollection<WorkflowStepDefinition> Steps { get; set; } = new List<WorkflowStepDefinition>();
        public ICollection<WorkflowTransitionDefinition> Transitions { get; set; } = new List<WorkflowTransitionDefinition>();
    }

    public class WorkflowStepDefinition
    {
        public int Id { get; set; }
        public int WorkflowDefinitionId { get; set; }
        public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public WorkflowStepType StepType { get; set; } = WorkflowStepType.Task;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<WorkflowTransitionDefinition> OutgoingTransitions { get; set; } = new List<WorkflowTransitionDefinition>();
        public ICollection<WorkflowTransitionDefinition> IncomingTransitions { get; set; } = new List<WorkflowTransitionDefinition>();
    }

    public class WorkflowTransitionDefinition
    {
        public int Id { get; set; }
        public int WorkflowDefinitionId { get; set; }
        public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

        public int FromStepId { get; set; }
        public WorkflowStepDefinition FromStep { get; set; } = null!;

        public int ToStepId { get; set; }
        public WorkflowStepDefinition ToStep { get; set; } = null!;

        [Required, MaxLength(100)]
        public string ActionName { get; set; } = "Approve"; // e.g., Approve, Reject, Return

        [MaxLength(1000)]
        public string? ConditionExpression { get; set; } // optional expression to evaluate

        // Optional restriction: who can perform this transition
        public int? ApproverGroupId { get; set; }
        public ApproverGroup? ApproverGroup { get; set; }
    }

    public enum WorkflowInstanceStatus
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2
    }

    public enum WorkflowStepInstanceStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Returned = 3,
        Skipped = 4
    }

    public class WorkflowInstance
    {
        public int Id { get; set; }
        public int WorkflowDefinitionId { get; set; }
        public WorkflowDefinition WorkflowDefinition { get; set; } = null!;

        public int RequestId { get; set; }
        public Request Request { get; set; } = null!;

        public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAtUtc { get; set; }

        public int? CurrentStepId { get; set; }
        public WorkflowStepDefinition? CurrentStep { get; set; }

        public WorkflowInstanceStatus Status { get; set; } = WorkflowInstanceStatus.Active;

        public ICollection<WorkflowStepInstance> StepInstances { get; set; } = new List<WorkflowStepInstance>();
    }

    public class WorkflowStepInstance
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public WorkflowInstance WorkflowInstance { get; set; } = null!;

        public int StepDefinitionId { get; set; }
        public WorkflowStepDefinition StepDefinition { get; set; } = null!;

        public DateTime EnteredAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ExitedAtUtc { get; set; }

        public WorkflowStepInstanceStatus Status { get; set; } = WorkflowStepInstanceStatus.Pending;

        public int? AssignedApproverGroupId { get; set; }
        public ApproverGroup? AssignedApproverGroup { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }

        public int? ActorEmployeeId { get; set; }
        public Employee? ActorEmployee { get; set; }
    }
}

