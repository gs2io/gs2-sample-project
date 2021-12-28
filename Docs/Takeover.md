# アカウント引継ぎ　解説

[GS2-Account](https://app.gs2.io/docs/index.html#gs2-account) で作成された匿名アカウントにメールアドレスや、  
Game Center/Google Play Game Service のアカウントを関連付けて  
アカウント引継ぎをするサンプルです。

## GS2-Deploy テンプレート

- [initialize_account_template.yaml - ログイン/アカウント連携・引継ぎ](../Templates/initialize_account_template.yaml)

## アカウント引継ぎ設定 Setting

![インスペクターウィンドウ](TakeOver.png)

| イベント | 説明 |
---------|------
| OnSetTakeOver(EzTakeOver takeOver) | アカウントの引継ぎ情報が設定されたときに呼び出されます。 |
| OnDeleteTakeOver(EzTakeOver takeOver) | アカウントの引継ぎ情報が削除されたときに呼び出されます。 |
| OnDoTakeOver(EzAccount takeOver) | アカウントの引継ぎが実行されたときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## アカウントの連携/引継ぎの流れ

`アカウント連携`ボタンから開いたメニューで、  
初回起動時に作成済みの匿名アカウントに、引継ぎ情報を登録する`アカウント連携`、  
引継ぎを実行する`アカウント引継ぎ`を選びます。

引継ぎを実行する時点でアカウントがログイン状態である必要はありません。

## アカウントの連携の流れ

`Email`でメールアドレス・パスワードを用いたアカウントの連携設定、  
`Game Center`もしくは`Google Play`で Game Center/Google Play Game Service  
のような配信プラットフォームのサービスを用いたアカウントとの連携を行うかを選択します。
プラットフォームサービスの連携は、先に各サービスにデバイス上でログインしてから連携を行う必要があります。

`Email`ではさらにEメールアドレスとパスワードの設定を行います。

### 引継ぎ設定を取得

現在設定されている引継ぎ設定を取得します。

```c#
AsyncResult<EzListTakeOverSettingsResult> result = null;
yield return client.Account.ListTakeOverSettings(
    r => { result = r; },
    session,
    accountNamespaceName,
    null,
    30
);
```

### 引継ぎ情報の登録

匿名アカウントに引継ぎ設定を登録します。

```c#
AsyncResult<EzAddTakeOverSettingResult> result = null;
yield return client.Account.AddTakeOverSetting(
    r => { result = r; },
    session,
    accountNamespaceName,
    type,
    userIdentifier,
    password
);
```

### 引継ぎ情報の削除

匿名アカウントに連携済みの引継ぎ設定を削除（解除）します。

```c#
AsyncResult<EzDeleteTakeOverSettingResult> result = null;
yield return client.Account.DeleteTakeOverSetting(
    r => { result = r; },
    session,
    accountNamespaceName,
    type
);
```

### アカウントの引継ぎ

メールアドレス・パスワードを指定してのアカウント引継ぎ、  
もしくはすでにプラットフォームのサービスと連携済みのアカウント引継ぎを実行します。  
取得したアカウント情報をローカルストレージに保存します。

すでにログイン済みの場合は、得られたアカウントで再ログインが必要です。

```c#
AsyncResult<EzDoTakeOverResult> result = null;
yield return client.Account.DoTakeOver(
    r => { result = r; },
    accountNamespaceName,
    type,
    userIdentifier,
    password
);
```



