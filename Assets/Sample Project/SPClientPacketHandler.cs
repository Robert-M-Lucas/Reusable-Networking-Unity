using System;
using System.Collections;
using System.Collections.Generic;

public class SPClientPacketHandler : PacketHandlerParent
{
    public Dictionary<int, Tuple<double, double, double>> PlayerPos =
        new Dictionary<int, Tuple<double, double, double>>();

    public override Dictionary<int, Action<Packet>> UIDtoAction { get; } =
        new Dictionary<int, Action<Packet>> { };

    public SPClientPacketHandler()
    {
        UIDtoAction[201] = (Packet p) => PositionUpdate(p);
    }

    public void PositionUpdate(Packet packet)
    {
        PositionUpdateServerPacket positionPacket = new PositionUpdateServerPacket(packet); // This packet is defined in Packets.txt

        PlayerPos[positionPacket.PlayerID] = new Tuple<double, double, double>(
            positionPacket.x,
            positionPacket.y,
            positionPacket.z
        );
    }
}
