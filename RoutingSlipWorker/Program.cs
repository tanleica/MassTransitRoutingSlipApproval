using MassTransit;
using MassTransit.Courier.Contracts;
using RoutingSlipWorker.Activities;
using Shared.Models;

var services = new ServiceCollection();

Console.WriteLine("Registering MassTransit...");
services.AddMassTransit(x =>
{
    // This is required even when using Rider (for internal bus setup)
    x.UsingInMemory((context, cfg) => { });

    // Rider block for Kafka
    x.AddRider(rider =>
    {
        Console.WriteLine("➡️ Registering Rider Activities");

        // Automatically scan and register all activities in your namespace
        // rider.AddActivitiesFromNamespaceContaining<ManagerApprovalActivity>();
        // rider.AddActivitiesFromNamespaceContaining<DirectorApprovalActivity>();
        // rider.AddActivitiesFromNamespaceContaining<PushNotificationActivity>();

        rider.AddActivity<ManagerApprovalActivity, ApprovalArguments, ApprovalLog>();
        rider.AddActivity<DirectorApprovalActivity, ApprovalArguments, ApprovalLog>();
        rider.AddExecuteActivity<PushNotificationActivity, ApprovalArguments>();

        rider.UsingKafka((context, kafka) =>
        {
            kafka.Host("159.223.59.17:9092");

            kafka.TopicEndpoint<RoutingSlip>("manager-approval_execute", "manager-group", e =>
            {
                Console.WriteLine("✅ Registered Kafka Endpoint: manager-approval_execute");
                e.ExecuteActivityHost<ManagerApprovalActivity, ApprovalArguments>(context);
            });

            kafka.TopicEndpoint<RoutingSlip>("manager-approval_execute", "debug-group", e =>
            {
                e.Handler<RoutingSlip>(context =>
                {
                    Console.WriteLine($"[DEBUG] RoutingSlip received: {context.Message.TrackingNumber}");
                    return Task.CompletedTask;
                });
            });

            kafka.TopicEndpoint<RoutingSlip>("director-approval_execute", "director-group", e =>
            {
                Console.WriteLine("✅ Registered Kafka Endpoint: director-approval_execute");
                e.ExecuteActivityHost<DirectorApprovalActivity, ApprovalArguments>(context);
            });

            kafka.TopicEndpoint<RoutingSlip>("push-notification_execute", "notify-group", e =>
            {
                Console.WriteLine("✅ Registered Kafka Endpoint: push-notification_execute");
                e.ExecuteActivityHost<PushNotificationActivity, ApprovalArguments>(context);
            });
        });


    });
});
Console.WriteLine("✅ MassTransit Registration Done");

var provider = services.BuildServiceProvider();

// ✅ Start MassTransit (Bus + Rider)
var bus = provider.GetRequiredService<IBusControl>();
await bus.StartAsync();

Console.WriteLine(" [✔] RoutingSlipWorker is running...");
await Task.Delay(-1); // Keep the app running
