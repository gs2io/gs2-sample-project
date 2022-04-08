# フレンド機能　解説

[GS2-Friend](https://app.gs2.io/docs/index.html#gs2-friend) をつかったフレンド機能の実装のサンプルです。  
自プレイヤーのプロフィールの設定、フレンドリストの表示、送信したフレンドの登録リクエストの一覧の表示、  
受信したフレンドリクエストの一覧の表示、ブラックリストの表示、フォローしているユーザーの一覧の表示等を行います。

中央下の吹き出しアイコンからアクセスできるチャットウィンドウの、  
受信した他プレイヤーのメッセージの吹き出しをタップすると、  
対象となる他プレイヤーへのフォロー、フレンドリクエスト、ブラックリストへの追加が行えます。  
他プレイヤーのUserIdの取得に、チャットの送受信を利用しています。

![PlayerInfo](PlayerInfo.png)

## GS2-Deploy テンプレート

- [initialize_friend_template.yaml - フレンド機能](../Templates/initialize_friend_template.yaml)

## フレンド設定 FriendSetting

![インスペクター](Friend.png)
![インスペクター](Friend2.png)

| 設定名 | 説明 |
|---|---|
| friendNamespaceName | GS2-Friend のネームスペース名 |

| イベント | 説明 |
|---|---|
| onGetProfile(EzProfile) | 自プレイヤーのプロフィールを取得したときに呼び出されます。 |
| onUpdateProfile(EzProfile) | 自プレイヤーのプロフィールを更新したときに呼び出されます。 |
| onDescribeFriends(List<EzFriendUser>) | フレンドの一覧を取得したときに呼び出されます。 |
| onSendRequest(EzFriendRequest) | フレンドリクエストを送信したときに呼び出されます。 |
| onAccept(EzFriendRequest) | フレンドリクエストを承認したときに呼び出されます。 |
| onReject(EzFriendRequest) | フレンドリクエストを拒否したときに呼び出されます。 |
| onDeleteRequest(EzFriendRequest) | 送信したフレンドリクエストを削除したときに呼び出されます。 |
| onDescribeSendRequests(List<EzFriendRequest>) | 送信したフレンドリクエストの一覧を取得したときに呼び出されます。 |
| onDescribeReceiveRequests(List<EzFriendRequest>) | 受信したフレンドリクエスト一覧を取得したときに呼び出されます。 |
| onGetFriend(EzFriendUser) | フレンド情報を取得したときに呼び出されます。 |
| onDeleteFriend(EzFriendUser) | フレンドを削除したときに呼び出されます。 |
| onGetPublicProfile(EzPublicProfile) | 他プレイヤーの公開プロフィールを取得したときに呼び出されます。 |
| onGetBlackList(List<string>) | ブラックリストを取得したときに呼び出されます。 |
| onRegisterBlackList(EzBlackList) | ブラックリストにユーザーを登録したときに呼び出されます。 |
| onUnregisterBlackList(EzBlackList) | ブラックリストからユーザーを削除したときに呼び出されます。 |
| onFollow(EzFollowUser) | 他プレイヤーをフォローしたときに呼び出されます。 |
| onUnfollow(EzFollowUser) | フォローしている相手をアンフォローしたときに呼び出されます。 |
| onDescribeFollowUsers(List<EzFollowUser>) | フォローしているユーザー一覧を取得したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## 自分のプロフィールの編集

`プロフィール`ボタンをタップすると、自分のプロフィールを取得し`プロフィール`ダイアログを開きます。

```c#
AsyncResult<EzGetProfileResult> result = null;
yield return client.Friend.GetProfile(
callback: r => { result = r; },
session,
friendNamespaceName
);
```

InputFieldでプロフィールの文言を編集後、`更新`ボタンをタップしプロフィールの更新を行います。

```c#
AsyncResult<EzUpdateProfileResult> result = null;
yield return client.Friend.UpdateProfile(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    publicProfile,
    followerProfile,
    friendProfile
);
```

## フレンドの一覧/削除

`フレンド`ボタンでフレンドの一覧を取得し`フレンドリスト`ダイアログを開きます。

```c#
AsyncResult<EzDescribeFriendsResult> result = null;
yield return client.Friend.DescribeFriends(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    false,
    30,
    null
);
```

`フレンドリスト`ダイアログのユーザー項目の`削除`で、フレンドを削除、フレンド登録を解除します。

```c#
AsyncResult<EzDeleteFriendResult> result = null;
yield return client.Friend.DeleteFriend(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

## フレンドリクエストの送信

チャットのメッセージをタップで開く`プレイヤー`ダイアログで`フレンド申請`をタップすると。  
対象となるユーザーに対してフレンドリクエストを送信します。  
相手ユーザーの承認/拒否を待っている状態になります。

```c#
AsyncResult<EzSendRequestResult> result = null;
yield return client.Friend.SendRequest(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

## 送信/受信したフレンドリクエストリクエストの一覧取得 

`送信中リクエスト`ボタンをタップして、送信したフレンドリクエストの一覧を取得し  
`送信したフレンドリクエスト`ダイアログを開きます。

```c#
AsyncResult<EzDescribeSendRequestsResult> result = null;
yield return client.Friend.DescribeSendRequests(
    callback: r => { result = r; },
    session,
    friendNamespaceName
);
```

送信したフレンドリクエストは相手が承認/拒否を行う前であれば削除、取り下げることができます。  
`送信中リクエスト`ボタンから開く`送信したフレンドリクエスト`ダイアログのユーザー項目の`削除`で、リクエストを削除します。

```c#
AsyncResult<EzDeleteRequestResult> result = null;
yield return client.Friend.DeleteRequest(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

`受信中リクエスト`ボタンをタップして、受信したフレンドリクエストの一覧を取得し  
`受信したフレンドリクエスト`ダイアログを開きます。

```c#
AsyncResult<EzDescribeReceiveRequestsResult> result = null;
yield return client.Friend.DescribeReceiveRequests(
    callback: r => { result = r; },
    session,
    friendNamespaceName
);
```

## フレンドリクエストの承認/拒否

`受信中リクエスト`ボタンから開く`受信したフレンドリクエスト`ダイアログのユーザー項目の`承認`ボタンで、フレンドリクエストを承認します。

```c#
AsyncResult<EzAcceptResult> result = null;
yield return client.Friend.Accept(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    fromUserId
);
```

`拒否`ボタンで、フレンドリクエストを拒否します。

```c#
AsyncResult<EzRejectResult> result = null;
yield return client.Friend.Reject(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    fromUserId
);
```

## フレンド登録の解除

`フレンド`ボタンから開く`フレンドリスト`ダイアログのユーザー項目の`削除`で、フレンドを削除、フレンド登録を解除します。

```c#
AsyncResult<EzDeleteFriendResult> result = null;
yield return client.Friend.DeleteFriend(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

## ブラックリスト

チャットのメッセージから開く`プレイヤー`ダイアログで他プレイヤーを`ブラックリストに追加`します。

```c#
AsyncResult<EzRegisterBlackListResult> result = null;
yield return client.Friend.RegisterBlackList(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

`ブラックリスト`ボタンで、ブラックリストに登録しているユーザーの一覧を取得し表示します。

```c#
AsyncResult<EzGetBlackListResult> result = null;
yield return client.Friend.GetBlackList(
    callback: r => { result = r; },
    session,
    friendNamespaceName
);
```

`ブラックリスト`ダイアログのユーザー項目の`削除`で、ブラックリスト登録している相手を解除します。

```c#
AsyncResult<EzUnregisterBlackListResult> result = null;
yield return client.Friend.UnregisterBlackList(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

## フォロー

チャットのメッセージから開く`プレイヤー`ダイアログで、他プレイヤーを`フォロー`します。

```c#
AsyncResult<EzFollowResult> result = null;
yield return client.Friend.Follow(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

`フォロー`ボタンからフォローしているユーザー一覧を取得し表示します。

```c#
AsyncResult<EzDescribeFollowUsersResult> result = null;
yield return client.Friend.DescribeFollowUsers(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    true
);
```

`フォロー`ダイアログのユーザー項目の`削除`で、フォローしている相手をアンフォローします。

```c#
AsyncResult<EzUnfollowResult> result = null;
yield return client.Friend.Unfollow(
    callback: r => { result = r; },
    session,
    friendNamespaceName,
    targetUserId
);
```

