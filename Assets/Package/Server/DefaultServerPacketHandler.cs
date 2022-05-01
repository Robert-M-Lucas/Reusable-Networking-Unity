using System;
using System.Collections;
using System.Collections.Generic;

public class DefaultServerPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction { get; } =
        new Dictionary<int, Action<Packet>> { { 4, (Packet p) => PingRespond(p) } };

    public static void PingRespond(Packet packet)
    {
        Server.getInstance().SendMessage(packet.From, ServerPingPacket.Build(0), false);
    }
}
