using System;
using System.Collections.Generic;
using Pixelise.Core.Blocks;
using UnityEngine;

namespace World
{
    public static class BlockUV
    {
        public const int AtlasSize = 32;
        private const float Padding = 0.001f;

        private static readonly Dictionary<(BlockType, BlockFace), Vector2Int>
            uvMap = new();

        static BlockUV()
        {
            // 🌱 GRASS
            Add(BlockType.Grass, BlockFace.Top, 0, 0);
            Add(BlockType.Grass, BlockFace.Bottom, 2, 0);
            AddSides(BlockType.Grass, 3, 0);

            // 🟫 DIRT
            AddAll(BlockType.Dirt, 2, 0);

            // 🪨 STONE
            AddAll(BlockType.Stone, 1, 0);

            // 🏜️ SAND
            AddAll(BlockType.Sand, 4, 0);

            // 🌊 WATER
            AddAll(BlockType.Water, 7, 0);

            // ❄️ SNOW
            Add(BlockType.Snow, BlockFace.Top, 5, 0);
            Add(BlockType.Snow, BlockFace.Bottom, 2, 0);
            AddSides(BlockType.Snow, 6, 0);
        }

        private static void Add(BlockType type, BlockFace face, int x, int y)
        {
            uvMap[(type, face)] = new Vector2Int(x, y);
        }

        private static void AddAll(BlockType type, int x, int y)
        {
            foreach (BlockFace face in Enum.GetValues(typeof(BlockFace)))
            {
                Add(type, face, x, y);
            }
        }

        private static void AddSides(BlockType type, int x, int y)
        {
            Add(type, BlockFace.Front, x, y);
            Add(type, BlockFace.Back, x, y);
            Add(type, BlockFace.Left, x, y);
            Add(type, BlockFace.Right, x, y);
        }

        public static Vector2[] GetUVs(BlockType type, BlockFace face)
        {
            if (!uvMap.TryGetValue((type, face), out var pos))
            {
                return new Vector2[4];
            }

            var tile = 1f / AtlasSize;

            var xMin = pos.x * tile + Padding;
            var yMin = 1f - (pos.y + 1) * tile + Padding;
            var xMax = (pos.x + 1) * tile - Padding;
            var yMax = 1f - pos.y * tile - Padding;

            return new[]
            {
                new Vector2(xMin, yMin),
                new Vector2(xMax, yMin),
                new Vector2(xMax, yMax),
                new Vector2(xMin, yMax)
            };
        }
    }
}