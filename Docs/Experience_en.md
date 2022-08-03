# Experience Explanation

This sample uses [GS2-Experience](https://app.gs2.io/docs/en/index.html#gs2-experience) to represent player experience and item growth in terms of experience.


## GS2-Deploy template

- [initialize_experience_template.yaml - experience](../Templates/initialize_experience_template.yaml)

## ExperienceSetting Experience Features Setting

![Inspector Window](Experience.png)

| Setting Name | Description |
|---|---|
| experienceNamespaceName | Namespace name of GS2-Experience
| playerExperienceModelName | model name of player experience table in GS2-Experience
| itemExperienceModelName | Model name of the item experience table of GS2-Experience
| exchangeNamespaceName | GS2-Exchange namespace name |
| playerEexchangeRateName | GS2-Exchange Player Experience Acquisition Exchange Rate Name    |
| itemExchangeRateName | GS2-Exchange Item Experience Acquisition Exchange Rate Name     |

| Event | Description |
|---|---|
| onGetExperienceModel(string, EzExperienceModel) | Called when an experience model is retrieved. |
| onGetStatuses(EzExperienceModel, List<EzStatus>) | Called when a list of status information is obtained. |
| onIncreaseExperience(EzExperienceModel, EzStatus, int) | Called when an experience increase is performed. |
| OnError(Gs2Exception error) | Called when an error occurs. |

## Get player experience

Gets the experience value of the player.

When UniTask is enabled
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
When coroutine is used
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

## Increased player experience

Performs an increase in the player's experience with Gs2-Exchange.  
Ranks are increased when the threshold for each rank is exceeded.    
It can be increased up to the set rank cap, and the increase stops when the rank reaches the rank value.

```c#
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

