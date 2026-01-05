using System.Collections.Generic;
using Pixelise.Core.Math;

namespace World
{
    public static class ChunkViewManager
    {
        private static readonly Dictionary<Int3, ChunkView> chunks = new();

        public static void Register(ChunkView view)
        {
            var coord = view.Data.Coord;
            chunks[coord] = view;
        }

        public static void Unregister(Int3 coord)
        {
            chunks.Remove(coord);
        }

        public static ChunkView Get(Int3 coord)
        {
            chunks.TryGetValue(coord, out var view);
            return view;
        }
    }
}