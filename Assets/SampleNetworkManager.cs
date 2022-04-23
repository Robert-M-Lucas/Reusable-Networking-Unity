using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;

public class SampleNetworkManager : MonoBehaviour
{
    void Awake(){
        DontDestroyOnLoad(this);

        // Required
        NetworkSettings.MainThreadStart();
    }
    // Start is called before the first frame update
    void Start()
    {
        // True in getInstance() denotes that you are expecting an instance to be created
        Server.getInstance(true).AcceptingClients = true;
        // Omit this line for no password
        Server.getInstance(true).server_password = "Ham";

        // Adding actions that get called when a log is updated
        Server.getInstance().ServerInfoUpdateAction = () => {Debug.Log(Server.getInstance().ServerInfo);};
        Server.getInstance().AcceptClientUpdateAction = () => {Debug.Log(Server.getInstance().AcceptClientThreadInfo);};
        Server.getInstance().RecieveUpdateAction = () => {Debug.Log(Server.getInstance().RecieveThreadInfo);};
        Server.getInstance().SendUpdateAction = () => {Debug.Log(Server.getInstance().SendThreadInfo);};
        // Start the server
        Server.getInstance().Start();

        // Adding actions that get called when a log is updated
        Client.getInstance(true).ConnectUpdateAction = () => {Debug.Log(Client.getInstance().ConnectThreadInfo);};
        Client.getInstance().ClientInfoUpdateAction = () => {Debug.Log(Client.getInstance().ClientInfo);};
        Client.getInstance().RecieveUpdateAction = () => {Debug.Log(Client.getInstance().RecieveThreadInfo);};
        Client.getInstance().SendUpdateAction = () => {Debug.Log(Client.getInstance().SendThreadInfo);};
        // Connect and start the client
        Client.getInstance().Connect("127.0.0.1", "Ham");
    }

    void OnApplicationQuit(){
        // Required
        NetworkController.Shutdown();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
