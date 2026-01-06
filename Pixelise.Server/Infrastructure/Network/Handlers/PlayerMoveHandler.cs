using LiteNetLib;
using MessagePack;
using Pixelise.Core.Network;
using Pixelise.Server.Infrastructure.Network.Abstractions;

namespace Pixelise.Server.Infrastructure.Network.Handlers;

public sealed class PlayerMoveHandler(INetworkBroadcaster network) : IPacketHandler<PlayerMovePacket>
{
    public PacketType Type => PacketType.PlayerMove;

    public Task HandleAsync(NetPeer peer, PlayerMovePacket packet)
    {
        // On repack proprement
        var netPacket = new NetPacket
        {
            Type = PacketType.PlayerMove,
            Payload = MessagePackSerializer.Serialize(packet)
        };

        // Broadcast à tous (y compris l'envoyeur, ou pas selon ton design)
        network.Broadcast(netPacket);

        return Task.CompletedTask;
    }
}