using Pixelise.Core.Blocks;
using SysMath = System.Math;

namespace Pixelise.Core.World
{
    public sealed class DefaultTerrainGenerator : ITerrainGenerator
    {
        // ========================
        // WORLD SETTINGS
        // ========================

        private const int SeaLevel = 100;
        private const int MinY = 0;
        private const int MaxY = 256;

        private const int BeachHeight = 2;
        private const int SandDepth = 4;

        // ========================
        // BIOME SETTINGS
        // ========================

        private static readonly SimplexNoise temperatureNoise = new SimplexNoise(1001);
        private static readonly SimplexNoise humidityNoise = new SimplexNoise(1002);

        // ========================
        // TERRAIN NOISE
        // ========================

        private static readonly SimplexNoise continentalnessNoise = new SimplexNoise(1234);
        private static readonly SimplexNoise erosionNoise = new SimplexNoise(2345);
        private static readonly SimplexNoise peaksValleysNoise = new SimplexNoise(3456);
        private static readonly SimplexNoise caveNoise = new SimplexNoise(4567);
        private static readonly SimplexNoise spaghettiNoise1 = new SimplexNoise(5678);
        private static readonly SimplexNoise spaghettiNoise2 = new SimplexNoise(6789);
        private static readonly SimplexNoise riverNoise = new SimplexNoise(7890);
        private static readonly SimplexNoise lakeNoise = new SimplexNoise(8901);
        private static readonly SimplexNoise treeNoise = new SimplexNoise(9001);

        // ========================
        // MAIN
        // ========================

        public BlockType GetBlock(int x, int y, int z)
        {
            if (y < MinY || y >= MaxY)
            {
                return BlockType.Air;
            }

            var density = GetDensity(x, y, z);
            var isSolid = density > 0;

            // ========================
            // AIR / WATER
            // ========================

            if (!isSolid)
            {
                if (y <= SeaLevel)
                {
                    return BlockType.Water;
                }

                return BlockType.Air;
            }

            var surfaceHeight = GetSurfaceHeight(x, z);

            // ========================
            // CAVES (UNDERGROUND ONLY)
            // ========================

            if (y > MinY + 5 && y < surfaceHeight - 6)
            {
                if (IsInCave(x, y, z))
                {
                    return BlockType.Air;
                }
            }

            // ========================
            // SURFACE DETECTION
            // ========================

            var aboveDensity = GetDensity(x, y + 1, z);
            if (aboveDensity <= 0)
            {
                return GetSurfaceBlock(x, y, z, surfaceHeight);
            }

            // ========================
            // SUB-SURFACE
            // ========================

            if (y > surfaceHeight - SandDepth && y < surfaceHeight)
            {
                if (SysMath.Abs(surfaceHeight - SeaLevel) <= BeachHeight)
                {
                    return BlockType.Sand;
                }

                return BlockType.Dirt;
            }

            return BlockType.Stone;
        }

        // ========================
        // SURFACE BLOCK (BIOMES)
        // ========================

        private BlockType GetSurfaceBlock(int x, int y, int z, int surfaceHeight)
        {
            // Beach
            if (SysMath.Abs(y - SeaLevel) <= BeachHeight)
            {
                return BlockType.Sand;
            }

            // Biome values
            var temp = temperatureNoise.Noise(x * 0.001, z * 0.001);
            var humidity = humidityNoise.Noise(x * 0.001, z * 0.001);

            // Snow biome
            if (temp < -0.3 && y > SeaLevel + 10)
            {
                return BlockType.Snow;
            }

            // Desert
            if (temp > 0.4 && humidity < -0.2)
            {
                return BlockType.Sand;
            }

            // Default grass
            return BlockType.Grass;
        }

        // ========================
        // DENSITY FUNCTION
        // ========================

        private double GetDensity(int x, int y, int z)
        {
            var continentalness = continentalnessNoise.Noise(x * 0.0005, z * 0.0005);
            var erosion = erosionNoise.Noise(x * 0.001, z * 0.001);
            var pv = peaksValleysNoise.Noise(x * 0.003, z * 0.003);

            var riverValue = SysMath.Abs(riverNoise.Noise(x * 0.0015, z * 0.0015));
            var lakeValue = lakeNoise.Noise(x * 0.001, z * 0.001);

            double baseHeight = SeaLevel;

            baseHeight += continentalness > 0
                ? continentalness * 40
                : continentalness * 20;

            baseHeight += erosion * 25;
            baseHeight += pv * 15;

            if (riverValue < 0.08)
            {
                baseHeight -= 15.0 * (1.0 - riverValue / 0.08);
            }

            if (lakeValue > 0.55)
            {
                baseHeight -= 12.0 * ((lakeValue - 0.55) / 0.45);
            }

            var offset = y - baseHeight;
            var density = -offset / 8.0;

            var noise3d = continentalnessNoise.Noise(x * 0.02, y * 0.02, z * 0.02);
            density += noise3d * 0.3;

            return density;
        }

        // ========================
        // SURFACE HEIGHT
        // ========================

        private int GetSurfaceHeight(int x, int z)
        {
            var continentalness = continentalnessNoise.Noise(x * 0.0005, z * 0.0005);
            var erosion = erosionNoise.Noise(x * 0.001, z * 0.001);
            var pv = peaksValleysNoise.Noise(x * 0.003, z * 0.003);

            double baseHeight = SeaLevel;
            baseHeight += continentalness > 0 ? continentalness * 40 : continentalness * 20;
            baseHeight += erosion * 25;
            baseHeight += pv * 15;

            return (int)SysMath.Floor(baseHeight);
        }

        // ========================
        // CAVES
        // ========================

        private bool IsInCave(int x, int y, int z)
        {
            var zone = caveNoise.Noise(x * 0.01, z * 0.01);
            if (zone < 0.2)
            {
                return false;
            }

            var a = spaghettiNoise1.Noise(x * 0.02, y * 0.03, z * 0.02);
            var b = spaghettiNoise2.Noise(x * 0.02, y * 0.03, z * 0.02);

            return SysMath.Abs(a) < 0.08 && SysMath.Abs(b) < 0.08;
        }
    }
}