using LiteNetLib;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using Pixelise.Core.Commands;
using Pixelise.Core.Network;
using Pixelise.Server.Infrastructure.Network.Abstractions;

namespace Pixelise.Server.Infrastructure.Network;

public sealed class PacketDispatcher(IServiceProvider services) : IPacketDispatcher
{
    public async Task DispatchAsync(NetPeer peer, NetPacket packet)
    {
        switch (packet.Type)
        {
            case PacketType.BlockCommand:
                await Dispatch<BlockCommand>(peer, packet);
                break;

            case PacketType.PlayerMove:
                await Dispatch<PlayerMovePacket>(peer, packet);
                break;

            case PacketType.PlayerChunk:
                await Dispatch<PlayerChunkPacket>(peer, packet);
                break;

            default:
                // optionnel: log inconnu
                break;
        }
    }

    private async Task Dispatch<T>(NetPeer peer, NetPacket packet)
    {
        var handler = services.GetRequiredService<IPacketHandler<T>>();
        var data = MessagePackSerializer.Deserialize<T>(packet.Payload);
        await handler.HandleAsync(peer, data);
    }
}