using System.Collections.Generic;
using Pixelise.Core.Math;
using Pixelise.Core.World;
using UnityEngine;

namespace World
{
    public class WorldRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject chunkPrefab;

        [Header("Settings")]
        [SerializeField] private int renderDistance = 6;

        private readonly Dictionary<Int3, ChunkView> chunkViews = new();
        private WorldData worldData;

        private void Awake()
        {
            worldData = new WorldData();

            if (player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }
        }

        // ========================
        // 🔥 NO MORE CLIENT GENERATION
        // ========================

        private void Update()
        {
            // plus rien ici : le serveur pousse les chunks
        }

        // ========================
        // 🌍 CHUNK FROM SERVER
        // ========================

        public void AddOrUpdateChunk(ChunkData data)
        {
            if (chunkViews.TryGetValue(data.Coord, out var view))
            {
                view.Refresh();
                return;
            }

            // créer la vue
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
        }
    }
}