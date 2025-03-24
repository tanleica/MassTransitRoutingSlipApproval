using MassTransit;
using MassTransit.Courier.Contracts;
using RoutingSlipWorker.Activities;
using Shared.Models;

var services = new ServiceCollection();

Console.WriteLine("Registering MassTransit...");
services.AddMassTransit(x =>
{

    x.UsingInMemory((context, cfg) => { });

    x.AddRider(rider =>
    {

        Console.WriteLine("➡️ Registering Rider Activities");

        // ✅ Register activities
        rider.AddActivity<ManagerApprovalActivity, ApprovalArguments, ApprovalLog>();
        rider.AddActivity<DirectorApprovalActivity, ApprovalArguments, ApprovalLog>();
        rider.AddExecuteActivity<PushNotificationActivity, ApprovalArguments>();

        rider.UsingKafka((context, kafka) =>
        {
            kafka.Host("159.223.59.17:9092");

            // ✅ Bind each topic to the correct ExecuteActivity
            kafka.TopicEndpoint<RoutingSlip>("manager-approval_execute", "manager-group", e =>
            {
                e.ExecuteActivityHost<ManagerApprovalActivity, ApprovalArguments>(context);
                Console.WriteLine("✅ Ready: ManagerApprovalActivity");
            });

            kafka.TopicEndpoint<RoutingSlip>("director-approval_execute", "director-group", e =>
            {
                e.ExecuteActivityHost<DirectorApprovalActivity, ApprovalArguments>(context);
                Console.WriteLine("✅ Ready: DirectorApprovalActivity");
            });

            kafka.TopicEndpoint<RoutingSlip>("push-notification_execute", "push-group", e =>
            {
                e.ExecuteActivityHost<PushNotificationActivity, ApprovalArguments>(context);
                Console.WriteLine("✅ Ready: PushNotificationActivity");
            });
        });
    });
});
Console.WriteLine("✅ MassTransit Registration Done");

var provider = services.BuildServiceProvider();
Console.WriteLine(" [✔] Check point 1");

// ✅ Start the bus
var bus = provider.GetRequiredService<IBusControl>();
Console.WriteLine(" [✔] Check point 2");

try
{
    await bus.StartAsync();
    Console.WriteLine(" [✔] RoutingSlipWorker is running...");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Worker failed to start: {ex.Message}");
}

Console.WriteLine(" [✔] Check point 3");
await Task.Delay(-1); // Keep the worker running