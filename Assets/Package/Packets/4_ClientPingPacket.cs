using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ClientPingPacket {
    public const int UID = 4;
    public int RID;
    public ClientPingPacket(Packet packet){
        RID = packet.RID;
    }

    public static byte[] Build(int _RID) {
            List<byte[]> contents = new List<byte[]>();
            return PacketBuilder.Build(UID, contents, _RID);
    }
}