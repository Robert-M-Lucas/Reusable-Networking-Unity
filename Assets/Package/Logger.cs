using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.IO;
using UnityEngine;


public static class ServerLogger
{
    public static string ServerWrap(string message){
        return "[SERVER] " + message;
    }
    public static void ServerLog(string message){
        try{
            Server.getInstance().ServerInfo = ServerWrap(message);
            Server.getInstance().ServerInfoUpdateAction();
        }
        catch (NullReferenceException){}
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

public static class ClientLogger
{
    public static string ClientWrap(string message){
        return "[CLIENT] " + message;
    }
    public static void ClientLog(string message){
        try{
        Client.getInstance().ClientInfo = ClientWrap(message);
        Client.getInstance().ClientInfoUpdateAction();
        }
        catch (NullReferenceException){}
        Logger.Log(ClientWrap(message));
    }
    // Connect Thread
    public static void C (string message){
        message = "[C] " + message;
        Client.getInstance().ConnectThreadInfo = ClientWrap(message);
        Client.getInstance().ConnectUpdateAction();
        Logger.Log(message);
    }
    // Recieve Thread
    public static void R (string message){
        message = "[R] " + message;
        Client.getInstance().RecieveThreadInfo = ClientWrap(message);
        Client.getInstance().RecieveUpdateAction();
        Logger.Log(message);
    }

    // Send Thread
    public static void S (string message){
        message = "[S] " + message;
        Client.getInstance().SendThreadInfo = ClientWrap(message);
        Client.getInstance().SendUpdateAction();
        Logger.Log(message);
    }
}

public static class Logger{
    public static Thread FileWriteThread = new Thread(ThreadedWrite);
    public static bool Stop = false;

    public static ConcurrentQueue<string> ToWrite = new ConcurrentQueue<string>();

    public static void Log(string LogMessage){
        if (NetworkSettings.LOG_PATH != null){
            ToWrite.Enqueue(DateTime.Now.ToString("[hh:mm:ss.ffff] ") + LogMessage + "\n");
            
            if (FileWriteThread.ThreadState != ThreadState.Running & FileWriteThread.ThreadState != ThreadState.WaitSleepJoin){
                try{
                    FileWriteThread.Start();
                }
                catch (ThreadStateException){}
            }
        }
    }

    public static void ThreadedWrite(){
        while (!Stop){
            while (ToWrite.Count > 0){
                string toWriteString;
                if (ToWrite.TryDequeue(out toWriteString)){
                    File.AppendAllText(NetworkSettings.LOG_PATH ,toWriteString);
                }
            }
            Thread.Sleep(50);
        }
    }

    public static void WriteAllRemaining(){
        while (ToWrite.Count > 0){
            string toWriteString;
            if (ToWrite.TryDequeue(out toWriteString)){
                File.AppendAllText(NetworkSettings.LOG_PATH ,toWriteString);
            }
        }
    }
}