using MassTransit;
using Shared.Models;

namespace RoutingSlipWorker.Activities
{
    public class ManagerApprovalActivity : IActivity<ApprovalArguments, ApprovalLog>
    {
        public async Task<ExecutionResult> Execute(ExecuteContext<ApprovalArguments> execution)
        {

            var args = execution.Arguments;

            Console.WriteLine($"[ManagerApproval] Approving vacation for user {args.UserId}");

            // Simulate some decision logic or API call
            await Task.Delay(500); // Simulate work

            return execution.Completed(new ApprovalLog
            {
                ApprovedBy = "Manager",
                Timestamp = DateTime.UtcNow
            });

        }

        public async Task<CompensationResult> Compensate(CompensateContext<ApprovalLog> compensation)
        {
            await Task.Run(() => Console.WriteLine($"[ManagerApproval] Compensating approval by: {compensation.Log.ApprovedBy}"));
            return compensation.Compensated();
        }
    }
}
