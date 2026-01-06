using Pixelise.Core.Math;
using Pixelise.Core.World;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World
{
    public class WorldRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private Transform player;

        [Header("Settings")]
        [SerializeField] private int renderDistance = 6;
        [SerializeField] private int maxChunksPerFrame = 1;

        private readonly Dictionary<Int3, ChunkView> chunkViews = new();
        private readonly Queue<ChunkData> pending = new();

        private Int3 lastPlayerChunk;

        [SerializeField] private ClientSpawnManager spawnManager;

        private void Awake()
        {
            if (player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
            }

            if (spawnManager == null)
                spawnManager = FindFirstObjectByType<ClientSpawnManager>();

            Debug.Log($"[WORLD] SpawnManager found = {spawnManager != null}");
        }

        private void Update()
        {
            ProcessPendingChunks();
            UnloadFarChunks();
        }

        // ========================
        // PUBLIC API
        // ========================

        public void AddOrUpdateChunk(ChunkData data)
        {
            if (chunkViews.ContainsKey(data.Coord))
                return;

            pending.Enqueue(data);
        }

        public ChunkView GetChunk(Int3 coord)
        {
            chunkViews.TryGetValue(coord, out var view);
            return view;
        }

        // ========================
        // INTERNAL
        // ========================

        private void ProcessPendingChunks()
        {
            var count = 0;

            while (pending.Count > 0 && count < maxChunksPerFrame)
            {
                var data = pending.Dequeue();
                BuildChunk(data);
                count++;
            }
        }

        private void BuildChunk(ChunkData data)
        {
            var obj = Instantiate(chunkPrefab, transform);
            obj.name = $"Chunk_{data.Coord.X}_{data.Coord.Z}";
            obj.transform.position = new Vector3(
                data.Coord.X * ChunkData.Width,
                0,
                data.Coord.Z * ChunkData.Depth
            );

            var chunkView = obj.GetComponent<ChunkView>();
            chunkView.Init(data);

            chunkViews[data.Coord] = chunkView;

            if (spawnManager != null && spawnManager.HasPendingSpawn)
            {
                var spawnChunk = spawnManager.GetSpawnChunk();

                if (spawnChunk.X == data.Coord.X &&
                    spawnChunk.Z == data.Coord.Z)
                {
                    spawnManager.TryApplySpawn();
                }
            }
        }


        private void UnloadFarChunks()
        {
            if (player == null)
                return;

            var worldPos = player.position.ToCore();
            var currentChunk = ChunkUtils.WorldToChunk(worldPos);

            if (currentChunk.X == lastPlayerChunk.X &&
                currentChunk.Z == lastPlayerChunk.Z)
                return;

            lastPlayerChunk = currentChunk;

            var toRemove = new List<Int3>();

            foreach (var kv in chunkViews)
            {
                var coord = kv.Key;

                var dx = Mathf.Abs(coord.X - currentChunk.X);
                var dz = Mathf.Abs(coord.Z - currentChunk.Z);

                if (dx > renderDistance || dz > renderDistance)
                {
                    toRemove.Add(coord);
                }
            }

            foreach (var coord in toRemove)
            {
                var view = chunkViews[coord];
                Destroy(view.gameObject);
                chunkViews.Remove(coord);
            }
        }
    }
}
