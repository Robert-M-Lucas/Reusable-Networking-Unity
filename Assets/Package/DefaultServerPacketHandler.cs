using System;
using System.Collections;
using System.Collections.Generic;

public class DefaultServerPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction {get;} = new Dictionary<int, Action<Packet>> {
        {0, (Packet p) => HandleOne(p)}
    };

    public static void HandleOne(Packet packet) {

    }
}