using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    public static class ChunkUtils
    {
        public static Int3 WorldToChunk(Int3 worldPos)
        {
            return new Int3(
                FloorDiv(worldPos.X, ChunkData.Width),
                0,
                FloorDiv(worldPos.Z, ChunkData.Depth)
            );
        }

        public static Int3 WorldToLocal(Int3 worldPos, Int3 chunkCoord)
        {
            return new Int3(
                worldPos.X - chunkCoord.X * ChunkData.Width,
                worldPos.Y,
                worldPos.Z - chunkCoord.Z * ChunkData.Depth
            );
        }

        private static int FloorDiv(int a, int b)
        {
            return a >= 0 ? a / b : (a - (b - 1)) / b;
        }
    }
}