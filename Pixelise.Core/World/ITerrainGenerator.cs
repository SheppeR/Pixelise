using Pixelise.Core.Blocks;

namespace Pixelise.Core.World
{
    public interface ITerrainGenerator
    {
        BlockType GetBlock(int worldX, int worldY, int worldZ);
    }
}