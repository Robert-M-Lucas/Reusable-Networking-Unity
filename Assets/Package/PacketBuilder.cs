using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class PacketDictionary: Dictionary<string, string>{

}

public struct Packet{
    public int UID;
    public int RID;
    public Dictionary<string, string> contents;

    public Packet(int _UID, int _RID, Dictionary<string, string> _contents){
        UID = _UID;
        RID = _RID;
        contents = _contents;
    }
}

public class PacketMissingAttributeException : Exception
{
    public PacketMissingAttributeException()
    {
    }

    public PacketMissingAttributeException(string message)
        : base(message)
    {
    }

    public PacketMissingAttributeException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public static class PacketBuilder
{
    public static Encoding encoder = new UTF8Encoding();

    public static string RemoveEOF(string data){
        return data.Substring(0, data.Length - 5);
    }
    public static string Build(int UID, Dictionary<string, string> contents = null, int RID = 0)
    {
        return Build(UID.ToString(), contents, RID.ToString());
    }

    public static string Build(string UID, Dictionary<string, string> contents = null, string RID = "0")
    {   
        # if UNITY_EDITOR
        foreach (string key in contents.Keys){
            if (key.Contains("#") | contents[key].Contains("#"))
            {
                Debug.LogError("PACKET CONTAINS #");
            }
        }
        # endif

        string dict_string = "";
        foreach (string key in contents.Keys){
            dict_string += "#" + key + "#" + contents[key];
        }

        return UID + "#" + RID + dict_string;
    }

    public static byte[] ByteEncode(string input){
        return encoder.GetBytes(input);
    }

    public static Packet Decode(string data){
        string[] split = data.Split('#');
        Dictionary<string, string> content = new Dictionary<string, string>();

        for (int i = 2; i < split.Length; i+=2){
            content[split[i]] = split[i+1];
        }

        return new Packet(int.Parse(split[0]), int.Parse(split[1]), content);
    }
}