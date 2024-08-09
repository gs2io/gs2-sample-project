# マッチメイキング　解説

[GS2-Matchmaking](https://app.gs2.io/docs/index.html#gs2-matchmaking) を使用して対戦・協力プレイをするプレイヤーを見つけるサンプルです。

## GS2-Deploy テンプレート

- [initialize_matchmaking_template.yaml](../Templates/initialize_matchmaking_template.yaml)

## マッチメイキング設定 MatchmakingSetting

![インスペクターウィンドウ](Matchmaking.png)

| 設定名 | 説明 |
---|---
| matchmakingNamespaceName | GS2-Matchmaking のネームスペース名 |

| イベント | 説明 |
---|---
| OnJoinPlayer(EzGathering gathering, string userId) | 参加中のギャザリングに新しい参加者が来た時に呼び出されます。 |
| OnLeavePlayer(EzGathering gathering, string userId) | 参加中のギャザリングから参加者が離脱した時に呼び出されます。 |
| OnUpdateJoinedPlayerIds(EzGathering gathering, List<string> joinedPlayerIds) | アカウントが作成されたときに呼び出されます。 |
| OnLogin(EzAccount account, GameSession session) | 参加中のギャザリングのプレイヤーIDリストが更新されたときに呼び出されます。 このコールバックは必ず OnJoinPlayer / OnLeavePlayer のいずれかと同じタイミングで呼び出されます。 |
| OnMatchmakingComplete(EzGathering gathering, List<string> joinedPlayerIds) | マッチメイキングが完了したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## マッチメイキングの流れ

![Matching](Matching.png)

`ギャザリング作成` で参加人数を設定してギャザリング（マッチングの単位）を作成します。  
`ギャザリング待機` でギャザリングへの参加をリクエストします。  

### ギャザリングの新規作成

自分を含むプレイヤー人数を入力し、 `Create` を選択するとギャザリングを新規作成します。

・UniTask有効時
```c#
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                var result = await domain.CreateGatheringAsync(
                    player: new EzPlayer
                    {
                        RoleName = "default"
                    },
                    attributeRanges: null,
                    capacityOfRoles: new[]
                    {
                        new EzCapacityOfRole
                        {
                            RoleName = "default",
                            Capacity = Capacity
                        }
                    },
                    allowUserIds: null,
                    expiresAt: null,
                    expiresAtTimeSpan: null
                );
                Gathering = await result.ModelAsync();

                JoinedPlayerIds.Clear();
                JoinedPlayerIds.Add(gameSession.AccessToken.UserId);

                onUpdateJoinedPlayerIds.Invoke(Gathering, JoinedPlayerIds);
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
```
・コルーチン使用時
```c#
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var future = domain.CreateGatheringFuture(
                player: new EzPlayer
                {
                    RoleName = "default"
                },
                attributeRanges: null,
                capacityOfRoles: new [] {
                    new EzCapacityOfRole
                    {
                        RoleName = "default",
                        Capacity = Capacity
                    }
                },
                allowUserIds: null,
                expiresAt: null,
                expiresAtTimeSpan: null
            );
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(
                    future.Error
                );
                yield break;
            }

            var future2 = future.Result.ModelFuture();
            yield return future2;
            if (future2.Error != null)
            {
                onError.Invoke(
                    future2.Error
                );
                yield break;
            }
            
            JoinedPlayerIds.Clear();
            Gathering = future2.Result;
            JoinedPlayerIds.Add(gameSession.AccessToken.UserId);

            onUpdateJoinedPlayerIds.Invoke(Gathering, JoinedPlayerIds);
```

募集条件を参加者全員が`default` ロールで設定し、誰でも参加可能なギャザリングを作成しています。  
Capacity に参加人数を指定しています。  
ギャザリングの作成に成功すると `マッチング待機` ダイアログになり、参加中のユーザーIDの一覧を表示します。

### 既存のギャザリングに参加

既存のギャザリングへの参加をリクエストします。

・UniTask有効時
```c#
            ResultGatherings.Clear();
            Gathering = null;
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            try
            {
                ResultGatherings = await domain.DoMatchmakingAsync(
                    new EzPlayer
                    {
                        RoleName = "default"
                    }
                ).ToListAsync();
                JoinedPlayerIds.Clear();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
```
・コルーチン使用時
```c#
            ResultGatherings.Clear();
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            );
            var it = domain.DoMatchmaking(
                new EzPlayer
                {
                    RoleName = "default"
                }
            );
            while (it.HasNext())
            {
                yield return it.Next();
                if (it.Error != null)
                {
                    onError.Invoke(it.Error);
                    break;
                }

                if (it.Current != null)
                {
                    ResultGatherings.Add(it.Current);
                }
            }
                
            JoinedPlayerIds.Clear();
```

このサンプルでは `default` ロールを募集しているギャザリングに参加します。  
ギャザリングが見つからなかった場合は `NotFoundException` が返ります。  

マッチメイキング処理の途中でタイムアウトをしたときには、
正常なレスポンスで `EzDoMatchmakingResult.Result.Item` に null が返ります。  
その場合は、戻り値に含まれる `MatchmakingContextToken` を使って  
再度ギャザリングを探す処理を継続するようリクエストします。  

ギャザリングの作成に成功すると `マッチング待機` ダイアログに遷移し、参加中のユーザーIDの一覧を表示します。

### マッチメイキングのキャンセル

マッチメイキングをキャンセルします。

・UniTask有効時
```c#
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            ).Gathering(
                gatheringName: Gathering.Name
            );
            try
            {
                var result = await domain.CancelMatchmakingAsync();
                Gathering = await result.ModelAsync();
                
                onMatchmakingCancel.Invoke(Gathering);
                Gathering = null;
                JoinedPlayerIds.Clear();
            }
            catch (Gs2Exception e)
            {
                onError.Invoke(e);
            }
```
・コルーチン使用時
```c#
            var domain = gs2.Matchmaking.Namespace(
                namespaceName: matchmakingNamespaceName
            ).Me(
                gameSession: gameSession
            ).Gathering(
                gatheringName: Gathering.Name
            );
            var future = domain.CancelMatchmaking();
            yield return future;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
 
            var domain2 = future.Result;
            var future2 = domain2.Model();
            yield return future2;
            if (future.Error != null)
            {
                onError.Invoke(future.Error);
                yield break;
            }
            
            onMatchmakingCancel.Invoke(future2.Result);
            
            Gathering = null;
            JoinedPlayerIds.Clear();
```

### 参加者の増減/マッチメイキング完了の通知

[GS2-Gateway](https://app.gs2.io/docs/index.html#gs2-gateway) を使用してサーバからの通知を受け取ります。
サーバーからは以下のようなメッセージが送られます。

| メッセージ | 説明 |
---|---
Gs2Matchmaking:Join | ギャザリングに新たにプレイヤーが参加した
Gs2Matchmaking:Leave | ギャザリングからプレイヤーが離脱した
Gs2Matchmaking:Complete | マッチングが完了した

```c#
        public void PushNotificationHandler(NotificationMessage message)
        {
            if (!message.issuer.StartsWith("Gs2Matchmaking:")) return;

            _issuer = message.issuer;

            if (message.issuer.EndsWith(":Join"))
            {
                var notification = JsonMapper.ToObject<JoinNotification>(message.payload);
                if (!_matchmakingModel.JoinedPlayerIds.Contains(notification.joinUserId))
                {
                    _matchmakingModel.JoinedPlayerIds.Add(notification.joinUserId);
                    _userId = notification.joinUserId;
                    _recievedNotification = true;
                }
            }
            else if (message.issuer.EndsWith(":Leave"))
            {
                var notification = JsonMapper.ToObject<LeaveNotification>(message.payload);
                _matchmakingModel.JoinedPlayerIds.Remove(notification.leaveUserId);
                _userId = notification.leaveUserId;
                _recievedNotification = true;
            }
            else if (message.issuer.EndsWith(":Complete"))
            {
                _recievedNotification = true;
                _complete = true;
            }
        }
```

通知ハンドラの登録
```c#
GameManager.Instance.Profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;
```

