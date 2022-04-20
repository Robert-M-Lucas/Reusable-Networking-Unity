using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ServerKickPacket {
    public const int UID = 2;
    public int RID;
    public string Reason;
    public ServerKickPacket(Packet packet){
        RID = packet.RID;
        Reason = ASCIIEncoding.ASCII.GetString(packet.contents[0]);
    }

    public static byte[] Build(int _RID, string _Reason) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_Reason));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}