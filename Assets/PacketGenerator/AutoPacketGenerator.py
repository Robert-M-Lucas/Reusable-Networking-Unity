from operator import le
import os
import sys

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
using System.Text;

public class {packet_name}Packet {{
"""
    data += f"    public const int UID = {uid};\n"
    for i in attributes[1:]:
        data += "    public " + i[1] + " " + i[0] + ";" + "\n"

    data += f"""    public {packet_name}Packet(Packet packet){{
        RID = packet.RID;
"""

    for j, i in enumerate(attributes[2:]):
        data += "        " + i[0] + " = "
        if i[1] == "string":
            data += "ASCIIEncoding.ASCII.GetString"
        elif i[1] == "int":
            data += "BitConverter.ToInt32"
        elif i[1] == "double":
            data += "BitConverter.ToDouble"
        else:
            print(f"Unsupported type: {i[1]}")
            sys.exit()
        data += f"(packet.contents[{j}]);\n"


    data += """    }

"""

    data += """    public static byte[] Build("""

    for i in attributes[1:]:
        data += i[1] + " _" + i[0] + ", "

    data = data[:-2]

    data += """) {
            List<byte[]> contents = new List<byte[]>();
"""

    for i in attributes[2:]:
        if i[1] == "string":
            data += f"            contents.Add(ASCIIEncoding.ASCII.GetBytes(_{i[0]}));\n"
        else:
            data += f"            contents.Add(BitConverter.GetBytes(_{i[0]}));\n"

    data += """            return PacketBuilder.Build(UID, contents, _RID);
    }
}"""


    with open(f"{path}\\{filename}", "w+") as f:
        f.write(data)