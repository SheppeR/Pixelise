namespace Pixelise.Core.Network
{
    public enum PacketType : byte
    {
        Hello,
        PlayerMove,
        BlockCommand,
        PlayerTransform,
        ChunkData,
        PlayerSpawn
    }
}