using MessagePack;

namespace Pixelise.Core.Math
{
    [MessagePackObject]
    public struct Int3
    {
        [Key(0)]
        public int X;

        [Key(1)]
        public int Y;

        [Key(2)]
        public int Z;

        public Int3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}