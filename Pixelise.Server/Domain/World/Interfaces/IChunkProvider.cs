using Pixelise.Core.Math;
using Pixelise.Core.World;

namespace Pixelise.Server.Domain.World.Interfaces;

public interface IChunkProvider
{
    ChunkData GetOrCreate(Int3 coord);

    void Save(ChunkData chunk);

    void Unload(Int3 coord);

    IEnumerable<Int3> GetLoadedCoords();
}