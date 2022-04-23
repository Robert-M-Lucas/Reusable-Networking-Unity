using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class RandomCustomClientPacket {
    public const int UID = 100;
    public int RID;
    public int a;
    public string b;
    public RandomCustomClientPacket(Packet packet){
        RID = packet.RID;
        a = BitConverter.ToInt32(packet.contents[0]);
        b = ASCIIEncoding.ASCII.GetString(packet.contents[1]);
    }

    public static byte[] Build(int _RID, int _a, string _b) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_a));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_b));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}