using MessagePack;
using Pixelise.Core.Math;

namespace Pixelise.Core.Network
{
    [MessagePackObject]
    public class PlayerChunkPacket
    {
        [Key(0)]
        public Int3 ChunkCoord;
    }
}