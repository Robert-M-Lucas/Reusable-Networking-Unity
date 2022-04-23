using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class PingPacket {
    public const int UID = 4;
    public int RID;
    public int PingPacketID;
    public int SendTimeStamp;
    public PingPacket(Packet packet){
        RID = packet.RID;
        PingPacketID = BitConverter.ToInt32(packet.contents[0]);
        SendTimeStamp = BitConverter.ToInt32(packet.contents[1]);
    }

    public static byte[] Build(int _RID, int _PingPacketID, int _SendTimeStamp) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_PingPacketID));
            contents.Add(BitConverter.GetBytes(_SendTimeStamp));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}