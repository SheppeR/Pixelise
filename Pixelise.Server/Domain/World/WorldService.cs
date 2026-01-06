using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.World;
using Pixelise.Server.Domain.World.Interfaces;

namespace Pixelise.Server.Domain.World;

public sealed class WorldService(IChunkProvider chunkProvider) : IWorldService
{
    public IEnumerable<ChunkData> GetChunks(Int3 center, int radius)
    {
        var result = new List<ChunkData>();

        for (var x = -radius; x <= radius; x++)
            for (var z = -radius; z <= radius; z++)
            {
                var coord = new Int3(center.X + x, 0, center.Z + z);
                result.Add(chunkProvider.GetOrCreate(coord));
            }

        return result;
    }

    public IEnumerable<ChunkData> GetSpawnChunks(Int3 center, int radius)
    {
        return GetChunks(center, radius);
    }

    public Int3 FindSafeSpawn(Int3 centerChunk)
    {
        var worldX = centerChunk.X * ChunkData.Width + ChunkData.Width / 2;
        var worldZ = centerChunk.Z * ChunkData.Depth + ChunkData.Depth / 2;

        for (var y = ChunkData.Height - 1; y > 100; y--)
        {
            if (GetBlockAt(worldX, y, worldZ) == BlockType.Grass)
            {
                return new Int3(worldX, y + 2, worldZ);
            }
        }

        return new Int3(worldX, 130, worldZ);
    }

    public bool ApplyBlockCommand(BlockCommand cmd)
    {
        var chunk = chunkProvider.GetOrCreate(cmd.ChunkCoord);

        chunk.Set(
            cmd.LocalPos,
            cmd.Action == BlockAction.Break
                ? BlockType.Air
                : cmd.Block
        );

        chunkProvider.Save(chunk);
        return true;
    }

    public void UnloadFarChunks(Int3 center, int radius)
    {
        var toRemove = new List<Int3>();

        foreach (var coord in chunkProvider.GetLoadedCoords())
        {
            var dx = Math.Abs(coord.X - center.X);
            var dz = Math.Abs(coord.Z - center.Z);

            if (dx > radius || dz > radius)
            {
                toRemove.Add(coord);
            }
        }

        foreach (var coord in toRemove)
        {
            chunkProvider.Unload(coord);
        }
    }

    private BlockType GetBlockAt(int worldX, int worldY, int worldZ)
    {
        var worldPos = new Int3(worldX, worldY, worldZ);
        var chunkCoord = ChunkUtils.WorldToChunk(worldPos);
        var chunk = chunkProvider.GetOrCreate(chunkCoord);
        var localPos = ChunkUtils.WorldToLocal(worldPos, chunkCoord);

        return chunk.Get(localPos);
    }
}
