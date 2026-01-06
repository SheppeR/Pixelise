using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pixelise.Core.Commands;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using Pixelise.Server.Domain.World;
using Pixelise.Server.Domain.World.Interfaces;
using Pixelise.Server.Infrastructure.Kernel;
using Pixelise.Server.Infrastructure.Logging;
using Pixelise.Server.Infrastructure.Network;
using Pixelise.Server.Infrastructure.Network.Abstractions;
using Pixelise.Server.Infrastructure.Network.Handlers;
using Pixelise.Server.Modules;
using Serilog;

LogBootstrapper.Configure();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices(services =>
    {
        // ========================
        // WORLD DATA
        // ========================
        services.AddSingleton<WorldData>();

        // ========================
        // WORLD DOMAIN
        // ========================
        services.AddSingleton<ITerrainGenerator, DefaultTerrainGenerator>();
        services.AddSingleton<IChunkGenerator, TerrainChunkGenerator>();
        services.AddSingleton<IWorldDecorator, TreeWorldDecorator>();
        services.AddSingleton<IChunkProvider, ChunkProvider>();
        services.AddSingleton<IWorldService, WorldService>();

        // ========================
        // NETWORK TRANSPORT
        // ========================
        services.AddSingleton<IPacketDispatcher, PacketDispatcher>();
        services.AddSingleton<LiteNetServerService>();
        services.AddSingleton<INetworkBroadcaster>(sp => sp.GetRequiredService<LiteNetServerService>());

        services.AddSingleton<IPacketHandler<BlockCommand>, BlockCommandHandler>();
        services.AddSingleton<IPacketHandler<PlayerChunkPacket>, PlayerChunkHandler>();
        services.AddSingleton<IPacketHandler<PlayerMovePacket>, PlayerMoveHandler>();

        // ========================
        // MODULES
        // ========================
        services.AddSingleton<IServerModule, WorldModule>();
        services.AddSingleton<IServerModule, NetworkModule>();

        // ========================
        // KERNEL
        // ========================
        services.AddHostedService<ServerKernel>();
    })
    .Build();

await host.RunAsync();

