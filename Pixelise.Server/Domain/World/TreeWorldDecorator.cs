using Pixelise.Core.World;
using Pixelise.Server.Domain.World.Interfaces;

namespace Pixelise.Server.Domain.World;

public sealed class TreeWorldDecorator : IWorldDecorator
{
    private readonly TreeGenerator _treeGenerator = new();

    public void Decorate(ChunkData chunk)
    {
        _treeGenerator.GenerateTrees(chunk);
    }
}