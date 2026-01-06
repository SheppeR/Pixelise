using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixelise.Core.World;
using Pixelise.Server;
using Pixelise.Server.Modules;
using Pixelise.Server.Services;
using Pixelise.Server.Utils.Logger;
using Pixelise.Server.Utils.Server;
using Pixelise.Server.World;
using Serilog;

LogBootstrapper.Configure();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        services.AddSingleton<WorldData>();
        services.AddSingleton<WorldSimulation>();
        services.AddSingleton<LiteNetServerService>();

        services.AddSingleton<IServerModule, WorldModule>();
        services.AddSingleton<IServerModule, NetworkModule>();

        services.AddHostedService<ServerKernel>();
    })
    .Build();

await host.RunAsync();