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

public static class ServerLogger
{
    // Accept Client Thread
    public static void AC (string message){
        Server.AcceptClientThreadInfo = message;
        Server.AcceptClientUpdateAction();
    }

    // Recieve Thread
    public static void R (string message){
        Server.RecieveThreadInfo = message;
        Server.RecieveUpdateAction();
    }

    // Send Thread
    public static void S (string message){
        Server.SendThreadInfo = message;
        Server.SendUpdateAction();
    }
}

public class Server : ServerClientParent
{
    public static bool IsRunning = false;

    private Socket Handler;
    private Socket listener;

    # region Threads
    private Thread AcceptClientThread;
    public static string AcceptClientThreadInfo = "";
    public static Action AcceptClientUpdateAction = () => { };
    public static Thread RecieveThread;
    public static string RecieveThreadInfo = "";
    public static Action RecieveUpdateAction = () => { };
    public static Thread SendThread;
    public static string SendThreadInfo = "";
    public static Action SendUpdateAction = () => { };
    # endregion

    public List<ServerClient> Players = new List<ServerClient>();

    public int UsersConnected {get {return Players.Count;}}

    ConcurrentQueue<Tuple<int, byte[]>> ContentQueue = new ConcurrentQueue<Tuple<int, byte[]>>();
    ConcurrentQueue<Tuple<int, byte[]>> SendQueue = new ConcurrentQueue<Tuple<int, byte[]>>();

    ConcurrentDictionary<int, Tuple<int, byte[]>> RequireResponse = new ConcurrentDictionary<int, Tuple<int, byte[]>>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();
    int RID = 1;
    CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    // Dictionary<string, Func<string, Server, int, bool>> PacketActions = new Dictionary<string, Func<string, Server, int, bool>>();
    
    PacketHandlerHierachy packetHandlerHierachy;

    bool AcceptingClients = false;

    // Singleton setup
    private Server() 
    {
        packetHandlerHierachy = new PacketHandlerHierachy(this);
    }
    private static Server instance = null;
    public static Server getInstance()
    {
        if (instance is null){
            instance = new Server();
        }

        return instance;
    }

    public void Start(){
       // AcceptClientThread = new Thread(AcceptClients);
       // AcceptClientThread.Start();
       // RecieveThread = new Thread(RecieveLoop);
       // RecieveThread.Start();
       // SendThread = new Thread(SendLoop);
       // SendThread.Start();
       IsRunning = true;
    }

    // Accept client
    void AcceptClients()
    {
        Debug.Log("SERVER: Server Client Accept Thread Start");

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8108);

        try
        {
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);

            listener.Listen(100);

            while (true)
            {
                ServerLogger.AC("SERVER: Waiting for a connection...");
                Socket Handler = listener.Accept();

                // Incoming data from the client.    
                byte[] rec_bytes = new byte[1024];
                int total_rec = 0;

                while (total_rec < 4) {
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(partial_bytes);

                    total_rec += bytesRec;
                    ArrayExtentions<byte>.Merge(rec_bytes, ArrayExtentions<byte>.Slice(partial_bytes, 0, bytesRec), total_rec);
                }

                int packet_len = PacketBuilder.GetPacketLength(rec_bytes);

                while (total_rec < packet_len + PacketBuilder.PacketLenLen){
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(partial_bytes);

                    total_rec += bytesRec;
                    ArrayExtentions<byte>.Merge(rec_bytes, ArrayExtentions<byte>.Slice(partial_bytes, 0, bytesRec), total_rec);
                }

                ClientConnectRequestPacket initPacket = new ClientConnectRequestPacket(PacketBuilder.Decode(rec_bytes));

                // Version mismatch
                if (initPacket.Version != NetworkSettings.VERSION){
                    Handler.Send(ServerKickPacket.Build(0, "Wrong Version:\nServer: " + NetworkSettings.VERSION.ToString() + "   Client (You): " + initPacket.Version.ToString()));
                    continue;
                }
                
                if (!AcceptingClients){
                    Handler.Send(ServerKickPacket.Build(0, "Server not accepting clients at this time"));
                    continue;
                }

                // TODO: Add player join logic
                
                SendMessage(PlayerID, ServerConnectAcceptPacket.Build(RID, PlayerID), true);

                ServerLogger.AC("Player " + PlayerID.ToString() + " (" + initPacket.Name + ")" + " connected");

                Handler.BeginReceive(Players[PlayerID].buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), Players[p]);
            }
            // SendMessage(0, ScenesSwitchPacket.Build(1, RID), true);
            // SendMessage(1, ScenesSwitchPacket.Build(1, RID), true);

            // Send information about clients to eachother
            SendMessage(0, OtherPlayerInfoPacketClient.Build(Players[1].name, RID), true);
            SendMessage(1, OtherPlayerInfoPacketClient.Build(Players[0].name, RID), true);
            
        }
        catch (ThreadAbortException){}
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    
    public void SendMessage(int ID, byte[] message, bool require_response){
        SendQueue.Enqueue(new Tuple<int, byte[]>(ID, message));

        if (require_response){
            RequireResponse[RID] = new Tuple<int, byte[]>(ID, message);
            RequiredResponseQueue.Enqueue(RID);
            RID++;
        }
    }

    void SendLoop()
    {
        try{
            while (true){
                if (!SendQueue.IsEmpty){
                    Tuple<int, byte[]> to_send;
                    if (SendQueue.TryDequeue(out to_send)){
                        Debug.Log("SERVER: Sent " + to_send.Item2);
                        Players[to_send.Item1].Handler.Send(to_send.Item2);
                    }
                }
                else if (!RequiredResponseQueue.IsEmpty){
                    int rid;
                    if (RequiredResponseQueue.TryDequeue(out rid)){
                        if (RequireResponse.ContainsKey(rid)){
                            Tuple<int, byte[]> to_send = RequireResponse[rid];
                            Debug.Log("SERVER: Sent " + to_send.Item2);
                            Players[to_send.Item1].Handler.Send(to_send.Item2);
                            RequiredResponseQueue.Enqueue(rid);
                        }
                    }
                }

                Thread.Sleep(5);
            }
        }
        catch (Exception e){
            Debug.LogError(e);
        }
    }

    // TODO: Rewrite this with new methods
    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;
 
        ServerClient CurrentPlayer = (ServerClient)ar.AsyncState;
        Socket handler = CurrentPlayer.Handler;

        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            CurrentPlayer.sb.Append(Encoding.UTF8.GetString(
                CurrentPlayer.buffer, 0, bytesRead));

            // Check for EOF
            content = CurrentPlayer.sb.ToString();
            if (content.IndexOf("<EOF>") > -1)
            {

                foreach (string subcontent in content.Split(new[] { "<EOF>" }, StringSplitOptions.None))
                {
                    if (subcontent.Length == 0) { continue; }

                    Debug.Log("SERVER: Recieved " + subcontent);

                    ContentQueue.Enqueue(new Tuple<int, byte[]>(CurrentPlayer.UID, subcontent));
                }
                CurrentPlayer.Reset(); // Reset buffers

                handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer); // Listen again
            }
            else
            {
                // Not all data received. Get more.  
                handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer);
            }
        }
        else
        {
            handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
                new AsyncCallback(ReadCallback), CurrentPlayer);
        }
    }

    void RecieveLoop()
    {
        try{
            while (true)
            {
                if (ContentQueue.IsEmpty){Thread.Sleep(2); continue;} // Nothing recieved

                Tuple<int, byte[]> content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                packetHandlerHierachy.HandlePacket(content.Item2);

            }
        }
        catch (Exception e){
            Debug.LogError(e);
        }
    }
    
    ~Server(){Stop();}
    public void Stop(){
        try{Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        try{listener.Shutdown(SocketShutdown.Both); }catch (Exception e){Debug.Log(e);}
        try{AcceptClientThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{RecieveThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{SendThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{Players[0].Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        try{Players[1].Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        instance = new Server();
    }
}
