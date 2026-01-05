using System.Collections.Generic;

namespace Pixelise.Core.Blocks
{
    public static class BlockRegistry
    {
        private static readonly Dictionary<BlockType, BlockDefinition> definitions =
            new Dictionary<BlockType, BlockDefinition>
            {
                { BlockType.Air, new BlockDefinition { Type = BlockType.Air, IsSolid = false, IsTransparent = true } },
                { BlockType.Water, new BlockDefinition { Type = BlockType.Water, IsSolid = false, IsTransparent = true } },
                { BlockType.Leaves, new BlockDefinition { Type = BlockType.Leaves, IsSolid = true, IsTransparent = true } },

                { BlockType.Dirt, new BlockDefinition { Type = BlockType.Dirt, IsSolid = true, IsBreakable = true } },
                { BlockType.Grass, new BlockDefinition { Type = BlockType.Grass, IsSolid = true, IsBreakable = true } },
                { BlockType.Stone, new BlockDefinition { Type = BlockType.Stone, IsSolid = true, IsBreakable = true } },
                { BlockType.Sand, new BlockDefinition { Type = BlockType.Sand, IsSolid = true, IsBreakable = true } },
                { BlockType.Snow, new BlockDefinition { Type = BlockType.Snow, IsSolid = true, IsBreakable = true } },
                { BlockType.WoodOak, new BlockDefinition { Type = BlockType.WoodOak, IsSolid = true, IsBreakable = true } },
                { BlockType.WoodBirch, new BlockDefinition { Type = BlockType.WoodBirch, IsSolid = true, IsBreakable = true } },
                { BlockType.WoodDarkOak, new BlockDefinition { Type = BlockType.WoodDarkOak, IsSolid = true, IsBreakable = true } },
                { BlockType.WoodAcacia, new BlockDefinition { Type = BlockType.WoodAcacia, IsSolid = true, IsBreakable = true } }
            };

        public static BlockDefinition Get(BlockType type)
        {
            return definitions[type];
        }
    }
}