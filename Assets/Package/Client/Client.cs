using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class ClientNotConnectedException: Exception {}
public class WaitingForPingResponseException: Exception {};

public class Client : ServerClientParent
{
    # region Server
    Socket Handler;

    byte[] server_long_buffer = new byte[1024];
    byte[] server_buffer = new byte[1024];
    int server_long_buffer_size = 0;
    int server_current_packet_length = -1;

    # endregion

    public string ClientInfo = "";
    public Action ClientInfoUpdateAction = () => { };
    bool connected = false;
    public Thread ConnectThread;
    public string ConnectThreadInfo = "";
    public Action ConnectUpdateAction = () => { };
    public Thread RecieveThread;
    public string RecieveThreadInfo = "";
    public Action RecieveUpdateAction = () => { };
    public Thread SendThread;
    public string SendThreadInfo = "";
    public Action SendUpdateAction = () => { };

    public Action DisconnectAction = () => { };

    public ServerClientHierachy hierachy;

    ConcurrentQueue<byte[]> ContentQueue = new ConcurrentQueue<byte[]>();
    ConcurrentQueue<byte[]> SendQueue = new ConcurrentQueue<byte[]>();

    ConcurrentDictionary<int, byte[]> RequireResponse = new ConcurrentDictionary<int, byte[]>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();
    int RID = 1;
    CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    public Action<int> PingResponseAction;
    public Stopwatch PingTimer = new Stopwatch();

    private Client() 
    {
        hierachy = new ServerClientHierachy(this);
        DefaultHierachy.Add(new DefaultClientPacketHandler());
    }

    private static Client instance = null;
    public static bool has_instance {
    get {
        return !(instance is null);
    }}
    public static Client getInstance(bool instantiate = false)
    {
        if (instance is null && instantiate){
            instance = new Client();
        }

        return instance;
    }

    public void GetPing(Action<int> callback){
        if (!connected) { throw new ClientNotConnectedException(); }
        if (PingTimer.IsRunning) { throw new WaitingForPingResponseException(); }

        PingResponseAction = callback;
        SendMessage(ClientPingPacket.Build(0), false);
        PingTimer.Start();
    }

    private void Start(){
        ClientLogger.ClientLog("Starting client");
    }

    public void Connect(string IP, string Password=""){
        ClientLogger.C("Starting connection thread");
        ConnectThread = new Thread(() => {ConnectThreaded(IP, Password);});
        ConnectThread.Start();
    }

    void ConnectThreaded(string IP, string Password=""){
        ClientLogger.C("Starting connection");

        IPAddress HostIpA = IPAddress.Parse(IP);
        IPEndPoint RemoteEP = new IPEndPoint(HostIpA, 8108);

        Handler = new Socket(HostIpA.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        Handler.Connect(RemoteEP);

        ClientLogger.C("Socket connected to " +
            Handler.RemoteEndPoint.ToString());
  
        Handler.Send(ClientConnectRequestPacket.Build(0, "Me", NetworkSettings.VERSION, Password));

        Handler.BeginReceive(server_buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null);

        ClientLogger.C("Connection successful, starting other threads");
        connected = true;
        RecieveThread = new Thread(RecieveLoop);
        RecieveThread.Start();
        SendThread = new Thread(SendLoop);
        SendThread.Start();
    }

    public void SendMessage(byte[] message, bool require_response){
        SendQueue.Enqueue(message);

        if (require_response){
            RequireResponse[RID] = message;
            RequiredResponseQueue.Enqueue(RID);
            RID++;
        }
    }

    void SendLoop()
    {
        try{
            while (!stopping){
                if (!SendQueue.IsEmpty){
                    byte[] to_send;
                    if (SendQueue.TryDequeue(out to_send)){
                        ClientLogger.S("To server; Sent packet");
                        Handler.Send(to_send);
                    }
                }
                else if (!RequiredResponseQueue.IsEmpty){
                    int rid;
                    if (RequiredResponseQueue.TryDequeue(out rid)){
                        if (RequireResponse.ContainsKey(rid)){
                            byte[] to_send = RequireResponse[rid];
                            ClientLogger.S("To server; Sent RID packet");
                            Handler.Send(to_send);
                            RequiredResponseQueue.Enqueue(rid);
                        }
                    }
                }

                Thread.Sleep(2);
            }
        }
        catch (ThreadAbortException) {}
        catch (Exception e){
            Debug.LogError(e);
            ClientLogger.S("[ERROR] " + e.ToString());
        }
    }

    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        int bytesRead = Handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            ArrayExtentions.Merge(server_long_buffer, server_buffer, server_long_buffer_size);
            server_long_buffer_size += bytesRead;

            ReprocessBuffer:

            if (server_current_packet_length == -1 && server_long_buffer_size >= PacketBuilder.PacketLenLen)
            {
                server_current_packet_length = PacketBuilder.GetPacketLength(server_long_buffer);
            }

            if (server_current_packet_length != -1 && server_long_buffer_size >= server_current_packet_length)
            {
                ClientLogger.R("Recieved Packet from server");
                ContentQueue.Enqueue(ArrayExtentions.Slice(server_long_buffer, 0, server_current_packet_length));
                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(server_long_buffer, server_current_packet_length, 1024), 0);
                server_long_buffer = new_buffer;
                server_long_buffer_size -= server_current_packet_length;
                server_current_packet_length = -1;
                if (server_long_buffer_size > 0){
                    goto ReprocessBuffer;
                }
            }

            // ContentQueue.Enqueue(new Tuple<int, byte[]>(server_ID, subcontent));
            // server_Reset(); // Reset buffers
// 
            Handler.BeginReceive(server_buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), null); // Listen again
            // }
            // else
            // {
            //     // Not all data received. Get more.  
            //     handler.BeginReceive(server_buffer, 0, 1024, 0,
            //     new AsyncCallback(ReadCallback), CurrentPlayer);
            // }
        }
        else
        {
            Handler.BeginReceive(server_buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), null);
        }
    }

    void RecieveLoop()
    {
        try{
            while (!stopping)
            {
                if (ContentQueue.IsEmpty){//Thread.Sleep(2);
                continue;} // Nothing recieved

                byte[] content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                ClientLogger.R("Handling Packet");
                bool handled = hierachy.HandlePacket(content);
                if (!handled){
                    ClientLogger.R("[ERROR] Failed to handle packed with UID " + PacketBuilder.Decode(content).UID + ". Probable hierachy error");
                }

            }
        }
        catch (ThreadAbortException) {}
        catch (Exception e){
            Debug.LogError(e);
            ClientLogger.R("[ERROR] " + e.ToString());
        }
    }
    
    ~Client(){Stop();}
    public void Disconnect(){
        try{
            DisconnectAction();
        }
        catch (Exception e){
            Debug.LogError(e);
            ClientLogger.ClientLog("Error while disconnecting: " + e);
        }
        Stop();
    }
    public void Stop(){
        ClientLogger.ClientLog("Client Shutting Down");
        stopping = true;
        Thread.Sleep(5);
        try{RecieveThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{SendThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        instance = null;
        ClientLogger.ClientLog("Client Shut Down Complete");
    }
}