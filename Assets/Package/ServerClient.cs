using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

public abstract class ServerClient
{
    public Socket Handler;

    public int UID;

    public byte[] buffer = new byte[1024];
    public StringBuilder sb = new StringBuilder();

    public ServerClient(Socket handler){
        Handler = handler;
    }

    public abstract void HandleRID(int RID);

    public void Reset(){
        buffer = new byte[1024];
        sb = new StringBuilder();
    }
}