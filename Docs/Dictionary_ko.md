# 도감 해설

[GS2-Dictionary](https://docs.gs2.io/ko/api_reference/dictionary/) 를 사용하여,
플레이어가 획득한 엔트리(도감 등록 상황)를 관리하는 샘플입니다.

## GS2-Deploy 템플릿

- [initialize_player_template.yaml - 도감](../Templates/initialize_player_template.yaml)

GS2-Dictionary 의 네임스페이스와 엔트리 모델(`monster-0001` 〜 `monster-0005`)을 정의하고 있습니다.

도감 엔트리의 등록(`Gs2Dictionary:AddEntriesByUserId`)과 삭제(`Gs2Dictionary:DeleteEntriesByUserId`)는
**서버 전용 API** 라서 클라이언트에서 직접 호출할 수 없기 때문에, 본 샘플에서는
[GS2-Exchange](https://docs.gs2.io/ko/api_reference/exchange/) 의 교환 레이트로 실행하고 있습니다.

```yaml
rateModels:
  # 도감 엔트리를 1건 등록한다. 등록할 엔트리 이름은 Config 로 전달한다
  - name: register-entry
    consumeActions: []
    acquireActions:
      - action: Gs2Dictionary:AddEntriesByUserId
        request:
          namespaceName: dictionary-0001
          userId: "#{userId}"
          entryModelNames:
            - "#{entryModelName}"
  # 도감 엔트리를 1건 삭제한다. 삭제할 엔트리 이름은 Config 로 전달한다
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

GS2-Exchange 의 네임스페이스는 __트랜잭션의 자동 실행__(`TransactionSetting.EnableAutoRun: true`)과
__일괄 실행__(`EnableAtomicCommit: true`)으로 설정되어 있기 때문에, 교환의 결과는 요청의 응답 시점에 확정됩니다.

## 도감 설정 DictionarySetting

`Gs2Settings` 오브젝트에 어태치되어 있습니다.

| 설정 이름 | 설명 |
---|---
| dictionaryNamespaceName | GS2-Dictionary 의 네임스페이스 이름 |
| exchangeNamespaceName | GS2-Exchange 의 네임스페이스 이름 |
| exchangeRateNameRegister | 도감 엔트리를 등록하는 교환 레이트 이름 |
| exchangeRateNameReset | 도감 엔트리를 삭제하는 교환 레이트 이름 |

| 이벤트 | 설명 |
---|---
| OnGetEntryModels(List&lt;EzEntryModel&gt; entryModels) | 엔트리 모델(정의)을 가져왔을 때 호출됩니다. |
| OnGetEntries(List&lt;EzEntry&gt; entries) | 획득한 엔트리를 가져왔을 때 호출됩니다. |
| OnRegisterEntry(string entryModelName) | 엔트리를 등록했을 때 호출됩니다. |
| OnResetEntries() | 엔트리를 모두 삭제했을 때 호출됩니다. |
| OnError(Gs2Exception error) | 오류가 발생했을 때 호출됩니다. |

## 엔트리 모델(정의) 가져오기

로그인 후의 초기화에서, 도감에 등록될 수 있는 엔트리의 목록을 가져옵니다.

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

## 획득한 엔트리 가져오기

도감을 연 시점에, 플레이어가 획득한 엔트리를 가져옵니다.

```c#
var it = gs2.Dictionary.Namespace(
    namespaceName: dictionaryNamespaceName
).Me(
    gameSession: gameSession
).Entries();
```

엔트리 모델의 목록과 획득한 엔트리를 대조하여, 등록됨／미등록을 표시합니다.

## 도감 엔트리의 등록

`등록` 버튼을 누르면, 미등록 엔트리를 1 건만 등록합니다.
등록할 엔트리 이름은 Config 의 `entryModelName` 으로 스탬프 시트에 전달되어,
`#{entryModelName}` 플레이스홀더가 치환됩니다.

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

// 트랜잭션의 자동 실행 완료를 대기(연쇄되는 트랜잭션도 포함하여 모두 대기)
var waitFuture = future.Result.WaitFuture(true);
yield return waitFuture;
if (waitFuture.Error != null)
{
    onError.Invoke(waitFuture.Error, null);
    callback.Invoke(waitFuture.Error);
    yield break;
}
```

## 도감의 리셋

`초기화` 버튼을 누르면, 획득한 엔트리를 모두 삭제합니다.
교환 레이트가 1 건씩 삭제하는 정의로 되어 있기 때문에, 획득한 엔트리의 수만큼 교환을 실행합니다.
