using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.World;

namespace Pixelise.Server.World;

public sealed class WorldSimulation
{
    private readonly WorldData world;
    private readonly ITerrainGenerator generator;

    public WorldSimulation(WorldData world)
    {
        this.world = world;
        generator = new DefaultTerrainGenerator();
    }

    // ========================
    // CHUNKS
    // ========================

    public ChunkData GetOrCreateChunk(Int3 coord)
    {
        var chunk = world.GetChunk(coord);
        if (chunk != null)
        {
            return chunk;
        }

        chunk = new ChunkData(coord);
        GenerateChunk(chunk);
        world.AddChunk(chunk);
        return chunk;
    }

    public IEnumerable<ChunkData> GenerateWorld(Int3 center, int radius)
    {
        var chunks = new List<ChunkData>();

        for (var x = -radius; x <= radius; x++)
            for (var z = -radius; z <= radius; z++)
            {
                var coord = new Int3(center.X + x, 0, center.Z + z);
                chunks.Add(GetOrCreateChunk(coord));
            }

        return chunks;
    }

    private void GenerateChunk(ChunkData chunk)
    {
        for (var x = 0; x < ChunkData.Width; x++)
            for (var y = 0; y < ChunkData.Height; y++)
                for (var z = 0; z < ChunkData.Depth; z++)
                {
                    var worldX = chunk.Coord.X * ChunkData.Width + x;
                    var worldZ = chunk.Coord.Z * ChunkData.Depth + z;

                    var block = generator.GetBlock(worldX, y, worldZ);
                    chunk.Set(new Int3(x, y, z), block);
                }
    }

    // ========================
    // SAFE SPAWN
    // ========================

    public Int3 FindSafeSpawn(Int3 chunkCoord)
    {
        var chunk = GetOrCreateChunk(chunkCoord);

        // recherche au centre du chunk
        var x = ChunkData.Width / 2;
        var z = ChunkData.Depth / 2;

        // scan vertical du haut vers le bas
        for (var y = ChunkData.Height - 2; y > 1; y--)
        {
            var feet = new Int3(x, y, z);
            var head = new Int3(x, y + 1, z);
            var ground = new Int3(x, y - 1, z);

            var feetBlock = chunk.Get(feet);
            var headBlock = chunk.Get(head);
            var groundBlock = chunk.Get(ground);

            if (feetBlock == BlockType.Air &&
                headBlock == BlockType.Air &&
                BlockRegistry.Get(groundBlock).IsSolid)
            {
                // position monde
                return new Int3(
                    chunk.Coord.X * ChunkData.Width + x,
                    y,
                    chunk.Coord.Z * ChunkData.Depth + z
                );
            }
        }

        // fallback ultra-safe
        return new Int3(
            chunk.Coord.X * ChunkData.Width + x,
            ChunkData.Height - 2,
            chunk.Coord.Z * ChunkData.Depth + z
        );
    }

    // ========================
    // BLOCK COMMANDS
    // ========================

    public bool Apply(BlockCommand cmd)
    {
        var chunk = GetOrCreateChunk(cmd.ChunkCoord);

        chunk.Set(
            cmd.LocalPos,
            cmd.Action == BlockAction.Break
                ? BlockType.Air
                : cmd.Block
        );

        return true;
    }

    public Int3 FindSafeSpawn(Int3 centerChunk, int searchRadius = 2)
    {
        Int3 best = default;
        var bestHeight = -1;

        for (var cx = -searchRadius; cx <= searchRadius; cx++)
        for (var cz = -searchRadius; cz <= searchRadius; cz++)
        {
            var coord = new Int3(
                centerChunk.X + cx,
                0,
                centerChunk.Z + cz
            );

            var chunk = GetOrCreateChunk(coord);

            for (var x = 0; x < ChunkData.Width; x++)
            for (var z = 0; z < ChunkData.Depth; z++)
            {
                for (var y = ChunkData.Height - 2; y > 1; y--)
                {
                    var feet = new Int3(x, y, z);
                    var head = new Int3(x, y + 1, z);
                    var ground = new Int3(x, y - 1, z);

                    if (chunk.Get(feet) != BlockType.Air) continue;
                    if (chunk.Get(head) != BlockType.Air) continue;

                    var groundBlock = chunk.Get(ground);
                    if (!BlockRegistry.Get(groundBlock).IsSolid) continue;
                    if (groundBlock == BlockType.Water) continue;

                    if (y > bestHeight)
                    {
                        bestHeight = y;
                        best = new Int3(
                            coord.X * ChunkData.Width + x,
                            y,
                            coord.Z * ChunkData.Depth + z
                        );
                    }
                }
            }
        }

        return bestHeight > 0
            ? best
            : new Int3(0, ChunkData.Height - 2, 0);
    }

}
