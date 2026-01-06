using Microsoft.Extensions.Logging;
using Pixelise.Server.Infrastructure.Kernel;
using Pixelise.Server.Infrastructure.Logging;
using Pixelise.Server.Infrastructure.Network;

namespace Pixelise.Server.Modules;

public sealed class NetworkModule(LiteNetServerService net, ILogger<NetworkModule> logger) : IServerModule
{
    public string Name => "Network";

    public Task StartAsync(CancellationToken ct)
    {
        logger.Section(Name);

        logger.Info($"Loading module : {Name}.");
        return net.StartAsync(ct);
    }

    public Task StopAsync()
    {
        logger.Info($"{Name} module stopped.");
        return net.StopAsync();
    }
}