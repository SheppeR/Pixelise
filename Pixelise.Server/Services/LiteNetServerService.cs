using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using MessagePack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using Pixelise.Server.World;

namespace Pixelise.Server.Services;

public sealed class LiteNetServerService : BackgroundService, INetEventListener
{
    private const int PreGenerateRadius = 8;

    // 🔥 réduit pour limiter les spikes
    private const int WorldRadius = 4;
    private readonly ILogger<LiteNetServerService> logger;

    // 🔥 cache des chunks envoyés
    private readonly Dictionary<NetPeer, HashSet<Int3>> sentChunks = new();
    private readonly NetManager server;
    private readonly WorldSimulation world;

    public LiteNetServerService(
        WorldSimulation world,
        ILogger<LiteNetServerService> logger)
    {
        this.world = world;
        this.logger = logger;

        // 🔥 PRÉ-GÉNÉRATION DU MONDE
        logger.LogInformation("Pre-generating world...");
        world.PreGenerateWorld(new Int3(0, 0, 0), PreGenerateRadius);
        logger.LogInformation("World pre-generated.");

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
        sentChunks[peer] = new HashSet<Int3>();

        var centerChunk = new Int3(0, 0, 0);
        var chunks = world.GenerateWorld(centerChunk, WorldRadius);

        foreach (var chunk in chunks)
        {
            SendChunk(peer, chunk);
        }

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
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
    {
        sentChunks.Remove(peer);
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

            case PacketType.PlayerChunk:
            {
                var msg = MessagePackSerializer.Deserialize<PlayerChunkPacket>(packet.Payload);
                var chunks = world.GenerateWorld(msg.ChunkCoord, WorldRadius);

                foreach (var chunk in chunks)
                {
                    SendChunk(peer, chunk);
                }

                break;
            }
        }
    }

    // ========================
    // UNUSED
    // ========================

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    public void OnNetworkReceiveUnconnected(
        IPEndPoint remoteEndPoint,
        NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
    }

    // ========================
    // SEND
    // ========================

    private void SendChunk(NetPeer peer, ChunkData chunk)
    {
        if (!sentChunks[peer].Add(chunk.Coord))
        {
            return;
        }

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
}