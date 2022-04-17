using System;
using System.Collections;
using System.Collections.Generic;

public abstract class PacketHandlerInterface{
    public abstract Dictionary<int, Action<Dictionary<string, string>>> UIDtoAction {get;}
}