using System;
using System.Collections;
using System.Collections.Generic;

public class SPServerPacketHandler : PacketHandlerParent
{
    // Dictionary<int, Tuple<double, double, double>> PlayerPos = new Dictionary<int, Tuple<double, double, double>>();

    public override Dictionary<int, Action<Packet>> UIDtoAction {get;} = new Dictionary<int, Action<Packet>> {
        {200, (Packet p) => PositionUpdate(p)} // {UID, Function}
    };

    public static void PositionUpdate(Packet packet) {
        PositionUpdateClientPacket positionPacket = new PositionUpdateClientPacket(packet); // This packet is defined in Packets.txt
        int send_to = 0;
        if (packet.From == 0) { send_to = 1; }
        if (Server.getInstance().GetPlayer(send_to) != null){
            Server.getInstance().SendMessage(send_to, PositionUpdateServerPacket.Build(0, packet.From, positionPacket.x, positionPacket.y, positionPacket.z), false);
        }
    }
}