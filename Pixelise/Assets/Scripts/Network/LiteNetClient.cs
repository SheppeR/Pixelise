using LiteNetLib;
using MessagePack;
using Pixelise.Core.Network;
using UnityEngine;

namespace Network
{
    public class LiteNetClient : MonoBehaviour, INetEventListener
    {
        [Header("Connection")]
        public string host = "127.0.0.1";
        public int port = 9000;
        public string connectionKey = "PixeliseKey";

        private NetManager client;
        private NetPeer serverPeer;

        [Header("Receivers")]
        [SerializeField] private ClientNetworkReceiver receiver;

        private void Awake()
        {
            if (receiver == null)
                receiver = FindFirstObjectByType<ClientNetworkReceiver>();
        }

        private void Start()
        {
            client = new NetManager(this)
            {
                AutoRecycle = true
            };

            client.Start();
            client.Connect(host, port, connectionKey);
        }

        private void Update()
        {
            client.PollEvents();
        }

        public void OnPeerConnected(NetPeer peer)
        {
            serverPeer = peer;
            Debug.Log("Connected to server");
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo info)
        {
            Debug.Log("Disconnected from server");
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod method)
        {
            var packet = MessagePackSerializer.Deserialize<NetPacket>(reader.GetRemainingBytes());
            receiver.OnPacket(packet);
        }

        public void Send(NetPacket packet)
        {
            if (serverPeer == null) return;

            var bytes = MessagePackSerializer.Serialize(packet);
            serverPeer.Send(bytes, DeliveryMethod.ReliableOrdered);
        }

        public void OnConnectionRequest(ConnectionRequest request) { }
        public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError) { }
        public void OnNetworkLatencyUpdate(NetPeer peer, int latency) { }
        public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }
    }
}
