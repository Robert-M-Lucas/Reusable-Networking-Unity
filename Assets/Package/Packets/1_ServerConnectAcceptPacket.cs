using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ServerConnectAcceptPacket {
    public const int UID = 1;
    public int RID;
    public int GivenUID;
    public ServerConnectAcceptPacket(Packet packet){
        RID = packet.RID;
        GivenUID = BitConverter.ToInt32(packet.contents[0]);
    }

    public static byte[] Build(int _RID, int _GivenUID) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_GivenUID));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}