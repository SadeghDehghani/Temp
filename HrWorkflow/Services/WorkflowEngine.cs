using HrWorkflow.Data;
using HrWorkflow.Models;
using Microsoft.EntityFrameworkCore;

namespace HrWorkflow.Services
{
    public class WorkflowEngine : IWorkflowEngine
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkflowEngine(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorkflowInstance> StartWorkflowAsync(int requestId, CancellationToken cancellationToken = default)
        {
            var request = await _dbContext.Requests
                .Include(r => r.RequestType)
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
                ?? throw new InvalidOperationException($"Request {requestId} not found");

            var workflowDefinition = await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps)
                .Include(w => w.Transitions)
                .FirstOrDefaultAsync(w => w.RequestTypeId == request.RequestTypeId && w.IsActive, cancellationToken)
                ?? throw new InvalidOperationException($"Active workflow definition for request type {request.RequestTypeId} not found");

            var startStep = workflowDefinition.Steps.FirstOrDefault(s => s.StepType == WorkflowStepType.Start)
                ?? throw new InvalidOperationException("Workflow does not have a Start step");

            var firstTransition = workflowDefinition.Transitions
                .Where(t => t.FromStepId == startStep.Id)
                .Select(t => t)
                .FirstOrDefault();

            var initialStep = firstTransition is not null
                ? workflowDefinition.Steps.First(s => s.Id == firstTransition.ToStepId)
                : startStep;

            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = workflowDefinition.Id,
                RequestId = request.Id,
                CurrentStepId = initialStep.Id,
                Status = WorkflowInstanceStatus.Active,
                StartedAtUtc = DateTime.UtcNow
            };

            var stepInstance = new WorkflowStepInstance
            {
                WorkflowInstance = instance,
                StepDefinitionId = initialStep.Id,
                EnteredAtUtc = DateTime.UtcNow,
                Status = WorkflowStepInstanceStatus.Pending,
                AssignedApproverGroupId = ResolveAssignedGroupForStep(initialStep)
            };

            _dbContext.WorkflowInstances.Add(instance);
            _dbContext.WorkflowStepInstances.Add(stepInstance);
            await _dbContext.SaveChangesAsync(cancellationToken);

            request.WorkflowInstanceId = instance.Id;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return instance;
        }

        public async Task<WorkflowInstance> AdvanceAsync(int workflowInstanceId, string actionName, int actorEmployeeId, string? comment = null, CancellationToken cancellationToken = default)
        {
            var instance = await _dbContext.WorkflowInstances
                .Include(i => i.WorkflowDefinition)
                    .ThenInclude(d => d.Steps)
                .Include(i => i.WorkflowDefinition)
                    .ThenInclude(d => d.Transitions)
                .FirstOrDefaultAsync(i => i.Id == workflowInstanceId, cancellationToken)
                ?? throw new InvalidOperationException($"Workflow instance {workflowInstanceId} not found");

            if (instance.Status != WorkflowInstanceStatus.Active)
            {
                throw new InvalidOperationException("Cannot advance a non-active workflow instance");
            }

            var currentStepId = instance.CurrentStepId ?? throw new InvalidOperationException("Instance has no current step");
            var currentStep = instance.WorkflowDefinition.Steps.First(s => s.Id == currentStepId);

            // Find matching transition
            var transitions = instance.WorkflowDefinition.Transitions
                .Where(t => t.FromStepId == currentStep.Id && string.Equals(t.ActionName, actionName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (transitions.Count == 0)
            {
                throw new InvalidOperationException($"No transition for action '{actionName}' from current step");
            }

            // For now, pick the first transition whose approver group (if any) contains the actor
            WorkflowTransitionDefinition? selected = null;
            foreach (var t in transitions)
            {
                if (t.ApproverGroupId is null)
                {
                    selected = t;
                    break;
                }

                var isMember = await _dbContext.ApproverGroupMembers
                    .AnyAsync(m => m.ApproverGroupId == t.ApproverGroupId && m.EmployeeId == actorEmployeeId, cancellationToken);
                if (isMember)
                {
                    selected = t;
                    break;
                }
            }

            if (selected is null)
            {
                throw new UnauthorizedAccessException("Actor is not authorized to perform this action");
            }

            // Close current step instance
            var openStepInstance = await _dbContext.WorkflowStepInstances
                .OrderByDescending(si => si.Id)
                .FirstOrDefaultAsync(si => si.WorkflowInstanceId == instance.Id && si.StepDefinitionId == currentStep.Id && si.Status == WorkflowStepInstanceStatus.Pending, cancellationToken);
            if (openStepInstance is null)
            {
                // Ensure we have a step instance to close
                openStepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instance.Id,
                    StepDefinitionId = currentStep.Id,
                    EnteredAtUtc = DateTime.UtcNow,
                    Status = WorkflowStepInstanceStatus.Pending
                };
                _dbContext.WorkflowStepInstances.Add(openStepInstance);
            }

            openStepInstance.Status = MapActionToStatus(actionName);
            openStepInstance.ExitedAtUtc = DateTime.UtcNow;
            openStepInstance.ActorEmployeeId = actorEmployeeId;
            openStepInstance.Comment = comment;

            // Move to next step
            var nextStep = instance.WorkflowDefinition.Steps.First(s => s.Id == selected.ToStepId);
            instance.CurrentStepId = nextStep.StepType == WorkflowStepType.End ? null : nextStep.Id;

            if (nextStep.StepType == WorkflowStepType.End)
            {
                instance.Status = WorkflowInstanceStatus.Completed;
                instance.CompletedAtUtc = DateTime.UtcNow;
            }
            else
            {
                var nextStepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instance.Id,
                    StepDefinitionId = nextStep.Id,
                    EnteredAtUtc = DateTime.UtcNow,
                    Status = WorkflowStepInstanceStatus.Pending,
                    AssignedApproverGroupId = ResolveAssignedGroupForStep(nextStep)
                };
                _dbContext.WorkflowStepInstances.Add(nextStepInstance);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return instance;
        }

        public async Task<IReadOnlyList<WorkflowTransitionDefinition>> GetAvailableActionsAsync(int workflowInstanceId, int actorEmployeeId, CancellationToken cancellationToken = default)
        {
            var instance = await _dbContext.WorkflowInstances
                .Include(i => i.WorkflowDefinition)
                    .ThenInclude(d => d.Transitions)
                .FirstOrDefaultAsync(i => i.Id == workflowInstanceId, cancellationToken)
                ?? throw new InvalidOperationException($"Workflow instance {workflowInstanceId} not found");

            if (instance.CurrentStepId is null)
            {
                return Array.Empty<WorkflowTransitionDefinition>();
            }

            var candidates = instance.WorkflowDefinition.Transitions
                .Where(t => t.FromStepId == instance.CurrentStepId.Value)
                .ToList();

            var result = new List<WorkflowTransitionDefinition>();
            foreach (var t in candidates)
            {
                if (t.ApproverGroupId is null)
                {
                    result.Add(t);
                }
                else
                {
                    var isMember = await _dbContext.ApproverGroupMembers
                        .AnyAsync(m => m.ApproverGroupId == t.ApproverGroupId && m.EmployeeId == actorEmployeeId, cancellationToken);
                    if (isMember)
                    {
                        result.Add(t);
                    }
                }
            }

            return result;
        }

        private static int? ResolveAssignedGroupForStep(WorkflowStepDefinition step)
        {
            // This can be extended to map step to a group. For now, use the first incoming transition's group as a hint.
            var groupId = step.IncomingTransitions.FirstOrDefault()?.ApproverGroupId;
            return groupId;
        }

        private static WorkflowStepInstanceStatus MapActionToStatus(string actionName)
        {
            return actionName.ToLowerInvariant() switch
            {
                "approve" => WorkflowStepInstanceStatus.Approved,
                "reject" => WorkflowStepInstanceStatus.Rejected,
                "return" => WorkflowStepInstanceStatus.Returned,
                _ => WorkflowStepInstanceStatus.Skipped
            };
        }
    }
}

