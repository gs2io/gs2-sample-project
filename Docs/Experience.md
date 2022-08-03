# 経験値　解説

[GS2-Experience](https://app.gs2.io/docs/index.html#gs2-experience) を使ってプレイヤーの経験値、アイテムの成長を経験値で表現するサンプルです。


## GS2-Deploy テンプレート

- [initialize_experience_template.yaml - 経験値](../Templates/initialize_experience_template.yaml)

## 経験値機能設定 ExperienceSetting

![インスペクターウィンドウ](Experience.png)

| 設定名 | 説明 |
|---|----------------------------------|
| experienceNamespaceName | GS2-Experienceのネームスペース名         |
| playerExperienceModelName | GS2-Experienceのプレイヤー経験値テーブルのモデル名 |
| itemExperienceModelName | GS2-Experienceのアイテム経験値テーブルのモデル名  |
| exchangeNamespaceName | GS2-Exchangeのネームスペース名 |
| playerEexchangeRateName | GS2-Exchangeのプレイヤー経験値入手交換レート名    |
| itemExchangeRateName | GS2-Exchangeのアイテム経験値入手交換レート名     |

| イベント | 説明 |
|---|---|
| onGetExperienceModel(string, EzExperienceModel) | 経験値モデルを取得したときに呼び出されます。 |
| onGetStatuses(EzExperienceModel, List<EzStatus>) | ステータス情報の一覧を取得したときに呼び出されます。 |
| onIncreaseExperience(EzExperienceModel, EzStatus, int) | 経験値の増加を実行したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## プレイヤーの経験値を取得

プレイヤーの経験値を取得します。

・UniTask有効時
```c#
try
{
    var _statuses = await gs2.Experience.Namespace(
        namespaceName: experienceNamespaceName
    ).Me(
        gameSession: gameSession
    ).StatusesAsync().ToListAsync();

    playerStatuses = _statuses.ToDictionary(status => status.PropertyId);

    onGetStatuses.Invoke(playerExperienceModel, _statuses);
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
}
```
・コルーチン使用時
```c#
var _statuses = new List<EzStatus>();
var it = gs2.Experience.Namespace(
    namespaceName: experienceNamespaceName
).Me(
    gameSession: gameSession
).Statuses();
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
        _statuses.Add(it.Current);
    }
}

playerStatuses = _statuses.ToDictionary(status => status.PropertyId);

onGetStatuses.Invoke(playerExperienceModel, _statuses);
```

## プレイヤーの経験値の増加

プレイヤーの経験値の増加を実行します。Gs2-Exchangeで経験値を増加させています。  
ランクごとの閾値を超えるとランクが増加します。    
設定されているランクキャップまで増加可能で、ランクがランク値まで到達すると増加は止まります。

```c#
// ※この処理はサンプルの動作確認のためものです。
// 実際にクライアントが直接経験値の増加を行う実装は非推奨となります。
// *This process is only for sample confirmation.
// The actual implementation of direct experience increase by the client is deprecated.

var domain = gs2.Exchange.Namespace(
    namespaceName: exchangeNamespaceName
).Me(
    gameSession: gameSession
).Exchange();
try
{
    var result = await domain.ExchangeAsync(
        rateName: exchangeRateName,
        count: value,
        config: new[]
        {
            new EzConfig
            {
                Key = "propertyId",
                Value = propertyId
            }
        }
    );
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
}
```

