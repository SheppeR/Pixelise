using System.Collections.Generic;
using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    public sealed class WorldData
    {
        private readonly Dictionary<Int3, ChunkData> chunks = new Dictionary<Int3, ChunkData>();

        public ChunkData GetChunk(Int3 coord)
        {
            return chunks.TryGetValue(coord, out var c) ? c : null;
        }

        public void AddChunk(ChunkData chunk)
        {
            chunks[chunk.Coord] = chunk;
        }
    }
}