using MessagePack;
using Pixelise.Core.Blocks;
using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    [MessagePackObject]
    public sealed class SavedChunk
    {
        [Key(1)]
        public BlockType[] Blocks;

        [Key(0)]
        public Int3 Coord;
    }
}