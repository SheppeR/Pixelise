using MessagePack;

namespace Pixelise.Core.World
{
    [MessagePackObject]
    public sealed class WorldSeed
    {
        [Key(0)]
        public int Seed;
    }
}