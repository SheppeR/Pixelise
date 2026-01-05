using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixelise.Core.World;
using Pixelise.Server.Services;
using Pixelise.Server.World;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<WorldData>();
        services.AddSingleton<WorldSimulation>();
        services.AddHostedService<LiteNetServerService>();
    })
    .Build();

await host.RunAsync();