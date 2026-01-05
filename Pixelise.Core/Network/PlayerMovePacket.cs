using MessagePack;
using Pixelise.Core.Math;

namespace Pixelise.Core.Network
{
    [MessagePackObject]
    public class PlayerMovePacket
    {
        [Key(0)]
        public Int3 Position;

        [Key(1)]
        public float Yaw;
    }
}