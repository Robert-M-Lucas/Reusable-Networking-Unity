using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ServerInformOfClientDisconnectPacket
{
    public const int UID = 6;
    public int RID;
    public int ClientUID;

    public ServerInformOfClientDisconnectPacket(Packet packet)
    {
        RID = packet.RID;
        ClientUID = BitConverter.ToInt32(packet.contents[0]);
    }

    public static byte[] Build(int _RID, int _ClientUID)
    {
        List<byte[]> contents = new List<byte[]>();
        contents.Add(BitConverter.GetBytes(_ClientUID));
        return PacketBuilder.Build(UID, contents, _RID);
    }
}
