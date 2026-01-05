using MessagePack;
using Network;
using Pixelise.Core.Blocks;
using Pixelise.Core.Commands;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using UnityEngine;
using World;

namespace Player
{
    public class BlockInteractor : MonoBehaviour
    {
        public float reach = 5f;
        public Camera cam;
        public BlockType selectedBlock = BlockType.Dirt;
        public LiteNetClient network;

        private void Awake()
        {
            if (cam == null)
            {
                cam = GetComponentInChildren<Camera>();
            }

            if (cam == null && Camera.main != null)
            {
                cam = Camera.main;
            }
        }

        // ========================
        // BREAK
        // ========================
        public void Break()
        {
            if (!RaycastBlock(BlockAction.Break, out var chunkView, out var localPos))
            {
                return;
            }

            Send(chunkView, localPos, BlockAction.Break);
        }

        // ========================
        // PLACE
        // ========================
        public void Use()
        {
            if (!RaycastBlock(BlockAction.Place, out var chunkView, out var localPos))
            {
                return;
            }

            Send(chunkView, localPos, BlockAction.Place);
        }

        // ========================
        // SEND
        // ========================
        private void Send(ChunkView chunkView, Int3 localPos, BlockAction action)
        {
            var cmd = new BlockCommand
            {
                ChunkCoord = chunkView.Data.Coord,
                LocalPos = localPos,
                Action = action,
                Block = selectedBlock
            };

            network.Send(new NetPacket
            {
                Type = PacketType.BlockCommand,
                Payload = MessagePackSerializer.Serialize(cmd)
            });
        }

        // ========================
        // RAYCAST
        // ========================
        private bool RaycastBlock(
            BlockAction action,
            out ChunkView chunkView,
            out Int3 localPos)
        {
            chunkView = null;
            localPos = default;

            if (cam == null)
            {
                return false;
            }

            if (!Physics.Raycast(
                    cam.transform.position,
                    cam.transform.forward,
                    out var hit,
                    reach))
            {
                return false;
            }

            chunkView = hit.collider.GetComponentInParent<ChunkView>();
            if (chunkView == null)
            {
                return false;
            }

            // 🔥 POSITION MONDE SELON L'ACTION
            var worldPoint =
                action == BlockAction.Break
                    ? hit.point - hit.normal * 0.01f // DANS le bloc
                    : hit.point + hit.normal * 0.01f; // DEVANT la face

            var local = worldPoint - chunkView.transform.position;

            localPos = new Int3(
                Mathf.FloorToInt(local.x),
                Mathf.FloorToInt(local.y),
                Mathf.FloorToInt(local.z)
            );

            return true;
        }
    }
}