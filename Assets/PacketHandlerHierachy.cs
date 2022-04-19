using System;
using System.Collections;
using System.Collections.Generic;

public class PacketHandlerHierachy
{
    ServerClientParent ServerClient;

    List<PacketHandlerParent> Hierachy = new List<PacketHandlerParent>();

    public bool IsEmpty {
        get {
            return Hierachy.Count == 0;
        }
    }

    public PacketHandlerHierachy(ServerClientParent serverClient){
        ServerClient = serverClient;
    }

    public bool HandlePacket(string data){
        Packet packet = PacketBuilder.Decode(data);

        foreach (PacketHandlerParent packetHandler in ServerClient.DefaultHierachy){
            if (packetHandler.UIDtoAction.ContainsKey(packet.UID)){
                packetHandler.UIDtoAction[packet.UID](packet.contents);
                return true;
            }
        }

        foreach (PacketHandlerParent packetHandler in Hierachy){
            if (packetHandler.UIDtoAction.ContainsKey(packet.UID)){
                packetHandler.UIDtoAction[packet.UID](packet.contents);
                return true;
            }
        }

        return false;
    }

    public void RemoveAtIndex(int index){
        Hierachy.RemoveAt(index);
    }

    public void AddAtIndex(int index, PacketHandlerParent packetHandler){
        Hierachy.Insert(index, packetHandler);
    }

    public void ClearHierachy(){
        Hierachy.Clear();
    }
}