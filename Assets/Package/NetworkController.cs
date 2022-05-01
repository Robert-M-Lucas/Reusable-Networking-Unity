using UnityEngine;
using System;

public static class NetworkController
{
    public static void Shutdown()
    {
        Logger.Log("[Network Controller] Shutdown started");
        if (Server.has_instance)
        {
            Logger.Log("[Network Controller] Shutting down server");
            Server.getInstance().Stop();
        }
        else
        {
            Logger.Log("[Network Controller] No server instance to shut down");
        }
        if (Client.has_instance)
        {
            Logger.Log("[Network Controller] Shutting down client");
            Client.getInstance().Stop();
        }
        else
        {
            Logger.Log("[Network Controller] No client instance to shut down");
        }
        Logger.Log("[Network Controller] Shutting down logger");
        Logger.Stop = true;
        try
        {
            Logger.FileWriteThread.Abort();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Logger.Log("[Network Controller] Shutdown complete");
        Logger.WriteAllRemaining();
    }
}
