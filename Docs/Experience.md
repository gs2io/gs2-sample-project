# 経験値　解説

[GS2-Experience](https://app.gs2.io/docs/index.html#gs2-experience) を使ってプレイヤーの経験値、アイテムの成長を経験値で表現するサンプルです。


## GS2-Deploy テンプレート

- [initialize_experience_template.yaml - 経験値](../Templates/initialize_experience_template.yaml)

## 経験値機能設定 ExperienceSetting

![インスペクターウィンドウ](Experience.png)

| 設定名 | 説明 |
|---|---|
| experienceNamespaceName | GS2-Experience のネームスペース名 |
| playerExperienceModelName | GS2-Experienceのプレイヤー経験値テーブルのモデル名 |
| itemExperienceModelName | GS2-Experienceのアイテム経験値テーブルのモデル名 |
| identifierIncreaseExperienceClientId | 経験値の増加が可能な権限のクライアントID |
| identifierIncreaseExperienceClientSecret | 経験値の増加が可能な権限のクライアントシークレット |

| イベント | 説明 |
|---|---|
| onGetExperienceModel(string, EzExperienceModel) | 経験値モデルを取得したときに呼び出されます。 |
| onGetStatuses(EzExperienceModel, List<EzStatus>) | ステータス情報の一覧を取得したときに呼び出されます。 |
| onIncreaseExperience(EzExperienceModel, EzStatus, int) | 経験値の増加を実行したときに呼び出されます。 |
| OnError(Gs2Exception error) | エラーが発生したときに呼び出されます。 |

## プレイヤーの経験値を取得

プレイヤーの経験値を取得します。

```c#
AsyncResult<EzListStatusesResult> result = null;
yield return client.Experience.ListStatuses(
    r =>
    {
        result = r;
    },
    session,
    experienceNamespaceName,
    playerExperienceModel.Name,
    pageToken
);
```

## プレイヤーの経験値の増加

プレイヤーの経験値の増加を実行します。ランクごとの閾値を超えるとランクが増加します。  
設定されているランクキャップまで増加可能で、ランクがランク値まで到達すると増加は止まります。

```c#
{
    // ※この処理はサンプルの動作確認のためものです。
    // 実際にクライアントが直接経験値の増加を行う実装は非推奨となります。
    
    var restSession = new Gs2RestSession(
        new BasicGs2Credential(
            identifierIncreaseExperienceClientId,
            identifierIncreaseExperienceClientSecret
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

    var restClient = new Gs2ExperienceRestClient(
        restSession
    );

    yield return restClient.AddExperienceByUserId(
        new AddExperienceByUserIdRequest()
            .WithNamespaceName(experienceNamespaceName)
            .WithUserId(session.AccessToken.UserId)
            .WithExperienceName(experienceModel.Name)
            .WithPropertyId(propertyId)
            .WithExperienceValue(value),
        r =>
        {
            if (r.Error != null)
            {
                onError.Invoke(r.Error);
                error = true;
            }
            else
            {
                onIncreaseExperience.Invoke(
                    experienceModel,
                    EzStatus.FromModel(r.Result.Item),
                    value
                );
            }
        }
    );
    
    yield return restSession.Close(() => { });
}
```

