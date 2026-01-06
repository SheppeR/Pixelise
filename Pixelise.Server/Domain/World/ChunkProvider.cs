using Pixelise.Core.Math;
using Pixelise.Core.World;
using Pixelise.Server.Domain.World.Interfaces;

namespace Pixelise.Server.Domain.World;

public sealed class ChunkProvider(
    WorldData worldData,
    IChunkGenerator generator,
    IWorldDecorator decorator) : IChunkProvider
{
    public ChunkData GetOrCreate(Int3 coord)
    {
        var chunk = worldData.GetChunk(coord);
        if (chunk != null)
            return chunk;

        if (worldData.TryLoadChunk(coord, out chunk))
            return chunk;

        chunk = new ChunkData(coord);
        generator.Generate(chunk);
        decorator.Decorate(chunk);

        worldData.AddChunk(chunk);
        worldData.SaveChunk(chunk);

        return chunk;
    }

    public void Save(ChunkData chunk)
    {
        worldData.SaveChunk(chunk);
    }

    public void Unload(Int3 coord)
    {
        var chunk = worldData.GetChunk(coord);
        if (chunk == null)
            return;

        worldData.SaveChunk(chunk);
        worldData.RemoveChunk(coord);
    }

    public IEnumerable<Int3> GetLoadedCoords()
    {
        return worldData.GetAllCoords();
    }
}
