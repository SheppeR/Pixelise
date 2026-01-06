using Pixelise.Core.Math;
using Pixelise.Core.World;
using Pixelise.Server.Domain.World.Interfaces;

namespace Pixelise.Server.Domain.World;

public sealed class TerrainChunkGenerator(ITerrainGenerator terrain) : IChunkGenerator
{
    public void Generate(ChunkData chunk)
    {
        for (var x = 0; x < ChunkData.Width; x++)
        for (var y = 0; y < ChunkData.Height; y++)
        for (var z = 0; z < ChunkData.Depth; z++)
        {
            var worldX = chunk.Coord.X * ChunkData.Width + x;
            var worldZ = chunk.Coord.Z * ChunkData.Depth + z;

            var block = terrain.GetBlock(worldX, y, worldZ);
            chunk.Set(new Int3(x, y, z), block);
        }
    }
}