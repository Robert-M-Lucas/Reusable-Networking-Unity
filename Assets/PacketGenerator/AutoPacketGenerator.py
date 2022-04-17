from operator import le
import os

from requests import patch

with open("C:\\Users\\rober\\Documents\\UnityProjects\\Reusable Networking\\Assets\\PacketGenerator\\Packets.txt", "r") as f:
    all_packets_split = f.read().split("\n")

all_packets = [i.split("\\") for i in all_packets_split]

path = "C:\\Users\\rober\\Documents\\UnityProjects\\Reusable Networking\\Assets\\Packets"

for j in all_packets:
    uid = int(j[0])
    packet_name = j[1]

    attributes = [["UID", "int"], ["RID", "int"]]

    x = 2
    while x < len(j):
        attributes.append([j[x], j[x+1]])
        x += 2

    filename = f"{uid}_{packet_name}Packet.cs"

    data = f"""using System;
using System.Collections;
using System.Collections.Generic;

public class {packet_name}Packet {{
"""

    for i in attributes:
        data += "    " + i[1] + " " + i[0] + ";" + "\n"

    data += f"""    public {packet_name}Packet(Packet packet){{
        UID = packet.UID;
        RID = packet.RID;
"""

    for i in attributes[2:]:
        if i[1] == "string":
            data += "        " + i[0] + " = packet.contents[\"" + i[0] + "\"];\n"
        else:
            data += "        " + i[0] + " = " + i[1] + ".Parse(packet.contents[\"" + i[0] + "\"]);\n"


    data += """    }

"""

    data += """    public static string Build("""

    for i in attributes:
        data += i[1] + " _" + i[0] + ", "

    data = data[:-2]

    data += """) {
            Dictionary<string, string> contents = new Dictionary<string, string>();
"""

    for i in attributes[2:]:
        data += f"            contents[\"{i[0]}\"] = _" + i[0]
        if i[1] != "string":
            data += ".ToString()"
        data += ";\n"

    data += """            return PacketBuilder.Build(_UID, contents, _RID);
    }
}"""


    with open(f"{path}\\{filename}", "w+") as f:
        f.write(data)