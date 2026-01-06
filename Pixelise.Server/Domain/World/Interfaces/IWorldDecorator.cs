using Pixelise.Core.World;

namespace Pixelise.Server.Domain.World.Interfaces;

public interface IWorldDecorator
{
    void Decorate(ChunkData chunk);
}