using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class AnotherTestPacket {
    public const int UID = 101;
    public int RID;
    public int attributeone;
    public string attributetwo;
    public double attributethree;
    public AnotherTestPacket(Packet packet){
        RID = packet.RID;
        attributeone = BitConverter.ToInt32(packet.contents[0]);
        attributetwo = ASCIIEncoding.ASCII.GetString(packet.contents[1]);
        attributethree = BitConverter.ToDouble(packet.contents[2]);
    }

    public static byte[] Build(int _RID, int _attributeone, string _attributetwo, double _attributethree) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_attributeone));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_attributetwo));
            contents.Add(BitConverter.GetBytes(_attributethree));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}