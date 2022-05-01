using UnityEngine;
using System;

public static class NetworkSettings
{
    public static string VERSION = "N/A";

    public static string LOG_PATH = null; // Set to null for no log file

    public const int PORT = 8108;

    public static void MainThreadStart()
    {
        LOG_PATH =
            Application.persistentDataPath
            + "/NetworkLog_"
            + DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss")
            + ".log";
        VERSION = Application.version;

        Debug.Log("Logging to: " + LOG_PATH);
    }
}
