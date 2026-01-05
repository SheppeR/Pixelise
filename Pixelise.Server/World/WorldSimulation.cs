using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.World;

namespace Pixelise.Server.World;

public sealed class WorldSimulation
{
    private readonly ITerrainGenerator generator;
    private readonly TreeGenerator treeGenerator = new();
    private readonly WorldData world;

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

        // charge depuis le disque si possible
        if (world.TryLoadChunk(coord, out chunk))
        {
            return chunk;
        }

        // sinon génère
        chunk = new ChunkData(coord);
        GenerateChunk(chunk);
        world.AddChunk(chunk);
        world.SaveChunk(chunk);

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

    public void PreGenerateWorld(Int3 center, int radius)
    {
        for (var x = -radius; x <= radius; x++)
        for (var z = -radius; z <= radius; z++)
        {
            var coord = new Int3(center.X + x, 0, center.Z + z);
            GetOrCreateChunk(coord);
        }
    }

    private void GenerateChunk(ChunkData chunk)
    {
        for (var x = 0; x < ChunkData.Width; x++)
        for (var y = 0; y < ChunkData.Height; y++)
        for (var z = 0; z < ChunkData.Depth; z++)
        {
            var worldX = chunk.Coord.X * ChunkData.Width + x;
            var worldZ = chunk.Coord.Z * ChunkData.Depth + z;

            // 🔥 seed persistante du monde
            var block = generator.GetBlock(
                worldX + world.Seed,
                y,
                worldZ + world.Seed);

            chunk.Set(new Int3(x, y, z), block);
        }

        treeGenerator.GenerateTrees(chunk);
    }

    // ========================
    // SAFE SPAWN (SIMPLE & NON BLOQUANT)
    // ========================

    public Int3 FindSafeSpawn(Int3 centerChunk)
    {
        var worldX = centerChunk.X * ChunkData.Width + ChunkData.Width / 2;
        var worldZ = centerChunk.Z * ChunkData.Depth + ChunkData.Depth / 2;

        // au-dessus du niveau de la mer
        for (var y = ChunkData.Height - 1; y > 100; y--)
        {
            if (GetBlockAt(worldX, y, worldZ) == BlockType.Grass)
            {
                return new Int3(worldX, y + 2, worldZ);
            }
        }

        // fallback sûr
        return new Int3(worldX, 130, worldZ);
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

        world.SaveChunk(chunk);
        return true;
    }

    // ========================
    // WORLD ACCESS
    // ========================

    private BlockType GetBlockAt(int worldX, int worldY, int worldZ)
    {
        var worldPos = new Int3(worldX, worldY, worldZ);
        var chunkCoord = ChunkUtils.WorldToChunk(worldPos);
        var chunk = GetOrCreateChunk(chunkCoord);
        var localPos = ChunkUtils.WorldToLocal(worldPos, chunkCoord);

        return chunk.Get(localPos);
    }
}