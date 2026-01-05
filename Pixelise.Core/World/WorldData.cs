using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using Pixelise.Core.Math;

namespace Pixelise.Core.World
{
    public sealed class WorldData
    {
        private readonly Dictionary<Int3, ChunkData> chunks =
            new Dictionary<Int3, ChunkData>();

        private readonly string metaPath;

        private readonly string worldPath;

        public WorldData()
        {
            worldPath = Path.Combine("Worlds", "default");
            Directory.CreateDirectory(worldPath);

            metaPath = Path.Combine(worldPath, "world.json");

            // 🔥 CHARGE OU CRÉE LA SEED
            if (File.Exists(metaPath))
            {
                var meta = MessagePackSerializer.Deserialize<WorldSeed>(
                    File.ReadAllBytes(metaPath));

                Seed = meta.Seed;
            }
            else
            {
                Seed = System.Math.Abs(Environment.TickCount);
                SaveMeta();
            }
        }

        public int Seed { get; }

        private void SaveMeta()
        {
            var meta = new WorldSeed { Seed = Seed };
            File.WriteAllBytes(metaPath, MessagePackSerializer.Serialize(meta));
        }

        // ========================
        // CHUNKS
        // ========================

        public ChunkData GetChunk(Int3 coord)
        {
            chunks.TryGetValue(coord, out var chunk);
            return chunk;
        }

        public void AddChunk(ChunkData chunk)
        {
            chunks[chunk.Coord] = chunk;
        }

        public bool TryLoadChunk(Int3 coord, out ChunkData chunk)
        {
            var path = GetChunkPath(coord);
            chunk = null;

            if (!File.Exists(path))
            {
                return false;
            }

            var data = MessagePackSerializer.Deserialize<SavedChunk>(
                File.ReadAllBytes(path));

            chunk = new ChunkData(coord);
            chunk.FromFlatArray(data.Blocks);

            chunks[coord] = chunk;
            return true;
        }

        public void SaveChunk(ChunkData chunk)
        {
            var data = new SavedChunk
            {
                Coord = chunk.Coord,
                Blocks = chunk.ToFlatArray()
            };

            File.WriteAllBytes(
                GetChunkPath(chunk.Coord),
                MessagePackSerializer.Serialize(data));
        }

        private string GetChunkPath(Int3 coord)
        {
            return Path.Combine(
                worldPath,
                $"chunk_{coord.X}_{coord.Z}.bin");
        }
    }
}