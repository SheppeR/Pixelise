using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using MessagePack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Server.World;

namespace Pixelise.Server.Services;

public sealed class LiteNetServerService : BackgroundService, INetEventListener
{
    private readonly ILogger<LiteNetServerService> logger;
    private readonly NetManager server;
    private readonly WorldSimulation world;

    private const int WorldRadius = 6;

    public LiteNetServerService(
        WorldSimulation world,
        ILogger<LiteNetServerService> logger)
    {
        this.world = world;
        this.logger = logger;

        server = new NetManager(this)
        {
            AutoRecycle = true
        };
    }

    // ========================
    // CONNECTION
    // ========================

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey("PixeliseKey");
    }

    public void OnPeerConnected(NetPeer peer)
    {
        logger.LogInformation($"Client connected: {peer.Address}");

        var centerChunk = new Int3(0, 0, 0);

        // 🌍 Génération + envoi monde
        var chunks = world.GenerateWorld(centerChunk, WorldRadius);
        foreach (var chunk in chunks)
        {
            SendChunk(peer, chunk);
        }

        // 🧍 Spawn SAFE
        var spawnPos = world.FindSafeSpawn(centerChunk);

        var spawnPacket = new NetPacket
        {
            Type = PacketType.PlayerSpawn,
            Payload = MessagePackSerializer.Serialize(
                new PlayerSpawnPacket
                {
                    Position = spawnPos,
                    Yaw = 0f
                })
        };

        peer.Send(
            MessagePackSerializer.Serialize(spawnPacket),
            DeliveryMethod.ReliableOrdered
        );

        logger.LogInformation($"Player spawned at {spawnPos.X},{spawnPos.Y},{spawnPos.Z}");
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        logger.LogInformation($"Client disconnected: {peer.Address}");
    }

    // ========================
    // RECEIVE
    // ========================

    public void OnNetworkReceive(
        NetPeer peer,
        NetPacketReader reader,
        byte channel,
        DeliveryMethod method)
    {
        var packet = MessagePackSerializer.Deserialize<NetPacket>(
            reader.GetRemainingBytes());

        switch (packet.Type)
        {
            case PacketType.BlockCommand:
                {
                    var cmd = MessagePackSerializer.Deserialize<BlockCommand>(packet.Payload);
                    world.Apply(cmd);
                    Broadcast(packet);
                    break;
                }

            case PacketType.PlayerMove:
                {
                    Broadcast(packet);
                    break;
                }
        }
    }

    // ========================
    // SEND
    // ========================

    private void SendChunk(NetPeer peer, Core.World.ChunkData chunk)
    {
        var packet = new NetPacket
        {
            Type = PacketType.ChunkData,
            Payload = MessagePackSerializer.Serialize(
                new ChunkDataPacket
                {
                    ChunkCoord = chunk.Coord,
                    Blocks = chunk.ToFlatArray()
                })
        };

        peer.Send(
            MessagePackSerializer.Serialize(packet),
            DeliveryMethod.ReliableOrdered
        );
    }

    private void Broadcast(NetPacket packet)
    {
        var bytes = MessagePackSerializer.Serialize(packet);

        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(bytes, DeliveryMethod.ReliableOrdered);
        }
    }

    // ========================
    // LOOP
    // ========================

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        server.Start(9000);
        logger.LogInformation("LiteNetLib server started on port 9000");

        while (!stoppingToken.IsCancellationRequested)
        {
            server.PollEvents();
            Thread.Sleep(15);
        }

        server.Stop();
        return Task.CompletedTask;
    }

    // ========================
    // UNUSED
    // ========================

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError) { }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
    public void OnNetworkReceiveUnconnected(
        IPEndPoint remoteEndPoint,
        NetPacketReader reader,
        UnconnectedMessageType messageType)
    { }
}
