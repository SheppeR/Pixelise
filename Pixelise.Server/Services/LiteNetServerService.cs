using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using MessagePack;
using Microsoft.Extensions.Logging;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using Pixelise.Server.Utils.Logger;
using Pixelise.Server.World;

namespace Pixelise.Server.Services;

public sealed class LiteNetServerService : INetEventListener
{
    private const int WorldRadius = 4;

    private readonly ILogger<LiteNetServerService> _logger;

    private readonly Dictionary<NetPeer, HashSet<Int3>> _sentChunks = new();
    private readonly NetManager _server;
    private readonly WorldSimulation _world;

    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    public LiteNetServerService(WorldSimulation world, ILogger<LiteNetServerService> logger)
    {
        _world = world;
        _logger = logger;

        _server = new NetManager(this)
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
        _logger.Info($"Client connected: {peer.Address}");
        _sentChunks[peer] = new HashSet<Int3>();

        var centerChunk = new Int3(0, 0, 0);
        var chunks = _world.GetSpawn(centerChunk, WorldRadius);

        foreach (var chunk in chunks)
        {
            SendChunk(peer, chunk);
        }

        var spawnPos = _world.FindSafeSpawn(centerChunk);

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
        _sentChunks.Remove(peer);
        _logger.Info($"Client disconnected: {peer.Address}");
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
                _world.Apply(cmd);
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
                var chunks = _world.GetChunks(msg.ChunkCoord, WorldRadius);

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
    // LIFECYCLE
    // ========================

    public Task StartAsync(CancellationToken ct)
    {
        _logger.Info("Starting LiteNetLib server...");

        _server.Start(9000);

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _loopTask = Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                _server.PollEvents();
                await Task.Delay(15, _cts.Token);
            }
        }, _cts.Token);

        _logger.Info("LiteNetLib server started on port 9000");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _logger.Info("Stopping LiteNetLib server...");

        if (_cts != null)
        {
            await _cts.CancelAsync();

            if (_loopTask != null)
            {
                try
                {
                    await _loopTask;
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }
        }

        _server.Stop();
        _logger.Info("LiteNetLib server stopped.");
    }

    // ========================
    // SEND
    // ========================

    private void SendChunk(NetPeer peer, ChunkData chunk)
    {
        if (!_sentChunks[peer].Add(chunk.Coord))
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

        foreach (var peer in _server.ConnectedPeerList)
        {
            peer.Send(bytes, DeliveryMethod.ReliableOrdered);
        }
    }
}