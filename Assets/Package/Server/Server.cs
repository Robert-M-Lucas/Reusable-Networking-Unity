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


public class Server : ServerClientParent
{
    public string server_password = "";

    public static bool IsRunning = false;
    // public bool stopping = false;

    private Socket Handler;
    private Socket listener;

    public string ServerInfo = "";
    public Action ServerInfoUpdateAction = () => { };

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

    public Action OnPlayerJoinAction = () => {};
    public Action OnPlayerLeaveAction = () => {};

    public Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();

    public int UsersConnected {get {return Players.Count;}}

    ConcurrentQueue<Tuple<int, byte[]>> ContentQueue = new ConcurrentQueue<Tuple<int, byte[]>>();
    ConcurrentQueue<Tuple<int, byte[]>> SendQueue = new ConcurrentQueue<Tuple<int, byte[]>>();

    ConcurrentDictionary<int, Tuple<int, byte[]>> RequireResponse = new ConcurrentDictionary<int, Tuple<int, byte[]>>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();
    int RID = 1;
    CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    // Dictionary<string, Func<string, Server, int, bool>> PacketActions = new Dictionary<string, Func<string, Server, int, bool>>();
    
    public ServerClientHierachy hierachy;

    public bool AcceptingClients = false;

    int playerIDCounter = 0;

    // Singleton setup
    private Server() 
    {
        hierachy = new ServerClientHierachy(this);
        DefaultHierachy.Add(new DefaultServerPacketHandler());
    }
    private static Server instance = null;
    public static bool has_instance {
    get {
        return !(instance is null);
    }}
    public static Server getInstance(bool instantiate = false)
    {
        if (instance is null && instantiate){
            instance = new Server();
        }
        return instance;
    }

    public void Start(){
        ServerLogger.ServerLog("Starting server");
        AcceptClientThread = new Thread(AcceptClients);
        AcceptClientThread.Start();
        RecieveThread = new Thread(RecieveLoop);
        RecieveThread.Start();
        SendThread = new Thread(SendLoop);
        SendThread.Start();
        IsRunning = true;
    }

    public ServerPlayer GetPlayer(int playerID){
        if (Players.ContainsKey(playerID)){
            return Players[playerID];
        }
        return null;
    }

    ServerPlayer AddPlayer(Socket handler){
        Players.Add(playerIDCounter, new ServerPlayer(handler, playerIDCounter));
        playerIDCounter++;
        new Thread(() => OnPlayerJoinAction()).Start();;
        return Players[playerIDCounter-1];
    }

    void RemovePlayer(int playerID){
        Players.Remove(playerID);

        foreach (int otherPlayerID in Players.Keys){
            SendMessage(playerID, ServerClientDisconnectPacket.Build(0, playerID), false);
        }

        new Thread(() => OnPlayerLeaveAction()).Start();;
    }

    public void UpdateAllPlayersAboutPlayer(ServerPlayer playerAbout){
        foreach (int playerID in Players.Keys){
            if (playerID == playerAbout.ID){ continue; }
            SendMessage(playerID, ServerOtherClientInfoPacket.Build(0, playerAbout.ID, playerAbout.Name), false);
        }
    }
    public void UpdatePlayerAboutAllPlayers(ServerPlayer playerUpdated){
        foreach (int playerID in Players.Keys){
            if (playerID == playerUpdated.ID){ continue; }
            ServerPlayer player = Players[playerID];
            SendMessage(playerUpdated.ID, ServerOtherClientInfoPacket.Build(0, player.ID, player.Name), false);
        }
    }

    // Accept client
    void AcceptClients()
    {
        ServerLogger.ServerLog("Server Client Accept Thread Start");

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
                    int bytesRec = Handler.Receive(rec_bytes);

                    total_rec += bytesRec;

                    // string _out2 = "";
                    // for (int i = 0; i < rec_bytes.Length; i++){
                    //     _out2 += rec_bytes[i].ToString() + ":";
                    // }
                    // Debug.Log(_out2);

                    Tuple<byte[], int> cleared = ArrayExtentions.ClearEmpty(rec_bytes);
                    rec_bytes = cleared.Item1;
                    total_rec -= cleared.Item2;

                    ArrayExtentions.Merge(rec_bytes, ArrayExtentions.Slice(partial_bytes, 0, bytesRec), total_rec);
                }

                int packet_len = PacketBuilder.GetPacketLength(ArrayExtentions.Slice(rec_bytes, 0, 4));

                while (total_rec < packet_len){
                    byte[] partial_bytes = new byte[1024];
                    int bytesRec = Handler.Receive(partial_bytes);

                    total_rec += bytesRec;
                    ArrayExtentions.Merge(rec_bytes, ArrayExtentions.Slice(partial_bytes, 0, bytesRec), total_rec);
                }

                ClientConnectRequestPacket initPacket = new ClientConnectRequestPacket(PacketBuilder.Decode(ArrayExtentions.Slice(rec_bytes, 0, packet_len)));

                if (!AcceptingClients){
                    Handler.Send(ServerKickPacket.Build(0, "Server not accepting clients at this time"));
                    ServerLogger.AC("SERVER: Client kicked - not accepting clients");
                    continue;
                }

                if (server_password != "" && initPacket.Password != server_password){
                    Handler.Send(ServerKickPacket.Build(0, "Wrong Password: '" + initPacket.Password + "'"));
                    ServerLogger.AC("SERVER: Client kicked - wrong password");
                    continue;
                }

                // Version mismatch
                if (initPacket.Version != NetworkSettings.VERSION){
                    Handler.Send(ServerKickPacket.Build(0, "Wrong Version:\nServer: " + NetworkSettings.VERSION.ToString() + "   Client (You): " + initPacket.Version));
                    ServerLogger.AC("SERVER: Client kicked - Wrong Version - Server: " + NetworkSettings.VERSION.ToString() + " Client: " + initPacket.Version);
                    continue;
                } 

                // TODO: Add player join logic
                ServerPlayer player = AddPlayer(Handler);
                player.Name = initPacket.Name;

                foreach (Action<ServerPlayer> action in hierachy.OnPlayerJoinActions){
                    action(player);
                }
                
                SendMessage(player.ID, ServerConnectAcceptPacket.Build(0, player.ID), false);

                UpdateAllPlayersAboutPlayer(player);
                UpdatePlayerAboutAllPlayers(player);

                ServerLogger.AC("Player " + player.GetUniqueString() + " connected. Beginning recieve");

                Handler.BeginReceive(player.buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), player);
            }
        }
        catch (ThreadAbortException){}
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            ServerLogger.AC("[ERROR] " + e.ToString());
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
            while (!stopping){
                if (!SendQueue.IsEmpty){
                    Tuple<int, byte[]> to_send;
                    if (SendQueue.TryDequeue(out to_send)){
                        ServerLogger.S("To " +Players[to_send.Item1].GetUniqueString() + "; Sent packet");
                        try {
                            Players[to_send.Item1].Handler.Send(to_send.Item2);
                        }
                        catch (SocketException se){
                            ServerLogger.S("Client: " + Players[to_send.Item1].GetUniqueString() + " disconnected due to socket exception: " + se);
                            Players.Remove(to_send.Item1);
                        }
                    }
                }
                else if (!RequiredResponseQueue.IsEmpty){
                    int rid;
                    if (RequiredResponseQueue.TryDequeue(out rid)){
                        if (RequireResponse.ContainsKey(rid)){
                            Tuple<int, byte[]> to_send = RequireResponse[rid];
                            ServerLogger.S("To " + Players[to_send.Item1].GetUniqueString() + "; Sent RID packet");
                            Players[to_send.Item1].Handler.Send(to_send.Item2);
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
            ServerLogger.S("[ERROR] " + e.ToString());
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
            ArrayExtentions.Merge(CurrentPlayer.long_buffer, CurrentPlayer.buffer, CurrentPlayer.long_buffer_size);
            CurrentPlayer.long_buffer_size += bytesRead;

            ReprocessBuffer:

            if (CurrentPlayer.current_packet_length == -1 && CurrentPlayer.long_buffer_size >= PacketBuilder.PacketLenLen)
            {
                CurrentPlayer.current_packet_length = PacketBuilder.GetPacketLength(CurrentPlayer.long_buffer);
            }

            if (CurrentPlayer.current_packet_length != -1 && CurrentPlayer.long_buffer_size >= CurrentPlayer.current_packet_length)
            {
                ServerLogger.R("Recieved Packet from " + CurrentPlayer.GetUniqueString());
                ContentQueue.Enqueue(new Tuple<int, byte[]>(CurrentPlayer.ID, ArrayExtentions.Slice(CurrentPlayer.long_buffer, 0, CurrentPlayer.current_packet_length)));
                byte[] new_buffer = new byte[1024];
                ArrayExtentions.Merge(new_buffer, ArrayExtentions.Slice(CurrentPlayer.long_buffer, CurrentPlayer.current_packet_length, 1024), 0);
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
            while (!stopping)
            {
                if (ContentQueue.IsEmpty){//Thread.Sleep(2); 
                continue;} // Nothing recieved

                Tuple<int, byte[]> content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                ServerLogger.R("Handling Packet");
                bool handled = hierachy.HandlePacket(content.Item2, content.Item1);
                if (!handled){
                    ServerLogger.R("[ERROR] Failed to handle packed with UID " + PacketBuilder.Decode(content.Item2).UID + ". Probable hierachy error");
                }

            }
        }
        catch (ThreadAbortException) {}
        catch (Exception e){
            Debug.LogError(e);
            ServerLogger.R("[ERROR] " + e.ToString());
        }
    }
    
    ~Server(){Stop();}
    public void Stop(){
        ServerLogger.ServerLog("Server Shutting Down");
        stopping = true;
        try{Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        try{listener.Shutdown(SocketShutdown.Both); }catch (Exception e){Debug.Log(e);}
        Thread.Sleep(5);
        try{AcceptClientThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{RecieveThread.Abort();}catch (Exception e){Debug.Log(e);}
        try{SendThread.Abort();}catch (Exception e){Debug.Log(e);}
        foreach (ServerPlayer player in Players.Values){
            try{player.Handler.Send(ServerKickPacket.Build(0, "Server shutting down"));}catch (Exception e){Debug.Log(e);}
            try{player.Handler.Shutdown(SocketShutdown.Both);}catch (Exception e){Debug.Log(e);}
        }
        instance = null;
        ServerLogger.ServerLog("Server Shut Down Complete");
    }
}
