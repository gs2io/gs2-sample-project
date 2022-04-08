# アカウントの作成・ログイン　解説

[GS2-Account](https://app.gs2.io/docs/index.html#gs2-account) を使用してログインする機能のサンプルです。

## GS2-Deploy テンプレート

- [initialize_account_template.yaml](../Templates/initialize_account_template.yaml)

## ログイン設定 LoginSetting

![Login](Gs2Settings.png)

| 設定名 | 説明 |
--------|-----
| accountNamespaceName | GS2-Account のネームスペース名 |
| accountEncryptionKeyId | GS2-Account でアカウント情報の暗号化に使用する GS2-Key の暗号鍵GRN |
| gatewayNamespaceName | GS2-Gateway のネームスペース名 |

| イベント | 説明 |
---------|------
| OnLoadAccount(EzAccount account) | アカウントがロードされたときに呼び出されます。 |
| OnSaveAccount(EzAccount account) | アカウントがセーブされたときに呼び出されます。 |
| OnCreateAccount(EzAccount account) | アカウントが作成されたときに呼び出されます。 |
| OnLogin(EzAccount account, GameSession session) | ログインに成功したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## ログインの流れ

PlayerPrefsから保存済みのアカウント情報を読み込みます。  
すでに保存済みのアカウント情報があれば、ログインを実行します。  
初回起動時等、保存済みアカウント情報がないときはアカウントの新規作成を行います。

### アカウント作成

__GS2-Account__ でアカウントを作成します。

```c#
AsyncResult<EzCreateResult> result = null;
yield return client.Account.Create(
    r =>
    {
        result = r;
    },
    accountNamespaceName
);
```

PlayerPrefsに新規作成したアカウント情報を保存します。  

### アカウント削除

アカウント情報をローカルストレージから削除します。  
主にデバッグ目的の機能です。  
クライアントに保存済みのアカウント情報と異なったプロジェクト、ネームスペースの環境にログインを実行した場合、  
サーバ側にアカウント情報が存在しないため、ログインに失敗します。その場合はアカウント情報を削除し、  
ログインを実行することで新規アカウントが作成され、ログインが可能となります。

### ログイン処理

```c#
AsyncResult<EzAuthenticationResult> result = null;
yield return client.Account.Authentication(
    r =>
    {
        result = r;
    },
    accountNamespaceName,
    userId,
    accountEncryptionKeyId,
    password
);
```
```c#
AsyncResult<EzLoginResult> result2 = null;
yield return client.Auth.Login(
    r =>
    {
        result2 = r;
    },
    userId,
    accountEncryptionKeyId,
    result.Result.Body,
    result.Result.Signature
);

var session = new GameSession(
    new AccessToken()
        .WithToken(result2.Result.Token)
        .WithExpire(result2.Result.Expire)
        .WithUserId(result2.Result.UserId)
);
```

アカウント情報で __GS2-Account__ の認証処理をおこない、  
結果の EzAuthenticationResult の body と signature を  
[GS2-Auth](https://app.gs2.io/docs/index.html#gs2-auth) の ログイン処理に渡してアクセストークンを得ます。 

```c#
AsyncResult<EzSetUserIdResult> result3 = null;
yield return client.Gateway.SetUserId(
    r => { result3 = r; },
    session,
    gatewayNamespaceName,
    true
);
```

[GS2-Gateway](https://app.gs2.io/docs/index.html#gs2-gateway) にログインした自分のユーザーIDを設定し
、このユーザークライアントに対するプッシュ通知が受け取れるようにしています。  
チャット([GS2-Chat](Chat.md))のメッセージ投稿の通知、
フレンド申請(GS2-Friend)](Friend.md)等の通知、
マッチメイキング([GS2-Matchmaking](Matchmaking.md))の遷移の通知を受けとるために使用します。


