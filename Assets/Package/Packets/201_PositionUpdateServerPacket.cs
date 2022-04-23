using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PositionUpdateServerPacket {
    public const int UID = 201;
    public int RID;
    public int PlayerID;
    public double x;
    public double y;
    public double z;
    public PositionUpdateServerPacket(Packet packet){
        RID = packet.RID;
        PlayerID = BitConverter.ToInt32(packet.contents[0]);
        x = BitConverter.ToDouble(packet.contents[1]);
        y = BitConverter.ToDouble(packet.contents[2]);
        z = BitConverter.ToDouble(packet.contents[3]);
    }

    public static byte[] Build(int _RID, int _PlayerID, double _x, double _y, double _z) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_PlayerID));
            contents.Add(BitConverter.GetBytes(_x));
            contents.Add(BitConverter.GetBytes(_y));
            contents.Add(BitConverter.GetBytes(_z));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}