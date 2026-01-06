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

        public void Init(ChunkData data)
        {
            Data = data;
            Refresh();
        }

        public void SetData(ChunkData data)
        {
            Data = data;
            Refresh();
        }

        public void Refresh()
        {
            meshBuilder.Build(Data);
        }
    }
}