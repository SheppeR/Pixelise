using System.Collections.Generic;
using MessagePack;
using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using UnityEngine;
using Utils;

namespace World
{
    public static class WorldEvents
    {
        private static GameObject localPlayer;
        private static PlayerSpawnPacket pendingSpawn;

        // 🔥 chunk requis pour le spawn
        private static Int3 spawnChunk;
        private static bool spawnChunkReady;

        // 🔥 file de chunks à construire
        private static readonly Queue<ChunkData> pendingChunks = new();

        // ========================
        // PLAYER REGISTRATION
        // ========================

        public static void RegisterLocalPlayer(GameObject player)
        {
            localPlayer = player;
            localPlayer.SetActive(false);
        }

        // ========================
        // NETWORK ENTRY
        // ========================

        public static void OnPacket(NetPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.ChunkData:
                {
                    var data = MessagePackSerializer.Deserialize<ChunkDataPacket>(packet.Payload);

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        var chunkData = new ChunkData(data.ChunkCoord);
                        chunkData.FromFlatArray(data.Blocks);

                        pendingChunks.Enqueue(chunkData);
                    });

                    break;
                }

                case PacketType.PlayerSpawn:
                {
                    var spawn = MessagePackSerializer.Deserialize<PlayerSpawnPacket>(packet.Payload);

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        pendingSpawn = spawn;
                        spawnChunk = ChunkUtils.WorldToChunk(spawn.Position);
                        spawnChunkReady = false;
                    });

                    break;
                }

                case PacketType.BlockCommand:
                {
                    var cmd = MessagePackSerializer.Deserialize<BlockCommand>(packet.Payload);

                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        var view = ChunkViewManager.Get(cmd.ChunkCoord);
                        if (view == null)
                        {
                            return;
                        }

                        view.Data.Set(
                            cmd.LocalPos,
                            cmd.Action == BlockAction.Break
                                ? BlockType.Air
                                : cmd.Block
                        );

                        view.Refresh();
                    });

                    break;
                }
            }
        }

        // ========================
        // PROCESS CHUNK QUEUE
        // ========================

        public static void ProcessChunkQueue()
        {
            if (pendingChunks.Count == 0)
            {
                return;
            }

            var chunk = pendingChunks.Dequeue();

            var renderer = Object.FindObjectOfType<WorldRenderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.AddOrUpdateChunk(chunk);

            // 🔥 est-ce le chunk du spawn ?
            if (pendingSpawn != null &&
                chunk.Coord.X == spawnChunk.X &&
                chunk.Coord.Z == spawnChunk.Z)
            {
                spawnChunkReady = true;
            }

            // 🔥 spawn uniquement quand le sol existe
            if (pendingSpawn != null && spawnChunkReady)
            {
                ApplySpawn(pendingSpawn);
                pendingSpawn = null;
            }
        }

        // ========================
        // APPLY SPAWN
        // ========================

        private static void ApplySpawn(PlayerSpawnPacket spawn)
        {
            localPlayer.transform.position =
                VectorExtensions.ToUnity(spawn.Position) + Vector3.up * 0.1f;

            localPlayer.transform.rotation =
                Quaternion.Euler(0f, spawn.Yaw, 0f);

            localPlayer.SetActive(true);

            Debug.Log("Player spawned safely (ground ready)");
        }
    }
}