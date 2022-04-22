using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.IO;
using System.Diagnostics;


public static class ServerLogger
{
    public static string ServerWrap(string message){
        return "[SERVER] " + message;
    }
    public static void ServerLog(string message){
        Logger.Log(ServerWrap(message));
    }
    // Accept Client Thread
    public static void AC (string message){
        message = "[AC] " + message;
        Server.getInstance().AcceptClientThreadInfo = ServerWrap(message);
        Server.getInstance().AcceptClientUpdateAction();
        Logger.Log(message);
    }

    // Recieve Thread
    public static void R (string message){
        message = "[R] " + message;
        Server.getInstance().RecieveThreadInfo = ServerWrap(message);
        Server.getInstance().RecieveUpdateAction();
        Logger.Log(message);
    }

    // Send Thread
    public static void S (string message){
        message = "[S] " + message;
        Server.getInstance().SendThreadInfo = ServerWrap(message);
        Server.getInstance().SendUpdateAction();
        Logger.Log(message);
    }
}

public static class Logger{
    public static void Log(string LogMessage){
        if (NetworkSettings.LOG_PATH != null){
            File.AppendAllText(NetworkSettings.LOG_PATH, DateTime.Now.ToString("[hh:mm:ss] ") + LogMessage + "\n");
        }
    }
}