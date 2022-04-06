# Real Time Battle

This is a sample of a communication match between players using [GS2-Realtime](https://app.gs2.io/docs/en/index.html#gs2-realtime).

## GS2-Deploy template

- [initialize_realtime_template.yaml - matchmaking/realtime_competition](../Templates/initialize_realtime_template.yaml)

## RealtimeSetting

![Inspector Window](Realtime.png)

| Setting Name | Description |
---|---
| realtimeNamespaceName | GS2-Realtime namespace name

| Event | Description |
---------|------
| OnRelayMessage(RelayBinaryMessage message) | Called when a message is received from a real-time game server. | OnRelayMessage(RelayBinaryMessage message)
| OnGetRoom(EzRoom room) | Called when IP address and port information of a real-time game server is obtained. | OnGetRoom(EzRoom room)
| OnJoinPlayer(Player player) | Called when a new player joins the real-time game server. | OnJoinPlayer(Player player)
| OnLeavePlayer(Player player) | Called when a player leaves the real-time game server. This callback is always called at the same time as either OnJoinPlayer / OnLeavePlayer. | OnJoinPlayer(Player)
| OnUpdateProfile(Player player) | Called when someone updates the player profile. | OnUpdateProfile(Player player)
| OnRelayError(Error error) | Called when an error occurs on the real-time game server. | OnRelayError(Error error)
| OnClose(CloseEventArgs error) | Called when disconnected from the real-time game server. | OnClose(CloseEventArgs error)
| OnGeneralError(ErrorEventArgs error) | Called when a connection-related error occurs. | OnGeneralError(ErrorEventArgs error)
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

### Retrieving Room Information

Get room information from GS2-Realtime.

```c#
AsyncResult<EzGetRoomResult> result = null;
yield return _realtimeModel.GetRoom(
    r => { result = r; },
    GameManager.Instance.Client,
    _realtimeSetting.realtimeNamespaceName,
    _realtimeSetting.onGetRoom,
    _realtimeSetting.onError
 );
```

### Connecting to a room

Connects to the `IP address` `port` of the game server listed in the room information.

```c#
var realtimeSession = new RelayRealtimeSession(
GameManager.Instance.Session.Session.AccessToken.Token,
ipAddress,
port,
encryptionKey,
ByteString.CopyFrom()
);

realtimeSession.OnRelayMessage += message =>
{
_realtimeSetting.onRelayMessage.Invoke(message);
};
realtimeSession.OnJoinPlayer += player =>
{
_realtimeSetting.onJoinPlayer.Invoke(player);
};
OnLeavePlayer += player =>
{
_realtimeSetting.onLeavePlayer.Invoke(player);
};
realtimeSession.OnGeneralError += args =>
{
_realtimeSetting.onGeneralError.Invoke(args);
};
realtimeSession.OnError += error =>
{
_realtimeSetting.onRelayError.Invoke(error);
};
realtimeSession.OnUpdateProfile += player =>
{
_realtimeSetting.onUpdateProfile.Invoke(player);
};
realtimeSession.OnClose += args =>
{
_realtimeSetting.onClose.Invoke(args);
};

AsyncResult<bool> result = null;
yield return realtimeSession.Connect(
this,
r =>
{
result = r;
}
);
```

### Synchronization during game play

The entered name and rock-paper-scissors selection information is periodically sent and received by other players.  
When information is received from another player, the player information is synchronized.

```c#
_realtimeSetting.onUpdateProfile.AddListener(
    player => 
    {
        if (players.ContainsKey(player.ConnectionId))
        {
            var data = player.Profile.ToByteArray();
            var p = players[player.ConnectionId];
            if (p ! = null)
                p.Deserialize(data);
        }
        else
        {
            JoinPlayerHandler(player);
        }
    }
);
```

```c#
realtimeSession.OnUpdateProfile += player =>
{
    _realtimeSetting.onUpdateProfile.Invoke(player);
};
```
