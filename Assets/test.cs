using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    void Awake(){
        Server.getInstance().AcceptClientUpdateAction = () => {Debug.Log(Server.getInstance().AcceptClientThreadInfo);};
        Server.getInstance().RecieveUpdateAction = () => {Debug.Log(Server.getInstance().RecieveThreadInfo);};
        Server.getInstance().SendUpdateAction = () => {Debug.Log(Server.getInstance().SendThreadInfo);};
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void OnApplicationQuit(){
        Debug.Log("Stopping");
        Server.getInstance().Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
