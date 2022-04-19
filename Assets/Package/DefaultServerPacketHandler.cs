using System;
using System.Collections;
using System.Collections.Generic;

public class DefaultServerPacketHandler : PacketHandlerParent
{
    public override Dictionary<int, Action<Dictionary<string, string>>> UIDtoAction {get;} = new Dictionary<int, Action<Dictionary<string, string>>> {
        {0, (Dictionary<string, string> x) => HandleOne(x)}
    };

    public static void HandleOne(Dictionary<string, string> contents) {

    }
}