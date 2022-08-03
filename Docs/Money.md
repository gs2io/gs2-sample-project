# 課金通貨/課金通貨ストア　解説

[GS2-Money](https://app.gs2.io/docs/index.html#gs2-money) を使って管理されている課金通貨を、  
[GS2-Showcase](https://app.gs2.io/docs/index.html#gs2-showcase) で販売するサンプルです。

サンプルで定義されている商品のうち１つには [GS2-Limit](https://app.gs2.io/docs/index.html#gs2-limit) による  
購入回数の制限がついており、１回のみ購入が可能になっています。

![商品リスト](Products.png)

## GS2-Deploy テンプレート

- [initialize_money_template.yaml - 課金通貨/課金通貨ストア](../Templates/initialize_money_template.yaml)

## Unity IAPの有効化、インポート

[GS2-Money](https://app.gs2.io/docs/index.html#gs2-money) を使用したサンプルの動作には、Unity IAPの有効化が必要になります。  
( https://docs.unity3d.com/ja/2019.4/Manual/UnityIAPSettingUp.html )  
サービスウィンドウでのIn-App Purchasingの有効化、  
IAP パッケージのインポートを行います。

## 課金通貨/課金通貨ストア設定 MoneySetting

![インスペクターウィンドウ](Money.png)

| 設定名 | 説明 | 
|---|---|
| moneyNamespaceName | GS2-Money のネームスペース名 |
| showcaseNamespaceName | GS2-Showcase のネームスペース名 |
| showcaseName | GS2-Showcase の陳列棚名 |

| イベント | 説明 |
|---|---|
| OnGetWallet(EzWalletDetail wallet) | ウォレットの情報を取得したときに呼び出されます。 |
| OnGetProducts(List<Product> products) | 販売中の商品一覧を取得したときに呼び出されます。 |
| OnBuy(Product product) | 商品の購入が完了したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## ウォレットの取得

![Wallet](Wallet.png)

ログイン後、以下で最新のウォレットの状態を取得します。

・UniTask有効時
```c#
var domain = gs2.Money.Namespace(
    namespaceName: moneyNamespaceName
).Me(
    gameSession: gameSession
).Wallet(
    slot: Slot
);
try
{
    Wallet = await domain.ModelAsync();

    onGetWallet.Invoke(Wallet);
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
}
```
・コルーチン使用時
```c#
var domain = gs2.Money.Namespace(
    namespaceName: moneyNamespaceName
).Me(
    gameSession: gameSession
).Wallet(
    slot: Slot
);
var future = domain.Model();
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error);
    yield break;
}

Wallet = future.Result;

onGetWallet.Invoke(Wallet);
```

## 課金通貨ストアの商品取得

![Products2](Products2.png)

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
    var showcase = await domain.ModelAsync();

    onGetProducts.Invoke(Products);

    return Products;
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
}
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
var future = domain.Model();
yield return future;
if (future.Error != null)
{
    onError.Invoke(
        future.Error
    );
    yield break;
}
```

取得した商品情報をパースし、販売価格や入手できる課金通貨の数量を取得します。  
購入回数制限が設定されている場合は、購入回数カウンターの状態も取得しています。

```c#
var products = new List<Product>();
foreach (var displayItem in result.Result.Item.DisplayItems)
{
    var depositRequest = GetAcquireAction<DepositByUserIdRequest>(
        displayItem.SalesItem, 
        "Gs2Money:DepositByUserId"
    );
    var recordReceiptRequest = GetConsumeAction<RecordReceiptRequest>(
        displayItem.SalesItem, 
        "Gs2Money:RecordReceipt"
    );
    var countUpRequest = GetConsumeAction<CountUpByUserIdRequest>(
        displayItem.SalesItem, 
        "Gs2Limit:CountUpByUserId"
    );
    var price = depositRequest.Price;
    var count = depositRequest.Count;

    int? boughtCount = null;
    if(countUpRequest != null) {
        AsyncResult<EzGetCounterResult> result2 = null;
        yield return client.Limit.GetCounter(
            r => { result2 = r; },
            session,
            countUpRequest.NamespaceName,
            countUpRequest.LimitName,
            countUpRequest.CounterName
        );
        if (result2.Error == null)
        {
            boughtCount = result2.Result.Item.Count;
        }
        else if (result2.Error is NotFoundException)
        {
            boughtCount = 0;
        }
    }
    products.Add(new Product
    {
        Id = displayItem.DisplayItemId,
        ContentsId = recordReceiptRequest.ContentsId,
        Price = price,
        CurrencyCount = count,
        BoughtCount = boughtCount,
        BoughtLimit = countUpRequest == null ? null : countUpRequest.MaxValue,
    });
}
```

## 購入処理

モバイル環境であれば、Unity IAP を使用して AppStore や GooglePlay でのコンテンツの購入を行います   
（商品の登録、設定が必要になります）。  
エディター環境ではFake Storeのレシート（デバッグ用の模擬レシート）が発行されます。  
得られたレシートを後続の処理で参照できるよう保持しておきます。

・UniTask有効時
```c#
string receipt;
{
#if GS2_ENABLE_PURCHASING
    try
    {
        PurchaseParameters result = await new IAPUtil().BuyAsync(
            selectedProduct.ContentsId
        );
        
        // 課金通貨商品購入 レシート情報
        // Billed Currency Product Purchase Receipt Information
        receipt = result.receipt;
    }
    catch (Gs2Exception e)
    {
        onError.Invoke(e);
        return e;
    }
#endif
}
```
・コルーチン使用時
```c#
string receipt;
{
#if GS2_ENABLE_PURCHASING
    AsyncResult<PurchaseParameters> result = null;
    yield return new IAPUtil().Buy(
        r => { result = r; },
        selectedProduct.ContentsId
    );
    if (result.Error != null)
    {
        onError.Invoke(
            result.Error
        );
        callback.Invoke(
            result.Error
        );
        yield break;
    }

    // 課金通貨商品購入 レシート情報
    // Billed Currency Product Purchase Receipt Information
    receipt = result.Result.receipt;
#endif
}
```

購入したレシートを使って、[GS2-Showcase](https://app.gs2.io/docs/index.html#gs2-showcase) の商品を購入する処理を実行します。  

・UniTask有効時
```c#
// Showcase 商品の購入をリクエスト
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
        displayItemId: selectedProduct.Id,
        config: new[]
        {
            new EzConfig
            {
                Key = "slot",
                Value = Slot.ToString(),
            },
            new EzConfig
            {
                Key = "receipt",
                Value = receipt,
            },
        }
    );
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    return e;
}

// 商品購入に成功
// Successful product purchase
onBuy.Invoke(selectedProduct);
return null;
```
・コルーチン使用時
```c#
// Showcase 商品の購入をリクエスト
// Request to purchase an item
var domain = gs2.Showcase.Namespace(
    namespaceName: showcaseNamespaceName
).Me(
    gameSession: gameSession
).Showcase(
    showcaseName: showcaseName
);
var future = domain.Buy(
    displayItemId: selectedProduct.Id,
    config: new []
    {
        new EzConfig
        {
            Key = "slot",
            Value = Slot.ToString(),
        },
        new EzConfig
        {
            Key = "receipt",
            Value = receipt,
        },
    }
);
yield return future;
if (future.Error != null)
{
    onError.Invoke(
        future.Error
    );
    callback.Invoke(
        future.Error
    );
    yield break;
}

// 商品購入に成功
// Successful product purchase

onBuy.Invoke(selectedProduct);

callback.Invoke(null);
```
Config には [GS2-Money](https://app.gs2.io/docs/index.html#gs2-money)  のウォレットスロット番号 __slot__ と、
レシートの内容 __receipt__ を渡します。
ウォレットスロット番号はこのサンプルで参考のためにプラットフォーム別に割り振った課金通貨の種別で、以下のように定義しています。

| プラットフォーム      | 番号 |
|---------------|---|
| スタンドアローン(その他) | 0 |
| iOS           | 1 |
| Android       | 2 |

Config はスタンプシートに動的なパラメータを渡すための仕組みです。  
[⇒スタンプシートの変数](https://app.gs2.io/docs/index.html#d7e97677c7)  
Config(EzConfig) はキー・バリュー形式で、渡したパラメータで #{Config で指定したキー値} のプレースホルダー文字列を置換することができます。
以下のスタンプシートの定義中の　#{slot}　はウォレットスロット番号、#{receipt}はレシートに置換されます。

```yaml
consumeActions:
  - action: Gs2Money:RecordReceipt
    request:
      namespaceName: ${MoneyNamespaceName}
      contentsId: io.gs2.sample.currency120
      userId: "#{userId}"
      receipt: "#{receipt}"
acquireActions:
  - action: Gs2Money:DepositByUserId
    request:
      namespaceName: ${MoneyNamespaceName}
      userId: "#{userId}"
      slot: "#{slot}"
      price: 120
      count: 50
```

購入処理により、GS2-Showcaseで課金通貨商品購入スタンプシートが発行されます。  
GS2Domainクラス（ソース内で”gs2”）を使用した実装ではクライアント側でのスタンプシートの処理は __自動実行__ されます。  

通常の課金通貨商品の購入スタンプシートの流れは以下になります。

![課金通貨購入](BuyGems.png)

購入制限のある課金通貨商品の購入スタンプシートの流れは以下になります。

![購入制限](BuyGems2.png)


