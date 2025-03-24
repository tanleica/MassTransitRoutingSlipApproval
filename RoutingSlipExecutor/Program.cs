using MassTransit;
using Shared.Models;
using Microsoft.Extensions.DependencyInjection;
using MassTransit.Courier.Contracts;

var services = new ServiceCollection();

services.AddMassTransit(x =>
{
    x.UsingInMemory((context, cfg) => { }); // Needed by MassTransit internally

    x.AddRider(rider =>
    {
        // ✅ Register producers for RoutingSlip per topic
        rider.AddProducer<RoutingSlip>("manager-approval_execute");
        rider.AddProducer<RoutingSlip>("director-approval_execute");
        rider.AddProducer<RoutingSlip>("push-notification_execute");

        rider.UsingKafka((context, kafka) =>
        {
            kafka.Host("159.223.59.17:9092");
        });
    });
});

var provider = services.BuildServiceProvider();

// ✅ Start the bus
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

        // var producer = provider.GetRequiredService<ITopicProducer<RoutingSlip>>();

        // var routingSlipTest = new RoutingSlipBuilder(NewId.NextGuid())
        //     .Build(); // 🔸 No activities, just a shell routing slip

        // await producer.Produce(routingSlipTest);

        builderSlip.AddActivity(
            "ManagerApprovalActivity",
            new Uri("topic:manager-approval_execute"),
            request);

        if (request.IsLongVacation)
        {
            builderSlip.AddActivity(
                "DirectorApprovalActivity",
                new Uri("topic:director-approval_execute"),
                request);
        }

        builderSlip.AddActivity(
            "PushNotificationActivity",
            new Uri("topic:push-notification_execute"),
            request);

        var routingSlip = builderSlip.Build();

        Console.WriteLine("📦 Routing Slip Activities:");
        foreach (var activity in routingSlip.Itinerary)
            Console.WriteLine($"→ {activity.Name} to {activity.Address}");

        await bus.Execute(routingSlip);

        Console.WriteLine($"[✔] Routing slip executed! Tracking #: {routingSlip.TrackingNumber}");
        Console.WriteLine("[✔] Waiting for next round after 15 minutes...\n");
        await Task.Delay(TimeSpan.FromMinutes(15)); // Simulate periodic scheduling
    }
}
finally
{
    await bus.StopAsync();
}