# 強化　解説

[GS2-Enhance](https://docs.gs2.io/ja/api_reference/enhance/) を使って、
所持しているキャラクターに素材を使用し、ランクを上げるサンプルです。

キャラクターのランク・経験値は [GS2-Experience](https://docs.gs2.io/ja/api_reference/experience/) で管理されます。
プレイヤー経験値のサンプル（`experience-0001`）とは別のネームスペース（`enhance-experience`）を使用しています。

## GS2-Deploy テンプレート

- [initialize_gamecycle_template.yaml - 強化](../Templates/initialize_gamecycle_template.yaml)

| リソース | 内容 |
---|---
| GS2-Enhance `enhance` | 強化レート `level` |
| GS2-Inventory `enhance-inventory` | 強化対象の `character`（`character-0001`）と強化素材の `material`（`material-0001`） |

※ `character` インベントリは容量1・スタック上限1のため、キャラクターは1体のみ所持できます。
既に所持している状態で `キャラ入手` を実行するとエラーになります。
| GS2-Experience `enhance-experience` | 強化対象のランク・経験値（経験値モデル `character`） |
| GS2-Exchange `enhance-exchange` | 動作確認用にキャラクター／素材を入手する交換レート `get-character` / `get-material` |

強化レートの定義では、強化対象・強化素材のインベントリと、経験値の加算先を指定します。

```yaml
rateModels:
  - name: level
    targetInventoryModelId: grn:...:inventory:enhance-inventory:model:character
    materialInventoryModelId: grn:...:inventory:enhance-inventory:model:material
    acquireExperienceSuffix: :level
    acquireExperienceHierarchy:
      - experience
    experienceModelId: grn:...:experience:enhance-experience:model:character
    bonusRates:
      - rate: 1.0
        weight: 1
```

素材1個あたりに加算される経験値は、素材のアイテムモデルのメタデータで定義しています。

```yaml
itemModels:
  - name: material-0001
    metadata: {"experience": 50}
```

ランクが上がる経験値のしきい値は、GS2-Experience の経験値モデル `character` で定義しています。
`values` は累計の経験値です。

```yaml
experienceModels:
  - name: character
    defaultRankCap: 5
    maxRankCap: 5
    rankThreshold:
      values:
        - 100
        - 300
        - 500
        - 1000
```

`bonusRates` は `rate: 1.0` の1通りだけなので、加算される経験値は常に `素材の個数 × 50` です。
しきい値が4つのためランクは1〜5で、`defaultRankCap` / `maxRankCap` もそれに合わせて 5 にしています。

| ランク | 必要な累計経験値 | 必要な素材（累計） | 直前のランクからの追加分 |
---|---:|---:|---:
| 1 → 2 | 100 | 2個 | 2個 |
| 2 → 3 | 300 | 6個 | 4個 |
| 3 → 4 | 500 | 10個 | 4個 |
| 4 → 5 | 1000 | 20個 | 10個 |

`素材入手` は交換レート `get-material` で素材を1回に5個入手するため、
4回実行すれば最大ランクまで上げられます。

## 強化設定 EnhanceSetting

`Gs2Settings` オブジェクトにアタッチされています。

| 設定名 | 説明 |
---|---
| enhanceNamespaceName | GS2-Enhance のネームスペース名 |
| enhanceRateName | GS2-Enhance の強化レート名 |
| inventoryNamespaceName | GS2-Inventory のネームスペース名 |
| characterInventoryModelName | 強化対象のインベントリモデル名 |
| materialInventoryModelName | 強化素材のインベントリモデル名 |
| experienceNamespaceName | GS2-Experience のネームスペース名 |
| experienceModelName | 強化対象の経験値モデル名 |
| exchangeNamespaceName | GS2-Exchange のネームスペース名（動作確認用の入手処理に使用） |
| exchangeRateNameGetCharacter | 強化対象を入手する交換レート名 |
| exchangeRateNameGetMaterial | 強化素材を入手する交換レート名 |

## 強化対象とランクの取得

強化メニューを開いたタイミングで、強化対象・強化素材のアイテムセットと、
強化対象のランク・経験値を取得します。

```c#
// 強化対象 / 強化素材
var it = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryModelName
).ItemSets();

// ランク・経験値
var it2 = gs2.Experience.Namespace(
    namespaceName: experienceNamespaceName
).Me(
    gameSession: gameSession
).Statuses();
```

GS2-Enhance は __強化対象のアイテムセットID__ に強化レートの `acquireExperienceSuffix` を連結したものを
GS2-Experience の __プロパティID__ として経験値を加算します。
本サンプルの強化レート `level` は `acquireExperienceSuffix` に `:level` を設定しているため、
プロパティIDは `<アイテムセットID>:level` になります。

そのため、一覧では `EzStatus.PropertyId` の __前方一致__ で `EzItemSet.ItemSetId` と突き合わせてランクを表示しています。
完全一致で比較すると suffix の分だけ一致せず、常に Rank 1 / 経験値 0 の表示になってしまいます。

```c#
public EzStatus FindStatus(EzItemSet itemSet)
{
    return Statuses.FirstOrDefault(
        status => status.PropertyId == itemSet.ItemSetId
                  || status.PropertyId.StartsWith(itemSet.ItemSetId + ":")
    );
}
```

一度も強化していないキャラクターにはステータスが存在しないため、その場合は Rank 1 として表示します。
このとき `EzStatus.NextRankUpExperienceValue` が参照できないので、
次のランクまでの必要経験値は経験値モデルの `RankThreshold.Values[0]` から取得して `0 / 100` と表示します。

## 強化の実行

一覧からキャラクターを選択するとランクアップダイアログが開きます。
使用する素材の数を選び、`強化` を押すと強化を実行します。

```c#
var domain = gs2.Enhance.Namespace(
    namespaceName: enhanceNamespaceName
).Me(
    gameSession: gameSession
).Enhance();
var future = domain.EnhanceFuture(
    rateName: enhanceRateName,
    targetItemSetId: target.ItemSetId,
    materials: new[]
    {
        new EzMaterial
        {
            MaterialItemSetId = material.ItemSetId,
            Count = count,
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

強化は「素材の消費」と「経験値の加算」をまとめたトランザクションとして実行されます。
GS2-Enhance のネームスペースは __トランザクションの自動実行__（`EnableAutoRun: true`）と
__一括実行__（`EnableAtomicCommit: true`）に設定しているため、結果はリクエストの応答時点で確定します。

強化の完了後は、インベントリとランク・経験値を取得し直して表示を更新します。

## キャラクター／素材の入手（動作確認用）

強化を試すためのキャラクターと素材は、GS2-Exchange の交換レートで入手します。

```c#
var future = domain.ExchangeFuture(
    rateName: exchangeRateName,   // get-character / get-material
    count: 1,
    config: new EzConfig[] { }
);
```

※実際のゲームでは、クライアントから直接アイテムを増やす実装は非推奨です。
