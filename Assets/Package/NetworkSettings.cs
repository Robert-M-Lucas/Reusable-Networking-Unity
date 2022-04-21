using UnityEngine;
using System;

public static class NetworkSettings {
    public const string VERSION = "0.1";

    public static string LOG_PATH = null; // Set to null for no log file

    public static void MainThreadStart(){
        LOG_PATH = Application.persistentDataPath + "/NetworkLog_" + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss") + ".log";
        Debug.Log("Logging to: " + LOG_PATH);
    }
}