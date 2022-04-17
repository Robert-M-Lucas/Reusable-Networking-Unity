using System;
using System.Collections;
using System.Collections.Generic;

public class ClientConnectRequestPacket {
    int UID;
    int RID;
    string Name;
    public ClientConnectRequestPacket(Packet packet){
        UID = packet.UID;
        RID = packet.RID;
        Name = packet.contents["Name"];
    }

    public static string Build(int _UID, int _RID, string _Name) {
            Dictionary<string, string> contents = new Dictionary<string, string>();
            contents["Name"] = _Name;
            return PacketBuilder.Build(_UID, contents, _RID);
    }
}