using MassTransit;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => { }); // Required base bus

    x.AddRider(rider =>
    {
        rider.UsingKafka((context, kafka) =>
        {
            kafka.Host("159.223.59.17:9092");
        });
    });
});

var provider = services.BuildServiceProvider();

// ✅ Start MassTransit (Bus + Rider)
var bus = provider.GetRequiredService<IBusControl>();
await bus.StartAsync();

try
{
    while (true)
    {

        var request = new ApprovalArguments
        {
            UserId = Guid.NewGuid().ToString(),
            RequestType = "Vacation",
            IsLongVacation = true,
            TargetDirector = "DirectorB"
        };

        var builderSlip = new RoutingSlipBuilder(NewId.NextGuid());

        builderSlip.AddActivity(
            "ManagerApprovalActivity",
            new Uri("queue:manager-approval_execute"),
            request);

        if (request.IsLongVacation)
        {
            builderSlip.AddActivity(
                "DirectorApprovalActivity",
                new Uri("queue:director-approval_execute"),
                request);
        }

        // Always end with push notification
        builderSlip.AddActivity(
            "PushNotificationActivity",
            new Uri("queue:push-notification_execute"),
            request);

        Console.WriteLine(" [→] Building routing slip...");

        var routingSlip = builderSlip.Build();

        Console.WriteLine("📦 Routing Slip Activities:");
        foreach (var activity in routingSlip.Itinerary)
        {
            Console.WriteLine($"→ {activity.Name} to {activity.Address}");
        }

        await bus.Execute(routingSlip);

        Console.WriteLine($"[✔] Routing slip executed! Tracking #: {routingSlip.TrackingNumber}");

        Console.WriteLine($"[✔] Waiting for next round after 15 minutes");
        await Task.Delay(TimeSpan.FromSeconds(10)); // Schedule-like behavior

    }
}
finally
{
    await bus.StopAsync();
}
