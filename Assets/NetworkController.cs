using UnityEngine;
using System;

public static class NetworkController {
    public static void Shutdown(){
        Logger.Log("[Network Controller] Shutdown started");
        Logger.Log("[Network Controller] Shutting down server");
        Server.getInstance().Stop();
        Logger.Log("[Network Controller] Shutting down logger");
        Logger.Stop = true;
        try{ Logger.FileWriteThread.Abort(); } catch (Exception e) { Debug.LogError(e); }
        Logger.Log("[Network Controller] Shutdown complete");
        Logger.WriteAllRemaining();
    }
}