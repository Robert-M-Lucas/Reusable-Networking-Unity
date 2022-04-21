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
        Server.getInstance().AcceptClientThreadInfo = message;
        Server.getInstance().AcceptClientUpdateAction();
    }

    // Recieve Thread
    public static void R (string message){
        Server.getInstance().RecieveThreadInfo = message;
        Server.getInstance().RecieveUpdateAction();
    }

    // Send Thread
    public static void S (string message){
        Server.getInstance().SendThreadInfo = message;
        Server.getInstance().SendUpdateAction();
    }
}

public class Server : ServerClientParent
{
    public static bool IsRunning = false;

    private Socket Handler;
    private Socket listener;

    # region Threads
    private Thread AcceptClientThread;
    public string AcceptClientThreadInfo = "";
    public Action AcceptClientUpdateAction = () => { };
    public Thread RecieveThread;
    public string RecieveThreadInfo = "";
    public Action RecieveUpdateAction = () => { };
    public Thread SendThread;
    public string SendThreadInfo = "";
    public Action SendUpdateAction = () => { };
    # endregion

    public List<ServerPlayer> Players = new List<ServerPlayer>();

    public int UsersConnected {get {return Players.Count;}}

    ConcurrentQueue<Tuple<int, byte[]>> ContentQueue = new ConcurrentQueue<Tuple<int, byte[]>>();
    ConcurrentQueue<Tuple<int, byte[]>> SendQueue = new ConcurrentQueue<Tuple<int, byte[]>>();

    ConcurrentDictionary<int, Tuple<int, byte[]>> RequireResponse = new ConcurrentDictionary<int, Tuple<int, byte[]>>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();
    int RID = 1;
    CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    // Dictionary<string, Func<string, Server, int, bool>> PacketActions = new Dictionary<string, Func<string, Server, int, bool>>();
    
    ServerHierachy serverHierachy;

    bool AcceptingClients = false;

    int playerIDCounter = 0;

    // Singleton setup
    private Server() 
    {
        serverHierachy = new ServerHierachy(this);
        Start();
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
        Debug.Log("Starting server");
        AcceptClientThread = new Thread(AcceptClients);
        AcceptClientThread.Start();
        RecieveThread = new Thread(RecieveLoop);
        RecieveThread.Start();
        SendThread = new Thread(SendLoop);
        SendThread.Start();
        IsRunning = true;
    }

    ServerPlayer GetPlayer(int playerID){
        foreach (ServerPlayer player in Players){
            if (player.ID == playerID){
                return player;
            }
        }
        return null;
    }

    ServerPlayer AddPlayer(Socket handler){
        Players.Add(new ServerPlayer(handler, playerIDCounter));
        playerIDCounter++;
        return Players[Players.Count-1];
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
                ServerLogger.AC("SERVER: Client connecting");

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

                while (total_rec < packet_len){
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(partial_bytes);

                    total_rec += bytesRec;
                    ArrayExtentions<byte>.Merge(rec_bytes, ArrayExtentions<byte>.Slice(partial_bytes, 0, bytesRec), total_rec);
                }

                ClientConnectRequestPacket initPacket = new ClientConnectRequestPacket(PacketBuilder.Decode(rec_bytes));

                // Version mismatch
                if (initPacket.Version != NetworkSettings.VERSION){
                    Handler.Send(ServerKickPacket.Build(0, "Wrong Version:\nServer: " + NetworkSettings.VERSION.ToString() + "   Client (You): " + initPacket.Version.ToString()));
                    ServerLogger.AC("SERVER: Client kicked - wrong version");
                    continue;
                }
                
                if (!AcceptingClients){
                    Handler.Send(ServerKickPacket.Build(0, "Server not accepting clients at this time"));
                    ServerLogger.AC("SERVER: Client kicked - not accepting clients");
                    continue;
                }

                // TODO: Add player join logic
                ServerPlayer player = AddPlayer(Handler);

                foreach (Action<ServerPlayer> action in serverHierachy.OnPlayerJoinActions){
                    action(player);
                }
                
                SendMessage(player.ID, ServerConnectAcceptPacket.Build(RID, player.ID), true);

                ServerLogger.AC("Player " + player.ID.ToString() + " (" + initPacket.Name + ")" + " connected");

                Handler.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);
            }
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

    private void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;
 
        ServerPlayer CurrentPlayer = (ServerPlayer) ar.AsyncState;
        Socket handler = CurrentPlayer.Handler;

        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0)
        {
            ArrayExtentions<byte>.Merge(CurrentPlayer.long_buffer, CurrentPlayer.buffer, CurrentPlayer.long_buffer_size);
            CurrentPlayer.long_buffer_size += bytesRead;

            ReprocessBuffer:

            if (CurrentPlayer.current_packet_length == -1 && CurrentPlayer.long_buffer_size >= PacketBuilder.PacketLenLen)
            {
                CurrentPlayer.current_packet_length = PacketBuilder.GetPacketLength(CurrentPlayer.long_buffer);
            }

            if (CurrentPlayer.current_packet_length != -1 && CurrentPlayer.long_buffer_size >= CurrentPlayer.current_packet_length)
            {
                ContentQueue.Enqueue(new Tuple<int, byte[]>(CurrentPlayer.ID, ArrayExtentions<byte>.Slice(CurrentPlayer.long_buffer, 0, CurrentPlayer.current_packet_length)));
                byte[] new_buffer = new byte[1024];
                ArrayExtentions<byte>.Merge(new_buffer, ArrayExtentions<byte>.Slice(CurrentPlayer.long_buffer, CurrentPlayer.current_packet_length, 1024), 0);
                CurrentPlayer.long_buffer = new_buffer;
                CurrentPlayer.long_buffer_size -= CurrentPlayer.current_packet_length;
                CurrentPlayer.current_packet_length = -1;
                if (CurrentPlayer.long_buffer_size > 0){
                    goto ReprocessBuffer;
                }
            }

            // ContentQueue.Enqueue(new Tuple<int, byte[]>(CurrentPlayer.ID, subcontent));
            // CurrentPlayer.Reset(); // Reset buffers
// 
            handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), CurrentPlayer); // Listen again
            // }
            // else
            // {
            //     // Not all data received. Get more.  
            //     handler.BeginReceive(CurrentPlayer.buffer, 0, 1024, 0,
            //     new AsyncCallback(ReadCallback), CurrentPlayer);
            // }
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

                serverHierachy.HandlePacket(content.Item2);

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
