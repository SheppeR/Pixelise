using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixelise.Server.Infrastructure.Logging;

namespace Pixelise.Server.Infrastructure.Kernel;

public sealed class ServerKernel(IEnumerable<IServerModule> modules, ILogger<ServerKernel> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        logger.Section("KERNEL");
        logger.Info("Starting server modules...");

        foreach (var module in modules)
        {
            await module.StartAsync(ct);
        }

        logger.Section("log");
        logger.Info("All modules started.");
    }

    public async Task StopAsync(CancellationToken ct)
    {
        logger.Info("Stopping server modules...");

        foreach (var module in modules.Reverse())
        {
            logger.Info($"Stopping module: {module.Name}");
            await module.StopAsync();
        }

        logger.Info("All modules stopped.");
    }
}