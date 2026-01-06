using LiteNetLib;
using MessagePack;
using Pixelise.Core.Network;
using Pixelise.Server.Domain.World.Interfaces;
using Pixelise.Server.Infrastructure.Network.Abstractions;

namespace Pixelise.Server.Infrastructure.Network.Handlers;

public sealed class PlayerChunkHandler(IWorldService world, INetworkBroadcaster network)
    : IPacketHandler<PlayerChunkPacket>
{
    public PacketType Type => PacketType.PlayerChunk;

    public Task HandleAsync(NetPeer peer, PlayerChunkPacket packet)
    {
        var chunks = world.GetChunks(packet.ChunkCoord, 4);

        foreach (var chunk in chunks)
        {
            var response = new NetPacket
            {
                Type = PacketType.ChunkData,
                Payload = MessagePackSerializer.Serialize(
                    new ChunkDataPacket
                    {
                        ChunkCoord = chunk.Coord,
                        Blocks = chunk.ToFlatArray()
                    })
            };

            network.Send(peer, response);
        }

        world.UnloadFarChunks(packet.ChunkCoord, 6);

        return Task.CompletedTask;
    }
}
