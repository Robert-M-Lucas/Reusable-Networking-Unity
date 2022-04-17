using System;
using System.Collections;
using System.Collections.Generic;

public class ServerKickPacket {
    int UID;
    int RID;
    string Reason;
    public ServerKickPacket(Packet packet){
        UID = packet.UID;
        RID = packet.RID;
        Reason = packet.contents["Reason"];
    }

    public static string Build(int _UID, int _RID, string _Reason) {
            Dictionary<string, string> contents = new Dictionary<string, string>();
            contents["Reason"] = _Reason;
            return PacketBuilder.Build(_UID, contents, _RID);
    }
}