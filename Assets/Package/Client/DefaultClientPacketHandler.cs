using System;
using System.Collections;
using System.Collections.Generic;

public class DefaultClientPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction {get;} = new Dictionary<int, Action<Packet>> {
        {1, (Packet p) => ServerAccept(p)},
        {2, (Packet p) => ServerKick(p)},
        {5, (Packet p) => PingResponse(p)},
    };

    public static void ServerAccept(Packet packet) {
        ServerConnectAcceptPacket acceptPacket = new ServerConnectAcceptPacket(packet);
        ClientLogger.ClientLog("Server accepted client connection");
    }

    public static void ServerKick(Packet packet) {
        ServerKickPacket kickPacket = new ServerKickPacket(packet);
        ClientLogger.ClientLog("Server kicked client, reason: " + kickPacket.Reason);
        Client.getInstance().Disconnect();
    }

    public static void PingResponse(Packet p){
        int ping = Client.getInstance().PingTimer.Elapsed.Milliseconds;
        Client.getInstance().PingResponseAction(ping);
        Client.getInstance().PingTimer.Reset();
    }
}