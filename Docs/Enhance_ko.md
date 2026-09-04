# 강화 해설

[GS2-Enhance](https://docs.gs2.io/ko/api_reference/enhance/) 를 사용하여,
보유하고 있는 캐릭터에 재료를 사용해 랭크를 올리는 샘플입니다.

캐릭터의 랭크·경험치는 [GS2-Experience](https://docs.gs2.io/ko/api_reference/experience/) 로 관리됩니다.
플레이어 경험치의 샘플(`experience-0001`)과는 다른 네임스페이스(`enhance-experience`)를 사용하고 있습니다.

## GS2-Deploy 템플릿

- [initialize_gamecycle_template.yaml - 강화](../Templates/initialize_gamecycle_template.yaml)

| 리소스 | 내용 |
---|---
| GS2-Enhance `enhance` | 강화 레이트 `level` |
| GS2-Inventory `enhance-inventory` | 강화 대상인 `character`(`character-0001`)와 강화 재료인 `material`(`material-0001`) |

※ `character` 인벤토리는 용량 1·스택 상한 1 이므로, 캐릭터는 1 체만 보유할 수 있습니다.
이미 보유한 상태에서 `캐릭터 획득` 을 실행하면 오류가 됩니다.
| GS2-Experience `enhance-experience` | 강화 대상의 랭크·경험치(경험치 모델 `character`) |
| GS2-Exchange `enhance-exchange` | 동작 확인용으로 캐릭터／재료를 획득하는 교환 레이트 `get-character` / `get-material` |

강화 레이트의 정의에서는 강화 대상·강화 재료의 인벤토리와, 경험치를 더할 곳을 지정합니다.

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

재료 1 개당 더해지는 경험치는 재료 아이템 모델의 메타데이터로 정의하고 있습니다.

```yaml
itemModels:
  - name: material-0001
    metadata: {"experience": 50}
```

랭크가 올라가는 경험치의 임계값은 GS2-Experience 의 경험치 모델 `character` 에서 정의하고 있습니다.
`values` 는 누적 경험치입니다.

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

`bonusRates` 는 `rate: 1.0` 한 가지뿐이므로, 더해지는 경험치는 항상 `재료의 개수 × 50` 입니다.
임계값이 4 개이므로 랭크는 1〜5 이며, `defaultRankCap` / `maxRankCap` 도 그에 맞춰 5 로 설정했습니다.

| 랭크 | 필요한 누적 경험치 | 필요한 재료(누적) | 직전 랭크로부터의 추가분 |
---|---:|---:|---:
| 1 → 2 | 100 | 2 개 | 2 개 |
| 2 → 3 | 300 | 6 개 | 4 개 |
| 3 → 4 | 500 | 10 개 | 4 개 |
| 4 → 5 | 1000 | 20 개 | 10 개 |

`재료 획득` 은 교환 레이트 `get-material` 로 재료를 1 회에 5 개 획득하므로,
4 회 실행하면 최대 랭크까지 올릴 수 있습니다.

## 강화 설정 EnhanceSetting

`Gs2Settings` 오브젝트에 어태치되어 있습니다.

| 설정 이름 | 설명 |
---|---
| enhanceNamespaceName | GS2-Enhance 의 네임스페이스 이름 |
| enhanceRateName | GS2-Enhance 의 강화 레이트 이름 |
| inventoryNamespaceName | GS2-Inventory 의 네임스페이스 이름 |
| characterInventoryModelName | 강화 대상의 인벤토리 모델 이름 |
| materialInventoryModelName | 강화 재료의 인벤토리 모델 이름 |
| experienceNamespaceName | GS2-Experience 의 네임스페이스 이름 |
| experienceModelName | 강화 대상의 경험치 모델 이름 |
| exchangeNamespaceName | GS2-Exchange 의 네임스페이스 이름(동작 확인용 획득 처리에 사용) |
| exchangeRateNameGetCharacter | 강화 대상을 획득하는 교환 레이트 이름 |
| exchangeRateNameGetMaterial | 강화 재료를 획득하는 교환 레이트 이름 |

## 강화 대상과 랭크 가져오기

강화 메뉴를 연 시점에, 강화 대상·강화 재료의 아이템 세트와,
강화 대상의 랭크·경험치를 가져옵니다.

```c#
// 강화 대상 / 강화 재료
var it = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryModelName
).ItemSets();

// 랭크·경험치
var it2 = gs2.Experience.Namespace(
    namespaceName: experienceNamespaceName
).Me(
    gameSession: gameSession
).Statuses();
```

GS2-Enhance 는 __강화 대상의 아이템 세트 ID__ 에 강화 레이트의 `acquireExperienceSuffix` 를 연결한 것을
GS2-Experience 의 __프로퍼티 ID__ 로 하여 경험치를 더합니다.
본 샘플의 강화 레이트 `level` 은 `acquireExperienceSuffix` 에 `:level` 을 설정하고 있으므로,
프로퍼티 ID 는 `<아이템 세트 ID>:level` 이 됩니다.

그래서 목록에서는 `EzStatus.PropertyId` 의 __전방 일치__ 로 `EzItemSet.ItemSetId` 와 대조하여 랭크를 표시하고 있습니다.
완전 일치로 비교하면 suffix 만큼 일치하지 않아, 항상 Rank 1 / 경험치 0 으로 표시되어 버립니다.

```c#
public EzStatus FindStatus(EzItemSet itemSet)
{
    return Statuses.FirstOrDefault(
        status => status.PropertyId == itemSet.ItemSetId
                  || status.PropertyId.StartsWith(itemSet.ItemSetId + ":")
    );
}
```

한 번도 강화하지 않은 캐릭터에는 스테이터스가 존재하지 않으므로, 그 경우에는 Rank 1 로 표시합니다.
이때 `EzStatus.NextRankUpExperienceValue` 를 참조할 수 없으므로,
다음 랭크까지의 필요 경험치는 경험치 모델의 `RankThreshold.Values[0]` 에서 가져와 `0 / 100` 으로 표시합니다.

## 강화의 실행

목록에서 캐릭터를 선택하면 랭크 업 다이얼로그가 열립니다.
사용할 재료의 수를 고르고 `강화` 를 누르면 강화를 실행합니다.

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

강화는 「재료의 소비」와 「경험치의 가산」을 묶은 트랜잭션으로 실행됩니다.
GS2-Enhance 의 네임스페이스는 __트랜잭션의 자동 실행__(`EnableAutoRun: true`)과
__일괄 실행__(`EnableAtomicCommit: true`)으로 설정되어 있기 때문에, 결과는 요청의 응답 시점에 확정됩니다.

강화가 완료된 후에는 인벤토리와 랭크·경험치를 다시 가져와 표시를 갱신합니다.

## 캐릭터／재료의 획득(동작 확인용)

강화를 시험하기 위한 캐릭터와 재료는 GS2-Exchange 의 교환 레이트로 획득합니다.

```c#
var future = domain.ExchangeFuture(
    rateName: exchangeRateName,   // get-character / get-material
    count: 1,
    config: new EzConfig[] { }
);
```

※ 실제 게임에서는 클라이언트에서 직접 아이템을 늘리는 구현은 권장되지 않습니다.
