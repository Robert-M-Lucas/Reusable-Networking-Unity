using System;
using System.Collections;
using System.Collections.Generic;

public abstract class PacketHandlerParent
{
    public abstract Dictionary<int, Action<Packet>> UIDtoAction { get; }
}
