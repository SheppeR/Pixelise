using Microsoft.Extensions.Logging;
using Pixelise.Core.Math;
using Pixelise.Server.Domain.World.Interfaces;
using Pixelise.Server.Infrastructure.Kernel;
using Pixelise.Server.Infrastructure.Logging;

namespace Pixelise.Server.Modules;

public sealed class WorldModule(
    IWorldService worldService,
    ILogger<WorldModule> logger) : IServerModule
{
    private const int PreGenerateRadius = 8;

    public string Name => "World";

    public Task StartAsync(CancellationToken ct)
    {
        logger.Section(Name);

        logger.Info($"Loading module : {Name}.");

        logger.Info("Pre-generating world...");

        // force la génération autour du spawn
        worldService.GetSpawnChunks(new Int3(0, 0, 0), PreGenerateRadius);

        logger.Info("World pre-generated.");
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        logger.Info($"{Name} module stopped.");
        return Task.CompletedTask;
    }
}