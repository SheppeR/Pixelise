using Pixelise.Core.World;
using UnityEngine;

namespace World
{
    public class ChunkView : MonoBehaviour
    {
        private ChunkMeshBuilder meshBuilder;
        public ChunkData Data { get; private set; }

        private void Awake()
        {
            meshBuilder = GetComponent<ChunkMeshBuilder>();
        }

        private void OnDestroy()
        {
            if (Data != null)
            {
                ChunkViewManager.Unregister(Data.Coord);
            }
        }

        public void Init(ChunkData data)
        {
            Data = data;

            // 🔥 REGISTER ICI, PAS AVANT
            ChunkViewManager.Register(this);

            Refresh();
        }

        public void Refresh()
        {
            meshBuilder.Build(Data);
        }
    }
}