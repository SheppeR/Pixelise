using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.World;

namespace Pixelise.Server.Domain.World.Interfaces;

public interface IWorldService
{
    IEnumerable<ChunkData> GetChunks(Int3 center, int radius);
    IEnumerable<ChunkData> GetSpawnChunks(Int3 center, int radius);
    Int3 FindSafeSpawn(Int3 centerChunk);
    bool ApplyBlockCommand(BlockCommand cmd);
    void UnloadFarChunks(Int3 center, int radius);
}