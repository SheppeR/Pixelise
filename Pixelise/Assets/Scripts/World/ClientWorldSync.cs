using MessagePack;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using UnityEngine;

namespace World
{
    public class ClientWorldSync : MonoBehaviour
    {
        [SerializeField] private WorldRenderer renderer;

        private void Awake()
        {
            if (renderer == null)
                renderer = FindFirstObjectByType<WorldRenderer>();
        }

        public void OnChunkPacket(NetPacket packet)
        {
            var data = MessagePackSerializer.Deserialize<ChunkDataPacket>(packet.Payload);

            var chunk = new ChunkData(data.ChunkCoord);
            chunk.FromFlatArray(data.Blocks);

            renderer.AddOrUpdateChunk(chunk);
        }
    }
}