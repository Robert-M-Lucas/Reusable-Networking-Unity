using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class test : MonoBehaviour
{
    void Awake(){
        NetworkSettings.MainThreadStart();
        Server.getInstance(true).AcceptingClients = true;
        Server.getInstance().AcceptClientUpdateAction = () => {Debug.Log(Server.getInstance().AcceptClientThreadInfo);};
        Server.getInstance().RecieveUpdateAction = () => {Debug.Log(Server.getInstance().RecieveThreadInfo);};
        Server.getInstance().SendUpdateAction = () => {Debug.Log(Server.getInstance().SendThreadInfo);};
    }
    // Start is called before the first frame update
    void Start()
    {
        IPAddress HostIpA = IPAddress.Parse("127.0.0.1");
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, 8108);
 
        Socket Handler = new Socket(HostIpA.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        Handler.Connect(RemoteEP);

        byte[] data = ClientConnectRequestPacket.Build(12, "Me", "0.1");

        Debug.Log(data.Length);
        Debug.Log(PacketBuilder.GetPacketLength(data));
        
        string _out = "";
        for (int i = 0; i < data.Length; i++){
            _out += data[i].ToString() + ":";
        }
        Debug.Log(_out);
 
        Handler.Send(data);
 
        Handler.Shutdown(SocketShutdown.Both);
    }

    void OnApplicationQuit(){
        Debug.Log("Stopping");
        Server.getInstance().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
