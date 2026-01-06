using LiteNetLib;
using Pixelise.Core.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pixelise.Server.Infrastructure.Network.Abstractions
{
    public interface IPacketHandler<in TPacket>
    {
        PacketType Type { get; }
        Task HandleAsync(NetPeer peer, TPacket packet);
    }

}
