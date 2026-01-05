using System;

namespace Pixelise.Core.World
{
    /// <summary>
    ///     Implémentation Simplex Noise 2D / 3D (Minecraft-like)
    ///     Déterministe, sans dépendance Unity
    /// </summary>
    public sealed class SimplexNoise
    {
        private static readonly int[][] grad3 =
        {
            new[] { 1, 1, 0 }, new[] { -1, 1, 0 }, new[] { 1, -1, 0 }, new[] { -1, -1, 0 },
            new[] { 1, 0, 1 }, new[] { -1, 0, 1 }, new[] { 1, 0, -1 }, new[] { -1, 0, -1 },
            new[] { 0, 1, 1 }, new[] { 0, -1, 1 }, new[] { 0, 1, -1 }, new[] { 0, -1, -1 }
        };

        private readonly int[] perm = new int[512];

        public SimplexNoise(int seed)
        {
            var rand = new Random(seed);
            var p = new int[256];

            for (var i = 0; i < 256; i++)
            {
                p[i] = i;
            }

            for (var i = 255; i > 0; i--)
            {
                var j = rand.Next(i + 1);
                (p[i], p[j]) = (p[j], p[i]);
            }

            for (var i = 0; i < 512; i++)
            {
                perm[i] = p[i & 255];
            }
        }

        // ========================
        // 2D SIMPLEX NOISE
        // ========================

        public double Noise(double xin, double yin)
        {
            double n0, n1, n2;

            const double F2 = 0.5 * (1.7320508075688772 - 1.0);
            var s = (xin + yin) * F2;
            var i = FastFloor(xin + s);
            var j = FastFloor(yin + s);

            const double G2 = (3.0 - 1.7320508075688772) / 6.0;
            var t = (i + j) * G2;
            var X0 = i - t;
            var Y0 = j - t;
            var x0 = xin - X0;
            var y0 = yin - Y0;

            int i1, j1;
            if (x0 > y0)
            {
                i1 = 1;
                j1 = 0;
            }
            else
            {
                i1 = 0;
                j1 = 1;
            }

            var x1 = x0 - i1 + G2;
            var y1 = y0 - j1 + G2;
            var x2 = x0 - 1.0 + 2.0 * G2;
            var y2 = y0 - 1.0 + 2.0 * G2;

            var ii = i & 255;
            var jj = j & 255;

            var t0 = 0.5 - x0 * x0 - y0 * y0;
            n0 = t0 < 0 ? 0 : (t0 *= t0) * t0 * Dot(grad3[perm[ii + perm[jj]] % 12], x0, y0);

            var t1 = 0.5 - x1 * x1 - y1 * y1;
            n1 = t1 < 0 ? 0 : (t1 *= t1) * t1 * Dot(grad3[perm[ii + i1 + perm[jj + j1]] % 12], x1, y1);

            var t2 = 0.5 - x2 * x2 - y2 * y2;
            n2 = t2 < 0 ? 0 : (t2 *= t2) * t2 * Dot(grad3[perm[ii + 1 + perm[jj + 1]] % 12], x2, y2);

            return 70.0 * (n0 + n1 + n2);
        }

        // ========================
        // 3D SIMPLEX NOISE
        // ========================

        public double Noise(double x, double y, double z)
        {
            return (Noise(x, y) + Noise(y, z) + Noise(x, z)) / 3.0;
        }

        private static int FastFloor(double x)
        {
            var xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }

        private static double Dot(int[] g, double x, double y)
        {
            return g[0] * x + g[1] * y;
        }
    }
}