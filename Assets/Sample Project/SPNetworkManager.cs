using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class SPNetworkManager : MonoBehaviour
{
    public Vector3 pos = new Vector3(0, 0, 0);
    public Transform myPlayer;
    public Transform foreignPlayer;

    public float LastSent;

    SPClientPacketHandler handler;

    void Awake(){
        DontDestroyOnLoad(this);

        // Required
        NetworkSettings.MainThreadStart();

        # if UNITY_EDITOR
        // True in getInstance() denotes that you are expecting an instance to be created
        Server.getInstance(true).AcceptingClients = true;
        // Omit this line for no password
        // Server.getInstance(true).server_password = "Ham";

        // Adding actions that get called when a log is updated
        Server.getInstance().ServerInfoUpdateAction = () => {Debug.Log(Server.getInstance().ServerInfo);};
        Server.getInstance().AcceptClientUpdateAction = () => {Debug.Log(Server.getInstance().AcceptClientThreadInfo);};
        // Server.getInstance().RecieveUpdateAction = () => {Debug.Log(Server.getInstance().RecieveThreadInfo);};
        // Server.getInstance().SendUpdateAction = () => {Debug.Log(Server.getInstance().SendThreadInfo);};

        // Add packet handler to packet handler hierachy
        Server.getInstance().hierachy.Hierachy.Add(new SPServerPacketHandler());

        // Start the server
        Server.getInstance().Start();
        # endif


        // Adding actions that get called when a log is updated
        Client.getInstance(true).ConnectUpdateAction = () => {Debug.Log(Client.getInstance().ConnectThreadInfo);};
        Client.getInstance().ClientInfoUpdateAction = () => {Debug.Log(Client.getInstance().ClientInfo);};
        // Client.getInstance().RecieveUpdateAction = () => {Debug.Log(Client.getInstance().RecieveThreadInfo);};
        // Client.getInstance().SendUpdateAction = () => {Debug.Log(Client.getInstance().SendThreadInfo);};

        // Add packet handler to packet handler hierachy
        handler = new SPClientPacketHandler();
        Client.getInstance().hierachy.Hierachy.Add(handler);

        // Connect and start the client
        Client.getInstance().Connect("127.0.0.1");
    }

    
    void Start()
    {  
        handler.PlayerPos[0] = new Tuple<double, double, double>(0, 0, 0);
        handler.PlayerPos[1] = new Tuple<double, double, double>(0, 0, 0);

        LastSent = Time.time;
    }

    void OnApplicationQuit(){
        // Required
        NetworkController.Shutdown();
    }

    // Update is called once per frame
    void Update()
    {
        # if UNITY_EDITOR
        int p = 1;
        # else
        int p = 0;
        # endif

        foreignPlayer.transform.position = new Vector3((float) handler.PlayerPos[p].Item1, (float) handler.PlayerPos[p].Item2, (float) handler.PlayerPos[p].Item3);

        if (Input.GetKey(KeyCode.W)){
            myPlayer.position += Vector3.forward * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A)){
            myPlayer.position += Vector3.left * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S)){
            myPlayer.position += Vector3.back * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D)){
            myPlayer.position += Vector3.right * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space)){
            myPlayer.position += Vector3.up * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftShift)){
            myPlayer.position += Vector3.down * Time.deltaTime;
        }

        if (Time.time - LastSent > ((float) 1/60)){
            LastSent = Time.time;
            Client.getInstance().SendMessage(PositionUpdateClientPacket.Build(0, myPlayer.position.x, myPlayer.position.y, myPlayer.position.z), false);
        }
    }
}
