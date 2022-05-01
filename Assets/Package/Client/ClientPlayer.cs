using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;

public class ClientPlayer : ClientPlayerExtraData
{
    public int ID;
    public string Name;

    public ClientPlayer(int _playerID, string _playerName = "")
    {
        ID = _playerID;
        Name = _playerName;
    }
}
