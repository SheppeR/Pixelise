using MessagePack;
using Pixelise.Core.Math;
using Pixelise.Core.Network;
using Pixelise.Core.World;
using UnityEngine;
using Utils;

namespace World
{
    public class ClientSpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject localPlayer;

        private PlayerSpawnPacket pendingSpawn;
        private bool hasPendingSpawn;

        private void Awake()
        {
            if (localPlayer == null)
                localPlayer = GameObject.FindWithTag("Player");

            localPlayer.SetActive(false);
        }

        public void OnSpawnPacket(NetPacket packet)
        {
            pendingSpawn = MessagePackSerializer.Deserialize<PlayerSpawnPacket>(packet.Payload);
            hasPendingSpawn = true;
        }

        public void TryApplySpawn()
        {
            if (!hasPendingSpawn) return;

            localPlayer.transform.position =
                VectorExtensions.ToUnity(pendingSpawn.Position) + Vector3.up * 0.1f;

            localPlayer.transform.rotation =
                Quaternion.Euler(0f, pendingSpawn.Yaw, 0f);

            localPlayer.SetActive(true);
            hasPendingSpawn = false;

            Debug.Log("Player spawned (chunk ready)");
        }

        public Int3 GetSpawnChunk()
        {
            if (!hasPendingSpawn) return default;

            return ChunkUtils.WorldToChunk(pendingSpawn.Position);
        }

        public bool HasPendingSpawn => hasPendingSpawn;

    }
}