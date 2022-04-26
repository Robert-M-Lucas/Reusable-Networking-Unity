# Reusable Networking Unity
This is made to function as reusable code for easy implementation of networking
in future games I make

## How to use
See ```SampleNetworkManager.cs```, ```SampleServerPacketHandler.cs``` and ```SampleClientPacketHandler.cs``` for examples of everything you can do.

### Setup
#### Required Setup
- Put ```NetworkSettings.MainThreadStart();``` at the start of an ```Awake()```function in a MonoBehaviour object that's in the first scene in the build order. This is because some constants that need to be accessed from a separate thread can only be accessed through the main thread.
- Put ```NetworkController.Shutdown();``` in an ```OnApplicationQuit()``` function in a MonoBehaviour object thats in the active scene to ensure correct shutdown.

#### Packet Setup
- To create custom packets modify the ```PacketGenerator\Packets.txt```
- Each packet is its own line, each argument is separated by a ```\```
- ```UID\Name\Attribute1\Attribute1Type\Attribute2\Attribute2Type...```
- You can use an equals after an attribute name to give it a default value
- Current supported types are ```int```, ```double``` and ```string```. (NOT FLOAT)
- Any line in ```Packets.txt``` that doesn't contain a ```\``` is seen as a comment

#### Misc Setup
- Change constants in ```NetworkSettings.cs```
- Add extra data to ```ServerPlayer``` and ```ClientPlayer``` objects with ```Server\ServerPlayerExtraData.cs``` and ```Client\ClientPlayerExtraData.cs``` respectively

### Usage
- Server and Client objects are both Singletons. Use ```Server.getInstance()``` and ```Client.getInstance()``` respectively to get a reference. To instantiate these objects use ```.getInstance(true)```
- Any reference to ```Server.*``` or ```Client.*``` refers to ```(Server/Client).getInstance().*```

## Internal workings 

### Packets
[Length in bytes] Description

Header
- [4] Packet Length
- [4] UID (Unique packet type ID, used to determin how packet is handled and what data it contains)
- [4] RID (Response ID - a code the recipient echoes back to confirm message recieved)

Payload
- [4] Data length
- [Data length] data

These two are repeated for every item of data sent
