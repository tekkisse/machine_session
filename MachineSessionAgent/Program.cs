using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder(args)
    //.UseWindowsService()
    .ConfigureServices(services =>
    {
        services.AddHostedService<AgentWorker>();
        services.AddSingleton<RabbitMqHandler>();
        services.AddSingleton<SessionDetector>();
        services.AddHttpClient();
    })
    .Build()
    .Run();