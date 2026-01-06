using System.Net;
using System.Net.Sockets;
using LiteNetLib;
using MessagePack;
using Microsoft.Extensions.Logging;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using Pixelise.Server.Domain.World.Interfaces;
using Pixelise.Server.Infrastructure.Logging;
using Pixelise.Server.Infrastructure.Network.Abstractions;

namespace Pixelise.Server.Infrastructure.Network;

public sealed class LiteNetServerService : INetEventListener, INetworkBroadcaster
{
    private const int WorldRadius = 4;

    private readonly ILogger<LiteNetServerService> _logger;

    private readonly NetManager _server;
    private readonly IWorldService _world;

    private CancellationTokenSource? _cts;
    private Task? _loopTask;
    private readonly IPacketDispatcher _dispatcher;

    public LiteNetServerService(
        IWorldService world,
        IPacketDispatcher dispatcher,
        ILogger<LiteNetServerService> logger)
    {
        _world = world;
        _dispatcher = dispatcher;
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

        var centerChunk = new Int3(0, 0, 0);
        var chunks = _world.GetSpawnChunks(centerChunk, WorldRadius);

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

        _ = _dispatcher.DispatchAsync(peer, packet);
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

    public void Broadcast(NetPacket packet)
    {
        var bytes = MessagePackSerializer.Serialize(packet);

        foreach (var peer in _server.ConnectedPeerList)
        {
            peer.Send(bytes, DeliveryMethod.ReliableOrdered);
        }
    }

    public void Send(NetPeer peer, NetPacket packet)
    {
        peer.Send(
            MessagePackSerializer.Serialize(packet),
            DeliveryMethod.ReliableOrdered
        );
    }
}