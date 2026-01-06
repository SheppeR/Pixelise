using Pixelise.Core.Network;
using UnityEngine;
using World;

namespace Network
{
    public class ClientNetworkReceiver : MonoBehaviour
    {
        [SerializeField] private ClientWorldSync worldSync;
        [SerializeField] private ClientSpawnManager spawnManager;
        [SerializeField] private ClientBlockSync blockSync;

        private void Awake()
        {
            if (worldSync == null) worldSync = FindFirstObjectByType<ClientWorldSync>();
            if (spawnManager == null) spawnManager = FindFirstObjectByType<ClientSpawnManager>();
            if (blockSync == null) blockSync = FindFirstObjectByType<ClientBlockSync>();
        }

        public void OnPacket(NetPacket packet)
        {
            switch (packet.Type)
            {
                case PacketType.ChunkData:
                    worldSync.OnChunkPacket(packet);
                    break;

                case PacketType.PlayerSpawn:
                    spawnManager.OnSpawnPacket(packet);
                    break;

                case PacketType.BlockCommand:
                    blockSync.OnBlockPacket(packet);
                    break;

                case PacketType.PlayerMove:
                    // futur : remote players
                    break;
            }
        }
    }
}