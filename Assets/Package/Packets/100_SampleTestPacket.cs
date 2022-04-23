using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class SampleTestPacket {
    public const int UID = 100;
    public int RID;
    public int ArgOne;
    public double ArgTwo;
    public string ArgThree;
    public string ArgFour;
    public SampleTestPacket(Packet packet){
        RID = packet.RID;
        ArgOne = BitConverter.ToInt32(packet.contents[0]);
        ArgTwo = BitConverter.ToDouble(packet.contents[1]);
        ArgThree = ASCIIEncoding.ASCII.GetString(packet.contents[2]);
        ArgFour = ASCIIEncoding.ASCII.GetString(packet.contents[3]);
    }

    public static byte[] Build(int _RID, int _ArgOne, double _ArgTwo, string _ArgThree, string _ArgFour="defaultVal") {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_ArgOne));
            contents.Add(BitConverter.GetBytes(_ArgTwo));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_ArgThree));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_ArgFour));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}