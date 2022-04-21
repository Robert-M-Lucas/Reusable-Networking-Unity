# Reusable Networking Unity
This is made to function as reusable code for easy implementation of networking
in future games I make

## How to use
- Server and Client objects are both Singletons. Use ```Server.getInstance()``` and ```Client.getInstance()``` respectively to get a reference. To instantiate these objects use ```.getInstance(true)```
- Any reference to ```Server.*``` or ```Client.*``` refers to ```(Server/Client).getInstance().*```

### Required Setup
- Put ```NetworkSettings.MainThreadStart();``` at the start of an Awake function in a MonoBehaviour object that's in the first scene in the build order

### Other Setup
- Change constants in ```NetworkSettings.cs```