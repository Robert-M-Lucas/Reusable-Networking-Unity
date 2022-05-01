using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class ServerPlayer : ServerPlayerExtraData
{
    public Socket Handler;

    public int ID;

    public string Name;

    public byte[] buffer = new byte[1024];
    public byte[] long_buffer = new byte[1024];
    public int current_packet_length = -1;
    public int long_buffer_size = 0;
    public StringBuilder sb = new StringBuilder();

    public string GetUniqueString()
    {
        return "[" + ID + "] " + "'" + Name + "'";
    }

    public void Reset()
    {
        buffer = new byte[1024];
        sb = new StringBuilder();
    }

    public ServerPlayer(Socket handler, int playerID)
    {
        Handler = handler;
        ID = playerID;
    }
}
