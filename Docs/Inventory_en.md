# Gold/Inventory Explanation

Inventory by [GS2-Inventory](https://app.gs2.io/docs/en/index.html#gs2-inventory), implementation of a bag to store items and  
This sample is used to manage gold (in-game currency).

## GS2-Deploy template

- [initialize_gold_template.yaml - gold](../Templates/initialize_gold_template.yaml)
- [initialize_inventory_template.yaml - inventory](../Templates/initialize_inventory_template.yaml)

## GoldSetting

![Inspector Window](Gold.png)

| Setting Name | Description |
---|---
| inventoryNamespaceName | GS2-Inventory's namespace name
| inventoryModelName | model name of GS2-Inventory
| ItemModelName | Gold type name in GS2-Inventory's CurrentItemModelMaster |
| exchangeNamespaceName | GS2-Exchange namespace name                    |
| exchangeRateName | Name of exchange rate for obtaining gold at GS2-Exchange       |

| Event | Description |
---------|------
| onGetInventoryModel(string inventoryName, EzInventoryModel, List<EzItemModel>) | Called when an inventory model is obtained. | onGetInventoryModel(string inventoryName, List<EzItemModel>)
| onGetInventory(EzInventory inventory, List<EzItemSet> itemSets) | Called when inventory information is retrieved. | onGetInventory(EzInventory inventory, List<EzItemSet> itemSets)
| onAcquire(Product product) | Called when gold is added. | onAcquire(Product product)
| onConsume(Product product) | Called when gold is consumed. | onConsume(Product product)
| onError(Gs2Exception error) | Called when an error occurs. | onError(Gs2Exception error)

## InventorySetting

![Inspector Window](Inventory.png)

| Setting Name | Description |
---|---
| inventoryNamespaceName | inventory namespace name for GS2-Inventory
| inventoryModelName | namespace name of the model in GS2-Inventory's inventory
| exchangeNamespaceName | GS2-Exchange namespace name               |
| exchangeRateNameFire | Name of exchange rate for obtaining items from GS2-Exchange (Fire)      |
| exchangeRateNameWater | Name of exchange rate for obtaining items from GS2-Exchange (water)      |

| Event | Description |
---|---
| onGetInventoryModel(string inventoryName, EzInventoryModel, List<EzItemModel>) | Called when an inventory model is obtained. | onGetInventoryModel(string inventoryName, List<EzItemModel>)
| onGetInventory(EzInventory inventory, List<EzItemSet> itemSets) | Called when inventory information is retrieved. | onGetInventory(EzInventory inventory, List<EzItemSet> itemSets)
| onAcquire(Product product) | Called when an item is added. | onAcquire(Product product)
| onConsume(Product product) | Called when an item is consumed. | onConsume(Product product)
| OnError(Gs2Exception error) | Called when an error occurs. | OnError(Gs2Exception error)

## Retrieve inventory model/item model

retrieve inventory and item models.

When UniTask is enabled
```c#
{
    var domain = gs2.Inventory.Namespace(
        namespaceName: inventoryNamespaceName
    ).InventoryModel(
        inventoryName: inventoryModelName
    );
    try
    {
        Model = await domain.ModelAsync();
    }
    catch (Gs2Exception e)
    {
        onError.Invoke(e);
        return;
    }
}
{
    ItemModels.Clear();
    var domain = gs2.Inventory.Namespace(
        namespaceName: inventoryNamespaceName
    ).InventoryModel(
        inventoryName: inventoryModelName
    );
    ItemModels = await domain.ItemModelsAsync().ToListAsync();

}

onGetInventoryModel.Invoke(inventoryModelName, Model, ItemModels);
```
When coroutine is used
```c#
{
    var domain = gs2.Inventory.Namespace(
        namespaceName: inventoryNamespaceName
    ).InventoryModel(
        inventoryName: inventoryModelName
    );
    var future = domain.Model();
    yield return future;
    if (future.Error != null)
    {
        onError.Invoke(future.Error);
        yield break;
    }

    Model = future.Result;
}
{
    ItemModels.Clear();
    var it = gs2.Inventory.Namespace(
        namespaceName: inventoryNamespaceName
    ).InventoryModel(
        inventoryName: inventoryModelName
    ).ItemModels();
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
            ItemModels.Add(it.Current);
        }
    }
}

onGetInventoryModel.Invoke(inventoryModelName, Model, ItemModels);
```

## Get Inventory

Retrieves inventory information.  
For inventory that is handled as gold (in-game currency), the  
The Count of the target ItemSet represents the amount of gold.

When UniTask is enabled
```c#
var domain = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryName
);
try
{
    Inventory = await domain.ModelAsync();
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
}
```
When coroutine is used
```c#
var domain = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryName
);
var future = domain.Model();
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error);
    yield break;
}

Inventory = future.Result;
```

## Consuming gold/items

Consume gold/items, reduce quantity.

When UniTask is enabled
```c#
var domain = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryName
);
var domain2 = domain.ItemSet(
    itemName: itemName,
    itemSetName: null
);
try
{
    var result = await domain2.ConsumeAsync(
        consumeCount: consumeValue
    );
    
    itemSets  = await result.ModelAsync();

    onConsume.Invoke(Inventory, itemSets.ToList(), consumeValue);
}
catch (Gs2Exception e)
{
    onError.Invoke(e);
    return;
}
```
When coroutine is used
```c#
var domain = gs2.Inventory.Namespace(
    namespaceName: inventoryNamespaceName
).Me(
    gameSession: gameSession
).Inventory(
    inventoryName: inventoryName
);
var future = domain.ItemSet(
    itemName: itemName,
    itemSetName: null
).Consume(
    consumeCount: consumeValue
);
yield return future;
if (future.Error != null)
{
    onError.Invoke(future.Error);
    yield break;
}
```

## Obtain gold/items

Obtain gold/items and increase quantity.

Calls for exchange process by GS2-Exchange to increase gold/items.
This is a sample of use for debugging purposes. 

```c#
// *This process is only for sample confirmation.
// The actual implementation in which the client directly increases the number of items is deprecated.

{
    var domain = gs2.Exchange.Namespace(
        namespaceName: exchangeNamespaceName
    ).Me(
        gameSession: gameSession
    ).Exchange();
    try
    {
        await domain.ExchangeAsync(
            rateName: exchangeRateName,
            count: value,
            config: null
        );
    }
    catch (Gs2Exception e)
    {
        onError.Invoke(e);
        return;
    }
}
```