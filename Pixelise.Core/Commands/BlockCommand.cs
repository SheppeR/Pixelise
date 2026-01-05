using MessagePack;
using Pixelise.Core.Blocks;
using Pixelise.Core.Math;

namespace Pixelise.Core.Commands
{
    [MessagePackObject]
    public class BlockCommand
    {
        [Key(2)]
        public BlockAction Action;

        [Key(3)]
        public BlockType Block;

        [Key(0)]
        public Int3 ChunkCoord;

        [Key(1)]
        public Int3 LocalPos;
    }
}