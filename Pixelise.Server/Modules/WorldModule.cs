using Microsoft.Extensions.Logging;
using Pixelise.Core.Math;
using Pixelise.Server.Utils.Logger;
using Pixelise.Server.Utils.Server;
using Pixelise.Server.World;

namespace Pixelise.Server.Modules;

public sealed class WorldModule(WorldSimulation world, ILogger<WorldModule> logger) : IServerModule
{
    private const int PreGenerateRadius = 8;
    public string Name => "World";

    public Task StartAsync(CancellationToken ct)
    {
        logger.Section(Name);

        logger.Info($"Loading module : {Name}.");
        world.PreGenerateWorld(new Int3(0, 0, 0), PreGenerateRadius);

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        logger.Info($"{Name} module stopped.");
        return Task.CompletedTask;
    }
}