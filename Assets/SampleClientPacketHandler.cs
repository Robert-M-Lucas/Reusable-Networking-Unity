using System;
using System.Collections;
using System.Collections.Generic;

public class SampleClientPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction { get; } =
        new Dictionary<int, Action<Packet>>
        {
            { 101, (Packet p) => HandlePacketWithUIDOneHundredAndOne(p) } // {UID, Function}
        };

    public static void HandlePacketWithUIDOneHundredAndOne(Packet packet)
    {
        SampleTestTwoPacket sampleTestPacket = new SampleTestTwoPacket(packet); // This packet is defined in Packets.txt
        ClientLogger.ClientLog(sampleTestPacket.ArgFour);
    }
}
