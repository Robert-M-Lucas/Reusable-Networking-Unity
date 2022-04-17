from operator import le
import os

from requests import patch

path = "C:\\Users\\rober\\Documents\\UnityProjects\\Reusable Networking\\Assets\\Packets"

uid = int(input("Enter UID: "))
packet_name = input("Enter packet name: ")

attributes = [["UID", "int"], ["RID", "int"]]

try:
    while True:
        attribute_name = input("Enter attribute name: ")
        attribute_type = input("Enter attribute type: ")
        new_attrib_name = ""
        for i in attribute_name.split(" "):
            if len(i) > 0:
                new_attrib_name += i[0].upper()
                if len(i) > 1:
                    new_attrib_name += i[1:]
        attribute_name = new_attrib_name
        attributes.append([attribute_name, attribute_type])
        print()
            
except KeyboardInterrupt:
    pass

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


with open(f"{path}\\{filename}", "x") as f:
    f.write(data)