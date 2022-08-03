# Real-Time Game Explanation

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
| OnRelayMessage(RelayBinaryMessage message) | Called when a message is received from a Realtime game server. |
| OnGetRoom(EzRoom room) | Called when IP address and port information of a Realtime game server is obtained. |
| OnJoinPlayer(Player player) | Called when a new player joins the Realtime game server. |
| OnLeavePlayer(Player player) | Called when a player leaves the Realtime game server. This callback is always called at the same time as either OnJoinPlayer / OnLeavePlayer. |
| OnUpdateProfile(Player player) | Called when someone updates the player profile. |
| OnRelayError(Error error) | Called when an error occurs on the Realtime game server. |
| OnClose(CloseEventArgs error) | Called when disconnected from the Realtime game server. |
| OnGeneralError(ErrorEventArgs error) | Called when a connection-related error occurs. |
| OnError(Gs2Exception error) | Called when an error occurs. |

### Retrieving Room Information

Get room information from GS2-Realtime.

When UniTask is enabled
```c#
var domain = gs2.Realtime.Namespace(
    namespaceName: realtimeNamespaceName
).Room(
    roomName: gatheringName
);
try
{
    room = await domain.ModelAsync();
    
    onGetRoom.Invoke(room);
    
    return room;
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    throw;
}
```
When coroutine is used
```c#
var domain = gs2.Realtime.Namespace(
    namespaceName: realtimeNamespaceName
).Room(
    roomName: gatheringName
);
var future = domain.Model();
yield return future;
if (future.Error != null)
{
    onError.Invoke(
        future.Error
    );
    callback.Invoke(null);
    yield break;
}

room = future.Result;

onGetRoom.Invoke(room);

callback.Invoke(room);
```

### Connecting to a room

Connect to the `IP address` `port` of the game server listed in the room information.  
After creating a RelayRealtimeSession, set up various callbacks  
Connect to the room `realtimeSession.Connect` function.

When UniTask is enabled
```c#
var realtimeSession = new RelayRealtimeSession(
    GameManager.Instance.Session.AccessToken.Token,
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
realtimeSession.OnLeavePlayer += player =>
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

try
{
    await realtimeSession.ConnectAsync(
        this
    );
}
catch (Gs2Exception e)
{
    _realtimeSetting.onError.Invoke(
        e
    );
    return null;
}

return realtimeSession;
```
When coroutine is used
```c#
var realtimeSession = new RelayRealtimeSession(
    GameManager.Instance.Session.AccessToken.Token,
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
realtimeSession.OnLeavePlayer += player =>
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

if (realtimeSession.Connected)
{
    callback.Invoke(
        new AsyncResult<RelayRealtimeSession>(realtimeSession, null)
    );
}
else
{
    if (result.Error != null)
    {
        _realtimeSetting.onError.Invoke(
            result.Error
        );
        callback.Invoke(
            new AsyncResult<RelayRealtimeSession>(null, result.Error)
        );
    }
}
```

### Synchronization during game play

The name entered in the Inputfield is periodically sent and received by other players as part of the player's profile information.  
When profile information is received from another player, the player information is synchronized.

> Send

When UniTask is enabled
```c#
public async UniTask UpdateProfileAsync()
{
    while (true)
    {
        await UniTask.Delay(300);

        ByteString binary = null;
        try
        {
            binary = ByteString.CopyFrom(ProfileSerialize());
        }
        catch (Exception e)
        {
            Debug.Log(e);
            continue;
        }

        if (Session != null && Session.Connected)
        {
            bool lockWasTaken = false;
            try
            {
                System.Threading.Monitor.TryEnter(this, ref lockWasTaken);

                if (lockWasTaken)
                {
                    await Session.UpdateProfileAsync(
                        binary
                    );
                }
            }
            finally
            {
                if (lockWasTaken) System.Threading.Monitor.Exit(this);
            }
        }
        else
        {
            break;
        }
    }
}
```
When coroutine is used
```c#
public IEnumerator UpdateProfile()
{
    while (true)
    {
        yield return new WaitForSeconds(0.3f);

        ByteString binary = null;
        try
        {
            binary = ByteString.CopyFrom(ProfileSerialize());
        }
        catch (Exception e)
        {
            Debug.Log(e);
            continue;
        }

        if (Session != null)
        {
            bool lockWasTaken = false;
            try
            {
                System.Threading.Monitor.TryEnter(this, ref lockWasTaken);

                if (lockWasTaken)
                {
                    yield return Session.UpdateProfile(
                        r => { },
                        binary
                    );
                }
            }
            finally
            {
                if (lockWasTaken) System.Threading.Monitor.Exit(this);
            }
        }
    }
}
```

> Receive

```c#
_realtimeSetting.onUpdateProfile.AddListener(
    player => 
    {
        if (players.ContainsKey(player.ConnectionId))
        {
            var data = player.Profile.ToByteArray();
            var p = players[player.ConnectionId];
            if (p != null)
                p.Deserialize(data);
        }
        else
        {
            JoinPlayerHandler(player);
        }
    }
);
```

### Sending binary data

`Send` sends information such as the selected rock-paper-scissors move to other players.  
If you specify an array of destination `connection ID` as the third argument of `Send`, the data will be sent to the specified player.  
If information is received from another player, the UI is updated with that player's information.

> Send

When UniTask is enabled
```c#
public async UniTask SendAsync()
{
    ByteString binary = null;
    try
    {
        binary = ByteString.CopyFrom(StateSerialize());
    }
    catch (Exception e)
    {
        Debug.Log(e);
    }

    await Session.SendAsync(
        binary
    );
}
```
When coroutine is used
```c#
public IEnumerator Send()
{
    ByteString binary = null;
    try
    {
        binary = ByteString.CopyFrom(StateSerialize());
    }
    catch (Exception e)
    {
        Debug.Log(e);
    }

    yield return Session.Send(
        r => { },
        binary
    );
}
```

> Receive
```c#
_realtimeSetting.onRelayMessage.AddListener(
    message => 
    {
        if (players.ContainsKey(message.ConnectionId))
        {
            var data = message.Data.ToByteArray();
            var p = players[message.ConnectionId];
            if (p != null)
                p.StateDeserialize(data);
        }
    }
);
```
