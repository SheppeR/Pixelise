using MessagePack;

namespace Pixelise.Core.Network
{
    [MessagePackObject]
    public class NetPacket
    {
        [Key(1)]
        public byte[] Payload;

        [Key(0)]
        public PacketType Type;
    }
}