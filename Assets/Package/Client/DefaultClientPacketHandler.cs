using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class DefaultClientPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Packet>> UIDtoAction { get; } =
        new Dictionary<int, Action<Packet>>
        {
            { 1, (Packet p) => ServerAccept(p) },
            { 2, (Packet p) => ServerKick(p) },
            { 3, (Packet p) => PlayerInformationUpdate(p) },
            { 6, (Packet p) => PlayerDisconnect(p) },
            { 5, (Packet p) => PingResponse(p) },
        };

    public static void ServerAccept(Packet packet)
    {
        ServerConnectAcceptPacket acceptPacket = new ServerConnectAcceptPacket(packet);
        ClientLogger.ClientLog("Server accepted client connection");
    }

    public static void ServerKick(Packet packet)
    {
        ServerKickPacket kickPacket = new ServerKickPacket(packet);
        ClientLogger.ClientLog("Server kicked client, reason: " + kickPacket.Reason);
        Client.getInstance().Disconnect();
    }

    public static void PlayerInformationUpdate(Packet packet)
    {
        ServerOtherClientInfoPacket infoPacket = new ServerOtherClientInfoPacket(packet);
        Client.getInstance().AddOrUpdatePlayer(infoPacket.ClientUID, infoPacket.ClientName);
        new Thread(() => Client.getInstance().OnPlayerUpdateAction()).Start();
    }

    public static void PlayerDisconnect(Packet packet)
    {
        ServerInformOfClientDisconnectPacket disconnectPacket =
            new ServerInformOfClientDisconnectPacket(packet);
        Client.getInstance().RemovePlayer(disconnectPacket.ClientUID);
    }

    public static void PingResponse(Packet p)
    {
        int ping = Client.getInstance().PingTimer.Elapsed.Milliseconds;
        Client.getInstance().PingResponseAction(ping);
        Client.getInstance().PingTimer.Reset();
    }
}
