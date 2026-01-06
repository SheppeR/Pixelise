using LiteNetLib;
using MessagePack;
using Pixelise.Core.Commands;
using Pixelise.Core.Network;
using Pixelise.Server.Domain.World.Interfaces;
using Pixelise.Server.Infrastructure.Network.Abstractions;

namespace Pixelise.Server.Infrastructure.Network.Handlers;

public sealed class BlockCommandHandler(IWorldService world, INetworkBroadcaster network) : IPacketHandler<BlockCommand>
{
    public PacketType Type => PacketType.BlockCommand;

    public Task HandleAsync(NetPeer peer, BlockCommand packet)
    {
        world.ApplyBlockCommand(packet);

        var netPacket = new NetPacket
        {
            Type = PacketType.BlockCommand,
            Payload = MessagePackSerializer.Serialize(packet)
        };

        network.Broadcast(netPacket);

        return Task.CompletedTask;
    }
}