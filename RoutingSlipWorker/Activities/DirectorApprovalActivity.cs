using MassTransit;
using Shared.Models;

namespace RoutingSlipWorker.Activities
{
    public class DirectorApprovalActivity : IActivity<ApprovalArguments, ApprovalLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<ApprovalArguments> execution)
        {
            var args = execution.Arguments;
            var approvedBy = string.IsNullOrWhiteSpace(args.TargetDirector)
                ? "DirectorA"
                : args.TargetDirector;

            Console.WriteLine($"[DirectorApproval] {approvedBy} is approving request for user {args.UserId}");

            // Simulate async approval work
            await Task.Delay(500);

            return execution.Completed( new ApprovalLog
            {
                ApprovedBy = approvedBy,
                Timestamp = DateTime.UtcNow
            });
        }

        public async Task<CompensationResult> Compensate(CompensateContext<ApprovalLog> compensation)
        {
            await Task.Run(() => Console.WriteLine($"[DirectorApproval] Compensating approval by: {compensation.Log.ApprovedBy}"));
            return compensation.Compensated();
        }
    }
}
