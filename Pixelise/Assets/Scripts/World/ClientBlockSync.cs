using MessagePack;
using Pixelise.Core.Commands;
using Pixelise.Core.Network;
using Pixelise.Core.Blocks;
using UnityEngine;

namespace World
{
    public class ClientBlockSync : MonoBehaviour
    {
        [SerializeField] private WorldRenderer renderer;

        private void Awake()
        {
            if (renderer == null)
                renderer = FindFirstObjectByType<WorldRenderer>();
        }

        public void OnBlockPacket(NetPacket packet)
        {
            var cmd = MessagePackSerializer.Deserialize<BlockCommand>(packet.Payload);

            var view = renderer.GetChunk(cmd.ChunkCoord);
            if (view == null) return;

            view.Data.Set(
                cmd.LocalPos,
                cmd.Action == BlockAction.Break
                    ? BlockType.Air
                    : cmd.Block
            );

            view.Refresh();
        }
    }
}