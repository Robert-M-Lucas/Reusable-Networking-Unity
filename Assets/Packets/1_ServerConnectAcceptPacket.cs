using System;
using System.Collections;
using System.Collections.Generic;

public class ServerConnectAcceptPacket {
    int UID;
    int RID;
    int GivenUID;
    public ServerConnectAcceptPacket(Packet packet){
        UID = packet.UID;
        RID = packet.RID;
        GivenUID = int.Parse(packet.contents["GivenUID"]);
    }

    public static string Build(int _UID, int _RID, int _GivenUID) {
            Dictionary<string, string> contents = new Dictionary<string, string>();
            contents["GivenUID"] = _GivenUID.ToString();
            return PacketBuilder.Build(_UID, contents, _RID);
    }
}