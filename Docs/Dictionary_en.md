# Dictionary Explanation

This is a sample that manages the entries a player has obtained (the registration status of the dictionary)
using [GS2-Dictionary](https://docs.gs2.io/api_reference/dictionary/).

## GS2-Deploy template

- [initialize_player_template.yaml - dictionary](../Templates/initialize_player_template.yaml)

It defines the GS2-Dictionary namespace and the entry models (`monster-0001` to `monster-0005`).

Registering (`Gs2Dictionary:AddEntriesByUserId`) and deleting (`Gs2Dictionary:DeleteEntriesByUserId`)
dictionary entries are **server-side only APIs** and cannot be called directly from the client,
so this sample executes them as [GS2-Exchange](https://docs.gs2.io/api_reference/exchange/) rates.

```yaml
rateModels:
  # Registers one dictionary entry. The entry name is passed via Config.
  - name: register-entry
    consumeActions: []
    acquireActions:
      - action: Gs2Dictionary:AddEntriesByUserId
        request:
          namespaceName: dictionary-0001
          userId: "#{userId}"
          entryModelNames:
            - "#{entryModelName}"
  # Deletes one dictionary entry. The entry name is passed via Config.
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

The GS2-Exchange namespace is configured for __automatic transaction execution__ (`TransactionSetting.EnableAutoRun: true`)
and __atomic commit__ (`EnableAtomicCommit: true`), so the result of the exchange is settled when the request returns.

## Dictionary settings DictionarySetting

Attached to the `Gs2Settings` object.

| Setting | Description |
---|---
| dictionaryNamespaceName | Namespace name of GS2-Dictionary |
| exchangeNamespaceName | Namespace name of GS2-Exchange |
| exchangeRateNameRegister | Exchange rate name that registers a dictionary entry |
| exchangeRateNameReset | Exchange rate name that deletes a dictionary entry |

| Event | Description |
---|---
| OnGetEntryModels(List&lt;EzEntryModel&gt; entryModels) | Called when the entry models (definitions) are retrieved. |
| OnGetEntries(List&lt;EzEntry&gt; entries) | Called when the registered entries are retrieved. |
| OnRegisterEntry(string entryModelName) | Called when an entry has been registered. |
| OnResetEntries() | Called when all the entries have been deleted. |
| OnError(Gs2Exception error) | Called when an error occurs. |

## Retrieving the entry models (definitions)

During the initialization after login, the list of entries that can be registered is retrieved.

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

## Retrieving the registered entries

When the dictionary is opened, the entries the player has already obtained are retrieved.

```c#
var it = gs2.Dictionary.Namespace(
    namespaceName: dictionaryNamespaceName
).Me(
    gameSession: gameSession
).Entries();
```

The list of entry models is compared with the registered entries to show which ones are registered.

## Registering a dictionary entry

Pressing the `Register` button registers exactly one unregistered entry.
The entry name is passed to the stamp sheet as the Config value `entryModelName`,
and the `#{entryModelName}` placeholder is replaced with it.

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

## Resetting the dictionary

Pressing the `Reset` button deletes all the registered entries.
The exchange rate deletes one entry at a time, so the exchange is executed once for each registered entry.
