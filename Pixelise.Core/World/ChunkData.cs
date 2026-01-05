using Pixelise.Core.Blocks;
using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    public sealed class ChunkData
    {
        public const int Width = 16;
        public const int Height = 64;
        public const int Depth = 16;
        private readonly BlockType[,,] blocks = new BlockType[Width, Height, Depth];

        public readonly Int3 Coord;

        public ChunkData(Int3 coord)
        {
            Coord = coord;
        }

        public BlockType Get(Int3 local)
        {
            if (!Inside(local))
            {
                return BlockType.Air;
            }

            return blocks[local.X, local.Y, local.Z];
        }

        public void Set(Int3 local, BlockType type)
        {
            if (!Inside(local))
            {
                return;
            }

            blocks[local.X, local.Y, local.Z] = type;
        }

        private static bool Inside(Int3 p)
        {
            return p.X >= 0 && p.X < Width &&
                   p.Y >= 0 && p.Y < Height &&
                   p.Z >= 0 && p.Z < Depth;
        }

        public BlockType[,,] GetAllBlocks()
        {
            return blocks;
        }

        public void SetAll(BlockType[,,] data)
        {
            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            for (var z = 0; z < Depth; z++)
            {
                blocks[x, y, z] = data[x, y, z];
            }
        }

        public BlockType[] ToFlatArray()
        {
            var data = new BlockType[Width * Height * Depth];
            var i = 0;

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            for (var z = 0; z < Depth; z++)
            {
                data[i++] = blocks[x, y, z];
            }

            return data;
        }

        public void FromFlatArray(BlockType[] data)
        {
            var i = 0;

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            for (var z = 0; z < Depth; z++)
            {
                blocks[x, y, z] = data[i++];
            }
        }

    }
}