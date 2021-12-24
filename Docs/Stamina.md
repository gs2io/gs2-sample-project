# スタミナ/スタミナストア　解説

GS2-Stamina を使ってスタミナ値を管理するサンプルです。  
GS2-Exchange と連携し GS2-Money の課金通貨を消費しスタミナ値を回復するストア機能のサンプルです。  

## GS2-Deploy テンプレート

- [initialize_stamina_template.yaml - スタミナ/スタミナストア](../Templates/initialize_stamina_template.yaml)

## スタミナ設定 StaminaSetting

![インスペクターウィンドウ](Stamina.png)

| 設定名 | 説明 |
---|---
| staminaNamespaceName | GS2-Stamina のネームスペース名 |
| staminaModelName | GS2-Stamina のスタミナモデル名 |
| staminaName | GS2-Stamina のスタミナの種類名 |
| exchangeNamespaceName | スタミナの回復に使用する GS2-Exchange のネームスペース名 |
| exchangeRateName | スタミナの回復に使用する GS2-Exchange の交換レート名 |
| exchangeKeyId | GS2-Exchange で交換処理に発行するスタンプシートの署名計算に使用する暗号鍵 |
| distributorNamespaceName | 交換したスタミナ回復処理を配送する GS2-Distributor のネームスペース名 |

| イベント | 説明 |
---|---
| OnGetStamina(EzStamina stamina) | スタミナの情報を取得したときに呼び出されます。 |
| OnBuy() | 交換が完了したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

### スタミナを取得

最新のスタミナの状態を取得します。

```csharp
AsyncResult<EzGetStaminaResult> result = null;
yield return client.Stamina.GetStamina(
    r =>
    {
        result = r;
    },
    session,
    staminaNamespaceName,
    staminaName
);
```

### スタミナを消費

スタミナを5消費します。

```csharp
AsyncResult<EzConsumeResult> result = null;
yield return client.Stamina.Consume(
    r =>
    {
        result = r;
    },
    session,
    staminaNamespaceName,
    staminaModel.Name,
    consumeValue
);
```

### スタミナ回復の購入

スタミナ回復の購入処理を実行します。

```csharp
AsyncResult<EzExchangeResult> result = null;
yield return gs2Client.client.Exchange.Exchange(
    r => { result = r; },
    request.gameSession,
    gs2StaminaSetting.exchangeNamespaceName,
    gs2StaminaSetting.exchangeRateName,
    1,
    new List<Gs2.Unity.Gs2Exchange.Model.EzConfig>
    {
        new Gs2.Unity.Gs2Exchange.Model.EzConfig
        {
            Key = "slot",
            Value = MoneyController.Slot.ToString(),
        }
    }
);
```

`result.Result.StampSheet` にスタンプシートが返りますので

```
var machine = new StampSheetStateMachine(
    stampSheet,
    gs2Client.client,
    gs2StaminaSetting.distributorNamespaceName,
    gs2StaminaSetting.exchangeKeyId
);
yield return machine.Execute();
```

このようにスタンプシートを実行することで実際に課金通貨とスタミナ値の交換を実行します。

![交換](Exchange.png)