using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(PacketBuilder.Build(
            123, 
        new Dictionary<string, string> {
            {"arg", "val"},
            {"arg2", "val2"}
            },
            12
        ));

        Server.AcceptClientUpdateAction = () => {Debug.Log(Server.AcceptClientThreadInfo);};
        Server.RecieveUpdateAction = () => {Debug.Log(Server.RecieveThreadInfo);};
        Server.SendUpdateAction = () => {Debug.Log(Server.SendThreadInfo);};
        ServerLogger.AC("Hi2");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
