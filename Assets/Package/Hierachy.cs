using System;
using System.Collections;
using System.Collections.Generic;

public class ServerHierachy: ServerClientHierachy{

    public List<Action<ServerPlayer>> OnPlayerJoinActions = new List<Action<ServerPlayer>>();

    public ServerHierachy(Server server): base(server){

    }
}

public class ServerClientHierachy
{
    ServerClientParent ServerClient;

    public List<PacketHandlerParent> Hierachy = new List<PacketHandlerParent>();

    public ServerClientHierachy(ServerClientParent serverClient){
        ServerClient = serverClient;
    }

    public bool HandlePacket(byte[] data){
        Packet packet = PacketBuilder.Decode(data);



        foreach (PacketHandlerParent packetHandler in ServerClient.DefaultHierachy){
            if (packetHandler.UIDtoAction.ContainsKey(packet.UID)){
                packetHandler.UIDtoAction[packet.UID](packet);
                return true;
            }
        }

        foreach (PacketHandlerParent packetHandler in Hierachy){
            if (packetHandler.UIDtoAction.ContainsKey(packet.UID)){
                packetHandler.UIDtoAction[packet.UID](packet);
                return true;
            }
        }

        return false;
    }
}