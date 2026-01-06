using Pixelise.Core.World;

namespace Pixelise.Server.Domain.World.Interfaces;

public interface IChunkGenerator
{
    void Generate(ChunkData chunk);
}