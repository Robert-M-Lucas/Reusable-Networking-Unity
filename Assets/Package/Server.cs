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

public class Server
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

    ConcurrentQueue<Tuple<int, string>> ContentQueue = new ConcurrentQueue<Tuple<int, string>>();
    ConcurrentQueue<Tuple<int, string>> SendQueue = new ConcurrentQueue<Tuple<int, string>>();

    public ConcurrentDictionary<int, Tuple<int, string>> RequireResponse = new ConcurrentDictionary<int, Tuple<int, string>>();
    ConcurrentQueue<int> RequiredResponseQueue = new ConcurrentQueue<int>();
    public int RID = 1;
    public CircularArray<int> RecievedRIDs = new CircularArray<int>(50);

    Dictionary<string, Func<string, Server, int, bool>> PacketActions = new Dictionary<string, Func<string, Server, int, bool>>();
    
    List<PacketHandlerInterface> DefaultPacketHandlerHierachy = new List<PacketHandlerInterface> {new DefaultServerPacketHandler()};
    List<PacketHandlerInterface> PacketHandlerHierachy = new List<PacketHandlerInterface>();

    // Singleton setup
    private Server() {}
    private static Server instance = new Server();
    public static Server getInstance()
    {
        return instance;
    }

    public void Start(){
       //AcceptClientThread = new Thread(AcceptClients);
       //AcceptClientThread.Start();
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

            while (UsersConnected < 2)
            {
                ServerLogger.AC("SERVER: Waiting for a connection...");
                Socket Handler = listener.Accept();

                // Incoming data from the client.    
                string data = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = Handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                ServerLogger.AC("SERVER: Text received : {0}" + data);

                Packet initPacket = PacketBuilder.Decode(PacketBuilder.RemoveEOF(data));

                // Version mismatch
                if (initPacket.contents["version"] != NetworkSettings.VERSION){
                    Handler.Send(PacketTools.Encode(KickPacketClient.Build("Wrong Version:\nServer: " + networkManager.version + "   Client (You): " + initPacket.version, 0)));
                    continue;
                }
                
                int p = 0;
                if (Players[0] != null){
                    p++;
                }
                Players[p] = new ServerClient(Handler);
                Players[p].name = initPacket.name;
                Players[p].UID = p;
                UsersConnected++;

                if (Players[p].name != networkManager.Username) { networkManager.OpponentName = Players[p].name; }
                
                SendMessage(p, InitalPacketClient.Build(RID, p), true);

                currentMessage = "Player " + p.ToString() + " (" + initPacket.name + ")" + " connected";
                Debug.Log(currentMessage);

                Handler.BeginReceive(Players[p].buffer, 0, 1024, 0, new AsyncCallback(ReadCallback), Players[p]);
            }
            Debug.Log("SERVER: Both clients connected");
            // SendMessage(0, ScenesSwitchPacket.Build(1, RID), true);
            // SendMessage(1, ScenesSwitchPacket.Build(1, RID), true);

            // Send information about clients to eachother
            SendMessage(0, OtherPlayerInfoPacketClient.Build(Players[1].name, RID), true);
            SendMessage(1, OtherPlayerInfoPacketClient.Build(Players[0].name, RID), true);
            
        }
        catch (ThreadAbortException){}
        catch (Exception e)
        {
            Debug.Log("ERROR");
            Debug.Log(e.ToString());
            networkManager.QuitLobby(e.ToString());
        }
    }

    /*
    public void SendMessage(int ID, string message, bool require_response){
        SendQueue.Enqueue(new Tuple<int, string>(ID, message));

        if (require_response){
            RequireResponse[RID] = new Tuple<int, string>(ID, message);
            RequiredResponseQueue.Enqueue(RID);
            RID++;
        }
    }

    void SendLoop()
    {
        try{
            while (true){
                if (!SendQueue.IsEmpty){
                    Tuple<int, string> to_send;
                    if (SendQueue.TryDequeue(out to_send)){
                        Debug.Log("SERVER: Sent " + to_send.Item2);
                        Players[to_send.Item1].Handler.Send(PacketTools.Encode(to_send.Item2));
                    }
                }
                else if (!RequiredResponseQueue.IsEmpty){
                    int rid;
                    if (RequiredResponseQueue.TryDequeue(out rid)){
                        if (RequireResponse.ContainsKey(rid)){
                            Tuple<int, string> to_send = RequireResponse[rid];
                            Debug.Log("SERVER: Sent " + to_send.Item2);
                            Players[to_send.Item1].Handler.Send(PacketTools.Encode(to_send.Item2));
                            RequiredResponseQueue.Enqueue(rid);
                        }
                    }
                }

                Thread.Sleep(5);
            }
        }
        catch (Exception e){
            networkManager.FatalException(e.ToString());
        }
    }

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

                    ContentQueue.Enqueue(new Tuple<int, string>(CurrentPlayer.UID, subcontent));
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
                // Update current game every x ms
                if (CurrentGame != null){
                    if (CurrentGame.MsBetweenUpdates != -1){
                        if (!CurrentGameStopwatch.IsRunning){
                            CurrentGameStopwatch.Start();
                        }

                        else if (CurrentGameStopwatch.ElapsedMilliseconds > CurrentGame.MsBetweenUpdates)
                        {
                            CurrentGameStopwatch.Restart();
                            CurrentGame.Update();
                        }
                    }
                }

                if (ContentQueue.IsEmpty){Thread.Sleep(2); continue;} // Nothing recieved

                Tuple<int, string> content;
                if (!ContentQueue.TryDequeue(out content)){ continue; }

                int rid = PacketTools.RequireResponse(content.Item2);

                // Confirm recieved
                // if (rid != 0) { 
                //     SendMessage(content.Item1, ResponseConfirmPacket.Build(rid), false); Debug.Log("CLIENT: CONFIRMED MESSAGE"); 
                //     if (RecievedRIDs.Contains(rid) != -1){
                //         continue; // Already recieved this message
                //     }
                //     else{
                //         RecievedRIDs.Add(rid);
                //     }
                // }

                string UID = "1"; //PacketTools.PacketToUID(content.Item2);

                // Try to get current game to handle packet
                // if (CurrentGame != null){
                //     if (CurrentGame.HandlePacket(UID, content.Item2, content.Item1)){
                //         continue;
                //     }
                // }

                // Handle packet
                if (PacketActions.ContainsKey(UID)){
                    PacketActions[UID](content.Item2, this, content.Item1);
                }
                else{
                    Debug.LogError("SERVER: Unhandled packet: " + content.Item2);
                }

            }
        }
        catch (Exception e){
            //networkManager.FatalException(e.ToString());
        }
    }
    */
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
