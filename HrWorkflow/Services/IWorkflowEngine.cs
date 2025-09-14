using HrWorkflow.Models;

namespace HrWorkflow.Services
{
    public interface IWorkflowEngine
    {
        Task<WorkflowInstance> StartWorkflowAsync(int requestId, CancellationToken cancellationToken = default);

        Task<WorkflowInstance> AdvanceAsync(
            int workflowInstanceId,
            string actionName,
            int actorEmployeeId,
            string? comment = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WorkflowTransitionDefinition>> GetAvailableActionsAsync(
            int workflowInstanceId,
            int actorEmployeeId,
            CancellationToken cancellationToken = default);
    }
}

