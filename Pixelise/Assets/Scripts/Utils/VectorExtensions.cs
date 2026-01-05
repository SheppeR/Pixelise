using Pixelise.Core.Math;
using UnityEngine;

namespace Utils
{
    public static class VectorExtensions
    {
        public static Int3 ToCore(this Vector3 v)
        {
            return new Int3(
                Mathf.FloorToInt(v.x),
                Mathf.FloorToInt(v.y),
                Mathf.FloorToInt(v.z)
            );
        }

        public static Vector3 ToUnity(this Int3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}