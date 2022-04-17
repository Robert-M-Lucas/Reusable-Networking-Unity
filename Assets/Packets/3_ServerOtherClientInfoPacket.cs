using System;
using System.Collections;
using System.Collections.Generic;

public class ServerOtherClientInfoPacket {
    int UID;
    int RID;
    int ClientUID;
    string ClientName;
    public ServerOtherClientInfoPacket(Packet packet){
        UID = packet.UID;
        RID = packet.RID;
        ClientUID = int.Parse(packet.contents["ClientUID"]);
        ClientName = packet.contents["ClientName"];
    }

    public static string Build(int _UID, int _RID, int _ClientUID, string _ClientName) {
            Dictionary<string, string> contents = new Dictionary<string, string>();
            contents["ClientUID"] = _ClientUID.ToString();
            contents["ClientName"] = _ClientName;
            return PacketBuilder.Build(_UID, contents, _RID);
    }
}