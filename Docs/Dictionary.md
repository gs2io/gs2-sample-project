# 図鑑　解説

[GS2-Dictionary](https://docs.gs2.io/ja/api_reference/dictionary/) を使って、
プレイヤーが取得したエントリー（図鑑への登録状況）を管理するサンプルです。

## GS2-Deploy テンプレート

- [initialize_player_template.yaml - 図鑑](../Templates/initialize_player_template.yaml)

GS2-Dictionary のネームスペースとエントリーモデル（`monster-0001` 〜 `monster-0005`）を定義しています。

図鑑エントリーの登録（`Gs2Dictionary:AddEntriesByUserId`）と削除（`Gs2Dictionary:DeleteEntriesByUserId`）は
**サーバー専用 API** でクライアントから直接呼び出せないため、本サンプルでは
[GS2-Exchange](https://docs.gs2.io/ja/api_reference/exchange/) の交換レートとして実行しています。

```yaml
rateModels:
  # 図鑑エントリーを1件登録する。登録するエントリー名は Config で渡す
  - name: register-entry
    consumeActions: []
    acquireActions:
      - action: Gs2Dictionary:AddEntriesByUserId
        request:
          namespaceName: dictionary-0001
          userId: "#{userId}"
          entryModelNames:
            - "#{entryModelName}"
  # 図鑑エントリーを1件削除する。削除するエントリー名は Config で渡す
  - name: reset-entry
    consumeActions:
      - action: Gs2Dictionary:DeleteEntriesByUserId
        request:
          namespaceName: dictionary-0001
          userId: "#{userId}"
          entryModelNames:
            - "#{entryModelName}"
    acquireActions: []
```

GS2-Exchange のネームスペースは __トランザクションの自動実行__（`TransactionSetting.EnableAutoRun: true`）と
__一括実行__（`EnableAtomicCommit: true`）に設定しているため、交換の結果はリクエストの応答時点で確定します。

## 図鑑設定 DictionarySetting

`Gs2Settings` オブジェクトにアタッチされています。

| 設定名 | 説明 |
---|---
| dictionaryNamespaceName | GS2-Dictionary のネームスペース名 |
| exchangeNamespaceName | GS2-Exchange のネームスペース名 |
| exchangeRateNameRegister | 図鑑エントリーを登録する交換レート名 |
| exchangeRateNameReset | 図鑑エントリーを削除する交換レート名 |

| イベント | 説明 |
---|---
| OnGetEntryModels(List&lt;EzEntryModel&gt; entryModels) | エントリーモデル（定義）を取得したときに呼び出されます。 |
| OnGetEntries(List&lt;EzEntry&gt; entries) | 取得済みのエントリーを取得したときに呼び出されます。 |
| OnRegisterEntry(string entryModelName) | エントリーを登録したときに呼び出されます。 |
| OnResetEntries() | エントリーをすべて削除したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## エントリーモデル（定義）の取得

ログイン後の初期化で、図鑑に登録されうるエントリーの一覧を取得します。

```c#
var it = gs2.Dictionary.Namespace(
    namespaceName: dictionaryNamespaceName
).EntryModels();
while (it.HasNext())
{
    yield return it.Next();
    if (it.Error != null)
    {
        onError.Invoke(it.Error, null);
        yield break;
    }

    if (it.Current != null)
    {
        EntryModels.Add(it.Current);
    }
}
```

## 取得済みエントリーの取得

図鑑を開いたタイミングで、プレイヤーが取得済みのエントリーを取得します。

```c#
var it = gs2.Dictionary.Namespace(
    namespaceName: dictionaryNamespaceName
).Me(
    gameSession: gameSession
).Entries();
```

エントリーモデルの一覧と取得済みエントリーを突き合わせて、登録済み／未登録を表示します。

## 図鑑エントリーの登録

`登録` ボタンを押すと、未登録のエントリーを1件だけ登録します。
登録するエントリー名は Config の `entryModelName` としてスタンプシートに渡され、
`#{entryModelName}` のプレースホルダーが置換されます。

```c#
var domain = gs2.Exchange.Namespace(
    namespaceName: exchangeNamespaceName
).Me(
    gameSession: gameSession
).Exchange();
var future = domain.ExchangeFuture(
    rateName: exchangeRateName,
    count: 1,
    config: new[]
    {
        new EzConfig
        {
            Key = "entryModelName",
            Value = entryModelName,
        },
    }
);
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error, null);
    callback.Invoke(future.Error);
    yield break;
}

// トランザクションの自動実行の完了を待機（連鎖するトランザクションも含めて全て待つ）
var waitFuture = future.Result.WaitFuture(true);
yield return waitFuture;
if (waitFuture.Error != null)
{
    onError.Invoke(waitFuture.Error, null);
    callback.Invoke(waitFuture.Error);
    yield break;
}
```

## 図鑑のリセット

`リセット` ボタンを押すと、取得済みのエントリーをすべて削除します。
交換レートは1件ずつ削除する定義になっているため、取得済みのエントリー分だけ交換を実行します。
