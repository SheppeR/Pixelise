using System;
using Pixelise.Core.Blocks;
using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    public sealed class TreeGenerator
    {
        private static readonly SimplexNoise temperatureNoise =
            new SimplexNoise(1001);

        private static readonly SimplexNoise humidityNoise =
            new SimplexNoise(1002);

        public void GenerateTrees(ChunkData chunk)
        {
            var rand = new Random(
                (chunk.Coord.X * 73856093) ^
                (chunk.Coord.Z * 19349663)
            );

            for (var x = 2; x < ChunkData.Width - 2; x++)
            for (var z = 2; z < ChunkData.Depth - 2; z++)
            {
                if (rand.NextDouble() > 0.004)
                {
                    continue;
                }

                for (var y = ChunkData.Height - 2; y > 1; y--)
                {
                    if (chunk.Get(new Int3(x, y, z)) != BlockType.Grass)
                    {
                        continue;
                    }

                    if (chunk.Get(new Int3(x, y + 1, z)) != BlockType.Air)
                    {
                        break;
                    }

                    var worldX = chunk.Coord.X * ChunkData.Width + x;
                    var worldZ = chunk.Coord.Z * ChunkData.Depth + z;

                    var type = GetTreeType(worldX, worldZ, rand);
                    PlaceTree(chunk, x, y + 1, z, type, rand);
                    break;
                }
            }
        }

        private static TreeType GetTreeType(int wx, int wz, Random rand)
        {
            var temp = temperatureNoise.Noise(wx * 0.001, wz * 0.001);
            var humidity = humidityNoise.Noise(wx * 0.001, wz * 0.001);

            if (temp > 0.4 && humidity < -0.2)
            {
                return TreeType.Acacia;
            }

            if (temp < 0.15)
            {
                return rand.NextDouble() < 0.6
                    ? TreeType.Birch
                    : TreeType.Oak;
            }

            return TreeType.Oak;
        }

        private static void PlaceTree(
            ChunkData chunk,
            int x,
            int y,
            int z,
            TreeType type,
            Random rand)
        {
            var height =
                type == TreeType.Birch ? rand.Next(6, 8) :
                type == TreeType.Acacia ? rand.Next(4, 6) :
                rand.Next(5, 7);

            var wood =
                type == TreeType.Birch ? BlockType.WoodBirch :
                type == TreeType.Acacia ? BlockType.WoodAcacia :
                BlockType.WoodOak;

            for (var i = 0; i < height; i++)
            {
                chunk.Set(new Int3(x, y + i, z), wood);
            }

            var canopyHeight = rand.Next(3, 5);
            var baseRadius = rand.Next(2, 4);

            for (var dy = 0; dy < canopyHeight; dy++)
            {
                var radius = baseRadius - dy / 2;

                for (var dx = -radius; dx <= radius; dx++)
                for (var dz = -radius; dz <= radius; dz++)
                {
                    var dist2 = dx * dx + dz * dz;
                    if (dist2 > radius * radius)
                    {
                        continue;
                    }

                    if (rand.NextDouble() < 0.15)
                    {
                        continue;
                    }

                    chunk.Set(
                        new Int3(x + dx, y + height - 1 + dy, z + dz),
                        BlockType.Leaves
                    );
                }
            }
        }
    }
}