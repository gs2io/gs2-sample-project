# ゴールド/インベントリ　解説

[GS2-Inventory](https://app.gs2.io/docs/index.html#gs2-inventory) によるインベントリ、アイテムを格納するバッグの実装と、  
ゴールド(ゲーム内の通貨)の管理に使用するサンプルです。  

## GS2-Deploy テンプレート

- [initialize_gold_template.yaml - ゴールド](../Templates/initialize_gold_template.yaml)
- [initialize_inventory_template.yaml - インベントリ](../Templates/initialize_inventory_template.yaml)

## ゴールド設定 GoldSetting

![インスペクターウィンドウ](Gold.png)

| 設定名 | 説明 |
---|---
| inventoryNamespaceName | GS2-Inventory のネームスペース名 |
| inventoryModelName | GS2-Inventory のモデル名 |
| ItemModelName | GS2-Inventory のCurrentItemModelMasterでのゴールドの種類名 |
| identifierAcquireGoldClientId | ゴールドの増加が可能な権限のクライアントID__ |
| identifierAcquireGoldClientSecret | ゴールドの増加が可能な権限のクライアントシークレット |

| イベント | 説明 |
---------|------
| onGetInventoryModel(string inventoryName, EzInventoryModel, List<EzItemModel>) | インベントリーモデルを取得したときに呼び出されます。 |
| onGetInventory(EzInventory inventory, List<EzItemSet> itemSets) | インベントリーの情報を取得したときに呼び出されます。 |
| onAcquire(Product product) | ゴールドを追加したときに呼び出されます。 |
| onConsume(Product product) | ゴールドを消費したときに呼び出されます。 |
| onError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## インベントリ設定 InventorySetting

![インスペクターウィンドウ](Inventory.png)

| 設定名 | 説明 |
---|---
| inventoryNamespaceName | GS2-Inventory のインベントリのネームスペース名 |
| inventoryModelName | GS2-Inventoryのインベントリのモデルのネームスペース名 |
| identifierAcquireItemClientId | アイテムの増加が可能な権限のクライアントID__ |
| identifierAcquireItemClientSecret | アイテムの増加が可能な権限のクライアントシークレット |

| イベント | 説明 |
---|---
| onGetInventoryModel(string inventoryName, EzInventoryModel, List<EzItemModel>) | インベントリーモデルを取得したときに呼び出されます。 |
| onGetInventory(EzInventory inventory, List<EzItemSet> itemSets) | インベントリーの情報を取得したときに呼び出されます。 |
| onAcquire(Product product) | アイテムを追加したときに呼び出されます。 |
| onConsume(Product product) | アイテムを消費したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## インベントリモデルの取得

インベントリモデルを取得します。

```c#
AsyncResult<EzGetInventoryModelResult> result = null;
yield return client.Inventory.GetInventoryModel(
    r => { result = r; },
    inventoryNamespaceName,
    inventoryModelName
);
```

## ゴールド/インベントリの取得

インベントリの情報を取得します。  
ゴールド（ゲーム内通貨）として扱うインベントリの場合は、  
対象の ItemSet の Count がゴールドの量を表しています。  

```c#
AsyncResult<EzGetInventoryResult> result = null;
yield return client.Inventory.GetInventory(
    r => { result = r; },
    session,
    inventoryNamespaceName,
    inventoryName
);
```

## ゴールド/アイテムの消費

ゴールド/アイテムを消費、量を減らします。

```c#
AsyncResult<EzConsumeResult> result = null;
yield return client.Inventory.Consume(
    r => { result = r; },
    session,
    inventoryNamespaceName,
    inventoryModelName,
    itemModelName,
    consumeValue,
    itemSetName
);
```

## ゴールド/アイテムの入手

ゴールド/アイテムを入手、量を増やします。

ゴールド/アイテムを増やす権限のある [GS2-Identifier](https://app.gs2.io/docs/index.html#gs2-identifier) ユーザーを追加し、  
クライアントからの操作を可能にしています。  
デバッグ目的の使用のサンプルです。  

```c#
{
    // ※この処理はサンプルの動作確認のためものです。
    // 実際にクライアントが直接アイテムの増加を行う実装は非推奨となります。
    
    var restSession = new Gs2RestSession(
        new BasicGs2Credential(
            identifierAcquireItemClientId,
            identifierAcquireItemClientSecret
        )
    );
    var error = false;
    yield return restSession.Open(
        r =>
        {
            if (r.Error != null)
            {
                onError.Invoke(r.Error);
                error = true;
            }
        }
    );

    if (error)
    {
        yield return restSession.Close(() => { });
        yield break;
    }

    var restClient = new Gs2InventoryRestClient(
        restSession
    );

    yield return restClient.AcquireItemSetByUserId(
        new AcquireItemSetByUserIdRequest()
            .WithNamespaceName(inventoryNamespaceName)
            .WithUserId(session.AccessToken.UserId)
            .WithInventoryName(inventoryModelName)
            .WithItemName(itemModelName)
            .WithAcquireCount(value),
        r =>
        {
            if (r.Error != null)
            {
                onError.Invoke(r.Error);
                error = true;
            }
            else
            {
                onAcquire.Invoke(
                    EzInventory.FromModel(r.Result.Inventory),
                    r.Result.Items.Select(item => EzItemSet.FromModel(item)).ToList(),
                    value
                );
            }
        }
    );

    yield return restSession.Close(() => { });
}
```