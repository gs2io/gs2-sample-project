# リアルタイム対戦

[GS2-Realtime](https://app.gs2.io/docs/index.html#gs2-realtime) を使用してプレイヤー間で通信対戦するサンプルです。

## GS2-Deploy テンプレート

- [initialize_realtime_template.yaml - マッチメイキング/リアルタイム対戦](../Templates/initialize_realtime_template.yaml)

## リアルタイム設定　RealtimeSetting

![インスペクターウィンドウ](Realtime.png)

| 設定名 | 説明 |
---|---
| realtimeNamespaceName | GS2-Realtime のネームスペース名 |

| イベント | 説明 |
---------|------
| OnRelayMessage(RelayBinaryMessage message) | リアルタイムゲームサーバからメッセージを受信したときに呼び出されます。 |
| OnGetRoom(EzRoom room) | リアルタイムゲームサーバのIPアドレス・ポート情報を取得したときに呼び出されます。 |
| OnJoinPlayer(Player player) | リアルタイムゲームサーバに新しいプレイヤーが参加したときに呼び出されます。 |
| OnLeavePlayer(Player player) | リアルタイムゲームサーバからプレイヤーが離脱したときに呼び出されます。 このコールバックは必ず OnJoinPlayer / OnLeavePlayer のいずれかと同じタイミングで呼び出されます。 |
| OnUpdateProfile(Player player) | 誰かがプレイヤープロフィールを更新したときに呼び出されます。 |
| OnRelayError(Error error) | リアルタイムゲームサーバでエラーが発生したときに呼び出されます。 |
| OnClose(CloseEventArgs error) | リアルタイムゲームサーバから切断されたときに呼び出されます。 |
| OnGeneralError(ErrorEventArgs error) | コネクション関連でエラーが発生したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

### ルーム情報の取得

GS2-Realtime からルームの情報を取得します。  

```c#
AsyncResult<EzGetRoomResult> result = null;
yield return _realtimeModel.GetRoom(
    r => { result = r; },
    GameManager.Instance.Cllient.Client,
    _realtimeSetting.realtimeNamespaceName,
    _realtimeSetting.onGetRoom,
    _realtimeSetting.onError
 );
```

### ルームへの接続

ルーム情報に記載されたゲームサーバの `IPアドレス` `ポート` に接続します。  

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
```

### ゲームプレイ中の同期

入力された名前とじゃんけん選択の情報を定期的に送信し、他のプレイヤーで受け取ります。  
他プレイヤーから情報を受け取った場合、そのプレイヤー情報の同期を行います。

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

```c#
realtimeSession.OnUpdateProfile += player =>
{
    _realtimeSetting.onUpdateProfile.Invoke(player);
};
```
