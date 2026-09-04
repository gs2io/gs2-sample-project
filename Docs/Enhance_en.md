# Enhancement Explanation

This is a sample that uses [GS2-Enhance](https://docs.gs2.io/api_reference/enhance/)
to consume materials on a character the player owns and raise its rank.

The rank and experience of the character are managed by [GS2-Experience](https://docs.gs2.io/api_reference/experience/).
A namespace (`enhance-experience`) separate from the player experience sample (`experience-0001`) is used.

## GS2-Deploy template

- [initialize_gamecycle_template.yaml - enhancement](../Templates/initialize_gamecycle_template.yaml)

| Resource | Description |
---|---
| GS2-Enhance `enhance` | The enhancement rate `level` |
| GS2-Inventory `enhance-inventory` | `character` (`character-0001`) as the enhancement target and `material` (`material-0001`) as the material |

*The `character` inventory has a capacity of 1 and a stacking limit of 1, so only one character can be owned.
Running `Get character` while one is already owned results in an error.
| GS2-Experience `enhance-experience` | The rank and experience of the enhancement target (experience model `character`) |
| GS2-Exchange `enhance-exchange` | The rates `get-character` / `get-material` that grant a character and materials for verification purposes |

The rate model specifies the inventories of the target and the materials, and where the experience is added.

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

The experience added per material is defined in the metadata of the material's item model.

```yaml
itemModels:
  - name: material-0001
    metadata: {"experience": 50}
```

The experience thresholds at which the rank goes up are defined in the `character` experience model
of GS2-Experience. `values` holds the cumulative experience.

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

`bonusRates` has only one entry with `rate: 1.0`, so the experience added is always
`number of materials x 50`. There are four thresholds, so the rank ranges from 1 to 5,
and `defaultRankCap` / `maxRankCap` are set to 5 to match.

| Rank | Cumulative experience | Materials (total) | Materials since the previous rank |
---|---:|---:|---:
| 1 -> 2 | 100 | 2 | 2 |
| 2 -> 3 | 300 | 6 | 4 |
| 3 -> 4 | 500 | 10 | 4 |
| 4 -> 5 | 1000 | 20 | 10 |

`Get material` grants five materials at a time through the `get-material` exchange rate,
so running it four times is enough to reach the maximum rank.

## Enhancement settings EnhanceSetting

Attached to the `Gs2Settings` object.

| Setting | Description |
---|---
| enhanceNamespaceName | Namespace name of GS2-Enhance |
| enhanceRateName | Rate model name of GS2-Enhance |
| inventoryNamespaceName | Namespace name of GS2-Inventory |
| characterInventoryModelName | Inventory model name of the enhancement targets |
| materialInventoryModelName | Inventory model name of the enhancement materials |
| experienceNamespaceName | Namespace name of GS2-Experience |
| experienceModelName | Experience model name of the enhancement targets |
| exchangeNamespaceName | Namespace name of GS2-Exchange (used to obtain items for verification purposes) |
| exchangeRateNameGetCharacter | Exchange rate name that grants an enhancement target |
| exchangeRateNameGetMaterial | Exchange rate name that grants enhancement materials |

## Retrieving the targets and their ranks

When the enhancement menu is opened, the item sets of the targets and materials
and the rank / experience of the targets are retrieved.

```c#
// Enhancement targets / materials
var it = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryModelName
).ItemSets();

// Rank and experience
var it2 = gs2.Experience.Namespace(
    namespaceName: experienceNamespaceName
).Me(
    gameSession: gameSession
).Statuses();
```

GS2-Enhance adds experience using the __item set ID of the enhancement target__ concatenated with the
`acquireExperienceSuffix` of the rate as the __property ID__ of GS2-Experience.
The `level` rate of this sample sets `:level` as its `acquireExperienceSuffix`,
so the property ID is `<itemSetId>:level`.

The list therefore matches `EzStatus.PropertyId` against `EzItemSet.ItemSetId` __by prefix__ to show the rank.
An exact comparison never matches because of the suffix, and every target would be shown as Rank 1 with 0 experience.

```c#
public EzStatus FindStatus(EzItemSet itemSet)
{
    return Statuses.FirstOrDefault(
        status => status.PropertyId == itemSet.ItemSetId
                  || status.PropertyId.StartsWith(itemSet.ItemSetId + ":")
    );
}
```

A character that has never been enhanced has no status yet, in which case it is shown as Rank 1.
`EzStatus.NextRankUpExperienceValue` is not available then, so the experience required for the next rank
is read from `RankThreshold.Values[0]` of the experience model and shown as `0 / 100`.

## Executing the enhancement

Selecting a character from the list opens the rank up dialog.
Choose the number of materials to use and press `Enhance` to execute the enhancement.

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

// Wait for automatic transaction execution to complete (including all chained transactions)
var waitFuture = future.Result.WaitFuture(true);
yield return waitFuture;
if (waitFuture.Error != null)
{
    onError.Invoke(waitFuture.Error, null);
    callback.Invoke(waitFuture.Error);
    yield break;
}
```

The enhancement is executed as a transaction that consumes the materials and adds the experience.
The GS2-Enhance namespace is configured for __automatic transaction execution__ (`EnableAutoRun: true`)
and __atomic commit__ (`EnableAtomicCommit: true`), so the result is settled when the request returns.

After the enhancement completes, the inventory and the rank / experience are retrieved again to update the display.

## Obtaining characters and materials (for verification purposes)

The character and materials used to try the enhancement are obtained through GS2-Exchange rates.

```c#
var future = domain.ExchangeFuture(
    rateName: exchangeRateName,   // get-character / get-material
    count: 1,
    config: new EzConfig[] { }
);
```

*In an actual game, an implementation in which the client directly increases items is deprecated.
