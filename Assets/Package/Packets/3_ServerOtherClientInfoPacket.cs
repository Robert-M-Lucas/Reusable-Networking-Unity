using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ServerOtherClientInfoPacket {
    public const int UID = 3;
    public int RID;
    public int ClientUID;
    public string ClientName;
    public ServerOtherClientInfoPacket(Packet packet){
        RID = packet.RID;
        ClientUID = BitConverter.ToInt32(packet.contents[0]);
        ClientName = ASCIIEncoding.ASCII.GetString(packet.contents[1]);
    }

    public static byte[] Build(int _RID, int _ClientUID, string _ClientName) {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(BitConverter.GetBytes(_ClientUID));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_ClientName));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}