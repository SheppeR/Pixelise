using LiteNetLib;
using Pixelise.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixelise.Server.Infrastructure.Network.Abstractions
{
    public interface INetworkBroadcaster
    {
        void Broadcast(NetPacket packet);
        void Send(NetPeer peer, NetPacket packet);
    }

}
