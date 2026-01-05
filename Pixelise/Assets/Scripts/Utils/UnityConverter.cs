using Pixelise.Core.Math;
using UnityEngine;

namespace Utils
{
    public static class UnityConverter
    {
        public static Vector3Int ToUnity(this Int3 v)
        {
            return new Vector3Int(v.X, v.Y, v.Z);
        }

        public static Int3 ToCore(this Vector3Int v)
        {
            return new Int3(v.x, v.y, v.z);
        }
    }
}