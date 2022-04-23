using System;
using System.Collections;
using System.Collections.Generic;

public class SampleServerPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction {get;} = new Dictionary<int, Action<Packet>> {
        {100, (Packet p) => HandlePacketWithUIDOneHundred(p)} // {UID, Function}
    };

    public static void HandlePacketWithUIDOneHundred(Packet packet) {
        SampleTestPacket sampleTestPacket = new SampleTestPacket(packet); // This packet is defined in Packets.txt
        ServerLogger.ServerLog(sampleTestPacket.ArgFour);
    }
}