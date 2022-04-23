using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ClientConnectRequestPacket {
    public const int UID = 0;
    public int RID;
    public string Name;
    public string Version;
    public string Password;
    public ClientConnectRequestPacket(Packet packet){
        RID = packet.RID;
        Name = ASCIIEncoding.ASCII.GetString(packet.contents[0]);
        Version = ASCIIEncoding.ASCII.GetString(packet.contents[1]);
        Password = ASCIIEncoding.ASCII.GetString(packet.contents[2]);
    }

    public static byte[] Build(int _RID, string _Name, string _Version, string _Password="") {
            List<byte[]> contents = new List<byte[]>();
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_Name));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_Version));
            contents.Add(ASCIIEncoding.ASCII.GetBytes(_Password));
            return PacketBuilder.Build(UID, contents, _RID);
    }
}