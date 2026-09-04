# 抽選機能　解説

[GS2-Showcase](https://docs.gs2.io/ja/api_reference/showcase/) で商品を販売、[GS2-Lottery](https://docs.gs2.io/ja/api_reference/lottery/) による抽選を行い、  
専用のインベントリーにアイテムの払い出しを行うサンプルです。

## GS2-Deploy テンプレート

- [initialize_gamecycle_template.yaml - 抽選機能](../Templates/initialize_gamecycle_template.yaml)

## Unity IAPの有効化、インポート

GS2-Money2を使用したサンプルの動作には、Unity IAPの有効化が必要になります。  
[Unity IAP の設定](https://docs.unity3d.com/ja/current/Manual/UnityIAPSettingUp.html)  
サービスウィンドウでのIn-App Purchasingの有効化、  
IAP パッケージのインポートを行います。

## 抽選機能設定 LotterySetting

![Lottery](Lottery.png)

| 設定名                   | 説明                                                                    |
|-----------------------|--------------------------------------------------------------------------|
| lotteryName           | GS2-Lotteryの抽選マスターデータの種類名, GS2-Showcaseの商品棚マスターデータの商品名 |
| ShowcaseNamespaceName | GS2-Showcaseのネームスペース名                                               |

| イベント                                                     | 説明  　　　　　　　　                    |
|-------------------------------------------------------------|---------------------------------------|
| OnGetShowcase(EzShowcase)                                   | 商品棚情報を取得したときに呼び出されます。    |
| OnAcquireInventoryItem(List<AcquireItemSetByUserIdRequest>) | 抽選でアイテムを入手したときに呼び出されます。 |
| OnError(Gs2Exception error)                                 | エラーが発生したときに呼び出されます。        |

## 抽選商品購入処理の流れ

### ストアの表示

![LotteryList](LotteryList.png)

商品リストを取得し、ストアを表示します。

・UniTask有効時
```c#
var domain = gs2.Showcase.Namespace(
    namespaceName: showcaseNamespaceName
).Me(
    gameSession: gameSession
).Showcase(
    showcaseName: showcaseName
);
try
{
    Showcase = await domain.ModelAsync();
    
    onGetShowcase.Invoke(Showcase);
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    return e;
}
return null;
```
・コルーチン使用時
```c#
var domain = gs2.Showcase.Namespace(
    namespaceName: showcaseNamespaceName
).Me(
    gameSession: gameSession
).Showcase(
    showcaseName: showcaseName
);
var future = domain.ModelFuture();
yield return future;
if (future.Error != null)
{
    onError.Invoke(
        future.Error
    );
    callback.Invoke(future.Error);
    yield break;
}

Showcase = future.Result;

onGetShowcase.Invoke(Showcase);

callback.Invoke(null);
```

### 購入処理

GS2-Showcaseに商品の購入をリクエストします。  
displayItemId　に購入する商品の陳列商品IDを指定します。  
quantity　に購入する数量を指定します。  

・UniTask有効時
```c#
// 商品の購入をリクエスト
// Request to purchase an item
var domain = gs2.Showcase.Namespace(
    namespaceName: showcaseNamespaceName
).Me(
    gameSession: gameSession
).Showcase(
    showcaseName: showcaseName
);
try
{
    var result = await domain.BuyAsync(
        displayItemId: displayItemId,
        quantity: null,
        config: tempConfig.ToArray()
    );
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    return;
}
```
・コルーチン使用時
```c#
// 商品の購入をリクエスト
// Request to purchase an item
var domain = gs2.Showcase.Namespace(
    namespaceName: showcaseNamespaceName
).Me(
    gameSession: gameSession
).Showcase(
    showcaseName: showcaseName
);
var future = domain.BuyFuture(
    displayItemId: displayItemId,
    config: tempConfig.ToArray()
);
yield return future;
if (future.Error != null)
{
    onError.Invoke(
        future.Error
    );
}
```

GS2-Showcaseで抽選商品購入のトランザクションが発行されます。  
initialize_gamecycle_template.yaml テンプレートでは、トランザクションの実行は __自動実行__ に設定されており、  
発行されたトランザクションはサーバー側で自動的に実行されます。  

```yaml
      TransactionSetting:
        EnableAutoRun: true
```

クライアントは購入トランザクションの完了を `WaitAsync(true)` / `WaitFuture(true)` で待機します（連鎖する抽選・報酬入手のトランザクションも含めて全て待ちます）。

抽選結果の商品リストは以下のコールバックで取得できます。

```c#
// 抽選処理の結果を取得
// Obtain the results of the lottery process
void LotteryResult(
    string _namespace,
    DrawByUserIdRequest request,
    DrawByUserIdResult result
)
{
    // 抽選で獲得したアイテム
    // Items won in the lottery
    var DrawnPrizes = new List<EzDrawnPrize>();
    if (result == null) return;
    var prizes = result.Items;
    foreach (var prize in prizes)
    {
        var item = EzDrawnPrize.FromModel(prize);
        DrawnPrizes.Add(item);
    }

    onAcquireInventoryItem.Invoke(
        DrawnPrizes
    );
}

// 抽選結果取得コールバックを登録
// Register lottery result acquisition callback
Gs2Lottery.Domain.Gs2Lottery.DrawByUserIdComplete.AddListener( LotteryResult );
```

抽選結果が取得できたタイミングで、実際のゲーム内では必要であればクライアントは抽選演出、取得したアイテムの一覧表示等を行います。  
本サンプルの GS2-Lottery / GS2-Showcase ネームスペースは `TransactionSetting.EnableAtomicCommit: true` を設定しているため、  
インベントリーへのアイテム入手はトランザクションの中でサーバー側が一括実行します（ジョブキューは経由しません）。

`TransactionSetting.QueueNamespaceId` を設定した場合は、入手処理が  
[GS2-JobQueue](https://docs.gs2.io/ja/api_reference/job_queue/) のジョブとして登録され、  
クライアントがジョブキューを実行することで配布されます。  
その場合は、ジョブキューを進行させる処理 Gs2Domain.Dispatch を実行しておくことで、自動で継続進行できます。

・UniTask有効時
```c#
async UniTask Impl()
{
    while (true)
    {
        await _domain.DispatchAsync(_session);

        await UniTask.Yield();
    }
}

_stampSheetDispatchCoroutine = StartCoroutine(Impl().ToCoroutine());
```
・コルーチン使用時
```c#
IEnumerator Impl()
{
    while (true)
    {
        var future = _domain.DispatchFuture(_session);
        yield return future;
        if (future != null)
        {
            yield break;
        }
        if (future.Result)
        {
            break;
        }
        yield return null;
    }
}
_stampSheetDispatchCoroutine = StartCoroutine(Impl());
```

抽選商品の購入トランザクション処理の流れは以下のようになります。

![LotteryStore](LotteryStore.png)
