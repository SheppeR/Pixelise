using Pixelise.Core.Blocks;

namespace Pixelise.Core.World
{
    public sealed class DefaultTerrainGenerator : ITerrainGenerator
    {
        // ========================
        // WORLD SETTINGS
        // ========================

        private const int SeaLevel = 32;

        private const int BaseHeight = 32;
        private const int HeightVariation = 24;

        // fréquences de bruit
        private const float LowFreq = 0.003f;   // continents
        private const float MidFreq = 0.01f;    // collines
        private const float HighFreq = 0.05f;   // détails

        // ========================
        // MAIN
        // ========================

        public BlockType GetBlock(int worldX, int worldY, int worldZ)
        {
            var height = GetHeight(worldX, worldZ);

            // 🌊 EAU
            if (worldY > height && worldY <= SeaLevel)
            {
                return BlockType.Water;
            }

            // 🌱 SURFACE
            if (worldY == height)
            {
                return BlockType.Grass;
            }

            // 🟫 SOUS-SOL
            if (worldY < height && worldY >= height - 4)
            {
                return BlockType.Dirt;
            }

            // 🪨 PROFOND
            if (worldY < height)
            {
                return BlockType.Stone;
            }

            return BlockType.Air;
        }

        // ========================
        // HEIGHTMAP (MINECRAFT-LIKE)
        // ========================

        private static int GetHeight(int x, int z)
        {
            // continents larges
            var continental = Perlin(x, z, LowFreq) * 0.6f;

            // collines moyennes
            var hills = Perlin(x, z, MidFreq) * 0.3f;

            // détails fins
            var details = Perlin(x, z, HighFreq) * 0.1f;

            var noise = continental + hills + details;

            // normalisation [0..1]
            noise = (noise + 1f) * 0.5f;

            var height = BaseHeight + noise * HeightVariation;

            return (int)System.Math.Floor(height);
        }

        // ========================
        // PERLIN
        // ========================

        private static float Perlin(int x, int z, float freq)
        {
            return UnityLikePerlin(x * freq, z * freq);
        }

        // Implémentation Perlin sans dépendance Unity
        private static float UnityLikePerlin(float x, float y)
        {
            var x0 = (int)System.Math.Floor(x);
            var x1 = x0 + 1;
            var y0 = (int)System.Math.Floor(y);
            var y1 = y0 + 1;

            var sx = Fade(x - x0);
            var sy = Fade(y - y0);

            var n0 = DotGridGradient(x0, y0, x, y);
            var n1 = DotGridGradient(x1, y0, x, y);
            var ix0 = Lerp(n0, n1, sx);

            n0 = DotGridGradient(x0, y1, x, y);
            n1 = DotGridGradient(x1, y1, x, y);
            var ix1 = Lerp(n0, n1, sx);

            return Lerp(ix0, ix1, sy);
        }

        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static float DotGridGradient(int ix, int iy, float x, float y)
        {
            var random = Hash(ix, iy);

            var angle = random * System.MathF.PI * 2f;
            var gx = System.MathF.Cos(angle);
            var gy = System.MathF.Sin(angle);

            var dx = x - ix;
            var dy = y - iy;

            return dx * gx + dy * gy;
        }

        private static float Hash(int x, int y)
        {
            var h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            return (h & 0x7fffffff) / (float)int.MaxValue;
        }
    }
}
