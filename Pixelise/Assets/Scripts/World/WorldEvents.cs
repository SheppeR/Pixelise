using MessagePack;
using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
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
        private static int receivedChunks;

        // ========================
        // PLAYER REGISTRATION
        // ========================

        public static void RegisterLocalPlayer(GameObject player)
        {
            localPlayer = player;
            localPlayer.SetActive(false);

            // spawn déjà reçu ?
            if (pendingSpawn != null && receivedChunks > 0)
            {
                ApplySpawn(pendingSpawn);
                pendingSpawn = null;
            }
        }

        // ========================
        // NETWORK ENTRY
        // ========================

        public static void OnPacket(NetPacket packet)
        {
            switch (packet.Type)
            {
                // ========================
                // 🌍 CHUNK DATA (SERVEUR)
                // ========================
                case PacketType.ChunkData:
                    {
                        var data = MessagePackSerializer.Deserialize<ChunkDataPacket>(packet.Payload);

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            var chunkData = new ChunkData(data.ChunkCoord);
                            chunkData.FromFlatArray(data.Blocks);

                            var renderer = Object.FindObjectOfType<WorldRenderer>();
                            if (renderer == null)
                            {
                                Debug.LogError("WorldRenderer not found");
                                return;
                            }

                            renderer.AddOrUpdateChunk(chunkData);
                            receivedChunks++;

                            // spawn en attente ?
                            if (pendingSpawn != null)
                            {
                                ApplySpawn(pendingSpawn);
                                pendingSpawn = null;
                            }
                        });

                        break;
                    }

                // ========================
                // 🧍 PLAYER SPAWN
                // ========================
                case PacketType.PlayerSpawn:
                    {
                        var spawn = MessagePackSerializer.Deserialize<PlayerSpawnPacket>(packet.Payload);

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            if (localPlayer == null || receivedChunks == 0)
                            {
                                pendingSpawn = spawn;
                                Debug.Log("Spawn reçu mais monde pas prêt → attente");
                                return;
                            }

                            ApplySpawn(spawn);
                        });

                        break;
                    }

                // ========================
                // 🧱 BLOCK UPDATE
                // ========================
                case PacketType.BlockCommand:
                    {
                        var cmd = MessagePackSerializer.Deserialize<BlockCommand>(packet.Payload);

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            var view = ChunkViewManager.Get(cmd.ChunkCoord);
                            if (view == null)
                            {
                                Debug.LogWarning($"Chunk not found {cmd.ChunkCoord}");
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
        // APPLY SPAWN
        // ========================

        private static void ApplySpawn(PlayerSpawnPacket spawn)
        {
            localPlayer.transform.position =
                VectorExtensions.ToUnity(spawn.Position) + Vector3.up * 0.1f;

            localPlayer.transform.rotation =
                Quaternion.Euler(0f, spawn.Yaw, 0f);

            localPlayer.SetActive(true);

            Debug.Log($"Player spawned at {spawn.Position.X},{spawn.Position.Y},{spawn.Position.Z}");
        }
    }
}
