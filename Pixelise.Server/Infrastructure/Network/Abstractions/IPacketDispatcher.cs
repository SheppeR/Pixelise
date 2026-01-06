using LiteNetLib;
using Pixelise.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixelise.Server.Infrastructure.Network.Abstractions
{
    public interface IPacketDispatcher
    {
        Task DispatchAsync(NetPeer peer, NetPacket packet);
    }

}
