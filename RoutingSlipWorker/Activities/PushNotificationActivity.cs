using System.Text;
using System.Text.Json;
using MassTransit;
using Shared.Models;

namespace RoutingSlipWorker.Activities
{
    public class PushNotificationActivity : IExecuteActivity<ApprovalArguments>
    {
        private static readonly HttpClient httpClient = new();

        public async Task<ExecutionResult> Execute(ExecuteContext<ApprovalArguments> execution)
        {
            var userId = execution.Arguments.UserId;
            var message = $"Routing Slip step '{execution.ActivityName}' completed for user {userId}.";

            await SendPushNotificationAsync(userId, message);
            return execution.Completed<ApprovalLog>(new { ApprovedBy = "Http Push Service", Timestamp = DateTime.UtcNow });
        }

        // ✅ Web Push Notification Sender
        private static async Task SendPushNotificationAsync(string userId, string message)
        {
            try
            {
                var url = "https://alpha.histaff.vn/api/WebPush/SendSimpleWebPushNotification";
                var payload = new { userId, message };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($" [✔] Push notification sent to {userId}");
                }
                else
                {
                    Console.WriteLine($" [!] Failed to send push notification: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Push Notification Error: {ex.Message}");
            }
        }
    }
}
