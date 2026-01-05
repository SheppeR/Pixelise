using MessagePack;
using Pixelise.Core.Blocks;
using Pixelise.Core.Math;

namespace Pixelise.Core.Network
{
    [MessagePackObject]
    public class ChunkDataPacket
    {
        [Key(1)]
        public BlockType[] Blocks; // 1D FLAT

        [Key(0)]
        public Int3 ChunkCoord;
    }
}