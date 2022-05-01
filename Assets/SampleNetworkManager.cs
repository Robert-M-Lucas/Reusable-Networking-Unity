using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class SampleNetworkManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);

        // Required
        NetworkSettings.MainThreadStart();
    }

    void Start()
    {
        // True in getInstance() denotes that you are expecting an instance to be created
        Server.getInstance(true).AcceptingClients = true;
        // Omit this line for no password
        // Server.getInstance(true).server_password = "Ham";

        // Adding actions that get called when a log is updated
        Server.getInstance().ServerInfoUpdateAction = () =>
        {
            Debug.Log(Server.getInstance().ServerInfo);
        };
        Server.getInstance().AcceptClientUpdateAction = () =>
        {
            Debug.Log(Server.getInstance().AcceptClientThreadInfo);
        };
        // Server.getInstance().RecieveUpdateAction = () => {Debug.Log(Server.getInstance().RecieveThreadInfo);};
        // Server.getInstance().SendUpdateAction = () => {Debug.Log(Server.getInstance().SendThreadInfo);};

        // On player join
        Server.getInstance().OnPlayerJoinAction = () => {
            // Code here
        };
        // On player disconnect
        Server.getInstance().OnPlayerLeaveAction = () => { };

        // Add packet handler to packet handler hierachy
        Server.getInstance().hierachy.Hierachy.Add(new SampleServerPacketHandler());

        // Start the server
        Server.getInstance().Start();

        // Adding actions that get called when a log is updated
        Client.getInstance(true).ConnectUpdateAction = () =>
        {
            Debug.Log(Client.getInstance().ConnectThreadInfo);
        };
        Client.getInstance().ClientInfoUpdateAction = () =>
        {
            Debug.Log(Client.getInstance().ClientInfo);
        };
        // Client.getInstance().RecieveUpdateAction = () => {Debug.Log(Client.getInstance().RecieveThreadInfo);};
        // Client.getInstance().SendUpdateAction = () => {Debug.Log(Client.getInstance().SendThreadInfo);};

        // Client action on player join or update
        Client.getInstance().OnPlayerUpdateAction = () =>
        {
            foreach (ClientPlayer player in Client.getInstance().Players.Values)
            {
                Debug.Log("Client name:'" + player.Name + "', ID: " + player.ID);
            }
        };
        // Client on player disconnect
        Client.getInstance().OnPlayerDisconnectAction = () => {
            // Code here
        };

        // Add packet handler to packet handler hierachy
        Client.getInstance().hierachy.Hierachy.Add(new SampleClientPacketHandler());

        // Connect and start the client
        Client.getInstance().Connect("127.0.0.1", "Ham");

        // Send a message to server
        Client.getInstance().SendMessage(SampleTestPacket.Build(0, 2, 3.5, "asd", "val"), false);

        Thread.Sleep(100);

        // Send a message to a client
        Server.getInstance().SendMessage(0, SampleTestTwoPacket.Build(0, 1, 2.2, "3"), false);
    }

    void OnApplicationQuit()
    {
        // Required
        NetworkController.Shutdown();
    }

    // Update is called once per frame
    void Update() { }
}
