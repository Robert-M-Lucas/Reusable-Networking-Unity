using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PositionUpdateClientPacket
{
    public const int UID = 200;
    public int RID;
    public double x;
    public double y;
    public double z;

    public PositionUpdateClientPacket(Packet packet)
    {
        RID = packet.RID;
        x = BitConverter.ToDouble(packet.contents[0]);
        y = BitConverter.ToDouble(packet.contents[1]);
        z = BitConverter.ToDouble(packet.contents[2]);
    }

    public static byte[] Build(int _RID, double _x, double _y, double _z)
    {
        List<byte[]> contents = new List<byte[]>();
        contents.Add(BitConverter.GetBytes(_x));
        contents.Add(BitConverter.GetBytes(_y));
        contents.Add(BitConverter.GetBytes(_z));
        return PacketBuilder.Build(UID, contents, _RID);
    }
}
