using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class CoordinateTestPacket {
    public const int UID = 100;
    public int RID;
    public double X;
    public double Y;
    public double Z;
    public string Name;
    public CoordinateTestPacket(Packet packet){
        RID = packet.RID;
        X = BitConverter.ToDouble(packet.contents[0]);
        Y = BitConverter.ToDouble(packet.contents[1]);
        Z = BitConverter.ToDouble(packet.contents[2]);
        Name = ASCIIEncoding.ASCII.GetString(packet.contents[3]);
    }

    public static byte[] Build(int _RID, double _X, double _Y, double _Z, string _Name) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_X));
            contents.Add(BitConverter.GetBytes(_Y));
            contents.Add(BitConverter.GetBytes(_Z));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_Name));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}